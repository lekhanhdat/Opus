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


using AveClientRequest.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Net;

namespace AvePoint.ObjectModel.ClientOM
{

    public class AveWebRequestExecutorFactory : WebRequestExecutorFactory
    {

        DataMonitor m_DataMonitor = null;
        Action<WebRequest> mChangeTokenFunc;
        Func<(Guid tenantId, string defaultAppId)> mGetTenantIdAndDefaultAppIdFunc;
        public AveWebRequestExecutorFactory() { }

        public AveWebRequestExecutorFactory(DataMonitor dataMonitor)
        {
            this.m_DataMonitor = dataMonitor;
        }
        /// <summary>
        /// For refresh token during execute.
        /// </summary>
        /// <param name="dataMonitor"></param>
        /// <param name="changeTokenFunc"></param>
        public AveWebRequestExecutorFactory(DataMonitor dataMonitor, Action<WebRequest> changeTokenFunc, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc)
        {
            mChangeTokenFunc = changeTokenFunc;
            mGetTenantIdAndDefaultAppIdFunc = getTenantIdAndDefaultAppIdFunc;
            this.m_DataMonitor = dataMonitor;
        }
        public override WebRequestExecutor CreateWebRequestExecutor(ClientRuntimeContext context, string requestUrl)
        {
            if (context == null)
            {
                throw new ArgumentNullException("context");
            }
            if (string.IsNullOrEmpty(requestUrl))
            {
                throw new ArgumentNullException("request");
            }
            if (m_DataMonitor == null)
            {
                m_DataMonitor = new DataMonitor();
            }
            AveWebRequestExecutor request = new AveWebRequestExecutor(context, requestUrl, mChangeTokenFunc, mGetTenantIdAndDefaultAppIdFunc, m_DataMonitor);
            return request;
        }
        public DataMonitor DataMonitor
        {
            get 
            {
                if (m_DataMonitor == null)
                {
                    m_DataMonitor = new DataMonitor();
                }
                return this.m_DataMonitor; 
            }
        }
    }
}
