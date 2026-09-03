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





using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using Microsoft.Data.SqlClient;
using AvePoint.Wrapper.Common;
using System.IO;
using System.Collections;
using System.Data;
using System.Xml;
using AvePoint.Common;
using AvePoint.Wrapper.Resource;
using AvePoint.Wrapper.Mapping;
using AvePoint.GCommon.Utility.I18N;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Wrapper.Restore
{
    [AveCodeReviewAttribute("2012/03/09", "Navy.Li@avepoint.com", "Bingkun.Wang@AvePoint.com",
        new string[] { CodeReviewConstants.CHECK_LIST_ID_CO_1, 
                       CodeReviewConstants.CHECK_LIST_ID_CO_6, 
                       CodeReviewConstants.CHECK_LIST_ID_FA_4 }, null, true)]
    public class AveSPItem : RestoreableObject,IDisposable
    {
        private static AveLogger log = AveLogger.GetInstance(typeof(AveSPItem));

        private IAveBackupRestoreQueryService mQueryService;
        protected IAveRestoreStream mReceiver;
        private AveSPFolder mParentFolder;
        private AveStorage mStorage;
        /// <summary>
        /// 详细请参考AveListSettingFlags类
        /// 1           ListSettingBackup       (0-No,      1-Yes)
        /// 2           IsListSettingChanged    (0-No,      1-Yes)
        /// 4           EnableVersions          (0-Disable, 1-Enable)
        /// 8           EnableMinorVersions     (0-Disable, 1-Enable)
        /// 16          EnableModeration        (0-Disable, 1-Enable)
        /// 32          ForceCheckout           (0-Disable, 1-Enable)
        /// </summary>
        //private int mListSettingFlag;
        //private DateTime mTimeCreated = DateTime.MinValue;
        //private DateTime mTimeLastModified = DateTime.MinValue;
        #region add for stub property
        private bool mIsStubData = false;
        private int mPicHeight;
        private int mPicWidth;
        #endregion
        private Dictionary<string, object> fieldsInMetaInfo = null;
        protected AveSPList mAveSPList;
        protected IAveTimeZone mAveTimeZone;
        private int mOriginalModerationStatus;
        protected AveBaseItemInfo mBaseItemInfo = new AveBaseItemInfo();
        private IAveItem mAveItem = null;
        private IReport report = new AveWrapperReport();
        public IReport GetReport()
        {
            return report;
        }
        private AveSPSite mAveParentSite;
        public AveStorage AveStorage
        {
            get { return mStorage; }
        }
        public bool HasStream
        {
            get { return mBaseItemInfo.HasStream; }
            set { this.mBaseItemInfo.HasStream = value; }
        }

        public AveBaseItemInfo BaseItemInfo
        {
            get { return mBaseItemInfo; }
        }


        public int RestoreVersion
        {
            get { return mBaseItemInfo.RestoreVersion; }
            set { mBaseItemInfo.RestoreVersion = value; }
        }
        public IAveRestoreStream Receiver
        {
            get { return this.mReceiver; }
            set { this.mReceiver = value; }
        }
        public IAveBackupRestoreQueryService QueryService
        {
            get { return this.mQueryService; }
            set { this.mQueryService = value; }
        }
        public bool IsStubData
        {
            get { return mIsStubData; }
        }
        public string Name
        {
            get { return mBaseItemInfo.Name; }
            set { mBaseItemInfo.Name = value; }
        }

        public string ScopeUrl
        {
            get { return mBaseItemInfo.ScopeUrl; }
            set { mBaseItemInfo.ScopeUrl = value; }
        }

        public Guid ScopeId
        {
            get { return mBaseItemInfo.ScopeId; }
            set { mBaseItemInfo.ScopeId = value; }
        }

        public Guid SiteId
        {
            get { return mBaseItemInfo.SiteId; }
            set { mBaseItemInfo.SiteId = value; }
        }

        public AveSPSite ParentSite { get { return mAveParentSite; } }

        public int Version
        {
            get { return mBaseItemInfo.Version; }
            set { mBaseItemInfo.Version = value; }
        }

        public int Level
        {
            get { return mBaseItemInfo.Level; }
            set { mBaseItemInfo.Level = value; }
        }

        public int RowId
        {
            get { return mBaseItemInfo.RowId; }
            set { mBaseItemInfo.RowId = value; }
        }

        public Guid Id
        {
            get { return mBaseItemInfo.GUID; }
            set { mBaseItemInfo.GUID = value; }
        }

        public bool HasUniqueRoleAssignments
        {
            get { return mBaseItemInfo.HasUniqueRoleAssignments; }
        }

        public IAveListItem SPListItem
        {
            get
            {
                if (mAveItem != null)
                {
                    return mAveItem.ListItem;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                mAveItem.ListItem = value;
            }
        }

        public IAveFile SPFile
        {
            get
            {
                if (mAveItem != null)
                {
                    return mAveItem.File;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                mAveItem.File = value;
            }
        }

        public IAveWeb SPWeb
        {
            get
            {
                return mAveItem != null ? mAveItem.Web : null;
            }
            set
            {
                mAveItem.Web = value;
            }
        }

        public AveSPFolder ParentFolder
        {
            get { return mParentFolder; }
        }

        public AveSPList ParentList
        {
            get
            {
                return mAveSPList;
            }
            set
            {
                mAveSPList = value;
            }
        }
        public int? InternalVersion
        {
            get { return mBaseItemInfo.InternalVersion; }
            set { mBaseItemInfo.InternalVersion = value; }
        }
        public int OriginalModerationStatus
        {
            get { return mOriginalModerationStatus; }
            set { mOriginalModerationStatus = value; }
        }
        private int mOldRowId;

        public int OldRowId
        {
            get { return mOldRowId; }
        }

        public bool IsNewCreatedDoc
        {
            get
            {
                return mBaseItemInfo.IsNewCreatedDoc;
            }
            set { mBaseItemInfo.IsNewCreatedDoc = value; }
        }
        public bool IsNewCreate
        {
            get
            {
                var listItemInfo = mBaseItemInfo as AveListItemInfo;
                if (listItemInfo != null)
                {
                    return listItemInfo.IsNewCreated;
                }
                var folderInfo = mBaseItemInfo as AveFolderInfo;
                if (folderInfo != null)
                {
                    return folderInfo.IsNewCreated;
                }
                return mBaseItemInfo.IsNewCreatedDoc;
            }
        }
        public bool IsCheckOut
        {
            get { return mBaseItemInfo.IsCheckOut; }
            set { mBaseItemInfo.IsCheckOut = value; }
        }
        public bool IsVersion
        {
            get { return mBaseItemInfo.IsVersion; }
            set { mBaseItemInfo.IsVersion = value; }
        }

        public string OwnerLoginName
        {
            get { return mAveItem.OwnerLoginName; }
        }

        private List<CacheMutiLookupValue> mutiLookupPostRestoreCache = new List<CacheMutiLookupValue>();

        //public AveSPItem(AveItemType type, AveSPFolder parentFolder, AveSqlConnection sqlConn, IAveRestoreStream receiver)
        //{
        //    mType = type;
        //    mParentFolder = parentFolder;
        //    mSqlConn = sqlConn;
        //    mReceiver = receiver;
        //    mSiteId = mParentFolder.ParentList.ParentWeb.ParentSite.SPSite.ID;
        //}

        public AveSPItem(AveSPList aveSPList, IAveRestoreStream aveRestoreStream)
        {
            mAveSPList = aveSPList;
            mReceiver = aveRestoreStream;
            try
            {
                if (mAveSPList.ParentWeb.SPWeb.RegionalSettings != null)
                {
                    mAveTimeZone = mAveSPList.ParentWeb.SPWeb.RegionalSettings.TimeZone;
                }
            }
            catch (Exception ex)
            {
                //RegionalSettings is not available in Contributes Permissions in BPOS
                ex.ToString();
            }
            mAveParentSite = aveSPList.ParentWeb.ParentSite;
        }

        /// <summary>
        /// 为folder 反差ct和column使用，只是构建一个list空壳
        /// </summary>
        /// <param name="parentFolder"></param>
        public AveSPItem(AveSPFolder parentFolder)
        {
            mAveSPList = parentFolder.ParentList;
            mAveParentSite = parentFolder.ParentList.ParentWeb.ParentSite;
        }

        public AveSPItem(AveBaseItemInfo info, AveItemType type, AveSPFolder parentFolder, IAveBackupRestoreQueryService queryService)
        {
            mIsStubData = false;
            mParentFolder = parentFolder;
            mQueryService = queryService;
            mAveSPList = parentFolder.ParentList;
            try
            {
                if (mAveSPList.ParentWeb.SPWeb.RegionalSettings != null)
                {
                    mAveTimeZone = mAveSPList.ParentWeb.SPWeb.RegionalSettings.TimeZone;
                }
            }
            catch (Exception ex)
            {
                //RegionalSettings is not available in Contributes Permissions in BPOS
                ex.ToString();
            }
            mAveParentSite = parentFolder.ParentList.ParentWeb.ParentSite;

            this.mBaseItemInfo = info;
            if (mAveSPList.SPList != null)
            {
                mBaseItemInfo.ListId = mAveSPList.SPList.ID;
            }
            mBaseItemInfo.SiteId = mParentFolder.ParentList.ParentWeb.ParentSite.SPSite.ID;
            mBaseItemInfo.ItemType = type;
            mBaseItemInfo.MappingManager = mParentFolder.ParentSite.MappingManager;
            //Here should find a way to get IsStubData property.
            //if (type == AveItemType.Document)
            //{
            //    if (mStorage == null)
            //    {
            //        mStorage =AveStorage.GetStorage(this);
            //    }
            //    info.IsStubData = mStorage.StorageInfo.IsBackupLinkForArchivedData;                
            //}

            mAveItem = parentFolder.ParentSite.ObjectModelFactory.CreateAveItem(mBaseItemInfo, parentFolder.SPFolder, mAveSPList.ParentWeb.SPWeb, mAveSPList.SPList);
            info.AveItem = mAveItem;
        }
        public AveSPItem(AveSPSite aveSite)
        {
            mQueryService = aveSite.QueryService;
            SiteId = aveSite.SPSite.ID;
            mAveParentSite = aveSite;
            mAveItem = mAveParentSite.ObjectModelFactory.CreateAveItem(mAveParentSite.SPSite);
        }

        public AveSPItem(AveItemType type, AveSPFolder parentFolder, string name)
        {
            //if (parentFolder.ParentList.ParentWeb.ReloadWebAndParentInternalForSPRequestTimeout(false))
            //{
            //    if (!parentFolder.ParentList.IsSystemList)
            //    {
            //        parentFolder.ParentList.ReloadList();
            //    }
            //}
            mAveParentSite = parentFolder.ParentSite;
            //mParentWeb = parentFolder.ParentWeb;
            mAveSPList = parentFolder.ParentList;
            mParentFolder = parentFolder;
            mQueryService = parentFolder.QueryService;
            try
            {
                if (mAveSPList.ParentWeb.SPWeb.RegionalSettings != null)
                {
                    mAveTimeZone = mAveSPList.ParentWeb.SPWeb.RegionalSettings.TimeZone;
                }
            }
            catch (Exception ex)
            {
                //RegionalSettings is not available in Contributes Permissions in BPOS
                log.Log(AveLogLevel.INFO, "cannot get regional setting TimeZone, exception:{0}", ex.ToString());
            }
            switch (type)
            {
                case AveItemType.Document:
                    mBaseItemInfo = new AveDocumentInfo();
                    break;
                case AveItemType.ListItem:
                    mBaseItemInfo = new AveListItemInfo();
                    break;
                case AveItemType.Folder:
                    //if (mAveSPList.SPList != null && mAveSPList.SPList.Title.Equals("NintexSnippets", StringComparison.OrdinalIgnoreCase) && mAveSPList.SPList.BaseTemplate == AveListTemplateType.NintexWrokflow)
                    //{
                    //    int userId = -1;
                    //    if (int.TryParse(name, out userId) && userId > 0)
                    //    {
                    //        var newUser = mAveParentSite.SPMembers.FindMember(userId, true, false, false);
                    //        if (newUser != null && newUser.ID > 0)
                    //        {
                    //            name = newUser.ID.ToString();
                    //        }
                    //    }
                    //}
                    mBaseItemInfo = new AveFolderInfo();
                    break;
                case AveItemType.Attachement:
                    mBaseItemInfo = new AveAttachmentInfo();
                    break;
                default:
                    mBaseItemInfo = new AveBaseItemInfo();
                    break;
            }
            mBaseItemInfo.Name = name;
            if (!string.IsNullOrEmpty(name))
            {
                int pos = name.IndexOf(':');
                if (pos >= 0)
                {
                    mBaseItemInfo.Name = name.Substring(0, pos);
                }
            }
            mBaseItemInfo.ItemType = type;
            InitBaseItemInfo();

            mAveItem = parentFolder.ParentSite.ObjectModelFactory.CreateAveItem(mBaseItemInfo, parentFolder.SPFolder, mAveSPList.ParentWeb.SPWeb, mAveSPList.SPList);
            mBaseItemInfo.AveItem = mAveItem;
        }
        private void InitBaseItemInfo()
        {
            if (mAveSPList.SPList != null)
            {
                mBaseItemInfo.ListId = mAveSPList.SPList.ID;
            }
            mBaseItemInfo.SiteId = mAveParentSite.SPSite.ID;
            mBaseItemInfo.ParentId = mParentFolder.Id;
            mBaseItemInfo.MappingManager = mAveParentSite.MappingManager;
            mBaseItemInfo.KeepDefaultValue = mAveParentSite.KeepDefaultValue;
            mBaseItemInfo.VerifyItemMMSColumnValue = mAveParentSite.VerifyItemMMSColumnValue;
        }

        public void SetStream(IAveRestoreStream stream)
        {
            mReceiver = stream;
        }

        /// <summary>
        /// Add Item Id Mapping
        /// </summary>
        /// <param name="rowId">source rowId</param>
        /// <param name="needReserveMappingInfo">True if you want the mapping info to be preserved, other the mapping will be cleared before the next list starts to restore</param>
        public void AddItemMapping(int rowId, bool needPreserveMappingInfo = false)
        {
            mOldRowId = rowId;
            if (mAveSPList.SPList != null)
            {
                lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ItemMappingLocker"))
                {
                    Dictionary<Guid, Dictionary<int, int>> idMapping = mAveParentSite.MappingManager.SiteMappingManager.ItemIdMapping;
                    Dictionary<Guid, Dictionary<int, int>> preservedIdMapping = mAveParentSite.MappingManager.SiteMappingManager.PreservedItemIdMapping;

                    if (!idMapping.ContainsKey(mParentFolder.ParentList.SPList.ID))
                    {
                        idMapping[mParentFolder.ParentList.SPList.ID] = new Dictionary<int, int>();
                    }
                    if (needPreserveMappingInfo && !preservedIdMapping.ContainsKey(mParentFolder.ParentList.SPList.ID))
                    {
                        preservedIdMapping[mParentFolder.ParentList.SPList.ID] = new Dictionary<int, int>();
                    }
                    if (mAveItem != null && mAveItem.ListItem != null)
                    {
                        idMapping[mParentFolder.ParentList.SPList.ID][rowId] = mAveItem.ListItem.ID;
                        if (needPreserveMappingInfo)
                        {
                            preservedIdMapping[mParentFolder.ParentList.SPList.ID][rowId] = mAveItem.ListItem.ID;
                        }
                    }
                }
            }
        }

       /* private IAveFile AddMHTDoc()
        {
            string defaultMHTContent = @"From: ""Saved by Microsoft Internet Explorer 7""
Subject: 
Date: Mon, 15 Jun 2009 10:02:18 +0800
MIME-Version: 1.0
Content-Type: text/html;
	charset=""utf-8""
Content-Transfer-Encoding: 7bit
Content-Location: http://localhost:3874/WebSite1/Default.aspx
X-MimeOLE: Produced By Microsoft MimeOLE V6.00.3790.4325

<!DOCTYPE HTML PUBLIC ""-//W3C//DTD HTML 4.0 Transitional//EN"">
<HTML><HEAD>
<META http-equiv=Content-Type content=""text/html; charset=utf-8"">
<META content=""MSHTML 6.00.3790.4426"" name=GENERATOR></HEAD>
<BODY></BODY></HTML>
";
            byte[] tempContent = Encoding.UTF8.GetBytes(defaultMHTContent);
            return mParentFolder.SPFolder.Files.Add(mBaseItemInfo.Name, tempContent, true);
        }*/

        /// <summary>
        /// added for renaming name and setuppath for ghosted page if user choosen language mapping
        /// </summary>
        internal void ProcessGhostPageNameAndPath(uint sLanguageId, uint dLanguageId, ref string name, ref string setupPath)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.ProcessGhostPageNameAndPath"))
            {
#endif
                try
                {
                    string sLanguageString = sLanguageId.ToString();
                    string dLanguageString = dLanguageId.ToString();
                    if (setupPath.StartsWith(sLanguageString, StringComparison.OrdinalIgnoreCase))
                    {
                        setupPath = dLanguageString + setupPath.Substring(sLanguageString.Length);
                    }
                    string sourceShortName = AveLanguageProcesser.CultureIdNameMapping[sLanguageId];
                    string destShortName = AveLanguageProcesser.CultureIdNameMapping[dLanguageId];

                    if (setupPath.Contains(sourceShortName))
                    {
                        setupPath = setupPath.Replace(sourceShortName, destShortName);
                    }
                    if (setupPath.Contains(sLanguageString))
                    {
                        setupPath = setupPath.Replace(sLanguageString, dLanguageString);
                    }
                    if (setupPath.Contains(dLanguageString))
                    {
                        name = name.Replace(dLanguageString, dLanguageString);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An Error occurred while setting ghost page name and setupPath,GhostPage name:{0},SetupPath:{1},Exception:{2}", name, setupPath, e.ToString());
                }
#if PerformanceLog
            }
#endif
        }


        public void PostAction()
        {
            //ResetParentListSetting();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "solution name")]
        public void ChangeWSPNameByNative()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.ChangeWSPNameByNative"))
            {
#endif
                if (!this.mAveParentSite.ObjectModelFactory.IsSPInstalled)
                {
                    return;
                }
                string extensionName = string.Empty;
                string originalName = string.Empty;
                try
                {
                    extensionName = mAveItem.ListItem.Name.EndsWith("AvePoint_wsp", StringComparison.OrdinalIgnoreCase) ? "AvePoint_wsp" : "AvePoint_stp";
                    originalName = mAveItem.ListItem.Name.Substring(0, mAveItem.ListItem.Name.LastIndexOf(extensionName, StringComparison.OrdinalIgnoreCase)) + '.' + extensionName.Substring(9);

                    QueryService.ChangeWSPNameByNative(originalName, mAveItem.ListItem.UniqueId, SiteId);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while change wsp file name. name:{0}\n error message:{1}", mBaseItemInfo.Name, e));
                    //mLog.Warn(e, "An error occurred while change wsp file name.name: '{0}'", mName);
                }
                try
                {
                    mBaseItemInfo.Name = originalName;
                    mAveItem.ListItem = this.mAveItem.ListItem.ParentList.GetItemById(mAveItem.ListItem.ID);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while change wsp file name. name:{0}\n error message:{1}", mBaseItemInfo.Name, e));
                    //mLog.Warn(e, "An error occurred while change wsp file name.name: '{0}'", mName);
                }
#if PerformanceLog
            }
#endif
        }

        public void AddFields(IAveListItem spListItem, Dictionary<string, object> fieldMap, AveBaseItemInfo info)
        {
            mAveItem.AddFields(spListItem, fieldMap, info);
        }

        public void AddFields(Dictionary<string, object> fieldMap)
        {
            AddFields(mAveItem.ListItem, fieldMap, mBaseItemInfo);
        }

        //public void ResetContentToFileShare()
        //{
        //    AveSPItemNativeInfo docInfo = new AveSPItemNativeInfo(mBaseItemInfo.SiteId, mParentFolder.ParentList.ParentWeb.SPWeb.ID, mBaseItemInfo.GUID, mBaseItemInfo.InternalVersion, mBaseItemInfo.Level, 0, mAveItem.File, null);
        //    mStorage.ConvertDBToFileSystem(docInfo);
        //}
        public void SetPicProperty(int width, int heigth)
        {
            mPicWidth = width;
            mPicHeight = heigth;
        }

        public int GetCurrentUIVersion(Guid siteId, IAveListItem item)
        {
            return mAveItem.GetCurrentUIVersion(siteId, item);
        }


        /// <summary>
        /// Return type:
        /// 0, No confiction (there is not document in alldocs table)
        /// 1, Confilict with RecycleBin
        /// 2, Confilict with current document
        /// 3, Confilict with both current document and RecycleBin
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="name"></param>
        /// <returns></returns>
        /*public int IsConflict(AveSqlConnection sqlConn, Guid siteId, Guid parentId, string name)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.IsConflict"))
            {
#endif
                //if (!AveEnvironment.IsSPInstalled) { return 0; } 
                sqlConn.ClearParameters();
                sqlConn.AddParameter("@SiteId", siteId);
                sqlConn.AddParameter("@ParentId", parentId);
                sqlConn.AddParameter("@LeafName", name);

                string cmdText = "SELECT DeleteTransactionId FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ParentId=@ParentId AND LeafName=@LeafName";
                int conflictType = 0;
                using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        conflictType |= 2;
                        //break;
                    }
                }

                cmdText = "SELECT DeleteTransactionId FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId<>0x AND ParentId=@ParentId AND LeafName=@LeafName";
                using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        conflictType |= 1;
                        //break;
                    }
                }

                return conflictType;
#if PerformanceLog
            }
#endif
        }*/

        /// <summary>
        /// get conflict type by tp_guid, only for a ListItem
        /// </summary>
        /// <param name="sqlConn"></param>
        /// <param name="siteId"></param>
        /// <param name="parentId"></param>
        /// <param name="tp_Guid"></param>
        /// <returns>Return type:
        /// 0, No conflict (there is not document in alldocs table)
        /// 1, conflict with RecycleBin
        /// 2, conflict with current item
        /// 3, conflict with both current item and RecycleBin</returns>
        /*public static int IsListItemConflict(AveSqlConnection sqlConn, Guid siteId, Guid parentId, Guid tp_Guid)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.IsListItemConflict"))
            {
#endif
                //if (!AveEnvironment.IsSPInstalled) { return 0; } 
                sqlConn.ClearParameters();
                sqlConn.AddParameter("@tp_SiteId", siteId);
                sqlConn.AddParameter("@tp_ParentId", parentId);
                sqlConn.AddParameter("@tp_Guid", tp_Guid);

                const string cmdText = @"SELECT Distinct tp_DeleteTransactionId from AllUserData 
                                        WHERE tp_SiteId=@tp_SiteId and tp_ParentId=@tp_ParentId 
                                        and tp_GUID=@tp_Guid and tp_IsCurrentVersion=1;";
                int conflictType = 0;
                using (SqlDataReader dr = sqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte[] transactionId = dr.GetSqlBinary(0).Value;
                        if (transactionId.Length > 0)
                        {
                            conflictType |= 1;
                        }
                        else
                        {
                            conflictType |= 2;
                        }
                    }
                }
                return conflictType;
#if PerformanceLog
            }
#endif
        }*/

        public void RestoreDataJunction(List<Dictionary<string, object>> junctionData)
        {
        }

        public Dictionary<string, object> GetDataJunction(List<Dictionary<string, object>> junctionData,bool getAveField=false)
        {
            Dictionary<string, object> multiLookupValues = new Dictionary<string, object>();
            if (junctionData == null)
            {
                return multiLookupValues;
            }
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.RestoreDataJunction"))
            {
#endif
                try
                {
                    int originalVersion = 0;
                    Hashtable fieldHt = new Hashtable();
                    Dictionary<int, string> lookupRealValue = new Dictionary<int, string>();
                    foreach (Dictionary<string, object> dic in junctionData)
                    {
                        if (originalVersion == 0)
                        {
                            originalVersion = (int)dic["tp_UIVersion"];
                        }
                        Guid fieldId = (Guid)dic["tp_FieldId"];
                        int id = (int)dic["tp_Id"];
                        if (!fieldHt.ContainsKey(fieldId))
                        {
                            fieldHt.Add(fieldId, new ArrayList());
                        }
                        ((ArrayList)fieldHt[fieldId]).Add(id);
                        object value;
                        if (dic.TryGetValue("tp_Value", out value))
                        {
                            lookupRealValue[id] = value.ToString();
                        }
                    }

                    //Ensure all field first.
                    //将EnsureField操作移至反插中，此处不再需要EnsureField
                    //foreach (DictionaryEntry de in fieldHt)
                    //{
                    //    try
                    //    {
                    //        lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("FieldLock"))
                    //        {
                    //            Guid fieldId = (Guid)de.Key;
                    //            mParentFolder.ParentList.AveFields.EnsureField(fieldId);
                    //        }
                    //    }
                    //    catch (AveSchemaDependencyConflictException ex)
                    //    {
                    //        log.Debug(ex.ToString());
                    //        //我们不会有冲突exception抛出，所以不会有这个异常，我们冲突就是skip或者overwrite
                    //    }
                    //    catch (AveSchemaDependencyNotFoundException ex)
                    //    {
                    //        log.Debug(ex.ToString());
                    //    }
                    //}
                    bool needUpdate = false;

                    foreach (DictionaryEntry de in fieldHt)
                    {
                        try
                        {
                            Guid fieldId = (Guid)de.Key;
                            Guid newValue = mParentFolder.ParentList.AveFields.FieldMapping.GetMappingRestoredFieldId(fieldId);
                            if (newValue != Guid.Empty)
                            {
                                fieldId = newValue;
                            }
                            ArrayList list = (ArrayList)de.Value;
                            IAveField field = mParentFolder.ParentList.SPList.Fields[fieldId];
                            if (!string.IsNullOrEmpty(mParentFolder.ParentList.AveFields.ExcelImportPath) && (mParentFolder.ParentList.AveFields.FieldMapping as AveFieldMapping).CustomMapping.GetMappingFieldBeforeAdd(new AveSourceFieldInfo() { SourceDisplayName = field.Title }) != null)
                            {
                                //对于导出的Excel中的column，其值用Excel中的值来还原
                                continue;
                            }
                            if (field is IAveFieldUser)
                            {
                                IAveFieldUserValueCollection userValueCol = null;
                                foreach (int userId in list)
                                {
                                    string itemName = string.Empty;
                                    if (SPFile != null && SPFile.Name != null)
                                    {
                                        itemName = SPFile.Name;
                                    }
                                    IAvePrincipal principal = mParentFolder.ParentList.AveFields.GetCustomMappingValueForDataJunction(itemName, userId, field.InternalName, field);
                                    if (principal == null)
                                    {
                                        log.Log(AveLogLevel.WARN, string.Format("Can't find principal when updating the Data Junction Info, principal id: {0}, field name: {1}.", userId, field.Title));
                                        //mLog.Warn("Can't find principal when updating the Data Junction Info, principal id: {0}, field name: {1}.", userId, field.Title);
                                        continue;
                                    }
                                    if (userValueCol == null)
                                    {
                                        userValueCol = mAveParentSite.ObjectModelFactory.CreateFieldUserValueCollection();
                                    }
                                    userValueCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldUserValue(mParentFolder.ParentList.ParentWeb.SPWeb, principal.ID, principal.Name));
                                }
                                if (userValueCol != null)
                                {
                                    if (originalVersion < Version || Level == 255)
                                    {
                                        List<int> values = new List<int>();
                                        foreach (IAveFieldUserValue userValue in userValueCol)
                                        {
                                            values.Add(userValue.LookupId);
                                        }
                                        Guid sourceListId = new Guid(((IAveFieldUser)field).LookupList);
                                        CreateDatajunctionByNative(mAveItem.ListItem, fieldId, sourceListId, originalVersion, values);
                                    }
                                    else
                                    {
                                        if (getAveField)
                                        {
                                           var value= new AveFieldValueInfo
                                           {
                                               ColValue = userValueCol.ToString(),
                                               ColName = field.ColName,
                                               FieldType = field.Type,
                                               RowOrdinal = field.RowOrdinal,
                                               Id = field.ID
                                           };
                                            multiLookupValues[field.InternalName] = value;
                                        }
                                        else
                                        {
                                            multiLookupValues[field.InternalName] = userValueCol.ToString();
                                        }
                                        needUpdate = true;
                                    }
                                }
                            }
                            else if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
                            {
                                //mParentFolder.ParentList.ParentWeb.ParentSite.AddNotUpdateLookupFieldValue(mParentFolder.ParentList.ParentWeb.SPWeb.ID, mParentFolder.ParentList.SPList.ID, mRowId, Version, fieldId, list);
                            }
                            else if (field is IAveFieldLookup)
                            {
                                IAveFieldLookupValueCollection lookupCol = null;
                                bool allFind = true;
                                if (mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.LookupFieldCache.ContainsKey(mParentFolder.ParentList.SPList.ID)
                                    && mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.LookupFieldCache[mParentFolder.ParentList.SPList.ID].ContainsKey(fieldId))
                                {
                                    AveLookupObject obj = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.LookupFieldCache[mParentFolder.ParentList.SPList.ID][fieldId];
                                    try
                                    {
                                        Guid oldListId = new Guid(obj.List);
                                        if (mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping.ContainsKey(oldListId))
                                        {
                                            foreach (int itemId in list)
                                            {
                                                Guid lookupListID = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping[oldListId];
                                                //对于replicator的increment job,有可能还原了list,但是不需要还原它下面的item
                                                if (!mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(lookupListID))
                                                {
                                                    allFind = false;
                                                    break;
                                                }
                                                Dictionary<int, int> listItemDic = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[lookupListID];
                                                if (listItemDic.ContainsKey(itemId))
                                                {
                                                    if (lookupCol == null)
                                                    {
                                                        lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                                                    }
                                                    lookupCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(listItemDic[itemId], "Title"));
                                                    continue;
                                                }
                                                else
                                                {
                                                    allFind = false;
                                                    break;
                                                }
                                            }
                                        }
                                        else
                                        {
                                            allFind = false;
                                        }
                                        if (!allFind && obj != null)
                                        {
                                            //在ID Mapping 找不到的情况下，尝试使用 column value 进行匹配 ，若匹配则返回得到的item ID 
                                            IAveFieldLookup avelookupField = field as IAveFieldLookup;
                                        if (avelookupField != null && avelookupField.InternalName.Equals("TaxCatchAll"))
                                        {
                                            //SAAS-41072 Skip to restore TaxCatchAll
                                            continue;
                                        }
                                        
                                        try
                                            {
                                                allFind = true;
                                                string internalName = avelookupField.LookupField;
                                                Guid webID = avelookupField.LookupWebId;
                                                Guid objList = new Guid(obj.List);
                                                IAveList lookupList;
                                                Dictionary<string, int> lookupIDValue;
                                                if (!mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.TryGetLookupListValueMapping(objList, internalName, out lookupIDValue))
                                                {
                                                    lookupIDValue = new Dictionary<string, int>();
                                                    if (!mAveSPList.IsLookupListValid(string.Concat(webID, "-", obj.ListTitle)) && !mAveSPList.IsLookupListValid(string.Concat(webID, "-", obj.List, "-", internalName)))
                                                    {
                                                        if (webID == Guid.Empty)
                                                        {
                                                            lookupList = mParentFolder.ParentList.ParentWeb.ParentSite.SPSite.OpenWeb(mParentFolder.ParentList.ParentWeb.OldId).GetListByTitle(obj.ListTitle);
                                                        }
                                                        else
                                                        {
                                                            lookupList = mParentFolder.ParentList.ParentWeb.ParentSite.SPSite.OpenWeb(webID).GetListByTitle(obj.ListTitle);
                                                        }
                                                    if (lookupList != null)
                                                    {
                                                        // End User or item level Restore don't query lookup list.
                                                        if (WrapperConfiguration.WrapperConfigurationForBPOS.IsEndUserRestore
                                                            || WrapperConfiguration.WrapperConfigurationForBPOS.HasItemLevelNode
                                                            || WrapperConfiguration.WrapperConfigurationForBPOS.SkipCacheLookColumn)
                                                        {
                                                            foreach (int itemId in list)
                                                            {
                                                                var item = lookupList.GetItemById(itemId);
                                                                if (item != null && item[internalName] != null)
                                                                {
                                                                    var fieldvalue = item[internalName].ToString();
                                                                    lookupIDValue[fieldvalue] = item.ID;
                                                                    log.Info($"[RECENTER-2151]Success to find this field:{internalName} by get item id in target lookup list:{obj.ListTitle}, field value:{fieldvalue}, item id:{item.ID}");
                                                                }
                                                                else
                                                                {
                                                                    log.Info($"Failed to find this field:{internalName} by get item id in target lookup list:{obj.ListTitle}, item id:{item.ID}");
                                                                }
                                                            }
                                                        }
                                                        else
                                                        {
                                                            mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddListIdMapping(objList, lookupList.ID);
                                                            try
                                                            {
                                                                foreach (IAveListItem item in lookupList.GetItemsLightly(internalName))
                                                                {
                                                                    object itemValue = item[internalName];
                                                                    if (itemValue != null)
                                                                    {
                                                                        lookupIDValue[itemValue.ToString()] = item.ID;
                                                                        log.Info("get column value in this item, item: {0} column: {1} --> value:{2} ", item.Title, internalName, itemValue);
                                                                    }
                                                                }
                                                            }
                                                            catch
                                                            {
                                                                //In case the field[InternalName] not exist in lookup list
                                                                mAveSPList.AddInvalidLookupListTitle(string.Concat(webID, "-", obj.List, "-", internalName));
                                                                throw;
                                                            }
                                                            mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddLookupListValueMapping(objList, internalName, lookupIDValue);
                                                        }
                                                    }
                                                    else
                                                    {
                                                        mAveSPList.AddInvalidLookupListTitle(string.Concat(webID, "-", obj.ListTitle));
                                                    }
                                                    }
                                                    else
                                                    {
                                                        log.Warn("Failed to find the list by title:{0} , when restore the lookup column: {1} or cannot find the column in the list", obj.ListTitle, avelookupField.InternalName);
                                                    }
                                                }
                                                foreach (int itemId in list)
                                                {
                                                    int newID;
                                                    if (lookupRealValue.ContainsKey(itemId) && lookupIDValue.TryGetValue(lookupRealValue[itemId], out newID))
                                                    {
                                                        if (lookupCol == null)
                                                        {
                                                            lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                                                        }
                                                        lookupCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(newID, "Title"));
                                                        Guid lookupListID = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping[new Guid(obj.List)];
                                                        mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddItemIdMapping(lookupListID, itemId, newID);
                                                    }
                                                    else
                                                    {
                                                        allFind = false;
                                                    }
                                                }
                                            }
                                            catch (Exception ex)
                                            {
                                                log.Warn("Lookup Column: {0}. Lookup List : {1}. MultiValue Mapping failed : {2} ", avelookupField.InternalName, obj.List, ex);
                                                allFind = false;
                                            }
                                        }

                                    }
                                    catch (AveSecurityTrimingException)
                                    {
                                        throw;
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore dataJunction. name:{0}\n error message:{1}", de.Key, e));
                                        //mLog.Warn(e, "An error occurred while restore datajunction. Name:{0}", de.Key);
                                    }
                                }
                                else
                                {
                                    Guid listId = new Guid(((IAveFieldLookup)field).LookupList);
                                    if (!listId.Equals(Guid.Empty) && list.Count != 0)
                                    {
                                        foreach (int itemId in list)
                                        {
                                            int destItemId = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.GetMappingItemId(listId, itemId);
                                            //如果lista在listb之前还原,就有可能找到正确的lookup关系
                                            if (destItemId != -1)
                                            {
                                                //value = itemId.ToString();
                                                //newdata[spField.InternalName] = value;
                                                if (lookupCol == null)
                                                {
                                                    lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                                                }
                                                lookupCol.Add(mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(destItemId, "Title"));
                                            }
                                            //如果没有找到正确的对应关系,将将其加入到PostAction中,需要注意此时lookupID传的是listId,而不是obj.List,所以在PostAction中还原的时候需要稍加处理
                                            else
                                            {
                                                if (!String.IsNullOrEmpty(itemId.ToString()))
                                                {
                                                    ArrayList list1 = new ArrayList();
                                                    list1.Add(Convert.ToInt32(itemId));
                                                    mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNotUpdateLookupFieldValue(listId, mParentFolder.ParentList.ParentWeb.SPWeb.ID, mParentFolder.ParentList.SPList.ID, RowId, Version, fieldId, list);
                                                }
                                            }
                                        }
                                    }
                                }
                                if (allFind && lookupCol != null)
                                {
                                    if (originalVersion < Version || Level == 255)
                                    {
                                        List<int> values = new List<int>();
                                        foreach (IAveFieldLookupValue lookupValue in lookupCol)
                                        {
                                            values.Add(lookupValue.LookupId);
                                        }
                                        Guid sourceListId = ParentFolder.ParentList.SPList.ID;
                                        if (Level == 255)
                                        {
                                            RemoveDatajunctionByNative(mQueryService, mAveItem.ListItem, fieldId, sourceListId, originalVersion);
                                        }
                                        CreateDatajunctionByNative(mAveItem.ListItem, fieldId, sourceListId, originalVersion, values);
                                    }
                                    else
                                    {
                                        if (getAveField)
                                        {
                                            var value = new AveFieldValueInfo
                                            {
                                                ColValue = lookupCol.ToString(),
                                                ColName = field.ColName,
                                                FieldType = field.Type,
                                                RowOrdinal = field.RowOrdinal,
                                                Id = field.ID
                                            };
                                            multiLookupValues[field.InternalName] = value;
                                        }
                                        else
                                        {
                                            multiLookupValues[field.InternalName] = lookupCol.ToString();
                                        }
                                        needUpdate = true;
                                    }
                                }
                                else if (!allFind)
                                {
                                    AveLookupObject obj = mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.LookupFieldCache[mParentFolder.ParentList.SPList.ID][fieldId];
                                    //item还没有还原row id为空，这个时候加入，post action不会处理，先cache一下，等到item还原结束再add
                                    CacheMutiLookupValue value = new Restore.CacheMutiLookupValue() { Obj = obj, OriVersion = originalVersion, fieldId = fieldId, List = list };
                                    //mParentFolder.ParentList.ParentWeb.ParentSite.AddNotUpdateLookupFieldValue(mParentFolder.ParentList.ParentWeb.SPWeb.ID, mParentFolder.ParentList.SPList.ID, mRowId, originalVersion, fieldId, list);
                                    mutiLookupPostRestoreCache.Add(value);
                                    //mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNotUpdateLookupFieldValue(new Guid(obj.List), mParentFolder.ParentList.ParentWeb.SPWeb.ID, mParentFolder.ParentList.SPList.ID, mBaseItemInfo.RowId, originalVersion, fieldId, list);
                                }
                            }
                        }
                        catch (AveSecurityTrimingException)
                        {
                            throw;
                        }
                        catch (Exception e)
                        {
                            log.Log(AveLogLevel.ERROR, string.Format("An error occurred while restore dataJunction. name:{0}\n error message:{1}", de.Key, e));
                        }
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while restore dataJunction.\n error message:{0}", ex));
                    report.AddDetail(new AveWrapperReportDto("", "", AveReportObjectType.DataJunctions, AveStatus.Skipped, "You don't have permission to restore data junctions. " + ex.Message));
                }
                return multiLookupValues;
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// cache lookup after restore listitem
        /// </summary>
        public void CacheMutiLookupValue()
        {
            foreach (CacheMutiLookupValue objValue in mutiLookupPostRestoreCache)
            {
                mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.AddNotUpdateLookupFieldValue(new Guid(objValue.Obj.List), mParentFolder.ParentList.ParentWeb.SPWeb.ID, mParentFolder.ParentList.SPList.ID, mBaseItemInfo.RowId, objValue.OriVersion, objValue.fieldId, objValue.List);
            }
            mutiLookupPostRestoreCache.Clear();
        }

        public bool CreateVersionByNative(int version, RestoringDto restoringDto)
        {
            if (!mAveParentSite.ObjectModelFactory.IsSPInstalled) { return false; }
            return mAveItem.CreateVersionByNative(mBaseItemInfo, version, restoringDto);

        }

        public static void RemoveDatajunctionByNative(IAveBackupRestoreQueryService queryService, IAveListItem item, Guid fieldId, Guid sourceListId, int version)
        {
            queryService.RemoveDatajunctionByNative(item, fieldId, sourceListId, version);
        }
        public void CreateDatajunctionByNative(IAveListItem item, Guid fieldId, Guid sourceListId, int version, List<int> values)
        {
            int ordinal = 0;
            foreach (int value in values)
            {
                InsertIntoAllUserDatajunction(item, fieldId, sourceListId, value, ordinal, version);
                ordinal++;
            }
        }

        public void UpdateColumnByNative(Guid siteId, IAveListItem item, int version, int rowOrdinal, string colName, object colValue)
        {
            mAveItem.UpdateColumnByNative(siteId, item, version, rowOrdinal, colName, colValue);
        }

        public void InsertIntoAllUserDatajunction(IAveListItem item, Guid fieldId, Guid sourceListId, int id, int ordinal, int version)
        {
            mAveItem.InsertIntoAllUserDatajunction(item, fieldId, sourceListId, id, ordinal, version);
        }

        /*private IAveFile ReloadFile(Guid fileID)
        {
            return ParentFolder.ParentList.SPList.ParentWeb.GetFile(fileID);
        }*/

        //Reload SPRequest SystemUpdate.
        public static void SystemUpdate(IAveListItem item)
        {
            //释放SPWeb对象的SPRequest对象
            //item.ParentList.ParentWeb.InvalidateRequest();
            ////调用SPWeb的InitializeSPRequest会重新获取SPRequest对象
            //item.ParentList.ParentWeb.InitializeSPRequest();
            item.SystemUpdate();
        }

        /*public Nullable<bool> CurrentIsEBS(IAveFile file)
        {
            file = ReloadFile(file.UniqueId);
            if (ParentFolder.ParentList != null && ParentFolder.ParentList.SPList != null && ParentFolder.ParentList.SPList.ID != Guid.Empty)
            {
                AveAssemblyUtility.SetFieldValue(file, "m_Item", this.ParentFolder.ParentList.SPList.GetItemById(file.Item.ID));
            }
        }

      

       

        #region For BPOS
        public void RestoreItemProperty(AveItemFieldCollectionInfo fieldCollection, IAveList list, IAveListItem item)
        {
            RestoreItemProperty(fieldCollection, list, item, false);
        }
        public void RestoreItemProperty(AveItemFieldCollectionInfo fieldCollection, IAveList list, IAveListItem item, bool overwriteVersion)
        {
            ConvertNameToMappedName(fieldCollection);

            string staticName = string.Empty;
            foreach (AveItemFieldInfo itemField in fieldCollection.ItemFields)
            {
                if (string.IsNullOrEmpty(itemField.StaticName))
                {
                    IAveField field = list.Fields.GetByInfo(itemField.DisplayName, itemField.Type);
                    if (field == null)
                    {
                        continue;
                    }
                    staticName = field.StaticName;
                }
                else
                {
                    staticName = itemField.StaticName;
                }
                if (!string.IsNullOrEmpty(itemField.Type) && itemField.Type.StartsWith(AveFieldType.User.ToString(), StringComparison.OrdinalIgnoreCase))
                {
                    string userValue = GetUserContent(list, itemField.Value);
                    if (!string.IsNullOrEmpty(userValue))
                    {
                        item[staticName] = userValue;
                    }
                }
                else if (itemField.Type == "DateTime")
                {
                    if (mAveTimeZone != null)
                    {
                        item[staticName] = mAveTimeZone.UTCToLocalTime((DateTime)itemField.Value);
                    }
                }
                else
                {
                    item[staticName] = itemField.Value;
                }
            }
            if (overwriteVersion)
            {
                item.UpdateOverwriteVersion();
            }
            else
            {
                item.Update();
            }
        }
        internal bool TryGetItem(AveItemFieldCollectionInfo fieldColInfo, IAveListItem aveItem)
        {
            IAveListItemCollection listItems = mAveSPList.SPList.Items;
            foreach (IAveListItem listItem in listItems)
            {
                if (string.Equals(listItem[mRestoreConfig.CheckUniqueField], fieldColInfo.UniqueId))
                {
                    aveItem = listItem;
                    return true;
                }
            }
            aveItem = null;
            return false;
        }
        private void ConvertNameToMappedName(AveItemFieldCollectionInfo fieldInfoCol)
        {
            //Dictionary<string, string> fieldNameMapping = mAveSPList.AveFields.FieldDisplayNameMapping;

            foreach (AveItemFieldInfo fieldInfo in fieldInfoCol.ItemFields)
            {
                if (fieldInfo.DisplayName != null)
                {
                    string mappedFieldName = mAveSPList.AveFields.FieldMapping.GetMappingRestoredFieldDisplayName(fieldInfo.DisplayName + fieldInfo.Type);
                    //if (fieldNameMapping.TryGetValue(fieldInfo.DisplayName + fieldInfo.Type, out mappedFieldName))
                    if (String.IsNullOrEmpty(mappedFieldName))
                    {
                        fieldInfo.DisplayName = mappedFieldName;
                    }
                }
            }
        }
        /*internal virtual bool ShouldRestoreItem(AveItemFieldCollectionInfo fieldColInfo)
        {
            if (TryGetItem(fieldColInfo, mAveItem.ListItem))
            {
                AveRestoreMode restoreMode = RestoreOption.mAveRestoreMode;

                if (restoreMode == AveRestoreMode.OverWrite || restoreMode == AveRestoreMode.Append)
                {
                    return true;
                }
                else if (restoreMode == AveRestoreMode.OverWriteByModifiedTime)
                {
                    AveItemFieldInfo fieldInfo = fieldColInfo.GetUniqueItemFieldInfoByDisplayName("ModifyDate");
                    if ((DateTime)fieldInfo.Value > (DateTime)mAveItem.ListItem["ModifyDate"])
                    {
                        return true;
                    }
                    return false;
                }
                return false;
            }
            else
            {
                return true;
            }
        }*/
        



        //DOC-70322 for replicator,用于replicator的increment job能够正确还原lookup field的value.
        public void RestoreLookupFieldGuidValue(Dictionary<string, string> lookupFieldGuidValue)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.RestoreLookupFieldGuidValue"))
            {
#endif
                IAveList list = mParentFolder.ParentList.SPList;
                if (list == null)
                {
                    return;
                }
                bool needUpdateItem = false;
                try
                {
                    foreach (KeyValuePair<string, string> pair in lookupFieldGuidValue)
                    {
                        string name = pair.Key;
                        string mappingName = mParentFolder.ParentList.AveFields.FieldMapping.GetMappingRestoredFieldInternalName(name);
                        if (!String.IsNullOrEmpty(mappingName))
                        {
                            name = mappingName;
                        }
                        IAveField field = list.Fields.GetFieldByInternalName(name);
                        if (field.Type != AveFieldType.Lookup)
                        {
                            log.Info("This field is not lookupField. field name:{0}, field displayName:{1}.", field.InternalName, field.Title);
                            continue;
                        }
                        if (field.InternalName.Equals("TaxCatchAll"))
                        {
                            continue;
                        }
                        IAveFieldLookup lookupField = field as IAveFieldLookup;
                        Guid lookupListId = new Guid(lookupField.LookupList);
                        string value = pair.Value;
                        if (!lookupField.AllowMultipleValues && value.IndexOf(';') < 0)
                        {
                            int sourceRowId = Int32.Parse(value.ToString().Substring(0, value.ToString().IndexOf('#')));
                            Guid guid = new Guid(value.ToString().Substring(value.ToString().IndexOf('#') + 1));
                            int rowId = GetLookupIdByGUID(lookupListId, guid);
                            if (rowId > 0)
                            {
                                SPListItem[name] = rowId;
                                if (!mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(lookupListId))
                                {
                                    mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[lookupListId] = new Dictionary<int, int>();
                                }
                                mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[lookupListId][sourceRowId] = rowId;
                                needUpdateItem = true;
                            }
                        }
                        else if (lookupField.AllowMultipleValues)
                        {
                            string[] values = value.Split(';');
                            IAveFieldLookupValueCollection lookupCol = null;
                            foreach (string temp in values)
                            {
                                if (string.IsNullOrEmpty(temp))
                                {
                                    continue;
                                }
                                int sourceRowId = Int32.Parse(temp.ToString().Substring(0, temp.ToString().IndexOf('#')));
                                Guid guid = new Guid(temp.ToString().Substring(temp.ToString().IndexOf('#') + 1));
                                int rowId = GetLookupIdByGUID(lookupListId, guid);
                                if (rowId > 0)
                                {
                                    if (lookupCol == null)
                                    {
                                        lookupCol = mAveParentSite.ObjectModelFactory.CreateFieldLookupValueCollection();
                                    }
                                    IAveFieldLookupValue lookupValue = mAveParentSite.ObjectModelFactory.CreateFieldLookupValue(rowId, "Title");
                                    lookupValue.LookupId = rowId;
                                    lookupCol.Add(lookupValue);
                                    if (!mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping.ContainsKey(lookupListId))
                                    {
                                        mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[lookupListId] = new Dictionary<int, int>();
                                    }
                                    mParentFolder.ParentList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping[lookupListId][sourceRowId] = rowId;
                                }
                            }
                            if (lookupCol != null)
                            {
                                SPListItem[name] = lookupCol;
                                needUpdateItem = true;
                            }
                        }
                    }
                    if (needUpdateItem)
                    {
                        SystemUpdate(SPListItem);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("An error occurred while RestoreLookupFieldGuidValue. error:{0}.", e.ToString());
                }

#if PerformanceLog
            }
#endif
        }

        public int GetLookupIdByGUID(Guid lookupListId, Guid GUID)
        {
            IAveList lookupList = this.mAveSPList.ParentWeb.SPWeb.Lists.GetById(lookupListId);
            AveCamlQuery query = new AveCamlQuery();
            query.ViewXml = "<View Scope='RecursiveAll'><Query><Where><Eq><FieldRef Name=\"GUID\"></FieldRef><Value Type=\"Guid\">" + GUID.ToString("B") + "</Value></Eq></Where></Query></View>";
            IAveListItemCollection items = lookupList.GetItems(query);
            if (items != null && items.Count == 1)
            {
                return items[0].ID;
            }
            return -1;
        }

        public void InitFieldsInMetaInfo(Dictionary<string, string> metaInfoDic)
        {
            if (metaInfoDic != null)
            {
                fieldsInMetaInfo = ParentFolder.ParentList.AveFields.GetFieldValuesInMetaInfo(-1, mBaseItemInfo.Version, metaInfoDic, mAveSPList.ParentWeb.SPWeb.ID, mBaseItemInfo.ListId);
                mBaseItemInfo.FieldsInfo.FieldsInMetaInfo = fieldsInMetaInfo;
            }
        }

        public void InitBySPListItem(IAveListItem listItem)
        {
            mAveItem.InitBySPListItem(listItem);
        }

        internal void UpdateFields(Dictionary<string, object> fieldData, AveBaseItemInfo info)
        {
            mAveItem.UpdateFields(fieldData, info);
        }

        public IAveFile LoadCheckOutFile(IAveWeb mSPWeb, Guid fileId, IAveUser iAveUser)
        {
            return mAveItem.LoadCheckOutFile(mSPWeb, fileId, iAveUser);
        }

        internal IAveFile GetFile(string name)
        {
            return mAveItem.GetFile(name);
        }

        //这个方法是为了更新Fieldmapping中关于TaxonomyField的信息，同时得到info.FieldsInfo.TermIdMapping为还原这种类型的值做准备。
        internal void GetTaxonomyTermIdMapping(Dictionary<string, object> fieldMapping, AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.GetTaxonomyTermIdMapping"))
            {
#endif
                Dictionary<Guid, Guid> termIdMapping = null;
                Dictionary<string, string> dic = new Dictionary<string, string>();
                foreach (string taxonomyField in mParentFolder.ParentList.TaxonomyFields)
                {
                    if (fieldMapping.ContainsKey(taxonomyField))
                    {
                        AveFieldValueInfo fieldValue = fieldMapping[taxonomyField] as AveFieldValueInfo;
                        if (fieldValue.ColValue is string)
                        {
                            dic.Add(taxonomyField, fieldValue.ColValue.ToString());
                        }
                        else
                        {
                            log.Info("This Taxonomy Field value Type is not string. value:{0}", fieldMapping[taxonomyField].ToString());
                        }
                        fieldMapping.Remove(taxonomyField);
                    }
                    //if(fieldMapping.ContainsKey(taxonomyField.Key))
                    //{
                    //    //做text field to taxonomy field的时候，源端的值会是string,该值在GetFieldValues时候处理成termName1;termName2形式
                    //    if(fieldMapping[taxonomyField.Key] is string)
                    //    {
                    //        dic.Add(taxonomyField.Key, fieldMapping[taxonomyField.Key].ToString());
                    //    }
                    //    fieldMapping.Remove(taxonomyField.Key);
                    //}
                    //else if(fieldMapping.ContainsKey(taxonomyField.Value))
                    //{
                    //    //taxonomy field的terms 存储在其对应的TextField上。格式如：term1Name|term1Id;term2Name|term2Id
                    //    if (fieldMapping[taxonomyField.Value] is string)
                    //    {
                    //        string value = string.Empty;
                    //        string[] tValues = fieldMapping[taxonomyField.Value].ToString().Split(';');
                    //        for (int i = 0; i < tValues.Length; i++)
                    //        {
                    //            string[] temp = tValues[i].Split('|');
                    //            if (temp.Length == 2)
                    //            {
                    //                value += temp[0] + ";";
                    //            }
                    //        }
                    //        value = value.Trim(';');
                    //        dic.Add(taxonomyField.Key, value);
                    //        fieldMapping.Remove(taxonomyField.Value);
                    //    }
                    //}
                }
                //Content Organizer Rules中的item，设置对metadata column的condition，需要替换RoutingConditions属性值中的termId，需要将TermIdMapping设置上
                //if (dic.Count > 0)
                {
                    info.FieldsInfo.TaxonomyFieldsInMapping = dic;
                    if (mParentFolder.ParentList.ParentWeb.ParentSite.MetadataService != null)
                    {
                        info.FieldsInfo.TermIdMapping = mParentFolder.ParentList.ParentWeb.ParentSite.MetadataService.TermIdMapping;
                    }
#if DEBUG
                    if (dic.Count > 0)
                    {
                        StringBuilder infos = new StringBuilder();
                        infos.AppendLine("Item Source TaxonomyField Value:");
                        foreach (KeyValuePair<string, string> pair in dic)
                        {
                            infos.AppendLine(pair.Key + " : " + pair.Value);
                        }
                        if (termIdMapping != null)
                        {
                            infos.AppendLine("termIdMapping count : " + termIdMapping.Count.ToString());
                        }
                        log.Debug(info.ToString());
                    }
#endif
                }
#if PerformanceLog
            }
#endif
        }

        public void ProcessPreCondition(Dictionary<string, object> data, Dictionary<string, object> userData)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.ProcessPreCondition"))
            {
#endif
                mBaseItemInfo.IsVersion = data.ContainsKey("IsUserDocVersion") ? (bool)data["IsUserDocVersion"] : false;
                mBaseItemInfo.HasStream = data.ContainsKey("HasStream") ? (Convert.ToInt32(data["HasStream"])) == 1 : false;
                mBaseItemInfo.OriginalVersion = data.ContainsKey("UIVersion") ? Convert.ToInt32(data["UIVersion"]) : -1;
                mBaseItemInfo.OriginalLevel = data.ContainsKey("Level") ? Convert.ToByte(data["Level"]) : (byte)0;
                mBaseItemInfo.DTimeCreated = data.ContainsKey("TimeCreated") ? (DateTime)data["TimeCreated"] : DateTime.MinValue;
                if (data.ContainsKey("TimeLastModified") && data["TimeLastModified"] != null)
                {
                    mBaseItemInfo.DTimeLastModified = (DateTime)data["TimeLastModified"];
                }
                else if (data.ContainsKey("BiggestVersionModified") && data["BiggestVersionModified"] != null)
                {
                    mBaseItemInfo.DTimeLastModified = (DateTime)data["BiggestVersionModified"];
                }
                else
                {
                    mBaseItemInfo.DTimeLastModified = DateTime.MinValue;
                }
                mBaseItemInfo.DraftOwnerId = data.ContainsKey("DraftOwnerId") ? Convert.ToInt32(data["DraftOwnerId"]) : -1;
                if (!mAveParentSite.ObjectModelFactory.ContextKind.Equals(AveContextKind.ClientObjectModel))
                {
                    mBaseItemInfo.GUID = data.ContainsKey("Id") ? new Guid(data["Id"].ToString()) : Guid.Empty;
                }
                else
                {
                    mBaseItemInfo.GUID = Guid.Empty;
                }
                if (mBaseItemInfo.DraftOwnerId > 0)
                {
                    mBaseItemInfo.DraftOwnerId = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMemberId(mBaseItemInfo.DraftOwnerId);
                }
                mBaseItemInfo.ModerationStatus = 0;
                if (userData.ContainsKey("#tp_ModerationStatus"))
                {
                    mBaseItemInfo.ModerationStatus = Convert.ToInt32(userData["#tp_ModerationStatus"]);
                }
                mBaseItemInfo.ModerationComments = string.Empty;
                if (userData.ContainsKey("_ModerationComments"))
                {
                    mBaseItemInfo.ModerationComments = (string)userData["_ModerationComments"];
                }
                mBaseItemInfo.CheckoutUserId = -1;
                if (data.ContainsKey("CheckoutUserId"))
                {
                    mBaseItemInfo.CheckoutUserId = Convert.ToInt32(data["CheckoutUserId"]);
                    mBaseItemInfo.CheckoutUserId = mAveSPList.ParentWeb.ParentSite.SPMembers.FindMemberId(mBaseItemInfo.CheckoutUserId);
                }
                mBaseItemInfo.OriginalRowId = -1;
                if (data.ContainsKey("DoclibRowId"))
                {
                    mBaseItemInfo.OriginalRowId = Convert.ToInt32(data["DoclibRowId"]);
                }
                mBaseItemInfo.SettingInfo.KEEP_ITEM_TPGUID = RestoreOption.mAveItemRestoreOption.KEEP_ITEM_TPGUID;
                mBaseItemInfo.SettingInfo.MOVE_ITEM_TO_CONFLICT_FOLDER = RestoreOption.mAveItemRestoreOption.MOVE_ITEM_TO_CONFLICT_FOLDER;
                mBaseItemInfo.SettingInfo.DESTSTUB_CONTENT = RestoreOption.mAveStorgeOption.DESTSTUB_CONTENT;
                mBaseItemInfo.SettingInfo.NewItemWithOutVerifyConflict = RestoreOption.mAveItemRestoreOption.NewItemWithOutVerifyConflict;
                mBaseItemInfo.SettingInfo.IncreaceVerionWithRowId = RestoreOption.mAveItemRestoreOption.IncreaceVerionWithRowId;
                //mBaseItemInfo.MappingInfo.ListLevelCTMapping = mAveSPList.ParentSite.MappingManager.ListMappingManager.ListLevelCTMapping;
                //mBaseItemInfo.MappingInfo.SiteAbsoluteUrlMapping = mAveSPList.ParentSite.MappingManager.SiteMappingManager.AbsoluteUrlMapping;
                //mBaseItemInfo.MappingInfo.SiteManagedMappings = mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.SiteManagedMappings;
                //mBaseItemInfo.MappingInfo.ItemIdMapping = mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ItemIdMapping;
                //mBaseItemInfo.MappingInfo.ListIdMapping = mAveSPList.ParentWeb.ParentSite.MappingManager.SiteMappingManager.ListIdMapping;
                //the following fields are prepared for document restore
                mBaseItemInfo.ParentWebRelativeUrl = this.ParentList.ParentWeb.SPWeb.ServerRelativeUrl;
                mBaseItemInfo.ParentFolderRelativeUrl = this.ParentFolder.SPFolder.ServerRelativeUrl;
                if (this.ParentList.SPList != null)//System Folder
                {
                    mBaseItemInfo.ParentListTitle = this.ParentList.SPList.Title;
                    mBaseItemInfo.ParentListId = this.ParentList.SPList.ID;
                }
                else if (mBaseItemInfo.ParentFolderRelativeUrl == null) //SAAS-21933,没有parentlist,说明是web system folder
                {
                    mBaseItemInfo.ParentFolderRelativeUrl = this.ParentList.ParentWeb.SPWeb.ServerRelativeUrl;
                }
                mBaseItemInfo.RestoreOption = (int)mRestoreOption.mAveRestoreMode;
                if (this.Receiver != null)
                {
                    mBaseItemInfo.DocumentSize = this.Receiver.ContentLength;
                }
                mBaseItemInfo.HasStream = this.HasStream;
                mBaseItemInfo.DocData = data;
                mBaseItemInfo.UserData = userData;
#if PerformanceLog
            }
#endif
        }

        internal void ReloadFile(bool fakeDeletedUser = false)
        {
            mAveItem.ReloadFile(fakeDeletedUser);
        }

        public bool MoveToConflictFolder(IAveList parentList, IAveFolder parentFolder, IAveListItem listItem, bool isSourceWin)
        {
            return mAveItem.MoveToConflictFolder(parentList, parentFolder, listItem, isSourceWin);
        }

        public AveItemHoldRecord GetHoldRecord(Hashtable metaInfos, byte[] dataMetaInfo, Dictionary<string, object> userData)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.GetHoldRecord"))
            {
#endif
                try
                {
                    string metaInfoString = null;
                    if (AveCompressedUtility.IsTCompressedBytes(dataMetaInfo))
                    {
                        metaInfoString = AveCompressedUtility.GetTCompressedString(dataMetaInfo);
                    }
                    else
                    {
                        metaInfoString = Encoding.UTF8.GetString(dataMetaInfo);
                    }

                    var fileHoldValue = AveCompressedUtility.ModifyMetaInfoString(metaInfos, metaInfoString);

                    return new AveItemHoldRecord()
                    {
                        ItemHoldRecordStatus = userData["_vti_ItemHoldRecordStatus"].ToString(),
                        ItemDeclaredRecord = userData.ContainsKey("_vti_ItemDeclaredRecord") ? userData["_vti_ItemDeclaredRecord"].ToString() : null,
                        IconOverlay = userData.ContainsKey("IconOverlay") ? userData["IconOverlay"].ToString() : null,
                        ItemLockHolders = fileHoldValue["ecm_ItemLockHolders"],
                        ItemDeleteBlockHolders = fileHoldValue["ecm_ItemDeleteBlockHolders"],
                        HoldsProperty = fileHoldValue["_dlc_Holds_Property"],
                        RecordRestrictions = fileHoldValue["ecm_RecordRestrictions"],
                        IsHold = fileHoldValue["_dlc_Holds_Property"] != null ? true : false,
                        IsRecord = userData.ContainsKey("_vti_ItemDeclaredRecord") ? true : false
                    };
                }
                catch (Exception ex)
                {
                    log.Log(AveLogLevel.INFO, "Get the lock and declared record metainfo. Error:{0}.", ex);
                }
                return null;
#if PerformanceLog
            }
#endif
        }

        public void CreateSouAndDesDefaultValueWithStream(IAveList list, IAveFile spFile, AveSPList aveSPList, bool overWrite)
        {
            if (HasStream)
            {
                Stream stream;
                XmlElement element;
                XmlDocument xSou = new XmlDocument();
                xSou.PreserveWhitespace = true;
                string linkUrl;
                string startStr = AveHttpUtility.UrlPathEncode(list.RootFolder.ServerRelativeUrl, true, false);
                string listRelativeUrl = AveHttpUtility.UrlPathEncode(aveSPList.ListInfo.ServerRelativeUrl, true, false);
                try
                {
                    stream = new AveSPFileStream(mReceiver);
                    xSou.Load(stream);
                    foreach (XmlNode node in xSou.DocumentElement.SelectNodes("a"))
                    {
                        element = (XmlElement)node;
                        linkUrl = element.GetAttribute("href");
                        if (!linkUrl.StartsWith(startStr, StringComparison.OrdinalIgnoreCase))
                        {
                            if (linkUrl.StartsWith(listRelativeUrl, StringComparison.OrdinalIgnoreCase))
                            {
                                element.SetAttribute("href", startStr + linkUrl.Substring(listRelativeUrl.Length));
                            }
                        }
                    }
                    spFile.SaveBinary(Encoding.UTF8.GetBytes(xSou.OuterXml));
                    spFile.Update();
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, WrapperRestoreResource.MergeFileError, list.RootFolder.ServerRelativeUrl + "/Forms/client_LocationBasedDefaults.html", e.ToString());
                }
            }
        }

        public void MergeSouAndDesDefaultValueWithStream(IAveList list, IAveFile spFile, AveSPList aveSPList, bool overWrite)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveSPItem.MergeSouAndDesDefaultValueWithStream"))
            {
#endif
                if (HasStream)
                {
                    XmlDocument xDes = new XmlDocument();
                    XmlDocument xSou = new XmlDocument();
                    xDes.PreserveWhitespace = true;
                    xSou.PreserveWhitespace = true;
                    XmlElement newChild = null;
                    XmlElement temp = null;
                    Stream stream = null;
                    bool existNode = false;
                    bool valueConflict = false;
                    string startStr = list.RootFolder.ServerRelativeUrl;
                    string oldLinkUrl = String.Empty;
                    string oldListRelativeUrl = aveSPList.ListInfo.ServerRelativeUrl;

                    oldListRelativeUrl = AveHttpUtility.UrlPathEncode(oldListRelativeUrl, true, false);
                    startStr = AveHttpUtility.UrlPathEncode(startStr, true, false);

                    Dictionary<string, AveXmlField> xmlFields = aveSPList.AveFields.XmlFields;
                    List<XmlNode> needAddList = new List<XmlNode>();
                    try
                    {
                        stream = new AveSPFileStream(mReceiver);
                        xSou.Load(stream);
                        xDes.LoadXml(new UTF8Encoding().GetString(spFile.OpenBinary())?.EncodeAmpersandInHref());
                        foreach (XmlNode node in xSou.DocumentElement.SelectNodes("a"))
                        {
                            temp = (XmlElement)node;
                            oldLinkUrl = temp.GetAttribute("href");
                            if (!oldLinkUrl.StartsWith(startStr, StringComparison.OrdinalIgnoreCase))
                            {
                                if (oldLinkUrl.StartsWith(oldListRelativeUrl, StringComparison.OrdinalIgnoreCase))
                                {
                                    temp.SetAttribute("href", startStr + oldLinkUrl.Substring(oldListRelativeUrl.Length));
                                }
                            }
                        }
                        foreach (XmlNode souNode in xSou.DocumentElement.SelectNodes("a"))
                        {
                            existNode = false;
                            XmlElement tempSou = (XmlElement)souNode;
                            foreach (XmlNode desNode in xDes.DocumentElement.SelectNodes("a"))
                            {
                                XmlElement tempDes = (XmlElement)desNode;
                                if (tempSou.GetAttribute("href").Equals(tempDes.GetAttribute("href"), StringComparison.OrdinalIgnoreCase))
                                {
                                    foreach (XmlNode addNode in tempSou.SelectNodes("DefaultValue"))
                                    {
                                        valueConflict = false;
                                        XmlElement tempAdd = (XmlElement)addNode;

                                        if (xmlFields.ContainsKey(tempAdd.GetAttribute("FieldName")))
                                        {
                                            xmlFields.Remove(tempAdd.GetAttribute("FieldName"));
                                        }

                                        foreach (XmlNode node in tempDes.SelectNodes("DefaultValue"))
                                        {
                                            temp = (XmlElement)node;
                                            string mappingValue = aveSPList.AveFields.FieldMapping.GetMappingRestoredFieldInternalName(tempAdd.GetAttribute("FieldName"));
                                            if (!String.IsNullOrEmpty(mappingValue))
                                            {
                                                if (temp.GetAttribute("FieldName").Equals(mappingValue))
                                                {
                                                    tempAdd.SetAttribute("FieldName", mappingValue);
                                                    break;
                                                }
                                            }
                                            else
                                            {
                                                if (temp.GetAttribute("FieldName").Equals(tempAdd.GetAttribute("FieldName")))
                                                {

                                                    if (overWrite)
                                                    {
                                                        temp.InnerText = tempAdd.InnerText;
                                                    }
                                                    valueConflict = true;
                                                    break;
                                                }
                                            }
                                        }
                                        if (!valueConflict)
                                        {
                                            newChild = tempDes.FirstChild.OwnerDocument.CreateElement("DefaultValue");
                                            newChild.SetAttribute("FieldName", tempAdd.GetAttribute("FieldName"));
                                            newChild.InnerText = tempAdd.InnerText;
                                            tempDes.AppendChild((XmlNode)newChild);
                                        }
                                    }

                                    if (overWrite)
                                    {
                                        foreach (XmlNode node in tempDes.SelectNodes("DefaultValue"))
                                        {
                                            temp = (XmlElement)node;
                                            if (xmlFields.ContainsKey(temp.GetAttribute("FieldName")))
                                            {
                                                desNode.RemoveChild(node);
                                            }
                                        }
                                    }

                                    existNode = true;
                                    break;
                                }
                            }
                            if (!existNode)
                            {
                                needAddList.Add(souNode);
                            }
                        }
                        foreach (XmlNode newNode in needAddList)
                        {
                            try
                            {
                                temp = (XmlElement)newNode;
                                newChild = xDes.CreateElement("a");
                                newChild.SetAttribute("href", temp.GetAttribute("href"));
                                newChild.InnerXml = temp.InnerXml;
                                xDes.DocumentElement.AppendChild((XmlNode)newChild);
                            }
                            catch (Exception e)
                            {
                                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.MergeFileValuesFailed, e);
                            }
                        }
                        spFile.SaveBinary(Encoding.UTF8.GetBytes(xDes.OuterXml));
                        spFile.Update();
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.MergeFileError, list.RootFolder.ServerRelativeUrl, e.ToString());
                    }
                }
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// 更新原端的list Retention xml stream流用于目的端还原
        /// </summary>
        /// <param name="list"></param>
        /// <param name="spFile"></param>
        /// <param name="aveSPList"></param>
        /// <param name="overWrite"></param>
        public void OverWriteRetionStream(IAveList list, IAveFile spFile, AveSPList aveSPList, bool overWrite)
        {
            try
            {
                if (overWrite)
                {
                    Stream stream = null;
                    stream = new AveSPFileStream(mReceiver);
                    XmlDocument retentionXml = new XmlDocument();
                    retentionXml.Load(stream);
                    XmlElement retentionEle = (XmlElement)retentionXml.DocumentElement.FirstChild;
                    retentionEle.SetAttribute("href", list.RootFolder.ServerRelativeUrl);
                    spFile.SaveBinary(Encoding.UTF8.GetBytes(retentionXml.OuterXml));
                    spFile.Update();
                }

            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, WrapperRestoreResource.OverWriteRetionStreamFailed, e);
            }
        }

        public bool EnsureItemSchemaDependency(Dictionary<string, object> userData, List<Dictionary<string, object>> junctionData, bool restoreSchemaDependency, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption, AveFieldRestoreOption fieldRestoreOption, bool throwException)
        {
            #region Restore item content type and fields
            try
            {
                lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("FieldLock"))
                {
                    ParentList.AveFields.EnsureFields(userData, junctionData, restoreSchemaDependency, skipItemWhenNotFound, skipItemWhenConflict, fieldRestoreOption);
                }
                string contentTypeIdStr = string.Empty;
                if (userData.ContainsKey("#tp_ContentTypeId"))
                {
                    contentTypeIdStr = AveConvert.ConvertByteToContentTypeId(mAveParentSite.ObjectModelFactory, (byte[])userData["#tp_ContentTypeId"]).ToString();
                }
                lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ContentTypeLock"))
                {
                    ParentList.AveContentTypes.EnsureContentType(contentTypeIdStr, ctRestoreOption, restoreSchemaDependency, skipItemWhenNotFound, skipItemWhenConflict, ParentList.AveFields.CreateFieldWhenEnsureFields);
                }
            }
            catch (AveSecurityTrimingException)
            {
                throw;
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, "Exception threw when try to ensure the item schema dependency. Exception info: {0}", e.ToString());
                if (throwException)
                {
                    throw;
                }
                else
                {
                    return false;
                }
            }
            finally
            {
                if (ParentList.SPList != null && ParentSite.MappingManager.SiteMappingManager.ListEnsureFields.ContainsKey(ParentList.SPList.ID)
                        && ParentSite.MappingManager.SiteMappingManager.ListEnsureFields[ParentList.SPList.ID].Count != 0)
                {
                    ParentList.ReloadList();
                }
            }
            #endregion
            return true;
        }

        public bool EnsureItemSchemaDependency(Dictionary<string, object> userData, List<Dictionary<string, object>> junctionData, bool restoreSchemaDependency, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption, AveFieldRestoreOption fieldRestoreOption)
        {
            return EnsureItemSchemaDependency(userData, junctionData, restoreSchemaDependency, skipItemWhenNotFound, skipItemWhenConflict, ctRestoreOption, fieldRestoreOption, true);
        }

        public void EnsureItemContentTypeDependency(Dictionary<string, object> userData, bool restoreSchemaDependency, bool skipItemWhenNotFound, bool skipItemWhenConflict, AveContentTypeRestoreOption ctRestoreOption)
        {
            string contentTypeIdStr = string.Empty;
            if (userData.ContainsKey("#tp_ContentTypeId"))
            {
                contentTypeIdStr = AveConvert.ConvertByteToContentTypeId(mAveParentSite.ObjectModelFactory, (byte[])userData["#tp_ContentTypeId"]).ToString();
            }
            //lock (AvePoint.Wrapper.Common.Common.Utility.LockerDispatcher.GetLocker("ContentTypeLock"))
            //{
            ParentList.AveContentTypes.EnsureContentType(contentTypeIdStr, ctRestoreOption, restoreSchemaDependency, skipItemWhenNotFound, skipItemWhenConflict, ParentList.AveFields.HasCreateFieldWhenEnsureFields);
            //}
        }

        public void EnsureRequiredFieldLink(Dictionary<string, object> userData)
        {
            string contentTypeIdStr = string.Empty;
            if (userData.ContainsKey("#tp_ContentTypeId"))
            {
                contentTypeIdStr = AveConvert.ConvertByteToContentTypeId(mAveParentSite.ObjectModelFactory, (byte[])userData["#tp_ContentTypeId"]).ToString();
            }
            ParentList.AveContentTypes.EnsureRequiredFieldLink(contentTypeIdStr);
        }

        public void Dispose()
        {
            if(report != null)
            {
                report.Dispose();
            }
        }
    }

    public class CacheMutiLookupValue
    {
        public AveLookupObject Obj;
        public int OriVersion;
        public Guid fieldId;
        public ArrayList List;
    }

    public class CurrentRestoreDocStatus
    {
        public int Status;
        public string Name = null;
        public int UIVersion;
        public bool HasPreCurrentVersion = false;
    }

    [Serializable]
    public class AveItemHoldRecord
    {
        public string ItemHoldRecordStatus { get; set; }
        public string ItemLockHolders { get; set; }
        public string ItemDeleteBlockHolders { get; set; }
        public string HoldsProperty { get; set; }
        public string IconOverlay { get; set; }
        public string ItemDeclaredRecord { get; set; }
        public string RecordRestrictions { get; set; }
        public bool IsHold { get; set; }
        public bool IsRecord { get; set; }
    }
}
