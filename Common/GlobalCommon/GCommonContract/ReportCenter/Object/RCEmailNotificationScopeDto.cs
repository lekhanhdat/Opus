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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    #region using directives

    #endregion

    /// <summary>
    /// 存取每个功能对应EmailNotification信息，以及Interval等
    /// </summary>
    public class RCEmailNotificationScopeDto
    {
        /// <summary>
        /// 对应的Email Notification Name
        /// </summary>
        public string NotificationName { set; get; }

        /// <summary>
        /// 每个功能发送Email的条件
        /// </summary>
        public string SendCondition { set; get; }

        /// <summary>
        /// Email Send Interval
        /// </summary>
        public int RCEmailInterval { set; get; }

        /// <summary>
        /// Email Send Interval Unit
        /// </summary>
        public RCEmailIntervalUnit IntervalUnit { set; get; }

        /// <summary>
        /// Scope ProfileId
        /// </summary>
        public string ScopeProfileId { set; get; }

    }

    /// <summary>
    /// 
    /// </summary>
    public enum RCEmailIntervalUnit
    { 
        Minute,Hour,Day,Week,Month,Year
    }
}