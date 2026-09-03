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
using System.Collections;
using System.Reflection;
namespace AvePoint.ObjectModel.Common
{
    class AveNavigationNode : AveClientObject, IAveNavigationNode
    {
        private AveSite mSite;
        private AveWeb mWeb;
        private IAveRequest mRequest;
        private AveNavigationNode mParentNavigationNode;
        private AveNavigationNodeCollection mNavigationNodeCol;
        private Dictionary<string, object> mLocation;

        public AveNavigationNode(AveWeb web, IAveNavigationNode parentNavigationNode, AveNavigationNodeCollection navigationNodeCol, IAveRequest request, Dictionary<string, object> navigationNodeProperties)
        {
            mSite = web.Site as AveSite;
            mWeb = web;
            mParentNavigationNode = parentNavigationNode as AveNavigationNode;
            mNavigationNodeCol = navigationNodeCol;
            mRequest = request;
            base.DataCache.AddPropertyies(navigationNodeProperties);
        }

        public AveNavigationNode(string title, string url, bool isExternal)
        {            
            base.DataCache.AddChangedProperty("Title", title);
            base.DataCache.AddChangedProperty("Url", url);
            base.DataCache.AddChangedProperty("IsExternal", isExternal);
            base.DataCache.AddChangedProperty("IsNew", true);         
        }

        public AveNavigationNode(string title, string url, bool isExternal, int nodeType,AveNavigationNodeCollection navigationNodeCol)
        {
            base.DataCache.AddChangedProperty("Title", title);
            base.DataCache.AddChangedProperty("Url", url);
            base.DataCache.AddChangedProperty("IsExternal", isExternal);
            base.DataCache.AddChangedProperty("IsNew", true);
            base.DataCache.AddChangedProperty("NodeType", nodeType);
            mNavigationNodeCol = navigationNodeCol;
            AveNavigationNode node = navigationNodeCol.AddAsLast(this) as AveNavigationNode;
            mParentNavigationNode = navigationNodeCol.Parent as AveNavigationNode;
            mWeb = node.mWeb;
            mRequest = node.mRequest;
            base.DataCache.AddPropertyies(node.DataCache.PropertiesCache);
        }

        public AveNavigationNode(string title, string url, bool isExternal, Dictionary<string, object> navigationNodeProperties, IAveRequest request, AveWeb web, AveNavigationNode parent)
            : this(title, url, isExternal)
        {
            this.mRequest = request;
            this.mWeb = web;
            this.mParentNavigationNode = parent;
            base.DataCache.AddPropertyies(navigationNodeProperties);
        }

        internal Dictionary<string, object> Location
        {
            get
            {
                if (mLocation == null && base.DataCache.IsPropertyAvailable("Id" + AveObjectModelConstant.ObjectPropertySuffix))
                {
                    mLocation = new Dictionary<string, object>();
                    mLocation.Add("Id" + AveObjectModelConstant.ObjectPropertySuffix, base.DataCache.PropertiesCache["Id" + AveObjectModelConstant.ObjectPropertySuffix]);
                    if (base.DataCache.IsPropertyAvailable("ClientContext"))
                    {
                        mLocation.Add("ClientContext", base.DataCache.PropertiesCache["ClientContext"]);
                    }
                }
                else if (mLocation == null && mParentNavigationNode == null)
                {
                    mLocation = new Dictionary<string, object>();
                    if (base.DataCache.IsPropertyAvailable("ClientContext"))
                    {
                        mLocation.Add("ClientContext", base.DataCache.PropertiesCache["ClientContext"]);
                    }
                }
                return mLocation;
            }
        }

        internal AveSite Site
        {
            set
            {
                mSite = value;
                mRequest = value.Request;
            }
        }
             

        public IAveNavigationNodeCollection Children
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("Children"))
                {
                    Dictionary<string, object> childrenProperites = base.DataCache.GetProperty<Dictionary<string, object>>("Children"+AveObjectModelConstant.ObjectPropertySuffix);
                    //Dictionary<string, object> childrenProperites = mRequest.GetNavigationNodes(mWeb.ServerRelativeUrl, this.ID, "children", base.DataCache.PropertiesCache);
                    AveNavigationNodeCollection children = new AveNavigationNodeCollection(mWeb, this, mRequest, childrenProperites,"children");
                    base.DataCache.PropertiesCache["Children"] = children;
                }
                return base.DataCache.GetProperty<IAveNavigationNodeCollection>("Children");
            }
        }

        public int ID
        {
            get
            {
                return base.DataCache.GetProperty<int>("Id");
            }
        }

        public Hashtable Properties
        {
            get
            {
                return new Hashtable(base.DataCache.PropertiesCache);//base.DataCache.GetProperty<Hashtable>("Properties");
            }
        }

        public string Title
        {
            get
            {
                return base.DataCache.GetProperty<string>("Title");
            }
            set
            {
                base.DataCache.AddChangedProperty("Title", value);
            }
        }

        public string Url
        {
            get
            {
                return base.DataCache.GetProperty<string>("Url");
            }
            set
            {
                base.DataCache.AddChangedProperty("Url", value);
            }
        }

        public bool IsExternal
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsExternal");
            }
        }

        public IAveNavigationNode Parent
        {
            get
            {
                return mParentNavigationNode;
            }
        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                Dictionary<string, object> nodeSource = new Dictionary<string, object>();
                nodeSource["Id"] = this.ID;
                nodeSource["Id" + AveObjectModelConstant.ObjectPropertySuffix] = base.DataCache.PropertiesCache["Id" + AveObjectModelConstant.ObjectPropertySuffix];
                if (base.DataCache.IsPropertyAvailable("ClientContext"))
                {
                    nodeSource["ClientContext"] = base.DataCache.PropertiesCache["ClientContext"];
                }
                Dictionary<string, object> updateProperties = this.mRequest.UpdateNavigationNode(this.mWeb.ServerRelativeUrl, nodeSource, base.DataCache.ChangedProperties);
                this.DataCache.UpdateProperties(updateProperties);
            }
        }

        public void Move(IAveNavigationNodeCollection collection, IAveNavigationNode previousSibling)
        {
            Dictionary<string, object> previousNodeProperties = new Dictionary<string, object>();            
            previousNodeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = (previousSibling as AveNavigationNode).DataCache.PropertiesCache["Id" + AveObjectModelConstant.ObjectPropertySuffix];
            Dictionary<string, object> nodeSource = new Dictionary<string, object>();
            nodeSource["Id" + AveObjectModelConstant.ObjectPropertySuffix] = base.DataCache.PropertiesCache["Id" + AveObjectModelConstant.ObjectPropertySuffix];
            this.mRequest.MoveNavigationNode(this.mWeb.ServerRelativeUrl, nodeSource, previousNodeProperties, "Move");
        }

        public void Move(IAveNavigationNodeCollection collection, int rankChild)
        {
            IAveNavigationNode previousSibling = collection[rankChild];
            Dictionary<string, object> previousNodeProperties = new Dictionary<string, object>();
            Dictionary<string, object> nodeSource = new Dictionary<string, object>();
            int oldPosition = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                IAveNavigationNode node = collection[i];
                if (node.ID == this.ID)
                {
                    oldPosition = i;
                    if (oldPosition == rankChild)
                    {
                        return;
                    }
                    (collection as AveNavigationNodeCollection).ListData.Remove(node);
                    (collection as AveNavigationNodeCollection).ListData.Insert(rankChild, node);
                    break;
                }
            }
            string source = string.Empty;
            IAveNavigationNode parentNode = collection.Parent;
            while (parentNode != null)
            {
                if (parentNode.Title.Equals("Quick launch"))
                {
                    source = "QuickLaunch";
                    break;
                }
                else if (parentNode.Title.Equals("SharePoint Top Navigation Bar"))
                {
                    source = "TopNavigationBar";
                    break;
                }
                parentNode = parentNode.Parent;
            }
            if (base.DataCache.IsPropertyAvailable("Id" + AveObjectModelConstant.ObjectPropertySuffix))
            {
                nodeSource["Id" + AveObjectModelConstant.ObjectPropertySuffix] = base.DataCache.PropertiesCache["Id" + AveObjectModelConstant.ObjectPropertySuffix];
                previousNodeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = previousSibling.ID;
            }
            nodeSource["NodeOldPosition"] = oldPosition;
            nodeSource["NodeNewPosition"] = rankChild;
            nodeSource["NodeSource"] = source;
            nodeSource["NodeParentId"] = previousSibling.Parent.ID;
            nodeSource["NodeCount"] = collection.Count;
            this.mRequest.MoveNavigationNode(this.mWeb.ServerRelativeUrl, nodeSource, previousNodeProperties, "Move");
        }

        public void MoveToFirst(IAveNavigationNodeCollection collection)
        {
            this.MoveNodeInCollection(collection, MoveType.MoveToFirst, 0);
        }

        public void MoveToLast(IAveNavigationNodeCollection collection)
        {
            if (this.Parent != null && collection.Parent != null && this.Parent.ID != collection.Parent.ID)
            {
                this.MoveQuickLanuchNodeToNodeCollection(collection);
            }
            else
            {
                this.MoveNodeInCollection(collection, MoveType.MoveToLast, collection.Count - 1);
            }
        }

        private void MoveNodeInCollection(IAveNavigationNodeCollection collection, MoveType type, int position)
        {
            int oldPosition = 0;
            for (int i = 0; i < collection.Count; i++)
            {
                IAveNavigationNode node = collection[i];
                if (node.ID == this.ID)
                {
                    oldPosition = i;
                    (collection as AveNavigationNodeCollection).ListData.Remove(node);
                    (collection as AveNavigationNodeCollection).ListData.Insert(position, node);
                    break;
                }
            }
            int parentId = this.Parent.ID;
            int count = collection.Count;
            string source = string.Empty;
            IAveNavigationNode parentNode = collection.Parent;
            while (parentNode != null)
            {
                if (parentNode.Title.Equals("Quick launch"))
                {
                    source = "QuickLaunch";
                    break;
                }
                else if (parentNode.Title.Equals("SharePoint Top Navigation Bar"))
                {
                    source = "TopNavigationBar";
                    break;
                }
                parentNode = parentNode.Parent;
            }

            Dictionary<string, object> nodeSource = new Dictionary<string, object>();
            if (base.DataCache.IsPropertyAvailable("Id" + AveObjectModelConstant.ObjectPropertySuffix))
            {
                nodeSource["Id" + AveObjectModelConstant.ObjectPropertySuffix] = base.DataCache.PropertiesCache["Id" + AveObjectModelConstant.ObjectPropertySuffix];
            }
            nodeSource["NodeOldPosition"] = oldPosition;
            nodeSource["NodeParentId"] = parentId;
            nodeSource["NodeCount"] = count;
            nodeSource["NodeSource"] = source;

            this.mRequest.MoveNavigationNode(this.mWeb.ServerRelativeUrl, nodeSource, null, type.ToString());
        }

        private void MoveQuickLanuchNodeToNodeCollection(IAveNavigationNodeCollection collection)
        {
            Dictionary<string, object> nodeProp = new Dictionary<string, object>();
            nodeProp["NodeId"] = this.ID;
            nodeProp["NodeTitle"] = this.Title;
            nodeProp["NodeParentId"] = collection.Parent.ID;
            this.mRequest.MoveNavigationNode(this.mWeb.ServerRelativeUrl, nodeProp, null, MoveType.MoveToCollection.ToString());
        }

        public void Delete()
        {
            Dictionary<string, object> previousNodeProperties = null;
            if (mParentNavigationNode != null && mParentNavigationNode.DataCache.PropertiesCache.ContainsKey("Id" + AveObjectModelConstant.ObjectPropertySuffix))
            {
                previousNodeProperties = new Dictionary<string, object>();
                previousNodeProperties["Id" + AveObjectModelConstant.ObjectPropertySuffix] = mParentNavigationNode.DataCache.PropertiesCache["Id" + AveObjectModelConstant.ObjectPropertySuffix];
                if (mParentNavigationNode.DataCache.IsPropertyAvailable("ClientContext"))
                {
                    previousNodeProperties["ClientContext"] = mParentNavigationNode.DataCache.PropertiesCache["ClientContext"];
                }
            }
            this.mRequest.DeleteNavigationNode(this.mWeb.ServerRelativeUrl, previousNodeProperties, this.DataCache.PropertiesCache);
            if (mNavigationNodeCol != null)
            {
                mNavigationNodeCol.ListData.Remove(this);
            }
        }

        private enum MoveType
        {
            Move = 0,
            MoveToFirst = 1,
            MoveToLast = 2,
            MoveToCollection
        }

        #region User Resource:need to confirm if support
        public IAveUserResource TitleResource
        {
            get { return null; }
        }
        #endregion
    }

}
