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

namespace AvePoint.Media.Storage.Cloud.Common
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.IO;
    using AvePoint.Media.Storage.Cloud.Common;
    using System.Net;
   
    #endregion

    interface ICloudOprationExecutor
    {
        void InitConfig(CloudOpenParameter prams);

        List<string> ListContainers();

        bool CheckContainer(string xSetName);

        bool CreateContainer(string xSetName);

        bool DeleteContainer(string xSetName);

        List<string> ListObject(string xSetName);

        List<string> ListObject(string xSetName, string prefix);

        bool CheckObject(string xSetName, string xStreamName);

        HttpWebRequest GetUploadRequest(string xSetName, string xStreamName, string mimeType, HttpWebRequest webRequest, int blockNumber, long dataLength);

        bool CreateObject(string xSetName, string xStreamName, HttpWebRequest request, long dataLength);

        /// <summary>
        /// 注意：调用者负责关闭Stream，使用完一定要关闭。
        /// </summary>
        /// <param name="xSetName"></param>
        /// <param name="xStreamName"></param>
        /// <param name="rangFrom"></param>
        /// <param name="rangeTo"></param>
        /// <returns></returns>
        Stream OpenObject(string xSetName, string xStreamName, int rangFrom, int rangeTo);

        Stream OpenObject(string container, string objectName, int[] lengths, FileMode mode);

        bool DeleteObject(string xSetName, string xStreamName, bool isDeleteSubFile);
        
        CloudFileInfo GetObjectInfo(string xSetName, string xStreamName);

        bool Login(string xSetName);

        long GetContainerSize(string xSetName);

        StorageOpenValidResult GetPermissions();

        string GetDocAveDefaultContainer();

        //new interface
        /// <summary>
        /// basic interface for list objects.
        /// </summary>
        /// <param name="baseURL"></param>
        /// <param name="queryParams"></param>
        /// <returns>default : xml format request</returns>
        ResponseInfo ListObjects(string baseURL, Dictionary<string, string> queryParams, Dictionary<string, string> headers);

        void CreateObjectWithNoContent(string fullURL, Dictionary<string, string> headers);

        HttpUploadStream OpenObjectForWrite(string fullURL, Dictionary<string, string> headers);

        HttpDownloadStream OpenObjectForRead(string fullURL, Dictionary<string, string> headers);

         bool DeleteObject(string fullURL, Dictionary<string, string> parameters, Dictionary<string, string> headers);
 
         bool CheckObject(string fullURL, Dictionary<string, string> parameters, Dictionary<string, string> headers);
 
         Dictionary<string, string> GetObjectInfo(string url, Dictionary<string, string> requestParams, Dictionary<string, string> requestHeaders);
    }
}
