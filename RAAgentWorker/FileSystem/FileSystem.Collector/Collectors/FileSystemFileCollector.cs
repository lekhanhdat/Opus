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
using AvePoint.RA.FileSystem.Collect;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;

namespace RAFileSystem.FileSystem.Collector
{
    public class FileSystemFileCollector
    {
        private readonly AveLogger logger = AveLogger.GetInstance(typeof(FileSystemFileCollector));
        private readonly StorageInfo dirInfo;
        private readonly IXSystem xSystem;
        private readonly string folderPath;
        private readonly long depth;
        private readonly IFileSystemFilter systemFilter;

        public FileSystemFileCollector(IXSystem xSystem, StorageInfo dirInfo, string folderPath)
        {
            this.xSystem = xSystem;
            this.dirInfo = dirInfo;
            this.folderPath = folderPath;
        }
        
        public FileSystemFileCollector(IXSystem xSystem, StorageInfo dirInfo, string folderPath, long depth, IFileSystemFilter systemFilter)
        {
            this.xSystem = xSystem;
            this.dirInfo = dirInfo;
            this.folderPath = folderPath;
            this.depth = depth;
            this.systemFilter = systemFilter;
        }

        public List<XFileInfo> Collect()
        {
            try
            {
                return xSystem.ListFiles(dirInfo) ?? new List<XFileInfo>();
            }
            catch (UnauthorizedAccessException e)
            {
                logger.Warn($"Access denied: {folderPath.LogBase64()}. Skipped. Ex: {e}");
                throw new FileSystemCollectorUnauthorizedAccessException(folderPath, e);
            }
            catch (Exception e)
            {
                logger.Error($"Collect files from {folderPath.LogBase64()} failed. Ex: {e}");
                throw;
            }
        }
        
        public IEnumerable<List<FSFileStub>> CollectInBatch()
        {
            foreach (var files in xSystem.GetFilesInBatch(dirInfo, 100))
            {
                List<FSFileStub> result = new List<FSFileStub>();
                files.ForEach(file =>
                {
                    if (systemFilter.ShouldIncludeFile(file))
                    {
                            
                        string fullPath = ExternalUtil.CombinePath(FSJobCache.Instance.RootPath, file.HighName, file.LowName);
                        FSFileStub fileStub = new FSFileStub()
                        {
                            FullPath = fullPath,
                            MediaObj = file,
                            SelfId = fullPath.ToLowerInvariant().ToMd5(),
                            ParentId = folderPath.ToMd5(),
                            ScopeSettingId = FSJobCache.Instance.DispoalSettingScopeId,
                            Depth = depth
                        };
                        result.Add(fileStub);
                    }
                });
                yield return result;
            } 
        }

        public int GetFilesCount()
        {
            try
            {
                if (dirInfo is XDirectoryInfo directoryInfo)
                {
                    return directoryInfo.FileCount;
                }

                return Collect().Count;
            }
            catch (Exception ex)
            {
                logger.Error($"GetFilesCount failed. Ex: {ex}");
                return -1;
            }
        }
    }
}
