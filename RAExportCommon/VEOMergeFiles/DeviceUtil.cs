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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.Media.ClassicStorage;
using Storage;
using Storage.Cloud.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using StorageCopyResult = Storage.StorageCopyResult;
using StorageDeleteResult = Storage.StorageDeleteResult;
using StorageInfo = Storage.StorageInfo;
using StorageResult = Storage.StorageResult;
using XDirectoryInfo = Storage.XDirectoryInfo;
using XFactory = Storage.XFactory;
using XFileInfo = Storage.XFileInfo;
using XStream = Storage.XStream;

namespace RAExportCommon
{
    public class DeviceUtil : IDisposable
    {
        public IXSystem instanceSystem;
        public IXSystemCommon instanceSystemAzure;
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public void Open(PhysicalDeviceDto deviceDto,bool isAzureStorage=false)
        {
            instanceSystem = XFactory.InstanceSystem(deviceDto.BuildXRI());
            instanceSystem.Open();
            if (isAzureStorage)
            {
                instanceSystemAzure = AvePoint.Media.ClassicStorage.XFactory.InstanceSystem(deviceDto.BuildXRI());
                instanceSystemAzure.Open();
            }
        }

        public XStream OpenStream(StorageInfo storageInfo, FileMode mode = FileMode.Open)
        {
            return instanceSystem.OpenStream(storageInfo, mode);
        }

        //For NetShare Only
        public XStream OpenStream(string filePath)
        {
            string highName = Path.GetDirectoryName(filePath);
            string lowName = Path.GetFileName(filePath);
            return instanceSystem.OpenStream(new StorageInfo() { HighName = highName, LowName = lowName }, FileMode.Open);
        }

        public XStream OpenStream(string highName, string lowName)
        {
            return instanceSystem.OpenStream(new StorageInfo() { HighName = highName, LowName = lowName }, FileMode.Open);
        }

        public StorageResult CommitStream(Stream commitStream, StorageInfo info) 
        {
            return instanceSystem.CommitStream(commitStream, info);
        }

        //For NetShare Only
        public List<XDirectoryInfo> GetDirectories(string filePath)
        {
            string highName = Path.GetDirectoryName(filePath);
            string lowName = string.Empty;
            return instanceSystem.ListDirectories(new StorageInfo() { HighName = highName, LowName = lowName });
        }

        public List<XDirectoryInfo> GetDirectories(string highName, string lowName)
        {
            return instanceSystem.ListDirectories(new StorageInfo() { HighName = highName, LowName = lowName });
        }

        public List<XDirectoryInfo> GetDirectories(StorageInfo storageInfo)
        {
            return instanceSystem.ListDirectories(storageInfo);
        }

        public bool CheckDirectoryExists(StorageInfo storageInfo)
        {
            return instanceSystem.DirectoryExists(storageInfo);
        }

        public bool CheckDirectoryExists(string path)
        {
            string highName = path;
            string lowName = string.Empty;
            return instanceSystem.DirectoryExists(new StorageInfo() { HighName = highName, LowName = lowName });
        }

        public void DeleteFile(StorageInfo storageInfo)
        {
            StorageDeleteResult deleteResult = instanceSystem.DeleteFile(storageInfo);
            //Media API，DeleteFile出错不会抛出相关异常，只会在Message中返回相关异常信息。
            if (!string.IsNullOrEmpty(deleteResult.Message))
            {
                throw new Exception(deleteResult.Message);
            }
        }

        public void DeleteFolder(StorageInfo storageInfo)
        {
            instanceSystem.DeleteDirectory(storageInfo);
        }

        public XDirectoryInfo GetOrCreateDirectory(StorageInfo storageInfo)
        {
            try
            {
                return instanceSystem.OpenDirectory(storageInfo, FileMode.OpenOrCreate);
            }
            catch (Exception ex)
            {
                mLog.Info(string.Format("Can not get or create directory while merge VEO file. Message:{0}.", ex.ToString()));
            }
            return null;
        }

        public XDirectoryInfo GetOrCreateDirectory(string dirPath)
        {
            try
            {
                var separator = Path.DirectorySeparatorChar;
                var storageInfo = new StorageInfo() { HighName = dirPath, LowName = string.Empty };
                if (instanceSystem.StorageType == XStorageType.Azure && !instanceSystem.DirectoryExists(storageInfo))
                {
                    string path = dirPath.TrimEnd(new char[] { '/', '\\' });
                    int index = path.LastIndexOfAny(new char[] { '/', '\\' });
                    string highName = String.Empty;
                    string lowName = String.Empty;
                    if (index > 0)
                    {
                        highName = path.Substring(0, index);
                        lowName = path.Substring(index);
                    }
                    else
                    {
                        lowName = path;
                    }
                    /* Fortify Issue Type: Path Manipulation 
                    * Sink Details: Storage XDirectoryInfo 31  
                    *                RAExportCommon     MergeVEOBase 66
                    * Ignore Reason: 1.使用第三方dll，无法修改不安全代码 
                    *                2.调用处传入的pathname是预设的，不会出现用户恶意攻击问题
                    */
                    var dirInfo = new CloudDirectoryInfo(highName, lowName);
                    //dirInfo.LowName = "";
                    return dirInfo;
                }
                else
                {
                    return instanceSystem.OpenDirectory(storageInfo, FileMode.OpenOrCreate);
                }
            }
            catch (Exception ex)
            {
                mLog.Info(string.Format("Can not get or create directory while merge VEO file. Message:{0}.", ex.ToString()));
            }
            return null;
        }


        public List<XFileInfo> GetFiles(StorageInfo storageInfo)
        {
            return instanceSystem.ListFiles(storageInfo);
        }

        public List<XFileInfo> GetFiles(string storageInfo)
        {
            return instanceSystem.ListFiles(new StorageInfo() { HighName = storageInfo });
        }

        public List<XFileInfo> GetFiles(XDirectoryInfo directoryInfo)
        {
            return instanceSystem.ListFiles(directoryInfo);
        }

        public void MergeFile(StorageInfo source, StorageInfo des, bool isDeleteSourceFile)
        {
            this.MergeFile(source, des, true, isDeleteSourceFile);
        }

        public void MergeFile(StorageInfo source, StorageInfo des, bool isOverwrite, bool isDeleteSourceFile)
        {
            StorageCopyResult copyResult = instanceSystem.CopyFile(source, des, isOverwrite);
            //Media API，文件copy出错不会抛出相关异常，只会在Message中返回相关异常信息。
            if (!copyResult.IsCopyed)
            {

                throw new Exception(copyResult.Message);
            }
            if (isDeleteSourceFile)
            {
                this.DeleteFile(source);
            }
        }

        public void Close()
        {
            if (instanceSystem != null)
            {
                instanceSystem.Close();
            }
        }

        public void Dispose()
        {
            this.Close();
        }
    }
}
