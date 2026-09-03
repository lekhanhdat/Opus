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
using System.Collections.Generic;
using AvePoint.GCommon;
using AvePoint.RA.Contract.Services;
using AvePoint.Media.Storage;

namespace RAFileSystem.FileSystem.Collector
{
    public class FileSystemFolderCollector
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(FileSystemFolderCollector));
        private readonly StorageInfo dirInfo;
        private readonly IXSystem xSystem;
        private readonly string folderPath;

        public FileSystemFolderCollector(IXSystem xSystem, StorageInfo dirInfo, string folderPath)
        {
            this.xSystem = xSystem;
            this.dirInfo = dirInfo;
            this.folderPath = folderPath;
        }

        public List<XDirectoryInfo> Collect()
        {
            try
            {
                return xSystem.ListDirectories(dirInfo) ?? new List<XDirectoryInfo>();
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Warn($"Access denied: {folderPath.LogBase64()}. Skipped. Ex: {e}");
                throw new FileSystemCollectorUnauthorizedAccessException(folderPath, e);
            }
            catch (Exception e)
            {
                logger.Error($"Collect folders from {folderPath.LogBase64()} failed. Ex: {e}");
                throw;
            }
        }
        
        public IEnumerable<List<XDirectoryInfo>> CollectInBatch()
        {
            try
            {
                return xSystem.GetDirectoriesInBatch(dirInfo, 100);
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Warn($"Access denied: {folderPath.LogBase64()}. Skipped. Ex: {e}");
                throw new FileSystemCollectorUnauthorizedAccessException(folderPath, e);
            }
            catch (Exception e)
            {
                logger.Error($"Collect folders from {folderPath.LogBase64()} failed. Ex: {e}");
                throw;
            }
        }
    }
}
