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
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using RAArchiverCommon.DestructionCache;

namespace AvePoint.RA.SharePoint.Common
{
    public class DestructionCacheLiteDBWrapper: IDisposable
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(DestructionCacheLiteDBWrapper));
        private readonly string _defaultCollectionName = "rec";
        private string _filename;
        private LiteDatabase _db;
        private static DestructionCacheLiteDBWrapper fileSystemSqliteWrapper = null;
        private readonly static object mLiteLock = new object();
        public static DestructionCacheLiteDBWrapper CreateInstance(string dbFilePath)
        {
            if (fileSystemSqliteWrapper == null)
            {
                lock (mLiteLock)
                {
                    if (fileSystemSqliteWrapper == null)
                    {
                        fileSystemSqliteWrapper = new DestructionCacheLiteDBWrapper(dbFilePath);
                    }
                }
            }
            return fileSystemSqliteWrapper;
        }
        private DestructionCacheLiteDBWrapper(string dbFilePath)
        {
            if (string.IsNullOrEmpty(dbFilePath))
            {
                throw new ArgumentNullException("filename");
            }
            _filename = dbFilePath;
            CheckExists(_filename);
            _db = new LiteDatabase(_filename);
            logger.Info($"Cache DB file {_filename}");
        }

        public void Insert(List<DestructionReport> datas)
        {
            _db.GetCollection<DestructionReport>(_defaultCollectionName).InsertBulk(datas);

            _db.GetCollection<DestructionReport>(_defaultCollectionName).EnsureIndex(a => a.ListId);           
        }

        public void EnsureIndex<TKey>(Expression<Func<DestructionReport, TKey>> keyselectror)
        {
            _db.GetCollection<DestructionReport>(_defaultCollectionName).EnsureIndex(_defaultCollectionName, keyselectror, false);
        }

        public List<DestructionReport> QueryAll(int pageIndex, int perPage, string orderColumn, bool isAsc = true)
        {
            Query q = LiteDB.Query.All();
            q.Order = isAsc ? 1 : -1;
            q.OrderBy = orderColumn;
            q.Limit = perPage;
            q.Offset = perPage * pageIndex;
            return _db.GetCollection<DestructionReport>(_defaultCollectionName).Find(q).ToList();
        }

        public List<DestructionReport> QueryAllByPage(int pageIndex, int perPage, Guid listId)
        {
            //Query q = LiteDB.Query.All();
            //q.Order = 1;
            //q.OrderBy = "FolderId";
            //q.Limit = perPage;
            //q.Offset = perPage * pageIndex;
            var expression = LiteDB.Query.EQ("ListId", listId);
            return _db.GetCollection<DestructionReport>(_defaultCollectionName).Find(expression, perPage * pageIndex, perPage).ToList();
        }

       
        public int QueryCount()
        {
            return _db.GetCollection<DestructionReport>(_defaultCollectionName).Count();
        }

        public long QueryCountByActionType(int actionType)
        {
            return _db.GetCollection<DestructionReport>(_defaultCollectionName).Find(Query.EQ("ActionType", actionType)).LongCount();
        }

        public long QueryCountByActionType(int actionType, string listID)
        {
            var express = Query.And(Query.EQ("ActionType", actionType), Query.EQ("ListId", new Guid(listID)));
            return _db.GetCollection<DestructionReport>(_defaultCollectionName).Find(express).LongCount();
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
            catch(Exception e)
            {
                logger.Error($"error occured when Dispose1,error:{e}");
            }
            DeleteDBFile();
            fileSystemSqliteWrapper = null;
        }

        public void DeleteDBFile()
        {
            try
            {
                logger.Info($"Delete DB file {_filename}");
                FileInfo fs = new FileInfo(_filename);
                if (!string.IsNullOrWhiteSpace(_filename) && fs.Directory.Exists)
                {
                    if (_db != null)
                    {
                        _db.Dispose();
                    }
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
