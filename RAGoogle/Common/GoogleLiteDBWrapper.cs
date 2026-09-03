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
using AvePoint.RA.Contract.Services;
using LiteDB;
using RAGoogle.Models.Contract;
using System.Linq.Expressions;

namespace RAGoogle.Common
{
    public class GoogleLiteDBWrapper : IDisposable
    {
        private static readonly IRALogger logger = RALogger.GetInstance(typeof(GoogleLiteDBWrapper));
        private readonly string _defaultCollectionName = "rec";
        private readonly string _fileName;
        private readonly LiteDatabase _db;
        private static GoogleLiteDBWrapper _instanceWrapper = null;
        private readonly static object _liteLock = new object();

        private GoogleLiteDBWrapper(string dbFilePath)
        {
            if (string.IsNullOrEmpty(dbFilePath))
            {
                throw new ArgumentNullException("filename");
            }
            _fileName = dbFilePath;
            CheckExist(_fileName);
            _db = new LiteDatabase(_fileName);
        }

        public static GoogleLiteDBWrapper CreateInstance(string dbFilePath)
        {
            if (_instanceWrapper == null)
            {
                lock (_liteLock)
                {
                    if (_instanceWrapper == null)
                    {
                        _instanceWrapper = new GoogleLiteDBWrapper(dbFilePath);
                    }
                }
            }
            return _instanceWrapper;
        }

        public void Insert(List<GoogleDestructionData> datas)
        {
            _db.GetCollection<GoogleDestructionData>(_defaultCollectionName).InsertBulk(datas);

            _db.GetCollection<GoogleDestructionData>(_defaultCollectionName).EnsureIndex(a => a.ScopeId);
        }

        public void EnsureIndex<TKey>(Expression<Func<GoogleDestructionData, TKey>> keyselector)
        {
            _db.GetCollection<GoogleDestructionData>(_defaultCollectionName).EnsureIndex(_defaultCollectionName, keyselector, false);
        }

        public List<GoogleDestructionData> QueryAll(int pageIndex, int perPage, string orderColumn, bool isAsc = false)
        {
            Query q = Query.All();
            q.Order = isAsc ? 1 : -1;
            q.OrderBy = orderColumn;
            q.Limit = perPage;
            q.Offset = perPage * pageIndex;
            return _db.GetCollection<GoogleDestructionData>(_defaultCollectionName).Find(q).ToList();
        }

        public List<GoogleDestructionData> QueryAllByPage(int pageIndex, int perPage, string scopeId)
        {
            var expression = Query.EQ("ScopeId", scopeId);
            return _db.GetCollection<GoogleDestructionData>(_defaultCollectionName).Find(expression, perPage * pageIndex, perPage).ToList();
        }
        public List<GoogleDestructionData> QueryAllByScopeIdAndPage(int pageIndex, int perPage, string scopeId)
        {
            var expression = Query.EQ("ScopeId", scopeId);
            return _db.GetCollection<GoogleDestructionData>(_defaultCollectionName).Find(expression, perPage * pageIndex, perPage).ToList();
        }

        public int QueryCount()
        {
            return _db.GetCollection<GoogleDestructionData>(_defaultCollectionName).Count();
        }



        #region validation

        private void CheckExist(string fileName)
        {
            try
            {
                FileInfo fs = new FileInfo(fileName);
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

        public void DeleteDBFile()
        {
            try
            {
                FileInfo fs = new FileInfo(_fileName);
                if (!string.IsNullOrEmpty(_fileName) && fs.Directory.Exists)
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

        public void Dispose()
        {
            try
            {
                if (_db != null)
                {
                    _db.Dispose();
                }
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
            }
            DeleteDBFile();
            _instanceWrapper = null;
        }
        #endregion
    }
}
