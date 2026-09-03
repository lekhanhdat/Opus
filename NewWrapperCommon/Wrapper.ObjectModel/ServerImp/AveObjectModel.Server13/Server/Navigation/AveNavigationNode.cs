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
using System.Collections;

namespace AvePoint.ObjectModel.Server13
{
    class AveNavigationNode : AveServerObject, IAveNavigationNode
    {
        private SPNavigationNode mNavigationNode;
        private AveNavigationNodeCollection mChildren;
        private AveNavigationNode mParent;

        internal SPNavigationNode NavigationNode
        {
            get
            {
                return mNavigationNode;
            }
        }

        public AveNavigationNode(SPNavigationNode navigationNode)
        {
            mNavigationNode = navigationNode;
        }

        public AveNavigationNode(string title, string url)
        {
            mNavigationNode = new SPNavigationNode(title, url);
        }

        public AveNavigationNode(string title, string url, bool isExternal)
        {
            mNavigationNode = new SPNavigationNode(title, url, isExternal);
        }

        #region IAveNavigationNode Members

        public int ID
        {
            get { return mNavigationNode.Id; }
        }

        public string Title
        {
            get
            {
                return AveAssemblyUtility.GetFieldValue(mNavigationNode, "m_strTitle") as string;
            }
            set
            {
                mNavigationNode.Title = value;
            }
        }

        public string Url
        {
            get
            {
                return mNavigationNode.Url;
            }
            set
            {
                mNavigationNode.Url = value;
            }
        }

        public IAveNavigationNodeCollection Children
        {
            get
            {
                if (mChildren == null)
                {
                    mChildren = new AveNavigationNodeCollection(mNavigationNode.Children);
                }
                return mChildren;
            }
        }

        public void Update()
        {
            mNavigationNode.Update();
        }

        public Hashtable Properties
        {
            get { return mNavigationNode.Properties; }
        }

        public void Move(IAveNavigationNodeCollection collection, IAveNavigationNode previousSibling)
        {
            mNavigationNode.Move((collection as AveNavigationNodeCollection).NavigationNodeCollection, (previousSibling as AveNavigationNode).mNavigationNode);
        }

        public void MoveToFirst(IAveNavigationNodeCollection collection)
        {
            mNavigationNode.MoveToFirst((collection as AveNavigationNodeCollection).NavigationNodeCollection);
        }

        public void MoveToLast(IAveNavigationNodeCollection collection)
        {
            mNavigationNode.MoveToLast((collection as AveNavigationNodeCollection).NavigationNodeCollection);
        }

        public bool IsExternal
        {
            get { return mNavigationNode.IsExternal; }
        }

        public IAveNavigationNode Parent
        {
            get
            {
                if (mParent == null)
                {
                    SPNavigationNode navigationNode = mNavigationNode.Parent;
                    if (navigationNode != null)
                    {
                        mParent = new AveNavigationNode(navigationNode);
                    }
                }
                return mParent;
            }
        }

        public void Delete()
        {
            mNavigationNode.Delete();
        }

        #endregion
    }
}
