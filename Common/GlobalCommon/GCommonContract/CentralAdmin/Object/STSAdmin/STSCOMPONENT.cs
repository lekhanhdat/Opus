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




using System.Runtime.Serialization;
using System.Xml.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.STSAdmin
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class STSCOMPONENT
    {

        [DataMember]
        [XmlAttribute]
        public string Name { set; get; }

        [DataMember]
        [XmlAttribute]
        public string Value { set; get; }

        [DataMember]
        [XmlAttribute]
        public int HeadType { set; get; }

        [DataMember]
        [XmlAttribute]
        public int BodyType { set; get; }

        [DataMember]
        [XmlAttribute]
        public int TailType { set; get; }

        [DataMember]
        [XmlAttribute]
        public bool Recommand { set; get; }

        [DataMember]
        [XmlAttribute]
        public string Description { set; get; }

        [DataMember]
        [XmlAttribute]
        //0，普通的component；-1，special component；1-9，与special component关联的item的index
        public int Parent { set; get; }

        [DataMember]
        public STSITEM[] Items { set; get; }

        [DataMember]
        public bool encrypt { set; get; }
 
    }

}
