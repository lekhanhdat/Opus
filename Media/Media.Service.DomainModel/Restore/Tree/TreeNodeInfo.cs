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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using System;
    using AvePoint.GCommon.Contract.Tree.Object;

    #endregion using directives

    public class TreeNodeInfo
    {
        public Int64 BackupTime { get; set; }

        public String Name { get; set; }

        public String Type { get; set; }

        public String ItemName { get; set; }

        public Single ItemVersionNumber { get; set; }

        public IndexBase Index { get; set; }

        public String PathMd5 { get; set; }

        public String SiteCollectionPath { get; set; }

        public String Path { get; set; }

        public String RealPath { get; set; }

        public Boolean NeedRestore(NodeLevel nodeLevel)
        {
            var needRestore = default(Boolean);
            if (nodeLevel == NodeLevel.SiteCollection ||
                nodeLevel == NodeLevel.Site ||
                nodeLevel == NodeLevel.List ||
                nodeLevel == NodeLevel.RootFolder ||
                nodeLevel == NodeLevel.Folder ||
                nodeLevel == NodeLevel.App ||
                nodeLevel == NodeLevel.AppData)
                needRestore = true;
            else if (nodeLevel == NodeLevel.Lists)
            {
                if (this.Type.Equals("L", StringComparison.OrdinalIgnoreCase))
                    needRestore = true;
            }
            else if (nodeLevel == NodeLevel.Apps)
            {
                if (this.Type.Equals("Y", StringComparison.OrdinalIgnoreCase))
                    needRestore = true;
            }
            else if (nodeLevel == NodeLevel.Sites)
            {
                if (this.Type.Equals("W", StringComparison.OrdinalIgnoreCase) && !this.Name.Equals(".", StringComparison.OrdinalIgnoreCase) || this.Type.Equals("P", StringComparison.OrdinalIgnoreCase))
                    needRestore = true;
            }
            else if (nodeLevel == NodeLevel.Folders)
            {
                if (this.Type.Equals("F", StringComparison.OrdinalIgnoreCase))
                    needRestore = true;
            }
            //Items下面不存在container
            else if (nodeLevel == NodeLevel.Items)
                needRestore = false;
            else
                throw new UnknownFileTypeException("RestoreTreeHandlerProcessNodeDtoInternalNodeException");
            return needRestore;
        }

        public NodeLevel ConverTypeToLevel()
        {
            return this.Type?.ToUpperInvariant() switch
            {
                "E" => NodeLevel.SiteCollection,
                "W" => NodeLevel.Site,
                "L" => NodeLevel.List,
                "F" => NodeLevel.Folder,
                "D" => NodeLevel.Document,
                "I" => NodeLevel.Item,
                _ => NodeLevel.Undefined,
            };
        }

        public override string ToString()
        {
            return string.Format("TreeNodeInfo : BackupTime : {0}, Name: {1}, Type: {2}",
                BackupTime, Name, Type);
        }
    }
}