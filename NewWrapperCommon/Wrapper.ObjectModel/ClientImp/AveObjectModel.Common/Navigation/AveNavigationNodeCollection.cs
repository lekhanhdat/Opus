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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveNavigationNodeCollection : AveAbstractCommonCollection<IAveNavigationNode>, IAveNavigationNodeCollection
    {
        private AveWeb mWeb;
        private IAveRequest mRequest;
        private AveNavigationNode mParent;
        private string mSource;

        public AveNavigationNodeCollection(AveWeb web, AveNavigationNode parent, IAveRequest request, Dictionary<string, object> navNodeProperties,string navigationColSource)
        {
            mWeb = web;
            mParent = parent;
            mRequest = request;
            mSource = navigationColSource;
            base.DataCache.AddPropertyies(navNodeProperties);
            InitNaviagtaionNodeCollection();
        }

        internal void InitNaviagtaionNodeCollection()
        {
            List<Dictionary<string, object>> navNodePropertiesList = base.DataCache.GetChildren();
            mListData = new List<IAveNavigationNode>(navNodePropertiesList.Count);
            foreach (Dictionary<string, object> navNodeProperties in navNodePropertiesList)
            {
                AveNavigationNode navigationNode = new AveNavigationNode(mWeb, mParent, this, mRequest, navNodeProperties);
                mListData.Add(navigationNode);
            }
        }

        public IAveNavigationNode Parent
        { 
            get
            {
                return mParent;
            }
        }
            
        public IAveNavigationNode Add(AveNavigationNodeCreationInformation parameters)
        {
            AveNavigationNode naviagtionNode = new AveNavigationNode(parameters.Title, parameters.Url, parameters.IsExternal);
            naviagtionNode.Site = mWeb.Site as AveSite;
            if (parameters.AsLastNode)
            {
                return this.AddAsLast(naviagtionNode);
            }
            else
            {
                return this.Add(naviagtionNode, parameters.PreviousNode);
            }
        }

        public IAveNavigationNode AddAsLast(IAveNavigationNode node)
        {
            Dictionary<string, object> newNavigationNodeProperties = (node as AveNavigationNode).DataCache.ChangedProperties;
            newNavigationNodeProperties["AsLastNode"] = true;
            Dictionary<string, object> navigationNodeProperties = mRequest.AddNavigationNode(this.mWeb.ServerRelativeUrl, mParent.Location, newNavigationNodeProperties,mSource);
            AveNavigationNode navigationNode = new AveNavigationNode(mWeb, mParent, this, mRequest, navigationNodeProperties);
            mListData.Add(navigationNode);
            return navigationNode;
        }

        public void Delete(IAveNavigationNode navNode)
        {
            Dictionary<string, object> prop = new Dictionary<string, object>();
            prop["Id" + AveObjectModelConstant.ObjectPropertySuffix] = (navNode as AveNavigationNode).DataCache.PropertiesCache["Id" + AveObjectModelConstant.ObjectPropertySuffix];
            if ((navNode as AveNavigationNode).DataCache.IsPropertyAvailable("ClientContext"))
            {
                prop["ClientContext"] = (navNode as AveNavigationNode).DataCache.PropertiesCache["ClientContext"];
            }
            mRequest.DeleteNavigationNode(this.mWeb.ServerRelativeUrl, mParent.DataCache.PropertiesCache, prop);
            mListData.Remove(navNode);
        }

        public IAveNavigationNode Add(IAveNavigationNode node, IAveNavigationNode previousNode)
        {            
            if (previousNode != null)
            {
                (node as AveNavigationNode).DataCache.AddChangedProperty("PreviousNode", (previousNode as AveNavigationNode).Location);                
            }
            Dictionary<string, object> navigationNodeProperties = mRequest.AddNavigationNode(this.mWeb.ServerRelativeUrl, mParent.Location, (node as AveNavigationNode).DataCache.ChangedProperties,mSource);
            AveNavigationNode navigationNode = new AveNavigationNode(mWeb, mParent, this, mRequest, navigationNodeProperties);
            mListData.Add(navigationNode);
            return navigationNode;
        }

        public IAveNavigation Navigation
        {
            get { throw new NotImplementedException(); }
        }
    }
}
