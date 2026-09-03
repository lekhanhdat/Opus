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
namespace AvePoint.ObjectModel.ClientOM
{
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using System;
    using System.IO;
    using AvePoint.Office365.Api;
    using System.Net;
    using GCommon;
    using AvePoint.ObjectModel.O365;

    class FileRestProcessor
    {
        private static IAveLogger mLogger = AveLogger.GetInstance(typeof(FileRestProcessor));

        /// <summary>
        /// will add by parent folder unique id first, if failed,add it with parent folder url then
        /// </summary>
        /// <param name="parentWeburl"></param>
        /// <param name="parentFolderUniqueId"></param>
        /// <param name="fileServerRelativeUrl"></param>
        /// <param name="content"></param>
        /// <param name="overwrite"></param>
        public static void AddFileByRestApi(ClientContext context, ITokenProvider provider, string parentWebFullUrl, Guid parentFolderUniqueId, string fileServerRelativeUrl, Stream content, bool overwrite)
        {
            bool fileAdded = false;
            try
            {
                if (parentFolderUniqueId != Guid.Empty)
                {
                    AddFileByRestApiWithParentFolderId(context, provider, parentWebFullUrl, parentFolderUniqueId, fileServerRelativeUrl, content, overwrite);
                    fileAdded = true;
                }
            }
            catch (Exception e)
            {
                mLogger.Error("Add file by folder unique id failed.File:{0}.Will Add With path.Error:{1}", fileServerRelativeUrl, e);
            }
            if (!fileAdded)
            {
                AddFileByRestApiWithParentFolderUrl(context, provider, parentWebFullUrl, fileServerRelativeUrl, content, overwrite);
            }
        }

        private static void AddFileByRestApiWithParentFolderUrl(ClientContext context, ITokenProvider provider, string parentWebFullUrl, string fileServerRelativeUrl, Stream body, bool isOverwrite)
        {
            int index = fileServerRelativeUrl.LastIndexOf('/');
            string indexstring = fileServerRelativeUrl.Substring(0, index);
            if (indexstring.Contains("'"))
            {
                indexstring = indexstring.Replace("'", "''");
            }
            string fileUrl = fileServerRelativeUrl.Substring(index + 1);
            if (fileUrl.Contains("'"))
            {
                fileUrl = fileUrl.Replace("'", "''");
            }
            string methodCmd = string.Format("getfolderbyserverrelativepath(decodedUrl='{0}')/files/addUsingPath(decodedUrl='{1}', overwrite={2})", Uri.EscapeDataString(indexstring), Uri.EscapeDataString(fileUrl), isOverwrite.ToString().ToLowerInvariant());
            string request = string.Format("{0}/_api/Web/{1}", parentWebFullUrl, methodCmd);
            mLogger.Info("Add Large OneNote file request: {0}", request);
            ExecuteAddFileRequest(context, provider, parentWebFullUrl, body, request);
        }

        private static void AddFileByRestApiWithParentFolderId(ClientContext context, ITokenProvider provider, string parentWebFullUrl, Guid parentFolderId, string fileName, Stream body, bool isOverwrite)
        {
            string realName = fileName;
            if (fileName.IndexOf('/') >= 0)
            {
                realName = fileName.Substring(fileName.LastIndexOf('/') + 1);
            }
            realName = realName.Replace("'", "''");
            string methodCmd = string.Format("getfolderbyid(guid'{0}')/files/addUsingPath(decodedUrl='{1}', overwrite={2})", parentFolderId, realName, isOverwrite.ToString().ToLowerInvariant());
            string request = string.Format("{0}/_api/Web/{1}", parentWebFullUrl, methodCmd);
            mLogger.Info("Add Large file request: {0}", request);
            ExecuteAddFileRequest(context, provider, parentWebFullUrl, body, request);
        }

        private static void ExecuteAddFileRequest(ClientContext context, ITokenProvider provider, string parentWebFullUrl, Stream body, string request)
        {
            ReconnectableHttpWebRequest webRequest = ReconnectableHttpWebRequest.CreateRequest(request);
            webRequest.RefreshDigestInfo(context as ClientContext);
            webRequest.Headers["X-FORMS_BASED_AUTH_ACCEPTED"] = "T";
            webRequest.SetTokenProvider(parentWebFullUrl, provider);
            webRequest.ContentLength = body.Length;
            webRequest.Method = "POST";
            webRequest.Timeout = 600000;
            webRequest.ReadWriteTimeout = 1800000;
            webRequest.AllowWriteStreamBuffering = false;
            Stream inputBody = webRequest.GetRequestStream();
            byte[] buffer = new byte[1024 * 64];
            int len = 0;
            while ((len = body.Read(buffer, 0, buffer.Length)) != 0)
            {
                inputBody.Write(buffer, 0, len);
            }
            AddContentStream(webRequest);
        }

        private static void AddContentStream(ReconnectableHttpWebRequest webRequest)
        {

            using (HttpWebResponse result = webRequest.GetResponse() as HttpWebResponse)
            {
                if (result != null)
                {
                    if (result.StatusCode != HttpStatusCode.OK)
                    {
                        mLogger.Error("Failed to Restore large OneNote file by Rest API.cause: {0}", result.StatusCode.ToString());
                        throw new WebException(string.Format("unable to save the one note file. {0}", result.StatusCode));
                    }
                    mLogger.Info("Finished upload a large file by Rest Api");
                }
                else
                {
                    mLogger.Error("Failed to get Response when Restore large OneNote file.");
                    throw new WebException("unable to save the one note file. ");
                }
            }
        }
    }
}
