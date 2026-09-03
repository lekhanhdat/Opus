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



namespace AvePoint.Media.Storage.Cloud.Atmos
{
    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Reflection;
    using System.Text;
    using System.Xml.XPath;
    using AvePoint.Media.Storage.Cloud.Common;
    using AvePoint.Media.Storage.Util;
    #endregion

    class AtmosSystem : CloudSystem
    {
        private AtmosOpenParameter openParam;
        private StorageLogger logger = StorageLogger.GetInstance(typeof(AtmosSystem));
        public ArrayListSubDirsWrapper dirs { set; get; }
        public ArrayListFilesWrapper files { set; get; }

        public override string Type
        {
            get
            {
                return "AtmosSystem";
            }
        }

        public AtmosSystem(string xriStr, string initMode, AbstractXSystem parentSystem)
            : base(xriStr, parentSystem)
        {
            Client = new AtmosClient();
            logger = StorageLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

            if (initMode.Equals(XSystemConst.MODE_NOW_INITSYSTEM))
            {
                ConstructCloudOpenParameter(xriStr);
                Client.InitConfig(openParam);
                Client.HttpClient.CurrentSystem = this;
                base.SystemHealth = XSystemHealth.Unknown;
            }
            this.Open();
        }

        void ConstructCloudOpenParameter(string xriStr)
        {
            openParam = new AtmosOpenParameter();
            base.ConstructCloudOpenParameter(xriStr, openParam);

            if (XriObject.Params.ContainsKey(XRIParameterKeys.CLOUD_TYPE_KEY))
            {
                openParam.CType = XriObject.Params[XRIParameterKeys.CLOUD_TYPE_KEY];
            }
            if (openParam.CType == null && XriObject.Params.ContainsKey(XRIParameterKeys.CLOUD_TYPE_KEY.ToLower(CultureInfo.InvariantCulture)))
            {
                openParam.CType = XriObject.Params[XRIParameterKeys.CLOUD_TYPE_KEY.ToLower(CultureInfo.InvariantCulture)];
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.OBJECT_ATMOS_CHECKSUM_UPLOAD))
            {
                openParam.EnableChecksumForCreate = true;
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.OBJECT_ATMOS_CHECKSUM_DOWNLOAD))
            {
                openParam.VerifyChecksumAtRead = true;
            }
            if (string.IsNullOrEmpty(openParam.CType))
            {
                if ("atmos_vim".Equals(XriObject.VIM))
                {
                    openParam.CType = XRIParameterKeys.CTYRE_ATMOS;
                }
                else if ("att_vim".Equals(XriObject.VIM))
                {
                    openParam.CType = XRIParameterKeys.CTYRE_ATT;
                }
                else
                {
                    throw new Exception("Unknown cloud type");
                }
            }
            if (XriObject.Params.ContainsKey(XRIParameterKeys.AccessPoinyKey))
            {
                openParam.AccessPoint = XriObject.Params[XRIParameterKeys.AccessPoinyKey];
            }
        }

        protected override void SetSystemDescription()
        {
            if (XriObject.VIM.Equals("atmos_vim"))
            {
                Properties[SystemPropertyKeys.SystemDescriptionKey] = "EMC Atmos, AccessPoint: " + openParam.AccessPoint + ", Full Token ID:" + openParam.UserName + ", Root Folder: " + SystemLocation;
            }
            else
            {
                Properties[SystemPropertyKeys.SystemDescriptionKey] = "AT&T Synaptic, Full Token ID: " + openParam.UserName + ", Root Folder: " + SystemLocation;
            }
            List<string> keys = new List<string>();
            keys.Add(this.openParam.SystemLocation);
            keys.Add(this.openParam.UserName);
            List<string> securityKeys = new List<string>();
            securityKeys.Add(this.openParam.Password);
            this.SystemKey = GenerateSystemKey(keys, securityKeys);
        }

        public override StorageResult CommitStream(System.IO.Stream commitStream, StorageInfo info)
        {
            if (openParam.EnableChecksumForCreate)
            {
                using (var sha = new SHA1Util())
                {
                    info.checksum = sha.GetChecksumStringForBlob(commitStream);
                    logger.Debug("commit file {0} SHA1 value is {1}", info.HighPlusLowName, info.checksum);
                }
            }
            return base.CommitStream(commitStream, info);
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "sha")]
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "x-emc-wschecksum")]
        public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        {
            string tempStr = info.HighName;
            StorageInfo infoTemp = PreproccessStorageInfo(info);
            CheckState(infoTemp.HighName);
            string fullURL = Client.BuildObjectAbsoluteURL(infoTemp.HighName, infoTemp.LowName);
            switch (fileMode)
            {
                case FileMode.Create:
                case FileMode.CreateNew:
                case FileMode.OpenOrCreate:
                case FileMode.Truncate:
                case FileMode.Append:
                    Dictionary<string, string> writerHeaders = Client.OpenStreamWriteModeHeaders;
                    writerHeaders["Content-Type"] = "DOCAVE/data".ToLower(CultureInfo.InvariantCulture);
                    writerHeaders["Content-Length"] = infoTemp.Length.ToString();
                    if (!string.IsNullOrEmpty(info.checksum))
                    {
                        writerHeaders["x-emc-wschecksum"] = "sha1/" + info.Length + "/" + info.checksum;
                    }
                    AddMetadata(info, writerHeaders);
                    XStream writeStream = (HttpUploadStream)Client.Invoke("OpenObjectForWrite", new object[] { fullURL, writerHeaders });
                    writeStream.System = this;
                    writeStream.Info = info;
                    writeStream.MaxRetryCount = Client.CloudOpenParam.MaxRetryCount;
                    this.Written = true;
                    if (info.DataVersion == Data_Version.DocAve5)
                    {
                        info.HighName = tempStr;
                    }
                    return writeStream;
                case FileMode.Open:
                    XStream readStream = null;
                    try
                    {
                        var readerHeaders = Client.OpenStreamReadModeHeaders;
                        if (infoTemp.Length > 0)
                        {
                            readerHeaders["Range"] = "bytes=" + infoTemp.Offset + "-" + (infoTemp.Length + infoTemp.Offset);
                        }
                        readStream = (XStream)Client.Invoke("OpenObjectForRead", new object[] { fullURL, readerHeaders });
                        readStream.System = this;
                        var checksum = ((HttpDownloadStream)readStream).Response.Headers.Get("x-emc-wschecksum");
                        if (this.openParam.VerifyChecksumAtRead && !string.IsNullOrEmpty(checksum))
                        {
                            var responseChecksum = AtmosUtils.GetChecksumStringForDownload(checksum);
                            info.checksum = responseChecksum;
                            logger.Debug("Get download stream successful, and the checksum value is {0}", responseChecksum);
                        }
                        if (infoTemp.Length <= 0)
                        {
                            info.Length = ((HttpDownloadStream)readStream).InnerLength;
                        }
                        readStream.Info = info;
                        readStream.MaxRetryCount = Client.CloudOpenParam.MaxRetryCount;
                        if (info.DataVersion == Data_Version.DocAve5)
                        {
                            info.HighName = tempStr;
                        }
                    }
                    catch (Exception e)
                    {
                        this.logger.Error("Failed to open the file. File Path: {0}.", fullURL);
                        logger.Error(e.Message, e);
                        throw;
                    }
                    return readStream;
                default:
                    throw new NotSupportedException("Unsupported file mode " + fileMode);
            }

        }

        public override StorageDeleteResult DeleteDirectory(StorageInfo info)
        {
            var dr = new StorageDeleteResult();
            if (DirectoryExists(info))
            {
                dr = RecursiveDelete(info);
                if (info.IsDeleteParentFolder)
                {
                    var infoTemp = PreproccessStorageInfo(info);
                    var directoryPaths = new List<String>();
                    var directoryNames = infoTemp.LowName.Split(new[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
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
                            Client.Invoke("DeleteObject", new object[] { infoTemp.HighName, directoryPaths[index] });
                        }
                        else
                        {
                            break;
                        }
                    }
                }
            }
            else
            {
                logger.Warn("The directory you want to delete is not exist, path:{0}", info.HighPlusLowName);
            }
            dr.IsDeleted = true;
            Deletion = true;
            return dr;
        }

        private StorageDeleteResult RecursiveDelete(StorageInfo info)
        {
            var deletedSize = default(Int64);
            var dirAndFiles = ListSubDirectoriesAndFiles(info);
            var infoTemp = PreproccessStorageInfo(info);
            foreach (var file in dirAndFiles.Files)
            {
                deletedSize += file.FileSize;
                this.DeleteFile(new StorageInfo { HighName = file.HighName, LowName = file.Name });
            }
            foreach (XDirectoryInfo dir in dirAndFiles.SubDirs)
            {
                deletedSize += RecursiveDelete(new StorageInfo { HighName = dir.HighName, LowName = dir.Name }).DeletedFileSize;
            }
            Client.Invoke("DeleteObject", new object[] { infoTemp.HighName, infoTemp.LowName });
            return new StorageDeleteResult { IsDeleted = true, DeletedFileSize = deletedSize };
        }

        public override StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo)
        {
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            AbstractCloudSystemWrapper abs = new AtmosSystemWrapper(this, Client);
            abs.GetListSubDirectoriesAndFilesCount(dirInfoTemp);
            dirs = new ArrayListSubDirsWrapper(abs);
            files = new ArrayListFilesWrapper(abs);
            CheckState(dirInfoTemp.HighName);
            string urlWithoutQueryParms = Client.BuildObjectAbsoluteURL(dirInfoTemp.HighName, dirInfoTemp.LowName);
            Dictionary<string, string> queryParams = Client.ListDirectoryQueryParams;
            List<XDirectoryInfo> dirsList = new List<XDirectoryInfo>();
            List<XFileInfo> filesList = new List<XFileInfo>();
            Dictionary<string, string> headers = Client.ListDirectoryHeaders;
            headers["X-EMC-SYSTEM-TAGS".ToLower(CultureInfo.InvariantCulture)] = "ATIME,SIZE".ToLower(CultureInfo.InvariantCulture);
            ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
            string responseXmlString = responseInfo.ResponseXml;
            ConvertXmlToList(dirsList, filesList, responseXmlString, dirInfoTemp);
            abs.ListResultsToArrayList(dirsList, filesList, (ArrayList)dirs, (ArrayList)files);
            dirs.SetState(responseInfo, queryParams,
            urlWithoutQueryParms, headers, null, dirInfo);
            files.SetState(responseInfo, queryParams,
            urlWithoutQueryParms, headers, null, dirInfo);
            StorageListResultSafety results = new StorageListResultSafety();
            results.SubDirs = dirs;
            results.Files = files;
            return results;

        }

        public override StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo)
        {
            Boolean frist = true;
            string token = string.Empty;
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            string urlWithoutQueryParms = Client.BuildObjectAbsoluteURL(dirInfoTemp.HighName, dirInfoTemp.LowName);
            Dictionary<string, string> queryParams = Client.ListDirectoryQueryParams;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();
            List<XFileInfo> files = new List<XFileInfo>();
            while (frist || token != string.Empty)
            {
                Dictionary<string, string> headers = Client.ListDirectoryHeaders;
                headers["X-EMC-SYSTEM-TAGS".ToLower(CultureInfo.InvariantCulture)] = "ATIME,SIZE".ToLower(CultureInfo.InvariantCulture);
                if (!frist)
                {
                    headers["X-EMC-TOKEN".ToLower(CultureInfo.InvariantCulture)] = token;
                    token = string.Empty;
                }
                frist = false;
                ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
                string responseXmlString = responseInfo.ResponseXml;
                if (responseInfo.Headers.Count > 0)
                {
                    token = responseInfo.Headers["X-EMC-TOKEN".ToLower(CultureInfo.InvariantCulture)];
                }
                ConvertXmlToList(dirs, files, responseXmlString, dirInfoTemp);
            }
            StorageListResult results = new StorageListResult();
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
        /// <param name="storageInfo">The storage info.</param>
        public void ConvertXmlToList(List<XDirectoryInfo> dirs,
                                    List<XFileInfo> files,
                                    String responseXmlString,
                                    StorageInfo storageInfo)
        {

            CloudDirectoryInfo dir;
            CloudFileInfo file;

            responseXmlString = responseXmlString.Replace("XMLNS='HTTP://WWW.EMC.COM/COS/'".ToLower(CultureInfo.InvariantCulture), "");
            responseXmlString = responseXmlString.Replace("XMLNS=\"HTTP://WWW.EMC.COM/COS/\"".ToLower(CultureInfo.InvariantCulture), "");
            List<XPathNavigator> navs = Client.FirstStepAnalyzeXML(responseXmlString, "ListDirectoryResponse/DirectoryList/DirectoryEntry");
            XPathNavigator singleNav;
            string name;
            string fileType;
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                singleNav = nav.SelectSingleNode("FileType");
                if (singleNav != null)
                {
                    fileType = singleNav.Value;
                    if ("directory".Equals(fileType, StringComparison.CurrentCultureIgnoreCase))
                    {
                        singleNav = nav.SelectSingleNode("Filename");
                        if (singleNav != null)
                        {
                            name = singleNav.Value;
                            if (!string.IsNullOrEmpty(name))
                            {
                                dir = new CloudDirectoryInfo(storageInfo.LowName, name);
                                dirs.Add(dir);
                            }
                        }
                    }
                    else
                    {
                        singleNav = nav.SelectSingleNode("Filename");
                        if (singleNav != null)
                        {
                            name = singleNav.Value;
                            if (!string.IsNullOrEmpty(name))
                            {
                                file = new CloudFileInfo(storageInfo.LowName, name, -1);
                                file.System = this;
                                files.Add(file);
                            }
                        }
                    }
                }
            }
        }

        public override List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo)
        {
            StorageInfo dirInfoTemp = Preproccess2DirectoryStorageInfo(dirInfo);
            CheckState(dirInfoTemp.HighName);
            string urlWithoutQueryParms = Client.BuildObjectAbsoluteURL(dirInfoTemp.HighName, dirInfoTemp.LowName);
            Dictionary<string, string> queryParams = Client.ListDirectoryQueryParams;
            Dictionary<string, string> headers = Client.ListDirectoryHeaders;
            headers["X-EMC-SYSTEM-TAGS".ToLower(CultureInfo.InvariantCulture)] = "ATIME,SIZE".ToLower(CultureInfo.InvariantCulture);
            //ResponseInfo responseInfo = client.ListObjects(urlWithoutQueryParms, queryParams, headers);
            ResponseInfo responseInfo = (ResponseInfo)Client.Invoke("ListObjects", new object[] { urlWithoutQueryParms, queryParams, headers });
            string responseXmlString = responseInfo.ResponseXml;
            responseXmlString = responseXmlString.Replace("XMLNS='HTTP://WWW.EMC.COM/COS/'".ToLower(CultureInfo.InvariantCulture), "");

            CloudDirectoryInfo dir;
            List<XDirectoryInfo> dirs = new List<XDirectoryInfo>();

            List<XPathNavigator> navs = Client.FirstStepAnalyzeXML(responseXmlString, "ListDirectoryResponse/DirectoryList/DirectoryEntry");
            XPathNavigator singleNav;
            string name;
            string fileType;
            foreach (XPathNavigator nav in navs)
            {
                name = null;
                singleNav = nav.SelectSingleNode("FileType");
                if (singleNav != null)
                {
                    fileType = singleNav.Value;
                    if ("directory".Equals(fileType, StringComparison.CurrentCultureIgnoreCase))
                    {
                        singleNav = nav.SelectSingleNode("Filename");
                        if (singleNav != null)
                        {
                            name = singleNav.Value;
                            if (!string.IsNullOrEmpty(name))
                            {
                                dir = new CloudDirectoryInfo(dirInfo.HighPlusLowName, name);
                                dirs.Add(dir);
                            }
                        }
                    }
                }
            }
            return dirs;
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
            info.HighName = info.HighName.ToLower(CultureInfo.InvariantCulture);
            info.LowName = info.LowName.ToLower(CultureInfo.InvariantCulture);
            return info;
        }

        public override StorageInfo D5PreproccessStorageInfo(StorageInfo dirInfo)
        {
            if (string.IsNullOrEmpty(dirInfo.LowName))
            {
                dirInfo.LowName = string.Empty;
            }
            StorageInfo info = dirInfo.Clone();
            SystemLocation = "/";

            info.LowName = PathUtil.CombinePath(info.HighName, info.LowName);
            info.HighName = SystemLocation;

            if (info.LowName.Equals("\\"))
            {
                info.LowName = "";
            }
            info.LowName = info.LowName.Replace('\\', '/').TrimStart('/');
            info.HighName = info.HighName.ToLower(CultureInfo.InvariantCulture);
            info.LowName = info.LowName.ToLower(CultureInfo.InvariantCulture);
            return info;
        }

        //public override XStream OpenStream(StorageInfo info, FileMode fileMode)
        //{
        //    Dictionary<string, string> preparedMetaInfos = new Dictionary<string, string>();
        //    StringBuilder listableMetas = new StringBuilder();
        //    foreach (KeyValuePair<string, string> entry in info.MetaInfos)
        //    {
        //        listableMetas.Append(entry.Key);
        //        listableMetas.Append("=");
        //        listableMetas.Append(entry.Value != null ? client.HttpClient.Encode(entry.Value) : entry.Value);
        //        listableMetas.Append(",");
        //    }
        //    if (listableMetas.Length > 0)
        //    {
        //        preparedMetaInfos["X-EMC-LISTABLE-META".ToLower(CultureInfo.InvariantCulture)] = listableMetas.ToString().TrimEnd(',');
        //    }
        //    info.MetaInfos = preparedMetaInfos;
        //    return base.OpenStream(info, fileMode);
        //}
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "fma")]
        public override bool AddMetadata(StorageInfo storageInfo, Dictionary<string, string> writerHeaders)
        {
            StringBuilder listableMetas = new StringBuilder();
            if (Client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.Close))
            {
                return true;
            }
            else if (Client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.CustomizedOnly))
            {
                foreach (KeyValuePair<string, string> entry in Client.CloudOpenParam.CustomizedMetaData)
                {
                    listableMetas.Append(entry.Key);
                    listableMetas.Append("=");
                    listableMetas.Append(entry.Value != null ? Client.HttpClient.Encode(entry.Value) : entry.Value);
                    listableMetas.Append(",");
                }
            }
            else if (Client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.DocAveOnly))
            {
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    listableMetas.Append(entry.Key);
                    listableMetas.Append("=");
                    listableMetas.Append(entry.Value != null ? Client.HttpClient.Encode(entry.Value) : entry.Value);
                    listableMetas.Append(",");
                }
                listableMetas.Append("fma_size");
                listableMetas.Append("=");
                listableMetas.Append(storageInfo.Length / 1024);
                listableMetas.Append(",");
            }
            else if (Client.CloudOpenParam.CustomizedMetaMode.Equals(CustomizedMode.SupportAll))
            {
                foreach (KeyValuePair<string, string> entry in Client.CloudOpenParam.CustomizedMetaData)
                {
                    listableMetas.Append(entry.Key);
                    listableMetas.Append("=");
                    listableMetas.Append(entry.Value != null ? Client.HttpClient.Encode(entry.Value) : entry.Value);
                    listableMetas.Append(",");
                }
                foreach (KeyValuePair<string, string> entry in storageInfo.MetaInfos)
                {
                    listableMetas.Append(entry.Key);
                    listableMetas.Append("=");
                    listableMetas.Append(entry.Value != null ? Client.HttpClient.Encode(entry.Value) : entry.Value);
                    listableMetas.Append(",");
                }
                listableMetas.Append("fma_size");
                listableMetas.Append("=");
                listableMetas.Append(storageInfo.Length / 1024);
                listableMetas.Append(",");
            }
            else
            {
                throw new Exception("unKnown Customized Mode");
            }
            if (listableMetas.Length > 0)
            {
                writerHeaders["X-EMC-LISTABLE-META"] = listableMetas.ToString().TrimEnd(',');
            }
            return true;
        }
    }
}
