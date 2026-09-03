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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public sealed class AveExceptionHelper
    {
        public static bool IsConnectionException(Exception e)
        {
            return IsConnectonForciblyClosedExceptioin(e) || IsConnectionFailureException(e);
        }

        public static bool IsConnectionFailureException(Exception e)
        {
            WebException webException = GetCertainInnerException<WebException>(e);
            if (webException != null)
            {
                return webException.Status == WebExceptionStatus.ConnectFailure;
            }
            return false;
        }

        public static bool IsConnectonForciblyClosedExceptioin(Exception e)
        {
            if (e.InnerException is SocketException || e.InnerException is IOException)
            {
                return true;
            }
            else if (e.InnerException != null)
            {
                return IsConnectonForciblyClosedExceptioin(e.InnerException);
            }
            return false;
        }

        public static bool IsHTTP429Error(Exception e, ref int interval)
        {
            #region get 429 error information for debug
            if (e is WebException)
            {
                HttpWebResponse response = (e as WebException).Response as HttpWebResponse;
                if (response != null)
                {
                    //mLogger.Debug("Http 429 Error Information.Message:{0},StatusCode:{1}", e.ToString(), (int)(response.StatusCode));
                    if (response.Headers != null)
                    {
                        StringBuilder headers = new StringBuilder();
                        foreach (string header in response.Headers.AllKeys)
                        {
                            headers.AppendLine(header + ":" + response.Headers[header]);
                        }
                        //mLogger.Debug("429 Error response headers:{0}", headers.ToString());
                    }
                }
            }
            #endregion

            if (e is WebException)
            {
                HttpWebResponse response = (e as WebException).Response as HttpWebResponse;
                if (response != null && (int)response.StatusCode == 429)
                {
                    interval = response.Headers != null && response.Headers.AllKeys.Contains("Retry-After") ? Convert.ToInt32(response.Headers["Retry-After"]) * 1000 : -1;
                    return true;
                }
            }
            else if (e.InnerException != null)
            {
                return IsHTTP429Error(e.InnerException, ref interval);
            }
            return false;
        }

        public static TException GetCertainInnerException<TException>(System.Runtime.InteropServices._Exception exception)
        {
            if (exception.InnerException is TException)
            {
                return GetCertainInnerException<TException>(exception.InnerException);
            }
            if (exception is TException)
            {
                return (TException)exception;
            }
            return default(TException);
        }
    }
}
