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
    class AveFieldLink : AveClientObject, IAveFieldLink
    {
        private IAveRequest mRequest;
        //private AveWeb mWeb;
        //private AveList mList;
        private AveContentType mContentType;
        private AveFieldLinkCollection mFieldLinkCollection;
        private AveField mField;

        public AveFieldLink(AveField field)
        {
            mField = field;
            Dictionary<string, object> fieldLinkProperties = new Dictionary<string, object>();
            fieldLinkProperties.Add("IsNew", true);
            fieldLinkProperties.Add("FieldId", field.ID);
            fieldLinkProperties.Add("fieldSource", field.DataCache.ChangedProperties["fieldSource"]);
            base.DataCache.PropertiesCache["Id"] = field.ID;
            base.DataCache.AddChangedProperty("AddFieldLink", fieldLinkProperties);
            base.DataCache.AddChangedProperty("DisplayName", field.InternalName);
            base.DataCache.AddChangedProperty("Name", field.InternalName);
        }
        public AveFieldLink(AveContentType contentType, AveFieldLinkCollection fieldLinkCollection, IAveRequest request, Dictionary<string, object> fieldLinkProperties)
        {
            Dictionary<string, object> existFieldLinkProperties = new Dictionary<string, object>();
            mContentType = contentType;
            mFieldLinkCollection = fieldLinkCollection;
            mRequest = request;
            existFieldLinkProperties.Add("site", this.mContentType.ParentWeb.Url);
            existFieldLinkProperties.Add("ParentList", this.mContentType.ParentList == null ? null : this.mContentType.ParentList.Title);
            existFieldLinkProperties.Add("Id", this.mContentType.Location["ContentTypeId"]);
            existFieldLinkProperties.Add("contentTypeSource", this.mContentType.Location["ContentTypeSource"]);
            base.DataCache.AddPropertyies(fieldLinkProperties);
            existFieldLinkProperties.Add("DisplayName", this.DisplayName);
            existFieldLinkProperties.Add("FieldId", this.ID);
            base.DataCache.AddChangedProperty("AddFieldLink", existFieldLinkProperties);
        }

        public string DisplayName
        {
            get
            {
                return base.DataCache.GetProperty<string>("DisplayName");
            }
            set
            {
                base.DataCache.AddChangedProperty("DisplayName", value);
                AddUpdateFieldlink("DisplayName", value);
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
                (base.DataCache.ChangedProperties["AddFieldLink"] as Dictionary<string, object>)["Hidden"] = value;
                AddUpdateFieldlink("Hidden", value);   
            }
        }
        public Guid ID
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }
        public string Name
        {
            get
            {
                return base.DataCache.GetProperty<string>("Name");
            }
        }
        public bool ReadOnly
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ReadOnly");
            }
            set
            {
                base.DataCache.AddChangedProperty("ReadOnly", value);
                AddUpdateFieldlink("ReadOnly", value);
            }
        }
        public bool Required
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Required");
            }
            set
            {
                base.DataCache.AddChangedProperty("Required", value);
                (base.DataCache.ChangedProperties["AddFieldLink"] as Dictionary<string, object>)["Required"] = value;
                AddUpdateFieldlink("Required", value);
            }
        }

        public void Delete()
        {
            if (!this.mContentType.DataCache.ChangedProperties.ContainsKey("DeleteFieldLink"))
            {
                List<Guid> deleteNames = new List<Guid>();
                deleteNames.Add(this.ID);
                this.mContentType.DataCache.AddChangedProperty("DeleteFieldLink", deleteNames);
            }
            else
            {
                this.mContentType.DataCache.GetProperty<List<Guid>>("DeleteFieldLink").Add(this.ID);
            }
        }


        public string XPath
        {
            get
            {
                return base.DataCache.GetProperty<string>("XPath");
            }
            set
            {
                base.DataCache.AddChangedProperty("XPath", value);
                AddUpdateFieldlink("XPath", value);
            }
        }

        public string AggregationFunction
        {
            get
            {
                return base.DataCache.GetProperty<string>("AggregationFunction");
            }
            set
            {
                base.DataCache.AddChangedProperty("AggregationFunction", value);
                AddUpdateFieldlink("AggregationFunction", value);
            }
        }

        public string SchemaXml
        {
            get { return base.DataCache.GetProperty<string>("SchemaXml"); }
        }

        private void AddUpdateFieldlink(string key, object value)
        {
            if (this.mContentType != null)
            {
                Dictionary<Guid, Dictionary<string, object>> fieldLinks = null;
                if (this.mContentType.DataCache.ChangedProperties.ContainsKey("UpdateFieldLinks"))
                {
                    fieldLinks = this.mContentType.DataCache.ChangedProperties["UpdateFieldLinks"] as Dictionary<Guid, Dictionary<string, object>>;
                }
                else
                {
                    fieldLinks = new Dictionary<Guid, Dictionary<string, object>>();
                    this.mContentType.DataCache.ChangedProperties["UpdateFieldLinks"] = fieldLinks;
                }

                Dictionary<string, object> fieldLink = null;
                if (fieldLinks.ContainsKey(this.ID))
                {
                    fieldLink = fieldLinks[this.ID];
                }
                else
                {
                    fieldLink = new Dictionary<string, object>();
                    fieldLinks[this.ID] = fieldLink;
                }
                fieldLink[key] = value;
            }
        }


        public string Customization
        {
            get
            {
                return base.DataCache.GetProperty<string>("Customization");
            }
            set
            {
                base.DataCache.AddChangedProperty("Customization", value);
                AddUpdateFieldlink("Customization", value);
            }
        }

        public string PIAttribute
        {
            get
            {
                return base.DataCache.GetProperty<string>("PIAttribute");
            }
            set
            {
                base.DataCache.AddChangedProperty("PIAttribute", value);
                AddUpdateFieldlink("PIAttribute", value);
            }
        }

        public string PITarget
        {
            get
            {
                return base.DataCache.GetProperty<string>("PITarget");
            }
            set
            {
                base.DataCache.AddChangedProperty("PITarget", value);
                AddUpdateFieldlink("PITarget", value);
            }
        }

        public string PrimaryPIAttribute
        {
            get
            {
                return base.DataCache.GetProperty<string>("PrimaryPIAttribute");
            }
            set
            {
                base.DataCache.AddChangedProperty("PrimaryPIAttribute", value);
                AddUpdateFieldlink("PrimaryPIAttribute", value);
            }
        }

        public string PrimaryPITarget
        {
            get
            {
                return base.DataCache.GetProperty<string>("PrimaryPITarget");
            }
            set
            {
                base.DataCache.AddChangedProperty("PrimaryPITarget", value);
                AddUpdateFieldlink("PrimaryPITarget", value);
            }
        }

        public bool ShowInDisplayForm
        {
            get
            {
                return base.DataCache.GetProperty<bool>("ShowInDisplayForm");
            }
            set
            {
                base.DataCache.AddChangedProperty("ShowInDisplayForm", value);
                AddUpdateFieldlink("ShowInDisplayForm", value);
            }
        }
    }
}
