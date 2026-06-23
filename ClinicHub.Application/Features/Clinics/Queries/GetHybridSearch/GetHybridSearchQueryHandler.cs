using AutoMapper;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.Services.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Geometries;

namespace ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch
{
    public class GetHybridSearchQueryHandler : IRequestHandler<GetHybridSearchQuery, PagginatedResult<ClinicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;

        public GetHybridSearchQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IMapService mapService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mapService = mapService;
        }

        public async Task<PagginatedResult<ClinicDto>> Handle(GetHybridSearchQuery request, CancellationToken cancellationToken)
        {
            var finalResultsMap = new Dictionary<string, ClinicDto>(StringComparer.OrdinalIgnoreCase);
            var normalizedSearchText = request.SearchText?.NormalizeArabic();

            Guid? specializationId = null;
            if (!string.IsNullOrEmpty(request.SpecializationId))
            {
                if (!Guid.TryParse(request.SpecializationId, out var guid))
                {
                    var spec = await _unitOfWork.SpecializationRepository
                        .GetFirstAsync(s => s.Name == request.SpecializationId || s.ArName == request.SpecializationId, cancellationToken);
                    specializationId = spec?.Id;
                }
                else
                {
                    specializationId = guid;
                }
            }

            Task<IEnumerable<Clinic>> internalSearchTask = null!;
            Task<List<ClinicExternalDto>> externalSearchTask = null!;
            Task<List<ClinicExternalDto>> geocodeTask = null!;
            Point? userPoint = null;

            if (request.UserLat.HasValue && request.UserLng.HasValue)
            {
                userPoint = new Point(request.UserLng.Value, request.UserLat.Value) { SRID = 4326 };

                if (request.IsNearest)
                {
                    var radiusInMeters = request.RadiusInKm * 1000;
                    internalSearchTask = _unitOfWork.ClinicRepository.GetWithinDistanceAsync(userPoint, radiusInMeters, specializationId, cancellationToken);

                    string category = "hospital,doctor,dentist";
                    externalSearchTask = _mapService.GetNearbyFromMapAsync(request.UserLat!.Value, request.UserLng!.Value, category, cancellationToken, radiusInMeters);
                }
                else
                {
                    internalSearchTask = GetInternalClinicsAsync(request, normalizedSearchText, specializationId, cancellationToken);
                    if (!string.IsNullOrEmpty(request.SearchText))
                    {
                        externalSearchTask = _mapService.GeocodeAsync(request.SearchText, cancellationToken, 10);
                    }
                }
            }
            else if (!string.IsNullOrEmpty(request.SearchText))
            {
                geocodeTask = _mapService.GeocodeAsync(request.SearchText, cancellationToken, 1);
                internalSearchTask = GetInternalClinicsAsync(request, normalizedSearchText, specializationId, cancellationToken);
            }
            else
            {
                internalSearchTask = GetInternalClinicsAsync(request, normalizedSearchText, specializationId, cancellationToken);
            }

            var tasksToWait = new List<Task>();
            if (internalSearchTask != null) tasksToWait.Add(internalSearchTask);
            if (geocodeTask != null) tasksToWait.Add(geocodeTask);
            if (externalSearchTask != null) tasksToWait.Add(externalSearchTask);

            await Task.WhenAll(tasksToWait);

            if (userPoint == null && geocodeTask != null && geocodeTask.Status == TaskStatus.RanToCompletion)
            {
                var geocodeResult = await geocodeTask;
                var firstMatch = geocodeResult.FirstOrDefault();
                if (firstMatch != null)
                {
                    userPoint = new Point(firstMatch.Lng, firstMatch.Lat) { SRID = 4326 };
                }
            }

            var internalClinics = internalSearchTask != null ? await internalSearchTask : Enumerable.Empty<Clinic>();
            foreach (var clinic in internalClinics)
            {
                if (!string.IsNullOrEmpty(normalizedSearchText) && request.IsNearest)
                {
                    if (!clinic.Name.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) &&
                        !(clinic.NameAr?.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                        continue;
                }

                var dto = _mapper.Map<ClinicDto>(clinic);
                if (userPoint != null)
                {
                    dto.Distance = CalculateDistance(userPoint.Y, userPoint.X, clinic.Location.Y, clinic.Location.X);
                }
                finalResultsMap[dto.Name] = dto;
            }

            var externalClinics = new List<ClinicExternalDto>();
            if (externalSearchTask != null && externalSearchTask.Status == TaskStatus.RanToCompletion)
                externalClinics.AddRange(await externalSearchTask);
            if (geocodeTask != null && geocodeTask.Status == TaskStatus.RanToCompletion)
                externalClinics.AddRange(await geocodeTask);

            foreach (var external in externalClinics)
            {
                if (!string.IsNullOrEmpty(normalizedSearchText))
                {
                    if (!external.Name.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) &&
                        !(external.NameAr?.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                        continue;
                }

                if (finalResultsMap.ContainsKey(external.Name)) continue;

                var distance = userPoint != null ? CalculateDistance(userPoint.Y, userPoint.X, external.Lat, external.Lng) : 0;
                if (request.IsNearest && userPoint != null && distance > (request.RadiusInKm * 1000)) continue;

                finalResultsMap[external.Name] = new ClinicDto
                {
                    Id = Guid.NewGuid(),
                    Name = external.Name,
                    NameAr = external.NameAr,
                    Address = external.Address,
                    AddressAr = external.AddressAr,
                    Lat = external.Lat,
                    Lng = external.Lng,
                    IsRegistered = false,
                    Distance = distance,
                    Phone = external.Phone,
                    Website = external.Website
                };
            }

            var finalResults = finalResultsMap.Values
                .OrderByDescending(c => c.IsRegistered)
                .ThenBy(c => c.Distance)
                .ToList();

            var pagedData = finalResults
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagginatedResult<ClinicDto>(pagedData, finalResults.Count, request.PageNumber, request.PageSize);
        }

        private async Task<IEnumerable<Clinic>> GetInternalClinicsAsync(GetHybridSearchQuery request, string? normalizedSearchText, Guid? specializationId, CancellationToken cancellationToken)
        {
            var internalQuery = _unitOfWork.ClinicRepository.GetAllWithIncluding(
                c => (string.IsNullOrEmpty(normalizedSearchText) ||
                      c.Name.Contains(request.SearchText!) ||
                      (c.NameAr != null && c.NameAr.Contains(request.SearchText!)) ||
                      c.Specialization.Name.Contains(request.SearchText!) ||
                      c.Specialization.ArName.Contains(request.SearchText!) ||
                      EF.Functions.Like(c.Name, $"%{normalizedSearchText}%") ||
                      (c.NameAr != null && EF.Functions.Like(c.NameAr, $"%{normalizedSearchText}%")) ||
                      EF.Functions.Like(c.Specialization.Name, $"%{normalizedSearchText}%") ||
                      EF.Functions.Like(c.Specialization.ArName, $"%{normalizedSearchText}%")) &&
                     (!specializationId.HasValue || c.SpecializationId == specializationId),
                c => c.Specialization);

            return await internalQuery.ToListAsync(cancellationToken);
        }

        private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
        {
            var r = 6371e3;
            var phi1 = lat1 * Math.PI / 180;
            var phi2 = lat2 * Math.PI / 180;
            var deltaPhi = (lat2 - lat1) * Math.PI / 180;
            var deltaLambda = (lon2 - lon1) * Math.PI / 180;

            var a = Math.Sin(deltaPhi / 2) * Math.Sin(deltaPhi / 2) +
                    Math.Cos(phi1) * Math.Cos(phi2) *
                    Math.Sin(deltaLambda / 2) * Math.Sin(deltaLambda / 2);
            var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));

            return r * c;
        }
    }
}
