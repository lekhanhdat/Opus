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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Xml;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.GranularRestore.Object;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.FileTransfer;
using AvePoint.GCommon.Contract.Media.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Item.Common;

namespace AvePoint.Item.Restore
{
    public class AveSiteRestore : AveRestoreBase
    {
        private IVersionControler versionControler;
        private int mFileNumber = 1;

        public AveSiteRestore()
        {
            versionControler = new D6VersionControler();
        }

        public AveSiteRestore(ProductVersion productVersion)
        {
            versionControler = VersionControlerFactory.GetVersionControler(productVersion);
        }

        public override void Init()
        {
            base.Init();
        }

        public override void RestoreSite(RestoreContentDto aveSiteDto)
        {
            log.Log(AveLogLevel.INFO, string.Format("Begin to restore {0}", aveSiteDto.Name));
            long dataSize = 0;
            var reportDto = new AveRestoreReportDto { Type = aveSiteDto.Type.ToString(), Path = aveSiteDto.Name, Title = aveSiteDto.Name };
            string tempFilePath = Path.Combine(Config.TempPath, Config.JobId + this.mFileNumber + ".rar");
            AveStsadmSite site = null;
            try
            {
                var attribute = ReceiveSiteFiles(aveSiteDto.Name, tempFilePath, out dataSize);
                site = new AveStsadmSite(Config, attribute);
                site.SPRestoreSite(aveSiteDto, tempFilePath);
            }
            catch (Exception e)
            {
                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.DataProtection_SP2010_GranularRestore_SiteCollectionLevel, new EventIds.SharePoint.RestoreSiteCollectionFailedEventMessage(aveSiteDto.Name, e));
                reportDto.Status = RestoreStatus.Failed;
                reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ASAWRRestoreSiteError.ToString(), @"Looks up a localized string similar to An error occurred while restoring the site collection. Site Collection Name: {0}, Error: {1}..", aveSiteDto.Name, e.Message);
            }
            finally
            {
                DeleteSiteFiles(tempFilePath);
                reportDto.Size = dataSize;
                reportDto.SourcePath = aveSiteDto.SrcUrl;
                reportDto.Title = site == null ? reportDto.Title : site.Title;
                reportDto.SetOption(aveSiteDto.RestoreOption.mAveRestoreMode,
                    site == null ? default(bool?) : site.IsSiteExist, reportDto.Status);
                AddReport(reportDto);
            }
        }

        private AveSiteAttributeInfo ReceiveSiteFiles(string destSiteUrl, string tempFilePath,
                                      out long dataSize)
        {
            AveSiteAttributeInfo info = new AveSiteAttributeInfo();
            FileStream fileStream = null;
            BinaryWriter binaryWriter = null;
            try
            {
                log.Info(@"Looks up a localized string similar to Begin receiving site data. URL: {0}.", destSiteUrl);
                dataSize = 0;
                info.WebAppUrl = AveItemRestoreUtility.GetWebAppUrl(Config.ObjectModelFactory, destSiteUrl);

                this.mFileNumber++;
                fileStream = new FileStream(tempFilePath, FileMode.Create, FileAccess.ReadWrite);
                binaryWriter = new BinaryWriter(fileStream);
                info.OwnerLogin = versionControler.GetOwnerLogin(FileReceiver);
                var byteLockFlag = new byte[10];
                FileReceiver.ReadBytes(byteLockFlag, 0, 10);
                string strLockFlag = Encoding.UTF8.GetString(byteLockFlag);
                log.Log(AveLogLevel.DEBUG, string.Format("Lock flag:{0}", strLockFlag));

                if (strLockFlag.Equals("AVEPOINT#$"))
                {
                    string srcHostHead;
                    var buffer4 = new byte[4];
                    //get the siteinfo xml byte[] length
                    FileReceiver.ReadBytes(buffer4, 0, 4);
                    int byteLenSiteInfo = BitConverter.ToInt32(buffer4, 0);
                    var srcSiteInfo = new byte[byteLenSiteInfo];
                    //get the webinfo xml
                    FileReceiver.ReadBytes(srcSiteInfo, 0, byteLenSiteInfo);
                    string srcSiteInfoXml = Encoding.UTF8.GetString(srcSiteInfo);

                    var xdoc = new XmlDocument();
                    xdoc.LoadXml(srcSiteInfoXml);
                    info.LockSuccess = xdoc.DocumentElement.GetAttribute("needUnlock").Equals("true", StringComparison.OrdinalIgnoreCase);
                    info.ReadState = xdoc.DocumentElement.GetAttribute("readState").Equals("true", StringComparison.OrdinalIgnoreCase);
                    info.WriteState = xdoc.DocumentElement.GetAttribute("writeState").Equals("true", StringComparison.OrdinalIgnoreCase);
                    srcHostHead = xdoc.DocumentElement.GetAttribute("hostHead");
                    //If restore out of place , there is no need to get the virtualUrl from attribute xml.
                    if (srcHostHead.Equals("true", StringComparison.OrdinalIgnoreCase) && Config.RestoreType != RestoreType.OutOfPlace)
                    {
                        info.WebAppUrl = xdoc.DocumentElement.GetAttribute("VirtualServer"); //??
                    }
                }
                else
                {
                    binaryWriter.Write(byteLockFlag, 0, 10);
                }

                const int size = 64 * 1024;
                var temp = new byte[size];
                while (true)
                {
                    int len = FileReceiver.ReadBytes(temp, 0, size);
                    if (len <= 0)
                    {
                        break;
                    }
                    dataSize += len;
                    binaryWriter.Write(temp, 0, len);
                }
                FileReceiver.GetFileTail();
                if (!File.Exists(tempFilePath))
                {
                    throw new FileNotFoundException("Can't find the file received from media.", tempFilePath);
                }
            }
            finally
            {
                if (binaryWriter != null)
                {
                    binaryWriter.Close();
                }
                if (fileStream != null)
                {
                    fileStream.Close();
                }
            }
            return info;

        }

        private void DeleteSiteFiles(string tempFilePath)
        {
            try
            {
                if (!File.Exists("C:\\AveKeepTempFile.txt") && File.Exists(tempFilePath))
                {
                    File.Delete(tempFilePath);
                }
            }
            catch (Exception ex)
            {
                log.Warn("Looks up a localized string similar to Deleted temp file failed, and the program will try again. \r\n{0},\r\n{1},\r\n{2}.", ex.ToString(), ex.Source, ex.StackTrace);
                bool flag = true;
                for (int i = 0; i < 30; i++)
                {
                    try
                    {
                        File.Delete(tempFilePath);
                        //mJob.LogEvent("Delete File " + FileUrl);
                        flag = false;
                        break;
                    }
                    catch (Exception e)
                    {
                        log.Warn("Looks up a localized string similar to Deleted a temp file failed. No.: {0},\r\n{1},\r\n{2}, \r\n{3}.", i.ToString(), e.ToString(), e.Source, e.StackTrace);
                        Thread.Sleep(10000);
                    }
                }
                if (flag)
                {
                    log.Warn(@"Looks up a localized string similar to Cannot delete the temp file. Error Message: {0}.", ex.ToString());
                }
            }
        }

        #region ===== 用于区分D5和D6数据差异 ======
        /// <summary>
        /// 用于区分D5和D6数据差异
        /// </summary>
        interface IVersionControler
        {
            /// <summary>
            /// D5不备份Site Collection Owner
            /// </summary>
            /// <param name="fileReceiver"></param>
            /// <returns></returns>
            string GetOwnerLogin(IFileReceiver fileReceiver);
        }

        private static class VersionControlerFactory
        {
            public static IVersionControler GetVersionControler(ProductVersion version)
            {
                IVersionControler versionControler;
                switch (version)
                {
                    case ProductVersion.Product6X:
                        versionControler = new D6VersionControler();
                        break;
                    case ProductVersion.Product5X:
                        versionControler = new D5VersionControler();
                        break;
                    case ProductVersion.Product4X:
                    case ProductVersion.ProductUnknown:
                        throw new AveException("Invalid product version.");
                    default:
                        throw new AveException("Invalid product version.");
                }
                return versionControler;
            }
        }

        private class D5VersionControler : IVersionControler
        {
            public string GetOwnerLogin(IFileReceiver fileReceiver)
            {
                return string.Empty;
            }
        }

        private class D6VersionControler : IVersionControler
        {
            public string GetOwnerLogin(IFileReceiver fileReceiver)
            {
                var byteSiteOwnerNameLen = new byte[4];
                fileReceiver.ReadBytes(byteSiteOwnerNameLen, 0, 4);
                int siteOwnerNameLen = BitConverter.ToInt32(byteSiteOwnerNameLen, 0);
                var byteSiteOwnerName = new byte[siteOwnerNameLen];
                fileReceiver.ReadBytes(byteSiteOwnerName, 0, siteOwnerNameLen);
                return Encoding.UTF8.GetString(byteSiteOwnerName);
            }
        }
        #endregion

    }

    public class AveWebRestore : AveRestoreBase
    {
        private IVersionControler versionControler;
        private string siteUrl;
        private bool noFileCompression;
        private bool isSiteRestoreFailed = false;
        private string siteOwnerName = string.Empty;
        private readonly List<string> unCheckedLists = new List<string>();
        private readonly Dictionary<string, string> webNameMapping = new Dictionary<string, string>();

        public AveWebRestore(ProductVersion productVersion)
        {
            this.versionControler = VersionControlerFactory.GetVersionControler(productVersion);
        }

        public override void Init()
        {
            //TODO: Initalize
            base.Init();
        }

        public override void RestoreSite(RestoreContentDto aveSiteDto)
        {
            var reportDto = new AveRestoreReportDto { Type = aveSiteDto.Type.ToString(), Path = aveSiteDto.Name, Title = aveSiteDto.Name };
            try
            {
                isSiteRestoreFailed = true;
                this.siteUrl = aveSiteDto.Name.TrimEnd('/');
                AveStsadmSite.CheckSiteLocked(Config.ObjectModelFactory, siteUrl);
                isSiteRestoreFailed = false;
                ReceiveSiteData();
            }
            catch (Exception e)
            {
                log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.DataProtection_SP2010_GranularRestore_SiteLevel, new EventIds.SharePoint.RestoreSiteCollectionFailedEventMessage(aveSiteDto.Name, e));
                reportDto.Status = RestoreStatus.Failed;
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_ASAWRRestoreSiteError.ToString(), RestoreReportResource.Item_ASAWRRestoreSiteError, aveSiteDto.Name, e.Message);
            }
            var title=AveStsadmSite.GetTitle(Config.ObjectModelFactory, siteUrl);
            reportDto.Title = string.IsNullOrEmpty(title) ? reportDto.Title : title;
            reportDto.SourcePath = aveSiteDto.SrcUrl;
            AddReport(reportDto);
        }

        private void ReceiveSiteData()
        {
            var byteSiteOwnerNameLen = new byte[4];
            FileReceiver.ReadBytes(byteSiteOwnerNameLen, 0, 4);
            int siteOwnerNameLen = BitConverter.ToInt32(byteSiteOwnerNameLen, 0);
            var byteSiteOwnerName = new byte[siteOwnerNameLen];
            FileReceiver.ReadBytes(byteSiteOwnerName, 0, siteOwnerNameLen);
            this.siteOwnerName = Encoding.UTF8.GetString(byteSiteOwnerName);

            var byteCompressionInfoLen = new byte[4];
            FileReceiver.ReadBytes(byteCompressionInfoLen, 0, 4);
            int compressionInfoLen = BitConverter.ToInt32(byteCompressionInfoLen, 0);
            var byteCompressionInfo = new byte[compressionInfoLen];
            FileReceiver.ReadBytes(byteCompressionInfo, 0, 20);
            string compressionInfo = Encoding.UTF8.GetString(byteCompressionInfo);
            if (compressionInfo.Equals("NoFileCompression"))
            {
                this.noFileCompression = true;
            }

            const int size = 64 * 1024;
            var temp = new byte[size];
            while (true)
            {
                int len = FileReceiver.ReadBytes(temp, 0, size);
                if (len <= 0)
                {
                    break;
                }
            }
            FileReceiver.GetFileTail();
        }

        public override void RestoreWeb(RestoreContentDto aveWebDto)
        {
            AveStsadmWeb web = null;
            var reportDto = new AveRestoreReportDto { Type = aveWebDto.Type.ToString(), Title = aveWebDto.Name };
            if (aveWebDto.Name.Length > 1 && aveWebDto.Name.StartsWith(AveConstants.ROOT_WEB, StringComparison.Ordinal))
            {
                aveWebDto.Name = aveWebDto.Name.Substring(2);
            }
            if (!aveWebDto.IsChecked)
            {
                unCheckedLists.Add(aveWebDto.Name);
                return;
            }
            if (isSiteRestoreFailed)
            {
                //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(RestoreReportKey.Item_CanNotFindWebParent.ToString(), RestoreReportResource.Item_CanNotFindWebParent, reportDto.Title);
            }
            else
            {
                try
                {
                    aveWebDto.Name = GetAvailableWebNameAndAddToMapping(aveWebDto.Name);
                    string realWebName = aveWebDto.Name.Equals(AveConstants.ROOT_WEB, StringComparison.Ordinal) ? string.Empty : aveWebDto.Name;
                    reportDto.Path = ReportAbsolutePath.GetWebAP(siteUrl, realWebName);
                    long dataSize;
                    string tempFilePath = this.noFileCompression ?
                        Path.Combine(Config.TempPath, Config.JobId + "_" + Guid.NewGuid()) : Config.TempPath;
                    var delFileList = ReceiveWebFiles(tempFilePath, out dataSize);
                    try
                    {
                        string filePath = this.noFileCompression ? tempFilePath : delFileList[0].ToString();
                        web = new AveStsadmWeb(Config, this.siteUrl, this.siteOwnerName);
                        web.SPRestoreWeb(realWebName, filePath, noFileCompression, ref isSiteRestoreFailed);
                    }
                    finally
                    {
                        DeleteWebFiles(delFileList);
                        try
                        {
                            if (this.noFileCompression)
                            {
                                Directory.Delete(tempFilePath);
                            }
                        }
                        catch (Exception ex)
                        {
                            log.Warn(@"Looks up a localized string similar to Deleted temp folder failed, message:{0}.", ex);
                        }
                        AvePoint.Wrapper.Common.ArgumentCheck.CheckNotNull(web);
                        string title = web?.GetTitle(siteUrl, realWebName);
                        if (!string.IsNullOrEmpty(title))
                        {
                            reportDto.Title = title;
                        }
                        reportDto.Size = dataSize;
                    }
                }
                catch (Exception e)
                {
                    log.Log(EventSources.DocAveAgentService, EventCategorys.DocAveAgentService.DataProtection_SP2010_GranularRestore_SiteLevel, new EventIds.SharePoint.RestoreWebFailedEventMessage(aveWebDto.Name, e));
                    reportDto.Status = RestoreStatus.Failed;
                    //reportDto.ErrorMessage = AveWrapperHandleErrorMessage.ConvertErrorMessageToXML(e, RestoreReportKey.Item_AWRRestoreWebError.ToString(), RestoreReportResource.Item_AWRRestoreWebError, aveWebDto.Name, e.Message);
                }
            }
            reportDto.SourcePath = aveWebDto.SrcUrl;
            reportDto.SetOption(aveWebDto.RestoreOption.mAveRestoreMode,
                web == null ? default(bool?) : web.IsWebExist, reportDto.Status);
            AddReport(reportDto);
        }

        private ArrayList ReceiveWebFiles(string tempFilePath, out long dataSize)
        {
            log.Info("Looks up a localized string similar to Begin receiving the web data....");
            var delFileList = new ArrayList();
            dataSize = 0;

            const int size = 64 * 1024;
            var temp = new byte[size];
            int fileCount = 0;
            var buffer4 = new byte[4];

            FileReceiver.ReadBytes(buffer4, 0, 4);
            fileCount = BitConverter.ToInt32(buffer4, 0);
            if (!Directory.Exists(tempFilePath))
            {
                Directory.CreateDirectory(tempFilePath);
            }

            for (int iCount = 0; iCount < fileCount; iCount++)
            {
                FileReceiver.ReadBytes(buffer4, 0, 4);
                int fileNameLen = BitConverter.ToInt32(buffer4, 0);
                var fileNameBytes = new byte[fileNameLen];
                FileReceiver.ReadBytes(fileNameBytes, 0, fileNameLen);
                string fileName = Encoding.UTF8.GetString(fileNameBytes);

                log.Debug("Looks up a localized string similar to File Number: {0}, File Name: {1}.");

                //string filePath = (Config.DefLogin ? Config.JobDir : Config.NetStorePath) + "\\" + fileName;
                string filePath = SecurityUtils.SafeCombinePath(tempFilePath, fileName);
                delFileList.Add(filePath);
                FileReceiver.ReadBytes(buffer4, 0, 4);
                int fileLen = BitConverter.ToInt32(buffer4, 0);
                dataSize += fileLen;
                var fileStream = new FileStream(filePath, FileMode.Create, FileAccess.ReadWrite);
                var binaryWriter = new BinaryWriter(fileStream);
                try
                {
                    int temptimes = fileLen / size;
                    int restsize = fileLen % size;
                    for (int tpt = 0; tpt < temptimes; tpt++)
                    {
                        int len = FileReceiver.ReadBytes(temp, 0, size);
                        binaryWriter.Write(temp, 0, len);
                    }
                    if (restsize != 0)
                    {
                        var bufrest = new byte[restsize];
                        int lenrest = FileReceiver.ReadBytes(bufrest, 0, restsize);
                        binaryWriter.Write(bufrest, 0, lenrest);
                    }
                }
                catch (Exception e)
                {
                    log.Warn("Looks up a localized string similar to File Number: {0}, File Name: {1}.", fileName, e.ToString());
                }
                finally
                {
                    binaryWriter.Close();
                    fileStream.Close();
                }
                if (!File.Exists(filePath))
                {
                    log.Warn("Looks up a localized string similar to {0} does not exist..", filePath);
                    continue;
                }
            }
            log.Info("Looks up a localized string similar to Receiving files finished..");

            FileReceiver.GetFileTail();
            return delFileList;
        }

        private void DeleteWebFiles(ArrayList delFileList)
        {
            if (!File.Exists("C:\\AveKeepTempFile.txt"))
            {
                foreach (object t in delFileList)
                {
                    var filePath = t.ToString();
                    if (filePath.IsNotNullOrEmpty() && File.Exists(filePath))
                    {
                        try
                        {
                            if (log.IsDebugEnabled)
                            {
                                log.Info("Looks up a localized string similar to Deleting {0}....", t.ToString());
                            }

                            File.Delete(t.ToString());
                        }
                        catch (Exception ex)
                        {
                            log.Warn(" Looks up a localized string similar to Deleted file failed.\r\n{0},\r\n{1},\r\n{2}.", ex.ToString(), ex.Source, ex.StackTrace);
                            bool flag = true;
                            for (int i = 0; i < 30; i++)
                            {
                                try
                                {
                                    File.Delete(t.ToString());
                                    log.Warn("Looks up a localized string similar to It will try to delete {0} again..", t.ToString());
                                    flag = false;
                                    break;
                                }
                                catch (Exception e)
                                {
                                    log.Warn("Looks up a localized string similar to Deleted file failed. No.{0},\r\n{1},\r\n{2},\r\n{3}.", i.ToString(), e.ToString(), e.Source, e.StackTrace);
                                    Thread.Sleep(10000);
                                }
                            }
                            if (flag)
                            {
                                throw new Exception("AveSP2010WebRestore Cannot TempFile " + ex);
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Find the available parent of webName if exist.
        /// </summary>
        /// <param name="webName">., a, a/b, a/b/c</param>
        /// <returns>if webName is a/b/c, return value can be
        /// a/b/c, a/c, c, "."</returns>
        private string GetAvailableWebName(string webName)
        {
            if (webName.StartsWith("/", StringComparison.Ordinal))
            {
                throw new ArgumentException("Parameter webName should be \".\" or site related url.\r\nFor http://webApplication/sites/siteCollection/a/b, webName should be a/b.");
            }
            if (webName.Equals(AveConstants.ROOT_WEB, StringComparison.Ordinal))
            {
                return AveConstants.ROOT_WEB;
            }
            string parentWebName = webName.LastIndexOf('/') < 0 ? string.Empty : webName.Remove(webName.LastIndexOf('/'));
            string singleWebName = webName.LastIndexOf('/') < 0 ? webName : webName.Substring(webName.LastIndexOf('/') + 1);
            try
            {
                using (IAveSite site = Config.ObjectModelFactory.CreateSite(this.siteUrl))
                {
                    while (true)
                    {
                        parentWebName = webNameMapping.ContainsKey(parentWebName) ? webNameMapping[parentWebName] : parentWebName; 
                        using (IAveWeb web = site.OpenWeb(parentWebName))
                        {
                            if (web.Exists)
                            {
                                return string.IsNullOrEmpty(parentWebName) ? singleWebName : parentWebName.TrimEnd('/') + '/' + singleWebName;
                            }
                            else if (!versionControler.IsWebInUncheckedLists(unCheckedLists, parentWebName))
                            {
                                throw new Exception(string.Format("Cannot find parent web of {0}.", singleWebName));
                            }
                            int index = parentWebName.LastIndexOf('/');
                            parentWebName = index < 0 ? string.Empty : parentWebName.Remove(index);
                        }
                    }
                }
            }
            catch (FileNotFoundException ex)
            {
                log.Log(AveLogLevel.WARN, string.Format("Site collection does not exist, url:{0}. inner exception:{1}", siteUrl, ex));
                if (!versionControler.IsWebInUncheckedLists(unCheckedLists, AveConstants.ROOT_WEB))
                {
                    throw new FileNotFoundException("Site collection does not exist", siteUrl, ex);
                }
                return AveConstants.ROOT_WEB;
            }
        }

        private string GetAvailableWebNameAndAddToMapping(string webName)
        {
            string realName = GetAvailableWebName(webName);
            if (!string.IsNullOrEmpty(realName)
                && !string.Equals(realName, webName, StringComparison.OrdinalIgnoreCase))
            {
                webNameMapping.Add(webName, realName);
            }
            return realName;
        }

        interface IVersionControler
        {
            bool IsWebInUncheckedLists(List<string> unCheckedLists, string webName);
        }

        private static class VersionControlerFactory
        {
            public static IVersionControler GetVersionControler(ProductVersion version)
            {
                IVersionControler versionControler;
                switch (version)
                {
                    case ProductVersion.Product6X:
                        versionControler = new D6VersionControler();
                        break;
                    case ProductVersion.Product5X:
                        versionControler = new D5VersionControler();
                        break;
                    case ProductVersion.Product4X:
                    case ProductVersion.ProductUnknown:
                        throw new AveException("Invalid product version.");
                    default:
                        throw new AveException("Invalid product version.");
                }
                return versionControler;
            }
        }

        private class D5VersionControler : IVersionControler
        {
            public bool IsWebInUncheckedLists(List<string> unCheckedLists, string webName)
            {
                return true;
            }
        }
        private class D6VersionControler : IVersionControler
        {
            public bool IsWebInUncheckedLists(List<string> unCheckedLists, string webName)
            {
                return unCheckedLists.Contains(webName);
            }
        }
    }
}