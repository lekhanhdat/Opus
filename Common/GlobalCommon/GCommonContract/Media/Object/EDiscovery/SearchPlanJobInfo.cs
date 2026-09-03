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



namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.Compliance.eDiscovery.Handler.Object;
    using AvePoint.GCommon.Contract.Storage.Entity;
    #endregion directives

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class SearchPlanJobInfo
    {
        /// <summary>
        /// 保存Offline Search结果的DB
        /// </summary>
        [DataMember]
        public PhysicalDeviceDto SearchLocation { get; set; }

        [DataMember]
        public Int32 JobType { get; set; }

        /// <summary>
        /// Full Text Index的Query条件.
        /// </summary>
        [DataMember]
        public EDFullTextIndexSearchRequest EDFullTextIndexSearchRequest { get; set; }

        [DataMember]
        public String JobId { get; set; }

        [DataMember]
        public String PlanId { get; set; }

        public override String ToString()
        {
            return String.Format("Search Plan Job Info: Job Type: {0}, Job Id: {1}, Plan Id: {2}",
                this.JobType,
                this.JobId,
                this.PlanId);
        }
    }
}