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
using AvePoint.RA.Contract.Discovery.DiscoveryPlan;
using AvePoint.RA.Contract.Discovery.Model.Rule;
using AvePoint.RA.Contract.Schedule;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text.Json;
using AvePoint.GCommon.Contract.Server.StubSetting;

namespace AvePoint.RA.Contract.Discovery.Model.PlanProfile
{
    [DataContract]
    public class RMDiscoveryPlanProfileInfo
    {
        [DataMember]
        [JsonProperty("id")]
        public int Id { get; set; }

        [DataMember]
        [JsonProperty("name")]
        public string Name { get; set; }

        [DataMember]
        [JsonProperty("siteMappings")]
        public List<SiteMappingRequest> SiteMappings { get; set; } = new List<SiteMappingRequest>();

        [DataMember]
        [JsonProperty("totalMappingSites")]
        public int TotalMappingSites { get; set; }

        [DataMember]
        [JsonProperty("criteriaInfoes")]
        public List<RMDiscoveryRuleCriteriaInfo> CriteriaInfoes { get; set; }

        [DataMember]
        [JsonProperty("action")]
        public RMDiscoveryPlanAction Action { get; set; }

        [DataMember]
        [JsonProperty("actionOptions")]
        public RMDiscoveryPlanActionOptions ActionOptions { get; set; }

        [DataMember]
        [JsonProperty("stubSetting")]
        public StubSettingUIDto StubSetting { get; set; }

        [DataMember]
        [JsonProperty("previousVersion")]
        public int PreviousVersion { get; set; }

        [DataMember]
        [JsonProperty("extension1")]
        public string Extension1 { get; set; }

        [DataMember]
        [JsonProperty("extension2")]
        public string Extension2 { get; set; }

        [DataMember]
        [JsonProperty("storageLocationId")]
        public string StorageLocationId { get; set; }

        [DataMember]
        [JsonProperty("storageName")]
        public string StorageName { get; set; }

        [DataMember]
        [JsonProperty("scheduleSetting")]
        public RMDiscoveryPlanScheduleInfo ScheduleSetting { get; set; }
    }

    [DataContract]
    public class RMDiscoveryPlanScheduleInfo
    {
        [DataMember]
        [JsonProperty("id")]
        public string Id { get; set; }

        [DataMember]
        [JsonProperty("noSchedule")]
        public bool NoSchedule { get; set; }

        [DataMember]
        [JsonProperty("startTime")]
        public string StartTime { get; set; }

        [DataMember]
        [JsonProperty("endTime")]
        public string EndTime { get; set; }

        [DataMember]
        [JsonProperty("nextTime")]
        public DateTime NextTime { get; set; }

        [DataMember]
        [JsonProperty("timeZoneId")]
        public string TimeZoneId { get; set; }

        [DataMember]
        [JsonProperty("isDaylightSaving")]
        public bool IsDaylightSaving { get; set; }

        [DataMember]
        [JsonProperty("endType")]
        public EndType EndType { get; set; }

        [DataMember]
        [JsonProperty("occurrencesTotal")]
        public int OccurrencesTotal { get; set; }

        [DataMember]
        [JsonProperty("occurrences")]
        public int Occurrences { get; set; }

        [DataMember]
        [JsonProperty("interval")]
        public int Interval { get; set; }

        [DataMember]
        [JsonProperty("intervalType")]
        public IntervalType IntervalType { get; set; }

        [DataMember]
        [JsonProperty("dayOfMonth")]
        public int DayOfMonth { get; set; }

        [DataMember]
        [JsonProperty("weekType")]
        public AvePoint.RA.Contract.Schedule.DayOfWeek WeekType { get; set; }
    }

    [DataContract]
    public class RMDiscoveryPlanSiteMappingDto
    {
        [DataMember]
        [JsonProperty("type")]
        public int Type { get; set; }

        [DataMember]
        [JsonProperty("siteMappings")]
        public List<SiteMappingRequest> SiteMappings { get; set; } = new List<SiteMappingRequest>();
    }

    [DataContract]
    public class RMDiscoveryPlanProfilePageInfo
    {
        [DataMember]
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [DataMember]
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [DataMember]
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [DataMember]
        [JsonProperty("items")]
        public List<RMDiscoveryPlanProfileInfo> Items { get; set; } = new List<RMDiscoveryPlanProfileInfo>();
    }

    [DataContract]
    public class RMDiscoveryPlanProfilePageRequest
    {
        [DataMember]
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [DataMember]
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [DataMember]
        [JsonProperty("sortBy")]
        public string SortBy { get; set; }

        [DataMember]
        [JsonProperty("isDesc")]
        public bool IsDesc { get; set; }
            
        [DataMember]    
        [JsonProperty("searchValue")]
        public string SearchName { get; set; }
    }

    [DataContract]
    public class RMRemoteSiteCollectionPageInfo
    {
        [DataMember]
        [JsonProperty("totalCount")]
        public int TotalCount { get; set; }

        [DataMember]
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; }

        [DataMember]
        [JsonProperty("pageSize")]
        public int PageSize { get; set; }

        [DataMember]
        [JsonProperty("items")]
        public List<RemoteSiteCollection> Items { get; set; } = new List<RemoteSiteCollection>();
    }

    [DataContract]
    public class RMRemoteSiteCollectionPageRequest
    {
        [DataMember]
        [JsonProperty("pageIndex")]
        public int PageIndex { get; set; } = 1;

        [DataMember]
        [JsonProperty("pageSize")]
        public int PageSize { get; set; } = 20;

        [DataMember]
        [JsonProperty("key")]
        public string Key { get; set; }

        [DataMember]
        [JsonProperty("planProfileId")]
        public int PlanProfileId { get; set; }
    }

    [DataContract]
    public class SiteMappingRequest
    {
        [DataMember]
        [JsonProperty("siteId")]
        public string SiteId { get; set; }

        [DataMember]
        [JsonProperty("isAdd")]
        public bool IsAdd { get; set; }
    }
}