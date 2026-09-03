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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.SharePoint.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.Discover.Base
{
    public class NodeItem
    {
        #region Field
        public object DiscoverObj;

        // for sitecollection
        public BposInfo BposInfo;

        //Only for WebApplication(group) node
        private RMSPTreeNode mTreeNode;

        private SortedList<Guid, NodeItem> mChildren = new SortedList<Guid, NodeItem>();

        // for report jobs
        public RMReportExtension ReportExtension;
        #endregion

        #region Properties
        public SortedList<Guid, NodeItem> Children
        {
            get
            {
                return mChildren;
            }
        }

        public NodeItem Parent { get; set; }

        public NodeItem Farm { get; set; }

        /// <summary>
        /// Only for Farm(root) node and WebApplication(group) node
        /// </summary>
        public RMSPTreeNode TreeNode { get { return mTreeNode; } }

        public Guid Id { get; set; }

        public string NameOrTitle { get; set; }

        public string FullPath { get; set; }

        public NodeLevel NodeLevel { get; set; }

        public NodeType NodeType { get; set; }

        public bool IsChecked { get; set; }

        public bool IncludeNew { get; set; }

        public bool HasCheckedChildren { get; set; }

        //Only for Farm(Root) node
        public int SiteCollectionCount { get; set; }

        //public bool IsPhysical { get; set; }
        #endregion


        #region Methods
        #region Public

        public NodeItem()
        {
        }

        public NodeItem(RMSPTreeNode spTreeNode, NodeItem parent = null)
        {
            if (parent != null)
            {
                this.Parent = parent;
                this.Farm = parent.Farm;
            }
            mChildren = new SortedList<Guid, NodeItem>();
            CopyTreeNodeAttributeToNodeItem(spTreeNode);
            AddChild(spTreeNode);
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendFormat("{{[NameOrTitle:{0}], [CheckState:{1}], [Level:{2}], [Children:{3}}}",
                NameOrTitle, IsChecked, NodeLevel, mChildren == null ? 0 : mChildren.Count);
            return sb.ToString();
        }

        public override bool Equals(object obj)
        {
            var other = obj as NodeItem;

            if (other == null)
            {
                return false;
            }

            return NodeLevel == other.NodeLevel && string.Equals(Id, other.Id);
        }

        public override int GetHashCode()
        {
            return ((Id == Guid.Empty ? 0 : Id.GetHashCode()) + (int)NodeLevel + Convert.ToInt32(IsChecked));
        }

        public void AddChild(NodeItem child)
        {
            if (!mChildren.ContainsKey(child.Id))
            {
                mChildren.Add(child.Id, child);
                if (child.IsChecked || child.IncludeNew)
                {
                    if (child.NodeLevel == NodeLevel.SiteCollection && child.Farm != null)
                    {
                        child.Farm.SiteCollectionCount += 1;
                    }
                    NodeItem tempNode = this;
                    do
                    {
                        if (tempNode.NodeLevel == NodeLevel.SiteCollection && child.Farm != null)
                        {
                            tempNode.Farm.SiteCollectionCount += 1;
                        }
                        tempNode.HasCheckedChildren = true;
                        tempNode = tempNode.Parent;
                    }
                    while (tempNode != null && !tempNode.HasCheckedChildren);
                }
            }
        }

        public void AddChild(RMSPTreeNode node)
        {
            if (node.Children != null)
            {
                foreach (var ni in node.Children.Select(tmp => new NodeItem(tmp, this)))
                {
                    AddChild(ni);
                }
            }
        }

        //public void RemoveChild(string name)
        //{
        //    mChildren.Remove(name);
        //}

        #endregion

        #region Private

        /// <summary>
        /// Convert TreeNode to NodeItem
        /// </summary>
        /// <param name="node"></param>
        private void CopyTreeNodeAttributeToNodeItem(RMSPTreeNode node)
        {
            Id = new Guid(node.SPObjectId);
            NameOrTitle = node.Name;
            FullPath = node.FullPath;
            IncludeNew = (IncludeNewState)node.IncludeNew == IncludeNewState.Checked;
            NodeLevel = (NodeLevel)node.Level;
            NodeType = (NodeType)node.NodeType;
            IsChecked = node.CheckNumber == 1;
            if (NodeLevel == NodeLevel.SiteCollection)
            {
                Id = new Guid(node.Id);
                BposInfo = node.BposInfo;
            }
            else if (NodeLevel == NodeLevel.Farm)
            {
                mTreeNode = node;
                Farm = this;
            }
            else if (NodeLevel == NodeLevel.WebApplication)
            {
                mTreeNode = node;
            }
        }

        #endregion
        #endregion
    }
}
