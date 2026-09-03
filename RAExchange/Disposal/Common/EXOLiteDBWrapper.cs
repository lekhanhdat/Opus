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
using AvePoint.RA.RAExchange.Disposal.Object;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace AvePoint.RA.RAExchange.Disposal
{
    public class EXOLiteDBWrapper: IDisposable
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(EXOLiteDBWrapper));
        private readonly string _defaultCollectionName = "rec";
        private string _filename;
        private LiteDatabase _db;
        private static EXOLiteDBWrapper fileSystemSqliteWrapper = null;
        private readonly static object mLiteLock = new object();
        public static EXOLiteDBWrapper CreateInstance(string dbFilePath)
        {
            if (fileSystemSqliteWrapper == null)
            {
                lock (mLiteLock)
                {
                    if (fileSystemSqliteWrapper == null)
                    {
                        fileSystemSqliteWrapper = new EXOLiteDBWrapper(dbFilePath);
                    }
                }
            }
            return fileSystemSqliteWrapper;
        }
        private EXOLiteDBWrapper(string dbFilePath)
        {
            if (string.IsNullOrEmpty(dbFilePath))
            {
                throw new ArgumentNullException("filename");
            }
            _filename = dbFilePath;
            CheckExists(_filename);
            _db = new LiteDatabase(_filename);
        }

        public void Insert(List<EXOArchiveData> datas)
        {
            _db.GetCollection<EXOArchiveData>(_defaultCollectionName).InsertBulk(datas);

            _db.GetCollection<EXOArchiveData>(_defaultCollectionName).EnsureIndex(a => a.RuleId);           
        }

        public void EnsureIndex<TKey>(Expression<Func<EXOArchiveData, TKey>> keyselectror)
        {
            _db.GetCollection<EXOArchiveData>(_defaultCollectionName).EnsureIndex(_defaultCollectionName, keyselectror, false);
        }

        public List<EXOArchiveData> QueryAll(int pageIndex, int perPage, string orderColumn, bool isAsc = true)
        {
            Query q = LiteDB.Query.All();
            q.Order = isAsc ? 1 : -1;
            q.OrderBy = orderColumn;
            q.Limit = perPage;
            q.Offset = perPage * pageIndex;
            return _db.GetCollection<EXOArchiveData>(_defaultCollectionName).Find(q).ToList();
        }

        public List<EXOArchiveData> QueryAllByPage(int pageIndex, int perPage, string ruleId)
        {
            //Query q = LiteDB.Query.All();
            //q.Order = 1;
            //q.OrderBy = "FolderId";
            //q.Limit = perPage;
            //q.Offset = perPage * pageIndex;
            var expression = LiteDB.Query.EQ("RuleId", ruleId);
            return _db.GetCollection<EXOArchiveData>(_defaultCollectionName).Find(expression, perPage * pageIndex, perPage).ToList();
        }

        public List<string> GetAllRules()
        {            
            Query q = LiteDB.Query.All();
            List<string> rules = _db.GetCollection<EXOArchiveData>(_defaultCollectionName).Include(i => i.RuleId).Find(q).Select(i => i.RuleId).Distinct().ToList();            
            return rules;
        }

        public string GetTermIdByRuleId(string ruleId)
        {
            Query q = LiteDB.Query.All();
            var termId = _db.GetCollection<EXOArchiveData>(_defaultCollectionName).Include(i => i.RuleId).Find(q).Select(i => i.TermId).FirstOrDefault();
            return termId;
        }
        public int QueryCount()
        {
            return _db.GetCollection<EXOArchiveData>(_defaultCollectionName).Count();
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
                logger.Error($"error occured when Dispose3,error:{e}");
            }
        }

        public void DeleteDBFile()
        {
            try
            {
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
