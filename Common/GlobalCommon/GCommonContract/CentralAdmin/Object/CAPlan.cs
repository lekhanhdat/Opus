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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Server.Common;
    #endregion
    
    /// <summary>
    /// To enclosure the CAPlan as a data contract object 
    /// </summary>
    [DataContract]
    public class CAPlan : PlanDto, IComparable<CAPlan>
    {

        [DataMember]
        public string Username { get; set; }
        [DataMember]
        public CAPlanDetail PlanDetail { get; set; }
        [DataMember]
        public bool Anonymous { set; get; }
        [DataMember]
        public bool TestRunPlan { get; set; }

        #region IComparable<CAPlan> Members 
        /// <summary>
        /// 对多个Plan进行排序的时候会按照Id从大到小排列, 也就是说最新的Plan会排在最前面
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int CompareTo(CAPlan other)
        {
            if (other == null)
            {
                return 1;
            }
            return -string.Compare(this.Id, other.Id, StringComparison.Ordinal);
        }

        #endregion
    }
        

    [DataContract]
    public class CAPlanDetail
    {
        [DataMember]
        public List<CAMessage> Messages { get; set; }
    }

    
    [DataContract]
    public class CANotification
    {
        [DataMember]
        public string Recepient { get; set; }
        [DataMember]
        public string Fromat { get; set; }

        // 1:Attach Brief Report(PDF) To Email, 2:Attach Search Result To Email
        [DataMember]
        public int AttachType { get; set; }
    }
}