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
using AvePoint.RA.Contract.RMWeb.Authentication;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IAuthenticationManagerService
    {
        /// <summary>
        /// 此方法return一个数据结构
        /// </summary>
        /// <param name="onlyEnableMode">true：只返回当前enable的domain，false：返回所有domain</param>
        /// <param name="containDomains">true：返回domain信息，不返回domain信息</param>
        /// <param name="onlyEnableDomain">true：只返回当前enable的domain信息</param>
        /// <returns></returns>
        List<RMAuthenticationDto> GetAuthenticationModes(bool onlyEnableMode, bool containDomains, bool onlyEnableDomain);

        bool EnableAuthenticationMode(int id);

        bool DisableAuthenticationMode(int id);

        bool SetDefaultAuthenticationMode(int id);

        RMAuthenticationDto GetDefaultAuthenticationMode();

        RMAuthenticationDto GetAuthenticationModeById(int id);

        RMDomainDto GetADDomain(int id, bool needPassword = false);

        List<RMDomainDto> GetADDomains(List<int> ids);

        List<RMDomainDto> GetADDomains(bool onlyEnableDomain);

        RMDomainDto AddADDomain(RMDomainDto info, out RMOperatingDomainError errorType);

        bool DeleteADDomain(int id);

        bool DeleteADDomain(List<int> ids);

        bool UpdateADDomainStatus(int id, bool status);

        bool UpdateADDomainStatus(List<int> ids, bool status);

        bool UpdateADDomainUserInfo(int id, string userName, string password);
    }
}
