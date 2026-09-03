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

namespace AvePoint.Media.Storage.FS
{
    interface IFSClient
    {
        XStream OpenStream(StorageInfo info, FileMode fileMode);

        XDirectoryInfo OpenDirectory(StorageInfo dirInfo, FileMode mode);

        XFileInfo OpenFile(StorageInfo fileInfo);

        StorageDeleteResult DeleteDirectory(StorageInfo info);

        StorageDeleteResult DeleteFile(StorageInfo info);

        List<XDirectoryInfo> ListDirectories(StorageInfo dirInfo);

        List<XFileInfo> ListFiles(StorageInfo dirInfo);

        IEnumerable<List<XDirectoryInfo>> ListDirectoriesInBatches(StorageInfo dirInfo, int batchSize);

        IEnumerable<List<XFileInfo>> ListFilesInBatches(StorageInfo dirInfo, int batchSize);

        StorageListResult ListSubDirectoriesAndFiles(StorageInfo dirInfo);

        StorageListResultSafety ListSubDirectoriesAndFilesSafety(StorageInfo dirInfo);

        bool DirectoryExists(StorageInfo info);

        bool FileExists(StorageInfo info);

        StorageCopyResult CopyFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        StorageMoveResult MoveFile(StorageInfo sourceFileInfo, StorageInfo targetFileInfo, bool isOverWrite);

        StorageMoveResult MoveDirectory(StorageInfo sourceDirInfo, StorageInfo targetDirInfo, bool isOverWrite);

        StorageCopyResult CopyFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite);

        StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem);

        StorageMoveResult MoveFile(StorageInfo srcFile, IXSystem destSystem, StorageInfo destFile, bool isOverWrite);

        void Close();

        Boolean ConvertLongPathToSymlink(String symlinkPath, String targetPath);

        SpeedResults GetNetshareSpeed(IOType type, int writeRatio, string blokeSize, string fileUNCPath);
    }
}
