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
using System.Collections;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.Wrapper.Resource;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Mapping;
using AvePoint.GCommon.Utility.Cryptography;

namespace AvePoint.Wrapper.Restore
{
    public class AveFieldHelper
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveFieldHelper));

        /// <summary>
        /// Field Type Mapping
        /// </summary>
        private static Hashtable s_dictType;

        #region Find site field
        /// <summary>
        /// Get field by id in web
        /// </summary>
        /// <param name="fieldId"></param>
        /// <param name="web"></param>
        /// <returns></returns>
        internal static IAveField GetSiteField(Guid fieldId, IAveWeb web, ref bool needSkip)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetSiteField"))
            {
#endif
                IAveField field = null;
                field = FindFieldInCollection(fieldId, web.Fields);
                if (field == null)
                {
                    field = FindFieldInCollection(fieldId, web.AvailableFields);
                    if (field != null)
                    {
                        needSkip = true;
                    }
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        internal static bool GetSiteFieldInChildren(string scope, Guid siteId, Guid fieldId, IAveWeb web)
        {
            IAveFieldCollection fieldCollection = web.Fields;
            return fieldCollection.GetFieldInSiteChildren(scope, siteId, fieldId);
        }


        /// <summary>
        /// Get field by internal name in web
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="web"></param>
        /// <returns></returns>
        internal static IAveField GetSiteFieldByInternalName(string fieldName, IAveWeb web)
        {
            return GetSiteField(fieldName, false, web);
        }

        internal static IAveField GetSiteFieldByInternalName(string fieldName, AveFieldType fieldType, IAveWeb web, ref bool needSkip)
        {
            return GetSiteField(fieldName, false, fieldType, web, ref needSkip);
        }

        internal static IAveField GetSiteField(string fieldName, IAveWeb web)
        {
            return GetSiteField(fieldName, true, web);
        }

        internal static IAveField GetSiteField(string fieldName, AveFieldType fieldType, IAveWeb web, ref bool needSkip)
        {
            return GetSiteField(fieldName, true, fieldType, web, ref needSkip);
        }

        private static IAveField GetSiteField(string fieldName, bool isDisplayName, IAveWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetSiteField"))
            {
#endif
                IAveField field = null;
                field = FindFieldInCollection(fieldName, isDisplayName, web.Fields);
                if (field == null)
                {
                    field = FindFieldInCollection(fieldName, isDisplayName, web.AvailableFields);
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        private static IAveField GetSiteField(string fieldName, bool isDisplayName, AveFieldType fieldType, IAveWeb web, ref bool needSkip)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetSiteField"))
            {
#endif
                IAveField field = null;
                field = FindFieldInCollection(fieldName, fieldType, isDisplayName, web.Fields);
                if (field == null)
                {
                    field = FindFieldInCollection(fieldName, fieldType, isDisplayName, web.AvailableFields);
                    if (field != null)
                    {
                        needSkip = true;
                    }
                }
                return field;
#if PerformanceLog
            }
#endif
        }
        /// <summary>
        /// Get field by static name in web
        /// </summary>
        /// <param name="fieldName"></param>
        /// <param name="web"></param>
        /// <returns></returns>
        internal static IAveField GetSiteFieldByStaticName(string fieldName, IAveWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetSiteFieldByStaticName"))
            {
#endif
                IAveField field = null;
                field = FindFieldInCollection(fieldName, true, web.Fields);
                if (field == null)
                {
                    field = FindFieldInCollection(fieldName, web.AvailableFields);
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        internal static IAveField GetSiteFieldByStaticName(string fieldName, AveFieldType fieldType, IAveWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetSiteFieldByStaticName"))
            {
#endif
                IAveField field = null;
                field = FindFieldInCollection(fieldName, fieldType, false, web.Fields);
                if (field == null)
                {
                    field = FindFieldInCollection(fieldName, fieldType, false, web.AvailableFields);
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        #endregion

        internal static bool IsLookupField(IAveField field)
        {
            if (field.Type != AveFieldType.Lookup)
            {
                return (field is IAveFieldLookup);
            }
            return true;
        }

        internal static bool IsLookupField(XmlNode fieldNode)
        {
            string namedStringItem = GetNamedStringItem(fieldNode, "Type");
            if (string.IsNullOrEmpty(namedStringItem))
            {
                return false;
            }
            return IsLookupField(GetFieldType(namedStringItem), fieldNode);
        }

        internal static bool IsLookupField(AveFieldType fieldType, XmlNode fieldNode)
        {
            if (fieldType != AveFieldType.Lookup)
            {
                return !string.IsNullOrEmpty(GetNamedStringItem(fieldNode, "List"));
            }
            return true;
        }

        /// <summary>
        /// Convert string to AveFieldType
        /// </summary>
        /// <param name="strType"></param>
        /// <returns></returns>
        internal static AveFieldType GetFieldType(string strType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetFieldType"))
            {
#endif
                if (s_dictType == null)
                {
                    Hashtable hashtable = new Hashtable();
                    foreach (AveFieldType type in Enum.GetValues(typeof(AveFieldType)))
                    {
                        hashtable[Enum.GetName(typeof(AveFieldType), type)] = type;
                    }
                    hashtable["LookupMulti"] = hashtable["Lookup"];
                    hashtable["UserMulti"] = hashtable["User"];
                    Interlocked.CompareExchange<Hashtable>(ref s_dictType, hashtable, null);
                }
                object obj2 = s_dictType[strType];
                if (obj2 != null)
                {
                    return (AveFieldType)obj2;
                }
                return AveFieldType.Invalid;
#if PerformanceLog
            }
#endif
        }

        internal static string GetNamedStringItem(XmlNode node, string strName)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetNamedStringItem"))
            {
#endif
                XmlNode namedItem = node.Attributes.GetNamedItem(strName);
                if (namedItem == null)
                {
                    return null;
                }
                return namedItem.Value;
#if PerformanceLog
            }
#endif
        }

        #region Find field in collection
        internal static IAveField FindSiteFieldBySchema(Guid fieldId, IAveWeb web, Dictionary<Guid, Guid> mappings, List<Dictionary<Guid, Guid>> availableMappings)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.FindSiteFieldBySchema"))
            {
#endif
                IAveField field = null;
                Guid tmpFieldId = GetMappingIdFromSchema(fieldId, mappings);
                if (Guid.Empty != tmpFieldId)
                {
                    field = FindFieldInCollection(tmpFieldId, web.Fields);
                }
                if (null == field)
                {
                    tmpFieldId = GetMappingIdFromAvailableSchema(fieldId, availableMappings);
                    if (Guid.Empty != tmpFieldId)
                    {
                        field = FindFieldInCollection(tmpFieldId, web.AvailableFields);
                    }
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        internal static IAveField FindListFieldBySchema(Guid fieldId, AveFieldType fieldType, IAveList list, Dictionary<Guid, Guid> mappings)
        {

#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.FindListFieldBySchema"))
            {
#endif
                IAveField field = null;
                fieldId = GetMappingIdFromSchema(fieldId, mappings);
                if (Guid.Empty != fieldId)
                {
                    field = FindFieldInCollection(fieldId, list.Fields);
                }
                if (field != null && IsFieldTypesCompatible(fieldType, field.Type))
                {
                    return field;
                }
                else
                {
                    return null;
                }
#if PerformanceLog
            }
#endif
        }

        internal static IAveField FindFieldInCollection(Guid fieldId, IAveFieldCollection collection)
        {
            IAveField field = null;
            try
            {
                if (!string.Equals(Guid.Empty, fieldId))
                {
                    field = collection[fieldId];
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldInCollectionError, e.ToString());
            }
            return field;
        }

        internal static IAveField FindFieldInCollection(string name, IAveFieldCollection collection)
        {
            return FindFieldInCollection(name, false, collection);
        }

        internal static IAveField FindFieldInCollection(string name, bool isDisplayName, IAveFieldCollection collection)
        {
            return FindFieldInCollection(name, AveFieldType.Invalid, isDisplayName, collection, false);
        }

        internal static IAveField FindFieldInCollection(string name, AveFieldType fieldType, IAveFieldCollection collection)
        {
            return FindFieldInCollection(name, fieldType, false, collection);
        }

        internal static IAveField FindFieldInCollection(string name, AveFieldType fieldType, bool isDisplayName, IAveFieldCollection collection)
        {
            return FindFieldInCollection(name, fieldType, isDisplayName, collection, true);
        }

        internal static IAveField FindFieldInCollection(string name, AveFieldType fieldType, IAveFieldCollection collection, bool needCompareType)
        {
            return FindFieldInCollection(name, fieldType, false, collection, needCompareType);
        }

        internal static IAveField FindFieldInCollection(string name, AveFieldType fieldType, bool isDisplayName, IAveFieldCollection collection, bool needCompareType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.FindFieldInCollection"))
            {
#endif
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("Argument 'name' cannot be null.");
                }
                IAveField field = null;
                try
                {
                    field = isDisplayName ? collection[name] : collection.GetFieldByInternalName(name);
                    if (null != field)
                    {
                        if (needCompareType && !IsFieldTypesCompatible(fieldType, field.Type))
                        {
                            log.Warn("Destination has the column with same name,but different type,column name:{0}.Source field type is {1}, Destination field type is {2}", name, fieldType, field.Type);
                            field = null;
                        }
                    }
                }
                catch (ArgumentException)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldFailed, name);
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldFailed, name, e);
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        internal static IAveField FindFieldInCollectionByStaticName(string name, IAveFieldCollection collection)
        {
            return FindFieldInCollectionByStaticName(name, AveFieldType.Invalid, collection, false);
        }

        internal static IAveField FindFieldInCollectionByStaticName(string name, AveFieldType fieldType, IAveFieldCollection collection)
        {
            return FindFieldInCollectionByStaticName(name, fieldType, collection, true);
        }

        internal static IAveField FindFieldInCollectionByStaticName(string name, AveFieldType fieldType, IAveFieldCollection collection, bool needCompareType)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.FindFieldInCollectionByStaticName"))
            {
#endif
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("Argument 'name' cannot be null.");
                }
                IAveField field = null;
                try
                {
                    field = collection.TryGetFieldByStaticName(name);
                    if (null != field)
                    {
                        if (needCompareType && !IsFieldTypesCompatible(fieldType, field.Type))
                        {
                            log.Warn("Destination has the column with same static name,but different type ,column name is:{0}.Source field type is {1}, Destination field type is {2}", name, fieldType, field.Type);
                            field = null;
                        }
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldFailed, name, e);
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        internal static IAveField FindFieldInCollectionByCustomMapping(string name, AveFieldType srcFieldType, string customTypeString, IAveFieldCollection collection)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.FindFieldInCollectionByCustomMapping"))
            {
#endif
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("Argument 'name' cannot be null.");
                }
                IAveField field = null;
                try
                {
                    field = collection[name];
                    if (null != field)
                    {
                        if (!String.IsNullOrEmpty(customTypeString))
                        {
                            if (!field.TypeAsString.Equals(customTypeString, StringComparison.OrdinalIgnoreCase))
                            {
                                field = null;
                            }
                        }
                        //else
                        //{
                        //    if (!IsFieldTypesCompatible(srcFieldType, field.Type))
                        //    {
                        //        field = null;
                        //    }
                        //}
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldFailed, name, e);
                }
                return field;
#if PerformanceLog
            }
#endif
        }

        internal static IAveField FindFieldInCollectionByCustomMapping(AveCustomFieldInfo customFieldInfo, IAveFieldCollection collection, AveFieldType srcFieldType)
        {
            IAveField field = null;
            if (!string.IsNullOrEmpty(customFieldInfo.InternalName) && customFieldInfo.UseInternalOrDisplay)
            {
                try
                {
                    field = collection.GetFieldByInternalName(customFieldInfo.InternalName);
                    if (field != null)
                    {
                        log.Debug("Find field title {0}, type: ,{1} by internal name {2} ", field.Title, field.Type, customFieldInfo.InternalName);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.INFO, WrapperRestoreResource.GetFieldByInternalNameError, e.ToString());
                    //no need catch this exception
                }
            }
            else if (!string.IsNullOrEmpty(customFieldInfo.Name) && !customFieldInfo.UseInternalOrDisplay)
            {

                try
                {
                    field = collection[customFieldInfo.Name];
                    if (field != null)
                    {
                        log.Info("Find field title {0}, type: ,{1} by display name {2} ", field.Title, field.Type, customFieldInfo.Name);
                    }
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFileByNameError, e.ToString());
                    //no need catch this exception
                }
            }
            if (field != null)
            {
                switch ((AveCustomFieldType)Enum.Parse(typeof(AveCustomFieldType), customFieldInfo.CustomFieldTypeAsString))
                {
                    case AveCustomFieldType.SameType:
                        //对sametype的判断 当源端目的端为choice或multichoice时 不需创建新field逻辑
                        if (!IsFieldTypesCompatible(srcFieldType, field.Type))
                        {
                            field = null;
                        }
                        break;
                    case AveCustomFieldType.ChangeToDes:
                        //增加判断当源端目的端类型不兼容时创建新的field,类型兼容时不创建新的field
                        /*if (CheckFieldTypeConflict(srcFieldType, field.Type))
                        {
                            field = null;
                        }*/
                        break;
                    case AveCustomFieldType.ChangeToMetadata:
                        if (!(field.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase)))
                        {
                            log.Debug("Change to MMS column mapping find incompatible column, create new column instead");
                            field = null;
                        }
                        break;
                    case AveCustomFieldType.ChangeToLookUp:
                        if (!(field.Type == AveFieldType.Lookup))
                        {
                            log.Debug("Change to lookup column mapping find incompatible column, create new column instead");
                            field = null;
                        }
                        break;
                }
            }
            return field;
        }

        internal static bool IsFieldTypesCompatible(AveFieldType sType, AveFieldType dType)
        {
            if (sType.Equals(dType)
                || (sType.Equals(AveFieldType.Choice) && dType.Equals(AveFieldType.MultiChoice))
                || (sType.Equals(AveFieldType.MultiChoice) && (dType.Equals(AveFieldType.Choice)))
                || (sType.Equals(AveFieldType.Text) && dType.Equals(AveFieldType.Note))
                || (sType.Equals(AveFieldType.Note) && dType.Equals(AveFieldType.Text)))
            {
                return true;
            }
            return false;
            //List<AveFieldType> singleTextTypeCol = new List<AveFieldType>() { AveFieldType.Text, AveFieldType.Note, AveFieldType.Choice, AveFieldType.Number, AveFieldType.Currency, AveFieldType.DateTime };
            //List<AveFieldType> numTypeCol = new List<AveFieldType>() { AveFieldType.Text, AveFieldType.Note, AveFieldType.Choice, AveFieldType.Number, AveFieldType.Currency, AveFieldType.Boolean };

            //List<AveFieldType> dateAndTimeTypeCol = new List<AveFieldType>  { AveFieldType.Text,AveFieldType.Note,AveFieldType.Choice,AveFieldType.Number,AveFieldType.Currency,AveFieldType.DateTime,AveFieldType.Lookup,
            //                                                             AveFieldType.Boolean,AveFieldType.User,AveFieldType.URL,AveFieldType.Calculated};

            //switch (sType)
            //{
            //    case AveFieldType.Text:
            //    case AveFieldType.Choice:
            //        return singleTextTypeCol.Contains(dType);

            //    case AveFieldType.Number:
            //    case AveFieldType.Currency:
            //    case AveFieldType.Boolean:
            //        return numTypeCol.Contains(dType);

            //    case AveFieldType.DateTime:
            //    case AveFieldType.Lookup:
            //    case AveFieldType.User:
            //        return dateAndTimeTypeCol.Contains(dType);

            //    case AveFieldType.URL:
            //    case AveFieldType.Calculated:
            //        return false;

            //    default:
            //        return false;
            //}
        }

        #endregion

        internal static bool IsGuid(string strToValidate)
        {
            bool isGuid = false;
            string strRegexPatten = @"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\"
                    + @"-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$";
            if (strToValidate != null && !strToValidate.Equals(""))
            {
                isGuid = System.Text.RegularExpressions.Regex.IsMatch(strToValidate, strRegexPatten);
            }
            return isGuid;
        }

        #region Update Field
        internal static bool UpdateFieldType(IAveField spField, AveXmlField xmlField)
        {
            try
            {
                if (spField.Type != xmlField.Type)
                {
                    if (UpdateSpecialFieldType(spField, xmlField))
                    {
                        return true;
                    }
                    spField.Type = xmlField.Type;
                    spField.Update();
                    return true;
                }
            }
            catch (Exception e)
            {
                log.Warn("The Source Field:{0}, Id:{1}, Type:{2}, SourceType:{3}, Exception:{4}", spField.Title, spField.ID, spField.Type, xmlField.Type, e.ToString());
            }
            return false;
        }
        internal static bool UpdateSpecialFieldType(IAveField spField, AveXmlField xmlField)
        {
            try
            {
                if (spField.Type == AveFieldType.DateTime && xmlField.Type == AveFieldType.Choice)
                {
                    if (!xmlField.XmlElement.GetAttribute("ID").Equals(spField.ID.ToString()))
                    {
                        xmlField.XmlElement.SetAttribute("ID", spField.ID.ToString());
                    }
                    spField.SchemaXml = xmlField.XmlElement.OuterXml;
                    spField.Update();
                    return true;
                }
            }
            catch (Exception ex)
            {
                log.Warn("The Source Field:{0}, Id:{1}, Type:{2}, SourceType:{3}, Exception:{4}", spField.Title, spField.ID, spField.Type, xmlField.Type, ex.ToString());
            }
            return false;
        }
        internal static void UpdateNoCrawl(IAveField spField, AveXmlField xmlField)
        {
            try
            {
                if (spField.NoCrawl != xmlField.NoCrawl)
                {
                    spField.NoCrawl = xmlField.NoCrawl;
                    spField.Update();
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while update field's NoCrawl. field title:{0}, field id:{1}\n error message:{2}", spField.Title, spField.ID, e));
            }
        }

        internal static void UpdateValidationInfo(IAveField field, string validationMessage, string validationFormula)
        {
            try
            {
                if (!field.ReadOnlyField && CanValidate(field.Type))
                {
                    field.ValidationMessage = validationMessage;
                    field.ValidationFormula = validationFormula.Replace(field.InternalName, "[" + field.Title + "]");

                    field.Update();
                }
            }
            catch (Exception ex)
            {
                log.Warn("Error happened while Update Validation Info. Field: " + field.Title + ", Message: " + validationMessage + ", Formula: " + validationFormula + ". Error: " + ex.ToString());
            }
        }

        internal static bool CanValidate(AveFieldType type)
        {
            switch (type)
            {
                case AveFieldType.Integer:
                case AveFieldType.Text:
                case AveFieldType.DateTime:
                case AveFieldType.Choice:
                case AveFieldType.Number:
                case AveFieldType.Currency:
                    return true;
            }
            return false;
        }

        internal static bool CompareFieldType(IAveField spField, AveXmlField xmlField)
        {
            return spField.Type == xmlField.Type;
        }

        internal static bool CompareNoCrawl(IAveField spField, AveXmlField xmlField)
        {
            return spField.NoCrawl == xmlField.NoCrawl;
        }

        #endregion

        #region Rename Field
        internal static string GetNewInternalName(string name, IAveFieldCollection fields)
        {
            int extentNum = 1;
            IAveField tField = null;
            IAveFieldCollection tFields = fields;
            string fieldName = name;
            do
            {
                try
                {
                    tField = tFields.GetFieldByInternalName(fieldName);
                }
                catch (ArgumentException)
                {
                    break;
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldByInternalNameError, e.ToString());
                    break;
                }
                fieldName = name + extentNum;
            }
            while (extentNum++ < 500);
            return fieldName;
        }
        internal static string GetNewDisplayName(string name, IAveFieldCollection fields)
        {
            int extentNum = 1;
            IAveField tField = null;
            IAveFieldCollection tFields = fields;
            string fieldDisplayName = name;
            do
            {
                try
                {
                    tField = tFields[fieldDisplayName];
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldByInternalNameError, e.ToString());
                    break;
                }
                fieldDisplayName = name + "_" + extentNum;
            }
            while (extentNum++ < 500);
            return fieldDisplayName;
        }
        #endregion

        internal static Dictionary<Guid, Guid> GetFieldMapping(Hashtable properties)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetFieldMapping"))
            {
#endif
                Dictionary<Guid, Guid> mappings = new Dictionary<Guid, Guid>();
                if (null != properties && properties.Contains("Doc_Field_Mappings"))
                {
                    try
                    {
                        string mappingXml = (string)properties["Doc_Field_Mappings"];
                        mappings = ConvertXmlToFieldIDMapping(mappingXml);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ConvertXmlToFieldIDMappingError, e.ToString());
                    }
                }
                return mappings;
#if PerformanceLog
            }
#endif
        }

        private static Dictionary<Guid, Guid> ConvertXmlToFieldIDMapping(string xml)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.ConvertXmlToFieldIDMapping"))
            {
#endif
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);
                if (!doc.DocumentElement.Name.Equals("AvePointFieldMappings", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AveException(string.Format("The mapping xml is not valid. \r\nXml={0}", doc.OuterXml));
                }

                Dictionary<Guid, Guid> mapping = new Dictionary<Guid, Guid>();
                foreach (XmlElement ele in doc.DocumentElement.ChildNodes)
                {
                    Guid sourceId = new Guid(ele.GetAttribute("SourceID"));
                    Guid destId = new Guid(ele.GetAttribute("CTID"));
                    mapping.Add(sourceId, destId);
                }
                return mapping;
#if PerformanceLog
            }
#endif
        }

        internal static string ConvertFieldIDMappingToXml(Dictionary<Guid, Guid> mappings)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.ConvertFieldIDMappingToXml"))
            {
#endif
                XmlDocument doc = new XmlDocument();
                doc.AppendChild(doc.CreateElement("AvePointFieldMappings"));
                foreach (KeyValuePair<Guid, Guid> mapping in mappings)
                {
                    XmlElement mappingElement = doc.CreateElement("Mapping");
                    mappingElement.SetAttribute("SourceID", mapping.Key.ToString());
                    mappingElement.SetAttribute("CTID", mapping.Value.ToString());
                    doc.DocumentElement.AppendChild(mappingElement);
                }
                return doc.OuterXml;
#if PerformanceLog
            }
#endif
        }

        internal static List<Dictionary<Guid, Guid>> GetAvaliableFieldIdMappings(IAveWeb web)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetAvaliableFieldIdMappings"))
            {
#endif
                List<Dictionary<Guid, Guid>> avaliableMappings = new List<Dictionary<Guid, Guid>>();
                if (null == web)
                {
                    return new List<Dictionary<Guid, Guid>>();
                }
                avaliableMappings.Add(GetFieldMapping(web.AllProperties));
                IAveWeb parent = web.ParentWeb;

                while (null != parent && parent.Exists)
                {
                    Dictionary<Guid, Guid> mappings = GetFieldMapping(parent.AllProperties);
                    if (mappings.Count > 0)
                    {
                        avaliableMappings.Add(mappings);
                    }
                    parent = parent.ParentWeb;
                }
                return avaliableMappings;
#if PerformanceLog
            }
#endif
        }

        internal static void UpdateFieldSchemaIdMappingProperty(IAveList list, Dictionary<Guid, Guid> mappings)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.UpdateFieldSchemaIdMappingProperty"))
            {
#endif
                if (null == mappings || mappings.Count == 0 || list.RootFolder.Properties == null)
                {
                    return;
                }
                string mappingStr = ConvertFieldIDMappingToXml(mappings);
                if (!list.RootFolder.Properties.Contains("Doc_Field_Mappings"))
                {
                    list.RootFolder.Properties.Add("Doc_Field_Mappings", mappingStr);
                }
                else
                {
                    list.RootFolder.Properties["Doc_Field_Mappings"] = mappingStr;
                }
                list.RootFolder.Update();
#if PerformanceLog
            }
#endif
        }

        internal static void UpdateFieldSchemaIdMappingProperty(IAveWeb web, Dictionary<Guid, Guid> mappings)
        {
            if (null == mappings || mappings.Count == 0)
            {
                return;
            }
            string mappingStr = ConvertFieldIDMappingToXml(mappings);
            if (!web.AllProperties.Contains("Doc_Field_Mappings"))
            {
                web.AllProperties.Add("Doc_Field_Mappings", mappingStr);
            }
            else
            {
                web.AllProperties["Doc_Field_Mappings"] = mappingStr;
            }
            web.Update();
        }

        internal static Guid GetMappingIdFromSchema(Guid sourceId, Dictionary<Guid, Guid> mappings)
        {
            if (null == mappings)
            {
                throw new Exception("Schema mapping is null.");
            }
            return mappings.ContainsKey(sourceId) ? mappings[sourceId] : Guid.Empty;
        }

        internal static Guid GetMappingIdFromAvailableSchema(Guid id, List<Dictionary<Guid, Guid>> availableMappings)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetMappingIdFromAvailableSchema"))
            {
#endif
                Guid result = Guid.Empty;
                if (null == availableMappings)
                {
                    throw new Exception("Available schema mapping is null.");
                }
                for (int index = availableMappings.Count; index > 0; )
                {
                    result = GetMappingIdFromSchema(id, availableMappings[--index]);
                    if (!Guid.Empty.Equals(result))
                    {
                        break;
                    }
                }
                return result;
#if PerformanceLog
            }
#endif
        }

        internal static string GetFieldDefaultValues(IAveField field)
        {
            string defaultValue = null;
            if (field is IAveTaxonomyField)
            {
                //TaxonomyField的DefaultValue格式为     :wssId1;#lable1|guid1[;#wssId2;#lable2|guid2]...
                //TaxonomyField的DefaultValueTyped格式为:lable1|guid1[;lable1|guid1]...
                //Wrapper目前无法解析带有wssId的Term值.
                defaultValue = field.DefaultValueTyped.ToString();
            }
            else if ((field is IAveFieldDateTime) && field.DefaultValue.Equals("[today]"))
            {
                defaultValue = DateTime.Now.ToString();
            }
            else if (string.IsNullOrEmpty(defaultValue))
            {
                defaultValue = field.DefaultValue;
            }
            return defaultValue;
        }

        #region MD5 Property
        public static string GetMD5FromSchemaXml(IAveField field)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.GetMD5FromSchemaXml"))
            {
#endif
                string md5 = string.Empty;
                if (field != null)
                {
                    try
                    {

                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(field.SchemaXml);
                        if (doc.DocumentElement.HasAttribute("AveMD5Property"))
                        {
                            md5 = doc.DocumentElement.GetAttribute("AveMD5Property");
                        }
                    }
                    catch (Exception ex)
                    {
                        log.Error("GetMD5FromSchemaXml Error.Exception:" + ex.ToString());
                    }
                }
                return md5;
#if PerformanceLog
            }
#endif
        }
        public static void UpdateMD5ToSchemaXml(IAveField field)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.UpdateMD5ToSchemaXml"))
            {
#endif
                try
                {
                    if (field != null)
                    {
                        XmlDocument doc = new XmlDocument();
                        doc.LoadXml(field.SchemaXml);
                        doc.DocumentElement.SetAttribute("AveMD5Property", GetCurrentMD5Property(field));
                        field.SchemaXml = doc.OuterXml;
                        field.Update();
                    }
                }
                catch (Exception ex)
                {
                    log.Error("UpdateMD5ToSchemaXml Error.Exception:" + ex.ToString());
                }
#if PerformanceLog
            }
#endif
        }
        public static string GetCurrentMD5Property(IAveField field)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.BuildMD5Property"))
            {
#endif
                if (String.IsNullOrEmpty(field.MD5))
                {
                    StringBuilder builder = new StringBuilder();
                    builder.Append(field.Title);
                    builder.Append(";" + BaseFieldMD5Property(field));
                    switch (field.Type)
                    {
                        case AveFieldType.Lookup:
                            IAveFieldLookup lookupField = field as IAveFieldLookup;
                            if (lookupField != null)
                            {
                                builder.Append(";" + LookupFieldMD5Property(lookupField));
                            }
                            break;
                        case AveFieldType.User:
                            IAveFieldUser userField = field as IAveFieldUser;
                            if (userField != null)
                            {
                                builder.Append(";" + UserFieldMD5Property(userField));
                            }
                            break;
                        case AveFieldType.DateTime:
                            IAveFieldDateTime timeField = field as IAveFieldDateTime;
                            if (timeField != null)
                            {
                                builder.Append(";" + DateTimeFieldMD5Property(timeField));
                            }
                            break;
                        case AveFieldType.Boolean:
                        //case AveFieldType.WhatsNew:
                        //case AveFieldType.Confidential:
                        case AveFieldType.AllDayEvent:
                            //case AveFieldType.AllowEditing:
                            IAveFieldBoolean boolField = field as IAveFieldBoolean;
                            if (boolField != null)
                            {
                                builder.Append(";" + BoolFieldMD5Property(boolField));
                            }
                            break;
                        case AveFieldType.Choice:
                        //case AveFieldType.ContactInfo:
                        //case AveFieldType.Whereabout:
                        case AveFieldType.WorkflowStatus:
                            IAveFieldChoice choiceField = field as IAveFieldChoice;
                            if (choiceField != null)
                            {
                                builder.Append(";" + ChoiceFieldMD5Property(choiceField));
                            }
                            break;
                        case AveFieldType.MultiChoice:
                            IAveFieldMultiChoice multiChocieField = field as IAveFieldMultiChoice;
                            if (multiChocieField != null)
                            {
                                builder.Append(";" + MultiChocieFieldMD5Property(multiChocieField));
                            }
                            break;
                        case AveFieldType.Calculated:
                            IAveFieldCalculated calField = field as IAveFieldCalculated;
                            if (calField != null)
                            {
                                builder.Append(";" + CalculatedFieldMD5Property(calField));
                            }
                            break;
                        case AveFieldType.Computed:
                            IAveFieldComputed computedField = field as IAveFieldComputed;
                            if (computedField != null)
                            {
                                builder.Append(";" + ComputedFieldMD5Property(computedField));
                            }
                            break;
                        case AveFieldType.Currency:
                            IAveFieldCurrency currencyField = field as IAveFieldCurrency;
                            if (currencyField != null)
                            {
                                builder.Append(";" + CurrencyFieldMD5Property(currencyField));
                            }
                            break;
                        case AveFieldType.Number:
                        case AveFieldType.Integer:
                        case AveFieldType.WorkflowEventType:
                            IAveFieldNumber numberField = field as IAveFieldNumber;
                            if (numberField != null)
                            {
                                builder.Append(";" + NumberFieldMD5Property(numberField));
                            }
                            break;
                        case AveFieldType.Note:
                            IAveFieldMultiLineText mulTextField = field as IAveFieldMultiLineText;
                            if (mulTextField != null)
                            {
                                builder.Append(";" + NoteFieldMD5Property(mulTextField));
                            }
                            break;
                        case AveFieldType.GridChoice:
                            IAveFieldRatingScale gridField = field as IAveFieldRatingScale;
                            if (gridField != null)
                            {
                                builder.Append(";" + GridFieldMD5Property(gridField));
                            }
                            break;
                        case AveFieldType.Text:
                            //case AveFieldType.Confirmations:
                            IAveFieldText textField = field as IAveFieldText;
                            if (textField != null)
                            {
                                builder.Append(";" + TextFieldMD5Property(textField));
                            }
                            break;
                        case AveFieldType.URL:
                            IAveFieldUrl urlField = field as IAveFieldUrl;
                            if (urlField != null)
                            {
                                builder.Append(";" + UrlFieldMD5Property(urlField));
                            }
                            break;
                        case AveFieldType.Invalid:
                            if (field.TypeAsString == "TaxonomyFieldType" || field.TypeAsString == "TaxonomyFieldTypeMulti")
                            {
                                builder.Append(";" + TaxonomyFieldMD5Property(field));
                            }
                            else
                            {
                                builder.Append(";" + InvalidFieldMD5Property(field));
                            }
                            break;
                        default:
                            break;
                    }
                    field.MD5 = SHA1Hash(builder.ToString());
                }
                return field.MD5;
#if PerformanceLog
            }
#endif
        }
        private static string BaseFieldMD5Property(IAveField field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.Type.ToString());
            builder.Append(";" + field.NoCrawl.ToString());
            builder.Append(";" + field.AggregationFunction);
            if (field.AllowDeletion.HasValue)
            {
                builder.Append(";" + field.AllowDeletion.Value.ToString());
            }
            builder.Append(";" + field.DefaultFormula);
            builder.Append(";" + field.DefaultValue);
            builder.Append(";" + field.Direction);
            builder.Append(";" + field.DisplaySize);
            builder.Append(";" + field.TypeAsString);
            builder.Append(";" + field.ValidationFormula);
            builder.Append(";" + field.ValidationMessage);
            builder.Append(";" + field.IMEMode);
            builder.Append(";" + field.Hidden.ToString());
            builder.Append(";" + field.Group);
            builder.Append(";" + field.JumpToField);
            builder.Append(";" + field.LinkToItem.ToString());
            builder.Append(";" + field.NoCrawl.ToString());
            builder.Append(";" + field.PIAttribute);
            builder.Append(";" + field.PITarget);
            builder.Append(";" + field.PrimaryPIAttribute);
            builder.Append(";" + field.PrimaryPITarget);
            builder.Append(";" + field.ReadOnlyField.ToString());
            builder.Append(";" + field.RelatedField);
            builder.Append(";" + field.Required.ToString());
            builder.Append(";" + field.Sealed.ToString());
            builder.Append(";" + field.ShowInDisplayForm.ToString());
            builder.Append(";" + field.ShowInEditForm.ToString());
            builder.Append(";" + field.ShowInListSettings.ToString());
            builder.Append(";" + field.ShowInNewForm.ToString());
            builder.Append(";" + field.ShowInVersionHistory.ToString());
            builder.Append(";" + field.ShowInViewForms.ToString());
            builder.Append(";" + field.StaticName);
            builder.Append(";" + field.TranslationXml);
            builder.Append(";" + field.EnforceUniqueValues.ToString());

            return builder.ToString();
        }
        private static string LookupFieldMD5Property(IAveFieldLookup field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.IsRelationship.ToString());
            builder.Append(";" + field.LookupField);
            builder.Append(";" + field.LookupWebId.ToString());
            builder.Append(";" + field.LookupList);
            builder.Append(";" + field.PrependId.ToString());
            builder.Append(";" + field.PrimaryFieldId);
            builder.Append(";" + field.RelationshipDeleteBehavior.ToString());
            builder.Append(";" + field.UnlimitedLengthInDocumentLibrary.ToString());
            builder.Append(";" + field.AllowMultipleValues.ToString());
            builder.Append(";" + field.CountRelated.ToString());
            return builder.ToString();
        }
        private static string UserFieldMD5Property(IAveFieldUser field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.AllowDisplay.ToString());
            builder.Append(";" + field.Presence.ToString());
            builder.Append(";" + field.SelectionGroup.ToString());
            builder.Append(";" + field.SelectionMode.ToString());
            builder.Append(";" + field.IsRelationship.ToString());
            builder.Append(";" + field.LookupField);
            builder.Append(";" + field.LookupWebId.ToString());
            builder.Append(";" + field.LookupList);
            builder.Append(";" + field.PrependId.ToString());
            builder.Append(";" + field.PrimaryFieldId);
            builder.Append(";" + field.RelationshipDeleteBehavior.ToString());
            builder.Append(";" + field.UnlimitedLengthInDocumentLibrary.ToString());
            builder.Append(";" + field.AllowMultipleValues.ToString());
            builder.Append(";" + field.CountRelated.ToString());
            return builder.ToString();
        }
        private static string DateTimeFieldMD5Property(IAveFieldDateTime field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.CalendarType.ToString());
            builder.Append(";" + field.DisplayFormat.ToString());
            return builder.ToString();
        }
        private static string BoolFieldMD5Property(IAveFieldBoolean field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.JumpToNoField);
            builder.Append(";" + field.JumpToYesField);
            return builder.ToString();
        }
        private static string ChoiceFieldMD5Property(IAveFieldChoice field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.EditFormat.ToString());

            builder.Append(";" + MultiChocieFieldMD5Property(field));
            return builder.ToString();
        }
        private static string MultiChocieFieldMD5Property(IAveFieldMultiChoice field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";");
            foreach (string ch in field.Choices)
            {
                builder.Append("," + ch);
            }
            builder.Append(";" + field.FillInChoice.ToString());
            return builder.ToString();
        }
        private static string CalculatedFieldMD5Property(IAveFieldCalculated field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.DateFormat.ToString());
            builder.Append(";" + field.Formula);
            builder.Append(";" + field.OutputType.ToString());
            return builder.ToString();
        }
        private static string ComputedFieldMD5Property(IAveFieldComputed field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.EnableLookup.ToString());
            return builder.ToString();
        }
        private static string CurrencyFieldMD5Property(IAveFieldCurrency field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.CurrencyLocaleId.ToString());
            builder.Append(";" + NumberFieldMD5Property(field));
            return builder.ToString();
        }
        private static string NumberFieldMD5Property(IAveFieldNumber field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.DisplayFormat.ToString());
            builder.Append(";" + field.MaximumValue.ToString());
            builder.Append(";" + field.MinimumValue.ToString());
            builder.Append(";" + field.DefaultValue);
            builder.Append(";" + field.ShowAsPercentage.ToString());
            return builder.ToString();
        }
        private static string NoteFieldMD5Property(IAveFieldMultiLineText field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.AllowHyperlink.ToString());
            builder.Append(";" + field.AppendOnly.ToString());
            builder.Append(";" + field.DifferencingLimit.ToString());
            builder.Append(";" + field.IsolateStyles.ToString());
            builder.Append(";" + field.NumberOfLines.ToString());
            builder.Append(";" + field.RichText.ToString());
            builder.Append(";" + field.RichTextMode.ToString());
            builder.Append(";" + field.UnlimitedLengthInDocumentLibrary.ToString());
            return builder.ToString();
        }
        private static string GridFieldMD5Property(IAveFieldRatingScale field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.GridEndNumber.ToString());
            builder.Append(";" + field.GridNAOptionText);
            builder.Append(";" + field.GridTextRangeAverage);
            builder.Append(";" + field.GridTextRangeHigh);
            builder.Append(";" + field.GridTextRangeLow);
            builder.Append(";" + MultiChocieFieldMD5Property(field));
            return builder.ToString();
        }
        private static string TextFieldMD5Property(IAveFieldText field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.MaxLength.ToString());
            builder.Append(";" + field.DifferencingLimit.ToString());
            return builder.ToString();
        }
        private static string UrlFieldMD5Property(IAveFieldUrl field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.DisplayFormat.ToString());
            return builder.ToString();
        }
        private static string TaxonomyFieldMD5Property(IAveField field)
        {
            IAveTaxonomyField taxField = field as IAveTaxonomyField;
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + taxField.TermSetId.ToString());
            return builder.ToString();
        }
        private static string InvalidFieldMD5Property(IAveField field)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(";" + field.Node.InnerXml);
            return builder.ToString();
        }
        private static string SHA1Hash(string text)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveFieldHelper.SHA1Hash"))
            {
#endif
                if (string.IsNullOrEmpty(text))
                    return string.Empty;
                IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
                byte[] orginaldata = Encoding.Default.GetBytes(text);
                byte[] data = hash.ComputeHash(orginaldata);
                string hashValue = BitConverter.ToString(data);
                hashValue = hashValue.Replace("-", string.Empty);
                return hashValue;
#if PerformanceLog
            }
#endif
        }

        /// <summary>
        /// 类型兼容还有问题，如果源端是Note，目的端是Text.这样就认为不一致，需要进行进一步更改，TODO_LONG
        /// </summary>
        /// <param name="sType"></param>
        /// <param name="dType"></param>
        /// <returns></returns>
        internal static bool CheckFieldTypeConflict(AveFieldType sType, AveFieldType dType)
        {
            if (sType.Equals(dType) || (sType.Equals(AveFieldType.Choice) && dType.Equals(AveFieldType.MultiChoice))
                || (sType.Equals(AveFieldType.MultiChoice) && dType.Equals(AveFieldType.Choice)))
            {
                return false;
            }
            List<AveFieldType> singleTextTypeCol = new List<AveFieldType>() { AveFieldType.Text, AveFieldType.Note, AveFieldType.Choice, AveFieldType.MultiChoice, AveFieldType.Number, AveFieldType.Currency, AveFieldType.DateTime };
            List<AveFieldType> numTypeCol = new List<AveFieldType>() { AveFieldType.Text, AveFieldType.Note, AveFieldType.Choice, AveFieldType.MultiChoice, AveFieldType.Number, AveFieldType.Currency, AveFieldType.Boolean };

            List<AveFieldType> dateAndTimeTypeCol = new List<AveFieldType>  { AveFieldType.Text,AveFieldType.Note,AveFieldType.Choice,AveFieldType.MultiChoice,AveFieldType.Number,AveFieldType.Currency,AveFieldType.DateTime,AveFieldType.Lookup,
                                                                         AveFieldType.Boolean,AveFieldType.User,AveFieldType.URL,AveFieldType.Calculated};

            switch (sType)
            {
                case AveFieldType.Text:
                case AveFieldType.Choice:
                    return !singleTextTypeCol.Contains(dType);

                case AveFieldType.Number:
                case AveFieldType.Currency:
                case AveFieldType.Boolean:
                    return !numTypeCol.Contains(dType);

                case AveFieldType.DateTime:
                case AveFieldType.Lookup:
                case AveFieldType.User:
                    return !dateAndTimeTypeCol.Contains(dType);

                case AveFieldType.URL:
                case AveFieldType.Calculated:
                    return true;

                default:
                    return true;
            }
        }

        #endregion
    }
}