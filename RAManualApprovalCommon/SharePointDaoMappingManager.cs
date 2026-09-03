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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Explorer.Model;
using AvePoint.RA.RACommonUtility.Browser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApprovalCommon.ReportRelateSettingManagers
{
    public class SharePointDaoMappingManager
    {

        private static readonly RALogger Logger = RALogger.GetInstance(typeof(SharePointDaoMappingManager));

        private static readonly Dictionary<Guid, Guid> GroupMappingCache = new Dictionary<Guid, Guid>();

        private static readonly Dictionary<Guid, RemoteSiteCollection> SiteCollectionMappingCache = new Dictionary<Guid, RemoteSiteCollection>();

        public static bool TryGetRecordGroupId(ManualExportReportInfo reportInfo, out Guid recordGroupId)
        {
            if (!GroupMappingCache.ContainsKey(reportInfo.SiteGroupID))
            {
                AddRecordCache(reportInfo);
            }

            return GroupMappingCache.TryGetValue(reportInfo.SiteGroupID, out recordGroupId);
        }

        public static bool TryGetRecordSiteCollectionId(ManualExportReportInfo reportInfo, out Guid recordSiteCollectionId)
        {
            recordSiteCollectionId = Guid.Empty;

            if (!SiteCollectionMappingCache.ContainsKey(reportInfo.RegistedSiteId))
            {
                AddRecordCache(reportInfo);
            }

            var hasValue = SiteCollectionMappingCache.TryGetValue(reportInfo.RegistedSiteId, out var siteCollection);
            if (siteCollection != null)
            {
                recordSiteCollectionId = Guid.Parse(siteCollection.id);
            }

            return hasValue;
        }

        public static bool TryGetRecordSiteCollection(ManualExportReportInfo reportInfo, out RemoteSiteCollection recordSiteCollection)
        {
            if (!SiteCollectionMappingCache.ContainsKey(reportInfo.RegistedSiteId))
            {
                AddRecordCache(reportInfo);
            }

            return SiteCollectionMappingCache.TryGetValue(reportInfo.RegistedSiteId, out recordSiteCollection);
        }

        public static bool TryGetRecordSiteCollection(Record record,  out RemoteSiteCollection recordSiteCollection)
        {
            if (!SiteCollectionMappingCache.ContainsKey(new Guid(record.AveSiteId)))
            {
                AddRecordCache(record);
            }

            return SiteCollectionMappingCache.TryGetValue(new Guid(record.AveSiteId), out recordSiteCollection);
        }

        private static void AddRecordCache(ManualExportReportInfo reportInfo)
        {
            var recordSiteCollection = RABrowserClient.GetRemoteSiteCollectionWithBposByUrl(reportInfo.SiteUrl);
            if (recordSiteCollection == null)
            {
                Logger.Warn($"Can't loaded site collection from record by url: [{reportInfo.SiteUrl}].");
                return;
            }

            GroupMappingCache[reportInfo.SiteGroupID] = new Guid(recordSiteCollection.parentId);
            Logger.Info($"Successfule add site group mapping cache. Dao site gorup id: [{reportInfo.SiteGroupID}], Record site group id: [{recordSiteCollection.parentId}].");

            SiteCollectionMappingCache[reportInfo.RegistedSiteId] = recordSiteCollection;

            Logger.Info($"Successfule add site mapping cache. Dao site id: [{reportInfo.RegistedSiteId}], Record site id: [{recordSiteCollection.id}].");
        }

        private static void AddRecordCache(Record reportInfo)
        {
            var recordSiteCollection = RABrowserClient.GetRemoteSiteCollectionWithBposById(reportInfo.AveSiteId);

            if (recordSiteCollection == null)
            {
                Logger.Warn($"Can't loaded site collection from record by Id: [{reportInfo.AveSiteId}].");
                return;
            }
            GroupMappingCache[new Guid(reportInfo.ContainerId)] = new Guid(reportInfo.ContainerId);
            Logger.Info($"Successfule add site group mapping cache. Dao site gorup id: [{reportInfo.ContainerId}], Record site group id: [{reportInfo.ContainerId}].");

            SiteCollectionMappingCache[new Guid(reportInfo.AveSiteId)] = recordSiteCollection;

            Logger.Info($"Successfule add site mapping cache. Dao site id: [{reportInfo.AveSiteId}], Record site id: [{recordSiteCollection.id}].");
        }
    }
}
