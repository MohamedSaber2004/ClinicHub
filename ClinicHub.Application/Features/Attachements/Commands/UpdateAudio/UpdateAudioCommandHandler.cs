using ClinicHub.Application.Common.Exceptions;
using ClinicHub.Application.Common.Interfaces;
using MediatR;

namespace ClinicHub.Application.Features.Attachements.Commands.UpdateAudio
{
    public class UpdateAudioCommandHandler : IRequestHandler<UpdateAudioCommand, string>
    {
        private readonly IAudioValidator _audioValidator;

        public UpdateAudioCommandHandler(IAudioValidator audioValidator)
        {
            _audioValidator = audioValidator;
        }

        public async Task<string> Handle(UpdateAudioCommand request, CancellationToken cancellationToken)
        {
            if (!string.IsNullOrEmpty(request.OldFileName))
            {
                await _audioValidator.DeleteAudio(request.OldFileName, request.Place);
            }

            var (uploaded, result) = await _audioValidator.UploadAudio(request.File, request.Place);

            if (!uploaded)
                throw new BadRequestException(result);

            return result;
        }
    }
}
