using AvePoint.GCommon;
using System;
using System.IO.Pipes;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Threading;

namespace RAFileSystemCore.Common.JobHandler
{
    public sealed class JobStopMonitor : IDisposable
    {
        private static readonly AveLogger _logger = AveLogger.GetInstance(typeof(JobStopMonitor));

        private readonly string _jobId;
        private readonly CancellationTokenSource _jobCts;
        private readonly NamedPipeServerStream _pipeServer;
        private readonly Thread _listenerThread;
        private volatile bool _disposed;

        public static string GetPipeName(string jobId)
        {
            return "RAStop_" + jobId;
        }

        public JobStopMonitor(string jobId, CancellationTokenSource jobCts)
        {
            _jobId = jobId ?? throw new ArgumentNullException(nameof(jobId));
            _jobCts = jobCts ?? throw new ArgumentNullException(nameof(jobCts));

            PipeSecurity pipeSecurity = new PipeSecurity();

            SecurityIdentifier currentUser = WindowsIdentity.GetCurrent().User;
            pipeSecurity.AddAccessRule(new PipeAccessRule(currentUser, PipeAccessRights.ReadWrite, AccessControlType.Allow));

            SecurityIdentifier localSystem = new SecurityIdentifier(WellKnownSidType.LocalSystemSid, null);
            pipeSecurity.AddAccessRule(new PipeAccessRule(localSystem, PipeAccessRights.ReadWrite, AccessControlType.Allow));

            _pipeServer = new NamedPipeServerStream(
                GetPipeName(jobId),
                PipeDirection.In,
                maxNumberOfServerInstances: 1,
                transmissionMode: PipeTransmissionMode.Byte,
                options: PipeOptions.Asynchronous,
                inBufferSize: 0,
                outBufferSize: 0,
                pipeSecurity: pipeSecurity);

            _listenerThread = new Thread(WaitForStopSignal)
            {
                IsBackground = true,
                Name = "RAStop_" + jobId
            };
            _listenerThread.Start();

            _logger.Info("JobStopMonitor started for job {0} with secure ACLs.", jobId);
        }

        private void WaitForStopSignal()
        {
            try
            {
                _pipeServer.WaitForConnection();

                if (!_disposed)
                {
                    _logger.Info("Stop signal received via pipe for job {0}. Triggering cancellation.", _jobId);
                    _jobCts.Cancel();
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception ex) when (_disposed)
            {
                _logger.Debug("Pipe listener exited after disposal for job {0}. Detail: {1}", _jobId, ex.Message);
            }
            catch (Exception ex)
            {
                _logger.Warn("Error in stop pipe listener for job {0}. Error: {1}", _jobId, ex.Message);
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;
            _pipeServer?.Dispose();
            _logger.Info("JobStopMonitor disposed for job {0}.", _jobId);
        }
    }
}