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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Utility.Cryptography.DataEncryptionManagement;
using AvePoint.RA.CommonUtil;
using DataExportCore.Cache;
using DataExportCore.Utils;
using Media.Service.ArchiverBackup;
using Storage;
using System.Diagnostics;
using System.Reflection;
using System.Text;

namespace DataExportCore.Export
{
    public abstract class ExportWorkerBase : IDisposable
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod()?.DeclaringType ?? typeof(ExportWorkerBase));
        protected Reporter Reporter;
        protected ExportQueue ExportQueue;
        protected IXSystem DestinationSystem;
        protected string GroupAddress = string.Empty;

        protected ExportWorkerBase(Reporter report, ExportQueue exportQueue, IXSystem destinationSystem)
        {
            this.Reporter = report;
            this.ExportQueue = exportQueue;
            this.DestinationSystem = destinationSystem;
        }

        protected ExportWorkerBase(Reporter report, ExportQueue exportQueue, IXSystem destinationSystem, string groupAddress)
        {
            this.Reporter = report;
            this.ExportQueue = exportQueue;
            this.DestinationSystem = destinationSystem;
            this.GroupAddress = groupAddress;
        }

        protected virtual string ExportSite(DiscoverNode node)
        {
            Reporter.ConfigForReport(ExportUtility.BuildTargetUrl(node.ExportPath), ExportUtility.IsNeedUploadAndDeleteCache());
            Reporter.RecordSuccessful(node);
            return node.ExportPath;
        }

        protected virtual void ExportWeb(DiscoverNode node)
        {
            Reporter.RecordSuccessful(node);
        }

        protected virtual void ExportApp(DiscoverNode node)
        {
            throw new NotImplementedException();
        }

        protected virtual void ExportList(ListDiscoverNode node)
        {
            ExportFolder(node);
        }

        protected virtual void ExportFolder(FolderDiscoverNode node)
        {
            Reporter.RecordSuccessful(node);

            foreach (var item in node.Items)
            {
                item.ExportPath = node.ExportPath;
                ExportItem(item);
            }

            foreach (var folder in node.SubFolders)
            {
                ExportFolder(folder);
            }
        }

        protected virtual void ExportItem(ItemDiscoverNode node)
        {
            Reporter.CurrentFile = node.Index.Url;

            if (node.Level == NodeType.ListItem)
            {
                Reporter.RecordSuccessful(node);
                return;
            }
            // Process export item
            try
            {
                if (GlobalCache.IsSkipAPData && GlobalDeviceCache.IsDeviceAPStorage(node.StorageId))
                {
                    logger.Info($"Skip export item [{node.Name}] with storageId [{node.StorageId}] because it is AP storage and Skip AP data is enabled.");
                    Reporter.RecordSkipped(node, I18NEntity.GetString("SATool_SkipExportContentFileInAPStorage"));
                    return;
                }

                if (GlobalDeviceCache.IsStorageOpenFailed(node.StorageId, out string? type))
                {
                    throw new ManagedException(ErrorType.CannotOpenDevice, new[] { node.StorageId, type ?? StorageDeviceType.None.ToString() });
                }

                if(node.DataEncryptionInfo != null) DataEncryptionInfoManager.PutEncryptionInfo(node.DataEncryptionInfo, Encoding.UTF8.GetString(node.DataEncryptionInfo.EncryptedDynamicKey));
                StorageInfo info = GetStorageInfo(BuildStorageInfoExportPath(node.ExportPath), node.Name, node.Level, node.Index.ItemName);
                node.ExportPath = Path.Combine(node.ExportPath, info.LowName);
                byte[] buffer = new byte[64 * 1024];
                using (XStream stream = DestinationSystem.OpenStream(info, FileMode.OpenOrCreate))
                {
                    if (node.Index.ContentLength != 0L)
                    {
                        using (var archiveDownloadStream = new ArchiveDownloadStream(node.Index, node.DataVolume, node.DataEncryptionInfo, GlobalDeviceCache.GetDeviceById(node.StorageId)))
                        {
                            Stopwatch stopwatch = Stopwatch.StartNew();
                            while (true)
                            {
                                int len = archiveDownloadStream.Read(buffer, 0, buffer.Length);
                                if (len <= 0) break;
                                stream.Write(buffer, 0, len);
                                node.ExportedFileSize += len;
                            }
                            stopwatch.Stop();
                            logger.Info($"[{node.SitePath}]Export item [{node.Name}] finished in [{stopwatch.Elapsed}]");
                        }
                    }
                    Reporter.RecordSuccessful(node);
                }
            }
            catch (ManagedException me)
            {
                logger.Error($"An error occurred while export item {node.Level} with id {node.Id}. ExType: {me.ErrorType}, Ex: {me}");
                Reporter.RecordFailed(node, me.Message);
            }
            catch (FileNotFoundException ex)
            {
                logger.Error($"Cannot find the archived content file [{ex.FileName}] to restore content for item {node.Level} with id {node.Id}. Ex: {ex}");
                Reporter.RecordFailed(node, string.Format(I18NEntity.GetString("SATool_ContentFileNotFoundError"), ex.FileName));
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while export item {node.Level} with id {node.Id}. Ex: {ex}");
                Reporter.RecordFailed(node, I18NEntity.GetString("SATool_ExportItemUnexpectedError"));
            }
        }

        public string Process()
        {
            string siteExportPath = string.Empty;
            try
            {
                DiscoverNode node;
                while ((node = ExportQueue.MoveNext()) != null)
                {
                    //Check job stop
                    try
                    {
                        //Log.Info
                        logger.Info($"export [{node.Level}] node [{node.Name}] in [{node.SitePath}]");
                        //Check heeartbeat
                        node.ExportPath = ExportUtility.BuildExportPath(string.IsNullOrEmpty(GroupAddress) ? GlobalCache.ExportLocation : Path.Combine(GlobalCache.ExportLocation, GroupAddress, I18NEntity.GetString("SATool_ExportPath_SiteCollections")), node.Name, node.SitePath, node.Level);
                        switch (node)
                        {
                            case SiteDiscoverNode:
                                siteExportPath = ExportSite(node);
                                break;
                            case WebDiscoverNode:
                                ExportWeb(node); // sub site
                                break;
                            case ListDiscoverNode listNode:
                                ExportList(listNode);
                                break;
                            default:
                                //unknown
                                break;
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Error($"An error occurs while export node. Ex:{e}");
                        Reporter.RecordFailed(node, I18NEntity.GetString("SATool_ExportItemUnexpectedError"));
                    }
                    finally
                    {
                    }
                }
                return siteExportPath;
            }
            catch (Exception e)
            {
                logger.Error($"An error occurred while export process. Ex:{e}");
                throw;
            }
            finally
            {
                Reporter.Complete();
            }
        }

        public void Dispose()
        {

        }

        #region support fuctions

        protected abstract string BuildStorageInfoExportPath(string exportPath);

        protected static bool CreateDirectory(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
                return true;
            }
            return false;
        }

        public static StorageInfo GetStorageInfo(string exportPath, string name, NodeType level, string itemName)
        {
            int colonPosition = name.LastIndexOf(":", StringComparison.OrdinalIgnoreCase);
            if (level == NodeType.Attachment)
            {
                string folderName = $"{name.Substring(0, colonPosition)}Attachment\\";
                exportPath = Path.Combine(exportPath, folderName);
                name = name.Substring(colonPosition + 1);
            }
            else
            {
                string fileName = itemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) >= 0 ? itemName.Remove(itemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase)) : itemName;
                string extension = itemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase) >= 0 ? itemName.Substring(itemName.LastIndexOf(".", StringComparison.OrdinalIgnoreCase)) : string.Empty;
                string newFileName = colonPosition > 0 ? fileName + '_' + name.Substring(colonPosition + 1) + extension : name;
                name = newFileName;
            }
            logger.Info($"Item path: {exportPath}, name {name} and level: {level}");
            return ProcessFileExist(exportPath, name);
        }

        protected static StorageInfo ProcessFileExist(string exportPath, string name)
        {
            var info = XConvert.FromNames(ExportUtility.ReplaceInvalidChar(exportPath, false), ExportUtility.ReplaceInvalidChar(name, true));
            var isFileExists = GlobalDeviceCache.ExportCacheSetting.FileExists(info);
            string fileName = Path.GetFileNameWithoutExtension(info.LowName);
            string fileExtend = Path.GetExtension(info.LowName);
            int fileNameIndex = 1;
            while (isFileExists)
            {
                info.LowName = $@"{fileName}({fileNameIndex++}){fileExtend}";
                isFileExists = GlobalDeviceCache.ExportCacheSetting.FileExists(info);
            }
            return info;
        }

        #endregion

    }
}
