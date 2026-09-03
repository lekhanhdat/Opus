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

using AvePoint.Wrapper.Common;
using System;
namespace AvePoint.Wrapper.Restore
{
    public interface IAveSPList : IRestoreableObject
    {
        void AddDefaultViewUrl(string destDefaultUrl);
        bool AutoDeclareRecord { get; }
        IAveSPContentTypeCollection AveContentTypes { get; }
        IAveSPFieldCollection AveFields { get; }
        /// <summary>
        /// Param needChange is only for SPM
        /// </summary>
        /// <param name="needChange">change list setting</param>
        void BackupListSetting(bool needChange = false);
        void BackupListWorkflowSetting();
        void DecodeNameForSpecialChar();
        void DeleteItemsForCategory();
        void Dispose();
        void EnableListVersioning(AvePoint.Wrapper.Common.AveVersionMode versionMode);
        AvePoint.Wrapper.Common.IReport GetReport();
        bool IsConfictWithRecycle(string name, Guid WebId, AvePoint.Wrapper.Common.IAveBackupRestoreQueryService queryService);
        bool IsNewCreated { get; }
        bool IsReportingMetadataList();
        bool IsSupportToSetNull(string internalName);
        bool IsSystemList { get; }
        bool IsTaxonomyList { get; }
        bool KeepDefaultValue { get; }
        AvePoint.Wrapper.Common.AveListInfo ListInfo { get; }
        AvePoint.Wrapper.Common.AveListSettingInfo ListSettingInfo { get; set; }
        bool MoveConnectorSetting { get; set; }
        string Name { get; set; }
        bool NeedContinue { get; set; }
        Guid OldId { get; }
        IAveSPSite ParentSite { get; }
        IAveSPWeb ParentWeb { get; }
        AvePoint.Wrapper.Common.IAveListItem PreItem { get; set; }
        void ProcessListRattingSetting(bool sourceEnable, bool destEnable);
        AvePoint.Wrapper.Common.IAveBackupRestoreQueryService QueryService { get; }
        void ReloadList();
        void RestoreDocumentTemplateUrl();
        void RestoreListProperty(AvePoint.Wrapper.Common.AveListSettingInfo listSettingInfo, bool RestoreListOnQuickLaunch = true);
        void RestoreListRootFolder();
        void RestoreListSelf(AvePoint.Wrapper.Common.AveListInfo listInfo);
        void RestoreListSelf(AvePoint.Wrapper.Common.AveListInfo listInfo, ListRestoreOption option, bool allowRestoreToSameList = false);
        void RestoreListSetting();
        void RestoreMetadataNavigationSettings();
        bool RestoreRssView { get; set; }
        void RestoreUnRestoreWebPart(IReport report);
        AvePoint.Wrapper.Common.RestoringDto RestoringFolder { get; }
        AvePoint.Wrapper.Common.IAveFolder RootFolder { get; }
        string RootFolderPath { get; }
        IAveObjectSecurity Security { get; }
        string ServerRelativeUrl { get; }
        void SetListSettingFlags(int value);
        AvePoint.Wrapper.Common.IAveList SPList { get; }
        bool StopAlerts { get; set; }
        System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<AvePoint.Wrapper.Common.AveExtendMasterPageInfo>> TempMasterSettings { get; }
        void Update_ReportTemplateistWebProperties();
        void UpdateDefaultValue();
        string Url { get; }
        string WelComePage { get; }
        void RestoreUserCustomActions(System.Collections.Generic.List<AvePoint.Wrapper.Common.AveUserCustomActionInfo> customActions);
    }
}
