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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.Common.Notification.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.Server.Service
{
    public interface IOnlineEmailExtender
    {
        /// <summary>
        /// 由于存在job report notification 的设置结构，一个notification需要拆分为多个email message
        /// </summary>
        /// <param name="msg"></param>
        /// <param name="job"></param>
        /// <param name="notification"></param>
        /// <returns></returns>
        List<EmailMessageDto> AssembleMessageForJobReport(EmailMessageDto msg, BaseJobDto job,NotificationDto notification);
        EmailMessageDto ConvertNotificationToEmailMsg(NotificationMessage msg);
    }
}
