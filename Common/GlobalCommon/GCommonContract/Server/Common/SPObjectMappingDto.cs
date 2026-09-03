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
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Server.Common
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SPObjectMappingDto
    {
        [DataMember]
        public string Id { set; get; }

        [DataMember]
        public string InternalId { get; set; }

        [DataMember]
        public string Name { set; get; }

        [DataMember]
        public string FarmInternalId { set; get; }

        [DataMember]
        public string SPId { set; get; }

        [DataMember]
        public NodeLevel Level { set; get; }

        [DataMember]
        public long UpdateTime { set; get; }

        [DataMember]
        public string Version { set; get; }

        public string ToString()
        {
            StringBuilder b = new StringBuilder();
            b.Append("Id : ").Append(Id).Append(",");
            b.Append("Name : ").Append(Name).Append(",");
            b.Append("FarmInternalId : ").Append(FarmInternalId).Append(",");
            b.Append("SPId : ").Append(SPId).Append(",");
            b.Append("Type : ").Append(Level).Append(",");
            b.Append("Version : ").Append(Version);
            return b.ToString();
        }

    }
}
