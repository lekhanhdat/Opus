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
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Contract.Server.StubSetting;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using Newtonsoft.Json;

namespace AvePoint.RA.Contract.Discovery.Model.Query.Progress
{
    public class RMDiscoveryProgressOptimizationPlanDetail
    {
        [JsonProperty("dataScopeInfo")]
        public RMDiscoveryProgressOptimizationPlanDataInfo DataScopeInfo { get; set; }

        [JsonProperty("objectScopeInfo")]
        public OptimizationObjectScopeInfo ObjectScopeInfo { get; set; }

        [JsonProperty("actionInfo")]
        public OptimizationActionInfo ActionInfo { get; set; }

        [JsonProperty("scheduleTime")]
        public string ScheduleTime { get; set; }

        [JsonProperty("storageName")]
        public string StorageName { get; set; }

        [JsonProperty("moveToAnotherTierType")]
        public int MoveToAnotherTierType { get; set; }
        [JsonProperty("storageDeviceUIDto")]
        public StorageDeviceUIDto StorageDeviceUIDto { get; set; }
    }

    public class OptimizationObjectScopeInfo
    {
        [JsonProperty("dataType")]
        public ArchiverDataType DataType { get; set; }

        [JsonProperty("inactiveEnable")]
        public bool InactiveEnable { get; set; }

        [JsonProperty("inactiveRules")]
        public List<string> InactiveRules { get; set; }

        [JsonProperty("rotEnable")]
        public bool RotEnable { get; set; }

        [JsonProperty("rotRules")]
        public List<string> ROTRules { get; set; }
    }

    public class OptimizationActionInfo
    {
        [JsonProperty("fileAction")]
        public FileAction FileAction { get; set; }

        [JsonProperty("versionAction")]
        public VersionAction VersionAction { get; set; }

        [JsonProperty("isEnableLeaveStub")]
        public bool IsEnableLeaveStub { get; set; }
        [JsonProperty("deleteRecords")]
        public bool DeleteRecords { get; set; }
        [JsonProperty("archiveVersionValue")]
        public string ArchivedLatestVersion { get; set; }
        [JsonProperty("deleteRecordToRecycleBin")]
        public bool DeleteRecordToRecycleBin { get; set; }
        [JsonProperty("deleteVersionToRecycleBin")]
        public bool DeleteVersionToRecycleBin { get; set; }

    }
}
