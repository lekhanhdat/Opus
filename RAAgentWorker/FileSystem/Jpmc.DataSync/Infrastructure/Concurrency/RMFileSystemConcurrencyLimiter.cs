using System;
using System.Threading;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.Jpmc.DataSync
{
    public class RMFileSystemConcurrencyLimiter
    {

        private readonly string _contextName;

        private readonly int _maxConcurrentOperations;

        private readonly SemaphoreSlim _semaphore;

        public RMFileSystemConcurrencyLimiter(string contextName, int maxConcurrentOperations)
        {
            _contextName = contextName;
            _maxConcurrentOperations = maxConcurrentOperations;
            _semaphore = new SemaphoreSlim(_maxConcurrentOperations, _maxConcurrentOperations);
        }

        public ValueTask<RMFileSystemConcurrencyReleaser> AcquireAsync(string callerName)
        {
            if (string.IsNullOrWhiteSpace(callerName))
            {
                throw new ArgumentException("Caller name cannot be null or whitespace.", nameof(callerName));
            }

            if (_semaphore.Wait(0))
            {
                return new ValueTask<RMFileSystemConcurrencyReleaser>(new RMFileSystemConcurrencyReleaser(_semaphore));
            }

            return WaitForSemaphoreAsync(callerName);
        }

        private async ValueTask<RMFileSystemConcurrencyReleaser> WaitForSemaphoreAsync(string callerName)
        {
            await _semaphore.WaitAsync().ConfigureAwait(false);
            return new RMFileSystemConcurrencyReleaser(_semaphore);
        }
    }

    public readonly struct RMFileSystemConcurrencyReleaser : IDisposable
    {
        private readonly SemaphoreSlim _semaphore;

        public RMFileSystemConcurrencyReleaser(SemaphoreSlim semaphore)
        {
            _semaphore = semaphore;
        }

        public void Dispose()
        {
            _semaphore?.Release();
        }
    }
}
