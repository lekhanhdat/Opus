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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.JPMC
{
    //public class JPMCEnableConfig
    //{
    //    [JsonProperty(PropertyName = "tenantConfigs")]
    //    public List<JPMCTenantConfig> TenantConfigs { get; set; }
    //}
    public class JPMCTenantConfig
    {

        [JsonProperty(PropertyName = "configSiteUrl")]
        public string ConfigSiteUrl { get; set; }

        [JsonProperty(PropertyName = "customColumns")]
        public JPMCCustomColumns CustomColumns { get; set; }

        [JsonProperty(PropertyName = "siteTypePropertyName")]
        public string SiteTypePropertyName { get; set; }

        [JsonProperty(PropertyName = "provisionedByGAOPropertyName")]
        public string ProvisionedByGAOPropertyName { get; set; }

        [JsonIgnore]
        public string M365TenantId { get; set; }

        [JsonIgnore]
        public RemoteSiteCollection ConfigSite { get; set; }
    }

    public class JPMCCustomColumns
    {
        [JsonProperty(PropertyName = "recordStatus")]
        public string RecordStatus { get; set; }

        [JsonProperty(PropertyName = "countryCode")]
        public string CountryCode { get; set; }

        [JsonProperty(PropertyName = "retentionType")]
        public string RetentionType { get; set; }

        [JsonProperty(PropertyName = "startDate")]
        public string StartDate { get; set; }

        [JsonProperty(PropertyName = "endDate")]
        public string EndDate { get; set; }

        [JsonProperty(PropertyName = "classCode")]
        public string ClassCode { get; set; }
    }

    public class TermAdvanceSettings
    {
        [JsonProperty("recordStatus")]
        public string RecordStatus { get; set; }
        
        [JsonProperty("siteType")]
        public string SiteType { get; set; }
    }
}
