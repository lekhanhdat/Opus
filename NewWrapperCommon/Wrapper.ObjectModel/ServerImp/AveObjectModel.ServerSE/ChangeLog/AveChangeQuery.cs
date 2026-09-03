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
using Microsoft.SharePoint;

namespace AvePoint.ObjectModel.ServerSE
{
    public class AveChangeQuery: IAveChangeQuery
    {
        private SPChangeQuery mChangeQuery = null;

        public AveChangeQuery(SPChangeQuery changeQuery)
        {
            if (changeQuery == null)
            {
                throw new ArgumentNullException();
            }
            mChangeQuery = changeQuery;
        }

        public AveChangeQuery(bool allChangeObjectTypes, bool allChangeTypes)
        {
            mChangeQuery = new SPChangeQuery(allChangeObjectTypes, allChangeTypes);
        }

        internal SPChangeQuery ChangeQuery
        {
            get { return mChangeQuery; }
        }

        public bool Add
        {
            get
            {
                return mChangeQuery.Add;
            }
            set
            {
                mChangeQuery.Add = value;
            }
        }

        public bool Alert
        {
            get
            {
                return mChangeQuery.Alert;
            }
            set
            {
                mChangeQuery.Alert = value;
            }
        }

        public IAveChangeToken ChangeTokenEnd
        {
            get
            {
                return new AveChangeToken(mChangeQuery.ChangeTokenEnd);
            }
            set
            {
                mChangeQuery.ChangeTokenEnd = (value as AveChangeToken).ChangeToken;
            }
        }

        public IAveChangeToken ChangeTokenStart
        {
            get
            {
                return new AveChangeToken(mChangeQuery.ChangeTokenStart);
            }
            set
            {
                mChangeQuery.ChangeTokenStart = (value as AveChangeToken).ChangeToken;
            }
        }

        public bool ContentType
        {
            get
            {
                return mChangeQuery.ContentType;
            }
            set
            {
                mChangeQuery.ContentType = value;
            }
        }

        public bool Delete
        {
            get
            {
                return mChangeQuery.Delete;
            }
            set
            {
                mChangeQuery.Delete = value;
            }
        }

        public long FetchLimit
        {
            get
            {
                return mChangeQuery.FetchLimit;
            }
            set
            {
                mChangeQuery.FetchLimit = value;
            }
        }

        public bool Field
        {
            get
            {
                return mChangeQuery.Field;
            }
            set
            {
                mChangeQuery.Field = value;
            }
        }

        public bool File
        {
            get
            {
                return mChangeQuery.File;
            }
            set
            {
                mChangeQuery.File = value;
            }
        }

        public bool Folder
        {
            get
            {
                return mChangeQuery.Folder;
            }
            set
            {
                mChangeQuery.Folder = value;
            }
        }

        public bool Group
        {
            get
            {
                return mChangeQuery.Group;
            }
            set
            {
                mChangeQuery.Group = value;
            }
        }

        public bool GroupMembershipAdd
        {
            get
            {
                return mChangeQuery.GroupMembershipAdd;
            }
            set
            {
                mChangeQuery.GroupMembershipAdd = value;
            }
        }

        public bool GroupMembershipDelete
        {
            get
            {
                return mChangeQuery.GroupMembershipDelete;
            }
            set
            {
                mChangeQuery.GroupMembershipDelete = value;
            }
        }

        public bool IgnoreStartTokenNotFoundError
        {
            get
            {
                var ignoreStartTokenNotFoundError = AveAssemblyUtility.GetPropertyValue(mChangeQuery, "IgnoreStartTokenNotFoundError");
                return (bool)ignoreStartTokenNotFoundError;
            }
            set
            {
                var ignoreStartTokenNotFoundError = value;
                AveAssemblyUtility.SetPropertyValue(mChangeQuery, "IgnoreStartTokenNotFoundError", ignoreStartTokenNotFoundError);
            }
        }

        public bool Item
        {
            get
            {
                return mChangeQuery.Item;
            }
            set
            {
                mChangeQuery.Item = value;
            }
        }

        public bool List
        {
            get
            {
                return mChangeQuery.List;
            }
            set
            {
                mChangeQuery.List = value;
            }
        }

        public bool Move
        {
            get
            {
                return mChangeQuery.Move;
            }
            set
            {
                mChangeQuery.Move = value;
            }
        }

        public bool Navigation
        {
            get
            {
                return mChangeQuery.Navigation;
            }
            set
            {
                mChangeQuery.Navigation = value;
            }
        }

        public bool Rename
        {
            get
            {
                return mChangeQuery.Rename;
            }
            set
            {
                mChangeQuery.Rename = value;
            }
        }

        public bool Restore
        {
            get
            {
                return mChangeQuery.Restore;
            }
            set
            {
                mChangeQuery.Restore = value;
            }
        }

        public bool RoleAssignmentAdd
        {
            get
            {
                return mChangeQuery.RoleAssignmentAdd;
            }
            set
            {
                mChangeQuery.RoleAssignmentAdd = value;
            }
        }

        public bool RoleAssignmentDelete
        {
            get
            {
                return mChangeQuery.RoleAssignmentDelete;
            }
            set
            {
                mChangeQuery.RoleAssignmentDelete = value;
            }
        }

        public bool RoleDefinitionAdd
        {
            get
            {
                return mChangeQuery.RoleDefinitionAdd;
            }
            set
            {
                mChangeQuery.RoleDefinitionAdd = value;
            }
        }

        public bool RoleDefinitionDelete
        {
            get
            {
                return mChangeQuery.RoleDefinitionDelete;
            }
            set
            {
                mChangeQuery.RoleDefinitionDelete = value;
            }
        }

        public bool RoleDefinitionUpdate
        {
            get
            {
                return mChangeQuery.RoleDefinitionUpdate;
            }
            set
            {
                mChangeQuery.RoleDefinitionUpdate = value;
            }
        }

        public bool SecurityPolicy
        {
            get
            {
                return mChangeQuery.SecurityPolicy;
            }
            set
            {
                mChangeQuery.SecurityPolicy = value;
            }
        }

        public bool Site
        {
            get
            {
                return mChangeQuery.Site;
            }
            set
            {
                mChangeQuery.Site = value;
            }
        }

        public bool SystemUpdate
        {
            get
            {
                return mChangeQuery.SystemUpdate;
            }
            set
            {
                mChangeQuery.SystemUpdate = value;
            }
        }

        public bool Update
        {
            get
            {
                return mChangeQuery.Update;
            }
            set
            {
                mChangeQuery.Update = value;
            }
        }

        public bool User
        {
            get
            {
                return mChangeQuery.User;
            }
            set
            {
                mChangeQuery.User = value;
            }
        }

        public bool View
        {
            get
            {
                return mChangeQuery.View;
            }
            set
            {
                mChangeQuery.View = value;
            }
        }

        public bool Web
        {
            get
            {
                return mChangeQuery.Web;
            }
            set
            {
                mChangeQuery.Web = value;
            }
        }
    }
}
