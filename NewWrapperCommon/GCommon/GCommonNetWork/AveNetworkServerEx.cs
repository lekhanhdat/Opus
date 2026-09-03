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

namespace AvePoint.GCommon.Network
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.Net.Sockets;

    #endregion

    public class AveNetworkServerEx
        : AveNetworkServer
        , IAveNetworkServer
    {
        public IAveNetworkEventEx NetworkEvent { get; set; }

        public AveNetworkServerEx(
            Int32 listenPort,
            IAveNetworkEventEx networkEvent)
            : base(listenPort, null, false, null)
        { }

        protected override void SocketAccepted(Object socketObj)
        {
            Socket socket = null;
            try
            {
                socket = socketObj as Socket;
                var targetForwardProcessId = this.NetworkEvent.GetForwardProcess();
                if (socket != null)
                {
                    var socketInformation = socket.DuplicateAndClose(targetForwardProcessId);
                    this.NetworkEvent.ForwardSocketToTargetProcess(targetForwardProcessId, socketInformation);
                }
            }
            catch (Exception ex)
            {
                AveNetworkTrace.TraceError("An error occurred while forwarding socket to runner service. {0}", ex.ToString());
                try
                {
                    if (socket != null && socket.Connected)
                        socket.Close();
                }
                catch (Exception e)
                {
                    AveNetworkTrace.TraceError("An error occurred while closing socket. {0}", e.ToString());
                }
            }
        }

        protected override void CloseRunnerProcess()
        {
            var processes = Process.GetProcessesByName("MediaServiceJobRunner");
            foreach (var pro in processes)
            {
                try
                {
                    pro.Kill();
                }
                catch (Exception ex)
                {
                    AveNetworkTrace.TraceWarning("An warn occurred while closing MediaServiceJobRunner. {0}", ex.ToString());
                }
            }
        }

        public override String ToString()
        {
            return String.Format("Ave Network currently listen on port:{0}", this.ListeningPort);
        }
    }
}