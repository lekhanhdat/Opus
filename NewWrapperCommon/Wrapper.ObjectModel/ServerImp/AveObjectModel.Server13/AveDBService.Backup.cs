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
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Globalization;
using System.IO;
using System.Text;
using System.Xml;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;

namespace AvePoint.ObjectModel.Server13
{
    internal class AveDBService
    {

    }


    /// <summary>
    /// Modifier:Mint
    /// To avoid initializing the same DB Connection for multiple times
    /// We can't invoke it in during multiple-threads
    /// </summary>
    internal partial class AveDBQueryService : AveDBServiceBase, IDisposable
    {
        internal List<AveUserInfo> GetSiteUsers(SPSite site, bool allAvailableUser)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetSiteUsers"))
            {
#endif
                string cmdText = string.Empty;
                if (!allAvailableUser)
                {
                    cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
FROM UserInfo 
WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
                AND ( tp_ID in (SELECT DISTINCT(tp_ID) FROM UserInfo WHERE tp_ID in (
                       SELECT PrincipalId FROM RoleAssignment 
                       WHERE SiteId=@SiteId And PrincipalId not in (SELECT Id FROM Groups WHERE Siteid=@SiteId))
                UNION
                SELECT Distinct(MemberId) FROM GroupMembership 
                       WHERE SiteId=@SiteId
                        AND GroupId in(
                             SELECT Id FROM Groups WHERE Id in
                             (SELECT PrincipalId FROM Roleassignment WHERE  SiteId=@SiteId)))
                      OR tp_SiteAdmin = 1)
Order by tp_ID";
                }
                else
                {
                    cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
FROM UserInfo 
WHERE tp_SiteID=@SiteId 
Order by tp_ID";
                }
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", site.ID);

                List<AveUserInfo> list = AveSqlUtility.GetDBRows<AveUserInfo>(SqlConn, cmdText, "tp_");

                return list;
#if PerformanceLog
            }
#endif
        }

        //
        // Summary:
        //     Gets the collection of AveUserInfo objects that all the users are explicitly assigned permissions
        //     in the Web site.
        //
        // Returns:
        //     An List<AveUserInfo> object that represents the users.
        internal List<AveUserInfo> GetWebUsers(SPWeb web, bool allAvailableUser)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebUsers"))
            {
#endif
                if (!web.HasUniqueRoleAssignments)
                {
                    return null;
                }

                string cmdText = string.Empty;
                if (!allAvailableUser)
                {
                    cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
   tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
   tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags
FROM UserInfo WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
                AND ( tp_Id in(
                SELECT distinct(tp_id) FROM UserInfo WHERE tp_id in(
                       SELECT principalId FROM RoleAssignment 
                       WHERE Scopeid=@ScopeId And SiteId=@SiteId And PrincipalId not in(SELECT Id FROM Groups WHERE Siteid=@SiteId))
                UNION
                SELECT Distinct(MemberId) FROM GroupMembership 
                       WHERE SiteId=@SiteId AND GroupId in(
                             SELECT Id FROM Groups WHERE Id in(SELECT PrincipalId FROM Roleassignment WHERE Scopeid=@ScopeId and SiteId=@SiteId)))
                OR tp_SiteAdmin = 1)
ORDER BY tp_Id";
                }
                else
                {
                    cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
FROM UserInfo 
WHERE tp_SiteID=@SiteId 
Order by tp_ID";
                }
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", web.Site.ID);
                SqlConn.AddParameter("@ScopeId", web.RoleAssignments.Id.ToString());

                List<AveUserInfo> list = AveSqlUtility.GetDBRows<AveUserInfo>(SqlConn, cmdText, "tp_");

                return list;
#if PerformanceLog
            }
#endif
        }

        internal List<AveGroupInfo> GetGroups(SPWeb web, bool allGroups)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetGroups"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", web.Site.ID);
                string cmdText = string.Empty;
                if (allGroups)
                {
                    cmdText = @"
SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags 
From Groups WHERE SiteId=@SiteId ORDER BY ID";
                }
                else
                {
                    cmdText = @"
SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags 
From Groups WHERE SiteId=@SiteId AND ID in(SELECT Id FROM Groups WHERE Id in
(SELECT PrincipalId FROM Roleassignment WHERE  SiteId=@SiteId)) ORDER BY ID";
                }
                List<AveGroupInfo> groupInfos = AveSqlUtility.GetDBRows<AveGroupInfo>(SqlConn, cmdText);

                if (groupInfos == null || groupInfos.Count == 0)
                {
                    return groupInfos;
                }
                groupInfos.Sort();
                cmdText = "SELECT GroupId,MemberId From GroupMembership WHERE SiteId=@SiteId ORDER BY GroupId,MemberId";

                int groupIndex = 0;
                int badGroupId = -1;
                AveGroupInfo group = groupInfos[groupIndex];
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    int groupId;
                    int memberId;
                    while (dr.Read())
                    {
                        groupId = dr.GetInt32(0);
                        memberId = dr.GetInt32(1);
                        if (badGroupId == groupId)
                        {
                            continue;
                        }
                        if (groupId != group.ID)
                        {
                            int i = groupIndex + 1;
                            while (i < groupInfos.Count)
                            {
                                if (groupInfos[i].ID == groupId)
                                {
                                    groupIndex = i;
                                    break;
                                }
                                ++i;
                            }
                            if (i == groupInfos.Count)
                            {
                                badGroupId = groupId;
                                continue;
                            }
                            else
                            {
                                group = groupInfos[i];
                                groupIndex = i;
                                badGroupId = -1;
                            }
                        }
                        if (group.Memberships == null)
                        {
                            group.Memberships = new List<int>();
                        }
                        group.Memberships.Add(memberId);
                    }
                }
                return groupInfos;
#if PerformanceLog
            }
#endif
        }

        internal AveSiteSettingInfo GetSiteSettingFromSites(SPSite site)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetSiteSettingFromSites"))
            {
#endif
                AveSiteSettingInfo info = new AveSiteSettingInfo();

                string cmdText = @"
SELECT Id,NextUserOrGroupId,OwnerID,SecondaryContactID,Subscribed,TimeCreated,UsersCount,
       BWUsed,DiskUsed,SecondStageDiskUsed,QuotaTemplateID,DiskQuota,UserQuota,DiskWarning,DiskWarned,
       CurrentResourceUsage,AverageResourceUsage,ResourceUsageWarning,ResourceUsageMaximum,BitFlags,
       SecurityVersion,CertificationDate,DeadWebNotifyCount,PortalURL,PortalName,LastContentChange,
       LastSecurityChange,AuditFlags,InheritAuditFlags,UserInfoListId,UserIsActiveFieldRowOrdinal,
       UserIsActiveFieldColumnName,UserAccountDirectoryPath,RootWebId,HashKey,DomainGroupMapVersion,
       DomainGroupMapCacheVersion,DomainGroupMapCache,HostHeader,SubscriptionId
FROM Sites WHERE Id=@SiteId";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", site.ID);
                AveSqlUtility.GetDBRow(info, SqlConn, cmdText);

                return info;
#if PerformanceLog
            }
#endif
        }

        internal long GetSiteSizeFromSites(SPSite site)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetSiteSizeFromSites"))
            {
#endif
                long siteSize = 0;
                string cmdText = @"SELECT DiskUsed FROM Sites WITH(NOLOCK) WHERE Id=@SiteId";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", site.ID);
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        siteSize = dr.GetInt64(0);
                    }
                }
                return siteSize;
#if PerformanceLog
            }
#endif
        }

        internal AveSiteSettingInfo GetFullSiteSetting(SPSite site)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetFullSiteSetting"))
            {
#endif
                AveSiteSettingInfo siteSettingInfo = new AveSiteSettingInfo();
                string cmdText = string.Empty;
                try
                {
                    cmdText = @"SELECT SolutionId FROM Solutions WHERE SiteId = @SiteId";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", site.ID);
                    using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                    {
                        if (siteSettingInfo.SolutionIdCollection == null)
                        {
                            siteSettingInfo.SolutionIdCollection = new List<Guid>();
                        }
                        while (dr.Read())
                        {
                            siteSettingInfo.SolutionIdCollection.Value.Add(dr.GetGuid(0));
                        }
                    }
                }
                catch (Exception e)
                {
                    //log
                }

                cmdText = @"
SELECT Id,NextUserOrGroupId,OwnerID,SecondaryContactID,Subscribed,
       TimeCreated,UsersCount,BWUsed,DiskUsed,SecondStageDiskUsed,
       QuotaTemplateID,DiskQuota,UserQuota,DiskWarning,DiskWarned,
       CurrentResourceUsage,AverageResourceUsage,ResourceUsageWarning,ResourceUsageMaximum,BitFlags,
       SecurityVersion,CertificationDate,DeadWebNotifyCount,PortalURL,PortalName,
       LastContentChange,LastSecurityChange,AuditFlags,InheritAuditFlags,UserInfoListId,
       UserIsActiveFieldRowOrdinal,UserIsActiveFieldColumnName,UserAccountDirectoryPath,RootWebId,HashKey,
       DomainGroupMapVersion,DomainGroupMapCacheVersion,DomainGroupMapCache,HostHeader,SubscriptionId
FROM Sites WHERE Id=@SiteId";

                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        int BitFlags = dr.GetInt32(19);
                        siteSettingInfo.SyndicationEnabled = Ave2010SiteFlags.SyndicationEnabled(BitFlags);
                        if (!dr.IsDBNull(27))
                        {
                            siteSettingInfo.AuditFlags = dr.GetInt32(27);
                        }
                        else
                        {
                            siteSettingInfo.AuditFlags = null;
                        }
                        siteSettingInfo.UseAuditFlagCache = site.Audit.UseAuditFlagCache;
                        siteSettingInfo.TrimAuditLog = Ave2010SiteFlags.TrimAuditLog(BitFlags);
                        AveSqlUtility.GetDBRow(siteSettingInfo, dr, AveSqlUtility.GetFieldMap(typeof(AveSiteSettingInfo), string.Empty), 0);
                    }
                }

                string cmdString = @"SELECT MetaInfo FROM Webs WHERE Id = @Id";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@Id", site.RootWeb.ID);
                byte[] buffer = (byte[])SqlConn.ExecuteScalar(cmdString);

                string metaInfo = AveCompressedUtility.GetTCompressedString(buffer);
                Dictionary<string, string> ProInMetaInfo = AveCompressedUtility.GetMetaInfoDictionary(metaInfo);
                if (ProInMetaInfo.ContainsKey("_auditlogtrimmingretention"))
                {
                    siteSettingInfo.AuditLogTrimmingRetention = Int32.Parse(ProInMetaInfo["_auditlogtrimmingretention:SW"]);
                }
                else
                {
                    siteSettingInfo.AuditLogTrimmingRetention = 0;
                }
                if (ProInMetaInfo.ContainsKey("_auditlogtrimmingcallout"))
                {
                    siteSettingInfo.AuditLogTrimmingCallout = ProInMetaInfo["_auditlogtrimmingcallout:SW"];
                }
                else
                {
                    siteSettingInfo.AuditLogTrimmingCallout = "";
                }
                if (ProInMetaInfo.ContainsKey("allowdesigner"))
                {
                    siteSettingInfo.AllowDesigner = Int32.Parse(ProInMetaInfo["allowdesigner:SW"]) == 0 ? false : true;
                }
                else
                {
                    siteSettingInfo.AllowDesigner = true;
                }
                if (ProInMetaInfo.ContainsKey("allowmasterpageediting"))
                {
                    siteSettingInfo.AllowMasterPageEditing = Int32.Parse(ProInMetaInfo["allowmasterpageediting:SW"]) == 0 ? false : true;
                }
                else
                {
                    siteSettingInfo.AllowMasterPageEditing = false;
                }
                if (ProInMetaInfo.ContainsKey("allowrevertfromtemplate"))
                {
                    siteSettingInfo.AllowRevertFromTemplate = Int32.Parse(ProInMetaInfo["allowrevertfromtemplate:SW"]) == 0 ? false : true;
                }
                else
                {
                    siteSettingInfo.AllowRevertFromTemplate = false;
                }

                return siteSettingInfo;
#if PerformanceLog
            }
#endif
        }

        internal string GetLeafNameFromAllDocs(string cmdText, Dictionary<string, object> parameters)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetLeafNameFromAllDocs"))
            {
#endif
                string leafName = string.Empty;

                SqlConn.ClearParameters();

                foreach (string key in parameters.Keys)
                {
                    SqlConn.AddParameter(key, parameters[key]);
                }

                leafName = (string)SqlConn.ExecuteScalar(cmdText);

                return leafName;
#if PerformanceLog
            }
#endif
        }

        internal AveWebSettingInfo GetWebSettingFromWebs(SPWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebSettingFromWebs"))
            {
#endif
                AveWebSettingInfo info = new AveWebSettingInfo();

                string cmdText = @"
SELECT Author, Title, TimeCreated, Description, SecurityProvider, MetaInfo, MetaInfoVersion, LastMetadataChange, NavStructNextEid, 
       NextWebGroupId, DefTheme, AlternateCSSUrl, CustomizedCss, CustomJSUrl, AlternateHeaderUrl, DailyUsageData, DailyUsageDataVersion, 
       MonthlyUsageData, MonthlyUsageDataVersion, DayLastAccessed, Language, Locale, TimeZone, Time24, CalendarType, AdjustHijriDays, 
       ProvisionConfig, Flags,MasterUrl,CustomMasterUrl, Collation, RequestAccessEmail, SiteLogoUrl, SiteLogoDescription, AuditFlags, 
       InheritAuditFlags, Ancestry, AltCalendarType, CalendarViewOptions, WorkDayStartHour, WorkDayEndHour,WorkDays 
FROM Webs WHERE Id=@WebId";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@WebId", web.ID);
                AveSqlUtility.GetDBRow(info, SqlConn, cmdText);

                return info;
#if PerformanceLog
            }
#endif
        }

        internal long GetWebSize(SPWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebSize"))
            {
#endif
                long webSize = 0;
                string cmdText =
    @"SELECT SUM(PartSize), SiteSize.WebId
FROM
(
	SELECT PartSize, WebId FROM
	(
		SELECT ISNULL
		(
			SUM
			(
				CAST
					(ISNULL(Size, 0) AS BIGINT) +
				CAST
					(ISNULL(MetaInfoSize, 0) AS BIGINT) +
				CAST
					(FileFormatMetaInfoSize AS BIGINT) +
				CAST
					(ISNULL(UnVersionedMetaInfoSize,0) AS BIGINT) +
				CAST
					(152 AS BIGINT)
			), 0
		) AS PartSize, WebId
		FROM
		(                
			SELECT
				WebId, Size, MetaInfoSize, FileFormatMetaInfoSize, UnVersionedMetaInfoSize
			FROM
				AllDocs WITH (NOLOCK, INDEX=AllDocs_ParentId)
			WHERE
				SiteId = @SiteId AND
				DeleteTransactionId = 0x
		) AS AD GROUP BY WebId
	) Docs_NoLock_Site
	UNION ALL
	SELECT PartSize, WebId FROM
	(
		SELECT (ISNULL((SUM(CAST((ISNULL(AFF.BlobSize, 0)) AS BIGINT))),0)) AS PartSize, WebId
		FROM
		(
			SELECT
				*
			FROM
				AllDocs WITH (NOLOCK, INDEX=AllDocs_ParentId)
			WHERE
				SiteId = @SiteId AND
				DeleteTransactionId = 0x
		) AS AD 
		CROSS APPLY
		(
			SELECT
				*
			FROM
				AllFileFragments WITH (NOLOCK, INDEX=AllFileFragments_PartId_UCI)
			WHERE
					DocId = AD.Id
		) AS AFF GROUP BY WebId
	)AllFileFragments_NoLock_DocId
	UNION ALL
	SELECT WL.PartSize, WL.WebId FROM 
	(
		SELECT (ISNULL((SUM(CAST((ISNULL(DATALENGTH(L.tp_ContentTypes), 0) + ISNULL(DATALENGTH(L.tp_Fields), 0)) AS BIGINT))),0)) AS PartSize, L.tp_WebId AS WebId
		FROM
		(
			SELECT
				*
			FROM
				Webs WITH (NOLOCK, INDEX=Webs_SiteIdParent)
			WHERE
				SiteId = @SiteId
		) AS W
		CROSS APPLY
		(
			SELECT
				*
			FROM
				AllLists WITH (NOLOCK, INDEX=AllLists_PK)
			WHERE
				tp_WebId = W.Id AND
				tp_DeleteTransactionId = 0x
		) AS L GROUP BY tp_WebId
	)WL
	UNION ALL
	SELECT UD_AL.PartSize, UD_AL.WebId FROM
	(
		SELECT (ISNULL((SUM(CAST((ISNULL(tp_Size, 0)) AS BIGINT))),0)) AS PartSize, tp_WebId AS WebId
		FROM
		(
			SELECT
				AUD.tp_Size, AL.tp_WebId
			FROM
				AllUserData AUD WITH (NOLOCK, INDEX=AllUserData_ParentId), AllLists AL WITH(NOLOCK)
			WHERE
				AUD.tp_SiteId = @SiteId AND
				AUD.tp_DeleteTransactionId = 0x AND
				(AUD.tp_IsCurrentVersion = CONVERT(BIT, 0) OR AUD.tp_IsCurrentVersion = CONVERT(BIT, 1)) AND
				AUD.tp_ListId = AL.tp_ID
		) AS UD GROUP BY tp_WebId
	)UD_AL
	UNION ALL
    SELECT AWP_AL.PartSize, AWP_AL.WebId FROM
	(
		SELECT (ISNULL((SUM(CAST((ISNULL(AWP.tp_Size, 0)) AS BIGINT))),0)) AS PartSize, AWP.WebId
		FROM
		(
			SELECT
				AW_T.tp_Deleted, AW_T.WebId, AW_T.tp_Size, AW_T.tp_ListId, AW_T.tp_SiteId
			FROM
			(
				SELECT AD.WebId, tp_Deleted, tp_Size, tp_ListId, tp_SiteId 
				FROM
					AllWebParts AW WITH (INDEX=PageUrlID_FK), AllDocs AD WITH(NOLOCK)
				WHERE
					AW.tp_PageUrlID = AD.Id AND
					AW.tp_SiteId = @SiteId AND 
					AW.tp_Deleted = CONVERT(BIT, 0)
			) AW_T
		) AS AWP
		WHERE AWP.tp_Deleted = CONVERT(BIT, 0) GROUP BY AWP.WebId		
	)AWP_AL
	UNION ALL
	SELECT P_AL.PartSize, P_AL.WebId FROM 
	(
		SELECT (ISNULL((SUM(CAST((ISNULL(P.tp_Size, 0)) AS BIGINT))),0)) AS PartSize, tp_WebId AS WebId
		FROM
		(
			SELECT
				PL.tp_Size, PL.tp_Deleted, AW.tp_ListId, AL.tp_WebId
			FROM
				Personalization AS PL WITH (NOLOCK, INDEX=Personalization_PK), AllWebParts AS AW WITH (INDEX=PageUrlID_FK), AllLists AL WITH(NOLOCK)
			WHERE
				PL.tp_SiteId = @SiteId AND
				PL.tp_WebPartID = Aw.tp_ID AND
				AW.tp_ListId = AL.tp_ID
		) AS P
		WHERE P.tp_Deleted = CONVERT(BIT,0) GROUP BY tp_WebId
	)P_AL
	UNION ALL
	SELECT Cmd.PartSize, Cmd.WebId FROM
	(
		SELECT (ISNULL((SUM(CAST((ISNULL(CM.Size, 0)) AS BIGINT))),0)) AS PartSize, AD.WebId AS WebId
		FROM ComMd CM, AllDocs AD
		WHERE CM.SiteId = @SiteId and DocId=AD.Id GROUP BY WebId
	)Cmd
	UNION ALL
	SELECT CT_Data.PartSize, CT_Data.WebId FROM
	(
		SELECT (ISNULL((SUM(CAST((ISNULL(CT_T.Size, 0)) AS BIGINT))),0)) AS PartSize, CT_T.Id AS WebId
		FROM
		(
			SELECT
				CT.Size, Web.Id
			FROM
				ContentTypes CT WITH (NOLOCK, INDEX=ContentTypes_SiteClassCTId), Webs Web WITH(NOLOCK) 
			WHERE
				CT.SiteId = @SiteId and (Web.FullUrl=CT.Scope or CT.Scope='') and Web.SiteId = @SiteId
		) AS CT_T GROUP BY CT_T.Id		
	) CT_Data
	UNION ALL
	SELECT R_Data.PartSize, R_Data.WebId FROM
	(
		SELECT ISNULL(SUM(R.Size),0) AS PartSize, WebId
		FROM
		(
			SELECT
				*
			FROM
				RecycleBin WITH (NOLOCK, INDEX=RecycleBin_SiteBinWebUser)
			WHERE
				SiteId = @SiteId AND
				BinId = 1
		) AS R GROUP BY WebId
	) R_Data
	UNION ALL
	SELECT PartSize, WebId FROM
	(
		SELECT (ISNULL((SUM(CAST((ISNULL(Size, 0) + ISNULL(MetaInfoSize, 0)) AS BIGINT))),0)) AS PartSize, WebId
		FROM
		(
			SELECT
				ADV.Size, ADV.MetaInfoSize, AD.WebId
			FROM
				AllDocVersions ADV WITH (NOLOCK, INDEX=AllDocVersions_PK), AllDocs AD
			WHERE
				ADV.SiteId = @SiteId AND
				ADV.DeleteTransactionId = 0x AND
				ADV.Id = AD.Id
		) AS ADV_AD
		GROUP BY WebId
	)ADV_AD_Data
) SiteSize
GROUP BY SiteSize.WebId";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", web.Site.ID);
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        if (!dr.IsDBNull(1) && dr.GetGuid(1) == web.ID)
                        {
                            webSize = dr.GetInt64(0);
                        }
                    }
                }
                return webSize;
#if PerformanceLog
            }
#endif
        }

        internal AveListInfo GetListInfo(SPList list)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetListInfo"))
            {
#endif
                AveListInfo listInfo = new AveListInfo();
                if (list == null)//when {System Folder}, the list is null
                {
                    listInfo.Title = AveConstants.SYSTEM_FOLDER;
                    return listInfo;
                }
                try
                {
                    SPWeb ParentWeb = list.ParentWeb;

                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@ListId", list.ID);
                    SqlConn.AddParameter("@WebId", ParentWeb.ID);
                    string cmdText = @"SELECT tp_Title, tp_Created, tp_LastSecurityChange, tp_Version, tp_Author, 
                                           tp_BaseType, tp_FeatureId, tp_ServerTemplate, tp_Template, tp_ImageUrl, 
                                           tp_ReadSecurity, tp_WriteSecurity, tp_Subscribed, tp_Direction, tp_Flags, 
                                           tp_ThumbnailSize, tp_WebImageWidth, tp_WebImageHeight, tp_Description, tp_EmailAlias, 
                                           tp_ScopeId, tp_HasFGP, tp_HasInternalFGP, tp_EventSinkAssembly, tp_EventSinkClass, 
                                           tp_EventSinkData, tp_MaxRowOrdinal, tp_Fields, tp_ContentTypes, tp_AuditFlags, 
                                           tp_InheritAuditFlags, tp_SendToLocation, tp_ListDataDirty, tp_CacheParseId, tp_MaxMajorVersionCount, 
                                           tp_MaxMajorwithMinorVersionCount, tp_DefaultWorkflowId, tp_NoThrottleListOperations,tp_ListSchemaVersion,tp_ID
                                    FROM AllLists 
                                    WHERE tp_WebId=@WebId and tp_Id=@ListId";

                    using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            ulong flags = (ulong)dr.GetInt64(14);
                            //AllLists.tp_ServerTemplate
                            listInfo.BaseTemplate = dr.GetInt32(7);
                            //listInfo.BaseTemplate = (int)list.BaseTemplate;
                            //AllLists.tp_FeatureId                
                            listInfo.TemplateFeatureId = dr.IsDBNull(6) ? Guid.Empty : dr.GetGuid(6);
                            //listInfo.TemplateFeatureId = list.TemplateFeatureId;
                            //AllLists.tp_Title
                            listInfo.Title = dr.IsDBNull(0) ? string.Empty : dr.GetString(0);
                            //AllLists.tp_Description
                            listInfo.Description = dr.IsDBNull(18) ? string.Empty : dr.GetString(18);
                            //AllLists.tp_ID
                            listInfo.Id = dr.GetGuid(39);
                            string url = list.RootFolder.ServerRelativeUrl.Substring(ParentWeb.RootFolder.ServerRelativeUrl.Length).Trim('/');
                            listInfo.Url = ParentWeb.Url.TrimEnd('/') + "/" + url;
                            listInfo.ServerRelativeUrl = list.RootFolder.ServerRelativeUrl;
                            if (list.BaseTemplate == SPListTemplateType.ExternalList)
                            {
                                if (list.HasExternalDataSource)
                                {
                                    listInfo.DataSourceXml = (string)AveAssemblyUtility.InvokeMethod(list.DataSource, list.DataSource.GetType(), "ToXml", new object[] { });
                                }
                            }
                            listInfo.RootWebOnly = Ave2010ListFlags.RootWebOnly(flags);
                        }
                    }

                }

                catch (Exception e)
                {
                    //mLog.Log(AveLogLevel.ERROR, "WP10BKListInf168", listInfo.Id, listInfo.Title, e);
                    throw;
                }
                return listInfo;
#if PerformanceLog
            }
#endif
        }

        internal List<AveRoleAssignmentInfo> GetListRoleAssignments(string SiteId, string ScopeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetListRoleAssignments"))
            {
#endif
                string cmdText = "SELECT RoleId,PrincipalId FROM RoleAssignment WHERE SiteId=@SiteId AND ScopeId=@ScopeId order by PrincipalId ASC";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", SiteId);
                SqlConn.AddParameter("@ScopeId", ScopeId);
                return AveSqlUtility.GetDBRows<AveRoleAssignmentInfo>(SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }

        internal List<Dictionary<string, object>> GetUserData(AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetUserData"))
            {
#endif
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();

                string cmdText =
              @"SELECT tp_ID,tp_RowOrdinal,tp_Version,tp_Author,tp_Editor,tp_Modified,tp_Created,tp_Ordering,tp_ThreadIndex,
         tp_HasAttachment,tp_ModerationStatus,tp_IsCurrent,tp_ItemOrder,tp_InstanceID,tp_GUID,tp_CopySource,
         tp_HasCopyDestinations,tp_AuditFlags,tp_InheritAuditFlags,tp_Size,tp_WorkflowVersion,tp_WorkflowInstanceID,
         tp_ContentTypeId,nvarchar1,nvarchar2,nvarchar3,nvarchar4,nvarchar5,nvarchar6,nvarchar7,nvarchar8,
         ntext1,ntext2,ntext3,ntext4,sql_variant1,nvarchar9,nvarchar10,nvarchar11,nvarchar12,nvarchar13,
         nvarchar14,nvarchar15,nvarchar16,ntext5,ntext6,ntext7,ntext8,sql_variant2,nvarchar17,nvarchar18,
         nvarchar19,nvarchar20,nvarchar21,nvarchar22,nvarchar23,nvarchar24,ntext9,ntext10,ntext11,ntext12,
         sql_variant3,nvarchar25,nvarchar26,nvarchar27,nvarchar28,nvarchar29,nvarchar30,nvarchar31,nvarchar32,
         ntext13,ntext14,ntext15,ntext16,sql_variant4,nvarchar33,nvarchar34,nvarchar35,nvarchar36,nvarchar37,
         nvarchar38,nvarchar39,nvarchar40,ntext17,ntext18,ntext19,ntext20,sql_variant5,nvarchar41,nvarchar42,
         nvarchar43,nvarchar44,nvarchar45,nvarchar46,nvarchar47,nvarchar48,ntext21,ntext22,ntext23,ntext24,
         sql_variant6,nvarchar49,nvarchar50,nvarchar51,nvarchar52,nvarchar53,nvarchar54,nvarchar55,nvarchar56,
         ntext25,ntext26,ntext27,ntext28,sql_variant7,nvarchar57,nvarchar58,nvarchar59,nvarchar60,nvarchar61,
         nvarchar62,nvarchar63,nvarchar64,ntext29,ntext30,ntext31,ntext32,sql_variant8,int1,int2,int3,int4,int5,
         int6,int7,int8,int9,int10,int11,int12,int13,int14,int15,int16,float1,float2,float3,float4,float5,float6,
         float7,float8,float9,float10,float11,float12,datetime1,datetime2,datetime3,datetime4,datetime5,datetime6,
         datetime7,datetime8,bit1,bit2,bit3,bit4,bit5,bit6,bit7,bit8,bit9,bit10,bit11,bit12,bit13,bit14,bit15,bit16,
         uniqueidentifier1,tp_Level,tp_IsCurrentVersion,tp_UIVersion,tp_CalculatedVersion,tp_DraftOwnerId,tp_CheckoutUserId
FROM  AllUserData
WHERE tp_SiteId=@SiteId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 or tp_IsCurrentVersion=1) AND
tp_ParentId = @ParentId AND tp_DocId = @DocId AND tp_UIVersion = @Version";

                //WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND (tp_IsCurrentVersion=0 OR tp_IsCurrentVersion=1) AND tp_ID=@RowId 
                //AND (tp_CalculatedVersion = 0 OR tp_CalculatedVersion =@Version) AND (tp_Level = 1 OR tp_Level =2 OR  tp_Level =255 ) AND tp_UIVersion = @Version";

                SqlConn.ClearParameters();
                //SqlConn.AddParameter("@ListId", info.ListId);
                //SqlConn.AddParameter("@RowId", info.RowId);
                SqlConn.AddParameter("@Version", info.Version);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@ParentId", info.ParentId);
                SqlConn.AddParameter("@DocId", info.GUID);
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        Dictionary<string, object> tempData = new Dictionary<string, object>();
                        AveSqlUtility.GetDBRow(tempData, dr);
                        data.Add(tempData);
                    }
                }

                return data;
#if PerformanceLog
            }
#endif
        }

        internal int GetParentIdByThreadIndex(Guid siteId, Guid listId, byte[] threadIndex)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetParentIdByThreadIndex"))
            {
#endif
                int parentId = 0;
                string cmdText = @"select tp_ID from AllUserData where tp_SiteId=@SiteId AND tp_ThreadIndex =@ThreadIndex and tp_ListId=@ListId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ListId", listId);
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ThreadIndex", threadIndex);
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        parentId = dr.GetInt32(0);
                        break;
                    }
                }
                return parentId;
#if PerformanceLog
            }
#endif
        }

        internal List<AveRoleAssignmentInfo> GetWebRoleAssignments(Guid SiteId, Guid ScopeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebRoleAssignments"))
            {
#endif
                string cmdText = "SELECT RoleId,PrincipalId FROM RoleAssignment WHERE SiteId=@SiteId AND ScopeId=@ScopeId order by PrincipalId ASC";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", SiteId);
                SqlConn.AddParameter("@ScopeId", ScopeId);
                return AveSqlUtility.GetDBRows<AveRoleAssignmentInfo>(SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }
        //internal   object[,] ExecuteQuery(string cmdText, Dictionary<string, object> param)
        //{
        //    SqlConn.ClearParameters();

        //    foreach (string key in param.Keys)
        //    {
        //        SqlConn.AddParameter(key, param[key]);
        //    }


        //}
        internal bool GetFieldCollectionRelationship(string siteId, string listId, string fieldId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetFieldCollectionRelationship"))
            {
#endif
                string text = "SELECT * FROM AllLookupRelationships WHERE SiteId=@SiteId AND ListId=@ListId AND FieldId = @FieldId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ListId", listId);
                SqlConn.AddParameter("@FieldId", fieldId);

                using (SqlDataReader reader = SqlConn.ExecuteReader(text))
                {
                    return reader.HasRows;
                }
#if PerformanceLog
            }
#endif
        }
        internal string GetListViewSchema(Guid siteId, Guid listId)
        {
            string viewFieldsSchema = null;
            try
            {
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ListId", listId);
                string cmdText = @"select tp_View from AllWebParts where tp_SiteId=@SiteId and tp_ListId=@ListId and tp_Type=0";
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte[] bytes = dr["tp_View"] as byte[];
                        if (bytes != null && bytes.Length > 0)
                        {
                            viewFieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                //mLog.Log(AveLogLevel.WARN, "WP10BKAveSPFC797", siteId, listId, e);
            }
            return viewFieldsSchema;
        }

        internal List<AveRoleInfo> GetWebRoles(Guid siteId, Guid FirstUniqueRoleDefinitionWebId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebRoles"))
            {
#endif
                string cmdText = @"
                    SELECT RoleId,Title,Description,PermMask,PermMaskDeny,Hidden,Type,WebGroupId,RoleOrder 
                    FROM Roles WHERE SiteId=@SiteId and WebId=@WebId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@WebId", FirstUniqueRoleDefinitionWebId);
                return AveSqlUtility.GetDBRows<AveRoleInfo>(SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }

        internal List<AveRoleAssignmentInfo> GetItemRoleAssignments(Guid siteId, Guid itemScopeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetItemRoleAssignments"))
            {
#endif
                string cmdText = "SELECT RoleId,PrincipalId FROM RoleAssignment WHERE SiteId=@SiteId AND ScopeId=@ScopeId order by PrincipalId ASC";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ScopeId", itemScopeId);
                return AveSqlUtility.GetDBRows<AveRoleAssignmentInfo>(SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }

        internal List<AveContentTypeFileInfo> GetContentTypeCollectionResources(Guid siteId, string folderUrl)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetContentTypeCollectionResources"))
            {
#endif
                List<AveContentTypeFileInfo> ResourceFolderFiles = new List<AveContentTypeFileInfo>();
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@DirName", folderUrl.TrimStart('/'));
                string cmdText = @"select Content,LeafName from AllDocStreams,AllDocs where
                        AllDocs.SiteId=@SiteId and DirName=@DirName and AllDocs.Id = AllDocStreams.Id";
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        byte[] content = dr["Content"] as byte[];
                        string url = folderUrl + "/" + dr["LeafName"] as string;
                        ResourceFolderFiles.Add(new AveContentTypeFileInfo(url, content));
                    }
                }

                return ResourceFolderFiles;
#if PerformanceLog
            }
#endif
        }

        internal string GetContentTypeName(Guid siteId, byte[] contentTypeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetContentTypeName"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ContentTypeId", contentTypeId);
                string cmdText = @"select ResourceDir,Definition from ContentTypes
                               where SiteId=@SiteId and Class=1 and ContentTypeId=@ContentTypeId";
                string name = null;
                string definition = string.Empty;
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        name = dr["ResourceDir"] as string;
                        try
                        {
                            if (!dr.IsDBNull(1))
                            {
                                definition = dr["Definition"] as string;
                                XmlDocument xDoc = new XmlDocument();
                                xDoc.InnerXml = definition;
                                XmlElement root = (XmlElement)xDoc.ChildNodes[0];
                                if (root.HasAttribute("Name"))
                                {
                                    name = root.Attributes["Name"].Value;//使用xml中的Name作为ContentType的真实名字
                                }
                            }
                        }
                        catch (Exception e)
                        {
                            //mLog.Warn("Get ContentType realName error, Exception:{0}", e.ToString());
                        }
                        break;
                    }
                }
                return name;
#if PerformanceLog
            }
#endif
        }

        internal List<AveWebPartBaseInfo> GetWebParts(Guid siteId, Guid itemId, byte itemlevel, bool itemIsVersion, int version)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebParts"))
            {
#endif
                string cmdText =
                   @"SELECT wp.tp_ID,wp.tp_ListId,wp.tp_Type,wp.tp_Flags,wp.tp_BaseViewID,wp.tp_DisplayName,wp.tp_Version,wp.tp_PartOrder,wp.tp_ZoneID,
                         wp.tp_IsIncluded,wp.tp_FrameState,wp.tp_View,wp.tp_WebPartTypeId,wp.tp_AllUsersProperties,wp.tp_PerUserProperties,
                         wp.tp_Cache,wp.tp_UserID,wp.tp_Source,wp.tp_CreationTime,wp.tp_Size,wp.tp_Level,wp.tp_Deleted,wp.tp_HasFGP,
                         wp.tp_ContentTypeId,wp.tp_PageVersion,wp.tp_SolutionId,wp.tp_IsCurrentVersion,wp.tp_Assembly,wp.tp_Class,wp.tp_WebPartIdProperty,l.tp_Title AS tp_ListTitle
                FROM AllWebParts wp LEFT JOIN AllLists l 
                ON wp.tp_ListId=l.tp_Id WHERE wp.tp_SiteId=@SiteId AND wp.tp_PageUrlId=@Id AND wp.tp_Level=@Level AND wp.tp_PageVersion=@PageVersion order by wp.tp_PartOrder ASC";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@Id", itemId);
                SqlConn.AddParameter("@Level", itemlevel);
                SqlConn.AddParameter("@PageVersion", itemIsVersion ? version : 0);
                List<AveWebPartBaseInfo> data = AveSqlUtility.GetDBRows<AveWebPartBaseInfo>(SqlConn, cmdText, "tp_");
                return data;
#if PerformanceLog
            }
#endif
        }

        internal string GetWebPartsInGallery(Guid siteId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebPartsInGallery"))
            {
#endif
                string sCommandString = "SELECT L.tp_ID AS GalleryListId FROM Lists L JOIN Webs W ON L.tp_WebId = W.Id WHERE W.SiteId = @SiteID AND L.tp_Title = 'Web Part Gallery'";
                DataTable dataTable = new DataTable("Web part gallery list");
                SqlConnection connection = SqlConn.Connection;
                {
                    SqlCommand sqlCommand = new SqlCommand(sCommandString, connection);
                    sqlCommand.Parameters.Add(new SqlParameter("SiteID", siteId));
                    new SqlDataAdapter(sqlCommand).Fill(dataTable);
                }
                if (dataTable.Rows.Count <= 0)
                {
                    return null;
                }
                string str2 = dataTable.Rows[0]["GalleryListId"].ToString();
                DataTable table2 = new DataTable("Web part types");
                SqlConn.ClearParameters();

                string str3 = "SELECT nvarchar9 as WebPartName, nvarchar8 as Assembly, nvarchar7 as Title, nvarchar10 as Image, ntext2 as Description " + ", nvarchar3 as FileType, nvarchar11 as Category " + " FROM userData WHERE tp_ListId = @ListID";
                SqlCommand selectCommand = new SqlCommand(str3, SqlConn.Connection);
                selectCommand.Parameters.Add(new SqlParameter("ListID", str2));
                new SqlDataAdapter(selectCommand).Fill(table2);

                if (table2.Rows.Count <= 0)
                {
                    return null;
                }
                StringWriter w = new StringWriter(new StringBuilder());
                XmlWriter writer2 = new XmlTextWriter(w);
                writer2.WriteStartElement("WebPartGallery");
                foreach (DataRow row in table2.Rows)
                {
                    writer2.WriteStartElement("WebPart");
                    writer2.WriteAttributeString("Name", row["WebPartName"].ToString());
                    writer2.WriteAttributeString("Assembly", row["Assembly"].ToString());
                    writer2.WriteAttributeString("Title", row["Title"].ToString());
                    writer2.WriteAttributeString("Description", row["Description"].ToString());
                    writer2.WriteAttributeString("Image", row["Image"].ToString());
                    writer2.WriteAttributeString("FileType", row["FileType"].ToString());
                    writer2.WriteAttributeString("Category", row["Category"].ToString());
                    writer2.WriteEndElement();
                }
                writer2.WriteEndElement();
                writer2.Flush();
                return w.ToString();
#if PerformanceLog
            }
#endif
        }

        internal string GetListTitle(Guid listId)
        {
            SqlConn.AddParameter("@ListId", listId);
            string cmdText = "SELECT tp_Title FROM AllLists WHERE tp_Id=@ListId";
            return (string)SqlConn.ExecuteScalar(cmdText);
        }

        internal void SetWebPartPersonalization(AveWebPartBaseInfo webPartInfo)
        {
            string cmdText =
                @"SELECT tp_UserID,tp_PartOrder,tp_ZoneID,tp_IsIncluded,tp_FrameState,tp_PerUserProperties,tp_Cache,tp_Size,tp_Deleted 
                FROM Personalization where tp_SiteId=@SiteId AND tp_PageUrlID=@Id AND tp_WebPartID=@WebPartID";

            SqlConn.AddParameter("@WebPartID", webPartInfo.ID);
            webPartInfo.Personalization = AveSqlUtility.GetDBRows<AvePersonalizationInfo>(SqlConn, cmdText, "tp_");
        }
        internal void SetWebPartLists(AveWebPartBaseInfo webPartInfo)
        {
            string cmdText =
                @"SELECT wp.tp_WebId,wp.tp_UserID,wp.tp_Level, w.FullUrl AS tp_FullUrl
                FROM WebPartLists wp LEFT JOIN Webs w ON wp.tp_WebId=w.Id WHERE tp_SiteId=@SiteId and tp_PageUrlID=@Id AND tp_WebPartID=@WebPartID
                ";
            SqlConn.AddParameter("@WebPartID", webPartInfo.ID);
            webPartInfo.WebPartList = AveSqlUtility.GetDBRows<AveWebPartListInfo>(SqlConn, cmdText, "tp_");
        }
        internal void GetVersionInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache)
        {
#if PerformanceLog
            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetVersionInfo"))
            {
#endif
                string cmdText =
                              @"SELECT UIVersion,InternalVersion,TimeCreated,DocFlags,MetaInfoSize,Size,MetaInfo,CheckinComment,
                         Level,DraftOwnerId,DeleteTransactionId,VirusVendorID,VirusStatus,VirusInfo
                FROM  AllDocVersions
                WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x AND UIVersion=@Version";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", itemInfo.SiteId);
                SqlConn.AddParameter("@Id", itemInfo.GUID);
                SqlConn.AddParameter("Version", itemInfo.Version);
                AveSqlUtility.TryGetDBRow(dataCache, SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }
        internal bool GetDocHasStream(AveBaseItemInfo itemInfo, int internalVersion)
        {
            string cmdText = @"select COUNT(Id) from AllDocStreams where SiteId=@SiteId and Id=@Id and InternalVersion=@internalVersion";
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@SiteId", itemInfo.SiteId);
            SqlConn.AddParameter("@Id", itemInfo.GUID);
            SqlConn.AddParameter("@internalVersion", internalVersion);
            return ((int)SqlConn.ExecuteScalar(cmdText) > 0);
        }
        internal Dictionary<string, object> GetAttachmentInfo(AveBaseItemInfo baseItemInfo)
        {
            Dictionary<string, object> dataCache = new Dictionary<string, object>();
            string cmdText =
@"SELECT LeafName as Title, TimeCreated as Created, TimeLastModified as Modified
FROM AllDocs
WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
            if (baseItemInfo.ParentId != Guid.Empty)
            {
                cmdText += " ParentID=@ParentID AND ";
            }
            cmdText += " Id=@Id AND UIVersion=@Version";
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@SiteId", baseItemInfo.SiteId);
            SqlConn.AddParameter("@ParentID", baseItemInfo.ParentId);
            SqlConn.AddParameter("@Id", baseItemInfo.GUID);
            SqlConn.AddParameter("@Version", baseItemInfo.Version);
            AveSqlUtility.TryGetDBRow(dataCache, SqlConn, cmdText);
            return dataCache;
        }
        internal void GetDocInfo(AveBaseItemInfo itemInfo, Dictionary<string, object> dataCache)
        {
#if PerformanceLog
            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetDocInfo"))
            {
#endif
                string cmdText =
    @"SELECT Id,DirName,LeafName,DoclibRowId,Type,SortBehavior,Size,UIVersion,Dirty,ListDataDirty,
         CacheParseId,DocFlags,ThicketFlag,CharSet,ProgId,TimeCreated,TimeLastModified,
         NextToLastTimeModified,MetaInfoTimeLastModified,TimeLastWritten,SetupPathVersion,
         SetupPath,SetupPathUser,CheckoutUserId,CheckoutDate,CheckoutExpires,VersionCreatedSinceSTCheckout,
         LTCheckoutUserId,VirusVendorID,VirusStatus,VirusInfo,MetaInfo,MetaInfoSize,MetaInfoVersion,
         UnVersionedMetaInfo,UnVersionedMetaInfoSize,UnVersionedMetaInfoVersion,WelcomePageUrl,
         WelcomePageParameters,IsCurrentVersion,Level,CheckinComment,AuditFlags,InheritAuditFlags,
         DraftOwnerId,UIVersionString,ParentId,HasStream,ScopeId,BuildDependencySet,ParentVersion,
         ParentVersionString,TransformerId,ParentLeafName,IsCheckoutToLocal,CtoOffset,Extension,
         ExtensionForFile,ItemChildCount,FolderChildCount,FileFormatMetaInfo,FileFormatMetaInfoSize,
         ListSchemaVersion,ClientId,InternalVersion,BumpVersion
FROM AllDocs
WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
                if (itemInfo.ParentId != Guid.Empty)
                {
                    cmdText += " ParentID=@ParentID AND ";
                }
                cmdText += " Id=@Id AND UIVersion=@Version";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", itemInfo.SiteId);
                SqlConn.AddParameter("@ParentID", itemInfo.ParentId);
                SqlConn.AddParameter("@Id", itemInfo.GUID);
                SqlConn.AddParameter("@Version", itemInfo.Version);
                AveSqlUtility.TryGetDBRow(dataCache, SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }

        internal int GetInternalVersion(AveBaseItemInfo itemInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetInternalVersion"))
            {
#endif
                //if (itemInfo.InternalVersion != null && itemInfo.InternalVersion != 0)
                //{
                //    return (int)itemInfo.InternalVersion;
                //}
                string cmdText = @"SELECT InternalVersion FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
                if (itemInfo.ParentId != Guid.Empty)
                {
                    cmdText += " ParentID=@ParentID AND ";
                }
                cmdText += " Id=@Id AND UIVersion=@UIVersion ";

                cmdText += @" UNION SELECT InternalVersion FROM AllDocVersions WHERE SiteId=@SiteId AND ID=@ID AND UIVersion=@UIVersion";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ID", itemInfo.GUID);
                SqlConn.AddParameter("@UIVersion", itemInfo.Version);
                SqlConn.AddParameter("@ParentID", itemInfo.ParentId);
                SqlConn.AddParameter("@SiteId", itemInfo.SiteId);
                object result = SqlConn.ExecuteScalar(cmdText);
                if (result is int)
                {
                    return (int)result;
                }
                return 0;
#if PerformanceLog
            }
#endif
        }

        internal int GetDocFlag(AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetDocFlag"))
            {
#endif
                string cmdText = @"SELECT DISTINCT DocFlags
                            FROM         AllDocs
                            WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
                if (info.ParentId != Guid.Empty)
                {
                    cmdText += " ParentID=@ParentID AND ";
                }
                cmdText += " Id=@Id AND UIVersion=@UIVersion ";
                cmdText += @"   UNION
                            SELECT     DocFlags
                            FROM         AllDocVersions
                            WHERE     (SiteId = @SiteId) AND (Id = @ID) AND (UIVersion = @UIVersion)";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ID", info.GUID);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@UIVersion", info.Version);
                SqlConn.AddParameter("@ParentID", info.ParentId);
                object result = SqlConn.ExecuteScalar(cmdText);
                if (result is int)
                {
                    return (int)result;
                }
                return 0;
#if PerformanceLog
            }
#endif
        }

        internal byte[] GetRbsIdByNative(AveBaseItemInfo info)
        {
            string cmdText = "SELECT RbsId FROM AllDocStreams WHERE Id=@ID AND SiteId=@SiteId AND InternalVersion = @InternalVersion";
            int internalVersion = GetInternalVersion(info);
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@ID", info.GUID);
            SqlConn.AddParameter("@SiteId", info.SiteId);
            SqlConn.AddParameter("@InternalVersion", internalVersion);
            return SqlConn.ExecuteScalar(cmdText) as byte[];
        }
        internal string GetStubInfoByNative(Guid siteId, Guid id, int internalVersion)
        {
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@InternalVersion", internalVersion);
            SqlConn.AddParameter("@SiteId", siteId);
            SqlConn.AddParameter("@Id", id);
            string cmdText = "SELECT DATALENGTH(Content),Content FROM AllDocStreams WHERE SiteId=@SiteId AND Id=@Id AND InternalVersion=@InternalVersion";
            using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
            {
                if (dr.Read())
                {
                    int length = (int)dr.GetInt64(0);
                    byte[] buffer = new byte[length];
                    dr.GetBytes(1, 0, buffer, 0, length);
                    return Encoding.UTF8.GetString(buffer);
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        internal string GetFields(Guid webId, Guid listId)
        {
            string fieldsSchema = null;

            SqlConn.ClearParameters();
            SqlConn.AddParameter("@WebId", webId);
            SqlConn.AddParameter("@ListId", listId);
            string cmdText = @"select tp_Fields from AllLists where tp_WebId=@WebId and tp_ID=@ListId";
            using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
            {
                while (dr.Read())
                {
                    byte[] bytes = dr["tp_Fields"] as byte[];
                    if (bytes != null && bytes.Length > 0)
                    {
                        fieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
                    }
                    break;
                }
            }
            if (fieldsSchema != null && fieldsSchema.Contains("<"))
            {
                fieldsSchema = fieldsSchema.Substring(fieldsSchema.IndexOf("<", StringComparison.OrdinalIgnoreCase));
            }

            return "<Fields>" + fieldsSchema + "</Fields>";
        }

        internal string GetViewFields(Guid siteId, Guid listId)
        {
            string viewFieldsSchema = null;

            SqlConn.ClearParameters();
            SqlConn.AddParameter("@SiteId", siteId);
            SqlConn.AddParameter("@ListId", listId);
            string cmdText = @"select tp_View from AllWebParts where tp_SiteId=@SiteId and tp_ListId=@ListId and tp_Type=0"; //0 means the webpart is in default view
            using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
            {
                while (dr.Read())
                {
                    byte[] bytes = dr["tp_View"] as byte[];
                    if (bytes != null && bytes.Length > 0)
                    {
                        viewFieldsSchema = AveCompressedUtility.GetTCompressedString(bytes);
                    }
                }
            }

            return viewFieldsSchema;
        }

        internal int GetThreadIndexParentId(Guid listId, byte[] threadIndex)
        {
            string cmdText = @"select tp_ID from AllUserData where tp_ThreadIndex =@ThreadIndex and tp_ListId=@ListId";
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@ListId", listId);
            SqlConn.AddParameter("@ThreadIndex", threadIndex);
            using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
            {
                while (dr.Read())
                {
                    return dr.GetInt32(0);
                }
            }
            return -1;
        }

        internal List<Dictionary<string, object>> GetUserDataJunction(AveBaseItemInfo infoItem)
        {
#if PerformanceLog
            using (AvePerformanceScope scope = new AvePerformanceScope("ObjectModel.Server.AveDBQueryService.GetUserDataJunction"))
            {
#endif
                if (infoItem.RowId <= 0)
                {
                    return null;
                }
                string cmdText = @"SELECT tp_FieldId,tp_Id,tp_UIVersion,tp_Ordinal,tp_SourceListId
                               FROM AllUserDataJunctions
                               WHERE tp_SiteId=@SiteId AND tp_DocId=@DocId AND tp_DeleteTransactionId=0x AND tp_UIVersion=@Version
                               ORDER BY tp_FieldId,tp_Ordinal";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", infoItem.SiteId);
                SqlConn.AddParameter("@DocId", infoItem.GUID);
                SqlConn.AddParameter("@Version", infoItem.Version);
                List<Dictionary<string, object>> data = new List<Dictionary<string, object>>();
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        Dictionary<string, object> dataCache = new Dictionary<string, object>();
                        AveSqlUtility.GetDBRow(dataCache, dr);
                        data.Add(dataCache);
                    }
                }
                if (data != null && data.Count > 0)
                {
                    return data;
                }

                return null;
#if PerformanceLog
            }
#endif
        }

        internal AveRBSStubInfo AveRBSBackup_BackupRBSStub(int collectionId, long blob_num, short blobStoreId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSBackup_BackupRBSStub"))
            {
#endif
                using (SqlCommand cmd = SqlConn.Connection.CreateCommand())
                {
                    cmd.CommandText = AveRBSCommon.CMD_FETCH_RBS_BLOBID_AND_POOLID;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@blob_number", blob_num);
                    cmd.Parameters.AddWithValue("@client_version", 0);
                    cmd.Parameters.Add(new SqlParameter("@blob_store_id", SqlDbType.SmallInt));
                    cmd.Parameters.Add(new SqlParameter("@store_pool_id", SqlDbType.VarBinary, 255));
                    cmd.Parameters.Add(new SqlParameter("@store_blob_id", SqlDbType.VarBinary, 255));
                    cmd.Parameters.Add(new SqlParameter("@create_time", SqlDbType.SmallDateTime));
                    cmd.Parameters.Add(new SqlParameter("@length", SqlDbType.BigInt));
                    cmd.Parameters["@blob_store_id"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@store_pool_id"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@store_blob_id"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@create_time"].Direction = ParameterDirection.Output;
                    cmd.Parameters["@length"].Direction = ParameterDirection.Output;
                    //cmd.Parameters.Add(new SqlParameter("@returnValue", SqlDbType.Int)).Direction = ParameterDirection.ReturnValue;

                    int i = cmd.ExecuteNonQuery();

                    short temProviderId = (short)(cmd.Parameters["@blob_store_id"].Value);
                    if (temProviderId != blobStoreId)
                        throw new Exception("This RBS Stub was not generated by DocAve.SP2010.Storage.RBSProvider");
                    byte[] tem_blobId = cmd.Parameters["@store_blob_id"].Value as byte[];
                    byte[] tem_poolId = cmd.Parameters["@store_pool_id"].Value as byte[];
                    long dataLen = (long)(cmd.Parameters["@length"].Value);

                    AveRBSStubInfo stubInfo = new AveRBSStubInfo(tem_blobId, tem_poolId, AveRBSCommon.RBS_PROVIDER_NAME, dataLen);
                    return stubInfo;
                }
#if PerformanceLog
            }
#endif
        }
        internal long AveRBSBackup_GenerateBlobNumber(byte[] rbs_id)
        {
            using (SqlCommand cmd = SqlConn.Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[mssqlrbs].[rbs_fn_get_blob_number]";
                cmd.Parameters.AddWithValue("@blob_id", rbs_id);
                cmd.Parameters.Add(new SqlParameter("@blob_num", SqlDbType.BigInt));
                cmd.Parameters["@blob_num"].Direction = ParameterDirection.ReturnValue;

                object x = cmd.ExecuteScalar();
                return (long)(cmd.Parameters["@blob_num"].Value);
            }
        }
        internal long AveRBSBackup_WriteBlobInformationToDB(AveRBSStubInfo stubinfo, int collectionId, short blobStoreId)
        {
            long blobNum = -1;
            long blobSize = stubinfo.DataLength;
            if (blobSize == 0)
                return -1;
            try
            {
                using (SqlCommand cmd = SqlConn.Connection.CreateCommand())
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "[mssqlrbs].[rbs_sp_register_blob]";
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@blob_store_id", blobStoreId);
                    cmd.Parameters.AddWithValue("@store_pool_id", stubinfo.StorePoolId);
                    cmd.Parameters.AddWithValue("@store_blob_id", stubinfo.StoreBlobId);
                    cmd.Parameters.AddWithValue("@create_time", DateTime.Now.ToUniversalTime());
                    cmd.Parameters.AddWithValue("@length", blobSize);
                    cmd.Parameters.AddWithValue("@client_version", 0);

                    cmd.Parameters.Add("@blob_number", SqlDbType.BigInt);
                    cmd.Parameters["@blob_number"].Direction = ParameterDirection.Output;

                    object x = cmd.ExecuteScalar();
                    blobNum = (long)cmd.Parameters["@blob_number"].Value;
                }
            }
            catch (SqlException e)
            {
                //由于可能在插入STUB的过程中破坏mssqlrbs_resources.rbs_internal_blobs的unique index 'rbs_internal_blobs_ix_orphan_scan'，因此，如果出现这样的错误
                //我们应该获取已存在的这条STUB的Blob_Number并利用它生成一个RbsId返回给调用者，这样，将会出现有两个或者多个alldocstreams中的记录拥有同一个
                //RBS Stub的情况，也就是有多个alldocstreams中的记录有着相同的RbsId。
                if (e.ToString().Contains(@"Cannot insert duplicate key row in object 'mssqlrbs_resources.rbs_internal_blobs' with unique index 'rbs_internal_blobs_ix_orphan_scan'."))
                {
                    return AveRBSExtenderRestore_GetBlobNumber(stubinfo, blobStoreId);
                }
                else
                    throw;
            }
            catch (Exception ex)
            {
                throw;
            }
            return blobNum;
        }

        internal long AveRBSExtenderRestore_GetBlobNumber(AveRBSStubInfo stubInfo, short blobStoreId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSExtenderRestore_GetBlobNumber"))
            {
#endif
                long blobNum = -1;
                string cmdStr = @"SELECT blob_number FROM [mssqlrbs_resources].[rbs_internal_blobs] 
WHERE blob_store_id=@blob_store_id AND store_pool_id=@store_pool_id AND store_blob_id=@store_blob_id";
                try
                {
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@blob_store_id", blobStoreId);
                    SqlConn.AddParameter("@store_pool_id", stubInfo.StorePoolId);
                    SqlConn.AddParameter("@store_blob_id", stubInfo.StoreBlobId);
                    using (SqlDataReader dr = SqlConn.ExecuteReader(cmdStr))
                    {
                        if (dr.Read())
                            blobNum = dr.GetInt64(0);
                    }
                }
                catch (Exception ex)
                {//log here
                    Console.WriteLine(ex.ToString());
                }
                return blobNum;
#if PerformanceLog
            }
#endif
        }
        internal byte[] AveRBSExtenderRestore_GenerateRbsId(int collectionId, long blob_num)
        {
            using (SqlCommand cmd = SqlConn.Connection.CreateCommand())
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandText = "[mssqlrbs].[rbs_fn_get_blob_id]";
                cmd.Parameters.AddWithValue("@collection_id", collectionId);
                cmd.Parameters.AddWithValue("@blob_number", blob_num);
                cmd.Parameters.Add("@blob_id", SqlDbType.VarBinary, 64);
                cmd.Parameters["@blob_id"].Direction = ParameterDirection.ReturnValue;

                object x = cmd.ExecuteScalar();
                return (byte[])cmd.Parameters["@blob_id"].Value;
            }

        }
        internal void AveRBSExtenderRestore_CreatePool(byte[] poolId, bool canStoreNewBlobs, int collectionId, short blobStoreId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSExtenderRestore_CreatePool"))
            {
#endif
                using (SqlCommand cmd = SqlConn.Connection.CreateCommand())
                {
                    try
                    {
                        cmd.CommandType = CommandType.StoredProcedure;
                        cmd.CommandText = "[mssqlrbs].[rbs_sp_add_pool]";
                        cmd.Parameters.AddWithValue("@blob_store_id", blobStoreId);
                        cmd.Parameters.AddWithValue("@store_pool_id", poolId);
                        cmd.Parameters.AddWithValue("@collection_id", collectionId);
                        cmd.Parameters.AddWithValue("@client_version", 0);
                        cmd.Parameters.Add("@pool_id", SqlDbType.Int);
                        cmd.Parameters["@pool_id"].Direction = ParameterDirection.Output;
                        object x = cmd.ExecuteScalar();
                        int poolIndex = (int)cmd.Parameters["@pool_id"].Value;

                        SqlConn.ClearParameters();
                        SqlConn.AddParameter("@BlobStoreId", blobStoreId);
                        SqlConn.AddParameter("@StorePoolId", poolId);
                        SqlConn.AddParameter("@PoolId", poolIndex);
                        SqlConn.AddParameter("@CanStoreNewBlobs", canStoreNewBlobs);
                        SqlConn.AddParameter("@CloseTime", DateTime.Now);
                        string commandText = @"UPDATE [mssqlrbs_resources].[rbs_internal_pools] 
SET [can_store_new_blobs]=@CanStoreNewBlobs,[close_time]=@CloseTime 
WHERE [blob_store_id]=@BlobStoreId AND [store_pool_id]=@StorePoolId AND [pool_id]=@PoolId";
                        SqlConn.ExecuteNonQuery(commandText);
                    }
                    catch (Exception e)
                    {
                        throw new Exception(string.Format("Cannot create archive pool in collection {0} for provider {1}. Exception: {2}", collectionId, blobStoreId, e.Message));
                    }
                }
#if PerformanceLog
            }
#endif
        }

        internal int[] AveRBSCommon_GetCollectionIdAndProviderId()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSCommon_GetCollectionIdAndProviderId"))
            {
#endif
                int[] temId = new int[2];
                string commandText = @"SELECT collection_id FROM [mssqlrbs_resources].[rbs_internal_collections] WHERE owning_application=@CollectionName";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@CollectionName", AveRBSCommon.COLLECTION_OWNING_APPLICATION);
                temId[0] = (int)SqlConn.ExecuteScalar(commandText);

                commandText = @"SELECT blob_store_id FROM [mssqlrbs_resources].[rbs_internal_blob_stores] WHERE blob_store_name=@ProviderName";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ProviderName", AveRBSCommon.RBS_PROVIDER_NAME);
                using (SqlDataReader sdr = SqlConn.ExecuteReader(commandText))
                {
                    if (sdr.Read())
                    {
                        temId[1] = sdr.GetInt16(0);
                    }
                }
                return temId;
#if PerformanceLog
            }
#endif
        }
        internal List<Guid> AveRBSCommon_GetPoolsOfDB()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSCommon_GetPoolsOfDB"))
            {
#endif
                List<Guid> temList = new List<Guid>();
                try
                {
                    byte[] poolIdBinary = null;
                    string commandText = @"SELECT store_pool_id FROM [mssqlrbs_resources].[rbs_internal_pools]";
                    using (SqlDataReader reader = SqlConn.ExecuteReader(commandText))
                    {
                        while (reader.Read() && !reader.IsDBNull(0))
                        {
                            poolIdBinary = (byte[])reader.GetValue(0);
                            Guid poolGuid = AveRBSCommon.GetPoolGuid(poolIdBinary);
                            if (!temList.Contains(poolGuid))
                            {
                                temList.Add(poolGuid);
                            }
                        }
                    }

                }
                catch //(Exception ex)
                {
                    temList = null;
                }
                return temList;
#if PerformanceLog
            }
#endif
        }
        internal long AveRBSConnectorRestore_RegisterBlob(int collectionId, int blobStoreId, byte[] storePoolId, byte[] storeBlobId, DateTime createTime, long blobSize)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSConnectorRestore_RegisterBlob"))
            {
#endif
                using (SqlCommand cmd = SqlConn.Command)
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "mssqlrbs.rbs_sp_register_blob";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@returnValue", SqlDbType.Int)).Direction = ParameterDirection.Output;

                    cmd.Parameters.AddWithValue("@collection_id", collectionId).SqlDbType = SqlDbType.Int;
                    cmd.Parameters.AddWithValue("@blob_store_id", blobStoreId).SqlDbType = SqlDbType.SmallInt;
                    cmd.Parameters.AddWithValue("@store_pool_id", storePoolId).SqlDbType = SqlDbType.VarBinary;
                    cmd.Parameters.AddWithValue("@store_blob_id", storeBlobId).SqlDbType = SqlDbType.VarBinary;
                    cmd.Parameters.AddWithValue("@create_time", createTime).SqlDbType = SqlDbType.SmallDateTime;
                    cmd.Parameters.AddWithValue("@length", blobSize).SqlDbType = SqlDbType.BigInt;

                    cmd.Parameters.Add(new SqlParameter("@blob_number", SqlDbType.BigInt)).Direction = ParameterDirection.Output;

                    cmd.ExecuteNonQuery();
                    int returnValue = Convert.ToInt32(cmd.Parameters["@returnValue"].Value, CultureInfo.InvariantCulture);
                    switch (returnValue)
                    {
                        case 0:
                            return Convert.ToInt64(cmd.Parameters["@blob_number"].Value);
                        default:
                            throw new Exception("Unexpected returnValue.");
                    }
                    throw new Exception("Unexpected stored procedure return code.");
                }
#if PerformanceLog
            }
#endif
        }

        internal byte[] AveRBSConnectorRestore_GetRbsId(int collectionId, long blobNumber)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSConnectorRestore_GetRbsId"))
            {
#endif
                byte[] RbsId = null;
                using (SqlCommand cmmd = new SqlCommand())
                {
                    cmmd.CommandType = CommandType.StoredProcedure;
                    cmmd.Connection = SqlConn.Connection;
                    cmmd.CommandTimeout = 0;
                    cmmd.CommandText = "[mssqlrbs].[rbs_fn_get_blob_id]";
                    cmmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmmd.Parameters.AddWithValue("@blob_number", blobNumber);
                    cmmd.Parameters.Add("@blob_id", SqlDbType.VarBinary, 64);
                    cmmd.Parameters["@blob_id"].Direction = ParameterDirection.ReturnValue;
                    object x = cmmd.ExecuteScalar();
                    RbsId = (byte[])cmmd.Parameters["@blob_id"].Value;
                }
                return RbsId;
#if PerformanceLog
            }
#endif
        }
        internal int AveRBSConnectorRestore_AddPool(int blobSotreId, byte[] storePoolId, int collectionId, int clientVersion)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSConnectorRestore_AddPool"))
            {
#endif
                using (SqlCommand cmd = SqlConn.Connection.CreateCommand())
                {
                    cmd.Parameters.Clear();
                    cmd.CommandText = "mssqlrbs.rbs_sp_add_pool";
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandTimeout = 0;
                    cmd.Parameters.Add(new SqlParameter("@returnValue", SqlDbType.Int)).Direction = ParameterDirection.ReturnValue;
                    cmd.Parameters.AddWithValue("@blob_store_id", blobSotreId);
                    cmd.Parameters.AddWithValue("@store_pool_id", storePoolId);
                    cmd.Parameters.AddWithValue("@collection_id", collectionId);
                    cmd.Parameters.AddWithValue("@client_version", clientVersion);
                    cmd.Parameters.Add("@pool_id", SqlDbType.Int).Direction = ParameterDirection.Output;
                    object re = cmd.ExecuteScalar();
                    return (int)cmd.Parameters["@pool_id"].Value;
                }
#if PerformanceLog
            }
#endif
        }
        internal int AveRBSConnectorRestore_ClosePool(int blobStoreId, byte[] storePoolId, int poolId, bool canStoreNewBlobs)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSConnectorRestore_ClosePool"))
            {
#endif
                string cmdText = "UPDATE " + "[mssqlrbs_resources].[rbs_internal_pools]"
                                                                    + " SET " + "[can_store_new_blobs]" + "=@CanStoreNewBlobs," + "[close_time]" + "=@CloseTime"
                                                                    + " WHERE " + "[blob_store_id]" + "=@BlobStoreId AND " + "[store_pool_id]" + "=@StorePoolId AND " + "[pool_id]" + "=@PoolId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@BlobStoreId", blobStoreId);
                SqlConn.AddParameter("@StorePoolId", blobStoreId).SqlDbType = SqlDbType.VarBinary;
                SqlConn.AddParameter("@PoolId", poolId);
                SqlConn.AddParameter("@CanStoreNewBlobs", canStoreNewBlobs);
                SqlConn.AddParameter("@CloseTime", DateTime.Now);
                return SqlConn.ExecuteNonQuery(cmdText);
#if PerformanceLog
            }
#endif
        }
        internal bool AveRBSConnectorRestore_CheckBlobExist(byte[] storePoolId, byte[] storeBlobId, int blobStoreId, int collectionId, ref long blobNumber)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.AveRBSConnectorRestore_CheckBlobExist"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@store_pool_id", storePoolId).SqlDbType = SqlDbType.VarBinary;
                SqlConn.AddParameter("@store_blob_id", storeBlobId).SqlDbType = SqlDbType.VarBinary;
                SqlConn.AddParameter("@blob_store_id", blobStoreId);
                SqlConn.AddParameter("@collection_id", collectionId);

                StringBuilder builder = new StringBuilder();
                builder.Append("Select [blob_number] From ");
                builder.Append("[mssqlrbs_resources].[rbs_internal_blobs]");
                builder.Append(" Where ");
                builder.Append("[blob_store_id]");
                builder.Append("=");
                builder.Append("@blob_store_id");
                builder.Append(" AND ");
                builder.Append("collection_id");
                builder.Append("=");
                builder.Append("@collection_id");
                builder.Append(" AND ");
                builder.Append("[store_pool_id]");
                builder.Append("=");
                builder.Append("@store_pool_id");
                builder.Append(" AND ");
                builder.Append("[store_blob_id]");
                builder.Append("=");
                builder.Append("@store_blob_id");
                using (SqlDataReader reader = SqlConn.ExecuteReader(builder.ToString()))
                {
                    if (reader.Read())
                    {
                        blobNumber = reader.GetInt64(0);
                        return true;
                    }
                }

                return false;
#if PerformanceLog
            }
#endif
        }

        internal SqlDataReader GetALLWebTemplates(Guid siteId)
        {
            string cmdText = @"SELECT Id,WebTemplate,ProvisionConfig FROM Webs WHERE SiteId=@SiteId";
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@SiteId", siteId);
            return SqlConn.ExecuteReader(cmdText);
        }
        internal int GetCheckOutUserId(AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetCheckOutUserId"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@Id", info.GUID);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@Version", info.Version);
                SqlConn.AddParameter("@ParentID", info.ParentId);

                string cmdText = string.Empty;
                cmdText = "SELECT CheckoutUserId FROM AllDocs WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND ";
                if (info.ParentId != Guid.Empty)
                {
                    cmdText += " ParentID=@ParentID AND ";
                }
                cmdText += " Id=@Id AND UIVersion=@Version";

                object result = SqlConn.ExecuteScalar(cmdText);
                if (result != null && result is int)
                {
                    return (int)result;
                }
                else
                {
                    return 0;
                }
#if PerformanceLog
            }
#endif
        }

        internal List<int> GetDocVersions(AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetDocVersions"))
            {
#endif
                List<int> versions = new List<int>();
                string cmdText = "Select UIVersion from Alldocs where SiteId=@SiteId And DeleteTransactionId=0x And";
                if (info.ParentId != Guid.Empty)
                {
                    cmdText += " ParentID=@ParentID AND";
                }
                cmdText += " Id=@Id";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@ParentID", info.ParentId);
                SqlConn.AddParameter("@Id", info.GUID);
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {

                    while (dr.Read())
                    {
                        int version = dr.GetInt32(0);
                        if (!versions.Contains(version))
                        {
                            versions.Add(version);
                        }
                    }
                }
                cmdText = "Select UIVersion from AllDocVersions where SiteId=@SiteId And Id=@Id And DeleteTransactionId=0x";
                using (SqlDataReader vr = SqlConn.ExecuteReader(cmdText))
                {

                    while (vr.Read())
                    {
                        int version = vr.GetInt32(0);
                        if (!versions.Contains(version))
                        {
                            versions.Add(version);
                        }
                    }
                }
                return versions;
#if PerformanceLog
            }
#endif
        }




        internal void GetParentContentTypeInfoTree(AveContentTypeInfo contentTypeInfo, Guid siteId, List<byte[]> parentIdList)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetParentContentTypeInfoTree"))
            {
#endif
                AveContentTypeInfo rootCTInfo = contentTypeInfo;
                try
                {
                    for (int i = 0; i < parentIdList.Count; i++)
                    {
                        AveContentTypeInfo ctInfo = null;
                        SqlConn.ClearParameters();
                        SqlConn.AddParameter("@SiteId", siteId);
                        SqlConn.AddParameter("@ContentTypeId", parentIdList[i]);
                        string cmdText = @"select ContentTypeId,Scope,Version,Definition,ResourceDir,SolutionId,IsFromFeature from ContentTypes
                               where Class=1 and ContentTypeId=@ContentTypeId and SiteId=@SiteId";

                        string parentName = null;
                        using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                        {
                            while (dr.Read())
                            {
                                try
                                {
                                    if (dr.IsDBNull(4))
                                    {
                                        continue;
                                    }
                                    parentName = dr["ResourceDir"] as string;
                                    if (dr.IsDBNull(3))
                                    {
                                        continue;
                                    }
                                    ctInfo = new AveContentTypeInfo();
                                    ctInfo.Name = dr["ResourceDir"] as string;
                                    if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ctInfo.Name = SPUtility.GetLocalizedString(ctInfo.Name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                    }
                                    ctInfo.Scope = dr["Scope"] as string;
                                    string definition = dr["Definition"] as string;
                                    XmlDocument xDoc = new XmlDocument();
                                    xDoc.InnerXml = definition;
                                    XmlElement root = (XmlElement)xDoc.ChildNodes[0];

                                    if (root.HasAttribute("Name"))
                                    {
                                        ctInfo.Name = root.Attributes["Name"].Value;//使用xml中的Name作为ContentType的真实名字
                                    }
                                    if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ctInfo.Name = SPUtility.GetLocalizedString(ctInfo.Name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                    }
                                    ctInfo.Id = root.Attributes["ID"].Value;
                                    ctInfo.ReadOnly = root.HasAttribute("ReadOnly") && root.Attributes["ReadOnly"].Value == "TRUE";
                                    ctInfo.Description = root.HasAttribute("Description") ? root.Attributes["Description"].Value : "";
                                    if (ctInfo.Description.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ctInfo.Description = SPUtility.GetLocalizedString(ctInfo.Description, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                    }
                                    string fieldRefs = root["FieldRefs"] != null ? root["FieldRefs"].InnerXml : "";
                                    fieldRefs = "<Fields>" + fieldRefs + "</Fields>";
                                    ctInfo.FieldsSchemaXml = fieldRefs;
                                    ctInfo.DocumentTemplate = root["DocumentTemplate"] != null ? root["DocumentTemplate"].Attributes["TargetName"].Value : "";
                                    ctInfo.Group = root.Attributes["Group"].Value;
                                    if (ctInfo.Group.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                    {
                                        ctInfo.Group = SPUtility.GetLocalizedString(ctInfo.Group, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                    }
                                    ctInfo.Hidden = root.HasAttribute("Hidden") && root.Attributes["Hidden"].Value == "TRUE";
                                    break;
                                }
                                catch (Exception e)
                                {
                                    //mLog.Log(AveLogSeverity.Error, "WP10BKAveSPCT427", e);
                                }
                            }
                        }
                        if (!String.IsNullOrEmpty(parentName))
                        {
                            rootCTInfo.ParentName = parentName;
                        }

                        if (ctInfo != null)
                        {
                            rootCTInfo.ParentContentTypeInfo = ctInfo;
                            rootCTInfo.ParentName = ctInfo.Name;
                            rootCTInfo = ctInfo;
                        }
                        else
                        {
                            break;
                        }
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Warn, "WP10BKAveSPCT444", contentTypeInfo.Name, e);
                }
#if PerformanceLog
            }
#endif
        }


        internal string GetContentTypeContent(Guid listId, Guid webId, Guid siteId, string scope)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetContentTypeContent"))
            {
#endif
                SqlConn.ClearParameters();
                if (scope.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    scope = scope.Substring(1);
                }
                try
                {
                    SqlConn.AddParameter("@ListId", listId);
                    SqlConn.AddParameter("@WebId", webId);
                    string cmdText = @"select tp_ContentTypes from AllLists where tp_WebId=@WebId and tp_ID=@ListId";
                    string contentTypesContent = null;
                    using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            byte[] content = dr["tp_ContentTypes"] as byte[];
                            if (content != null)
                            {
                                contentTypesContent = AveCompressedUtility.GetTCompressedString(content);
                            }
                            break;
                        }
                    }
                    return contentTypesContent;
                }
                catch (Exception ex)
                {
                    return null;
                }
#if PerformanceLog
            }
#endif
        }

        internal AveContentTypeCollectionInfo GetContentTypeInfos(Guid siteId, string scope)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetContentTypeInfos"))
            {
#endif
                AveContentTypeCollectionInfo infos = new AveContentTypeCollectionInfo();
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                if (scope.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                {
                    scope = scope.Substring(1);
                }
                try
                {
                    SqlConn.AddParameter("@Scope", scope);
                    string cmdText = @"select ContentTypeId,Scope,Version,Definition,ResourceDir,SolutionId,IsFromFeature from ContentTypes
                               where SiteId=@SiteId and Class=1 and Scope=@Scope";

                    using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            try
                            {
                                if (dr.IsDBNull(3))
                                {
                                    continue;
                                }
                                AveContentTypeInfo ctInfo = new AveContentTypeInfo();
                                ctInfo.Name = dr["ResourceDir"] as string;
                                ctInfo.Scope = dr["Scope"] as string;
                                string definition = dr["Definition"] as string;
                                XmlDocument xDoc = new XmlDocument();
                                xDoc.InnerXml = definition;
                                XmlElement root = (XmlElement)xDoc.ChildNodes[0];

                                if (root.HasAttribute("Name"))
                                {
                                    ctInfo.Name = root.Attributes["Name"].Value;
                                }
                                if (ctInfo.Name.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Name = SPUtility.GetLocalizedString(ctInfo.Name, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                }
                                ctInfo.Id = root.Attributes["ID"].Value;
                                ctInfo.ReadOnly = root.HasAttribute("ReadOnly") && root.Attributes["ReadOnly"].Value == "TRUE";
                                ctInfo.Description = root.HasAttribute("Description") ? root.Attributes["Description"].Value : "";
                                if (ctInfo.Description.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Description = SPUtility.GetLocalizedString(ctInfo.Description, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                }
                                string fieldRefs = root["FieldRefs"] != null ? root["FieldRefs"].InnerXml : "";
                                fieldRefs = "<Fields>" + fieldRefs + "</Fields>";
                                ctInfo.FieldsSchemaXml = fieldRefs;
                                ctInfo.DocumentTemplate = root["DocumentTemplate"] != null ? root["DocumentTemplate"].Attributes["TargetName"].Value : "";
                                ctInfo.Group = root.Attributes["Group"].Value;
                                if (ctInfo.Group.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                                {
                                    ctInfo.Group = SPUtility.GetLocalizedString(ctInfo.Group, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                                }
                                ctInfo.Hidden = root.HasAttribute("Hidden") && root.Attributes["Hidden"].Value == "TRUE";
                                ctInfo.ResourceFolder = root["Folder"] != null ? root["Folder"].Attributes["TargetName"].Value : null;

                                if (root["XmlDocuments"] != null)
                                {
                                    foreach (XmlNode node in root["XmlDocuments"].ChildNodes)
                                    {
                                        string temp = AveCompressedUtility.GetStringFromBase64String(node.InnerText);
                                        ctInfo.XmlDocuments.Add(temp);
                                    }
                                }

                                infos.ContentTypes.Add(ctInfo);
                            }
                            catch (Exception e)
                            {

                                //mLog.Log(AveLogSeverity.Warn, "WP10BKAveCTCO126", e);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {

                }
                return infos;
#if PerformanceLog
            }
#endif
        }

        internal void GetViews(ref Dictionary<string, List<AveViewInfo>> viewCache, Guid listId, Guid defaultViewId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetViews"))
            {
#endif
                viewCache.Clear();
                string cmdText = @"select tp_ID,tp_DisplayName,tp_Type,DirName+'/'+LeafName,tp_Flags,tp_BaseViewID,tp_UserID from AllWebParts,AllDocs where tp_ListId =@listid and (tp_Type=1  or tp_Type=0) and tp_PageUrlID=Id  and tp_DisplayName!=''";

                SqlConn.Command.Parameters.Clear();
                SqlConn.AddParameter("@listid", listId);
                using (SqlDataReader sdr = SqlConn.ExecuteReader(cmdText))
                {
                    while (sdr.Read())
                    {
                        if (sdr.IsDBNull(1))
                        {
                            continue;
                        }
                        string url = sdr[3].ToString();

                        if (!viewCache.ContainsKey(url))
                        {
                            viewCache.Add(url, new List<AveViewInfo>());
                        }
                        List<AveViewInfo> views = viewCache[url];
                        AveViewInfo viewInfo = new AveViewInfo();
                        viewInfo.Id = sdr.GetGuid(0);
                        viewInfo.Title = sdr.GetString(1);
                        try
                        {
                            if (!sdr.IsDBNull(5))
                            {
                                viewInfo.BaseViewId = sdr.GetByte(5);
                            }
                        }
                        catch (Exception ce)
                        { }

                        if (!sdr.IsDBNull(6))
                        {
                            viewInfo.UserID = sdr.GetInt32(6);
                        }
                        try
                        {
                            bool isDefaultView = false;
                            if (defaultViewId.Equals(viewInfo.Id))
                            {
                                isDefaultView = true;
                            }
                            viewInfo.IsDefaultView = isDefaultView;
                        }
                        catch (Exception e)
                        {
                            viewInfo.IsDefaultView = false;
                            //mLog.Log(AveLogSeverity.Warn, "WP10BKeSPList344", viewInfo.Title, e);
                        }
                        int i = Convert.ToInt32(sdr[4]);
                        viewInfo.IsPersonal = (i & 262144) == 262144 ? true : false;
                        viewInfo.ViewType = Convert.ToInt32(sdr[4]);
                        views.Add(viewInfo);
                    }
                }
#if PerformanceLog
            }
#endif
        }

        internal List<string> GetFields(Guid siteId, string scope)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetFields"))
            {
#endif
                List<string> fields = new List<string>();
                try
                {
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", siteId);
                    if (scope.StartsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        scope = scope.Substring(1);
                    }
                    SqlConn.AddParameter("@Scope", scope);
                    string cmdText = @"select Definition from ContentTypes where 
                                SiteId=@SiteId and Class=0 and Scope=@Scope and Definition is not null";

                    using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            if (dr.IsDBNull(0))
                            {
                                continue;
                            }
                            string definition = dr["Definition"] as string;
                            fields.Add(definition);
                        }
                    }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Warn, "WP10BKAveSPFC614", siteId, scope, e);
                }
                return fields;
#if PerformanceLog
            }
#endif
        }

        internal bool CheckContentTypeExist(Guid siteId, byte[] ctId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.CheckContentTypeExist"))
            {
#endif
                try
                {
                    string cmdTxt = @"SELECT COUNT(ContentTypeId) FROM ContentTypes WHERE SiteId=@SiteId AND Class=1 AND ContentTypeId=@ContentTypeId";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", siteId);
                    SqlConn.AddParameter("@ContentTypeId", ctId);

                    if (((int)SqlConn.ExecuteScalar(cmdTxt)) > 0)
                    {
                        return true;
                    }
                }
                catch { }
                return false;
#if PerformanceLog
            }
#endif
        }

        internal bool CheckIfContentTypeExistInChildren(Guid siteId, string scope, byte[] ctId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.CheckIfContentTypeExistInChildren"))
            {
#endif
                try
                {
                    string cmdTxt = @"WITH CT
                                            AS
                                            (SELECT * FROM 
                                            TVF_ContentTypes_SiteClassCTId(
                                            @SiteId, 1, @ContentTypeId))
                                            SELECT COUNT(ContentTypeId) FROM CT WHERE SCOPE LIKE @Scope";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", siteId);
                    SqlConn.AddParameter("@Scope", scope.TrimStart('/') + "/%");
                    SqlConn.AddParameter("@ContentTypeId", ctId);

                    if (((int)SqlConn.ExecuteScalar(cmdTxt)) > 0)
                    {
                        return true;
                    }
                }
                catch { }
                return false;
#if PerformanceLog
            }
#endif
        }

        internal void DeleteWebPartByNative(Guid siteId, Guid docId, string webPartId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.DeleteWebPartByNative"))
            {
#endif
                string idProperty = webPartId;
                if (webPartId != null && webPartId.Length > 36)
                {
                    webPartId = webPartId.Substring(webPartId.Length - 36);
                    webPartId = webPartId.Replace("_", "-");
                }
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteID", siteId);
                SqlConn.AddParameter("@PageID", docId);
                SqlConn.AddParameter("@ID", new Guid(webPartId));
                SqlConn.AddParameter("@IdProperty", idProperty);
                string cmdText = "delete from AllWebParts where tp_SiteId=@SiteID AND tp_PageUrlID=@PageID AND (tp_ID=@ID or tp_WebPartIdProperty=@IdProperty)";
                SqlConn.ExecuteNonQuery(cmdText);
#if PerformanceLog
            }
#endif
        }

        internal string GetWebCTNameById(Guid siteId, string contentTypeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetWebCTNameById"))
            {
#endif
                string ctName = string.Empty;

                string cmdText = "SELECT ResourceDir FROM ContentTypes WHERE SiteId=@SiteId AND Class=1 AND ContentTypeId=@ContentTypeId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ContentTypeId", contentTypeId);
                ctName = (string)SqlConn.ExecuteScalar(cmdText);

                return ctName;
#if PerformanceLog
            }
#endif
        }

        internal string InitialAlert(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.InitialAlert"))
            {
#endif
                StringBuilder mQueryConditions = new StringBuilder();
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                if (listId.Equals(Guid.Empty))
                {
                    mQueryConditions.Append(" WHERE SiteId=@SiteId AND ListId is NULL");
                }
                else
                {
                    mQueryConditions.Append(" WHERE SiteId=@SiteId AND ListId=@ListId");
                    SqlConn.AddParameter("@ListId", listId);
                }

                switch (hostType)
                {
                    case AveSPAlertHostType.List:
                    case AveSPAlertHostType.Folder:
                        mQueryConditions.Append(" AND ItemId is NULL AND Deleted=0");
                        break;
                    case AveSPAlertHostType.Doc:
                        SqlConn.AddParameter("@ItemId", itemRowId);
                        mQueryConditions.Append(" AND ItemId=@ItemId AND Deleted=0");
                        break;
                    case AveSPAlertHostType.Item:
                        SqlConn.AddParameter("@ItemId", itemRowId);
                        mQueryConditions.Append(" AND ItemId=@ItemId AND Deleted=0");
                        break;
                    default:
                        break;
                }

                return mQueryConditions.ToString();
#if PerformanceLog
            }
#endif
        }

        private Hashtable GetMetaInfoDic(byte[] metaInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetMetaInfoDic"))
            {
#endif
                Hashtable Dic = new Hashtable();
                string info = AveCompressedUtility.GetTCompressedString(metaInfo);
                string[] mSplitedString = info.Replace("\r\n", "*").Split(new char[] { '*' });
                foreach (string mStr in mSplitedString)
                {
                    try
                    {
                        int index1 = mStr.IndexOf(":", StringComparison.OrdinalIgnoreCase);
                        int index2 = mStr.IndexOf("|", StringComparison.OrdinalIgnoreCase);
                        if (index1 < 0 && index2 < 0)
                        {
                            continue;
                        }
                        string key = index1 > 0 ? mStr.Substring(0, index1) : mStr.Substring(0, index2);
                        string typeStr = index1 < index2 ? mStr.Substring(index1 + 1, 2).ToUpper() : string.Empty;
                        string valueStr = index2 > 0 ? mStr.Substring(index2 + 1) : String.Empty;
                        object value = null;
                        switch (typeStr)
                        {
                            case "IW":
                                value = Int32.Parse(valueStr);
                                break;
                            case "BW":
                                value = Boolean.Parse(valueStr);
                                break;
                            default:
                                value = valueStr;
                                break;
                        }
                        Dic.Add(key, value);
                    }
                    catch (Exception e)
                    {
                        //mLog.Warn(e, "Get Value Error{0}", !String.IsNullOrEmpty(mStr) ? mStr : "mStr is Empty");
                        //mLog.Log(AveLogSeverity.Warn, "WP10BKListInf549", e);
                        continue;
                    }
                }
                return Dic;
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="list"></param>
        /// <param name="parentWeb"></param>
        /// <param name="listSettingInfo"></param>
        /// <returns>flag</returns>
        internal ulong GetListSettingInfoByDB(IAveList list, IAveWeb parentWeb, AveListSettingInfo listSettingInfo)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetListSettingInfoByDB"))
            {
#endif
                ulong flags = 0;
                try
                {
                    IAveSite parentSite = parentWeb.Site;
                    if (AveSPEnv.IsMoss)
                    {
                        listSettingInfo.AllowRatingSetting = GetListRatingSettingByMossAPI(list);
                    }
                    try
                    {
                        listSettingInfo.DefaultView = parentWeb.Url.Substring(0, parentWeb.Url.Length - (list.ParentWebUrl.Length > 1 ? list.ParentWebUrl.Length : 0)) + list.DefaultViewUrl;
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogSeverity.Warn, "WP10BKListInf357", list.Title, list.ID, e);
                    }
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@ListId", list.ID);
                    SqlConn.AddParameter("@WebId", parentWeb.ID);
                    string cmdText = @"
SELECT tp_Title, tp_Created, tp_LastSecurityChange, tp_Version, tp_Author, 
       tp_BaseType, tp_FeatureId, tp_ServerTemplate, tp_Template, tp_ImageUrl, 
       tp_ReadSecurity, tp_WriteSecurity, tp_Subscribed, tp_Direction, tp_Flags, 
       tp_ThumbnailSize, tp_WebImageWidth, tp_WebImageHeight, tp_Description, tp_EmailAlias, 
       tp_ScopeId, tp_HasFGP, tp_HasInternalFGP, tp_EventSinkAssembly, tp_EventSinkClass, 
       tp_EventSinkData, tp_MaxRowOrdinal, tp_Fields, tp_ContentTypes, tp_AuditFlags, 
       tp_InheritAuditFlags, tp_SendToLocation, tp_ListDataDirty, tp_CacheParseId, tp_MaxMajorVersionCount, 
       tp_MaxMajorwithMinorVersionCount, tp_DefaultWorkflowId, tp_NoThrottleListOperations,tp_ListSchemaVersion,tp_ID
FROM AllLists 
WHERE tp_WebId=@WebId and tp_Id=@ListId";

                    AveSqlUtility.GetDBRow(listSettingInfo, SqlConn, cmdText, "tp_");
                    listSettingInfo.RootFolderInfo = new AveListRootFolderInfo();
                    //因为上面已经加了参数，这里就不再加参数

                    using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                    {
                        while (dr.Read())
                        {
                            flags = (ulong)dr.GetInt64(14);
                            AveListTemplateType template = (AveListTemplateType)dr.GetInt32(7);
                            AveBaseType baseType = (AveBaseType)dr.GetInt32(5);
                            //AllLists.tp_ID
                            if (list.ID.ToString().Equals(parentWeb.TaxonomyList))
                            {
                                listSettingInfo.IsTaxonomyHiddenList = true;
                            }
                            ///////////////////////
                            // 0,UseListSetting
                            // 1,.Browser
                            // 2, PreferClient
                            ////////////////////////
                            if (!Ave2010ListFlags.DefaultItemOpenUseListSetting(flags))
                            {
                                listSettingInfo.DefaultItemOpen = 0;
                            }
                            else
                            {
                                listSettingInfo.DefaultItemOpen = Ave2010ListFlags.DefaultItemOpen(flags, parentSite.BrowserDocumentsEnabled) == AveDefaultItemOpen.Browser ? 1 : 2;
                            }
                            listSettingInfo.AllowContentTypes = Ave2010ListFlags.AllowContentTypes(flags, template);
                            listSettingInfo.AllowDeletion = Ave2010ListFlags.AllowDeletion(flags);
                            listSettingInfo.AllowMultiResponses = Ave2010ListFlags.AllowMultiResponses(flags);
                            listSettingInfo.EnableFolderCreation = Ave2010ListFlags.EnableFolderCreation(flags);
                            listSettingInfo.EnableModeration = Ave2010ListFlags.EnableModeration(flags);
                            listSettingInfo.IrmEnabled = Ave2010ListFlags.IrmEnabled(flags);
                            listSettingInfo.IrmExpire = Ave2010ListFlags.IrmExpire(flags);
                            listSettingInfo.IrmReject = Ave2010ListFlags.IrmReject(flags);
                            listSettingInfo.EnableVersioning = Ave2010ListFlags.EnableVersioning(flags);
                            listSettingInfo.Ordered = Ave2010ListFlags.IrmReject(flags);
                            listSettingInfo.ContentTypesEnabled = Ave2010ListFlags.ContentTypesEnabled(flags);
                            listSettingInfo.EnableAssignToEmail = Ave2010ListFlags.EnableAssignToEmail(flags);
                            listSettingInfo.EnableDeployWithDependentList = Ave2010ListFlags.EnableDeployWithDependentList(flags);
                            listSettingInfo.EnableDeployingList = Ave2010ListFlags.EnableDeployingList();
                            listSettingInfo.EnablePeopleSelector = Ave2010ListFlags.EnablePeopleSelector(flags);
                            listSettingInfo.EnableResourceSelector = Ave2010ListFlags.EnableResourceSelector(flags);
                            listSettingInfo.EnableSchemaCaching = Ave2010ListFlags.EnableSchemaCaching(flags);
                            listSettingInfo.EnforceDataValidation = Ave2010ListFlags.EnforceDataValidation(flags);
                            listSettingInfo.EnableSyndication = Ave2010ListFlags.EnableSyndication(flags);
                            listSettingInfo.ExcludeFromOfflineClient = Ave2010ListFlags.ExcludeFromOfflineClient(flags);
                            listSettingInfo.ExcludeFromTemplate = Ave2010ListFlags.ExcludeFromTemplate(flags);
                            listSettingInfo.Hidden = Ave2010ListFlags.Hidden(flags);
                            listSettingInfo.MultipleDataList = Ave2010ListFlags.MultipleDataList(flags);
                            listSettingInfo.NoCrawl = Ave2010ListFlags.NoCrawl(flags);
                            listSettingInfo.EnableAttachments = Ave2010ListFlags.EnableAttachments(flags, baseType, template);
                            listSettingInfo.EnableMinorVersions = Ave2010ListFlags.EnableMinorVersions(flags, baseType);
                            listSettingInfo.ForceCheckout = Ave2010ListFlags.ForceCheckout(flags, baseType);
                            //reader =0,author=1,approval = 3
                            listSettingInfo.DraftVersionVisibility = (int)Ave2010ListFlags.DraftVersionVisibility(flags, baseType);
                            listSettingInfo.AllowRssFeads = listSettingInfo.EnableSyndication.Value && parentSite.AllowRssFeeds;
                            listSettingInfo.EnableThrottling = !dr.GetBoolean(37);
                            listSettingInfo.IsThrottled = listSettingInfo.EnableThrottling.Value && (list.ItemCount > parentSite.WebApplication.MaxItemsPerThrottledOperation);
                            listSettingInfo.ShowUser = Ave2010ListFlags.ShowUser(flags);
                            //AllLists.tp_ScopeId

                            listSettingInfo.HasUniqueRoleAssigntments = list.HasUniqueRoleAssignments;
                            listSettingInfo.OnQuickLaunch = list.OnQuickLaunch;

                            //AllList.tp_SendToLocation, split with '|' SendToLoacationName = string[0] and SendToLoacationUrl = string[1];
                            if (!dr.IsDBNull(31))
                            {
                                string sendToLocationProperty = dr.GetString(31);
                                string[] splitLoacationProperty = sendToLocationProperty.Split(new char[] { '|' });
                                listSettingInfo.SendToLocationName = splitLoacationProperty[0];
                                listSettingInfo.SendToLocationUrl = splitLoacationProperty[1];
                            }
                            else
                            {
                                listSettingInfo.SendToLocationName = null;
                                listSettingInfo.SendToLocationUrl = null;
                            }
                            listSettingInfo.DisableGridEditing = Ave2010ListFlags.DisableGridEditing(flags);
                            listSettingInfo.NavigateForFormsPages = Ave2010ListFlags.NavigateForFormsPages(flags);
                        }
                    }

                    if (list is IAveDocumentLibrary)
                    {
                        IAveDocumentLibrary docLib = list as IAveDocumentLibrary;
                        listSettingInfo.DocumentTemplateUrl = docLib.DocumentTemplateUrl;
                    }
                    try
                    {
                        Guid rootFolderId = new Guid(SqlConn.ExecuteScalar(@"Select tp_RootFolder From AllLists Where tp_WebId=@WebId and tp_Id=@ListId;").ToString());

                        cmdText = "SELECT CharSet,TimeCreated,TimeLastModified,MetaInfo,Dirty,DocFlags,WelcomePageUrl FROM AllDocs WHERE SiteId=@SiteId AND Id=@Id AND DeleteTransactionId=0x";

                        SqlConn.ClearParameters();
                        SqlConn.AddParameter("@SiteId", parentSite.ID);
                        SqlConn.AddParameter("@Id", rootFolderId);
                        AveSqlUtility.GetDBRow(listSettingInfo.RootFolderInfo.Value, SqlConn, cmdText);
                        if (listSettingInfo.RootFolderInfo.Value.MetaInfo != null)
                        {
                            listSettingInfo.RootFolderInfo.Value.MetaInfoDic = GetMetaInfoDic(listSettingInfo.RootFolderInfo.Value.MetaInfo);
                            listSettingInfo.IsSiteAssetsLibrary = false;
                            if (listSettingInfo.RootFolderInfo.Value.MetaInfoDic.ContainsKey("IsAttachmentLibrary"))
                            {
                                listSettingInfo.IsSiteAssetsLibrary = Int32.Parse(listSettingInfo.RootFolderInfo.Value.MetaInfoDic["IsAttachmentLibrary"].ToString()) == 0 ? false : true;
                            }
                        }
                        else
                        {
                            listSettingInfo.RootFolderInfo.Value.MetaInfoDic = null;
                            listSettingInfo.IsSiteAssetsLibrary = false;
                        }

                        bool RssViewExist = AveSPListUtility.IsViewExist(list, "RssView");

                        if (RssViewExist)
                        {
                            IAveView rssView = list.Views["RssView"];
                            listSettingInfo.RssViewField = rssView.ViewFields.SchemaXml;
                        }
                        else
                        {
                            listSettingInfo.RssViewField = "";
                        }
                    }
                    catch (Exception e)
                    {
                        //mLog.Log(AveLogSeverity.Error, "WP10BKListInf487", listSettingInfo.Title, e);
                    }
                    //listSettingInfo.RootFolderInfo.MetaInfoDic = GetMetaInfoDic(listSettingInfo.RootFolderInfo.MetaInfo);

                    listSettingInfo.ValidationFormula = list.ValidationFormula;
                    listSettingInfo.ValidationMessage = list.ValidationMessage;
                    //the code below has bugs, DB store the internal name of the field not the displayname, so use api backup.
                    //                cmdText = @"select ValidationFormula, ValidationMessage from AllListsPlus
                    //                            where ListId = @ListId";
                    //                SqlConn.ClearParameters();
                    //                SqlConn.AddParameter("@ListId", list.ID);
                    //                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                    //                {
                    //                    while (dr.Read())
                    //                    {
                    //                        //AllListsPlus.ValidationFormula
                    //                        if (dr.IsDBNull(0))
                    //                        {
                    //                            listSettingInfo.ValidationFormula = string.Empty;
                    //                        }
                    //                        else
                    //                        {
                    //                            listSettingInfo.ValidationFormula = dr.GetString(0);
                    //                        }
                    //                        //AllListsPlus.ValidationMessage
                    //                        if (dr.IsDBNull(1))
                    //                        {
                    //                            listSettingInfo.ValidationMessage = string.Empty;
                    //                        }
                    //                        else
                    //                        {
                    //                            listSettingInfo.ValidationMessage = dr.GetString(1);
                    //                        }
                    //                    }
                    //                }
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Warn, "WP10BKListInf521", listSettingInfo.Title, e);
                    //Log Error
                    throw;
                }

                return flags;
#if PerformanceLog
            }
#endif
        }

        private static bool GetListRatingSettingByMossAPI(IAveList list)
        {
            Guid averageRatings = Microsoft.SharePoint.Publishing.FieldId.AverageRatings;
            Guid ratingsCount = Microsoft.SharePoint.Publishing.FieldId.RatingsCount;
            return list.Fields.Contains(averageRatings) && list.Fields.Contains(ratingsCount);
        }

        internal List<Dictionary<string, object>> GetImmedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType, string folderUrl)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetImmedSubscriptions"))
            {
#endif
                string mImmedQueryCmd =
    @"SELECT Id,UserId,UserEmail,SiteUrl,WebUrl,WebTitle,WebLanguage,WebLocale,WebTimeZone,
         WebTime24,WebCalendarType,WebAdjustHijriDays,ListUrl,ListTitle,ListBaseType,
         ListServerTemplate,AlertTitle,AlertType,AlertTemplateName,Filter,BinaryFilter,
         Properties,Status,ItemDocId,DeliveryChannel,EventType
FROM  ImmedSubscriptions";
                SqlConn.ClearParameters();
                string queryConditions = InitialAlert(siteId, webId, listId, itemRowId, hostType);
                List<Dictionary<string, object>> ImmedSubscriptions = new List<Dictionary<string, object>>();
                Dictionary<string, object> dataCache = null;
                using (SqlDataReader dr = SqlConn.ExecuteReader(mImmedQueryCmd + queryConditions.ToString()))
                {
                    while (dr.Read())
                    {
                        dataCache = new Dictionary<string, object>();
                        AveSqlUtility.GetDBRow(dataCache, dr);
                        //Folder和List的Alert的区别就在于Filter里面包含ItemFullUrl，或者Properties里面包含filefilter
                        if (hostType == AveSPAlertHostType.Folder || hostType == AveSPAlertHostType.List)
                        {
                            string properties = dataCache["Properties"].ToString();
                            if (hostType == AveSPAlertHostType.List)
                            {
                                if (properties.ToLower().Contains("filterpath"))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (!GetFileFilter(properties).Equals(folderUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }
                        }

                        ImmedSubscriptions.Add(dataCache);
                    }
                }
                return ImmedSubscriptions;
#if PerformanceLog
            }
#endif
        }
        internal List<Dictionary<string, object>> GetSchedSubscriptions(Guid siteId, Guid webId, Guid listId, int itemRowId, AveSPAlertHostType hostType, string folderUrl)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetSchedSubscriptions"))
            {
#endif
                string mSchedQueryCmd =
      @"SELECT Id,NotifyFreq,NotifyTime,NotifyTimeUTC,UserId,UserEmail,SiteUrl,WebUrl,WebTitle,
         WebLanguage,WebLocale,WebTimeZone,WebTime24,WebCalendarType,WebAdjustHijriDays,
         ListUrl,ListTitle,ListBaseType,ListServerTemplate,AlertTitle,AlertType,AlertTemplateName,
         Filter,BinaryFilter,Properties,Status,ItemDocId,DeliveryChannel,EventType
FROM  SchedSubscriptions";
                SqlConn.ClearParameters();
                string queryConditions = InitialAlert(siteId, webId, listId, itemRowId, hostType);
                List<Dictionary<string, object>> ImmedSubscriptions = new List<Dictionary<string, object>>();
                Dictionary<string, object> dataCache = null;
                using (SqlDataReader dr = SqlConn.ExecuteReader(mSchedQueryCmd + queryConditions.ToString()))
                {
                    while (dr.Read())
                    {
                        dataCache = new Dictionary<string, object>();
                        AveSqlUtility.GetDBRow(dataCache, dr);

                        //Folder和List的Alert的区别就在于Filter里面包含ItemFullUrl，或者Properties里面包含filefilter
                        if (hostType == AveSPAlertHostType.Folder || hostType == AveSPAlertHostType.List)
                        {
                            string properties = dataCache["Properties"].ToString();
                            if (hostType == AveSPAlertHostType.List)
                            {
                                if (properties.ToLower().Contains("filterpath"))
                                {
                                    continue;
                                }
                            }
                            else
                            {
                                if (!GetFileFilter(properties).Equals(folderUrl.Trim('/'), StringComparison.OrdinalIgnoreCase))
                                {
                                    continue;
                                }
                            }
                        }

                        ImmedSubscriptions.Add(dataCache);
                    }
                }
                return ImmedSubscriptions;
#if PerformanceLog
            }
#endif
        }

        private string GetFileFilter(string filter)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetFileFilter"))
            {
#endif
                if (!string.IsNullOrEmpty(filter))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(filter);
                    foreach (XmlNode node in doc.GetElementsByTagName("property"))
                    {
                        string value = node.Attributes["value"].Value;
                        string name = node.Attributes["name"].Value;
                        if (name.Equals("filterpath", StringComparison.OrdinalIgnoreCase))
                        {
                            return value.Trim('/');
                        }
                    }
                }
                return "";
#if PerformanceLog
            }
#endif
        }

        internal int SetAttachmentSize(AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.SetAttachmentSize"))
            {
#endif
                int length = 0;
                try
                {
                    string cmdText = @"Select Size From AllDocs With(noLock) Where SiteId =@SiteId
                              And DeleteTransactionId=0x And Id=@Id And UIVersion=@Version";
                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@Id", info.GUID);
                    SqlConn.AddParameter("@Version", info.Version);
                    using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                    {
                        if (dr.Read())
                        {
                            length = dr.GetInt32(0);
                        }
                    }
                }
                catch (Exception e)
                {
                    //mLog.Info("Get Attachment Size Error, AttachmentName:{0},Exception:{1}", mName, e.ToString());
                }
                return length;
#if PerformanceLog
            }
#endif
        }

        internal Guid GetFirstUniqueRoleDefinitionWebGuid(Guid siteId, Guid scopeId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetFirstUniqueRoleDefinitionWebGuid"))
            {
#endif
                string cmdTxt = @"select RoleDefWebId from Perms where SiteId=@SiteId AND ScopeId=@ScopeId";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ScopeId", scopeId);
                return (Guid)SqlConn.ExecuteScalar(cmdTxt);
#if PerformanceLog
            }
#endif
        }

        internal int GetRoleAssignmentCount(Guid siteId, Guid scopeId, int roleId, int principalId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetRoleAssignmentCount"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@ScopeId", scopeId);
                SqlConn.AddParameter("@RoleId", roleId);
                SqlConn.AddParameter("@PrincipalId", principalId);
                return (int)SqlConn.ExecuteScalar("SELECT COUNT(*) from RoleAssignment WHERE SiteId=@SiteId and ScopeId=@ScopeId and RoleId=@RoleId and PrincipalId=@PrincipalId");
#if PerformanceLog
            }
#endif
        }

        internal void UpdateUserInfo(Guid siteId, Guid listId, int userId, AveUserInfo old, string displayField, string nameField)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateUserInfo"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@UserListId", listId);
                SqlConn.AddParameter("@LoginName", old.Login);
                SqlConn.AddParameter("@Id", userId);
                SqlConn.AddParameter("@SystemId", old.SystemID);
                SqlConn.AddParameter("@Title", old.Title);

                SqlConn.Command.CommandText = "UPDATE UserInfo SET tp_SystemId=@SystemId,tp_Login=@LoginName,tp_Title=@Title WHERE tp_SiteId=@SiteId AND tp_Id=@Id " +
                                    "UPDATE AllUserData SET " + displayField + "=@Title," + nameField + "=@LoginName WHERE tp_ListId=@UserListId AND tp_Id=@Id";

                SqlConn.Command.ExecuteNonQuery();
#if PerformanceLog
            }
#endif
        }

        internal AveGroupInfo GetGroupInfo(Guid siteId, int principalId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetGroupInfo"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@Id", principalId);
                string cmdText = @"
SELECT ID,Title,Description,Owner,OwnerIsUser,DLAlias,DLErrorMessage,DLFlags,DLJobId,DLArchives,RequestEmail,Flags 
From Groups WHERE SiteId=@SiteId AND ID=@Id";

                List<AveGroupInfo> groupList = AveSqlUtility.GetDBRows<AveGroupInfo>(SqlConn, cmdText);
                if (groupList == null || groupList.Count == 0)
                {
                    return null;
                }
                AveGroupInfo groupInfo = groupList[0];

                cmdText = "SELECT MemberId From GroupMembership WHERE SiteId=@SiteId AND GroupId=@Id";

                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    while (dr.Read())
                    {
                        int memberId = dr.GetInt32(0);
                        if (groupInfo.Memberships == null)
                        {
                            groupInfo.Memberships = new List<int>();
                        }
                        groupInfo.Memberships.Add(memberId);
                    }
                }

                return groupInfo;

                #region client Method
                //IAveGroup group = aveSite.SPSite.RootWeb.Groups.GetByID(principalId);
                //AveGroupInfo groupInfo = new AveGroupInfo();
                //groupInfo.ID = group.ID;
                //groupInfo.Title = group.Name;
                //foreach (IAveUser user in group.Users)
                //{
                //    groupInfo.Memberships.Add(user.ID);
                //}
                //groupInfo.RequestEmail = group.RequestToJoinLeaveEmailSetting;
                ////do something
                //return groupInfo;
                #endregion
#if PerformanceLog
            }
#endif
        }

        internal AveUserInfo GetUserInfo(Guid siteId, int principalId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetUserInfo"))
            {
#endif
                AveUserInfo userInfo = null;
                string cmdText = @"
SELECT tp_ID,tp_DomainGroup,tp_SystemID,tp_Deleted,tp_SiteAdmin,tp_IsActive,tp_Login,tp_Title,tp_Email,tp_Notes,
       tp_Token,tp_ExternalTokenLastUpdated,tp_Locale,tp_CalendarType,tp_AdjustHijriDays,tp_TimeZone,tp_Time24,
       tp_AltCalendarType,tp_CalendarViewOptions,tp_WorkDays,tp_WorkDayStartHour,tp_WorkDayEndHour,tp_Mobile,tp_Flags 
FROM UserInfo 
WHERE tp_SiteID=@SiteId AND tp_ID=@Id";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@Id", principalId);

                List<AveUserInfo> Users = AveSqlUtility.GetDBRows<AveUserInfo>(SqlConn, cmdText, "tp_");
                if (Users != null)
                {
                    userInfo = Users[0];
                }
                return userInfo;

                #region client method
                //IAveUser user = aveSite.SPSite.RootWeb.AllUsers.GetByID(principalId);
                //AveUserInfo userInfo = new AveUserInfo();
                //userInfo.ID = user.ID;
                //userInfo.Login = user.LoginName;
                //userInfo.Title = user.Name;
                //userInfo.Email = user.Email;
                //userInfo.Notes = user.Notes;
                //if (user.RegionalSettings != null)
                //{
                //    userInfo.WorkDays = user.RegionalSettings.WorkDays;
                //    userInfo.WorkDayStartHour = user.RegionalSettings.WorkDayStartHour;
                //    userInfo.WorkDayEndHour = user.RegionalSettings.WorkDayEndHour;
                //    userInfo.CalendarType = user.RegionalSettings.CalendarType;
                //    userInfo.AdjustHijriDays = user.RegionalSettings.AdjustHijriDays;
                //    userInfo.AltCalendarType = (byte?)user.RegionalSettings.AlternateCalendarType;
                //    userInfo.Time24 = user.RegionalSettings.Time24;
                //}
                //return userInfo;
                #endregion
#if PerformanceLog
            }
#endif
        }

        internal bool CheckUserIfAvailable(Guid siteId, int userId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.Import"))
            {
#endif
                string cmdText = @"
SELECT COUNT(*)
FROM UserInfo 
WHERE tp_SiteID=@SiteId AND tp_IsActive=1 AND tp_Deleted=0 
                AND tp_ID in (
                SELECT DISTINCT(PrincipalId) FROM RoleAssignment WHERE SiteId=@SiteId And PrincipalId=@UserId
                UNION
                SELECT DISTINCT(MemberId) FROM GroupMembership WHERE SiteId=@SiteId AND MemberId=@UserId
)";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@UserId", userId);

                return (int)SqlConn.ExecuteScalar(cmdText) > 0;
#if PerformanceLog
            }
#endif
        }

        internal Guid GetListId(Guid webId, string listTitle)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetListId"))
            {
#endif
                Guid id = Guid.Empty;
                if (String.IsNullOrEmpty(listTitle))
                {
                    return id;
                }
                string text = "SELECT tp_Id FROM AllLists WHERE tp_WebId=@WebId AND tp_Title=@Title AND tp_DeleteTransactionId=0x";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@WebId", webId);
                SqlConn.AddParameter("@Title", listTitle);
                using (SqlDataReader reader = SqlConn.ExecuteReader(text))
                {
                    if (reader.Read())
                    {
                        id = reader.GetGuid(0);
                    }
                }
                return id;
#if PerformanceLog
            }
#endif
        }

        internal void UpdateSpecialPropertyByNative(string editor, string author, DateTime modified, DateTime created, AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateSpecialPropertyByNative"))
            {
#endif
                try
                {
                    string cmdStr = string.Empty;
                    cmdStr = @"UPDATE AllUserData SET tp_Editor=@Editor, tp_Author=@Author,tp_Created=@Created,
                               tp_Modified=@Modified WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                        AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level";

                    SqlConn.ClearParameters();
                    SqlConn.AddParameter("@Editor", editor);
                    SqlConn.AddParameter("@Author", author);
                    SqlConn.AddParameter("@Created", created);
                    SqlConn.AddParameter("@Modified", modified);

                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@ID", info.GUID);
                    SqlConn.AddParameter("@ParentId", info.ParentId);
                    SqlConn.AddParameter("@IsCurrentVersion", true);
                    SqlConn.AddParameter("@CalculatedVersion", 0);
                    SqlConn.AddParameter("@Level", info.Level);


                    SqlConn.ExecuteNonQuery(cmdStr);
                    //if (timeLastModified != DateTime.MinValue)
                    //{
                    //    cmdStr = "UPDATE AllDocs SET TimeLastModified=@TimeLastModified WHERE SiteId=@SiteId AND Id=@ID AND UIVersion=@UIVersion AND DeleteTransactionId=0x";
                    //    SqlConn.ClearParameters();
                    //    SqlConn.AddParameter("@TimeLastModified", timeLastModified);
                    //    SqlConn.AddParameter("@SiteId", mSiteId);
                    //    SqlConn.AddParameter("@ID", SPListItem.UniqueId);
                    //    SqlConn.AddParameter("@UIVersion", mVersion);
                    //    SqlConn.ExecuteNonQuery(cmdStr);
                    //}
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Warn, "WP10RTSPItem0987", SPListItem.Url, SPListItem.UniqueId, e);
                    //mLog.Warn(e, "An error occurred while updating an item SpecialProperty. Url:{0}, Id:{1}", SPListItem.Url, SPListItem.UniqueId);
                }
#if PerformanceLog
            }
#endif
        }

        internal void UpdateModifiedBy(string modifiedBy, string createdBy, string colNameModified, string colNameCreated, AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.UpdateModifiedBy"))
            {
#endif
                try
                {
                    string cmdStr = string.Empty;
                    cmdStr = @"UPDATE AllUserData SET " + colNameModified + @" = @ModifiedBy," + colNameCreated + @" =@CreatedBy
                                 WHERE tp_SiteId=@SiteId AND tp_ParentId=@ParentId AND tp_DocID=@ID 
                        AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=@IsCurrentVersion AND tp_CalculatedVersion=@CalculatedVersion AND tp_Level=@Level";

                    SqlConn.ClearParameters();

                    SqlConn.AddParameter("@SiteId", info.SiteId);
                    SqlConn.AddParameter("@ID", info.GUID);
                    SqlConn.AddParameter("@ParentId", info.ParentId);
                    SqlConn.AddParameter("@IsCurrentVersion", true);
                    SqlConn.AddParameter("@CalculatedVersion", 0);
                    SqlConn.AddParameter("@Level", info.Level);
                    SqlConn.AddParameter("@ModifiedBy", modifiedBy);
                    SqlConn.AddParameter("@CreatedBy", createdBy);

                    SqlConn.ExecuteNonQuery(cmdStr);
                }
                catch (Exception e)
                {
                    //mLog.Log(AveLogSeverity.Warn, "WP10RTSPItem1018", SPListItem.Url, SPListItem.UniqueId, e);
                    //mLog.Warn(e, "An error occurred while updating an item SpecialProperty. Url:{0}, Id:{1}", SPListItem.Url, SPListItem.UniqueId);
                }
#if PerformanceLog
            }
#endif
        }

        internal Guid GetFolderIdByName(AveBaseItemInfo info)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetFolderIdByName"))
            {
#endif
                Guid id = Guid.Empty;
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@LeafName", info.Name);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@ParentId", info.ParentId);
                string cmdText = "SELECT ID FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x AND type=1";
                try
                {
                    id = (Guid)SqlConn.ExecuteScalar(cmdText);
                }
                catch
                {
                    //cannot get folder's id by name
                }

                return id;
#if PerformanceLog
            }
#endif
        }

        internal List<AveHiddenFileInfo> GetHiddenFiles(Guid siteId, Guid webId, Guid listId, Guid folderId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetHiddenFiles"))
            {
#endif
                List<AveHiddenFileInfo> hiddenFiles = new List<AveHiddenFileInfo>();
                string commandText = @"
SELECT Id, LeafName, UIVersion, DocFlags, Level
FROM AllDocs
WHERE SiteId = @SiteId
AND DeleteTransactionId=0x
AND ParentId=@FolderId
AND WebId = @WebId ";
                if (listId != Guid.Empty)
                {
                    commandText += "AND ListId = @ListId ";
                }
                commandText += @"
AND Type = 0
AND DocLibRowId IS NULL
AND IsCurrentVersion = 1
ORDER BY LeafName, UIVersion
";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                SqlConn.AddParameter("@WebId", webId);
                if (listId != Guid.Empty)
                {
                    SqlConn.AddParameter("@ListId", listId);
                }
                SqlConn.AddParameter("@FolderId", folderId);
                using (SqlDataReader reader = SqlConn.ExecuteReader(commandText))
                {
                    while (reader.Read())
                    {

                        AveHiddenFileInfo fileInfo = new AveHiddenFileInfo();
                        fileInfo.ID = reader[0].ToString();
                        fileInfo.Name = reader[1].ToString();
                        fileInfo.Version = reader.GetInt32(2);
                        fileInfo.DocFlags = reader.GetInt32(3);
                        fileInfo.Level = reader.GetByte(4);
                        hiddenFiles.Add(fileInfo);
                    }
                }

                return hiddenFiles;
#if PerformanceLog
            }
#endif
        }

        internal Guid GetListItemGuid(Guid listId, int rowId)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetListItemGuid"))
            {
#endif
                Guid tpGUid = Guid.Empty;
                string cmdText = @"SELECT tp_GUID from AllUserData WHERE tp_ListId=@ListId AND tp_DeleteTransactionId=0x AND tp_IsCurrentVersion=1 AND tp_CalculatedVersion=0 
                                        AND tp_ID=@RowId AND tp_RowOrdinal=0";
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@ListId", listId);
                SqlConn.AddParameter("@RowId", rowId);
                using (SqlDataReader dr = SqlConn.ExecuteReader(cmdText))
                {
                    if (dr.Read())
                    {
                        Guid tp_GUID = dr.GetGuid(0);
                        return tp_GUID;
                    }
                }
                return tpGUid;
#if PerformanceLog
            }
#endif
        }

        internal byte[] GetDocStream(AveDocumentInfo info, Guid guid)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetDocStream"))
            {
#endif
                SqlConn.ClearParameters();
                string cmdText = @"SELECT Content FROM AllDocStreams WHERE Id=@Id AND SiteId=@SiteId AND InternalVersion=@InternalVersion";
                SqlConn.AddParameter("@Id", guid);
                SqlConn.AddParameter("@SiteId", info.SiteId);
                SqlConn.AddParameter("@InternalVersion", info.InternalVersion);
                using (SqlDataReader sqlReader = SqlConn.ExecuteReader(cmdText))
                {
                    while (sqlReader.Read())
                    {
                        return (byte[])sqlReader[0];
                    }
                }
                return null;
#if PerformanceLog
            }
#endif
        }

        internal bool IsAttachmentExsits(Guid siteId, Guid parentId, string leafName)
        {
            SqlConn.ClearParameters();
            SqlConn.AddParameter("@SiteId", siteId);
            SqlConn.AddParameter("@ParentId", parentId);
            SqlConn.AddParameter("@LeafName", leafName);

            string cmdText = "SELECT count(Id) FROM AllDocs WHERE SiteId=@SiteId AND ParentId=@ParentId AND LeafName=@LeafName AND DeleteTransactionId=0x";
            return ((int)SqlConn.ExecuteScalar(cmdText) > 0);
        }

        public void Dispose()
        {
            if (SqlConn != null)
            {
                SqlConn.Dispose();
                SqlConn = null;
            }
        }

        internal void GetCurrentVersionDocInfo(Guid siteId, Guid parentId, Guid itemId, Dictionary<string, object> dataCache)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.GetCurrentVersionDocInfo"))
            {
#endif
                string cmdText =
    @"SELECT Id,DirName,LeafName,DoclibRowId,Type,SortBehavior,Size,UIVersion,Dirty,ListDataDirty,
         CacheParseId,DocFlags,ThicketFlag,CharSet,ProgId,TimeCreated,TimeLastModified,
         NextToLastTimeModified,MetaInfoTimeLastModified,TimeLastWritten,SetupPathVersion,
         SetupPath,SetupPathUser,CheckoutUserId,CheckoutDate,CheckoutExpires,VersionCreatedSinceSTCheckout,
         LTCheckoutUserId,VirusVendorID,VirusStatus,VirusInfo,MetaInfo,MetaInfoSize,MetaInfoVersion,
         UnVersionedMetaInfo,UnVersionedMetaInfoSize,UnVersionedMetaInfoVersion,WelcomePageUrl,
         WelcomePageParameters,IsCurrentVersion,Level,CheckinComment,AuditFlags,InheritAuditFlags,
         DraftOwnerId,UIVersionString,ParentId,HasStream,ScopeId,BuildDependencySet,ParentVersion,
         ParentVersionString,TransformerId,ParentLeafName,IsCheckoutToLocal,CtoOffset,Extension,
         ExtensionForFile,ItemChildCount,FolderChildCount,FileFormatMetaInfo,FileFormatMetaInfoSize,
         ListSchemaVersion,ClientId,InternalVersion,BumpVersion
FROM AllDocs
WHERE SiteId=@SiteId AND DeleteTransactionId=0x AND IsCurrentVersion=1 AND";
                if (parentId != Guid.Empty)
                {
                    cmdText += " ParentID=@ParentID AND ";
                }
                cmdText += " Id=@Id";

                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteId);
                if (parentId != Guid.Empty)
                {
                    SqlConn.AddParameter("@ParentID", parentId);
                }
                SqlConn.AddParameter("@Id", itemId);
                AveSqlUtility.TryGetDBRow(dataCache, SqlConn, cmdText);
#if PerformanceLog
            }
#endif
        }

        //public AveFeatureInfoBox GetFeatures(SPSite site, AveFeatureScope scope)
        //{
        //    AveFeatureInfoBox featureBox = new AveFeatureInfoBox();

        //    string cmdText = "select FeatureId from Features where SiteId=@siteid and webid=@webid order by TimeActivated";
        //    SqlConn.Command.Parameters.Clear();
        //    SqlConn.Command.Parameters.AddWithValue("siteid", site.ID);
        //    SqlConn.Command.Parameters.AddWithValue("webid", new Guid("00000000-0000-0000-0000-000000000000"));
        //    using (SqlDataReader sdr = SqlConn.ExecuteReader(cmdText))
        //    {
        //        while (sdr.Read())
        //        {
        //            AveFeatureInfo info = new AveFeatureInfo();
        //            info.Id = sdr.GetGuid(0);
        //            info.Scope = scope;
        //            featureBox.FeatureList.Add(info);
        //        }
        //    }
        //    return featureBox;
        //}

        internal void AveSOUpdateRbsID(Guid siteID, Guid itemID, int uiVersion, int Size, byte[] data, bool isRbsID)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveDBQueryService.Import"))
            {
#endif
                SqlConn.ClearParameters();
                SqlConn.AddParameter("@SiteId", siteID);
                SqlConn.AddParameter("@Id", itemID);
                SqlConn.AddParameter("@UIVersion", uiVersion);
                string cmdText = string.Empty;
                if (isRbsID)
                {
                    SqlConn.AddParameter("@RbsId", data);
                    SqlConn.AddParameter("@Size", Size);
                    cmdText = @"Update AllDocStreams Set Content=null, RbsId=@RbsId where SiteId=@SiteId AND Id=@Id AND InternalVersion in(
                            (SELECT InternalVersion FROM AllDocs WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion))union
                            (SELECT InternalVersion FROM AllDocVersions WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)));
                            UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion);";
                }
                else
                {
                    SqlConn.AddParameter("@Content", data);
                    SqlConn.AddParameter("@Size", data.Length);
                    cmdText = @"Update AllDocStreams Set Content=@Content, RbsId=null where SiteId=@SiteId AND Id=@Id AND InternalVersion in(
                            (SELECT InternalVersion FROM AllDocs WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion))union
                            (SELECT InternalVersion FROM AllDocVersions WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion)));
                            UPDATE AllDocs SET Size = @Size WHERE (SiteId = @SiteId) AND (Id = @Id) AND (UIVersion = @UIVersion);";
                }
                SqlConn.ExecuteNonQuery(cmdText);
#if PerformanceLog
            }
#endif
        }
    }
}
