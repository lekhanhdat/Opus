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
using AvePoint.Hybrid.Utility.Cryptography;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Utils;
using RAFileSystem.FileSystem.Collector;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RAFileSystem.FileSystem.FileSystem.Collector.FilterImplement
{
    public class FileSystemFileDisposalFilter : IFileSystemFilter
    {
        private IEnumerable<string> ValidFolderPaths { get; set; }

        public FileSystemFileDisposalFilter(IEnumerable<string> validFolderPaths)
        {
            ValidFolderPaths = validFolderPaths;
        }

        public bool ShouldDiscoverDirectory(object filterObj = null)
        {
            return true;
        }

        public bool ShouldIncludeDirectory(object filterObj = null)
        {
            return true;
        }

        public bool ShouldIncludeFile(object filterObj = null)
        {
            if (filterObj is XFileInfo fileInfo)
            {
                if (fileInfo.Name.IndexOf(".stub.html", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }
                var originalPath = System.IO.Path.GetDirectoryName(fileInfo.OriginalFileFullPath);
                return ValidFolderPaths.Contains(originalPath);
            }

            return false;

        }
    }

    public class FileSystemFolderDisposalFilter : IFileSystemFilter
    {
        private AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<string> BreakNodeUrls { get; }
        public List<string> RunningJobNodeUrls { get; }
        public Dictionary<Guid, FSSettingDto> ScopeSettingCache { get; }

        public FileSystemFolderDisposalFilter(List<string> breakNodeUrls, List<string> runningJobNodeUrls1, Dictionary<Guid, FSSettingDto> scopeSettingCache)
        {
            BreakNodeUrls = breakNodeUrls;
            RunningJobNodeUrls = runningJobNodeUrls1;
            ScopeSettingCache = scopeSettingCache;
        }

        public bool ShouldDiscoverDirectory(object filterObj = null)
        {
            if (filterObj is XDirectoryInfo folderInfo)
            {
                string fullPath = ExternalUtil.CombinePath(folderInfo.OriginalDirFullPath, folderInfo.Name);
                Guid id = fullPath.ToLowerInvariant().ToMd5();
                string sha1Url = RAEncodeUtil.EncryptBySHA1(fullPath.ToLowerInvariant());
                if (BreakNodeUrls != null && BreakNodeUrls.Contains(sha1Url))
                {
                    logger.Debug("The folder node {0} has unique setting.", fullPath);
                    return false;
                }
                if (ScopeSettingCache != null && ScopeSettingCache.ContainsKey(id))
                {
                    if (!ScopeSettingCache[id].IsActive)
                    {
                        logger.Debug("The folder node {0}  has been deactived.", fullPath);
                        return false;
                    }
                }
                if (RunningJobNodeUrls != null && RunningJobNodeUrls.Contains(sha1Url))
                {
                    logger.Debug("There is already a job running on this node. Path:{0}", fullPath);
                    return false;
                }
                return true;
            }

            return false;
        }

        public bool ShouldIncludeDirectory(object filterObj = null)
        {
            return true;
        }

        public bool ShouldIncludeFile(object filterObj = null)
        {
            if (filterObj is XFileInfo fileInfo)
            {
                if (fileInfo.Name.IndexOf(".stub.html", StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    return false;
                }

                return true;
            }

            return false;
        }
    }
}
