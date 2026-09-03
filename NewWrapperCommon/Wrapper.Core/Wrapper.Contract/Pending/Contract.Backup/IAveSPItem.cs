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
using AvePoint.Wrapper.Common;
using System.IO;

namespace AvePoint.Wrapper.Backup
{
    public interface IAveSPItem
    {
        IAveSPSite AveSPSite { get; }
        IAveSPList AveSPList { get; }
        IAveItem Item { get; }
        IAveListItem SPListItem { get; }
        bool HasUniqueRoleAssignments { get; }
        bool IsSystemFileOrFolder { get; }
        bool IsVersion { get; }
        bool IsPRItemBackup { get; set; }
        Guid Id { get; }
        int RowId { get; }
        Guid ParentId { get; }
        long DocumentSize { get; }
        int Version { get; }
        Guid ScopeId { get; }
        string Title { get; }
        string Name { get; set; }
        string ServerRelativeUrl { get; set; }
        bool IsBackupLinkForArchivedData { get; set; }
        AveStorageType StorageType { get; }
        AveUserInfo Author { get; }
        AddExtraPropertyInDataCache ExtraPropertyInDataCache { get; set; }
        bool IsConnectorLinkFile { get; }

        void ExportUserDataInfo(IAveBackupStream output, AveBackupOption backupColumnOption = null, bool includeUserAndGroup = true, bool onlyUnAvaiableUser = false);
        void ExportDataJunctionInfo(IAveBackupStream output, bool includeUserAndGroup = true, bool onlyUnAvaiableUser = false);
        void ExportLookupFieldGuidValue(IAveBackupStream output);
        void ExportVersions(IAveBackupStream output);
        List<int> GetItemVersions();


        /// <summary>只有Raplicator需要，别的模块User不需要单独控制，会跟Permission走</summary>
        void ExportUsers(IAveBackupStream output);

        /// <summary>只有Raplicator需要，别的模块Group不需要单独控制，会跟Permission走</summary>
        void ExportGroups(IAveBackupStream output);

        /// <param name="includeUserAndGroup">是否先备份相关的User和Group，避免还原的时候不存在</param>
        void ExportRoleAssignments(IAveBackupStream output, bool includeUserAndGroup = true);
        void ExportWorkflowInstance(IAveBackupStream output, bool forceBackup = false, string contentDBconnectionString = null, string configDBconnectionString = null);
        void ExportWorkflowSchedule(IAveBackupStream output, bool forceBackup = false, string contentDBconnectionString = null, string configDBconnectionString = null);
        void ExportFullTextIndex(IAveBackupStream output, Dictionary<string, object> customFieldValues, FullTextIndexLevel level);
        void ExportLinksInfos(IAveBackupStream output);

        Stream GetContent();
        Dictionary<string, string> GetMetaInfo();
        Dictionary<string, object> GetColumnValues(ColumnsLevel level = ColumnsLevel.AllColumns, bool forceGetByAPI = true);
        FullTextIndex GetFullTextIndex(FullTextIndexLevel level, Dictionary<string, object> customFieldValues = null);
        void ExportComplianceTag(IAveBackupStream output);
    }

    /// <summary>
    /// AllVisiableColumns 是非隐藏的所有column，获取是column的Displayname;
    /// AllColumns 指所有的column，包括隐藏和系统column，获取的key 是column的InternalName.
    /// </summary>
    public enum ColumnsLevel
    {
        None = 0,
        DisplayColumns = 1,
        AllVisiableColumns = 2,
        AllColumns = 3,
    }


    public enum FullTextIndexLevel
    {
        Invalid = -1,
        BaseInfo = 0,
        IncludeDefaultViewColumns = 1,
        IncludeAllVisiableColumns = 2,
        //已弃用，原来是指所有DB中有值的column
        IncludeAllColumns = 3,
        IncludeAllColumnsAndSystemColumns = 4,
    }

    public delegate void AddExtraPropertyInDataCache(Dictionary<string, object> dataCache, Dictionary<string, object> docData);

}
