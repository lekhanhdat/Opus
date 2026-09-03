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
using System.Collections.Specialized;
using System.Globalization;
using System.Collections;
using System.Threading;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Resource;
using System.Diagnostics.CodeAnalysis;
using System.Web;

namespace AvePoint.Wrapper.Restore
{

    public class AveNintexFormDataFieldInfo
    {
        public string FormData { get; set; }
        public int Version { get; set; }
    }

    public class AveXmlField
    {
        //private int mRowOrdinal = 0;
        //private bool mFromBase = false;
        //private bool mReadOnly = false;
        //private bool mRequired = false;
        //private bool mComputed = false;
        //private string mDisplayname = "";
        private string mInternalName = "";
        //private string mAssoicatedList = "";
        //private ArrayList mCols;
        private AveFieldType mFieldType;
        protected static AveLogger log = AveLogger.GetInstance(typeof(AveXmlField));
        private XmlElement mXmlElement = null;
        private string mKeyName;
        private AveCustomFieldInfo mCustomFieldInfo;
        private bool mNeedSkipForCustom;

        /// <summary>
        /// Get the list field internal name.
        /// </summary>
        public string FieldInternalName
        {
            get { return mInternalName; }
        }
        public bool NeedSkipForCustom
        {
            get
            {
                return mNeedSkipForCustom;
            }
            set
            {
                mNeedSkipForCustom = value;
            }
        }
        public AveFieldType Type
        {
            get { return mFieldType; }
        }

        public AveCustomFieldInfo CustomFieldInfo
        {
            get
            {
                return mCustomFieldInfo;
            }
            set
            {
                mCustomFieldInfo = value;
            }
        }

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

        public AveXmlField(XmlElement xe, string keyName)
            : this(xe)
        {
            mKeyName = keyName;
        }

        public AveXmlField(XmlElement xe)
            : this(xe, 1033)
        { }

        public AveXmlField(XmlElement xe, int lcid)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveXmlField.Constructor"))
            {
#endif

                mXmlElement = xe;
                mInternalName = xe.Attributes["Name"].Value;

                SetFieldProperties(xe, lcid);

                mFieldType = GetFieldType(mTypeAsString);
                switch (mFieldType)
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
                        SetCalculatedProperties(xe);
                        break;
                    case AveFieldType.Computed:
                        this.mEnableLookup = GetFieldBoolValue("EnableLookup");
                        break;
                    case AveFieldType.Currency:
                        SetCurrencyProperties(xe);
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
                        mDisplayFormat_Url = xe.HasAttribute("Format") ? (AveUrlFieldFormatType)Enum.Parse(typeof(AveUrlFieldFormatType), xe.GetAttribute("Format")) : AveUrlFieldFormatType.Hyperlink;
                        break;
                    case AveFieldType.Invalid:
                        if (this.TypeAsString == "TaxonomyFieldType" || this.TypeAsString == "TaxonomyFieldTypeMulti")
                        {
                            SetCustomization();
                        }
                        break;
                    default:
                        break;
                }

                SetAveProperties();
                SetUserResourceProperties();
#if PerformanceLog
            }
#endif
        }

        public Dictionary<string, string> TitleResource { get; set; }
        public Dictionary<string, string> DescriptionResource { get; set; }

        private void SetUserResourceProperties()
        {
            XmlNode node = mXmlElement.SelectSingleNode(AveUserResourceConstants.RESOURCE_NODE);
            if (node != null)
            {
                TitleResource = GetMultiLangResourceProperties(AveUserResourceConstants.TITLE_RESOUCE_NODE, node);
                DescriptionResource = GetMultiLangResourceProperties(AveUserResourceConstants.DESCRIPTION_RESOUCE_NODE, node);

                mXmlElement.RemoveChild(node);
            }
        }

        private Dictionary<string, string> GetMultiLangResourceProperties(string name, XmlNode parentNode)
        {
            Dictionary<string, string> result = new Dictionary<string, string>();
            XmlNode node = parentNode.SelectSingleNode(name);
            if (node != null)
            {
                foreach (XmlAttribute attrib in node.Attributes)
                {
                    result.Add(attrib.Name, attrib.Value);
                }

                parentNode.RemoveChild(node);
            }
            return result;
        }
        private void SetLookupProperties(XmlElement xe)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveXmlField.SetLookupProperties"))
            {
#endif
                if (xe.HasAttribute("ShowField"))
                {
                    mLookupField = xe.GetAttribute("ShowField");//move localized logic to backup module
                }
                if (xe.HasAttribute("UnlimitedLengthInDocumentLibrary"))
                {
                    mUnlimitedLengthInDocumentLibrary = Convert.ToBoolean(xe.GetAttribute("UnlimitedLengthInDocumentLibrary"));
                }
                if (xe.HasAttribute("RelationshipDeleteBehavior"))
                {
                    mRelationshipDeleteBehavior = (AveRelationshipDeleteBehavior)Enum.Parse(typeof(AveRelationshipDeleteBehavior), xe.GetAttribute("RelationshipDeleteBehavior"));
                }
                //Set this property in SetFieldProperties
                //if (xe.HasAttribute("Mult"))
                //{
                //    mAllowMultipleValues = Convert.ToBoolean(xe.GetAttribute("Mult"));
                //}
                if (xe.HasAttribute("IsRelationship"))
                {
                    mIsRelationship = Convert.ToBoolean(xe.GetAttribute("IsRelationship"));
                }
                if (xe.HasAttribute("List"))
                {
                    mLookupList = xe.GetAttribute("List");
                }
                if (xe.HasAttribute("WebId"))
                {
                    mLookupWebId = xe.GetAttribute("WebId");
                }
                if (xe.HasAttribute("PrependId"))
                {
                    mPrependId = Convert.ToBoolean(xe.GetAttribute("PrependId"));
                }
                if (xe.HasAttribute("FieldRef"))
                {
                    mPrimaryFieldId = xe.GetAttribute("FieldRef");
                }
                if (xe.HasAttribute("CountRelated"))
                {
                    mCountRelated = Convert.ToBoolean(xe.GetAttribute("CountRelated"));
                }
#if PerformanceLog
            }
#endif
        }

        private void SetAveProperties()
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveXmlField.SetAveProperties"))
            {
#endif
                foreach (XmlElement element in mXmlElement.ChildNodes)
                {
                    if (element.Name == "AveFieldInfo")
                    {
                        if (element.HasAttribute("AveLookupListTitle"))
                        {
                            mAveLookupListTitle = element.GetAttribute("AveLookupListTitle");
                        }
                        if (element.HasAttribute("AveLookupWebTitle"))
                        {
                            mAveLookupWebTitle = element.GetAttribute("AveLookupWebTitle");
                        }
                        if (element.HasAttribute("AveSourceType"))
                        {
                            mAveSourceType = element.GetAttribute("AveSourceType");
                        }
                        if (element.HasAttribute("AveLookupListID"))
                        {
                            mAveLookupListID = element.GetAttribute("AveLookupListID");
                        }
                        // the IsRelationship  property is in childnode,we should get at here
                        if (element.HasAttribute("IsRelationship"))
                        {
                            mIsRelationship = Convert.ToBoolean(element.GetAttribute("IsRelationship"));
                        }
                        mXmlElement.RemoveChild(element);
                        break;
                    }
                }
#if PerformanceLog
            }
#endif
        }

        private void SetUserProperties(XmlElement xe)
        {
            mAllowDisplay = xe.GetAttribute("ForcedDisplay") != "***";

            if (xe.HasAttribute("Presence"))
            {
                mPresence = Convert.ToBoolean(xe.GetAttribute("Presence"));
            }
            else
            {
                mPresence = true;
            }
            if (xe.HasAttribute("UserSelectionScope"))
            {
                mSelectionGroup = Convert.ToInt32(xe.GetAttribute("UserSelectionScope"));
            }
            if (xe.HasAttribute("UserSelectionMode"))
            {
                mSelectionMode = (AveFieldUserSelectionMode)Enum.Parse(typeof(AveFieldUserSelectionMode), xe.GetAttribute("UserSelectionMode"));
            }

            SetLookupProperties(xe);
        }

        private void SetDateTimeProperties(XmlElement xe)
        {
            if (xe.HasAttribute("CalType"))
            {
                mCalendarType = (AveCalendarType)Convert.ToInt32(xe.GetAttribute("CalType"));
            }
            try
            {
                if (xe.HasAttribute("Format"))
                {
                    if (xe.GetAttribute("Format").Equals("DateOnly") || xe.GetAttribute("Format").Equals("DateTime"))
                    {
                        mDisplayFormat = (AveDateTimeFieldFormatType)Enum.Parse(typeof(AveDateTimeFieldFormatType), xe.GetAttribute("Format"));
                    }
                    else
                    {
                        //To do with TimeOnly、ISO8601、ISO8601Basic 和 ISO8601Gregorian。
                    }
                }
                if (xe.HasAttribute("FriendlyDisplayFormat"))
                {
                    mFriendlyDisplayFormat = (AveDateTimeFieldFriendlyFormatType)Enum.Parse(typeof(AveDateTimeFieldFriendlyFormatType), xe.GetAttribute("FriendlyDisplayFormat"));
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.WARN, string.Format("An error occurred while set DataTime property. format:{0}\n error message:{1}", xe.GetAttribute("Format"), e));
            }
        }

        private void SetBooleanProperties(XmlElement xe)
        {
            if (xe.HasAttribute("JumpToNo"))
            {
                mJumpToNoField = xe.GetAttribute("JumpToNo");
            }
            if (xe.HasAttribute("JumpToYes"))
            {
                mJumpToYesField = xe.GetAttribute("JumpToYes");
            }
        }

        private void SetNoteProperties(XmlElement xe)
        {
            if (xe.HasAttribute("UnlimitedLengthInDocumentLibrary"))
            {
                mUnlimitedLengthInDocumentLibrary = Convert.ToBoolean(xe.GetAttribute("UnlimitedLengthInDocumentLibrary"));
            }
            if (xe.HasAttribute("RichTextMode"))
            {
                mRichTextMode = (AveRichTextMode)Enum.Parse(typeof(AveRichTextMode), xe.GetAttribute("RichTextMode"), true);
            }
            if (xe.HasAttribute("RichText"))
            {
                mRichText = Convert.ToBoolean(xe.GetAttribute("RichText"));
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
            mAllowHyperlink = xe.GetAttribute("AllowHyperlink") == "TRUE";
            mAppendOnly = xe.GetAttribute("AppendOnly") == "TRUE";
            mDifferencingLimit = xe.HasAttribute("DifferencingLimit") ? Convert.ToInt32(xe.GetAttribute("DifferencingLimit")) : 0x5dc;
            mIsolateStyles = xe.GetAttribute("IsolateStyles") == "TRUE";
            mNumberOfLines = xe.HasAttribute("NumLines") ? Convert.ToInt32(xe.GetAttribute("NumLines")) : 6;
            if (mNumberOfLines >= 0x3e9 || mNumberOfLines <= 0)
            {
                mNumberOfLines = 6;
            }
        }

        private void SetMultiChoiceProperties(XmlElement xe)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveXmlField.SetMultiChoiceProperties"))
            {
#endif
                this.mFillInChoice = GetFieldBoolValue("FillInChoice");
                if (xe.HasChildNodes)
                {
                    foreach (XmlElement choicesElement in xe.ChildNodes)
                    {
                        if (choicesElement.Name == "CHOICES")
                        {
                            foreach (XmlElement choice in choicesElement.ChildNodes)
                            {
                                this.Choices.Add(choice.InnerText);
                            }
                            break;
                        }
                    }
                }
#if PerformanceLog
            }
#endif
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Fillin:Attribute of choice property.")]
        private void SetChoiceProperties(XmlElement xe)
        {
            mFillinChoiceJumpTo = GetFieldAttributeValue("JumpToFillinChoice");
            SetMultiChoiceProperties(xe);
            if (xe.HasAttribute("Format"))
            {
                mEditFormat = (AveChoiceFormatType)Enum.Parse(typeof(AveChoiceFormatType), xe.GetAttribute("Format"));
            }
        }

        private void SetNumberProperties(XmlElement xe)
        {
            mShowAsPercentage = GetFieldBoolValue("Percentage");
            try
            {
                if (xe.HasAttribute("Min"))
                {
                    mMinimumValue = Convert.ToDouble(xe.GetAttribute("Min"));
                }
                if (xe.HasAttribute("Max"))
                {
                    mMaximumValue = Convert.ToDouble(xe.GetAttribute("Max"));
                }
            }
            catch (Exception e)
            {
                log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.SetMinAndMaxValueFailed, e);
            }

            if (xe.HasAttribute("Decimals"))
            {
                int decimals = GetFieldIntValue("Decimals");
                if (decimals > 5 || decimals < -1)
                {
                    mDisplayFormat_Number = AveNumberFormatTypes.Automatic;
                }
                else
                {
                    mDisplayFormat_Number = (AveNumberFormatTypes)decimals;
                }
            }
        }

        private void SetCurrencyProperties(XmlElement xe)
        {
            if (xe.HasAttribute("LCID"))
            {
                mCurrencyLocaleId = GetFieldIntValue("LCID");
            }
            SetNumberProperties(xe);
        }

        private void SetCalculatedProperties(XmlElement xe)
        {
            if (xe.HasAttribute("LCID"))
            {
                mCurrencyLocaleId = GetFieldIntValue("LCID");
            }
            if (xe.HasAttribute("Format"))
            {
                mDateFormat =
                    (AveDateTimeFieldFormatType)
                    Enum.Parse(typeof(AveDateTimeFieldFormatType), GetFieldAttributeValue("Format"));
            }
            else
            {
                mDateFormat = AveDateTimeFieldFormatType.DateOnly;
            }
            //SAAS-10359添加如下attribute后会导致目的端还原结束后目的端web无法执行Save as template的操作，所以源端不再添加该attribute。
            //if (xe.HasAttribute("DateFormat"))
            //{
            //    mDateFormat = (AveDateTimeFieldFormatType)Enum.Parse(typeof(AveDateTimeFieldFormatType), GetFieldAttributeValue("DateFormat"));
            //}
            //if (xe.HasAttribute("Decimals"))  //client api 不支持SAAS-967
            //{
            //    int decimals = GetFieldIntValue("Decimals");
            //    if (decimals > 5 || decimals < -1)
            //    {
            //        mDisplayFormat_Calculated = AveNumberFormatTypes.Automatic;
            //    }
            //    else
            //    {
            //        mDisplayFormat_Calculated = (AveNumberFormatTypes)decimals;
            //    }
            //}
            mFormula = HttpUtility.HtmlDecode(GetSingleNodeValue("Formula", false));
            //SAAS-10359添加如下attribute后会导致目的端还原结束后目的端web无法执行Save as template的操作，所以源端不再添加该attribute。
            //if (xe.HasAttribute("Formula")) 
            //{
            //    mFormula = GetFieldAttributeValue("Formula");
            //}
            if (xe.HasAttribute("ResultType"))
            {
                mOutputType = GetFieldType(GetFieldAttributeValue("ResultType"));
            }
            //if (mOutputType == AveFieldType.Number && GetFieldBoolValue("Percentage"))  //client api 不支持SAAS-967
            //{
            //    mShowAsPercentage = true;
            //}
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Rng:Attribute of field property.")]
        private void SetGridChoiceProperties(XmlElement xe)
        {
            mGridEndNumber = GetFieldIntValue("GridEndNum");
            mGridNAOptionText = GetFieldAttributeValue("GridNATxt");
            mGridStartNumber = GetFieldIntValue("GridStartNum");
            mGridTextRangeAverage = GetFieldAttributeValue("GridTxtRng2");
            mGridTextRangeHigh = GetFieldAttributeValue("GridTxtRng3");
            mGridTextRangeLow = GetFieldAttributeValue("GridTxtRng1");

            SetMultiChoiceProperties(xe);
        }

        private void SetTextProperties(XmlElement xe)
        {
            mDifferencingLimit = xe.HasAttribute("DifferencingLimit") ? Convert.ToInt32(xe.GetAttribute("DifferencingLimit")) : 0x5dc;
            mMaxLength = xe.HasAttribute("MaxLength") ? Convert.ToInt32(xe.GetAttribute("MaxLength")) : 0xff;
        }


        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "Ecb:Attribute of field property.")]
        private void SetFieldProperties(XmlElement xe, int lcid)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Restore.AveXmlField.SetFieldProperties"))
            {
#endif

                mId = GetFieldGuidValue("ID");
                mAggregationFunction = GetFieldAttributeValue("Aggregation");
                mAllowDeletion = GetFieldBooleanValue("AllowDeletion");
                //mAllowDuplicateValues = mXmlElement.GetAttribute("AllowDuplicateValues") != "FALSE";
                mDefaultFormula = GetSingleNodeValue("DefaultFormula", false);
                if (mDefaultFormula.Equals(string.Empty))
                {
                    mDefaultFormula = null;
                }
                mDefaultValue = GetSingleNodeValue("Default", false);
                if (String.IsNullOrEmpty(mDefaultValue))
                {
                    mDefaultValue = null;
                }
                mDescription = GetFieldAttributeValueNotNull("Description");

                mDirection = GetFieldAttributeValue("Direction");
                if (String.IsNullOrEmpty(mDirection))
                {
                    mDirection = "none";
                }
                mDisplaySize = GetFieldAttributeValue("DisplaySize");
                if (xe.HasAttribute("EcbMenuAllowed"))
                {
                    string fieldAttributeValue = xe.GetAttribute("EcbMenuAllowed");
                    if (fieldAttributeValue == "Prohibited")
                    {
                        this.mEcbMenuAllowed = false;
                    }
                    else if (fieldAttributeValue == "Required")
                    {
                        this.mEcbMenuAllowed = true;
                    }
                    else
                    {
                        this.mEcbMenuAllowed = null;
                    }
                }
                if (mEcbMenuAllowed.HasValue)
                {
                    mEcbMenu = mEcbMenuAllowed.Value;
                }
                else if (GetFieldAttributeValue("EcbMenu") == "TRUE")
                {
                    mEcbMenu = true;
                }
                else
                {
                    mEcbMenu = GUIDHasEcbMenu(mId);
                }

                mGroup = GetFieldAttributeValue("Group");
                if (String.IsNullOrEmpty(mGroup))
                {
                    mGroup = AveSPResource.GetString(lcid, "CustomColumnsGroup");
                    //mGroup = SPResource.GetString("CustomColumnsGroup", new object[0]);
                }
                mHidden = GetFieldBoolValue("Hidden");

                mIMEMode = GetFieldAttributeValue("IMEMode");
                mIndexed = GetFieldBoolValue("Indexed");
                mJumpToField = GetFieldAttributeValue("JumpTo");
                if (xe.HasAttribute("LinkToItemAllowed"))
                {
                    string fieldAttributeValue = xe.GetAttribute("LinkToItemAllowed");
                    if (fieldAttributeValue == "Prohibited")
                    {
                        this.mLinkToItemAllowed = false;
                    }
                    else if (fieldAttributeValue == "Required")
                    {
                        this.mLinkToItemAllowed = true;
                    }
                    else
                    {
                        this.mLinkToItemAllowed = null;
                    }
                }
                if (mLinkToItemAllowed.HasValue)
                {
                    mLinkToItem = mLinkToItemAllowed.Value;
                }
                else
                {
                    mLinkToItem = GetFieldBoolValue("LinkToItem");
                }
                mNoCrawl = xe.GetAttribute("NoCrawl") == "TRUE";
                mPIAttribute = GetFieldAttributeValue("PIAttribute");
                mPITarget = GetFieldAttributeValue("PITarget");
                mPrimaryPIAttribute = GetFieldAttributeValue("PrimaryPIAttribute");
                mPrimaryPITarget = GetFieldAttributeValue("PrimaryPITarget");
                mReadOnlyField = GetFieldBoolValue("ReadOnly");
                mRelatedField = GetFieldAttributeValue("RelatedField");
                mRequired = GetFieldBoolValue("Required");
                mEnforceUniqueValues = GetFieldBoolValue("EnforceUniqueValues");
                mSealed = GetFieldBoolValue("Sealed");
                mShowInDisplayForm = GetFieldBooleanValue("ShowInDisplayForm");
                mShowInEditForm = GetFieldBooleanValue("ShowInEditForm");
                mShowInListSettings = GetFieldBooleanValue("ShowInListSettings");
                mShowInNewForm = GetFieldBooleanValue("ShowInNewForm");

                mShowInViewForms = GetFieldBooleanValue("ShowInViewForms");
                mStaticName = GetFieldAttributeValue("StaticName");
                if (String.IsNullOrEmpty(mStaticName))
                {
                    mStaticName = mInternalName;
                }
                mTitle = GetFieldAttributeValue("DisplayName");
                if (!string.IsNullOrEmpty(mTitle) && mTitle.StartsWith("$Resources", StringComparison.OrdinalIgnoreCase))
                {
                    mTitle = WrapperRuntime.CurrentContext.ModelFactory.Utility.GetLocalizedString(mTitle, "core", (uint)CultureInfo.CurrentUICulture.LCID);
                }
                SourceTitle = mTitle;
                mTranslationXml = GetSingleNodeValue("Translations", true);
                mTypeAsString = GetFieldAttributeValue("Type");
                if (mTypeAsString == AveFieldType.Guid.ToString())
                {
                    mHidden = true;
                }
                mValidationFormula = HttpUtility.HtmlDecode(GetSingleNodeValue("Validation", false));
                if (mValidationFormula.Equals(String.Empty))
                {
                    mValidationFormula = null;
                }
                mValidationMessage = GetFieldAttributeValue("Validation", "Message");
                mXPath = GetFieldAttributeValue("Node");

                mShowInVersionHistory = GetFieldBooleanValue("ShowInVersionHistory");
                mAllowMultipleValues = GetFieldBoolValue("Mult");
#if PerformanceLog
            }
#endif

        }

        private bool GUIDHasEcbMenu(Guid FieldId)
        {
            if ((!(FieldId == AveBuiltInFieldId.URLwMenu) && !(FieldId == AveBuiltInFieldId.LinkTitle)) && !(FieldId == AveBuiltInFieldId.LinkFilename))
            {
                return (FieldId == AveBuiltInFieldId.LinkDiscussionTitle);
            }
            return true;
        }

        private string GetFieldAttributeValue(string nodeName, string attribute)
        {
            XmlElement elment = mXmlElement.SelectSingleNode(nodeName) as XmlElement;
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
                else
                {
                    return node.InnerText;
                }
            }
            return string.Empty;
        }

        public string GetFieldAttributeValue(string attribute)
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
            if (dis == "TRUE")
            {
                return true;
            }
            else if (dis == "FALSE")
            {
                return false;
            }
            else
            {
                return null;
            }
        }

        private int GetFieldIntValue(string attribute)
        {
            if (mXmlElement.HasAttribute(attribute))
            {
                try
                {
                    return Convert.ToInt32(mXmlElement.GetAttribute(attribute));
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, WrapperRestoreResource.GetFieldIntValueFailed, e);
                }
            }
            return 0;
        }


        private Hashtable s_dictType = null;

        private AveFieldType GetFieldType(string fieldName)
        {
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
            object obj2 = s_dictType[fieldName];
            if (obj2 != null)
            {
                return (AveFieldType)obj2;
            }
            if (fieldName == "TaxonomyFieldType" || fieldName == "TaxonomyFieldTypeMulti")
            {

            }
            return AveFieldType.Invalid;
        }

        #region IAveField
        private Guid mId = Guid.Empty;

        //private bool mAllowDuplicateValues = true;
        //private bool mSortable = true;
        private string mAggregationFunction = null;
        private bool? mAllowDeletion = null;
        private string mDefaultFormula = null;
        private string mDefaultValue = null;
        private string mDescription = String.Empty;
        private string mDirection = "none";
        private string mDisplaySize = null;
        private bool mEcbMenu = false;
        private bool? mEcbMenuAllowed = null;
        private string mTypeAsString = null;
        private string mValidationFormula = null;
        private string mValidationMessage = null;
        private string mXPath = null;
        private bool mIndexed = false;
        private string mIMEMode = null;
        private bool mHidden = false;
        private string mGroup = null;
        private string mJumpToField = null;
        private bool mLinkToItem = false;
        private bool? mLinkToItemAllowed = null;
        private bool mNoCrawl = false;
        private string mPIAttribute = null;
        private string mPITarget = null;
        private string mPrimaryPIAttribute = null;
        private string mPrimaryPITarget = null;
        private bool mReadOnlyField = false;
        private string mRelatedField = null;
        private bool mRequired = false;
        private bool mSealed = false;
        private bool? mShowInDisplayForm = null;
        private bool? mShowInEditForm = null;
        private bool? mShowInListSettings = null;
        private bool? mShowInNewForm = null;
        private bool? mShowInVersionHistory = null;
        private bool? mShowInViewForms = null;
        private string mStaticName = null;
        private string mTitle = null;
        private string mTranslationXml = null;

        //public bool Sortable
        //{
        //    get { return mSortable; }
        //}
        public Guid ID
        {
            get { return mId; }
        }

        public string AggregationFunction
        {
            get { return mAggregationFunction; }
        }

        public bool? AllowDeletion
        {
            get { return mAllowDeletion; }
        }

        //public bool AllowDuplicateValues
        //{
        //    get { return mAllowDuplicateValues; }
        //}

        public string DefaultFormula
        {
            get { return mDefaultFormula; }
        }

        public virtual string DefaultValue
        {
            get { return mDefaultValue; }
        }

        public string Description
        {
            get { return mDescription; }
        }

        public string Direction
        {
            get { return mDirection; }
        }

        public string DisplaySize
        {
            get { return mDisplaySize; }
        }

        public bool EcbMenu
        {
            get { return mEcbMenu; }
        }

        public bool? EcbMenuAllowed
        {
            get { return mEcbMenuAllowed; }
        }

        public string Group
        {
            get { return mGroup; }
        }

        public bool Hidden
        {
            get { return mHidden; }
        }

        public string IMEMode
        {
            get { return mIMEMode; }
        }

        public virtual bool Indexed
        {
            get { return mIndexed; }
        }

        public string JumpToField
        {
            get { return mJumpToField; }
        }

        public bool LinkToItem
        {
            get { return mLinkToItem; }
        }

        public bool? LinkToItemAllowed
        {
            get { return mLinkToItemAllowed; }
        }

        public virtual bool NoCrawl
        {
            get { return mNoCrawl; }
        }

        public string PIAttribute
        {
            get { return mPIAttribute; }
        }

        public string PITarget
        {
            get { return mPITarget; }
        }

        public string PrimaryPIAttribute
        {
            get { return mPrimaryPIAttribute; }
        }

        public string PrimaryPITarget
        {
            get { return mPrimaryPITarget; }
        }

        public bool ReadOnlyField
        {
            get { return mReadOnlyField; }
        }

        public string RelatedField
        {
            get { return mRelatedField; }
        }

        public bool Required
        {
            get { return mRequired; }
        }

        public bool Sealed
        {
            get { return mSealed; }
        }

        public bool? ShowInDisplayForm
        {
            get { return mShowInDisplayForm; }
        }

        public bool? ShowInEditForm
        {
            get { return mShowInEditForm; }
        }

        public bool? ShowInListSettings
        {
            get { return mShowInListSettings; }
        }

        public bool? ShowInNewForm
        {
            get { return mShowInNewForm; }
        }

        public bool? ShowInVersionHistory
        {
            get { return mShowInVersionHistory; }
        }

        public bool? ShowInViewForms
        {
            get { return mShowInViewForms; }
        }

        public string StaticName
        {
            get { return mStaticName; }
        }

        public string Title
        {
            get { return mTitle; }
            set { mTitle = value; }
        }

        //这个title是从备份数据取出来的用来保存备份数据里的title，无论什么情况都不应该变。
        public string SourceTitle { get; set; }

        public string TranslationXml
        {
            get { return mTranslationXml; }
        }

        public string TypeAsString
        {
            get { return mTypeAsString; }
        }

        public string ValidationFormula
        {
            get { return mValidationFormula; }
        }

        public string ValidationMessage
        {
            get { return mValidationMessage; }
        }

        public string XPath
        {
            get { return mXPath; }
        }
        #endregion

        #region SPFieldLookup
        private bool mAllowMultipleValues = false;
        private bool mCountRelated = false;
        private bool mIsRelationship = false;
        //private bool mIsRelationship = true;
        private string mLookupField = null;
        private string mLookupList = null;
        private string mLookupWebId = null;
        private bool mPrependId = false;
        private string mPrimaryFieldId = null;
        private AveRelationshipDeleteBehavior mRelationshipDeleteBehavior = AveRelationshipDeleteBehavior.None;
        private bool mUnlimitedLengthInDocumentLibrary = false;
        private bool mEnforceUniqueValues;
        public bool IsRelationship
        {
            get { return mIsRelationship; }
        }

        public string LookupField
        {
            get { return mLookupField; }
        }

        public string LookupList
        {
            get { return mLookupList; }
        }

        public string LookupWebId
        {
            get { return mLookupWebId; }
        }

        public bool PrependId
        {
            get { return mPrependId; }
        }

        public string PrimaryFieldId
        {
            get { return mPrimaryFieldId; }
        }

        public AveRelationshipDeleteBehavior RelationshipDeleteBehavior
        {
            get { return mRelationshipDeleteBehavior; }
        }

        public bool UnlimitedLengthInDocumentLibrary
        {
            get { return mUnlimitedLengthInDocumentLibrary; }
        }

        public bool AllowMultipleValues
        {
            get { return mAllowMultipleValues; }
        }

        public bool CountRelated
        {
            get { return mCountRelated; }
        }

        public bool EnforceUniqueValues
        {
            get { return mEnforceUniqueValues; }
        }

        public string KeyName
        {
            get
            {
                if (string.IsNullOrEmpty(this.mKeyName))
                {
                    return this.FieldInternalName;
                }
                return mKeyName;
            }
        }

        #region Ave Field
        private string mAveLookupWebTitle;
        private string mAveLookupListTitle;
        private string mAveSourceType;
        private string mAveLookupListID;

        public string AveLookupListID
        {
            get { return mAveLookupListID; }
        }
        public string AveLookupWebTitle
        {
            get { return mAveLookupWebTitle; }
        }
        public string AveLookupListTitle
        {
            get { return mAveLookupListTitle; }
        }
        public string AveSourceType
        {
            get { return mAveSourceType; }
        }
        #endregion
        #endregion

        #region SPFieldDateTime
        private AveCalendarType mCalendarType = AveCalendarType.None;
        private AveDateTimeFieldFormatType mDisplayFormat = AveDateTimeFieldFormatType.DateTime;
        /// <summary>
        /// 默认是Unspecified，不是Disabled
        /// </summary>
        private AveDateTimeFieldFriendlyFormatType mFriendlyDisplayFormat = AveDateTimeFieldFriendlyFormatType.Unspecified;

        public AveCalendarType CalendarType
        {
            get { return mCalendarType; }
        }

        public AveDateTimeFieldFormatType DisplayFormat
        {
            get { return mDisplayFormat; }
        }
        public AveDateTimeFieldFriendlyFormatType FriendlyDisplayFormat
        {
            get { return mFriendlyDisplayFormat; }
        }
        #endregion

        #region SPFieldBoolean
        private string mJumpToNoField = null;
        private string mJumpToYesField = null;
        public string JumpToNoField
        {
            get { return mJumpToNoField; }
        }
        public string JumpToYesField
        {
            get { return mJumpToYesField; }
        }
        #endregion

        #region SPFieldChoice
        //Inherit SPFieldMultiChoice
        private string mFillinChoiceJumpTo = null;
        private AveChoiceFormatType mEditFormat = AveChoiceFormatType.Dropdown;

        public string FillinChoiceJumpTo
        {
            get { return mFillinChoiceJumpTo; }
        }
        public AveChoiceFormatType EditFormat
        {
            get { return mEditFormat; }
        }
        #endregion

        #region SPFieldCalculated
        private int mCurrencyLocaleId = -1;  // check number if is -1, set value as web local id.
        private AveDateTimeFieldFormatType mDateFormat = AveDateTimeFieldFormatType.DateTime;
        private AveNumberFormatTypes mDisplayFormat_Calculated = AveNumberFormatTypes.Automatic;
        private string mFormula = null;
        private AveFieldType mOutputType = AveFieldType.Text;
        private bool mShowAsPercentage = false;

        public int CurrencyLocaleId
        {
            get { return mCurrencyLocaleId; }
        }

        public AveDateTimeFieldFormatType DateFormat
        {
            get { return mDateFormat; }
        }

        public AveNumberFormatTypes DisplayFormat_Calculated
        {
            get { return mDisplayFormat_Calculated; }
        }

        public string Formula
        {
            get { return mFormula; }
            set { mFormula = value; }
        }

        public AveFieldType OutputType
        {
            get { return mOutputType; }
        }

        public bool ShowAsPercentage
        {
            get { return mShowAsPercentage; }
        }
        #endregion

        #region SPFieldComputed
        private bool mEnableLookup = false;

        public bool EnableLookup
        {
            get { return mEnableLookup; }
        }
        #endregion

        #region SPFieldCurrency
        //inherit SPFieldNumber
        //CurrencyLocaleId
        #endregion

        #region SPFieldMultiChoice
        private bool mFillInChoice = false;
        private StringCollection mChoices = null;

        public bool FillInChoice
        {
            get { return mFillInChoice; }
        }

        public StringCollection Choices
        {
            get
            {
                if (mChoices == null)
                {
                    mChoices = new StringCollection();
                }
                return mChoices;
            }
        }
        #endregion

        #region SPFieldMultiLineText
        //UnlimitedLengthInDocumentLibrary
        private bool mAllowHyperlink = false;
        private bool mAppendOnly = false;
        private int mDifferencingLimit = 0;
        private bool mIsolateStyles = false;
        private int mNumberOfLines = 0;
        //private SPPreviewValueSize mPreviewValueSize = SPPreviewValueSize.Small;
        private bool mRestrictedMode = false;
        private bool mRichText = false;
        private AveRichTextMode mRichTextMode = AveRichTextMode.Compatible;

        public bool AllowHyperlink
        {
            get { return mAllowHyperlink; }
        }

        public bool AppendOnly
        {
            get { return mAppendOnly; }
        }

        public int DifferencingLimit
        {
            get { return mDifferencingLimit; }
        }

        public bool IsolateStyles
        {
            get { return mIsolateStyles; }
        }

        public int NumberOfLines
        {
            get { return mNumberOfLines; }
        }

        //public SPPreviewValueSize PreviewValueSize
        //{
        //    get { return mPreviewValueSize; }
        //}

        public bool RestrictedMode
        {
            get { return mRestrictedMode; }
        }

        public bool RichText
        {
            get { return mRichText; }
        }

        public virtual AveRichTextMode RichTextMode
        {
            get { return mRichTextMode; }
        }
        #endregion

        #region SPFieldNumber
        //ShowAsPercentage
        private AveNumberFormatTypes mDisplayFormat_Number = AveNumberFormatTypes.Automatic;
        private double mMaximumValue = double.MaxValue;
        private double mMinimumValue = double.MinValue;

        public AveNumberFormatTypes DisplayFormat_Number
        {
            get { return mDisplayFormat_Number; }
        }

        public double MaximumValue
        {
            get { return mMaximumValue; }
        }

        public double MinimumValue
        {
            get { return mMinimumValue; }
        }
        #endregion

        #region SPFieldRatingScale
        private int mGridEndNumber = 0;
        private string mGridNAOptionText = null;
        private int mGridStartNumber = 0;
        private string mGridTextRangeAverage = null;
        private string mGridTextRangeHigh = null;
        private string mGridTextRangeLow = null;

        public int GridEndNumber
        {
            get { return mGridEndNumber; }
        }

        public string GridNAOptionText
        {
            get { return mGridNAOptionText; }
        }
        public int GridStartNumber
        {
            get { return mGridStartNumber; }
        }

        public string GridTextRangeAverage
        {
            get { return mGridTextRangeAverage; }
        }

        public string GridTextRangeHigh
        {
            get { return mGridTextRangeHigh; }
        }

        public string GridTextRangeLow
        {
            get { return mGridTextRangeLow; }
        }

        #endregion

        #region SPFieldText
        //DifferencingLimit
        private int mMaxLength = 0;
        public int MaxLength
        {
            get { return mMaxLength; }
        }
        #endregion

        #region SPFieldUrl
        private AveUrlFieldFormatType mDisplayFormat_Url = AveUrlFieldFormatType.Hyperlink;

        public AveUrlFieldFormatType DisplayFormat_Url
        {
            get { return mDisplayFormat_Url; }
        }
        #endregion

        #region SPFieldUser
        private bool mAllowDisplay = true;
        private bool mPresence = true;
        private int mSelectionGroup = 0;
        private AveFieldUserSelectionMode mSelectionMode = AveFieldUserSelectionMode.PeopleAndGroups;

        public bool AllowDisplay
        {
            get { return mAllowDisplay; }
        }

        public bool Presence
        {
            get { return mPresence; }
        }

        public int SelectionGroup
        {
            get { return mSelectionGroup; }
        }

        public AveFieldUserSelectionMode SelectionMode
        {
            get { return mSelectionMode; }
        }
        #endregion

        #region Customization
        private string mCustomization;
        private Hashtable mCustomProperties = new Hashtable();

        public string Customization
        {
            get { return mCustomization; }
        }

        public Hashtable CustomProperties
        {
            get { return mCustomProperties; }
        }

        private void SetCustomization()
        {
            foreach (XmlElement customElement in mXmlElement)
            {
                if (customElement.Name.Equals("Customization"))
                {
                    mCustomization = customElement.OuterXml;

                    foreach (XmlElement element in customElement.ChildNodes)
                    {
                        if (element.Name.Equals("ArrayOfProperty"))
                        {
                            foreach (XmlElement propertyElement in element.ChildNodes)
                            {
                                try
                                {
                                    if (propertyElement.Name.Equals("Property"))
                                    {
                                        string name = null;
                                        object value = null;
                                        XmlNodeList elements = propertyElement.GetElementsByTagName("Name");
                                        if (elements != null && elements.Count > 0)
                                        {
                                            XmlElement nameElement = (XmlElement)elements[0];
                                            name = nameElement.InnerText;
                                        }
                                        elements = propertyElement.GetElementsByTagName("Value");
                                        if (elements != null && elements.Count > 0)
                                        {
                                            XmlElement valueElement = (XmlElement)elements[0];
                                            string text = valueElement.InnerText;
                                            string type = valueElement.GetAttribute("p4:type");
                                            if (string.IsNullOrEmpty(type))
                                            {
                                                type = valueElement.GetAttribute("p3:type");
                                            }
                                            type = type.Substring(type.IndexOf(":", StringComparison.OrdinalIgnoreCase) + 1);
                                            ArgumentNullException.ThrowIfNull(name);
                                            if (name.Equals("SspId") || name.Equals("GroupId") || name.Equals("TermSetId") || name.Equals("AnchorId"))
                                            {
                                                string tValue = valueElement.InnerText;
                                                if (tValue.Contains('|'))
                                                {
                                                    string[] temp = tValue.ToString().Split('|');
                                                    if (temp.Length == 2)
                                                    {
                                                        mCustomProperties.Add(name, valueElement.InnerText);
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
                                        if (!String.IsNullOrEmpty(name) && !mCustomProperties.ContainsKey(name))
                                        {
                                            mCustomProperties.Add(name, value);
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

        public object GetCustomerProperty(string name)
        {
            if (!String.IsNullOrEmpty(name) && mCustomProperties.ContainsKey(name))
            {
                return mCustomProperties[name];
            }
            return null;
        }
        #endregion
        public void SetAllowMultipleValues(bool allow)
        {
            mAllowMultipleValues = allow;
        }
    }
    //这个类是为了存储当GetFieldValues传入的RowID是-1的时候，需要缓存一下LookupFields的信息，等Item还原之后，
    //在进行重置AveLookup那个Dictionary，为PostAction调用
    public class AveLookupFieldInfo
    {
        public Guid LookupListID { get; set; }
        public Guid LookupFieldID { get; set; }
        public object LookupFieldValue { get; set; }
        public int Version { get; set; }
    }
}
