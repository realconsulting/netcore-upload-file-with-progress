using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using z.FileUpload.Data;

namespace z.FileUpload.Interfaces
{
    public interface IUploadService
    {
        FileSession createSession(long user, String fileName, int chunkSize, long fileSize);
        FileSession getSession(String id);
        List<FileSession> getAllSessions();
        void persistBlock(String sessionId, long userId, int chunkNumber, byte[] buffer);
        void WriteToStream(Stream stream, FileSession FileSession);
        Stream GetFileStream(FileSession FileSession);
        void removeFromMemory(string sessionId);
    }
}
