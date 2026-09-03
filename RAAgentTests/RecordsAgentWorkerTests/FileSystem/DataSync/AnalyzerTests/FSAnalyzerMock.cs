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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.Media.Storage;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.FileSystem.Core;
using AvePoint.RA.FileSystem.Stubs;
using Newtonsoft.Json;
using RAFileSystem.Utils;
using System.Security.AccessControl;


namespace RecordsAgentWorkerTests.FileSystem.DataSync.AnalyzerTests;

public class FSAnalyzerMock
{
    private readonly string _rootPath;

    public FSAnalyzerMock(string rootPath)
    {
        _rootPath = rootPath;
    }
    public void AssembleFolderBasicInfo(Stub stub)
    {
        XDirectoryInfoEx xObj = new XDirectoryInfoEx(stub.MediaObj);
        FileSystemRecordDto record = new FileSystemRecordDto();
        record.BulkImportEnabled = JobContext.Current.BulkImportEnabled;
        record.BulkSize = JobContext.Current.BulkSize;
        record.RecordStatus = 1;
        record.Depth = stub.Depth;
        record.AveSiteId = Guid.NewGuid().ToString();
        //record.CollectionTime = DateTime.UtcNow.Ticks;
        if (xObj.Owner.Contains('\\'))
        {
            var splitCreateBy = xObj.Owner.Split('\\');
            var createByName = splitCreateBy[1];
            if (createByName.Any(char.IsUpper) && createByName.Any(char.IsLower))
            {
                record.CreatedBy = xObj.Owner;
            }
            else
            {
                record.CreatedBy = string.Join("\\", splitCreateBy[0], createByName.ToLower());
            }
        }
        else
        {
            record.CreatedBy = xObj.Owner;
        }

        record.DirPath = stub.FullPath;
        record.FolderId = stub.ParentId;
        record.FullPath = stub.FullPath;
        record.ItemId = stub.SelfId;
        record.ItemRowId = -1;
        record.LeafName = xObj.Name;
        record.ListId = Guid.Empty;
        record.NodeId = stub.SelfId;
        record.NodeType = (int)NodeLevel.FSFolder;
        record.ScopeId = _rootPath.ToLowerInvariant().ToMd5();
        record.SourceFlag = (int)SourceFlag.FileSystem;
        record.TimeCreated1 = xObj.CreationTimeUtc;
        record.TimeLastModified = xObj.LastWriteTimeUtc.Ticks;
        record.ParentId = stub.ParentId;
        record.SortTicks = Snowflake.Instance().GetTicks();
        RecordMetaInfo metaInfo = new RecordMetaInfo
        {
            FileSize = xObj.Length,
            LocalFullPath = xObj.LocalFullPath
        };
        record.FileSize = xObj.Length;
        record.MetaInfo = JsonConvert.SerializeObject(metaInfo);
    }

}