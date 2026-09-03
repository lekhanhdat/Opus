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
using AvePoint.Wrapper.Common;
using System.Collections.Generic;

namespace AvePoint.Wrapper.Discovery
{
    public interface IAveDiscoverObjectInfo
    {
        int? ID { get; set; } //DocLibRowID
        Guid DocID { get; set; }
        Guid tp_GUID { get; set; }
        ChangeType ChangeType { get; set; }
        ChangeType RoleAssignmentsChangeType { get; }
        ChangeType AlertChangeType { get; }
        bool isRename { get; set; }
        ItemType ObjType { get; set; }
        string SourceName { get; set; } //上次FB时的LeafName，为了在IB处理rename的情况
        string FullUrl { get; set; }  //List Releated
        string ItemName { get; set; }//当前Item上一次Rename的Name,为了在IB处理rename的情况，对应的EventCache表的ItemName
        long Size { get; set; }
        string ModifyBy { get; set; }
        string CreatedBy { get; set; }
        DateTime TimeLastModified { get; set; }
        string DirName { get; set; }
        string LeafName { get; set; } //当前的LeafName
        byte Level { get; set; }
        int Uiversion { get; set; }
        string UiVersionString { get; set; }
        bool IsCurrentVersion { get; set; }
        Guid ParentID { get; set; }
        byte Type { get; set; }
        DateTime TimeCreated { get; set; }
        int? DocFlags { get; set; }
        byte[] RbsId { get; set; }
        DateTime EventTime { get; set; }
        int? CheckoutUserId { get; set; }
        bool HasStream { get; set; }
        bool? Hidden { get; set; }
        int QueryType { get; set; } //Just For Extender. 2 is from Alldocs,3 is from alldocversions
        byte[] Content { get; set; } //Just For Extender
        bool ItemPermissionChanged { get; set; }
        List<AveSecurityObject> DeleteRoleAssignments { get; set; }
        byte[] DeleteTransactionId { get; set; }
    }
}

