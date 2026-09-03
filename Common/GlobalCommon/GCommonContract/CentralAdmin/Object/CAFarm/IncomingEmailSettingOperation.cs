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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class IncomingEmailSettingOperation : CAOperation
    {
        [DataMember]
        public bool SMTPServiceInstalled { get; set; }
        [DataMember]
        public bool Enabled { get; set; }
        [DataMember]
        public bool UseAutomaticSetting { get; set; }
        [DataMember]
        public DirectoryManagementServiceMode DirectoryManagementServiceMode { get; set; }        
        [DataMember]
        public String ADContainer { get; set; }
        [DataMember]
        public String DirectoryManagementServiceUrl { get; set; }
        [DataMember]
        public String ServerAddress { get; set; }
        [DataMember]
        public String SmtpMailServer { get; set; }
        [DataMember]
        public bool DLsRequireAuthenticatedSenders { get; set; }
        [DataMember]
        public bool DistributioinGroupsEnabled { get; set; }
        [DataMember]
        public bool RequireCreateDLApproval { get; set; }
        [DataMember]
        public bool RequireRenameDlApproval { get; set; }
        [DataMember]
        public bool RequireModifyDLApproval { get; set; }
        [DataMember]
        public bool RequireDeleteDLApproval { get; set; }
        [DataMember]
        public String ServerDisplayAddress { get; set; }
        [DataMember]
        public String DropFolder { get; set; }
        [DataMember]
        public bool AcceptMailFromAllServers { get; set; }
        [DataMember]
        public List<string> SafeMailServers { get; set; }
        [DataMember]
        public bool AutomaticDisabled { get; set; }
        [DataMember]
        public bool HasSMTPService { get; set; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum DirectoryManagementServiceMode
    {
        [EnumMember]
        Yes,
        [EnumMember]
        No,
        [EnumMember]
        UseRemote
    }
}
