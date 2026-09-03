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
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace AvePoint.RA.Contract.Discovery.Model.Configuration.Office365
{
    [DataContract]
    public class RMDiscoveryOffice365OptimizationSetting
    {
        [DataMember]
        [JsonProperty("archiveDataType")]
        public int ArchiveDataType { get; set; }
        [DataMember]
        [JsonProperty("ms365DataType")]
        public int MS365DataType { get; set; }
        [DataMember]
        [JsonProperty("dataType")]
        public int DataType { get; set; }
        [DataMember]
        [JsonProperty("selectedStorageParameter")]
        public StorageDeviceUIDto SelectedStorage { get; set; }
        [DataMember]
        [JsonProperty("nodeQueryParameter")]
        public RMDiscoveryOffice365NodeQueryParameter NodeQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("o365TenantId")]
        public string O365TenantId { get; set; }
        [DataMember]
        [JsonProperty("sizeRangeQueryParameter")]
        public RMDiscoveryOffice365SizeRangeQueryParameter SizeRangeQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("withoutDateQueryParameter")]
        public RMDiscoveryOffice365WithoutDateQueryParameter WithoutDateQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("fileExtensionQueryParameter")]
        public RMDiscoveryOffice365FileExtensionQueryParameter FileExtensionQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("scheduleParameter")]
        public ScheduleParameter ScheduleParameter { get; set; }
        [DataMember]
        [JsonProperty("processActionParameter")]
        public ProcessActionParameter ProcessActionParameter { get; set; }
        [DataMember]
        [JsonProperty("inactiveRuleQueryParameter")]
        public InactiveRuleQueryParameter InactiveRuleQueryParameter { get; set; }
        [DataMember]
        [JsonProperty("rotRuleQueryParameter")]
        public ROTRuleQueryParameter ROTRuleQueryParameter { get; set; }
        [DataMember]
        public List<int> NodeIds { get; set; }
        [DataMember]
        public long NextTime { get; set; }
        [DataMember]
        [JsonProperty("moveToAnotherTierType")]
        public int MoveToAnotherTierType { get; set; }

        public static string XMLCompatibleConvert(string xml)
        {
            if (xml.Contains("RMDiscoveryOffice365OptimizationSetting"))
            {
                return xml;
            }
            xml = xml.Replace("RMDiscoveryOptimizationSetting", "RMDiscoveryOffice365OptimizationSetting")
                .Replace("AvePoint.RA.Contract.Discovery.Model.Configuration", "AvePoint.RA.Contract.Discovery.Model.Configuration.Office365")
                .Replace("AvePoint.RA.Contract.Discovery.Model.Query.Parameter", "AvePoint.RA.Contract.Discovery.Model.Query.Office365.Parameter");
            return xml;
        }
    }
    [DataContract]
    public class ScheduleParameter
    {
        [DataMember]
        [JsonProperty("scheduleType")]
        public ScheduleType ScheduleType { get; set; }
        [DataMember]
        [JsonProperty("selectedDate")]
        public DateTime StartTime { get; set; }
        [DataMember]
        [JsonProperty("selectedTime")]
        public DateTime SelectedTime { get; set; }
        [DataMember]
        [JsonProperty("timeZoneId")]
        public string TimeZoneId { get; set; }
    }
    [DataContract]
    public class ROTRuleQueryParameter
    {
        [DataMember]
        [JsonProperty("ruleCategories")]
        public List<RMDiscoveryROTRuleCategoryQueryParameter> RuleCategories { get; set; } = new();
        [DataMember]
        [JsonProperty("enable")]
        public bool Enable { get; set; }
    }
    [DataContract]
    public class InactiveRuleQueryParameter
    {
        [DataMember]
        [JsonProperty("ruleIds")]
        public List<int> RuleIds { get; set; }
        [DataMember]
        [JsonProperty("enable")]
        public bool Enable { get; set; }
    }
    [DataContract]
    public class ProcessActionParameter
    {
        [DataMember]
        [JsonProperty("archiveOrRemoveFile")]
        public FileAction FileAction { get; set; }
        [DataMember]
        [JsonProperty("archiveOrRemoveVersion")]
        public VersionAction VersionAction { get; set; }
        [DataMember]
        [JsonProperty("isEnableLeaveStub")]
        public bool IsEnableLeaveStub { get; set; }
        [DataMember]
        [JsonProperty("selectedLevelStub")]
        public StubSettingUIDto StubSettingDto { get; set; }
        [DataMember]
        [JsonProperty("deleteRecords")]
        public bool DeleteRecords { get; set; }
        [DataMember]
        [JsonProperty("deleteRecordToRecycleBin")]
        public bool DeleteRecordToRecycleBin { get; set; }
        [DataMember]
        [JsonProperty("deleteVersionToRecycleBin")]
        public bool DeleteVersionToRecycleBin { get; set; }
        [DataMember]
        [JsonProperty("archiveVersionValue")]
        public int ArchivedLatestVersion { get; set; }
        [DataMember]
        [JsonProperty("isArchiveVersionOption")]
        public bool EnableArchivedLatestVersion { get; set; }
        [DataMember]
        [JsonProperty("archiverOnlyLastestVersion")]
        public int ArchivedOnlyLatestVersion { get; set; }
        [DataMember]
        [JsonProperty("isArchiveOnlyVersionOption")]
        public bool EnableArchivedOnlyLatestVersion { get; set; }
    }
    [DataContract]
    public enum ScheduleType
    {
        [EnumMember]
        Now = 1,
        [EnumMember]
        Date = 2
    }
    [DataContract]
    public enum FileAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ArchiveAndRemove = 1,
        [EnumMember]
        Remove = 2,
        [EnumMember]
        Archive = 3,
    }
    [DataContract]
    public enum VersionAction
    {
        [EnumMember]
        None = 0,
        [EnumMember]
        ArchiveAndRemoveVerison = 1,
        [EnumMember]
        RemoveVersion = 2
    }
    public enum DiscoverOptimizationScheduleStatus
    {
        Ready = 1,
        Finish = 2
    }
    public enum ArchiverDataType
    {
        All = 1,
        Special = 2,
        Phl = 3
    }

    public enum MS365DataType
    {
        None = 0,
        Default = 1,
        Phl = 2
    }
}
