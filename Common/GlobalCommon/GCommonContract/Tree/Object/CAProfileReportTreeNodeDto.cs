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
using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAProfileReportTreeNodeDto : AveTreeNodeDto<CAProfileReportTreeNodeDto>
    {
        [DataMember(EmitDefaultValue = false)]
        public AdminRuleBasicInfo RuleInfo { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ProfileId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ProfileName { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public ProfileReturnMessage ProfileReturnMessage { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute]
        public String SPObjectId { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlAttribute("siteLockStatus")]
        public Int32 SiteLockStatus { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [XmlElement("spVersion")]
        public Int32 SPVersion { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public SPType SPType { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public String Url { get; set; }

        public override bool Equals(object obj)
        {
            if (!(obj is CAProfileReportTreeNodeDto))
            {

                return false;
            }

            CAProfileReportTreeNodeDto node = obj as CAProfileReportTreeNodeDto;

            return ID == node.ID &&　Name == node.Name && FullPath == node.FullPath;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}
