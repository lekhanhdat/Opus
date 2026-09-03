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
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;
using System.Xml;
using System.ComponentModel;
using AvePoint.Common;
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CodeReview;
using AvePoint.GCommon.Utility.I18N;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Mapping;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.GCommon.Utility.Exceptions.SharePoint;

namespace AvePoint.Wrapper.Restore
{
    class ValueConvertObjectFactory
    {
        public static IValueConvertObject CreateInstance(string sourceFieldName, IAveField destField, object sourceValue, AveSPItem mItem, int originalVersion, int originalRowId, Dictionary<string, object> userData,MetadataOption option)
        {
            switch (destField.Type)
            {
                case AveFieldType.Text:
                case AveFieldType.Computed:
                    return new BaseValueConvertObject(destField, mItem, originalRowId);
                case AveFieldType.Choice:
                case AveFieldType.MultiChoice:
                case AveFieldType.OutcomeChoice:
                    return new ChoiceValueConvertObject(destField, mItem, originalRowId);
                case AveFieldType.Number:
                case AveFieldType.Currency:
                    return new NumberValueConvertObject(destField, mItem, originalRowId);
                case AveFieldType.Boolean:
                    return new BooleanValueConvertObject(destField, mItem, originalRowId);
                case AveFieldType.Note:
                    return new NoteValueConvertObject(destField, mItem, originalRowId, sourceValue, sourceFieldName);
                case AveFieldType.DateTime:
                    return new DateTimeValueConvertObject(destField, mItem, originalRowId);
                case AveFieldType.Lookup:
                    if (option.isHSM)
                    {
                        return new HSMLookupValueConvertObject(destField as IAveFieldLookup, mItem, originalRowId, sourceValue, originalVersion, sourceFieldName);
                    }
                    return new LookupValueConvertObject(destField as IAveFieldLookup, mItem, originalRowId, sourceValue, originalVersion, sourceFieldName);
                case AveFieldType.User:
                    return new UserValueConvertObject(destField, mItem, originalRowId);
                case AveFieldType.URL:
                    object description;
                    if (userData.TryGetValue(sourceFieldName + "#2", out description))
                    {
                        return new URLValueConvertObject(destField, mItem, originalRowId, description.ToString(),originalVersion);
                    }
                    else
                    {
                        return new URLValueConvertObject(destField, mItem, originalRowId, string.Empty,originalVersion);
                    }
                case AveFieldType.Geolocation:
                    return new GeolocationValueConvertObject(destField, mItem, originalRowId);
                case AveFieldType.Invalid:
                    switch (destField.TypeAsString)
                    {
                        case "TaxonomyFieldType":
                        case "TaxonomyFieldTypeMulti":
                            return new TaxonomyValueConvertObject(destField, mItem, originalRowId);
                        case "HTML":
                            return new NoteValueConvertObject(destField, mItem, originalRowId, sourceValue, sourceFieldName);
                        case "Link":
                        case "SummaryLinks":
                        case "Image":
                        case "MediaFieldType":
                            return new LinkValueConvertObject(destField, mItem, originalRowId);
                        default:
                            return new BaseValueConvertObject(destField, mItem, originalRowId);
                    }
                default:
                    return new BaseValueConvertObject(destField, mItem, originalRowId);
            }
        }
    }
}
