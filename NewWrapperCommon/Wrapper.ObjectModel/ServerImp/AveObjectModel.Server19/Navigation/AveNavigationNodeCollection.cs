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



using Microsoft.SharePoint.Navigation;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Server19
{
    class AveNavigationNodeCollection : AveAbstractCommonCollection<IAveNavigationNode>, IAveNavigationNodeCollection
    {
        private SPNavigationNodeCollection mNodes;
        private AveNavigationNode mParent;

        public AveNavigationNodeCollection(SPNavigationNodeCollection nodes)
            : base(nodes)
        {
            mNodes = nodes;
        }

        internal SPNavigationNodeCollection NavigationNodeCollection
        {
            get
            {
                return mNodes;
            }
        }

        #region IAveNavigationNodeCollection Members

        public IAveNavigationNode Add(AveNavigationNodeCreationInformation parameters)
        {
            SPNavigationNode node = new SPNavigationNode(parameters.Title, parameters.Url, parameters.IsExternal);
            AveNavigationNode navigationNode = null;
            int iPreviousNodeId = -1;
            if (parameters.PreviousNode != null)
            {
                iPreviousNodeId = parameters.PreviousNode.ID;
                navigationNode = new AveNavigationNode(mNodes.Add(node, (parameters.PreviousNode as AveNavigationNode).NavigationNode));
            }
            else if (parameters.AsLastNode)
            {
                iPreviousNodeId = -2;
                navigationNode = new AveNavigationNode(mNodes.AddAsLast(node));
            }
            else
            {
                navigationNode = new AveNavigationNode(mNodes.AddAsFirst(node));
            }
            return navigationNode;
        }

        public IAveNavigationNode Add(IAveNavigationNode node, IAveNavigationNode previousNode)
        {
            return new AveNavigationNode(mNodes.Add((node as AveNavigationNode).NavigationNode, (previousNode as AveNavigationNode).NavigationNode));
        }

        public IAveNavigationNode Parent
        {
            get
            {
                if (mParent == null)
                {
                    SPNavigationNode navigationNode = mNodes.Parent;
                    if (navigationNode != null)
                    {
                        mParent = new AveNavigationNode(navigationNode);
                    }
                }
                return mParent;
            }
        }

        public IAveNavigationNode AddAsLast(IAveNavigationNode node)
        {
            return new AveNavigationNode(mNodes.AddAsLast((node as AveNavigationNode).NavigationNode));
        }

        public void Delete(IAveNavigationNode navNode)
        {
            mNodes.Delete((navNode as AveNavigationNode).NavigationNode);
        }

        public override IAveNavigationNode this[int index]
        {
            get
            {
                return new AveNavigationNode(mNodes[index]);
            }
        }

        protected override object CreatElementInstance(object t)
        {
            return new AveNavigationNode(t as SPNavigationNode);
        }

        public override int Count
        {
            get { return mNodes.Count; }
        }

        #endregion

        public IAveNavigation Navigation
        {
            get { return new AveNavigation(mNodes.Navigation); }
        }
    }
}
