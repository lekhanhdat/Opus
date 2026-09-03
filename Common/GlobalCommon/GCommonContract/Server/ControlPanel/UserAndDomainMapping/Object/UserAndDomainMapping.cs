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
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;
namespace AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot(ElementName = "UserAndDomainMapping")]
    public class UserAndDomainMapping
    {
        [DataMember]
        [XmlElement("UserMappings")]
        public UserMappings UserMappings { set; get; }

        [DataMember]
        [XmlElement("DomainMappings")]
        public DomainMappings DomainMappings { set; get; }

        [DataMember]
        [XmlAttribute("placeHolderAccount")]
        public String placeHolderAccount { set; get; }

        [DataMember]
        [XmlAttribute("sourcePlaceHolderAccount")]
        public String sourcePlaceHolderAccount { set; get; }

        [DataMember]
        [XmlAttribute("destDefaultUser")]
        public String destDefaultUser { set; get; }

        [DataMember]
        [XmlAttribute("sourceDefaultUser")]
        public String sourceDefaultUser { set; get; }

        [DataMember]
        [XmlAttribute("description")]
        public String description { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("UserMappings")]
    public class UserMappings
    {
        [DataMember]
        [XmlElement("UserMapping")]
        public List<UserMapping> UserMapping { set; get; }

    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("DomainMapping")]
    public class DomainMappings
    {
        [DataMember]
        [XmlElement("DomainMapping")]
        public List<DomainMapping> DomainMapping { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("UserMapping")]
    public class UserMapping
    {
        [DataMember]
        [XmlAttribute("sourceUser")]
        public String sourceUser { set; get; }

        [DataMember]
        [XmlAttribute("destinationUser")]
        public String destinationUser { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    [XmlRoot("DomainMapping")]
    public class DomainMapping
    {
        [DataMember]
        [XmlAttribute("sourceDomain")]
        public String sourceDomain { set; get; }

        [DataMember]
        [XmlAttribute("destinationDomain")]
        public String destinationDomain { set; get; }
    }
}
