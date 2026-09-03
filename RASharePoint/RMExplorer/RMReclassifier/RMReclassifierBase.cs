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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Report;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Global.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.SharePoint.Common;
using AvePoint.RA.SharePoint.EnforceRetention;
using AvePoint.Wrapper.Common;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
namespace AvePoint.RA.SharePoint.RMExplorer.RMReclassifier
{
    public abstract class RMReclassifierBase
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region interface
        private ISharePointSettingDao mSharePointSettingDao = null;
        public ISharePointSettingDao SharePointSettingDao
        {
            get
            {
                if (mSharePointSettingDao == null)
                {
                    mSharePointSettingDao = (ISharePointSettingDao)PlatformWindsorManager.GetService(typeof(ISharePointSettingDao));
                }
                return mSharePointSettingDao;
            }
        }

        private IRMClassificationHistoryDao mClassificationHistoryDao;
        protected IRMClassificationHistoryDao ClassificationHistoryDao
        {
            get
            {
                if (mClassificationHistoryDao == null)
                {
                    mClassificationHistoryDao = (IRMClassificationHistoryDao)PlatformWindsorManager.GetService(typeof(IRMClassificationHistoryDao));
                }
                return mClassificationHistoryDao;
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

        private ITenantService mTenantService;
        public ITenantService TenantService
        {
            get
            {
                if (mTenantService == null)
                {
                    mTenantService = (ITenantService)PlatformWindsorManager.GetService(typeof(ITenantService));
                }
                return mTenantService;
            }
        }


        #endregion

        public int mFailedCount = 0;
        public int mSucceedCount = 0;
        protected ChangeTermDto _jobContextDto;
        protected ExplorerDao _explorerDao = new ExplorerDao();
        protected bool overWriteExistingTerm = false;
        protected bool reclassifySubFiles = false;
        protected bool mNeedSendReport = true;
        protected Hashtable processedFolderIds = new Hashtable();
        public List<Guid> rootFolderIds = new List<Guid>();
        protected abstract SourceFlag Flag { get; }
        protected bool isNewLogicAccount;
        protected bool isManualData;
        public RMReclassifierBase(ChangeTermDto dto)
        {
            _jobContextDto = dto;
            overWriteExistingTerm = dto.OverWriteSubFiles;
            reclassifySubFiles = dto.ReclassifySubFiles;
            isManualData = dto.IsManualData;
            isNewLogicAccount = TenantService.IsNewOpusTenant();
            RMSPReclassifierCache.Instance.FolderDirPaths = new List<string>();
            RMSPReclassifierCache.Instance.Init(dto);
        }

        public async System.Threading.Tasks.Task RunForGlobalSearchActionAsync(List<Record> folders)
        {
            rootFolderIds = folders.Select(a => a.Id).ToList();
            var folderInfos = folders.Where(o => !string.IsNullOrEmpty(o.DirPath)).OrderBy(o => o.DirPath.Length).Select(o => new { o.DirPath, o.AveSiteId }).ToList();

            logger.Info($"total count of folder dir path, {folderInfos?.Count}");
            if (folderInfos != null && folderInfos.Count > 0)
            {
                foreach (var folderInfo in folderInfos)
                {
                    var folderDir = folderInfo.DirPath;
                    var aveSiteId = folderInfo.AveSiteId;

                    if (reclassifySubFiles)
                    {
                        if (RMSPReclassifierCache.Instance.ExistsParentFolderDirPath($"{aveSiteId}|{folderDir}"))
                        {
                            logger.Debug($"Skip the current folder dir path Already exists, siteId:{aveSiteId}, path:{folderDir}");
                            continue;
                        }
                        RMSPReclassifierCache.Instance.FolderDirPaths.Add(folderDir);
                    }
                    logger.Debug($"process folder dirpath is {folderDir} with siteId is {aveSiteId}");
                    var records = BrowseFolder(folderDir, aveSiteId);
                    await ChangeTermsAsync(records);
                }
            }
        }

        public virtual List<Record> BrowseFolder(string folderDirPath, string aveSiteId)
        {
            List<Record> tempList = new List<Record>();
            bool hasNext = true;
            string pageIndex = string.Empty;
            var pateSize = 5000;
            List<Record> datas = new List<Record>();
            while (hasNext)
            {
                Tuple<IEnumerable<Record>, string> result = _explorerDao.QueryByPage(GetQueryCondition(folderDirPath, aveSiteId), pateSize, pageIndex);
                hasNext = !string.IsNullOrEmpty(result.Item2);
                pageIndex = result.Item2;
                datas = result.Item1.ToList();
                tempList.AddRange(datas);
                logger.Debug($"Got {datas.Count()} children under the folder");
            }
            return tempList;
        }

        private Expression<Func<Record, bool>> GetQueryCondition(string folderDirPath, string aveSiteId)
        {
            var parentFolderPath = folderDirPath;
            if (!parentFolderPath.EndsWith("/"))
            {
                parentFolderPath += "/";
            }
            Expression<Func<Record, bool>> predicate;
            if (reclassifySubFiles)
            {
                predicate = o => o.SourceFlag == (int)Flag && o.RecordStatus == (int)RMRecordStatus.Active && o.AveSiteId == aveSiteId && (o.DirPath.StartsWith(parentFolderPath) || o.DirPath == folderDirPath);
            }
            else
            {
                predicate = o => o.SourceFlag == (int)Flag && (o.RecordStatus == (int)RMRecordStatus.Active || (o.RecordStatus == (int)RMRecordStatus.ManualPreSync && isManualData)) && o.AveSiteId == aveSiteId && (o.DirPath == folderDirPath);
            }
            return predicate;
        }

        protected IAveListItem GetAveListItem(Record record, IAveList list)
        {
            IAveListItem item;
            if (record.NodeType == (int)NodeLevel.Folder)
            {
                var folder = list.GetFolder(record.DirPath);
                if (folder.Exists)
                {
                    item = folder.Item;
                }
                else
                {
                    throw new Exception("Item does not exist. It may have been deleted by another user.");
                }
            }
            else
            {
                item = list.GetItemByUniqueId(record.ItemId);
            }
            return item;
        }

        protected bool NeedSkip(Record record)
        {
            if (record.NodeType == (int)NodeLevel.Folder && rootFolderIds.Contains(record.Id))
            {
                return false;
            }
            if (reclassifySubFiles && !overWriteExistingTerm && record.TermId != Guid.Empty)
            {
                return true;
            }
            return false;
        }

        protected bool IsProcessedFolder(Record record)
        {
            return record.NodeType == (int)NodeLevel.Folder && processedFolderIds.ContainsKey(record.NodeId);
        }

        protected void AddProcessedFolderId(Record record)
        {
            if (record.NodeType == (int)NodeLevel.Folder)
            {
                processedFolderIds.Add(record.NodeId, null);
            }
        }

        protected string GetRealException(Exception e)
        {
            if (e == null)
            {
                return null;
            }
            if(e.Message.Contains("Field or property \"RevIMBCS\" does not exist."))
            {
                return "RM_SPS_DS_NotFoundBCSColumn";
            }
            if (e is TargetInvocationException && e.InnerException != null)
            {
                return GetRealException(e.InnerException);
            }
            return e.Message;
        }

        protected string GetItemTypeI18N(Record record, bool isDocument)
        {
            if (record.NodeType == (int)NodeLevel.Folder)
            {
                return "RM_RDM_RecordDetails_DataType_SPFolder";
            }
            if (record.NodeType == (int)NodeLevel.Item)
            {
                return isDocument ? "RM_JS_Rule_CreateRule_FilterLevel_Document" : "RM_RDM_RecordDetails_DataType_SPItem";
            }
            return "";
        }
        public abstract System.Threading.Tasks.Task ChangeTermsAsync(List<Record> records);

        public void Dispose()
        {
            if (_explorerDao != null)
            {
                _explorerDao.Dispose();
            }
        }
    }
}
