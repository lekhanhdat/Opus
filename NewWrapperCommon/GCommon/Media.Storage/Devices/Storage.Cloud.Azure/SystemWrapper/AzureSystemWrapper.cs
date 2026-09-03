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
using System.Collections;
using System.Text.RegularExpressions;
using AvePoint.Media.Storage.Cloud.Azure;

namespace AvePoint.Media.Storage.Cloud.Common
{
    class AzureSystemWrapper : AbstractCloudSystemWrapper
    {
        private AzureSystem azureSystem;
        private ArrayList dirsTemp;
        private ArrayList filesTemp;
        private List<XDirectoryInfo> dirsList;
        private List<XFileInfo> filesList;

        public AzureSystemWrapper(AzureSystem azureSystem, AbstractRESTOprationExecutor client):base(azureSystem , client)
        {
            this.azureSystem = azureSystem;

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
            ResponseInfo response = GetNextResponseInfo(responseInfo, queryParams, urlWithoutQueryParms, headers);
            azureSystem.ConvertXmlToList(dirsList, filesList, response.ResponseXml, dirInfo, storageInfo);
            filesList.Clear();
            ListResultsToArrayList(dirsList, null, dirsTemp, filesTemp);
            azureSystem.dirs.SetState(response, queryParams,
            urlWithoutQueryParms, headers, dirInfo, storageInfo);
            return dirsTemp;
        }


        public override ArrayList GetNextFilesResults(ResponseInfo responseInfo, Dictionary<string, string> queryParams,
           string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            ResponseInfo response = GetNextResponseInfo(responseInfo, queryParams, urlWithoutQueryParms, headers);
            azureSystem.ConvertXmlToList(dirsList, filesList, response.ResponseXml, dirInfo, storageInfo);
            dirsList.Clear();
           ListResultsToArrayList(null, filesList, dirsTemp, filesTemp);
            azureSystem.files.SetState(response, queryParams,
                      urlWithoutQueryParms, headers, dirInfo, storageInfo);
            return filesTemp;
        }

        public ResponseInfo GetNextResponseInfo(ResponseInfo responseInfo, Dictionary<string, string> queryParams, string urlWithoutQueryParms, Dictionary<string, string> headers)
        {
            dirsTemp = new ArrayList();
            filesTemp = new ArrayList();
            dirsList = new List<XDirectoryInfo>();
            filesList = new List<XFileInfo>();

            Regex defaultRegex = new Regex("<NextMarker>(.+)</NextMarker>");
            MatchCollection matches = defaultRegex.Matches(responseInfo.ResponseXml);
            string markerValue = "";
            if (matches.Count == 1 && !markerValue.Equals(matches[0].Groups[1].Value))
            {
                markerValue = matches[0].Groups[1].Value;
                if (queryParams.ContainsKey("marker"))
                {
                    queryParams["marker"] = markerValue;
                }
                else
                {
                    queryParams.Add("marker", markerValue);
                }
            }
            responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);

            return responseInfo;
        }

        public override void GetListSubDirectoriesAndFilesCount(StorageInfo storageInfo)
        {
            azureSystem.CheckState(storageInfo.HighName);
            string urlWithoutQueryParms = client.BuildURLWithOutQueryParams(storageInfo.HighName);
            Dictionary<string, string> queryParams = client.ListDirectoryQueryParams;
            if (!string.IsNullOrEmpty(storageInfo.LowName) && !"/".Equals(storageInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams["prefix"] = storageInfo.LowName;
            }
            else
            {
                queryParams["delimiter"] = "/";
            }
            //queryParams.Add("format", "xml");
            Dictionary<string, string> headers = client.ListDirectoryHeaders;
            ResponseInfo responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
            string responseXmlString = responseInfo.ResponseXml;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            List<XFileInfo> files = new List<XFileInfo>();

            sdtemp = GetSubDirsAndFilesCount.GetAzureSubDirsAndFilesCount(responseXmlString, storageInfo, client);
            sd.dirsCount = sdtemp.dirsCount;
            sd.filesCount = sdtemp.filesCount;

            while (true)
            {
                Regex defaultRegex = new Regex("<NextMarker>(.+)</NextMarker>");
                MatchCollection matches = defaultRegex.Matches(responseInfo.ResponseXml);
                string markerValue = "";
                if (matches.Count == 1 && !markerValue.Equals(matches[0].Groups[1].Value))
                {
                    markerValue = matches[0].Groups[1].Value;
                    if (queryParams.ContainsKey("marker"))
                    {
                        queryParams["marker"] = markerValue;
                    }
                    else
                    {
                        queryParams.Add("marker", markerValue);
                    }

                    responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
                    sdtemp = GetSubDirsAndFilesCount.GetAzureSubDirsAndFilesCount(responseInfo.ResponseXml, storageInfo, client);
                    sd.dirsCount = sd.dirsCount + sdtemp.dirsCount;
                    sd.filesCount = sd.filesCount + sdtemp.filesCount;
                }
                else
                {
                    break;
                }
            }

        }


    }
}
