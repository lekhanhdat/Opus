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
using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.AgentService.Object;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    [KnownType(typeof(AveLoadBalanceInfo))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ReportCenterMessage : AveMessage
    {
        bool isOk = true;
        [DataMember]
        public bool IsOk { get { return isOk; } set { isOk = value; } }
        [DataMember]
        public string ErrorMessage { get; set; }
        [DataMember]
        public string DashboardId { get; set; }

        [Obsolete("Please use Chart for DataBrowserService")]
        [DataMember]
        public CommonCanvas Canvas { get; set; }

        [DataMember]
        public BaseChart Chart { get; set; }


        private BaseCollectorDefinition collectorDefinition;

        [DataMember]
        public BaseCollectorDefinition CollectorDefinition
        {
            get
            {
                if (collectorDefinition != null)
                {
                    collectorDefinition.UserId = UserId;
                }
                return collectorDefinition;
            }
            set { collectorDefinition = value; }
        }


        [DataMember]
        public object ControlMessage { get; set; }

        /// <summary>
        /// 导出报表消息契约
        /// </summary>
        [DataMember]
        public ExportSercviceMessage ExportMessage { get; set; }

        /// <summary>
        /// 在页面上的设置消息
        /// </summary>
        [DataMember]
        public ConfigurationMessage ConfigurationMessage { get; set; }
       
        /// <summary>
        /// current logon user id
        /// </summary>
        [DataMember]
        public string UserId { get; set; }

        [DataMember]
        public string CultureName { get; set; }

        [DataMember]
        public string TimeZoneId { get; set; }

        [DataMember]
        public PlanUpdateResult PlanUpdateResult { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PlanUpdateResult
    {
        [EnumMember]
        Passed,
        [EnumMember]
        PlanNameExist,
        [EnumMember]
        NeedShareSiteCollection,
    }
}
