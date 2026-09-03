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
using AvePoint.GCommon.Contract.Storage.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract]
    public class HSMArchiverDto
    {
        [DataMember]
        public List<string> SiteUrls{ get; set; }
        [DataMember]
        public StorageDeviceUIDto SelectedStorage { get; set; }
        [DataMember]
        public string SourceDataStorageId { get; set; }
        [DataMember]
        public string StubTemplateId { get; set; }
        [DataMember]
        public string O365TenantId { get; set; }
        
        [DataMember]
        public string Location { get; set; }
        [DataMember]
        public string DataContentStorageId { get; set; }
        [DataMember]
        public string MainJobId { get; set; }

        [DataMember]
        public bool SkipCheckFileExtension { get; set; }
        [DataMember]
        public string TraceId { get; set; }
    }
    [DataContract]
    public class HSMArchiverResult
    {
        [DataMember]
        public string MainJobId { get; set; }
        [DataMember]
        public bool IsSuccessStartJob { get; set; }
    }
}
