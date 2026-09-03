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
using System.Threading.Tasks;

namespace AvePoint.RA.Common.GraphApi.Tenant
{
    public class RMGraphTenantConstants
    {

        public static IReadOnlyList<string> SHAREPOINT_AVAILABLE_SUBSCRIPTION { get { return SHAREPOINT_AVAILABLE_SUBSCRIPTION_Inner.AsReadOnly(); } }

        private static readonly List<string> SHAREPOINT_AVAILABLE_SUBSCRIPTION_Inner = new List<string>()
        {
            "DESKLESSPACK_GOV",
            "ENTERPRISEPREMIUM_GOV",
            "EXCHANGE_STANDARD_ALUMNI",
            "M365_G5_GCC",
            "ENTERPRISEPREMIUM_NOPSTNCONF_USGOV_GCCHIGH",
            "EXCHANGESTANDARD_USGOV_GCCHIGH",
            "SPE_E5_USGOV_GCCHIGH",
            "SPE_E5_NOPSTNCONF",
            "M365_G3_GOV",
            "STANDARDPACK_USGOV_GCCHIGH",
            "LITEPACK_P2",
            "MIDSIZEPACK",
            "ENTERPRISEPACK",
            "ENTERPRISEPACKWSCAL",
            "DESKLESSPACK_YAMMER",
            "DESKLESSWOFFPACK",
            "STANDARDPACK_STUDENT",
            "STANDARDPACK_FACULTY",
            "EXCHANGESTANDARD_STUDENT",
            "ENTERPRISEPACK_STUDENT",
            "ENTERPRISEWITHSCAL_STUDENT",
            "ENTERPRISEPACK_FACULTY",
            "ENTERPRISEWITHSCAL_FACULTY",
            "STANDARDPACK_GOV",
            "STANDARDWOFFPACK_GOV",
            "ENTERPRISEPACK_GOV",
            "ENTERPRISEWITHSCAL_GOV",
            "DESKLESSPACK_GOV",
            "ESKLESSWOFFPACK_GOV",
            "EXCHANGESTANDARD_GOV",
            "EXCHANGEENTERPRISE_GOV",
            "SHAREPOINTENTERPRISE_GOV",
            "EXCHANGE_S_ENTERPRISE_GOV",
            "LITEPACK",
            "STANDARDPACK",
            "STANDARDWOFFPACK",
            "O365_BUSINESS_PREMIUM",
            "WACONEDRIVESTANDARD",
            "SMB_BUSINESS_PREMIUM",
            "EXCHANGESTANDARD",
            "ENTERPRISEPACKWITHOUTPROPLUS",
            "SHAREPOINTENTERPRISE",
            "EXCHANGEENTERPRISE",
            "DEVELOPERPACK",
            "O365_BUSINESS_ESSENTIALS",
            "ENTERPRISEWITHSCAL",
            "EXCHANGEENTERPRISE_FACULTY",
            "SHAREPOINTSTANDARD",
            "ENTERPRISEPREMIUM",
            "ENTERPRISEPREMIUM_NOPSTNCONF",
            "SHAREPOINTSTANDARD_YAMMER",
            "EOP_ENTERPRISE",
            "WACONEDRIVEENTERPRISE",
            "STANDARDWOFFPACK_IW_FACULTY",
            "SMB_BUSINESS_ESSENTIALS",
            "SPB",
            "O365_BUSINESS_PREMIUM_DE",
            "SPE_E3",
            "SPE_E5",
            "ENTERPRISEPACKPLUS_FACULTY",
            "M365EDU_A5_FACULTY",
            "STANDARDPACK_GOV ",
            "STANDARDWOFFPACK_GOV",
            "ENTERPRISEPACK_GOV",
            "ENTERPRISEWITHSCAL_GOV",
            "M365EDU_A3_FACULTY",
            "M365EDU_A3_STUUSEBNFT",
            "ENTERPRISEPREMIUM_FACULTY",
            "M365_G3_GCCHIGH",
            "EXCHANGEESSENTIALS",
            "EXCHANGE_S_ESSENTIALS",
            "SPE_E3_USGOV_DOD",
            "SPE_E3_USGOV_GCCHIGH",
            "ENTERPRISEPACK_USGOV_DOD",
            "ENTERPRISEPACK_USGOV_GCCHIGH",
        };
    }
}
