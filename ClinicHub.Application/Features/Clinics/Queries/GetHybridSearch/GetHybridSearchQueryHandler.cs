using AutoMapper;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.Services.Interfaces;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using NetTopologySuite.Geometries;

namespace ClinicHub.Application.Features.Clinics.Queries.GetHybridSearch
{
    public class GetHybridSearchQueryHandler : IRequestHandler<GetHybridSearchQuery, PagginatedResult<ClinicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMapService _mapService;
        private readonly ILogger<GetHybridSearchQueryHandler> _logger;

        private static readonly string[] HealthcareCategories =
        [
            "hospital", "doctor", "dentist", "medical_center",
            "physiotherapist", "diagnostic_center", "laboratory", "radiology_center"
        ];

        public GetHybridSearchQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IMapService mapService, ILogger<GetHybridSearchQueryHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _mapService = mapService;
            _logger = logger;
        }

        public async Task<PagginatedResult<ClinicDto>> Handle(GetHybridSearchQuery request, CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "GetHybridSearch: SearchText=\"{SearchText}\", SpecId={SpecId}, UserLat={Lat}, UserLng={Lng}, IsNearest={IsNearest}, Radius={Radius}km, Page={Page}/{Size}",
                request.SearchText, request.SpecializationId, request.UserLat, request.UserLng,
                request.IsNearest, request.RadiusInKm, request.PageNumber, request.PageSize);

            var finalResultsMap = new Dictionary<string, ClinicDto>(StringComparer.OrdinalIgnoreCase);
            var normalizedSearchText = request.SearchText?.NormalizeArabic();

            Guid? specializationId = await ResolveSpecializationId(request.SpecializationId, cancellationToken);

            Point? userPoint = null;
            if (request.UserLat.HasValue && request.UserLng.HasValue)
            {
                userPoint = new Point(request.UserLng.Value, request.UserLat.Value) { SRID = 4326 };
            }

            var internalTask = GetInternalClinicsAsync(request, normalizedSearchText, specializationId, cancellationToken);
            List<Task<List<ClinicExternalDto>>> externalTasks = [];

            if (request.IsNearest && userPoint != null)
            {
                externalTasks.Add(GetExternalNearbyAsync(userPoint, request.RadiusInKm, request.SearchText, cancellationToken));
            }
            else if (!string.IsNullOrEmpty(request.SearchText))
            {
                externalTasks.Add(GetExternalTextSearchAsync(request.SearchText, userPoint, request.RadiusInKm, request.SpecializationId, cancellationToken));
            }

            if (userPoint == null && !string.IsNullOrEmpty(request.SearchText))
            {
                externalTasks.Add(GeocodeAndSearchAsync(request.SearchText, request.RadiusInKm, cancellationToken));
            }

            var internalClinics = await internalTask;

            foreach (var externalTask in externalTasks)
            {
                var _ = externalTask;
            }

            await Task.WhenAll(externalTasks);

            _logger.LogDebug("Internal clinics: {Count}", internalClinics.Count());

            foreach (var clinic in internalClinics)
            {
                var dto = _mapper.Map<ClinicDto>(clinic);
                if (userPoint != null)
                {
                    dto.Distance = CalculateDistance(userPoint.Y, userPoint.X, clinic.Location.Y, clinic.Location.X);
                }

                var dedupKey = clinic.Name.NormalizeArabic();
                if (!finalResultsMap.ContainsKey(dedupKey))
                {
                    finalResultsMap[dedupKey] = dto;
                }
            }

            int externalAdded = 0;
            int externalFilteredByName = 0;
            int externalFilteredByDistance = 0;
            int externalDuplicates = 0;

            var seenPlaceIds = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (var task in externalTasks)
            {
                var extResults = task.Status == TaskStatus.RanToCompletion ? await task : [];

                foreach (var external in extResults)
                {
                    if (!string.IsNullOrEmpty(external.PlaceId))
                    {
                        if (!seenPlaceIds.Add(external.PlaceId))
                        {
                            externalDuplicates++;
                            continue;
                        }
                    }

                    if (!string.IsNullOrEmpty(normalizedSearchText))
                    {
                        var nameMatch = external.Name.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase);
                        var nameArMatch = external.NameAr?.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) ?? false;
                        if (!nameMatch && !nameArMatch)
                        {
                            externalFilteredByName++;
                            continue;
                        }
                    }

                    var dedupKey = external.Name.NormalizeArabic();
                    if (finalResultsMap.ContainsKey(dedupKey))
                    {
                        externalDuplicates++;
                        continue;
                    }

                    var distance = userPoint != null
                        ? CalculateDistance(userPoint.Y, userPoint.X, external.Lat, external.Lng)
                        : 0;

                    if (request.IsNearest && userPoint != null && distance > (request.RadiusInKm * 1000))
                    {
                        externalFilteredByDistance++;
                        continue;
                    }

                    finalResultsMap[dedupKey] = new ClinicDto
                    {
                        Id = Guid.NewGuid(),
                        Name = external.Name,
                        NameAr = external.NameAr,
                        Address = external.Address,
                        AddressAr = external.AddressAr,
                        Phone = external.Phone,
                        Website = external.Website,
                        Lat = external.Lat,
                        Lng = external.Lng,
                        IsRegistered = false,
                        Distance = distance,
                        SpecializationName = null,
                        SpecializationNameAr = null
                    };

                    externalAdded++;
                }
            }

            _logger.LogInformation(
                "Merge results: internal={InternalCount}, externalAdded={ExternalAdded}, " +
                "filteredByName={FilteredByName}, filteredByDistance={FilteredByDistance}, " +
                "duplicates={Duplicates}, totalUnique={Total}",
                internalClinics.Count(), externalAdded,
                externalFilteredByName, externalFilteredByDistance,
                externalDuplicates, finalResultsMap.Count);

            var scoredResults = ScoreAndRank(finalResultsMap.Values, normalizedSearchText, userPoint);

            var pagedData = scoredResults
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagginatedResult<ClinicDto>(pagedData, scoredResults.Count, request.PageNumber, request.PageSize);
        }

        private Task<List<ClinicExternalDto>> GetExternalNearbyAsync(Point userPoint, double radiusInKm, string? searchText, CancellationToken cancellationToken)
        {
            var categories = !string.IsNullOrEmpty(searchText)
                ? string.Join(",", HealthcareCategories)
                : string.Join(",", HealthcareCategories);

            return _mapService.GetNearbyFromMapAsync(
                userPoint.Y, userPoint.X,
                categories, cancellationToken,
                radiusInKm * 1000);
        }

        private Task<List<ClinicExternalDto>> GetExternalTextSearchAsync(string searchText, Point? userPoint, double radiusInKm, string? specializationId, CancellationToken cancellationToken)
        {
            var query = searchText;

            if (!string.IsNullOrEmpty(specializationId))
            {
                query = $"{searchText} {specializationId}";
            }

            return _mapService.TextSearchAsync(
                query,
                userPoint?.Y, userPoint?.X,
                radiusInKm * 1000,
                cancellationToken);
        }

        private async Task<List<ClinicExternalDto>> GeocodeAndSearchAsync(string searchText, double radiusInKm, CancellationToken cancellationToken)
        {
            var geocodeResults = await _mapService.GeocodeAsync(searchText, cancellationToken, 1);
            var firstMatch = geocodeResults.FirstOrDefault();
            if (firstMatch != null)
            {
                return await _mapService.TextSearchAsync(
                    searchText,
                    firstMatch.Lat, firstMatch.Lng,
                    radiusInKm * 1000,
                    cancellationToken);
            }
            return [];
        }

        private async Task<Guid?> ResolveSpecializationId(string? specializationIdInput, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(specializationIdInput))
                return null;

            if (Guid.TryParse(specializationIdInput, out var guid))
                return guid;

            var spec = await _unitOfWork.SpecializationRepository
                .GetFirstAsync(s => s.Name == specializationIdInput || s.ArName == specializationIdInput, cancellationToken);
            return spec?.Id;
        }

        private async Task<IEnumerable<Clinic>> GetInternalClinicsAsync(GetHybridSearchQuery request, string? normalizedSearchText, Guid? specializationId, CancellationToken cancellationToken)
        {
            if (request.IsNearest && request.UserLat.HasValue && request.UserLng.HasValue)
            {
                var userPoint = new Point(request.UserLng.Value, request.UserLat.Value) { SRID = 4326 };
                var radiusInMeters = request.RadiusInKm * 1000;
                return await _unitOfWork.ClinicRepository.GetWithinDistanceAsync(userPoint, radiusInMeters, specializationId, cancellationToken);
            }

            var internalQuery = _unitOfWork.ClinicRepository.GetAllWithIncluding(
                c => c.IsActive && !c.IsDeleted &&
                     (string.IsNullOrEmpty(normalizedSearchText) ||
                      c.Name.Contains(request.SearchText!) ||
                      (c.NameAr != null && c.NameAr.Contains(request.SearchText!)) ||
                      c.Specialization.Name.Contains(request.SearchText!) ||
                      c.Specialization.ArName.Contains(request.SearchText!)) &&
                     (!specializationId.HasValue || c.SpecializationId == specializationId),
                c => c.Specialization);

            return await internalQuery.ToListAsync(cancellationToken);
        }

        private static List<ClinicDto> ScoreAndRank(IEnumerable<ClinicDto> results, string? normalizedSearchText, Point? userPoint)
        {
            return results
                .Select(c =>
                {
                    double score = 0;

                    if (c.IsRegistered)
                        score += 100;

                    if (!string.IsNullOrEmpty(normalizedSearchText))
                    {
                        var normalizedName = c.Name.NormalizeArabic();

                        if (string.Equals(normalizedName, normalizedSearchText, StringComparison.OrdinalIgnoreCase))
                            score += 50;
                        else if (normalizedName.StartsWith(normalizedSearchText, StringComparison.OrdinalIgnoreCase))
                            score += 30;
                        else if (normalizedName.Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase))
                            score += 15;

                        if (c.SpecializationName != null &&
                            c.SpecializationName.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase))
                            score += 10;
                    }

                    if (c.Distance > 0)
                    {
                        score += Math.Max(0, 20 - (c.Distance / 1000));
                    }

                    return new { Dto = c, Score = score };
                })
                .OrderByDescending(x => x.Score)
                .ThenBy(x => x.Dto.Distance)
                .Select(x => x.Dto)
                .ToList();
        }

        private static double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
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
