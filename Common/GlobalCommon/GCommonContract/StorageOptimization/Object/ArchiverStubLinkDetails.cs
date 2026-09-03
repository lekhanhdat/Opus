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
using AvePoint.GCommon.Contract.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverStubLink
    {
        [DataMember]
        public string TenantID { get; set; }
        [DataMember]
        public string SiteUrl { get; set; }
        [DataMember]
        public string FileServerRelativeUrl { get; set; }
        [DataMember]
        public string PathMD5 { get; set; }
        [DataMember]
        public string JobID { get; set; }
        [DataMember]
        public string User { get; set; }
        [DataMember]
        public bool HasArchiveHistory { get; set; }
        [DataMember]
        public string StubType { get; set; }
        [DataMember]
        public string StubId { get; set; }
        [DataMember]
        public string FileSize { get; set; }
        [DataMember]
        public StubProductSource StubProductSource { get; set; }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum StubProductSource
    {
        Opus = 0,
        AOSP = 1
    }
}
