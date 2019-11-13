using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using z.FileUpload.Data;

namespace z.FileUpload.Repositories
{
    public class LocalFileSystemRepository : FileRepository
    {
        private readonly string ROOT;

        public LocalFileSystemRepository(FileUploadOptions options)
        {
            ROOT = options.RootPath;
        }

        public async override void Persist(string id, int chunkNumber, byte[] buffer)
        {
            string chunkDestinationPath = Path.Combine(ROOT, id);

            if (!Directory.Exists(chunkDestinationPath))
            {
                Directory.CreateDirectory(chunkDestinationPath);
            }

            string path = Path.Combine(ROOT, id, chunkNumber.ToString());
            await File.WriteAllBytesAsync(path, buffer);
        }

        public override byte[] Read(string id, int chunkNumber, bool removeOnRead)
        {
            string targetPath = Path.Combine(ROOT, id, chunkNumber.ToString());
            var fileData = File.ReadAllBytes(targetPath);
            if (removeOnRead)
                File.Delete(targetPath);
            return fileData;
        }

        public override void RebuildFile(FileSession fileSession)
        {
            using (var fs = new FileStream(Path.Combine(ROOT, fileSession.FileInfo.FileName), FileMode.Create, FileAccess.Write))
            {
                fileSession.RemoveChunkOnRead = true;
                WriteToStream(fs, fileSession);

                var dir = Path.Combine(ROOT, fileSession.Id);
                if (Directory.Exists(dir))
                    Directory.Delete(dir);
            }
        }

        public override void WriteToStream(Stream stream, string fileName)
        {
            using (var fs = File.OpenRead(Path.Combine(ROOT, fileName)))
            {
                fs.CopyTo(stream);
            } 
            stream.Flush();
        }
    }
}
