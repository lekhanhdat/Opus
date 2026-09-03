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


namespace ExchangeUtility
{
    using System;
    static class ExceptionExtension
    {
        public static bool WaitForNextRequest(this Exception xEx)
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

        public static bool IsConnectonForciblyClosedExceptioin(this Exception te)
        {
            if (te.InnerException != null && !string.IsNullOrEmpty(te.InnerException.Message) &&
               (te.InnerException.Message.Contains("An existing connection was forcibly closed by the remote host") ||
                te.InnerException.Message.Contains("The underlying connection was closed")))
            {
                return true;
            }
            else if (te.InnerException != null)
            {
                return IsConnectonForciblyClosedExceptioin(te.InnerException);
            }
            return false;
        }
    }
}
