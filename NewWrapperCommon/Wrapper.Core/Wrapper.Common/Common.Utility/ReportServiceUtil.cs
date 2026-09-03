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
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    static class ReportServiceUtil
    {
        static Dictionary<ReportFileType, IAveContentTypeId> reportFilePair;
        internal class BuiltinContentTypeId
        {
            #region SQL Server Reporting Services Content Types
            public const string ReportBuilderReport = "0x010100C3676CDFA2F24E1D949A8BF2B06F6B8B";
            public const string ReportDataSource = "0x0101007DFDDF56F8A9492DAA9366B83A95B3A0";
            public const string ReportBuilderModel = "0x010100D8704AF8ED734F4088724751E0F2727D";
            #endregion

            #region PerformancePoint
            public const string PerformancePointDataSource = "0x0101004C06BE72B56941358D9BD0B31603EC4D01";

            //public const string PerformancePointBase = "0x01002DDC53CB1D5F4520BE0568558051291F";
            //public const string PerformancePointKPI = "0x01002DDC53CB1D5F4520BE0568558051291F01";
            //public const string PerformancePointScorecard = "0x01002DDC53CB1D5F4520BE0568558051291F02";
            //public const string PerformancePointIndicator = "0x01002DDC53CB1D5F4520BE0568558051291F03";
            //public const string PerformancePointReport = "0x01002DDC53CB1D5F4520BE0568558051291F04";
            //public const string PerformancePointFilter = "0x01002DDC53CB1D5F4520BE0568558051291F05";
            //public const string PerformancePointDashboard = "0x01002DDC53CB1D5F4520BE0568558051291F06";
            #endregion
        }
        static ReportServiceUtil()
        {
            reportFilePair = new Dictionary<ReportFileType, IAveContentTypeId>()
            {
                { ReportFileType.RSDS, WrapperRuntime.CurrentContext.ModelFactory.CreateContentTypeId(BuiltinContentTypeId.ReportDataSource) },
                { ReportFileType.RDL, WrapperRuntime.CurrentContext.ModelFactory.CreateContentTypeId(BuiltinContentTypeId.ReportBuilderReport) },
                { ReportFileType.PPSDC, WrapperRuntime.CurrentContext.ModelFactory.CreateContentTypeId(BuiltinContentTypeId.PerformancePointDataSource) },
            };
        }
        private static bool MatchContentTypes(this ReportFileType type, IAveContentTypeId ctId)
        {
            IAveContentTypeId parentCTId;
            return reportFilePair.TryGetValue(type, out parentCTId)
                && ctId.IsChildOf(parentCTId);
        }

        public static ReportFileType GetReportFileType(string fileName, IAveContentTypeId ctId)
        {
            if (string.IsNullOrEmpty(fileName) || ctId == null) return ReportFileType.None;

            ReportFileType result;
            if (TryConvertToReportFileType(fileName, out result)
                && result.MatchContentTypes(ctId))
            {
                return result;
            }
            return ReportFileType.None;
        }

        private static bool TryConvertToReportFileType(string fileName, out ReportFileType type)
        {
            var fileExtension = System.IO.Path.GetExtension(fileName).TrimStart(".".ToArray());
            return EnumExtension.TryParse(fileExtension, true, out type);
        }
    }

    enum ReportFileType
    {
        None = 0,
        RSDS,
        RDL,
        PPSDC,
    }
}
