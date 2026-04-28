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
            var finalResults = new List<ClinicDto>();
            var seenNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            Point? userPoint = null;
            var normalizedSearchText = request.SearchText?.NormalizeArabic();
            List<ClinicExternalDto> initialGeocodedResults = null!;

            // 1. Handle missing coordinates and initial geocoding
            if (!string.IsNullOrEmpty(request.SearchText) && (!request.UserLat.HasValue || !request.UserLng.HasValue))
            {
                initialGeocodedResults = await _mapService.GeocodeAsync(request.SearchText, cancellationToken, 10);
                
                foreach (var geocoded in initialGeocodedResults)
                {
                    if (seenNames.Add(geocoded.Name))
                    {
                        finalResults.Add(new ClinicDto
                        {
                            Name = geocoded.Name,
                            NameAr = geocoded.NameAr,
                            Address = geocoded.Address,
                            Lat = geocoded.Lat,
                            Lng = geocoded.Lng,
                            IsRegistered = false,
                            Distance = 0
                        });
                    }
                }

                var firstMatch = initialGeocodedResults.FirstOrDefault();
                if (firstMatch != null)
                {
                    request.UserLat = firstMatch.Lat;
                    request.UserLng = firstMatch.Lng;
                }
            }

            if (request.UserLat.HasValue && request.UserLng.HasValue)
            {
                userPoint = new Point(request.UserLng.Value, request.UserLat.Value) { SRID = 4326 };
            }

            // 2. Parallel Search
            if (request.IsNearest && userPoint != null)
            {
                var radiusInMeters = request.RadiusInKm * 1000;
                
                Task<IEnumerable<Clinic>> internalSearchTask = _unitOfWork.ClinicRepository.GetWithinDistanceAsync(userPoint, radiusInMeters, request.SpecializationId, cancellationToken);
                
                string category = "hospital,clinic,doctors,dentist,health_post,medical_centre";
                Task<List<ClinicExternalDto>> externalSearchTask = _mapService.GetNearbyFromMapAsync(request.UserLat!.Value, request.UserLng!.Value, category, cancellationToken, radiusInMeters);
                
                await Task.WhenAll(internalSearchTask, externalSearchTask);

                var internalClinics = await internalSearchTask;
                var externalResults = await externalSearchTask;

                foreach (var clinic in internalClinics)
                {
                    if (!string.IsNullOrEmpty(normalizedSearchText) && 
                        !clinic.Name.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) && 
                        !(clinic.NameAr?.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                        continue;

                    if (seenNames.Add(clinic.Name))
                    {
                        var distance = CalculateDistance(userPoint.Y, userPoint.X, clinic.Location.Y, clinic.Location.X);
                        var dto = _mapper.Map<ClinicDto>(clinic);
                        dto.Distance = distance;
                        finalResults.Add(dto);
                    }
                }

                foreach (var external in externalResults)
                {
                    if (!string.IsNullOrEmpty(normalizedSearchText) && 
                        !external.Name.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) && 
                        !(external.NameAr?.NormalizeArabic().Contains(normalizedSearchText, StringComparison.OrdinalIgnoreCase) ?? false))
                        continue;

                    var distance = CalculateDistance(userPoint.Y, userPoint.X, external.Lat, external.Lng);
                    if (distance > radiusInMeters) continue;

                    if (seenNames.Add(external.Name))
                    {
                        finalResults.Add(new ClinicDto
                        {
                            Name = external.Name,
                            NameAr = external.NameAr,
                            Address = external.Address,
                            AddressAr = external.AddressAr,
                            Lat = external.Lat,
                            Lng = external.Lng,
                            IsRegistered = false,
                            Distance = distance
                        });
                    }
                }
            }
            else
            {
                var internalQuery = _unitOfWork.ClinicRepository.GetAllWithIncluding(
                    c => (string.IsNullOrEmpty(request.SearchText) || c.Name.Contains(request.SearchText) || (c.NameAr != null && c.NameAr.Contains(request.SearchText))) &&
                         (!request.SpecializationId.HasValue || c.SpecializationId == request.SpecializationId),
                    c => c.Specialization);

                Task<List<Clinic>> internalClinicsTask = internalQuery.ToListAsync(cancellationToken);
                
                Task<List<ClinicExternalDto>> externalSearchTask;
                if (initialGeocodedResults != null)
                {
                    externalSearchTask = Task.FromResult(initialGeocodedResults);
                }
                else if (!string.IsNullOrEmpty(request.SearchText))
                {
                    externalSearchTask = _mapService.GeocodeAsync(request.SearchText, cancellationToken, 10);
                }
                else
                {
                    externalSearchTask = Task.FromResult(new List<ClinicExternalDto>());
                }

                await Task.WhenAll(internalClinicsTask, externalSearchTask);

                var internalClinics = await internalClinicsTask;
                var externalResults = await externalSearchTask;

                foreach (var clinic in internalClinics)
                {
                    if (seenNames.Add(clinic.Name))
                    {
                        var dto = _mapper.Map<ClinicDto>(clinic);
                        if (userPoint != null)
                        {
                            dto.Distance = CalculateDistance(userPoint.Y, userPoint.X, clinic.Location.Y, clinic.Location.X);
                        }
                        finalResults.Add(dto);
                    }
                }

                foreach (var ext in externalResults)
                {
                    if (seenNames.Add(ext.Name))
                    {
                        finalResults.Add(new ClinicDto
                        {
                            Name = ext.Name,
                            NameAr = ext.NameAr,
                            Address = ext.Address,
                            Lat = ext.Lat,
                            Lng = ext.Lng,
                            IsRegistered = false,
                            Distance = userPoint != null ? CalculateDistance(userPoint.Y, userPoint.X, ext.Lat, ext.Lng) : 0
                        });
                    }
                }
            }

            var orderedResults = finalResults
                .OrderByDescending(c => c.IsRegistered)
                .ThenBy(c => c.Distance)
                .ToList();

            var pagedData = orderedResults
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToList();

            return new PagginatedResult<ClinicDto>(pagedData, finalResults.Count, request.PageNumber, request.PageSize);
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
