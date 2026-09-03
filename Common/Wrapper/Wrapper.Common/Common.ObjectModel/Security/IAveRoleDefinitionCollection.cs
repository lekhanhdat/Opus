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
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveRoleDefinitionCollection : ICollection, IEnumerable<IAveRoleDefinition>, IEnumerable
    {
        IAveRoleDefinition Add(AveRoleDefinitionCreationInformation roleDefCreationInfo);
        IAveRoleDefinition Add(IAveRoleDefinition roleDefinition);
        void BreakInheritance(bool copyRoleDefinitions, bool keepRoleAssignments);
        void Delete(IAveRoleDefinition roleDefinition);
        void DeleteById(int id);
        IAveRoleDefinition GetById(int id);
        IAveRoleDefinition GetByName(string name);
        IAveRoleDefinition GetByType(AveRoleType roleType);

        IAveRoleDefinition this[string name] { get; }
        IAveRoleDefinition this[int index] { get; }
    }        

    public sealed class AveRoleDefinitionCreationInformation
    {
        private AveBasePermissions mbasePermissions;
        private string mdescription;
        private string mname;
        private int morder;

        public AveBasePermissions BasePermissions
        {
            get
            {
                return mbasePermissions;
            }
            set
            {
                mbasePermissions = value;
            }
        }

        public string Description
        {
            get
            {
                return mdescription;
            }
            set
            {
                mdescription = value;
            }
        }

        public string Name
        {
            get
            {
                return mname;
            }
            set
            {
                mname = value;
            }
        }

        public int Order
        {
            get
            {
                return morder;
            }
            set
            {
                morder = value;
            }
        }
    }

    public enum AveRoleType
    {
        None,
        Guest,
        Reader,
        Contributor,
        WebDesigner,
        Administrator,
        Editor
    }

    public enum AvePermissionKind
    {
        AddAndCustomizePages = 0x13,
        AddDelPrivateWebParts = 0x1d,
        AddListItems = 2,
        ApplyStyleSheets = 0x15,
        ApplyThemeAndBorder = 20,
        ApproveItems = 5,
        BrowseDirectories = 0x1b,
        BrowseUserInfo = 0x1c,
        CancelCheckout = 9,
        CreateAlerts = 40,
        CreateGroups = 0x19,
        CreateSSCSite = 0x17,
        DeleteListItems = 4,
        DeleteVersions = 8,
        EditListItems = 3,
        EditMyUserInfo = 0x29,
        EmptyMask = 0,
        EnumeratePermissions = 0x3f,
        FullMask = 0x41,
        ManageAlerts = 0x27,
        ManageLists = 12,
        ManagePermissions = 0x1a,
        ManagePersonalViews = 10,
        ManageSubwebs = 0x18,
        ManageWeb = 0x1f,
        Open = 0x11,
        OpenItems = 6,
        UpdatePersonalWebParts = 30,
        UseClientIntegration = 0x25,
        UseRemoteAPIs = 0x26,
        ViewFormPages = 13,
        ViewListItems = 1,
        ViewPages = 0x12,
        ViewUsageData = 0x16,
        ViewVersions = 7
    }
}
