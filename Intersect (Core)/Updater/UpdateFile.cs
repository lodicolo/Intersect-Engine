using System;
using System.IO;
using System.IO.Hashing;
using Intersect.IO.Files;
using Newtonsoft.Json;

namespace Intersect.Updater
{
    public class UpdateFile
    {
        public UpdateFile()
        {
        }

        public UpdateFile(
            string baseDirectory,
            FileInfo fileInfo,
            string checksum = default,
            NonCryptographicHashAlgorithm algorithm = default
        )
        {
            if (fileInfo == default)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            Path = FileSystemHelper.RelativePath(baseDirectory, fileInfo.FullName);
            Size = fileInfo.Exists ? fileInfo.Length : -1;
            Hash = checksum ?? ComputeChecksum(fileInfo, algorithm);
        }

        [JsonConstructor]
        public UpdateFile(string path, string hash, long size)
        {
            Path = path;
            Hash = hash;
            Size = size;
        }

        public string Path { get; }

        public string Hash { get; }

        public long Size { get; }

        public bool ClientIgnore { get; set; }
        public bool EditorIgnore { get; set; }

        public static string ComputeChecksum(FileInfo fileInfo, NonCryptographicHashAlgorithm algorithm = default)
        {
            if (fileInfo == default)
            {
                throw new ArgumentNullException(nameof(fileInfo));
            }

            if (algorithm == default)
            {
                algorithm = new Crc64();
            }

            using (var fileStream = fileInfo.OpenRead())
            {
                algorithm.Append(fileStream);
            }

            var data = algorithm.GetHashAndReset();
            return Convert.ToBase64String(data);
        }
    }
}