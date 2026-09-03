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
namespace ExchangeUtility.Graph
{
    #region namespace
    using Newtonsoft.Json;
    #endregion
    [JsonObject(ItemNullValueHandling = NullValueHandling.Ignore)]
    public class YammerNetwork
    {
        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("id")]
        public long Id { get; set; }

        [JsonProperty("email")]
        public string Email { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("community")]
        public bool Community { get; set; }

        [JsonProperty("permalink")]
        public string Permalink { get; set; }

        [JsonProperty("web_url")]
        public string WebUrl { get; set; }

        [JsonProperty("show_upgrade_banner")]
        public bool ShowUpgradeBanner { get; set; }

        [JsonProperty("header_background_color")]
        public string HeaderBackgroundColor { get; set; }

        [JsonProperty("header_text_color")]
        public string HeaderTextColor { get; set; }

        [JsonProperty("navigation_background_color")]
        public string NavigationBackgroundColor { get; set; }

        [JsonProperty("navigation_text_color")]
        public string NavigationTextColor { get; set; }

        [JsonProperty("paid")]
        public bool Paid { get; set; }

        [JsonProperty("moderated")]
        public bool Moderated { get; set; }

        [JsonProperty("is_freemium")]
        public bool IsFreemium { get; set; }

        [JsonProperty("is_org_chart_enabled")]
        public bool IsOrgChartEnabled { get; set; }

        [JsonProperty("is_group_enabled")]
        public bool IsGroupEnabled { get; set; }

        [JsonProperty("is_chat_enabled")]
        public bool IsChatEnabled { get; set; }

        [JsonProperty("is_translation_enabled")]
        public bool IsTranslationEnabled { get; set; }

        [JsonProperty("created_at")]
        public string CreatedAt { get; set; }

        [JsonProperty("profile_fields_config")]
        public ProfileFieldsConfig ProfileFieldsConfig { get; set; }

        [JsonProperty("browser_deprecation_url")]
        public object BrowserDeprecationUrl { get; set; }

        [JsonProperty("external_messaging_state")]
        public string ExternalMessagingState { get; set; }

        [JsonProperty("state")]
        public string State { get; set; }

        [JsonProperty("enforce_office_authentication")]
        public bool EnforceOfficeAuthentication { get; set; }

        [JsonProperty("office_authentication_committed")]
        public bool OfficeAuthenticationCommitted { get; set; }

        [JsonProperty("is_gif_shortcut_enabled")]
        public bool IsGifShortcutEnabled { get; set; }

        [JsonProperty("is_link_preview_enabled")]
        public bool IsLinkPreviewEnabled { get; set; }

        [JsonProperty("attachments_in_private_messages")]
        public bool AttachmentsInPrivateMessages { get; set; }

        [JsonProperty("secret_groups")]
        public bool SecretGroups { get; set; }

        [JsonProperty("force_connected_groups")]
        public bool ForceConnectedGroups { get; set; }

        [JsonProperty("connected_all_company")]
        public bool ConnectedAllCompany { get; set; }

        [JsonProperty("m365_native_mode")]
        public bool M365NativeMode { get; set; }

        [JsonProperty("force_optin_modern_client")]
        public bool ForceOptinModernClient { get; set; }

        [JsonProperty("aad_guests_enabled")]
        public bool AadGuestsEnabled { get; set; }

        [JsonProperty("all_company_group_creation_state")]
        public int AllCompanyGroupCreationState { get; set; }

        [JsonProperty("unseen_message_count")]
        public int UnseenMessageCount { get; set; }

        [JsonProperty("preferred_unseen_message_count")]
        public int PreferredUnseenMessageCount { get; set; }

        [JsonProperty("private_unseen_thread_count")]
        public int PrivateUnseenThreadCount { get; set; }

        [JsonProperty("inbox_unseen_thread_count")]
        public int InboxUnseenThreadCount { get; set; }

        [JsonProperty("private_unread_thread_count")]
        public int PrivateUnreadThreadCount { get; set; }

        [JsonProperty("unseen_notification_count")]
        public int UnseenNotificationCount { get; set; }

        [JsonProperty("has_fake_email")]
        public bool HasFakeEmail { get; set; }

        [JsonProperty("is_primary")]
        public bool IsPrimary { get; set; }

        [JsonProperty("allow_attachments")]
        public bool AllowAttachments { get; set; }

        [JsonProperty("attachment_types_allowed")]
        public string AttachmentTypesAllowed { get; set; }

        [JsonProperty("privacy_link")]
        public string PrivacyLink { get; set; }
    }

    public class ProfileFieldsConfig
    {
        [JsonProperty("enable_work_history")]
        public bool EnableWorkHistory { get; set; }

        [JsonProperty("enable_education")]
        public bool EnableEducation { get; set; }

        [JsonProperty("enable_job_title")]
        public bool EnableJobTitle { get; set; }

        [JsonProperty("enable_work_phone")]
        public bool EnableWorkPhone { get; set; }

        [JsonProperty("enable_mobile_phone")]
        public bool EnableMobilePhone { get; set; }

        [JsonProperty("enable_summary")]
        public bool EnableSummary { get; set; }

        [JsonProperty("enable_interests")]
        public bool EnableInterests { get; set; }

        [JsonProperty("enable_expertise")]
        public bool EnableExpertise { get; set; }

        [JsonProperty("enable_location")]
        public bool EnableLocation { get; set; }

        [JsonProperty("enable_im")]
        public bool EnableIm { get; set; }

        [JsonProperty("enable_skype")]
        public bool EnableSkype { get; set; }

        [JsonProperty("enable_websites")]
        public bool EnableWebsites { get; set; }
    }
}