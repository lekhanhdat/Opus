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

namespace AvePoint.Wrapper.Common
{
    public class AveListObject
    {
        public string ModifiedBy { get; set; }
        public Guid ListId { get; set; }
        public Guid RootFolderId { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int Type { get; set; }
        public int ItemCount { get; set; }
        public string RootFolderUrl { get; set; }
        public object Flag { get; set; }
        public ChangeType ChangeType { get; set; }
        public ChangeType ChangeTypeBeforeDelete { get; set; }
        public int? ServerTemplate { get; set; }
        public bool? Hidden { get; set; }
        public int? ListTemplate { get; set; }
        public DateTime ModifiedTime { get; set; }
        public List<AveSecurityObject> DeleteRoleAssignments = new List<AveSecurityObject>();//存放permission的删除事件
    }
}
