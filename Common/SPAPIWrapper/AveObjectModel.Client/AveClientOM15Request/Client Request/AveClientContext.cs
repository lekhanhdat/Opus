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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;
using System.Net;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveClientContext : ClientContext, IDisposable
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveClientContext));
        private DataMonitor m_DataMonitor = null;
        private Guid mTenantId = Guid.Empty;
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
        public AveClientContext(string webFullUrl, string tenantId = null, Action<WebRequest> changeTokenFunc = null, Func<(Guid tenantId, string defaultAppId)> getTenantIdAndDefaultAppIdFunc = null)
            : base(webFullUrl)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(tenantId))
                {
                    mTenantId = new Guid(tenantId);
                }
            }
            catch (Exception e)
            {
                mLogger.Error($"error occured in AveClientContext,error:{e}");
            }
            this.WebRequestExecutorFactory = new AveWebRequestExecutorFactory(this.DataMonitor, changeTokenFunc, getTenantIdAndDefaultAppIdFunc);
        }
        

        public override void ExecuteQuery()
        {
            if (base.HasPendingRequest)
            {
                string scopeName = GetPerformanceScope();
                //long s = this.DataMonitor.ByteSend;
                //long r = this.DataMonitor.ByteReceive;
                using (new AveRequestStatisticScope(scopeName))
                {
                    base.ExecuteQuery();
                }
                //long s2 = this.DataMonitor.ByteSend;
                //long r2 = this.DataMonitor.ByteReceive;
            }
        }

        private string GetPerformanceScope()
        {
            try
            {
                // Skip 2 frames:
                // Frame 0: GetPerformanceScope()
                // Frame 1: AveClientContext.ExecuteQuery()
                var st = new System.Diagnostics.StackTrace(skipFrames: 2, fNeedFileInfo: false);

                for (int i = 0; i < st.FrameCount; i++)
                {
                    var method = st.GetFrame(i)?.GetMethod();
                    var declaringType = method?.DeclaringType;

                    if (declaringType == null) continue;

                    if (typeof(ClientContext).IsAssignableFrom(declaringType)) continue;

                    bool isCompilerGenerated = declaringType.IsDefined(typeof(System.Runtime.CompilerServices.CompilerGeneratedAttribute), false);

                    if (isCompilerGenerated)
                    {
                        // async/yield/iterator method
                        var realClass = declaringType.DeclaringType?.Name ?? declaringType.Name;

                        string realMethod = declaringType.Name;
                        int startIndex = realMethod.IndexOf('<');
                        int endIndex = realMethod.IndexOf('>');
                        if (startIndex >= 0 && endIndex > startIndex)
                        {
                            realMethod = realMethod.Substring(startIndex + 1, endIndex - startIndex - 1);
                        }

                        return $"AveClientContext.ExecuteQuery-{realClass}.{realMethod}";
                    }
                    else
                    {
                        // sync
                        return $"AveClientContext.ExecuteQuery-{declaringType.Name}.{method.Name}";
                    }
                }
            }
            catch (Exception e)
            {
                mLogger.Error("[AveClientContext.GetPerformanceScope] An error occurred while getting caller class and method. Error:{0}", e);
            }

            return "AveClientContext.ExecuteQuery-Unknown";
        }

        public void Dispose()
        {
            RecordDataMonitor();
            base.Dispose();
        }

        private void RecordDataMonitor()
        {
            //mLogger.Debug("method {0} completed successfully. Memory used: {1} MB", this.MethodName, System.Diagnostics.Process.GetCurrentProcess().PrivateMemorySize64 / (1000.00 * 1024));
            //DataMonitor monitor = (request as AveClientOMRequest).getDataMonitor();
            long bytesReceived = this.DataMonitor.ByteReceive - this.DataMonitor.ByteLastReceive;
            long bytesSent = this.DataMonitor.ByteSend - this.DataMonitor.ByteLastSend;
            if (bytesSent != 0 || bytesReceived != 0)
            {
                //mLogger.Debug("method {0} completed successfully. Stream size: {1} BYTE received, {2} BYTE sent", this.MethodName, bytesReceived, bytesSent);                
                System.Threading.Interlocked.Add(ref AveStreamStatistics.streamReceived, bytesReceived);
                System.Threading.Interlocked.Add(ref AveStreamStatistics.streamSent, bytesSent);
                this.DataMonitor.RecordStream();
            }
        }
    }
}
