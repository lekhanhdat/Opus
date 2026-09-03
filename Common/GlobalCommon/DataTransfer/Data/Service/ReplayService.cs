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




using System;
using System.ServiceModel;
using System.ServiceModel.Activation;
using AvePoint.GCommon.Transfer.Common;
using AvePoint.GCommon.Transfer.Data.Interface;

namespace AvePoint.GCommon.Transfer.Data.Service
{
    /// <summary>
    /// 实现数据传输的底层WCF服务
    /// </summary>
    [ServiceBehavior(InstanceContextMode=InstanceContextMode.PerSession,ConcurrencyMode=ConcurrencyMode.Reentrant)]
    [AspNetCompatibilityRequirements(RequirementsMode = AspNetCompatibilityRequirementsMode.Allowed)]
    public class RelayService : IRelay, IDisposable
    {
        static AveLogger logger = AveLogger.GetInstance(typeof(RelayService), false);

        CommonPerformanceTimerPool timerPool = new CommonPerformanceTimerPool(DataTransferConfiguration.DisablePerformanceLogger);

        public int CheckStatus(string sessionId, string identifier)
        {
            return 0;
        }
        public SessionStatus InitSession(string sessionId, string identifier, bool isInited)
        {
            if (isInited)
            {
                return BufferStorage.InitSessionManagement(sessionId, identifier);
            }
            else
            {
                return BufferStorage.IsSessionManagementExisting(sessionId, identifier);
            }
        }
        public BufferStatus PutBuffer(string sessionId, string identifier, long serialNo, byte[] buffer)
        {
            timerPool.Action("PutBuffer", true);
            BufferStatus status = BufferStorage.PutBuffer(sessionId, identifier, serialNo, buffer);
            timerPool.Action("PutBuffer", false);
            return status;
        }

        public BufferStatus CheckBuffer(string sessionId, string identifier, long serialNo, bool isSender)
        {
            timerPool.Action("CheckBuffer", true);
            BufferStatus status = BufferStorage.CheckBuffer(sessionId, identifier, serialNo, isSender);
            timerPool.Action("CheckBuffer", false);
            return status;
        }

        public BufferStatus GetBuffer(string sessionId, string identifier, long serialNo, out byte[] buffer)
        {
            timerPool.Action("GetBuffer", true);
            BufferStatus status = BufferStorage.GetBuffer(sessionId, identifier, serialNo, out buffer);
            timerPool.Action("GetBuffer", false);
            return status;
        }
        public int ClearSession(string sessionId, string identifier)
        {            
            return BufferStorage.ClearBuffer(sessionId, identifier);
        }
        public int ClearSessionManagement(string sessionId)
        {
            return BufferStorage.ClearSessionManagement(sessionId);
        }
        public void SetTimeout(string sessionId, string identifier, int timeout, bool isSender)
        {
            BufferStorage.SetTimeout(sessionId, identifier, timeout, isSender);
        }
        public bool KeepAlive(string sessionId, string identifier, bool isSender)
        {
            return BufferStorage.UpdateModifyTime(sessionId, identifier, isSender);
        }
        public bool CheckSessionInUse(string sessionId, string identifier, bool isSender)
        {
            return BufferStorage.BufferSessionInUse(sessionId, identifier, isSender);
        }
        public void Dispose()
        {
            //GlobalWCFServiceInstanceManager.UnRegistInstance(this);
            logger.Debug(timerPool.ToString());
        }
    }
}
