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
using System.Collections.Generic;
namespace AvePoint.Wrapper.Restore
{
    public interface IAveSPWeb:IRestoreableObject
    {
        string AlternateCSSUrl { get; set; }
        void ClearDefaultList();
        void ClearWebNavigation();
        IAveSPContentTypeCollection ContentTypes { get; }
        void Dispose();
        IAveObjectFeature Feature { get; }
        IAveSPFieldCollection Fields { get; }
        AvePoint.Wrapper.Common.IAveFolder GetFolderByRelativeUrl(string relativeUrl);
        AvePoint.Wrapper.Common.IReport GetReport();
        AvePoint.Wrapper.Common.AveRoleInfo GetRoleByName(string roleName);
        int GetUserIdByDisplayName(string userName);
        int GetUserIdByName(string name);
        bool IsNewCreated { get; }
        System.Collections.Generic.Dictionary<Guid, System.Collections.Generic.Dictionary<Guid, Guid>> ListAlertIdMappings { get; }
        IAveSPMembers Members { get; }
        string Name { get; set; }
        IAveSPNavigation Navigation { get; }
        bool NeedContinue { get; set; }
        Guid OldId { get; }
        IAveSPSite ParentSite { get; }
        AvePoint.Wrapper.Common.IAveBackupRestoreQueryService QueryService { get; }
        void ReloadWeb();
        string ReportMessage { get; }
        void Restore();
        void RestoreAlternateCSSUrl();
        void RestoreAssociateGroups();
        void RestoreAuthor();
        void RestoreCacheProfileListId();
        void RestoreContentOrginazerSetting();
        void RestoreEmailSubmittedRecordsListIDProperty();
        void RestoreHiddenPageProperty();
        [Obsolete("Used for DocAve5 Site bin Restore, never used in DocAve6")]
        void RestoreOriginTitle();
        bool RestorePermissionLevel { set; }
        void RestoreRelationShipListSetting();
        void RestoreRequestAccessEmail();
        void RestoreSiteLogoUrl();
        void RestoreThemeCssFolderUrl();
        void RestoreWebProperty(AvePoint.Wrapper.Common.AveWebSettingInfo webSettingInfo);
        void RestoreWebProperty(AvePoint.Wrapper.Common.AveWebSettingInfo webSettingInfo, bool isRestoreWebRegionalSettings);
        void RestoreWebSelf(AvePoint.Wrapper.Common.AveWebInfo webInfo);
        void RestoreWelcomePage();
        AvePoint.Wrapper.Common.RestoringDto RestoringWeb { get; }
        System.Collections.Generic.List<AvePoint.Wrapper.Common.AveRoleInfo> Roles { get; set; }
        string ScopeString { get; }
        IAveObjectSecurity Security { get; }
        string ServerRelativeUrl { get; }
        void SetLanguageForNew(uint LCD);
        long Size { get; }
        AvePoint.Wrapper.Common.IAveWeb SPWeb { get; }
        string SrcUrl { get; }
        Guid TaxonomyHiddenList { get; }
        string ThemedCssFolderUrl { get; set; }
        string ThemeTitle { get; set; }
        AvePoint.Wrapper.Common.IAveThmxTheme ThmxTheme { get; }
        void UpdateDocumentSetCT();
        string Url { get; }
        AvePoint.Wrapper.Common.AveWebInfo WebInfo { get; }
        bool WebNavigationRestore { get; set; }
        AvePoint.Wrapper.Common.AveWebSettingInfo WebSettingInfo { get; set; }
        uint WebSrcLanguageId { get; }
        void SetListTitleMapping(Dictionary<string, string> mapping);
        void AddNintexFormControlTypeMapping(Guid listId, string contentTypeId, Dictionary<Guid, AveNintexFormControlType> uniqueIdMapping, Dictionary<string, AveNintexFormControlType> displayNameMapping);
        void RestoreProjectPolicy();
        void RestoreUserCustomActions(System.Collections.Generic.List<AvePoint.Wrapper.Common.AveUserCustomActionInfo> customActions);
        void AddtoWFEnableCache(Guid listId, Guid definationId, bool enable);
    }
}
