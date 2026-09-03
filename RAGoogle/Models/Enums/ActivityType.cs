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
namespace RAGoogle.Models.Enums;

public enum ActivityType
{
    #region GDrive type = access
    deny_access_request = 1,
    expire_access_request = 2,
    request_access = 3,
    add_to_folder = 4,
    appeal_abuse_violation = 5,
    approval_canceled = 6,
    approval_comment_added = 7,
    approval_completed = 8,
    approval_decisions_reset = 9,
    approval_due_time_change = 10,
    approval_requested = 11,
    approval_reviewer_change = 12,
    approval_reviewer_responded = 13,
    create_comment = 14,
    delete_comment = 15,
    edit_comment = 16,
    reassign_comment = 17,
    reopen_comment = 18,
    resolve_comment = 19,
    connected_sheets_query = 20,
    copy = 21,
    create = 22,
    delete = 23,
    download = 24,
    email_as_attachment = 25,
    edit = 26,
    email_collaborators = 27,
    cancel_esignature = 28,
    complete_esignature = 29,
    request_esignature = 30,
    review_esignature = 31,
    download_forms_response = 32,
    access_item_content = 33,
    label_added = 34,
    label_added_by_item_create = 35,
    label_field_changed = 36,
    label_removed = 37,
    add_lock = 38,
    move = 39,
    preview = 40,
    print = 41,
    remove_from_folder = 42,
    rename = 43,
    report_abuse = 44,
    untrash = 45,
    delete_revision = 46,
    pin_revision = 47,
    unpin_revision = 48,
    create_script_trigger = 49,
    delete_script_trigger = 50,
    sheets_import_url = 51,
    sheets_import_range = 52,
    source_copy = 53,
    accept_suggestion = 54,
    create_suggestion = 55,
    delete_suggestion = 56,
    reject_suggestion = 57,
    trash = 58,
    remove_lock = 59,
    unmovable_item_reparented = 60,
    upload = 61,
    access_url = 62,
    delete_video_caption = 63,
    download_video_caption = 64,
    uploaded_video_caption = 65,
    view = 66,
    #endregion

    #region GDrive type = alc_change
    apply_security_update = 101,
    shared_drive_apply_security_update = 102,
    shared_drive_remove_security_update = 103,
    change_owner_hierarchy_reconciled = 104,
    change_owner = 105,
    publish_change = 106,
    change_acl_editors = 107,
    change_document_access_scope = 108,
    change_document_access_scope_hierarchy_reconciled = 109,
    change_document_visibility = 110,
    change_document_visibility_hierarchy_reconciled = 111,
    publish_new_version = 112,
    remove_security_update = 113,
    shared_drive_membership_change = 114,
    shared_drive_settings_change = 115,
    sheets_import_range_access_change = 116,
    change_user_access = 117,
    change_user_access_hierarchy_reconciled = 118,
    #endregion

    #region GDrive type = pool_quota_metadata
    storage_usage_update = 201,
    #endregion

    #region Admin type = DOCS_SETTINGS
    TRANSFER_DOCUMENT_OWNERSHIP = 301,
    DOCS_ORG_BRANDING_PROVISIONING = 302,
    DOCS_ORG_BRANDING_UPLOAD = 303,
    DRIVE_DATA_RESTORE = 304,
    CHANGE_DOCS_SETTING = 305,
    MOVE_SHARED_DRIVE_TO_ORG_UNIT = 306,
    #endregion
}
