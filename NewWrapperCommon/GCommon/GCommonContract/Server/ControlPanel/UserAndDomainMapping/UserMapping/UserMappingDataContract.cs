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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.UserMapping
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserMappingDataContract : IProfileContent
    {
        [DataMember]
        public String id { get; set; }
        [DataMember]
        public String mappingName { get; set; }
        [DataMember]
        public String description { get; set; }
        [DataMember]
        public Boolean useCustomSetting { get; set; }
        [DataMember]
        public String targetdefaultUser { get; set; }
        [DataMember]
        public Boolean useDefaultUser { get; set; }
        [DataMember]
        public Boolean useplaceHolder { get; set; }
        /// <summary>
        /// 该属性是留给two way转移的情况
        /// </summary>
        [DataMember]
        public String sourceDefaultUser { get; set; }
        /// <summary>
        /// 该属性是留给two way转移的情况
        /// </summary>
        [DataMember]
        public String sourcePlaceHolder { get; set; }
        [DataMember]
        public String targetPlaceHolder { get; set; }
        [DataMember]
        public List<MappingContent> mappings { get; set; }
        [DataMember]
        public long modifiedTime { get; set; }

        public UserMappingDataContract()
        {
            mappings = new List<MappingContent>();
        }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class MappingContent
    {
        [DataMember]
        public String sourceUserName { get; set; }
        [DataMember]
        public String targetUserName { get; set; }
    }
}
