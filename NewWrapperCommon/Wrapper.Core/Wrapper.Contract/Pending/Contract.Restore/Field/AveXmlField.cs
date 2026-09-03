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
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Web;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.GCommon.Utility;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.Restore;

namespace AvePoint.Wrapper.Restore
{
    public class AveXmlField
    {
        //private int mRowOrdinal = 0;
        //private bool mFromBase = false;
        //private bool mReadOnly = false;
        //private bool mRequired = false;
        //private bool mComputed = false;
        //private string mDisplayname = "";
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveXmlField));
        private readonly XmlElement mXmlElement;
        private Hashtable dictType;
        public bool? FromParent = false;

        public bool FromBaseType { get; private set; }

        public AveXmlField()
        {
        }
        public AveXmlField(XmlElement xe, int lcid)
        {
            Direction = "none";
            Description = String.Empty;
            ID = Guid.Empty;
            EditFormat = AveChoiceFormatType.Dropdown;
            OutputType = AveFieldType.Text;
            DisplayFormatCalculated = AveNumberFormatTypes.Automatic;
            DateFormat = AveDateTimeFieldFormatType.DateTime;
            CurrencyLocaleId = -1;
            RichTextMode = AveRichTextMode.Compatible;
            MinimumValue = double.MinValue;
            MaximumValue = double.MaxValue;
            DisplayFormatNumber = AveNumberFormatTypes.Automatic;
            DisplayFormatUrl = AveUrlFieldFormatType.Hyperlink;
            SelectionMode = AveFieldUserSelectionMode.PeopleAndGroups;
            Presence = true;
            AllowDisplay = true;
            CustomProperties = new Hashtable();
            DisplayFormat = AveDateTimeFieldFormatType.DateTime;
            CalendarType = AveCalendarType.None;
            RelationshipDeleteBehavior = AveRelationshipDeleteBehavior.None;
            RestrictedMode = false;
            RestoreStatus = FieldRestoreStatus.None;

            using (new AvePerformanceScope("Restore.AveXmlField.Constructor"))
            {


                mXmlElement = xe;
                FieldInternalName = xe.Attributes["Name"].Value;

                SetFieldProperties(xe, lcid);
                Type = GetFieldType(FieldBaseType);
                switch (Type)
                {
                    case AveFieldType.Lookup:
                        //case SPFieldType.Facilities:
                        SetLookupProperties(xe);
                        break;
                    case AveFieldType.User:
                        //case SPFieldType.CallTo:
                        //case SPFieldType.SendTo:
                        SetUserProperties(xe);
                        break;
                    case AveFieldType.DateTime:
                        //case SPFieldType.From:
                        //case SPFieldType.DueDate:
                        //case SPFieldType.CallTime:
                        //case SPFieldType.Until:
                        SetDateTimeProperties(xe);
                        break;
                    case AveFieldType.Boolean:
                    //case SPFieldType.WhatsNew:
                    //case SPFieldType.Confidential:
                    case AveFieldType.AllDayEvent:
                        //case SPFieldType.AllowEditing:
                        SetBooleanProperties(xe);
                        break;
                    case AveFieldType.Choice:
                    //case SPFieldType.ContactInfo:
                    //case SPFieldType.Whereabout:
                    case AveFieldType.WorkflowStatus:
                    case AveFieldType.OutcomeChoice:
                        SetChoiceProperties(xe);
                        break;
                    case AveFieldType.Calculated:
                        SetCalculatedProperties(xe, lcid);
                        break;
                    case AveFieldType.Computed:
                        EnableLookup = GetFieldBoolValue("EnableLookup");
                        break;
                    case AveFieldType.Currency:
                        SetCurrencyProperties(xe, lcid);
                        break;
                    case AveFieldType.Number:
                    case AveFieldType.Integer:
                    case AveFieldType.WorkflowEventType:
                        SetNumberProperties(xe);
                        break;
                    case AveFieldType.MultiChoice:
                        SetMultiChoiceProperties(xe);
                        break;
                    case AveFieldType.Note:
                        SetNoteProperties(xe);
                        break;
                    case AveFieldType.GridChoice:
                        SetGridChoiceProperties(xe);
                        break;
                    case AveFieldType.Text:
                        //case SPFieldType.Confirmations:
                        SetTextProperties(xe);
                        break;
                    case AveFieldType.URL:
                        DisplayFormatUrl = AveUrlFieldFormatType.Hyperlink;
                        if (xe.HasAttribute("Format"))
                        {
                            string tempFormat = xe.GetAttribute("Format");
                            if (!string.IsNullOrEmpty(tempFormat))
                            {
                                try
                                {
                                    AveUrlFieldFormatType type = (AveUrlFieldFormatType)Enum.Parse(typeof(AveUrlFieldFormatType), tempFormat);
                                    DisplayFormatUrl = type;
                                }
                                catch (Exception e)
                                {
                                    log.Warn("Parse format:{0} to AveUrlFieldFormatType failed:{1}", tempFormat, e.ToString());
                                }
                            }
                        }
                        break;
                    case AveFieldType.Invalid:
                        if (FieldBaseType == "TaxonomyFieldType" || FieldBaseType == "TaxonomyFieldTypeMulti")
                        {
                            SetCustomization();
                        }
                        break;
                }

                SetAveProperties();
                SetUserResource();

            }

        }

        private void SetUserResource()
        {
            var node = mXmlElement.SelectSingleNode(SerializationConstants.ColumnConstants.RESOURCE_NODE);
            if (node != null)
            {
                TitleResource = ConvertToUserResource(node, SerializationConstants.ColumnConstants.TITLE_RESOUCE_NODE);
                DescriptionResource = ConvertToUserResource(node, SerializationConstants.ColumnConstants.DESCRIPTION_RESOUCE_NODE);
            }
        }

        /// <summary>
        /// Get the list field internal name.
        /// </summary>
        public string FieldInternalName { get; set; }

        public AveFieldType Type { get; set; }

        public AveCustomFieldInfo CustomFieldInfo { get; set; }

        public FieldRestoreStatus RestoreStatus { get; set; }

        public bool NeedFindByDisplayName { get; set; }

        ///// <summary>
        ///// Get the row ordinal of a list item.
        ///// </summary>
        //public int FieldRowOrdinal
        //{
        //    get { return mRowOrdinal; }
        //}

        public XmlElement XmlElement
        {
            get { return mXmlElement; }
        }

        private void SetLookupProperties(XmlElement xe)
        {

            using (new AvePerformanceScope("Restore.AveXmlField.SetLookupProperties"))
            {

                if (xe.HasAttribute("ShowField"))
                {
                    LookupField = xe.GetAttribute("ShowField"); //move localized logic to backup module
                }
                if (xe.HasAttribute("UnlimitedLengthInDocumentLibrary"))
                {
                    UnlimitedLengthInDocumentLibrary =
                        Convert.ToBoolean(xe.GetAttribute("UnlimitedLengthInDocumentLibrary"));
                }
                if (xe.HasAttribute("RelationshipDeleteBehavior"))
                {
                    RelationshipDeleteBehavior =
                        (AveRelationshipDeleteBehavior)
                        Enum.Parse(typeof(AveRelationshipDeleteBehavior), xe.GetAttribute("RelationshipDeleteBehavior"));
                }
                //Set this property in SetFieldProperties
                //if (xe.HasAttribute("Mult"))
                //{
                //    mAllowMultipleValues = Convert.ToBoolean(xe.GetAttribute("Mult"));
                //}
                if (xe.HasAttribute("IsRelationship"))
                {
                    IsRelationship = Convert.ToBoolean(xe.GetAttribute("IsRelationship"));
                }
                if (xe.HasAttribute("List"))
                {
                    LookupList = xe.GetAttribute("List");
                }
                if (xe.HasAttribute("WebId"))
                {
                    LookupWebId = xe.GetAttribute("WebId");
                }
                if (xe.HasAttribute("PrependId"))
                {
                    PrependId = Convert.ToBoolean(xe.GetAttribute("PrependId"));
                }
                if (xe.HasAttribute("FieldRef"))
                {
                    PrimaryFieldId = xe.GetAttribute("FieldRef");
                }
                if (xe.HasAttribute("CountRelated"))
                {
                    CountRelated = Convert.ToBoolean(xe.GetAttribute("CountRelated"));
                }

            }

        }

        private void SetAveProperties()
        {

            using (new AvePerformanceScope("Restore.AveXmlField.SetAveProperties"))
            {

                foreach (XmlNode node in mXmlElement.ChildNodes)
                {
                    if (node is XmlElement)
                    {
                        XmlElement element = (XmlElement)node;
                        if (element.Name == "AveFieldInfo")
                        {
                            if (element.HasAttribute("AveLookupListTitle"))
                            {
                                AveLookupListTitle = element.GetAttribute("AveLookupListTitle");
                            }
                            if (element.HasAttribute("AveLookupWebTitle"))
                            {
                                AveLookupWebTitle = element.GetAttribute("AveLookupWebTitle");
                            }
                            if (element.HasAttribute("AveSourceType"))
                            {
                                AveSourceType = element.GetAttribute("AveSourceType");
                            }
                            if (element.HasAttribute("AveLookupListID"))
                            {
                                AveLookupListID = element.GetAttribute("AveLookupListID");
                                if ("Docs".Equals(AveLookupListID, StringComparison.OrdinalIgnoreCase))
                                {//Old Backup Data.
                                    AveLookupListID = string.Empty;
                                }
                            }
                            // the IsRelationship  property is in childnode,we should get at here
                            if (element.HasAttribute("IsRelationship"))
                            {
                                IsRelationship = Convert.ToBoolean(element.GetAttribute("IsRelationship"));
                            }
                            mXmlElement.RemoveChild(element);
                            break;
                        }
                    }
                }

            }

        }

        private AveUserResourceInfo ConvertToUserResource(XmlNode parent, string tagName)
        {
            var resourceNode = parent.SelectSingleNode(tagName);
            if (resourceNode != null)
            {
                return (AveUserResourceInfo)SerializerHelper.DeserializeByDataContractSerializer(resourceNode.InnerXml, typeof(AveUserResourceInfo));
            }
            return null;
        }

        private void SetUserProperties(XmlElement xe)
        {
            AllowDisplay = xe.GetAttribute("ForcedDisplay") != "***";

            Presence = !xe.HasAttribute("Presence") || Convert.ToBoolean(xe.GetAttribute("Presence"));
            if (xe.HasAttribute("UserSelectionScope"))
            {
                SelectionGroup = Convert.ToInt32(xe.GetAttribute("UserSelectionScope"));
            }
            if (xe.HasAttribute("UserSelectionMode"))
            {
                SelectionMode =
                    (AveFieldUserSelectionMode)
                    Enum.Parse(typeof(AveFieldUserSelectionMode), xe.GetAttribute("UserSelectionMode"));
            }

            SetLookupProperties(xe);
        }

        private void SetDateTimeProperties(XmlElement xe)
        {
            if (xe.HasAttribute("CalType"))
            {
                CalendarType = (AveCalendarType)Convert.ToInt32(xe.GetAttribute("CalType"));
            }
            try
            {
                if (xe.HasAttribute("Format"))
                {
                    if (xe.GetAttribute("Format").Equals("DateOnly") || xe.GetAttribute("Format").Equals("DateTime"))
                    {
                        DisplayFormat =
                            (AveDateTimeFieldFormatType)
                            Enum.Parse(typeof(AveDateTimeFieldFormatType), xe.GetAttribute("Format"));
                    }
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN,
                        string.Format("An error occurred while set DataTime property. format:{0}\n error message:{1}",
                                      xe.GetAttribute("Format"), e));
            }
            if (xe.HasAttribute("FriendlyDisplayFormat"))
            {
                FriendlyDisplayFormat = (AveSPDateTimeFieldFriendlyFormatType)Enum.Parse(typeof(AveSPDateTimeFieldFriendlyFormatType), xe.GetAttribute("FriendlyDisplayFormat"));
            }
        }

        private void SetBooleanProperties(XmlElement xe)
        {
            if (xe.HasAttribute("JumpToNo"))
            {
                JumpToNoField = xe.GetAttribute("JumpToNo");
            }
            if (xe.HasAttribute("JumpToYes"))
            {
                JumpToYesField = xe.GetAttribute("JumpToYes");
            }
        }

        private void SetNoteProperties(XmlElement xe)
        {
            if (xe.HasAttribute("UnlimitedLengthInDocumentLibrary"))
            {
                UnlimitedLengthInDocumentLibrary =
                    Convert.ToBoolean(xe.GetAttribute("UnlimitedLengthInDocumentLibrary"));
            }
            if (xe.HasAttribute("RichTextMode"))
            {
                var richTextModeValue = xe.GetAttribute("RichTextMode");
                if (string.Equals(richTextModeValue, "ThemeHtml", StringComparison.Ordinal))
                {
                    RichTextMode = AveRichTextMode.ThemeHtml;
                }
                else if (string.Equals(richTextModeValue, "HtmlAsXml", StringComparison.Ordinal))
                {
                    RichTextMode = AveRichTextMode.HtmlAsXml;
                }
                else if (string.Equals(richTextModeValue, "FullHtml", StringComparison.Ordinal))
                {
                    RichTextMode = AveRichTextMode.FullHtml;
                }
                else
                {
                    RichTextMode = AveRichTextMode.Compatible;
                }
            }
            if (xe.HasAttribute("RichText"))
            {
                RichText = Convert.ToBoolean(xe.GetAttribute("RichText"));
            }
            //XmlNode namedItem = xe.Attributes.GetNamedItem("RestrictedMode");
            //if (((namedItem != null) && !(namedItem.Value == "TRUE")) && !(namedItem.Value == "-1"))
            //{
            //    mRestrictedMode = false;
            //}
            //else
            //{
            //    mRestrictedMode = true;
            //}
            AllowHyperlink = xe.GetAttribute("AllowHyperlink") == "TRUE";
            AppendOnly = xe.GetAttribute("AppendOnly") == "TRUE";
            DifferencingLimit = xe.HasAttribute("DifferencingLimit")
                                    ? Convert.ToInt32(xe.GetAttribute("DifferencingLimit"))
                                    : 0x5dc;
            IsolateStyles = xe.GetAttribute("IsolateStyles") == "TRUE";
            NumberOfLines = xe.HasAttribute("NumLines") ? Convert.ToInt32(xe.GetAttribute("NumLines")) : 6;
            if (NumberOfLines >= 0x3e9 || NumberOfLines <= 0)
            {
                NumberOfLines = 6;
            }
        }

        private void SetMultiChoiceProperties(XmlElement xe)
        {

            using (new AvePerformanceScope("Restore.AveXmlField.SetMultiChoiceProperties"))
            {

                FillInChoice = GetFieldBoolValue("FillInChoice");
                if (xe.HasChildNodes)
                {
                    foreach (XmlNode node in xe.ChildNodes)
                    {
                        if (node is XmlElement)
                        {
                            XmlElement choicesElement = (XmlElement)node;
                            if (choicesElement.Name == "CHOICES")
                            {
                                foreach (XmlNode childNode in choicesElement.ChildNodes)
                                {
                                    if (childNode is XmlElement)
                                    {
                                        XmlElement choice = (XmlElement)childNode;
                                        Choices.Add(choice.InnerText);
                                    }
                                }
                                break;
                            }
                        }
                    }
                }

            }

        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues",
            Justification = "Fillin:Attribute of choice property.")]
        private void SetChoiceProperties(XmlElement xe)
        {
            FillinChoiceJumpTo = GetFieldAttributeValue("JumpToFillinChoice");
            SetMultiChoiceProperties(xe);
            if (xe.HasAttribute("Format"))
            {
                EditFormat = (AveChoiceFormatType)Enum.Parse(typeof(AveChoiceFormatType), xe.GetAttribute("Format"));
            }
        }

        private void SetNumberProperties(XmlElement xe)
        {
            ShowAsPercentage = GetFieldBoolValue("Percentage");
            try
            {
                if (xe.HasAttribute("Min"))
                {
                    MinimumValue = double.Parse(xe.GetAttribute("Min"), CultureInfo.InstalledUICulture);
                }
                if (xe.HasAttribute("Max"))
                {
                    MaximumValue = double.Parse(xe.GetAttribute("Max"), CultureInfo.InstalledUICulture);
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetMinAndMaxValueFailed, e);
            }
            //-1 是DisplayFormatNumber的默认值，表示Automatic，如果置成0会导致在2013里document version还出错
            int decimals = GetFieldIntValue("Decimals", -1);
            if (decimals > 5 || decimals < -1)
            {
                DisplayFormatNumber = AveNumberFormatTypes.Automatic;
            }
            else
            {
                DisplayFormatNumber = (AveNumberFormatTypes)decimals;
            }
        }

        private void SetCurrencyProperties(XmlElement xe, int lcid)
        {
            CurrencyLocaleId = GetFieldIntValue("LCID", lcid);
            SetNumberProperties(xe);
        }

        private void SetCalculatedProperties(XmlElement xe, int lcid)
        {
            CurrencyLocaleId = GetFieldIntValue("LCID", lcid);
            if (xe.HasAttribute("Format"))
            {
                DateFormat =
                    (AveDateTimeFieldFormatType)
                    Enum.Parse(typeof(AveDateTimeFieldFormatType), GetFieldAttributeValue("Format"));
            }
            else
            {
                DateFormat = AveDateTimeFieldFormatType.DateOnly;
            }
            int decimals = GetFieldIntValue("Decimals", -1);
            if (decimals > 5 || decimals < -1)
            {
                DisplayFormatCalculated = AveNumberFormatTypes.Automatic;
            }
            else
            {
                DisplayFormatCalculated = (AveNumberFormatTypes)decimals;
            }
            Formula = HttpUtility.HtmlDecode(GetSingleNodeValue("Formula", false));
            var fieldRefsXmlNode = (xe as XmlNode).SelectSingleNode("FieldRefs");
            if (fieldRefsXmlNode != null)
            {
                FieldRefXml = fieldRefsXmlNode.InnerXml;
            }
            else
            {
                FieldRefXml = String.Empty;
            }
            if (xe.HasAttribute("ResultType"))
            {
                OutputType = GetFieldType(GetFieldAttributeValue("ResultType"));
            }
            if (OutputType == AveFieldType.Number && GetFieldBoolValue("Percentage"))
            {
                ShowAsPercentage = true;
            }
            //If this is a calculated field, it must be readonly. Added by Austin
            ReadOnlyField = true;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues",
            Justification = "Rng:Attribute of field property.")]
        private void SetGridChoiceProperties(XmlElement xe)
        {
            GridEndNumber = GetFieldIntValue("GridEndNum");
            GridNAOptionText = GetFieldAttributeValue("GridNATxt");
            GridStartNumber = GetFieldIntValue("GridStartNum");
            GridTextRangeAverage = GetFieldAttributeValue("GridTxtRng2");
            GridTextRangeHigh = GetFieldAttributeValue("GridTxtRng3");
            GridTextRangeLow = GetFieldAttributeValue("GridTxtRng1");

            SetMultiChoiceProperties(xe);
        }

        private void SetTextProperties(XmlElement xe)
        {
            DifferencingLimit = GetIntFromAttribute(xe, "DifferencingLimit", 0x5dc);
            MaxLength = GetIntFromAttribute(xe, "MaxLength", 0xff);
        }

        private int GetIntFromAttribute(XmlElement xe, string attributeName, int defalutValue)
        {
            int value = 0;
            if (xe.HasAttribute(attributeName))
            {
                if (Int32.TryParse(xe.GetAttribute(attributeName), out value))
                {
                    return value;
                }
            }
            return defalutValue;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues",
            Justification = "Ecb:Attribute of field property.")]
        private void SetFieldProperties(XmlElement xe, int lcid)
        {

            using (new AvePerformanceScope("Restore.AveXmlField.SetFieldProperties"))
            {


                ID = GetFieldGuidValue("ID");
                AggregationFunction = GetFieldAttributeValue("Aggregation");
                AllowDeletion = GetFieldBooleanValue("AllowDeletion");
                FromParent = GetFieldBooleanValue("FromParent");
                //mAllowDuplicateValues = mXmlElement.GetAttribute("AllowDuplicateValues") != "FALSE";
                DefaultFormula = GetSingleNodeValue("DefaultFormula", false);
                if (DefaultFormula.Equals(string.Empty))
                {
                    DefaultFormula = null;
                }
                DefaultValue = GetSingleNodeValue("Default", false);
                if (String.IsNullOrEmpty(DefaultValue))
                {
                    DefaultValue = null;
                }
                Description = GetFieldAttributeValueNotNull("Description");

                Direction = GetFieldAttributeValue("Direction");
                if (String.IsNullOrEmpty(Direction))
                {
                    Direction = "none";
                }
                DisplaySize = GetFieldAttributeValue("DisplaySize");
                if (xe.HasAttribute("EcbMenuAllowed"))
                {
                    string fieldAttributeValue = xe.GetAttribute("EcbMenuAllowed");
                    if (fieldAttributeValue == "Prohibited")
                    {
                        EcbMenuAllowed = false;
                    }
                    else if (fieldAttributeValue == "Required")
                    {
                        EcbMenuAllowed = true;
                    }
                    else
                    {
                        EcbMenuAllowed = null;
                    }
                }
                if (EcbMenuAllowed.HasValue)
                {
                    EcbMenu = EcbMenuAllowed.Value;
                }
                else if (GetFieldAttributeValue("EcbMenu") == "TRUE")
                {
                    EcbMenu = true;
                }
                else
                {
                    EcbMenu = GuidHasEcbMenu(ID);
                }

                Group = GetFieldAttributeValue("Group");
                if (String.IsNullOrEmpty(Group))
                {
                    Group = AveSPResource.GetString(lcid, "CustomColumnsGroup");
                    //mGroup = SPResource.GetString("CustomColumnsGroup", new object[0]);
                }
                bool? hidden = GetFieldBooleanValue("Hidden");
                Hidden = hidden.HasValue ? hidden.Value : false;

                bool? fromBaseType = GetFieldBooleanValue("FromBaseType");
                FromBaseType = fromBaseType.HasValue ? fromBaseType.Value : false;

                IMEMode = GetFieldAttributeValue("IMEMode");
                if (lcid != 0x404 && lcid != 0x804 && lcid != 0xc04 && lcid != 0x1004 && lcid != 0x411 && lcid != 0x412)
                {
                    IMEMode = null;
                }

                Indexed = GetFieldBoolValue("Indexed");
                JumpToField = GetFieldAttributeValue("JumpTo");
                if (xe.HasAttribute("LinkToItemAllowed"))
                {
                    string fieldAttributeValue = xe.GetAttribute("LinkToItemAllowed");
                    if (fieldAttributeValue == "Prohibited")
                    {
                        LinkToItemAllowed = false;
                    }
                    else if (fieldAttributeValue == "Required")
                    {
                        LinkToItemAllowed = true;
                    }
                    else
                    {
                        LinkToItemAllowed = null;
                    }
                }
                LinkToItem = LinkToItemAllowed.HasValue ? LinkToItemAllowed.Value : GetFieldBoolValue("LinkToItem");
                NoCrawl = xe.GetAttribute("NoCrawl") == "TRUE";
                PIAttribute = GetFieldAttributeValue("PIAttribute");
                PITarget = GetFieldAttributeValue("PITarget");
                PrimaryPIAttribute = GetFieldAttributeValue("PrimaryPIAttribute");
                PrimaryPITarget = GetFieldAttributeValue("PrimaryPITarget");
                ReadOnlyField = GetFieldBoolValue("ReadOnly");
                RelatedField = GetFieldAttributeValue("RelatedField");
                Required = GetFieldBoolValue("Required");
                EnforceUniqueValues = GetFieldBoolValue("EnforceUniqueValues");
                Sealed = GetFieldBoolValue("Sealed");
                ShowInDisplayForm = GetFieldBooleanValue("ShowInDisplayForm");
                ShowInEditForm = GetFieldBooleanValue("ShowInEditForm");
                ShowInListSettings = GetFieldBooleanValue("ShowInListSettings");
                ShowInNewForm = GetFieldBooleanValue("ShowInNewForm");

                ShowInViewForms = GetFieldBooleanValue("ShowInViewForms");
                StaticName = GetFieldAttributeValue("StaticName");
                if (String.IsNullOrEmpty(StaticName))
                {
                    StaticName = FieldInternalName;
                }
                Title = GetFieldAttributeValue("DisplayName");
                if (!string.IsNullOrEmpty(Title) && Title.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    Title = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(Title, "core",
                                                                                                  (uint)
                                                                                                  CultureInfo.
                                                                                                      CurrentUICulture.
                                                                                                      LCID);
                }
                SourceTitle = Title;
                TranslationXml = GetSingleNodeValue("Translations", true);
                TypeAsString = GetFieldAttributeValue("Type");
                if (mXmlElement.HasAttribute("FieldBaseType"))
                {
                    mFieldBaseType = mXmlElement.GetAttribute("FieldBaseType");
                    mXmlElement.RemoveAttribute("FieldBaseType");
                }
                //there is a column called TranslationStateJobId in list 'Translation Status' which both guid and hidden are true
                if (TypeAsString == AveFieldType.Guid.ToString() && !hidden.HasValue && FieldInternalName.Equals("TranslationStateJobId", StringComparison.OrdinalIgnoreCase))
                {
                    Hidden = true;
                }
                ValidationFormula = HttpUtility.HtmlDecode(GetSingleNodeValue("Validation", false));
                if (ValidationFormula.Equals(String.Empty))
                {
                    ValidationFormula = null;
                }
                ValidationMessage = GetFieldAttributeValue("Validation", "Message");
                XPath = GetFieldAttributeValue("Node");

                ShowInVersionHistory = GetFieldBooleanValue("ShowInVersionHistory");
                AllowMultipleValues = GetFieldBoolValue("Mult");

                CalloutMenu = GetFieldBoolValue("CalloutMenu");
                JSLink = GetFieldAttributeValue("JSLink");
                SchemaVersion = GetFieldAttributeValue("SchemaVersion");

            }

        }

        private bool GuidHasEcbMenu(Guid fieldId)
        {
            if ((!(fieldId == AveBuiltInFieldId.URLwMenu) && !(fieldId == AveBuiltInFieldId.LinkTitle)) &&
                !(fieldId == AveBuiltInFieldId.LinkFilename))
            {
                return (fieldId == AveBuiltInFieldId.LinkDiscussionTitle);
            }
            return true;
        }

        private string GetFieldAttributeValue(string nodeName, string attribute)
        {
            var elment = mXmlElement.SelectSingleNode(nodeName) as XmlElement;
            if (elment != null)
            {
                if (elment.HasAttribute(attribute))
                {
                    return elment.GetAttribute(attribute);
                }
            }
            return null;
        }

        private Guid GetFieldGuidValue(string attribute)
        {
            if (mXmlElement.HasAttribute(attribute))
            {
                return new Guid(mXmlElement.GetAttribute(attribute));
            }
            return Guid.Empty;
        }

        private string GetSingleNodeValue(string nodeName, bool outXml)
        {
            XmlNode node = mXmlElement.SelectSingleNode(nodeName);
            if (node != null)
            {
                if (outXml)
                {
                    return node.OuterXml;
                }
                return node.InnerText;
            }
            return string.Empty;
        }

        private string GetFieldAttributeValue(string attribute)
        {
            if (mXmlElement.HasAttribute(attribute))
            {
                return mXmlElement.GetAttribute(attribute);
            }
            return null;
        }

        private string GetFieldAttributeValueNotNull(string attribute)
        {
            return mXmlElement.GetAttribute(attribute);
        }

        private bool GetFieldBoolValue(string attribute)
        {
            if (mXmlElement.HasAttribute(attribute))
            {
                return mXmlElement.GetAttribute(attribute) == "TRUE";
            }
            return false;
        }

        private bool? GetFieldBooleanValue(string attribute)
        {
            string dis = GetFieldAttributeValue(attribute);
            bool result;
            if (Boolean.TryParse(dis, out result))
            {
                return result;
            }
            return null;
        }

        private int GetFieldIntValue(string attribute)
        {
            return GetFieldIntValue(attribute, 0);
        }

        private int GetFieldIntValue(string attribute, int defaultValue)
        {
            if (mXmlElement.HasAttribute(attribute))
            {
                int tmp;
                if (int.TryParse(mXmlElement.GetAttribute(attribute), out tmp))
                {
                    return tmp;
                }
            }
            return defaultValue;
        }

        private AveFieldType GetFieldType(string fieldName)
        {
            if (dictType == null)
            {
                var hashtable = new Hashtable();
                foreach (AveFieldType type in Enum.GetValues(typeof(AveFieldType)))
                {
                    hashtable[type.ToString()] = type;
                }
                hashtable["LookupMulti"] = hashtable["Lookup"];
                hashtable["UserMulti"] = hashtable["User"];
                Interlocked.CompareExchange(ref dictType, hashtable, null);
            }
            object obj2 = dictType[fieldName];
            if (obj2 != null)
            {
                return (AveFieldType)obj2;
            }
            if (fieldName == "TaxonomyFieldType" || fieldName == "TaxonomyFieldTypeMulti")
            {
            }
            return AveFieldType.Invalid;
        }

        public void SetAllowMultipleValues(bool allow)
        {
            AllowMultipleValues = allow;
        }

        #region IAveField

        //private bool mAllowDuplicateValues = true;
        //private bool mSortable = true;

        //public bool Sortable
        //{
        //    get { return mSortable; }
        //}
        public Guid ID { get; private set; }

        public string AggregationFunction { get; private set; }

        public bool? AllowDeletion { get; private set; }

        //public bool AllowDuplicateValues
        //{
        //    get { return mAllowDuplicateValues; }
        //}

        public string DefaultFormula { get; private set; }

        public virtual string DefaultValue { get; set; }

        public string Description { get; set; }

        public string Direction { get; private set; }

        public string DisplaySize { get; private set; }

        public bool EcbMenu { get; private set; }

        public bool? EcbMenuAllowed { get; private set; }

        public string Group { get; set; }

        public bool Hidden { get; private set; }

        public string IMEMode { get; set; }

        public virtual bool Indexed { get; private set; }

        public string JumpToField { get; private set; }

        public bool LinkToItem { get; private set; }

        public bool? LinkToItemAllowed { get; private set; }

        public virtual bool NoCrawl { get; private set; }

        public string PIAttribute { get; private set; }

        public string PITarget { get; private set; }

        public string PrimaryPIAttribute { get; private set; }

        public string PrimaryPITarget { get; private set; }

        public bool ReadOnlyField { get; private set; }

        public string RelatedField { get; private set; }

        public bool Required { get; private set; }

        public bool Sealed { get; private set; }

        public bool? ShowInDisplayForm { get; private set; }

        public bool? ShowInEditForm { get; private set; }

        public bool? ShowInListSettings { get; private set; }

        public bool? ShowInNewForm { get; private set; }

        public bool? ShowInVersionHistory { get; set; }

        public bool? ShowInViewForms { get; private set; }

        public string StaticName { get; private set; }

        public string Title { get; set; }

        //这个title是从备份数据取出来的用来保存备份数据里的title，无论什么情况都不应该变。
        public string SourceTitle { get; set; }

        public string TranslationXml { get; private set; }

        public string TypeAsString { get; set; }
        //此属性记录column的base type，如客户一个自定义的field，继承lookup field，那么这个值就是lookup
        private string mFieldBaseType;
        public string FieldBaseType
        {
            get
            {
                return !String.IsNullOrEmpty(mFieldBaseType) ? mFieldBaseType : TypeAsString;
            }
        }

        public string ValidationFormula { get; private set; }

        public string ValidationMessage { get; private set; }

        public string XPath { get; private set; }

        //13 property
        public bool CalloutMenu { get; set; }

        public string SchemaVersion { get; private set; }

        public string JSLink { get; set; }
        #endregion

        #region SPFieldLookup

        public bool IsRelationship { get; private set; }

        public string LookupField { get; private set; }

        public string LookupList { get; set; }

        public string LookupWebId { get; set; }

        public bool LookupWebFound { get; set; }

        public bool PrependId { get; private set; }

        public string PrimaryFieldId { get; set; }

        public AveRelationshipDeleteBehavior RelationshipDeleteBehavior { get; private set; }

        public bool UnlimitedLengthInDocumentLibrary { get; private set; }

        public bool AllowMultipleValues { get; private set; }

        public bool CountRelated { get; private set; }

        public bool EnforceUniqueValues { get; set; }

        #region Ave Field

        public string AveLookupListID { get; private set; }

        public string AveLookupWebTitle { get; private set; }

        public string AveLookupListTitle { get; private set; }

        public string AveSourceType { get; private set; }

        public bool LookupNeedPostAction { get; set; }

        public AveUserResourceInfo TitleResource { get; set; }

        public AveUserResourceInfo DescriptionResource { get; set; }

        #endregion

        #endregion

        #region SPFieldDateTime

        public AveCalendarType CalendarType { get; private set; }

        public AveDateTimeFieldFormatType DisplayFormat { get; private set; }

        public AveSPDateTimeFieldFriendlyFormatType FriendlyDisplayFormat { get; private set; }

        #endregion

        #region SPFieldBoolean

        public string JumpToNoField { get; private set; }

        public string JumpToYesField { get; private set; }

        #endregion

        #region SPFieldChoice

        //Inherit SPFieldMultiChoice

        public string FillinChoiceJumpTo { get; private set; }

        public AveChoiceFormatType EditFormat { get; private set; }

        #endregion

        #region SPFieldCalculated

        public int CurrencyLocaleId { get; private set; }

        public AveDateTimeFieldFormatType DateFormat { get; private set; }

        public AveNumberFormatTypes DisplayFormatCalculated { get; private set; }

        public string Formula { get; set; }

        public AveFieldType OutputType { get; private set; }

        public bool ShowAsPercentage { get; private set; }

        #endregion

        #region SPFieldComputed

        public bool EnableLookup { get; private set; }

        #endregion

        #region SPFieldCurrency

        //inherit SPFieldNumber
        //CurrencyLocaleId

        #endregion

        #region SPFieldMultiChoice

        private StringCollection mChoices;

        public bool FillInChoice { get; private set; }

        public StringCollection Choices
        {
            get { return mChoices ?? (mChoices = new StringCollection()); }
            set
            {
                mChoices = value;
            }
        }

        #endregion

        #region SPFieldMultiLineText

        //UnlimitedLengthInDocumentLibrary
        //private SPPreviewValueSize mPreviewValueSize = SPPreviewValueSize.Small;

        public bool AllowHyperlink { get; private set; }

        public bool AppendOnly { get; private set; }

        public int DifferencingLimit { get; private set; }

        public bool IsolateStyles { get; private set; }

        public int NumberOfLines { get; private set; }

        //public SPPreviewValueSize PreviewValueSize
        //{
        //    get { return mPreviewValueSize; }
        //}

        public bool RestrictedMode { get; private set; }

        public bool RichText { get; private set; }

        public virtual AveRichTextMode RichTextMode { get; private set; }

        #endregion

        #region SPFieldNumber

        //ShowAsPercentage

        public AveNumberFormatTypes DisplayFormatNumber { get; private set; }

        public double MaximumValue { get; private set; }

        public double MinimumValue { get; private set; }

        #endregion

        #region SPFieldRatingScale

        public int GridEndNumber { get; private set; }

        public string GridNAOptionText { get; private set; }

        public int GridStartNumber { get; private set; }

        public string GridTextRangeAverage { get; private set; }

        public string GridTextRangeHigh { get; private set; }

        public string GridTextRangeLow { get; private set; }

        #endregion

        #region SPFieldText

        //DifferencingLimit

        public int MaxLength { get; private set; }

        #endregion

        #region SPFieldUrl

        public AveUrlFieldFormatType DisplayFormatUrl { get; private set; }

        #endregion

        #region SPFieldUser

        public bool AllowDisplay { get; private set; }

        public bool Presence { get; private set; }

        public int SelectionGroup { get; set; }

        public AveFieldUserSelectionMode SelectionMode { get; private set; }

        #endregion

        #region Customization

        public string Customization { get; private set; }

        public Hashtable CustomProperties { get; private set; }

        public bool CreateValuesInEditForm { get; private set; }

        public String FieldRefXml { get; private set; }
        private void SetCustomization()
        {
            foreach (XmlNode customNode in mXmlElement)
            {
                if (customNode is XmlElement)
                {
                    var customElement = customNode as XmlElement;
                    if (customElement.Name.Equals("Customization"))
                    {
                        Customization = customElement.OuterXml;

                        foreach (XmlNode elementNode in customElement.ChildNodes)
                        {
                            if (elementNode is XmlElement && elementNode.Name.Equals("ArrayOfProperty"))
                            {
                                var element = elementNode as XmlElement;
                                foreach (XmlNode propertyNode in element.ChildNodes)
                                {
                                    try
                                    {
                                        if (propertyNode is XmlElement && propertyNode.Name.Equals("Property"))
                                        {
                                            var propertyElement = propertyNode as XmlElement;
                                            string name = null;
                                            object value = null;
                                            XmlNodeList elements = propertyElement.GetElementsByTagName("Name");
                                            if (elements != null && elements.Count > 0)
                                            {
                                                var nameElement = (XmlElement)elements[0];
                                                name = nameElement.InnerText;
                                            }
                                            elements = propertyElement.GetElementsByTagName("Value");
                                            if (elements != null && elements.Count > 0)
                                            {
                                                var valueElement = (XmlElement)elements[0];
                                                string type = valueElement.GetAttribute("p4:type");
                                                type =
                                                    type.Substring(type.IndexOf(":", StringComparison.OrdinalIgnoreCase) + 1);

                                                if (name.Equals("SspId") || name.Equals("GroupId") ||
                                                    name.Equals("TermSetId") || name.Equals("AnchorId"))
                                                {
                                                    string tValue = valueElement.InnerText;
                                                    if (tValue.Contains('|'))
                                                    {
                                                        string[] temp = tValue.Split('|');
                                                        if (temp.Length == 2)
                                                        {
                                                            CustomProperties.Add(name, valueElement.InnerText);
                                                            valueElement.InnerText = temp[0];
                                                            continue;
                                                        }
                                                    }
                                                }
                                                switch (type)
                                                {
                                                    case "datetime":
                                                        value = Convert.ToDateTime(valueElement.InnerText);
                                                        break;
                                                    case "boolean":
                                                        value = Convert.ToBoolean(valueElement.InnerText);
                                                        break;
                                                    case "guid":
                                                        value = new Guid(valueElement.InnerText);
                                                        break;
                                                    case "int32":
                                                    case "int":
                                                        value = Convert.ToInt32(valueElement.InnerText);
                                                        break;
                                                    case "double":
                                                        value = Convert.ToDouble(valueElement.InnerText);
                                                        break;
                                                    default:
                                                        value = valueElement.InnerText;
                                                        break;
                                                }
                                            }
                                            if (!String.IsNullOrEmpty(name) && !CustomProperties.ContainsKey(name))
                                            {
                                                CustomProperties.Add(name, value);
                                            }
                                        }
                                    }
                                    catch (Exception e)
                                    {
                                        log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetCustomizationFailed, e);
                                    }
                                }
                                break;
                            }
                        }
                        break;
                    }
                }
            }
        }

        public object GetCustomerProperty(string name)
        {
            if (!String.IsNullOrEmpty(name) && CustomProperties.ContainsKey(name))
            {
                return CustomProperties[name];
            }
            return null;
        }

        #endregion
    }

    /// <summary>
    /// 这个类是为了存储当GetFieldValues传入的RowID是-1的时候，需要缓存一下LookupFields的信息，等Item还原之后，
    /// 在进行重置AveLookup那个Dictionary，为PostAction调用
    /// </summary>
    public class AveLookupFieldInfo
    {
        public Guid LookupListID { get; set; }
        public Guid LookupFieldID { get; set; }
        public object LookupFieldValue { get; set; }
        public int Version { get; set; }
    }

    public class AveUrlFieldInfo
    {
        public Guid FieldId { get; set; }
        public Guid SourceItemId { get; set; }
        public int Version { get; set; }
    }

    public class AveNintexFormDataFieldInfo
    {
        public string FormData { get; set; }
        public int Version { get; set; }
    }
    /// <summary>
    /// 缓存UD表中RelatedItems中的信息，在Item还原以后再添加到SiteMapping Cache中
    /// </summary>
    public class AveRelatedItemsInfo
    {
        public Guid WebId { get; set; }
        public Guid ListId { get; set; }
        public int Version { get; set; }
        public string Schema { get; set; }
    }
}