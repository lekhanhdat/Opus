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




namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.Common;
    using AvePoint.GCommon.Contract.Server.Common.BackupDataSearch;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion

    [KnownType(typeof(PRStagingPolicyGUIMessge))]
    [KnownType(typeof(PRRestoreGUIMessge))]
    [KnownType(typeof(PRAdvanceSearchGUIMessge))]
    [KnownType(typeof(PRStoragePolicyGUIMessge))]
    [KnownType(typeof(PRRestorePlanGUIMessge))]
    [KnownType(typeof(PRBackupGUIMessage))]
    [KnownType(typeof(PRKeepLiveGUIMessage))]
    [KnownType(typeof(PRDataManagerMessage))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRGUIMessge
    {
        [DataMember]
        public string FarmID { get; set; }

        [DataMember]
        public string FarmName { get; set; }

        [DataMember]
        public string JobId { get; set; }

        // 平台信息
        [DataMember]
        public PRPlatformType PlatformType { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRKeepLiveGUIMessage : PRGUIMessge
    {
        [DataMember]
        public string Key { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRDataManagerMessage : PRGUIMessge
    {
        [DataMember]
        public PlatformType MediaPlateformType;
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRBackupGUIMessage : PRGUIMessge
    { 
        [DataMember]
        public bool IsPlanTemplate { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRStagingPolicyGUIMessge : PRGUIMessge
    {
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRRestorePlanGUIMessge : PRGUIMessge
    {
        [DataMember]
        public List<NameAndIdDto> AllLanguageMappingSettings { get; set; }

        [DataMember]
        public List<NameAndIdDto> AllUserMappingSettings { get; set; }

        [DataMember]
        public List<NameAndIdDto> AllDomainMappingSettings { get; set; }

        
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRStoragePolicyGUIMessge : PRGUIMessge
    {
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRAdvanceSearchGUIMessge : PRGUIMessge
    {
        [DataMember]
        public BackupDataSearchContract SearchContract { get; set; }

        [DataMember]
        public bool IsLiveMode { get; set; }

        [DataMember]
        public PRStagingPolicyDto StagingPolicy { get; set; }

        [DataMember]
        public PRTreeNodeDto Node { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRRestoreGUIMessge : PRGUIMessge
    {
        [DataMember]
        public string ErrorString { get; set; }

        [DataMember]
        public Dictionary<string, List<SimpleDataDto>> FarmAndPlanNames { get; set; }

        [DataMember]
        public List<PRRestoreRecordDto> SearchDataList { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRCustomActionInfo
    {
        [DataMember]
        public string CustomActionFilePath { get; set; }
        [DataMember]
        public string CustomActionDescription { get; set; }
        [DataMember]
        public List<string> CustomActionArguments { get; set; }
        [DataMember]
        public bool ExistData { get; set; }
        [DataMember]
        public string LogicalDeviceName { get; set; } 
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRCheckRunningWaitingPlanMessage
    {
        [DataMember]
        public List<string> RunningPlanList { get; set; }
        [DataMember]
        public List<string> WaitingPlanList { get; set; }
        [DataMember]
        public bool IsCheckPassed { get; set; }
    }
}
