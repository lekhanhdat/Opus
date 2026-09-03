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
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface ISyncFailureItemDao
    {
        bool Add(string tenantGroupId, List<SyncFailureItemEntity> entities);

        List<SyncFailureItemEntity> GetAll(string tenantGroupId, string siteId);

        List<SyncFailureItemEntity> GetAllByDataSource(string tenantGroupId, string siteId, int dataSource);

        List<SyncFailureItemEntity> GetAllByDataSource(string tenantGroupId, string siteId, string listId, int dataSource);

        List<SyncFailureItemEntity> GetAll(string tenantGroupId, string siteId, string listId);

        List<SyncFailureItemEntity> GetDataByPage(string tenantGroupId, string siteId, int dataSource, long sortTicks, int pageSize);

        List<SyncFailureItemEntity> GetDataByPage(string tenantGroupId, string siteId, string listId, int dataSource, long sortTicks, int pageSize);

        bool Remove(string tenantGroupId, SyncFailureItemEntity entity);

        bool RemoveAll(string tenantGroupId, string siteId);
    }
}
