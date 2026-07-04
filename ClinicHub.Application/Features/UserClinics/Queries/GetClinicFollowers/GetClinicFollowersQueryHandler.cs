using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClinicHub.Application.Features.UserClinics.DTOs;
using ClinicHub.Domain.Entities;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace ClinicHub.Application.Features.UserClinics.Queries.GetClinicFollowers
{
    public class GetClinicFollowersQueryHandler : IRequestHandler<GetClinicFollowersQuery, List<ClinicFollowerDto>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetClinicFollowersQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<ClinicFollowerDto>> Handle(GetClinicFollowersQuery request, CancellationToken cancellationToken)
        {
            var followers = await _unitOfWork.GetRepository<UserClinic, Guid>()
                .GetAllAsync(uc => uc.ClinicId == request.ClinicId)
                .OrderByDescending(uc => uc.FollowedAt)
                .ProjectTo<ClinicFollowerDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return followers;
        }
    }
}
