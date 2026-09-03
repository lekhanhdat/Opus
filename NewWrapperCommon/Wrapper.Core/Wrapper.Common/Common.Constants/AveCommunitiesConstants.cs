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
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public sealed class AveCommunitiesConstants
    {
        //public static readonly string AboutUsContainerDivClass = "ms-comm-aboutUs ms-core-defaultFont";
        //public static readonly string AboutUsDescriptionDivClass = "ms-comm-aboutUsDescription";
        //public static readonly string AboutUsDescriptionFormat = "<p class=\"ms-textLarge ms-comm-aboutUsDescription\">{0}</p>";
        //public static readonly string AboutUsEstablishedDateDivClass = "ms-comm-aboutUsEstablishedDate";
        public static readonly string AboutUsRulesHeadingHtmlFormat = "<h1>{0}</h1>";
        //public static readonly string AboutUsRulesHtmlFormat = "<h2 class=\"ms-comm-aboutUsRulesHeading\">{0}</h2>\r\n                        <ul class=\"ms-comm-aboutUsRulesList\">\r\n                            <li>\r\n                                {1}\r\n                            </li>\r\n                            <li>\r\n                                {2}\r\n                            </li>\r\n                            <li>\r\n                                {3}\r\n                            </li>\r\n                            <li>\r\n                                {4}\r\n                            </li>\r\n                        </ul>\r\n                        <p>{5}</p>";
        public static readonly Guid AbuseReports_Comments_FieldId = new Guid("{50A65E43-B8FA-433f-AB49-FD44A8D7AB08}");
        public static readonly Guid AbuseReports_CommentsLookup_FieldId = new Guid("{672D9500-5649-49ae-8166-777F40527874}");
        public static readonly Guid AbuseReports_Count_FieldId = new Guid("{C3FC749D-C4A7-478b-A915-21C1C68F7199}");
        public static readonly Guid AbuseReports_DiscussionItemId_FieldId = new Guid("{33F11310-37D5-4c93-BB4F-28B95CCBDCFA}");
        public static readonly Guid AbuseReports_Lookup_FieldId = new Guid("{69062C99-D89F-4162-BBC5-B1ACF8BFE123}");
        public static readonly Guid AbuseReports_Reporter_FieldId = new Guid("{A708FB99-6F8B-4e87-AC4D-DBF4F899D5DC}");
        public static readonly Guid AbuseReports_ReporterLookup_FieldId = new Guid("{CD4DF6FB-0DA8-4ac9-B551-ED4FA6CD88FD}");
        public static readonly Guid AbuseReports_TitleOrBody_FieldId = new Guid("{4C481E72-F3FA-46d7-98DD-A258C3DF5403}");
        public static readonly string AbuseReportsCommentsLookupFieldName = "AbuseReportsCommentsLookup";
        public static readonly string AbuseReportsCountFieldName = "AbuseReportsCount";
        public static readonly Guid AbuseReportsList_FeatureId = new Guid("C6A92DBF-6441-4b8b-882F-8D97CB12C83A");
        public static readonly string AbuseReportsLookupFieldName = "AbuseReportsLookup";
        public static readonly string AbuseReportsReporterLookupFieldName = "AbuseReportsReporterLookup";
        public static readonly Guid AccessRequestsListInheritingRequestedWebIdFieldId = new Guid("8F3ECCCD-CE41-4c7b-89B7-D9609844BC3D");
        public static readonly Guid AccessRequestsListRequestedByUserIdFieldId = new Guid("E542DC30-4FA9-4ec7-AEA3-A5A81C902ECF");
        public static readonly Guid AccessRequestsListRequestedForUserIdFieldId = new Guid("00BA4B7D-8942-441d-878A-82C7D7065E3E");
        public static readonly int AccessRequestsListStatusApprove = 1;
        public static readonly int AccessRequestsListStatusDenied = 3;
        public static readonly Guid AccessRequestsListStatusFieldId = new Guid("7CEA1B74-B154-44a9-BCF4-6653E9F93C21");
        public static readonly int AccessRequestsListStatusPending = 0;
        public static readonly string achievementPoints = "AchievementPoints";
        public static readonly string achievementPointsEnabled = "AchievementPointsEnabled";
        //public static readonly string ActivityContainerDivClass = "ms-comm-activity ms-core-defaultFont ms-noList";
        //public static readonly string ActivityStatsDivClass = "ms-comm-activityStats";
        public const string AdminCategoriesBaseViewID = "2";
        //public static readonly string AdminLinksContainerDivClass = "ms-comm-adminLinks ms-core-defaultFont ms-noList";
        public static readonly char AssociateGroupSeparator = ';';
        public static readonly string AuthorGiftedBadgeLookupFieldName = "AuthorGiftedBadgeLookup";
        public static readonly string AuthorLastActivityLookupFieldName = "AuthorLastActivityLookup";
        public static readonly string AuthorMemberSinceLookupFieldName = "AuthorMemberSinceLookup";
        public static readonly string AuthorMemberStatusIntLookupFieldName = "AuthorMemberStatusIntLookup";
        public static readonly string AuthorNumOfBestResponsesLookupFieldName = "AuthorNumOfBestResponsesLookup";
        public static readonly string AuthorNumOfPostsLookupFieldName = "AuthorNumOfPostsLookup";
        public static readonly string AuthorNumOfRepliesLookupFieldName = "AuthorNumOfRepliesLookup";
        public static readonly string AuthorReputationLookupFieldName = "AuthorReputationLookup";
        internal const string BatchOperation = "<?xml version=\"1.0\" encoding=\"UTF-8\"?><Batch>{0}</Batch>";
        public static readonly string camlWhereClause2 = "<Where><And><Eq><FieldRef Name=\"{0}\" /><Value Type=\"{1}\">{2}</Value></Eq><Eq><FieldRef Name=\"{3}\" /><Value Type=\"{4}\">{5}</Value></Eq></And></Where>";
        public static readonly Guid Categories_CategoriesLookup_FieldId = new Guid("{3F44DEE7-B4BA-4E0F-9A4C-84F4420DFAF6}");
        public static readonly Guid CategoriesList_Desc_FieldId = new Guid("{AB065451-14D6-485a-88C3-414C908D50D3}");
        public static readonly Guid CategoriesList_FeatureId = new Guid("D32700C7-9EC5-45e6-9C89-EA703EFCA1DF");
        public static readonly Guid CategoriesList_Image_FieldId = new Guid("{7CC564F1-ABD4-4a2f-BD9B-85DD1D071BDC}");
        public static readonly Guid CategoriesList_LastPostBy_FieldId = new Guid("{497E00DF-75C8-4e61-AC5C-A143B6A0FDDC}");
        public static readonly Guid CategoriesList_LastPostDate_FieldId = new Guid("{539458A6-152C-460f-A915-53722C6EB4A6}");
        public static readonly Guid CategoriesList_ReplyCount_FieldId = new Guid("{D42630F0-0084-4b16-B876-80FE8CF88879}");
        public static readonly int CategoriesList_TemplateType = 500;
        public static readonly Guid CategoriesList_TopicCount_FieldId = new Guid("{D2264183-83DC-4d08-A57D-974686192D7A}");
        public const int CategoryFeaturedPreviewViewRowLimit = 3;
        public const string CategoryFeaturedViewPropertyKey = "CategoryFeaturedFullView";
        public const string CategoryPageNamePropertyName = "CategoryPageName";
        public const string CategoryPagePropertyKey = "CategoryPage";
        //public static readonly string CommunitiesCssFile = "Themable/communities.css";
        public static readonly Guid Community_TopicPageUrl_FieldId = new Guid("{F841E7C6-0491-449f-86DF-9DAE475E2132}");
        public static readonly string CommunityAbuseReportsViewGuid = "vti_CommunityAbuseReportsViewGuid";
        //public static readonly string CommunityActivityListItemFormat = "\r\n                    <li id=\"{0}\" class=\"ms-comm-activityStatsItem ms-metadata\"> \r\n                        {1}\r\n                    </li>";
        //public static readonly string CommunityActivityNumberFormat = "\r\n                    <div class=\"ms-comm-activityStatsNumber ms-largeNumber\">{0}</div>";
        public static readonly string CommunityDiscussionViewCategoryFeaturedPreviewGuid = "vti_CommunityDiscussionViewCategoryFeaturedPreviewGuid";
        public static readonly string CommunityDiscussionViewCategoryGuid = "vti_CommunityDiscussionViewCategoryGuid";
        public static readonly string CommunityEnableAutoApprovalKey = "vti_CommunityEnableAutoApproval";
        public static readonly string CommunityEnableReportAbuseKey = "vti_CommunityEnableReportAbuse";
        public static readonly string CommunityEstablishedDateKey = "vti_CommunityEstablishedDate";
        public static readonly string CommunityGroupDescLinkFormat = "<a href=\"{0}\">{1}</a>";
        public static readonly string CommunityJoinPage = "CommunityJoinPage.aspx";
        public static readonly string CommunityMember_ContentTypeId = "0x010027FC2137D8DE4b00A40E14346D070D5201";
        public static readonly string CommunityMemberDetailsTemplateName = "CommunityMemberDetails";
        public static readonly Guid CommunityMembership_GiftedBadgeLookup_FieldId = new Guid("{ABE62893-898D-48f9-9A52-3778B420F81C}");
        public static readonly Guid CommunityMembership_GiftedBadgeText_FieldId = new Guid("{797192BF-C571-4f18-9A85-BE0ACF22DA05}");
        public static readonly Guid CommunityMembership_LastActivity_FieldId = new Guid("{CBA948C8-9E42-44a0-B9F1-A39D91B28CB0}");
        public static readonly Guid CommunityMembership_MemberStatusInt_FieldId = new Guid("{E236652C-CF8F-4917-8BAA-30FFCCCFB7E8}");
        public static readonly Guid CommunityMembership_NumBestResponses_FieldId = new Guid("{1BC74B88-BB81-4be5-961D-9CF75DFE0911}");
        public static readonly Guid CommunityMembership_NumDiscussions_FieldId = new Guid("{178D4AF1-459B-4f61-BB41-B347986EE37B}");
        public static readonly Guid CommunityMembership_NumReplies_FieldId = new Guid("{51139F59-4BAC-45cb-8047-9C633EED1DB0}");
        public static readonly Guid CommunityMembership_ReputationScore_FieldId = new Guid("{EDD35D15-AE36-4b1b-91AA-0E288DF6C612}");
        internal const string CommunityModerationJSKey = "sp.ui.communitymoderation.js";
        internal const string CommunityModerationTab = "Ribbon.CommunityModerationTab";
        internal const string CommunityModerationViewTab = "Ribbon.CommunityModerationViewTab";
        public static readonly string CommunityReportAbuseCustomActionGuidKey = "vti_CommunityReportAbuseCustomActionGuid";
        public static readonly string CommunityReputationSettingsPage = "CommunityReputationSettings.aspx";
        public static readonly string CommunitySettingsPage = "CommunitySettings.aspx";
        public static readonly string CommunityTemplateName = "COMMUNITY";
        public static readonly Guid ContentReputation_DescendantLikesCount_FieldId = new Guid("{16582F9F-BA8C-42F7-8A63-9994650BB6C8}");
        public static readonly Guid ContentReputation_DescendantRatingsCount_FieldId = new Guid("{5FEB760D-E1C5-42D7-92AC-26AE20A1365A}");
        public static readonly Guid ContentReputation_LastRatedOrLikedBy_FieldId = new Guid("{5D45DB58-9AE3-4541-9BD0-759872D0D8D6}");
        public static readonly Guid ContentReputation_LikedBy_FieldId = new Guid("{2CDCD5EB-846D-4f4d-9AAF-73E8E73C7312}");
        public static readonly Guid ContentReputation_LikesCount_FieldId = new Guid("{6E4D832B-F610-41a8-B3E0-239608EFDA41}");
        public static readonly Guid ContentReputation_Popularity_FieldId = new Guid("{898232F1-83C0-41DF-9F1A-64B08A03F62D}");
        public static readonly Guid CustomList_FeatureId = new Guid("00BFEA71-DE22-43B2-A848-C05709900100");
        internal const string DeleteListItemOpertion = "<Method>\r\n                <SetList Scope=\"Request\">{0}</SetList>\r\n                <SetVar Name=\"ID\">{1}</SetVar>\r\n                <SetVar Name=\"Cmd\">Delete</SetVar>\r\n              </Method>";
        public static readonly string DiscussionTitleOrBodyFieldName = "DiscussionTitleOrBody";
        public static readonly string displayAchievementAsImage = "DisplayAchievementAsImage";
        public const string FeaturedDiscussionBaseViewID = "7";
        public const string FlatDiscussionBaseViewID = "2";
        public const string HomePagePropertyKey = "HomePage";
        //public static readonly string HomePageWelcomeDivClass = "ms-comm-homeWelcome ms-core-defaultFont";
        internal const string IDQueryKey = "ID";
        public static readonly string IllegalCharsInGroupName = "/\\[]:|<>+=;,?*'\"@";
        //public static readonly string JoinDivClass = "ms-comm-join ms-core-defaultFont";
        public static readonly string level1 = "Level1";
        public static readonly string level1Text = "Level1Text";
        public static readonly string level2 = "Level2";
        public static readonly string level2Text = "Level2Text";
        public static readonly string level3 = "Level3";
        public static readonly string level3Text = "Level3Text";
        public static readonly string level4 = "Level4";
        public static readonly string level4Text = "Level4Text";
        public static readonly string level5 = "Level5";
        public static readonly string level5Text = "Level5Text";
        public static readonly string levelThresholds = "LevelThresholds";
        public const string ListCategoriesBaseViewID = "1";
        internal const string ListQueryKey = "List";
        internal const string ManageMembersTab = "Ribbon.ManageMembersTab";
        public const string ManagementDiscussionBaseViewID = "5";
        public static readonly int MaxMembersPerGiftedBadge = 100;
        public static readonly int MaxTPTextLength = 0xff;
        public static readonly string MemberFieldName = "Member";
        public static readonly string MemberLookupFieldName = "MemberLookup";
        public static readonly Guid Members_MemberLookup_FieldId = new Guid("{1805E563-22CF-44ed-96F5-58EBB8A6CB80}");
        public const string MembersAdminBaseViewID = "4";
        public const string MembersBaseViewID = "1";
        internal const string MembersCountIndexedPropertyKey = "Community_MembersCount";
        public static readonly Guid MembersList_FeatureId = new Guid("947AFD14-0EA1-46c6-BE97-DEA1BF6F5BAE");
        public static readonly int MembersList_TemplateType = 880;
        public const string MembersPagePropertyKey = "MembersPage";
        public static readonly int MemberStatusInt_Active = 1;
        public static readonly int MemberStatusInt_Inactive = 2;
        public static readonly string MKEY_AssociateGroups = "vti_associategroups";
        //public static readonly string MKEY_CreatedAssociateGroups = "vti_createdassociategroups";
        public const string NewMembersBaseViewID = "3";
        public static readonly int PeopleReputation_Level1 = 0;
        public static readonly int PeopleReputation_Level2 = 100;
        public static readonly int PeopleReputation_Level3 = 500;
        public static readonly int PeopleReputation_Level4 = 0x9c4;
        public static readonly int PeopleReputation_Level5 = 0x2710;
        public static readonly char ratingValuesDelimiter = ',';
        public const string RecursiveDiscussionBaseViewID = "6";
        public static readonly string ReferrerViewstateKey = "__REFERER__";
        internal const string RepliesCountIndexedPropertyKey = "Community_RepliesCount";
        public const string RepliesDiscussionBaseViewID = "4";
        public static readonly string ShowModerationTabQueryString = "ShowModerationTab";
        public static readonly string ShowModerationTabQueryStringValue = "1";
        public const string SingleCategoryBaseViewID = "3";
        public const string SingleMemberBaseViewID = "5";
        public static readonly Guid Site_FeatureId = new Guid("961D6A9C-4388-4cf2-9733-38EE8C89AFD4");
        public static readonly string SiteMembership_ContentTypeId = "0x010027FC2137D8DE4b00A40E14346D070D52";
        public static readonly Guid SiteMembership_StatusInt_FieldId = new Guid("{E236652C-CF8F-4917-8BAA-30FFCCCFB7E8}");
        public static readonly string StatusFieldName = "MemberStatus";
        public static readonly string StatusIntFieldName = "MemberStatusInt";
        public const string SubjectDiscussionBaseViewID = "3";
        public const string ThreadedDiscussionBaseViewID = "1";
        public const string TileCategoriesBaseViewID = "4";
        public const string TopContributorsBaseViewID = "2";
        public const string TopicPagePropertyKey = "TopicPage";
        internal const string TopicsCountIndexedPropertyKey = "Community_TopicsCount";
        public static readonly string WebpartTitleHtmlFormat = "\r\n                    <div class=\"ms-webpart-titleText\">\r\n                        {0}\r\n                    </div>";
        public static readonly string WikiFieldName = "WikiField";
        public static readonly string WikiPageHtmlDivEnd = "\r\n                        </div>\r\n                    </div>";
        public static readonly string WikiPageHtmlDivFormat = "<div class=\"{0}\">{1}</div>";
        //public static readonly string WikiPageHtmlDivStart = "\r\n                    <div class=\"ms-rte-layoutszone-outer\" style=\"width: 100%\" >\r\n                        <div class=\"ms-rte-layoutszone-inner\" >";
        public static readonly string WikiPageHtmlHeadingEnd = "\r\n                            </span>\r\n                        </span>\r\n                    </h1>";
        public static readonly string WikiPageHtmlHeadingStart = "\r\n                    <h1 class=\"ms-rteElement-H1B\" style=\"margin-bottom:0px;\" >\r\n                        <span>\r\n                            <span>";
        public static readonly string WikiPageHtmlParaBreak = "<p>&#160;</p>";
        //public static readonly string WikiPageHtmlTableEnd = "\r\n                                </td>\r\n                            </tr>\r\n                        </tbody>\r\n                    </table>\r\n                    <span id=\"layoutsData\" class=\"ms-hide\">false,false,2</span>";
        public static readonly string WikiPageHtmlTableMid = "\r\n                                </td>\r\n                                <td class=\"ms-wiki-columnSpacing\" style=\"width: 33.3%\">";
        //public static readonly string WikiPageHtmlTableStart = "\r\n                    <table id=\"layoutsTable\" style=\"width: 100%;\">\r\n                        <tbody>\r\n                            <tr style=\"vertical-align: top;\">\r\n                                <td style=\"width: 66.6%\">";
        //public static readonly string WikiPageHtmlWebPartDivFormat = "\r\n                    <div class=\"ms-rtestate-read ms-rte-wpbox ms-comm-wiki-divFormat1\">\r\n                        <div class=\"ms-rtestate-read {0}\" id=\"div_{0}\">\r\n                        </div>\r\n                        <div class=\"ms-rtestate-read ms-hide ms-comm-wiki-divFormat2\" id=\"vid_{0}\" >\r\n                        </div>\r\n                    </div>";
        //public static readonly string WikiPageWebPartZoneId = "wpz";

        // Methods
        private AveCommunitiesConstants()
        {
        }
    }
}
