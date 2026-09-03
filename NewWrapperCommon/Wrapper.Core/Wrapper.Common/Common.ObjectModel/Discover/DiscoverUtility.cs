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



using System.Collections.Generic;
using System;
using System.Data.SqlClient;
using System.Diagnostics.CodeAnalysis;
namespace AvePoint.Wrapper.Common
{
    public class DiscoverUtility
    {
        public const long EnableVersion = 0x0000000000000080;
        public const long DisableAttachment = 0x0000000000000008;
        private static List<string> SYSTEM_LIST_EXCLUDE_NAMES = new List<string>();

        static DiscoverUtility()
        {
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_catalogs");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_vti_pvt");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_cts");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_private");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("_themes");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("Lists");
            SYSTEM_LIST_EXCLUDE_NAMES.Add("m");
        }

        public static bool IsEnableVersion(long flag)
        {
            return (flag & EnableVersion) != 0;
        }

        public static bool IsEnableAttachment(long flag)
        {
            return (flag & DisableAttachment) == 0;
        }

        public static SecurityType GetSecurityObjectType(NativeChangeType nativeType)
        {
            switch (nativeType)
            {
                case NativeChangeType.RoleAdd:
                case NativeChangeType.RoleDelete:
                case NativeChangeType.RoleUpdate:
                    return SecurityType.Role;
                case NativeChangeType.AssignmentAdd:
                case NativeChangeType.AssignmentDelete:
                    return SecurityType.Assignment;
                case NativeChangeType.ScopeAdd:
                case NativeChangeType.ScopeDelete:
                    return SecurityType.Scope;
                default:
                    return SecurityType.None;
            }
        }

        public static ChangeType GetSecurityChangeType(NativeChangeType nativeType)
        {
            switch (nativeType)
            {
                case NativeChangeType.RoleAdd:
                case NativeChangeType.AssignmentAdd:
                case NativeChangeType.ScopeAdd:
                    return ChangeType.Add;
                case NativeChangeType.RoleDelete:
                case NativeChangeType.ScopeDelete:
                case NativeChangeType.AssignmentDelete:
                    return ChangeType.Delete;
                case NativeChangeType.RoleUpdate:
                    return ChangeType.Edit;
                default:
                    return ChangeType.None;
            }
        }

        public static ChangeType GetChangeType(NativeChangeType nativeType)
        {
            switch (nativeType)
            {
                case NativeChangeType.ItemAdd:
                case NativeChangeType.ChangeAdd:
                case NativeChangeType.DiscAdd:
                case NativeChangeType.ItemAdd | NativeChangeType.ChangeAdd:
                    return ChangeType.Add;
                case NativeChangeType.ChangeDelete:
                case NativeChangeType.ItemDelete:
                case NativeChangeType.ChangeDelete | NativeChangeType.ItemDelete:
                    return ChangeType.Delete;
                case NativeChangeType.ItemModify:
                case NativeChangeType.ChangeModify:
                case NativeChangeType.ChangeSystemModify:
                //case NativeChangeType.Navigation:
                case NativeChangeType.ChangeSystemModify | NativeChangeType.ChangeModify:
                case NativeChangeType.ItemModify | NativeChangeType.ChangeModify:
                case NativeChangeType.MemberAdd:
                case NativeChangeType.MemberDelete:
                case NativeChangeType.ListContenTypeAdd:
                case NativeChangeType.ListContenTypeDelete:
                    return ChangeType.Edit;
                case NativeChangeType.ItemRestore:
                case NativeChangeType.ChangeRestore:
                case NativeChangeType.ItemRestore | NativeChangeType.ChangeRestore:
                    return ChangeType.Restore;
                default:
                    return ChangeType.None;
            }
        }

        //public static ChangeType GetChangeType(AveChangeType apiType)
        //{
        //    switch (apiType)
        //    {
        //        case AveChangeType.Add:
        //            return ChangeType.Add;
        //        case AveChangeType.Update:
        //        case AveChangeType.Rename:
        //        case AveChangeType.MoveAway:
        //        case AveChangeType.MoveInto:
        //        case AveChangeType.RoleUpdate:
        //        case AveChangeType.SystemUpdate:
        //        case AveChangeType.Navigation:
        //        case AveChangeType.RoleDelete:
        //        case AveChangeType.AssignmentDelete:
        //        case AveChangeType.MemberDelete:
        //        case AveChangeType.ScopeDelete:
        //        case AveChangeType.ListContentTypeDelete:
        //        case AveChangeType.RoleAdd:
        //        case AveChangeType.AssignmentAdd:
        //        case AveChangeType.MemberAdd:
        //        case AveChangeType.ScopeAdd:
        //        case AveChangeType.ListContentTypeAdd:
        //            return ChangeType.Edit;
        //        case AveChangeType.Delete:
        //            return ChangeType.Delete;
        //        case AveChangeType.Restore:
        //            return ChangeType.Restore;
        //    }
        //    return ChangeType.None;
        //}

        public static void FillWebPartDicFromAllWebParts(AveViewObject viewObj, SqlDataReader sr)
        {
            viewObj.ViewID = (Guid)sr.GetValue(ViewColumn.Id);
            viewObj.ViewType = (int)sr.GetValue(ViewColumn.Flags);
            viewObj.IsPersonalView = (sr.GetInt32(ViewColumn.Flags) & 262144) == 262144 ? true : false;
            if (!sr.IsDBNull(ViewColumn.BaseViewID))
            {
                viewObj.BaseViewId = (byte)sr.GetValue(ViewColumn.BaseViewID);
            }
            if (!sr.IsDBNull(ViewColumn.DisplayName))
            {
                viewObj.ViewTitle = (string)sr.GetValue(ViewColumn.DisplayName);
            }
            viewObj.PageUrlID = (Guid)sr.GetValue(ViewColumn.PageUrlID);
            if (!sr.IsDBNull(ViewColumn.UserID))
            {
                viewObj.ViewUserID = (int?)sr.GetValue(ViewColumn.UserID);
            }
        }

        internal static bool IsUnusedFolder(string leafName, bool noList)
        {
            if (!noList)
            {
                return false;
            }
            return SYSTEM_LIST_EXCLUDE_NAMES.Contains(leafName);
        }
    }

}
