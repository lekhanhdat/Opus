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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.RACommonUtility.Browser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAManualApproval.ReportRelateSettingManagers
{
    public class ExchangeOnlineDaoMappingManager
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(ExchangeOnlineDaoMappingManager));

        private static readonly Dictionary<string, Guid> GroupAndSiteMappingCache = new Dictionary<string, Guid>();

        public static bool TryGetRecordGroupId(ManualExportReportInfo reportInfo, out Guid recordGroupId)
        {
            if(!GroupAndSiteMappingCache.ContainsKey(reportInfo.SiteGroupID.ToString()))
            {
                AddRecordCache(reportInfo);
            }

            return GroupAndSiteMappingCache.TryGetValue(reportInfo.SiteGroupID.ToString(), out recordGroupId);
        }

        public static bool TryGetRecordMailBoxId(ManualExportReportInfo reportInfo, out Guid recordMailBoxId)
        {
            if (!GroupAndSiteMappingCache.ContainsKey(reportInfo.SiteUrl))
            {
                AddRecordCache(reportInfo);
            }

            return GroupAndSiteMappingCache.TryGetValue(reportInfo.SiteUrl, out recordMailBoxId);
        }

        private static void AddRecordCache(ManualExportReportInfo reportInfo)
        {
            var recordMailBox = RABrowserClient.GetExchangeNodeByMailBox(reportInfo.SiteUrl);
            if (recordMailBox == null)
            {
                Logger.Warn($"Can't loaded mailbox from record by url: [{reportInfo.SiteUrl}].");
                return;
            }

            GroupAndSiteMappingCache[reportInfo.SiteGroupID.ToString()] = new Guid(recordMailBox.ParentId);
            Logger.Info($"Successfule add mailbox group mapping cache. Dao mailbox gorup id: [{reportInfo.SiteGroupID}], Record mailbox group id: [{recordMailBox.ParentId}].");

            GroupAndSiteMappingCache[reportInfo.SiteUrl] = new Guid(recordMailBox.ID);

            Logger.Info($"Successfule add mailbox mapping cache. Dao mailbox id: [{reportInfo.MailBoxID}], Record mailbox id: [{recordMailBox.ID}].");
        }
    }
}
