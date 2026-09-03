using AvePoint.GCommon;
using AvePoint.RA.FileSystem;
using AvePoint.RA.FileSystem.Core;
using System;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemDataSyncEngineRunner : IScheduleJobWorker
    {
        private readonly AveLogger _logger = AveLogger.GetInstance(typeof(RMFileSystemDataSyncEngineRunner));

        private RMFileSystemDataSyncEngine _engine;

        public void Bind(string msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) throw new ArgumentNullException(nameof(msg));

            try
            {
                JobContext.Current.JobMessage = msg;
                _engine = new RMFileSystemDataSyncEngine(msg);
            }
            catch (Exception ex)
            {
                _logger.Error("Failed to initialize RMFileSystemDataSyncEngine. Exception:{0}", ex);
                throw;
            }
        }

        public void Run()
        {
            if (_engine == null) throw new InvalidOperationException("Worker is not initialized. Call Bind before Run.");
            _engine.RunAsync().GetAwaiter().GetResult();
        }
    }
}