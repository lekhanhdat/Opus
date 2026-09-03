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

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.STSAdmin
{
    [DataContract]
    public class STSAdminDto
    {
        [DataMember]
        [XmlElement("Agents")]
        public List<string> Agents { set; get; }

        [DataMember]
        [XmlElement("Cmd")]
        public STSCMD Cmd { set; get; }

        //[DataMember]
        //[XmlElement("STSIISReset")]
        //public STSIISReset IISRest { set; get; }

        [DataMember]
        [XmlAttribute]
        public bool IsJob { set; get; }

        [DataMember]
        [XmlAttribute]
        public string JobId { set; get; }

        [DataMember]
        [XmlAttribute]
        public string Name { set; get; }

        //[DataMember]
        //public string ProfileName { set; get; }

        [DataMember]
        [XmlAttribute]
        public bool NeedReset { set; get; }

        [DataMember]
        [XmlAttribute]
        public bool ResetOnly { set; get; }

        [DataMember]
        [XmlAttribute]
        public string ResetType { set; get; }

        [DataMember]
        [XmlAttribute]
        public string FarmId { set; get; }

        //[DataMember]
        //public STSConfig[] Params { set; get; }
    }
}
