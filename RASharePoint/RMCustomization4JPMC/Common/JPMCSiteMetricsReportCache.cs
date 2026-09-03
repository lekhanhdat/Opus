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
using System.Collections.Generic;

namespace AvePoint.RA.SharePoint.RMCustomization4JPMC.Common
{
    class JPMCSiteMetricsReportCache
    {
        public const string ConfigListName = "OpusAppConfig";
        public const string JPMCExcelJsonConfigFileName = "opus_site_metrics_report_config.json";

        public static readonly List<string> SiteStatsConfigKeys =
        [
            "Site ID",
            "Site Name",
            "Site URL",
            "Site Status",
            "Information Owner",
            "Alternate Owner",
            "Data Classification",
            "Site Template",
            "OBR",
            "LOB1",
            "LOB2",
            "LOB3",
            "LOB4",
            "Total Libraries",
            "Total Active Records",
            "Total Archived Records",
            "Total Destroyed Records",
            "Total Managed Records"
        ];

        public static readonly List<string> LibrariesConfigKeys =
        [
            "Site ID",
            "Library Name",
            "Library URL",
            "Library Type",
            "Total Active Records",
            "Total Archived Records",
            "Total Destroyed Records",
            "Total Managed Records"
        ];

        public static readonly List<string> DERsConfigKeys =
        [
            "Site ID",
            "Site URL",
            "Information Owner",
            "Alternate Owner",
            "RCC Country",
            "RCC",
            "Record Status",
            "LOB1",
            "LOB2",
            "LOB3",
            "LOB4",
            "Total Active Records",
            "Total Record Volume(GB)",
            "Total Records Eligible Destruction Today(Count)",
            "Total Records Eligible Destruction Today Volume(GB)",
            "Total Records Eligible Disposed Till Date(Count)",
            "Total Records Eligible Disposed Till Date Volume(GB)",
            "Record Pending Approval  (0-60 Days)",
            "Record Pending Approval  (60-90 Days)",
            "Record Pending Approval  (90 -180 Days)",
            "Record Pending Approval  (180-365 Days)",
            "Record Pending Approval  (>365 Days)"
        ];

        public static readonly List<string> RCCsConfigKeys =
        [
            "Site ID",
            "Site URL",
            "Information Owner",
            "Alternate Owner",
            "RCC Country",
            "Record Class code(Term)",
            "RCC Status",
            "LOB1",
            "LOB2",
            "LOB3",
            "LOB4",
            "Record Count"
        ];

        public static readonly List<string> AllSitesConfigKeys =
        [
            "Site ID",
            "Site URL",
            "Site Status",
            "Site Name",
            "Site Description",
            "Site Owner SID",
            "Site Owner Name",
            "Alternate Owner SID",
            "Alternate Owner Name",
            "Developer SID",
            "JADE Classification",
            "Template",
            "Quota",
            "Size [MB]",
            "Version",
            "Site Creation Date",
            "Cost Center",
            "Sort Code",
            "Billing Cost Center",
            "Billing Sort Code",
            "Allow Cross LOB Cost Center",
            "Regulatory Compliance",
            "OBR",
            "Last Attested",
            "LOB1",
            "LOB2",
            "LOB3",
            "LOB4",
            "EEA Nexus",
            "Process PI",
            "PI Source",
            "SEAL ID",
            "Controller-Processor",
            "Countries",
            "Business Purpose",
            "Legal Base(s)",
            "Categories of PI Processed",
            "Category of Workforce",
            "Category of Individuals",
            "Category of Corporate Client/Service Provider",
            "Categories of Internal Recipients of Data",
            "Categories of External Recipient of Data",
            "Librarians",
            "Site Admins",
            "Valid Attestation",
            //"Extended property 01",
            //"Extended property 02",
            //"Extended property 03",
            //"Extended property 04",
            //"Extended property 05"
        ];
    }
}
