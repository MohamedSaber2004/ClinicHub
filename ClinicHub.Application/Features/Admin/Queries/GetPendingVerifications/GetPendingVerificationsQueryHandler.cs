using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Extensions;
using ClinicHub.Application.Common.Models;
using ClinicHub.Application.Features.Users.DTOs;
using ClinicHub.Domain.Enums;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Admin.Queries.GetPendingVerifications
{
    public sealed class GetPendingVerificationsQueryHandler : IRequestHandler<GetPendingVerificationsQuery, PagginatedResult<UserVerificationDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetPendingVerificationsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<PagginatedResult<UserVerificationDto>> Handle(GetPendingVerificationsQuery request, CancellationToken cancellationToken)
        {
            return await _unitOfWork.UserVerificationRepository
                .GetAllAsync(v => v.Status == VerificationStatus.Pending)
                .ProjectTo<UserVerificationDto>(_mapper.ConfigurationProvider)
                .AsPagginatedListAsync(request.PageNumber, request.PageSize, cancellationToken);
        }
    }
}
