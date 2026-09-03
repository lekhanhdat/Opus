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



using AvePoint.Media.Storage.Cloud.Common;
using AvePoint.Media.Storage.Util;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Xml.XPath;

namespace AvePoint.Media.Storage.Cloud.Rackspace
{
    class RackspaceSystem : CloudSystem
    {
        private RackspaceOpenParameter OpenParam;
        private StorageLogger logger = new StorageLogger(typeof(RackspaceSystem));
        public ArrayListSubDirsWrapper dirs { set; get; }
        public ArrayListFilesWrapper files { set; get; }

        public override string Type
        {
            get
            {
                return "RackspaceSystem";
            }
        }

        public override StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite)
        {
            CheckState(SystemLocation);
            StorageCopyResult rs = new StorageCopyResult();
            try
            {
                if ((bool)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(sourceFileInfo.HighName, sourceFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }))
                {
                    if (sourceFileInfo.LowName.Equals(targetFileInfo.LowName, StringComparison.OrdinalIgnoreCase))
                    {
                        if ((bool)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }) && !isOverWrite)
                        {
                            rs.IsCopyed = true;
                            return rs;
                        }
                    }

                    StorageInfo srcStorageInfo = PreproccessStorageInfo(sourceFileInfo);
                    StorageInfo destStorageInfo = PreproccessStorageInfo(targetFileInfo);

                    Dictionary<string, string> queryParams = Client.CopyFileQueryParams;
                    Dictionary<string, string> queryHeaders = Client.CopyFileHeaders;

                    string srcBaseURL = Client.BuildObjectAbsoluteURL(srcStorageInfo.HighName, srcStorageInfo.LowName);

                    string destPath = "/" + PathUtil.CombinePath(Client.HttpClient.Encode(SystemLocation), Client.HttpClient.Encode(destStorageInfo.LowName));
                    destPath = destPath.Replace("\\", "/");

                    queryHeaders.Add("Destination", destPath);
                    rs.IsCopyed = (bool)Client.Invoke("CopyFile", new object[] { srcBaseURL, queryParams, queryHeaders });
                }
                else
                {
                    rs.Message = "source file is not exist";
                    rs.IsCopyed = false;
                }
            }
            catch (Exception e)
            {
                if (e is GatewayTimeoutException)
                {
                    Int16 retryCount = 0;
                    Thread.Sleep(this.RetryInterval);
                    while (true)
                    {
                        if ((bool)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(targetFileInfo.HighName, targetFileInfo.LowName).TrimStart(new char[] { '\\', '/' }) }))
                        {
                            rs.Message = "copy file succeed.";
                            rs.IsCopyed = true;
                            logger.Info("Copy file succeed.Name = {0}", targetFileInfo.LowName);
                            break;
                        }
                        else if (retryCount <= this.MaxRetryCount)
                        {
                            logger.Debug("RetryCount = {0}", retryCount);
                            retryCount++;
                            Thread.Sleep(this.RetryInterval);
                            continue;
                        }
                        else
                        {
                            rs.Message = e.ToString();
                            rs.IsCopyed = false;
                            logger.Error("copy file failed:{0}", e.ToString());
                            break;
                        }
                    }
                }
                else
                {
                    rs.Message = e.ToString();
                    rs.IsCopyed = false;
                    logger.Error("copy file failed:{0}", e.ToString());
                }
            }
            return rs;
        }


        public override StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite)
        {
            if (destSystem is RackspaceSystem
                && XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY].Equals(destSystem.XriObject.Params[XRIParameterKeys.Cloud_USERNAME_KEY], StringComparison.OrdinalIgnoreCase))
            {
                CheckState(SystemLocation);
                CloudSystem destSystem2 = destSystem as RackspaceSystem;
                destSystem2.CheckState(destSystem2.SystemLocation);
                StorageCopyResult result = new StorageCopyResult();
                try
                {
                    if ((bool)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(srcFile.HighName, srcFile.LowName).TrimStart(new char[] { '\\', '/' }) }))
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

                        Dictionary<string, string> queryParams = Client.CopyFileQueryParams;
                        Dictionary<string, string> queryHeaders = Client.CopyFileHeaders;

                        string srcBaseURL = Client.BuildObjectAbsoluteURL(srcStorageInfo.HighName, srcStorageInfo.LowName);

                        string destPath = "/" + PathUtil.CombinePath(Client.HttpClient.Encode(destSystem2.SystemLocation), Client.HttpClient.Encode(destStorageInfo.LowName));
                        destPath = destPath.Replace("\\", "/");

                        queryHeaders.Add("Destination", destPath);
                        result.IsCopyed = (bool)Client.Invoke("CopyFile", new object[] { srcBaseURL, queryParams, queryHeaders });

                    }
                    else
                    {
                        result.Message = "source file is not exist";
                        result.IsCopyed = false;
                    }
                }
                catch (Exception e)
                {
                    if (e is GatewayTimeoutException)
                    {
                        Int16 retryCount = 0;
                        Thread.Sleep(this.RetryInterval);
                        while (true)
                        {
                            if ((bool)Client.Invoke("CheckObject", new object[] { SystemLocation, PathUtil.CombinePath(srcFile.HighName, srcFile.LowName).TrimStart(new char[] { '\\', '/' }) }))
                            {
                                result.Message = "copy file succeed.";
                                result.IsCopyed = true;
                                logger.Debug("Copy file succeed.");
                                break;
                            }
                            else if (retryCount <= this.MaxRetryCount)
                            {
                                Thread.Sleep(this.RetryInterval);
                                retryCount++;
                                continue;
                            }
                            else
                            {
                                result.Message = e.ToString();
                                result.IsCopyed = false;
                                logger.Error("copy file failed: {0}", e);
                                break;
                            }
                        }
                    }
                    else
                    {
                        result.Message = e.ToString();
                        result.IsCopyed = false;
                        logger.Error("copy file failed: {0}", e);
                    }
                }
                return result;
            }
            else
            {
                return base.CopyFile(srcFile, destSystem, destFile, isOverWrite);
            }

        }


        public RackspaceSystem(string xri, string initMode, AbstractXSystem parentSystem)
            : base(xri, parentSystem)
        {
            Client = new RackspaceClient();
            logger = StorageLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            if (initMode.Equals(XSystemConst.MODE_NOW_INITSYSTEM))
            {
                ConstructCloudOpenParameter(xri);
                Client.InitConfig(OpenParam);
                Client.HttpClient.CurrentSystem = this;
                base.SystemHealth = XSystemHealth.Unknown;
            }
            this.Open();
        }

        void ConstructCloudOpenParameter(string xriStr)
        {
            OpenParam = new RackspaceOpenParameter();
            base.ConstructCloudOpenParameter(xriStr, OpenParam);

            if (XriObject.Params.ContainsKey(XRIParameterKeys.CDN_KEY))
            {
                if (string.Compare(XriObject.Params[XRIParameterKeys.CDN_KEY].ToLower(CultureInfo.InvariantCulture), "true", StringComparison.OrdinalIgnoreCase) == 0)
                {
                    OpenParam.CdnEnaled = true;
                }
                else
                {
                    OpenParam.CdnEnaled = false;
                }
            }

        }

        protected override void SetSystemDescription()
        {
            Properties[SystemPropertyKeys.SystemDescriptionKey] = "Rackspace Cloud File, Username: " + OpenParam.UserName + ", Container: " + SystemLocation;
            List<string> keys = new List<string>();
            keys.Add(this.OpenParam.SystemLocation);
            keys.Add(this.OpenParam.UserName);
            List<string> securityKeys = new List<string>();
            securityKeys.Add(this.OpenParam.Password);
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
            StorageInfo storageInfo = Preproccess2DirectoryStorageInfo(info);
            CheckState(storageInfo.HighName);
            Dictionary<string, string> queryObjectsParams = Client.ListObjectQueryParams;
            if (!string.IsNullOrEmpty(storageInfo.LowName) && !"/".Equals(storageInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryObjectsParams.Add("prefix", storageInfo.LowName);
            }
            queryObjectsParams.Add("format", "xml");

            Dictionary<string, string> queryObjectsHeaders = Client.ListObjectHeaders;
            string baseURL = Client.BuildURLWithOutQueryParams(storageInfo.HighName);
            ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { baseURL, queryObjectsParams, queryObjectsHeaders });
            string responseString = responseInfo.ResponseXml;

            //delete get deleted size;
            List<string> needDeletedObjectsBytes = Client.AnalyzeXML(responseString, "container/object/bytes");
            long deletedSize = 0;

            foreach (string needDeletedObjectBytes in needDeletedObjectsBytes)
            {
                deletedSize += long.Parse(needDeletedObjectBytes);
            }

            List<string> needDeletedObjectsPaths = Client.AnalyzeXML(responseString, "container/object/name");
            string fullURL;
            Dictionary<string, string> deleteHeaders = Client.Headers;
            foreach (string needDeletedObjectPath in needDeletedObjectsPaths)
            {
                fullURL = Client.BuildObjectAbsoluteURL(storageInfo.HighName, needDeletedObjectPath);
                Client.Invoke("DeleteObject", new object[] { fullURL, new Dictionary<string, string>(), deleteHeaders });
            }

            List<string> needDeletedObjectsPaths2 = Client.AnalyzeXML(responseString, "container/" + "SUBDIR".ToLower(CultureInfo.InvariantCulture) + "/name");

            foreach (string needDeletedObjectPath in needDeletedObjectsPaths2)
            {
                fullURL = Client.BuildObjectAbsoluteURL(storageInfo.HighName, needDeletedObjectPath);
                Client.Invoke("DeleteObject", new object[] { fullURL, new Dictionary<string, string>(), deleteHeaders });
            }
            if (this.OpenParam.ModuleType == 1)
            {
                Client.Invoke("DeleteObject", new object[] { storageInfo.HighName, storageInfo.LowName.TrimEnd('/'), false });
            }
            if (info.IsDeleteParentFolder)
            {
                var directoryPaths = new List<String>();
                var directoryNames = storageInfo.LowName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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
                        fullURL = Client.BuildObjectAbsoluteURL(storageInfo.HighName, directoryPaths[index]);
                        Client.Invoke("DeleteObject", new object[] { fullURL, new Dictionary<string, string>(), deleteHeaders });
                    }
                    else
                    {
                        break;
                    }
                }
            }
            Deletion = true;
            return new StorageDeleteResult { IsDeleted = true, DeletedFileSize = deletedSize }; ;
        }

        public override StorageInfo D5Preproccess2DirectoryStorageInfo(StorageInfo dirInfo)
        {
            StorageInfo info = null;
            if (string.IsNullOrEmpty(dirInfo.LowName))
            {
                dirInfo.LowName = string.Empty;
            }
            if (!string.IsNullOrEmpty(dirInfo.HighName))
            {
                dirInfo.HighName = dirInfo.HighName.Replace("\\", ":");
            }
            SystemLocation = string.Empty;
            info = dirInfo.Clone();

            if (!string.IsNullOrEmpty(SystemLocation))
            {
                info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
                info.HighName = SystemLocation;
            }

            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            info.LowName = info.LowName.Replace('\\', '/').TrimEnd('/').TrimStart('/') + "/";

            return info;
        }

        public override StorageInfo D5PreproccessStorageInfo(StorageInfo dirInfo)
        {
            StorageInfo info = null;
            if (string.IsNullOrEmpty(dirInfo.LowName))
            {
                dirInfo.LowName = string.Empty;
            }
            if (!string.IsNullOrEmpty(dirInfo.HighName))
            {
                dirInfo.HighName = dirInfo.HighName.Replace("\\", ":");
            }
            SystemLocation = string.Empty;
            info = dirInfo.Clone();

            if (!string.IsNullOrEmpty(SystemLocation))
            {
                info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
                info.HighName = SystemLocation;
            }

            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            this.OpenParam.D5FolderName = info.HighName;
            info.LowName = dirInfo.LowName.Replace('\\', '/').TrimStart('/');

            return info;
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
                    return base.DirectoryExists(info);
                }
            }
            else
            {
                return base.DirectoryExists(info);
            }
        }

        public override List<XFileInfo> ListFiles(StorageInfo dirInfo)
        {
            if (dirInfo.DataVersion == Data_Version.DocAve5)
            {
                bool flag = false;
                List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
                List<XFileInfo> files = new List<XFileInfo>();
                string responseXmlString = String.Empty;
                List<XPathNavigator> fileNavs = new List<XPathNavigator>();
                List<XPathNavigator> dirNavs = new List<XPathNavigator>();

                StorageInfo storageInfo = Preproccess2DirectoryStorageInfo(dirInfo);
                CheckState(storageInfo.HighName);
                string urlWithoutQueryParms = Client.BuildURLWithOutQueryParams(storageInfo.HighName);
                Dictionary<string, string> queryParams = Client.ListDirectoryQueryParams;
                queryParams.Add("format", "xml");
                do
                {
                    if (flag)
                    {
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
                    flag = true;

                    Dictionary<string, string> headers = Client.ListDirectoryHeaders;
                    ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
                    responseXmlString = responseInfo.ResponseXml;
                    ConvertXmlToList(dirs, files, responseXmlString, storageInfo, dirInfo);

                } while ((dirNavs.Count + fileNavs.Count) == 10000);
                StorageListResult results = new StorageListResult();
                results.SubDirs = dirs;
                results.Files = files;
                return files;
            }
            else
            {
                return base.ListFiles(dirInfo);
            }
        }
        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            StorageInfo sInfo = null;
            bool flag = false;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            List<XFileInfo> files = new List<XFileInfo>();
            string responseXmlString = String.Empty;

            if (dirInfo.DataVersion == Data_Version.DocAve5)
            {
                dirInfo.LowName = string.Empty;
                SystemLocation = string.Empty;
                sInfo = dirInfo.Clone();

                sInfo.LowName = PathUtil.CombinePath(sInfo.HighName, sInfo.LowName);
                sInfo.HighName = SystemLocation;

                if (sInfo.LowName.Equals("\\"))
                {
                    sInfo.LowName = "";
                }
                sInfo.LowName = sInfo.LowName.Replace('\\', '/').TrimEnd('/').TrimStart('/') + "/";
            }
            else
            {
                sInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            }
            CheckState(sInfo.HighName);
            string urlWithoutQueryParms = Client.BuildURLWithOutQueryParams(sInfo.HighName);

            Dictionary<string, string> queryParams = Client.ListDirectoryQueryParams;
            if (!string.IsNullOrEmpty(sInfo.LowName)
                && !"/".Equals(sInfo.LowName, StringComparison.CurrentCultureIgnoreCase)
                && dirInfo.DataVersion == Data_Version.DocAve6)
            {
                queryParams.Add("prefix", sInfo.LowName);
            }
            queryParams.Add("format", "xml");

            do
            {
                if (flag)
                {
                    if (responseXmlString.EndsWith("</" + "SUBDIR".ToLower(CultureInfo.InvariantCulture) + "></container>", StringComparison.OrdinalIgnoreCase))
                    {
                        string tempName = dirs[dirs.Count - 1].Name;
                        queryParams["marker"] = tempName;
                    }
                    else
                    {
                        string tempName = files[files.Count - 1].Name;
                        queryParams["marker"] = tempName;
                    }
                }
                flag = true;

                Dictionary<string, string> headers = Client.ListDirectoryHeaders;
                ResponseInfo responseInfo = null;

                if (dirInfo.DataVersion == Data_Version.DocAve5)
                {
                    responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, new Dictionary<string, string>(), headers });
                    responseXmlString = responseInfo.ResponseXml;
                    string[] strs = responseXmlString.Split('\n');
                    GetD5DirFile(dirInfo, sInfo, strs, dirs);
                }
                else
                {
                    responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
                    responseXmlString = responseInfo.ResponseXml;
                    ConvertXmlToList(dirs, files, responseXmlString, sInfo, dirInfo);
                }
            } while ((dirs.Count + files.Count) == 10000);
            StorageListResult results = new StorageListResult();
            results.SubDirs = dirs;
            results.Files = files;
            return results;
        }

        public void GetD5DirFile(StorageInfo dirInfo, StorageInfo sInfo, string[] strs, List<XDirectoryInfo> dirs)
        {
            foreach (string name in strs)
            {
                if (sInfo.IsLoadFirstLevel)
                {
                    if (name.Contains(":"))
                    {
                        string[] temps = name.Split(':');
                        CloudDirectoryInfo dir = new CloudDirectoryInfo(dirInfo.HighPlusLowName, temps[0]);
                        dirs.Add(dir);
                    }
                    else
                    {
                        CloudDirectoryInfo dir = new CloudDirectoryInfo(dirInfo.HighPlusLowName, name);
                        dirs.Add(dir);
                    }
                }
                else
                {
                    string tempName = name.Replace(":", "/");
                    if (tempName.Contains(sInfo.HighPlusLowName.Substring(0, sInfo.HighPlusLowName.Length - 1)) && !tempName.Equals(sInfo.HighPlusLowName.Substring(0, sInfo.HighPlusLowName.Length - 1)))
                    {
                        string temp = tempName.Substring(sInfo.HighPlusLowName.Length);
                        if (!string.IsNullOrEmpty(temp))
                        {
                            if (temp.Contains("/"))
                            {
                                CloudDirectoryInfo dir = new CloudDirectoryInfo(dirInfo.HighPlusLowName, temp.Substring(0, temp.IndexOf("/", StringComparison.CurrentCulture)));
                                dirs.Add(dir);
                            }
                            else
                            {
                                CloudDirectoryInfo dir = new CloudDirectoryInfo(dirInfo.HighPlusLowName, temp);
                                dirs.Add(dir);
                            }
                        }
                    }
                }
            }
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            StorageInfo sInfo = Preproccess2DirectoryStorageInfo(dirInfo);
            AbstractCloudSystemWrapper abs = new RackspaceSystemWrapper(this, Client);

            abs.GetListSubDirectoriesAndFilesCount(sInfo);

            dirs = new ArrayListSubDirsWrapper(abs);
            files = new ArrayListFilesWrapper(abs);

            bool flag = false;
            List<XDirectoryInfo> dirsList = new List<XDirectoryInfo>();
            List<XFileInfo> filesList = new List<XFileInfo>();

            string responseXmlString = String.Empty;
            List<XPathNavigator> fileNavs = new List<XPathNavigator>();
            List<XPathNavigator> dirNavs = new List<XPathNavigator>();


            CheckState(sInfo.HighName);
            string urlWithoutQueryParms = Client.BuildURLWithOutQueryParams(sInfo.HighName);
            Dictionary<string, string> queryParams = Client.ListDirectoryQueryParams;
            if (!string.IsNullOrEmpty(sInfo.LowName) && !"/".Equals(sInfo.LowName, StringComparison.CurrentCultureIgnoreCase))
            {
                queryParams.Add("prefix", sInfo.LowName);
            }
            queryParams.Add("format", "xml");
            if (flag)
            {
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
            flag = true;

            Dictionary<string, string> headers = Client.ListDirectoryHeaders;
            ResponseInfo responseInfo = Client.ListObjects(urlWithoutQueryParms, queryParams, headers);
            responseXmlString = responseInfo.ResponseXml;
            ConvertXmlToList(dirsList, filesList, responseXmlString, sInfo, dirInfo);
            abs.ListResultsToArrayList(dirsList, filesList, (ArrayList)dirs, (ArrayList)files);
            dirs.SetState(responseInfo, queryParams,
            urlWithoutQueryParms, headers, sInfo, dirInfo);

            files.SetState(responseInfo, queryParams,
            urlWithoutQueryParms, headers, sInfo, dirInfo);

            StorageListResultSafety results = new StorageListResultSafety();
            results.SubDirs = dirs;
            results.Files = files;
            return results;
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
            List<XPathNavigator> navs = Client.FirstStepAnalyzeXML(responseXmlString, "container/object");
            XPathNavigator singleNav;
            XPathNavigator isFileOrFolder;
            string name;
            long size;
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                size = 0;
                singleNav = nav.SelectSingleNode("name");
                isFileOrFolder = nav.SelectSingleNode("content_type");
                if (singleNav != null)
                {
                    name = singleNav.Value;

                    singleNav = nav.SelectSingleNode("bytes");
                    if (singleNav != null)
                    {
                        size = singleNav.ValueAsLong;
                    }

                    if ((name.EndsWith("/", StringComparison.OrdinalIgnoreCase) || isFileOrFolder.Value.EndsWith("directory", StringComparison.OrdinalIgnoreCase)) && size == 0)
                    {
                        name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
                        if (!string.IsNullOrEmpty(name))
                        {
                            dir = new CloudDirectoryInfo(storageInfo.HighPlusLowName, name);
                            dir.IsExists = true;
                            dirs.Add(dir);
                        }
                    }
                    else
                    {
                        name = name.RemoveFirst(dirInfo.LowName);
                        if (!string.IsNullOrEmpty(name))
                        {
                            file = new CloudFileInfo(storageInfo.HighPlusLowName, name, size);
                            files.Add(file);
                        }
                    }
                }
            }

            navs = Client.FirstStepAnalyzeXML(responseXmlString, "container/" + "SUBDIR".ToLower(CultureInfo.InvariantCulture));
            foreach (XPathNavigator nav in navs)
            {
                singleNav = nav.SelectSingleNode("name");
                if (singleNav != null)
                {
                    name = singleNav.Value;
                    name = name.RemoveFirst(dirInfo.LowName).TrimEnd('/');
                    if (!string.IsNullOrEmpty(name))
                    {
                        dir = new CloudDirectoryInfo(storageInfo.HighPlusLowName, name);
                        var dto = dirs.Find(c => c.LowName.Equals(name, StringComparison.OrdinalIgnoreCase));
                        if (dto == null)
                        {
                            dirs.Add(dir);
                        }
                    }
                }
            }
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            return ListSubDirectoriesAndFiles(dirInfo).SubDirs;
        }

        public override bool AddMetadata(StorageInfo storageInfo, Dictionary<string, string> writerHeaders)
        {
            if (Client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.Close))
            {
                return true;
            }
            else if (Client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.CustomizedOnly))
            {
                foreach (KeyValuePair<string, string> entry in Client.CloudOpenParam.CustomizedMetaData)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders["X-Object-Meta-" + entry.Key] = entry.Value;
                    }
                }
            }
            else if (Client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.DocAveOnly))
            {
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
            }
            else if (Client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.SupportAll))
            {
                foreach (KeyValuePair<string, string> entry in Client.CloudOpenParam.CustomizedMetaData)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders["X-Object-Meta-" + entry.Key] = entry.Value;
                    }
                }
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    if (!string.IsNullOrEmpty(entry.Value) && entry.Value.Length < 256)
                    {
                        writerHeaders[entry.Key] = entry.Value;
                    }
                }
            }
            else
            {
                throw new Exception("unKnown Customized Mode");
            }
            return base.AddMetadata(storageInfo);
        }

        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            Dictionary<string, string> preparedMetaInfos = new Dictionary<string, string>();
            foreach (KeyValuePair<string, string> entry in info.MetaInfos)
            {
                preparedMetaInfos["X-Object-Meta-" + entry.Key] = entry.Value != null ? Client.HttpClient.Encode(entry.Value) : entry.Value;
            }
            info.MetaInfos = preparedMetaInfos;
            return base.OpenStream(info, fileMode);
        }
    }
}
