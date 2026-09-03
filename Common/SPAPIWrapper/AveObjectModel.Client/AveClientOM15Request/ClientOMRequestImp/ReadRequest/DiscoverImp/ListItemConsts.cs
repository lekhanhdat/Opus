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
namespace AvePoint.ObjectModel.ClientOM
{
    using Microsoft.SharePoint.Client;

    public class ListItemConsts
    {
        public const string FieldRef = "FileRef";
        public const string FieldLeafRef = "FileLeafRef";
        public const string Id = "Id";
        public const string CustomizedPageStatus = "CustomizedPageStatus";

        protected const string QueryItemsByType = "<View Scope=\"RecursiveAll\"><Query><Where><Eq><FieldRef Name='FSObjType'/><Value Type='Lookup'>{0}</Value></Eq></Where></Query></View>";

        public static string GetQueryItemsString(FileSystemObjectType type)
        {
            return string.Format(QueryItemsByType, (int)type);
        }
    }
}
