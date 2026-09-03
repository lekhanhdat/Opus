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
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Authentication
{
    public class RMAuthenticationDto
    {
        public int Id { set; get; }

        public string Name { get; set; }

        public bool Enable { get; set; }

        public bool IsDefault { get; set; }

        public RMAuthenticationTypes Type { get; set; }

        public List<RMDomainDto> Domains { get; set; }
    }
    [DataContract]
    public class RMDomainDto
    {
        [DataMember]
        public int Id { set; get; }
        [DataMember]
        public string DomainName { get; set; }
        [DataMember]
        public string RealName { get; set; }
        [DataMember]
        public string NetBiosName { get; set; }
        [DataMember]
        public string UserName { get; set; }
        [DataMember]
        [DataType(DataType.Password)]
        public string Password { get; set; }
        [DataMember]
        public bool Enable { get; set; }

    }

    public enum RMOperatingDomainError
    {
        None = 0,
        DomainIsExist,
        UnableConnectDomain,
    }
}
