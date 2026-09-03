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


using System.Reflection;
using Microsoft.SharePoint.Client;
using System;
using System.Net;
using System.IO;


namespace AveClientRequest.Common
{
    public class AveWebRequestExecutor : WebRequestExecutor
    {
        private object m_SPWebRequestExecutor_Object = null;
        private Type m_SPWebRequestExecutor_Type = null;
        private AveWebStream m_RequestStream = null;
        private AveWebStream m_ResponseStream = null;
        private DataMonitor m_DataMonitor = null;

        public AveWebRequestExecutor(ClientRuntimeContext context, string requestUrl, DataMonitor dataMonitor)
        {
            Assembly ass = typeof(WebRequestExecutor).Assembly;
            m_SPWebRequestExecutor_Type = ass.GetType("Microsoft.SharePoint.Client.SPWebRequestExecutor");
            m_SPWebRequestExecutor_Object = Activator.CreateInstance(m_SPWebRequestExecutor_Type, context, requestUrl);
            m_DataMonitor = dataMonitor;
        }
        public object InvokeMethod(string methodName, params object[] parameters)
        {
            MethodInfo info = m_SPWebRequestExecutor_Type.GetMethod(methodName);
            return info.Invoke(m_SPWebRequestExecutor_Object, parameters);
        }
        public override void Execute()
        {
            InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
        }
        public override Stream GetRequestStream()
        {
            Stream request = (Stream)InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            if (m_RequestStream == null || (m_RequestStream != null && !m_RequestStream.Equals(request)))
            {
                m_DataMonitor.RecordStream();
                m_DataMonitor.ByteSend += this.RequestHeaders.ToString().Length;
            }
            m_RequestStream = new AveWebStream(request, m_DataMonitor);
            return m_RequestStream;
        }

        public override Stream GetResponseStream()
        {
            Stream response = (Stream)InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            if (m_ResponseStream == null || (m_ResponseStream != null && !m_ResponseStream.Equals(response)))
            {
                m_DataMonitor.ByteReceive += this.ResponseHeaders.ToString().Length;
            }
            m_ResponseStream = new AveWebStream(response, m_DataMonitor);
            return m_ResponseStream;
        }

        public override void Dispose()
        {
            InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            base.Dispose();
        }

        public long DataSend
        {
            get 
            {
                return this.m_DataMonitor.ByteSend;  
            }
        }

        public long DataReceive
        {
            get 
            {
                return this.m_DataMonitor.ByteReceive; 
            }
        }

        public override string RequestContentType
        {
            get
            {
                return (string)InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            }
            set
            {
                InvokeMethod(MethodBase.GetCurrentMethod().Name, value);
            }
        }

        public override WebHeaderCollection RequestHeaders
        {
            get 
            {
                return (WebHeaderCollection)InvokeMethod(MethodBase.GetCurrentMethod().Name, null); 
            }
        }

        public override bool RequestKeepAlive
        {
            get
            {
                return (bool)InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            }
            set
            {
                InvokeMethod(MethodBase.GetCurrentMethod().Name, value);
            }
        }

        public override string RequestMethod
        {
            get
            {
                return (string)InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            }
            set
            {
                InvokeMethod(MethodBase.GetCurrentMethod().Name, value);
            }
        }

        public override string ResponseContentType
        {
            get 
            {
                return (string)InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            }
        }

        public override WebHeaderCollection ResponseHeaders
        {
            get 
            {
                return (WebHeaderCollection)InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            }
        }

        public override HttpStatusCode StatusCode
        {
            get 
            {
                return (HttpStatusCode)InvokeMethod(MethodBase.GetCurrentMethod().Name, null);
            }
        }
        public override HttpWebRequest WebRequest
        {
            get
            {
                return (HttpWebRequest)InvokeMethod(MethodBase.GetCurrentMethod().Name, null); ;
            }
        }

        public object InnerWebRequestExecutor
        {
            get
            {
                return m_SPWebRequestExecutor_Object;
            }
        }

        public Type InnerWebRequestExecutorType
        {
            get
            {
                return m_SPWebRequestExecutor_Type;
            }
        }
    }
}
