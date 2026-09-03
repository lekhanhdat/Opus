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
    class AveTaxonomyField : AveFieldLookup, IAveTaxonomyField
    {
        private IAveRequest m_Request;
        private AveList m_ParentList;
        private AveWeb m_Web;
        private AveFieldCollection m_FieldCollection;
        private string m_FieldSource;
        private IDictionary<string, object> m_ContentTypeProp;

        public AveTaxonomyField(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, IDictionary<string, object> contentTypeProp, IDictionary<string, object> prop)
            : base(request, list, web, fieldSource, fieldCollection, contentTypeProp, prop)
        {
            m_Request = request;
            m_ParentList = list;
            m_Web = web;
            m_FieldCollection = fieldCollection;
            m_FieldSource = fieldSource;
            m_ContentTypeProp = contentTypeProp;
            base.DataCache.AddPropertyies(prop);
        }

        #region IAveTaxonomyField Members

        public Guid AnchorId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("AnchorId");
            }
            set
            {
                base.DataCache.AddChangedProperty("AnchorId", value);
            }
        }

        public bool IsPathRendered
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsPathRendered");
            }
            set
            {
                base.DataCache.AddChangedProperty("IsPathRendered", value);
            }
        }

        public bool Open
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Open");
            }
            set
            {
                base.DataCache.AddChangedProperty("Open", value);
            }
        }

        public Guid SspId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("SspId");
            }
            set
            {
                base.DataCache.AddChangedProperty("SspId", value);
            }
        }

        public Guid TermSetId
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("TermSetId");
            }
            set
            {
                base.DataCache.AddChangedProperty("TermSetId", value);
            }
        }

        public Guid TextField
        {
            get
            {
                return base.DataCache.GetProperty<Guid>("TextField");
            }
            set
            {
                base.DataCache.AddChangedProperty("TextField", value);
            }
        }

        public bool UserCreated
        {
            get
            {
                return base.DataCache.GetProperty<bool>("UserCreated");
            }
            set
            {
                base.DataCache.AddChangedProperty("UserCreated", value);
            }
        }

        public bool IsKeyword
        {
            get
            {
                return base.DataCache.GetProperty<bool>("IsKeyword");
            }
        }

        public bool CreateValuesInEditForm
        {
            get
            {
                return base.DataCache.GetProperty<bool>("CreateValuesInEditForm");
            }
            set
            {
                base.DataCache.AddChangedProperty("CreateValuesInEditForm", value);
            }
        }

        public string TargetTemplate
        {
            get
            {
                return base.DataCache.GetProperty<string>("TargetTemplate");
            }
            set
            {
                base.DataCache.AddChangedProperty("TargetTemplate", value);
            }
        }

        public IAveTaxonomyFieldValue TaxonomyFieldValue
        {
            get { return new AveTaxonomyFieldValue(this); }
        }

        public IAveTaxonomyFieldValueCollection TaxonomyFieldValueCollection
        {
            get { return new AveTaxonomyFieldValueCollection(); }
        }

        public string GetValidatedString(IAveTaxonomyFieldValueCollection fieldValueCollection)
        {
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (IAveTaxonomyFieldValue value in fieldValueCollection)
            {
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    builder.Append(";#");
                }
                builder.Append(this.GetValidatedString(value));
            }
            return builder.ToString();
        }

        public string GetValidatedString(IAveTaxonomyFieldValue fieldValue)
        {
            return fieldValue.WssId.ToString() + ";#" + fieldValue.ToString();
        }

        public void SetFieldValue(IAveListItem listItem, IAveTaxonomyFieldValue fieldValue)
        {
            this.m_Request.SetTaxonomyFieldValue(
                listItem.Web.ServerRelativeUrl,
                listItem.ParentList.ID,
                listItem.ID,
                this.InternalName,
                fieldValue.TermGuid.ToString(),
                fieldValue.Label);
        }

        public void SetFieldValue(IAveListItem listItem, ICollection<IAveTerm> terms)
        {
            throw new NotImplementedException();
        }

        #endregion


        public override object GetFieldValue(string value)
        {
            if (value != null)
            {
                if (!AllowMultipleValues)
                {
                    List<string> fieldValue = new List<string>();
                    int lookupId;
                    string lookupValue;
                    if (AveSPUtility.TryParseMultiColumnValue(value, out fieldValue))
                    {
                        if (fieldValue.Count == 2)
                        {
                            lookupValue = fieldValue[1];
                            if (int.TryParse(fieldValue[0], out lookupId))
                            {
                                //return new AveTaxonomyFieldValue(this.mWeb, lookupId, lookupValue);
                            }
                        }
                    }
                }
                else
                {
                   // return new AveFieldUserValueCollection(this.mWeb, value);
                }
            }
            return null;
        }
    }
}
