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
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.ClassicStorage.Util;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.GCommon.Utility;
using PnP.Framework.Diagnostics;
using Storage;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace RAExportCommon
{
    public class MergeVEOBase : IVaultMergeVEO
    {
        private readonly AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private double LimitFolderSize = 0;
        private long TransferFolderSize = 0;
        private long LimitFileConut = 0;
        private string mergeFolderName = string.Empty;
        private string tempFolderName = string.Empty;
        private bool IsDeleteOldFile = false;
        private static int number = 0;
        private long CurrentFolderSize = 0;


        //Default file conut is 300 ,and Folder Size is 1024M
        public MergeVEOBase()
            : this(300, 1, "Transfer Set", false)
        {

        }

        public MergeVEOBase(long fileCount, double folderSize, string folderName, bool isDeleteOldFile)
        {
            LimitFolderSize = folderSize;
            LimitFileConut = fileCount;
            mergeFolderName = folderName;
            IsDeleteOldFile = isDeleteOldFile;
        }

        public virtual void Init(long fileCount, double folderSize)
        {
            LimitFolderSize = folderSize;
            LimitFileConut = fileCount;
        }

        /// <summary>
        /// VEO Merge 计算Folder Size & File Count均只计算以.VEO后缀结尾的文件，其它类型文件均不计算在内.
        /// </summary>

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "veo")]
        public virtual List<MergeVEOJobDetail> MergeVEO(PhysicalDeviceDto physical, string JobId)
        {
            string xmlFolderPath = SecurityUtils.SafeCombinePath(
                AppDomain.CurrentDomain.BaseDirectory, "AgentData", "jobs", JobId);//use for local manifest.xml path
            if (!Directory.Exists(xmlFolderPath))
            {
                Directory.CreateDirectory(xmlFolderPath);
            }
            string xmlPath = SecurityUtils.SafeCombinePath(xmlFolderPath, "manifest.xml");
            bool isAzureStorage = false;
            List<MergeVEOJobDetail> jobdetails = new List<MergeVEOJobDetail>();
            //转换FolderSize单位，由G转换为KB，并转换成Long，有1Byte内的精度损失忽略不计.
            //如果FolderSize为0，则默认Size为long最大值.
            TransferFolderSize = LimitFolderSize < 1E-06 ? long.MaxValue : Convert.ToInt64(LimitFolderSize * 1024 * 1024 * 1024);
            mLog.Info(string.Format("Begin MergeVEO method, MergeVEO TransferFolderSize:{0}, Physical Device path:{1}.", TransferFolderSize, physical.Location));
            tempFolderName = mergeFolderName;
            try
            {
                XRI xri = XRI.ValueOf(physical.BuildXRI());
                if (xri.VIM.ToLower() == "azure_vim")
                {
                    isAzureStorage = true;
                    mLog.Info("this is azure storage when MergeVeo");
                }
            }
            catch (Exception e)
            {
                mLog.Warn("some thing went wrong when ValueOf xri string");
            }

            using (DeviceUtil deviceUtil = new DeviceUtil())
            {
                deviceUtil.Open(physical, isAzureStorage);
                //每次mergeVEO Job需要判断目的端folder是否在目的端存在，如果存在需要Rename Folder Name再创建Folder.eg：目的端存在Folder_001，我们需要创建新的Folder，Name:Folder_002
                GetRealFolderName(deviceUtil, JobId);
                Manifest_Schema.SetManifest surceManifest = null;
                List<XDirectoryInfo> directories = deviceUtil.GetDirectories(JobId).Where(d => d.Name.StartsWith(JobId, StringComparison.OrdinalIgnoreCase)).ToList();
                mLog.Info(string.Format("Current JobId [{0}] have {1} directories.", JobId, directories.Count));
                foreach (XDirectoryInfo directory in directories)
                {
                    try
                    {
                        using (new CheckJobStopScope()) { }
                        List<XFileInfo> sourceFiles = GetFiles(deviceUtil, directory, isAzureStorage);
                        mLog.Info(string.Format("Begin process source directory [{0}], directory file count:{1}.", directory.Name, sourceFiles.Count));
                        XDirectoryInfo destinationFolderInfo = null;
                        if (sourceFiles.Count > 0)
                        {
                            //创建MergeVEO目的端Folder，之前已经处理Folder Name，因此此处直接创建Folder即可。
                            destinationFolderInfo = CreateNewFolder(deviceUtil, JobId);
                            long desFileCounts = GetFiles(deviceUtil, destinationFolderInfo, isAzureStorage).Where(f => f.Name.EndsWith(".veo", StringComparison.OrdinalIgnoreCase)).ToList().Count;
                            mLog.Info(string.Format("Destination folder:{0}, file count:{1}.", destinationFolderInfo.Name, desFileCounts));
                            List<XFileInfo> sourceVEOFiles = sourceFiles.Where(f => f.Name.EndsWith(".veo", StringComparison.OrdinalIgnoreCase)).ToList();
                            List<XFileInfo> sourceManifests = sourceFiles.Where(f => f.Name.Equals("manifest.xml", StringComparison.OrdinalIgnoreCase)).ToList();
                            mLog.Info(string.Format("Source VEO File Count:{0}.Source Manifest Count:{1}.", sourceVEOFiles.Count, sourceManifests.Count));
                            //获取原端Manifest并反序列化成Manifest对象,获取源端sourceManifestObjectItem,并转换成List
                            Manifest_Schema.ElectronicTransfer source_VEO_ElectronicTransfer = null;
                            List<Manifest_Schema.ManifestObjectItem> sourceManifestObjectItem = null;
                            if (sourceManifests.Count != 0)
                            {
                                using (XStream stream = deviceUtil.OpenStream(sourceManifests[0]))
                                {
                                    surceManifest = (Manifest_Schema.SetManifest)new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Deserialize(stream);
                                }
                                source_VEO_ElectronicTransfer = (Manifest_Schema.ElectronicTransfer)surceManifest.Item;
                                sourceManifestObjectItem = source_VEO_ElectronicTransfer.manifest_object_list.ToList();
                            }
                            List<Manifest_Schema.ManifestObjectItem> tempSourceManifestObjectItem = new List<Manifest_Schema.ManifestObjectItem>();

                            //获取目的端Manifest并反序列化成Manifest对象
                            List<XFileInfo> desFiles = GetFiles(deviceUtil, destinationFolderInfo, isAzureStorage);
                            //如果目的端存在文件，则需要统计没Merge之前源端文件Size
                            CurrentFolderSize = GetExistFolderFileSize(desFiles);
                            List<XFileInfo> desManifests = desFiles.Where(f => f.Name.Equals("manifest.xml", StringComparison.OrdinalIgnoreCase)).ToList();

                            #region 目的端已经存在Manifest
                            //如果目的端已经存在Manifest，则利用目的端的Manifest生成目的端的Manifest，但是当目的端超过上限则会创建新的folder，此时需要用原端的manifest生成
                            if (desManifests.Count > 0)
                            {
                                Manifest_Schema.SetManifest desManifest = null;
                                using (XStream stream = deviceUtil.OpenStream(desManifests[0]))
                                {
                                    desManifest = (Manifest_Schema.SetManifest)new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Deserialize(stream);
                                }
                                //获取目的端 des_VEO_ElectronicTransfer并转换成List
                                Manifest_Schema.ElectronicTransfer des_VEO_ElectronicTransfer = (Manifest_Schema.ElectronicTransfer)desManifest.Item;
                                List<Manifest_Schema.ManifestObjectItem> desManifestObjectItem = des_VEO_ElectronicTransfer.manifest_object_list.ToList();
                                mLog.Info(string.Format("Destination has manifest file, Manifest Object Item count:{0}.", desManifestObjectItem.Count));
                                List<Manifest_Schema.ManifestObjectItem> tempDesManifestObjectItem = new List<Manifest_Schema.ManifestObjectItem>();

                                foreach (XFileInfo file in sourceVEOFiles)
                                {
                                    using (new CheckJobStopScope()) { }
                                    try
                                    {
                                        //源端不存在Manifest，则源端File不merge到目的端，job finish with Exception，给出相应提示语
                                        if (sourceManifests.Count == 0)
                                        {
                                            mLog.Info(string.Format("Source Manifest not exist and current file [{0}] will not merge to destination folder.", file.Name));
                                            throw new Exception("StorageOptimization_MergeVEOSourceManifestNotExist");
                                        }
                                        if (desFileCounts >= LimitFileConut)
                                        {
                                            mLog.Info(string.Format("Destination file count [{0}] greater than LimitFileCount [{1}], merge file count [{2}].", desFileCounts, LimitFileConut, tempDesManifestObjectItem.Count));
                                            if (desManifests.Count == 0)
                                            {
                                                source_VEO_ElectronicTransfer.manifest_object_list = tempDesManifestObjectItem.ToArray();
                                                //destinationFolderInfo.LowName = "manifest.xml";
                                                var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                                //using (XStream stream = deviceUtil.OpenStream(destinationFolderInfo, FileMode.Create))
                                                //{
                                                //    new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(stream, surceManifest);
                                                //}
                                                CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, surceManifest);
                                            }
                                            else
                                            {
                                                desManifestObjectItem.AddRange(tempDesManifestObjectItem);
                                                des_VEO_ElectronicTransfer.manifest_object_list = desManifestObjectItem.ToArray();
                                                //destinationFolderInfo.LowName = "manifest.xml";
                                                var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                                //using (XStream stream = deviceUtil.OpenStream(destinationFolderInfo, FileMode.Create))
                                                //{
                                                //    new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(stream, desManifest);
                                                //}
                                                CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, desManifest);
                                            }
                                            tempDesManifestObjectItem.Clear();
                                            destinationFolderInfo = CreateNewFolder(deviceUtil, JobId, true);
                                            desFileCounts = GetFiles(deviceUtil, destinationFolderInfo, isAzureStorage).Where(f => f.Name.EndsWith(".veo", StringComparison.OrdinalIgnoreCase)).ToList().Count;
                                            desManifests = GetFiles(deviceUtil, destinationFolderInfo, isAzureStorage).Where(f => f.Name.Equals("manifest.xml", StringComparison.OrdinalIgnoreCase)).ToList();
                                            ResetFolderSize();
                                        }
                                        //Check size before added file
                                        if (CurrentFolderSize + file.FileSize > TransferFolderSize)
                                        {
                                            mLog.Info(string.Format("Destination folder size [{0}] greater than TransferFolderSize [{1}], merge file count [{2}].", CurrentFolderSize, TransferFolderSize, tempDesManifestObjectItem.Count));
                                            if (desManifests.Count == 0)
                                            {
                                                source_VEO_ElectronicTransfer.manifest_object_list = tempDesManifestObjectItem.ToArray();
                                                //destinationFolderInfo.LowName = "manifest.xml";
                                                var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                                //using (XStream stream = deviceUtil.OpenStream(destinationFolderInfo, FileMode.Create))
                                                //{
                                                //    new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(stream, surceManifest);
                                                //}
                                                CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, surceManifest);
                                            }
                                            else
                                            {
                                                desManifestObjectItem.AddRange(tempDesManifestObjectItem);
                                                des_VEO_ElectronicTransfer.manifest_object_list = desManifestObjectItem.ToArray();
                                                //destinationFolderInfo.LowName = "manifest.xml";
                                                var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                                //using (XStream stream = deviceUtil.OpenStream(destinationFolderInfo, FileMode.Create))
                                                //{
                                                //    new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(stream, desManifest);
                                                //}
                                                CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, desManifest);
                                            }
                                            tempDesManifestObjectItem.Clear();
                                            destinationFolderInfo = CreateNewFolder(deviceUtil, JobId, true);
                                            desFileCounts = GetFiles(deviceUtil, destinationFolderInfo, isAzureStorage).Where(f => f.Name.EndsWith(".veo", StringComparison.OrdinalIgnoreCase)).ToList().Count;
                                            desManifests = GetFiles(deviceUtil, destinationFolderInfo, isAzureStorage).Where(f => f.Name.Equals("manifest.xml", StringComparison.OrdinalIgnoreCase)).ToList();
                                            ResetFolderSize();
                                        }
                                        //destinationFolderInfo.LowName = file.LowName;
                                        var info = new StorageInfo() { HighName = file.HighName, LowName = file.LowName };
                                        var desInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = file.LowName };
                                        deviceUtil.MergeFile(info, desInfo, IsDeleteOldFile);
                                        jobdetails.Add(new MergeVEOJobDetail() { FileName = file.LowName, SourceFolder = deviceUtil.instanceSystem.SystemLocation + "\\" + directory.HighPlusLowName, DesFolder = destinationFolderInfo.HighPlusLowName.TrimEnd('/'), Status = (int)ExportState.Succeed, Size = file.FileSize, FinishTime = DateTime.UtcNow, Comment = "" });
                                        tempDesManifestObjectItem.Add(sourceManifestObjectItem.Find(delegate (Manifest_Schema.ManifestObjectItem p) { return p.computer_filename == file.LowName; }));
                                        desFileCounts++;
                                        CurrentFolderSize += file.FileSize;
                                    }
                                    catch (Exception ex)
                                    {
                                        jobdetails.Add(new MergeVEOJobDetail() { FileName = file.LowName, SourceFolder = deviceUtil.instanceSystem.SystemLocation + "\\" + directory.HighPlusLowName, DesFolder = destinationFolderInfo.HighPlusLowName.TrimEnd('/'), Status = (int)ExportState.Failed, Size = file.FileSize, FinishTime = DateTime.UtcNow, Comment = ex.Message });
                                        mLog.Error(string.Format("An error occur while merge VEO file.FileName:{0}, Message:{1}.", file.Name, ex.ToString()));
                                    }
                                }
                                //当前原端Folder move veo file文件完事，需要重新在目的端生成一个manifest，否则最后一个folder中不会有manifest.
                                if (sourceManifests.Count != 0)//源端 manifest不存在则不merge manifest
                                {
                                    if (desManifests.Count == 0)
                                    {
                                        if (source_VEO_ElectronicTransfer != null)
                                        {
                                            source_VEO_ElectronicTransfer.manifest_object_list = tempDesManifestObjectItem.ToArray();
                                        }
                                        //destinationFolderInfo.LowName = "manifest.xml";
                                        var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                        //using (XStream stream = deviceUtil.OpenStream(desStorageInfo, FileMode.Create))
                                        //{
                                        //    new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(stream, surceManifest);
                                        //}
                                        CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, surceManifest);
                                    }
                                    else
                                    {
                                        desManifestObjectItem.AddRange(tempDesManifestObjectItem);
                                        des_VEO_ElectronicTransfer.manifest_object_list = desManifestObjectItem.ToArray();
                                        //destinationFolderInfo.LowName = "manifest.xml";
                                        var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                        //using (XStream stream = deviceUtil.OpenStream(desStorageInfo, FileMode.Create))
                                        //{
                                        //    new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(stream, desManifest);
                                        //}
                                        CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, desManifest);
                                    }
                                }
                                tempDesManifestObjectItem.Clear();
                            }
                            #endregion

                            #region 目的端不存在Manifest
                            //目的端不存在Manifest,则利用原端的Manifest生成目的端的Manifest，此种情况不会出现目的端存在manifest的情况，除非已经到了下个文件夹，
                            //会走上面有manifest的逻辑.
                            else
                            {
                                mLog.Info("Destination doesn't have manifest file.");
                                IEnumerable<XFileInfo> sortedFiles = sourceVEOFiles.OrderBy(file => file.FileSize);
                                foreach (XFileInfo file in sortedFiles)
                                {
                                    using (new CheckJobStopScope()) { }
                                    try
                                    {
                                        //源端不存在Manifest，则源端File不merge到目的端，job finish with Exception，给出相应提示语
                                        if (sourceManifests.Count == 0)
                                        {
                                            mLog.Info(string.Format("Source Manifest not exist and current file  will not merge to destination folder."));
                                            throw new Exception("StorageOptimization_MergeVEOSourceManifestNotExist");
                                        }
                                        if (desFileCounts >= LimitFileConut)
                                        {
                                            mLog.Info(string.Format("Destination file count [{0}] greater than LimitFileCount [{1}], merge file count [{2}].", desFileCounts, LimitFileConut, tempSourceManifestObjectItem.Count));
                                            if (source_VEO_ElectronicTransfer != null)
                                            {
                                                source_VEO_ElectronicTransfer.manifest_object_list = tempSourceManifestObjectItem.ToArray();
                                            }
                                            //destinationFolderInfo.LowName = "manifest.xml";
                                            var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                            //using (XStream stream = deviceUtil.OpenStream(destinationFolderInfo, FileMode.Create))
                                            //{
                                            //    new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(stream, surceManifest);
                                            //}
                                            CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, surceManifest);
                                            tempSourceManifestObjectItem.Clear();
                                            destinationFolderInfo = CreateNewFolder(deviceUtil, JobId, true);
                                            desFileCounts = GetFiles(deviceUtil, destinationFolderInfo, isAzureStorage).Where(f => f.Name.EndsWith(".veo", StringComparison.OrdinalIgnoreCase)).ToList().Count;
                                            ResetFolderSize();
                                            mLog.Info(string.Format("File count is over : {0}, we create another folder", desFileCounts));
                                        }
                                        //Check size before added file
                                        if (CurrentFolderSize + file.FileSize > TransferFolderSize)
                                        {
                                            mLog.Info(string.Format("Destination folder size [{0}] greater than TransferFolderSize [{1}], merge file count [{2}].", CurrentFolderSize, TransferFolderSize, tempSourceManifestObjectItem.Count));
                                            if (source_VEO_ElectronicTransfer != null)
                                            {
                                                source_VEO_ElectronicTransfer.manifest_object_list = tempSourceManifestObjectItem.ToArray();
                                            }
                                            //destinationFolderInfo.LowName = "manifest.xml";
                                            var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                            //using (XStream stream = deviceUtil.OpenStream(destinationFolderInfo, FileMode.Create))
                                            //{
                                            //    new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(stream, surceManifest);
                                            //}
                                            CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, surceManifest);
                                            tempSourceManifestObjectItem.Clear();
                                            destinationFolderInfo = CreateNewFolder(deviceUtil, JobId, true);
                                            desFileCounts = GetFiles(deviceUtil, destinationFolderInfo, isAzureStorage).Where(f => f.Name.EndsWith(".veo", StringComparison.OrdinalIgnoreCase)).ToList().Count;
                                            ResetFolderSize();
                                            mLog.Info(string.Format("File size will over : {0}, we create another folder", LimitFolderSize));
                                        }
                                        //destinationFolderInfo.LowName = file.LowName;
                                        var info = new StorageInfo() { HighName = file.HighName, LowName = file.LowName };
                                        //var desInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = file.LowName ,FileTierType = AccessTierType.Cool};
                                        //TODO nliu
                                        var desInfo = new StorageInfo()
                                        {
                                            HighName = destinationFolderInfo.LowName,
                                            LowName = file.LowName
                                        };
                                        deviceUtil.MergeFile(info, desInfo, IsDeleteOldFile);
                                        jobdetails.Add(new MergeVEOJobDetail() { FileName = file.LowName, SourceFolder = deviceUtil.instanceSystem.SystemLocation + "\\" + directory.HighPlusLowName, DesFolder = destinationFolderInfo.HighPlusLowName.TrimEnd('/'), Status = (int)ExportState.Succeed, Size = file.FileSize, FinishTime = DateTime.UtcNow, Comment = "" });
                                        tempSourceManifestObjectItem.Add(sourceManifestObjectItem.Find(delegate (Manifest_Schema.ManifestObjectItem p) { return p.computer_filename == file.LowName; }));
                                        desFileCounts++;
                                        CurrentFolderSize += file.FileSize;
                                    }
                                    catch (Exception ex)
                                    {
                                        jobdetails.Add(new MergeVEOJobDetail() { FileName = file.LowName, SourceFolder = deviceUtil.instanceSystem.SystemLocation + "\\" + directory.HighPlusLowName, DesFolder = destinationFolderInfo.HighPlusLowName.TrimEnd('/'), Status = (int)ExportState.Failed, Size = file.FileSize, FinishTime = DateTime.UtcNow, Comment = ex.Message });
                                        mLog.Error(string.Format("An error occur while merge VEO file.FileName:{0}, Message:{1}.", file.Name, ex.ToString()));
                                    }
                                }
                                //当前原端Folder move veo file文件完事，需要重新在目的端生成一个manifest，否则最后一个folder中不会有manifest.
                                if (sourceManifests.Count != 0)//源端 manifest不存在则不merge manifest
                                {
                                    if (source_VEO_ElectronicTransfer != null)
                                    {
                                        source_VEO_ElectronicTransfer.manifest_object_list = tempSourceManifestObjectItem.ToArray();
                                    }
                                    //destinationFolderInfo.LowName = "manifest.xml";
                                    var desStorageInfo = new StorageInfo() { HighName = destinationFolderInfo.LowName, LowName = "manifest.xml" };
                                    //using (XStream stream = deviceUtil.OpenStream(destinationFolderInfo, FileMode.Create))
                                    //using (var stream = deviceUtil.OpenStream(desStorageInfo, FileMode.Create))
                                    //{
                                    //    using (var downloadStream = deviceUtil.OpenStream(desStorageInfo, FileMode.Open))
                                    //    {
                                    //        new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(ConvertStreamToMemoryStream(downloadStream), surceManifest);
                                    //    }
                                    //}
                                    CommitXmlToStorageInfo(xmlPath, desStorageInfo, deviceUtil, surceManifest);
                                }
                                tempSourceManifestObjectItem.Clear();
                            }
                            #endregion
                            //当当前directory文件都move到目的端，删除源端manifest.
                            if (IsDeleteOldFile)
                            {
                                if (sourceManifests.Count != 0)
                                {
                                    deviceUtil.DeleteFile(sourceManifests[0]);
                                }
                                List<XFileInfo> files = GetFiles(deviceUtil, directory, isAzureStorage);
                                //如果源端Folder中不存在文件，则删除当前Folder，否则不删除.
                                if (files.Count == 0)
                                {
                                    bool existDirectories = ExistDirectories(deviceUtil, directory, isAzureStorage);
                                    if (!existDirectories)
                                    {
                                        mLog.Info($"no file and directories in the folder,so delete it,folderName:{directory.LowName},highName:{directory.HighName}");
                                        deviceUtil.DeleteFolder(directory);
                                    }
                                    else
                                    {
                                        mLog.Info($"there exist directories in the folder,so skip delete it,folderName:{directory.LowName}");
                                    }
                                }
                            }
                        }
                        else
                        {
                            mLog.Info(string.Format("Source Folder is Empty Folder While Merge VEO, FolderName:{0}.", directory.Name));
                        }
                    }
                    catch (JobStopException ex)
                    {
                        mLog.Warn("merge veo job is stopped by manual");
                        throw;
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("An error occur while process mergeVEO directory, directory name:{0}, Message:{1}.", directory.FullName, e.ToString());
                    }
                }

                try
                {
                    if (Directory.Exists(xmlFolderPath))
                    {
                        Directory.Delete(xmlFolderPath);
                    }
                }
                catch (Exception ex)
                {
                    mLog.Warn("Delete manifest folder error, Folder Path: {0}, Message: {1}", xmlFolderPath, ex.ToString());
                }
            }
            return jobdetails;
        }

        //Commit manifest.xml to desStorageInfo
        private StorageResult CommitXmlToStorageInfo(string xmlPath, StorageInfo desStorageInfo, DeviceUtil deviceUtil, Manifest_Schema.SetManifest manifest)
        {
            using (FileStream nFileStream = new FileStream(xmlPath, FileMode.Create))
            {
                XmlSerializerNamespaces ns = new XmlSerializerNamespaces();
                ns.Add("dam", "http://www.prov.vic.gov.au/digitalarchive/");
                ns.Add("xsi", "http://www.w3.org/2001/XMLSchema-instance");
                new XmlSerializer(typeof(Manifest_Schema.SetManifest)).Serialize(nFileStream, manifest, ns);
                desStorageInfo.Length = nFileStream.Length;
                nFileStream.Position = 0;
                return deviceUtil.CommitStream(nFileStream, desStorageInfo);//commit stream to desStorageInfo
            }
        }
        private List<XFileInfo> GetFiles(DeviceUtil deviceUtil, XDirectoryInfo dirInfo, bool isAzureStorage = false)
        {
            if (isAzureStorage)
            {
                List<AvePoint.Media.ClassicStorage.XFileInfo> files = deviceUtil.instanceSystemAzure.ListFiles(new AvePoint.Media.ClassicStorage.StorageInfo() { HighName = dirInfo.HighName, LowName = dirInfo.LowName });
                return ConverClassicStorageToStorageInfo(files);
            }
            else
            {
                return deviceUtil.GetFiles(dirInfo);
            }
        }
        private bool ExistDirectories(DeviceUtil deviceUtil, XDirectoryInfo dirInfo, bool isAzureStorage = false)
        {
            if (isAzureStorage)
            {
                List<AvePoint.Media.ClassicStorage.XDirectoryInfo> directories = deviceUtil.instanceSystemAzure.ListDirectories(new AvePoint.Media.ClassicStorage.StorageInfo() { HighName = dirInfo.HighName, LowName = dirInfo.LowName });
                return directories.Count > 0;
            }
            else
            {
                var directories = deviceUtil.GetDirectories(dirInfo);
                return directories.Count > 0;
            }
        }
        private List<XFileInfo> ConverClassicStorageToStorageInfo(List<AvePoint.Media.ClassicStorage.XFileInfo> infos)
        {
            List<XFileInfo> result = new List<XFileInfo>();
            foreach (var temp in infos)
            {
                XFileInfo re = new XFileInfo()
                {
                    HighName = temp.HighName,
                    LowName = temp.LowName,
                    FileSize = temp.FileSize
                };
                result.Add(re);
            }
            return result;
        }

        public virtual void MergeManifest(StorageInfo source, StorageInfo destination, bool isOverwrite = true)
        {

        }

        private XDirectoryInfo CreateNewFolder(DeviceUtil deviceUtil, string jobID, bool realCreateFolder = false)
        {
            if (realCreateFolder)
            {
                GetRealFolderName(deviceUtil, jobID);
            }
            return deviceUtil.GetOrCreateDirectory(mergeFolderName);
        }

        private void ResetFolderSize()
        {
            CurrentFolderSize = 0;
        }

        /// <summary>
        /// 获取已经存在Folder下VEO文件Size
        /// </summary>

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", MessageId = "veo")]
        private long GetExistFolderFileSize(List<XFileInfo> files)
        {
            long size = 0;
            try
            {
                IEnumerable<XFileInfo> fileInfo = files.Where(f => f.Name.EndsWith(".veo", StringComparison.OrdinalIgnoreCase));
                foreach (var file in fileInfo)
                {
                    size += file.FileSize;
                }
            }
            catch (Exception e)
            {
                size = 0;
                mLog.Info(string.Format("Can not Get Exist Folder File Size,Message:{0}.", e.ToString()));
            }
            return size;
        }

        private void GetRealFolderName(DeviceUtil deviceUtil, string JobID)
        {
            if (number < 10)
            {
                mergeFolderName = tempFolderName + "_" + JobID + "_00" + number;
            }
            else if (number < 100 && number >= 10)
            {
                mergeFolderName = tempFolderName + "_" + JobID + "_0" + number;
            }
            else
            {
                mergeFolderName = tempFolderName + "_" + JobID + "_" + number;
            }
            number++;
            if (deviceUtil.CheckDirectoryExists(mergeFolderName))
            {
                GetRealFolderName(deviceUtil, JobID);
            }
        }
    }
}
