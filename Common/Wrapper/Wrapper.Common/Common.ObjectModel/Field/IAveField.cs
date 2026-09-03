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
using System.Text;
using System.Xml;

namespace AvePoint.Wrapper.Common
{
    public interface IAveField
    {
        string AggregationFunction { get; set; }
        bool? AllowDeletion { get; set; }
        string ColName { get; }
        string DefaultFormula { get; set; }
        string DefaultValue { get; set; }
        object DefaultValueTyped { get; }
        string Description { get; set; }
        IAveUserResource DescriptionResource { get; }
        string Direction { get; set; }
        string DisplaySize { get; set; }
        string Group { get; set; }
        bool Hidden { get; set; }
        Guid ID { get; }
        string IMEMode { get; set; }
        string InternalName { get; }
        bool Indexed { get; set; }
        bool LinkToItem { get; set; }
        string MD5 { get; set; }
        bool NoCrawl { get; set; }
        XmlNode Node { get; }
        string JumpToField { get; set; }
        IAveList ParentList { get; }
        string PIAttribute { get; set; }
        string PITarget { get; set; }
        string PrimaryPIAttribute { get; set; }
        string PrimaryPITarget { get; set; }
        string RelatedField { get; set; }
        bool ReadOnlyField { get; set; }
        bool Reorderable { get; }
        bool Required { get; set; }
        String SchemaXml { get; set; }
        bool Sealed { get; set; }
        string SchemaXmlWithResourceTokens { get; }
        string Scope { get; set; }
        bool? ShowInDisplayForm { get; set; }
        bool? ShowInEditForm { get; set; }
        bool? ShowInListSettings { get; set; }
        bool? ShowInNewForm { get; set; }
        bool ShowInVersionHistory { get; set; }
        bool? ShowInViewForms { get; set; }
        String StaticName { get; set; }
        String Title { get; set; }
        IAveUserResource TitleResource { get; }
        string TranslationXml { get; set; }
        AveFieldType Type { get; set; }
        String TypeAsString { get; set; }
        string TypeDisplayName { get; }     //
        string ValidationFormula { get; set; }
        string ValidationMessage { get; set; }
        bool Indexable { get; }
        AveCompositeIndexableStatus CompositeIndexable { get; }
        IAveFieldTypeDefinition FieldTypeDefinition { get; }
        string SourceId { get; }
        Type FieldValueType { get; }
        int RowOrdinal { get; }
        bool CanBeDisplayedInEditForm { get; }
        string EntityPropertyName { get; }

        void Delete();
        string GetProperty(string propName);
        void Update();
        void UpdateReadOnlyField(); //used for reduce communication count in bpos-d, currently the code is field.ReadOnly = false; field.Update(); field.ReadOnly = true; field.Update()
        bool RemoveFieldAttributeValue(string attrName);
        string SetFieldAttributeValue(string attrName, string attrValue);
        void SetFieldValue(string name, object value);
        string GetFieldAttributeValue(string attrName);
        string GetFieldValueAsText(object uV);
        object GetFieldValue(string value);
        string GetAttributeFromSchemaXml(string attrName);
        object GetCustomProperty(string p);
        void SetCustomProperty(string propertyName, object propertyValue);

        bool EnforceUniqueValues { get; set; }

        void SetShowInDisplayForm(bool value);
        void SetShowInEditForm(bool value);
        void SetShowInNewForm(bool value);

        string JSLink { get; set; }
    }
}