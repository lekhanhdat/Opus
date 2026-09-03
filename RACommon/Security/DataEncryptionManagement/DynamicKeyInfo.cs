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
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Cryptography.Wrapper;

    public class DynamicKeyInfo
    {
        private const int MAX_DYNA_SIZE = 5000;

        private static bool isUnlimited = false;

        public List<string> DynamicKeyList { get; private set; }

        public ConcurrentDictionary<string, DataEncryptionInfoWrapper> DynamicDic { get; private set; }

        public DynamicKeyInfo()
        {
            DynamicKeyList = new List<string>();
            DynamicDic = new ConcurrentDictionary<string, DataEncryptionInfoWrapper>();
        }

        public void AddDynamicKey(string key, DataEncryptionInfoWrapper val)
        {
            if (DynamicKeyList == null || DynamicDic == null)
            {
                DynamicKeyList = new List<string>();
                DynamicDic = new ConcurrentDictionary<string, DataEncryptionInfoWrapper>();
            }
            else
            {
                if ((!isUnlimited) && DynamicKeyList.Count == MAX_DYNA_SIZE)
                {
                    string tempKey = DynamicKeyList[0];
                    DynamicDic.TryRemove(tempKey, out DataEncryptionInfoWrapper d);
                    DynamicKeyList.RemoveAt(0);
                }
            }
            DynamicKeyList.Add(key);
            DynamicDic[key] = val;
        }

        public bool ContainsKey(string key)
        {
            return DynamicDic.ContainsKey(key);
        }

        public static void UnrestrictDynamicKeyListSize()
        {
            isUnlimited = true;
        }
    }

}