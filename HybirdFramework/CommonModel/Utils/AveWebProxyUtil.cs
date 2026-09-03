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
using HybridCommonModel.DataModel;
using HybridCommonModel.DataModel.Configuration;
using Newtonsoft.Json;
using System;
using System.Text;

namespace HybridCommonModel.Utils
{
    public class AveWebProxyUtil
    {
        public static AveWebProxyOptions ReadProxySetting()
        {
            var encryptBase64Str = RegistryManager.ReadLocalMachine(RegistryConstants.SubKeyName, RegistryConstants.ProxySettingKey);
            if (string.IsNullOrEmpty(encryptBase64Str)) return null;

            return JsonConvert.DeserializeObject<AveWebProxyOptions>(Encoding.UTF8.GetString(AveProtectedDataUtil.UnProtectWithBase64(encryptBase64Str)));
        }

        /// <summary>
        /// write proxy setting to registry
        /// </summary>
        /// <param name="options"></param>
        public static void WriteProxySetting(AveWebProxyOptions options)
        {
            var jsonStr = JsonConvert.SerializeObject(options);
            var encryptBase64Str = AveProtectedDataUtil.ProtectWithBase64(Encoding.UTF8.GetBytes(jsonStr));
            RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, RegistryConstants.SubKeyName, RegistryConstants.ProxySettingKey, encryptBase64Str);
        }

        /// <summary>
        /// remove proxy setting from registry
        /// </summary>
        public static void RemoveProxySetting()
        {
            RegistryManager.RemoveValueFromRegKey(BaseKey.LocalMachine, RegistryConstants.SubKeyName, RegistryConstants.ProxySettingKey);
        }
    }
}
