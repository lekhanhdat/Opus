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
using AvePoint.Media.Storage;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using AvePoint.RA.FileSystem.Utils;

namespace RecordsAgentWorkerTests.FileSystem.DataSync;

public class FSDiscoverMock
{
    private IXSystem _system;

    private string _rootPath;
    private string _highName;

    public FSDiscoverMock(string rootPath, string highName)
    {
        _rootPath = rootPath;
        _highName = highName;
        _system = ExternalUtil.OpenXSystem(_rootPath);
  
    }
    
    public List<Stub> GetAllFiles()
    {
        StorageInfo dirInfo = new StorageInfo() { HighName = _highName };
        XDirectoryInfo dir = _system.OpenDirectory(dirInfo, FileMode.Open);
        List<XFileInfo> files = _system.ListFiles(dir);
        List<Stub> fileStubs = new List<Stub>();
        files.ForEach(t =>
        {
            string fullPath = ExternalUtil.CombinePath(_rootPath, t.HighName, t.LowName);
            var fileId = fullPath.ToLowerInvariant().ToMd5();
            fileStubs.Add(new FSFileStub
            {
                FullPath = fullPath,
                MediaObj = t,
                SelfId = fileId,
                ParentId = Guid.NewGuid(),
                ScopeSettingId = Guid.NewGuid(),
                failedInPreJob = false,
            });
        });
        return fileStubs;
    }

    public List<Stub> QuerySubFoldersFileLevel()
    {
        StorageInfo dirInfo = new StorageInfo() { HighName = _highName };
        XDirectoryInfo currentDir = _system.OpenDirectory(dirInfo, FileMode.Open);
        var dirs = _system.ListDirectories(currentDir);
        var dirStubs = new List<Stub>(dirs.Count);

        foreach (var dir in dirs)
        {
            var fullPath = ExternalUtil.CombinePath(_rootPath, dir.HighName, dir.LowName);
            var normalizedPath = fullPath.ToLowerInvariant();
            var selfId = normalizedPath.ToMd5();

            dirStubs.Add(new FSFolderStub
            {
                FullPath = fullPath,
                MediaObj = dir,
                ScopeSettingId = Guid.NewGuid(),
                SelfId = selfId,
                ParentId = Guid.NewGuid(),
                failedInPreJob = false,
                Depth = 1,
            });
        }

        return dirStubs;
    }
    
    public IEnumerable<List<Stub>> GetFilesInBatch()
    {
        StorageInfo dirInfo = new StorageInfo() { HighName = _highName };
        XDirectoryInfo dir = _system.OpenDirectory(dirInfo, FileMode.Open);
        int batchSize = 100;
        var files = _system.GetFilesInBatch(dir, batchSize);
        var batch = new List<Stub>(batchSize);
        foreach (var batchFiles in files)
        {
            batchFiles.ForEach(t =>
            {
                string fullPath = ExternalUtil.CombinePath(_rootPath, t.HighName, t.LowName);
                var fileId = fullPath.ToLowerInvariant().ToMd5();
                batch.Add(new FSFileStub
                {
                    FullPath = fullPath,
                    MediaObj = t,
                    SelfId = fileId,
                    ParentId = Guid.NewGuid(),
                    ScopeSettingId = Guid.NewGuid(),
                    failedInPreJob = false,
                });
            });
            yield return batch;
            batch = new List<Stub>(batchSize);
        }
    }
}