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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Contract.DataIngestion;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Protobuf;
using AvePoint.RA.Contract.Services;
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using System;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

namespace RAFileSystem.Disposal.NewLogic
{
    public class DisposalDataUpdaterV2
    {
        private readonly AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private IProgressService ProgressService { get; set; }

        private readonly ChannelReader<(FSAzureTableEntityDto FileDto, FileSystemRecordDto FSRecordDto)> workerReader;
        private readonly ChannelWriter<FSAzureTableEntityDto> cosmosWriter;

        public DisposalDataUpdaterV2(ChannelReader<(FSAzureTableEntityDto FileDto, FileSystemRecordDto FSRecordDto)> archivedReader, ChannelWriter<FSAzureTableEntityDto> cosmosOut) : this()
        {
            this.workerReader = archivedReader;
            this.cosmosWriter = cosmosOut;
        }

        public DisposalDataUpdaterV2()
        {
            ProgressService = JobContext.Current.mProgressManager.Create();
            workerReader = FSJobCache.Instance.WorkerToUpdater.Reader;
            cosmosWriter = FSJobCache.Instance.DiscoveryToCosmos.Writer;
        }

        public async Task Run()
        {
            try
            {
                Thread.CurrentThread.Name = string.Format("DisposalDataUpdaterThread[{0}]", Thread.CurrentThread.ManagedThreadId);

                while (await workerReader.WaitToReadAsync().ConfigureAwait(false))
                {
                    while (workerReader.TryRead(out var channelItem))
                    {
                        var file = channelItem.FileDto;
                        var fsRecord = channelItem.FSRecordDto;

                        try
                        {
                            if (file == null || fsRecord == null)
                            {
                                continue;
                            }

                            if (fsRecord.ManualApprovedStatus == (int)SOApproveDBStatus.Approved)
                            {
                                file.NoNeedSendReport = true;
                                file.DestroyedTime = DateTime.UtcNow.Ticks;
                                await cosmosWriter.WriteAsync(file).ConfigureAwait(false);
                                logger.Info($"File ID: {file.FilePathMd5} updated status to Approved");
                                continue;
                            }

                            if (FSDataDisposalV2.ClassificationLevel == NodeLevel.FSFile)
                            {
                                var executionResult = new RMDataIngestionAgentWorkItemExecutionResult
                                {
                                    Id = fsRecord.NodeId,
                                    NodeType = fsRecord.NodeType,
                                    LeafName = fsRecord.LeafName,
                                    DirPath = fsRecord.DirPath,
                                    RuleAction = fsRecord.RuleAction,
                                    RuleName = fsRecord.RuleName,
                                    Size = fsRecord.FileSize,
                                    Depth = fsRecord.Depth,
                                };

                                if (file.RecordStatus == (int)RMRecordStatus.Moved)
                                {
                                    if (fsRecord.RecordStatus == (int)RMRecordStatus.Active) // new record after move
                                    {
                                        fsRecord.CreateDate = file.CreateDate;
                                        logger.Info($"File {fsRecord.NodeId} is moved, change record status to Moved and write to data ingestion {fsRecord.RecordStatus}");
                                        await WriteRecordToDataIngestion(fsRecord, executionResult);
                                    }

                                    if (file.Status == (int)SOApproveDBStatus.Archived)
                                    {
                                        var sourceRecord = fsRecord;
                                        sourceRecord.NodeId = file.FilePathMd5;
                                        sourceRecord.RecordStatus = (int)RMRecordStatus.Moved;
                                        var sourceResult = new RMDataIngestionAgentWorkItemExecutionResult
                                        {
                                            Id = file.FilePathMd5,
                                            NodeType = executionResult.NodeType,
                                            LeafName = file.LowName,
                                            DirPath = file.HighName,
                                            RuleAction = file.RuleAction,
                                            RuleName = executionResult.RuleName,
                                            Size = file.Size,
                                            Depth = file.Depth,
                                        };
                                        logger.Info($"File {sourceRecord.NodeId} is moved and archived, create a moved record for source file with Moved status {sourceRecord.RecordStatus}");
                                        await WriteRecordToDataIngestion(sourceRecord, sourceResult);
                                    }
                                }
                                else if (file.RecordStatus == (int)RMRecordStatus.Destroyed)
                                {
                                    fsRecord.RecordStatus = (int)RMRecordStatus.Destroyed;
                                    await WriteRecordToDataIngestion(fsRecord, executionResult);
                                }
                                
                                logger.Info($"Change status of file {file.FilePathMd5} from {file.RecordStatus} to {fsRecord.RecordStatus}");
                            }
                        }
                        catch (Exception itemex)
                        {
                            string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName, file.LowName);
                            logger.Error("Failed to process item. Object:{0}, Exception:{1}", fullPath.LogBase64(), itemex.ToString());
                            ProgressService.Increase();
                            FSJobCache.Instance.FailedCount++;
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error("Failed to update disposal data. Exception:{0}", ex.ToString());
            }
            finally
            {
                cosmosWriter?.TryComplete();
            }
        }

        private static async Task WriteRecordToDataIngestion(FileSystemRecordDto fsRecord, RMDataIngestionAgentWorkItemExecutionResult executionResult = null)
        {
            if (fsRecord.RecordStatus != (int)RMRecordStatus.Active)
            {
                fsRecord.DestroyedTime = DateTime.UtcNow.Ticks;
            }

            ApplyClassCodeData(fsRecord);

            var dataIngestion = new FSDataIngestion<FileSystemRecordDto>() { Item = fsRecord };
            await FSJobCache.Instance.DataIngestionDataCollector.WriteDataAsync(dataIngestion, executionResult);

            if (string.IsNullOrWhiteSpace(fsRecord.RecordsId))
            {
                FSJobCache.Instance.DataIngestMessageExtensionManager.ModifyState(ext => { ext.NewRecordIdsRange += 1; });
            }
        }

        private static void ApplyClassCodeData(FileSystemRecordDto fsRecord)
        {
            if (fsRecord.TermId == Guid.Empty) return;

            if (!FSJobCache.Instance.ClassCodeInfoByTermId.TryGetValue(fsRecord.TermId, out var classCodeInfo))
                return;

            if (string.IsNullOrEmpty(fsRecord.ClassCode))
                fsRecord.ClassCode = classCodeInfo.ClassCode;

            if (string.IsNullOrEmpty(fsRecord.CountryCode))
                fsRecord.CountryCode = classCodeInfo.CountryCode;

            if (fsRecord.RetentionType == 0)
                fsRecord.RetentionType = classCodeInfo.RetentionType;

            if (fsRecord.StartDate == 0)
                fsRecord.StartDate = classCodeInfo.StartDate;

            if (fsRecord.EndTime == 0)
                fsRecord.EndTime = classCodeInfo.EndTime;

            if (fsRecord.PolicyValueUnit == 0)
                fsRecord.PolicyValueUnit = classCodeInfo.PolicyValueUnit;

            if (fsRecord.PolicyValueNumber == 0)
                fsRecord.PolicyValueNumber = classCodeInfo.PolicyValueNumber;
        }
    }
}
