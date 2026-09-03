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
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.SharePointBrowser.Object;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAListAlertMeOperation : CAOperation
    {
        [DataMember]
        public string ListId { get; set; }
        /// <summary>
        ///     Alert Title
        /// </summary>
        [DataMember]
        public string AlertTitle { get; set; }

        [DataMember]
        public List<UserDetail> SendAlertsTo { get; set; }
        /// <summary>
        ///     Delivery Method 
        /// </summary>
        [DataMember]
        public bool IsEmailShown { get; set; }

        [DataMember]
        public bool IsSmsShown { get; set; }

        [DataMember]
        public bool IsEmailChecked { get; set; }

        [DataMember]
        public bool IsSmsEnabled { get; set; }

        [DataMember]
        public bool IsMobileAddrEnabled { get; set; }

        [DataMember]
        public bool SendUrlChecked { get; set; }

        [DataMember]
        public string MobileAddr { get; set; }

        [DataMember]
        public List<string> DayName { get; set; }

        [DataMember]
        public List<string> HourName { get; set; }

        

        [DataMember]
        public bool AlwaysNotify { get; set; }

        [DataMember]
        public string UserEmail { get; set; }
        /// <summary>
        ///     Immediate = 0,
        ///     Daily = 1,
        ///     Weekly = 2,
        /// </summary>
        [DataMember]
        public int Frequency { get; set; }

        [DataMember]
        public int CurDay { get; set; }

        [DataMember]
        public int CurHour { get; set; }

        [DataMember]
        public List<string> ChangeType { get; set; }

        [DataMember]
        public int CurChangeType { get; set; }

        [DataMember]
        public List<string> SendAlertsForChange { get; set; }

        [DataMember]
        public int CurSendAlertsForChange { get; set; }

        [DataMember]
        public Dictionary<string, string> Views { get; set; }

        [DataMember]
        public string CurView { get; set; }

        [DataMember]
        public int ListTemplateType { get; set; }

        /// <summary>
        /// 当从SP取出来的ListTemplateType值超出了契约中定义
        /// 的枚举范围之后需要使用此值存储从SP取出来的值
        /// </summary>
        [DataMember]
        public int ListTemplateTypeIntValue { get; set; }

        [DataMember]
        public string FullPath { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAItemAlertMeOperation : CAListAlertMeOperation
    { }
}
