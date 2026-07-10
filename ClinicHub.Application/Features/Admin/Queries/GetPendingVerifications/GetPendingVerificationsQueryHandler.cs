using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Features.Users.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.Admin.Queries.GetPendingVerifications
{
    public sealed class GetPendingVerificationsQueryHandler : IRequestHandler<GetPendingVerificationsQuery, IReadOnlyList<UserVerificationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPendingVerificationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IReadOnlyList<UserVerificationDto>> Handle(GetPendingVerificationsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.UserVerificationRepository
                .GetAllAsync(v => v.Status == VerificationStatus.Pending)
                .ProjectTo<UserVerificationDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
