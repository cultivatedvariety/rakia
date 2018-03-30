﻿using System;
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
     * Any value with a length of zero is considered to be deleted (tombstoned)
     */
    public class LogSlice : ILogSlice, IEnumerable<KeyValuePair<byte[], byte[]>>
    {
        public string DirectoryPath { get; }
        public string FileName { get; }
        public long Size { get; }
        public string SliceFilePath => Path.Combine(DirectoryPath, FileName);

        private ILogSliceIndex _index;
        private FileStream _fileStream;
        private readonly byte[] _terminatorBytes = BitConverter.GetBytes('\0');
        private readonly ILogSliceMetricsRecorder _metricsRecorder;
        
        public LogSlice(string directoryPath, string fileName, ILogSliceMetricsRecorder metricsRecorder)
        {
            if (string.IsNullOrWhiteSpace(directoryPath))
                throw new ArgumentNullException(nameof(directoryPath));
            if (string.IsNullOrWhiteSpace(fileName))
                throw new ArgumentNullException(nameof(fileName));
            _metricsRecorder = metricsRecorder;

            DirectoryPath = directoryPath;
            FileName = fileName;
            Initialise();
        }


        public void Append(byte[] key, byte[] value)
        {
            long pos = _fileStream.Position;
            var keyLengthBytes = BitConverter.GetBytes(key.Length);
            var valueLengthBytes = BitConverter.GetBytes(value.Length);
            _fileStream.Write(keyLengthBytes, 0, keyLengthBytes.Length);
            _fileStream.Write(valueLengthBytes, 0, valueLengthBytes.Length);
            _fileStream.Write(key, 0, key.Length);
            _fileStream.Write(value, 0, value.Length);
            _fileStream.Write(_terminatorBytes, 0, _terminatorBytes.Length);
            _fileStream.Flush();
            if (value.Length > 0)
                _index.UpdateIndex(key, pos);
            else 
                _index.RemoveFromIndex(key); //value of length 0 is conidered deleted
        }

        public bool Contains(byte[] key)
        {
            return _index.GetSeekPosition(key).HasValue;
        }

        public byte[] Get(byte[] key)
        {
            long? seekPos = _index.GetSeekPosition(key);
            if (!seekPos.HasValue)
            {
                throw new ArgumentOutOfRangeException(nameof(key), $"key does not exist in {this.GetType().Name}. Use Contains first");
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

        public void Remove(byte[] key)
        {
            Append(key, new byte[0]);
        }

        /**
         * Close the log segment
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

        private void Initialise()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }
            _fileStream = new FileStream(SliceFilePath, FileMode.OpenOrCreate);
            _index = new LogSliceIndex($"{SliceFilePath}.idx", _metricsRecorder);
        }
    }
}