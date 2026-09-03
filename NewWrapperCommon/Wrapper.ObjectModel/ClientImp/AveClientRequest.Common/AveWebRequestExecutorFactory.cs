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


using Microsoft.SharePoint.Client;
using System;

namespace AveClientRequest.Common
{
    public class AveWebRequestExecutorFactory : WebRequestExecutorFactory
    {
        AveWebRequestExecutor m_AveWebRequestExecutor = null;
        DataMonitor m_DataMonitor = null;
        public AveWebRequestExecutorFactory(DataMonitor dataMonitor)
        {
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
            return new AveWebRequestExecutor(context, requestUrl, m_DataMonitor);
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
