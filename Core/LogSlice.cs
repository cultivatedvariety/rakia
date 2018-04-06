using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.IO;
using System.Text;
using Core.Metrics;

namespace Core
{
    /*
     * Implement tation of ILogSlice based on LSM data structure.
     * The format of data in the file for a single entry is:
     * <key length: 4 bytes><value length: 4 bytes><key: x bytes><value: x bytes><terminator: 2 bytes>
     */
    public class LogSlice : ILogSlice, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        public long Size { get; }
        public string SliceFilePath { get; }

        private readonly ILogSliceIndex _index;
        private readonly FileStream _fileStream;
        private readonly byte[] _terminatorBytes = BitConverter.GetBytes('\0');
        private readonly ILogSliceMetricsRecorder _metricsRecorder;
        
        public LogSlice(string sliceFilePath, ILogSliceIndex index, ILogSliceMetricsRecorder metricsRecorder)
        {
            if (string.IsNullOrWhiteSpace(sliceFilePath))
                throw new ArgumentNullException(nameof(sliceFilePath));
            SliceFilePath = sliceFilePath;
            _metricsRecorder = metricsRecorder;
            _index = index;
            
            _fileStream = new FileStream(SliceFilePath, FileMode.OpenOrCreate);
        }


        public void Append(byte[] key, byte[] value)
        {
            try
            {
                _metricsRecorder.AppendStarted();
                long pos = _fileStream.Position;
                var keyLengthBytes = BitConverter.GetBytes(key.Length);
                var valueLengthBytes = BitConverter.GetBytes(value.Length);
                _fileStream.Write(keyLengthBytes, 0, keyLengthBytes.Length);
                _fileStream.Write(valueLengthBytes, 0, valueLengthBytes.Length);
                _fileStream.Write(key, 0, key.Length);
                _fileStream.Write(value, 0, value.Length);
                _fileStream.Write(_terminatorBytes, 0, _terminatorBytes.Length);
                _fileStream.Flush();
                _index.UpdateIndex(key, pos);
                
            }
            finally
            {
                _metricsRecorder.AppendFinished();
            }
        }

        public bool Contains(byte[] key)
        {
            try
            {
                _metricsRecorder.ContainsStarted();
                return _index.GetSeekPosition(key).HasValue;
            }
            finally
            {
                _metricsRecorder.ContainsFinished();
            }
        }

        public byte[] Get(byte[] key)
        {
            try
            {
                _metricsRecorder.GetStarted();
                long? seekPos = _index.GetSeekPosition(key);
                if (!seekPos.HasValue)
                {
                    return null;
                }

                _fileStream.Seek(seekPos.Value, SeekOrigin.Begin);
                byte[] keyLengthBytes = new byte[4];
                byte[] valueLengthBytes = new byte[4];

                _fileStream.Read(keyLengthBytes, 0, keyLengthBytes.Length);
                _fileStream.Read(valueLengthBytes, 0, valueLengthBytes.Length);

                int keyLength = BitConverter.ToInt32(keyLengthBytes, 0);
                int valueLength = BitConverter.ToInt32(valueLengthBytes, 0);

                _fileStream.Seek(keyLength, SeekOrigin.Current); // skip the key
                byte[] value = new byte[valueLength];
                _fileStream.Read(value, 0, value.Length);

                return value;
            }
            finally
            {
                _metricsRecorder.GetFinished();
            }
        }
        
        /**
         * Close the log slice
         */
        public void Close()
        {
            _index?.Close();
            _fileStream?.Close();
        }
        
        public IEnumerator<KeyValuePair<byte[], byte[]>> GetEnumerator()
        {
            _fileStream.Position = 0;
            byte[] keyLengthBytes = new byte[4];
            byte[] valueLengthBytes = new byte[4];
            while (_fileStream.Position < _fileStream.Length)
            {
                _fileStream.Read(keyLengthBytes, 0, keyLengthBytes.Length);
                _fileStream.Read(valueLengthBytes, 0, valueLengthBytes.Length);
                // TODO: optimise by creating byte arrays once and re-using?
                byte[] key = new byte[BitConverter.ToInt32(keyLengthBytes, 0)];
                byte[] value = new byte[BitConverter.ToInt32(valueLengthBytes, 0)];
                _fileStream.Read(key, 0, key.Length);
                _fileStream.Read(value, 0, value.Length);
                _fileStream.Seek(2, SeekOrigin.Current); // skip the terminator
                yield return new KeyValuePair<byte[], byte[]>(key, value);

            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}