using System.IO;
using z.FileUpload.Data;

namespace z.FileUpload.Interfaces
{
    public interface IFileRepository
    {
        void Persist(string id, int chunkNumber, byte[] buffer);
        byte[] Read(string id, int chunkNumber, bool removeOnRead);
        void WriteToStream(Stream stream, FileSession session);
        void WriteToStream(Stream stream, string fileName);
        Stream GetFileStream(FileSession session);
        void RebuildFile(FileSession fileSession);
    }
}
