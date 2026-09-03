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
using System.IO;
using AvePoint.Media.Storage.Util;

namespace AvePoint.Media.Storage.FS
{
    abstract class AbstractFSClient : IFSClient
    {

        public virtual string CombinePath(string firstPath, string secondPath)
        {
            if (string.IsNullOrEmpty(firstPath))
            {
                return secondPath;
            }
            if (string.IsNullOrEmpty(secondPath) || secondPath.Equals("\\", StringComparison.OrdinalIgnoreCase))
            {
                return firstPath;
            }
            if (secondPath.StartsWith("\\", StringComparison.OrdinalIgnoreCase))
            {
                secondPath = secondPath.TrimStart(new char[] { '\\' });
            }
            return Path.Combine(firstPath, secondPath);
        }

        protected virtual void WipeFile(StorageInfo info)
        {
            var fileInfo = OpenFile(info);
            if (fileInfo != null && fileInfo.Exists)
            {
                var b = new Byte[fileInfo.FileSize];
                info.BufferSize = 1024 * 1024;
                for (int i = 0; i < FSSystemConst.SECURELY_DELETE_WRITTEN_COUNT; i++)
                {
                    using (var stream = OpenStream(info, FileMode.OpenOrCreate))
                    {
                        stream.Position = 0;
                        stream.Write(b, 0, b.Length);
                        stream.SetLength(0);
                    }
                }
            }
        }

        public abstract XStream OpenStream(StorageInfo info, FileMode fileMode);
        public abstract XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode);

        public abstract XFileInfo OpenFile(StorageInfo fileInfo);

        public abstract StorageDeleteResult DeleteDirectory(StorageInfo info);

        public abstract StorageDeleteResult DeleteFile(StorageInfo info);

        public abstract List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo);

        public abstract List<XFileInfo> ListFiles(StorageInfo dirInfo);

        public abstract IEnumerable<List<XDirectoryInfo>> ListDirectoriesInBatches(StorageInfo dirInfo, int batchSize);

        public abstract IEnumerable<List<XFileInfo>> ListFilesInBatches(StorageInfo dirInfo, int batchSize);

        public abstract StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo);

        public abstract StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo);

        public abstract bool DirectoryExists(StorageInfo info);

        public abstract bool FileExists(StorageInfo info);

        public abstract StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        public abstract StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        public abstract StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite);

        public abstract StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite);

        public abstract StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem);

        public abstract StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite);

        public virtual void Close()
        {
            //do nothing
        }

        public virtual Boolean ConvertLongPathToSymlink(String symlinkPath, String targetPath)
        {
            return true;
        }

        public virtual SpeedResults GetNetshareSpeed(IOType type, int writeRatio, string blokeSize, string fileUNCPath)
        {
            throw new NotImplementedException("Not Implemented in this layer.");
        }
    }

    class FSClientOpenParam
    {
        public FSIdentity StorageIdentity { get; set; }
        public AbstractXSystem StorageSystem { get; set; }
        public string SystemLocation { get; set; }
        public string SystemUserName { get; set; }
        public string SystemDomain { get; set; }
        public string SystemPassword { get; set; }
        public string OriginalSystemLocation { get; set; }
        public ModuleType ModuleType { get; set; }
        public bool IsReadonly { get; set; }
        public bool securelyDelete { get; set; }
    }
}
