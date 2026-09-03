/********************************************************************
 *
 *  PROPRIETARY and CONFIDENTIAL
 *
 *  This file is licensed from, and is a trade secret of:
 *
 *                   AvePoint, Inc.
 *                   525 Washington Blvd, Suite 1400
 *                   Jersey City, NJ 07310
 *                   United States of America
 *                   Telephone: +1-201-793-1111
 *                   WWW: www.avepoint.com
 *
 *  Refer to your License Agreement for restrictions on use,
 *  duplication, or disclosure.
 *
 *  RESTRICTED RIGHTS LEGEND
 *
 *  Use, duplication, or disclosure by the Government is
 *  subject to restrictions as set forth in subdivision
 *  (c)(1)(ii) of the Rights in Technical Data and Computer
 *  Software clause at DFARS 252.227-7013 (Oct. 1988) and
 *  FAR 52.227-19 (C) (June 1987).
 *
 *  Copyright © 2017-2026 AvePoint® Inc. All Rights Reserved. 
 *
 *  Unpublished - All rights reserved under the copyright laws of the United States.
 */




namespace AvePoint.Media.Service
{
    using AvePoint.Application.StorageApiModern;
    #region using directives

    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Common;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.Common;
    using AvePoint.RA.Contract.Exceptions;
    using AvePoint.RA.Contract.RMWeb.JobMonitor;
    using AvePoint.RA.Contract.RMWeb.StorageDevice;
    using AvePoint.RA.DB.Dao;
    using AvePoint.RA.DB.Dao.Impl;
    using Merged18NResources.MediaServiceApplicationModel;
    using Merged18NResources.MediaServiceArchiverBackup;
    using PnP.Framework.Modernization;
    using Storage;
    using Storage.Cloud.Azure;
    using Storage.Cloud.Google;
    using Storage.Util;
    using System;
    using System.Buffers;
    using System.Reflection;

    #endregion using directives

    public abstract class RetentionServiceBase<TParameter, TResult>
        : ApplicationModelServiceBase
        , IRetentionService
        where TParameter : class,IRetentionInfo
        where TResult : class,IRetentionResult
    {
        AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly TimeSpan FileMoveTimeout = TimeSpan.FromHours(2);

        protected static string CACHE_FODER_PATH = SecurityUtils.SafeCombinePath(AppDomain.CurrentDomain.BaseDirectory, "RetentionCache");

        protected string PossiblyStubSuffix { get; set; }
        protected Action<JMArchiverRententionJobDetails> mReportAction;
        protected Action<JMArchiverRententionMigrationDetails>? mMigrationReportAction;

        protected Dictionary<string, string> storageDeviceIDs = new Dictionary<string, string>(); // key is archiver sub job id
        protected Dictionary<string, IXSystem> dataLogicalDevices = new Dictionary<string, IXSystem>();

        protected bool IsEnableExtendedMoveActionForRetention { get; set; }
        protected bool IsProcessingArchivedFile { get; set; }

        public IArchiverIndexSubInfoDao ArchiverIndexSubInfoDao { get; set; }

        public IStorageDeviceService StorageDeviceService { get; set; }

        public IRetentionResult Retain(IRetentionInfo retentionInfo, Action<JMArchiverRententionJobDetails> reportAction)
        {
            var info = retentionInfo as TParameter;
            mReportAction = reportAction;
            return this.InternalRetain(info);
        }

        public IRetentionResult Retain(IRetentionInfo retentionInfo, Action<JMArchiverRententionJobDetails> reportAction, Action<JMArchiverRententionMigrationDetails> migrationReportAction)
        {
            var info = retentionInfo as TParameter;
            mReportAction = reportAction;
            mMigrationReportAction = migrationReportAction;
            return this.InternalRetain(info);
        }

        protected StorageResult RealMove(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice)
        {
            StorageResult storageResult = null;
            string fileName = CACHE_FODER_PATH + Path.DirectorySeparatorChar + DateTime.UtcNow.Ticks;
            byte[] buffer = new byte[64 * 1024];
            try
            {
                if (this.IsEnableExtendedMoveActionForRetention)
                {
                    KeepFileTier(sourceDevice.StorageType, sourceInfo, destinationDevice.StorageType, destinationInfo);
                }

                if (!Directory.Exists(CACHE_FODER_PATH))
                {
                    Directory.CreateDirectory(CACHE_FODER_PATH);
                }
                using (var sourceStream = sourceDevice.OpenStream(sourceInfo, FileMode.Open))
                {
                    using (var tempFile = new FileStream(fileName, FileMode.Create))
                    {
                        int bytesRead = 0;
                        while ((bytesRead = sourceStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            tempFile.Write(buffer, 0, bytesRead);
                        }
                        tempFile.Flush(true);
                    }
                }
                using (Stream cacheStream = File.OpenRead(fileName))
                {
                    storageResult = destinationDevice.CommitStream(cacheStream, destinationInfo);
                }
            }
            catch (Exception ex)
            {
                storageResult = null;
                this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceRealMoveInvalidDevice, ex.ToString());
                throw;
            }
            finally
            {
                FileUtility.TryDelete(fileName);
            }
            return storageResult;
        }

        protected async Task<StorageResult> RealMoveAsync(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice, long blockLength = 100, CancellationToken cancellationToken = default)
        {
            StorageResult storageResult = null;

            using var timeoutSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutSource.CancelAfter(FileMoveTimeout);

            try
            {
                double totalMB = sourceInfo.Length * 1.0 / XConstants.MB;
                var startTime = DateTime.UtcNow;

                if (this.IsEnableExtendedMoveActionForRetention)
                {
                    KeepFileTier(sourceDevice.StorageType, sourceInfo, destinationDevice.StorageType, destinationInfo);
                }

                //Direct streaming: source → buffer → destination (NO disk I/O)
                if (totalMB > blockLength)
                {
                    this.logger.Info("Source file is larger than block length, use MoveLargeItemAsync. Source: {0}, Size: {1:F2}MB", sourceInfo.LowName, totalMB);
                    storageResult = await MoveLargeItemAsync(sourceInfo, sourceDevice, destinationInfo, destinationDevice, timeoutSource.Token);
                }
                else
                {
                    this.logger.Info("Source file is smaller than block length, use MoveSmallItemAsync. Source: {0}, Size: {1:F2}MB", sourceInfo.LowName, totalMB);
                    storageResult = await MoveSmallItemAsync(sourceInfo, sourceDevice, destinationInfo, destinationDevice, timeoutSource.Token);
                }

                //Log throughput metrics
                var duration = (DateTime.UtcNow - startTime).TotalSeconds;
                var throughputMBps = duration > 0 ? totalMB / duration : 0;
                this.logger.Info("File moved (direct stream): {0} -> {1}, Size: {2:F2}MB, Time: {3:F2}s, Throughput: {4:F2}MB/s",
                    sourceInfo.LowName, destinationInfo.LowName, totalMB, duration, throughputMBps);
            }
            catch (OperationCanceledException ex) when (timeoutSource.IsCancellationRequested && !cancellationToken.IsCancellationRequested)
            {
                var message = string.Format("File move timed out after {0:F2} hours. Source: {1}, Destination: {2}",
                    FileMoveTimeout.TotalHours, sourceInfo.LowName, destinationInfo.LowName);
                this.logger.Error(message, ex.ToString());
                throw new TimeoutException(message, ex);
            }
            catch (Exception ex)
            {
                this.logger.Error(MediaServiceArchiverBackupResource.RetentionServiceRealMoveInvalidDevice, ex.ToString());
                throw;
            }

            return storageResult ?? new StorageResult();
        }

        private void KeepFileTier(XStorageType sourceType, StorageInfo sourceInfo, XStorageType destinationType, StorageInfo destinationInfo)
        {
            if ((sourceType == XStorageType.Azure && destinationType == XStorageType.GoogleCloud)
                && (sourceInfo is AzureCloudInfo azureSrcInfo && destinationInfo is GoogleCloudInfo gcDestInfo))
            {
                // Handle rehydration scenario because the archived file in Azure will be rehydrated to hot tier before moving.
                if (IsProcessingArchivedFile)
                {
                    gcDestInfo.StorageClass = GoogleStorageClass.Archive;
                }
                else
                {
                    gcDestInfo.StorageClass = azureSrcInfo.FileTierType.ToGoogleStorageClass();
                }
            }
            //else if ((sourceType == XStorageType.GoogleCloud && destinationType == XStorageType.Azure)
            //    && (sourceInfo is GoogleCloudInfo gcSrcInfo && destinationInfo is AzureCloudInfo azureDestInfo))
            //{
            //    azureDestInfo.FileTierType = gcSrcInfo.StorageClass.ToAzureTierType();
            //}
        }

        private async Task<StorageResult> MoveLargeItemAsync(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice, CancellationToken cancellationToken = default)
        {
            StorageResult storageResult = null;
            using (var sourceStream = await sourceDevice.OpenReadAsync(sourceInfo, cancellationToken))
            {
                storageResult = await destinationDevice.UploadAsyncExt(sourceStream, destinationInfo, overWrite: true, cancellationToken);
            }
            return storageResult;
        }

        private async Task<StorageResult> MoveSmallItemAsync(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice, CancellationToken cancellationToken = default)
        {
            StorageResult storageResult = null;
            const int bufferSize = 1 * XConstants.MB;
            try
            {
                using (var sourceStream = await sourceDevice.OpenReadAsync(sourceInfo, cancellationToken))
                {
                    var tempStream = new MemoryStream();
                    sourceStream.CopyTo(tempStream, bufferSize);
                    storageResult = await destinationDevice.UploadAsyncExt(tempStream, destinationInfo, overWrite: true, cancellationToken);
                }
            }
            catch (Exception)
            {
                throw;
            }
            return storageResult;
        }

        //protected StorageResult RealMove(StorageInfo sourceInfo, IXSystem sourceDevice, StorageInfo destinationInfo, IXSystem destinationDevice)
        //{
        //    StorageResult storageResult;
        //    var startTime = DateTime.UtcNow;

        //    try
        //    {
        //        // Direct streaming: Azure → Network → Azure (no disk, no temp files)
        //        // CommitStream passes sourceStream directly to Storage API
        //        // Large files: Streamed in 50MB chunks (memory efficient)
        //        // Small files: Downloaded once for CRC64 hash (acceptable overhead)
        //        using (var sourceStream = sourceDevice.OpenStream(sourceInfo, FileMode.Open))
        //        {
        //            storageResult = destinationDevice.CommitStream(sourceStream, destinationInfo);
        //        }

        //        // Log throughput metrics
        //        var duration = (DateTime.UtcNow - startTime).TotalSeconds;
        //        this.logger.Info("File moved (CommitStream): {0} -> {1}, Time: {2:F2}s",
        //            sourceInfo.LowName, destinationInfo.LowName, duration);
        //    }
        //    catch (Exception ex)
        //    {
        //        this.logger.Error("RealMove failed: {0} -> {1}, Error: {2}",
        //            sourceInfo.LowName, destinationInfo.LowName, ex.ToString());
        //        throw;
        //    }

        //    return storageResult;
        //}

        private TResult InternalRetain(TParameter retentionInfo)
        {
            //var jobState = 2; //2 stand for job successful
            var result = Activator.CreateInstance(typeof(TResult)) as TResult;
            try
            {
                this.Open(retentionInfo);
                result = this.Retain(retentionInfo);
            }
            catch (JobStopException e)
            {
                logger.Warn("Job will stop, throw JobStopException in internalRetain.");
                throw;
            }
            catch (AuthenticationFailedException e)
            {
                this.ProcessException(e, result);
                throw;
            }
            catch (Exception e)
            {
                this.ProcessException(e, result);
                throw;
            }
            finally
            {
                this.Close();
            }
            return result;
        }

        public abstract void Open(TParameter retentionInfo);

        public abstract TResult Retain(TParameter retentionInfo);

        public abstract void ProcessException(Exception e, TResult result);

        public virtual void Close()
        {
            this.Dispose();
        }

        //for Archiver
        public virtual void GenerateJobReport(Int32 jobState) { }

        public virtual void UpdateJobStatusAndControlTable(Int32 jobState) { }

        public void AddToReport(JMArchiverRententionJobDetails rententionJobDetails)
        {
            if (mReportAction != null && rententionJobDetails != null)
            {
                mReportAction(rententionJobDetails);
            }
        }

        private string GetStorageDeviceIdByArchiverJobId(string jobId)
        {
            if (!this.storageDeviceIDs.TryGetValue(jobId, out var storageId))
            {
                storageId = ArchiverIndexSubInfoDao.Find(i => i.SubSubJobId == jobId)?.CurrentStorageId;
                if (string.IsNullOrEmpty(storageId))
                {
                    logger.Error($"Can't find physical device id by jobId: {jobId}");
                }

                this.storageDeviceIDs[jobId] = storageId;
            }

            return storageId;
        }

        protected IXSystem GetDataLogicalDeviceByJobId(string jobId)
        {
            var storageId = GetStorageDeviceIdByArchiverJobId(jobId);
            if (string.IsNullOrEmpty(storageId))
            {
                return null;
            }
            else
            {
                return GetDataLogicalDeviceByDeviceId(storageId);
            }
        }

        private IXSystem GetDataLogicalDeviceByDeviceId(string storageDeviceId)
        {
            if (!this.dataLogicalDevices.TryGetValue(storageDeviceId, out var dataLogicalDevice))
            {
                var storageDevice = StorageDeviceService.GetStorageDeviceById(storageDeviceId, needDecryptSecert: true);
                if (storageDevice != null)
                {
                    dataLogicalDevice = this.StorageDeviceManager.Open(new List<string>() { storageDevice.BuildXRI() });
                }
                else
                {
                    logger.Error($"Can't find storage device by id: {storageDeviceId}");
                }

                this.dataLogicalDevices[storageDeviceId] = dataLogicalDevice;
            }

            return dataLogicalDevice;
        }

        protected string EnsureStubType(string stubType)
        {
            switch (stubType)
            {
                case "Aspx":
                    logger.Info("stub type is aspx.");
                    return ".aspx";
                case "Html":
                    logger.Info("stub type is html.");
                    return ".html";
                case "Txt":
                    logger.Info("stub type is txt.");
                    return ".txt";
                case "Link":
                    logger.Info("stub type is link.");
                    return ".url";
                default:
                    logger.Warn("stub type is empty or the type not exist.");
                    return string.Empty;
            }
        }
        private readonly List<string> _StubSuffixes = new()
        {
            ".aspx",
            ".html",
            ".txt",
            ".url",
        };
        protected List<string> GetPossiblyStubSuffixes(string stubSuffix)
        {
            List<string> possiblyStubSuffixes = new List<string>();
            if (!string.IsNullOrEmpty(PossiblyStubSuffix))
            {
                possiblyStubSuffixes.Add(PossiblyStubSuffix);
            }

            if (!string.IsNullOrEmpty(stubSuffix))
            {
                TryAddStubSuffixToList(possiblyStubSuffixes, stubSuffix);
            }
            else
            {
                foreach (var item in _StubSuffixes)
                {
                    TryAddStubSuffixToList(possiblyStubSuffixes, item);
                }
            }

            return possiblyStubSuffixes;
        }
        private bool TryAddStubSuffixToList(List<string> stubSuffixes, string stubSuffix)
        {
            if (!stubSuffixes.Any(i => i == stubSuffix))
            {
                stubSuffixes.Add(stubSuffix);
                return true;
            }
            return false;
        }

        #region Support write all the necessary migration detail info

        protected void AddToMigrationReport(JMArchiverRententionMigrationDetails migrationDetails)
        {
            if (mMigrationReportAction != null && migrationDetails != null)
            {
                mMigrationReportAction(migrationDetails);
            }
        }

        #endregion

        #region IDisposable

        public abstract void Dispose();

        #endregion IDisposable
    }
}