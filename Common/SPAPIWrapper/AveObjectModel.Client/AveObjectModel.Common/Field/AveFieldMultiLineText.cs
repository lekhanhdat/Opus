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
    class AveFieldMultiLineText : AveField, IAveFieldMultiLineText
    {
        private IAveRequest mRequest;
        private AveList mParentList;
        private AveWeb mWeb;
        private AveFieldCollection mFieldCollection;
        private string mFieldSource;
        private IDictionary<string, object> mContentTypeProp;

        public AveFieldMultiLineText(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, IDictionary<string, object> contentTypeProp, IDictionary<string, object> prop)
            : base(request, list, web, fieldSource, fieldCollection, contentTypeProp, prop)
        {
            mRequest = request;
            mParentList = list;
            mWeb = web;
            mFieldCollection = fieldCollection;
            mFieldSource = fieldSource;
            mContentTypeProp = contentTypeProp;
            base.DataCache.AddPropertyies(prop);
        }
        public bool AllowHyperlink
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AllowHyperlink");
            }
            set
            {
                base.DataCache.AddChangedProperty("AllowHyperlink", value);
            }
        }
        public bool AppendOnly
        {
            get
            {
                return base.DataCache.GetProperty<bool>("AppendOnly");
            }
            set
            {
                base.DataCache.AddChangedProperty("AppendOnly", value);
            }
        }
        public int DifferencingLimit
        {
            get
            {
                return base.DataCache.GetProperty<int>("DifferencingLimit");
            }
            set
            {
                base.DataCache.AddChangedProperty("DifferencingLimit", value);
            }
        }
        public bool IsolateStyles
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsolateStyles");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsolateStyles", value);
            }
        }
        public int NumberOfLines
        {
            get
            {
                return base.DataCache.GetProperty<int>("NumberOfLines");
            }
            set
            {
                base.DataCache.AddChangedProperty("NumberOfLines", value);
            }
        }
        public bool RestrictedMode
        {
            get
            {
                return base.DataCache.GetProperty<bool>("RestrictedMode");
            }
            set
            {
                base.DataCache.AddChangedProperty("RestrictedMode", value);
            }
        }
        public bool RichText
        {
            get
            {
                return base.DataCache.GetProperty<bool>("RichText");
            }
            set
            {
                base.DataCache.AddChangedProperty("RichText", value);
            }
        }
        public AveRichTextMode RichTextMode
        {
            get
            {
                return base.DataCache.GetProperty<AveRichTextMode>("RichTextMode");
            }
            set
            {
                //we are going to use webservice to update this property, so the type is better to be string
                base.DataCache.AddChangedProperty("RichTextMode", (int)value);
            }
        }
        public bool UnlimitedLengthInDocumentLibrary
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UnlimitedLengthInDocumentLibrary");
            }
            set
            {
                base.DataCache.AddChangedProperty("UnlimitedLengthInDocumentLibrary", value);
            }
        }
        public bool WikiLinking
        {
            get
            {
                return base.DataCache.GetProperty<bool>("WikiLinking");
            }
        }

        protected override System.Collections.Generic.Dictionary<string, object> InternalUpdate(string listTitle, Guid listId)
        {
            if (base.DataCache.ChangedProperties.ContainsKey("RichTextMode"))
            {
                base.DataCache.ChangedProperties["RichTextMode"] = this.RichTextMode.ToString();
                return this.mRequest.UpdateField(this.mWeb.ServerRelativeUrl, listTitle, listId, this.InternalName, this.mFieldSource, this.mContentTypeProp, base.DataCache.ChangedProperties, this.SchemaXml);
            }
            else if (base.DataCache.ChangedProperties.ContainsKey("UnlimitedLengthInDocumentLibrary"))
            {
                return this.mRequest.UpdateField(this.mWeb.ServerRelativeUrl, listTitle, listId, this.InternalName, this.mFieldSource, this.mContentTypeProp, base.DataCache.ChangedProperties, this.SchemaXml);
            }
            else
            {
                return base.InternalUpdate(listTitle, listId);
            }
        }
    }
}
