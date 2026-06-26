using AutoMapper;
using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Features.Clinics.DTOs;
using ClinicHub.Application.Localization;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Clinics.Queries.GetClinicByIdForUser
{
    public class GetClinicByIdForUserQueryHandler : IRequestHandler<GetClinicByIdForUserQuery, ClinicManagementDto>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetClinicByIdForUserQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<ClinicManagementDto> Handle(GetClinicByIdForUserQuery request, CancellationToken cancellationToken)
        {
            var clinic = await _unitOfWork.ClinicRepository
                .GetAllAsync(c => c.Id == request.Id)
                .Include(c => c.Specialization)
                .Include(c => c.ClinicAdmin)
                .FirstOrDefaultAsync(cancellationToken);

            if (clinic == null)
                throw new NotFoundException(LocalizationKeys.ClinicMessages.ClinicNotFound.Value);

            return _mapper.Map<ClinicManagementDto>(clinic);
        }
    }
}
