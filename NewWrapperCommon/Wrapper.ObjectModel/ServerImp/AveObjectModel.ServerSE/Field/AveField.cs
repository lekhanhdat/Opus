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
using Microsoft.SharePoint;
using AvePoint.Wrapper.Common;
using AvePoint.Common;
using System.Xml;
using System.Text;
using System.Globalization;
using Microsoft.SharePoint.Taxonomy;

namespace AvePoint.ObjectModel.ServerSE
{
    class AveField : AveServerObject, IAveField
    {
        private AveFieldCollection mFields;
        private SPField mField;
        private AveFieldTypeDefinition mFieldTypeDefinition;
        private string mMD5;
        private string mBaseFieldType;

        public AveField(AveFieldCollection fieldColl, SPField field)
        {
            mFields = fieldColl;
            mField = field;
        }

        public AveField(SPField field)
        {
            mField = field;
        }

        internal SPField Field
        {
            get
            {
                return mField;
            }
        }

        public IAveFieldCollection Fields
        {
            get
            {
                return mFields;
            }
        }

        #region IAveField Members

        public string Title
        {
            get
            {
                if (mField.TitleResource == null)
                {
                    return mField.Title;
                }
                CultureInfo culture = System.Globalization.CultureInfo.InvariantCulture;
                if (Fields.Web != null)
                {
                    culture = Fields.Web.UICulture;
                }
                return mField.TitleResource.GetValueForUICulture(culture);
            }
            set
            {
                if (mField.TitleResource != null && (int)mFields.Web.Language != CultureInfo.CurrentUICulture.LCID)
                {
                    CultureInfo cul = new CultureInfo((int)mFields.Web.Language);
                    mField.TitleResource.SetValueForUICulture(cul, value);
                    SetFieldAttributeValue("DisplayName", value);
                }
                else
                {
                    mField.Title = value;
                }
            }
        }

        public string StaticName
        {
            get
            {
                return mField.StaticName;
            }
            set
            {
                mField.StaticName = value;
            }
        }

        public string TypeAsString
        {
            get
            {
                return mField.TypeAsString;
            }
            set
            {
                mField.TypeAsString = value;
            }
        }

        public string SchemaXml
        {
            get
            {
                return mField.SchemaXml;
            }
            set
            {
                mField.SchemaXml = value;
            }
        }

        public bool ReadOnlyField
        {
            get
            {
                return mField.ReadOnlyField;
            }
            set
            {
                mField.ReadOnlyField = value;
            }
        }

        public Guid ID
        {
            get
            {
                return mField.Id;
            }
        }

        public string AggregationFunction
        {
            get
            {
                return mField.AggregationFunction;
            }
            set
            {
                mField.AggregationFunction = value;
            }
        }

        public bool? AllowDeletion
        {
            get
            {
                return mField.AllowDeletion;
            }
            set
            {
                mField.AllowDeletion = value;
            }
        }

        public string DefaultFormula
        {
            get
            {
                return mField.DefaultFormula;
            }
            set
            {
                mField.DefaultFormula = value;
            }
        }

        public string Description
        {
            get
            {
                return mField.Description;
            }
            set
            {
                if ((int)mFields.Web.Language != CultureInfo.CurrentUICulture.LCID)
                {
                    CultureInfo cul = new CultureInfo((int)mFields.Web.Language);
                    mField.DescriptionResource.SetValueForUICulture(cul, value);
                    SetFieldAttributeValue("Description", value);
                }
                else
                {
                    mField.Description = value;
                }
            }
        }

        public string Direction
        {
            get
            {
                return mField.Direction;
            }
            set
            {
                mField.Direction = value;
            }
        }

        public string DisplaySize
        {
            get
            {
                return mField.DisplaySize;
            }
            set
            {
                mField.DisplaySize = value;
            }
        }

        public string Group
        {
            get
            {
                return mField.Group;
            }
            set
            {
                mField.Group = value;
            }
        }

        public bool Hidden
        {
            get
            {
                return mField.Hidden;
            }
            set
            {
                mField.Hidden = value;
            }
        }

        public virtual string IMEMode
        {
            get
            {
                return mField.IMEMode;
            }
            set
            {
                mField.IMEMode = value;
            }
        }

        public string InternalName
        {
            get { return mField.InternalName; }
        }

        public virtual bool Indexed
        {
            get
            {
                return mField.Indexed;
            }
            set
            {
                mField.Indexed = value;
            }
        }

        public bool LinkToItem
        {
            get
            {
                return mField.LinkToItem;
            }
            set
            {
                mField.LinkToItem = value;
            }
        }

        public virtual bool NoCrawl
        {
            get
            {
                return mField.NoCrawl;
            }
            set
            {
                mField.NoCrawl = value;
            }
        }

        public string JumpToField
        {
            get
            {
                return mField.JumpToField;
            }
            set
            {
                mField.JumpToField = value;
            }
        }

        public string PIAttribute
        {
            get
            {
                return mField.PIAttribute;
            }
            set
            {
                mField.PIAttribute = value;
            }
        }

        public string PITarget
        {
            get
            {
                return mField.PITarget;
            }
            set
            {
                mField.PITarget = value;
            }
        }

        public string PrimaryPIAttribute
        {
            get
            {
                return mField.PrimaryPIAttribute;
            }
            set
            {
                mField.PrimaryPIAttribute = value;
            }
        }

        public string PrimaryPITarget
        {
            get
            {
                return mField.PrimaryPITarget;
            }
            set
            {
                mField.PrimaryPITarget = value;
            }
        }

        public string RelatedField
        {
            get
            {
                return mField.RelatedField;
            }
            set
            {
                mField.RelatedField = value;
            }
        }

        public bool Required
        {
            get
            {
                return mField.Required;
            }
            set
            {
                mField.Required = value;
            }
        }

        public bool Sealed
        {
            get
            {
                return mField.Sealed;
            }
            set
            {
                mField.Sealed = value;
            }
        }

        public string SchemaXmlWithResourceTokens
        {
            get { return mField.SchemaXmlWithResourceTokens; }
        }

        public bool? ShowInDisplayForm
        {
            get
            {
                return mField.ShowInDisplayForm;
            }
            set
            {
                mField.ShowInDisplayForm = value;
            }
        }

        public bool? ShowInEditForm
        {
            get
            {
                return mField.ShowInEditForm;
            }
            set
            {
                mField.ShowInEditForm = value;
            }
        }

        public bool? ShowInListSettings
        {
            get
            {
                return mField.ShowInListSettings;
            }
            set
            {
                mField.ShowInListSettings = value;
            }
        }

        public bool? ShowInNewForm
        {
            get
            {
                return mField.ShowInNewForm;
            }
            set
            {
                mField.ShowInNewForm = value;
            }
        }

        public bool ShowInVersionHistory
        {
            get
            {
                return mField.ShowInVersionHistory;
            }
            set
            {
                mField.ShowInVersionHistory = value;
            }
        }

        public bool? ShowInViewForms
        {
            get
            {
                return mField.ShowInViewForms;
            }
            set
            {
                mField.ShowInViewForms = value;
            }
        }

        public string TranslationXml
        {
            get
            {
                return mField.TranslationXml;
            }
            set
            {
                mField.TranslationXml = value;
            }
        }

        public AveFieldType Type
        {
            get
            {
                return (AveFieldType)Enum.Parse(typeof(AveFieldType), mField.Type.ToString());
            }
            set
            {
                mField.Type = (SPFieldType)Enum.Parse(typeof(SPFieldType), value.ToString());
            }
        }

        public virtual string TypeDisplayName
        {
            get
            {
                return mField.TypeDisplayName;
            }
        }

        public string ValidationFormula
        {
            get
            {
                return mField.ValidationFormula;
            }
            set
            {
                mField.ValidationFormula = value;
            }
        }

        public string ValidationMessage
        {
            get
            {
                return mField.ValidationMessage;
            }
            set
            {
                mField.ValidationMessage = value;
            }
        }

        public virtual string GetProperty(string propName)
        {
            return mField.GetProperty(propName);
        }

        public virtual void Update()
        {
            mField.Update();
        }

        public void UpdateReadOnlyField()
        {
            mField.ReadOnlyField = false;
            mField.Update();
            mField = mFields.FieldCollection[mField.Id];
            mField.ReadOnlyField = true;
            mField.Update();
        }

        public void Delete()
        {
            mField.Delete();
        }

        public XmlNode Node
        {
            get { return (XmlNode)AveAssemblyUtility.GetPropertyValue(mField, "Node"); }
        }

        public string ColName
        {
            get
            {
                //这个属性有可能是null，所以在ToString()之前需要进行判断
                object obj = AveAssemblyUtility.GetPropertyValue(mField, "ColName");
                if (obj == null)
                {
                    return null;
                }
                return obj.ToString();
            }
        }

        public int RowOrdinal
        {
            get { return (int)AveAssemblyUtility.GetPropertyValue(mField, "RowOrdinal"); }
        }

        public virtual string DefaultValue
        {
            get
            {
                return mField.DefaultValue;
            }
            set
            {
                mField.DefaultValue = value;
            }
        }

        public IAveList ParentList
        {
            get
            {
                return mFields.List;
            }
        }

        public bool Indexable
        {
            get { return mField.Indexable; }
        }

        public AveCompositeIndexableStatus CompositeIndexable
        {
            get { return (AveCompositeIndexableStatus)mField.CompositeIndexable; }
        }

        public IAveFieldTypeDefinition FieldTypeDefinition
        {
            get
            {
                if (mFieldTypeDefinition == null)
                {
                    SPFieldTypeDefinition fieldTypeDefinition = mField.FieldTypeDefinition;
                    if (fieldTypeDefinition != null)
                    {
                        mFieldTypeDefinition = new AveFieldTypeDefinition(fieldTypeDefinition);
                    }
                }
                return mFieldTypeDefinition;
            }
        }

        public bool Reorderable
        {
            get { return mField.Reorderable; }
        }

        public virtual string GetFieldValueAsText(object value)
        {
            if (value == null)
            {
                return string.Empty;
            }
            return value.ToString();
        }

        public string SourceId
        {
            get { return mField.SourceId; }
        }

        public bool RemoveFieldAttributeValue(string attrName)
        {
            return (bool)AveAssemblyUtility.InvokeMethod(mField, "RemoveFieldAttributeValue", new Type[] { typeof(string) }, new object[] { attrName });
        }

        public string SetFieldAttributeValue(string attrName, string attrValue)
        {
            return AveAssemblyUtility.InvokeMethod(mField, "SetFieldAttributeValue", new Type[] { typeof(string), typeof(string) }, new object[] { attrName, attrValue }).ToString();
        }

        public void SetFieldValue(string name, object value)
        {
            AveAssemblyUtility.SetFieldValue(mField, name, value);
        }

        public string GetFieldAttributeValue(string attrName)
        {
            object value = AveAssemblyUtility.InvokeMethod(mField, "GetFieldAttributeValue", new Type[] { typeof(string) }, new object[] { attrName });
            if (value == null)
            {
                return string.Empty;
            }
            return value.ToString();
        }

        public object GetPropertyValue(string ColName)
        {
            return AveAssemblyUtility.GetPropertyValue(mField, ColName);
        }

        public virtual object GetFieldValue(string value)
        {
            return value;
        }

        public virtual Type FieldValueType
        {
            get
            {
                if (this.Type == AveFieldType.Guid)
                {
                    return typeof(Guid);
                }
                return null;
            }
        }

        public string GetAttributeFromSchemaXml(string attrName)
        {

            using (AvePerformanceScope pc = new AvePerformanceScope("Server.AveField.GetAttributeFromSchemaXml"))
            {

                if (base.DataCache.IsPropertyNotLoaded("SchemaXmlDocument"))
                {
                    XmlDocument xmlD = new XmlDocument();
                    xmlD.LoadXml(this.SchemaXml);
                    base.DataCache.PropertiesCache.Add("SchemaXmlDocument", xmlD);
                }
                XmlDocument sxd = base.DataCache.PropertiesCache["SchemaXmlDocument"] as XmlDocument;
                XmlNode node = sxd.SelectSingleNode("Field");
                if (node != null && node.Attributes[attrName] != null)
                {
                    return node.Attributes[attrName].Value;
                }
                else
                {
                    return null;
                }

            }

        }

        public object GetCustomProperty(string p)
        {
            return this.mField.GetCustomProperty(p);
        }

        public void SetCustomProperty(string propertyName, object propertyValue)
        {
            this.mField.SetCustomProperty(propertyName, propertyValue);
        }

        public string Scope
        {
            get { return mField.Scope; }
        }

        #endregion


        public bool EnforceUniqueValues
        {
            get
            {
                return mField.EnforceUniqueValues;
            }
            set
            {
                mField.EnforceUniqueValues = value;
            }
        }

        public string MD5
        {
            get
            {
                return mMD5;
            }
            set
            {
                mMD5 = value;
            }
        }

        public bool CanBeDisplayedInEditForm
        {
            get
            {
                return mField.CanBeDisplayedInEditForm;
            }
        }
        
        public virtual object DefaultValueTyped
        {
            get
            {
                return mField.DefaultValueTyped;
            }
        }

        public bool CalloutMenu
        {
            get
            {
                return mField.CalloutMenu;
            }
            set
            {
                mField.CalloutMenu = value;
            }
        }

        public string EntityPropertyName
        {
            get { return mField.EntityPropertyName; }
        }

        public string JSLink
        {
            get
            {
                return mField.JSLink;
            }
            set
            {
                mField.JSLink = value;
            }
        }

        public string BaseTypeString
        {
            get
            {
                if (string.IsNullOrEmpty(mBaseFieldType))
                {
                    if (Type == AveFieldType.Invalid)
                    {
                        if (mField is SPFieldUser)
                        {
                            mBaseFieldType = (mField as SPFieldUser).AllowMultipleValues ? "UserMulti" : "User";
                        }
                        else if (mField is SPFieldLookup)
                        {
                            mBaseFieldType = (mField as SPFieldLookup).AllowMultipleValues ? "LookupMulti" : "Lookup";
                        }
                        else
                        {
                            mBaseFieldType = TypeAsString;
                        }
                        if (AveEnv.IsMoss)
                        {
                            GetTaxonomyFieldBaseTypeString();
                        }
                    }
                    else
                    {
                        mBaseFieldType = TypeAsString;
                    }
                }
                return mBaseFieldType;
            }
        }

        private void GetTaxonomyFieldBaseTypeString()
        {
            if (mField is TaxonomyField)
            {
                mBaseFieldType = (mField as TaxonomyField).AllowMultipleValues ? "TaxonomyFieldTypeMulti" : "TaxonomyFieldType";
            }
        }
        #region User Resource
        public IAveUserResource DescriptionResource
        {
            get { return new AveUserResource(Field.DescriptionResource); }
        }

        public IAveUserResource TitleResource
        {
            get { return new AveUserResource(Field.TitleResource); }
        }
        #endregion


        public bool CanBeDeleted
        {
            get { return mField.CanBeDeleted; }
        }

        public string ValidationEcmaScript
        {
            get
            {
                return mField.ValidationEcmaScript;
            }
        }

        public bool Sortable
        {
            get { return mField.Sortable; }
        }

        public bool FromBaseType
        {
            get
            {
                return mField.FromBaseType;
            }
        }
    }
}
