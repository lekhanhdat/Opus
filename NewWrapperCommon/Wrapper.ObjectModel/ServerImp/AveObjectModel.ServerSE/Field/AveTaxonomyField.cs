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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Taxonomy;
using AvePoint.Wrapper.Common;
using System.Text;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveTaxonomyField : AveFieldLookup, IAveTaxonomyField
    {
        private TaxonomyField m_TaxonomyField;

        public AveTaxonomyField(AveFieldCollection fieldColl, TaxonomyField field)
            : base(fieldColl, field)
        {
            m_TaxonomyField = field;
        }

        internal TaxonomyField TaxonomyField
        {
            get { return m_TaxonomyField; }
        }

        public override Type FieldValueType
        {
            get
            {
                if (!this.AllowMultipleValues)
                {
                    return typeof(IAveTaxonomyFieldValue);
                }
                return typeof(IAveTaxonomyFieldValueCollection);
            }
        }

        public override object GetFieldValue(string value)
        {
            if (!this.AllowMultipleValues)
            {
                return new AveTaxonomyFieldValue(new TaxonomyFieldValue(value, m_TaxonomyField));
            }
            if ((this.IsKeyword && !string.IsNullOrEmpty(value)) && value.Equals(";#"))
            {
                return value;
            }
            return new AveTaxonomyFieldValueCollection(new TaxonomyFieldValueCollection(value, m_TaxonomyField));
        }

        public override string GetFieldValueAsText(object value)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveTaxonomyField.GetFieldValueAsText"))
            {


                if (value == null)
                {
                    return string.Empty;
                }
                AveTaxonomyFieldValue value2 = value as AveTaxonomyFieldValue;
                if (value2 != null)
                {
                    return value2.Label;
                }
                AveTaxonomyFieldValueCollection values = value as AveTaxonomyFieldValueCollection;
                if (values != null)
                {
                    StringBuilder builder = new StringBuilder();
                    bool flag = true;
                    foreach (AveTaxonomyFieldValue value3 in values)
                    {
                        if (flag)
                        {
                            flag = false;
                        }
                        else
                        {
                            builder.Append(';');
                        }
                        builder.Append(this.GetFieldValueAsText(value3));
                    }
                    return builder.ToString();
                }
                throw new ArgumentException();


            }

        }

        public override void Update()
        {
            m_TaxonomyField.Update();
        }

        #region IAveTaxonomyField Members

        public Guid AnchorId
        {
            get
            {
                return m_TaxonomyField.AnchorId;
            }
            set
            {
                m_TaxonomyField.AnchorId = value;
            }
        }

        public bool IsPathRendered
        {
            get
            {
                return m_TaxonomyField.IsPathRendered;
            }
            set
            {
                m_TaxonomyField.IsPathRendered = value;
            }
        }

        public bool Open
        {
            get
            {
                return m_TaxonomyField.Open;
            }
            set
            {
                m_TaxonomyField.Open = value;
            }
        }

        public Guid SspId
        {
            get
            {
                return m_TaxonomyField.SspId;
            }
            set
            {
                m_TaxonomyField.SspId = value;
            }
        }

        public Guid TermSetId
        {
            get
            {
                return m_TaxonomyField.TermSetId;
            }
            set
            {
                m_TaxonomyField.TermSetId = value;
            }
        }

        public Guid TextField
        {
            get
            {
                return m_TaxonomyField.TextField;
            }
            set
            {
                m_TaxonomyField.TextField = value;
            }
        }

        public bool UserCreated
        {
            get
            {
                return m_TaxonomyField.UserCreated;
            }
            set
            {
                m_TaxonomyField.UserCreated = value;
            }
        }

        public bool IsKeyword
        {
            get
            {
                return m_TaxonomyField.IsKeyword;
            }
        }

        public bool CreateValuesInEditForm
        {
            get
            {
                return m_TaxonomyField.CreateValuesInEditForm;
            }
            set
            {
                m_TaxonomyField.CreateValuesInEditForm = value;
            }
        }

        public string TargetTemplate
        {
            get
            {
                return m_TaxonomyField.TargetTemplate;
            }
            set
            {
                m_TaxonomyField.TargetTemplate = value;
            }
        }

        public IAveTaxonomyFieldValue TaxonomyFieldValue
        {
            get
            {
                return new AveTaxonomyFieldValue(this);
            }
        }

        public IAveTaxonomyFieldValueCollection TaxonomyFieldValueCollection
        {
            get
            {
                return new AveTaxonomyFieldValueCollection(this);
            }
        }

        public string GetValidatedString(IAveTaxonomyFieldValueCollection fieldValueCollection)
        {
            return m_TaxonomyField.GetValidatedString((fieldValueCollection as AveTaxonomyFieldValueCollection).TaxonomyFieldValueCollection);
        }

        public string GetValidatedString(IAveTaxonomyFieldValue fieldValue)
        {
            return m_TaxonomyField.GetValidatedString((fieldValue as AveTaxonomyFieldValue).TaxonomyFieldValue);
        }

        public void SetFieldValue(IAveListItem listItem, IAveTerm term)
        {
            m_TaxonomyField.SetFieldValue((listItem as AveListItem).ListItem, (term as AveTerm).Term);
        }

        public void SetFieldValue(IAveListItem listItem, ICollection<IAveTerm> terms)
        {
            List<Term> mTerms = new List<Term>();
            foreach (IAveTerm term in terms)
            {
                mTerms.Add(((AveTerm)term).Term);
            }
            m_TaxonomyField.SetFieldValue((listItem as AveListItem).ListItem, mTerms);
        }

        public override object DefaultValueTyped
        {
            get
            {
                if (this.DefaultValue == null || this.DefaultValue != null && string.IsNullOrEmpty(this.DefaultValue)) return null;
                if (this.AllowMultipleValues) return new AveTaxonomyFieldValueCollection(new TaxonomyFieldValueCollection(this.DefaultValue, m_TaxonomyField));
                return new AveTaxonomyFieldValue(new TaxonomyFieldValue(this.DefaultValue, m_TaxonomyField));
            }
        }

        #endregion
    }
}
