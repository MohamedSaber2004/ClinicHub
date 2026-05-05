using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UploadAudio
{
    public class UploadAudioCommandHandler : IRequestHandler<UploadAudioCommand, string>
    {
        private readonly IAudioValidator _audioValidator;

        public UploadAudioCommandHandler(IAudioValidator audioValidator)
        {
            _audioValidator = audioValidator;
        }

        public async Task<string> Handle(UploadAudioCommand request, CancellationToken cancellationToken)
        {
            var (uploaded, result) = await _audioValidator.UploadAudio(request.File, request.Place);

            if (!uploaded)
                throw new BadRequestException(result);

            return result;
        }
    }
}
