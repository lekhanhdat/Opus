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
namespace Microsoft365.Graph.Extensions;

/// <summary>
/// Provides extension methods for Graph Beta models.
/// </summary>
public static partial class ModelExtensions
{

    public static bool IsFolder(this ListItem item)
    {
        return item.ContentType.EnsureIfNotNull().Id.IsChildrenOf("0x0120");
    }

    /// <summary>
    /// Determines whether the specified ListItem has attachments.
    /// <param name="item">The ListItem to check for attachments.</param>
    /// <returns>true if the ListItem has attachments; otherwise, false.</returns>
    public static bool HasAttachments(this ListItem item)
    {
        return item.TryGetFieldValue("Attachments", out bool b) && b;
    }

    /// <summary>
    /// Tries to get the value of a specified field from a ListItem and cast it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to which the field value should be cast.</typeparam>
    /// <param name="item">The ListItem from which to get the field value.</param>
    /// <param name="fieldName">The name of the field whose value is to be retrieved.</param>
    /// <param name="v">When this method returns, contains the value of the specified field, if the field is found and can be cast to the specified type; otherwise, the default value for the type of the v parameter.</param>
    /// <returns>true if the field is found and can be cast to the specified type; otherwise, false.</returns>
    public static bool TryGetFieldValue<T>(this ListItem item, string fieldName, out T? v)
    {
        if (item.TryGetFieldValue(fieldName, out object? obj) && obj is T value)
        {
            v = value;
            return true;
        }
        v = default;
        return false;
    }

    internal static bool TryGetFieldValue(this ListItem item, string fieldName, out object? obj)
    {
        item.Fields.EnsureIfNotNull();
        return item.Fields.AdditionalData.TryGetValue(fieldName, out obj);
    }

    private static bool IsChildrenOf(this string? ctId1, string? ctId2)
    {
        if (ctId1 is null || ctId2 is null) return false;
        return ctId1.StartsWith(ctId2);
    }
    /// <summary>
    /// It is contenttype.ColumnsPositions for now, and may be changed to contenttype.Columns in the future.
    /// </summary>
    /// <param name="contentType">content type</param>
    /// <returns></returns>
    public static List<ColumnDefinition> Columns(this ContentType contentType)
    {
        return contentType.ColumnPositions.EnsureIfNotNull();
    }


    public static string? Title(this ListItem item)
    {
        if (item.AdditionalData.TryGetValue("Title", out var title))
        {
            return title.ToString();
        }
        return null;
    }

    /// <summary>
    /// Read only or hidden
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public static bool IsBuiltInColumn(this ColumnDefinition column)
    {
        return column.ReadOnly == true || "_Hidden".EqualsIgnoreCase(column.ColumnGroup) || column.Hidden == true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public static bool Supported(this ColumnDefinition column)
    {
        return column.Type() != null;
    }
    /// <summary>
    /// If column.Type is null, use the column type facet like Boolean, Text..
    /// </summary>
    /// <param name="column"></param>
    /// <returns></returns>
    public static ColumnTypes? Type(this ColumnDefinition column)
    {
        if (column.Type is not null) return column.Type;//list column does not return Type in response, only in contenttype/{id}/columns

        return column switch
        {
            { Boolean: not null } => ColumnTypes.Boolean,
            { Calculated: not null } => ColumnTypes.Calculated,
            { Choice: { } choice } when choice.AllowMultipleValues() => ColumnTypes.Multichoice,
            { Choice: not null } => ColumnTypes.Choice,
            { Currency: not null } => ColumnTypes.Currency,
            { DateTime: not null } => ColumnTypes.DateTime,
            //ColumnTypes.Location, column.Id=column2.FieldRef
            { Geolocation: not null } => ColumnTypes.Geolocation,//contenttype.Columns ONLY, Location: Coordinates
            { HyperlinkOrPicture: not null } => ColumnTypes.Url,//contenttype.Columns ONLY
            { Lookup: not null } => ColumnTypes.Lookup,//column.Lookup.PrimaryLookupColumnId
            { Number: not null } => ColumnTypes.Number,
            { PersonOrGroup: not null } => ColumnTypes.User,
            { Term.AllowMultipleValues: true } => ColumnTypes.Multiterm,//contenttype.Columns ONLY
            { Term: not null } => ColumnTypes.Term,//contenttype.Columns ONLY
            { Text.AllowMultipleLines: true } => ColumnTypes.Note,
            { Text: not null } => ColumnTypes.Text,
            { Thumbnail: not null } => ColumnTypes.Thumbnail,//image, contenttype.Columns ONLY
            { ContentApprovalStatus: not null } => ColumnTypes.ApprovalStatus,
            _ => null,
        };
    }

    public static bool IsPersonAndGroup(this ColumnDefinition column)
    {
        return column.PersonOrGroup is not null;
    }

    internal static bool AllowMultipleValues(this ChoiceColumn column)
    {
        return "checkBoxes".EqualsIgnoreCase(column.DisplayAs);
    }
    public static bool IsDeleted(this ListItem item)
    {
        //TODO: return item.Deleted != null;//beta
        throw new NotImplementedException();
    }
    public static string? WebUrlDecoded(this BaseItem item)
    {
        return item.WebUrl.UriDecode();
    }

    public static string SiteId(this BaseItem item)
    {
        return item.ParentReference.EnsureIfNotNull().SiteId.EnsureIfNotNull();
    }

    public static string? SiteUrlDecoded(this SharepointIds ids)
    {
        return ids.SiteUrl.UriDecode();
    }
}