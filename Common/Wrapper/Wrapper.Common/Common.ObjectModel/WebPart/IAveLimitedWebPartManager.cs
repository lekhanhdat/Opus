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
using System.Text;
using System.Collections;
using System.Collections.Concurrent;

namespace AvePoint.Wrapper.Common
{
    public class AveWebPartCache
    {
        public Dictionary<string, Dictionary<Guid, Guid>> WebPartMapping { get; set; }
        public AveViewDocInfo ViewInfo { get; set; }
        public Dictionary<Guid, Guid> ListIdMapping { get; set; }
        public Dictionary<Guid, Dictionary<Guid, Dictionary<string, List<object>>>> UnRestoreWebPartCache { get; set; }
        public Dictionary<string, string> SiteUrlMapping { get; set; }
        public Dictionary<string, string> WebUrlMapping { get; set; }
        public List<Dictionary<string, string>> SiteManagedMappings { get; set; }
        public Dictionary<Guid, Guid> NeedWebPartIDMapping { get; set; }
        public Dictionary<Guid, string> WebPartTypeIDMapping { get; set; }
        public Dictionary<Guid, Guid> WebIDMapping { get; set; }
        public string DefaultUser { get; set; }
        //public Dictionary<string, Dictionary<string, List<AveWebPartBaseInfo>>> unRestoreWebPartCacheInWeb { get; set; }
        public AveLanguageProcesser LanguageProcesser { get; set; }
        public Dictionary<string, string> AudienceIDMapping { get; set; }
        public Dictionary<int, object> SiteUserIDMapping { get; set; }
        public Dictionary<string, string> SiteUserNameMapping { get; set; }
        public Dictionary<string, IAveContentType> ListLevelCTMapping { get; set; }
        public Dictionary<string, IAveContentTypeId> ListLevelCTIdMapping { get; set; }
        public Dictionary<string, Dictionary<string, IAveContentTypeId>> DesListCTIdMapping { get; set; }
        public Dictionary<Guid, IAveFieldMapping> ListFieldsMapping { get; set; }
        public Dictionary<int, object> UserMapping { get; set; }
        public Dictionary<Guid, Guid> ViewGuidMapping { get; set; }
        public Dictionary<Guid, Dictionary<Guid, List<Guid>>> NeedResetCalendarSettingsViews { get; set; }
        public Dictionary<string, string> FieldInternalNameMapping { get; set; }
        public Dictionary<string, string> FieldDisplayNameMapping { get; set; }
        public Dictionary<string, string> ListUrlMapping { get; set; }
        public Dictionary<string, string> FullUrlMapping { get; set; }
        public AveSiteInfo SourceSiteInfo { get; set; }
        public AveSiteInfo DestSiteInfo { get; set; }
        public Dictionary<Guid, Guid> TermIdMapping { get; set; }
        public Dictionary<Guid, Dictionary<string, string>> ListCTIdMapping { get; set; }
        public ConcurrentDictionary<Guid, Guid> ProjectCustomFieldIdMapping { get; set; }
    }

    public interface IAveLimitedWebPartManager : IDisposable
    {
        /*
        [Obsolete("Use AddWebPart(IAveWebPart webPart, string zoneId, int zoneIndex) instead")]
        void AddWebPart(WebPart webPart, string zoneId, int zoneIndex);
        [Obsolete("Use CloseWebPart(IAveWebPart webPart) instead")]
        void CloseWebPart(WebPart webPart);
        [Obsolete("Use DeleteWebPart(IAveWebPart webPart) instead")]
        void DeleteWebPart(WebPart webPart);
        [Obsolete("Use MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex) instead")]
        void MoveWebPart(WebPart webPart, string zoneId, int zoneIndex);
        [Obsolete("Use MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex, bool isShared) instead")]
        void MoveWebPart(WebPart webPart, string zoneId, int zoneIndex, bool isShared);
        [Obsolete("Use SaveChanges(IAveWebPart webPart) instead")]
        void SaveChanges(WebPart webPart);
        [Obsolete("Use SaveChanges(IAveWebPart webPart, bool isShared) instead")]
        void SaveChanges(WebPart webPart, bool isShared);
        [Obsolete("Use OpenWebPart(IAveWebPart webPart) instead")]
        void OpenWebPart(WebPart webPart);
        [Obsolete("Use ResetPersonalizationState(IAveWebPart webPart) instead")]
        void ResetPersonalizationState(WebPart webPart);

        void AddWebPart(IAveWebPart webPart, string zoneId, int zoneIndex);
        void CloseWebPart(IAveWebPart webPart);
        void DeleteWebPart(IAveWebPart webPart);
        [Obsolete("Use MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex, bool isShared) insted")]
        void MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex);
        void MoveWebPart(IAveWebPart webPart, string zoneId, int zoneIndex, bool isShared);
        [Obsolete("Use SaveChanges(IAveWebPart webPart, bool isShared) insted")]
        void SaveChanges(IAveWebPart webPart);
        void SaveChanges(IAveWebPart webPart, bool isShared);
        void OpenWebPart(IAveWebPart webPart);
        void ResetPersonalizationState(IAveWebPart webPart);
        */

        List<AveWebPartBaseInfo> GetWebParts(AveBaseItemInfo info);
        void RestoreWebParts(IList webParts, bool clearAll);
        void RestoreWebParts(List<AveWebPartBaseInfo> webparts, bool post);
        void PostRestoreWebParts(List<AveWebPartBaseInfo> webparts);
        void UpdateWebParts(List<string> webparts);
        void SetRestoreReport(IReport report);
        void Dispose();
        IAveWebPart ImportAndAddWebPart(string webPartXml, string zoneId, int zoneIndex);

        IAveWeb Web { get; }
        IAveLimitedWebPartCollection WebParts { get; }

        AveWebPartCache Cache { set; }
        ///// <summary>
        ///// 
        ///// </summary>
        ///// <param name="webPartMapping"></param>
        ///// <param name="viewInfo"></param>
        ///// <param name="listIdMapping"></param>
        ///// <param name="unRestoreWebPartCache"></param>
        ///// <param name="siteUrlMapping"></param>
        ///// <param name="webUrlMapping"></param>
        ///// <param name="siteManagedMappings"></param>
        ///// <param name="needWebPartIDMapping"></param>
        ///// <param name="webPartTypeIDMapping"></param>
        ///// <param name="webIDMapping"></param>
        ///// <param name="defaultUser"></param>
        ///// <param name="unRestoreWebPartCacheInWeb"></param>
        ///// <param name="languageProcesser"></param>
        ///// <param name="audienceIDMapping"></param>
        ///// <param name="siteUserIDMapping"></param>
        ///// <param name="listLevelCTMapping"></param>
        ///// <param name="userMapping"></param>
        ///// <param name="viewGuidMapping"></param>
        //void Init(Dictionary<string, Dictionary<Guid, Guid>> webPartMapping, AveViewDocInfo viewInfo,
        //    Dictionary<Guid, Guid> listIdMapping, Dictionary<Guid, Dictionary<Guid, Dictionary<Guid, List<object>>>> unRestoreWebPartCache,
        //    Dictionary<string, string> siteUrlMapping, Dictionary<string, string> webUrlMapping, List<Dictionary<string, string>> siteManagedMappings,
        //   Dictionary<Guid, Guid> needWebPartIDMapping, Dictionary<Guid, string> webPartTypeIDMapping, Dictionary<Guid, Guid> webIDMapping, string defaultUser,
        //    Dictionary<string, Dictionary<string, List<AveWebPartBaseInfo>>> unRestoreWebPartCacheInWeb, AveLanguageProcesser languageProcesser,
        //    Dictionary<string, string> audienceIDMapping, Dictionary<int, object> siteUserIDMapping, Dictionary<string, IAveContentType> listLevelCTMapping,
        //    Dictionary<int, object> userMapping, Dictionary<Guid, Guid> viewGuidMapping, Dictionary<Guid, Dictionary<Guid, List<Guid>>> needResetCalendarSettingsViews,
        //    Dictionary<string, string> fieldInternalNameMapping);

        //WebPart CreateWebPartInstance(string assemblyName, string webPartType);

        //void UpdatePropertiesInDatabase(string webPartId, Guid siteId, Guid fileId, byte[] allUsersProperties, byte[] perUserProperties);

        //void UpdatePersonalPropertiesInDatabase(string webPartId, Guid siteId, int currentUserId, byte[] perUserBytes);

        //void UpdateUserID(string webPartId, Guid siteId, Guid fileId, int currentUserId, int userId, bool isPersonal);

        //void UpdateView(string webPartId, Guid siteId, Guid fileId, int baseViewId, byte[] view, byte[] contentTypeId);

        //void UpdateWebPartInfo(string webPartId, Guid siteId, Guid fileId, int pageVersion, byte oldLevel, byte newLevel, bool isCurrentVersion, int uIVersion);

        //void DeleteWebPartByNative(Guid siteId, Guid docId, string webPartId);
    }
}
