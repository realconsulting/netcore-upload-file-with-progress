using System;
using System.Collections.Generic;
using System.Text;

namespace z.FileUpload.Data
{
    public class FileUploadOptions
    {
        /// <summary>
        /// <RootPath>/files_store
        /// </summary>
        public string RootPath { get; set; } = "./files_store";

        /// <summary>
        /// Chunk Limit equals to 1024 * 1024
        /// </summary>
        public int ChunkLimit { get; set; } = 1024 * 1024;

        /// <summary>
        /// Rebuild file to its original structure
        /// </summary>
        public bool RebuildFile { get; set; } = false;
    }
}
