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
using AvePoint.Hybrid.Contract.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object
{
    public class RMCertificateDto
    {
        public Guid Id { get; set; }

        public string Name { get; set; }

        public string Thumbprint { get; set; }

        public DateTime? ValidFrom { get; set; }

        public DateTime? ValidTo { get; set; }

        public byte[] BinaryContent { get; set; }
        public string PWD { get; set; }

        /// <summary>
        /// indicate if this is the default certificate
        /// </summary>
        public bool IsDefault { get; set; }

        /// <summary>
        /// incicate if the certificate is expired.
        /// </summary>
        public bool IsExpired => ValidTo.HasValue && (ValidTo.Value.Ticks < DateTime.UtcNow.Ticks);
        public CertificateStatus Status => !ValidTo.HasValue ? CertificateStatus.None
            : ValidTo.Value < DateTime.UtcNow ? CertificateStatus.Expired
            : ValidTo.Value.AddMonths(-1) <= DateTime.UtcNow ? CertificateStatus.ToBeExpired
            : CertificateStatus.Active;
    }

    public class RMCertificateCreateRequest
    {
        public bool SetAsDefault { get; set; }

        public RMCertificateDto Certificate { get; set; }
    }

    [DataContract]
    public enum CertificateStatus
    {
        [EnumMember]
        None = 0,
        [EnumMember] 
        Active = 1,
        [EnumMember] 
        ToBeExpired = 2,
        [EnumMember] 
        Expired = 3
    }


    public class CertificateUpdateResult
    {
        public CertificateUpdateResultEnum ResultCode { get; set; }
        public List<AgentCertificateUpdateResult> Agents { get; set; }
    }

    public enum CertificateUpdateResultEnum
    {
        AllSucceed = 0,
        NoDefaultCertificate = 1,
        CertificateExpired = 2,
        NoActiveAgent = 3,
        AllFailed = 4,
        HasFailed = 5
    }
}
