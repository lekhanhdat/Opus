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



namespace AvePoint.GCommon.Contract.Media.Object
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.GCommon.Contract.Tree.Object;
    #endregion
    [Serializable]
    public class RestoreTreeNode
    {
        public String Name { get; set; }
        public NodeType Type { get; set; }
        public Int64 BackupTime { get; set; }
        public Int32 CheckNumber { get; set; }

        private RestoreTreeNodeCollection mChildren = new RestoreTreeNodeCollection();
        public RestoreTreeNodeCollection Children { get { return mChildren; } }

        public List<RestoreTreeNode> GetRestoreFolders()
        {
            return Children.Containers;
        }

        public List<RestoreTreeNode> GetRestoreFiles()
        {
            return Children.Files;
        }

        public override String ToString()
        {
            return String.Format("Restore Tree Node: Name: {0}, Type: {1}, Check Number: {2}",
                this.Name,
                this.Type,
                this.CheckNumber);
        }
    }

    [Serializable]
    public class RestoreTreeNodeCollection
    {
        private List<RestoreTreeNode> mContainers = new List<RestoreTreeNode>();
        private List<RestoreTreeNode> mFiles = new List<RestoreTreeNode>();

        public List<RestoreTreeNode> Containers { get { return mContainers; } }
        public List<RestoreTreeNode> Files { get { return mFiles; } }

        public void Add(RestoreTreeNode treeNode)
        {
            if ((int)treeNode.Type == 900)
            {
                mFiles.Add(treeNode);
            }
            else
            {
                mContainers.Add(treeNode);
            }
        }

        public void Clear()
        {
            mFiles.Clear();
            mContainers.Clear();
        }
    }
}
