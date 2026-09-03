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

namespace AvePoint.Hybrid.ClientLibrary.Data
{
    [DataContract(Namespace = HBContractConstants.Namespace)]
    public class HBJobStatusInfo
    {
        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string MainJobId { get; set; }

        /// <summary>
        /// 是否为subJob
        /// </summary>
        [DataMember]
        public Boolean IsSubJob { get; set; }

        /// <summary>
        /// 当前进度, 调用UpdateJobProgress方法会更新进度. UpdateJobStatus方法不会更新进度.
        /// </summary>
        [DataMember]
        public double Progress { get; set; }

        /// <summary>
        /// subJob进度在整个job进度集合中所占的加权值,
        /// </summary>
        [DataMember]
        public int Weight { get; set; }

        /// <summary>
        /// job的状态. 调用UpdateJobStatus方法会更新状态. UpdateJobProgress方法不会更新状态.
        /// </summary>
        [DataMember]
        public int State { get; set; }

        [DataMember]
        public string Comment { get; set; }

    }
}
