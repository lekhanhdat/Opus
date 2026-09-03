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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class ExtRecordDao : IExtRecordDao
    {
       
        //public void AddDataToManagedExt(RMExtForManagedRecord extInfo)
        //{
        //    using (var ctx = GetNewContext())
        //    {
                
        //        var exist = ctx.ExtForManagedRecord.Any(d => d.ScopeId == extInfo.ScopeId && d.DirPath.Equals(extInfo.DirPath));
        //        if (!exist)
        //        {
        //            ctx.ExtForManagedRecord.Add(extInfo);
        //            ctx.SaveChanges();
        //        }
        //    }
        //}

        //public void AddDataToDestroyExt(RMExtForDestroyedRecord extInfo)
        //{
        //    using (var ctx = GetNewContext())
        //    {
        //        var exist = ctx.ExtForDestroyedRecord.Any(d => d.ScopeId == extInfo.ScopeId && d.DirPath.Equals(extInfo.DirPath));
        //        if (!exist)
        //        {
        //            ctx.ExtForDestroyedRecord.Add(extInfo);
        //            ctx.SaveChanges();
        //        }

        //    }
        //}

        //public void DeleteRecord(bool destroyed, Guid scopeId, string dirPath)
        //{
        //    using (var ctx = GetNewContext())
        //    {
        //        if (destroyed)
        //        {
        //            ctx.ExtForDestroyedRecord.Where(m => m.ScopeId == scopeId && m.DirPath.Equals(dirPath)).Delete();
        //        }
        //        else
        //        {
        //            ctx.ExtForManagedRecord.Where(m => m.ScopeId == scopeId && m.DirPath.Equals(dirPath)).Delete();
        //        }
        //    }
            
        //}

        //public RMBaseExtForRecord GetExtisionByKey(bool destroyed, Guid scopeId, string dirPath)
        //{
        //    RMBaseExtForRecord result = null;
        //    using (var ctx = GetNewContext())
        //    {
        //        if (destroyed)
        //        {
        //            result = ctx.ExtForDestroyedRecord.Where(d => d.ScopeId == scopeId && d.DirPath.Equals(dirPath)).FirstOrDefault();
                    
        //        }
        //        else
        //        {
        //            result = ctx.ExtForManagedRecord.Where(d => d.ScopeId == scopeId && d.DirPath.Equals(dirPath)).FirstOrDefault();
        //        }
        //    }
        //    return result;
        //}

       
        //public RMBaseExtForRecord GetMetaInfoByKey(bool destroyed, Guid scopeId, string dirPath)
        //{
        //    RMBaseExtForRecord result = null;
        //    using (var ctx = GetNewContext())
        //    {
        //        if (destroyed)
        //        {
        //            result = ctx.ExtForDestroyedRecord.Where(d => d.ScopeId == scopeId && d.DirPath.Equals(dirPath)).Select(m => new RMBaseExtForRecord() { FullPath = m.FullPath, MetaInfo = m.MetaInfo } ).FirstOrDefault();

        //        }
        //        else
        //        {
        //            result = ctx.ExtForManagedRecord.Where(d => d.ScopeId == scopeId && d.DirPath.Equals(dirPath)).Select(m => new RMBaseExtForRecord() { FullPath = m.FullPath, MetaInfo = m.MetaInfo }).FirstOrDefault();
        //        }
        //    }
        //    return result;
        //}

        //private Core.RMDbContext GetNewContext()
        //{
        //    return Core.RMDBContextManager.GetNewDBContext();
        //}

        
    }
}
