using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;

namespace Core
{
    public class SliceIndex : ISliceIndex
    {
        public string FilePath { get; }
        private readonly Dictionary<byte[], long> _keyToValueSeekPositionMap;
        private FileStream _fileStream;

        public SliceIndex(string filePath)
        {
            FilePath = filePath;
            _keyToValueSeekPositionMap = new Dictionary<byte[], long>(5000, new ByteArrayEqualityComparer());
            InitialiseSeekFile();
        }

        public void Put(byte[] key, long seekPosition)
        {
            if (_keyToValueSeekPositionMap.ContainsKey(key))
            {
                var updateSeekPosition = _keyToValueSeekPositionMap[key];
                _fileStream.Seek(updateSeekPosition, SeekOrigin.Begin);
                var seekPositionBytes = BitConverter.GetBytes(seekPosition);
                _fileStream.Write(seekPositionBytes, 0, seekPositionBytes.Length);
            }
            else
            {
                _fileStream.Seek(_fileStream.Length, SeekOrigin.Begin);
                var keyLengthBytes = BitConverter.GetBytes(key.Length);
                _fileStream.Write(keyLengthBytes, 0, keyLengthBytes.Length);
                _fileStream.Write(key, 0, key.Length);
                
                var keySeekPosition = _fileStream.Position;
                
                var seekPositionBytes = BitConverter.GetBytes(seekPosition);
                _fileStream.Write(seekPositionBytes, 0, seekPositionBytes.Length);
                
                _keyToValueSeekPositionMap.Add(key, keySeekPosition);
            }   
            _fileStream.Flush(true);
        }

        public long? Get(byte[] key)
        {
            if (!_keyToValueSeekPositionMap.ContainsKey(key))
                return null;
            _fileStream.Seek(_keyToValueSeekPositionMap[key], SeekOrigin.Begin);
            var value = new byte[8];
            _fileStream.Read(value, 0, value.Length);
            return BitConverter.ToInt64(value, 0);
        }

        public long GetMaxKeySeekPosition()
        {
            return _keyToValueSeekPositionMap.Max(kvp => kvp.Value);
        }
        
        public void Close()
        {
            _fileStream?.Close();
        }

        private void InitialiseSeekFile()
        {
            // TODO: need to verify the file is valid at the same time
            _fileStream = new FileStream(FilePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            byte[] keyLengthBuffer = new byte[4];
            while (true)
            {
                var read = _fileStream.Read(keyLengthBuffer, 0, keyLengthBuffer.Length);
                if (read == 0)
                    break;
                var keyLength = BitConverter.ToInt32(keyLengthBuffer, 0);
                byte[] key = new byte[keyLength];
                _fileStream.Read(key, 0, key.Length); // could go wrong here with corrupted file
                _keyToValueSeekPositionMap.Add(key, _fileStream.Position);
                _fileStream.Seek(8, SeekOrigin.Current); // skip the value associated with the key
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