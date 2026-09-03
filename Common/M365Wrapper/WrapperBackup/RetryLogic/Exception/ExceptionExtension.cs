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


namespace ExchangeUtility.Graph
{
    using AvePoint.GCommon.GraphAPI;
    using ExchangeCommonWrapper;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Net.Sockets;
    using System.Text;

    public static class ExceptionExtension
    {
        internal static bool WaitForNextRequest(this Exception xEx)
        {
            #region error msg
            //Failed to sync items, error: System.ArgumentNullException: Array cannot be null. Parameter name: bytes
            //at System.Text.ASCIIEncoding.GetBytes(String chars, Int32 charIndex, Int32 charCount, Byte[] bytes, Int32 byteIndex)
            //at System.Net.HttpWebRequest.GenerateRequestLine(Int32 headersSize)
            //at System.Net.HttpWebRequest.SerializeHeaders()
            //at System.Net.HttpWebRequest.EndSubmitRequest()
            //at System.Net.HttpWebRequest.CheckDeferredCallDone(ConnectStream stream)
            //at System.Net.HttpWebRequest.GetResponse()
            //at Microsoft.Exchange.WebServices.Data.EwsHttpWebRequest.Microsoft.Exchange.WebServices.Data.IEwsHttpWebRequest.GetResponse()
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.GetEwsHttpWebResponse(IEwsHttpWebRequest request)
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.ValidateAndEmitRequest(IEwsHttpWebRequest& request)
            //at Microsoft.Exchange.WebServices.Data.SimpleServiceRequestBase.InternalExecute(Boolean retry)
            //at Microsoft.Exchange.WebServices.Data.MultiResponseServiceRequest`1.InternalExecuteWithoutRetry()
            //at ExchangeUtility.AADTokenRefresher.Retry(Func`1 tryBlockAction, RequestInfo requestInfo)
            //at ExchangeUtility.Retryable.Retry(Func`1 tryBlockAction, RequestInfo requestInfo, Int32 maxRetryTime)
            //at ExchangeUtility.Retryable.Retry(Func`1 tryBlockAction, RequestInfo requestInfo)
            //at Microsoft.Exchange.WebServices.Data.MultiResponseServiceRequest`1.Execute()
            //at ExchangeBackupUtility.ExchangeFolder.SyncItems(Int32 pageSize, String& syncState, List`1& items, List`1& deleteItemIds)
            //at ExchangeOnlineBackup.FolderEntityBackupHelper.<>c__DisplayClass9_0.<GetSubItemsAsync>b__0(Object obj)    Version:6.19.0.5003  
            #endregion
            //retry immediately
            return true;
        }

        public static bool IsSkipException(this Exception ex)
        {
            return ex is ExchangeSkipException;
        }

        public static bool IsConnectonForciblyClosedExceptioin(this Exception te)
        {
            if (te is SocketException)
            {
                return true;
            }
            else if (te.InnerException != null && !string.IsNullOrEmpty(te.InnerException.Message) &&
               (te.InnerException.Message.Contains("An existing connection was forcibly closed by the remote host") ||
                te.InnerException.Message.Contains("The underlying connection was closed") ||
                te.InnerException.Message.Contains("Connection reset by peer")))
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsConnectonForciblyClosedExceptioin(te.InnerException);
            }
            return false;
        }

        public static bool IsTaskCanceledExceptioin(this Exception te)
        {
            if (te.InnerException != null && !string.IsNullOrEmpty(te.InnerException.Message) && te.InnerException.Message.Contains("A task was canceled"))
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsTaskCanceledExceptioin(te.InnerException);
            }
            return false;
        }
        public static bool IsErrorRequestExceptioin(this Exception te)
        {
            if (te.InnerException != null && !string.IsNullOrEmpty(te.InnerException.Message) && (te.InnerException.Message.Contains("The remote name could not be resolved")
                || te.InnerException.Message.Contains("The request was aborted") || te.InnerException.Message.Contains("An error occurred while sending the request")))

            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsErrorRequestExceptioin(te.InnerException);
            }
            return false;
        }

        public static bool IsTimeOutExceptioin(this Exception te)
        {
            if (te.InnerException != null && (te.InnerException is TimeoutException ||
                (!string.IsNullOrEmpty(te.InnerException.Message) && te.InnerException.Message.Contains("The operation has timed out."))))
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsTimeOutExceptioin(te.InnerException);
            }
            return false;
        }

        public static bool IsHostedContentExceedLimitLength(this Exception ex, List<HostedContent> hostedContent, int limit)
        {
            if (ex is GraphAPIException &&
                (ex.Message.Contains("The maximum number of bytes allowed to be read from the stream has been exceeded") ||
                 (ex.Message.Contains("status code: ServiceUnavailable, internal error code:UnknownError") && IsExceedLimit(hostedContent, limit))))

            {
                return true;
            }
            if (ex.InnerException != null)
            {
                return IsHostedContentExceedLimitLength(ex.InnerException, hostedContent, limit);
            }
            return false;
        }

        private static bool IsExceedLimit(List<HostedContent> hostedContent, int limit)
        {
            if (hostedContent == null)
            {
                return false;
            }

            return hostedContent.Sum(h => Encoding.UTF8.GetByteCount(h.ContentBytes)) > limit;
        }
    }
}