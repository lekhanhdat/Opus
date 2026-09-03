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
    using System;
    using System.Net;

    using Microsoft.Exchange.WebServices.Data;

    using AvePoint.RA.CommonUtil;

    internal static class ServiceRequestExceptionExtension
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ServiceRequestExceptionExtension));

        public static bool WaitForNextRequest(this ServiceRequestException ex)
        {
            if (ex != null)
            {
                if (IsConnectionClosedError(ex)) return true;
                if (IsUnstableNetworkException(ex)) return true;
            }
            return false;
        }

        private static bool IsUnstableNetworkException(ServiceRequestException ex)
        {
            var webEx = ex.InnerException as WebException;
            if (webEx != null)
            {
                logger.Warn("WebExceptionStatus: {0}", webEx.Status);
                switch (webEx.Status)
                {
                    case WebExceptionStatus.NameResolutionFailure:
                    case WebExceptionStatus.SecureChannelFailure:
                    case WebExceptionStatus.ConnectFailure:
                    case WebExceptionStatus.KeepAliveFailure:
                    case WebExceptionStatus.ConnectionClosed:
                    case WebExceptionStatus.PipelineFailure:
                    case WebExceptionStatus.SendFailure:
                    case WebExceptionStatus.ReceiveFailure:
                    case WebExceptionStatus.UnknownError:
                    case WebExceptionStatus.Pending:
                    case WebExceptionStatus.Timeout:
                        return true;
                }

                var webResponse = webEx.Response as HttpWebResponse;
                if (webResponse != null)
                {
                    logger.Warn("WebResponse.StatusCode: {0}", webResponse.StatusCode);
                    switch (webResponse.StatusCode)
                    {
                        #region 503 Exception msg& stack

                        //Microsoft.Exchange.WebServices.Data.ServiceRequestException: The request failed.The remote server returned an error: (503) Server Unavailable. --->System.Net.WebException: The remote server returned an error: (503) Server Unavailable.
                        //at System.Net.HttpWebRequest.GetResponse()
                        //at Microsoft.Exchange.WebServices.Data.EwsHttpWebRequest.Microsoft.Exchange.WebServices.Data.IEwsHttpWebRequest.GetResponse()
                        //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.GetEwsHttpWebResponse(IEwsHttpWebRequest request)
                        //-- - End of inner exception stack trace-- -
                        // at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.GetEwsHttpWebResponse(IEwsHttpWebRequest request)
                        //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.ValidateAndEmitRequest(IEwsHttpWebRequest & request)
                        //at Microsoft.Exchange.WebServices.Data.SimpleServiceRequestBase.InternalExecute(Boolean retry)
                        //at Microsoft.Exchange.WebServices.Data.MultiResponseServiceRequest`1.InternalExecuteWithoutRetry()
                        //at ExchangeUtility.AADTokenRefresher.Retry(Func`1 tryBlockAction)
                        //at ExchangeUtility.Retryable.Retry(Func`1 tryBlockAction, Int32 maxRetryTime)
                        //at ExchangeUtility.Retryable.Retry(Func`1 tryBlockAction)
                        //at Microsoft.Exchange.WebServices.Data.MultiResponseServiceRequest`1.Execute()
                        //at Microsoft.Exchange.WebServices.Data.ExchangeService.InternalBindToFolders(IEnumerable`1 folderIds, PropertySet propertySet, ServiceErrorHandling errorHandling)
                        //at Microsoft.Exchange.WebServices.Data.ExchangeService.BindToFolder(FolderId folderId, PropertySet propertySet)
                        //at Microsoft.Exchange.WebServices.Data.ExchangeService.BindToFolder[TFolder](FolderId folderId, PropertySet propertySet)
                        //at ExchangeBackupUtility.ExchangeFolder.InternalOpen().Version:6.15.1.5008

                        #endregion

                        case HttpStatusCode.ServiceUnavailable:
                            return true;
                    }
                }
            }
            return false;
        }

        private static bool IsConnectionClosedError(Exception ex)
        {
            #region Exception msg& stack

            //Microsoft.Exchange.WebServices.Data.ServiceRequestException: The request failed. The underlying connection was closed: A connection that was expected to be kept alive was closed by the server.
            //---> System.Net.WebException: The underlying connection was closed: A connection that was expected to be kept alive was closed by the server.
            //---> System.IO.IOException: Unable to read data from the transport connection: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond.
            //---> System.Net.Sockets.SocketException: A connection attempt failed because the connected party did not properly respond after a period of time, or established connection failed because connected host has failed to respond
            //at System.Net.Sockets.Socket.EndReceive(IAsyncResult asyncResult)
            //at System.Net.Sockets.NetworkStream.EndRead(IAsyncResult asyncResult)
            //--- End of inner exception stack trace ---
            //at System.Net.Security._SslStream.EndRead(IAsyncResult asyncResult)
            //at System.Net.TlsStream.EndRead(IAsyncResult asyncResult)
            //at System.Net.PooledStream.EndRead(IAsyncResult asyncResult)
            //at System.Net.Connection.ReadCallback(IAsyncResult asyncResult)
            //--- End of inner exception stack trace ---
            //at System.Net.HttpWebRequest.GetResponse()
            //at Microsoft.Exchange.WebServices.Data.EwsHttpWebRequest.Microsoft.Exchange.WebServices.Data.IEwsHttpWebRequest.GetResponse()
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.GetEwsHttpWebResponse(IEwsHttpWebRequest request)
            //--- End of inner exception stack trace ---
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.GetEwsHttpWebResponse(IEwsHttpWebRequest request)
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.ValidateAndEmitRequest(IEwsHttpWebRequest& request)
            //at Microsoft.Exchange.WebServices.Data.SimpleServiceRequestBase.InternalExecute(Boolean retry)
            //at Microsoft.Exchange.WebServices.Data.MultiResponseServiceRequest`1.InternalExecuteWithoutRetry()
            //at ExchangeUtility.AADTokenRefresher.Retry(Func`1 tryBlockAction)
            //at ExchangeUtility.Retryable.Retry(Func`1 tryBlockAction, Int32 maxRetryTime)

            //ERROR 08-01 14:25:45,730 11 ExchangeOnlineBackup.BaseBackupHelper 0- Failed to sync items, error: Microsoft.Exchange.WebServices.Data.ServiceRequestException: The response received from the service didn't contain valid XML. ---> System.Xml.XmlException: Root element is missing.
            //at System.Xml.XmlTextReaderImpl.Throw(Exception e)
            //at System.Xml.XmlTextReaderImpl.ParseDocumentContent()
            //at System.Xml.XmlTextReaderImpl.Read()
            //at System.Xml.XmlTextReader.Read()
            //at System.Xml.XmlCharCheckingReader.Read()
            //at Microsoft.Exchange.WebServices.Data.EwsXmlReader.Read()
            //at Microsoft.Exchange.WebServices.Data.EwsXmlReader.Read(XmlNodeType nodeType)
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.ReadXmlDeclaration(EwsServiceXmlReader reader)
            //--- End of inner exception stack trace ---
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.ReadXmlDeclaration(EwsServiceXmlReader reader)
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.ReadPreamble(EwsServiceXmlReader ewsXmlReader)
            //at Microsoft.Exchange.WebServices.Data.ServiceRequestBase.ReadResponse(EwsServiceXmlReader ewsXmlReader, WebHeaderCollection responseHeaders)
            //at Microsoft.Exchange.WebServices.Data.SimpleServiceRequestBase.ReadResponseXml(Stream responseStream, WebHeaderCollection responseHeaders)
            //at Microsoft.Exchange.WebServices.Data.SimpleServiceRequestBase.ReadResponse(IEwsHttpWebResponse response)
            //at Microsoft.Exchange.WebServices.Data.SimpleServiceRequestBase.InternalExecute(Boolean retry)
            //at Microsoft.Exchange.WebServices.Data.MultiResponseServiceRequest`1.InternalExecuteWithoutRetry()
            //at ExchangeUtility.AADTokenRefresher.Retry(Func`1 tryBlockAction, RequestInfo requestInfo)
            //at ExchangeUtility.Retryable.Retry(Func`1 tryBlockAction, RequestInfo requestInfo, Int32 maxRetryTime)
            //at ExchangeUtility.Retryable.Retry(Func`1 tryBlockAction, RequestInfo requestInfo)
            //at Microsoft.Exchange.WebServices.Data.MultiResponseServiceRequest`1.Execute()
            //at ExchangeBackupUtility.ExchangeFolder.SyncItems(Int32 pageSize, String& syncState, List`1& items, List`1& deleteItemIds)
            //at ExchangeOnlineBackup.FolderEntityBackupHelper.<>c__DisplayClass9_0.<GetSubItemsAsync>b__0(Object obj)    Version:6.19.0.5002

            #endregion

            if (ex == null) return false;
            if (ex is System.IO.IOException ||
                ex is System.Net.Sockets.SocketException ||//todo: check ErrorCode
                ex is System.Xml.XmlException)
            {
                return true;
            }
            return IsConnectionClosedError(ex.InnerException);
        }
    }
}