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
    using AvePoint.GCommon.Contract.Tree;
    #region using directives

    using AvePoint.GCommon.Contract.Tree.Object;
    using System;

    #endregion using directives

    public class MediaRestoreNode
    {
        public Boolean IsInverted { get; set; }

        public Boolean IsExpanded { get; set; }

        public Boolean IsChecked { get; set; }

        public Boolean IsVirtualNode { get; set; }

        public NodeLevel Level { get; set; }

        public String DisplayName { get; set; }

        public MediaRestoreNode()
        { }

        public MediaRestoreNode(SPTreeNodeDto treeNode)
        {
            this.IsChecked = treeNode.CheckNumber == 1;
            this.IsInverted = treeNode.SelectAll == SelectAllState.Checked;
            treeNode.ChildrenCount = treeNode.Children.Count;
            this.IsExpanded = !this.IsInverted;
            this.IsVirtualNode = treeNode.IsVirtualNode();
            this.Level = treeNode.Level;
            this.DisplayName = treeNode.DisplayName;
        }

        public MediaRestoreNode(ExchangeOnlineTreeNodeDto treeNode)
        {
            this.IsChecked = treeNode.CheckNumber == 1;
            this.IsInverted = true;
            treeNode.ChildrenCount = treeNode.Children.Count;
            this.IsExpanded = treeNode.ChildrenCount > 0 || !this.IsInverted;
            this.IsVirtualNode = treeNode.IsVirtualNode();
            this.Level = treeNode.Level;
            this.DisplayName = treeNode.DisplayName;
        }
        public MediaRestoreNode(GoogleDriveTreeNodeDto treeNode)
        {
            this.IsChecked = treeNode.CheckNumber == 1;
            this.IsInverted = true;
            treeNode.ChildrenCount = treeNode.Children.Count;
            this.IsExpanded = treeNode.ChildrenCount > 0 || !this.IsInverted;
            //this.IsVirtualNode = treeNode.IsVirtualNode();
            this.Level = treeNode.Level;
            this.DisplayName = treeNode.DisplayName;
        }
        public override string ToString()
        {
            return string.Format("MediaRestoreNode : DisplayName : {0}, Level :{1}", DisplayName, Level);
        }
    }
}