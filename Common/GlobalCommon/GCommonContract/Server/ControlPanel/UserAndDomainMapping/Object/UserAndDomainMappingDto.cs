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

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserAndDomainMappingDto
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public string Name { set; get; }
        [DataMember]
        public string Type { get; set; }
        [DataMember]
        public List<UserMappingDto> UserMappings { get; set; }
        [DataMember]
        public List<DomainMappingDto> DomainMappings { get; set; }
        [DataMember]
        public String placeHolderAccount { set; get; }
        [DataMember]
        public String destDefaultUser { set; get; }
        [DataMember]
        public String sourceDefaultUser { set; get; }
        [DataMember]
        public String description { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class UserMappingDto 
    {
        [DataMember]
        public String sourceUser { set; get; }
        [DataMember]
        public String destinationUser { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class DomainMappingDto 
    {
        [DataMember]
        public String sourceDomain { set; get; }
        [DataMember]
        public String destinationDomain { set; get; }
    }
}
