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
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using System.Globalization;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveFieldHelper
    {
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveFieldHelper));

        internal static bool IsGuid(string strToValidate)
        {
            bool isGuid = false;
            const string strRegexPatten = @"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\"
                                          + @"-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$";
            if (strToValidate != null && !strToValidate.Equals(""))
            {
                isGuid = Regex.IsMatch(strToValidate, strRegexPatten);
            }
            return isGuid;
        }

        internal static Dictionary<Guid, Guid> GetFieldMapping(Hashtable properties)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.GetFieldMapping"))
            {
                var mappings = new Dictionary<Guid, Guid>();
                if (null != properties && properties.Contains("Doc_Field_Mappings"))
                {
                    try
                    {
                        var mappingXml = (string)properties["Doc_Field_Mappings"];
                        mappings = ConvertXmlToFieldIDMapping(mappingXml);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.ConvertXmlToFieldIDMappingError, e.ToString());
                    }
                }
                return mappings;
            }
        }

        private static Dictionary<Guid, Guid> ConvertXmlToFieldIDMapping(string xml)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.ConvertXmlToFieldIDMapping"))
            {
                var doc = new XmlDocument();
                doc.LoadXml(xml);
                if (!doc.DocumentElement.Name.Equals("AvePointFieldMappings", StringComparison.OrdinalIgnoreCase))
                {
                    throw new AveException(string.Format("The mapping xml is not valid. \r\nXml={0}", doc.OuterXml));
                }

                var mapping = new Dictionary<Guid, Guid>();
                foreach (XmlElement ele in doc.DocumentElement.ChildElements())
                {
                    var sourceId = new Guid(ele.GetAttribute("SourceID"));
                    var destId = new Guid(ele.GetAttribute("CTID"));
                    mapping.Add(sourceId, destId);
                }
                return mapping;
            }
        }

        private static string ConvertFieldIDMappingToXml(IAveFieldMapping mappings)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.ConvertFieldIDMappingToXml"))
            {
                var doc = new XmlDocument();
                doc.AppendChild(doc.CreateElement("AvePointFieldMappings"));
                foreach (var mapping in mappings.EnumFieldSchemaMapping())
                {
                    XmlElement mappingElement = doc.CreateElement("Mapping");
                    mappingElement.SetAttribute("SourceID", mapping.Key.ToString());
                    mappingElement.SetAttribute("CTID", mapping.Value.ToString());
                    doc.DocumentElement.AppendChild(mappingElement);
                }
                return doc.OuterXml;
            }
        }

        internal static List<IAveFieldMapping> GetAvailableFieldIdMappings(IAveWeb web)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.GetAvailableFieldIdMappings"))
            {
                var avaliableMappings = new List<IAveFieldMapping>();
                if (null != web)
                {
                    avaliableMappings.Add(ConvertDictionaryToMapping(GetFieldMapping(web.AllProperties)));
                    IAveWeb parent = web.ParentWeb;

                    while (null != parent && parent.Exists)
                    {
                        var mappings = GetFieldMapping(parent.AllProperties);
                        avaliableMappings.Add(ConvertDictionaryToMapping(mappings));
                        var tmp = parent;
                        parent = parent.ParentWeb;
                        tmp.Dispose();
                    }
                }
                return avaliableMappings;
            }
        }

        private static IAveFieldMapping ConvertDictionaryToMapping(Dictionary<Guid, Guid> dictionary)
        {
            var mapping = new AveFieldMapping();
            mapping.SetFieldIdSchemaMappings(dictionary);
            return mapping;
        }

        internal static void UpdateFieldSchemaIdMappingProperty(IAveList list, IAveFieldMapping mappings)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.UpdateFieldSchemaIdMappingProperty"))
            {
                // for [ADO-55928]. sunguiwu modified. from | to  ||. 2012-11-28
                if (list == null || list.RootFolder.Properties == null)
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
            }
        }

        internal static void UpdateFieldSchemaIdMappingProperty(IAveWeb web, IAveFieldMapping mappings)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.UpdateFieldSchemaIdMappingProperty_1"))
            {
                string mappingStr = ConvertFieldIDMappingToXml(mappings);
                try
                {
                    bool changed = false;
                    if (!web.AllProperties.Contains("Doc_Field_Mappings"))
                    {
                        web.AllProperties.Add("Doc_Field_Mappings", mappingStr);
                        changed = true;
                    }
                    else
                    {
                        web.AllProperties["Doc_Field_Mappings"] = mappingStr;
                        changed = true;
                    }
                    if (changed)
                    {
                        web.Update();
                    }
                }
                catch (AveSecurityTrimingException ex)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while updating web property. WebId:{0}, WebUrl:{1}\n error message:{2}", web.ID, web.Url, ex));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.WARN, string.Format("An error occurred while updating web property. WebId:{0}, WebUrl:{1}\n error message:{2}", web.ID, web.Url, e));
                }
            }
        }

        private static Guid GetMappingIdFromAvailableSchema(Guid id, List<IAveFieldMapping> availableMappings)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.GetMappingIdFromAvailableSchema"))
            {
                if (null == availableMappings)
                {
                    throw new Exception("Available schema mapping is null.");
                }
                foreach (var mapping in availableMappings)
                {
                    var result = mapping.GetMappingSchemaFieldId(id);
                    if (Guid.Empty != result)
                    {
                        return result;
                    }
                }
                return Guid.Empty;
            }
        }

        internal static object GetFieldDefaultValues(IAveField field)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.GetFieldDefaultValues"))
            {
                object defaultValue = null;
                if ((field is IAveFieldDateTime) && field.DefaultValue.Equals("[today]", StringComparison.Ordinal))
                {
                    defaultValue = DateTime.Now.ToUniversalTime();
                }
                else
                {
                    //support text and calculated default value
                    defaultValue = field.DefaultValueTyped.ToString();
                }
                return defaultValue;
            }
        }
        internal static string GetTaxonomyFieldValue(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            try
            {
                string[] strArray2 = value.Split(new string[] { ";#" }, StringSplitOptions.None);
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
                log.Warn("An error occurred while get field DefaultValueTyped by DefaultValue.DefaultValue:{1},Error:{2}", value, ex);
            }
            if (builder.Length > 0)
            {
                builder.Length--;
            }
            return builder.ToString();
        }

        #region MD5 Property

        public static string GetMD5FromSchemaXml(IAveField field)
        {
            string md5 = string.Empty;
            if (field != null)
            {
                try
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(field.SchemaXml);
                    if (doc.DocumentElement.HasAttribute("AveMD5Property"))
                    {
                        md5 = doc.DocumentElement.GetAttribute("AveMD5Property");
                    }
                }
                catch (Exception ex)
                {
                    log.Error("GetMD5FromSchemaXml Error.Exception:" + ex);
                }
            }
            return md5;
        }

        public static void UpdateMD5ToSchemaXml(IAveField field)
        {
            try
            {
                if (field != null)
                {
                    var doc = new XmlDocument();
                    doc.LoadXml(field.SchemaXml);
                    doc.DocumentElement.SetAttribute("AveMD5Property", GetCurrentMD5Property(field));
                    field.SchemaXml = doc.OuterXml;
                    field.Update();
                }
            }
            catch (Exception ex)
            {
                log.Error("UpdateMD5ToSchemaXml Error.Exception:" + ex);
            }
        }

        public static string GetCurrentMD5Property(IAveField field)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.GetCurrentMD5Property"))
            {
                if (String.IsNullOrEmpty(field.MD5))
                {
                    var builder = new StringBuilder();
                    builder.Append(field.Title);
                    builder.Append(";" + BaseFieldMD5Property(field));
                    switch (field.Type)
                    {
                        case AveFieldType.Lookup:
                            var lookupField = field as IAveFieldLookup;
                            if (lookupField != null)
                            {
                                builder.Append(";" + LookupFieldMD5Property(lookupField));
                            }
                            break;
                        case AveFieldType.User:
                            var userField = field as IAveFieldUser;
                            if (userField != null)
                            {
                                builder.Append(";" + UserFieldMD5Property(userField));
                            }
                            break;
                        case AveFieldType.DateTime:
                            var timeField = field as IAveFieldDateTime;
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
                            var boolField = field as IAveFieldBoolean;
                            if (boolField != null)
                            {
                                builder.Append(";" + BoolFieldMD5Property(boolField));
                            }
                            break;
                        case AveFieldType.Choice:
                        //case AveFieldType.ContactInfo:
                        //case AveFieldType.Whereabout:
                        case AveFieldType.WorkflowStatus:
                            var choiceField = field as IAveFieldChoice;
                            if (choiceField != null)
                            {
                                builder.Append(";" + ChoiceFieldMD5Property(choiceField));
                            }
                            break;
                        case AveFieldType.MultiChoice:
                            var multiChocieField = field as IAveFieldMultiChoice;
                            if (multiChocieField != null)
                            {
                                builder.Append(";" + MultiChocieFieldMD5Property(multiChocieField));
                            }
                            break;
                        case AveFieldType.Calculated:
                            var calField = field as IAveFieldCalculated;
                            if (calField != null)
                            {
                                builder.Append(";" + CalculatedFieldMD5Property(calField));
                            }
                            break;
                        case AveFieldType.Computed:
                            var computedField = field as IAveFieldComputed;
                            if (computedField != null)
                            {
                                builder.Append(";" + ComputedFieldMD5Property(computedField));
                            }
                            break;
                        case AveFieldType.Currency:
                            var currencyField = field as IAveFieldCurrency;
                            if (currencyField != null)
                            {
                                builder.Append(";" + CurrencyFieldMD5Property(currencyField));
                            }
                            break;
                        case AveFieldType.Number:
                        case AveFieldType.Integer:
                        case AveFieldType.WorkflowEventType:
                            var numberField = field as IAveFieldNumber;
                            if (numberField != null)
                            {
                                builder.Append(";" + NumberFieldMD5Property(numberField));
                            }
                            break;
                        case AveFieldType.Note:
                            var mulTextField = field as IAveFieldMultiLineText;
                            if (mulTextField != null)
                            {
                                builder.Append(";" + NoteFieldMD5Property(mulTextField));
                            }
                            break;
                        case AveFieldType.GridChoice:
                            var gridField = field as IAveFieldRatingScale;
                            if (gridField != null)
                            {
                                builder.Append(";" + GridFieldMD5Property(gridField));
                            }
                            break;
                        case AveFieldType.Text:
                            //case AveFieldType.Confirmations:
                            var textField = field as IAveFieldText;
                            if (textField != null)
                            {
                                builder.Append(";" + TextFieldMD5Property(textField));
                            }
                            break;
                        case AveFieldType.URL:
                            var urlField = field as IAveFieldUrl;
                            if (urlField != null)
                            {
                                builder.Append(";" + UrlFieldMD5Property(urlField));
                            }
                            break;
                        case AveFieldType.Invalid:
                            if (field.TypeAsString == "TaxonomyFieldType" ||
                                field.TypeAsString == "TaxonomyFieldTypeMulti")
                            {
                                builder.Append(";" + TaxonomyFieldMD5Property(field));
                            }
                            else
                            {
                                builder.Append(";" + InvalidFieldMD5Property(field));
                            }
                            break;
                    }
                    field.MD5 = SHA1Hash(builder.ToString());
                }
                return field.MD5;
            }
        }

        private static string BaseFieldMD5Property(IAveField field)
        {
            var builder = new StringBuilder();
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
            var builder = new StringBuilder();
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
            var builder = new StringBuilder();
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
            var builder = new StringBuilder();
            builder.Append(";" + field.CalendarType.ToString());
            builder.Append(";" + field.DisplayFormat.ToString());
            return builder.ToString();
        }

        private static string BoolFieldMD5Property(IAveFieldBoolean field)
        {
            var builder = new StringBuilder();
            builder.Append(";" + field.JumpToNoField);
            builder.Append(";" + field.JumpToYesField);
            return builder.ToString();
        }

        private static string ChoiceFieldMD5Property(IAveFieldChoice field)
        {
            var builder = new StringBuilder();
            builder.Append(";" + field.EditFormat.ToString());

            builder.Append(";" + MultiChocieFieldMD5Property(field));
            return builder.ToString();
        }

        private static string MultiChocieFieldMD5Property(IAveFieldMultiChoice field)
        {
            var builder = new StringBuilder();
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
            var builder = new StringBuilder();
            builder.Append(";" + field.DateFormat.ToString());
            builder.Append(";" + field.Formula);
            builder.Append(";" + field.OutputType.ToString());
            return builder.ToString();
        }

        private static string ComputedFieldMD5Property(IAveFieldComputed field)
        {
            var builder = new StringBuilder();
            builder.Append(";" + field.EnableLookup.ToString());
            return builder.ToString();
        }

        private static string CurrencyFieldMD5Property(IAveFieldCurrency field)
        {
            var builder = new StringBuilder();
            builder.Append(";" + field.CurrencyLocaleId.ToString());
            builder.Append(";" + NumberFieldMD5Property(field));
            return builder.ToString();
        }

        private static string NumberFieldMD5Property(IAveFieldNumber field)
        {
            var builder = new StringBuilder();
            builder.Append(";" + field.DisplayFormat.ToString());
            builder.Append(";" + field.MaximumValue.ToString());
            builder.Append(";" + field.MinimumValue.ToString());
            builder.Append(";" + field.DefaultValue);
            builder.Append(";" + field.ShowAsPercentage.ToString());
            return builder.ToString();
        }

        private static string NoteFieldMD5Property(IAveFieldMultiLineText field)
        {
            var builder = new StringBuilder();
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
            var builder = new StringBuilder();
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
            var builder = new StringBuilder();
            builder.Append(";" + field.MaxLength.ToString());
            builder.Append(";" + field.DifferencingLimit.ToString());
            return builder.ToString();
        }

        private static string UrlFieldMD5Property(IAveFieldUrl field)
        {
            var builder = new StringBuilder();
            builder.Append(";" + field.DisplayFormat.ToString());
            return builder.ToString();
        }

        private static string TaxonomyFieldMD5Property(IAveField field)
        {
            var taxField = field as IAveTaxonomyField;
            var builder = new StringBuilder();
            builder.Append(";" + taxField.TermSetId.ToString());
            return builder.ToString();
        }

        private static string InvalidFieldMD5Property(IAveField field)
        {
            var builder = new StringBuilder();
            builder.Append(";" + field.Node.InnerXml);
            return builder.ToString();
        }

        private static string SHA1Hash(string text)
        {

            using (new AvePerformanceScope("Restore.AveFieldHelper.SHA1Hash"))
            {

                if (string.IsNullOrEmpty(text))
                    return string.Empty;
                IHashAlgorithm hash = HashAlgorithmFactory.CreateHashAlgorithm(HashAlgorithm.SHA1);
                byte[] orginaldata = Encoding.Default.GetBytes(text);
                byte[] data = hash.ComputeHash(orginaldata);
                string hashValue = BitConverter.ToString(data);
                hashValue = hashValue.Replace("-", string.Empty);
                return hashValue;

            }

        }

        #endregion

        #region Update Field

        internal static bool UpdateFieldType(IAveField spField, AveXmlField xmlField,AveSPList list)
        {
            try
            {
                if (spField.Type != xmlField.Type)
                {
                    if (UpdateSpecialFieldType(spField, xmlField))
                    {
                        return true;
                    }
                    else if (list != null && UpdateListChoiceFieldType(spField, xmlField, list))
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
                log.Warn("The Source Field:{0}, Id:{1}, Type:{2}, SourceType:{3}, Exception:{4}", spField.Title,
                         spField.ID, spField.Type, xmlField.Type, e.ToString());
            }
            return false;
        }
        /// <summary>
        /// 更改Choice类型Column type。
        /// </summary>
        /// <param name="spField"></param>
        /// <param name="xmlField"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        private static bool UpdateListChoiceFieldType(IAveField spField, AveXmlField xmlField, AveSPList list)
        {
            if (spField.Type == AveFieldType.Choice && xmlField.Type == AveFieldType.MultiChoice)
            {
                if (spField.EnforceUniqueValues || spField.Indexed)
                {
                    spField.EnforceUniqueValues = false;
                    spField.Indexed = false;
                    spField.Update();
                    list.SPList.ReloadFields();
                    spField = list.SPList.Fields[spField.ID];
                }
                spField.Type = AveFieldType.MultiChoice;
                spField.Update();
                list.SPList.ReloadFields();
                return true;
            }
            else if (spField.Type == AveFieldType.MultiChoice && xmlField.Type == AveFieldType.Choice)
            {
                spField.Type = AveFieldType.Choice;
                spField.Update();
                list.SPList.ReloadFields();
                return true;
            }
            else
            {
                return false;
            }
        }

        private static bool UpdateSpecialFieldType(IAveField spField, AveXmlField xmlField)
        {
            try
            {
                //Merge CI[CI-29815]: SPM在处理Folder还原的时候会有Folder 还原失败的情况，为了避免Folder 还原失败导致之后所有Folder 还原失败，SPM添加了新的Folder 还原逻辑。wrapper 对这种Folder 需要提供支持。
                if ((spField.Type == AveFieldType.DateTime && xmlField.Type == AveFieldType.Choice) || (spField.Type == AveFieldType.Choice && xmlField.Type == AveFieldType.Number))
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
                log.Warn("The Source Field:{0}, Id:{1}, Type:{2}, SourceType:{3}, Exception:{4}", spField.Title,
                         spField.ID, spField.Type, xmlField.Type, ex.ToString());
            }
            return false;
        }
        #endregion

        #region Rename Field

        internal static string GetNewInternalName(string name, IAveFieldCollection fields)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.GetNewInternalName"))
            {
                int extentNum = 1;
                IAveFieldCollection tFields = fields;
                string fieldName = name;
                do
                {
                    try
                    {
                        tFields.GetFieldByInternalName(fieldName);
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
                } while (extentNum++ < 500);
                return fieldName;
            }
        }

        internal static string GetNewDisplayName(string name, IAveFieldCollection fields)
        {
            return GetNewDisplayName(name, Guid.Empty, fields);
        }

        internal static string GetNewDisplayName(string name, Guid fieldId, IAveFieldCollection fields)
        {
            int extentNum = 1;
            IAveFieldCollection tFields = fields;
            string fieldDisplayName = name;
            while (tFields.Any<IAveField>(field => field.Title.Equals(fieldDisplayName, StringComparison.OrdinalIgnoreCase) && !field.Hidden && (fieldId == Guid.Empty || (fieldId != field.ID))))
            {
                fieldDisplayName = name + "_" + extentNum++;
            }
            return fieldDisplayName;
        }

        #endregion

        #region Find field in collection

        internal static IAveField FindFieldBySchema(Guid fieldId, IAveFieldCollection collection, List<IAveFieldMapping> mappings)
        {
            Guid mappedFieldId = GetMappingIdFromAvailableSchema(fieldId, mappings);
            if (Guid.Empty != mappedFieldId)
            {
                return FindFieldById(mappedFieldId, collection);
            }
            return null;
        }

        internal static IAveField FindFieldById(Guid fieldId, IAveFieldCollection collection)
        {
            IAveField field = null;
            try
            {
                if (!Equals(Guid.Empty, fieldId))
                {
                    field = collection[fieldId];
                }
            }
            catch (ArgumentException)
            {
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldInCollectionError, e.ToString());
            }
            return field;
        }

        internal static IAveField FindFieldByDisplayName(string name, IAveFieldCollection collection, AveXmlField xmlField, AveCustomFieldType customFieldType = AveCustomFieldType.SameType)
        {
            foreach (var field in collection)
            {
                if (String.Equals(name, field.Title, StringComparison.OrdinalIgnoreCase))
                {
                    switch (customFieldType)
                    {
                        case AveCustomFieldType.SameType:

                            bool isMatch = IsFieldTypesCompatible(xmlField.TypeAsString, field.TypeAsString);

                            var xdoc = new XmlDocument();
                            xdoc.LoadXml(field.SchemaXml);
                            var srcHasRef = xmlField.XmlElement.HasAttribute("FieldRef");
                            var destHasRef = xdoc.DocumentElement.HasAttribute("FieldRef");
                            var srcRef = xmlField.XmlElement.GetAttribute("FieldRef");
                            var destRef = xdoc.DocumentElement.GetAttribute("FieldRef");
                            if (srcHasRef != destHasRef || !srcRef.Equals(destRef, StringComparison.OrdinalIgnoreCase))
                            {
                                isMatch = false;
                            }

                            if (isMatch)
                            {
                                return field;
                            }
                            break;
                        case AveCustomFieldType.ChangeToDestination:
                            //ChangeToDestination不再需要检查兼容性
                            return field;
                        case AveCustomFieldType.ChangeToMetadata:
                            if (field.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                            {
                                return field;
                            }
                            break;
                        case AveCustomFieldType.ChangeToLookup:
                            if (field.Type == AveFieldType.Lookup)
                            {
                                return field;
                            }
                            break;
                    }
                }
            }
            return null;
        }

        internal static IAveField FindFieldByInternalName(string name, IAveFieldCollection collection, AveXmlField xmlField, AveCustomFieldType customFieldType = AveCustomFieldType.SameType)
        {
            IAveField result = null;
            IAveField field = collection.GetFieldByInternalName(name);
            switch (customFieldType)
            {
                case AveCustomFieldType.SameType:
                    result = IsFieldTypesCompatible(xmlField.TypeAsString, field.TypeAsString) ? field : null;
                    break;
                case AveCustomFieldType.ChangeToDestination:
                    //ChangeToDestination不再需要检查兼容性
                    result = field;            
                    break;
                case AveCustomFieldType.ChangeToMetadata:
                    result = field.TypeAsString.Equals("TaxonomyFieldType", StringComparison.OrdinalIgnoreCase) || field.TypeAsString.Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase) ? field : null;
                    break;
                case AveCustomFieldType.ChangeToLookup:
                    result = field.Type == AveFieldType.Lookup ? field : null;
                    break;
            }
            return result;
        }

        public static IAveField FindFieldByName(string name, string TypeAsString, IAveFieldCollection collection, FieldFindOption findOption)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.FindFieldByName"))
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new ArgumentException("Argument 'name' cannot be null.");
                }
                IAveField field = null;
                try
                {
                    switch (findOption)
                    {
                        case FieldFindOption.InternalName:
                            field = collection.GetFieldByInternalName(name);
                            break;
                        case FieldFindOption.StaticName:
                            field = collection.TryGetFieldByStaticName(name);
                            break;
                        case FieldFindOption.DisplayName:
                            field = collection[name];
                            break;
                    }
                    if (null != field)
                    {
                        if (!IsFieldTypesCompatible(TypeAsString, field.TypeAsString))
                        {
                            field = null;
                        }
                    }
                }
                catch (ArgumentException)
                {
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.FindFieldFailed, name, e);
                }
                return field;
            }
        }

        internal static IAveField FindFieldByCustomMapping(AveXmlField xmlField, IAveFieldCollection collection)
        {
            using (new AvePerformanceScope("Restore.AveFieldHelper.FindFieldByCustomMapping"))
            {
                IAveField field = null;
                AveCustomFieldInfo customFieldInfo = xmlField.CustomFieldInfo;
                string srcFieldTypeString = xmlField.TypeAsString;
                if (!string.IsNullOrEmpty(customFieldInfo.InternalName) && customFieldInfo.UseInternalOrDisplay)
                {
                    try
                    {
                        field = FindFieldByInternalName(customFieldInfo.InternalName, collection, xmlField, customFieldInfo.CustomFieldType);
                        log.Debug("Find field title {0}, type: ,{1} by internal name {2} ", field.Title, field.Type, customFieldInfo.InternalName);
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldByInternalNameError, e.ToString());
                    }
                }
                else if (!string.IsNullOrEmpty(customFieldInfo.Name) && !customFieldInfo.UseInternalOrDisplay)
                {
                    try
                    {
                        field = FindFieldByDisplayName(customFieldInfo.Name, collection, xmlField, customFieldInfo.CustomFieldType);
                        if (field != null)
                        {
                            log.Debug("Find field title {0}, type: ,{1} by display name {2} ", field.Title, field.Type, customFieldInfo.Name);
                        }
                    }
                    catch (Exception e)
                    {
                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFileByNameError, e.ToString());
                    }
                }
                return field;
            }
        }

        private static bool IsFieldTypesCompatible(string sType, string dType)
        {
            if (sType.Equals(dType, StringComparison.Ordinal)
                || (sType.Equals("Choice", StringComparison.Ordinal) && dType.Equals("MultiChoice", StringComparison.Ordinal))
                || (sType.Equals("MultiChoice", StringComparison.Ordinal) && dType.Equals("Choice", StringComparison.Ordinal))
                || (sType.Equals("TaxonomyFieldType", StringComparison.Ordinal) && dType.Equals("TaxonomyFieldTypeMulti", StringComparison.Ordinal))
                || (sType.Equals("TaxonomyFieldTypeMulti", StringComparison.Ordinal) && dType.Equals("TaxonomyFieldType", StringComparison.Ordinal))
                || (sType.Equals("Lookup", StringComparison.Ordinal) && dType.Equals("LookupMulti", StringComparison.Ordinal))
                || (sType.Equals("LookupMulti", StringComparison.Ordinal) && dType.Equals("Lookup", StringComparison.Ordinal))
                || (sType.Equals("User", StringComparison.Ordinal) && dType.Equals("UserMulti", StringComparison.Ordinal))
                || (sType.Equals("UserMulti", StringComparison.Ordinal) && dType.Equals("User", StringComparison.Ordinal)))
            {
                return true;
            }

            return false;
        }

        #endregion
    }
}