using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using z.FileUpload.Data;
using z.FileUpload.Interfaces;

namespace z.FileUpload.Repositories
{
    public abstract class FileRepository : IFileRepository
    {
        public abstract void Persist(string id, int chunkNumber, byte[] buffer);

        public abstract byte[] Read(string id, int chunkNumber, bool removeOnRead);

        public abstract void RebuildFile(FileSession fileSession);
        public abstract void WriteToStream(Stream stream, string fileName);

        public virtual void WriteToStream(Stream stream, FileSession FileSession)
        {
            using (var sw = new BinaryWriter(stream))
            {
                for (int i = 1; i <= FileSession.FileInfo.TotalNumberOfChunks; i++)
                {
                    sw.Write(Read(FileSession.Id, i, FileSession.RemoveChunkOnRead));
                }
            }

            if (stream.CanRead)
                stream.Flush();
        }

        public virtual Stream GetFileStream(FileSession FileSession)
        {
            return new ChunkedFileStream(this, FileSession);
        }

        private class ChunkedFileStream : Stream
        {
            public override long Position { get; set; }
            public override long Length
            {
                get
                {
                    return FileSession.FileInfo.FileSize;
                }
            }
            public override bool CanWrite { get; } = false;

            public override bool CanSeek { get; }
            public override bool CanRead { get; } = true;

            private Dictionary<long, byte[]> ChunkCache { get; set; } = new Dictionary<long, byte[]>();

            public ChunkedFileStream(FileRepository repository, FileSession FileSession)
            {
                this.Repository = repository;
                this.FileSession = FileSession;

            }
            public FileRepository Repository { get; private set; }

            public FileSession FileSession { get; private set; }

            public override void Flush() { }

            public override int Read(byte[] buffer, int offset, int count)
            {
                var bytesRead = 0;

                for (int i = 0; i < count; i++)
                {
                    byte b;

                    if (TryReadByte(Position, out b))
                    {
                        buffer[i] = b;
                        Position++;
                        bytesRead++;
                    }
                    else
                    {
                        return bytesRead;
                    }
                }

                return bytesRead;
            }

            private bool TryReadByte(long byteIndex, out byte b)
            {
                b = 0;

                if (byteIndex >= FileSession.FileInfo.FileSize)
                    return false;

                // calculate chunk index by byte number
                long chunkNumber = (byteIndex / FileSession.FileInfo.ChunkSize) + 1;

                if (!ChunkCache.ContainsKey(chunkNumber))
                {
                    ChunkCache.Clear();
                    ChunkCache.Add(chunkNumber, Repository.Read(FileSession.Id, (int)chunkNumber, false));
                }

                // get the i-th byte inside that chunk
                b = ChunkCache[chunkNumber][byteIndex % FileSession.FileInfo.ChunkSize];
                return true;
            }

            public override long Seek(long offset, SeekOrigin origin)
            {
                throw new NotSupportedException();
            }
            public override void SetLength(long value)
            {
                throw new NotSupportedException();
            }
            public override void Write(byte[] buffer, int offset, int count)
            {
                throw new NotSupportedException();
            }
        }
    }
}
