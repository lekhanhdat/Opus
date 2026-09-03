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

using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.RACommonUtility;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.SharePoint.RelatedRecords;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Extension
{
    public static class DueDisposalReportExtension
    {
        private static RALogger mLog = RALogger.GetInstance(typeof(DueDisposalReportExtension));
        public static void BuildRelatedRecords(this DueDisposalReport report, IAveListItem item, string siteUrl, RelatedRecordOption relatedRecords)
        {
            try
            {
                using (PerformanceScope scope6 = new PerformanceScope("DueDisposalReportProcessor.RelatedRecord", addToStatistics:true))
                {
                    List<ReportRelatedRecords> allSourceReportRelatedRecords = new List<ReportRelatedRecords>();
                    List<ReportRelatedRecords> electronicReportRelatedRecords = new List<ReportRelatedRecords>();
                    List<ReportRelatedRecords> physicalReportRelatedRecords = new List<ReportRelatedRecords>();
                    var relatedUtil = new RelatedRecordsUtility();
                    var rProps = relatedUtil.GetRelatedProperties(item);
                    if (rProps != null && rProps.Count > 0)
                    {
                        foreach (var rProp in rProps)
                        {
                            if (rProp.SourceFlag == (int)SourceFlag.Physical)
                            {
                                physicalReportRelatedRecords.Add(new ReportRelatedRecords() { Name = rProp.recId, Url = "" });
                            }
                            else
                            {
                                string itemFullUrl = string.Empty;
                                if (!rProp.url.StartsWith(siteUrl))
                                {
                                    itemFullUrl = AvePoint.RA.Common.Util.WebUtil.MakeFullUrl(siteUrl, rProp.url);
                                }
                                else
                                {
                                    itemFullUrl = rProp.url;
                                }
                                electronicReportRelatedRecords.Add(new ReportRelatedRecords() { Name = rProp.name, Url = itemFullUrl });
                            }
                        }
                        allSourceReportRelatedRecords.AddRange(electronicReportRelatedRecords);
                        allSourceReportRelatedRecords.AddRange(physicalReportRelatedRecords);
                        report.RelatedRecords = SerializerHelper.SerializeToXmlString(allSourceReportRelatedRecords);
                    }
                }
            }
            catch (Exception e)
            {
                mLog.Warn("get related record info error{0}", e.ToString());
            }
            report.RelatedRecordsAction = (int)relatedRecords;

        }
    }
}
