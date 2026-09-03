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
using System.Text;
using AvePoint.Media.Storage.Cloud.Common;
using System.Collections;
using System.Text.RegularExpressions;
using System.Globalization;

namespace AvePoint.Media.Storage.Cloud.Atmos
{
    class AtmosSystemWrapper : AbstractCloudSystemWrapper
    {
        private AtmosSystem atmosSystem;
        private AbstractRESTOprationExecutor client;
        private ArrayList dirsTemp;
        private ArrayList filesTemp;
        private List<XDirectoryInfo> dirsList;
        private List<XFileInfo> filesList;

        public AtmosSystemWrapper(AtmosSystem atmosSystem, AbstractRESTOprationExecutor client): base(atmosSystem , client)
        {
            this.atmosSystem = atmosSystem;
            this.client = client;

        }

        public override int GetDirsResultsCount()
        {
            return sd.dirsCount;
        }

        public override int GetFilesResultsCount()
        {
            return sd.filesCount;
        }

        public override ArrayList GetNextDirsResults(ResponseInfo responseInfo, Dictionary<string, string> queryParams,
           string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo)
        {

            ResponseInfo res = GetNextResponseInfo(responseInfo, queryParams, urlWithoutQueryParms, headers);
            atmosSystem.ConvertXmlToList(dirsList, filesList, res.ResponseXml, storageInfo);
            filesList.Clear();
            ListResultsToArrayList(dirsList, null, dirsTemp, filesTemp);
            atmosSystem.dirs.SetState(res, queryParams,
            urlWithoutQueryParms, headers, dirInfo, storageInfo);
            return dirsTemp;

        }


        public override ArrayList GetNextFilesResults(ResponseInfo responseInfo, Dictionary<string, string> queryParams,
           string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            ResponseInfo res = GetNextResponseInfo(responseInfo, queryParams, urlWithoutQueryParms, headers);
            atmosSystem.ConvertXmlToList(dirsList, filesList, res.ResponseXml, storageInfo);
            dirsList.Clear();
            ListResultsToArrayList(null, filesList, dirsTemp, filesTemp);
            atmosSystem.files.SetState(res, queryParams,
                      urlWithoutQueryParms, headers, dirInfo, storageInfo);
            return filesTemp;

        }

        public ResponseInfo GetNextResponseInfo(ResponseInfo responseInfo, Dictionary<string, string> queryParams, string urlWithoutQueryParms, Dictionary<string, string> headers)
        {
            dirsTemp = new ArrayList();
            filesTemp = new ArrayList();
            dirsList = new List<XDirectoryInfo>();
            filesList = new List<XFileInfo>();

            string token = string.Empty;

            token = responseInfo.Headers["X-EMC-TOKEN".ToLower(CultureInfo.InvariantCulture)];
            headers["X-EMC-TOKEN".ToLower(CultureInfo.InvariantCulture)] = token;
            headers["X-EMC-SYSTEM-TAGS".ToLower(CultureInfo.InvariantCulture)] = "ATIME,SIZE".ToLower(CultureInfo.InvariantCulture);
            responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);

            return responseInfo;
        }


        public override void GetListSubDirectoriesAndFilesCount(StorageInfo storageInfo)
        {

            Boolean frist = true;
            string token = string.Empty;
            atmosSystem.CheckState(storageInfo.HighName);
            string urlWithoutQueryParms = client.BuildObjectAbsoluteURL(storageInfo.HighName, storageInfo.LowName);
            Dictionary<string, string> queryParams = client.ListDirectoryQueryParams;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            List<XFileInfo> files = new List<XFileInfo>();

            while (frist || token != string.Empty)
            {

                Dictionary<string, string> headers = client.ListDirectoryHeaders;
                headers["X-EMC-SYSTEM-TAGS".ToLower(CultureInfo.InvariantCulture)] = "ATIME,SIZE".ToLower(CultureInfo.InvariantCulture);
                if (!frist)
                {
                    headers["X-EMC-TOKEN".ToLower(CultureInfo.InvariantCulture)] = token;
                    token = string.Empty;
                }
                frist = false;
                ResponseInfo responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
                string responseXmlString = responseInfo.ResponseXml;
                if (responseInfo.Headers.Count > 0)
                {
                    token = responseInfo.Headers["X-EMC-TOKEN".ToLower(CultureInfo.InvariantCulture)];
                }

                atmosSystem.ConvertXmlToList(dirs, files, responseXmlString, storageInfo);
                sdtemp = GetSubDirsAndFilesCount.GetAtmosSubDirsAndFilesCount(responseInfo.ResponseXml, storageInfo, client);
                sd.dirsCount = sd.dirsCount + sdtemp.dirsCount;
                sd.filesCount = sd.filesCount + sdtemp.filesCount;
            }
        }
    }
}
