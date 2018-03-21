using System;
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
    public class SliceIndexTest
    {
        private SliceIndex _sliceIndex;
        private String _sliceFilePath;
        private Mock<ISliceIndexMetricsRecorder> _metricsRecorder;
        
        [SetUp]
        public void SetUp()
        {
            _sliceFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "slice.idx");
            _metricsRecorder = new Mock<ISliceIndexMetricsRecorder>();
            if (File.Exists(_sliceFilePath))
                File.Delete(_sliceFilePath);
            _sliceIndex = new SliceIndex(_sliceFilePath, _metricsRecorder.Object);
        }

        [TearDown]
        public void TearDown()
        {
            _sliceIndex?.Close();
        }
        
        [Test]
        public void When_KVPIsPut_Then_KVPCanBeRetrieved()
        {
            byte[] key1 = Encoding.UTF8.GetBytes("key1");
            byte[] key2 = Encoding.UTF8.GetBytes("key2");
            byte[] key3 = Encoding.UTF8.GetBytes("key3");
            
            _sliceIndex.Put(key1, 1);
            _sliceIndex.Put(key2, 2);
            _sliceIndex.Put(key3, 3);
            
            Assert.AreEqual(1, _sliceIndex.Get(key1));
            Assert.AreEqual(2, _sliceIndex.Get(key2));
            Assert.AreEqual(3, _sliceIndex.Get(key3));
        }

        [Test]
        public void When_IndexIsEmpty_Then_NullValuesAreReturned()
        {
            Assert.IsNull(_sliceIndex.Get(Encoding.UTF8.GetBytes("doesnotexist1")));
            Assert.IsNull(_sliceIndex.Get(Encoding.UTF8.GetBytes("doesnotexist2")));
            Assert.IsNull(_sliceIndex.Get(Encoding.UTF8.GetBytes("doesnotexist3")));
        }

        [Test]
        public void When_KeyAlreadyExists_Then_KeyIsUpdated()
        {
            byte[] key1 = Encoding.UTF8.GetBytes("key1");
            
            _sliceIndex.Put(key1, 1);
            _sliceIndex.Put(key1, 2);

            Assert.AreEqual(2, _sliceIndex.Get(key1));
        }

        [Test]
        public void When_Starting_Then_IndexIsInitialised()
        {
            byte[] key1 = Encoding.UTF8.GetBytes("key1");
            byte[] key2 = Encoding.UTF8.GetBytes("key2");
            byte[] key3 = Encoding.UTF8.GetBytes("key3");
            
            _sliceIndex.Put(key1, 1);
            _sliceIndex.Put(key2, 2);
            _sliceIndex.Put(key3, 3);
            
            _sliceIndex.Close();
            
            _sliceIndex = new SliceIndex(_sliceFilePath, _metricsRecorder.Object);
            
            Assert.AreEqual(1, _sliceIndex.Get(key1));
            Assert.AreEqual(2, _sliceIndex.Get(key2));
            Assert.AreEqual(3, _sliceIndex.Get(key3));
        }

        [Test]
        public void When_FileIsInvalid_Then_InvalidContentsAreErased()
        {
            // add a bad entry to the file, confirm that the bad entry is erased, confirm that the rest of the entries
            // can be loaded
            throw new NotImplementedException();
        }
    }
}