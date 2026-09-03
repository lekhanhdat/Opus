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
            this.DataCache.PropertiesCache["allChangeObjectTypes"] = allChangeObjectTypes;
            this.DataCache.PropertiesCache["allChangeTypes"] = allChangeTypes;
        }

        public bool Add
        {
            get { return this.DataCache.GetProperty<bool>("Add"); }
            set { this.DataCache.PropertiesCache["Add"] = value; }
        }

        public bool Alert
        {
            get { return this.DataCache.GetProperty<bool>("Alert"); }
            set { this.DataCache.PropertiesCache["Alert"] = value; }
        }

        public IAveChangeToken ChangeTokenEnd
        {
            get
            {
                return new AveChangeToken(this.DataCache.GetProperty<string>("ChangeTokenEnd"));
            }
            set
            {
                this.DataCache.PropertiesCache["ChangeTokenEnd"] = value.ToString();
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
                this.DataCache.PropertiesCache["ChangeTokenStart"] = value.ToString();
            }
        }

        public bool ContentType
        {
            get { return this.DataCache.GetProperty<bool>("ContentType"); }
            set { this.DataCache.PropertiesCache["ContentType"] = value; }
        }

        public bool Delete
        {
            get { return this.DataCache.GetProperty<bool>("Delete"); }
            set { this.DataCache.PropertiesCache["Delete"] = value; }
        }

        public long FetchLimit
        {
            get { throw new NotImplementedException(); }
            set { throw new NotImplementedException(); }
        }

        public bool Field
        {
            get { return this.DataCache.GetProperty<bool>("Field"); }
            set { this.DataCache.PropertiesCache["Field"] = value; }
        }

        public bool File
        {
            get { return this.DataCache.GetProperty<bool>("File"); }
            set { this.DataCache.PropertiesCache["File"] = value; }
        }

        public bool Folder
        {
            get { return this.DataCache.GetProperty<bool>("Folder"); }
            set { this.DataCache.PropertiesCache["Folder"] = value; }
        }

        public bool Group
        {
            get { return this.DataCache.GetProperty<bool>("Group"); }
            set { this.DataCache.PropertiesCache["Group"] = value; }
        }

        public bool GroupMembershipAdd
        {
            get { return this.DataCache.GetProperty<bool>("GroupMembershipAdd"); }
            set { this.DataCache.PropertiesCache["GroupMembershipAdd"] = value; }
        }

        public bool GroupMembershipDelete
        {
            get { return this.DataCache.GetProperty<bool>("GroupMembershipDelete"); }
            set { this.DataCache.PropertiesCache["GroupMembershipDelete"] = value; }
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
            set { this.DataCache.PropertiesCache["Item"] = value; }
        }

        public bool List
        {
            get { return this.DataCache.GetProperty<bool>("List"); }
            set { this.DataCache.PropertiesCache["List"] = value; }
        }

        public bool Move
        {
            get { return this.DataCache.GetProperty<bool>("Move"); }
            set { this.DataCache.PropertiesCache["Move"] = value; }
        }

        public bool Navigation
        {
            get { return this.DataCache.GetProperty<bool>("Navigation"); }
            set { this.DataCache.PropertiesCache["Navigation"] = value; }
        }

        public bool Rename
        {
            get { return this.DataCache.GetProperty<bool>("Rename"); }
            set { this.DataCache.PropertiesCache["Rename"] = value; }
        }

        public bool Restore
        {
            get { return this.DataCache.GetProperty<bool>("Restore"); }
            set { this.DataCache.PropertiesCache["Restore"] = value; }
        }

        public bool RoleAssignmentAdd
        {
            get { return this.DataCache.GetProperty<bool>("RoleAssignmentAdd"); }
            set { this.DataCache.PropertiesCache["RoleAssignmentAdd"] = value; }
        }

        public bool RoleAssignmentDelete
        {
            get { return this.DataCache.GetProperty<bool>("RoleAssignmentDelete"); }
            set { this.DataCache.PropertiesCache["RoleAssignmentDelete"] = value; }
        }

        public bool RoleDefinitionAdd
        {
            get { return this.DataCache.GetProperty<bool>("RoleDefinitionAdd"); }
            set { this.DataCache.PropertiesCache["RoleDefinitionAdd"] = value; }
        }

        public bool RoleDefinitionDelete
        {
            get { return this.DataCache.GetProperty<bool>("RoleDefinitionDelete"); }
            set { this.DataCache.PropertiesCache["RoleDefinitionDelete"] = value; }
        }

        public bool RoleDefinitionUpdate
        {
            get { return this.DataCache.GetProperty<bool>("RoleDefinitionUpdate"); }
            set { this.DataCache.PropertiesCache["RoleDefinitionUpdate"] = value; }
        }

        public bool SecurityPolicy
        {
            get { return this.DataCache.GetProperty<bool>("SecurityPolicy"); }
            set { this.DataCache.PropertiesCache["SecurityPolicy"] = value; }
        }

        public bool Site
        {
            get { return this.DataCache.GetProperty<bool>("Site"); }
            set { this.DataCache.PropertiesCache["Site"] = value; }
        }

        public bool SystemUpdate
        {
            get { return this.DataCache.GetProperty<bool>("SystemUpdate"); }
            set { this.DataCache.PropertiesCache["SystemUpdate"] = value; }
        }

        public bool Update
        {
            get { return this.DataCache.GetProperty<bool>("Update"); }
            set { this.DataCache.PropertiesCache["Update"] = value; }
        }

        public bool User
        {
            get { return this.DataCache.GetProperty<bool>("User"); }
            set { this.DataCache.PropertiesCache["User"] = value; }
        }

        public bool View
        {
            get { return this.DataCache.GetProperty<bool>("View"); }
            set { this.DataCache.PropertiesCache["View"] = value; }
        }

        public bool Web
        {
            get { return this.DataCache.GetProperty<bool>("Web"); }
            set { this.DataCache.PropertiesCache["Web"] = value; }
        }
    }
}
