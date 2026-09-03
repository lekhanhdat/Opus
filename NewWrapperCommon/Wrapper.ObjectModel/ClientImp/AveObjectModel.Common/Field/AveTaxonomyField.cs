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
namespace AvePoint.ObjectModel.Common
{

    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Reflection;
    using System.Text;
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;

    class AveTaxonomyField : AveFieldLookup, IAveTaxonomyField
    {
        private IAveRequest m_Request;
        private AveList m_ParentList;
        private AveWeb m_Web;
        private AveFieldCollection m_FieldCollection;
        private string m_FieldSource;
        private Dictionary<string, object> m_ContentTypeProp;
        protected static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public AveTaxonomyField(IAveRequest request, AveList list, AveWeb web, string fieldSource, AveFieldCollection fieldCollection, Dictionary<string, object> contentTypeProp, Dictionary<string, object> prop)
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
            get 
            {
                return new AveTaxonomyFieldValue(this);
            }
        }

        public IAveTaxonomyFieldValueCollection TaxonomyFieldValueCollection
        {
            get 
            {
                return new AveTaxonomyFieldValueCollection();
            }
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

        public void SetFieldValue(IAveListItem listItem, IAveTerm term)
        {
            if ((listItem == null) || (term == null))
            {
                throw new ArgumentException();
            }
            string fieldValue = term.Name + "|" + term.ID.ToString();
            listItem[base.ID] = fieldValue;
            listItem[this.TextField] = fieldValue;
        }

        public void SetFieldValue(IAveListItem listItem, ICollection<IAveTerm> terms)
        {
            if ((listItem == null) || (terms == null))
            {
                throw new ArgumentException();
            }
            StringBuilder builder = new StringBuilder();
            bool flag = true;
            foreach (IAveTerm term in terms)
            {
                if (term.ID == null || term.Name == null)
                {
                    continue;
                }
                if (flag)
                {
                    flag = false;
                }
                else
                {
                    builder.Append(';');
                }
                builder.Append(term.Name+"|"+term.ID);
            }
            listItem[base.ID] = builder.ToString();
            listItem[this.TextField] = builder.ToString();
        }

        #endregion
        public Dictionary<string, object> GetFieldValueAsTaxonomyFieldValue(string text)
        {
            var properties = new Dictionary<string, object>();
            //10模拟没实现
            if(m_Request is IAveRequest)
            {
                properties = (m_Request ).GetFieldValueAsTaxonomyFieldValue(this.m_Web.ServerRelativeUrl, this.ParentList == null ? Guid.Empty : this.ParentList.ID, this.ID, text);

            }
            return properties;
        }

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

        public override string GetFieldValueAsText(object obj)
        {
            string stringValue = obj as string;
            if (!string.IsNullOrEmpty(stringValue))
            {
                string[] values = stringValue.Split(';');
                StringBuilder builder = new StringBuilder();
                foreach(string value in values)
                {
                    var index = value.IndexOf('|');
                    if(index == 0)
                    {
                         continue;   
                    }
                    if (index < 0)
                    {
                        builder.Append(value);
                    }
                    else
                    {
                        builder.Append(value.Substring(0, index));
                    }
                    builder.Append(';');
                }
                return builder.ToString().TrimEnd(';');
            }
            return String.Empty;
        }

        public override void Delete()
        {
            if (this.m_FieldCollection.Contains(this.TextField))
            {
                this.m_FieldCollection.ListData.Remove(this.m_FieldCollection[this.TextField]);
            }
            base.Delete();
        }

        public override object DefaultValueTyped
        {
            get
            {
                return InitializeDefaultValueTyped();

            }
        }

        private string InitializeDefaultValueTyped()
        {
            string fieldValue = DefaultValue;
            if (string.IsNullOrEmpty(fieldValue))
            {
                mLog.Debug("This Field does not have default values.Field Title:{0}", Title);
                return null;
            }
            StringBuilder builder = new StringBuilder();
            try
            {
                string[] strArray2 = fieldValue.Split(new string[] { ";#" }, StringSplitOptions.None);
                if ((strArray2.Length % 2) != 0)
                {
                    throw new ArgumentException("ErrorValueNotFormatted");
                }
                List<string> values = new List<string> { };
                for (int i = 0; i < strArray2.Length; i += 2)
                {
                    if (!string.IsNullOrEmpty(strArray2[i]) && !string.IsNullOrEmpty(strArray2[i + 1]))
                    {
                        int lookupId = -1;
                        if (!int.TryParse(strArray2[i], NumberStyles.Integer, CultureInfo.InvariantCulture, out lookupId))
                        {
                            throw new ArgumentException("LookupIdNotFormatted");
                        }
                        builder.Append(strArray2[i + 1]);
                        builder.Append(';');
                    }
                }
            }
            catch (Exception ex)
            {
                mLog.Warn("An error occurred while get field DefaultValueTyped by DefaultValue.Field Title:{0},DefaultValue:{1},Error:{2}", Title, DefaultValue, ex.ToString());
            }
            if (builder.Length > 0)
            {
                builder.Length--;
            }
            return builder.ToString();
        }
    }
}
