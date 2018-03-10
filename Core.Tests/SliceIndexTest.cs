using System;
using System.IO;
using System.Reflection;
using System.Text;
using Core;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class SliceIndexTest
    {
        private SliceIndex _sliceIndex;
        private String _sliceFilePath;
        
        [SetUp]
        public void SetUp()
        {
            _sliceFilePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "slice.idx");
            if (File.Exists(_sliceFilePath))
                File.Delete(_sliceFilePath);
            _sliceIndex = new SliceIndex(_sliceFilePath);
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
            
            _sliceIndex = new SliceIndex(_sliceFilePath);
            
            Assert.AreEqual(1, _sliceIndex.Get(key1));
            Assert.AreEqual(2, _sliceIndex.Get(key2));
            Assert.AreEqual(3, _sliceIndex.Get(key3));
        }
    }
}