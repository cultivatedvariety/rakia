using System.Collections.Generic;
using System.IO;

namespace Core
{
    public class Log
    {
        private readonly string _directory;
        private readonly ILogSliceFactory _logSliceFactory;
        private readonly List<ILogSlice> _logSlices;

        public Log(string directory, ILogSliceFactory logSliceFactory)
        {
            _directory = directory;
            _logSliceFactory = logSliceFactory;
            _logSlices = new List<ILogSlice>();
            Initialise();
        }
        
//        public void Delete(byte[] key)
//        {
//            try
//            {
//                _metricsRecorder.RemoveStarted();
//                Append(key, new byte[0]);
//            }
//            finally
//            {
//                _metricsRecorder.RemoveFinished();
//            }
//        }

        private void Initialise()
        {
            if (!Directory.Exists(_directory))
            {
                Directory.CreateDirectory(_directory);
            }

            var slicesFiles = Directory.GetFiles(_directory, "*.slice");
            foreach (var slicesFile in slicesFiles)
            {
                var sliceIndexFile = string.Format("{0}.idx", slicesFile);
                _logSlices.Add(_logSliceFactory.CreateSlice(sliceIndexFile));
            }
            
        }
    }
}