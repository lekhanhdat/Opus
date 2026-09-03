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

namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PolicyEnforcerAdminEmailInfo
    {
        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailPolicyDetail { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailAutoUndo { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailOutOfPolicy { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailNodeOutOfPolicy { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailDetail { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailRuleTrigger { get; set; }

        [DataMember]
        public Dictionary<string, List<PolicyEnforcerMessageInfo>> MessageInRule { get; set; }

        [DataMember]
        public AdminRuleBasicInfo Rule { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PolicyEnforcerEmailResult
    {
        [DataMember]
        public string EmailMessage { get; set; }

        [DataMember]
        public string EmailSubject { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PolicyEnforcerMessageInfo
    {
        [DataMember]
        public string ScopeUrl { get; set; }

        [DataMember]
        public string StrMessage { get; set; }

        [DataMember]
        public List<CAStringFormatMessage> ContextDetailMsg { get; set; }

        //旧数据的ContextDetail
        [DataMember]
        public string ContextDetail { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class PolicyEnforcerEndUserEmailInfo
    {
        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailHello { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailMessage { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailPolicyScope { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailAction { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailContactAdmin { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailRuleName { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailRuleDescription { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailPolicyTrigger { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailActionItemCreate { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailActionListCreate { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailActionChangePermission { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailActionSiteCreate { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailActionDelete { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailActionInherit { get; set; }

        [DataMember]
        public string CentralAdmin_PolicyEnforcerEmailActionNoAction { get; set; }

        [DataMember]
        public Dictionary<string, Dictionary<string, List<PolicyEnforcerMessageInfo>>> EmailMessageMapping { get; set; }

        [DataMember]
        public Dictionary<string, AdminRuleBasicInfo> Rules { get; set; }

        [DataMember]
        public string ProfileName { get; set; }
    }

}
