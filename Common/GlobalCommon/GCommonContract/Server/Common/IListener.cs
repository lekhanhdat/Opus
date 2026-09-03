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
using AvePoint.GCommon.Contract.CentralAdmin.Object;

namespace AvePoint.GCommon.Contract.Server.Common
{
    public interface IListener
    {
        /// <summary>
        /// 监听者在收到通知后所执行的操作，无需返回值.
        /// </summary>
        /// <param name="profile">被修改的profile</param>
        void ExecuteAction(NameAndIdDto profile);
    }

    public interface IWithReturnValueListener
    {
        /// <summary>
        /// 监听者在收到通知后所执行的操作，需要返回值.
        /// </summary>
        /// <param name="profile">被修改的profile list</param>
        /// <returns>返回一个CommonDetailInfoDto，里面包含了引用了此profile的模块AveModule，plan/setting的name，id，其中id不是必须的。</returns>
        List<CommonDetailInfoDto> ExecuteAction(List<NameAndIdDto> profiles);
    }
}
