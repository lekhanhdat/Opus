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
namespace Microsoft365.Authentication.Configuration
{
    using Microsoft365.Common.RequestMonitor;
    using Microsoft365.Configuration;
    using System.Collections.Generic;
    public class M365AuthenticationConfiguration : IM365AuthenticationConfiguration
    {
        public TokenSetting TokenSetting { get; private set; }
        public BeforeRequestToken BeforeRequestTokenEvent { get;private set; }
        public M365AuthenticationConfiguration()
        {
            TokenSetting = new TokenSetting
            {
                MaxCacheInstance = TokenSettingDefaultValue.MaxCacheInstance,
                CacheInstanceLifeCycleSecondTime = TokenSettingDefaultValue.CacheInstanceLifeCycleSecondTime,
                CacheInstanceLifeCycleEdge = TokenSettingDefaultValue.CacheInstanceLifeCycleEdge
            };
            BeforeRequestTokenEvent = (BeforeGetTokenArg arg) =>
            {
                Microsoft365RequestMonitorService.Instance.AddTokenAuditor(new TokenMonitorItem
                {
                    Identity = arg.Identity,
                    IdentityType = arg.IdentityType,
                    ResourceUrl = arg.ResourceUrl
                });
            };
        }

        /// <summary>
        /// should not use this
        /// </summary>
        /// <param name="beforeRequestTokenEvent"></param>
        /// <returns></returns>
        //public IM365AuthenticationConfiguration AddBeforeRequestTokenSetting(BeforeRequestToken beforeRequestTokenEvent)
        //{
        //    BeforeRequestTokenEvent = beforeRequestTokenEvent;
        //    return this;
        //}
        public IM365AuthenticationConfiguration AddTokenSetting(TokenSetting tokenSetting)
        {
            if (tokenSetting != null)
            {
                TokenSetting.CacheInstanceLifeCycleEdge= tokenSetting.CacheInstanceLifeCycleEdge>0? tokenSetting.CacheInstanceLifeCycleEdge: TokenSetting.CacheInstanceLifeCycleEdge;
                TokenSetting.CacheInstanceLifeCycleSecondTime = tokenSetting.CacheInstanceLifeCycleSecondTime > 0 ? tokenSetting.CacheInstanceLifeCycleSecondTime : TokenSetting.CacheInstanceLifeCycleSecondTime;
                TokenSetting.MaxCacheInstance = tokenSetting.MaxCacheInstance > 0 ? tokenSetting.MaxCacheInstance : TokenSetting.MaxCacheInstance;
            }
            return this;
        }

    }
}