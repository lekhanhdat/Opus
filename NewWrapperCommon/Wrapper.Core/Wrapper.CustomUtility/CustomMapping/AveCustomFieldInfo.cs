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




namespace AvePoint.Wrapper.Common
{
    using System;
using System.Collections.Generic;
using System.Collections.Specialized;

    [Serializable]
    public class AveCustomFieldInfo
    {
        /// <summary>
        /// 一些Mapping，比如Excel mapping，找不到就不需要还原了
        /// </summary>
        public bool NeedSkipRestore { get; set; }

        public string Name { get; set; }  //displayName

        public string InternalName { get; set; }

        public AveFieldType Type { get; set; }

        public string TypeAsString { get; set; }

        public bool IsMulti { get; set; }

        public bool UseInternalOrDisplay { get; set; }

        [Obsolete("Please use CustomFieldType instead.")]
        public string CustomFieldTypeAsString
        {
            get
            {
                return this.CustomFieldType.ToString();
            }
            set
            {
                this.CustomFieldType = (AveCustomFieldType)Enum.Parse(typeof(AveCustomFieldType), value, true);
            }
        }

        public AveCustomFieldType CustomFieldType { get; set; }

        public AveFieldType SourceType { get; set; }
    }

    public enum AveCustomFieldType
    {
        SameType,
        ChangeToMetadata,
        ChangeToDestination,
        ChangeToLookup,
    }

    [Serializable]
    public class AveCustomChangeToDesInfo : AveCustomFieldInfo
    {
        public string SeparateChar { get; set; }
    }

    [Serializable]
    public class AveCustomMetadataFieldInfo : AveCustomFieldInfo
    {
        public string TermGroup { get; set; }

        public string TermSet { get; set; }

        public string SeparateChar { get; set; }

        public string Terms { get; set; }
    }

    [Serializable]
    public class AveCustomLookupFieldInfo : AveCustomFieldInfo
    {
        public string WebRelativeUrl { get; set; }

        public string ListTitle { get; set; }
        /// <summary>
        /// Field display name
        /// </summary>
        public string FieldName { get; set; }

        public string SeparateChar { get; set; }
    }

    [Serializable]
    public class AveCustomChoiceFieldInfo : AveCustomFieldInfo
    {
        public StringCollection Choices { get; set; }
    }

    [Serializable]
    public class AveCustomYesOrNoFieldInfo : AveCustomFieldInfo
    {
        public string DefaultValue { get; set; }
    }

    [Serializable]
    public class AveSourceFieldInfo
    {
        public string SourceInternalName { get; set; }
        public string SourceDisplayName { get; set; }
        public Guid SourceFieldId { get; set; }
        public AveFieldType SourceType { get; set; }
        public bool IsHidenOrReadOnly { get; set; }
        
        //Start version : 6.6
        public string SourceTypeAsString { get; set; }
        //Start version : 6.6    Note Column Setting
        public bool RichText { get; set; }
        //Start version : 6.6    URL Column Need
        public string SourceWebAppUrl { get; set; }
        //Start version : 6.6    URL Column Need
        public string SourceSiteUrl { get; set; }

        public AveSourceFieldInfo()
        {
            this.SourceTypeAsString = string.Empty;
        }
    }

    [Serializable]
    public class AveSourceFieldValueInfo
    {
        public AveSourceFieldInfo SourceFieldInfo { get; set; }
        public int SourceItemRowId { get; set; }
        public string SourceValue { get; set; }
        public string SplitString { get; set; }
        public Dictionary<int, string> SourceDataJunction { get; set; }
        public string SourceItemName { get; set; }
    }
}
