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
using AvePoint.RA.Common.Report;
using AvePoint.RA.Contract.DocAve;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Explorer.Dao;
using System.Collections.Generic;

namespace AvePoint.RA.Service.Services.SecurityContainer.Job
{
    internal class RMSPOContainersSyncProcessor : RMSPOOneDriverContainersSyncProcessor
    {
        public RMSPOContainersSyncProcessor(IRMReportManager reportManger, ISPSettingTreeService spSettingTreeService, 
            IRMSecurityContainerService rmSecurityContainerService, IRMScopeRoleAssignmentDao scopeRoleAssignmentDao, 
            IRMSecurityContainerDao securityContainerDao, IExplorerDao explorerDao, IList<string> containerInGroups)
            : base(reportManger, spSettingTreeService, rmSecurityContainerService, scopeRoleAssignmentDao, securityContainerDao, explorerDao, containerInGroups, SourceFlag.SharePoint)
        {
        }

        protected override RMBrowseTreeNodeSourceType GetBrowseTreeNodeSourceType()
        {
            return RMBrowseTreeNodeSourceType.SharepointOnline;
        }
    }
}
