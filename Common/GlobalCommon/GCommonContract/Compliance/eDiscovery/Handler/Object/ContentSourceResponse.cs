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
using AvePoint.GCommon.Contract.Compliance.eDiscovery.Object;

namespace AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ContentSourceResponse : EDiscoveryResponse
    {

        [DataMember]
        public List<ContentSourceDto> ContentSourceList { get; set; }
        [DataMember]
        public ContentSourceDto ContentSourceDto { get; set; }
        [DataMember]
        public List<DeleteResult> DeleteResultList { get; set; }
        //        [DataMember]
        //        public bool SaveContentSourcePlanResult { get; set; }
        //        [DataMember]
        //        public bool SaveScheduleResult { get; set; }
        //        [DataMember]
        //        public bool CrawlNotifySuccessful { get; set; }
        //

        [DataMember]
        public List<bool> SaveScheduleResults { get; set; }

        [DataMember]
        public List<bool> CrawlNotifyList { get; set; }

        [DataMember]
        public List<string> ExistWebAppUrl { get; set; }

        [DataMember]
        public CreateConetntSourcMessage SaveContentSourceResult { get; set; }

        /// <summary>
        /// 如果是保存schedule，check每一个schedule的starttime
        /// 把chedk结果保存到这个list中
        /// true表示通过
        /// false表示没有通过
        /// </summary>
         [DataMember]
        public List<bool> ScheduleStartTimeStatus { get; set; }
    }


    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ContentSourceResponseState
    {
        [EnumMember]
        Successful = 0,
        [EnumMember]
        IllegitimateRequest = 1,
        [EnumMember]
        FindException = 2,
        [EnumMember]
        AgentIsDown = 3,
        [EnumMember]
        IllegitimateSchedule = 4,
    }

}
