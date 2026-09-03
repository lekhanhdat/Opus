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
using System.Diagnostics.CodeAnalysis;
using System.IO;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPDoc
    {
        private AveLogger mLogger = AveLogger.GetInstance(typeof(AveSPDoc));
        private AveSPFolder mParentFolder;
        private IAveBackupStream mSender;
        private AveSPItem mAveSPItem;
        private DateTime mBiggestVersionModified = DateTime.MinValue;

        public bool IsFormPage = false;
        public AveSPDoc(AveSPFolder aveFolder, Guid id, int rowId, int version)
            : this(aveFolder, id, rowId, version, null)
        { }

        public AveSPDoc(AveSPFolder aveFolder, Guid id, int rowId, int version, string serverRelativeUrl)
            : this(aveFolder,id, rowId, version, serverRelativeUrl, null)
        {

        }

        public AveSPDoc(AveSPFolder aveFolder, Guid id, int rowId, int version, string serverRelativeUrl, IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.Constructor"))
            {
                mParentFolder = aveFolder;
                mSender = aveFolder.Sender;
                mAveParentSite = aveFolder.ParentSite;
                mAveSPItem = new AveSPItem(id, rowId, version, serverRelativeUrl, AveItemType.Document, mParentFolder.Id,
                    aveFolder.AveList.ParentWeb.ParentSite.SPSite.ID, aveFolder.AveList, aveFolder.Sender, aveFolder.QueryService, aveFolder.AveList.Fields, aveFolder.AveList.SolutionStatus, item, aveFolder.SPFolder);
                //mAveSPItem.ParentId = mParentFolder.Id;
            }
        }

        public AveSPFolder ParentFolder
        {
            get { return mParentFolder; }
        }

        public void SetBiggestVersionModified(DateTime value)
        {
            mBiggestVersionModified = value;
        }

        public void CachePrincipalFromMetadata()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.CachePrincipalFromMetadata"))
            {
                mAveSPItem.CachePrincipalFromMetadata();
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Datajunction is a part of method name")]
        public void CachePrincipalFromDatajunction()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.CachePrincipalFromDatajunction"))
            {
                mAveSPItem.CachePrincipalFromDatajunction();
            }
        }

        public void CacheUserFromAlert()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.CacheUserFromAlert"))
            {
                mAveSPItem.CacheUserFromAlert(this);
            }
        }

        public void CachePrincipalFromPermission(int value)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.CachePrincipalFromPermission"))
            {
                mAveSPItem.CachePrincipalFromPermission(value);
            }
        }

        public void CachePrincipalOfTargetAudience()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.CachePrincipalOfTargetAudience"))
            {
                mAveSPItem.CachePrincipalOfTargetAudience();
            }
        }

        public void CacheUserFromWebParts()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.CacheUserFromWebParts"))
            {
                mAveSPItem.CacheUserFromWebParts();
            }
        }

        public void ExportUnavailableUserInCache(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportUnavailableUserInCache"))
            {
                mAveSPItem.ExportUnavailableUserInCache(output);
            }
        }

        public void ExportUserCache(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportUserCache"))
            {
                output.WriteMetadata(AveMetadataType.UserCache, mAveSPItem.DataCache.UserList);
            }
        }

        public string ExportUserCache()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.UserCache.ToString(), mAveSPItem.DataCache.UserList);
        }

        public void ExportGroupCache(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportGroupCache"))
            {
                output.WriteMetadata(AveMetadataType.GroupCache, mAveSPItem.DataCache.GroupList);
            }
        }

        public string ExportGroupCache()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.GroupCache.ToString(), mAveSPItem.DataCache.GroupList);
        }

        public void ExportDocInfo(IAveBackupStream output, bool exportAddtional_CommentsOnInfo = false)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportDocInfo"))
            {
                Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo(getAddtional_CommentsOnInfo: exportAddtional_CommentsOnInfo);
                if (mAveSPItem.RowId <= 0)  //SAAS-11718
                {
                    mAveSPItem.CheckPageView(docInfo, mParentFolder.AveList.GetViews(mAveSPItem.Id));
                }
                mAveSPItem.CheckFormView(docInfo);
                if (docInfo != null)
                {
                    if (docInfo.ContainsKey("IsFormPage"))
                    {
                        IsFormPage = Convert.ToBoolean(docInfo["IsFormPage"]);
                    }
                    if (mBiggestVersionModified != DateTime.MinValue)
                    {
                        docInfo["BiggestVersionModified"] = mBiggestVersionModified;
                    }
                    output.WriteMetadata(AveMetadataType.DocProperty, docInfo);
                }
            }
        }

        public string ExportContentAndCalculateCRC(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportContentAndCalculateCRC"))
            {
                return mAveSPItem.ExportContentByAPIAndCalculateCRC(output);
            }
        }

        public string ExportDocInfo()
        {
            string xml = string.Empty;
            Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
            mAveSPItem.CheckPageView(docInfo, mParentFolder.AveList.GetViews(mAveSPItem.Id));
            mAveSPItem.CheckFormView(docInfo);
            if (docInfo != null)
            {
                xml = AveConvert.ConvertAveObjToAveXml(AveMetadataType.DocProperty.ToString(), docInfo);
            }
            return xml;
        }

        /// <summary>
        /// To backup all the versions of this item. (Version Filter in Item backup2010 will use it.)
        /// </summary>
        /// <param name="output"></param>
        public void ExportDocVersions(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportDocVersions"))
            {
                List<int> docVersions = mAveSPItem.GetDocVersions();
                if (docVersions != null && docVersions.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.DocVersions.ToString(), docVersions);
                }
            }
        }

        public void ExportUserDataInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportUserDataInfo"))
            {
                Dictionary<string, object> userData = mAveSPItem.UserDataCache;
                if (userData == null)
                {
                    userData = mAveSPItem.GetUserData();
                }
                if (userData != null)
                {
                    if (mParentFolder.AveList.NeedExportExcel && !mParentFolder.AveList.SPList.Hidden)
                    {
                        if (!string.IsNullOrEmpty(this.Url))
                        {
                            mAveSPItem.ExportDataToExcel(this.Url.Substring(this.Url.IndexOf(this.AveSPWeb.ScopeString.ToString(), StringComparison.OrdinalIgnoreCase)));
                        }
                    }
                    output.WriteMetadata(AveMetadataType.DocData, userData);
                }
                else if (this.AveSPItem.RowId > 0)
                {
                    mLogger.Warn("user data is missing, item url: {0}", this.Url);
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "StorgeInfo is a part of common method name")]
        public void ExportStorgeInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportStorgeInfo"))
            {
                mAveSPItem.ExportStorageInfo(output);
            }
        }

        public void ExportDataJunctionInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportDataJunctionInfo"))
            {
                List<Dictionary<string, object>> userDatajunction = mAveSPItem.UserDatajunctionCache;
                if (userDatajunction != null)
                {
                    output.WriteMetadata(AveMetadataType.DocDataJunction, userDatajunction);
                }
            }
        }

        public void ExportWebParts(IAveBackupStream output)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPDoc.ExportWebParts"))
            {
                AveSPLiminitedWebPartManager manager = new AveSPLiminitedWebPartManager(mAveSPItem);
                manager.Export(output);
            }
        }

        public void ExportLookupFieldGuidValue(IAveBackupStream output)
        {
            using (AvePerformanceScope scope = new AvePerformanceScope("Backup.AveSPDoc.ExportLookupFieldGuidValue"))
            {
                Dictionary<string, string> lookupFieldGuidValue = mAveSPItem.GetLookupFieldGuidValue();
                if (lookupFieldGuidValue != null && lookupFieldGuidValue.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.LookupFieldGuidValue.ToString(), lookupFieldGuidValue);
                }
            }
        }

        public void ExportRbsId(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportRbsId"))
            {
                mAveSPItem.ExportRbsId(output);
            }
        }

        //public void ExportContent(IAveBackupStream output, IStreamConvertor streamConvertor)
        //{
        //    using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportContent"))
        //    {
        //        mAveSPItem.ExportContent(output, streamConvertor);
        //    }
        //}

        public void ExportContent(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportContent"))
            {
                mAveSPItem.ExportContent(output);
            }
        }

        public Stream GetContent()
        {
            return mAveSPItem.GetContent();
        }

        public void ExportFullTextIndex(IAveBackupStream output, FullTextIndexLevel level = FullTextIndexLevel.BaseInfo)
        {
            ExportFullTextIndex(output, null, level);
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues, FullTextIndexLevel level = FullTextIndexLevel.BaseInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.ExportFullTextIndex"))
            {
                mAveSPItem.ExportFullTextIndex(output, customFieldValues, level);
            }
        }

        public Dictionary<string, object> GetAllColumnValues(ColumnsLevel columnsLevel = ColumnsLevel.None)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.GetAllColumnValues"))
            {
                return mAveSPItem.GetAllColumnValues(columnsLevel);
            }
        }

        public Dictionary<string, string> GetMetaInfo()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.GetMetaInfo"))
            {
                return mAveSPItem.GetMetaInfo();
            }
        }

        public AveSPItem AveSPItem
        {
            get
            {
                return mAveSPItem;
            }
        }

        private AveSPSite mAveParentSite;

        public AveSPSite ParentSite
        {
            get { return mAveParentSite; }
        }

        public AveSPWeb AveSPWeb
        {
            get
            {
                return mParentFolder.AveList.ParentWeb;
            }
        }

        public bool HasContent
        {
            get { return mAveSPItem.HasStream; }
        }

        public bool IsVersion
        {
            get { return mAveSPItem.IsVersion; }
        }

        public bool HasUniqueRoleAssignments
        {
            get
            {
                return mAveSPItem.HasUniqueRoleAssignments;
            }
        }

        public string Url
        {
            get
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPDoc.Url"))
                {
                    //  s...d/a.aspx
                    if (String.IsNullOrEmpty(mAveSPItem.ScopeUrl))
                    {
                        return string.Empty;
                    }
                    string fileUrl = mAveSPItem.ScopeUrl.TrimStart('/').Substring(AveSPWeb.SPWeb.ServerRelativeUrl.TrimStart('/').Length).TrimStart('/');
                    return AveSPWeb.SPWeb.Url.TrimEnd('/') + "/" + fileUrl;
                }
            }
        }

        #region For Archiver
        /// <summary>
        /// Export Metadata for document
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="backupOption"></param>
        public void ExportMetadata(IAveBackupStream stream, SPItemMetadataBackupOption backupOption)
        {
            var metadata = new AveSPDocumentMetadataDto();

            #region backup ItemMetadata
            if (this.mAveSPItem.RowId > 0)
            {
                var userData = mAveSPItem.GetUserDataInfoWithDependence(backupOption);
                metadata.UserDataInfo = userData.ItemA;
                metadata.MetadataInfo = userData.ItemB;

                metadata.DocDataJunction = mAveSPItem.GetUserDataJunctionCache(true);
                //output.WriteMetadata(AveMetadataType.DocDataJunction, dataCache);

                if (backupOption != null && backupOption.BackupItemTPGUIDofLookupValue)
                {
                    metadata.ItemTPGUIDofLookupValue = mAveSPItem.GetLookupFieldGuidValue();
                }
            }
            #endregion
            ArgumentNullException.ThrowIfNull(backupOption);
            if (backupOption.IncludeAllUIVersions)
            {
                metadata.ItemUIVersionNums = this.mAveSPItem.GetDocVersions();
            }

            //var storageInfo = mAveSPItem.GetAllStorageInfo();
            //metadata.StorageInfo = storageInfo.ItemA;
            //metadata.StorageInfo13 = storageInfo.ItemB;

            metadata.WebParts = GetWebParts(true);

            metadata.DocInfo_Old = GetDocInfo();
            if (backupOption != null)
            {
                if (backupOption.IncludeUser)
                {
                    metadata.UserCache = mAveSPItem.GetUserCache(false);
                }
                if (backupOption.IncludeGroup)
                {
                    metadata.GroupCache = mAveSPItem.GetGroupCache();
                }
            }

            stream.WriteMetadata(AveMetadataType.ItemMetadataDto, metadata);
        }

        internal List<AveWebPartBaseInfo> GetWebParts(bool includeUsers)
        {
            if (includeUsers)
            {
                this.AveSPItem.CacheUserFromWebParts();
            }
            var manager = new AveSPLiminitedWebPartManager(mAveSPItem);
            return manager.GetWebParts();
        }

        /// <summary>
        /// Get Doc Info
        /// </summary>
        /// <returns></returns>
        private Dictionary<string, object> GetDocInfo()
        {
            Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
            mAveSPItem.CheckPageView(docInfo, mParentFolder.AveList.ViewCache);
            if (docInfo != null)
            {
                if (mBiggestVersionModified != DateTime.MinValue)
                {
                    docInfo["BiggestVersionModified"] = mBiggestVersionModified;
                }
            }
            return docInfo;
        }
        #endregion
    }
}