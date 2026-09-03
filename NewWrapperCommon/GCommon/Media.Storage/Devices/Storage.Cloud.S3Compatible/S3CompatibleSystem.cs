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


namespace AvePoint.Media.Storage.S3Compatible
{
    #region using directives
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.S3Compatible.REST;
    using AvePoint.Media.Storage.S3Compatible.SystemWrapper;
    using AvePoint.Media.Storage.Util;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text.RegularExpressions;
    using System.Xml.Linq;
    using System.Xml.XPath;
    #endregion

    class S3CompatibleSystem : CloudSystem
    {
        private S3CompatibleOpenParameter openParam;
        private StorageLogger logger = new StorageLogger(typeof(S3CompatibleSystem));
        public ArrayListSubDirsWrapper dirs { set; get; }
        public ArrayListFilesWrapper files { set; get; }

        protected override void SetContainerKeyName()
        {
            this.SystemLocationKeyName = "bucketName".ToLower(CultureInfo.InvariantCulture);
        }

        public override string Type
        {
            get
            {
                return "S3CompatibleSystem";
            }
        }

        public Int32 TypeValue { get; set; }

        public String SystemPath { get; set; }

        public S3CompatibleSystem(string xriStr, string initMode, AbstractXSystem parentSystem)
            : base(xriStr, parentSystem)
        {
            logger = StorageLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            if (XriObject.Params.ContainsKey(XRIParameterKeys.EndPoint))
            {
                this.Client = new S3CompatibleClient(XriObject.Params[XRIParameterKeys.EndPoint]);
            }
            else
            {
                this.logger.Error("S3Compatible endpoint is null.");
                throw new ArgumentNullException("S3Compatible endpoint is null.");
            }
            if (initMode.Equals(XSystemConst.MODE_NOW_INITSYSTEM))
            {
                ConstructCloudOpenParameter(xriStr);
                this.Client.InitConfig(openParam);
                this.Client.HttpClient.CurrentSystem = this;
                base.SystemHealth = XSystemHealth.Unknown;
            }
            if (string.IsNullOrEmpty(SystemLocation))
            {
                SystemLocation = this.Client.GetDocAveDefaultContainer();
            }
        }

        /// <summary>
        /// Converts the XML to list.
        /// </summary>
        /// <param name="dirs">The dirs.</param>
        /// <param name="files">The files.</param>
        /// <param name="responseXmlString">The response XML string.</param>
        /// <param name="dirInfo">The dir info.</param>
        /// <param name="storageInfo">The storage info.</param>
        public void ConvertXmlToList(List<XDirectoryInfo> dirs,
                                    List<XFileInfo> files,
                                    string responseXmlString,
                                    StorageInfo dirInfo,
                                    StorageInfo storageInfo)
        {
            CloudDirectoryInfo dir;
            CloudFileInfo file;
            responseXmlString = responseXmlString.Replace(" xmlns=\"http://s3.amazonaws.com/doc/2006-03-01/\"", "");
            List<XPathNavigator> navs = this.Client.FirstStepAnalyzeXML(responseXmlString, "ListBucketResult/Contents");
            XPathNavigator singleNav;
            string name;
            long size;
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

            navs = this.Client.FirstStepAnalyzeXML(responseXmlString, "ListBucketResult/CommonPrefixes");
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
            AbstractCloudSystemWrapper abs = new S3CompatibleSystemWrapper(this, this.Client);
            StorageInfo storageInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            abs.GetListSubDirectoriesAndFilesCount(storageInfo);
            dirs = new ArrayListSubDirsWrapper(abs);
            files = new ArrayListFilesWrapper(abs);
            CheckState(storageInfo.HighName);
            string urlWithoutQueryParms = this.Client.BuildURLWithOutQueryParams(storageInfo.HighName);
            Dictionary<string, string> queryParams = this.Client.ListDirectoryQueryParams;
            if (!string.IsNullOrEmpty(storageInfo.LowName) && !"/".Equals(storageInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams["prefix"] = storageInfo.LowName;
            }
            queryParams["delimiter"] = "/";
            queryParams.Add("format", "xml");
            Dictionary<string, string> headers = this.Client.ListDirectoryHeaders;
            ResponseInfo responseInfo = (ResponseInfo)this.Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
            string responseXmlString = responseInfo.ResponseXml;
            List<XDirectoryInfo> dirsList = new List<XDirectoryInfo>();
            List<XFileInfo> filesList = new List<XFileInfo>();
            ConvertXmlToList(dirsList, filesList, responseXmlString, storageInfo, dirInfo);
            abs.ListResultsToArrayList(dirsList, filesList, (ArrayList)dirs, (ArrayList)files);
            dirs.SetState(responseInfo, queryParams,
            urlWithoutQueryParms, headers, storageInfo, dirInfo);
            files.SetState(responseInfo, queryParams,
            urlWithoutQueryParms, headers, storageInfo, dirInfo);
            StorageListResultSafety safeResults = new StorageListResultSafety();
            safeResults.SubDirs = dirs;
            safeResults.Files = files;
            return safeResults;
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {

            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            string urlWithoutQueryParms = this.Client.BuildURLWithOutQueryParams(dirInfoTemp.HighName);
            Dictionary<string, string> queryParams = this.Client.ListDirectoryQueryParams;
            if (!string.IsNullOrEmpty(dirInfoTemp.LowName) && !"/".Equals(dirInfoTemp.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams.Add("prefix", dirInfoTemp.LowName);
            }
            queryParams.Add("delimiter", "/");
            queryParams.Add("format", "xml");
            Dictionary<string, string> headers = this.Client.ListDirectoryHeaders;
            ResponseInfo responseInfo = (ResponseInfo)this.Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
            string responseXmlString = responseInfo.ResponseXml;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            List<XFileInfo> files = new List<XFileInfo>();
            ConvertXmlToList(dirs, files, responseXmlString, dirInfoTemp, dirInfo);
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
                    responseInfo = (ResponseInfo)this.Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
                    ConvertXmlToList(dirs, files, responseInfo.ResponseXml, dirInfoTemp, dirInfo);
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

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            CheckState(SystemLocation);
            StorageCopyResult rs = new StorageCopyResult();
            try
            {
                if ((bool)this.Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }))
                {
                    if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                    {
                        if ((bool)this.Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }) && !isOverWrite)
                        {
                            rs.IsCopyed = true;
                            return rs;
                        }
                    }
                    StorageInfo srcStorageInfo = PreproccessStorageInfo(sourceFileInfo);
                    StorageInfo destStorageInfo = PreproccessStorageInfo(targetFileInfo);
                    Dictionary<string, string> queryParams = this.Client.CopyFileQueryParams;
                    Dictionary<string, string> queryHeaders = this.Client.CopyFileHeaders;
                    string destPath = this.Client.BuildObjectAbsoluteURL(destStorageInfo.HighName, destStorageInfo.LowName);
                    string srcPath = "/" + PathUtil.CombinePath(SystemLocation, srcStorageInfo.LowName);
                    srcPath = srcPath.Replace("\\", "/");
                    queryHeaders.Add(S3CompatibleConstants.S3Compatible_REST_HEADER_PREFIX + "copy-source", this.Client.HttpClient.Encode(srcPath));
                    rs.IsCopyed = (bool)this.Client.Invoke("CopyFile", new object[] { destPath, queryParams, queryHeaders });
                }
                else
                {
                    rs.Message = "source file is not exist";
                    rs.IsCopyed = false;
                }
            }
            catch (Exception e)
            {
                rs.Message = e.ToString();
                rs.IsCopyed = false;
                logger.Error("copy file failed:" + e.Message);
            }
            return rs;
        }

        public override StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            if (destSystem is S3CompatibleSystem
               && XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY].Equals(destSystem.XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY], StringComparison.OrdinalIgnoreCase)
               && XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY].Equals(destSystem.XriObject.Params[XRIParameterKeys.Cloud_PASSWORD_KEY], StringComparison.OrdinalIgnoreCase))
            {
                CheckState(SystemLocation);
                CloudSystem destSystem2 = destSystem as S3CompatibleSystem;
                destSystem2.CheckState(destSystem2.SystemLocation);
                StorageCopyResult result = new StorageCopyResult();
                try
                {
                    if ((bool)this.Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(srcFile.HighName, srcFile.LowName).TrimStart(new char[] { '\\', '/' }) }))
                    {
                        if (srcFile.LowName.Equals(destFile.LowName, StringComparison.OrdinalIgnoreCase))
                        {
                            if ((bool)destSystem2.Client.Invoke("CheckObject", new object[] { destSystem2.SystemLocation, PathUtil.CombinePath(destFile.HighName, destFile.LowName).TrimStart(new char[] { '\\', '/' }) }) && !isOverWrite)
                            {
                                result.IsCopyed = true;
                                return result;
                            }
                        }
                        StorageInfo srcStorageInfo = PreproccessStorageInfo(srcFile);
                        StorageInfo destStorageInfo = destSystem2.PreproccessStorageInfo(destFile);
                        Dictionary<string, string> queryParams = this.Client.CopyFileQueryParams;
                        Dictionary<string, string> queryHeaders = this.Client.CopyFileHeaders;
                        string destPath = destSystem2.Client.BuildObjectAbsoluteURL(destStorageInfo.HighName, destStorageInfo.LowName);
                        string srcPath = "/" + PathUtil.CombinePath(SystemLocation, srcStorageInfo.LowName);
                        srcPath = srcPath.Replace("\\", "/");
                        queryHeaders.Add(S3CompatibleConstants.S3Compatible_REST_HEADER_PREFIX + "copy-source", srcPath);
                        result.IsCopyed = (bool)this.Client.Invoke("CopyFile", new object[] { destPath, queryParams, queryHeaders });
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
                    logger.Error("copy file failed:" + e.Message);
                }
                return result;
            }
            else
            {
                return base.CopyFile(srcFile, destSystem, destFile, isOverWrite);
            }
        }

        public override StorageInfo D5Preproccess2DirectoryStorageInfo(StorageInfo dirInfo)
        {
            StorageInfo dirInfoTemp;

            if (string.IsNullOrEmpty(dirInfo.LowName))
            {
                dirInfo.LowName = string.Empty;
            }

            dirInfoTemp = dirInfo.Clone();

            dirInfoTemp.LowName = PathUtil.CombinePath(dirInfoTemp.HighName, dirInfoTemp.LowName);
            if (dirInfoTemp.LowName.Equals("\\"))
            {
                dirInfoTemp.LowName = "";
            }
            dirInfoTemp.LowName = dirInfoTemp.LowName.Replace('\\', '/').TrimEnd('/').TrimStart('/') + "/";

            return dirInfoTemp;
        }

        public override StorageInfo D5PreproccessStorageInfo(StorageInfo dirInfo)
        {
            StorageInfo dirInfoTemp;

            if (string.IsNullOrEmpty(dirInfo.LowName))
            {
                dirInfo.LowName = string.Empty;
            }

            dirInfoTemp = dirInfo.Clone();

            dirInfoTemp.LowName = PathUtil.CombinePath(dirInfoTemp.HighName, dirInfoTemp.LowName);
            if (dirInfoTemp.LowName.Equals("\\"))
            {
                dirInfoTemp.LowName = "";
            }
            dirInfoTemp.LowName = dirInfoTemp.LowName.Replace('\\', '/').TrimStart('/');

            return dirInfoTemp;
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(info);
            CheckState(dirInfoTemp.HighName);
            string urlWithoutQueryParms = this.Client.BuildURLWithOutQueryParams(dirInfoTemp.HighName);
            Dictionary<string, string> queryParams = this.Client.ListDirectoryQueryParams;
            if (!string.IsNullOrEmpty(dirInfoTemp.LowName) && !"/".Equals(dirInfoTemp.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams.Add("prefix", dirInfoTemp.LowName);
            }
            Dictionary<string, string> headers = this.Client.ListDirectoryHeaders;

            bool loop = false;
            int size = 0;
            do
            {
                loop = false;
                ResponseInfo responseInfo = (ResponseInfo)this.Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });

                var xmlObjs = XElement.Parse(responseInfo.ResponseXml);
                XNamespace xmlns = xmlObjs.GetDefaultNamespace();
                XName contentsName = xmlns + "Contents";
                XName keyName = xmlns + "Key";
                var deleteContent = new XElement("Delete", from xmlObj in xmlObjs.Elements(contentsName)
                                                           select new XElement("Object", new XElement("Key", xmlObj.Element(keyName).Value)));
                var deletingItems = deleteContent.Elements();
                if (deletingItems.Count() == 0)
                {
                    logger.Warn("There is no found any file under the path: {0}", dirInfoTemp.LowName);
                    break;
                }
                (this.Client as S3CompatibleClient).DeleteObjects(this.Client.BuildURLWithOutQueryParams(dirInfoTemp.HighName) + "/?delete", null, this.Client.Headers, deleteContent.ToString());

                size += xmlObjs.Elements(xmlns + "Contents").Sum(obj => int.Parse(obj.Element(xmlns + "Size") == null ? "0" : obj.Element(xmlns + "Size").Value));

                if (bool.Parse(xmlObjs.Element(xmlns + "IsTruncated").Value) && xmlObjs.Elements(xmlns + "Contents").Count() > 0)
                {
                    loop = true;
                }
            } while (loop);

            return new StorageDeleteResult() { IsDeleted = true, DeletedFileSize = size };
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        protected void ConstructCloudOpenParameter(string xriStr)
        {
            openParam = new S3CompatibleOpenParameter();
            base.ConstructCloudOpenParameter(xriStr, openParam);
            if (XriObject.Params.ContainsKey(XRIParameterKeys.BUCKET_NAME))
            {
                openParam.Bucket = XriObject.Params[XRIParameterKeys.BUCKET_NAME];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.EndPoint))
            {
                openParam.EndPoint = XriObject.Params[XRIParameterKeys.EndPoint].ToLower();
                SetSecurityProtocol(openParam.EndPoint);
                    }
            this.Type = "S3Compatible";
            this.TypeValue = 410;
            this.SystemPath = String.Format("{0}/{1}", this.openParam.EndPoint, this.openParam.Bucket);
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "S3Compatible, Access Key ID: " + openParam.UserName + ", Bucket: " + SystemLocation;
            List<string> keys = new List<string>();
            keys.Add(this.openParam.SystemLocation);
            keys.Add(this.openParam.UserName);
            List<string> securityKeys = new List<string>();
            securityKeys.Add(this.openParam.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override XStream OpenStream(StorageInfo info, System.IO.FileMode fileMode)
        {
            Dictionary<string, string> preparedMetaInfos = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in info.MetaInfos)
            {
                preparedMetaInfos["X-AMZ-META-".ToLower(CultureInfo.InvariantCulture) + entry.Key] = entry.Value != null ? this.Client.HttpClient.Encode(entry.Value) : entry.Value;
            }
            info.MetaInfos = preparedMetaInfos;
            return base.OpenStream(info, fileMode);
        }
    }
}
