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
using System.Net;
using System.Text;

namespace AvePoint.RA.Common.Cache
{
    public static class RedisKeyUtil
    {
        private const string SemicolonStr = ":";
        #region AOSSync_Tenant
        private static readonly object tenantLocker = new object();
        public static string GenerateSyncNodesTenantLevelKey(string tenantGroupId, AOSSync_TenantLevelFuncType funcType)
        {
            lock (tenantLocker)
            {
                var parts = new string[] { tenantGroupId, tenantLevelFuncTypeToStrDict[funcType] };
                return GenerateKey(parts);
            }
        }

        private const string DistLock = "DLock";
        private const string RemoteNode = "RNode";
        private const string Mailbox = "MBox";
        private const string TenantMessageQueue = "MsgQ";
        private const string PrivateChannel = "PCha";

        private static Dictionary<AOSSync_TenantLevelFuncType, string> tenantLevelFuncTypeToStrDict => new Dictionary<AOSSync_TenantLevelFuncType, string>()
        {
            {AOSSync_TenantLevelFuncType.DistLock, DistLock},
            {AOSSync_TenantLevelFuncType.RemoteNode, RemoteNode},
            {AOSSync_TenantLevelFuncType.Mailbox, Mailbox},
            {AOSSync_TenantLevelFuncType.TenantMessageQueue, TenantMessageQueue},
            {AOSSync_TenantLevelFuncType.PrivateChannel, PrivateChannel},
        };
        #endregion

        #region AOSSync_Global
        private const string GlobalKey = "Global";
        private static readonly object globalLocker = new object();
        public static string GenerateSyncNodesGlobalLevelKey(AOSSync_GlobalLevelFuncType funcType)
        {
            lock (globalLocker)
            {
                var parts = new string[] { GlobalKey, GlobalLevelFuncTypeToStrDict[funcType] };
                return GenerateKey(parts);
            }
        }

        private const string ConflictSetting = "GroupConflict";
        private const string LastPullMessageTime = "LastPullMsgTime";

        private static Dictionary<AOSSync_GlobalLevelFuncType, string> GlobalLevelFuncTypeToStrDict => new Dictionary<AOSSync_GlobalLevelFuncType, string>()
        {
            {AOSSync_GlobalLevelFuncType.ConflictSetting, ConflictSetting},
            {AOSSync_GlobalLevelFuncType.LastPullMessageTime, LastPullMessageTime},
        };
        #endregion

        private static string GenerateKey(string[] parts)
        {
            var sBuilder = new StringBuilder();
#if DEBUG // 在开发环境中，需要添加机器名，保证各开发只使用自己的Redis数据
            string hostName = Dns.GetHostName();
            sBuilder.Append(hostName).Append(SemicolonStr);
#endif
            foreach (string part in parts)
            {
                sBuilder.Append(part).Append(SemicolonStr);
            }
            sBuilder = sBuilder.Remove(sBuilder.Length - 1, 1);
            return sBuilder.ToString();
        }
    }

    public enum AOSSync_TenantLevelFuncType
    {
        RemoteNode,
        Mailbox,
        DistLock,
        TenantMessageQueue,
        PrivateChannel,
    }

    public enum AOSSync_GlobalLevelFuncType
    {
        LastPullMessageTime,
        ConflictSetting,
    }

}
