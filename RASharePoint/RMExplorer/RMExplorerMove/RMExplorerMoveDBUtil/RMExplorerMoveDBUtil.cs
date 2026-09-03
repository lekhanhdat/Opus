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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.Records.Core.Utilities.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class RMExplorerMoveDBUtil
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMExplorerMoveDBUtil));

        private Dictionary<Guid, RMTerm> mRMTermDic = new Dictionary<Guid, RMTerm>();

        public RMExplorerMoveDBUtil()
        {
            mRMTermDic = TermDao.GetAllTermsForce().ToDictionary(t => t.UniqueId);
        }

        private IExplorerDao mExplorerDao;
        public IExplorerDao ExplorerDao
        {
            get
            {
                if (mExplorerDao == null)
                {
                    mExplorerDao = new ExplorerDao();
                }
                return mExplorerDao;
            }
        }

        private IRecordAllianceDao mIRecordAllianceDao;
        protected IRecordAllianceDao RecordAllianceDao
        {
            get
            {
                if (mIRecordAllianceDao == null)
                {
                    mIRecordAllianceDao = new RecordAllianceDao();
                }
                return mIRecordAllianceDao;
            }
        }

        private IRecordsHistoryService mRecordsHistoryService = null;
        public IRecordsHistoryService RecordsHistoryService
        {
            get
            {
                if (mRecordsHistoryService == null)
                {
                    mRecordsHistoryService = (IRecordsHistoryService)PlatformWindsorManager.GetService(typeof(IRecordsHistoryService));
                }
                return mRecordsHistoryService;
            }
        }


        private ITermDao mTermDao = null;
        public ITermDao TermDao
        {
            get
            {
                if (mTermDao == null)
                {
                    mTermDao = (ITermDao)PlatformWindsorManager.GetService(typeof(ITermDao));
                }
                return mTermDao;
            }
        }


        public void UpdateMovedRecords(JobResult jobResult, SourceBase source, MoveDestinationManager desInfo, string user, RemoteSiteCollection destSite, Guid destTermId, bool failedUpdateColumn = false)
        {
            if (source.NodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.ItemVersion)
            {
                return;
            }
            var sourceRcord = GetRecord(source.Id);
            string desUrl = jobResult?.DestStub?.FullPath;
            if (string.IsNullOrEmpty(desUrl))
            {
                desUrl = desInfo.Destination.DestinationContainerUrl;
            }
            var destinationFlag = GetDestinationFlag(destSite);
            ArgumentCheck.NotNull(jobResult, nameof(jobResult));
            AddHistory(source.Id, sourceRcord?.RecordHistory, source.SourceUrl, desUrl, user, jobResult.Status);
            if (jobResult.Status == Contract.RMWeb.JobMonitor.JobDetailsStatus.Successful)
            {
                Guid destScopeId = jobResult.DestStub.DestFlag == (int)RecordFlag.SP ? (desInfo.Destination as SPDestination).SiteId : desInfo.DestRootPath.ToLowerInvariant().ToMd5();
                var destRecords = new List<Record>();
                if (jobResult.DestStub.OriginalNodeId == Guid.Empty)
                {
                    logger.Info("Original node id is empty.");
                }
                else
                {
                    var destId = IDGenerator.GetRecordId(destScopeId, jobResult.DestStub.OriginalNodeId);
                    destRecords = GetRecords(destId);
                }
                ArgumentCheck.NotNull(sourceRcord, nameof(sourceRcord));
                var sourceDataSource = sourceRcord.SourceFlag;
                var finalRecord = GenerateRecord(sourceRcord, jobResult, source, desInfo, user);
                finalRecord.TeamsId = string.IsNullOrEmpty(destSite.TeamId)?Guid.Empty:new Guid(destSite.TeamId);
                finalRecord.SourceFlag = destinationFlag;
                AppendMetaInfo(finalRecord);
                if (destinationFlag == (int)SourceFlag.SharePoint)
                {
                    if (destTermId != Guid.Empty)
                    {
                        finalRecord.TermId = destTermId;
                        finalRecord.TermName = mRMTermDic.ContainsKey(finalRecord.TermId) ? mRMTermDic[finalRecord.TermId].Name : string.Empty;
                    }
                    else
                    {
                        finalRecord.TermId = Guid.Empty;
                        finalRecord.TermName = string.Empty;
                    }
                }
                else
                {
                    if (!desInfo.KeepSourceClassification || (desInfo.KeepSourceClassification && failedUpdateColumn))
                    {
                        finalRecord.TermId = Guid.Empty;
                        finalRecord.TermName = string.Empty;
                    }
                }

                if (failedUpdateColumn || finalRecord.SourceFlag != sourceDataSource || finalRecord.TermId == Guid.Empty)
                {
                    finalRecord.RuleId = Guid.Empty;
                    finalRecord.DisposalDueDate = DueDateUtil.ConvertStringDueDate2Long("RM_JS_JM_EndTimePending");
                    finalRecord.RecordOwner = I18NEntity.GetString("RM_JS_JM_EndTimePending");
                    finalRecord.RecordOwner_Array = finalRecord.RecordOwner.ExplorerSearchSplit();
                }
                //如果原本目的端有对应的Record 记录，则执行update操作，否则执行Add 操作
                if (destRecords != null && destRecords.Count > 0)
                {
                    //ExplorerDao.UpdateAll(r => r.ScopeId == finalRecord.ScopeId && r.NodeId == finalRecord.NodeId, rec => rec = finalRecord);
                    //destRecords.ForEach(des => ExplorerDao.UpdateRecordState(des.ScopeId, des.Id, 5));
                    foreach (var destRec in destRecords)
                    {
                        ExplorerDao.UpdateAll(s => s.ScopeId == destRec.ScopeId && s.Id == destRec.Id && s.RecordStatus == (int)RMRecordStatus.Active, r => { r.RecordStatus = 5; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                    }
                }
                //else
                //{
                ExplorerDao.Add(finalRecord);

                //clone history to new record
                logger.Info($"clone history to new record, source record: {source.Id}, new record: {finalRecord.Id}");
                RecordsHistoryService.CloneMoveHistoryRecords(source.Id, finalRecord.Id);
                //}
                if (sourceRcord != null)
                {
                    //RECO-3552 Move 的时候，其他功能逻辑要求不去删除Record Explorer DB 的记录，这里更新成 4 = Moved， 给其他功能使用
                    //ExplorerDao.Delete(source.ScopeId, source.Id);
                    //ExplorerDao.UpdateRecordState(source.ScopeId, source.Id, 4);
                    ExplorerDao.UpdateAll(s => s.ScopeId == source.ScopeId && s.Id == source.Id && s.RecordStatus == (int)RMRecordStatus.Active, r => { r.RecordStatus = 4; r.DestroyedTime = DateTime.UtcNow.Ticks; });
                }
            }
            //else
            //{
            //    ExplorerDao.AddReocrdHistory(new List<Guid>() { source.Id }, history);
            //}
        }

        private void AppendMetaInfo(Record record)
        {
            if (string.IsNullOrWhiteSpace(record.MetaInfo))
            {
                RecordMetaInfo metaInfo = new RecordMetaInfo
                {
                    DataStatus = (int)DataStatus.Moved
                };
                record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
            else
            {
                var metaInfo = JsonConvert.DeserializeObject<RecordMetaInfo>(record.MetaInfo);
                metaInfo.DataStatus = (int)DataStatus.Moved;
                record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
            }
        }

        private int GetDestinationFlag(RemoteSiteCollection site)
        {
            if (site.NodeType == GCommon.Contract.Server.ControlPanel.Office365.RemoveNodeType.SkyDrivePro)
            {
                return (int)SourceFlag.OneDrive;
            }
            else if (site.NodeType == RemoveNodeType.O365GroupSites || site.NodeType == RemoveNodeType.PrivateChannel)
            {
                if (AvePoint.Wrapper.Common.WrapperConfiguration.IsEnableTeams)
                {
                    return (int)SourceFlag.Teams;
                }
                else
                {
                    return (int)SourceFlag.SharePoint;
                }
            }
            else
            {
                return (int)SourceFlag.SharePoint;
            }
        }



        private int GenerateNodeType(int sourceNodeType, int destFlag)
        {
            var destNodeType = sourceNodeType;
            var sourceFlag = RecordFlag.None;
            if (sourceNodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.Item)
            {
                sourceFlag = RecordFlag.SP;
            }
            else if (sourceNodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.FSFile || sourceNodeType == (int)GCommon.Contract.Tree.Object.NodeLevel.FSFolder)
            {
                sourceFlag = RecordFlag.FS;
            }
            if ((int)sourceFlag != destFlag)
            {
                logger.Info(string.Format("Need to calculate node type, source is : {0}, destination is {1}", sourceFlag.ToString(), destFlag.ToString()));
                if (destFlag == (int)RecordFlag.SP)
                {
                    switch (sourceNodeType)
                    {
                        case (int)GCommon.Contract.Tree.Object.NodeLevel.FSFile:
                            {
                                destNodeType = (int)GCommon.Contract.Tree.Object.NodeLevel.Item;
                                break;
                            }
                        case (int)GCommon.Contract.Tree.Object.NodeLevel.FSFolder:
                            {
                                destNodeType = (int)GCommon.Contract.Tree.Object.NodeLevel.Folder;
                                break;
                            }
                        default:
                            throw new Exception(string.Format("Do not support move the file type: {0}", sourceNodeType));
                    }
                }
                else if (destFlag == (int)RecordFlag.FS)
                {
                    //Current we only support fs to fs, so no need to convert node type
                }
            }
            return destNodeType;
        }

        private Record GetRecord(Guid nodeId)
        {
            return this.GetRecords(nodeId).FirstOrDefault();
            //if (record.Count > 0)
            //{
            //    return record[0];
            //}
            //else
            //{
            //    return null;
            //}
        }

        public List<Record> GetRecords(params Guid[] nodeIds)
        {
            var ids = nodeIds.ToList();
            return ExplorerDao.GetRecordByIds(ids);
        }

        private void AddHistory(Guid recordsId, string history, string sourceUrl, string destUrl, string user, JobDetailsStatus status = JobDetailsStatus.Successful)
        {
            //RecordHistoryXml recordHistory;
            string actionKey = string.Empty;
            switch (status)
            {
                case JobDetailsStatus.Successful:
                    actionKey = "RM_Explorer_RecordHistorySuccessfulInformation";
                    break;
                case JobDetailsStatus.Skipped:
                    actionKey = "RM_Explorer_RecordHistorySkippedInformation";
                    break;
                case JobDetailsStatus.Failed:
                    actionKey = "RM_Explorer_RecordHistoryFailedInformation";
                    break;
                default:
                    break;
            }
            RecordsHistoryService.AddRecordsHistory(new List<Guid> { recordsId }, $"{actionKey}{I18NEntity.Separator}{sourceUrl}{I18NEntity.Separator}{destUrl}");
            //if (!string.IsNullOrEmpty(history))
            //{
            //    var old = XmlUtil.GetXmlObject<RecordHistoryXml>(history);
            //    old.HistoryList.Add(new RecordHistory() { Action = $"{actionKey}{I18NEntity.Separator}{sourceUrl}{I18NEntity.Separator}{destUrl}", User = user, TimeUTC = DateTime.UtcNow.Ticks });
            //    recordHistory = old;
            //}
            //else
            //{
            //    RecordHistoryXml historyXML = new RecordHistoryXml
            //    {
            //        HistoryList = new List<Contract.RMWeb.RecordHistory>()
            //    };
            //    historyXML.HistoryList.Add(new RecordHistory() { Action = $"{actionKey}{I18NEntity.Separator}{sourceUrl}{I18NEntity.Separator}{destUrl}", User = user, TimeUTC = DateTime.UtcNow.Ticks });
            //    recordHistory = historyXML;
            //}
            //return recordHistory;
        }

        private Record GenerateRecord(Record sourceRecord, JobResult jobResult, SourceBase source, MoveDestinationManager desInfo, string user)
        {
            sourceRecord.ScopeId = jobResult.DestStub.DestFlag == (int)RecordFlag.SP ? (desInfo.Destination as SPDestination).SiteId : desInfo.DestRootPath.ToLowerInvariant().ToMd5();
            sourceRecord.Id = IDGenerator.GetRecordId(sourceRecord.ScopeId, jobResult.DestStub.ItemId);
            sourceRecord.NodeId = jobResult.DestStub.ItemId;
            sourceRecord.AveSiteId = desInfo.AveSiteId.ToString();
            sourceRecord.NodeType = GenerateNodeType(source.NodeType, jobResult.DestStub.DestFlag);
            sourceRecord.LeafName = jobResult.DestStub.LeafName;
            sourceRecord.DirPath = jobResult.DestStub.DirPath;
            sourceRecord.CollectTime = DateTime.UtcNow.Ticks;
            //sourceRecord.RecordHistory = XmlUtil.GetXmlString(history);
            sourceRecord.RecordHistory = "";
            sourceRecord.WebId = jobResult.DestStub.WebId;
            sourceRecord.ListId = jobResult.DestStub.ListId;
            sourceRecord.FolderId = jobResult.DestStub.FolderId;
            sourceRecord.ItemId = jobResult.DestStub.ItemId;
            sourceRecord.ItemRowId = jobResult.DestStub.ItemRowId;
            sourceRecord.ParentId = jobResult.DestStub.ParentId;
            sourceRecord.TimeModified = jobResult.DestStub.DateModified;
            sourceRecord.ContainerId = desInfo.ContainerId;
            return sourceRecord;
        }

        public Record GetRecord4Phy(Guid nodeId)
        {
            return this.GetRecords(nodeId).FirstOrDefault();
        }

        public void UpdateRecordRelatedInfo(Guid id, List<RMRelatedItemInfo> updateResult)
        {
            ExplorerDao.UpdateAll(r => r.NodeId == id, rec =>
            {
                if (updateResult == null)
                {
                    rec.RelatedRecordsCount = 0;
                    rec.RelatedRecords = string.Empty;
                }
                else
                {
                    rec.RelatedRecordsCount = updateResult.Count;
                    rec.RelatedRecords = SerializerHelper.SerializeToXmlString(updateResult);
                }
            });
        }

        internal class UpdateDBInfo
        {
            //Restore 后返回的JobResult，里面包含了Move 后目的端的信息DestStub
            public JobResult JobResult;
            //Explorer db中的主键Id， 从缓存中查到的数据，通过Id 进行后期的update 操作
            public int Id;
            //从缓存中获取的Source 文件在DB 中的部分属性，目前只有Id， scopeid， nodeId，不可添加nvarchar 等string到缓存，会影响内存, 目前没有用到，预留属性
            //public RMBaseRecord SourceDBInfo;
            //转移数据的原端信息，
            public SourceBase Source;
            //转移数据的目的端信息
            public MoveDestinationManager DesInfo;
            //执行转移操作的User，目前用来更新RecordHistory
            public string User;
        }
    }
}
