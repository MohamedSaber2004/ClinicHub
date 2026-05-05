using AutoMapper;
using ClinicHub.Application.Features.Specializations.DTOs;
using ClinicHub.Infrastructure.UnitOfWork.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Specializations.Queries.GetSpecializationById
{
    public class GetSpecializationByIdQueryHandler : IRequestHandler<GetSpecializationByIdQuery, SpecializationDto?>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetSpecializationByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<SpecializationDto?> Handle(GetSpecializationByIdQuery request, CancellationToken cancellationToken)
        {
            var specialization = await _unitOfWork.SpecializationRepository.GetByIdAsync(request.Id);

            return _mapper.Map<SpecializationDto>(specialization);
        }
    }
}
