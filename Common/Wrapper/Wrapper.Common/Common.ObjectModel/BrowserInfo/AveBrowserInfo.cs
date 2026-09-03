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
using System.Collections;
using System.Collections.Generic;

namespace AvePoint.Wrapper.Common
{
    public class AveSiteBrowserInfo
    {
        public Guid ID;
        public string Url;
        public string DisplayName;
        public string Title;
        public string TemplateTitle;
        public string TemplateName;
        public string ContentDBID;
        public string ContentDBName;
        public uint Language;
        public int AuditActions;
        public int BitFlags;

        #region  SiteCollection filter policy
        //public string Owner;
        public string OwnerLoginName;
        public string OwnerTitle;
        public DateTime Modified { get; set; }
        public DateTime Created { get; set; }
        //public long Size { get; set; }
        //public bool EnableAuditing { get; set; }
        public Hashtable ColumnInfos { get; set; }
        #endregion
    }

    public class AveWebBrowserInfo
    {
        public bool HasUniqueRoleAssignments;
        public bool IsRootWeb;
        public string Url;
        public string Name;
        public string TemplateName;
        public string TemplateTitle;
        public uint Language;
        public string Title;
        public Guid ID;
        public string ServerRelativeUrl;
    }

    public class AveListBrowserInfo
    {
        public Guid ID;
        public string Title;
        public string ServerRelativeUrl;
        public string Url;
        public string WebServerRelativeUrl;
        public string Name;
        public int BaseTemplate;
        public int BaseType;
        public bool Hidden;
        public bool HasUniqueRoleAssignments;
        public bool EnableFolderCreation;
        public string rootFolderName;
    }

    public class AveProjectBrowserInfo
    {
        public string Name;
        public Guid ID;
        public Guid EnterpriseProjectTypeId;
        public bool IsEnterpriseProject;
        public string Url;
        public bool IsCheckedOut;

    }

    public class AveFolderBrowserInfo
    {
        public string ServerRelativeUrl;
        public string Name;
        public string Url;
        public Guid ParentListId;
        public Guid RootFolderListId;
        public Guid ParentId;
        public Guid UniqueId;
        //public bool ListHasUniqueRoleAssignments;
        public bool HasUniqueRoleAssignments;
        public bool Hidden;
        public int ParentListBaseType;
    }

    public class AveItemBrowserInfo
    {
        public string Url;
        public string Name;
        public string DisplayName;
        public Guid UniqueId;
        public int ID;
        public Guid ParentFolderUniqueID;
        public Guid ParentListID;
        public int ListBaseType;
        public bool HasUniqueRoleAssignments;

        public Dictionary<string, byte> Versions = new Dictionary<string, byte>();
        public string CurrentUIVersionString;
        public int LastModifier;
        public string LastModifierName;
        public DateTime LastModifyTime;// utc time
        public byte Level;
        public Guid TpGuid;
    }

    public class AveItemVersionBrowserInfo
    {
        public string Url;
        public string VersionLabel;
        public string ItemName;
        public string ItemDisplayName;
        public Guid ItemID;
        public Guid ItemUniqueID;

    }

    public class AveAppBrowserInfo
    {
        public string Name;
        public string DisplayName;
        public Uri Url;
        public Guid SPObjectId;
        public Guid Id;
        public int Status;
        public bool AppIsUpdateAvailable;
    }

    public class AveSolutionBrowserInfo
    {
        public string Name;
        public string DisplayName;
        public string SolutionId;
        public string SolutionHasAssemblies;
        public string SolutionHash;
        public int Status;
    }

    public class AveFieldBrowserInfo
    {
        public string Name;
        public string DisplayName;
        public string ID;
        public bool Hidden;
        public string Group;
        public string ParentWebUrl;
        public string ParentListTitle;
    }

    public class AveWorkflowAssociationBrowserInfo
    {
        public string Name;
        public Guid ID;
        public Guid BaseId;
        public DateTime Created;
    }
}
