using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using z.FileUpload.Data;
using z.FileUpload.Exceptions;
using z.FileUpload.Interfaces;

namespace z.FileUpload.Services
{
    public class UploadService : IUploadService
    {

        private readonly FileUploadOptions Options;

        Dictionary<String, FileSession> sessions;
        IFileRepository fileStorage;

        public UploadService(IFileRepository storage, FileUploadOptions options)
        {
            this.fileStorage = storage;
            sessions = new Dictionary<String, FileSession>();
            Options = options;
        }

        public FileSession createSession(long user, String fileName, int chunkSize, long fileSize)
        {

            if (String.IsNullOrWhiteSpace(fileName))
                throw new BadRequestException("File name missing");



            if (chunkSize > Options.ChunkLimit)
                throw new BadRequestException(String.Format("Maximum chunk size is {0} bytes", Options.ChunkLimit));

            if (chunkSize < 1)
                throw new BadRequestException("Chunk size must be greater than zero");

            if (fileSize < 1)
                throw new BadRequestException("Total size must be greater than zero");

            FileSession FileSession = new FileSession(user, new FileInformation(fileSize, fileName, chunkSize));
            sessions.Add(FileSession.Id, FileSession);


            return FileSession;
        }

        public FileSession getSession(String id)
        {
            return sessions[id];
        }

        public List<FileSession> getAllSessions()
        {
            return sessions.Values.ToList();
        }

        public void persistBlock(String sessionId, long userId, int chunkNumber, byte[] buffer)
        {
            FileSession FileSession = getSession(sessionId);

            try
            {
                if (FileSession == null)
                {
                    throw new NotFoundException("FileSession not found");
                }

                fileStorage.Persist(sessionId, chunkNumber, buffer);

                FileSession.FileInfo.MarkChunkAsPersisted(chunkNumber);
                FileSession.RenewTimeout();

                if (FileSession.IsConcluded && Options.RebuildFile)
                {
                    fileStorage.RebuildFile(FileSession); 
                }
            }
            catch (System.Exception e)
            {
                if (FileSession != null)
                    FileSession.MaskAsFailed();

                throw e;
            }
        }

        public void WriteToStream(Stream stream, FileSession FileSession)
        {
            if (Options.RebuildFile)
            {
                fileStorage.WriteToStream(stream, FileSession.FileInfo.FileName);
            }
            else
                fileStorage.WriteToStream(stream, FileSession);
        }

        public Stream GetFileStream(FileSession FileSession)
        {
            return fileStorage.GetFileStream(FileSession);
        }

        public void removeFromMemory(string sessionId)
        {
            sessions.Remove(sessionId);
        }
    }
}
