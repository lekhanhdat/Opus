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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract]
    [XmlRootAttribute("SecuritySearchTreeNode")]
    public class SecuritySearchTreeNodeDto : SPTreeNodeDto
    {
        //start security search
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("allowMembersEditMembership")]
        public bool AllowMembersEditMembership { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("allowRequestToJoinLeave")]
        public bool AllowRequestToJoinLeave { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("autoAcceptRequestToJoinLeave")]
        public bool AutoAcceptRequestToJoinLeave { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("onlyAllowMembersViewMembership")]
        public bool OnlyAllowMembersViewMembership { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("requestToJoinLeaveEmailSetting")]
        public string RequestToJoinLeaveEmailSetting { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("permissioinText")]
        public string PermissionText { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("nodeID")]
        public int MemberID { get; set; }

        /// <summary>
        /// AD User和AD Group用到此属性
        /// </summary>
        [DataMember(EmitDefaultValue = false)]
        [XmlElement("memberState")]
        public MemberStateType MemberState { get; set; }
        //end security search

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("memberType")]
        public MemberType MemberType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("permissions")]
        public List<Permission> Permissions { get; set; }
    }
}
