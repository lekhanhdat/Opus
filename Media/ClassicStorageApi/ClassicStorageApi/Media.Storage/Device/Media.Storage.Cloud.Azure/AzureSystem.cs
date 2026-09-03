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




namespace AvePoint.Media.ClassicStorage.Cloud.Azure
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using AvePoint.Media.ClassicStorage.Cloud.Common;
    using AvePoint.Media.ClassicStorage.Cloud.Azure.REST;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.Media.ClassicStorage.Util;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Config;
    using System.Xml.XPath;
    using System.Text.RegularExpressions;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Client;
    using System.Collections;
    using AvePoint.Media.ClassicStorage.Cloud.Azure.GetCount;
    using ArrayListTest;
    using AvePoint.Media.ClassicStorage.Cloud.Common.ListWrapper;
    using AvePoint.Media.ClassicStorage.Cloud.Common.Warpper.SystemWrapper;
    using System.IO;
    using System.Globalization;
    using AvePoint.Media.StorageApi;
    #endregion

    public class AzureSystem : CloudSystem
    {
        private AzureOpenParameter openParam;
        private AveLogger logger = new AveLogger(typeof(AzureSystem));
        public ArrayListSubDirsWrapper dirs { set; get; }
        public ArrayListFilesWrapper files { set; get; }

        private ulong assignedSpace = 0;
        private ulong assignedUsedSpace = 0;

        public override string Type
        {
            get
            {
                return "AzureSystem";
            }
        }

        public AzureSystem(string xriStr, string initMode, AbstractXSystem parentSystem)
            : base(xriStr, parentSystem)
        {
            //logger.Info(xriStr);
            client = new AzureClient();
            logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            if (initMode.Equals(XSystemConst.MODE_NOW_INITSYSTEM))
            {
                ConstructCloudOpenParameter(xriStr);
                client.InitConfig(openParam);
                client.HttpClient.CurrentSystem = this;
                base.SystemHealth = (global::Storage.XSystemHealth)XSystemHealth.Unknown;
            }
        }

        protected override void ConstructCloudOpenParameter(string xriStr)
        {
            openParam = new AzureOpenParameter();
            base.ConstructCloudOpenParameter(xriStr, openParam);
            if (XriObject.Params.ContainsKey(XRIParameterKeys.CDNED))
            {
                openParam.CdnEnaled = false;//bool.Parse(XriObject.Params[XRIParameterKeys.CDNED]);
                if (openParam.CdnEnaled)
                {
                    if (XriObject.Params.ContainsKey(XRIParameterKeys.CDN_GUID))
                    {
                        openParam.CdnGuid = XriObject.Params[XRIParameterKeys.CDN_GUID];
                    }
                }
            }
            if (!XriObject.Params.ContainsKey(XRIParameterKeys.AccessPoinyKey))
            {
                openParam.AccessPoint = "http://blob.core.windows.net";
            }
            else
            {
                openParam.AccessPoint = XriObject.Params[XRIParameterKeys.AccessPoinyKey];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.BLOCK_LENGTH))
            {
                openParam.BlockLength = Convert.ToInt64(XriObject.Params[XRIParameterKeys.BLOCK_LENGTH]);
            }
            //if (XriObject.Params.ContainsKey("assignedspace"))
            //{
            //    this.assignedSpace = Convert.ToUInt64(XriObject.Params["assignedspace"]);
            //}
            //if (XriObject.Params.ContainsKey("usedspace"))
            //{
            //    this.assignedUsedSpace = Convert.ToUInt64(XriObject.Params["usedspace"]);
            //}
            //if (assignedUsedSpace > assignedSpace)
            //{
            //    assignedUsedSpace = assignedSpace;
            //}

            //PhysicalDeviceDto 新增type
            this.SpaceThresholdUnit = SpaceThresholdUnit.MB;
            this.SpaceThreshold = 0;

            this.Type = "AzureSystem";
            this.TypeValue = 403;
            this.SystemPath = String.Format("http://{0}.blob.core.windows.net/{1}", this.openParam.UserName, this.openParam.SystemLocation);
        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "Microsoft Azure Storage, Account: " + openParam.AccessPoint + ", Container: " + SystemLocation;
            List<string> keys = new List<string>();
            keys.Add(this.openParam.SystemLocation);
            keys.Add(this.openParam.UserName);
            List<string> securityKeys = new List<string>();
            securityKeys.Add(this.openParam.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            if (!DirectoryExists(info))
            {
                logger.Warn("The directory you want to delete is not exist, path:{0}", info.HighPlusLowName);
                Deletion = true;
                return new StorageDeleteResult { IsDeleted = true };
            }
            StorageInfo dirInfo = Preproccess2DirectoryStorageInfo(info);
            CheckState(dirInfo.HighName);
            string urlWithoutQueryParms = client.BuildURLWithOutQueryParams(dirInfo.HighName);
            Dictionary<string, string> queryParams = client.ListDirectoryQueryParams;
            if (!string.IsNullOrEmpty(dirInfo.LowName) && !"/".Equals(dirInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams.Add("prefix", dirInfo.LowName);
            }
            //queryParams.Add("format", "xml");
            Dictionary<string, string> headers = client.ListDirectoryHeaders;
            //ResponseInfo responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
            ResponseInfo responseInfo = (ResponseInfo)client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
            string responseXmlString = responseInfo.ResponseXml;
            List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "EnumerationResults/Blobs/Blob");
            XPathNavigator singleNav;
            List<string> needDeletedObject = new List<string>();
            string name;
            long size = 0;
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                size = 0;
                singleNav = nav.SelectSingleNode("Name");
                if (singleNav != null)
                {
                    name = singleNav.Value;
                    if (!needDeletedObject.Contains(name))
                    {
                        needDeletedObject.Add(name);
                    }
                    singleNav = nav.SelectSingleNode("Properties/Content-Length");
                    if (singleNav != null)
                    {
                        size += singleNav.ValueAsLong;

                    }
                }
            }

            navs = client.FirstStepAnalyzeXML(responseXmlString, "EnumerationResults/Blobs/BlobPrefix");

            foreach (XPathNavigator nav in navs)
            {
                singleNav = nav.SelectSingleNode("Name");
                if (singleNav != null)
                {
                    name = singleNav.Value;
                    if (!needDeletedObject.Contains(name))
                    {
                        needDeletedObject.Add(name);
                    }
                }
            }

            string fullURL;
            foreach (string needDeletefile in needDeletedObject)
            {
                fullURL = client.BuildObjectAbsoluteURL(dirInfo.HighName, needDeletefile);
                Dictionary<string, string> deleteHeaders = client.Headers;
                //client.DeleteObject(fullURL,null, headers);
                client.Invoke("DeleteObject", new object[] { fullURL, new Dictionary<string, string>(), headers });
            }

            StorageDeleteResult rs = new StorageDeleteResult();
            rs.DeletedFileSize = size;
            rs.IsDeleted = true;

            //标记执行过删除
            Deletion = true;
            return rs;
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
            List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "EnumerationResults/Blobs/Blob");
            XPathNavigator singleNav;
            string name;
            long size;
            string modified;

            foreach (XPathNavigator nav in navs)
            {
                name = null;
                size = 0;
                singleNav = nav.SelectSingleNode("Name");
                modified = null;
                if (singleNav != null)
                {
                    name = singleNav.Value;

                    singleNav = nav.SelectSingleNode("Properties/Content-Length");
                    if (singleNav != null)
                    {
                        size = singleNav.ValueAsLong;
                    }
                    singleNav = nav.SelectSingleNode("Properties/Last-Modified");
                    if (singleNav != null)
                    {
                        modified = singleNav.Value;
                    }
                    name = RemoveFirst(name, dirInfo.LowName);
                    if (name.Contains("/"))
                    {
                        int index = name.IndexOf('/');
                        if (index > 0)
                        {
                            name = name.Substring(0, index);
                        }
                        if (!string.IsNullOrEmpty(name) && !name.Contains("/"))
                        {
                            dir = new CloudDirectoryInfo(storageInfo.HighPlusLowName, name, modified);
                            int i = dirs.FindIndex(delegate(XDirectoryInfo x)
                            {
                                if (name.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    return true;
                                }
                                return false;
                            });
                            if (i >= 0)
                            {
                                continue;
                            }
                            dirs.Add(dir);
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(name))
                        {
                            file = new CloudFileInfo(storageInfo.HighPlusLowName, name, size, modified);
                            files.Add(file);
                        }
                    }
                }
            }

            navs = client.FirstStepAnalyzeXML(responseXmlString, "EnumerationResults/Blobs/BlobPrefix");
            foreach (XPathNavigator nav in navs)
            {
                modified = String.Empty;
                singleNav = nav.SelectSingleNode("Name");
                if (singleNav != null)
                {
                    name = singleNav.Value;
                    singleNav = nav.SelectSingleNode("Properties/Last-Modified");
                    if (singleNav != null)
                    {
                        modified = singleNav.Value;
                    }
                    name = RemoveFirst(name, dirInfo.LowName);
                    if (name.Contains("/"))
                    {
                        int index = name.IndexOf('/');
                        if (index > 0)
                        {
                            name = name.Substring(0, index);
                        }
                        if (!string.IsNullOrEmpty(name) && !name.Contains("/"))
                        {
                            dir = new CloudDirectoryInfo(storageInfo.HighPlusLowName, name, modified);
                            int i = dirs.FindIndex(delegate(XDirectoryInfo x)
                            {
                                if (name.Equals(x.Name, StringComparison.CurrentCultureIgnoreCase))
                                {
                                    return true;
                                }
                                return false;
                            });
                            if (i >= 0)
                            {
                                continue;
                            }
                            dirs.Add(dir);
                        }
                    }
                }
            }
        }

        public override StorageCopyResult CopyFile(StorageInfo srcFile, IXSystemCommon destSystem, StorageInfo destFile, bool isOverWrite)
        {
            if (this.Type.Equals(destSystem.Type))
            {
                var rs = new StorageCopyResult();
                try
                {
                    if ((bool)client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(srcFile.HighName, srcFile.LowName).TrimStart(new char[] { '\\', '/' }) }))
                    {
                        if (destSystem.FileExists(destFile))
                        {
                            if (!isOverWrite)
                            {
                                rs.IsCopyed = true;
                                return rs;
                            }
                            else
                            {
                                destSystem.DeleteFile(destFile);
                            }
                        }
                        var copyResource = (string)client.Invoke("GetCopyResource", new object[] { srcFile, openParam.Password });
                        var result = ((AzureSystem)destSystem).CopyFile(copyResource, destFile);
                        if (result)
                        {
                            rs.IsCopyed = true;
                        }
                        else
                        {
                            rs.IsCopyed = false;
                        }
                    }
                    else
                    {
                        rs.Message = "source file is not exist";
                        rs.IsCopyed = false;
                    }
                }
                catch (Exception e)
                {
                    rs.Message = e.Message;
                    rs.IsCopyed = false;
                    logger.Error("Copy file failed: {0}", e);
                }
                return rs;
            }
            else
            {
                return base.CopyFile(srcFile, destSystem, destFile, isOverWrite);
            }
        }

        public bool CopyFile(string copyResource, StorageInfo copyTo)
        {
            var copyPath = client.BuildObjectAbsoluteURL(SystemLocation, PathUtil.CombinePath(copyTo.HighName, copyTo.LowName).TrimStart(new char[] { '\\', '/' }));
            var result = (bool)client.Invoke("CopyFile", new object[] { copyResource, copyPath });
            return result;
        }

        public bool CopyFileAndSetTier(string copyResource, StorageInfo copyTo, AccessTierType tier)
        {
            var copyPath = client.BuildObjectAbsoluteURL(SystemLocation, PathUtil.CombinePath(copyTo.HighName, copyTo.LowName).TrimStart(new char[] { '\\', '/' }));
            string accessTier;
            switch (tier)
            {
                case AccessTierType.Cool:
                    {
                        accessTier = "Cool";
                        break;
                    }
                case AccessTierType.Archive:
                    {
                        accessTier = "Archive";
                        break;
                    }
                case AccessTierType.Other:
                case AccessTierType.Hot:
                default:
                    {
                        accessTier = "Hot";
                        break;
                    }
            }
            var result = (bool)client.Invoke("CopyFile", new object[] { copyResource, copyPath, accessTier });
            return result;
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            CheckState();
            StorageCopyResult rs = new StorageCopyResult();
            XStream sourceStream = null;
            XStream destStream = null;
            try
            {
                if ((bool)client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }))
                {
                    if ((bool)client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }))
                    {
                        if (!isOverWrite)
                        {
                            rs.IsCopyed = true;
                            return rs;
                        }
                        else
                        {
                            StorageInfo infoTemp = PreproccessStorageInfo(targetFileInfo);
                            client.Invoke("DeleteObject", new object[] { infoTemp.HighName, infoTemp.LowName, false });
                        }
                    }
                    var copyResource = (string)client.Invoke("GetCopyResource", new object[] { sourceFileInfo, openParam.Password });
                    if (targetFileInfo.FileTierType != AccessTierType.Other && client is AzureClient)
                    {
                        CopyFileAndSetTier(copyResource, targetFileInfo, targetFileInfo.FileTierType);
                    }
                    else
                    {
                        var result = this.CopyFile(copyResource, targetFileInfo);
                        if (result)
                        {
                            rs.IsCopyed = true;
                        }
                        else
                        {
                            rs.IsCopyed = false;
                        }
                    }
                }
                else
                {
                    rs.Message = "source file is not exist";
                    rs.IsCopyed = false;
                }
            }
            catch (Exception e)
            {
                rs.Message = e.Message;
                rs.IsCopyed = false;
                logger.Error("copy file failed:" + e.Message);
            }
            finally
            {
                if (destStream != null)
                {
                    destStream.Close();
                }
                if (sourceStream != null)
                {
                    sourceStream.Close();
                }
            }
            return rs;
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            StorageInfo storageInfo = Preproccess2DirectoryStorageInfo(dirInfo);

            AbstractCloudSystemWrapper abs = new AzureSystemWrapper(this, client);
            abs.GetListSubDirectoriesAndFilesCount(storageInfo);
            dirs = new ArrayListSubDirsWrapper(abs);
            files = new ArrayListFilesWrapper(abs);

            CheckState(storageInfo.HighName);
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
            //ResponseInfo responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
            ResponseInfo responseInfo = (ResponseInfo)client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
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

        public override StorageInfo D5Preproccess2DirectoryStorageInfo(StorageInfo dirInfo)
        {
            if (string.IsNullOrEmpty(dirInfo.LowName))
            {
                dirInfo.LowName = string.Empty;
            }
            StorageInfo info = dirInfo.Clone();
            SystemLocation = string.Empty;
            info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
            info.HighName = SystemLocation;

            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            info.LowName = info.LowName.Replace('\\', '/').TrimEnd('/').TrimStart('/') + "/";

            return info;
        }

        public override StorageInfo D5PreproccessStorageInfo(StorageInfo dirInfo)
        {
            if (string.IsNullOrEmpty(dirInfo.LowName))
            {
                dirInfo.LowName = string.Empty;
            }
            client.Data_Version = dirInfo.DataVersion;
            StorageInfo info = dirInfo.Clone();

            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            info.LowName = info.LowName.Replace('\\', '/').TrimStart('/');

            return info;
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            return base.ListFiles(dirInfo);
        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            string tempHighName = dirInfo.HighName;
            StorageInfo storageInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(storageInfo.HighName);
            string urlWithoutQueryParms = string.Empty;
            string queryParms = string.Empty;
            if (dirInfo.DataVersion == Data_Version.DocAve5)
            {
                urlWithoutQueryParms = client.BuildURLWithOutQueryParams(storageInfo.HighName);
                queryParms = urlWithoutQueryParms.Substring(0, urlWithoutQueryParms.LastIndexOf("/", StringComparison.CurrentCulture) + 1) + "?comp=list";
            }
            else
            {
                urlWithoutQueryParms = client.BuildURLWithOutQueryParams(storageInfo.HighName);
            }
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
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            List<XFileInfo> files = new List<XFileInfo>();
            ResponseInfo responseInfo = null;
            string responseXmlString = string.Empty;

            if (dirInfo.DataVersion == Data_Version.DocAve5)
            {
                //responseInfo = client.ListObjects(queryParms, null, headers);
                responseInfo = (ResponseInfo)client.Invoke("ListObjects", new object[] { queryParms, new Dictionary<string, string>(), headers });
                responseXmlString = responseInfo.ResponseXml;
                List<XPathNavigator> navs = client.FirstStepAnalyzeXML(responseXmlString, "EnumerationResults/Containers/Container");
                XPathNavigator singleNav;
                string name;
                List<string> lists = new List<string>();
                List<string> names = new List<string>();
                foreach (XPathNavigator nav in navs)
                {
                    name = null;
                    singleNav = nav.SelectSingleNode("Name");
                    if (singleNav != null)
                    {
                        name = singleNav.Value;
                        lists.Add(urlWithoutQueryParms.Substring(0, urlWithoutQueryParms.LastIndexOf("/", StringComparison.CurrentCulture) + 1) + name + "?restype=container");
                    }
                }
                foreach (string path in lists)
                {
                    names.Add(client.ListAzureMetaName(path, null, headers));
                }
                foreach (string n in names)
                {
                    try
                    {
                        if (!string.IsNullOrEmpty(n))
                        {
                            if (storageInfo.IsLoadFirstLevel)
                            {
                                if (n.Contains("\\"))
                                {
                                    continue;
                                }
                                if (n.Contains("/"))
                                {
                                    string[] temps = n.Split(new char[] { '/' });
                                    CloudDirectoryInfo dir = new CloudDirectoryInfo(dirInfo.HighPlusLowName, temps[0]);
                                    dirs.Add(dir);
                                }
                                else
                                {
                                    CloudDirectoryInfo dir = new CloudDirectoryInfo(dirInfo.HighPlusLowName, n);
                                    dirs.Add(dir);
                                }
                            }
                            else
                            {
                                string tempName = n.Replace("\\", "/");
                                string tempPath = tempHighName.Replace("\\", "/");
                                if (tempName.StartsWith(tempPath.Substring(0, tempPath.Length), StringComparison.CurrentCulture) && !tempName.Equals(tempPath.Substring(0, tempPath.Length)))
                                {
                                    string temp = tempName.Substring(tempPath.Length + 1);
                                    if (!string.IsNullOrEmpty(temp))
                                    {
                                        if (temp.Contains("/"))
                                        {
                                            CloudDirectoryInfo dir = new CloudDirectoryInfo(tempHighName, temp.Substring(0, temp.IndexOf("/", StringComparison.CurrentCulture)));
                                            dirs.Add(dir);
                                        }
                                        else
                                        {
                                            CloudDirectoryInfo dir = new CloudDirectoryInfo(tempHighName, temp);
                                            dirs.Add(dir);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ee)
                    {
                        string m = ee.Message;
                    }
                }
            }
            else
            {
                //responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
                responseInfo = (ResponseInfo)client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
                responseXmlString = responseInfo.ResponseXml;
                ConvertXmlToList(dirs, files, responseXmlString, storageInfo, dirInfo);
            }
            //ConvertXmlToList(dirs, files, responseXmlString, storageInfo, dirInfo);

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
                    //responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
                    responseInfo = (ResponseInfo)client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
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


        public override bool DirectoryExists(StorageInfo info)
        {
            if (info.DataVersion == Data_Version.DocAve5)
            {
                if (string.IsNullOrEmpty(info.HighName))
                {
                    return true;
                }
                List<XDirectoryInfo> dirs = ListDirectories(info);
                if (dirs.Count != 0)
                {
                    foreach (XDirectoryInfo item in dirs)
                    {
                        if (item.FullName.StartsWith(info.HighName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    if (string.IsNullOrEmpty(info.LowName))
                    {
                        info.LowName = "/";
                    }
                    return (bool)client.Invoke("CheckObject", new object[] { info.HighName, info.LowName });
                }
            }
            else
            {
                return base.DirectoryExists(info);
            }
        }

        public override bool DirectoryExistsAzure(StorageInfo info)
        {
            if (info.DataVersion == Data_Version.DocAve5)
            {
                if (string.IsNullOrEmpty(info.HighName))
                {
                    return true;
                }
                List<XDirectoryInfo> dirs = ListDirectories(info);
                if (dirs.Count != 0)
                {
                    foreach (XDirectoryInfo item in dirs)
                    {
                        if (item.FullName.StartsWith(info.HighName, StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                    return false;
                }
                else
                {
                    if (string.IsNullOrEmpty(info.LowName))
                    {
                        info.LowName = "/";
                    }
                    return (bool)client.Invoke("CheckObject", new object[] { info.HighName, info.LowName });
                }
            }
            else
            {
                return base.DirectoryExists(info);
            }
        }
        public override Int64 GetDirectorySize(StorageInfo info)
        {
            var tempInfo = Preproccess2DirectoryStorageInfo(info);
            CheckState(tempInfo.HighName);
            return this.client.GetContainerSize(tempInfo.HighName);
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        public override XDirectoryInfo OpenDirectory(StorageInfo dirInfo, System.IO.FileMode mode)
        {
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            CloudDirectoryInfo dir = null;
            string name = "/".Equals(dirInfoTemp.LowName) ? "" : dirInfoTemp.LowName;
            string fullURL = client.BuildObjectAbsoluteURL(dirInfoTemp.HighName, name);
            Dictionary<string, string> headers = client.OpenDirectoryWriteModeHeaders;
            switch (mode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                    headers["Content-Type"] = "DOCAVE/directory".ToLower(CultureInfo.InvariantCulture);
                    headers["Content-Length"] = "0";
                    if (!string.IsNullOrEmpty(dirInfoTemp.HighName))
                    {
                        client.Invoke("CreateObjectWithNoContent", new object[] { fullURL, headers });
                    }
                    dir = new CloudDirectoryInfo(dirInfoTemp.HighName, dirInfoTemp.LowName);
                    dir.IsExists = true;
                    break;
                case FileMode.Open:
                    bool exists = DirectoryExists(dirInfo);
                    if (!exists)
                    {
                        return null;
                    }
                    dir = new CloudDirectoryInfo(dirInfoTemp.HighName, dirInfoTemp.LowName);
                    dir.IsExists = exists;
                    break;

                case FileMode.Append:
                case FileMode.Truncate:
                default:
                    break;
                    throw new UnsupportedXException("Unsupported File Mode : " + mode.ToString());
            }
            return dir;
        }

        public override XStream OpenStream(StorageInfo info, System.IO.FileMode fileMode)
        {
            Dictionary<string, string> preparedMetaInfos = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in info.MetaInfos)
            {
                preparedMetaInfos[entry.Key] = entry.Value != null ? client.HttpClient.Encode(entry.Value) : entry.Value;
            }
            info.MetaInfos = preparedMetaInfos;
            return base.OpenStream(info, fileMode);
        }

        public override bool IsFull
        {
            get
            {
                return this.totalFreeSpace <= 0;
            }
        }

        public override SpaceInfo CheckFreeSpace()
        {
            SpaceInfo spaceInfo = new SpaceInfo();
            if (this.assignedSpace > 0)
            {
                spaceInfo.TotalSpace = this.assignedSpace;
                spaceInfo.TotalUsedSpace = this.assignedUsedSpace;
                spaceInfo.TotalFreeSpace = this.assignedSpace - this.assignedUsedSpace;
                this.totalFreeSpace = spaceInfo.TotalFreeSpace;
            }
            else
            {
                spaceInfo.TotalSpace = long.MaxValue - 1;
                spaceInfo.TotalUsedSpace = 0;
                spaceInfo.TotalFreeSpace = long.MaxValue - 1;
                this.totalFreeSpace = long.MaxValue - 1;
            }
            return spaceInfo;
        }

        public override StorageChangeResult ChangeFileTier(StorageInfo info)
        {
            this.CheckState();
            var result = new StorageChangeResult();
            StorageInfo infoTemp = PreproccessStorageInfo(info);
            result.IsChanged = (Boolean)client.Invoke("ChangeBlobTier", new object[] { infoTemp.HighName, infoTemp.LowName, infoTemp.FileTierType.ToString() });
            return result;
        }
    }
}

