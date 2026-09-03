using AvePoint.RA.Common.Utils.ProtoBuf;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Protobuf;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace RAFileSystem.FileSystem.DataIngestion.V2
{
    public class RMDataIngestionDataWriter : IAsyncDisposable
    {

        private readonly RALogger _logger = RALogger.GetInstance(typeof(RMDataIngestionDataWriter));

        private readonly string _jobId;

        private readonly RMDataIngestionOperationType _operationType;

        private readonly string _folderPath;

        private readonly string _mark;

        private readonly int _maxRecordPerFile;

        private readonly Func<string, Task> _uploadAsync;

        private string _currentFilePath;

        private FileStream _currentFileStream;

        private int _currentRecordCount;

        internal RMDataIngestionDataWriter(
            string jobId,
            RMDataIngestionOperationType operationType,
            string folderPath,
            string mark,
            int maxRecordPerFile,
            Func<string, Task> uploadAsync)
        {
            _jobId = jobId;
            _operationType = operationType;
            _folderPath = folderPath;
            _mark = mark;
            _maxRecordPerFile = maxRecordPerFile;
            _uploadAsync = uploadAsync ?? throw new ArgumentNullException(nameof(uploadAsync));
            ProtobufRuntimeHelper.EnsureTypeRegistered<FileSystemRecordDto>();
        }

        public async Task<bool> WriteAsync(FileSystemRecordDto record)
        {
            var failedRecords = await WriteAsync(new List<FileSystemRecordDto> { record }).ConfigureAwait(false);
            return failedRecords.Count == 0;
        }

        public async Task<List<FileSystemRecordDto>> WriteAsync(IEnumerable<FileSystemRecordDto> records)
        {
            var failedRecords = new List<FileSystemRecordDto>();

            if (records == null) return failedRecords;

            foreach (var record in records)
            {
                EnsureStreamInitialized();

                try
                {
                    Serializer.SerializeWithLengthPrefix(_currentFileStream, new FSDataIngestion<FileSystemRecordDto> { Item = record }, PrefixStyle.Base128, 1);
                }
                catch(Exception ex)
                {
                    _logger.Error($"[Data Writer] Failed to serialize record: {record.NodeId}. Error: {ex.Message}");
                    failedRecords.Add(record);
                }
                _currentRecordCount++;

                if (_currentRecordCount >= _maxRecordPerFile)
                {
                    await FlushAndUploadCurrentFileAsync().ConfigureAwait(false);
                }
            }

            return failedRecords;
        }

        private void EnsureStreamInitialized()
        {
            if (_currentFileStream == null)
            {
                string fileName = $"{_jobId}_{_operationType}_{_mark}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}.bin";
                _currentFilePath = Path.Combine(_folderPath, fileName);
                _currentFileStream = File.Create(_currentFilePath);
                _currentRecordCount = 0;
            }
        }

        private async Task FlushAndUploadCurrentFileAsync()
        {
            if (_currentFileStream == null) return;

            _currentFileStream.Dispose();
            _currentFileStream = null;

            if (_currentRecordCount > 0 && !string.IsNullOrEmpty(_currentFilePath))
            {
                await _uploadAsync(_currentFilePath).ConfigureAwait(false);
            }

            _currentRecordCount = 0;
            _currentFilePath = null;
        }

        public async ValueTask DisposeAsync()
        {
            await FlushAndUploadCurrentFileAsync().ConfigureAwait(false);
        }
    }
}
