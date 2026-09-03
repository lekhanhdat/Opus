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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.AzureFileShare.Model;
using AvePoint.RA.Contract.AzureFileShare.Model.Api;
using AvePoint.RA.Service.Services.AzureFileShare.Exceptions;
using Azure;
using Azure.Storage;
using Azure.Storage.Files.Shares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.AzureFileShare.Api
{
    public class AzureFileShareApiContext
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(AzureFileShareApiContext));

        private readonly object Locker = new object();

        public bool IsConnected { get; private set; }

        public AzureFileShareConnectionInfo ConnectionInfo { get; private set; }

        public ShareClient ShareClient { get; private set; }

        public string ConnectionFullUrl { get; private set; }

        public AzureFileShareApiContext(AzureFileShareConnectionInfo connectionInfo)
        {
            ConnectionInfo = connectionInfo;
            ConnectionFullUrl = AzureFileShareApiUtil.UrlCombin(connectionInfo.AccessEndPoint, connectionInfo.FileShareName);
        }

        public bool ValidateConnection()
        {
            try
            {
                var serviceUri = new Uri(ConnectionInfo.AccessEndPoint);
                var credential = new StorageSharedKeyCredential(ConnectionInfo.AccountName, ConnectionInfo.AccountKey);
                var service = new ShareServiceClient(serviceUri, credential);
                var shareClient = service.GetShareClient(ConnectionInfo.FileShareName);
                var response = shareClient.Exists().GetRawResponse();
                var exist = shareClient.Exists().Value;

                if (!exist)
                {
                    Logger.Error($"Failed to connection azure storage file share, has an reponse error: {response.ReasonPhrase}");
                    return false;
                }
                else
                {
                    var directoryName = "AvePoint_Cloud_Record_Directory_" + Guid.NewGuid();
                    var createdDirectory = shareClient.CreateDirectory(directoryName);
                    var createdDirectoryResponse = createdDirectory.GetRawResponse();
                    Logger.Info($"Created directory [{directoryName}] test response status [{createdDirectoryResponse.Status}]");

                    var deletedDirectoryResponse = shareClient.DeleteDirectory(directoryName);
                    Logger.Info($"Deleted directory [{directoryName}] test response status [{createdDirectoryResponse.Status}]");

                    return true;
                }
            }
            catch (RequestFailedException e)
            {
                Logger.Error($"Failed to connect azure storage file share, has request failed exception. Error: {e}");
                return false;
            }
            catch (AggregateException e)
            {
                if (e.InnerException.GetType() == typeof(RequestFailedException))
                {
                    Logger.Error($"Failed to connect azure storage file share, has request failed exception. Error: {e.InnerException}");
                }
                else
                {
                    Logger.Error($"Failed to connect azure storage file share, has aggregate exception. Error: {e}");
                }
                return false;
            }
            catch (Exception e)
            {
                Logger.Error($"Failed to connect azure storage file share. Error: {e}");
                return false;
            }
        }

        public bool EnsureConnection()
        {
            if(!IsConnected)
            {
                lock (Locker)
                {
                    if(!IsConnected)
                    {
                        try
                        {
                            var serviceUri = new Uri(ConnectionInfo.AccessEndPoint);
                            var credential = new StorageSharedKeyCredential(ConnectionInfo.AccountName, ConnectionInfo.AccountKey);
                            var service = new ShareServiceClient(serviceUri, credential);
                            var shareClient = service.GetShareClient(ConnectionInfo.FileShareName);
                            var response = shareClient.Exists().GetRawResponse();
                            var exist = shareClient.Exists().Value;
                            
                            if (!exist)
                            {
                                IsConnected = false;
                                Logger.Error($"Failed to connection azure storage file share, has an reponse error: {response.ReasonPhrase}");
                            }
                            else
                            {
                                IsConnected = true;
                                ShareClient = shareClient;
                            }
                        }
                        catch (RequestFailedException e)
                        {
                            IsConnected = false;
                            Logger.Error($"Failed to connect azure storage file share, has request failed exception. Error: {e}");
                        }
                        catch (AggregateException e)
                        {
                            IsConnected = false;
                            if (e.InnerException.GetType() == typeof(RequestFailedException))
                            {
                                Logger.Error($"Failed to connect azure storage file share, has request failed exception. Error: {e.InnerException}");
                            }
                            else
                            {
                                Logger.Error($"Failed to connect azure storage file share, has aggregate exception. Error: {e}");
                            }
                        }
                        catch (Exception e)
                        {
                            IsConnected = false;
                            Logger.Error($"Failed to connect azure storage file share. Error: {e}");
                        }
                    }
                }
            }
            return IsConnected;
        }

        internal ShareDirectoryClient GetDirectory(string relativePath)
        {
            if(!EnsureConnection())
            {
                throw new AzureFileShareApiException($"Failed to connection azure storage file share.");
            }
            return ShareClient.GetDirectoryClient(relativePath);
        }

        internal ShareFileClient GetFile(string relativePath)
        {
            if (!EnsureConnection())
            {
                throw new AzureFileShareApiException($"Failed to connection azure storage file share.");
            }
            var rootDirectoryClient = ShareClient.GetRootDirectoryClient();
            return rootDirectoryClient.GetFileClient(relativePath);
        }
    }
}
