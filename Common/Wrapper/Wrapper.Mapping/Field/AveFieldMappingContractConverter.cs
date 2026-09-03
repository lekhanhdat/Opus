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
using AvePoint.GCommon.Contract.Server.ControlPanel.ColumnMapping.Object;

namespace AvePoint.Wrapper.Mapping
{
    public class AveFieldMappingContractConverter
    {
        public static XmlDocument Convert(ColumnMappingDataContract contract)
        {
            XmlDocument doc = new XmlDocument();
            doc.AppendChild(doc.CreateElement("FieldMappings"));
            if (contract.MappingList != null)
            {
                foreach (ConditionAndColumnMapping mapping in contract.MappingList)
                {
                    XmlElement fieldMapping = doc.CreateElement("FieldMapping");
                    doc.DocumentElement.AppendChild(fieldMapping);
                    XmlElement condition = doc.CreateElement("Condition");
                    fieldMapping.AppendChild(condition);
                    XmlElement siteCondition = doc.CreateElement("SiteCondition");
                    if (mapping.SiteFilterList != null)
                    {
                        foreach (ColumnFilter siteFilter in mapping.SiteFilterList)
                        {
                            InitConditionRule(siteCondition, siteFilter.Conditions);  
                        }
                    }
                    condition.AppendChild(siteCondition);
                    XmlElement listCondition = doc.CreateElement("ListCondition");
                    if (mapping.ListFilterList != null)
                    {
                        foreach (ColumnFilter listFilter in mapping.ListFilterList)
                        {
                            InitConditionRule(listCondition, listFilter.Conditions);   
                        }
                    }
                    condition.AppendChild(listCondition);
                    XmlElement itemCondition = doc.CreateElement("ItemCondition");
                    if (mapping.ItemFilterList != null)
                    {
                        foreach (ColumnFilter itemFilter in mapping.ItemFilterList)
                        {
                            InitConditionRule(itemCondition, itemFilter.Conditions);    
                        }
                    }
                    condition.AppendChild(itemCondition);
                    XmlElement mappings = doc.CreateElement("Mappings");
                    if (mapping.ColumnMappingList != null)
                    { 
                        foreach (ColumnMappingValue mappingValue in mapping.ColumnMappingList)
                        {
                            InitMapping(mappings, mappingValue);
                        }  
                    }
                    fieldMapping.AppendChild(mappings);
                }
            }
            return doc;
        }

        private static void InitConditionRule(XmlElement element, List<ConditionItem> conditions)
        {
            foreach (ConditionItem condition in conditions)
            {
                XmlElement conditionRule = element.OwnerDocument.CreateElement("ConditionRule");
                conditionRule.SetAttribute("type", condition.MetaDataType.ToString());
                conditionRule.SetAttribute("condition", condition.ConditionType.ToString());
                conditionRule.SetAttribute("value", condition.Value);
                conditionRule.SetAttribute("relation", condition.AndOr.ToString());

                element.AppendChild(conditionRule);
            }
        }
        private static void InitMapping(XmlElement element, ColumnMappingValue mappingValue)
        {
            XmlElement mapping = element.OwnerDocument.CreateElement("Mapping");
            mapping.SetAttribute("type", mappingValue.Type.ToString());
            mapping.SetAttribute("sourceName", mappingValue.SourceInternalName);
            mapping.SetAttribute("sourceDisplayName", mappingValue.SourceColumnName);
            mapping.SetAttribute("destinationName", mappingValue.DesInternalName);
            mapping.SetAttribute("destinationDisplayName", mappingValue.DesColumnName);
            XmlElement setting = element.OwnerDocument.CreateElement("Setting");
            switch (mappingValue.Type)
            {
                case ColumnType.ChangeToMetadata:
                    if (mappingValue.metadataSetting != null)
                    {
                        setting.SetAttribute("termSetPath", mappingValue.metadataSetting.TermSetPath);
                        setting.SetAttribute("allowMultiValue", mappingValue.metadataSetting.IsAllowMultiterm.ToString());
                        if (mappingValue.metadataSetting.IsMigrateString)
                        {
                            setting.SetAttribute("separateChar", mappingValue.metadataSetting.MigrateBy);
                        }
                        else
                        {
                            setting.SetAttribute("separateChar", string.Empty);
                        }
                    }
                    break;
                case ColumnType.ChangeToLookUp:
                    if (mappingValue.LookUpSetting != null)
                    {
                        setting.SetAttribute("listTitle", mappingValue.LookUpSetting.ListTitle);
                        setting.SetAttribute("columnName", mappingValue.LookUpSetting.ColumnName);
                        setting.SetAttribute("allowMultiValue", mappingValue.LookUpSetting.IsAllowMultiterm.ToString());
                        if (mappingValue.LookUpSetting.IsMigrateString)
                        {
                            setting.SetAttribute("separateChar", mappingValue.LookUpSetting.MigrateBy);
                        }
                        else
                        {
                            setting.SetAttribute("separateChar", ";");
                        }
                    }
                    break;
            }
            mapping.AppendChild(setting);
            XmlElement valueMappings = element.OwnerDocument.CreateElement("ValueMappings");
            if (mappingValue.ValueList != null)
            {
                foreach (ValueMapping value in mappingValue.ValueList)
                {
                    InitMappingValue(valueMappings, value);
                }   
            }
            mapping.AppendChild(valueMappings);
            element.AppendChild(mapping);
        }

        private static void InitMappingValue(XmlElement element, ValueMapping value)
        {
            XmlElement valueMapping = element.OwnerDocument.CreateElement("ValueMapping");
            valueMapping.SetAttribute("sourceValue", value.SourceValue);
            valueMapping.SetAttribute("destinationValue", value.DesValue);
            element.AppendChild(valueMapping);
        }
    }
}
