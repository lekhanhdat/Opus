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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object.JobMessage;
using AvePoint.RA.Contract.RMRelatedRecord;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.RelatedRecords;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RMExplorer
{
    public class RMTrimRelatedWorker : IDisposable
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMTrimRelatedWorker));


        public IRMManagedRecordRelatedDao recordRelatedDao => PlatformWindsorManager.GetService<IRMManagedRecordRelatedDao>();
        private IRMLocationDao LocationDao => PlatformWindsorManager.GetService<IRMLocationDao>();
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao(true);
                }
                return _explorerDao;
            }
        }
        private IRMScopeDao RMScopeDao => PlatformWindsorManager.GetService<IRMScopeDao>();
        private static readonly IRMReportManager ReportManager = ReportMangerFactory.Instance.ReportManager;

        private JobType jobType;
        private int RelatedBaseOn = 0;
        private string jobRunBy;
        private string mCurrentJobId;
        private AvePoint.RA.SharePoint.Object.JobResult Result;
        private string relationshipFileName = "ImportRecordTRIMRelationship.csv";
        private string commomErrorMessage = "RM_TS_SS_Summary";
        private List<RMLocation> locationCache = new List<RMLocation>();

        public RMTrimRelatedWorker(RMImportJobMessage message)
        {
            this.jobType = message.JobType;
            this.RelatedBaseOn = message.SharePointSettingID;
            this.mCurrentJobId = message.JobID;
            this.jobRunBy = message.JobRunBy;
            ReportMangerFactory.Instance.Init(mCurrentJobId, this.jobType);
            Result = new AvePoint.RA.SharePoint.Object.JobResult();
            //默认初始化 进度为2
            ReportManager.Increase(2);
            ReportManager.StartUpdateJobProgress();
        }

        #region Base On Physical Import


        public void ProcessRelatedMain()
        {
            string logstr = this.RelatedBaseOn == 0 ? "Physical" : "Electronic";
            logger.Info($"Start to process record relationships base on {logstr}");
            if (this.RelatedBaseOn == 0)
            {
                this.InitLocationCache();
                this.ProcessPhysicalRelated();
            }
            else
            {
                this.ProcessSPRelated();
            }
        }

        public void ProcessPhysicalRelated()
        {
            JobStatus status = JobStatus.None;
            try
            {
                List<RMManagedRecordRelated> allRelated = this.ExcludeDuplicate(recordRelatedDao.GetAll());
                if (allRelated.Count > 0)
                {
                    logger.Info("Related info count in RMManagedRecordRelateds is {0}", allRelated.Count);
                    List<GroupRelatedInfoStr> groupedRelated = allRelated.GroupBy(a => a.SrcUniqueId).Select(rel => new GroupRelatedInfoStr() { UniqueId = rel.Key, Relateds = rel.ToList() }).ToList();
                    logger.Info($"Start to process related records, grouped related count: {groupedRelated.Count}");
                    foreach (GroupRelatedInfoStr related in groupedRelated)
                    {
                        string relatedLog = string.Join(";", related.Relateds.Select(a => a.RelatedUniqueId).ToArray());
                        logger.Debug($"Src unique Id:{related.UniqueId}, related count:{related.Relateds.Count}, related:{relatedLog}");
                        this.PhysicalRelated(related);
                    }
                }
                status = Result.HasFailed
                   ? Result.HasSuccessful
                       ? JobStatus.FinishWithException
                       : JobStatus.Failed
                   : JobStatus.Finished;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw e;
            }
            finally
            {
                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                   ? commomErrorMessage
                   : string.Empty;
                ReportManager.SetJobFinished(status, jobComment);
                RelatedRecordsUtility.ClearContextCache();
            }
        }

        private void PhysicalRelated(GroupRelatedInfoStr related)
        {
            JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
            detail.SrcType = "Physical";
            detail.SrcName = related.UniqueId;
            try
            {
                logger.Info("Start one-----------------------");
                Record src = ExplorerDao.GetPhysicalRecordByRecordsId(related.UniqueId);
                if (src == null)
                {
                    logger.Warn("Src records is null");
                    this.AssembleNotFoundSourceDetail(related);
                    return;
                }
                string locationDirPath = this.AssebleLocationFullPath(src);
                if(src.NodeType == (int)RMNodeType.PhyBox)
                {
                    logger.Warn("Src record is physical box");
                    AssembleSourceBoxFailedDetail(related, src, locationDirPath);
                    return;
                }
                if (src.SendTo == "sub folder")
                {
                    logger.Warn("Src record is sub folder");
                    AssembleSubFolderSourceDetail(src, related, locationDirPath);
                    return;
                }
                detail.SrcId = src.RecordsId;
                List<Record> relList = ExplorerDao.GetPhysicalRecordByRecordIds(related.Relateds.Select(a => a.RelatedUniqueId).ToList());
                List<Record> boxList = relList.Where(a => a.NodeType == (int)RMNodeType.PhyBox).ToList();
                this.AssembleBoxRelatedDetail(src, boxList, locationDirPath);
                List<Record> subfolderList = relList.Where(a => a.SendTo == "sub folder").ToList();
                this.AssembleSubFolderRelatedDetail(src, subfolderList, locationDirPath);
                if (boxList.Count + subfolderList.Count == related.Relateds.Count)
                {
                    logger.Info("All related records are box or sub folder, none to proceed");
                    return;
                }
                List<Record> availableList = relList.Where(a => a.NodeType != (int)RMNodeType.PhyBox && a.SendTo != "sub folder").ToList();
                if (availableList.Count > 0)
                {
                    logger.Info("Related physical records count {0}, list:{1}", availableList.Count, string.Format(";", availableList.Select(a => a.RecordsId).ToArray()));
                    detail.DestType = "Physical";
                    detail.DestName = string.Join(";", availableList.Select(a => a.RecordsId).ToArray()) + ";";
                }
                Dictionary<Guid, RMScope> dicMap = null;
                List<RMManagedRecordRelated> nonePhysicalRelates = related.Relateds.Where(a => !availableList.Any(r => r.RecordsId == a.RelatedUniqueId)).ToList();
                List<Record> spList = GetSPRelatedRecordsInPhysical(nonePhysicalRelates);
                if (spList.Count > 0)
                {
                    string relatedSPName = string.Join(";", spList.Select(a => a.RecordsId).ToArray());
                    logger.Info("Related SP Record count {0}, list:{1}", spList.Count, relatedSPName);
                    dicMap = RMScopeDao.GetScopeInfoByIds(spList.Select(a => a.ScopeId).ToList());
                    List<Record> spfolder = spList.Where(a => a.NodeType == (int)NodeLevel.Folder).ToList();
                    this.AssembleSPFolderRelatedDetail4Physical(src, spfolder, locationDirPath, dicMap); 
                    availableList.AddRange(spList.Where(a=>a.NodeType != (int)NodeLevel.Folder));
                    detail.DestType = detail.DestType == null ? "Electronic" : detail.DestType + ", Electronic";
                    detail.DestName += relatedSPName + ";";
                } 
                this.AssembleNotFoundRelatedDetail(src, related, relList, spList, locationDirPath);
                if (availableList.Count == 0)
                {
                    logger.Info("No available related after filter.");
                    return;
                }
                List<RMRelatedItemInfo> relatedInfos = GetRelatedItemInfo(availableList, dicMap);
                availableList.Add(src); //收集完Related信息之后, 把自身也加入Record列表中, 方便最后更新DB
                Dictionary<UpdateRelatedRecordParams, string> result = null;
                using (RelatedRecordsUtility utility = new RelatedRecordsUtility(src, true))
                {
                    logger.Info("Start to update column");
                    result = utility.UpdateRelatedPropertiesForTool(src, relatedInfos, availableList);
                    Result.HasSuccessful = true;
                }
                AssembleFinalDetail(result, locationDirPath, dicMap);
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (InputParameterException ex)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = ex.Message;
                Result.HasFailed = true;
                logger.Warn(ex.ToString());
            }
            catch (SkipItemException ex)
            {
                detail.Status = JobDetailsStatus.Skipped;
                detail.Comment = ex.Message;
            }
            catch (GCommon.Utility.AveException ae)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = ae.Message;
                Result.HasFailed = true;
                logger.Error("process related failed for {0}, related {1}", related.UniqueId, string.Join(", ", related.Relateds.Select(a => a.RelatedUniqueId).ToArray()));
            }
            catch (Exception e)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                Result.HasFailed = true;
                logger.Error(e.Message, e);
                logger.Error("process related failed for {0}, related {1}", related.UniqueId, string.Join(", ", related.Relateds.Select(a => a.RelatedUniqueId).ToArray()));
                ReportManager.SendJobDetail(detail);
            }
            finally
            {
                logger.Info("Finish one-------------------------");
                ReportManager.Increase();
                //if (!CheckJobStatusUtility.isStopping)
                //{
                //    ReportManager.SendJobDetail(detail);
                //}
            }
        }

        private void AssembleFinalDetail(Dictionary<UpdateRelatedRecordParams, string> result, string locationPath, Dictionary<Guid, RMScope> dicMap)
        {
            if(result == null)
            {
                return;
            }
            try
            {
                foreach (var item in result)
                {
                    RMRelatedItemInfo src = item.Key.SourceInfo;  //DestRelatedInfos.First(r => r.id == item.Key.SourceInfo.id && r.SiteId == item.Key.SourceInfo.SiteId);
                    RMRelatedItemInfo dest = item.Key.DesInfo; //SourceRelatedInfos.First(r => r.id == item.Key.DesInfo.id && r.SiteId == item.Key.DesInfo.SiteId);
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = src.recId;
                    detail.SrcName = src.name;
                    detail.SrcLocation = locationPath;
                    detail.SrcType = "Physical"; 
                    if (item.Key.DesInfo.SourceFlag == (int)SourceFlag.Physical)
                    {
                        detail.DestType = "Physical";
                        detail.DestItemId = dest.recId;
                        detail.DestName = dest.name;
                    }
                    else
                    {
                        detail.DestType = "Electronic";
                        detail.DestItemId = dest.id + "";
                        detail.DestItemUrl = dest.url;
                        detail.DestSiteId = dest.SiteId + "";
                        detail.DestSiteUrl = dicMap[dest.SiteId].FullPath;
                    }
                    if (item.Value == "")
                    {
                        detail.Status = JobDetailsStatus.Successful;
                    }
                    else
                    {
                        Result.HasFailed = true;
                        detail.Status = JobDetailsStatus.Failed;
                        detail.Comment = item.Value;
                    }
                    ReportManager.SendJobDetail(detail);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        private void AssembleSourceBoxFailedDetail(GroupRelatedInfoStr related, Record src, string locationFullPath)
        {
            if (src.NodeType == (int)RMNodeType.PhyBox)
            {
                //全部Failed 
                foreach (RMManagedRecordRelated rec in related.Relateds)
                {
                    logger.Warn("The source record {0} is physical box which is unsupported to have related records or be added as related record, {1}", rec.SrcUniqueId, rec.RelatedUniqueId);
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = src.RecordsId;
                    detail.SrcName = src.LeafName;
                    detail.SrcType = "Physical";
                    detail.SrcLocation = locationFullPath; 
                    detail.DestItemId = rec.RelatedUniqueId;
                    detail.DestType = "Unkown";
                    detail.Comment = "The source record is physical box which is unsupported to have related records or be added as related record";
                    detail.Status = JobDetailsStatus.Failed;
                    ReportManager.SendJobDetail(detail);
                    Result.HasFailed = true;
                }
            }

        }
        #region Location Dir Path

        private string AssebleLocationFullPath(Record physicalRec)
        {
            ///location from dic
            return getLocationFullPath(physicalRec.LocationId);
        }
        private string getLocationFullPath(Guid LocationId)
        {
            if (this.locationCache.Any(a => a.UniqueId == LocationId))
            {
                RMLocation location = locationCache.First(a => a.UniqueId == LocationId);
                return getLocationFullPath(location);
            }
            logger.Error("No location found with id {0}", LocationId);
            return null;
        }

        private string getLocationFullPath(RMLocation location)
        {
            string dirPath = GetLocationPath(location.DirPath);
            return string.Format("{0}/{1}", dirPath, location.Name);
        }
        private string GetLocationPath(string dirPath)
        {
            var result = string.Empty;
            if (!string.IsNullOrEmpty(dirPath))
            {
                try
                {
                    dirPath = dirPath.TrimEnd('/');
                    List<string> locationIds = dirPath.Split('/').ToList();
                    for (int i = 0; i < locationIds.Count; i++)
                    {
                        int tempId = Convert.ToInt32(locationIds[i]);
                        if (locationCache.Any(a => a.Id == tempId))
                        {
                            RMLocation tempLocation = locationCache.First(a => a.Id == tempId);
                            string tempPath = tempLocation.Name;
                            if (i == 0)
                            {
                                result = tempPath;
                            }
                            else
                            {
                                result = result + "/" + tempPath;
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                }
            }
            return result;
        }
        #endregion
        private void AssembleSubFolderSourceDetail(Record src, GroupRelatedInfoStr related, string location)
        {
            try
            {
                foreach (RMManagedRecordRelated rec in related.Relateds)
                {
                    logger.Warn("src {0} is sub folder, related item id {0}", related.UniqueId, rec.SrcUniqueId);
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = related.UniqueId;
                    detail.SrcName = src.LeafName;
                    detail.SrcType = "Physical";
                    detail.SrcLocation = location;
                    detail.DestItemId = rec.RelatedUniqueId;
                    detail.DestType = "Unknown";
                    detail.Comment = "The source record is sub folder which is not migrated to physical records explorer";
                    detail.Status = JobDetailsStatus.Failed;
                    ReportManager.SendJobDetail(detail);
                    Result.HasFailed = true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
        private void AssembleSubFolderRelatedDetail(Record srcRecord, List<Record> subFolders, string location)
        {
            try
            {
                if (subFolders.Count > 0)
                {
                    foreach (Record rec in subFolders)
                    {
                        logger.Warn("src {0}, related item ids {0} is sub folder", srcRecord.RecordsId, rec.RecordsId);
                        JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                        detail.SrcId = srcRecord.RecordsId;
                        detail.SrcName = srcRecord.LeafName;
                        detail.SrcType = "Physical";
                        detail.SrcLocation = location;
                        detail.DestItemId = rec.RecordsId;
                        detail.DestType = "Physical";
                        detail.Comment = "The related record is sub folder which is not migrated to physical records explorer";
                        detail.Status = JobDetailsStatus.Failed;
                        ReportManager.SendJobDetail(detail);
                        Result.HasFailed = true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
        private void AssembleNotFoundSourceDetail(GroupRelatedInfoStr related)
        {
            try
            {
                foreach (RMManagedRecordRelated rec in related.Relateds)
                {
                    logger.Warn("src {0} not found, related item id {0}", related.UniqueId, rec.RelatedUniqueId);
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = related.UniqueId;
                    //detail.SrcItemId = related.UniqueId;
                    detail.SrcType = "Physical";
                    detail.DestItemId = rec.RelatedUniqueId;
                    detail.DestType = "Unknown";
                    detail.Comment = "The source record doesn't exist or deleted by end user.";
                    detail.Status = JobDetailsStatus.Failed;
                    ReportManager.SendJobDetail(detail);
                    Result.HasFailed = true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
        private void AssembleNotFoundRelatedDetail(Record srcRecord, GroupRelatedInfoStr related, List<Record> relatedPhyRecords, List<Record> relatedSPRecords, string location)
        {
            try
            {
                //List<Guid> relatedIds = related.Relateds.Select(a => a.DestRecordId).ToList();
                List<RMManagedRecordRelated> notFoundRelations = related.Relateds.Where(a => !relatedPhyRecords.Any(r => r.RecordsId == a.RelatedUniqueId) && !relatedSPRecords.Any(r=> r.ApproveUsers == a.RelatedUniqueId)).ToList();
                if (notFoundRelations.Count > 0)
                {
                    foreach (RMManagedRecordRelated rec in notFoundRelations)
                    {
                        logger.Warn("src {0}, related item id {0} not found", rec.SrcUniqueId, rec.RelatedUniqueId);
                        JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                        detail.SrcId = srcRecord.RecordsId;
                        detail.SrcName = srcRecord.LeafName;
                        detail.SrcType = "Physical";
                        detail.SrcLocation = location;
                        detail.DestItemId = rec.RelatedUniqueId;
                        detail.DestType = "Unknown";
                        detail.Comment = "The related record is neither in physical records explorer nor in electronic records explorer";
                        detail.Status = JobDetailsStatus.Failed;
                        ReportManager.SendJobDetail(detail);
                        Result.HasFailed = true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        private void AssembleBoxRelatedDetail(Record srcRecord, List<Record> relatedBoxRecords, string locationFullPath)
        {
            try
            {
                //List<Guid> relatedIds = related.Relateds.Select(a => a.DestRecordId).ToList();
                if (relatedBoxRecords.Count > 0)
                {
                    foreach (Record rec in relatedBoxRecords)
                    {
                        logger.Warn("src {0}, The related record {1} is physical box which is unsupported to be have related records or be added as related record", srcRecord.RecordsId, rec.RecordsId);
                        JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                        detail.SrcId = srcRecord.RecordsId;
                        detail.SrcName = srcRecord.LeafName;
                        detail.SrcType = "Physical";
                        detail.SrcLocation = locationFullPath;
                        detail.DestName = rec.LeafName;
                        detail.DestItemId = rec.RecordsId;
                        detail.DestType = "Physical";
                        detail.Comment = "The related record is physical box which is unsupported to be have related records or be added as related record";
                        detail.Status = JobDetailsStatus.Failed;
                        ReportManager.SendJobDetail(detail);
                        Result.HasFailed = true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
        private void AssembleSPFolderRelatedDetail4Physical(Record srcRecord, List<Record> relSPFolder, string locationFullPath, Dictionary<Guid, RMScope> dicMap)
        {
            foreach (Record rec in relSPFolder)
            {
                logger.Warn("src {0}, The related record {1} is electronic folder and doesn't exist in electronic records explorer", srcRecord.RecordsId, rec.RecordsId);
                JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                detail.SrcId = srcRecord.RecordsId;
                detail.SrcName = srcRecord.LeafName;
                detail.SrcType = "Physical";
                detail.SrcLocation = locationFullPath;
                detail.DestName = rec.LeafName;
                detail.DestItemId = rec.RecordsId;
                detail.DestItemUrl = rec.DirPath;
                detail.DestSiteId = rec.ScopeId + "";
                detail.DestSiteUrl = dicMap[rec.ScopeId].FullPath; // rec.ScopeId+"";
                detail.DestType = "Electronic";
                detail.Comment = "The related record is electronic folder and doesn't exist in electronic records explorer";
                detail.Status = JobDetailsStatus.Failed;
                ReportManager.SendJobDetail(detail);
                Result.HasFailed = true;
            }
        }

        private List<RMManagedRecordRelated> ExcludeDuplicate(List<RMManagedRecordRelated> list)
        {
            List<RMManagedRecordRelated> result = new List<RMManagedRecordRelated>();
            foreach (RMManagedRecordRelated rel in list)
            {
                if (!result.Any(a => (a.SrcUniqueId == rel.SrcUniqueId && a.RelatedUniqueId == rel.RelatedUniqueId) || (a.SrcUniqueId == rel.RelatedUniqueId && a.RelatedUniqueId == rel.SrcUniqueId)))
                {
                    result.Add(rel);
                }
            }
            return result;
        }


        private List<Record> GetSPRelatedRecordsInPhysical(List<RMManagedRecordRelated> nonePhysicalRelates)
        {
            if (nonePhysicalRelates == null || nonePhysicalRelates.Count == 0)
            {
                return new List<Record>();
            }
            List<string> spUniqueIds = nonePhysicalRelates.Select(a => a.RelatedUniqueId).ToList();
            List<Record> list = ExplorerDao.QueryAll(a => spUniqueIds.Contains(a.ApproveUsers)).ToList();
            return list;
        }

        private List<RMRelatedItemInfo> GetRelatedItemInfo(List<Record> records, Dictionary<Guid, RMScope> dicMap)
        {
            List<RMRelatedItemInfo> result = new List<RMRelatedItemInfo>();
            foreach (var relatedRecord in records)
            {
                var iteminfo = relatedRecord;
                if (iteminfo.SourceFlag == (int)SourceFlag.SharePoint)
                {
                    if (dicMap.ContainsKey(iteminfo.ScopeId))
                    {
                        var sPath = dicMap[iteminfo.ScopeId];
                        iteminfo.FullPath = WebUtil.MakeFullUrl(sPath?.FullPath, iteminfo.DirPath);
                        if (iteminfo.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem")
                        {
                            iteminfo.FullPath = WebUtil.GetListItemRealPath(iteminfo.FullPath);
                        }
                    }
                    RMRelatedItemInfo info = new RMRelatedItemInfo()
                    {
                        SourceFlag = iteminfo.SourceFlag,
                        DocLibRowId = iteminfo.ItemRowId,
                        ListId = iteminfo.ListId,
                        url = iteminfo.FullPath,
                        name = iteminfo.LeafName,
                        NeedDelete = false,
                        id = iteminfo.ItemId,
                        FolderId = iteminfo.FolderId,
                        WebId = iteminfo.WebId,
                        AveId = iteminfo.AveSiteId,
                        SiteId = iteminfo.ScopeId,
                        NodeType = iteminfo.NodeType,
                        level = iteminfo.ExtensionForFile == "RM_RDM_RecordDetails_DataType_SPItem" ? SORelativeDataArchiverNodeLevel.Item : SORelativeDataArchiverNodeLevel.Document,
                    };
                    result.Add(info);
                }
                else if (iteminfo.SourceFlag == (int)SourceFlag.Physical)
                {
                    RMRelatedItemInfo info = new RMRelatedItemInfo()
                    {
                        SourceFlag = iteminfo.SourceFlag,
                        id = iteminfo.Id,
                        //url = LocationManagementService.GetLocationPathById(iteminfo.LocationId),
                        //name = iteminfo.LeafName,
                        recId = iteminfo.RecordsId,
                        NeedDelete = false,
                        NodeType = iteminfo.NodeType,
                    };
                    result.Add(info);
                }
            }
            return result;
        }
        #endregion

        public void Dispose()
        {

        }

        #region Base On SP Mig DB

        public void ProcessSPRelated()
        {
            JobStatus status = JobStatus.None;
            try
            {
                this.InitTRIMRelationShip();
                //group by siteid
                Dictionary<Guid, List<MigRelateship>> siteDic = this.spRelatedIdMap.GroupBy(a => a.SrcSiteId).ToDictionary(rel => rel.Key, val => val.ToList());
                foreach (KeyValuePair<Guid, List<MigRelateship>> val in siteDic)
                {
                    ProcessOneSite(val);
                }
                status = Result.HasFailed
                  ? Result.HasSuccessful
                      ? JobStatus.FinishWithException
                      : JobStatus.Failed
                  : JobStatus.Finished;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw e;
            }
            finally
            {
                string jobComment = (status == JobStatus.FinishWithException || status == JobStatus.Failed)
                   ? commomErrorMessage
                   : string.Empty;
                ReportManager.SetJobFinished(status, jobComment);
                RelatedRecordsUtility.ClearContextCache();
            }
        }

        private void ProcessOneSite(KeyValuePair<Guid, List<MigRelateship>> valueInSite)
        {
            Guid scopeId = valueInSite.Key;
            List<MigRelateship> mappingList = valueInSite.Value;

            logger.Info($"Related info count in Site {scopeId} is {mappingList.Count}");
            List<GroupRelatedInfoGuid> groupedRelated = mappingList.GroupBy(a => a.SrcRecordId).Select(rel => new GroupRelatedInfoGuid() { SrcRecordId = rel.Key, Relateds = rel.ToList() }).ToList();
            foreach (GroupRelatedInfoGuid rel in groupedRelated)
            {
                ElectronicRelated(scopeId, rel);
            }
        }
        private void ElectronicRelated(Guid scopeId, GroupRelatedInfoGuid related)
        {
            JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
            detail.SrcType = "Electronic";
            detail.DestType = "Electronic"; 
            try
            {
                logger.Info("Start one---------------------------------"); 
                Record src = ExplorerDao.ReadById(scopeId, related.SrcRecordId);
                if (src == null)
                {
                    logger.Warn("Src record {0} is null", related.SrcRecordId);
                    AssembleNotFoundSourceDetail(related);
                    return; 
                }
                detail.SrcItemId = src.ItemId + "";
                detail.SrcItemUrl = src.DirPath;
                detail.SrcSiteId = src.ScopeId + "";
                if(src.NodeType == (int)NodeLevel.Folder)
                {
                    logger.Warn("Src record {0} is SP Folder", src.RecordsId);
                    AssembleFolderSourceDetail(related);
                    return;
                } 
                List<Record> allList = ExplorerDao.GetRecordByIds(related.Relateds.Select(a => a.DestRecordId).ToList());
                this.AssembleNotFoundRelatedDetail(src, related, allList);

                this.AssembleFolderRelatedDetail(allList, related);
                List<Record> relList = allList.Where(a => a.NodeType != (int)NodeLevel.Folder).ToList();
                if (relList.Count == 0)
                {
                    logger.Info("No available item level record.");
                    return;
                } 
                logger.Info("Source record {0}, related: {1}", src.DirPath, string.Join(";", relList.Select(a => a.DirPath).ToArray()));
                List<Record> temp = new List<Record>(relList);
                temp.Add(src);
                Dictionary<Guid, RMScope> dicMap = RMScopeDao.GetScopeInfoByIds(temp.Select(a => a.ScopeId).Distinct().ToList()); 
                List<RMRelatedItemInfo> relatedInfos = GetRelatedItemInfo(relList, dicMap);
                relList.Add(src); //把自身也加入所有关联的列表
                Dictionary<UpdateRelatedRecordParams, string> result = null;
                using (RelatedRecordsUtility utility = new RelatedRecordsUtility(src, true))
                {
                    try
                    {
                        result = utility.UpdateRelatedPropertiesForTool(src, relatedInfos, relList);
                        Result.HasSuccessful = true;
                    }
                    catch (Exception e)
                    {
                        Result.HasFailed = true;
                        //SP更新各个 Client Query都有可能出问题， 这里Catch到异常比较Result个数， 不足的用异常信息补齐
                        logger.Error(e.Message, e); 
                        if(result == null || result.Count < relatedInfos.Count)
                        {
                            List<RMRelatedItemInfo> unProcessedRelated = relatedInfos.Where(a => result.Keys.Any(r => r.DesInfo.id == a.id && r.DesInfo.SiteId == a.SiteId)).ToList();
                            this.AssembleUnprocessedAfterException(src, unProcessedRelated, dicMap, string.Format("Unexpected error: {0}",e.Message));
                        }
                    }
                }
                AssembleFinalDetailElectronic(result, dicMap);
            }
            catch (JobStopException)
            {
                throw new JobStopException("This Job is stopped.");
            }
            catch (InputParameterException ex)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = ex.Message;
                Result.HasFailed = true;
                logger.Warn(ex.ToString());
            }
            catch (SkipItemException ex)
            {
                detail.Status = JobDetailsStatus.Skipped;
                detail.Comment = ex.Message;
            }
            catch (GCommon.Utility.AveException ae)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = ae.Message;
                Result.HasFailed = true;
                logger.Error("process related failed for {0}, related {1}", related.SrcRecordId, string.Join(", ", related.Relateds.Select(a => a.DestRecordId).ToArray()));
                ReportManager.SendJobDetail(detail);
            }
            catch (Exception e)
            {
                detail.Status = JobDetailsStatus.Failed;
                detail.Comment = string.Format(I18NEntity.GetString("RM_CommonErrorMessage"), e.Message);
                Result.HasFailed = true;
                logger.Error(e.Message, e);
                logger.Error("process related failed for {0}, related {1}", related.SrcRecordId, string.Join(", ", related.Relateds.Select(a => a.DestRecordId).ToArray()));
                ReportManager.SendJobDetail(detail);
            }
            finally
            {
                ReportManager.Increase();
                if (!CheckJobStatusUtility.isStopping)
                {
                    //ReportManager.SendJobDetail(detail);
                }
                logger.Info("Finish one------------------------------------");
            }
        }

        private void AssembleFinalDetailElectronic(Dictionary<UpdateRelatedRecordParams, string> result, Dictionary<Guid, RMScope> dicMap)
        {
            if(result == null)
            {
                return;
            }
            try
            {
                foreach (var item in result)
                {
                    RMRelatedItemInfo src = item.Key.SourceInfo;
                    RMRelatedItemInfo dest = item.Key.DesInfo;
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = src.recId;
                    detail.SrcType = "Electronic";
                    detail.SrcItemId = src.id + "";
                    detail.SrcItemUrl = src.url;
                    detail.SrcSiteId = src.SiteId + "";
                    detail.SrcLocation = dicMap[src.SiteId].FullPath;
                     
                    detail.DestType = "Electronic";
                    detail.DestItemId = dest.id + "";
                    detail.DestItemUrl = dest.url;
                    detail.DestSiteId = dest.SiteId + "";
                    detail.DestSiteUrl = dicMap[dest.SiteId].FullPath; 
                    if (item.Value == "")
                    {
                        detail.Status = JobDetailsStatus.Successful;
                    }
                    else
                    {
                        Result.HasFailed = true;
                        detail.Status = JobDetailsStatus.Failed;
                        detail.Comment = item.Value;
                    }
                    ReportManager.SendJobDetail(detail);
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        private void AssembleUnprocessedAfterException(Record src, List<RMRelatedItemInfo> unProcessed, Dictionary<Guid, RMScope> dicMap, string errorMsg)
        {
            try
            {
                foreach (RMRelatedItemInfo rec in unProcessed)
                {
                    logger.Warn("src {0}, related item id {0}, unexpected error failed.", src.ItemId, rec.id);
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = src.RecordsId;
                    detail.SrcType = "Electronic";
                    detail.SrcItemId = src.ItemId + "";
                    detail.SrcItemUrl = src.DirPath;
                    detail.SrcSiteId = src.ScopeId + "";
                    detail.SrcLocation = dicMap[src.ScopeId].FullPath;
                    detail.DestItemId = rec.id + "";
                    detail.DestItemUrl = rec.url;
                    detail.DestSiteId = rec.SiteId + "";
                    detail.DestSiteUrl = dicMap.ContainsKey(rec.SiteId) ? dicMap[rec.SiteId].FullPath : "";
                    detail.DestType = "Electronic";
                    detail.Comment = errorMsg;
                    detail.Status = JobDetailsStatus.Failed;
                    ReportManager.SendJobDetail(detail);
                    Result.HasFailed = true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
       

        private void AssembleNotFoundSourceDetail(GroupRelatedInfoGuid related)
        {
            try
            {
                foreach (MigRelateship rec in related.Relateds)
                {
                    logger.Warn("src {0} not found, related item id {0}", rec.SrcItemId, rec.DestItemId);
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = rec.SrcRecordId +""; 
                    detail.SrcType = "Electronic";
                    detail.SrcItemId = rec.SrcItemId +"";
                    detail.SrcItemUrl = rec.SrcItemUrl;
                    detail.SrcSiteId = rec.SrcSiteId + "";
                    detail.SrcLocation = rec.SrcSiteUrl;
                    detail.DestItemId = rec.DestItemId + "";
                    detail.DestItemUrl = rec.DestItemUrl;
                    detail.DestSiteId = rec.DestSiteId + "";
                    detail.DestSiteUrl = rec.DestSiteUrl;
                    detail.DestType = "Electronic";
                    detail.Comment = "The source record doesn't exist or deleted by end user.";
                    detail.Status = JobDetailsStatus.Failed;
                    ReportManager.SendJobDetail(detail);
                    Result.HasFailed = true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
        private void AssembleNotFoundRelatedDetail(Record srcRecord, GroupRelatedInfoGuid related, List<Record> relatedRecords)
        {
            try
            { 
                List<MigRelateship> notFoundRelations = related.Relateds.Where(a => !relatedRecords.Any(r => r.Id == a.DestRecordId)).ToList();
                if (notFoundRelations.Count > 0)
                {
                    logger.Info("Not found related record count {0}", notFoundRelations.Count);
                    foreach (MigRelateship rec in notFoundRelations)
                    {
                        logger.Warn("src '{0}' site '{1}', related not fount item id '{2}' site '{3}'.", rec.SrcItemId, rec.SrcSiteId, rec.DestItemId, rec.DestSiteId);
                        JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                        detail.SrcId = rec.SrcRecordId + "";
                        detail.SrcType = "Electronic";
                        detail.SrcItemId = rec.SrcItemId + "";
                        detail.SrcItemUrl = rec.SrcItemUrl;
                        detail.SrcSiteId = rec.SrcSiteId + "";
                        detail.SrcLocation = rec.SrcSiteUrl;
                        detail.DestItemId = rec.DestItemId + "";
                        detail.DestItemUrl = rec.DestItemUrl;
                        detail.DestSiteId = rec.DestSiteId + "";
                        detail.DestSiteUrl = rec.DestSiteUrl;
                        detail.DestType = "Electronic";
                        detail.Comment = "The related record doesn't exist or deleted by end user";
                        detail.Status = JobDetailsStatus.Failed;
                        ReportManager.SendJobDetail(detail);
                        Result.HasFailed = true;
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        private void AssembleFolderSourceDetail(GroupRelatedInfoGuid related)
        {
            try
            {
                foreach (MigRelateship rec in related.Relateds)
                {
                    logger.Warn("src {0} is SP Folder, related item id {0}", rec.SrcItemId, rec.DestItemId);
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = rec.SrcRecordId + "";
                    detail.SrcType = "Electronic";
                    detail.SrcItemId = rec.SrcItemId + "";
                    detail.SrcItemUrl = rec.SrcItemUrl;
                    detail.SrcSiteId = rec.SrcSiteId + "";
                    detail.SrcLocation = rec.SrcSiteUrl;
                    detail.DestItemId = rec.DestItemId + "";
                    detail.DestItemUrl = rec.DestItemUrl;
                    detail.DestSiteId = rec.DestSiteId + "";
                    detail.DestSiteUrl = rec.DestSiteUrl;
                    detail.DestType = "Electronic";
                    detail.Comment = "The source record is folder which is unsupported to have related records or to be added as related records in Opus.";
                    detail.Status = JobDetailsStatus.Failed;
                    ReportManager.SendJobDetail(detail);
                    Result.HasFailed = true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }

        private void AssembleFolderRelatedDetail(List<Record> relList, GroupRelatedInfoGuid related)
        {
            try
            {
                int folder = (int)NodeLevel.Folder;
                List<MigRelateship> folders = related.Relateds.Where(a => relList.Any(r => r.NodeType == folder && a.DestRecordId == r.Id)).ToList();
                foreach (MigRelateship rec in folders)
                {
                    logger.Warn("src {0} , related is SP Folder item id {0}", rec.SrcItemId, rec.DestItemId);
                    JMImportRecordsRelatedJobDetail detail = new JMImportRecordsRelatedJobDetail();
                    detail.SrcId = rec.SrcRecordId + "";
                    detail.SrcType = "Electronic";
                    detail.SrcItemId = rec.SrcItemId + "";
                    detail.SrcItemUrl = rec.SrcItemUrl;
                    detail.SrcSiteId = rec.SrcSiteId + "";
                    detail.SrcLocation = rec.SrcSiteUrl;
                    detail.DestItemId = rec.DestItemId + "";
                    detail.DestItemUrl = rec.DestItemUrl;
                    detail.DestSiteId = rec.DestSiteId + "";
                    detail.DestSiteUrl = rec.DestSiteUrl;
                    detail.DestType = "Electronic";
                    detail.Comment = "The related record is folder which is unsupported to have related records or to be added as related records in Opus.";
                    detail.Status = JobDetailsStatus.Failed;
                    ReportManager.SendJobDetail(detail);
                    Result.HasFailed = true;
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
        }
        private void InitLocationCache()
        {
            if (this.locationCache == null || this.locationCache.Count == 0)
            {
                locationCache = LocationDao.GetAllLocations();
            }
        }

        private void InitTRIMRelationShip()
        {
            DateTime dt = DateTime.Now;
            var blobName = Path.Combine(JobReportUtility.GetTenantIdentity(), JobReportUtility.ImportCSVFile, relationshipFileName);
            string metaFileName = JobReportUtility.GetImportJobMetaFileWithoutDeletion(blobName);

            List<string[]> temp = new List<string[]>();
            using (FileStream fs = new FileStream(metaFileName, FileMode.Open, FileAccess.Read))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(fs, Encoding.UTF8))
                    {
                        while (!sr.EndOfStream)
                        {
                            string csvLine = sr.ReadLine();
                            if (csvLine != null) temp.Add(CSVHelper.AnalyseCSVRow2Array(csvLine));
                        }
                    }
                }
                catch (Exception e)
                {
                    logger.Error(e.Message, e);
                    throw e;
                }
            }
            CombineRelationshipID(temp);
            logger.Info("Distinct relationship count {0}", temp.Count);
        }

        private RelatedHeaderIndex checkHeader(string[] header)
        {
            RelatedHeaderIndex index = new RelatedHeaderIndex();
            for(int i = 0; i<header.Length; i++)
            {
                if(string.Equals(header[i], "srcItemId", StringComparison.OrdinalIgnoreCase))
                {
                    index.SrcItemId = i;
                }
                else if (string.Equals(header[i], "srcUrl", StringComparison.OrdinalIgnoreCase))
                {
                    index.HasUrlColumn = true;
                    index.SrcItemUrl = i;
                }
                else if (string.Equals(header[i], "srcSiteId", StringComparison.OrdinalIgnoreCase))
                {
                    index.SrcSiteId = i;
                }
                else if (string.Equals(header[i], "srcSiteUrl", StringComparison.OrdinalIgnoreCase))
                {
                    index.HasUrlColumn = true;
                    index.SrcSiteUrl = i;
                }
                else if (string.Equals(header[i], "relatedItemId", StringComparison.OrdinalIgnoreCase))
                {
                    index.DestItemId = i;
                }
                else if (string.Equals(header[i], "relatedUrl", StringComparison.OrdinalIgnoreCase))
                {
                    index.HasUrlColumn = true;
                    index.DestItemUrl = i;
                }
                else if (string.Equals(header[i], "relatedSiteId", StringComparison.OrdinalIgnoreCase))
                {
                    index.DestSiteId = i;
                }
                else if (string.Equals(header[i], "relatedSiteUrl", StringComparison.OrdinalIgnoreCase))
                {
                    index.HasUrlColumn = true;
                    index.DestSiteUrl = i;
                }
            }
            return index;
        }

        List<MigRelateship> spRelatedIdMap = new List<MigRelateship>();  //src NodeId, SiteId,  dest NodeId, SiteId
        private void CombineRelationshipID(List<string[]> temp)
        {
            RelatedHeaderIndex headerIndex = this.checkHeader(temp[0]);
            int index = 0;
            foreach (string[] row in temp)
            {
                if (index == 0 || row.Length < 4 || isCellEmpty(row[headerIndex.SrcItemId]) || isCellEmpty(row[headerIndex.SrcSiteId]) || isCellEmpty(row[headerIndex.DestItemId]) || isCellEmpty(row[headerIndex.DestSiteId]))
                {
                    index++;
                    continue;
                }
                index++;
                Guid srcId = new Guid(row[headerIndex.SrcItemId].Trim());
                Guid srcSiteId = new Guid(row[headerIndex.SrcSiteId].Trim());
                Guid srcRecordId = IDGenerator.GetRecordId(srcSiteId, srcId);  //not sure
                Guid destId = new Guid(row[headerIndex.DestItemId].Trim());
                Guid destSiteId = new Guid(row[headerIndex.DestSiteId].Trim());
                Guid destRecordId = IDGenerator.GetRecordId(destSiteId, destId);
                if (spRelatedIdMap.Any(a => (a.SrcRecordId == srcRecordId && a.DestRecordId == destRecordId) || (a.DestRecordId == srcRecordId && a.SrcRecordId == destRecordId)))
                {
                    logger.Debug($"Source site {srcSiteId}, and node id  {srcId}, related site  {destSiteId}, and node id {destId}, already exist");
                }
                else
                {
                    MigRelateship relateship = new MigRelateship {
                        SrcRecordId = srcRecordId, SrcItemId = srcId,
                        SrcSiteId = srcSiteId, DestRecordId = destRecordId,
                        DestSiteId = destSiteId, DestItemId = destId };
                    this.AppendUrlInfo(row, headerIndex, relateship);
                    spRelatedIdMap.Add(relateship);
                }
            }
            logger.Info("Relationship count without duplication is {0}", spRelatedIdMap.Count);
        }
        private void AppendUrlInfo(string[] row, RelatedHeaderIndex headerIndex, MigRelateship relateship)
        {
            if(headerIndex.SrcItemUrl != -1)
            {
                relateship.SrcItemUrl = row[headerIndex.SrcItemUrl];
            }
            if (headerIndex.SrcSiteUrl != -1)
            {
                relateship.SrcSiteUrl = row[headerIndex.SrcSiteUrl];
            }
            if (headerIndex.DestItemUrl != -1)
            {
                relateship.DestItemUrl = row[headerIndex.DestItemUrl];
            }
            if (headerIndex.DestSiteUrl != -1)
            {
                relateship.DestSiteUrl = row[headerIndex.DestSiteUrl];
            }
        }
        private bool isCellEmpty(string cell)
        {
            if (cell == null || cell == string.Empty)
            {
                return true;
            }
            if ("null".Equals(cell, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
            return false;
        }
        #endregion
    }

    internal class RelatedHeaderIndex
    {
        public int SrcItemId;
        public int SrcSiteId;
        public int SrcItemUrl;
        public int SrcSiteUrl;
        public int DestItemId;
        public int DestSiteId;
        public int DestItemUrl;
        public int DestSiteUrl;
        public bool HasUrlColumn;
        public RelatedHeaderIndex()
        {
            SrcItemId = -1;
            SrcSiteId = -1;
            SrcItemUrl = -1;
            SrcSiteUrl = -1;
            DestItemId = -1;
            DestSiteId = -1;
            DestItemUrl = -1;
            DestSiteUrl = -1;
        }
    }

    internal class GroupRelatedInfoStr
    {
        public string UniqueId { set; get; }
        public List<RMManagedRecordRelated> Relateds { set; get; }
    }
    internal class GroupRelatedInfoGuid
    { 
        public Guid SrcRecordId { set; get; }
        public List<MigRelateship> Relateds { set; get; }
    }
    internal class MigRelateship
    {
        public Guid SrcSiteId { set; get; }
        public Guid SrcRecordId { set; get; }
        /// <summary>
        /// 更新Detail用
        /// </summary>
        public Guid SrcItemId { set; get; }
        public Guid DestSiteId { set; get; }
        public Guid DestRecordId { set; get; }
        /// <summary>
        /// 更新Detail用
        /// </summary>
        public Guid DestItemId { set; get; }

        /// <summary>
        /// 更新Detail用
        /// </summary>
        public string SrcSiteUrl { set; get; } 
        /// <summary>
        /// 更新Detail用
        /// </summary>
        public string SrcItemUrl { set; get; }
        /// <summary>
        /// 更新Detail用
        /// </summary>
        public string DestSiteUrl { set; get; } 
        /// <summary>
        /// 更新Detail用
        /// </summary>
        public string DestItemUrl { set; get; }
    }
}
