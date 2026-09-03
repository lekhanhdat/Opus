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
using System.Xml;
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.Common
{
    class AveView : AveClientObject, IAveView
    {
        private IAveRequest mRequest;
        private AveViewCollection mContainer;
        private AveList mParentList;

        public AveView(AveList parentList, AveViewCollection container, IAveRequest request, Dictionary<string, object> prop)
        {
            mRequest = request;
            mContainer = container;
            mParentList = parentList;
            InitHtmlSchemaXml(parentList, prop);
            base.DataCache.AddPropertyies(prop);
        }

        #region IAveView Members

        public string Aggregations
        {
            get
            {
                return base.DataCache.GetProperty<string>("Aggregations");
            }
            set
            {
                base.DataCache.AddChangedProperty("Aggregations", value);
            }
        }

        public string AggregationsStatus
        {
            get
            {
                return base.DataCache.GetProperty<string>("AggregationsStatus");
            }
            set
            {
                base.DataCache.AddChangedProperty("AggregationsStatus", value);
            }
        }

        public string BaseViewId
        {
            get
            {
                return base.DataCache.GetProperty<string>("BaseViewId");
            }
        }

        public IAveContentTypeId ContentTypeId
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ContentTypeId"))
                {
                    AveContentTypeId Id = new AveContentTypeId(base.DataCache.GetProperty<string>("ContentTypeId" + AveObjectModelConstant.ObjectPropertySuffix));
                    base.DataCache.PropertiesCache["ContentTypeId"] = Id;
                }
                return base.DataCache.GetProperty<IAveContentTypeId>("ContentTypeId");
            }
            set
            {
                base.DataCache.PropertiesCache["ContentTypeId"] = value;
                base.DataCache.AddChangedProperty("UpdateContentTypeId", value.ToString());
            }
        }

        public bool DefaultView
        {
            get
            {
                return base.DataCache.GetProperty<bool>("DefaultView");
            }
            set
            {
                base.DataCache.AddChangedProperty("DefaultView", value);
            }
        }

        public bool DefaultViewForContentType
        {
            get
            {
                return base.DataCache.GetProperty<bool>("DefaultViewForContentType");
            }
            set
            {
                base.DataCache.AddChangedProperty("DefaultViewForContentType", value);
            }
        }

        public bool EditorModified
        {
            get
            {
                return base.DataCache.GetProperty<bool>("EditorModified");
            }
            set
            {
                base.DataCache.AddChangedProperty("EditorModified", value);
            }
        }

        public string Formats
        {
            get
            {
                return base.DataCache.GetProperty<string>("Formats");
            }
            set
            {
                base.DataCache.AddChangedProperty("Formats", value);
            }
        }

        public bool Hidden
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Hidden");
            }
            set
            {
                base.DataCache.AddChangedProperty("Hidden", value);
            }
        }

        public string HtmlSchemaXml
        {
            get
            {
                Guid cacheHandlerId = (mParentList.ParentWeb as AveWeb).CacheHandlerId;
                return AveClientCacheHandler.GetSchemaXml(cacheHandlerId, mParentList.ParentWeb.ID.ToString(), mParentList.ID.ToString(), this.ID.ToString(), SchemaType.View);
            }
        }
        public string ListViewXml
        {
            get
            {
                return base.DataCache.GetProperty<string>("ListViewXml");
            }
            set
            {
                base.DataCache.AddChangedProperty("ListViewXml", value);
            }
        }

        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }

        public string ImageUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ImageUrl");
            }
        }

        public bool IncludeRootFolder
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IncludeRootFolder");
            }
            set
            {
                base.DataCache.AddChangedProperty("IncludeRootFolder", value);
            }
        }

        public string Method
        {
            get
            {
                return base.DataCache.GetProperty<string>("Method");
            }
            set
            {
                base.DataCache.AddChangedProperty("Method", value);
            }
        }

        public bool MobileDefaultView
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MobileDefaultView");
            }
            set
            {
                base.DataCache.AddChangedProperty("MobileDefaultView", value);
            }
        }

        public bool MobileView
        {
            get
            {
                return base.DataCache.GetProperty<bool>("MobileView");
            }
            set
            {
                base.DataCache.AddChangedProperty("MobileView", value);
            }
        }

        public string ModerationType
        {
            get
            {
                return base.DataCache.GetProperty<string>("ModerationType");
            }
        }

        public bool OrderedView
        {
            get
            {
                return base.DataCache.GetProperty<bool>("OrderedView");
            }
        }

        public bool Paged
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Paged");
            }
            set
            {
                base.DataCache.AddChangedProperty("Paged", value);
            }
        }

        public bool PersonalView
        {
            get
            {
                return base.DataCache.GetProperty<bool>("PersonalView");
            }
        }

        public bool ReadOnlyView
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ReadOnlyView");
            }
        }

        public bool RequiresClientIntegration
        {
            get
            {
                return base.DataCache.GetProperty<bool>("RequiresClientIntegration");
            }
        }

        public uint RowLimit
        {
            get
            {
                return base.DataCache.GetProperty<uint>("RowLimit");
            }
            set
            {
                base.DataCache.AddChangedProperty("RowLimit", value);
            }
        }

        public AveViewScope Scope
        {
            get
            {
                return base.DataCache.GetProperty<AveViewScope>("Scope");
            }
            set
            {
                base.DataCache.AddChangedProperty("Scope", (int)value);
            }
        }

        public string ServerRelativeUrl
        {
            get
            {
                return base.DataCache.GetProperty<string>("ServerRelativeUrl");
            }
        }

        public string StyleId
        {
            get
            {
                return base.DataCache.GetProperty<string>("StyleId");
            }
        }

        public bool Threaded
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Threaded");
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

        public string Toolbar
        {
            get
            {
                return base.DataCache.GetProperty<string>("Toolbar");
            }
            set
            {
                base.DataCache.AddChangedProperty("Toolbar", value);
            }
        }

        public string ToolbarTemplateName
        {
            get { return base.DataCache.GetProperty<string>("ToolbarTemplateName"); }
        }

        public string Url
        {
            get { return base.DataCache.GetProperty<string>("Url"); }
        }

        public string ViewData
        {
            get
            {
                return base.DataCache.GetProperty<string>("ViewData");
            }
            set
            {
                base.DataCache.AddChangedProperty("ViewData", value);
            }
        }

        public IAveViewFieldCollection ViewFields
        {
            get
            {
                if (base.DataCache.IsPropertyNotLoaded("ViewFields"))
                {
                    Dictionary<string, object> viewFieldsDic = base.DataCache.GetProperty<Dictionary<string, object>>("ViewFields" + AveObjectModelConstant.ObjectPropertySuffix);
                    AveViewFieldCollection viewFields = new AveViewFieldCollection(mRequest, mParentList, this, viewFieldsDic);
                    base.DataCache.PropertiesCache["ViewFields"] = viewFields;
                }
                return base.DataCache.GetProperty<IAveViewFieldCollection>("ViewFields");
            }
        }

        public string ViewJoins
        {
            get
            {
                return base.DataCache.GetProperty<string>("ViewJoins");
            }
            set
            {
                base.DataCache.AddChangedProperty("ViewJoins", value);
            }
        }

        public string ViewProjectedFields
        {
            get
            {
                return base.DataCache.GetProperty<string>("ViewProjectedFields");
            }
            set
            {
                base.DataCache.AddChangedProperty("ViewProjectedFields", value);
            }
        }

        public string Query
        {
            get
            {
                return base.DataCache.GetProperty<string>("Query");
            }
            set
            {
                base.DataCache.AddChangedProperty("Query", value);
            }
        }

        public string Type
        {
            get
            {
                return base.DataCache.GetProperty<string>("Type");
            }
        }

        public void DeleteObject()
        {
            mRequest.DeleteView(mParentList.ParentWeb.ServerRelativeUrl, mParentList.Title, mParentList.ID, this.ID);
            mContainer.Remove(this);

        }

        public void Update()
        {
            if (base.DataCache.ChangedProperties.Count > 0)
            {
                if (base.DataCache.ChangedProperties.ContainsKey("Query"))
                {
                    base.DataCache.ChangedProperties["ViewQuery"] = base.DataCache.ChangedProperties["Query"];
                    base.DataCache.ChangedProperties.Remove("Query");
                }
                Dictionary<string, object> newPro = mRequest.UpdateView(mParentList.ParentWeb.ServerRelativeUrl, mParentList.Title, mParentList.ID, this.ID, base.DataCache.ChangedProperties);
                if (newPro.ContainsKey("HtmlSchemaXml"))
                {
                    Guid cacheHandlerId = (mParentList.ParentWeb as AveWeb).CacheHandlerId;
                    AveClientCacheHandler.WriteSchemaXml(newPro["HtmlSchemaXml"].ToString(), cacheHandlerId, mParentList.ParentWeb.ID.ToString(), mParentList.ID.ToString(), this.ID.ToString(), SchemaType.View);
                    newPro.Remove("HtmlSchemaXml");
                }
                base.DataCache.UpdateProperties(newPro);
            }
        }

        #endregion

        public Guid PageUrlID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("PageUrlID");
            }
        }

        public IAveList ParentList
        {
            get
            {
                return mParentList;
            }
        }

        public string RowLimitExceeded
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public void ApplyStyle(IAveViewStyle viewStyles)
        {
            throw new NotImplementedException();
        }

        public string GroupByFooter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string GroupByHeader
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string OpenApplicationExtension
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ViewBody
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ViewEmpty
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ViewFooter
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ViewHeader
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ParameterBindings
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Joins
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string InlineEdit
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string XslLink
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string Xsl
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }


        public string CalendarSettings
        {
            get
            {
                if (string.IsNullOrEmpty(this.HtmlSchemaXml))
                {
                    return string.Empty;
                }
                XmlDocument document = new XmlDocument();
                document.LoadXml(this.HtmlSchemaXml);
                var setting = document.SelectSingleNode("//CalendarSettings");
                if (setting != null)
                {
                    return setting.InnerXml;
                }
                return string.Empty;
            }
            set
            {
                XmlDocument document = new XmlDocument();
                document.LoadXml(this.ListViewXml);
                var calenarSettingNode = document.SelectSingleNode("//CalendarSettings");
                if (calenarSettingNode == null)
                {
                    calenarSettingNode = document.CreateElement("CalendarSettings");
                    document.DocumentElement.InsertBefore(calenarSettingNode, document.DocumentElement.FirstChild);
                }
                calenarSettingNode.InnerXml = value;
                this.ListViewXml = document.DocumentElement.InnerXml;
            }
        }

        private void InitHtmlSchemaXml(AveList list, Dictionary<string, object> property)
        {
            if (property.ContainsKey("HtmlSchemaXml"))
            {
                string viewId = Guid.Empty.ToString();
                if (property.ContainsKey("Id"))
                {
                    viewId = property["Id"].ToString();
                }
                Guid cacheHandlerId = (list.ParentWeb as AveWeb).CacheHandlerId;
                AveClientCacheHandler.WriteSchemaXml(property["HtmlSchemaXml"].ToString(), cacheHandlerId, list.ParentWeb.ID.ToString(), list.ID.ToString(), viewId, SchemaType.View);
                property.Remove("HtmlSchemaXml");
            }
        }

        public uint Flag
        {
            get { throw new NotImplementedException(); }
        }


        public string CssStyleSheet
        {
            get { throw new NotImplementedException(); }
        }

        public uint MobileItemLimit
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string MobileSimpleViewField
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public Uri MobileUrl
        {
            get { throw new NotImplementedException(); }
        }

        public string ProjectedFields
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string PropertiesXml
        {
            get { throw new NotImplementedException(); }
        }

        public bool RecurrenceRowset
        {
            get { throw new NotImplementedException(); }
        }

        public bool TabularView
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public string ToolbarType
        {
            get { throw new NotImplementedException(); }
        }

        public AveFileLevel Level
        {
            get { throw new NotImplementedException(); }
        }


        public IAveUserResource TitleResource
        {
            get { return null; }
        }
    }
}
