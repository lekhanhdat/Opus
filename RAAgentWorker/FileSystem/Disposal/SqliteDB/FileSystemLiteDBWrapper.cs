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

using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.FileSystem.Stubs;
using LiteDB;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using AvePoint.RA.FileSystem.Core;
using AvePoint.GCommon;

namespace RAFileSystem.Disposal
{
    public class FileSystemLiteDBWrapper : IDisposable
    {
        protected AveLogger logger = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly string _defaultCollectionName = "rec";
        private string _filename;
        private LiteDatabase _db;
        private static FileSystemLiteDBWrapper fileSystemSqliteWrapper = null;
        private static object mLiteLock = new object();
        public static FileSystemLiteDBWrapper CreateInstance(string dbFilePath)
        {
            if (fileSystemSqliteWrapper == null)
            {
                lock (mLiteLock)
                {
                    if (fileSystemSqliteWrapper == null)
                    {
                        fileSystemSqliteWrapper = new FileSystemLiteDBWrapper(dbFilePath);
                    }
                }
            }
            return fileSystemSqliteWrapper;
        }
        private FileSystemLiteDBWrapper(string dbFilePath)
        {
            if (string.IsNullOrEmpty(dbFilePath))
            {
                throw new ArgumentNullException("filename");
            }
            _filename = dbFilePath;
            CheckExists(_filename);
            _db = new LiteDatabase(_filename);
        }

        public void Insert(List<FileSystemRecordDto> datas)
        {
            _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).InsertBulk(datas);

            _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(a => a.FolderId);
            //_db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(a => a.RecordsId);
            //_db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(a => a.TimeCreated);
            //_db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(a => a.TimeModified);
            //_db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(a => a.CreatedBy);
            //_db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(a => a.ModifiedBy);
            //_db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(a => a.DeclaredBy);
            //_db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(a => a.HoldStatus);
        }

        public void EnsureIndex<TKey>(Expression<Func<FileSystemRecordDto, TKey>> keyselectror)
        {
            _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).EnsureIndex(_defaultCollectionName, keyselectror, false);
        }

        public List<FileSystemRecordDto> QueryAll(int pageIndex, int perPage, string orderColumn, bool isAsc = true)
        {
            Query q = LiteDB.Query.All();
            q.Order = isAsc ? 1 : -1;
            q.OrderBy = orderColumn;
            q.Limit = perPage;
            q.Offset = perPage * pageIndex;
            return _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).Find(q).ToList();
        }

        public List<FileSystemRecordDto> QueryAllByPage(int pageIndex, int perPage, Guid folderId)
        {
            //Query q = LiteDB.Query.All();
            //q.Order = 1;
            //q.OrderBy = "FolderId";
            //q.Limit = perPage;
            //q.Offset = perPage * pageIndex;
            var expression = LiteDB.Query.EQ("FolderId", folderId);
            return _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).Find(expression, perPage * pageIndex, perPage).ToList();
        }
        
        public List<FileSystemRecordDto> QueryBySelfIds(IEnumerable<Guid> selfIds)
        {
            var nodeIds = selfIds.Select(id => new BsonValue(id));
            var expression = LiteDB.Query.In("NodeId", nodeIds);
            return _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).Find(expression).ToList();
        }

        public List<FSDisposalDiscoverFolder> GetDisposalDiscoverFolders()
        {
            List<FSDisposalDiscoverFolder> allFolderCache = new List<FSDisposalDiscoverFolder>();
            Query q = LiteDB.Query.All();
            var folderIds = _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).Include(i => i.FolderId).Find(q).Select(i => i.FolderId).Distinct().ToList();
            foreach (var folderId in folderIds)
            {
                var expression = LiteDB.Query.EQ("FolderId", folderId);
                var folderPath = _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).FindOne(expression).DirPath;
                allFolderCache.Add(new FSDisposalDiscoverFolder()
                {
                    FolderId = folderId,
                    FolderPath = folderPath
                });
            }
            return allFolderCache;
        }
        
        public List<FSDisposalDiscoverFolder> GetDisposalDiscoverFoldersV2()
        {
            return _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName)
                .Query()
                .Select(record => new
                {
                    record.FolderId,
                    record.DirPath,
                    record.LeafName,
                    record.NodeId
                })
                .ToEnumerable()
                .GroupBy(record => record.NodeId)
                .Select(group => new FSDisposalDiscoverFolder
                {
                    FolderId = group.Key,
                    FolderPath = group.Select(item => item.DirPath+"\\"+ item.LeafName).FirstOrDefault(path => !string.IsNullOrEmpty(path))
                })
                .ToList();
        }
        public int QueryCount()
        {
            return _db.GetCollection<FileSystemRecordDto>(_defaultCollectionName).Count();
        }
        private void CheckExists(string filename)
        {
            try
            {
                FileInfo fs = new FileInfo(filename);
                if (!fs.Directory.Exists)
                {
                    fs.Directory.Create();
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
        }

        public void Dispose()
        {
            try
            {
                if (_db != null)
                {
                    _db.Dispose();
                }
            }
            catch
            {

            }
        }

        public void DeleteDBFile()
        {
            try
            {
                FileInfo fs = new FileInfo(_filename);
                if (!string.IsNullOrWhiteSpace(JobContext.Current.JobId) && fs.Directory.FullName.Contains(JobContext.Current.JobId) && fs.Directory.Exists)
                {
                    fs.Directory.Delete(true);
                }
            }
            catch (Exception e)
            {
                logger.Warn(e.Message, e);
            }
        }
    }
}
