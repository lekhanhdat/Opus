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

namespace AvePoint.Wrapper.Common
{
    public class AveQueryOption
    {
        public AveQueryOption() : this(false, false) { }
        public AveQueryOption(bool allChangeObjectTypes, bool allChangeTypes)
        {
            if (allChangeObjectTypes)
            {
                Item = true;
                List = true;
                Web = true;
                Site = true;
                File = true;
                Folder = true;
                Alert = true;
                User = true;
                Group = true;
                ContentType = true;
                Field = true;
                SecurityPolicy = true;
                View = true;
            }
            if (allChangeTypes)
            {
                Add = true;
                Update = true;
                DeleteObject = true;
                Rename = true;
                Move = true;
                Restore = true;
                RoleDefinitionAdd = true;
                RoleDefinitionDelete = true;
                RoleDefinitionUpdate = true;
                RoleAssignmentAdd = true;
                RoleAssignmentDelete = true;
                GroupMembershipAdd = true;
                GroupMembershipDelete = true;
                SystemUpdate = true;
                Navigation = true;
            }
        }
        public bool Activity { get; set; }
        public bool Add { get; set; }
        public bool Alert { get; set; }
        public bool ContentType { get; set; }
        public bool DeleteObject { get; set; }
        public long FetchLimit { get; set; }
        public bool Field { get; set; }
        public bool File { get; set; }
        public bool Folder { get; set; }
        public bool Group { get; set; }
        public bool GroupMembershipAdd { get; set; }
        public bool GroupMembershipDelete { get; set; }
        public bool Item { get; set; }
        public bool LatestFirst { get; set; }
        public bool List { get; set; }
        public bool Move { get; set; }
        public bool Navigation { get; set; }
        public bool RecursiveAll { get; set; }
        public bool Rename { get; set; }
        public bool RequireSecurityTrim { get; set; }
        public bool Restore { get; set; }
        public bool RoleAssignmentAdd { get; set; }
        public bool RoleAssignmentDelete { get; set; }
        public bool RoleDefinitionAdd { get; set; }
        public bool RoleDefinitionDelete { get; set; }
        public bool RoleDefinitionUpdate { get; set; }
        public bool SecurityPolicy { get; set; }
        public bool Site { get; set; }
        public bool SystemUpdate { get; set; }
        public bool Update { get; set; }
        public bool User { get; set; }
        public bool View { get; set; }
        public bool Web { get; set; }
    }
}
