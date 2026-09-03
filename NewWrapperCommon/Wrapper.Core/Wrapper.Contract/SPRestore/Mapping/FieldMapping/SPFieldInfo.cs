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

namespace AvePoint.Wrapper.Core.SPRestore.Mapping
{
    using System;
    using System.Collections.Generic;

    #region Field Info, used for args in IFieldMapping interface
    #region Basic FiledInfo
    /// <summary>
    /// Wrapper field properties used for column mapping.
    /// Only include the basic information that all the fields use, create derived class if wants to extend this class. Such as ChangeToLookup.
    /// </summary>
    public class SPFieldInfo
    {
        /// <summary>
        /// Get or set the display name for the field info, Microsoft.SharePoint.SPField.Title
        /// </summary>
        public string DisplayName { get; internal set; }
        /// <summary>
        /// Get or set the internal name for the field info, Microsoft.SharePoint.SPField.InternalName
        /// </summary>
        public string InternalName { get; internal set; }
        /// <summary>
        /// Get or set the type of the field info as a string value, Microsoft.SharePoint.SPField.TypeAsString
        /// todo:Oliver use Microsoft.SharePoint.SPField.Type instead?
        /// </summary>
        public string TypeAsString { get; internal set; }
    }
    #endregion

    #region Conditionable Field Info
    /// <summary>
    /// 支持通过condition匹配是否需要mapping
    /// </summary>
    public class SPConditionableFieldInfo : SPFieldInfo
    {
        /// <summary>
        /// Gets or sets a Boolean value that specifies whether the field is displayed in the list, Microsoft.SharePoint.SPField.Hidden
        /// </summary>
        public bool Hidden { get; internal set; }
        /// <summary>
        /// Gets or sets a Boolean value that specifies whether values in the field can be modified, Microsoft.SharePoint.SPField.ReadOnlyField
        /// </summary>
        public bool ReadOnlyField { get; internal set; }
        /// <summary>
        /// 原端备份出来的信息，用于比较是否符合条件去Mapping。
        /// 目前只有两个Level:WebFieldMappingConditionInfo(Web级别的Field使用)，ListFieldMappingConditionInfo(List级别Field使用)
        /// </summary>
        public MappingConditionInfo ConditionInfo { get; set; }
    }
    #endregion

    #region FieldInfo for different type
    /// <summary>
    /// Mapping之后的Metadata field info类
    /// </summary>
    public class SPMetadataFieldInfo : SPFieldInfo
    {
        /// <summary>
        /// 创建mms column的term group信息
        /// </summary>
        public string TermGroup { get; internal set; }

        /// <summary>
        /// 创建mms column的term set信息
        /// </summary>
        public string TermSet { get; internal set; }
        
        /// <summary>
        /// 用于分割mms column值的字符
        /// </summary>
        public string SeparateChar { get; internal set; }
        
        /// <summary>
        /// 创建mms column的term信息, 如果没有term则为string.Empty
        /// </summary>
        public string Terms { get; internal set; }

        /// <summary>
        /// mms column值是否允许多值
        /// </summary>
        public bool AllowMultiValue { get; internal set; }
    }

    /// <summary>
    /// Mapping之后的lookup field info类
    /// </summary>
    public class SPLookupFieldInfo : SPFieldInfo
    {
        /// <summary>
        /// 暂时没有用到 todo:oliver
        /// </summary>
        public string WebRelativeUrl { get; internal set; }

        /// <summary>
        /// 用于构造lookup column的list title
        /// </summary>
        public string ListTitle { get; internal set; }

        /// <summary>
        /// 用于构造lookup column的filed name
        /// </summary>
        public string FieldName { get; internal set; }

        /// <summary>
        /// 用于分割多值lookup column的字符
        /// </summary>
        public string SeparateChar { get; internal set; }

        /// <summary>
        /// lookup column值是否允许多值
        /// </summary>
        public bool AllowMultiValue { get; internal set; }
    }
    #endregion
    #endregion
}
