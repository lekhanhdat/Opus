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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Client;
using System;

namespace AvePoint.ObjectModel.O365
{
    public class AveO365ClientContext : AveClientContext, IDisposable
    {
        private DataMonitor m_DataMonitor = null;
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
        public AveO365ClientContext(string webFullUrl)
            : base(webFullUrl)
        {
            this.WebRequestExecutorFactory = new AveWebRequestExecutorFactory(this.DataMonitor);
        }
        

        public override void ExecuteQuery()
        {
            if (base.HasPendingRequest)
            {
                //long s = this.DataMonitor.ByteSend;
                //long r = this.DataMonitor.ByteReceive;
                base.ExecuteQuery();
                //long s2 = this.DataMonitor.ByteSend;
                //long r2 = this.DataMonitor.ByteReceive;
            }
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
