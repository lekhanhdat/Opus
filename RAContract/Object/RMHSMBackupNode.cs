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
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Contract.Discovery.Model.Configuration.Office365;
using AvePoint.RA.Contract.Explorer;
using System;

namespace AvePoint.RA.Contract.Object
{
    public class RMHSMBackupNode
    {
        public string SiteUrl { get; set; }
        public Guid SettingId { get; set; }
        public long SiteInfoId { get; set; }
        public Guid O365TenantId { get; set; }
        public Guid SiteId { get; set; }
        public RemoveNodeType sourceFlag { get; set; }
        public RMSPTreeNode TreeNode { get; set; }
        public StorageDeviceUIDto SelectedStorage { get; set; }
        public string SourceDataStorageId { get; set; }
        public string DataContentStorageId { get; set; }
        public string StubTemplateId { get; set; }
        public bool SkipCheckFileExtension { get; set; }
        public string TraceId { get; set; }
    }
}
