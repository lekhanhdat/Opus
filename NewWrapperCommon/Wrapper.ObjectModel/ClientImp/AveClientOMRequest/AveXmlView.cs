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
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Xml;
using System.Text;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveXmlView
    {
        private uint mRowLimit = 30;
        private int mViewStyle = -1;

        public AveXmlView(string viewXml)
        {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(viewXml);
            InitViewAttributes(doc.FirstChild);
            InitViewChildNodes(doc.FirstChild);
        }

        public AveXmlView(XmlNode viewNode)
        {
            InitViewAttributes(viewNode);
            InitViewChildNodes(viewNode);
        }

        private void InitViewAttributes(XmlNode viewNode)
        {
            foreach (XmlAttribute attr in viewNode.Attributes)
            {
                switch (attr.Name)
                {
                    case "DefaultView":
                        this.DefaultView = Convert.ToBoolean(attr.Value);
                        break;
                    case "MobileView":
                        this.MobileView = Convert.ToBoolean(attr.Value);
                        break;
                    case "MobileDefaultView":
                        this.MobileDefaultView = Convert.ToBoolean(attr.Value);
                        break;
                    case "DisplayName":
                        this.Title = attr.Value;
                        break;
                    case "ContentTypeID":
                        this.ContentTypeId = attr.Value;
                        break;
                    case "Scope":
                        this.Scope = attr.Value;
                        break;
                    default:
                        continue;
                }
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rowset:RowSet as a key.")]
        private void InitViewChildNodes(XmlNode viewNode)
        {
            foreach (XmlElement node in viewNode.ChildElements())
            {
                if (node != null)
                {
                    switch (node.Name)
                    {
                        case "Query":
                            this.Query = node.InnerXml;
                            break;
                        case "ViewFields":
                            StringCollection viewFields = new StringCollection();
                            foreach (XmlNode viewFieldNode in node.ChildNodes)
                            {
                                viewFields.Add(viewFieldNode.Attributes["Name"].Value);   //注释掉下面逻辑，下面修改只是可能在目的端多显示出来，而过滤掉隐藏的column 会导致很多不确定问题。如ADO-176631
                                //if (viewFieldNode.Attributes != null)
                                //{
                                //    var explicitAttribute = viewFieldNode.Attributes["Explicit"];
                                //    //Explicit的view field不显示,client API无法支持Explicit属性还原,需要过滤,否则两端view field显示不一致
                                //    if (explicitAttribute == null ||!string.Equals(explicitAttribute.Value,bool.TrueString,StringComparison.OrdinalIgnoreCase))
                                //    {
                                //        viewFields.Add(viewFieldNode.Attributes["Name"].Value);
                                //    }
                                //}
                            }
                            this.ViewFields = viewFields;
                            break;
                        case "RowLimit":
                            this.RowLimit = Convert.ToUInt32(node.InnerText);
                            if (node.Attributes["Paged"] != null)
                            {
                                this.Paged = Convert.ToBoolean(node.Attributes["Paged"].Value);
                            }
                            break;
                        case "RowLimitExceeded":
                            this.RowLimitExceeded = node.InnerXml;
                            break;
                        case "Formats":
                            this.Formats = node.InnerXml;
                            break;
                        case "GroupByFooter":
                            this.GroupByFooter = node.InnerXml;
                            break;
                        case "GroupByHeader":
                            this.GroupByHeader = node.InnerXml;
                            break;
                        case "Aggregations":
                            this.Aggregations = node.InnerXml;
                            if (node.HasAttribute("Value"))
                            {
                                //this.Aggregations = node.GetAttribute("Value");
                                this.AggregationsStatus = node.GetAttribute("Value");
                            }
                            break;
                        case "OpenApplicationExtension":
                            this.OpenApplicationExtension = node.InnerXml;
                            break;
                        case "ViewData":
                            this.ViewData = node.InnerXml;
                            break;
                        case "ViewBody":
                            this.ViewBody = node.InnerXml;
                            break;
                        case "ViewEmpty":
                            this.ViewEmpty = node.InnerXml;
                            break;
                        case "ViewFooter":
                            this.ViewFooter = node.InnerXml;
                            break;
                        case "ViewHeader":
                            this.ViewHeader = node.InnerXml;
                            break;
                        case "Toolbar":
                            if (!string.IsNullOrEmpty(node.InnerXml))
                            {
                                this.Toolbar = node.InnerXml;
                            }
                            if (node.Attributes["Type"] != null)
                            {
                                this.ToolbarType = node.Attributes["Type"].Value;
                            }
                            break;
                        case "ParameterBindings":
                            this.ParameterBindings = node.InnerXml;
                            break;
                        case "Joins":
                            this.Joins = node.InnerXml;
                            break;
                        case "InlineEdit":
                            this.InlineEdit = node.InnerXml;
                            break;
                        case "XslLink":
                            this.XslLink = node.InnerXml;
                            break;
                        case "Xsl":
                            this.Xsl = node.InnerXml;
                            break;
                        case "ViewStyle":
                            this.ViewStyle = int.Parse(node.GetAttribute("ID"));
                            break;
                        case "CalendarViewStyles":
                            this.CalendarViewStyles = node.InnerXml;
                            break;
                        case "PagedRecurrenceRowset":
                            this.PagedRecurrenceRowset = node.InnerXml;
                            break;
                        case "CalendarSettings":
                            this.CalendarSettings = node.InnerXml;
                            break;
                        default:
                            continue;
                    }
                }
            }
        }

        public string Query
        {
            get;
            set;
        }

        public StringCollection ViewFields
        {
            get;
            set;
        }

        public uint RowLimit
        {
            get
            {
                return mRowLimit;
            }
            set
            {
                mRowLimit = value;
            }
        }

        public bool Paged
        {
            get;
            set;
        }

        public string Aggregations
        {
            get;
            set;
        }

        public string AggregationsStatus
        {
            get;
            set;
        }

        public string Toolbar
        {
            get;
            set;
        }

        public string ToolbarType
        {
            get;
            set;
        }

        public string Title
        {
            get;
            set;
        }

        public bool DefaultView
        {
            get;
            set;
        }

        public bool MobileView
        {
            get;
            set;
        }

        public bool MobileDefaultView
        {
            get;
            set;
        }

        public string ContentTypeId
        {
            get;
            set;
        }

        public string RowLimitExceeded
        {
            get;
            set;
        }

        public string Formats
        {
            get;
            set;
        }

        public string GroupByFooter
        {
            get;
            set;
        }

        public string GroupByHeader
        {
            get;
            set;
        }

        public string OpenApplicationExtension
        {
            get;
            set;
        }

        public string ViewData
        {
            get;
            set;
        }

        public string ViewBody
        {
            get;
            set;
        }
        public string ViewEmpty
        {
            get;
            set;
        }
        public string ViewFooter
        {
            get;
            set;
        }

        public string ViewHeader
        {
            get;
            set;
        }

        public string ParameterBindings
        {
            get;
            set;
        }

        public string PagedRecurrenceRowset
        {
            set;
            get;
        }
        public string CalendarSettings
        {
            set;
            get;
        }

        public string Joins
        {
            get;
            set;
        }

        public string InlineEdit
        {
            get;
            set;
        }

        public string XslLink
        {
            get;
            set;
        }

        public string Xsl
        {
            get;
            set;
        }

        public int ViewStyle
        {
            get
            {
                return mViewStyle;
            }
            set
            {
                mViewStyle = value;
            }
        }

        public string CalendarViewStyles
        {
            get;
            set;
        }

        public string Scope
        {
            get;
            set;
        }
    }
}
