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




namespace AvePoint.GCommon.Transfer.Data.Service
{
    ///// <summary>
    ///// 工具类，用于管理进程内WCF服务实例，解决进程内服务的调用问题。
    ///// </summary>
    //internal class GlobalWCFServiceInstanceManager
    //{
    //    static string mLock = "Lock";
    //    static Dictionary<string, IRelay> mInstanceDictionary = new Dictionary<string, IRelay>();//用于管理进程内的WCF服务实例
    //    /// <summary>
    //    /// 注册实例到管理列表中
    //    /// </summary>
    //    /// <param name="sessionId">当前实例对应的SessionID</param>
    //    /// <param name="serviceInstance">注册的服务实例对象</param>
    //    public static void RegistInstance(string sessionId, IRelay serviceInstance)
    //    {
    //        if (mInstanceDictionary.ContainsKey(sessionId)) return;
    //        lock (mLock)
    //        {
    //            if (!mInstanceDictionary.ContainsKey(sessionId))
    //            {
    //                mInstanceDictionary.Add(sessionId, serviceInstance);
    //            }
    //        }
    //    }
    //    /// <summary>
    //    /// 当服务实例释放的时候需要反注册全局列表，避免空引用
    //    /// </summary>
    //    /// <param name="instance">注销的服务实例对象</param>
    //    public static void UnRegistInstance(IRelay instance)
    //    {
    //        lock (mLock)
    //        {
    //            foreach (string id in mInstanceDictionary.Keys)
    //            {
    //                if (mInstanceDictionary[id].Equals(instance))
    //                {
    //                    mInstanceDictionary.Remove(id);
    //                    break;
    //                }
    //            }
    //        }
    //    }
    //    /// <summary>
    //    /// 通过sessionId对象获得对应的实例对象
    //    /// </summary>
    //    /// <param name="sessionId">sessionID对象</param>
    //    /// <returns>返回一个WCF服务实例</returns>
    //    public static IRelay GetInstance(string sessionId)
    //    {
    //        if (mInstanceDictionary.ContainsKey(sessionId))
    //        {
    //            return mInstanceDictionary[sessionId];
    //        }
    //        else
    //        {
    //            return null;
    //        }
    //    }
    //}
}
