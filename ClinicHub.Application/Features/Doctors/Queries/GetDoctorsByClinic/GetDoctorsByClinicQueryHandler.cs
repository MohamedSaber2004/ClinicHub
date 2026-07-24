using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Doctors.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Doctors.Queries.GetDoctorsByClinic
{
    public class GetDoctorsByClinicQueryHandler : IRequestHandler<GetDoctorsByClinicQuery, PagginatedResult<DoctorDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetDoctorsByClinicQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<DoctorDto>> Handle(GetDoctorsByClinicQuery request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository.ExistsAsync(c => c.Id == request.ClinicId, cancellationToken);
            if (!clinic)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            IQueryable<Doctor> query = _unitOfWork.DoctorRepository
                .GetAllAsync(d => d.ClinicId == request.ClinicId)
                .IgnoreQueryFilters()
                .Include(d => d.User)
                .Include(d => d.Clinic)
                .Include(d => d.Specialization);

            if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            {
                var term = request.SearchTerm.ToLower();
                query = query.Where(d =>
                    (d.User != null && d.User.FullName.ToLower().Contains(term)) ||
                    (d.User != null && d.User.Email != null && d.User.Email.ToLower().Contains(term)));
            }

            if (request.SpecializationId.HasValue)
                query = query.Where(d => d.SpecializationId == request.SpecializationId.Value);

            var totalCount = await query.CountAsync(cancellationToken);

            var doctors = await query
                .OrderByDescending(d => d.CreatedAt)
                .Skip((request.PageNumber - 1) * request.PageSize)
                .Take(request.PageSize)
                .ToListAsync(cancellationToken);

            var items = _mapper.Map<List<DoctorDto>>(doctors);
            return new PagginatedResult<DoctorDto>(items, totalCount, request.PageNumber, request.PageSize);
        }
    }
}
