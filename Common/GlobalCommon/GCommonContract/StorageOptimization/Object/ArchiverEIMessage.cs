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
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Contract.Server.Common.Profile.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.StorageOptimization.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverEIMessage
    {
        [DataMember]
        public EITreeNodeDto Tree { set; get; }

        [DataMember]
        public string OldIndexDeviceId { set; get; }
        [DataMember]
        public LogicalDeviceDto OldIndexDevice { set; get; }
        [DataMember]
        public List<StoragePolicyDto> StoragePolicys { set; get; }
        /// <summary>
        /// Docave or Netapp or IBM
        /// </summary>
        [DataMember]
        public PlatformType PlatformType { set; get; }
        [DataMember]
        public ServiceDto MediaDto { set; get; }
        [DataMember]
        public ProfileDto EmailProfile { set; get; }
        [DataMember]
        public bool DoVerify { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class EIVerifyMessage
    {
        /// <summary>
        /// Verify job id
        /// </summary>
        [DataMember]
        public string JobId { set; get; }
        /// <summary>
        /// site collection url
        /// </summary>
        [DataMember]
        public string SiteUrl { set; get; }
        /// <summary>
        /// data verify state , 0 failed, 1 success
        /// </summary>
        [DataMember]
        public int State { set; get; }

    }
}
