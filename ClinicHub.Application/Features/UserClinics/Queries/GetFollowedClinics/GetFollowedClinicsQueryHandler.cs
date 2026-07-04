using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Common.Interfaces;
using ClinicHub.Application.Features.UserClinics.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.UserClinics.Queries.GetFollowedClinics
{
    public class GetFollowedClinicsQueryHandler : IRequestHandler<GetFollowedClinicsQuery, List<FollowedClinicDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ICurrentUserService _currentUser;

        public GetFollowedClinicsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _currentUser = currentUser;
        }

        public async Task<List<FollowedClinicDto>> Handle(GetFollowedClinicsQuery request, CancellationToken cancellationToken)
        {
            var clinics = await _unitOfWork.GetRepository<UserClinic, Guid>()
                .GetAllAsync(uc => uc.UserId == _currentUser.UserId)
                .OrderByDescending(uc => uc.FollowedAt)
                .ProjectTo<FollowedClinicDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return clinics;
        }
    }
}
