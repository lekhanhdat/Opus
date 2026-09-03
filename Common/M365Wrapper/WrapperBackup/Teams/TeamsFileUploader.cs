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
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using Microsoft365.Authentication;
using AvePoint.Wrapper.Common;
using ExchangeUtility.Graph.SharePointRestAPI;
using Microsoft365.SharePoint.Extension;
using AvePoint.RA.CommonUtil;

namespace ExchangeUtility.Graph
{
    public class TeamsFileUploader
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(TeamsFileUploader));

        public static void Test()
        {
            
        }

        private string siteUrl;
        private string webServerRelativeUrl;
        private string docLibServerRelativeUrl;
        private ITokenProvider tokenProvider;
        private const int BUFFER_LENGTH = 64 * 1024;
        private const string DocumentsUrl = "Shared Documents";

        /// <summary>
        ///
        /// </summary>
        /// <param name="siteUrl">https://m365x124511.sharepoint.com/sites/testgroup111</param>
        /// <param name="tokenProvider"></param>
        public TeamsFileUploader(string siteUrl, string filesUrl, ITokenProvider tokenProvider)
        {
            this.siteUrl = siteUrl.TrimEnd('/');
            this.webServerRelativeUrl = new Uri(this.siteUrl).AbsolutePath.TrimEnd('/');
            string filesName = !string.IsNullOrEmpty(filesUrl) ? filesUrl : DocumentsUrl;
            this.docLibServerRelativeUrl = AppendUrl(this.webServerRelativeUrl, filesName);
            this.tokenProvider = tokenProvider;
        }

        public TeamsFileUploader(ITokenProvider tokenProvider)
        {
            this.tokenProvider = tokenProvider;
        }

        /// <summary>
        /// Upload file to document library
        /// </summary>
        /// <param name="folderName">string.Empty, folder1 or folder1/folder2</param>
        /// <param name="fileName">file1.txt</param>
        /// <param name="content">recommend FileStream or MemoryStream</param>
        /// <param name="overwrite">true: overwrite if exist(increase version if versioning is enabled), false: throw http 400 if exist</param>
        /// <exception cref="">Password incorrect: Microsoft.SharePoint.Client.IdcrlException: The sign-in name or password does not match one in the Microsoft account system.</exception>
        /// <exception cref="">No permission: System.Net.WebException: The remote server returned an error: (401) Unauthorized.</exception>
        /// <exception cref="">File exist and overwrite=false: System.Net.WebException: The remote server returned an error: (400) Bad Request.</exception>
        public void UploadFileToDocumentLibrary(string folderName, string fileName, Stream content, bool overwrite)
        {
            folderName = folderName.Trim('/');
            var folderServerRelatedUrl = AppendUrl(this.docLibServerRelativeUrl, folderName);
            try
            {
                AddFileByRestApi(folderServerRelatedUrl, fileName, content, overwrite);
            }
            catch (WebException wEx)
            {
                var reason = GetExceptionInfo(wEx);
                logger.Warn("WebException Status:{0}. StatusCode: {1}. ", wEx.Status, wEx.StatusCode());
                logger.Error("An error occurred while to upload file. FolderServerRelatedUrl: {0}. FileName: {1}. Resaon: {2}. ", folderServerRelatedUrl, fileName, reason);
                if (content.CanSeek && wEx.StatusCode() == HttpStatusCode.NotFound)
                {
                    CreateFolder(folderName);
                    content.Seek(0L, SeekOrigin.Begin);
                    AddFileByRestApi(folderServerRelatedUrl, fileName, content, overwrite);
                    return;
                }
                if (reason.Contains("The length of the URL for this request exceeds the configured maxUrlLength value")) throw new Exception(reason, wEx);
                throw;
            }
        }

        /// <summary>
        /// Uploads large files by splitting them into sequential chunks. Handles missing directory creation automatically.
        /// </summary>
        public void UploadFileByChunkToDocumentLibrary(string folderName, string fileName, Stream content, bool overwrite, int chunkSize, int maxRetries = 10, int initialDelayMs = 2000)
        {
            folderName = folderName.Trim('/');
            var folderServerRelatedUrl = AppendUrl(this.docLibServerRelativeUrl, folderName);
            try
            {
                AddLargeFileByRestApi(folderServerRelatedUrl, fileName, content, overwrite, chunkSize, maxRetries, initialDelayMs);
            }
            catch (WebException wEx)
            {
                var reason = GetExceptionInfo(wEx);
                logger.Warn("Chunk upload caught Exception. Status: {0}. StatusCode: {1}.", wEx.Status, wEx.StatusCode());
                logger.Error("An error occurred during chunk upload pipeline. Folder: {0}. File: {1}. Reason: {2}.", folderServerRelatedUrl, fileName, reason);
                if (content.CanSeek && wEx.StatusCode() == HttpStatusCode.NotFound)
                {
                    logger.Info("Target folder not found. Creating folder '{0}' and resetting stream.", folderName);
                    CreateFolder(folderName);
                    content.Seek(0L, SeekOrigin.Begin);
                    AddLargeFileByRestApi(folderServerRelatedUrl, fileName, content, overwrite, chunkSize, maxRetries, initialDelayMs);
                    return;
                }
                throw;
            }
        }

        public void AddLargeFileByRestApi(string folderServerRelatedUrl, string fileName, Stream content, bool isOverwrite, int chunkSize, int maxRetries = 10, int initialDelayMs = 2000)
        {
            var chunkRequest = new AddFileByChunkRequest(this.siteUrl, this.tokenProvider)
            {
                FolderServerRelativeUrl = folderServerRelatedUrl,
                FileName = fileName,
                FileServerRelativeUrl = AppendUrl(folderServerRelatedUrl, fileName),
                OverWrite = isOverwrite,
                Content = content,
                MaxRetries = maxRetries,
                ChunkSize = chunkSize,
                InitialDelayMs = initialDelayMs,
            };
            chunkRequest.Execute();
        }

        private static string GetExceptionInfo(WebException wEx)
        {
            string errorInfo;
            try
            {
                using (StreamReader sr = new StreamReader(wEx.Response.GetResponseStream()))
                {
                    errorInfo = sr.ReadToEnd();
                }
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while to get WebException. Reason: {0}.", ex.ToString());
                errorInfo = wEx.ToString();
            }
            return errorInfo;
        }

        /// <summary>
        ///
        /// </summary>
        /// <param name="folderName"></param>
        private bool CreateFolder(string folderName)
        {
            var folderNames = folderName.Split('/');
            var url = this.docLibServerRelativeUrl;
            foreach (var subFolder in folderNames)
            {
                url = AppendUrl(url, subFolder);
                CreateIfNotExist(url);
            }
            return true;
        }

        private string AppendUrl(params string[] urls)
        {
            return string.Join("/", urls);
        }

        public void CreateIfNotExist(string folderServerRelatedUrl)
        {
            var request = new CreateFolderRequest(this.siteUrl, this.tokenProvider) { FolderServerRelativeUrl = folderServerRelatedUrl };
            request.Execute();
        }

        public void AddFileByRestApi(string folderServerRelatedUrl, string fileName, Stream content, bool isOverwrite)
        {
            var request = new AddFileRequest(this.siteUrl, this.tokenProvider)
            {
                Content = content,
                FileName = fileName,
                FolderServerRelativeUrl = folderServerRelatedUrl,
                OverWrite = isOverwrite,
            };
            request.Execute();
        }

        public string GetGroupSiteUrl(string groupSiteRequestUrl)
        {
            string outputSiteUrl = string.Empty;
            AveTaskRetryHelper helper = new AveTaskRetryHelper(30, true);
            string responseBody = null;
            WebHeaderCollection headers = null;
            helper.ExecuteWithRetryMechanism(() =>
            {
                ReliableHttpWebRequest httpGetRequest = ReliableHttpWebRequest.CreateRequest(groupSiteRequestUrl) as ReliableHttpWebRequest;
                httpGetRequest.SetTokenProvider(groupSiteRequestUrl, tokenProvider, false);
                httpGetRequest.Method = "GET";
                httpGetRequest.AllowAutoRedirect = false;
                //studo::httpGetRequest.Timeout = WrapperConfiguration.BPOS_S.HttpWebRequestTimeout;
                try
                {
                    WebResponse response = httpGetRequest.GetResponse();
                    if (response != null)
                    {
                        responseBody = new StreamReader(response.GetResponseStream(), Encoding.UTF8).ReadToEnd();
                        headers = response.Headers;
                        response.Close();
                    }
                    if (!headers.AllKeys.Contains("Location") || string.IsNullOrEmpty(headers.Get("Location")) || !headers.Get("Location").ToLower().StartsWith("http"))
                    {
                        throw new Exception("Cannot find the Location header while retrieving the office365 group site url.");
                    }
                }
                catch (WebException ex)
                {
                    if (ex.Response != null)
                    {
                        ex.Response.Close();
                    }
                    throw;
                }
                finally
                {
                    Thread.Sleep(10000);
                }
            }
            );
            outputSiteUrl = headers.Get("Location");
            logger.Info(string.Format("Successfully retrieve team site for office365 group. Url:{0}", outputSiteUrl));
            return outputSiteUrl;
        }
    }
}