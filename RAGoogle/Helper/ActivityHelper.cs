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
using RAGoogle.Models.Enums;

namespace RAGoogle.Helper;

public static class DeleteActivityFamily
{
    private static readonly HashSet<ActivityType> types = [
        ActivityType.delete,
        ActivityType.remove_from_folder,
        ActivityType.trash,
    ];

    public static bool Contains(ActivityType type)
    {
        return types.Contains(type);
    }

    public static bool Contains(string type)
    {
        if (Enum.TryParse<ActivityType>(type, true, out var activityType))
        {
            // Check if the parsed enum value is in the types set
            return types.Contains(activityType);
        }
        return false;
    }
}

public static class UpdateActivityFamily
{
    private static readonly HashSet<ActivityType> types = [
        ActivityType.move,
        ActivityType.rename,
        ActivityType.untrash,
        ActivityType.create,
        ActivityType.edit,
        ActivityType.upload,
        ActivityType.copy,
        ActivityType.add_to_folder,
    ];

    public static bool Contains(ActivityType type)
    {
        return types.Contains(type);
    }

    public static bool Contains(string type)
    {
        if (Enum.TryParse<ActivityType>(type, true, out var activityType))
        {
            // Check if the parsed enum value is in the types set
            return types.Contains(activityType);
        }
        return false;
    }
}

public static class CreateActivityFamily
{
    private static readonly HashSet<ActivityType> types = [
        ActivityType.untrash,
        ActivityType.create,
        ActivityType.upload,
        ActivityType.copy,
        ActivityType.add_to_folder,
        ActivityType.rename,
    ];

    public static bool Contains(ActivityType type)
    {
        return types.Contains(type);
    }

    public static bool Contains(string type)
    {
        if (Enum.TryParse<ActivityType>(type, true, out var activityType))
        {
            // Check if the parsed enum value is in the types set
            return types.Contains(activityType);
        }
        return false;
    }
}

public static class HandleActivityFamily
{
    private static readonly HashSet<ActivityType> types = [
        ActivityType.untrash,
        ActivityType.create,
        ActivityType.upload,
        ActivityType.copy,
        ActivityType.add_to_folder,
        ActivityType.rename,
        ActivityType.label_added,
        ActivityType.label_removed,
        ActivityType.label_added_by_item_create,
        ActivityType.move,
        ActivityType.edit,
        ActivityType.delete,
        ActivityType.remove_from_folder,
        ActivityType.trash,
    ];

    public static bool Contains(ActivityType type)
    {
        return types.Contains(type);
    }

    public static bool Contains(string type)
    {
        if (Enum.TryParse<ActivityType>(type, true, out var activityType))
        {
            // Check if the parsed enum value is in the types set
            return types.Contains(activityType);
        }
        return false;
    }
}

public static class LabelChangeActivityFamily
{
    private static readonly HashSet<ActivityType> types = [
        ActivityType.label_added,
        ActivityType.label_added_by_item_create,
        ActivityType.label_field_changed,
        ActivityType.label_removed
    ];

    public static bool Contains(ActivityType type)
    {
        return types.Contains(type);
    }

    public static bool Contains(string type)
    {
        if (Enum.TryParse<ActivityType>(type, true, out var activityType))
        {
            // Check if the parsed enum value is in the types set
            return types.Contains(activityType);
        }
        return false;
    }
}


