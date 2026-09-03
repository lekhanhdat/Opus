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
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using AvePoint.Office365.Api;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using System.IO;
using System.Net.Http.Headers;

namespace AvePoint.ObjectModel.AveGraphRequest
{
    /// <summary>
    /// Graph Drive 对应SharePoint DocumentLibrary
    /// </summary>
    public class GraphDrive : GraphBase
    {
        private string webId;
        private string driveId;
        public GraphDrive(ITokenProvider tokenProvider, IWebProxy proxy, string webId, string driveId)
            : base(tokenProvider, proxy)
        {
            this.webId = webId;
            this.driveId = driveId;
        }
        public JObject ListChildrenInRoot()
        {
            string requestUri = string.Format("{0}/sites/{1},{2},{3}/drives/{4}/root/children", GraphApiUrl.V1, webId, driveId);
            return GetObjectInfo(requestUri);
        }

        public void CopyTo(string fileId, string folderId, string fileName)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}/copy", GraphApiUrl.V1, webId, driveId, fileId);
            var content = JsonConvert.SerializeObject(new { parentReference = new { driveId = driveId, id = folderId }, name = fileName });
            var parameter = GenerateStringContentRequestParameters(requestUri, content, "application/json");

            request.PostRequest(parameter);
        }

        public JObject MoveTo(string fileId, string folderId, string fileName)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}", GraphApiUrl.V1, webId, driveId, fileId);
            var content = JsonConvert.SerializeObject(new { parentReference = new { id = folderId }, name = fileName });
            var parameter = GenerateStringContentRequestParameters(requestUri, content, "application/json");

            return request.PatchJsonAsync<JObject>(parameter).Result;
        }

        public void DeleteFile(string fileId)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}", GraphApiUrl.V1, webId, driveId, fileId);
            DeleteObject(requestUri);
        }

        public JObject ListDriveItemChildrenByItemId(string folderId)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}/children", GraphApiUrl.V1, webId, driveId, folderId);
            return GetObjectInfo(requestUri);
        }

        public JObject GetDriveItemByPath(string itemPath)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/root:/{3}", GraphApiUrl.V1, webId, driveId, itemPath);
            return GetObjectInfo(requestUri);
        }

        public JObject GetDriveItemById(string itemId)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}", GraphApiUrl.V1, webId, driveId, itemId);
            return GetObjectInfo(requestUri);
        }

        /// <summary>
        /// 还有一个分段下载的 以后有需要的时候添加
        /// </summary>
        /// <param name="itemPath"></param>
        /// <returns></returns>
        public byte[] DownloadDriveItemContentByPath(string itemPath)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/root:/{3}:/content", GraphApiUrl.V1, webId, driveId, itemPath);
            var parameter = GenerateRequestsParameters(requestUri);
            return request.GetByteArrayAsync(parameter).Result;
        }


        public byte[] DownloadDriveItemContentById(string itemId)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}/content", GraphApiUrl.V1, webId, driveId, itemId);
            var parameter = GenerateRequestsParameters(requestUri);
            return request.GetByteArrayAsync(parameter).Result;
        }

        public JObject UploadFile(string parentFolderId, string fileName, byte[] content)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}:/{4}:/content", GraphApiUrl.V1, webId, driveId, parentFolderId, fileName);
            var parameter = GenerateByteArrayContentRequestParameters(requestUri, content, "text/plain");
            return request.PutAsync<JObject>(parameter).Result;
        }

        public JObject ReplaceExistFile(string fileId, byte[] content)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}/content", GraphApiUrl.V1, webId, driveId, fileId);
            var parameter = GenerateByteArrayContentRequestParameters(requestUri, content, "text/plain");
            return request.PutAsync<JObject>(parameter).Result;
        }

        private string GetUploadSessionUrlForNewFile(string fileName)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/root:/{3}:/createUploadSession", GraphApiUrl.V1, webId, driveId, fileName);
            return GetUploadSessionUrl(requestUri);
        }

        private string GetUploadSessionUrl(string requestUri)
        {
            var parameter = GenerateRequestsParameters(requestUri);
            dynamic result = request.PostAsync<JObject>(parameter).Result;
            return result.uploadUrl.ToString();
        }

        private string GetUploadSessionUrlForExistFile(string itemId)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}/createUploadSession", GraphApiUrl.V1, webId, driveId, itemId);
            return GetUploadSessionUrl(requestUri);
        }


        public void AddLargeFile(string fileName, Stream fileStream)
        {
            var uploadUrl = GetUploadSessionUrlForNewFile(fileName);

            AddLargeFile(uploadUrl, fileStream);
        }

        private void AddLargeFileBySessionUrl(string requestUri, Stream fileStream)
        {
            var parameter = GenerateRequestsParameters(requestUri);
            var length = fileStream.Length;
            using (BinaryReader br = new BinaryReader(fileStream))
            {
                byte[] buffer = new byte[2 * 1024 * 1024];//该值没有经过测试 
                int bytesRead = 0;
                long totalBytesRead = 0;
                while ((bytesRead = br.Read(buffer, 0, buffer.Length)) > 0)
                {
                    totalBytesRead = totalBytesRead + bytesRead;
                    if (totalBytesRead == length)
                    {
                        // Copy to a new buffer that has the correct size
                        var lastBuffer = new byte[bytesRead];
                        Array.Copy(buffer, 0, lastBuffer, 0, bytesRead);
                        buffer = lastBuffer;
                    }

                    parameter.Content = new ByteArrayContentRequest(buffer, null, buffer.Length, new ContentRangeHeaderValue(totalBytesRead - bytesRead, totalBytesRead - 1, length));
                    var uploadResult = request.PutAsync<JObject>(parameter).Result;
                }
            }
        }

        public void ReplaceLargeFile(string itemId, Stream fileStream)
        {
            var uploadUrl = GetUploadSessionUrlForExistFile(itemId);
            AddLargeFile(uploadUrl, fileStream);
        }

        public JObject Search(string searchQuery)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/root/search(q='{3}')", GraphApiUrl.V1, webId, driveId, searchQuery);
            return GetObjectInfo(requestUri);
        }

        public JObject GetVersions(string itemId)
        {
            string requestUri = string.Format("{0}/sites/{1}/drives/{2}/items/{3}/versions", GraphApiUrl.V1, webId, driveId, itemId);
            return GetObjectInfo(requestUri);
        }
    }
}
