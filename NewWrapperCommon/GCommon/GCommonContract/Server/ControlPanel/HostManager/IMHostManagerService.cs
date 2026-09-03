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
using AvePoint.GCommon.Contract.Server.ControlPanel.HostManager.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel.HostManager
{
    public interface IMHostManagerService
    {
        /// <summary>
        /// 根据 ids 获取Profile
        /// </summary>
        /// <param name="ids">需要获取Profile的id</param>
        /// <returns></returns>
        List<VMCredentialProfileDto> GetProfilesByIds(IEnumerable<string> ids);

        /// <summary>
        /// 获取所有的Profile
        /// </summary>
        /// <returns></returns>
        List<VMCredentialProfileDto> GetCredentialProfiles();

        /// <summary>
        /// Create Profile，同时检查Profile Name是否存在并验证信息是否正确
        /// </summary>
        /// <param name="profileDto">创建的ProfileDto</param>
        /// <param name="needCheckHyperV">是否验证一个Agent只允许创建一个HyperV类型的Profile(外围模块直接传ProfileDto创建时，不需要此步验证)</param>
        /// <returns></returns>
        CredentialErrorCode CreateAndTest(VMCredentialProfileDto profileDto, bool needCheckHyperV);
    }
}
