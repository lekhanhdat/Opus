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
namespace RAGoogle.Util;

public class GoogleConstant
{
    #region Google MimeType 
    public const string GoogleFolder = "application/vnd.google-apps.folder";
    public const string GoogleShortcut = "application/vnd.google-apps.shortcut";
    public const string GoogleSitePage = "application/google-sites-page";
    public const string NoFileExtention = "application/octet-stream";
    public const string GoogleMP4 = "video/mp4";
    public const long DRIVE_FILE_SIZE_100MB = 100 * 1024 * 1024;
    public const long DRIVE_FILE_SIZE_10MB = 10 * 1024 * 1024;
    public const long MAX_RESUME_RETRIES = 3;


    public static Dictionary<string, string> GoogleExportMimeType = new(StringComparer.OrdinalIgnoreCase){
        { "application/vnd.google-apps.document", "application/vnd.openxmlformats-officedocument.wordprocessingml.document"},
        { "application/vnd.google-apps.jam", "application/pdf"},
        { "application/vnd.google-apps.spreadsheet", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
        { "application/vnd.google-apps.photo", "image/png" },
        { "application/vnd.google-apps.script", "application/vnd.google-apps.script+json" },
        { "application/vnd.google-apps.presentation", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
        { "application/vnd.google-apps.mail-layout", "text/plain" },
        { "application/vnd.google-apps.vid", "video/mp4"},
        { "application/vnd.google-apps.drawing", "image/png" }, //"image/svg+xml" change to png that download defalut file format is
    };

    public static Dictionary<string, string> GoogleSecondExportMimeType = new(StringComparer.OrdinalIgnoreCase){
        { "application/vnd.google-apps.document", "application/pdf"},
        { "application/vnd.google-apps.spreadsheet", "text/csv" },
        { "application/vnd.google-apps.script", "application/vnd.google-apps.script+json" },
        { "application/vnd.google-apps.presentation", "application/pdf" },
    };
    public static List<string> GoogleGASMimeType = [
       "application/vnd.google-apps.script"
       ];
    public static List<string> GoogleVideoMimeType = [
        "application/vnd.google-apps.video",
        "application/vnd.google-apps.vid"
        ];

    public static List<string> NotSupportedMimeType = [
        "application/vnd.google-apps.form",
        "application/vnd.google-apps.site",
        "application/vnd.google-apps.map"
        ];
    public static List<string> GoogleSupportVersion = [
        "application/vnd.google-apps.document",
        "application/vnd.google-apps.spreadsheet",
        "application/vnd.google-apps.presentation",
        "application/vnd.google-apps.drawing",
    ];

    public static List<string> UnsupportedRestoreMimeType = new List<string>
    {
            "application/vnd.google-apps.map",
            "application/vnd.google-apps.site",
            "application/vnd.google-apps.fusiontable",
            "application/vnd.google-apps.form",
            "application/vnd.google-apps.jam",
            "application/vnd.google-apps.drawing",
            "application/vnd.google-apps.shortcut",
            "application/vnd.google-apps.drive-sdk",
            "application/vnd.google-apps.vid",
            "application/vnd.google-apps.video",
    };

    #endregion

    #region Incremental activities

    public static readonly string DeleteActivityKey = "delete";
    public static readonly string UpdateActivityKey = "update";
    public static readonly string LabelChangeActivityKey = "labelChange";

    #endregion

    #region Activity parameters
    public static readonly string ActivityGDrive = "Audit.GoogleDrive";
    public static readonly string ActivityGAdmin = "Audit.GoogleAdmin";
    public static readonly string Parameter_old_value = "old_value";
    public static readonly string Parameter_new_value = "new_value";

    public static readonly string Parameter_doc_id = "doc_id";
    public static readonly string Parameter_doc_type = "doc_type";
    public static readonly string Parameter_doc_title = "doc_title";
    public static readonly string Parameter_shared_drive_id = "shared_drive_id";
    public static readonly string Parameter_team_drive_id = "team_drive_id";
    public static readonly string Parameter_originating_app_id = "originating_app_id";

    public static readonly string Parameter_membership_change_type = "membership_change_type";
    public static readonly string Parameter_target_user = "target_user";
    public static readonly string Parameter_added_role = "added_role";
    public static readonly string Parameter_removed_role = "removed_role";
    public static readonly string Parameter_owner_is_shared_drive = "owner_is_shared_drive";
    public static readonly string Parameter_new_owner_team_drive_id = "new_owner_team_drive_id";
    public static readonly string Parameter_new_owner_is_team_drive = "new_owner_is_team_drive";
    public static readonly string Parameter_new_owner = "new_owner";
    public static readonly string Parameter_owner = "owner";
    public static readonly string Parameter_owner_is_team_drive = "owner_is_team_drive";
    public static readonly string Parameter_owner_team_drive_id = "owner_team_drive_id";
    #endregion

    public const int LimitAppliedLabel = 5;
    
    public enum DiscoverJobType
    {
        None = 0,
        Full = 1,
        Incremental = 2
    }
    public enum JobAction
    {
        None = 0,
        Create = 1,
        Update = 2,
        Delete = 3,
    }
}
