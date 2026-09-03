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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP
{
    [DataContract]
    public class RMDiscoveryAOSPOptimizationSetting
    {
        [DataMember]
        [JsonProperty("archiveDataType")]
        public int ArchiveDataType { get; set; }
        [DataMember]
        [JsonProperty("dataType")]
        public int DataType { get; set; }
        [DataMember]
        [JsonProperty("selectedStorageParameter")]
        public AOSPStorageDeviceUIDto SelectedStorage { get; set; }
        [DataMember]
        [JsonProperty("nodeQueryParameter")]
        public RMDiscoveryAOSPNodeQueryParameter NodeQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("o365TenantId")]
        public string O365TenantId { get; set; }
        [DataMember]
        [JsonProperty("logonUserName")]
        public string LogonUserName { get; set; }
        [DataMember]
        [JsonProperty("sizeRangeQueryParameter")]
        public RMDiscoveryAOSPSizeRangeQueryParameter SizeRangeQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("withoutDateQueryParameter")]
        public RMDiscoveryAOSPWithoutDateQueryParameter WithoutDateQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("fileExtensionQueryParameter")]
        public RMDiscoveryAOSPFileExtensionQueryParameter FileExtensionQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("scheduleParameter")]
        public ScheduleParameter ScheduleParameter { get; set; }
        [DataMember]
        [JsonProperty("processActionParameter")]
        public ProcessActionParameter ProcessActionParameter { get; set; }
        [DataMember]
        [JsonProperty("enableRetainArchivedData")]
        public bool EnableRetainArchivedData { get; set; } = false;
        [DataMember]
        [JsonProperty("retentionDataTimeType")]
        public KeepDateType RetentionDataTimeType { get; set; } = KeepDateType.ArchiveTime;
        [DataMember]
        [JsonProperty("retentionKeepValue")]
        public int RetentionKeepValue { get; set; } = 1;
        [DataMember]
        [JsonProperty("retentionKeepUnit")]
        public TimeUnit RetentionKeepUnit { get; set; } = TimeUnit.Year;
        [DataMember]
        [JsonProperty("removeRelatedJobsFromJobMonitor")]
        public bool RemoveRelatedJobsFromJobMonitor { get; set; } = true;
        [DataMember]
        [JsonProperty("deleteRelatedStubsFromOriginalLocations")]
        public bool DeleteRelatedStubsFromOriginalLocations { get; set; } = false;
        [DataMember]
        [JsonProperty("inactiveRuleQueryParameter")]
        public InactiveRuleQueryParameter InactiveRuleQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("rotRuleQueryParameter")]
        public ROTRuleQueryParameter ROTRuleQueryParameter { get; set; }
        [DataMember]
        public List<string> NodeIds { get; set; }
        [DataMember]
        [JsonProperty("siteInfos")]
        public List<SiteInfo> SiteInfos { get; set; }
        [DataMember]
        public long NextTime { get; set; }
        [DataMember]
        [JsonProperty("moveToAnotherTierType")]
        public int MoveToAnotherTierType { get; set; }

        [DataMember]
        public string AppProfileId { get; set; }

        [DataMember]
        public string SiteAdminUrl { get; set; }

        [DataMember]
        public string StorageId { get; set; }
        [DataMember]
        public List<RMDiscoveryRuleDefinition> RuleDefinition { get; set; }
        [DataMember]
        public bool UseArchiverProfile { get; set; }
        [DataMember]
        public string ArchiverProfileId { get; set; }
        [DataMember]
        public string ArchiverProfileName { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        [JsonProperty("supportLockedSite")]
        public bool SupportLockedSite { get; set; }

        public static string XMLCompatibleConvert(string xml)
        {
            if (xml.Contains("RMDiscoveryAOSPOptimizationSetting"))
            {
                return xml;
            }
            xml = xml.Replace("RMDiscoveryOptimizationSetting", "RMDiscoveryAOSPOptimizationSetting")
                .Replace("AvePoint.RA.Contract.Discovery.Model.Configuration", "AvePoint.RA.Contract.Discovery.Model.Configuration.AOSP")
                .Replace("AvePoint.RA.Contract.Discovery.Model.Query.Parameter", "AvePoint.RA.Contract.Discovery.Model.Query.AOSP.Parameter");
            return xml;
        }
    }

    public class AOSPStorageDeviceUIDto
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class SiteInfo
    {
        [DataMember]
        [JsonProperty("siteUrl")]
        public string SiteUrl { get; set; }
        [DataMember]
        [JsonProperty("siteId")]
        public string SiteId { get; set; }
    }

    public enum ArchiverDataType
    {
        All = 1,
        Special = 2
    }
}
