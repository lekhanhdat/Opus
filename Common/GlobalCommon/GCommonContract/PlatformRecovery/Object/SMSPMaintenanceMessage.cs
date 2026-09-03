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
using AvePoint.GCommon.Contract.Media.TCPRequest.Backup;
using AvePoint.GCommon.Contract.PlatformRecovery.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.PlatformRecovery.PRSNMaintenance
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PRSNMaintenanceMessage : PRMultipleControlMessage
    {
        [DataMember]
        public bool IsOffLine { get; set; }
        [DataMember]
        public bool IsVerifyJob { get; set; }
        [DataMember]
        public PRSNMaintenanceOptionDto PRSNMaintenanceOption { get; set; }
        [DataMember]
        public DatabaseGroups DbGroups = new DatabaseGroups();
        [DataMember]
        public Dictionary<string, PlatformBackupRequest> ConfigForMediaList = new Dictionary<string,PlatformBackupRequest>();      
        [DataMember]
        public PRMaintenanceJobDto MaintenanceJob { get; set; }
        [DataMember]
        public Dictionary<string, PRBackupJobDto> JobList = new Dictionary<string,PRBackupJobDto>();        
        [DataMember]
        public List<string> VerifyJobList = new List<string>();          
        [DataMember]
        public List<string> IndexJobList = new List<string>();
        [DataMember]
        public Dictionary<string, ServiceDto> MediaList = new Dictionary<string, ServiceDto>();
    }
}