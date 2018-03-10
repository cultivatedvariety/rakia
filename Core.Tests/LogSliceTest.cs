using System;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

namespace Tests
{
    [TestFixture]
    public class LogSliceTest
    {
        [Test]
        public void When_KVPIsAppended_Then_KVPCanBeRetrieved()
        {
            // confirm that when a kvp is appended then the correct seek position is returned
            throw new NotImplementedException();
        }

        [Test]
        public void When_KVPsAreAppeneded_Then_TheyAreAppendedToTheEnd()
        {
            // test that the slice is actually append only - all new values are appended to the end
            throw new NotImplementedException();
        }

        [Test]
        public void When_KeyIsNotInSlice_Then_NullSeekPositionIsReturned()
        {
            // test that when the key is not in the slice, a NULL seek position is returned
            throw new NotImplementedException();
        }

        [Test]
        public void When_KeyExists_Then_SeekPositionIsReturned()
        {
            // test that when a key exists the correct seek position is returned
            throw new NotImplementedException();
        }

        [Test]
        public void When_KeyExistsMultipleTime_Then_SeekPositionOfLatestInstanceIsReturned()
        {
            // test that if a key exists in a slice multiple times
            throw new NotImplementedException();
        }
        
    }
}