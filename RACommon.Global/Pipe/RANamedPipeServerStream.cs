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
using System.Collections.Generic;
using System.IO.Pipes;

namespace AvePoint.RA.Common.Pipe
{
    /// <summary>
    /// Server 端：双向同步管道
    /// </summary>
    public class RANamedPipeServerStream : IDisposable
    {

        #region private property

        private static readonly PipeDirection DefaultPipeDirection = PipeDirection.InOut;

        private static readonly int MinNumberOfServerInstances = 1;

        private static readonly int MaxNumberOfServerInstances = 254;

        private static readonly int DefaultNumberOfServerInstances = 127;

        private readonly HashSet<RANamedPipeServerStreamInner> PipeServerStreams = new HashSet<RANamedPipeServerStreamInner>();

        private Action<RANamedPipeServerStreamInner> ClientOnConnect { get; set; }

        #endregion

        #region public property

        public string PipeName { get; }

        public PipeDirection Direction { get; }

        public int NumberOfServerInstances { get; }

        public bool IsConnected { get => PipeServerStreams.Count > 0; }

        #endregion

        public RANamedPipeServerStream(string pipeName)
            : this(pipeName, DefaultNumberOfServerInstances)
        {

        }

        public RANamedPipeServerStream(string pipeName, int numberOfServerInstances)
        {
            PipeName = pipeName;
            Direction = DefaultPipeDirection;
            NumberOfServerInstances = numberOfServerInstances;
            if(NumberOfServerInstances < 1)
            {
                NumberOfServerInstances = MinNumberOfServerInstances;
            }
            else if(NumberOfServerInstances > 254)
            {
                NumberOfServerInstances = MaxNumberOfServerInstances;
            }
        }

        public void Connect()
        {
            if (!IsConnected)
            {
                var initalCount = NumberOfServerInstances - PipeServerStreams.Count;
                for (var i = 0; i < initalCount; i++)
                {
                    var pipeServer = new RANamedPipeServerStreamInner(PipeName, Direction, NumberOfServerInstances, ClientOnConnect, PipeServerStreamEndConnect);
                    PipeServerStreams.Add(pipeServer);
                }
            }
        }

        public void RegisterClientOnConnectCallBack(Action<RANamedPipeServerStreamInner> clientOnConnect)
        {
            ClientOnConnect = clientOnConnect;
        }

        private void PipeServerStreamEndConnect(RANamedPipeServerStreamInner pipeServerStream)
        {
            pipeServerStream.Dispose();
            PipeServerStreams.Remove(pipeServerStream);
            var pipeServer = new RANamedPipeServerStreamInner(PipeName, Direction, NumberOfServerInstances, ClientOnConnect, PipeServerStreamEndConnect);
            PipeServerStreams.Add(pipeServer);
        }

        public void Dispose()
        {
            foreach(var pipeServer in PipeServerStreams)
            {
                pipeServer.Dispose();
            }
            PipeServerStreams.Clear();
        }
    }
}
