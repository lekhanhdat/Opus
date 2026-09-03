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
using System.Runtime.Serialization;
using System.Text;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.ReportCenter.AdminReport.Object
{
    [KnownType(typeof(SubSitesAndPageInformationItem))]
    [KnownType(typeof(SiteCollectionContentAnalysis))]
    [KnownType(typeof(SiteCollectionFeatures))]
    [KnownType(typeof(SiteCollectionGeneralSettingItem))]
    [KnownType(typeof(SiteCollectionSearch))]
    [KnownType(typeof(SiteCollectionUsageReport))]
    [KnownType(typeof(SiteCollectionStorageReportItem))]
    [KnownType(typeof(SiteSecuritySettingItem))]
    [KnownType(typeof(SiteRSSSettingItem))]
    [KnownType(typeof(SiteAlertsSettingItem))]
    [KnownType(typeof(SiteRegionalSettingItem))]
    [KnownType(typeof(SitePropertiesItem))]
    [KnownType(typeof(SiteGeneralSettingItem))]
    [KnownType(typeof(SiteFeaturesItem))]
    [KnownType(typeof(SiteUsageReport))]
    [KnownType(typeof(SiteSearch))]
    [KnownType(typeof(AuditInfoSettingItem))]
    [KnownType(typeof(SubSitesAndPageInformationItem))]
    [KnownType(typeof(ContentAnalysisItem))]
    [KnownType(typeof(ListAndDocumentLibraryInformation))]
    [KnownType(typeof(ListStorageReportItem))]
    [KnownType(typeof(ListGeneralSettingItem))]
    [KnownType(typeof(ListSecuritySettingItem))]
    [KnownType(typeof(SiteStorageReportItem))]
    [DataContract(Namespace = ContractConstants.Namespace)]
    public abstract class BaseReportSettingItem
    {
        [DataMember]
        public string Id { set; get; }
        [DataMember]
        public string NodeId { set; get; }
        [DataMember]
        public string SettingName { set; get; }
        [DataMember]
        public ReportSettingType SettingType { set; get; }

        public override string ToString()
        {
            StringBuilder baseReportSetting = new StringBuilder();
            baseReportSetting.AppendFormat("NodeId:{0}.", NodeId);
            baseReportSetting.AppendFormat("SettingType:{0}.", SettingType);
            return baseReportSetting.ToString();
        }

        //server端DifferentReport用
        public string NodeUrl { set; get; }

        public abstract List<AdminReportValue> Row();

        public virtual bool IsDifferentFromAnotherOne(BaseReportSettingItem anotherItem, List<BaseReportSettingItem> allItems)
        {
            return false;
        }
    }

    public class AdminReportValue
    {
        public AdminReportValueType ValueType { set; get; }
        public string Value { set; get; }
    }

    public enum AdminReportValueType
    {
        Undefined,
        BasicValue,
        Key,
        UnitValue,
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReportSettingType
    {
        [EnumMember]
        Undefined = 0,
        [EnumMember]
        ContentDBGenerateSettingItem = 1,
        [EnumMember]
        ContentDBSharePointStorageReportItem = 2,
        [EnumMember]
        ContentDBPropertiesItem = 3,

        [EnumMember]
        WebApplicationGeneralSettingItem = 4,
        [EnumMember]
        WebApplicationSecuritySettingItem = 5,
        [EnumMember]
        WebApplicationIISSettingItem = 6,
        [EnumMember]
        WebApplicationManagedPathItem = 7,
        [EnumMember]
        WebApplicationBlockedFileTypesItem = 8,
        [EnumMember]
        WebApplicationWebApplicationFeaturesItem = 9,
        [EnumMember]
        WebApplicationPropertiesItem = 10,
        [EnumMember]
        WebApplicationStorageReportItem = 11,

        [EnumMember]
        FarmConfigurationDatabaseItem = 12,
        [EnumMember]
        FarmDefaultDatabaseServerItem = 13,
        [EnumMember]
        FarmAntivirusItem = 14,
        [EnumMember]
        FarmOutgoingEmailSettingsItem = 15,
        [EnumMember]
        FarmIncomingEmailSettingsItem = 16,
        [EnumMember]
        FarmCurrentLicenseItem = 17,
        [EnumMember]
        FarmTypeItem = 18,
        [EnumMember]
        FarmSecuritySettingsItem = 19,
        [EnumMember]
        FarmPropertiesItem = 20,
        [EnumMember]
        FarmServersAndServicesItem = 21,
        [EnumMember]
        FarmSolutionsItem = 22,
        [EnumMember]
        FarmFeaturesItem = 23,
        [EnumMember]
        FarmFarmFeaturesItem = 24,
        [EnumMember]
        FarmEnvironmentOverviewItem = 25,

        [EnumMember]
        SiteSecuritySettingItem = 26,
        [EnumMember]
        SiteGeneralSettingItem = 27,
        [EnumMember]
        SiteRSSSettingItem = 28,
        [EnumMember]
        SiteAlertsSettingItem = 29,
        [EnumMember]
        SiteRegionalSettingItem = 30,
        [EnumMember]
        SitePropertiesItem = 31,
        [EnumMember]
        SiteFeaturesItem = 32,
        [EnumMember]
        ContentAnalysisItem = 33,
        [EnumMember]
        ListAndDocumentLibraryInformation = 34,
        [EnumMember]
        SiteUsageReport = 35,
        [EnumMember]
        SubSitesAndPageInformationItem = 58,
        [EnumMember]
        SiteSearchSettingItem = 59,

        [EnumMember]
        ListGeneralSettingItem = 36,
        [EnumMember]
        ListSecuritySettingItem = 37,

        [EnumMember]
        SiteCollectionGeneralSettingItem = 38,
        [EnumMember]
        SiteCollectionFeatures = 39,
        [EnumMember]
        SiteCollectionSearch = 40,
        [EnumMember]
        SiteCollectionContentAnalysis = 41,
        [EnumMember]
        SiteCollectionUsageReport = 42,
        [EnumMember]
        SiteCollectionStorageReportItem = 43,

        [EnumMember]
        FarmStorageReportItem = 46,
        [EnumMember]
        SiteStorageReportItem = 47,
        [EnumMember]
        ListStorageReportItem = 48,

        [EnumMember]
        FarmSharedServiceItem = 49,
        [EnumMember]
        FarmSharedServiceSearchBasedAlertsItem = 50,
        [EnumMember]
        FarmSharedServiceScopesItem = 51,
        [EnumMember]
        FarmSharedServiceMetadataPropertiesItem = 52,
        [EnumMember]
        FarmSharedServiceFederatedLocationsItem = 53,
        [EnumMember]
        FarmSharedServiceAuthoritativePagesItem = 54,
        [EnumMember]
        FarmSharedServiceFileTypesItem = 55,
        [EnumMember]
        FarmSharedServiceCrawlRulesItem = 56,
        [EnumMember]
        FarmSharedServiceContentSourceItem = 57,
        [EnumMember]
        AuditInformation = 60
    }

    public class SiteCollectionSecuritySettingItem : BaseReportSettingItem
    {
        public static readonly string Group = "Group";
        public static readonly string[] GroupColumns = { "Group Name", "User Name", "Permission" };
        public static readonly string User = "User";
        public static readonly string[] UserColumns = { "User Name", "Permission" };
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_1)]
        public string GroupOrUserName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_2)]
        public string Username { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_3)]
        public string GroupName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_4)]
        public string Permission { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_5)]
        public string UUserName { set; get; }
        [DataMember]
        [ColumnMapAttribute(DBColumn = ContractConstants.STRING_6)]
        public string UPermission { set; get; }

        public override List<AdminReportValue> Row()
        {
            return null;
        }
    }

    public class SiteCollectionSettingNameConstants
    {
        public static readonly string IncludeLowerLevelOptions = "Include Lower Level Options";
        //public static readonly string SecuritySettings = "Security Settings";
        public static readonly string GeneralSettings = "General Settings";
        //public static readonly string RSSSettings = "RSS Settings";
        public static readonly string SiteCollectionFeatures = "Site Collection Features";
        //public static readonly string ArchivedContent = "Archived Content";
        public static readonly string SharePointStorageReport = "SharePoint Storage Report";
        public static readonly string Search = "Search Settings";
        public static readonly string SiteCollectionUsage = "Site Collection Usage";
        public static readonly string ContentAnalysis = "Content Analysis";
        //public static readonly string RegionalSettings = "Regional Settings";
        //public static readonly string Properties = "Properties";

        // Second Setting
        public static readonly string Size = "Size(MB)";
        public static readonly string Administrators = "Administrators";
        public static readonly string AdministratorMembersOfGroup = "AdministratorMembersOfGroup";
        public static readonly string Description = "Description";
        public static readonly string CreatedTime = "Created Time";
        public static readonly string CreatedBy = "Created By";
        public static readonly string LastModified = "Last Modified";
        public static readonly string LastAccessedTime = "Last Accessed Time";
        public static readonly string PortalConnection = "Portal Connection";
        public static readonly string Owners = "Owners";
        public static readonly string OwnersMembersOfGroup = "OwnersMembersOfGroup";
        public static readonly string Lock = "Lock";
        public static readonly string Quota = "Quota";
        public static readonly string Bandwidth = "Bandwidth";
        public static readonly string DiscussionStorage = "Discussion Storage";
        public static readonly string Visits = "Visits";
        public static readonly string PrimarySiteAdministrator = "Primary Site Administrator";
        public static readonly string SecondarySiteAdministrator = "Secondary Site Administrator";
        public static readonly string SiteAdministratorContract = "Site Administrator Contact";
        public static readonly string SharingOutsideYourCompany = "Sharing Outside Your Company";
        public static readonly string ExternalShareDomainSettings = "External Share Domain Settings";
        public static readonly string Domains = "Domains";
        public static readonly string GroupsAndPermission = "Groups And Permission";
        public static readonly string UsersAndPermission = "Users And Permission";

        public static readonly string SearchSettings = "Search Settings";
        public static readonly string SearchVisibility = "Search Visibility";

        public static readonly string Site = "Site";
        public static readonly string List = "List";

        public static readonly string Activated = "Activated";
        public static readonly string Deactivated = "Deactivated";

        public static readonly string SearchCenterURL = "Search Center URL";
        public static readonly string SendQueriesToThisSearchResultsPage = "Send Queries to this Search Results Page";
    }

    public class FarmSettingNameConstants
    {
        public static readonly string ConfigurationDatabase = "Configuration Database";
        public static readonly string DefaultDatabaseServer = "Default Database Server";
        public static readonly string Antivirus = "Antivirus";
        public static readonly string OutgoingEmailSettings = "Outgoing Email Settings";
        public static readonly string IncomingEmailSettings = "Incoming Email Settings";
        public static readonly string CurrentLicense = "Current License";
        public static readonly string FarmType = "Farm Type";
        public static readonly string SecuritySettings = "Security Settings";
        public static readonly string Properties = "Properties";
        public static readonly string ServersAndServices = "Servers and Services";
        public static readonly string Solutions = "Solutions";
        public static readonly string Features = "Features";
        public static readonly string FarmFeatures = "Farm Features";
        public static readonly string SharedServices = "Shared Services";
        public static readonly string EnvironmentOverview = "Environment Overview";
        public static readonly string SharePointStorageReport = "SharePoint Storage Report";
        public static readonly string IncludeLowerLevelOptions = "Include Lower Level Options";

        //second setting
        public static readonly string ConfigDatabaseServer = "Configuration Database Server";
        public static readonly string ConfigDatabaseName = "Configuration Database Name";
        public static readonly string ConfigDatabaseVersion = "Configuration Database Version";

        public static readonly string ScanDocumentsOnUpload = "Virus Scanner Scan Documents on Upload";
        public static readonly string ScanDocumentsOnDownload = "Virus Scanner Scan Documents on Download";
        public static readonly string AllowUsersToDownloadInfectedDocuments = "Virus Scanner Allow Users to Download Infected Documents";
        public static readonly string AttemptToCleanInfectedDocuments = "Virus Scanner Attempt to Clean Infected Documents";
        public static readonly string TimeOutDuration = "Virus Scanner Time Out Duration(In Seconds)";
        public static readonly string Threads = "Number of Threads the Virus Scanner May Use";

        public static readonly string SMTPServer = "Outbound SMTP Server";
        public static readonly string OutboundfromAddress = "Outbound from Address";
        public static readonly string OutboundReplyToAddress = "Outbound Reply-to Address";

        public static readonly string EnableSitesOnThisServer = "Enable Sites on This Server to Receive Incoming Email";
        public static readonly string IncomingEmailServerDisplayAddress = "Incoming Email Server Display Address";
        public static readonly string IncomingEmailDropFolder = "Incoming Email Drop Folder";

        public static readonly string FarmAge = "Farm Age";
        public static readonly string WebApplication = "Web Application";
        public static readonly string ContentDB = "Databases (Content)";
        public static readonly string SiteCollection = "Site Collection";
        public static readonly string Site = "Site";
        public static readonly string List = "List";
        public static readonly string Item = "Item";
        public static readonly string EndUserRecycleBin = "Recycle Bin (End User)";
        public static readonly string SiteCollectionRecycleBin = "Recycle Bin (Site Collection)";
    }

    public class SiteSettingNameConstants
    {
        public static readonly string GeneralSettings = "General Settings";
        public static readonly string RegionalSettings = "Regional Settings";
        public static readonly string Properties = "Properties";
        public static readonly string SecuritySettings = "Security Settings";
        public static readonly string RSSSettings = "RSS Settings";
        public static readonly string SiteFeatures = "Site Features";
        public static readonly string SiteUsage = "Site Usage";
        public static readonly string AlertsSettings = "Alerts Settings";
        public static readonly string Search = "Search Settings";
        //public static readonly string ArchivedContent = "Archived Content";
        public static readonly string ContentAnalysis = "Content Analysis";
        public static readonly string SharePointStorageReport = "SharePoint Storage Report";
        public static readonly string SubSitesAndPageInformation = "Sub-sites and Page Information";
        public static readonly string ListAndDocumentLibraryInformation = "List and Document Library Information";
        public static readonly string AuditInformation = "Audit Information";
        public static readonly string IncludeLowerLevelOptions = "Include Lower Level Options";

        public static readonly string SubSiteLevel = "Sub-site";
        public static readonly string ListLevel = "List";

        //Second Setting
        public static readonly string SiteName = "Site Name";
        public static readonly string SiteUrl = "Site URL";
        public static readonly string OrphanSiteStatus = "Orphan Site Status";
        public static readonly string Description = "Description";
        public static readonly string Size = "Size(MB)";
        public static readonly string Author = "Author";
        public static readonly string CreatedTime = "Created Time";
        public static readonly string CreatedBy = "Created By";
        public static readonly string LastModified = "Last Modified";
        public static readonly string LastAccessedTime = "Last Accessed Time";
        public static readonly string Template = "Template";
        public static readonly string TemplateID = "Template ID";
        public static readonly string Owners = "Owners";
        public static readonly string OwnersMembersOfGroup = "OwnersMembersOfGroup";
        public static readonly string DatabaseName = "Database Name";
        public static readonly string ParentSite = "Parent Site";
        public static readonly string SiteCollectionUrl = "Site Collection URL";
        public static readonly string LastModifier = "Last Modifier";
        public static readonly string AccessRequestEmail = "Access Request Email Receiver";

        public static readonly string SitePermissions = "Site Permission";
        public static readonly string FullControlUsers = "Full Control Users";

        public static readonly string NumberOfSubSites = "Number of Sub-sites";
        public static readonly string NumberOfPages = "Number of Pages";
        public static readonly string NumberOfCustomPages = "Number of Custom Pages";
        public static readonly string PercentageOfCustomizedPages = "Percentage of Customized Pages";

        public static readonly string NumberOfDocumentLibraries = "Number of Document Libraries";
        public static readonly string NumberOfDocument = "Number of Document";
        public static readonly string NumberOfListAttachmentDocuments = "Number of List Attachment Documents";
        public static readonly string DocumentsTotalSize = "Documents Total Size";
        public static readonly string DocumentVersionTotalSize = "Document Version Total Size";
        public static readonly string NumberOfLists = "Number of Lists";
        public static readonly string NumberOfListItems = "Number of List Items";
        public static readonly string ListTotalSize = "List Total Size";
        public static readonly string NumberOfFilesOver10MB = "Number of Files over 10 MB";
        public static readonly string NumberOfFileTypes = "Number of File Types";
        public static readonly string NumberOfListPersonalViews = "Number of List/Library Personal Views";
        public static readonly string NumberOfListPublicViews = "Number of List/Library Public Views";
        public static readonly string NumberOfAuditRecordForSite = "Number of Audit Record for the Site";
        public static readonly string ApproxSizeOfSiteAuditRecords = "Approx Size of Site Audit Records";
        public static readonly string DiscussionBoardCount = "Discussion Board Count";
        public static readonly string DiscussionItemCount = "Discussion Item Count";
        public static readonly string DiscussionBoardTotalSize = "Discussion Board Total Size";
        public static readonly string SurveyCount = "Survey Count";
        public static readonly string SurveyResponseCount = "Survey Response Count";
        public static readonly string SurveyTotalSize = "Survey Total Size";

        public static readonly string SearchVisibility = "Search Visibility";

        public static readonly string TurnOnAccessRequestSettings = "Turn on Access Request Settings";
        public static readonly string NumberOfListAllowAccessRequestEmail = "The Number of Lists/Libraries that Allow Requests for Access";


        public static readonly string TurnOnAllowListReceiveEmail = "Turn on Allow this List/Library to Receive E-mail";
        public static readonly string NumberOfListTurnedOnReceiveEmail = "The Number of Lists/Libraries that Have E-Mail Enabled";
        public static readonly string ListLibrary = "Lists/Libraries";
        public static readonly string Number = "Number";
        public static readonly string Recipients = "Recipients";

        public static readonly string Locale = "Locale";
        public static readonly string SortOrder = "Sort Order";
        public static readonly string TimeZone = "Time Zone";
        public static readonly string Calendar = "Calendar";
        public static readonly string Alternate = "Alternate";
        public static readonly string ShowWeek = "Show Week";
        public static readonly string WorkWeek = "Work Week";
        public static readonly string FirstDayOfWeek = "First Day Of Week";
        public static readonly string FirstWeekOfYear = "First Week Of Year";
        public static readonly string StartTime = "Start Time";
        public static readonly string EndTime = "End Time";
        public static readonly string TimeSystem = "Time System";

        public static readonly string AllowRSSFeedsInThisSiteCollection = "Allow RSS feeds in this site collection";
        public static readonly string AllowRSSFeedsInThisSite = "Allow RSS feeds in this site";
        public static readonly string Copyright = "Copyright";
        public static readonly string ManagingEditor = "Managing Editor";
        public static readonly string Webmaster = "Webmaster";
        public static readonly string TimeToLive = "Time To Live (minutes)";

        public static readonly string AllowThisSiteToAppearInSearchResults = "Allow this site to appear in search results?";
        public static readonly string TheSiteASPXPageIndexingBehavior = @"The site's ASPX page indexing behavior";

        public static readonly string Activated = "Activated";
        public static readonly string Deactivated = "Deactivated";
    }



    public class ListSettingNameConstants
    {
        public static readonly string GeneralSettings = "General Settings";
        public static readonly string SharepointStorageReport = "SharePoint Storage Report";
        public static readonly string SecuritySettings = "Security Settings";

        //Second Setting
        public static readonly string ListName = "List Name";
        public static readonly string ListURL = "List URL";
        public static readonly string ListTotalSize = "List Total Size";
        public static readonly string ParentSiteURL = "Parent Site URL";
        public static readonly string Template = "Template";
        public static readonly string CreatedTime = "Created Time";
        public static readonly string CreatedBy = "Created By";
        public static readonly string LastModified = "Last Modified";
        public static readonly string FileNumber = "File/Attachment Numbers";
    }

    public class ContentDBSettingNameConstants
    {
        public static readonly string GeneralSetting = "General Settings";
        public static readonly string Properties = "Properties";
        public static readonly string SharePointStorageReport = "SharePoint Storage Report";

        public static readonly string Size = "Size(MB)";
        public static readonly string FreeSize = "Free Size(MB)";
        public static readonly string DatabaseServer = "Database Server";
        public static readonly string DatabaseName = "Database Name";
        public static readonly string DatabaseStatus = "Database Status";
        public static readonly string DatabaseAuthentication = "Database Authentication";
        public static readonly string CurrentNumberOfSites = "Current Number of Sites";
        public static readonly string SiteLevelWarning = "Site Level Warning";
        public static readonly string MaximunNumberOfSites = "Maximum Number of Sites";
    }

    public class WebApplicationSettingNameConstants
    {
        public static readonly string IncludeLowerLevelOptions = "Include Lower Level Options";
        public static readonly string GeneralSetting = "General Settings";
        public static readonly string IISSettings = "IIS Settings";
        public static readonly string SecuritySetting = "Security Settings";
        public static readonly string ManagedPath = "Managed Path";
        public static readonly string BlockedFileTypes = "Blocked File Types";
        public static readonly string WebApplicationFeatures = "Web Application Features";
        public static readonly string SharePointStorageReport = "SharePoint Storage Report";
        public static readonly string Properties = "Properties";


        public static readonly string QuataTemplate = "Quota Template";
        //public static readonly string SmartTagAndStatus = "Enable Person Name Smart Tag and Online Status for Members";
        public static readonly string SmartTagAndStatus = "Enable additional actions and Online Status for members";
        public static readonly string MaxUploadFileSize = "Maximum Upload File Size";
        public static readonly string TotalAlerts = "Alerts on This Server";
        public static readonly string MaxAlertsNum = "Maximum Number of Alerts That a User Can Create";
        public static readonly string EnableRSSFeeds = "Enable RSS Feeds";
        public static readonly string AcceptUserInfo = "Accept Username and Password from the API";
        public static readonly string SendUserInfo = "Send Username and Password";
        public static readonly string BackwardEventHandler = "Backward-Compatible Event Handlers";
        public static readonly string DeleteEntry = "Delete Entries from the Change Log";
        public static readonly string CleanRecycleBin = "Delete Items in the Recycle Bin";

        public static readonly string LowerLevelContentDB = "Content Database";
        public static readonly string LowerLevelSiteCollection = "Site Collection";
        public static readonly string LowerLevelWeb = "Site";
        public static readonly string LowerLevelList = "List";

        //Second Setting
        public static readonly string TimeZone = "Time Zone";
        public static readonly string QuotaTemplate = "Quota Template";
        //public static readonly string EnablePersonName = "Enable Person Name Smart Tag and Online Status for Members";
        public static readonly string EnablePersonName = "Enable additional actions and Online Status for members";
        public static readonly string MaximumUploadFileSize = "Maximum Upload File Size";
        public static readonly string AlertsOnThisServer = "Alerts on This Server";
        public static readonly string MaximumNumberOfAlertsUserCanCreate = "Maximum Number of Alerts That a User Can Create";
        public static readonly string EnableRssFeeds = "Enable RSS Feeds";
        public static readonly string EnableBlogAPI = "Enable Blog API";
        public static readonly string AcceptUsernameAndPasswordFromTheAPI = "Accept Username and Password from the API";
        public static readonly string SecurityValidation = "Security Validation";
        public static readonly string SecurityValidationExpires = "Security Validation Expires";
        public static readonly string SendUserNameAndPassword = "Send Username and Password";
        public static readonly string BackwardCompatibleEventHandlers = "Backward-Compatible Event Handlers";
        public static readonly string DeleteEntriesFromTheChangeLog = "Delete Entries from the Change Log";
        public static readonly string RecycleBinStatus = "Recycle Bin Status";
        public static readonly string DeleteItemsIntheRecycleBin = "Delete Items in the Recycle Bin";
        public static readonly string SecondStageRecycleBin = "Second Stage Recycle Bin";

        public static readonly string AuthenticationType = "Authentication Type";
        public static readonly string EnableAnonymousAccess = "Enable anonymous access";
        public static readonly string IntegratedWindowsAuthentication = "Integrated Windows authentication";
        public static readonly string UseNTLM = "Use NTLM";
        public static readonly string BasicAuthentication = "Basic authentication (password is sent in clear text)";
        public static readonly string EnableClientIntegration = "Enable Client Integration";
    }
}
