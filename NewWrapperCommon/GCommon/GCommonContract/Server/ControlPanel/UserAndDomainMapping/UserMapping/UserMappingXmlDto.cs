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

namespace AvePoint.GCommon.Contract.Server.ControlPanel.UserAndDomainMapping.UserMapping
{
    [XmlRoot(ElementName = "UserMappingXml")]
    public class UserMappingXmlDto
    {
        [XmlAttribute]
        public String sourceDefaultUser { get; set; }
        [XmlAttribute]
        public String destinationDefaultUser { get; set; }
        [XmlIgnore]
        public String id { get; set; }
        [XmlAttribute]
        public String sourcePlaceHolder { get; set; }
        [XmlAttribute]
        public String destinationPlaceHolder { get; set; }
        [XmlElement]
        public UserMappingNode UserMappings { get; set; }

        public UserMappingXmlDto()
        {
            UserMappings = new UserMappingNode();
        }
    }

    public class UserMappingNode
    { 
        [XmlElement]
        public List<Mapping> UserMapping { get; set; }

        public UserMappingNode()
        {
            UserMapping = new List<Mapping>();
        }
    }

    public class Mapping
    {
        [XmlAttribute]
        public String sourceUser { get; set; }
        [XmlAttribute]
        public String destinationUser { get; set; }
    }
}
