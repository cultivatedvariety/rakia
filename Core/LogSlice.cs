using System;
using System.IO;
using System.Text;

namespace Core
{
    public class LogSlice : ILogSlice
    {
        public string DirectoryPath { get; }
        public string FileName { get; }
        public long Size { get; }
        public string SliceFilePath => Path.Combine(DirectoryPath, FileName);
        
        public ISliceIndex Index { get; }

        private StreamWriter _sliceWriter;
        private StreamWriter _indexWriter;
        
        public LogSlice(string directoryPath, string fileName)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentNullException(nameof(directoryPath));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            
            DirectoryPath = directoryPath;
            FileName = fileName;
        }


        public long Append(byte[] key, byte[] value)
        {
            throw new System.NotImplementedException();
        }

        public long? Contains(byte[] key)
        {
            throw new System.NotImplementedException();
        }

        public (byte[] key, byte[] value) Read(long seekPosition)
        {
            throw new System.NotImplementedException();
        }

        /**
         * Close the log segment
         */
        public void Close()
        {
            
        }

        private void InitialiseDirectory()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
        }

        private void InitialiseSlice()
        {
            _sliceWriter = new StreamWriter(SliceFilePath, true, Encoding.UTF8);
            /* TODO - run through recovery
                 - check index file and ensure all entries in slice are up-to-date and accounted for.
                 - verify that no corrupted entries exist in slice (headers are all in place and length is correct
             */ 
            
        }
    }
}