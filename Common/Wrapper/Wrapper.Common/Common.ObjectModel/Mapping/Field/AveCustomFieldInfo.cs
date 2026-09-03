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
    using System.Linq;
    using System.Text;


    public class AveCustomFieldInfo
    {
        public string Name { get; set; }  //displayName

        public string InternalName { get; set; }

        public AveFieldType Type { get; set; }

        public string TypeAsString { get; set; }

        public bool IsMulti { get; set; }

        public bool UseInternalOrDisplay { get; set;}

        public string CustomFieldTypeAsString { get; set; }

        public AveFieldType SourceType { get; set; }
    }

    public class AveCustomMetadataFieldInfo : AveCustomFieldInfo
    {
        public string TermGroup { get; set; }

        public string TermSet { get; set; }

        public string SeparateChar { get; set; }

        public string Terms { get; set; }
    }

    public class AveCustomLookupFieldInfo : AveCustomFieldInfo
    {
        public string WebRelativeUrl { get; set; }

        public string ListTitle { get; set; }

        public string FieldName { get; set; }

        public string SeparateChar { get; set; }
    }

    public class AveCustomChoiceFieldInfo : AveCustomFieldInfo
    {
        public string Choices { get; set; }
    }

    public class AveCustomYesOrNoFieldInfo : AveCustomFieldInfo
    {
        public string DefaultValue { get; set; }
    }

    public class AveSourceFieldInfo
    {
        public string SourceInternalName { get; set; }
        public string SourceDisplayName { get; set; }
        public Guid SourceFieldId { get; set; }
        public AveFieldType SourceType { get; set; }
    }

    public class AveSourceFieldValueInfo
    {
        public AveSourceFieldInfo SourceFieldInfo { get; set; }
        public int SourceItemRowId { get; set; }
        public string SourceValue { get; set; }
        public string SourceItemName { get; set; }
    }
}
