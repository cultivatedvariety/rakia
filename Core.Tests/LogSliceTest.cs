using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.IO;
using System.Reflection;
using System.Text;
using Core;
using Core.Metrics;
using Moq;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class LogSliceTest
    {
        private LogSlice _logSlice;
        private string _filePath;
        private Mock<ILogSliceIndex> _index;
        private Mock<ILogSliceMetricsRecorder> _metricsRecorder;
        private Dictionary<byte[], long> _indexEntries;

        [SetUp]
        public void SetUp()
        {
            _filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "test.log");
            _indexEntries = new Dictionary<byte[], long>();
            _index = new Mock<ILogSliceIndex>();
            _index.Setup(idx => idx.UpdateIndex(It.IsAny<byte[]>(), It.IsAny<long>()))
                .Callback<byte[], long>((k, v) => _indexEntries.AddOrUpdate(k, v));
            _index.Setup(idx => idx.GetSeekPosition(It.IsAny<byte[]>()))
                .Returns<byte[]>(key => _indexEntries.ContainsKey(key) ? _indexEntries[key] : (long?)null);
            _index.Setup(idx => idx.RemoveFromIndex(It.IsAny<byte[]>()))
                .Callback<byte[]>(k => _indexEntries.TryRemove(k));
            _metricsRecorder = new Mock<ILogSliceMetricsRecorder>();
            _logSlice = new LogSlice(_filePath, _index.Object, _metricsRecorder.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _logSlice?.Close();            
            if (File.Exists(_filePath))
                File.Delete(_filePath);
        }
        
        [Test]
        public void When_KVPExists_Then_GetReturnsValue()
        {
            byte[] key = Encoding.UTF8.GetBytes("key");
            byte[] expectedValue = Encoding.UTF8.GetBytes("value");

            _logSlice.Append(key, expectedValue);

            byte[] actualValue = _logSlice.Get(key);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void When_KVPExistsMultipleTimes_Then_GetReturnsLatestValue()
        {
            byte[] key = Encoding.UTF8.GetBytes("key");
            byte[] expectedValue = Encoding.UTF8.GetBytes("latest value");

            _logSlice.Append(key, Encoding.UTF8.GetBytes("value"));
            _logSlice.Append(key, expectedValue);

            byte[] actualValue = _logSlice.Get(key);
            Assert.AreEqual(expectedValue, actualValue);
        }

        [Test]
        public void When_KVPIsInLog_Then_ContainsReturnsTrue()
        {
            byte[] key = Encoding.UTF8.GetBytes("key");
            byte[] expectedValue = Encoding.UTF8.GetBytes("value");

            _logSlice.Append(key, expectedValue);
            
            Assert.IsTrue(_logSlice.Contains(key));
        }

        [Test]
        public void When_KVPIsNotInLog_Then_ContainsReturnsFalse()
        {
            byte[] key = Encoding.UTF8.GetBytes("key");
            byte[] expectedValue = Encoding.UTF8.GetBytes("value");

            _logSlice.Append(key, expectedValue);
            
            Assert.IsFalse(_logSlice.Contains(Encoding.UTF8.GetBytes("another key")));
        }

        [Test]
        public void When_KVPIsDeletedFromLog_Then_ContainsReturnsFalse()
        {
            byte[] key = Encoding.UTF8.GetBytes("key");
            byte[] expectedValue = Encoding.UTF8.GetBytes("value");

            _logSlice.Append(key, expectedValue);
            _logSlice.Remove(key);
            
            Assert.IsFalse(_logSlice.Contains(key));
        }
        
        [Test]
        public void When_EnumeratorReachesEndOfFile_Then_ItReturnsFalse()
        {
            // test enumeration
            byte[] key = Encoding.UTF8.GetBytes("key");

            _logSlice.Append(key, Encoding.UTF8.GetBytes("value"));
            _logSlice.Append(key, Encoding.UTF8.GetBytes("value1"));
            _logSlice.Append(key, Encoding.UTF8.GetBytes("value2"));

            var kvpEnumerable = _logSlice.GetEnumerator();
            
            Assert.IsTrue(kvpEnumerable.MoveNext());
            Assert.IsTrue(kvpEnumerable.MoveNext());
            Assert.IsTrue(kvpEnumerable.MoveNext());
            Assert.IsFalse(kvpEnumerable.MoveNext());
        }
                
        [Test]
        public void When_KVPIsPutMultipleTimes_Then_AllEntriesExistInTheLog()
        {
            // test that nothing is replaced in the file, append only
            byte[] key = Encoding.UTF8.GetBytes("key");

            _logSlice.Append(key, Encoding.UTF8.GetBytes("value"));
            _logSlice.Append(key, Encoding.UTF8.GetBytes("value1"));
            _logSlice.Append(key, Encoding.UTF8.GetBytes("value2"));

            var kvpEnumerable = _logSlice.GetEnumerator();
            kvpEnumerable.MoveNext();
            
            Assert.AreEqual(kvpEnumerable.Current.Value, Encoding.UTF8.GetBytes("value"));
            kvpEnumerable.MoveNext();
            Assert.AreEqual(kvpEnumerable.Current.Value, Encoding.UTF8.GetBytes("value1"));
            kvpEnumerable.MoveNext();
            Assert.AreEqual(kvpEnumerable.Current.Value, Encoding.UTF8.GetBytes("value2"));
        }

        

        [Test]
        public void When_KVPIsDeleted_Then_TombstoneValueIsWrittenToTheEndOfTheLog()
        {
            byte[] key = Encoding.UTF8.GetBytes("key");

            _logSlice.Append(key, Encoding.UTF8.GetBytes("value"));
            _logSlice.Remove(key);

            var enumerator = _logSlice.GetEnumerator();
            enumerator.MoveNext(); // skip the first value
            enumerator.MoveNext();
            
            Assert.AreEqual(0, enumerator.Current.Value.Length);

        }

    }
    
    public static class LogSliceTestExtensionMethods
    {
        public static void AddOrUpdate(this Dictionary<byte[], long> dict, byte[] key, long val)
        {
            if (dict.ContainsKey(key))
            {
                dict[key] = val;
            }
            else
            {
                dict.Add(key, val);
            }
        }

        public static void TryRemove(this Dictionary<byte[], long> dict, byte[] key)
        {
            if (dict.ContainsKey(key))
            {
                dict.Remove(key);
            }
        }
    }
}