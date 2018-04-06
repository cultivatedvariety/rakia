using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using App.Metrics;
using App.Metrics.Meter;
using Core.Metrics;

namespace Core
{
    /**
     * Implementation of ISliceIndex that stores the index in a file. The format of data in the file for a single entry is:
     * <key length: 4 bytes><key: x bytes><seek position: 8 bytes><terminator: 2 bytes>
     * - key length: length of upcoming key. int32 in size
     * - key: the key itself
     * - seek position: the seek position associated with the key. int64 in size
     * - terminator: terminator character, indicating the end of the entry. used to validate the file on startup
     */
    public class LogSliceIndex : ILogSliceIndex
    {
        private readonly ISliceIndexMetricsRecorder _metricsRecorder;
        public string FilePath { get; }
        public long FileLength => _fileStream?.Length ?? 0;
        private readonly Dictionary<byte[], long> _keyToValueSeekPositionLocationMap;
        private FileStream _fileStream;
        private readonly byte[] _terminatorBytes = BitConverter.GetBytes('\0');

        public LogSliceIndex(string filePath, ISliceIndexMetricsRecorder metricsRecorder)
        {
            _metricsRecorder = metricsRecorder;
            FilePath = filePath;
            _keyToValueSeekPositionLocationMap = new Dictionary<byte[], long>(5000, new ByteArrayEqualityComparer());
            InitialiseSeekFile();
        }

        public void UpdateIndex(byte[] key, long seekPosition)
        {
            try
            {
                _metricsRecorder.UpdatedStarted();
                if (_keyToValueSeekPositionLocationMap.ContainsKey(key))
                {
                    // update an existing entry by seeking to it's position in the file and overwriting the existing value
                    var updateSeekPosition = _keyToValueSeekPositionLocationMap[key];
                    _fileStream.Seek(updateSeekPosition, SeekOrigin.Begin);
                    var seekPositionBytes = BitConverter.GetBytes(seekPosition);
                    _fileStream.Write(seekPositionBytes, 0, seekPositionBytes.Length);
                }
                else
                {
                    // append a new entry to the file
                    _fileStream.Seek(_fileStream.Length, SeekOrigin.Begin);
                    var keyLengthBytes = BitConverter.GetBytes(key.Length);
                    _fileStream.Write(keyLengthBytes, 0, keyLengthBytes.Length);
                    _fileStream.Write(key, 0, key.Length);

                    var valueSeekPosition = _fileStream.Position;

                    var seekPositionBytes = BitConverter.GetBytes(seekPosition);
                    _fileStream.Write(seekPositionBytes, 0, seekPositionBytes.Length);
                    _fileStream.Write(_terminatorBytes, 0, _terminatorBytes.Length);

                    _keyToValueSeekPositionLocationMap.Add(key, valueSeekPosition);
                }

                _fileStream.Flush(true);
            }
            finally
            {
                _metricsRecorder.UpdateFinished();
            }

        }

        public long? GetSeekPosition(byte[] key)
        {
            try
            {
                _metricsRecorder.GetStarted();
                if (!_keyToValueSeekPositionLocationMap.ContainsKey(key))
                {
                    return null;
                }

                byte[] seekPosition = new byte[8];
                _fileStream.Seek(_keyToValueSeekPositionLocationMap[key], SeekOrigin.Begin);
                _fileStream.Read(seekPosition, 0, seekPosition.Length);

                return BitConverter.ToInt64(seekPosition, 0);
            }
            finally
            {
                _metricsRecorder.GetFinished();
            }
        }

        public long GetMaxKeySeekPosition()
        {
            return _keyToValueSeekPositionLocationMap.Max(kvp => kvp.Value);
        }
        
        public void Close()
        {
            _fileStream?.Close();
        }

        private void InitialiseSeekFile()
        {
            // TODO: need to verify the file is valid at the same time
            _fileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] keyLengthBytes = new byte[4];
            while (_fileStream.Position < _fileStream.Length)
            {
                var lastGoodReadPosition = _fileStream.Position;
                _fileStream.Read(keyLengthBytes, 0, keyLengthBytes.Length);
                var keyLength = BitConverter.ToInt32(keyLengthBytes, 0);
                byte[] key = new byte[keyLength];
                byte[] seekPosition = new byte[8];
                _fileStream.Read(key, 0, key.Length); // could go wrong here with corrupted file
                var valueSeekPosition = _fileStream.Position; 
                
                _fileStream.Read(seekPosition, 0, seekPosition.Length);

                //verify the entry is corrrectly terminated
                byte[] terminatorBytes = new byte[_terminatorBytes.Length];
                var read = _fileStream.Read(terminatorBytes, 0, terminatorBytes.Length);
                if (read != terminatorBytes.Length || !terminatorBytes.SequenceEqual(_terminatorBytes))
                {
                    //corrupted file - missing terminator character
                    _fileStream.Seek(lastGoodReadPosition, SeekOrigin.Begin);
                    _fileStream.SetLength(lastGoodReadPosition); //erase the rest of the file
                    _fileStream.Flush();
                }
                
                _keyToValueSeekPositionLocationMap.Add(key, valueSeekPosition);
            }
        }

        private class ByteArrayEqualityComparer : IEqualityComparer<byte[]>
        {
            public bool Equals(byte[] x, byte[] y)
            {
                if (x == null || y == null)
                    return false;
                if (x.Length != y.Length)
                    return false;
                for (int i = 0; i < x.Length; i++)
                {
                    if (x[i] != y[i])
                        return false;
                }

                return true;
            }

            public int GetHashCode(byte[] obj)
            {
                var result = 0;
                foreach (byte b in obj)
                    result = (result*31) ^ b;
                return result;
            }
        }
    }
}