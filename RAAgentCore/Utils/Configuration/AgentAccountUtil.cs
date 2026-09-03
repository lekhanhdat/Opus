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
using AvePoint.Hybrid.Contract.Object;
using HybridCommonModel.DataModel.Configuration;
using HybridCommonModel.Utils;
using Newtonsoft.Json;
using System;
using System.Text;
using BaseKey = HybridCommonModel.Utils.BaseKey;
using RegistryManager = HybridCommonModel.Utils.RegistryManager;

namespace AvePoint.Hybrid.Utility.Configuration
{
    public class AgentAccountUtil
    {
        /// <summary>
        /// save account info to registry
        /// </summary>
        /// <param name="account"></param>
        public static void Save(AgentAccount account)
        {
            var jsonStr = JsonConvert.SerializeObject(account);
            var encryptBase64Str = AveProtectedDataUtil.ProtectWithBase64(Encoding.UTF8.GetBytes(jsonStr));

            //var encryptBase64Str = Convert.ToBase64String(AESEncriptionHelper.Encrypt(Encoding.UTF8.GetBytes(jsonStr), RegistryConstants.DefaultEncryptionKey));
            RegistryManager.SetValueToRegKey(BaseKey.LocalMachine, RegistryConstants.SubKeyName, RegistryConstants.AgentAccountKey, encryptBase64Str);
        }

        /// <summary>
        /// get account info from registry
        /// </summary>
        /// <returns></returns>
        public static AgentAccount Get()
        {
            var encryptBase64Str = RegistryManager.ReadLocalMachine(RegistryConstants.SubKeyName, RegistryConstants.AgentAccountKey);
            if (string.IsNullOrEmpty(encryptBase64Str)) return null;
            try
            {
                var decryptBase64Str = Encoding.UTF8.GetString(AveProtectedDataUtil.UnProtectWithBase64(encryptBase64Str));
                //var decryptBase64Str = Encoding.UTF8.GetString(AESEncriptionHelper.Decrypt(Convert.FromBase64String(encryptBase64Str), RegistryConstants.DefaultEncryptionKey));
                return JsonConvert.DeserializeObject<AgentAccount>(decryptBase64Str);
            }
            catch (Exception)
            {
                return null;
            }

        }
    }
}
