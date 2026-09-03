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



namespace AvePoint.GCommon.Media.StorageService
{
    #region directives

    using AvePoint.Media.Storage;
    using System;
    using System.Collections.Generic;

    #endregion directives

    public class StorageManager
        : IStorageManager
    {
        private StorageManagerInfo storageDeviceManagerInfo;
        private IXSystem physicalDevice;
        private List<String> directoriseList;

        public void Open(StorageManagerInfo storageManagerInfo)
        {
            this.storageDeviceManagerInfo = storageManagerInfo;
            this.directoriseList = new List<String>();
            this.physicalDevice = XFactory.InstanceSystem(this.storageDeviceManagerInfo.PhysicalDevice.BuildXRI());
            this.physicalDevice.Open();
        }

        public MediaCommonStorageResult DeleteFile(StorageFileInfo storageFileInfo)
        {
            MediaCommonStorageResult result = new MediaCommonStorageResult();
            StorageInfo storageInfo = new StorageInfo() { HighName = storageFileInfo.FileContainer, LowName = storageFileInfo.FileName };
            var tempResult = this.physicalDevice.DeleteFile(storageInfo);
            result.IsDeleted = tempResult.IsDeleted;
            return result;
        }

        /// <summary>
        /// 删除device下所有空文件夹
        /// </summary>
        /// <returns></returns>
        public MediaCommonStorageResult DeleteDirectory()
        {
            MediaCommonStorageResult result = new MediaCommonStorageResult();
            StorageInfo storageInfo = new StorageInfo();
            this.InnerDeleteDirectory(storageInfo);
            result.deletedPath = this.directoriseList;
            return result;
        }

        public MediaCommonStorageResult DeleteDirectoryIfEmpty(String highName)
        {
            var storageInfo = new StorageInfo(highName, "");
            var result = new MediaCommonStorageResult();
            if (this.physicalDevice.DirectoryExists(storageInfo))
            {
                var storageListResult = this.physicalDevice.ListSubDirectoriesAndFiles(storageInfo);
                if (storageListResult.Files.Count <= 0 && storageListResult.SubDirs.Count <= 0)
                {
                    if (this.physicalDevice.DeleteDirectory(storageInfo).IsDeleted)
                        this.directoriseList.Add(storageInfo.HighName);
                }
                result.deletedPath = this.directoriseList;
            }
            else
            {
                result.IsDeleted = true;
                result.deletedPath = new List<string> { storageInfo.HighPlusLowName };
            }
            return result;
        }

        public void Close()
        {
            if (this.physicalDevice != null)
            {
                this.physicalDevice.Close();
            }
        }

        private void InnerDeleteDirectory(StorageInfo storageInfo)
        {
            List<XDirectoryInfo> directorise = this.physicalDevice.ListDirectories(storageInfo);
            foreach (var directory in directorise)
            {
                var result = this.physicalDevice.ListSubDirectoriesAndFiles(directory);
                if (result.Files.Count == 0 && result.SubDirs.Count == 0)
                {
                    if (this.physicalDevice.DeleteDirectory(directory).IsDeleted)
                        directoriseList.Add(directory.HighPlusLowName);
                }
                else
                    InnerDeleteDirectory(directory);
            }
        }
    }
}