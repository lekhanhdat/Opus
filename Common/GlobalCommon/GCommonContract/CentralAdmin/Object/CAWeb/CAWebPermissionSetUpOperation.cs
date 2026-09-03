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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.SharePointBrowser.Object;

namespace AvePoint.Adonis.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAPermissionSetupOperation : CAOperation
    {
        [DataMember]
        public CAPermSetupSetParam VisitorsNewGroup { get; set; }
        [DataMember]
        public CAPermSetupSetParam MembersNewGroup { get; set; }
        [DataMember]
        public CAPermSetupSetParam OwnersNewGroup { get; set; }
        // For GUI
        public string ParentNodeId { get; set; }
        [DataMember]
        public CAPermSetupWebInfo WebInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAPermSetupGetParam
    {
        [DataMember]
        public string DefaultVisitorsNewGroup { get; set; }
        [DataMember]
        public string DefaultMembersNewGroup { get; set; }
        [DataMember]
        public string DefaultOwnersNewGroup { get; set; }
        [DataMember]
        public PermSetupGroup ExistingVisitorsNewGroup { get; set; }
        [DataMember]
        public PermSetupGroup ExistingMembersNewGroup { get; set; }
        [DataMember]
        public PermSetupGroup ExistingOwnersNewGroup { get; set; }

        // For GUI
        public string ParentNodeId { get; set; }
        [DataMember]
        public CAPermSetupWebInfo WebInfo { get; set; }
        [DataMember]
        public Dictionary<PermSetupGroup, bool> ExistingGroups { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class NewCAPermSetupGetParam
    {
        [DataMember]
        public string DefaultVisitorsNewGroup { get; set; }
        [DataMember]
        public string DefaultMembersNewGroup { get; set; }
        [DataMember]
        public string DefaultOwnersNewGroup { get; set; }
        [DataMember]
        public PermSetupGroup ExistingVisitorsNewGroup { get; set; }
        [DataMember]
        public PermSetupGroup ExistingMembersNewGroup { get; set; }
        [DataMember]
        public PermSetupGroup ExistingOwnersNewGroup { get; set; }

        // For GUI
        public string ParentNodeId { get; set; }
        [DataMember]
        public CAPermSetupWebInfo WebInfo { get; set; }
        [DataMember]
        public List<ExistingGroupsParam> ExistingGroups { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ExistingGroupsParam
    {
        [DataMember]
        public PermSetupGroup PermSetupGroup { get; set; }
        [DataMember]
        public bool Value { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAPermSetupWebInfo
    {
        [DataMember]
        public UserDetail CurrentUser { get; set; }
        [DataMember]
        public Guid WebID { get; set; }
        [DataMember]
        public string WebUrl { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAPermSetupSetParam
    {
        [DataMember]
        public bool UseExsit { get; set; }
        [DataMember]
        public string GroupName { get; set; }
        [DataMember]
        public List<UserDetail> UserInGroup { get; set; }
        [DataMember]
        public PermSetupGroup ExistGroup { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PermSetupGroup
    {
        [DataMember]
        public int GroupID { get; set; }
        [DataMember]
        public string Name { get; set; }
    }
}
