using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using z.FileUpload.Data;

namespace z.FileUpload.Repositories
{
    public class MemoryRepository : FileRepository
    {
        private Dictionary<string, Dictionary<int, byte[]>> internalStorage;

        public MemoryRepository()
        {
            internalStorage = new Dictionary<string, Dictionary<int, byte[]>>();
        }

        public override void Persist(string id, int chunkNumber, byte[] buffer)
        {
            if (!internalStorage.ContainsKey(id))
            {
                internalStorage.Add(id, new Dictionary<int, byte[]>());
            }

            Dictionary<int, byte[]> blocks = internalStorage[id];
            blocks.Add(chunkNumber, buffer);
        }

        public override byte[] Read(string id, int chunkNumber, bool removeOnRead)
        {
            if (!internalStorage.ContainsKey(id))
            {
                throw new Exception("Session not found on internalStorage");
            }

            var fileData = internalStorage[id][chunkNumber];
            if (removeOnRead)
                internalStorage[id].Remove(chunkNumber);

            return fileData;
        }

        public override void RebuildFile(FileSession fileSession)
        {
            //
        }

        public override void WriteToStream(Stream stream, string fileName)
        {
            //
        }
    }
}
