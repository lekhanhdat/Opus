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





namespace AvePoint.Wrapper.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using AvePoint.Wrapper.Common;
    using System.IO;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.Wrapper.Resource;

    public class AveCustomFieldMappingForExcel : IAveCustomFieldMapping
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private Dictionary<AveSourceFieldInfo, AveCustomFieldInfo> internalExcelFieldMapping;
        private Dictionary<int, Dictionary<string, string>> ItemsValues;
        private bool IfSetInternalExcelFieldMappingValue;
        public bool RestoreCustomFieldFromExcel { get; set; }
        public Dictionary<AveSourceFieldInfo, AveCustomFieldInfo> InternalExcelFieldMapping
        {
            get { return internalExcelFieldMapping; }
        }

        public Dictionary<string, object> NullToDefaultValueMapping
        {
            get { return new Dictionary<string, object>(); }
        }

        public AveCustomFieldMappingForExcel(string ExcelFolderPath)
        {
#if PerformanceLog
            using (AvePerformanceScope pc = new AvePerformanceScope("Wrapper.Mapping.AveCustomFieldMappingForExcel.Constructor"))
            {
#endif
                DirectoryInfo folderPath = new DirectoryInfo(ExcelFolderPath);
                internalExcelFieldMapping = new Dictionary<AveSourceFieldInfo, AveCustomFieldInfo>(new AveCustomFieldInfoEqualityComparer());
                ItemsValues = new Dictionary<int, Dictionary<string, string>>();
                //foreach (FileInfo file in folderPath.GetFiles())
                //{
                //    GetValuesFromExcel(file.DirectoryName + "\\" + file.Name);
                //}
#if PerformanceLog
            }
#endif

        }
        public void GetValuesFromExcel(string ExcelPath)
        {
#if PerformanceLog
            using (AvePerformanceScope pcd = new AvePerformanceScope("Wrapper.Mapping.AveCustomFieldMappingForExcel.GetValuesFromExcel"))
            {
#endif
                if (string.IsNullOrEmpty(ExcelPath))
                {
                    return;
                }
                RestoreCustomFieldFromExcel = false;
                FMExcelOpenXml mExcel = new FMExcelOpenXml();
                try
                {
                    mExcel.Open(ExcelPath);
                    while (true)
                    {
                        Dictionary<string, string> tempDic = mExcel.ReadLine();
                        if (!IfSetInternalExcelFieldMappingValue)
                        {
                            foreach (string key in tempDic.Keys)
                            {
                                IAveCustomFieldInExcel mField = new IAveCustomFieldInExcel();
                                mField.SetFieldInfo(key);
                                if (key.Equals("Path"))
                                {
                                    continue;
                                }
                                string realKey = key.Substring(0, key.IndexOf(":="));
                                AveSourceFieldInfo info = new AveSourceFieldInfo() { SourceDisplayName = realKey };
                                if (!this.internalExcelFieldMapping.ContainsKey(info))
                                {
                                    this.internalExcelFieldMapping.Add(info, mField.AfterFieldInfo);
                                }
                            }
                            IfSetInternalExcelFieldMappingValue = true;
                        }
                        if (tempDic == null)
                        {
                            break;
                        }
                        tempDic = SetFieldNameInfo(tempDic);
                        ItemsValues.Add(Convert.ToInt32(tempDic["ID"]), tempDic);
                    }
                }
                catch (Exception e)
                {
                    mLog.Log(AveLogLevel.ERROR, "When load Excel " + ExcelPath + " for custom field: " + e.ToString());
                }
                finally
                {
                    mExcel.Dispose();
                }
#if PerformanceLog
            }
#endif
        }
        internal Dictionary<string, string> SetFieldNameInfo(Dictionary<string, string> tempDic)
        {
            Dictionary<string, string> tpDic = new Dictionary<string, string>();
            foreach (string key in tempDic.Keys)
            {
                if (key.Contains(":="))
                {
                    if (!tpDic.ContainsKey(key.Substring(0, key.IndexOf(":="))))
                    {
                        tpDic.Add(key.Substring(0, key.IndexOf(":=")), tempDic[key]);
                    }
                }
            }
            tempDic = null;
            return tpDic;
        }

        public AveCustomFieldInfo GetMappingFieldBeforeAdd(AveSourceFieldInfo sourceFieldInfo)
        {
            if (internalExcelFieldMapping != null && internalExcelFieldMapping.ContainsKey(sourceFieldInfo))
            {
                return internalExcelFieldMapping[sourceFieldInfo];
            }
            else
            {
                return null;
            }
        }

        public string GetMappingValue(AveSourceFieldValueInfo sourceFieldValueInfo)
        {
            if (this.internalExcelFieldMapping != null && this.internalExcelFieldMapping.ContainsKey(sourceFieldValueInfo.SourceFieldInfo))
            {
                if (this.ItemsValues.ContainsKey(sourceFieldValueInfo.SourceItemRowId))
                {
                    return this.ItemsValues[sourceFieldValueInfo.SourceItemRowId][sourceFieldValueInfo.SourceFieldInfo.SourceDisplayName];
                }
            }
            return null;
        }

        #region 根据rowId判断item是否在excel表中
        public bool CheckItemExistInExcel(int rowId)
        {
            if (this.ItemsValues.ContainsKey(rowId))
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        #endregion

        public void Dispose()
        {
            if (internalExcelFieldMapping != null)
            {
                internalExcelFieldMapping = null;
            }
            if (ItemsValues != null)
            {
                ItemsValues = null;
            }
        }

        public Dictionary<string, object> GetNullToDefaultValueMapping()
        {
            return new Dictionary<string, object>();
        }
    }
    public class IAveCustomFieldInExcel
    {
        private static readonly AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveCustomFieldInfo mAfterFieldInfo;

        private string mFieldType;

        private Dictionary<string, string[]> mSplitDic;
        public string Path { get; set; }
        public string FieldType
        {
            get { return mFieldType; }
        }

        private bool mIsNormalField;

        public bool IsNormalField
        {
            get { return mIsNormalField; }
        }

        internal string[] ValueArray;

        public AveCustomFieldInfo AfterFieldInfo
        {
            get { return mAfterFieldInfo; }
        }

        internal void SetSplitValue()
        {
            //mSplitDic.Add("lookup", new string[] { "(", ")" });
            //mSplitDic.Add("taxonomy", new string[] { ":=", ":", ":", ":", ":" });
            //mSplitDic.Add("choice", new string[] { ":=", ":", ":" });
            //mSplitDic.Add("multipleText", new string[] { ":=", ":" });
            mSplitDic.Add("normal", new string[] { ":=" });
        }

        public IAveCustomFieldInExcel()
        {
            mSplitDic = new Dictionary<string, string[]>();
            this.SetSplitValue();
        }

        internal void SetCustomLookupFieldInfo(string modifiedInfo)
        {
            mAfterFieldInfo = new AveCustomLookupFieldInfo();
            ValueArray = modifiedInfo.Split(mSplitDic["normal"], StringSplitOptions.None);
            mAfterFieldInfo.Type = (AveFieldType)Enum.Parse(typeof(AveFieldType), ValueArray[1]);
            mAfterFieldInfo.Name = ValueArray[0];
            mAfterFieldInfo.TypeAsString = ValueArray[1];
            mIsNormalField = true;
        }

        internal void SetCustomMultiTextInfo(string modifiedInfo)
        {
            ValueArray = modifiedInfo.Split(mSplitDic["multipleText"], StringSplitOptions.None);
            mAfterFieldInfo = new AveCustomFieldInfo();
            mAfterFieldInfo.IsMulti = ValueArray[2].Equals("1") ? true : false;
        }

        internal void SetCustomMetadataFieldInfo(string modifiedInfo)
        {
            ValueArray = modifiedInfo.Split(mSplitDic["taxonomy"], StringSplitOptions.None);
            mAfterFieldInfo = new AveCustomMetadataFieldInfo();
            (mAfterFieldInfo as AveCustomMetadataFieldInfo).IsMulti = ValueArray[2].Equals("1") ? true : false;
            (mAfterFieldInfo as AveCustomMetadataFieldInfo).TermGroup = ValueArray[3];
            (mAfterFieldInfo as AveCustomMetadataFieldInfo).TermSet = ValueArray[4];
        }

        internal void SetCustomChoiceFieldInfo(string modifiedInfo)
        {
            ValueArray = modifiedInfo.Split(mSplitDic["choice"], StringSplitOptions.None);
            mAfterFieldInfo = new AveCustomChoiceFieldInfo();
            (mAfterFieldInfo as AveCustomChoiceFieldInfo).IsMulti = ValueArray[2].Equals("1") ? true : false;
            (mAfterFieldInfo as AveCustomChoiceFieldInfo).Choices = ValueArray[3];
        }

        internal void SetCustomNormalFieldInfo(string modifiedInfo)
        {
            ValueArray = modifiedInfo.Split(mSplitDic["normal"], StringSplitOptions.None);
            ValueArray[1] = FieldInternalTypeAndGuiTypeMapping.GetInternalTypeByGuiType(ValueArray[1]);
            mAfterFieldInfo = new AveCustomFieldInfo();
            try
            {
                #region 在Export导出的Excel表中，添加允许多值的column(如person and group类型)未转移到目的端，这里特殊处理一下
                if (ValueArray[1].Equals("UserMulti", StringComparison.OrdinalIgnoreCase))
                {
                    mAfterFieldInfo.Type = (AveFieldType)Enum.Parse(typeof(AveFieldType), "User");
                }
                else if (ValueArray[1].Equals("LookupMulti", StringComparison.OrdinalIgnoreCase))
                {
                    mAfterFieldInfo.Type = (AveFieldType)Enum.Parse(typeof(AveFieldType), "Lookup");
                }
                else if (ValueArray[1].Equals("TaxonomyFieldTypeMulti", StringComparison.OrdinalIgnoreCase))
                {
                    mAfterFieldInfo.Type = (AveFieldType)Enum.Parse(typeof(AveFieldType), "TaxonomyFieldType");
                }
                #endregion
                else
                {
                    mAfterFieldInfo.Type = (AveFieldType)Enum.Parse(typeof(AveFieldType), ValueArray[1]);
                }
            }
            catch(Exception e) 
            {
                log.Log(AveLogLevel.DEBUG, WrapperMappingResource.AWMConvertValueToFieldTypeError, e.ToString());
            }
            mAfterFieldInfo.Name = ValueArray[0];
            mAfterFieldInfo.TypeAsString = ValueArray[1];
            mIsNormalField = true;
        }

        public void SetFieldInfo(string value)
        {
            if (value.Contains(":="))
            {
                this.SetCustomNormalFieldInfo(value);
                //if (!value.Split(mSplitDic["normal"], StringSplitOptions.None)[1].StartsWith("Lookup"))
                //{
                //    #region //...
                //    ////Lookup
                //    //if (modifiedInfo.Substring(modifiedInfo.IndexOf(":=") + 2).StartsWith("lookup"))
                //    //{
                //    //    this.SetCustomLookupFieldInfo(modifiedInfo);
                //    //}
                //    ////Metadata
                //    //else if (modifiedInfo.Substring(modifiedInfo.IndexOf(":=") + 2).StartsWith("taxonomy"))
                //    //{
                //    //    this.SetCustomMetadataFieldInfo(modifiedInfo);
                //    //}
                //    ////Choice
                //    //else if (modifiedInfo.Substring(modifiedInfo.IndexOf(":=") + 2).StartsWith("choice"))
                //    //{
                //    //    this.SetCustomChoiceFieldInfo(modifiedInfo);
                //    //}
                //    ////MultipleText
                //    //else if (modifiedInfo.Substring(modifiedInfo.IndexOf(":=") + 2).StartsWith("multipleText"))
                //    //{
                //    //    this.SetCustomMultiTextInfo(modifiedInfo);
                //    //}
                //    #endregion
                //    this.SetCustomNormalFieldInfo(value);
                //}
                //else if (value.Split(mSplitDic["normal"], StringSplitOptions.None)[1].StartsWith("Lookup"))
                //{
                //    this.SetCustomLookupFieldInfo(value);
                //}
            }
            else if (value.Equals("Path"))
            {
                Path = value;
            }
        }
    }
    public class FieldInternalTypeAndGuiTypeMapping
    {
        private static Dictionary<string, string> typeMappings = new Dictionary<string, string>();
        private static void InitializeTypeMappings()
        {
            typeMappings.Add("Single line of text", "Text");
            typeMappings.Add("Multiple lines of text", "Note");
            typeMappings.Add("Choice (menu to choose from)", "Choice");
            typeMappings.Add("Choice (menu to choose from)_AllowMultiple", "MultiChoice");
            typeMappings.Add("Number (1, 1.0, 100)", "Number");
            typeMappings.Add("Currency ($, ¥, €)", "Currency");
            typeMappings.Add("Date and Time", "DateTime");
            typeMappings.Add("Lookup (information already on this site)", "Lookup");
            typeMappings.Add("Lookup (information already on this site)_AllowMultiple", "LookupMulti");
            typeMappings.Add("Yes/No (check box)", "Boolean");
            typeMappings.Add("Person or Group", "User");
            typeMappings.Add("Person or Group_AllowMultiple", "UserMulti");
            typeMappings.Add("Hyperlink or Picture", "URL");
            typeMappings.Add("Calculated (calculation based on other columns)", "Calculated");
            typeMappings.Add("Managed Metadata", "TaxonomyFieldType");
            typeMappings.Add("Managed Metadata_AllowMultiple", "TaxonomyFieldTypeMulti");
        }
        public static string GetInternalTypeByGuiType(string guiType)
        {
            if (typeMappings.Count == 0)
            {
                InitializeTypeMappings();
            }
            if (typeMappings.ContainsKey(guiType))
            {
                return typeMappings[guiType];
            }
            else
            {
                return guiType;
            }
        }
    }
}
