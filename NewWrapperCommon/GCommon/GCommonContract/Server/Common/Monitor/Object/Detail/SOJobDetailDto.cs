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
using System.Text;
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOJobDetailDto : JobDetailDto
    { 
        /// <summary>
        /// 需要混合显示Detail信息的时候，用了区分Action， 而Action字段是国际化过的
        /// </summary>
        [DataMember]
        public int EntityType { get; set; }

        [DataMember]
        public string Farm { get; set; }

        [DataMember]
        public string RuleName { get; set; }

        [DataMember]
        public string DataOperation { get; set; }

        [DataMember]
        public string SiteCollectionUrl { get; set; }

        [DataMember]
        public string DestURL { get; set; }

        [DataMember]
        public string ConfigName { get; set; }

        [DataMember]
        public long StartTime { get; set; }

        [DataMember]
        public long FinishTime { get; set; }

        [DataMember]
        public string StoragePolicy { get; set; }

        [DataMember]
        public string JobId { get; set; }

        [DataMember]
        public string ContentDBName { get; set; }

        [DataMember]
        public string FileName { get; set; }

        [DataMember]
        public string TargetFolder { get; set; }

        [DataMember]
        public string SourceFolder { get; set; }

        [DataMember]
        public string OtherInfo { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SOTestRunJobDetailDto : JobDetailDto
    {
        /// <summary>
        /// for Created Time
        /// </summary>
        [DataMember]
        public long StartTime { get; set; }

        [DataMember]
        public string CreatedBy { get; set; }

        /// <summary>
        /// for Modified Time
        /// </summary>
        [DataMember]
        public long FinishTime { get; set; }

        [DataMember]
        public string ModifiedBy { get; set; }

        [DataMember]
        public string ActionTaken { set; get; }

        [DataMember]
        public string RuleName { set; get; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ArchiverDataEIJobDetailDto : JobDetailDto
    {
        [DataMember]
        public string SiteCollectionUrl { set; get; }

        [DataMember]
        public string Name { set; get; }

        [DataMember]
        public string Type { set; get; }
    }
}
