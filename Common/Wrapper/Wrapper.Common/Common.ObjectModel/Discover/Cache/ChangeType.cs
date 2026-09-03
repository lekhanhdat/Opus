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

namespace AvePoint.Wrapper.Common
{
    [Flags]
    public enum NativeChangeType
    {
        ItemAdd = 1,
        ChangeAdd = 0x1000,//4096

        ItemModify = 2,
        ChangeModify = 0x2000,//8192
        ChangeSystemModify = 0x100000,//1048576

        ItemDelete = 4,//
        ChangeDelete = 0x4000,//16384

        ItemRestore = 9,
        ChangeRestore = 0x20000,//131072

        Rename = 0x8000,
        MoveInto = 0x10000,

        RoleAdd = 0x40000,
        AssignmentAdd = 0x80000,//524288
        ScopeAdd = 0xc0000,
        MemberAdd = 0x200000,
        MemberDelete = 0x400000,
        RoleDelete = 0x800000,
        RoleUpdate = 0x1000000,
        AssignmentDelete = 0x2000000,//33554432
        ScopeDelete = 0x2800000,

        MoveAway = 0x4000000,
        Navigation = 0x8000000,//134217728
        ListContenTypeAdd = 0x10000000, //268435456
        ListContenTypeDelete = 0x20000000, //536870912

        ChangeAll = 0x3ffff000,

        //DiscXXX belongs to Web Discussion,SharePoint Server 2010 does not support Web discussions.
        //sharepoint 2003 support DiscXXX 
        DiscAdd = 0x10,//16
        DiscModify = 0x20,
        DiscDeleted = 0x40,
        DiscClose = 0x80,
        DiscActivate = 0x100,
        DiscAll = 0xff0,
    }

    [Flags]
    public enum ChangeObjectType
    {
        Alert = 0x40,
        All = 0xffff,
        ContentType = 0x200,
        Field = 0x400,
        File = 0x10, //Attachment, View, default page
        FileFrag = 0x2000,
        Folder = 0x20, //System Folder, Ignore
        Group = 0x100,
        Item = 1,
        List = 2,
        SecurityPolicy = 0x800,
        Site = 8,
        User = 0x80,
        View = 0x1000,
        Web = 4
    }

    [Flags]
    public enum ChangeType
    {
        None = 0,
        Add = 1,
        Edit = 2,
        Delete = 4,
        Restore = 8,
        Repair=12,
    }
}
