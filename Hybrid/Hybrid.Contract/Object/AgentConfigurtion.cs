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
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.Contract.Object
{
    [DataContract]
    public class AgentConfigurtion
    {
        /// <summary>
        /// agent id
        /// </summary>
        [DataMember]
        public string Id { get; set; }

        /// <summary>
        /// tenant id
        /// </summary>
        [DataMember]
        public string CustomerId { get; set; }

        /// <summary>
        /// app client id in AOS
        /// </summary>
        [DataMember]
        public string ClientId { get; set; }
        [DataMember]
        public string RecordsApiUrl { get; set; }
        [DataMember]
        public string IdentityServiceUrl { get; set; }
        [DataMember]
        public string SiginalRServiceUrl { get; set; }

        [DataMember]
        public string InstallationCode { get; set; }

        [DataMember]
        public string AuthCode { get; set; }
        [DataMember]
        public string CertificateContent { get; set; }
        [DataMember]
        public string CertificatePWD { get; set; }
        [DataMember]
        public string RecordsCertContent { get; set; }

        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public bool IsMultiGeoMainDC { get; set; }

        /// <summary>
        /// Package id, starting from May 2022 release, it has the same value with agent id, however, it has different value for each downloading before.
        /// </summary>
        [DataMember]
        public string PackageId { get; set; }

        /// <summary>
        /// used for part of encrytion key
        /// </summary>
        [DataMember]
        public static string Salt = "8C0E4C60-7B88-ED21-80FC-00155D3C0105";

        [DataMember]
        public string CurrentDC { get; set; }
    }

    //public enum AgentConfigurtionFileValidateResult
    //{
    //    Succeed,
    //    AgentIdInvalid,
    //    AuthCodeInvalid,
    //    AlreadyUsed,
    //    OtherError
    //}
}
