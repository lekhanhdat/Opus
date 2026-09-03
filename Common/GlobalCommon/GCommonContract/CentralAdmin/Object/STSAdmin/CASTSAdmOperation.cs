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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.Common.AdminSearch.Object;

namespace AvePoint.GCommon.Contract.CentralAdmin.Object.STSAdmin
{
    [KnownType(typeof(IDictionary<string, string>))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASTSAdmOperation : CAOperation
    {
        [DataMember]
        [XmlElement("StsAdmConfig")]
        public STSAdminDto STSAdmConfig { get; set; }

        [DataMember]
        [XmlIgnore]
        public IList<STSCMD> CMDs { get; set; }

        [DataMember]
        [XmlIgnore]
        public IDictionary<string, string> Agents { get; set; }

        [DataMember]
        [XmlIgnore]
        public IList<string> Emails { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CASTSAdmResult : ResultBase
    {
        [DataMember]
        public string STSAdmOutput { get; set; }
        [DataMember]
        public string STSAdmOperation { get; set; }
    }
}
