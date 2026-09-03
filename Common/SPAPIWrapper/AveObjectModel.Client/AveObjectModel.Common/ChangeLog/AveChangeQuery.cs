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

using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.ObjectModel.Common
{
    class AveChangeQuery : AveClientObject, IAveChangeQuery
    {
        public AveChangeQuery() : this(false, false) { }
        public AveChangeQuery(bool allChangeObjectTypes, bool allChangeTypes)
        {
            this.DataCache.AddProperty("allChangeObjectTypes",allChangeObjectTypes);
            this.DataCache.AddProperty("allChangeTypes",allChangeTypes);
        }

        public bool Add
        {
            get { return this.DataCache.GetProperty<bool>("Add"); }
            set { this.DataCache.AddProperty("Add",value); }
        }

        public bool Alert
        {
            get { return this.DataCache.GetProperty<bool>("Alert"); }
            set { this.DataCache.AddProperty("Alert",value); }
        }

        public IAveChangeToken ChangeTokenEnd
        {
            get
            {
                return new AveChangeToken(this.DataCache.GetProperty<string>("ChangeTokenEnd"));
            }
            set
            {
                this.DataCache.AddProperty("ChangeTokenEnd",value.ToString());
            }
        }

        public IAveChangeToken ChangeTokenStart
        {
            get
            {
                return new AveChangeToken(this.DataCache.GetProperty<string>("ChangeTokenStart"));
            }
            set
            {
                this.DataCache.AddProperty("ChangeTokenStart", value.ToString());
            }
        }

        public bool ContentType
        {
            get { return this.DataCache.GetProperty<bool>("ContentType"); }
            set { this.DataCache.AddProperty("ContentType", value); }
        }

        public bool Delete
        {
            get { return this.DataCache.GetProperty<bool>("Delete"); }
            set { this.DataCache.AddProperty("Delete", value); }
        }

        public long FetchLimit
        {
            get { return this.DataCache.GetProperty<long>("FetchLimit"); }
            set { this.DataCache.AddProperty("FetchLimit", value); }
        }

        public bool Field
        {
            get { return this.DataCache.GetProperty<bool>("Field"); }
            set { this.DataCache.AddProperty("Field", value); }
        }

        public bool File
        {
            get { return this.DataCache.GetProperty<bool>("File"); }
            set { this.DataCache.AddProperty("File", value); }
        }

        public bool Folder
        {
            get { return this.DataCache.GetProperty<bool>("Folder"); }
            set { this.DataCache.AddProperty("Folder", value); }
        }

        public bool Group
        {
            get { return this.DataCache.GetProperty<bool>("Group"); }
            set { this.DataCache.AddProperty("Group", value); }
        }

        public bool GroupMembershipAdd
        {
            get { return this.DataCache.GetProperty<bool>("GroupMembershipAdd"); }
            set { this.DataCache.AddProperty("GroupMembershipAdd", value); }
        }

        public bool GroupMembershipDelete
        {
            get { return this.DataCache.GetProperty<bool>("GroupMembershipDelete"); }
            set { this.DataCache.AddProperty("GroupMembershipDelete", value); }
        }

        public bool IgnoreStartTokenNotFoundError
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public bool Item
        {
            get { return this.DataCache.GetProperty<bool>("Item"); }
            set { this.DataCache.AddProperty("Item", value); }
        }

        public bool List
        {
            get { return this.DataCache.GetProperty<bool>("List"); }
            set { this.DataCache.AddProperty("List", value); }
        }

        public bool Move
        {
            get { return this.DataCache.GetProperty<bool>("Move"); }
            set { this.DataCache.AddProperty("Move", value); }
        }

        public bool Navigation
        {
            get { return this.DataCache.GetProperty<bool>("Navigation"); }
            set { this.DataCache.AddProperty("Navigation", value); }
        }

        public bool Rename
        {
            get { return this.DataCache.GetProperty<bool>("Rename"); }
            set { this.DataCache.AddProperty("Rename", value); }
        }

        public bool Restore
        {
            get { return this.DataCache.GetProperty<bool>("Restore"); }
            set { this.DataCache.AddProperty("Restore", value); }
        }

        public bool RoleAssignmentAdd
        {
            get { return this.DataCache.GetProperty<bool>("RoleAssignmentAdd"); }
            set { this.DataCache.AddProperty("RoleAssignmentAdd", value); }
        }

        public bool RoleAssignmentDelete
        {
            get { return this.DataCache.GetProperty<bool>("RoleAssignmentDelete"); }
            set { this.DataCache.AddProperty("RoleAssignmentDelete", value); }
        }

        public bool RoleDefinitionAdd
        {
            get { return this.DataCache.GetProperty<bool>("RoleDefinitionAdd"); }
            set { this.DataCache.AddProperty("RoleDefinitionAdd", value); }
        }

        public bool RoleDefinitionDelete
        {
            get { return this.DataCache.GetProperty<bool>("RoleDefinitionDelete"); }
            set { this.DataCache.AddProperty("RoleDefinitionDelete", value); }
        }

        public bool RoleDefinitionUpdate
        {
            get { return this.DataCache.GetProperty<bool>("RoleDefinitionUpdate"); }
            set { this.DataCache.AddProperty("RoleDefinitionUpdate", value); }
        }

        public bool SecurityPolicy
        {
            get { return this.DataCache.GetProperty<bool>("SecurityPolicy"); }
            set { this.DataCache.AddProperty("SecurityPolicy", value); }
        }

        public bool Site
        {
            get { return this.DataCache.GetProperty<bool>("Site"); }
            set { this.DataCache.AddProperty("Site", value); }
        }

        public bool SystemUpdate
        {
            get { return this.DataCache.GetProperty<bool>("SystemUpdate"); }
            set { this.DataCache.AddProperty("SystemUpdate", value); }
        }

        public bool Update
        {
            get { return this.DataCache.GetProperty<bool>("Update"); }
            set { this.DataCache.AddProperty("Update", value); }
        }

        public bool User
        {
            get { return this.DataCache.GetProperty<bool>("User"); }
            set { this.DataCache.AddProperty("User", value); }
        }

        public bool View
        {
            get { return this.DataCache.GetProperty<bool>("View"); }
            set { this.DataCache.AddProperty("View", value); }
        }

        public bool Web
        {
            get { return this.DataCache.GetProperty<bool>("Web"); }
            set { this.DataCache.AddProperty("Web", value); }
        }
    }
}
