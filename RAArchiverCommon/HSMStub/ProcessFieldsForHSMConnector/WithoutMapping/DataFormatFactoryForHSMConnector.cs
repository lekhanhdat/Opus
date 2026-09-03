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

using System.Collections.Generic;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore
{
    class DataFormatFactoryForHSMConnector
    {
        private static List<string> specialFieldName = new List<string>
        {
            "RoutingContentTypeInternal",
            "MasterSeriesItemID",
            "Target_x0020_Audiences",
            "Modified_x0020_By",
            "Created_x0020_By",
            "RoutingTargetPath",
            "TemplateUrl",
            "ViewGuid",
            "ContentType",
            "ProjectWebGuid",
            "ProjectParentWebGuid",
            "_SourceUrl",
            "FormData"
        };

        public static IDataFormatForHSMConnector CreateInstance(AveXmlField xmlField, IAveField destField, IAveListItem mItem, int originalVersion, int originalRowId, Dictionary<string, object> userData, AveObjectModelFactory aveObjectModelFactory)
        {
            if (specialFieldName.Contains(destField.InternalName))
            {
                return new SpecialNameDataFormatForHSMConnector(xmlField, destField, mItem, userData, originalVersion);
            }
            switch (xmlField.Type)
            {
                case AveFieldType.Boolean:
                case AveFieldType.CrossProjectLink:
                case AveFieldType.AllDayEvent:
                case AveFieldType.Recurrence:
                    return new BooleanDataFormatForHSMConnector(xmlField, destField, mItem);
                case AveFieldType.Guid:
                    return new GuidDataFormatForHSMConnector(xmlField, destField, mItem);
                case AveFieldType.Number:
                case AveFieldType.Currency:
                    return new NumberDataFormatForHSMConnector(xmlField, destField, mItem);
                case AveFieldType.DateTime:
                    return new DateTimeDataFormatForHSMConnector(xmlField, destField, mItem);
                case AveFieldType.User:
                    return new UserFieldDataFormatForHSMConnector(xmlField, destField, mItem);
                case AveFieldType.Note:
                    return new NoteDataFormatForHSMConnector(xmlField, destField, mItem, originalRowId);
                case AveFieldType.Choice:
                case AveFieldType.MultiChoice:
                case AveFieldType.OutcomeChoice:
                    return new ChoiceDataFormatForHSMConnector(xmlField, destField, mItem);
                case AveFieldType.Lookup:
                    return new HSARLookupDataFormatForHSMConnector(aveObjectModelFactory, xmlField, destField, mItem, originalVersion);
                case AveFieldType.URL:
                    object description;
                    if (userData.TryGetValue(xmlField.FieldInternalName + "#2", out description))
                    {
                        return new URLDataFormatForHSMConnector(aveObjectModelFactory, xmlField, destField, mItem, description.ToString(), originalVersion);
                    }
                    else
                    {
                        return new URLDataFormatForHSMConnector(aveObjectModelFactory, xmlField, destField, mItem, string.Empty, originalVersion);
                    }
                case AveFieldType.Geolocation:
                    return new GeolocationDataFormatForHSMConnector(xmlField, destField, mItem);
                case AveFieldType.Invalid:
                    switch (xmlField.TypeAsString)
                    {
                        case "TaxonomyFieldType":
                        case "TaxonomyFieldTypeMulti":
                            return new TaxonomyDataFormatForHSMConnector(xmlField, destField, mItem);
                        case "Link":
                        case "HTML":
                        case "SummaryLinks":
                        case "Image":
                            return new NoteDataFormatForHSMConnector(xmlField, destField, mItem, originalRowId);
                        case "PublishingScheduleStartDateFieldType":
                        case "PublishingScheduleEndDateFieldType":
                            return new DateTimeDataFormatForHSMConnector(xmlField, destField, mItem);
                        case "AverageRating":
                        case "Likes":
                        case "RatingCount":
                            return new NumberDataFormatForHSMConnector(xmlField, destField, mItem);
                        case "ChannelAliasFieldType":
                            return new BaseDataFormatForHSMConnector(xmlField, destField, mItem);
                        case "Facilities":
                            return new LookupDataFormatForHSMConnector(aveObjectModelFactory, xmlField, destField, mItem, originalVersion);
                        case "SendTo":
                            return new UserFieldDataFormatForHSMConnector(xmlField, destField, mItem);
                        default:
                            return new BaseDataFormatForHSMConnector(xmlField, destField, mItem);
                    }
                default:
                    {
                        return new BaseDataFormatForHSMConnector(xmlField, destField, mItem);
                    }
            }
        }
    }
}