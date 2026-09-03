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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.SharePoint.Archiver.Scan.Base;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object
{
    public class RAReturnMessage
    {
        public string ErrorMessage { get; set; }
        public RAFailedType FaildType { get; set; }
        public RAMessageType MessageType { get; set; }
        public string Extension { get; set; }
        public object Extsion1 { get; set; }
    }

    public class MAReturnMessage
    {
        [JsonProperty("errorMessage")]
        public string ErrorMessage { get; set; }

        [JsonProperty("faildType")]
        public RAFailedType FaildType { get; set; }

        [JsonProperty("messageType")]
        public RAMessageType MessageType { get; set; }

        [JsonProperty("extension")]
        public string Extension { get; set; }

        [JsonProperty("extsion1")]
        public object Extsion1 { get; set; }
    }

    public class RMDiscoveryReturnMessage
    {
        public string ErrorMessage { get; set; }
        public RAMessageType MessageType { get; set; }
        public string JobId { get; set; }
        public string SiteCollectionUrl { get; set; }
    }

    public class RMEndUserArchiveReturnMessage
    {
        public string ErrorMessage { get; set; }
        public RAMessageType MessageType { get; set; }
        public Dictionary<string, List<EndUserFileInfo>> JobInfos { get; set; }
    }

    public class RARealTimeJobMessage
    {
        public string JobId { get; set; }
        public int Status { get; set; }
        public List<string> Items { get; set; }
        public RAMessageType MessageType { get; set; }
        public string ErrorMessage { get; set; }
        public bool Waiting4EXO { get; set; }
    }
    public enum RAFailedType
    {
        None = 0,
        NameExisting = 1,
        RunningJobExist = 2,
        NoIndexDevice,
        NoDBSetting,
        NoIndexDeviceAndDBSetting,
        NoLocation,
        LicenseExpired,
        ScheduleServiceFailed = 8,
        DefaultTermIsOrphaned = 9,
        DisableRecordsManagement = 10,
        BreakFolderNode = 11,
        PhysicalMoveHasHoldConflict = 12,
        DeleteUsingSuite = 13,
        DeleteUningTemplate = 14,
        SoftDeleted = 15,
        UpdateFailed = 16,
        HasRunningWorkflowInstance = 17,
        UniqueIdSettingIsEmpty = 18,
        NotAvailableAgent = 19,
        AccessDenied = 20,
        EarlierThanNow = 21,
        MissingRequiredSettings = 22,
        SSOLoginFailed = 23,
        NotAcceptLicenseAgreement = 24,
        ParameterIsIncorrect=25,
        SecurityProfileDuplicated=26,
        IndexDeviceCannotBeDeleted=27,
        CloudArchiverLicenseExpired = 28,
        LicenseDoesNotAllowLogin = 29,
        UseCloudArchiving = 30,
        EnableInsightsDataCollection = 31,
        ArchiverMigrating = 32,
        TermNotExistAnyTenant = 33,
        AllTermGroupsHavingBothNoneOptions = 34,
        RuleLevelNotMatchNodeLevel = 35,
        BlockedByIpRestriction = 36

    }
    public enum RAMessageType
    {
        FailedWithEx = -1,
        Successful = 0,
        Failed = 1,
        Exception = 2,
        Skipped = 3,
        Confirmation = 4
    }

    public class GOReturnMessage
    {
        public string ErrorMessage { get; set; }
        public RAFailedType FaildType { get; set; }
        public RAMessageType MessageType { get; set; }
        public StorageIdAndName storageIdAndName { get; set; }
    }
    public class MyhubClassifyReturnMessage
    {
        public string Message { get; set; }
        public RAMessageType MessageType { get; set; }
        public NodeLevel Type { get; set; }
        public string Name { get; set; }
    }
}
