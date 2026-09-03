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
using System.Net;
using System.Net.Sockets;

//using Microsoft365Backup.Logger;

//using Polly;
namespace Microsoft365.Graph.Core;

public static class SocketHttpHandlerExtension
{
    //private static ICloudBackupLogger logger = CloudBackupLogManager.Get(typeof(SocketsHttpHandler));
    public static SocketsHttpHandler ConfigureCallBack(this SocketsHttpHandler socketsHttpHandler, bool async = true)
    {
        if (async)
        {
            socketsHttpHandler.ConnectCallback = ConnectCallbackAsync;
        }
        else
        {
            socketsHttpHandler.ConnectCallback = ConnectCallback;
        }
        return socketsHttpHandler;
    }

    private static async ValueTask<Stream> ConnectCallbackAsync(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        Stream? stream = null;
        Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            await socket.ConnectAsync(context.DnsEndPoint, cancellationToken).ConfigureAwait(false);
            OutputSocketInfo(context, socket);
            stream = new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
        return stream;
    }
    public static void OutputSocketInfo(SocketsHttpConnectionContext context, Socket socket)
    {
        //logger.Info($"DnsEndPoint:{(context.DnsEndPoint)},RemoteEndPoint:{socket.RemoteEndPoint}");
    }

    private static ValueTask<Stream> ConnectCallback(SocketsHttpConnectionContext context, CancellationToken cancellationToken)
    {
        Stream? stream = null;
        Socket socket = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
        try
        {
            using (cancellationToken.UnsafeRegister(static s => ((Socket)s!).Dispose(), socket))
            {
                socket.Connect(context.DnsEndPoint);
            }
            OutputSocketInfo(context, socket);
            stream = new NetworkStream(socket, ownsSocket: true);
        }
        catch
        {
            socket.Dispose();
            throw;
        }
        return ValueTask.FromResult(stream);
    }
}
