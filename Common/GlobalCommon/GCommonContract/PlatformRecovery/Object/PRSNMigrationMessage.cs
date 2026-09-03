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
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSNMigrationMessage : PRMultipleControlMessage
    {
        [DataMember]
        public SMSPMigrationRunDto RunDto;
        [DataMember]
        public bool IsForDatabase;
        [DataMember]
        public NeedToStopServices NeedToStopServices;
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NeedRestartServicesForPRSNMigration
    {
        [DataMember]
        public ServiceDto CurrentAgent { get; set; }
        [DataMember]
        public List<string> ServicesName { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NeedToStopServices
    {
        [DataMember]
        public bool SPAdministrationService = true;
        [DataMember]
        public bool SPUserCodeHostService = true;
        [DataMember]
        public bool SPTracing = true;
        [DataMember]
        public bool SPServerSearchService = true;
        [DataMember]
        public bool SPTimerService = true;
        [DataMember]
        public bool SPFoundationSearchServcie = true;
        [DataMember]
        public bool WebAnalyticsService = true;
        [DataMember]
        public bool IISService = false;
        [DataMember]
        public bool ForefrontIdentityManagerService = true;
        [DataMember]
        public bool ForefrontIdentityManagerSynchronizationService = true;
    }
    //[DataContract(Namespace = ContractConstants.Namespace)]
    //public class PRSNMigrationInstanceInfo
    //{
    //    [DataMember]
    //    public string InstanceName;
    //    [DataMember]
    //    public string SnapInfo;
    //    [DataMember]
    //    public string Status;
    //}
}
