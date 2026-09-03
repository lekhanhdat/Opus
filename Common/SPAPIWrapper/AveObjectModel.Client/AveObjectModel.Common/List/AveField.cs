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
using System.Xml;
using System.Reflection;
namespace AvePoint.ObjectModel.Common
{
    class AveField : AveClientObject, IAveField
    {
        public AveField(){}
        public string AggregationFunction
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("AggregationFunction"); 
            }
            set
            {
                base.DataCache.AddChangedProperty("AggregationFunction", value); 
            }
        }
        public bool? AllowDeletion
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("AllowDeletion"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("AllowDeletion", value);
            }
        }
        public string ColName 
        { 
            get 
            { 
                return base.DataCache.GetProperty<string>("ColName"); 
            } 
        }
        public string DefaultFormula
        {
            get 
            {
                return base.DataCache.GetProperty<string>("DefaultFormula");
            }
            set 
            {
                base.DataCache.AddChangedProperty("DefaultFormula", value); 
            }
        }
        public string Description
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("Description"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("Description", value);
            }
        }
        public string Direction
        {
            get
            { 
                return base.DataCache.GetProperty<string>("Direction");
            }
            set 
            { 
                base.DataCache.AddChangedProperty("Direction", value); 
            }
        }
        public string DisplaySize
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("DisplaySize"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("DisplaySize", value); 
            }
        }
        public string Group
        {
            get
            { 
                return base.DataCache.GetProperty<string>("Group"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("Group", value);
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
        public Guid Id 
        { 
            get
            {
                return base.DataCache.GetProperty<Guid>("Id");
            }
        }
        public string IMEMode
        {
            get
            { 
                return base.DataCache.GetProperty<string>("IMEMode");
            }
            set 
            {
                base.DataCache.AddChangedProperty("IMEMode", value); 
            }
        }
        public string InternalName 
        { 
            get 
            { 
                return base.DataCache.GetProperty<string>("InternalName"); 
            }
        }
        public bool Indexed
        {
            get 
            { 
                return base.DataCache.GetProperty<bool>("Indexed");
            }
            set
            {
                base.DataCache.AddChangedProperty("Indexed", value); 
            }
        }
        public bool LinkToItem
        {
            get 
            { 
                return base.DataCache.GetProperty<bool>("LinkToItem"); 
            }
            set 
            {
                base.DataCache.AddChangedProperty("LinkToItem", value);
            }
        }
        public bool NoCrawl
        {
            get 
            { 
                return base.DataCache.GetProperty<bool>("NoCrawl"); 
            }
            set
            { 
                base.DataCache.AddChangedProperty("NoCrawl", value);
            }
        }
        public XmlNode Node 
        { 
            get
            {
                return base.DataCache.GetProperty<XmlNode>("Node"); 
            } 
        }
        public string JumpToField
        {
            get
            {
                return base.DataCache.GetProperty<string>("JumpToField"); 
            }
            set 
            {             
                base.DataCache.AddChangedProperty("JumpToField", value);
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
            }
        }
        public string RelatedField
        {
            get 
            {
                return base.DataCache.GetProperty<string>("RelatedField");
            }
            set
            { 
                base.DataCache.AddChangedProperty("RelatedField", value); 
            }
        }
        public bool ReadOnlyField
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("ReadOnlyField");
            }
            set
            { 
                base.DataCache.AddChangedProperty("ReadOnlyField", value); 
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
            }
        }
        public int RowOrdinal 
        {
            get 
            { 
                return base.DataCache.GetProperty<int>("RowOrdinal");
            }
        }
        public String SchemaXml
        {
            get
            { 
                return base.DataCache.GetProperty<String>("SchemaXml");
            }
            set 
            { 
                base.DataCache.AddChangedProperty("SchemaXml", value);
            }
        }
        public bool Sealed
        {
            get
            {
                return base.DataCache.GetProperty<bool>("Sealed"); 
            }
            set
            { 
                base.DataCache.AddChangedProperty("Sealed", value);
            }
        }
        public string SchemaXmlWithResourceTokens
        { 
            get 
            { 
                return base.DataCache.GetProperty<string>("SchemaXmlWithResourceTokens");
            }
        }
        public bool? ShowInDisplayForm
        {
            get 
            { 
                return base.DataCache.GetProperty<bool>("ShowInDisplayForm"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("ShowInDisplayForm", value); 
            }
        }
        public bool? ShowInEditForm
        {
            get
            { 
                return base.DataCache.GetProperty<bool>("ShowInEditForm");
            }
            set 
            { 
                base.DataCache.AddChangedProperty("ShowInEditForm", value); 
            }
        }
        public bool? ShowInListSettings
        {
            get 
            { 
                return base.DataCache.GetProperty<bool>("ShowInListSettings"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("ShowInListSettings", value); 
            }
        }
        public bool? ShowInNewForm
        {
            get 
            { 
                return base.DataCache.GetProperty<bool>("ShowInNewForm"); 
            }
            set
            { 
                base.DataCache.AddChangedProperty("ShowInNewForm", value); 
            }
        }
        public bool ShowInVersionHistory
        {
            get 
            { 
                return base.DataCache.GetProperty<bool>("ShowInVersionHistory"); 
            }
            set
            { 
                base.DataCache.AddChangedProperty("ShowInVersionHistory", value);
            }
        }
        public bool? ShowInViewForms
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("ShowInViewForms"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("ShowInViewForms", value); 
            }
        }
        public String StaticName 
        { 
            get
            {
                return base.DataCache.GetProperty<String>("StaticName");
            } 
            set
            {
                base.DataCache.AddChangedProperty("StaticName", value);
            } 
        }
        public String Title
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
        public string TranslationXml
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("TranslationXml");
            }
            set
            {
                base.DataCache.AddChangedProperty("TranslationXml", value);
            }
        }
        public AveFieldType Type
        { 
            get
            {
                return base.DataCache.GetProperty<AveFieldType>("Type");
            }
            set
            {
                base.DataCache.AddChangedProperty("Type", value);
            }
        }
        public String TypeAsString
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("TypeAsString"); 
            }
            set
            { 
                base.DataCache.AddChangedProperty("TypeAsString", value); 
            }
        }
        public string TypeDisplayName
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("TypeDisplayName");
            }
        }
        public string ValidationFormula
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("ValidationFormula"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("ValidationFormula", value); 
            }
        }
        public string ValidationMessage
        {
            get 
            { 
                return base.DataCache.GetProperty<string>("ValidationMessage"); 
            }
            set 
            {
                base.DataCache.AddChangedProperty("ValidationMessage", value);
            }
        }
        public AveCompositeIndexableStatus CompositeIndexable
        {
            get 
            {
                return base.DataCache.GetProperty<AveCompositeIndexableStatus>("CompositeIndexable"); 
            } 
        }
        public string DefaultValue
        {
            get 
            {
                return base.DataCache.GetProperty<string>("DefaultValue"); 
            }
            set 
            { 
                base.DataCache.AddChangedProperty("DefaultValue", value);
            } 
        }
        public IAveFieldTypeDefinition FieldTypeDefinition 
        {
            get 
            {
                return base.DataCache.GetProperty<IAveFieldTypeDefinition>("FieldTypeDefinition"); 
            } 
        }
        public bool Reorderable
        {
            get
            { 
                return base.DataCache.GetProperty<bool>("Reorderable"); 
            } 
        }


        public bool Indexable
        {
            get 
            {
                return base.DataCache.GetProperty<bool>("Indexable"); 
            } 
        }

        public IAveList ParentList 
        {
            get 
            {
                return base.DataCache.GetProperty<IAveList>("ParentList");
            } 
        }

        public void Delete()
        {
            throw new NotImplementedException();
        }
        public string GetProperty(string propName)
        {

            throw new NotImplementedException();
        }
        public void Update()
        {

            throw new NotImplementedException();
        }

        #region private method
        private AveFieldType ConvertType(string type)
        {
            foreach (FieldInfo field in typeof(AveFieldType).GetFields())
            {
                if (field.Name.Equals(type))
                {
                    return (AveFieldType)field.GetValue(AveFieldType.Invalid);
                }
            }
            return AveFieldType.Invalid;
        }
        private AveFieldType ConvertTypeII(string type)
        {
            AveFieldType T = AveFieldType.Invalid;
            switch (type)
            {
                case "Integer":
                    T = AveFieldType.Integer;
                    break;
                case "Text":
                    T = AveFieldType.Text;
                    break;
                case "Note":
                    T = AveFieldType.Note;
                    break;
                case "DateTime":
                    T = AveFieldType.DateTime;
                    break;
                case "Counter":
                    T = AveFieldType.Counter;
                    break;
                case "Choice":
                    T = AveFieldType.Choice;
                    break;
                case "Lookup":
                    T = AveFieldType.Lookup;
                    break;
                case "Boolean":
                    T = AveFieldType.Boolean;
                    break;
                case "Number":
                    T = AveFieldType.Number;
                    break;
                case "Currency":
                    T = AveFieldType.Currency;
                    break;
                case "URL":
                    T = AveFieldType.URL;
                    break;
                case "Computed":
                    T = AveFieldType.Computed;
                    break;
                case "Threading":
                    T = AveFieldType.Threading;
                    break;
                case "Guid":
                    T = AveFieldType.Guid;
                    break;
                case "MultiChoice":
                    T = AveFieldType.MultiChoice;
                    break;
                case "GridChoice":
                    T = AveFieldType.GridChoice;
                    break;
                case "Calculated":
                    T = AveFieldType.Calculated;
                    break;
                case "File":
                    T = AveFieldType.File;
                    break;
                case "Attachments":
                    T = AveFieldType.Attachments;
                    break;
                case "User":
                    T = AveFieldType.User;
                    break;
                case "Recurrence":
                    T = AveFieldType.Recurrence;
                    break;
                case "ModStat":
                    T = AveFieldType.ModStat;
                    break;
                case "CrossProjectLink":
                    T = AveFieldType.CrossProjectLink;
                    break;
                case "Error":
                    T = AveFieldType.Error;
                    break;
                case "ContentTypeId":
                    T = AveFieldType.ContentTypeId;
                    break;
                case "PageSeparator":
                    T = AveFieldType.PageSeparator;
                    break;
                case "ThreadIndex":
                    T = AveFieldType.ThreadIndex;
                    break;
                case "WorkflowStatus":
                    T = AveFieldType.WorkflowStatus;
                    break;
                case "AllDayEvent":
                    T = AveFieldType.AllDayEvent;
                    break;
                case "WorkflowEventType":
                    T = AveFieldType.WorkflowEventType;
                    break;
                case "MaxItems":
                    T = AveFieldType.MaxItems;
                    break;
                default:
                    break;
            }
            return T;
        }
        #endregion
    }
}
