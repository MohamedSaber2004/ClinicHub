using Asp.Versioning;
using ClinicHub.Application.Common.Models;
using ClinicHub.API.Routes;
using ClinicHub.Application.Features.Attachements.Commands.DownloadFile;
using ClinicHub.Application.Features.Attachements.Commands.UpdateAudio;
using ClinicHub.Application.Features.Attachements.Commands.UpdateFile;
using ClinicHub.Application.Features.Attachements.Commands.UpdateImage;
using ClinicHub.Application.Features.Attachements.Commands.UpdateVideo;
using ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Images;
using ClinicHub.Application.Features.Attachements.Commands.Upload_Multi_Videos;
using ClinicHub.Application.Features.Attachements.Commands.UploadAudio;
using ClinicHub.Application.Features.Attachements.Commands.UploadFile;
using ClinicHub.Application.Features.Attachements.Commands.UploadImage;
using ClinicHub.Application.Features.Attachements.Commands.UploadVideo;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace ClinicHub.API.Controllers.Version1
{
    [ApiVersion("1.0")]
    //[Authorize]
    public class AttachementsController : BaseApiController
    {
        public AttachementsController(IMediator mediator) : base(mediator)
        {

        }

        /// <summary>
        /// Upload an image file to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Attachments.UploadImage)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadImage([FromForm] UploadImageCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Upload an audio file to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Attachments.UploadAudio)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadAudio([FromForm] UploadAudioCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Upload a video file to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Attachments.UploadVideo)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadVideo([FromForm] UploadVideoCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Upload a file to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Attachments.UploadFile)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadFile([FromForm] UploadFileCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Update an image file on the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route(ApiRoutes.Attachments.UpdateImage)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateImage(string name, [FromForm] UpdateImageCommand command)
        {
            command.OldFileName = name;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Update an audio file on the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route(ApiRoutes.Attachments.UpdateAudio)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateAudio(string name, [FromForm] UpdateAudioCommand command)
        {
            command.OldFileName = name;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Update a file on the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route(ApiRoutes.Attachments.UpdateFile)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateFile(string name, [FromForm] UpdateFileCommand command)
        {
            command.OldFileName = name;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Update a video file on the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPatch]
        [Route(ApiRoutes.Attachments.UpdateVideo)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateVideo(string name, [FromForm] UpdateVideoCommand command)
        {
            command.OldFileName = name;
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Download file from the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Attachments.DownloadFile)]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DownloadFile([FromForm] DownloadFileCommand command)
        {
            var result = await _mediator.Send(command);

            if (result.Success)
            {
                return PhysicalFile(result.FilePath, result.ContentType, result.FileName);
            }

            return BadRequest(result.ErrorMessage);
        }

        /// <summary>
        /// Upload list of images to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Attachments.UploadMultipleImages)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMultipleImages([FromForm]UploadMultipleImagesCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        /// <summary>
        /// Upload list of videos to the server.
        /// </summary>
        /// <param name="command"></param>
        /// <returns></returns>
        [HttpPost]
        [Route(ApiRoutes.Attachments.UploadMultipleVideos)]
        [Consumes("multipart/form-data")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UploadMultipleVideos([FromForm] UploadMultiVideosCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }
    }
}
