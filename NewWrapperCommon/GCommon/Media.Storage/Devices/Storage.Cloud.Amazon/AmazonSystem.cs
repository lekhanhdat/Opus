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

namespace AvePoint.Media.Storage.Cloud.Amazon
{
    #region using directives
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    #endregion

    class AmazonSystem : CloudSystem
    {
        private AmazonOpenParameter openParam;
        private StorageLogger logger = new StorageLogger(typeof(AmazonSystem));
        public ArrayListSubDirsWrapper DirsWrapper { set; get; }
        public ArrayListFilesWrapper FilesWrapper { set; get; }

        protected override void SetContainerKeyName()
        {
            this.SystemLocationKeyName = "bucketName".ToLower(CultureInfo.InvariantCulture);
        }

        public override String Type
        {
            get
            {
                return "AmazonSystem";
            }
        }

        public AmazonSystem(String xriStr, String initMode, AbstractXSystem parentSystem)
            : base(xriStr, parentSystem)
        {
            Client = new AmazonClient();
            logger = StorageLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            if (initMode.Equals(XSystemConst.MODE_NOW_INITSYSTEM))
            {
                ConstructCloudOpenParameter(xriStr);
                Client.InitConfig(openParam);
                Client.HttpClient.CurrentSystem = this;
                base.SystemHealth = XSystemHealth.Unknown;
            }
            if (string.IsNullOrEmpty(SystemLocation))
            {
                SystemLocation = Client.GetDocAveDefaultContainer();
            }
            this.Open();
        }

        public void ConvertXmlToList(List<XDirectoryInfo> dirs, List<XFileInfo> files, String responseXmlString, StorageInfo dirInfo, StorageInfo storageInfo)
        {
            CloudDirectoryInfo dir;
            CloudFileInfo file;
            responseXmlString = responseXmlString.Replace(" xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"", "");
            List<XPathNavigator> navs = Client.FirstStepAnalyzeXML(responseXmlString, "ListBucketResult/Contents");
            XPathNavigator singleNav;
            String name;
            Int64 size;
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                size = 0;
                singleNav = nav.SelectSingleNode("Key");
                if (singleNav != null)
                {
                    name = singleNav.Value;
                    singleNav = nav.SelectSingleNode("Size");
                    if (singleNav != null)
                    {
                        size = singleNav.ValueAsLong;
                    }
                    if (name.EndsWith("/", StringComparison.OrdinalIgnoreCase))
                    {
                        name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
                        if (string.IsNullOrEmpty(name))
                        {
                            continue;
                        }
                        dir = new CloudDirectoryInfo(storageInfo.HighPlusLowName, name);
                        dirs.Add(dir);
                    }
                    else
                    {
                        file = new CloudFileInfo(storageInfo.HighPlusLowName, name.RemoveFirst(dirInfo.LowName), size);
                        files.Add(file);
                    }
                }
            }
            navs = Client.FirstStepAnalyzeXML(responseXmlString, "ListBucketResult/CommonPrefixes");
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                size = 0;
                singleNav = nav.SelectSingleNode("Prefix");
                if (singleNav != null)
                {
                    name = singleNav.Value;
                    name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
                    if (string.IsNullOrEmpty(name))
                    {
                        continue;
                    }
                    dir = new CloudDirectoryInfo(storageInfo.HighPlusLowName, name);
                    dirs.Add(dir);
                }
            }
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            AbstractCloudSystemWrapper abstactCloudSystemWrapper = new AmazonSystemWrapper(this, Client);
            StorageInfo storageInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            abstactCloudSystemWrapper.GetListSubDirectoriesAndFilesCount(storageInfo);
            DirsWrapper = new ArrayListSubDirsWrapper(abstactCloudSystemWrapper);
            FilesWrapper = new ArrayListFilesWrapper(abstactCloudSystemWrapper);
            CheckState(storageInfo.HighName);
            String urlWithoutQueryParms = Client.BuildURLWithOutQueryParams(storageInfo.HighName);
            Dictionary<String, String> queryParams = Client.ListDirectoryQueryParams;
            if (!String.IsNullOrEmpty(storageInfo.LowName) && !"/".Equals(storageInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams["prefix"] = storageInfo.LowName;
            }
            else
            {
                queryParams["delimiter"] = "/";
            }
            Dictionary<String, String> headers = Client.ListDirectoryHeaders;
            ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
            String responseXmlString = responseInfo.ResponseXml;
            List<XDirectoryInfo> dirsList = new List<XDirectoryInfo>();
            List<XFileInfo> filesList = new List<XFileInfo>();
            ConvertXmlToList(dirsList, filesList, responseXmlString, storageInfo, dirInfo);
            abstactCloudSystemWrapper.ListResultsToArrayList(dirsList, filesList, (ArrayList)DirsWrapper, (ArrayList)FilesWrapper);
            DirsWrapper.SetState(responseInfo, queryParams, urlWithoutQueryParms, headers, storageInfo, dirInfo);
            FilesWrapper.SetState(responseInfo, queryParams, urlWithoutQueryParms, headers, storageInfo, dirInfo);
            StorageListResultSafety safeResults = new StorageListResultSafety();
            safeResults.SubDirs = DirsWrapper;
            safeResults.Files = FilesWrapper;
            return safeResults;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            StorageInfo storageInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(storageInfo.HighName);
            String urlWithoutQueryParms = Client.BuildURLWithOutQueryParams(storageInfo.HighName);
            Dictionary<String, String> queryParams = Client.ListDirectoryQueryParams;
            if (!String.IsNullOrEmpty(storageInfo.LowName) && !"/".Equals(storageInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams.Add("prefix", storageInfo.LowName);
            }
            queryParams.Add("delimiter", "/");
            queryParams.Add("format", "xml");
            Dictionary<String, String> headers = Client.ListDirectoryHeaders;
            ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
            String responseXmlString = responseInfo.ResponseXml;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            List<XFileInfo> files = new List<XFileInfo>();
            ConvertXmlToList(dirs, files, responseXmlString, storageInfo, dirInfo);
            while (true)
            {
                Regex defaultRegex = new Regex("<NextMarker>(.+)</NextMarker>");
                MatchCollection matches = defaultRegex.Matches(responseInfo.ResponseXml);
                String markerValue = "";
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
                    responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
                    ConvertXmlToList(dirs, files, responseInfo.ResponseXml, storageInfo, dirInfo);
                }
                else
                {
                    break;
                }
            }
            StorageListResult results = new StorageListResult();
            results.SubDirs = dirs;
            results.Files = files;
            return results;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, Boolean isOverWrite)
        {
            CheckState(SystemLocation);
            StorageCopyResult storageCopyResult = new StorageCopyResult();
            try
            {
                if ((Boolean)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }))
                {
                    if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                    {
                        if ((Boolean)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }) && !isOverWrite)
                        {
                            storageCopyResult.IsCopyed = true;
                            return storageCopyResult;
                        }
                    }
                    StorageInfo sourceStorageInfo = PreproccessStorageInfo(sourceFileInfo);
                    StorageInfo targetStorageInfo = PreproccessStorageInfo(targetFileInfo);
                    Dictionary<String, String> queryParams = Client.CopyFileQueryParams;
                    Dictionary<String, String> queryHeaders = Client.CopyFileHeaders;
                    String destPath = Client.BuildObjectAbsoluteURL(targetStorageInfo.HighName, targetStorageInfo.LowName);
                    String sourcePath = "/" + PathUtil.CombinePath(SystemLocation, sourceStorageInfo.LowName);
                    sourcePath = sourcePath.Replace("\\", "/");
                    queryHeaders.Add(AmazonConstants.AWS3_REST_HEADER_PREFIX + "copy-source", Client.HttpClient.Encode(sourcePath));
                    storageCopyResult.IsCopyed = (Boolean)Client.Invoke("CopyFile", new object[] { destPath, queryParams, queryHeaders });
                }
                else
                {
                    storageCopyResult.Message = "source file is not exist";
                    storageCopyResult.IsCopyed = false;
                }
            }
            catch (Exception e)
            {
                storageCopyResult.Message = e.ToString();
                storageCopyResult.IsCopyed = false;
                logger.Error("copy file failed.Details:{0}", e.ToString());
            }
            return storageCopyResult;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourcFile, IXSystem destSystem, StorageInfo destFile, Boolean isOverWrite)
        {
            if (destSystem is AmazonSystem
               && XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY].Equals(destSystem.XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY], StringComparison.OrdinalIgnoreCase)
               && XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY].Equals(destSystem.XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY], StringComparison.OrdinalIgnoreCase))
            {
                CheckState(SystemLocation);
                CloudSystem destCloudSystem = destSystem as AmazonSystem;
                destCloudSystem.CheckState(destCloudSystem.SystemLocation);
                StorageCopyResult result = new StorageCopyResult();
                try
                {
                    if ((Boolean)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourcFile.HighName, sourcFile.LowName).TrimStart(new char[] { '\\', '/' }) }))
                    {
                        if (sourcFile.LowName.Equals(destFile.LowName, StringComparison.OrdinalIgnoreCase))
                        {
                            if ((Boolean)destCloudSystem.Client.Invoke("CheckObject", new object[] { destCloudSystem.SystemLocation, PathUtil.CombinePath(destFile.HighName, destFile.LowName).TrimStart(new char[] { '\\', '/' }) }) && !isOverWrite)
                            {
                                result.IsCopyed = true;
                                return result;
                            }
                        }
                        StorageInfo sourceStorageInfo = PreproccessStorageInfo(sourcFile);
                        StorageInfo destStorageInfo = destCloudSystem.PreproccessStorageInfo(destFile);
                        Dictionary<String, String> queryParams = Client.CopyFileQueryParams;
                        Dictionary<String, String> queryHeaders = Client.CopyFileHeaders;
                        String destPath = destCloudSystem.Client.BuildObjectAbsoluteURL(destStorageInfo.HighName, destStorageInfo.LowName);
                        String sourcePath = "/" + PathUtil.CombinePath(SystemLocation, sourceStorageInfo.LowName);
                        sourcePath = sourcePath.Replace("\\", "/");
                        queryHeaders.Add(AmazonConstants.AWS3_REST_HEADER_PREFIX + "copy-source", sourcePath);
                        result.IsCopyed = (bool)Client.Invoke("CopyFile", new object[] { destPath, queryParams, queryHeaders });
                    }
                    else
                    {
                        result.Message = "source file is not exist";
                        result.IsCopyed = false;
                    }
                }
                catch (Exception e)
                {
                    result.Message = e.ToString();
                    result.IsCopyed = false;
                    logger.Error("copy file failed.Details:{0}", e.Message);
                }
                return result;
            }
            else
            {
                return base.CopyFile(sourcFile, destSystem, destFile, isOverWrite);
            }

        }

        public override StorageInfo D5Preproccess2DirectoryStorageInfo(StorageInfo dirInfo)
        {
            var storageInfo=D5PreproccessStorageInfo(dirInfo);
            storageInfo.LowName = storageInfo.LowName.TrimEnd('/') + "/";
            return storageInfo;
        }

        public override StorageInfo D5PreproccessStorageInfo(StorageInfo dirInfo)
        {
            StorageInfo dirInfoTemp;
            if (String.IsNullOrEmpty(dirInfo.LowName))
            {
                dirInfo.LowName = String.Empty;
            }
            dirInfoTemp = dirInfo.Clone();
            dirInfoTemp.LowName = PathUtil.CombinePath(dirInfoTemp.HighName, dirInfoTemp.LowName);
            switch (XriObject.Params[XRIParameterKeys.REGION_KEY].ToLower(CultureInfo.InvariantCulture))
            {
                case AmazonConstants.US_WEST:
                    dirInfoTemp.HighName = StorageUrl.AmazonBucket_US_West + '.' + XriObject.Params["name"].ToLower(CultureInfo.InvariantCulture);
                    break;
                case AmazonConstants.US:
                    dirInfoTemp.HighName = StorageUrl.AmazonBucket_US + '.' + XriObject.Params["name"].ToLower(CultureInfo.InvariantCulture);
                    break;
                case AmazonConstants.EU:
                    dirInfoTemp.HighName = StorageUrl.AmazonBucket_EU + '.' + XriObject.Params["name"].ToLower(CultureInfo.InvariantCulture);
                    break;
                case AmazonConstants.TOKYO:
                    dirInfoTemp.HighName = StorageUrl.AmazonBucket_Tokyo + '.' + XriObject.Params["name"].ToLower(CultureInfo.InvariantCulture);
                    break;
                case AmazonConstants.APAC:
                    dirInfoTemp.HighName = StorageUrl.AmazonBucket_APAC + '.' + XriObject.Params["name"].ToLower(CultureInfo.InvariantCulture);
                    break;
            }
            if (dirInfoTemp.LowName.Equals("\\"))
            {
                dirInfoTemp.LowName = "";
            }
            dirInfoTemp.LowName = dirInfoTemp.LowName.Replace('\\', '/').TrimStart('/');
            return dirInfoTemp;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            if (!DirectoryExists(info))
            {
                logger.Warn("The directory you want to delete is not exist, path:{0}", info.HighPlusLowName);
                Deletion = true;
                return new StorageDeleteResult { IsDeleted = true };
            }
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(info);
            CheckState(dirInfoTemp.HighName);
            String urlWithoutQueryParms = Client.BuildURLWithOutQueryParams(dirInfoTemp.HighName);
            Dictionary<String, String> queryParams = Client.ListDirectoryQueryParams;
            if (!String.IsNullOrEmpty(dirInfoTemp.LowName) && !"/".Equals(dirInfoTemp.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams.Add("prefix", dirInfoTemp.LowName);
            }
            Dictionary<String, String> headers = Client.ListDirectoryHeaders;
            Boolean loop = false;
            long size = 0;
            do
            {
                loop = false;
                ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
                var xmlObjs = XElement.Parse(responseInfo.ResponseXml);
                XNamespace xmlns = xmlObjs.GetDefaultNamespace();
                XName contentsName = xmlns + "Contents";
                XName keyName = xmlns + "Key";
                var deleteContent = new XElement("Delete", from xmlObj in xmlObjs.Elements(contentsName)
                                                           select new XElement("Object", new XElement("Key", xmlObj.Element(keyName).Value)));
                (Client as AmazonClient).DeleteObjects(Client.BuildURLWithOutQueryParams(dirInfoTemp.HighName) + "/?delete", null, Client.Headers, deleteContent.ToString());
                size += xmlObjs.Elements(xmlns + "Contents").Sum(obj => long.Parse(obj.Element(xmlns + "Size") == null ? "0" : obj.Element(xmlns + "Size").Value));
                if (Boolean.Parse(xmlObjs.Element(xmlns + "IsTruncated").Value) && xmlObjs.Elements(xmlns + "Contents").Count() > 0)
                {
                    loop = true;
                }
            } while (loop);
            if (info.IsDeleteParentFolder)
            {
                var directoryPaths = new List<String>();
                var directoryNames = dirInfoTemp.LowName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                for (int i = 0; i < directoryNames.Length - 1; i++)
                {
                    directoryNames[i] = directoryNames[i] + "/";
                    if (i == 0)
                    {
                        directoryPaths.Add(directoryNames[i]);
                    }
                    else
                    {
                        directoryPaths.Add(directoryPaths[i - 1] + directoryNames[i]);
                    }
                }
                for (int index = directoryPaths.Count - 1; index >= 0; index--)
                {
                    var subBlobs = ListSubDirectoriesAndFiles(new StorageInfo(directoryPaths[index], ""));
                    if (subBlobs.Files.Count == 0 && subBlobs.SubDirs.Count == 0)
                    {
                        var deleteContent = new XElement("Delete", new XElement("Object", new XElement("Key", directoryPaths[index])));
                        (Client as AmazonClient).DeleteObjects(Client.BuildURLWithOutQueryParams(dirInfoTemp.HighName) + "/?delete", null, Client.Headers, deleteContent.ToString());
                    }
                    else
                    {
                        break;
                    }
                }
            }
            Deletion = true;
            return new StorageDeleteResult { IsDeleted = true, DeletedFileSize = size };
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        void ConstructCloudOpenParameter(String xriStr)
        {
            openParam = new AmazonOpenParameter();
            base.ConstructCloudOpenParameter(xriStr, openParam);
            if (XriObject.Params.ContainsKey(XRIParameterKeys.SIGNATUREVERSION_KEY))
            {
                string signVerStr = XriObject.Params[XRIParameterKeys.SIGNATUREVERSION_KEY].Trim();
                var signVer = Int32.Parse(signVerStr);
                openParam.SignatureVersion = signVer;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.REGION_KEY))
            {
                openParam.Region = XriObject.Params[XRIParameterKeys.REGION_KEY];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CUSTOMIZEDREGION_KEY))
            {
                openParam.CustomizedRegion = XriObject.Params[XRIParameterKeys.CUSTOMIZEDREGION_KEY];
                openParam.Region = openParam.CustomizedRegion;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.Enable_SSL))
            {
                openParam.Protocol = Boolean.Parse(XriObject.Params[XRIParameterKeys.Enable_SSL]) ? "https" : "http";
            }
            else
            {
                openParam.Protocol = "http";
            }
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "Amazon S3, Access Key ID: " + openParam.UserName + ", Container: " + SystemLocation;
            List<String> keys = new List<String>();
            keys.Add(this.openParam.SystemLocation);
            keys.Add(this.openParam.UserName);
            List<String> securityKeys = new List<String>();
            securityKeys.Add(this.openParam.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override XStream OpenStream(StorageInfo info, System.IO.FileMode fileMode)
        {
            Dictionary<String, String> preparedMetaInfos = new Dictionary<String, String>();
            foreach (KeyValuePair<String, String> entry in info.MetaInfos)
            {
                preparedMetaInfos["X-AMZ-META-".ToLower(CultureInfo.InvariantCulture) + entry.Key] = entry.Value != null ? Client.HttpClient.Encode(entry.Value) : entry.Value;
            }
            info.MetaInfos = preparedMetaInfos;
            return base.OpenStream(info, fileMode);
        }
    }
}
