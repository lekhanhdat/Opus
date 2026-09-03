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
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Dashboard.Query
{
    public class SharePointDashboardQuery : DashboardQueryable
    {
        public override SourceFlag Flag => SourceFlag.SharePoint;

        public override long GetApplySettingCount(SecurityUserPermissionsDto permission)
        {
            if (permission.IsAdmin)
            {
                return DashboardDao.GetSharePointSettingCount();
            }
            var containerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == Flag)?.ScopeIds;
            return DashboardDao.GetSharePointSettingCount(containerIds);
        }

        public override long GetSourceActiveCount(SecurityUserPermissionsDto permission)
        {
            if (permission.IsAdmin)
            {
                return DashboardDao.GetSourceActiveCount(Flag);
            }
            var containerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == Flag)?.ScopeIds.ConvertAll(item => item.ToString());
            return DashboardDao.GetSourceActiveCount(Flag, containerIds);
        }

        public override long GetSourceDestroyedCount(SecurityUserPermissionsDto permission)
        {
            if (permission.IsAdmin)
            {
                return DashboardDao.GetSourceDestroyedCount(Flag);
            }
            var containerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == Flag)?.ScopeIds.ConvertAll(item => item.ToString());
            return DashboardDao.GetSourceDestroyedCount(Flag, containerIds);
        }

        public override long GetSourceArchivedCount(SecurityUserPermissionsDto permission)
        {
            if (permission.IsAdmin)
            {
                return DashboardDao.GetSourceArchivedCount(Flag);
            }
            var containerIds = permission.ScopePermissionInfo.Find(item => item.DataSourceType == Flag)?.ScopeIds.ConvertAll(item => item.ToString());
            return DashboardDao.GetSourceArchivedCount(Flag, containerIds);
        }
    }
}
