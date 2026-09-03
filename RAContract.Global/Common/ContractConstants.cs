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
using System.Linq;

namespace AvePoint.RA.Contract.Common
{
    public class ContractConstants
    {
        public const string Namespace = "http://www.avepoint.com";
        public const string HybridAgentScope = "hybridrecord.readwrite.all";
        public const string HybridInernalScope = "records.common.readwrite.all";
        public const string RecordsPublicScope = "records.readwrite.all";
        public const string Product_Name = "HybridAgent";
        public const string UniqueId_DefaultPrefix = "REC";
        public const string DATETYPEForAPI003 = "yyyy-MM-dd HH:mm";
        /// <summary>
        /// JS 通用datetime传输格式
        /// </summary>
        public const string DEFAULT_TIME_FORMAT = "yyyy/MM/dd HH:mm:ss";
        public const string Records_Processor_Name = "RecordsProcessor.exe";
        public const string RECORDS_HYBRID_NAME = "HybridAgent";

        public static readonly IReadOnlyCollection<string> ENVIRONMENT_NAME_GCP = new HashSet<string> { "gcp", "gcp test"};

        public const string SHAREPOINT_SITECOLUMN_SPACE_ESCAPE_CHARACTER = "_x0020_";

        public const long ITEMSIZEFORLICENSE = 1048576;//1m
        public const long STUBPREVIEWSIZE = 10485760;//10m
        public const long GBSizeInterval = 1073741824;

    }
}
