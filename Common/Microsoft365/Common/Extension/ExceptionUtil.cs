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
//using Microsoft365.Authentication.ADAL;

namespace Microsoft365.Common;
public static class ExceptionUtil
{
    public static bool IsSocketOrIOException(this System.Exception ex)
    {
        #region Exception samples
        // ConnectionRest: 104 is NativeErrorCode on linux, 10054 is NativeErrorCode on window
        // use ScoketException.SocketError enum for cross-platform compatibility
        //System.IO.IOException: Unable to read data from the transport connection: Connection reset by peer.
        //  ---> System.Net.Sockets.SocketException (104): Connection reset by peer
        //    --- End of inner exception stack trace ---
        //    at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
        //    at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource<System.Int32>.GetResult(Int16 token)
        //    at System.Net.Security.SslStream.EnsureFullTlsFrameAsync[TIOAdapter](TIOAdapter adapter)
        //    at System.Net.Security.SslStream.ReadAsyncInternal[TIOAdapter](TIOAdapter adapter, Memory`1 buffer)
        //    at System.Net.Http.HttpConnection.ReadAsync(Memory`1 destination)
        //    at System.Net.Http.HttpConnection.ContentLengthReadStream.ReadAsync(Memory`1 buffer, CancellationToken cancellationToken)
        //    at Microsoft365.Common.HttpReadOnlyStream.ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken)
        //    at OneDriveForBusiness.ModernBackup.BackupWrapper.DriveItemBackup.ExportContentAsync()
        //System.IO.IOException: Unable to read data from the transport connection: An existing connection was forcibly closed by the remote host..
        //  --->System.Net.Sockets.SocketException(10054): An existing connection was forcibly closed by the remote host.
        //   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.CreateException(SocketError error, Boolean forAsyncThrow)
        //   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ReceiveAsync(Socket socket, CancellationToken cancellationToken)
        //   at System.Net.Sockets.NetworkStream.ReadAsync(Memory`1 buffer, CancellationToken cancellationToken)
        //   at System.Net.Security.SslStream.EnsureFullTlsFrameAsync[TIOAdapter](TIOAdapter adapter)

        //System.IO.IOException: Unable to read data from the transport connection: The I/ O operation has been aborted because of either a thread exit or an application request..
        //  --->System.Net.Sockets.SocketException(995): The I/ O operation has been aborted because of either a thread exit or an application request.
        //   ---End of inner exception stack trace-- -
        //   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
        //   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource<System.Int32>.GetResult(Int16 token)
        //   at System.Net.Security.SslStream.EnsureFullTlsFrameAsync[TIOAdapter](TIOAdapter adapter)
        //   at System.Net.Security.SslStream.ReadAsyncInternal[TIOAdapter](TIOAdapter adapter, Memory`1 buffer)
        //   at System.Net.Http.HttpConnection.ReadAsync(Memory`1 destination)
        //   at System.Net.Http.HttpConnection.ContentLengthReadStream.ReadAsync(Memory`1 buffer, CancellationToken cancellationToken)
        //   at Microsoft365.Common.HttpReadOnlyStream.ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken) in C:\code\CB\cloud\BackupLite\common\Microsoft365\Common\Stream\HttpReadOnlyStream.cs:line 71
        //   at Microsoft365.Graph.Service.Tests.GraphDriveServiceTests.DownloadFileTest2() in C:\code\CB\cloud\BackupLite\common\Microsoft365\Microsoft365.Graph.DriveTests\Service\GraphDriveServiceTests.cs:line 92


        //Task cancel
        //System.Threading.Tasks.TaskCanceledException: The operation was canceled.
        // --->System.IO.IOException: Unable to read data from the transport connection: The I/ O operation has been aborted because of either a thread exit or an application request..--->System.Net.Sockets.SocketException(995): The I/ O operation has been aborted because of either a thread exit or an application request.
        //   ---End of inner exception stack trace-- -
        //   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.ThrowException(SocketError error, CancellationToken cancellationToken)
        //   at System.Net.Sockets.Socket.AwaitableSocketAsyncEventArgs.System.Threading.Tasks.Sources.IValueTaskSource<System.Int32>.GetResult(Int16 token)
        //   at System.Net.Security.SslStream.EnsureFullTlsFrameAsync[TIOAdapter](TIOAdapter adapter)
        //   at System.Net.Security.SslStream.ReadAsyncInternal[TIOAdapter](TIOAdapter adapter, Memory`1 buffer)
        //   at System.Net.Http.HttpConnection.ReadAsync(Memory`1 destination)
        //   at System.Net.Http.HttpConnection.ContentLengthReadStream.ReadAsync(Memory`1 buffer, CancellationToken cancellationToken)
        //   -- - End of inner exception stack trace-- -
        //   at System.Net.Http.HttpConnection.ContentLengthReadStream.ReadAsync(Memory`1 buffer, CancellationToken cancellationToken)
        //   at Microsoft365.Common.HttpReadOnlyStream.ReadAsync(Byte[] buffer, Int32 offset, Int32 count, CancellationToken cancellationToken) in C:\code\CB\cloud\BackupLite\common\Microsoft365\Common\Stream\HttpReadOnlyStream.cs:line 71
        //   at Microsoft365.Graph.Service.Tests.GraphDriveServiceTests.DownloadFileTest2() in C:\code\CB\cloud\BackupLite\common\Microsoft365\Microsoft365.Graph.DriveTests\Service\GraphDriveServiceTests.cs:line 92
        #endregion
        
        //从目前的经验上看，对于.Net6 HttpClient系列库，只判断IOException就足够了，也不需要递归InnerException。
        //但此处作为公用类库，也用在一些老旧用法处，故暂时保持下面的行为。
        if (ex is SocketException || ex is IOException)
        {
            return true;
        }
        else if (ex.InnerException != null)
        {
            return IsSocketOrIOException(ex.InnerException);
        }
        return false;
    }

    public static bool IsIncorrectUserNamePwdException(this System.Exception se)
    {
        return false;
        //return se is not null &&
        //    ((se is AdalException adalEx && adalEx.ErrorCode.EqualsIgnoreCase("invalid_grant"))
        //        || IsIncorrectUserNamePwdException(se.InnerException));
    }
  
}
