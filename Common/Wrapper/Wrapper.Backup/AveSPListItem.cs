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
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPListItem
    {
        private Guid mId;
        private string mName;
        private AveSPFolder mParentFolder;
        private IAveBackupRestoreQueryService mQueryService;
        private AveSPItem mAveSPItem;
        private AveSPList mAveList;

        private int mVersion;
        private DateTime mBiggestVersionModified = DateTime.MinValue;

        public void SetBiggestVersionModified(DateTime value)
        {
            mBiggestVersionModified = value;
        }

        public AveSPListItem(AveSPFolder aveFolder, string name, Guid id, int rowId, int version)
            : this(aveFolder, name, id, rowId, version, null, null)
        {

        }

        public AveSPListItem(AveSPFolder aveFolder, string name, Guid id, int rowId, int version, IAveListItem item)
            : this(aveFolder, name, id, rowId, version, null, item)
        {

        }

        public AveSPListItem(AveSPFolder aveFolder, string name, Guid id, int rowId, int version, string serverRelativeUrl, IAveListItem item)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.Constructor"))
            {
                mParentFolder = aveFolder;
                mQueryService = aveFolder.QueryService;
                mAveList = aveFolder.AveList;
                mVersion = version;
                mId = id;
                mName = name;
                Init(rowId, serverRelativeUrl, item);
            }
        }

        //add by adrian for 07 item backup 07item 备份userdata时，需要 serverRelativeUrl
        public AveSPListItem(AveSPFolder aveFolder, string name, Guid id, int rowId, int version, string serverRelativeUrl)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.Constructor"))
            {
                mParentFolder = aveFolder;
                mQueryService = aveFolder.QueryService;
                mAveList = aveFolder.AveList;
                mVersion = version;
                mId = id;
                mName = name;
                Init(rowId, serverRelativeUrl);
            }
        }

        private void Init(int rowId, string serverRelativeUrl, IAveListItem item)
        {
            mAveSPItem = new AveSPItem(mId, rowId, mVersion, serverRelativeUrl, AveItemType.ListItem, mParentFolder.Id,
                mAveList.ParentWeb.ParentSite.SPSite.ID, mAveList,
                null, mQueryService, mAveList.Fields, null, item, mParentFolder.SPFolder);
            //mAveSPItem.ParentId = mParentFolder.Id;
        }

        // add by adrian for 07 item backup 07item 备份userdata时，需要 serverRelative
        private void Init(int rowId, string serverRelativeUrl)
        {
            mAveSPItem = new AveSPItem(mId, rowId, mVersion, serverRelativeUrl, AveItemType.ListItem, mParentFolder.Id,
                mAveList.ParentWeb.ParentSite.SPSite.ID, mAveList,
                null, mQueryService, mAveList.Fields, mAveList.SolutionStatus, null);
            //mAveSPItem.ParentId = mParentFolder.Id;
        }

        public void CachePrincipalFromMetadata()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.CachePrincipalFromMetadata"))
            {
                mAveSPItem.CachePrincipalFromMetadata();
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "InfomationList is a part of common method name")]
        public void CacheUserForUserInfomationList()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.CacheUserForUserInfomationList"))
            {
                mAveSPItem.CacheUserForUserInfomationList();
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Datajunction is a part of common method name")]
        public void CachePrincipalFromDatajunction()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.CachePrincipalFromDatajunction"))
            {
                mAveSPItem.CachePrincipalFromDatajunction();
            }
        }

        public void CachePrincipalFromPermission(int value)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.CachePrincipalFromPermission"))
            {
                mAveSPItem.CachePrincipalFromPermission(value);
            }
        }

        public void CachePrincipalOfTargetAudience()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.CachePrincipalOfTargetAudience"))
            {
                mAveSPItem.CachePrincipalOfTargetAudience();
            }
        }

        public void ExportUnavailableUserInCache(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportUnavailableUserInCache"))
            {
                mAveSPItem.ExportUnavailableUserInCache(output);
            }
        }

        public void CacheUserFromAlert()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.CacheUserFromAlert"))
            {
                mAveSPItem.CacheUserFromAlert(this);
            }
        }

        public void ExportUserCache(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportUserCache"))
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
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportGroupCache"))
            {
                output.WriteMetadata(AveMetadataType.GroupCache, mAveSPItem.DataCache.GroupList);
            }
        }

        public string ExportGroupCache()
        {
            return AveConvert.ConvertAveObjToAveXml(AveMetadataType.GroupCache.ToString(), mAveSPItem.DataCache.GroupList);
        }

        public void ExportDocInfo(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportDocInfo"))
            {
                Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
                if (docInfo != null)
                {
                    if (mBiggestVersionModified != DateTime.MinValue)
                    {
                        docInfo["BiggestVersionModified"] = mBiggestVersionModified;
                    }
                    output.WriteMetadata(AveMetadataType.DocProperty, docInfo);
                }
            }
        }

        public string ExportDocInfo()
        {
            string xml = string.Empty;
            Dictionary<string, object> docInfo = mAveSPItem.GetDocInfo();
            if (docInfo != null)
            {
                xml = AveConvert.ConvertAveObjToAveXml(AveMetadataType.DocProperty.ToString(), docInfo);
            }
            return xml;
        }

        public void ExportDocVersions(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportDocVersions"))
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
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportUserDataInfo"))
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
                        mAveSPItem.ExportDataToExcel(this.mParentFolder.ServerRelativeUrl.Substring(this.mParentFolder.ServerRelativeUrl.IndexOf(this.AveSPWeb.ScopeString.ToString(), StringComparison.OrdinalIgnoreCase)));
                    }
                    output.WriteMetadata(AveMetadataType.DocData, userData);
                }
            }
        }

        public void ExportDataJunctionInfo(IAveBackupStream output)
        {
            List<Dictionary<string, object>> userDatajunction = mAveSPItem.UserDatajunctionCache;
            if (userDatajunction != null)
            {
                output.WriteMetadata(AveMetadataType.DocDataJunction, userDatajunction);
            }
        }

        public void ExportLookupFieldGuidValue(IAveBackupStream output)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportLookupFieldGuidValue"))
            {
                Dictionary<string, string> lookupFieldGuidValue = mAveSPItem.GetLookupFieldGuidValue();
                if (lookupFieldGuidValue != null && lookupFieldGuidValue.Count > 0)
                {
                    output.WriteMetadata(AveMetadataType.LookupFieldGuidValue.ToString(), lookupFieldGuidValue);
                }
            }
        }

        public AveSPItem AveSPItem
        {
            get
            {
                return mAveSPItem;
            }
        }

        public AveSPSite AveSPSite
        {
            get
            {
                return mParentFolder.AveList.ParentWeb.ParentSite;
            }
        }

        public AveSPWeb AveSPWeb
        {
            get
            {
                return mParentFolder.AveList.ParentWeb;
            }
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

        public void ExportFullTextIndex(IAveBackupStream output, FullTextIndexLevel level = FullTextIndexLevel.BaseInfo)
        {
            ExportFullTextIndex(output, null, level);
        }

        public void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues, FullTextIndexLevel level = FullTextIndexLevel.BaseInfo)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.ExportFullTextIndex"))
            {
                mAveSPItem.ExportFullTextIndex(output, customFieldValues, level);
            }
        }

        public Dictionary<string, object> GetAllColumnValues(ColumnsLevel columnsLevel = ColumnsLevel.None)
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.GetAllColumnValues"))
            {
                return mAveSPItem.GetAllColumnValues(columnsLevel);
            }
        }

        public Dictionary<string, string> GetMetaInfo()
        {
            using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.GetMetaInfo"))
            {
                return mAveSPItem.GetMetaInfo();
            }
        }

        public string TagUrl
        {
            get
            {
                using (AvePerformanceScope pc = new AvePerformanceScope("Backup.AveSPListItem.TagUrl"))
                {
                    //  s...d/a.aspx
                    string fileUrl = string.Empty;
                    string webUrl = AveSPWeb.SPWeb.Url;
                    string webRelativeUrl = AveSPWeb.SPWeb.ServerRelativeUrl;
                    if (!string.IsNullOrEmpty(mAveList.SPList.DefaultDisplayFormUrl))
                    {
                        if (mAveList.SPList.BaseTemplate == AveListTemplateType.UserInformation)
                        {
                            fileUrl = mAveList.SPList.DefaultDisplayFormUrl + "?ID=" + mAveSPItem.RowId;
                        }
                        else
                        {
                            fileUrl = webUrl.TrimEnd('/') + "/" + mAveList.SPList.DefaultDisplayFormUrl.TrimStart('/').Substring(webRelativeUrl.TrimStart('/').Length).TrimStart('/') + "?ID=" + mAveSPItem.RowId;
                        }
                    }
                    else if (mAveSPItem.AveSPList.SPList.BaseTemplate == AveListTemplateType.Meetings && mAveSPItem.Item.ListItem.ID != 0)
                    {
                        if (this.mParentFolder.SPFolder.HiddenFiles != null)
                        {
                            if (this.mParentFolder.SPFolder.HiddenFiles.Count > 1 && webUrl.LastIndexOf(webRelativeUrl, StringComparison.OrdinalIgnoreCase) > 0)
                            {
                                fileUrl = webUrl.Substring(0, webUrl.LastIndexOf(webRelativeUrl, StringComparison.OrdinalIgnoreCase)) + this.mParentFolder.ServerRelativeUrl + "/" + this.mParentFolder.SPFolder.HiddenFiles[0].Name + "?ID=" + mAveSPItem.Item.ListItem.ID;
                            }
                        }
                    }
                    return fileUrl;
                }
            }
        }
    }
}