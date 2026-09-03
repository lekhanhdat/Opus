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
using System.Xml.XPath;
using System.Globalization;

namespace AvePoint.Media.Storage.Cloud.Rackspace
{
    class RackspaceSystemWrapper : AbstractCloudSystemWrapper
    {
        private RackspaceSystem rackspaceSystem;
        private ArrayList dirsTemp;
        private ArrayList filesTemp;
        private List<XDirectoryInfo> dirsList;
        private List<XFileInfo> filesList;

        public RackspaceSystemWrapper(RackspaceSystem rackspaceSystem, AbstractRESTOprationExecutor client):base(rackspaceSystem , client)
        {
            this.rackspaceSystem = rackspaceSystem;
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
            rackspaceSystem.ConvertXmlToList(dirsList, filesList, response.ResponseXml, dirInfo, storageInfo);
            filesList.Clear();
            ListResultsToArrayList(dirsList, null, dirsTemp, filesTemp);
            rackspaceSystem.dirs.SetState(response, queryParams,
            urlWithoutQueryParms, headers, dirInfo, storageInfo);
            return dirsTemp;

        }


        public override ArrayList GetNextFilesResults(ResponseInfo responseInfo, Dictionary<string, string> queryParams,
           string urlWithoutQueryParms, Dictionary<string, string> headers, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            ResponseInfo response = GetNextResponseInfo(responseInfo, queryParams, urlWithoutQueryParms, headers);
            rackspaceSystem.ConvertXmlToList(dirsList, filesList, response.ResponseXml, dirInfo, storageInfo);
            dirsList.Clear();
            ListResultsToArrayList(null, filesList, dirsTemp, filesTemp);
            rackspaceSystem.files.SetState(response, queryParams,
                      urlWithoutQueryParms, headers, dirInfo, storageInfo);
            return filesTemp;

        }

        public ResponseInfo GetNextResponseInfo(ResponseInfo responseInfo, Dictionary<string, string> queryParams, string urlWithoutQueryParms, Dictionary<string, string> headers)
        {

            dirsTemp = new ArrayList();
            filesTemp = new ArrayList();
            dirsList = new List<XDirectoryInfo>();
            filesList = new List<XFileInfo>();
            List<XPathNavigator> fileNavs;
            List<XPathNavigator> dirNavs;


            string responseXmlString = responseInfo.ResponseXml;
            List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "container/object");
            fileNavs = navs;
            navs = client.FirstStepAnalyzeXML(responseXmlString, "container/" + "SUBDIR".ToLower(CultureInfo.InvariantCulture));
            dirNavs = navs;
            if (responseXmlString.EndsWith("</" + "SUBDIR".ToLower(CultureInfo.InvariantCulture) + "></container>", StringComparison.OrdinalIgnoreCase))
            {
                string tempName = dirNavs[dirNavs.Count - 1].SelectSingleNode("name").Value;
                queryParams["marker"] = tempName;
            }
            else
            {
                string tempName = fileNavs[fileNavs.Count - 1].SelectSingleNode("name").Value;
                queryParams["marker"] = tempName;
            }
            responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
            return responseInfo;
        }

        public override void GetListSubDirectoriesAndFilesCount(StorageInfo storageInfo)
        {
            bool flag = false;
            string responseXmlString = String.Empty;
            List<XPathNavigator> fileNavs = new List<XPathNavigator>();
            List<XPathNavigator> dirNavs = new List<XPathNavigator>();

            rackspaceSystem.CheckState(storageInfo.HighName);
            string urlWithoutQueryParms = client.BuildURLWithOutQueryParams(storageInfo.HighName);
            Dictionary<string, string> queryParams = client.ListDirectoryQueryParams;
            if (!string.IsNullOrEmpty(storageInfo.LowName) && !"/".Equals(storageInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams.Add("prefix", storageInfo.LowName);
            }
            queryParams.Add("format", "xml");
            //queryParams.Add("limit", "6");
            do
            {

                flag = true;

                Dictionary<string, string> headers = client.ListDirectoryHeaders;
                ResponseInfo responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
                responseXmlString = responseInfo.ResponseXml;

                sdtemp = GetSubDirsAndFilesCount.GetRackspaceSubDirsAndFilesCount(responseInfo.ResponseXml, storageInfo, client);
                sd.dirsCount = sd.dirsCount + sdtemp.dirsCount;
                sd.filesCount = sd.filesCount + sdtemp.filesCount;

                if (flag)
                {
                    List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "container/object");
                    fileNavs = navs;

                    navs = client.FirstStepAnalyzeXML(responseXmlString, "container/" + "SUBDIR".ToLower(CultureInfo.InvariantCulture));
                    dirNavs = navs;

                    if (responseXmlString.EndsWith("</" + "SUBDIR".ToLower(CultureInfo.InvariantCulture) + "></container>", StringComparison.OrdinalIgnoreCase))
                    {
                        string tempName = dirNavs[dirNavs.Count - 1].SelectSingleNode("name").Value;
                        queryParams["marker"] = tempName;
                    }
                    else
                    {
                        string tempName = fileNavs[fileNavs.Count - 1].SelectSingleNode("name").Value;
                        queryParams["marker"] = tempName;
                    }
                }



            } while ((sdtemp.dirsCount + sdtemp.filesCount) == 10);


        }

    }
}
