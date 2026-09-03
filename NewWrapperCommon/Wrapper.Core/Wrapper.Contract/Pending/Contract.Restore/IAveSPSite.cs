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
    public interface IAveSPSite:IRestoreableObject
    {
        void AddUnReplaceUrlIDCache(Guid webId, Guid listId, int itemId, string fieldName);
        void AddUnRestoreFileHoldRecordInfo(Guid webId, string url, AveItemHoldRecord itemHoldRecord);
        void AddUnRestoreItemHoldRecordInfo(Guid webId, Guid listId, int itemId, AveItemHoldRecord itemHoldRecord);
        void AddUnRestoreListLastModifiedTime(Guid listId, DateTime lastModified);
        string ApplicationName { get; }
        bool AutoDropOffContentOrganizer { get; set; }
        AvePoint.Wrapper.Common.AveLanguageProcesser AveLanguageProcesser { get; set; }
        AvePoint.Wrapper.Common.AveBPOSAccountInfo BPOSUserAccountInfo { get; }
        Guid CheckOutFileId { get; set; }
        int CheckOutUser { get; set; }
        void ClearSiteGroups();
        string DefaultUser { get; set; }
        string DestinationURL { get; set; }
        void DisableSPEventReceiver();
        void Dispose();
        void EnableSPEventReceiver();
        System.Collections.Generic.List<Guid> GetAllWebsGuid(AvePoint.Wrapper.Common.IAveSite site);
        System.Collections.Generic.List<Guid> GetAllWebsGuidByNative(Guid siteId);
        void getDestSelfHidden(string navigationExclude, System.Text.StringBuilder selfNavigation, AvePoint.Wrapper.Common.IAveWeb web, System.Collections.Generic.Dictionary<string, string> sourcWebsAndPages);
        System.Collections.Generic.List<AvePoint.Wrapper.Common.IAveListItem> GetHoldItemID(string holdsProperty);
        Guid GetList(Guid webId, string title);
        AvePoint.Wrapper.Common.IAveView GetListViewFromUrl(AvePoint.Wrapper.Common.IAveList list, string viewUrl);
        string GetNameByLanguageMapping(string name, AvePoint.Wrapper.Common.AveLanguageMappingType languageType);
        string GetPlaceHolderAccount();
        AvePoint.Wrapper.Common.IReport GetReport();
        Guid GetWeb(AvePoint.Wrapper.Common.IAveBackupRestoreQueryService queryService, string p);
        AvePoint.Wrapper.Common.IAveWeb GetWebByName(string name);
        bool HasFBAProvider();
        string IdReplace(string oldUrl, ref bool needReplaceLast);
        bool IsFBAUser(string domain);
        bool IsNewCreated { get; set; }
        bool KeepDefaultValue { get; set; }
        uint LanguageForNewCreate { get; set; }
        AvePoint.Wrapper.Common.AveMappingManager MappingManager { get; }
        void mergeNavigation(string dest, string source, ref System.Text.StringBuilder finalNavigation);
        IAveMetadataService MetadataService { get; set; }
        AvePoint.Wrapper.Common.NavigationRestoreSetting NavigationRestoreSetting { get; set; }
        AvePoint.Wrapper.Common.AveObjectModelFactory ObjectModelFactory { get; }
        AvePoint.Wrapper.Common.IAveWeb OpenWeb(string relativeUrl);
        AvePoint.Wrapper.Common.IAvePublishing Publishing { get; }
        AvePoint.Wrapper.Common.IAveBackupRestoreQueryService QueryService { get; }
        AvePoint.Wrapper.Common.AveRBSRestore RBSRestore { get; }
        void ReloadSite();
        void RestoreCalendarSettings();
        void RestoreDataSourceFields();
        void RestoreHiddenSiteProperty();
        void RestoreLanguageFile(AvePoint.Wrapper.Common.AveLanguageInfo languageInfo);
        void RestoreListLastModifiedTime();
        void RestoreLookupFields(Guid oldId);
        void RestoreLookupFieldValues();
        void RestoreLookupFieldValues(Guid ID, ref IAveWeb parentWeb, ref IAveList parentList);
        void RestoreMasterPageProperty();
        void RestoreMySiteRecentBlog();
        void RestoreNavNodes(IReport report);
        void RestorePerformancePointProperties();
        void RestoreSiteSelf(AvePoint.Wrapper.Common.AveSiteInfo siteInfo);
        void RestoreSiteSelf(AvePoint.Wrapper.Common.AveSiteInfo siteInfo, bool needCreateSite);
        void RestoreUnrestoredWebParts();
        void RestoreUnRestoreHoldRecord();
        void RestoreUnRestoreWebPart(IReport report);
        void RestoreUrlIDNeedReplace();
        void RestoreUrlNeedPost();
        void RestoreWebLastModifiedTime();
        void RestroeLanguageFile(AvePoint.Wrapper.Common.AveLanguageInfo languageInfo);
        AveRestoreGhostPageOption SaveBinaryForGhostPage { get; set; }
        void ScheduleDocument();
        string ServerRelativeUrl { get; }
        AvePoint.Wrapper.SPService.AveServiceContext ServiceContext { get; }
        void SetContentDBId(Guid id);
        void SetFieldFilter(System.Collections.Generic.HashSet<string> includeFields, System.Collections.Generic.HashSet<string> excludeFields, int mode);
        void SetLanguageForNew(uint LCD);
        void SetLanguageMapping(AvePoint.Wrapper.Common.AveLanguageProcesser languageMapping);
        bool SetLookupFieldSourceValue { get; }
        void SetLookupSourceValue(bool setSourceValue);
        void SetPlaceHolderAccount(string login);
        void SetSiteCreationAccount(string ownerlogin, AvePoint.Wrapper.Common.AveSiteInfo info);
        void SetTemplateMapping(System.Xml.XmlElement xe);
        void SetTimeoutForReloadSPRequest(int hours);
        void SetUseHostHeader(bool value);
        void SetUserMapping(System.Collections.Generic.Dictionary<string, string> userMapping, System.Collections.Generic.Dictionary<string, string> domainMapping, string defaultUser);
        void SetWebTemplate(Guid webId);
        void SetRestoreManagedMetadataNavigation(bool restoreMetadataNavigation);
        bool SiteReadOnly { get; }
        string SiteUrl { get; }
        long Size { get; }
        AvePoint.Wrapper.Common.AveSiteInfo SourceSiteInfo { get; set; }
        AvePoint.Wrapper.Common.AveSiteSettingInfo SourceSiteSettingInfo { get; set; }
        AvePoint.Wrapper.Common.AveContextKind SPContextKind { get; }
        IAveSPMembers SPMembers { get; }
        AvePoint.Wrapper.Common.IAveSite SPSite { get; set; }
        AvePoint.Wrapper.Common.IAveWebApplication SPWebApplication { get; set; }
        uint SrcLanguageId { get; }
        AvePoint.Wrapper.Common.IAveTemplateMapping TemplateMapping { get; }
        bool VerifyItemMMSColumnValue { get; set; }
        bool OverWriteNavigation { get; set; }
        bool CheckIisDirectory(AvePoint.Wrapper.Common.AveUrlZone zone = AvePoint.Wrapper.Common.AveUrlZone.Default);
        void RestoreUserCustomActions(System.Collections.Generic.List<AvePoint.Wrapper.Common.AveUserCustomActionInfo> customActions);
        void RestoreWorkflowStartOption();
    }

    public enum AveRestoreGhostPageOption
    {
        NoAction,
        KeepStreamOnly,
        KeepPathOnly,
        KeepStreamAndPath
    }
}
