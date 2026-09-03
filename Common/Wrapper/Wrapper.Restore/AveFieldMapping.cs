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

namespace AvePoint.Wrapper.Restore
{
    //public class AveFieldMapping
    //{
    //    public Dictionary<string, AveFieldMappingInfo> AveFieldMappingInfosByName = new Dictionary<string, AveFieldMappingInfo>(StringComparer.OrdinalIgnoreCase);
    //    public Dictionary<string, AveFieldMappingInfo> AveFieldMappingInfosByDisplayName = new Dictionary<string, AveFieldMappingInfo>(StringComparer.OrdinalIgnoreCase);
    //    public Dictionary<string, AveFieldMappingInfo> AveMappedFields = new Dictionary<string, AveFieldMappingInfo>();
    //    public void Add(AveFieldMappingInfo aveFieldMappingInfo, bool replaceExist)
    //    {
    //        //AveFieldMappingInfos.Add(aveFieldMappingInfo);
    //        if (aveFieldMappingInfo == null)
    //        {
    //            return;
    //        }
    //        if (!string.IsNullOrEmpty(aveFieldMappingInfo.SourceName))
    //        {
    //            if (AveFieldMappingInfosByName.ContainsKey(aveFieldMappingInfo.SourceName) && !replaceExist)
    //            {
    //                return;
    //            }
    //            AveFieldMappingInfosByName[aveFieldMappingInfo.SourceName] = aveFieldMappingInfo;
    //        }
    //        else if (!string.IsNullOrEmpty(aveFieldMappingInfo.SourceDisplayName))
    //        {
    //            if (AveFieldMappingInfosByDisplayName.ContainsKey(aveFieldMappingInfo.SourceDisplayName) && !replaceExist)
    //            {
    //                return;
    //            }
    //            AveFieldMappingInfosByDisplayName[aveFieldMappingInfo.SourceDisplayName] = aveFieldMappingInfo;
    //        }
    //    }
    //    //匹配方式：优先匹配fieldName，fieldName不匹配再匹配fieldDisplayName。
    //    public AveFieldMappingInfo GetMappingFieldInfo(string fieldName, string fieldDisplayName)
    //    {
    //        if (!string.IsNullOrEmpty(fieldName))
    //        {
    //            if (AveFieldMappingInfosByName.ContainsKey(fieldName))
    //            {
    //                return AveFieldMappingInfosByName[fieldName];
    //            }
    //        }
    //        if (!string.IsNullOrEmpty(fieldDisplayName))
    //        {
    //            if (AveFieldMappingInfosByDisplayName.ContainsKey(fieldDisplayName))
    //            {
    //                return AveFieldMappingInfosByDisplayName[fieldDisplayName];
    //            }
    //        }
    //        return null;
    //    }
    //}

    //public class AveFieldMappingInfo
    //{
    //    public string SourceName;
    //    public string DestinationName;
    //    public bool IgnoreType;
    //    public string SourceDisplayName;
    //    public string DestinationDisplayName;
    //    public Dictionary<object, object> ValueMapping = new Dictionary<object, object>();

    //    //for text field mapping to taxonomy field
    //    public bool IsTaxonomyField = false;
    //    public string TermSetPath = string.Empty; //TermSetPath like "group;termset"
    //    public bool AllowMultiValue = false;
    //    public string SplitChar = ";";
    //    public string HiberarchyChar = "<";
    //    public AveFieldMappingInfo()
    //    {
    //    }
    //    public AveFieldMappingInfo(string sourceName, string destinationName, bool ignoreType, string sourceDisplayName, string destinationDisplayName)
    //    {
    //        SourceName = sourceName;
    //        DestinationName = destinationName;
    //        IgnoreType = ignoreType;
    //        SourceDisplayName = sourceDisplayName;
    //        DestinationDisplayName = destinationDisplayName;
    //    }
    //    public AveFieldMappingInfo(string sourceName, string destinationName, bool ignoreType, string sourceDisplayName, string destinationDisplayName, Dictionary<object, object> valueMapping)
    //    {
    //        SourceName = sourceName;
    //        DestinationName = destinationDisplayName;
    //        IgnoreType = ignoreType;
    //        SourceDisplayName = sourceDisplayName;
    //        DestinationDisplayName = destinationDisplayName;
    //        ValueMapping = valueMapping;
    //    }
    //    public void SetTaxonomyMapping(bool isTaxonomyField, string termSetPath, bool allowMultiValue, string splitChar)
    //    {
    //        IsTaxonomyField = isTaxonomyField;
    //        TermSetPath = termSetPath;
    //        AllowMultiValue = allowMultiValue;
    //        SplitChar = splitChar;
    //    }
    //    public void SetTaxonomyMapping(bool isTaxonomyField, string termSetPath, bool allowMultiValue, string splitChar, string hiherarchyChar)
    //    {
    //        IsTaxonomyField = isTaxonomyField;
    //        TermSetPath = termSetPath;
    //        AllowMultiValue = allowMultiValue;
    //        SplitChar = splitChar;
    //        HiberarchyChar = hiherarchyChar;
    //    }
    //    public void AddValueMapping(object sourceValue, object DestinationValue)
    //    {
    //        ValueMapping[sourceValue] = DestinationValue;
    //    }

    //    //过滤";", "<", "|", ">", "\t" 特殊字符
    //    public string FilterSpecialChars(string value)
    //    {
    //        string newValue = value;
    //        List<string> filterChares = new List<string>();
    //        filterChares.Add(";");
    //        filterChares.Add("<");
    //        filterChares.Add("|");
    //        filterChares.Add(">");
    //        filterChares.Add("\"");
    //        filterChares.Add("\t");

    //        if (filterChares.Contains(HiberarchyChar))
    //        {
    //            filterChares.Remove(HiberarchyChar);
    //        }

    //        string[] sp = new string[1] { SplitChar };
    //        //通过分割符分割成数组，这样也同时解决了，分割符是多个字符并包含特殊字符的情况
    //        string[] strs = value.ToString().Split(sp, StringSplitOptions.None);
    //        for (int i = 0; i < strs.Length; i++)
    //        {
    //            //提高内存缓冲效率
    //            StringBuilder tempStringBuilder = new StringBuilder(strs[i]);
    //            foreach (string filterChar in filterChares)
    //            {
    //                if (strs[i].Contains(filterChar))
    //                {
    //                    tempStringBuilder.Replace(filterChar, "");
    //                }
    //            }
    //            strs[i] = tempStringBuilder.ToString();
    //        }
    //        StringBuilder sb = new StringBuilder();
    //        for (int i = 0; i < strs.Length; i++)
    //        {
    //            if (strs[i] == "")
    //            {
    //                continue;
    //            }
    //            sb.Append(strs[i]);
    //            sb.Append(";");
    //        }
    //        newValue = sb.ToString().TrimEnd(';');

    //        //默认的层次分隔符是'<', 如果不是'<',在此替换成'<'
    //        if (HiberarchyChar != string.Empty && HiberarchyChar != "<")
    //        {
    //            newValue = newValue.ToString().Replace(HiberarchyChar, "<");
    //        }
    //        return newValue;
    //    }
    //}
}