using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using z.FileUpload.Data;
using z.FileUpload.Interfaces;
using z.FileUpload.Models;

namespace z.FileUpload
{
    [Route("FileUpload")]
    [EnableCors("FileUploadPolicy")]
    public class FileUploadController : Controller
    {
        private IUploadService uploadService;

        public FileUploadController(IUploadService _uploadService)
        {
            uploadService = _uploadService;
        }

        /// <summary>
        /// Create an upload session
        /// </summary>
        /// <remarks>creates a new upload session</remarks>
        /// <param name="userId">User ID</param>
        /// <param name="sessionParams">Session creation params</param>
        [HttpPost("create/{userId}")]
        [Produces("application/json")]
        public SessionCreationStatusResponse StartSession([FromRoute] long userId, [FromForm] CreateSessionParams sessionParams)
        {

            FileSession session = uploadService.createSession(userId, sessionParams.FileName,
                                                          sessionParams.ChunkSize.Value,
                                                          sessionParams.TotalSize.Value);

            return SessionCreationStatusResponse.fromSession(session);
        }

        /// <summary>
        /// Uploads a file chunk
        /// </summary>
        /// <remarks>uploads a file chunk</remarks>
        /// <param name="userId">User ID</param>
        /// <param name="sessionId">Session ID</param>
        /// <param name="chunkNumber">Chunk number (starts from 1)</param>
        /// <param name="inputFile">File chunk content</param>
        [HttpPut("upload/user/{userId}/session/{sessionId}/")]
        [Produces("application/json")]
        [Consumes("multipart/form-data")]
        public JsonResult UploadFileChunk([FromRoute, Required] long? userId, [FromRoute, Required] string sessionId, [FromQuery, Required] int? chunkNumber, [FromForm] IFormFile inputFile)
        {
            if (!userId.HasValue)
                return badRequest("User missing");

            if (String.IsNullOrWhiteSpace(sessionId))
                return badRequest("Session ID is missing");

            if (chunkNumber < 1)
                return badRequest("Invalid chunk number");

            // due to a bug, inputFile comes null from Mvc
            // however, I want to test the code and have to pass it to the UploadFileChunk function...
            IFormFile file = (inputFile ?? Request.Form.Files.First());

            uploadService.persistBlock(sessionId, userId.Value, chunkNumber.Value, ToByteArray(file.OpenReadStream()));

            return Json("Ok");
        }

        /// <summary>
        /// Gets the status of a single upload
        /// </summary>
        /// <remarks>gets the status of a single upload</remarks>
        /// <param name="sessionId">Session ID</param>
        [HttpGet("upload/{sessionId}")]
        [Produces("application/json")]
        public UploadStatusResponse GetUploadStatus([FromRoute, Required] string sessionId)
        {
            return UploadStatusResponse.fromSession(uploadService.getSession(sessionId));
        }

        /// <summary>
        /// Gets the status of all uploads
        /// </summary>
        /// <remarks>gets the status of all uploads</remarks>
        [HttpGet("uploads")]
        [Produces("application/json")]
        public List<UploadStatusResponse> GetAllUploadStatus()
        {
            return UploadStatusResponse.fromSessionList(uploadService.getAllSessions());
        }

        /// <summary>
        /// Downloads a previously uploaded file
        /// </summary>
        /// <param name="sessionId">Session ID</param>
        /// <remarks>downloads a previously uploaded file</remarks>
        [HttpGet("download/{sessionId}")]
        [Produces("multipart/form-data")]
        public void DownloadFile([FromRoute, Required] string sessionId)
        {
            FileSession session = uploadService.getSession(sessionId);

            var response = Response;
            response.ContentType = "application/octet-stream";
            response.ContentLength = session.FileInfo.FileSize;
            response.Headers["Content-Disposition"] = "attachment; fileName=" + session.FileInfo.FileName;

            uploadService.WriteToStream(Response.Body, session);
        }

        [HttpGet("uploadedFile/{sessionId}")]
        [Produces("application/json")]
        public FileSession GetUploadedFile([FromRoute, Required] string sessionId)
        {
            return uploadService.getSession(sessionId);
        }

        [HttpGet("removeFileMemory/{sessionId}")]
        [Produces("application/json")]
        public void RemoveSessionFromMemory([FromRoute, Required] string sessionId)
        {
              uploadService.removeFromMemory(sessionId);
        }

        private byte[] ToByteArray(Stream stream)
        {
            using (MemoryStream ms = new MemoryStream())
            {
                stream.CopyTo(ms);
                return ms.ToArray();
            }
        }

        private JsonResult badRequest(string message)
        {
            var result = new JsonResult("{'message': '" + message + "' }");
            result.StatusCode = 400;
            return result;
        }
    }
}
