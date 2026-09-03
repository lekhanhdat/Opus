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

namespace AvePoint.Application.Security.Core.Cryptography.Encryption.DataEncryptionManagement
{
    using System;
    using System.Collections.Concurrent;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;

    public static class EncryptionUtil
    {
        private static DataEncryptionInfo blowfish;

        private static readonly object locker = new object();
        private static readonly DynamicKeyInfo dynamicKeyInfo = new DynamicKeyInfo();
        private static readonly ConcurrentDictionary<string, DataEncryptionInfoWrapper> encryptionInfoTable = new ConcurrentDictionary<string, DataEncryptionInfoWrapper>();

        public static DataEncryptionInfoWrapper ResolveDynamicKey(DataEncryptionInfo encryptionInfo)
        {
            if (encryptionInfo == null)
            {
                throw new ArgumentNullException("encryptionInfo");
            }

            if (encryptionInfo.EncryptedDynamicKey != null && encryptionInfo.EncryptedDynamicKey.Length > 0)
            {
                lock (dynamicKeyInfo)
                {
                    var key = Convert.ToBase64String(encryptionInfo.EncryptedDynamicKey);
                    if (dynamicKeyInfo.DynamicDic == null ||
                        !dynamicKeyInfo.DynamicDic.TryGetValue(key, out DataEncryptionInfoWrapper d))
                    {
                        throw new Exception(string.Format("The dynamic key of profile:{0} is not put.", encryptionInfo.ProtectionGuid));
                    }

                    dynamicKeyInfo.DynamicDic.TryGetValue(key, out DataEncryptionInfoWrapper w);
                    if (w.DynamicKey == null)
                    {
                        throw new Exception(string.Format("The dynamic key of profile:{0} is empty.", encryptionInfo.ProtectionGuid));
                    }

                    return w;
                }
            }

            if (!encryptionInfoTable.TryGetValue(encryptionInfo.ProtectionGuid, out DataEncryptionInfoWrapper wrapper))
            {
                throw new Exception(string.Format("The dynamic key of profile:{0} is put.", encryptionInfo.ProtectionGuid));
            }
            wrapper = encryptionInfoTable[encryptionInfo.ProtectionGuid];
            if (wrapper.DynamicKey == null)
            {
                throw new Exception(string.Format("The dynamic key of profile:{0} is empty.", encryptionInfo.ProtectionGuid));
            }
            return wrapper;
        }

        public static DataEncryptionInfoWrapper PutEncryptionInfo(DataEncryptionInfo info, string dynamicKey = null)
        {
            var wrapper = new DataEncryptionInfoWrapper
            {
                DynamicKey = dynamicKey,
                EncryptionInfo = info
            };
            if (wrapper.EncryptionInfo.EncryptedDynamicKey != null && wrapper.EncryptionInfo.EncryptedDynamicKey.Length > 0)
            {
                lock (dynamicKeyInfo)
                {
                    var key = Convert.ToBase64String(wrapper.EncryptionInfo.EncryptedDynamicKey);
                    if (!dynamicKeyInfo.ContainsKey(key))
                    {
                        dynamicKeyInfo.AddDynamicKey(key, wrapper);
                    }
                    wrapper = dynamicKeyInfo.DynamicDic[key];
                    return wrapper;
                }
            }

            if (!encryptionInfoTable.ContainsKey(wrapper.EncryptionInfo.ProtectionGuid))
            {
                encryptionInfoTable[wrapper.EncryptionInfo.ProtectionGuid] = wrapper;
            }
            return wrapper;
        }

        public static DataEncryptionInfoWrapper PutEncryptionInfo(DataEncryptionInfoWrapper wrapper)
        {
            if (wrapper.EncryptionInfo.EncryptedDynamicKey != null && wrapper.EncryptionInfo.EncryptedDynamicKey.Length > 0)
            {
                lock (dynamicKeyInfo)
                {
                    var key = Convert.ToBase64String(wrapper.EncryptionInfo.EncryptedDynamicKey);
                    if (!dynamicKeyInfo.ContainsKey(key))
                    {
                        dynamicKeyInfo.AddDynamicKey(key, wrapper);
                    }
                    return wrapper;
                }
            }

            if (!encryptionInfoTable.ContainsKey(wrapper.EncryptionInfo.ProtectionGuid))
            {
                encryptionInfoTable[wrapper.EncryptionInfo.ProtectionGuid] = wrapper;
            }
            return wrapper;
        }
    }
}