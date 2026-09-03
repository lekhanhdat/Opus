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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.PlatformRecovery.Object
{
    [KnownType(typeof(SMSPMigrationRunDto))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SMSPMigrationRunDto
    {
        [DataMember]
        public string FarmId { get; set; }
        [DataMember]
        public ServiceDto MemberAgent { get; set; }
        [DataMember]
        public List<PRSNMigrationInstanceInfo> ObjectInfo { get; set; }
        [DataMember]
        public PRRunSNMigrationStep MigrationStep { get; set; }
        [DataMember]
        public NeedRestartServicesForPRSNMigration AgentServices { get; set; }
    }
    
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PRRunSNMigrationStep
    {
        [EnumMember]
        StopServices = 0,
        [EnumMember]
        RunMigration = 1,
        [EnumMember]
        StartServices = 2,
        [EnumMember]
        FinishedMigration = 4
    }
}
