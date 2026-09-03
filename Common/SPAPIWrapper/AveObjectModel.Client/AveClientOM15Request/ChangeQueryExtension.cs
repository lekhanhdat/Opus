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
using Microsoft.SharePoint.Client;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ClientOM
{
    public static class ChangeQueryExtension
    {
        public static void InitChangeQuery(this ChangeQuery query, AveQueryOption option)
        {
            query.Activity = option.Activity;
            query.Add = option.Add;
            query.Alert = option.Alert;
            query.ContentType = option.ContentType;
            query.DeleteObject = option.DeleteObject;
            query.FetchLimit = option.FetchLimit;
            query.Field = option.Field;
            query.File = option.File;
            query.Folder = option.Folder;
            query.Group = option.Group;
            query.GroupMembershipAdd = option.GroupMembershipAdd;
            query.GroupMembershipDelete = option.GroupMembershipDelete;
            query.Item = option.Item;
            query.LatestFirst = option.LatestFirst;
            query.List = option.List;
            query.Move = option.Move;
            query.Navigation = option.Navigation;
            query.RecursiveAll = option.RecursiveAll;
            query.Rename = option.Rename;
            query.RequireSecurityTrim = option.RequireSecurityTrim;
            query.Restore = option.Restore;
            query.RoleAssignmentAdd = option.RoleAssignmentAdd;
            query.RoleAssignmentDelete = option.RoleAssignmentDelete;
            query.RoleDefinitionAdd = option.RoleDefinitionAdd;
            query.RoleDefinitionDelete = option.RoleDefinitionDelete;
            query.RoleDefinitionUpdate = option.RoleDefinitionUpdate;
            query.SecurityPolicy = option.SecurityPolicy;
            query.Site = option.Site;
            query.SystemUpdate = option.SystemUpdate;
            query.Update = option.Update;
            query.User = option.User;
            query.View = option.View;
            query.Web = option.Web;
        }
    }
}
