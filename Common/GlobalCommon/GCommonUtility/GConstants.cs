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
namespace AvePoint.GCommon.Utility
{
    /// <summary>
    /// 只有Agent和Manager共用的常量才可以放在这里。不共用的常量，请放到Agent和Manager各自的Constants里
    /// </summary>
    public class GConstants
    {
        public class TransferFlag
        {
            public const byte AGENT_ENCRYPTED = 1 << 5;
            public const byte AGENT_COMPRESSED = 1 << 4;
            public const byte MEDIA_ENCRYPTED = 1 << 3;
            public const byte MEDIA_COMPRESSED = 1 << 2;
            public const byte MESSAGE_COMPRESSED = 1 << 1;
            public const byte MESSAGE_ENCRYPTED = 1 << 0;

            public const int CRC32_FLAG = 0x01;

            public static bool IsModeSet(byte srcMode, byte destMode)
            {
                return (srcMode & destMode) != 0;
            }
        }

        //public class SPNodeLevel
        //{
        //    public const int Root = -2;
        //    public const int Farm = -1;
        //    public const int WebApplication = 2;
        //    public const int ContentDB = 30;
        //    public const int SiteCollection = 100;
        //    public const int Site = 200;
        //    public const int Lists = 201;
        //    public const int Sites = 202;
        //    public const int List = 300;
        //    public const int Library = 301;
        //    public const int Folder = 400;
        //    public const int Folders = 401;
        //    public const int RootFolder = 402;
        //    public const int Item = 500;
        //    public const int Items = 501;


        //    /*for CA Security Search start*/
        //    public const int Groups = 1000;
        //    public const int SharePointGroup = 1001;
        //    public const int DomainGroup = 1002;
        //    public const int SharePointUser = 1003;
        //    public const int Users = 1100;
        //    public const int User = 1101;
        //    /*for CA Security Search end*/
        //}

        public class FSNodeLevel
        {
            public const int Root = -2;
            public const int AgentGroup = -1;
            public const int Device = 2;
            public const int Folder = 100;
            public const int File = 200;
        }

        //public class SPNodeType
        //{
        //    public const int UnspecifiedBaseType = -1;
        //    public const int GenericList = 0;
        //    public const int DocumentLibrary = 1;
        //    public const int Unused = 2;
        //    public const int DiscussionBoard = 3;
        //    public const int Survey = 4;
        //    public const int Issue = 5;

        //    public const int ManualInput = 100;

        //    public const int CAWebapp = 201;

        //    public const int MOSSFarm = -2;
        //    public const int RemoteFarm = -3;
        //}

        public class SPNodeTemplate
        {
            public const int InvalidType = -1;
            public const int NoListTemplate = 0;
            public const int GenericList = 100;
            public const int DocumentLibrary = 101;
            public const int Survey = 102;
            public const int Links = 103;
            public const int Announcements = 104;
            public const int Contacts = 105;
            public const int Events = 106;
            public const int Tasks = 107;
            public const int DiscussionBoard = 108;
            public const int PictureLibrary = 109;
            public const int DataSources = 110;
            public const int WebTemplateCatalog = 111;
            public const int UserInformation = 112;
            public const int WebPartCatalog = 113;
            public const int ListTemplateCatalog = 114;
            public const int XMLForm = 115;
            public const int MasterPageCatalog = 116;
            public const int NoCodeWorkflows = 117;
            public const int WorkflowProcess = 118;
            public const int WebPageLibrary = 119;
            public const int CustomGrid = 120;
            public const int SolutionCatalog = 121;
            public const int NoCodePublic = 122;
            public const int ThemeCatalog = 123;
            public const int DataConnectionLibrary = 130;
            public const int WorkflowHistory = 140;
            public const int GanttTasks = 150;
            public const int Meetings = 200;
            public const int Agenda = 201;
            public const int MeetingUser = 202;
            public const int Decision = 204;
            public const int MeetingObjective = 207;
            public const int TextBox = 210;
            public const int ThingsToBring = 211;
            public const int HomePageLibrary = 212;
            public const int Posts = 301;
            public const int Comments = 302;
            public const int Categories = 303;
            public const int Facility = 402;
            public const int Whereabouts = 403;
            public const int CallTrack = 404;
            public const int Circulation = 405;
            public const int Timecard = 420;
            public const int Holidays = 421;
            public const int IMEDic = 499;
            public const int ExternalList = 600;
            public const int IssueTracking = 1100;
            public const int AdminTasks = 1200;
            public const int HealthRules = 1220;
            public const int HealthReports = 1221;
        }

        public class SPVersion
        {
            public const int Unknown = 0;
            public const int MOSS03 = 1;
            public const int MOSS07 = 2;
            public const int MOSS10 = 4;
            public const int MOSS13 = 5;
        }

        public class SPNodeName
        {
            public const string ContentDBs = "Content Databases";
            public const string SiteCollections = "Site Collections";
            public const string TopLevelSite = "top-level site";
            public const string Lists = "Lists";
            public const string Sites = "Sites";
            public const string Apps = "Apps";
            public const string RootFolder = "Root Folder";
            public const string Items = "Items";
            public const string Folders = "Folders";

            public const string RootSite = "Root Site";

            /*for CA Security Search start*/
            public const string Groups = "Groups";
            public const string Users = "Users";
            /*for CA Security Search end*/
        }

        public class EONodeName
        {
            public const string Items = "Content Databases";
            /*for CA Security Search end*/
        }

        public class ExChangeOnlineNodeName
        {
            public const string Farm = "My Registered Mailboxes";
            public const string Items = "Items";
            public const string Folders = "Folders";
            public const string InPlaceArchiveMail_Suffix = "(In-Place Archive Mailbox)";
            public const string ResourceMail_Suffix = "(Resource Mailbox)";
        }

        public class TreeOperation
        {
            public const int BROWSE = 1;
            public const int REFRESH = 2;
            /// <summary>
            /// just use for object based restore tree.
            /// </summary>
            public const int SEARCH = 3;
        }

        public class TreeCheckNumber
        {
            public const int UNCHECKED = 0;
            public const int CHECKED = 1;
        }

        public class TreeCheckState
        {
            public const int UNCHECK = 0;
            public const int CHECKED = 1;
            public const int HALF_CHECK = 2;
        }
        public class PRNodeIcon
        {
            public const string Farm = "/Image/Common/PRTree/farm_2010_16x16.png";
            public const string FarmAgent = "/Image/Common/PRTree/farm_16x16.png";
            //public const string FarmPersistObject = "FarmPersistObject";
            public const string WebService = "/Image/Common/PRTree/web_application_root_node_16x16.png";
            public const string AdminWebService = "/Image/Common/PRTree/admin_web_service_16x16.png";
            public const string OSearchService = "/Image/Common/PRTree/osearch_service_16x16.png";
            public const string SPSearchService = "/Image/Common/PRTree/spsearch_service_16x16.png";
            public const string SPSearchServiceInstance = "/Image/Common/PRTree/sp_search_service_instance_16x16.png";
            public const string Solutions = "/Image/Common/PRTree/soulutions_16x16.png";
            public const string Solution = "/Image/Common/PRTree/soulutions_16x16.png";
            public const string FormService = "/Image/Common/PRTree/info_path_forms_services_16x16.png";
            public const string InfoPathFormsServices = "/Image/Common/PRTree/info_path_forms_services_16x16.png";
            public const string InfoPathForm = "InfoPathForm";
            public const string InfoPath = "/Image/Common/PRTree/info_path_16x16.png";
            public const string GlobalTemplates = "GlobalTemplates";
            public const string Template = "/Image/Common/PRTree/template_16x16.png";
            public const string ExemptUserAgents = "/Image/Common/PRTree/exempt_user_agents_16x16.png";
            public const string ExemptUserAgent = "/Image/Common/PRTree/exempt_user_agent_16x16.png";
            public const string SSP = "SSP";
            //public const string SSPSharedObject = "SSPSharedObject";
            public const string InfoPathFormTemplates = "/Image/Common/PRTree/info_path_form_templates_16x16.png";
            public const string InfoPathFormTemplate = "/Image/Common/PRTree/info_path_form_template_16x16.png";
            public const string FrontEndWebServers = "/Image/Common/PRTree/front_end_web_servers_16x16.png";
            public const string FrontEndWebServer = "/Image/Common/DMTree/web_front_end_agent_16x16.png";
            public const string DataConnectionFiles = "/Image/Common/PRTree/data_connection_files_16x16.png";
            public const string DataConnectionFile = "/Image/Common/PRTree/data_connection_file_16x16.png";
            public const string WebApp = "/Image/Common/PRTree/web_application_16x16.png";
            public const string AdminWebApp = "/Image/Common/PRTree/web_application_16x16.png";
            //public const string FewList = "FewList";
            //public const string FewBackupList = "FewBackupList";
            public const string IISSettings = "/Image/Common/PRTree/iis_settings_16x16.png";
            public const string IISWebSite = "/Image/Common/DMTree/iis_site_node_16x16.png";
            public const string IISFolder = "IISFolder";
            public const string IISTemplates = "/Image/Common/DMTree/iis_templates_node_16x16.png";
            public const string IISWebconfig = "/Image/Common/DMTree/iis_web_config_16x16.png";
            public const string FileSystemFolder = "/Image/Common/DMTree/file_system_folder_16x16.png";
            public const string FileSystemFile = "/Image/Common/DMTree/file_system_file_16x16.png";
            public const string FileSystem = "/Image/Common/DMTree/file_system_16x16.png";
            public const string CustomFeatures = "/Image/Common/DMTree/custom_features_16x16.png";
            public const string Feature = "/Image/Common/DMTree/custom_feature_16x16.png";
            public const string SystemFeatures = "/Image/Common/PRTree/system_features_16x16.png";
            //public const string Index="Index";
            public const string OSearchIndex = "/Image/Common/PRTree/osearch_index_16x16.png";
            //public const string SPSearchIndex="SPSearchIndex";
            public const string SolutionFeatures = "/Image/Common/PRTree/solution_features_16x16.png";
            public const string SiteDefinitions = "/Image/Common/DMTree/sharepoint_site_definitions_16x16.png";
            public const string SiteDefinition = "/Image/Common/DMTree/sharepoint_definitions_16x16.png";
            public const string TempLateFolder = "/Image/Common/PRTree/template_folder_16x16.png";
            public const string GACAll = "GACAll";
            public const string NetVersion = "/Image/Common/PRTree/net_version_16x16.png";
            public const string AssemblyFolder = "/Image/Common/PRTree/assembly_folder_16x16.png";
            public const string DB = "/Image/Common/PRTree/database_16x16.png";
            public const string ConfigDB = "/Image/Common/PRTree/database_16x16.png";
            public const string ContentDB = "/Image/Common/PRTree/database_16x16.png";
            public const string AdminContentDB = "/Image/Common/PRTree/database_16x16.png";
            public const string SSPAdminDB = "/Image/Common/PRTree/database_16x16.png";
            public const string SSPSearchDB = "/Image/Common/PRTree/database_16x16.png";
            public const string SPSearchDB = "/Image/Common/PRTree/database_16x16.png";
            public const string SSODB = "/Image/Common/PRTree/database_16x16.png";
            public const string FBA = "/Image/Common/PRTree/fba_16x16.png";
            public const string FBADB = "/Image/Common/PRTree/fba_db_16x16.png";
            public const string FBAWebapp = "/Image/Common/PRTree/fba_web_application_16x16.png";
            public const string SLK = "SLK";
            public const string SLKDB = "/Image/Common/PRTree/database_16x16.png";
            public const string SLKSite = "SLKSite";
            public const string ProjectPSISharedApplication = "ProjectPSISharedApplication";
            public const string ProjectSite = "ProjectSite";
            public const string ProjectDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string SolutionDepend = "SolutionDepend";
            public const string SolutionLanguagePack = "SolutionLanguagePack";
            public const string Nintex = "/Image/Common/PRTree/nintex_16x16.png";
            public const string NintexConfigDB = "/Image/Common/PRTree/database_16x16.png";
            public const string NintexContentDB = "/Image/Common/PRTree/database_16x16.png";
            public const string SSPSettingProperty = "SSPSettingProperty";
            public const string ACS_SETTINGS = "/Image/Common/PRTree/acs_settings_16x16.png";
            public const string ACS_DB = "/Image/Common/PRTree/database_16x16.png";
            public const string ATM_DB = "/Image/Common/PRTree/database_16x16.png";
            public const string JobDefinitionGroup = "/Image/Common/PRTree/timer_jobs_group_16x16.png";
            public const string ServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string BDCServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string BDCServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string WordConversionServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string WordConversionServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string StateServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string StateServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string ManagedMetadataWebServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string ManagedMetadataWebServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string SecureStoreServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string SecureStoreServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string SearchServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string WebAnalyticsWebServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string WebAnalyticsWebServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string UserProfileServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string UserProfileServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string WebServiceEndPoint = "/Image/Common/PRTree/web_service_end_point_16x16.png";
            public const string WebServiceEndPointGroup = "/Image/Common/PRTree/web_service_end_point_group_16x16.png";
            //public const string BDCService = "BDCService";
            public const string BDCServiceApplication = "/Image/Common/PRTree/bdc_service_application_16x16.png";
            public const string WordConversionService = "WordConversionService";
            public const string WordConversionServiceApplication = "/Image/Common/PRTree/word_conversion_service_application_16x16.png";
            public const string StateService = "/Image/Common/PRTree/state_service_root_node_16x16.png";
            public const string StateServiceApplication = "/Image/Common/PRTree/state_service_application_16x16.png";
            //public const string ManagedMetadataWebService = "ManagedMetadataWebService";
            public const string ManagedMetadataWebServiceApplication = "/Image/Common/PRTree/managed_metadata_web_service_application_16x16.png";
            //public const string SecureStoreService = "SecureStoreService";
            public const string SecureStoreServiceApplication = "/Image/Common/PRTree/secure_store_service_application_16x16.png";
            public const string SearchService = "SearchService";
            //public const string WebAnalyticsWebService = "WebAnalyticsWebService";
            public const string WebAnalyticsWebServiceApplication = "/Image/Common/PRTree/web_analytics_web_service_application_16x16.png";
            //public const string UserProfileService = "UserProfileService";
            public const string UserProfileServiceApplication = "/Image/Common/PRTree/user_profile_service_application_16x16.png";
            public const string SecureStoreServiceDB = "/Image/Common/PRTree/database_16x16.png";
            public const string ManagedMetadataWebServiceDB = "/Image/Common/PRTree/database_16x16.png";
            public const string WebAnalyticsWebServiceReportDB = "/Image/Common/PRTree/database_16x16.png";
            public const string WebAnalyticsWebServiceStagingDB = "/Image/Common/PRTree/database_16x16.png";
            public const string WordConversionServiceDB = "/Image/Common/PRTree/database_16x16.png";
            public const string BDCServiceDB = "/Image/Common/PRTree/database_16x16.png";
            //public const string ExcelCalculationService = "ExcelCalculationService";
            public const string ExcelCalculationServiceApplication = "/Image/Common/PRTree/excel_calculation_service_application_16x16.png";
            public const string VisioGraphicsService = "/Image/Common/PRTree/visio_graphics_service_application_16x16.png";
            public const string VisioGraphicsServiceApplication = "/Image/Common/PRTree/visio_graphics_service_application_16x16.png";
            //public const string AssessService = "AssessService";
            public const string AssessServiceApplication = "/Image/Common/PRTree/access_service_application_16x16.png";
            public const string UserProfileServerProfileDB = "/Image/Common/PRTree/database_16x16.png";
            public const string UserProfileServerSyncDB = "/Image/Common/PRTree/database_16x16.png";
            public const string UserProfileServerSocialDB = "/Image/Common/PRTree/database_16x16.png";
            public const string VisioGraphicsServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string SharedServices = "/Image/Common/DMTree/shared_services_16x16.png";
            public const string SharedServicesApplications = "/Image/Common/PRTree/shared_services_applications_16x16.png";
            public const string SearchServiceApplication = "/Image/Common/PRTree/search_service_application_16x16.png";
            public const string SearchAdminDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string SearchPropertyStoreDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string SearchGathererDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string BRAdminComponent = "/Image/Common/PRTree/br_admin_component_16x16.png";
            public const string BRIndexPartition = "/Image/Common/PRTree/br_index_partition_16x16.png";
            public const string BRQueryComponent = "/Image/Common/PRTree/br_query_component_16x16.png";
            public const string BRCrawlComponent = "/Image/Common/PRTree/br_crawl_component_16x16.png";
            public const string SharedServicesProxies = "/Image/Common/PRTree/shared_services_proxies_16x16.png";
            public const string SearchServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string DiagnosticsService = "/Image/Common/PRTree/microsoft_sharepoint_foundation_diagnostics_service_16x16.png";
            public const string NgDiagnosticsService = "/Image/Common/PRTree/microsoft_sharepoint_foundation_diagnostics_service_16x16.png";
            public const string SPDiagnosticsService = "/Image/Common/PRTree/microsoft_sharepoint_foundation_diagnostics_service_16x16.png";
            //public const string BIMonitoringService = "BIMonitoringService";
            public const string BIMonitoringServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string BIMonitoringServiceApplication = "/Image/Common/PRTree/bi_monitoring_service_application_16x16.png";
            public const string BIMonitoringServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string BIMonitoringServiceDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string StateServiceApplicationDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string UserCodeService = "/Image/Common/PRTree/user_code_service_16x16.png";
            public const string SolutionValidatorGroup = "/Image/Common/PRTree/solution_validator_group_16x16.png";
            public const string DefaultSolutionValidator = "DefaultSolutionValidator";
            public const string PopularityLoadBalancerProvider = "/Image/Common/PRTree/popularity_load_balancer_provider_16x16.png";
            public const string ResourceMeasureGroup = "ResourceMeasureGroup";
            public const string ResourceMeasure = "ResourceMeasure";
            public const string ExecutionTierGroup = "ExecutionTierGroup";
            public const string ExecutionTier = "ExecutionTier";
            public const string ApplicationRegistryService = "/Image/Common/PRTree/application_registry_service_application_16x16.png";
            public const string ApplicationRegistryServiceApplication = "/Image/Common/PRTree/application_registry_service_application_16x16.png";
            public const string ApplicationRegistryServiceDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string ApplicationRegistryServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string SecurityTokenServiceApplication = "/Image/Common/PRTree/security_token_service_application_16x16.png";
            public const string ClaimEncodingManager = "/Image/Common/PRTree/claim_encoding_manager_16x16.png";
            public const string SecurityTokenServiceManager = "/Image/Common/PRTree/security_token_service_manager_16x16.png";
            public const string ClaimProviderManager = "/Image/Common/PRTree/claim_provider_manager_16x16.png";
            //public const string SecurityTokenService = "SecurityTokenService";
            public const string AccessServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string ExcelServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string LotusNotesConnectorProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string UsageandHealthDataCollectionProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string KlImaging = "/Image/Common/PRTree/kl_imaging_16x16.png";
            public const string KlIndexComponent = "/Image/Common/PRTree/kl_index_component_16x16.png";
            public const string KlViewComponent = "/Image/Common/PRTree/kl_view_component_16x16.png";
            public const string KlExportComponent = "/Image/Common/PRTree/kl_export_component_16x16.png";
            public const string KlSearchComponent = "/Image/Common/PRTree/kl_search_component_16x16.png";
            public const string KlPrintComponent = "/Image/Common/PRTree/kl_print_compvonent_16x16.png";
            public const string KlIndexDB = "/Image/Common/PRTree/database_16x16.png";
            public const string KlViewDB = "/Image/Common/PRTree/database_16x16.png";
            public const string FastSearchFarms = "/Image/Common/PRTree/fast_search_farms_16x16.png";
            public const string FastSearchAdminServer = "/Image/Common/PRTree/fast_search_admin_server_16x16.png";
            public const string FastSearchServer = "/Image/Common/PRTree/fast_search_server_16x16.png";
            public const string FastSearchAdminDB = "/Image/Common/PRTree/database_16x16.png";
            public const string Webparts = "/Image/Common/PRTree/web_parts_16x16.png";
            public const string webpart = "/Image/Common/PRTree/web_part_16x16.png";
            public const string SubscriptionSettingsServiceApplication = "/Image/Common/PRTree/subscription_settings_service_application_16x16.png";
            public const string SubscriptionSettingsDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string SubscriptionSettingsServiceApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            //public const string SubscriptionSettingsService = "SubscriptionSettingsService";
            public const string SubscriptionSettingsServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string SharedSearchSettings = "/Image/Common/PRTree/search_service_setting_16x16.png";
            public const string CustomDatabaseServer = "/Image/Common/PRTree/sql_server_agent_16x16.png";
            public const string CustomDatabaseInstance = "/Image/Common/PRTree/sql_server_instance_16x16.png";
            public const string CustomDatabaseInstanceDown = "/Image/Common/PRTree/sql_server_instance_down_16x16.png";
            public const string CustomDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string CustomDatabasesRoot = "/Image/Common/PRTree/custom_database_16x16.png";
            public const string DBNoAgent = "/Image/Common/PRTree/db_no_agent_16x16.png";
            public const string Default = "/Image/Common/PRTree/sharepoint_object_16x16.png";
            public const string NgSocialSiteReportingDB = "/Image/Common/PRTree/database_16x16.png";
            public const string NgSocialSiteServiceDB = "/Image/Common/PRTree/database_16x16.png";
            public const string NgSocialSiteServiceProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            public const string NgSocialSiteServiceApp = "/Image/Common/PRTree/newsgator_2010_16x16.png";

            public const string SPUsageService = "/Image/Common/PRTree/usage_and_health_data_collection_service_16x16.png";
            public const string SPUsageApplication = "/Image/Common/PRTree/usage_and_health_data_collection_service_application_16x16.png";
            public const string SPUsageDatabase = "/Image/Common/PRTree/database_16x16.png";
            public const string SPUsageApplicationProxy = "/Image/Common/PRTree/service_proxy_16x16.png";
            //SSA setting icons
            public const string SSASettings = "/Image/Common/PRTree/ssa_settings_16x16.png";
            public const string SSAScopes = "/Image/Common/PRTree/scopes_16x16.png";
            public const string SSAOneScope = "/Image/Common/PRTree/one_scope_16x16.png";
            public const string SSAOneManagedProperty = "/Image/Common/PRTree/one_managed_property_16x16.png";
            public const string SSAOneFederatedLocations = "/Image/Common/PRTree/one_federated_location_16x16.png";
            public const string SSAOneCrawlerImpactRule = "/Image/Common/PRTree/one_crawler_impact_rule_16x16.png";
            public const string SSAOneCrawlerRule = "/Image/Common/PRTree/one_crawl_rule_16x16.png";
            public const string SSAOneContentSource = "/Image/Common/PRTree/one_content_source_16x16.png";
            public const string SSAMetadataProperites = "/Image/Common/PRTree/metadata_properties_16x16.png";
            public const string SSAManagerdProperties = "/Image/Common/PRTree/managed_properties_16x16.png";
            public const string SSAFileTypes = "/Image/Common/PRTree/file_types_16x16.png";
            public const string SSAFederatedLocations = "/Image/Common/PRTree/federated_locations_16x16.png";
            public const string SSACrawlerImpactRules = "/Image/Common/PRTree/crawler_impact_rules_16x16.png";
            public const string SSACrawledProperties = "/Image/Common/PRTree/crawled_property_16x16.png";
            public const string SSACrawledProperty = "/Image/Common/PRTree/crawled_properties_16x16.png";
            public const string SSACrawlRule = "/Image/Common/PRTree/crawl_rules_16x16.png";
            public const string SSAContentSource = "/Image/Common/PRTree/content_sources_16x16.png";
            public const string SSAauthoritativePages = "/Image/Common/PRTree/authoritative_pages_16x16.png";

            public const string KLServcieDatabase = "/Image/Common/PRTree/kl_imaging_service_database_16x16.png";
            public const string KLServcieAppProxy = "/Image/Common/PRTree/kl_imaging_service_application_proxy_16x16.png";
            public const string KLServiceApplication = "/Image/Common/PRTree/kl_imaging_service_application_16x16.png";
            public const string KLDatabase = "/Image/Common/PRTree/kl_imaging_data_database_16x16.png";
            public const string KLData = "/Image/Common/PRTree/kl_imaging_data_16x16.png";

            // session
            public const string SessionStateService = "/Image/Common/PRTree/session_state_service_16x16.png";
            public const string SessionStateServiceApplication = "/Image/Common/PRTree/session_state_service_application_16x16.png";
            public const string SessionStateDatabase = "/Image/Common/PRTree/database_16x16.png";

            // SSA setting trees上的节点
            public const string AuthoritativeNode = "/Image/Common/PRTree/authoritative_node_16x16.png";
            public const string AuthoritativeLevels = "/Image/Common/PRTree/authoritative_levels_16x16.png";
            public const string NonAuthorNode = "/Image/Common/PRTree/non_author_node_16x16.png";
            public const string OwningSite = "/Image/Common/PRTree/owning_site_16x16.png";
            public const string Category = "/Image/Common/PRTree/category_16x16.png";

            // blob
            public const string ConnectorBlob = "/Image/Common/PRTree/connector_16x16.png";
            public const string ExtenderBlob = "/Image/Common/PRTree/storage_manager_16x16.png";
            #region for smsp
            public const string BRQueryComponentNotOnLun = "/Image/Common/PRTree/br_query_component_not_on_lun_16x16.png";
            public const string BRQueryComponentOnLun = "/Image/Common/PRTree/br_query_component_on_lun_16x16.png";
            public const string BRQueryComponentSnapMirrored = "/Image/Common/PRTree/br_query_component_snap_mirrored_16x16.png";
            public const string BRQueryComponentSnapMirroredAndVault = "/Image/Common/PRTree/br_query_component_snap_mirrored_and_vault_16x16.png";
            public const string BRQueryComponentSnapVault = "/Image/Common/PRTree/br_query_component_snap_vault_16x16.png";
            public const string BRQueryComponentUnknown = "/Image/Common/PRTree/br_query_component_unknown_16x16.png";

            public const string DatabaseNotOnLun = "/Image/Common/PRTree/database_not_on_lun_16x16.png";
            public const string DatabaseOnLun = "/Image/Common/PRTree/database_on_lun_16x16.png";
            public const string DatabaseSnapMirrored = "/Image/Common/PRTree/database_snap_mirrored_16x16.png";
            public const string DatabaseSnapMirroredAndVault = "/Image/Common/PRTree/database_snap_mirrored_and_vault_16x16.png";
            public const string DatabaseSnapVault = "/Image/Common/PRTree/database_snap_vault_16x16.png";
            public const string DatabaseUnknown = "/Image/Common/PRTree/database_unknown_16x16.png";

            public const string SPSearchServiceInstanceNotOnLun = "/Image/Common/PRTree/sp_search_service_instance_not_on_lun_16x16.png";
            public const string SPSearchServiceInstanceOnLun = "/Image/Common/PRTree/sp_search_service_instance_on_lun_16x16.png";
            public const string SPSearchServiceInstanceSnapMirrored = "/Image/Common/PRTree/sp_search_service_instance_snap_mirrored_16x16.png";
            public const string SPSearchServiceInstanceSnapMirroredAndVault = "/Image/Common/PRTree/sp_search_service_instance_snap_mirrored_and_vault_16x16.png";
            public const string SPSearchServiceInstanceSnapVault = "/Image/Common/PRTree/sp_search_service_instance_snap_vault_16x16.png";
            public const string SPSearchServiceInstanceUnknown = "/Image/Common/PRTree/sp_search_service_instance_unknown_16x16.png";

            public const string FastSearchAdminServerOnLun = "/Image/Common/PRTree/fast_search_admin_server_on_lun_16x16.png";
            public const string FastSearchAdminServerNotOnLun = "/Image/Common/PRTree/fast_search_admin_server_not_on_lun_16x16.png";
            public const string FastSearchAdminServerSnapMirrored = "/Image/Common/PRTree/fast_search_admin_server_snap_mirrored_16x16.png";
            public const string FastSearchAdminServerSnapMirroredAndVault = "/Image/Common/PRTree/fast_search_admin_server_snap_mirrored_and_vault_16x16.png";
            public const string FastSearchAdminServerSnapVault = "/Image/Common/PRTree/fast_search_admin_server_snap_vault_16x16.png";
            public const string FastSearchAdminServerUnknown = "/Image/Common/PRTree/fast_search_admin_server_unknown_16x16.png";

            public const string FastSearchServerOnLun = "/Image/Common/PRTree/fast_search_server_on_lun_16x16.png";
            public const string FastSearchServerNotOnLun = "/Image/Common/PRTree/fast_search_server_not_on_lun_16x16.png";
            public const string FastSearchServerSnapMirrored = "/Image/Common/PRTree/fast_search_server_snap_mirrored_16x16.png";
            public const string FastSearchServerSnapMirroredAndVault = "/Image/Common/PRTree/fast_search_server_snap_mirrored_and_vault_16x16.png";
            public const string FastSearchServerSnapVault = "/Image/Common/PRTree/fast_search_server_snap_vault_16x16.png";
            public const string FastSearchServerUnknown = "/Image/Common/PRTree/fast_search_server_unknown_16x16.png";
            #endregion
            //public const string DataNode = "DataNode";
            //public const string InvisibleDataNode = "InvisibleDataNode";
        }

        public class DMNodeIcon
        {
            public const string WebApplications = "/Image/Common/DMTree/web_applications_16x16.png";
            public const string DesignObjRootFolder = "/Image/Common/DMTree/sharepoint_designer_objects_16x16.png";
            public const string DesignFolders = "/Image/Common/DMTree/design_folders_16x16.png";
            public const string DesignFolder = "/Image/Common/DMTree/design_folder_16x16.png";
            public const string DesignItems = "/Image/Common/DMTree/design_items_16x16.png";
            public const string DesignItem = "/Image/Common/DMTree/design_item_16x16.png";
            public const string DesignLists = "/Image/Common/DMTree/design_lists_16x16.png";
            public const string SiteAdmin = "/Image/Common/DMTree/site_administration_16x16.png";
            public const string SiteColumns = "/Image/Common/DMTree/site_columns_16x16.png";
            public const string SiteColumnGroup = "/Image/Common/DMTree/site_column_group_16x16.png";
            public const string SiteColumn = "/Image/Common/DMTree/site_column_16x16.png";
            public const string ContentTypes = "/Image/Common/DMTree/site_content_types_16x16.png";
            public const string ContentTypeGroup = "/Image/Common/DMTree/content_type_group_16x16.png";
            public const string SiteContentType = "/Image/Common/DMTree/content_type_16x16.png";
            public const string WebFrontEnd = "/Image/Common/DMTree/web_front_end_16x16.png";
            public const string FEWAgentNode = "/Image/Common/DMTree/web_front_end_agent_16x16.png";
            public const string IISettingsVirtualNode = "/Image/Common/DMTree/iis_settings_16x16.png";
            public const string IISPNode = "/Image/Common/DMTree/iis_sharepoint_node_16x16.png";
            public const string IISTemplatesNode = "/Image/Common/DMTree/iis_templates_node_16x16.png";
            public const string IISiteNode = "/Image/Common/DMTree/iis_site_node_16x16.png";
            public const string IISWebConfigNode = "/Image/Common/DMTree/iis_web_config_16x16.png";
            public const string IISDefaultSiteNode = "/Image/Common/DMTree/iis_default site_16x16.png";
            public const string IISNonIISiteNode = "/Image/Common/DMTree/iis_none_iis_site_16x16.png";
            public const string IISFolderNode = "/Image/Common/DMTree/iis_folder_16x16.png";
            public const string IISFileNode = "/Image/Common/DMTree/iis_file_16x16.png";
            public const string GACVirtualNode = "/Image/Common/DMTree/globa_assembly_cache_16x16.png";
            public const string GACFirstVirtualNode = "/Image/Common/DMTree/gac_first_node_16x16.png";
            public const string GACSecondVirtualNode = "/Image/Common/DMTree/gac_first_node_16x16.png";
            public const string GACNode = "/Image/Common/DMTree/gac_node_16x16.png";
            public const string CustomFeatureVirtualNode = "/Image/Common/DMTree/custom_features_16x16.png";
            public const string CustomFeatureNode = "/Image/Common/DMTree/custom_feature_16x16.png";
            public const string SiteDefinitionVirtualNode = "/Image/Common/DMTree/sharepoint_site_definitions_16x16.png";
            public const string SiteDefinitionNode = "/Image/Common/DMTree/sharepoint_definitions_16x16.png";
            public const string FileSystemVirtualNode = "/Image/Common/DMTree/file_system_16x16.png";
            public const string FileSystemDiskNode = "/Image/Common/DMTree/file_system_disk_16x16.png";
            public const string FileSystemFoldersNode = "/Image/Common/DMTree/file_system_folders_16x16.png";
            public const string FileSystemFolderNode = "/Image/Common/DMTree/file_system_folder_16x16.png";
            public const string FileSystemFilesNode = "/Image/Common/DMTree/file_system_files_16x16.png";
            public const string FileSystemFileNode = "/Image/Common/DMTree/file_system_file_16x16.png";
            public const string Solutions = "/Image/Common/DMTree/farm_solutions_16x16.png";
            public const string SolutionDeployNode = "/Image/Common/DMTree/deploy_solution_16x16.png";
            public const string SolutionNotDeployNode = "/Image/Common/DMTree/not_deploy_solution_16x16.png";
            public const string SolutionActivateNode = "/Image/Common/DMTree/activate_solutions_16x16.png";
            public const string SolutionDeactivateNode = "/Image/Common/DMTree/deactivate_solutions_16x16.png";
            public const string SolutionRunningNode = "/Image/Common/DMTree/running_solution_16x16.png";
            public const string UserSolutionGalleryNode = "/Image/Common/DMTree/user_solution_gallery_16x16.png";
            public const string SharedServices = "/Image/Common/DMTree/shared_services_16x16.png";
            public const string ListSetting = "/Image/Common/DMTree/list_settings_16x16.png";
            public const string SiteSetting = "/Image/Common/DMTree/site_settings_16x16.png";

            public const string EmmGroupNode = "/Image/Common/DMTree/emm_group_16x16.png";
            public const string EmmRootNode = "/Image/Common/DMTree/emm_root_16x16.png";
            public const string EmmSpecialSetNode = "/Image/Common/DMTree/emm_special_set_16x16.png";
            public const string EmmTermNode = "/Image/Common/DMTree/emm_term_16x16.png";
            public const string EmmTermSetNode = "/Image/Common/DMTree/emm_term_set_16x16.png";
            public const string ManagedMetadataServiceNode = "/Image/Common/DMTree/managed_metadata_service_16x16.png";

            public const string ListAdministration = "/Image/Common/DMTree/list_administration_16x16.png";
            public const string ListColumns = "/Image/Common/DMTree/list_columns_16x16.png";
            public const string ListColumnGroup = "/Image/Common/DMTree/list_column_group_16x16.png";
            public const string ListColumn = "/Image/Common/DMTree/list_column_16x16.png";
            public const string ListContentTypes = "/Image/Common/DMTree/list_content_types_16x16.png";
            public const string ListContentTypeGroup = "/Image/Common/DMTree/list_content_group_16x16.png";
            public const string ListContentType = "/Image/Common/DMTree/list_content_type_16x16.png";
            public const string ListContentType_Workflow = "/Image/Common/DMTree/list_content_type_workflow_16x16.png";
            public const string SiteContentType_Workflow = "/Image/Common/DMTree/site_content_type_workflow_16x16.png";
            public const string Workflows = "/Image/Common/DMTree/workflows_16x16.png";
            public const string Workflow = "/Image/Common/DMTree/workflow_16x16.png";
            //public const string TraditionalMode = "/Image/Common/DMTree/traditional_mode_16x16.png";
            //public const string MultiTenantMode = "/Image/Common/DMTree/multi-tenant_mode_16x16.png";
        }

        public class NodeIcon
        {
            public const string Farm2007 = "/Image/Common/Tree/farm_2007_16x16.png";
            public const string Farm2010 = "/Image/Common/Tree/farm_2010_16x16.png";
            public const string SkyDriveProSitesGroup = "/Image/Common/Tree/my_site_group_16x16.png";
            public const string WebApp = "/Image/Common/Tree/web_application_16x16.png";
            public const string SkyDriveProSites = "/Image/Common/Tree/my_site_16x16.png";
            public const string SiteCollection = "/Image/Common/Tree/site_collection_16x16.png";
            public const string ReadOnlySiteCollection = "/Image/Common/Tree/site_collection_read_only_16x16.png";
            public const string RootSite = "/Image/Common/Tree/root_site_16x16.png";
            public const string Site = "/Image/Common/Tree/root_site_16x16.png";
            public const string InheritSite = "/Image/Common/Tree/site_inherit_16x16.png";
            public const string NotInheritSite = "/Image/Common/Tree/site_not_inherit_16x16.png";
            public const string Lists = "/Image/Common/Tree/lists_16x16.png";
            public const string Sites = "/Image/Common/Tree/sites_16x16.png";
            public const string List = "/Image/Common/Tree/list_16x16.png";
            public const string InheritList = "/Image/Common/Tree/list_inherit_16x16.png";
            public const string NotInheritList = "/Image/Common/Tree/list_not_inherit_16x16.png";
            public const string Library = "/Image/Common/Tree/list_library_16x16.png";
            public const string InheritLibrary = "/Image/Common/Tree/library_inherit_16x16.png";
            public const string NotInheritLibrary = "/Image/Common/Tree/library_not_inherit_16x16.png";
            public const string RootFolder = "/Image/Common/Tree/root_folder_16x16.png";
            public const string Folders = "/Image/Common/Tree/folders_16x16.png";
            public const string Folder = "/Image/Common/Tree/folder_16x16.png";
            public const string InheritFolder = "/Image/Common/Tree/folder_inherit_16x16.png";
            public const string NotInheritFolder = "/Image/Common/Tree/folder_not_inherit_16x16.png";
            public const string Items = "/Image/Common/Tree/items_16x16.png";
            public const string Item = "/Image/Common/Tree/item_16x16.png";
            public const string InheritItem = "/Image/Common/Tree/item_inherit_16x16.png";
            public const string NotInheritItem = "/Image/Common/Tree/item_not_inherit_16x16.png";
            public const string IncludeNew = "/Image/Common/Tree/include_new_16x16.png";
            public const string SelectAll = "/Image/Common/Tree/select_all_16x16.png";

            public const string SelectAll_New = "/Image/Common/Tree/MailboxTree/select_all_16x16.png";

            public const string Groups = "/Image/Common/Tree/groups_16x16.png";
            public const string SharePointGroup = "/Image/Common/Tree/sharepoint_groups_16x16.png";
            public const string Users = "/Image/Common/Tree/users_16x16.png";
            public const string User = "/Image/Common/Tree/user_16x16.png";
            public const string DomainGroup = "/Image/Common/Tree/domain_group_16x16.png";

            public const string webApplicationRuleConfigured = "/Image/Common/Tree/web_application_rules_configured_16x16.png";
            public const string webApplicationDisbleRuleConfigured = "/Image/Common/Tree/web_application_rules_configured_16x16_dis.png";
            public const string siteCollectionRuleConfigured = "/Image/Common/Tree/site_collection_rules_configured_16x16.png";
            public const string siteCollectionDisableRuleConfigured = "/Image/Common/Tree/site_collection_rules_configured_16x16_dis.png";
            public const string siteRuleConfigured = "/Image/Common/Tree/root_site_rules_configured_16x16.png";
            public const string siteDisableRuleConfigured = "/Image/Common/Tree/root_site_rules_configured_16x16_dis.png";
            public const string listRuleConfigured = "/Image/Common/Tree/list_rules_configured_16x16.png";
            public const string listDisableRuleConfigured = "/Image/Common/Tree/list_rules_configured_16x16_dis.png";
            public const string libraryRuleConfigured = "/Image/Common/Tree/library_rules_configured_16x16.png";
            public const string libraryDisableRuleConfigured = "/Image/Common/Tree/library_rules_configured_16x16_dis.png";
            public const string folderRuleConfigured = "/Image/Common/Tree/folder_rules_configured_16x16.png";
            public const string folderDisableRuleConfigured = "/Image/Common/Tree/folder_rules_configured_16x16_dis.png";
            public const string rootFolderRuleConfigured = "/Image/Common/Tree/root_folder_rules_configured_16x16.png";
            public const string rootFolderDisableRuleConfigured = "/Image/Common/Tree/root_folder_rules_configured_16x16_dis.png";
        }

        public class NodeIcon2013
        {
            public const string SiteCollection = "/Image/Common/Tree/13Tree/site_collection_16x16.png";
            public const string RootSite = "/Image/Common/Tree/13Tree/root_site_16x16.png";
            public const string Site = "/Image/Common/Tree/13Tree/root_site_16x16.png";
            public const string InheritSite = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritSite = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string Lists = "/Image/Common/Tree/13Tree/lists_16x16.png";
            public const string Sites = "/Image/Common/Tree/13Tree/sites_16x16.png";
            public const string Apps = "/Image/Common/Tree/13Tree/apps_16x16.png";
            public const string App = "/Image/Common/Tree/13Tree/app_16x16.png";
            public const string AppData = "/Image/Common/Tree/13Tree/app_data_16x16.png";
            public const string List = "/Image/Common/Tree/13Tree/list_16x16.png";
            public const string InheritList = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritList = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string Library = "/Image/Common/Tree/13Tree/list_library_16x16.png";
            public const string InheritLibrary = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritLibrary = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string RootFolder = "/Image/Common/Tree/13Tree/root_folder_16x16.png";
            public const string Folders = "/Image/Common/Tree/13Tree/folders_16x16.png";
            public const string Folder = "/Image/Common/Tree/13Tree/folder_16x16.png";
            public const string InheritFolder = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritFolder = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string Items = "/Image/Common/Tree/13Tree/items_16x16.png";
            public const string Item = "/Image/Common/Tree/13Tree/item_16x16.png";
            public const string InheritItem = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritItem = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string IncludeNew = "/Image/Common/Tree/13Tree/include_new_16x16.png";
            public const string SelectAll = "/Image/Common/Tree/13Tree/select_all_16x16.png";
            public const string Groups = "/Image/Common/Tree/13Tree/groups_16x16.png";
            public const string SharePointGroup = "/Image/Common/Tree/13Tree/sharepoint_groups_16x16.png";
            public const string Users = "/Image/Common/Tree/13Tree/users_16x16.png";
            public const string User = "/Image/Common/Tree/13Tree/user_16x16.png";
            public const string DomainGroup = "/Image/Common/Tree/13Tree/domain_group_16x16.png";
            public const string Inherit = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInherit = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
        }

        public class SP13NodeIcon
        {
            public const string webApplicationRuleConfigured = "/Image/Common/Tree/13Tree/web_application_rules_configured_16x16.png";
            public const string webApplicationDisbleRuleConfigured = "/Image/Common/Tree/13Tree/web_application_rules_configured_16x16_dis.png";
            public const string siteCollectionRuleConfigured = "/Image/Common/Tree/13Tree/site_collection_rules_configured_16x16.png";
            public const string siteCollectionDisableRuleConfigured = "/Image/Common/Tree/13Tree/site_collection_rules_configured_16x16_dis.png";
            public const string siteRuleConfigured = "/Image/Common/Tree/13Tree/root_site_rules_configured_16x16.png";
            public const string siteDisableRuleConfigured = "/Image/Common/Tree/13Tree/root_site_rules_configured_16x16_dis.png";
            public const string listRuleConfigured = "/Image/Common/Tree/13Tree/list_rules_configured_16x16.png";
            public const string listDisableRuleConfigured = "/Image/Common/Tree/13Tree/list_rules_configured_16x16_dis.png";
            public const string libraryRuleConfiged = "/Image/Common/Tree/13Tree/library_rules_configured_16x16.png";
            public const string libraryDisableRuleConfigured = "/Image/Common/Tree/13Tree/library_rules_configured_16x16_dis.png";
            public const string folderRuleConfigured = "/Image/Common/Tree/13Tree/folder_rules_configured_16x16.png";
            public const string folderDisableRuleConfigured = "/Image/Common/Tree/13Tree/folder_rules_configured_16x16_dis.png";
            public const string rootFolderRuleConfigured = "/Image/Common/Tree/13Tree/root_folder_rules_configured_16x16.png";
            public const string rootFolderDisableRuleConfigured = "/Image/Common/Tree/13Tree/root_folder_rules_configured_16x16_dis.png";

            public const string Farm2013 = "/Image/Common/Tree/13Tree/farm_16x16.png";
            public const string WebApp = "/Image/Common/Tree/13Tree/web_application_16x16.png";
            public const string SkyDriveProSitesGroup = "/Image/Common/Tree/my_site_group_16x16.png";
            public const string SiteCollection = "/Image/Common/Tree/13Tree/site_collection_16x16.png";
            public const string SkyDriveProSites = "/Image/Common/Tree/my_site_16x16.png";
            public const string ReadOnlySiteCollection = "/Image/Common/Tree/13Tree/site_collection_read_only_16x16.png";
            public const string RootSite = "/Image/Common/Tree/13Tree/root_site_16x16.png";
            public const string Site = "/Image/Common/Tree/13Tree/root_site_16x16.png";
            public const string Apps = "/Image/Common/Tree/13Tree/apps_16x16.png";
            public const string App = "/Image/Common/Tree/13Tree/app_16x16.png";
            public const string AppData = "/Image/Common/Tree/13Tree/app_data_16x16.png";
            public const string InheritSite = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritSite = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string Lists = "/Image/Common/Tree/13Tree/lists_16x16.png";
            public const string Sites = "/Image/Common/Tree/13Tree/sites_16x16.png";
            public const string List = "/Image/Common/Tree/13Tree/list_16x16.png";
            public const string InheritList = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritList = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string Library = "/Image/Common/Tree/13Tree/list_library_16x16.png";
            public const string InheritLibrary = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritLibrary = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string RootFolder = "/Image/Common/Tree/13Tree/root_folder_16x16.png";
            public const string Folders = "/Image/Common/Tree/13Tree/folders_16x16.png";
            public const string Folder = "/Image/Common/Tree/13Tree/folder_16x16.png";
            public const string InheritFolder = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritFolder = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string Items = "/Image/Common/Tree/13Tree/items_16x16.png";
            public const string Item = "/Image/Common/Tree/13Tree/item_16x16.png";
            public const string InheritItem = "/Image/Common/Tree/13Tree/inherit_16x16.png";
            public const string NotInheritItem = "/Image/Common/Tree/13Tree/not_inherit_16x16.png";
            public const string IncludeNew = "/Image/Common/Tree/13Tree/include_new_16x16.png";
            public const string SelectAll = "/Image/Common/Tree/13Tree/select_all_16x16.png";

            public const string Groups = "/Image/Common/Tree/13Tree/groups_16x16.png";
            public const string SharePointGroup = "/Image/Common/Tree/13Tree/sharepoint_groups_16x16.png";
            public const string Users = "/Image/Common/Tree/13Tree/users_16x16.png";
            public const string User = "/Image/Common/Tree/13Tree/user_16x16.png";
            public const string DomainGroup = "/Image/Common/Tree/13Tree/domain_group_16x16.png";
            public const string TempIcon = "/Image/Common/tryout_16x16_normal.png";

        }

        public class FileSystemNodeIcon
        {
            /*
            * For File System tree node icon
            * */
            public const string AgentGroup = "";
            public const string Device = "/Image/Common/Tree/export_location_16x16.png";
            public const string Folder = "/Image/Common/Tree/folder_16x16.png";
            public const string File = "";
            #region For Data Import Tree Node
            public const string Plan = "/Image/Common/Tree/plan_id_16x16.png";
            public const string Cycle = "/Image/Common/Tree/job_id_16x16.png";
            #endregion
        }

        public class DMSolutionStoreNodeIcon
        {
            /*
            * For Solution Store tree node icon
            * */
            public const string StorageLevel = "/Image/Common/DMTree/solution_store_16x16.png";
        }

        public class MigrationCommonNodeIcon
        {
            public const string MigrationAgent = "/Image/Common/MigrationTree/agent_16x16.png";
        }

        public class FileMigrationSrcNodeIcon
        {
            public const string AgentGroup = "";
            public const string Connection = "/Image/Common/MigrationTree/connection_16x16.png";
            public const string Folder = "/Image/Common/MigrationTree/folder_16x16.png";
            public const string Items = "/Image/Common/MigrationTree/file_16x16.png";
            public const string Undefined = "/Image/Common/tryout_16x16_normal.png";
        }

        public class NotesMigrationSrcNodeIcon
        {
            public const string DominoServer = "/Image/Common/MigrationTree/domino_server_16x16.png";
            public const string Database = "/Image/Common/MigrationTree/database_16x16.png";
            public const string View = "/Image/Common/MigrationTree/view_16x16.png";
            public const string Document = "/Image/Common/MigrationTree/document_16x16.png";
            public const string Items = "/Image/Common/tryout_16x16_normal.png";
            public const string Undefined = "/Image/Common/tryout_16x16_normal.png";
        }

        public class eRoomMigrationSrcNodeIcon
        {
            public const string Community = "/Image/Common/MigrationTree/eroom_community_16x16.png";
            public const string Facility = "/Image/Common/MigrationTree/eroom_facility_16x16.png";
            public const string Room = "/Image/Common/MigrationTree/eroom_room_16x16.png";
            public const string List = "/Image/Common/MigrationTree/eroom_library_16x16.png";
            public const string Folder = "/Image/Common/MigrationTree/eroom_folder_16x16.png";
            public const string Items = "/Image/Common/MigrationTree/file_16x16.png";

            public const string HomeFolder = "/Image/Common/MigrationTree/eroom_library_16x16.png";
            public const string eRoomDiscussionPage = "/Image/Common/MigrationTree/eroom_discussion_16x16.png";
            public const string eRoomAllNotes = "/Image/Common/MigrationTree/eroom_note_16x16.png";
            public const string eRoomPollPage = "/Image/Common/MigrationTree/eroom_poll_16x16.png";
            public const string eRoomCalendarPage = "/Image/Common/MigrationTree/eroom_calendar_16x16.png";
            public const string eRoomProjectSchedulePage = "/Image/Common/MigrationTree/eroom_project_plan_16x16.png";
            public const string eRoomDBPage = "/Image/Common/MigrationTree/eroom_database_16x16.png";
            public const string eRoomAllLinks = "/Image/Common/MigrationTree/eroom_link_16x16.png";
            public const string eRoomInbox = "/Image/Common/MigrationTree/eroom_inbox_16x16.png";

            public const string Undefined = "/Image/Common/MigrationTree/tryout_16x16_normal.png";
        }

        public class LiveLinkMigrationSrcNodeIcon
        {
            public const string Connection = "/Image/Common/MigrationTree/connection_16x16.png";
            public const string Enterprise_Workspace = "/Image/Common/MigrationTree/enterprise_workspace_16x16.png";
            public const string Other_Accessible_Workspaces = "/Image/Common/MigrationTree/other_accessible_workspaces_16x16.png";
            public const string Poll = "/Image/Common/MigrationTree/poll_16x16.png";
            public const string Folder = "/Image/Common/MigrationTree/folder_16x16.png";
            public const string Appearance = "/Image/Common/MigrationTree/appearance_16x16.png";
            public const string Category = "/Image/Common/MigrationTree/category_16x16.png";
            public const string Channcel = "/Image/Common/MigrationTree/channcel_16x16.png";
            public const string Discussion = "/Image/Common/MigrationTree/discussion_16x16.png";
            public const string Domino_Server = "/Image/Common/MigrationTree/domino_server_16x16.png";
            public const string File = "/Image/Common/MigrationTree/file_16x16.png";
            public const string Project = "/Image/Common/MigrationTree/project_16x16.png";
            public const string Prospector = "/Image/Common/MigrationTree/prospector_16x16.png";
            public const string My_WorkSpace = "/Image/Common/MigrationTree/my_workspace_16x16.png";
            public const string Compound_Document = "/Image/Common/MigrationTree/compound_document_16x16.png";
            public const string Custom_View = "/Image/Common/MigrationTree/custom_view_16x16.png";
            public const string Document = "/Image/Common/MigrationTree/document_16x16.png";
            public const string URL = "/Image/Common/MigrationTree/url_16x16.png";
            public const string View = "/Image/Common/MigrationTree/view_16x16.png";
            public const string Shortcut = "/Image/Common/MigrationTree/shortcut_16x16.png";
            public const string Xml_DTD = "/Image/Common/MigrationTree/xml_dtd_16x16.png";
            public const string Task_List = "/Image/Common/MigrationTree/task_list_16x16.png";
            public const string Task_Group = "/Image/Common/MigrationTree/task_group_16x16.png";
            public const string Appearance_Workspace_Folder = "/Image/Common/MigrationTree/appearance_workspace_folder_16x16.png";
        }

        public class PublicFolderMigrationSrcNodeIcon
        {
            public const string ConnectionPublicFolder = "/Image/Common/MigrationTree/connection_public_folder_16x16.png";
            public const string CalenderItems = "/Image/Common/MigrationTree/calender_items_16x16.png";
            public const string ContactItems = "/Image/Common/MigrationTree/contact_items_16x16.png";
            public const string InfopathFormItems = "/Image/Common/MigrationTree/infopath_form_items_16x16.png";
            public const string JournalItems = "/Image/Common/MigrationTree/journal_items_16x16.png";
            public const string TaskItems = "/Image/Common/MigrationTree/task_items_16x16.png";
            public const string NoteItems = "/Image/Common/MigrationTree/note_items_16x16.png";
            public const string MailAndPostItems = "/Image/Common/MigrationTree/mail_and_post_items_16x16.png";
        }

        public class ExchangeOnlineTreeNodeIcon
        {
            public const string EOFram = "/Image/Common/Tree/MailboxTree/farm_16x16.png";
            public const string EOMailBox = "/Image/Common/Tree/MailboxTree/mailbox_16x16.png";
            public const string EOMailGroups = "/Image/Common/Tree/MailboxTree/mailbox_group_16x16.png";
            public const string EOMailFolder = "/Image/Common/Tree/MailboxTree/mail_folder_16x16.png";
            public const string EOInfoPathsFolder = "/Image/Common/Tree/MailboxTree/infopaths_folder_16x16.png";
            public const string EORSSFeedsFolder = "/Image/Common/Tree/MailboxTree/rss_feeds_folder_16x16.png";
            public const string EONotesFolder = "/Image/Common/Tree/MailboxTree/notes_folder_16x16.png";
            public const string EOCalendarFolder = "/Image/Common/Tree/MailboxTree/calendar_folder_16x16.png";
            public const string EOContactsFolder = "/Image/Common/Tree/MailboxTree/contacts_folder_16x16.png";
            public const string EOTasksFolder = "/Image/Common/Tree/MailboxTree/tasks_folder_16x16.png";
            public const string EOJournalFolder = "/Image/Common/Tree/MailboxTree/journal_folder_16x16.png";
            public const string EOItem = "/Image/Common/Tree/MailboxTree/item_16x16.png";
            public const string EOItems = "/Image/Common/Tree/MailboxTree/items_16x16.png";
            public const string EOFolders = "/Image/Common/Tree/MailboxTree/folders_16x16.png";
        }

        public class DownloadConstants
        {
            public const string GroupId = "GroupId";
            public const string UserId = "UserId";
            public const string UserStatusId = "StatusId";
            public const string FileName = "FileName";
            public const string Type = "Type";
            public const string Selection = "Selection";
            public const double PageSize = 10240;
        }

        //public class TreeConfigPath
        //{
        //    public const string CentralAdminScopeTree = "Etc/TreeSettings/ControlCAScopeTree.config";
        //    public const string CentralAdminSecuritySearchTree = "Etc/TreeSettings/ControlCASecuritySearchTree.config";
        //    public const string ContentManagerSrcTree = "Etc/TreeSettings/ControlCMSrcTree.config";
        //    public const string ContentManagerDestTree = "Etc/TreeSettings/ControlCMDestTree.config";
        //    public const string ContentManagerFilterPreviewTree = "Etc/TreeSettings/ControlCMFilterPreviewTree.config";
        //    public const string ContentManagerFSTree = "Etc/TreeSettings/ControlCMFSTree.config";
        //    public const string GranularBackupTree = "Etc/TreeSettings/ControlGranularBackupTree.config";
        //    public const string GranularRestoreTree = "Etc/TreeSettings/ControlGranularRestoreTree.config";
        //    public const string ContentManagerDeestOverviewTree = "Etc/TreeSettings/ControlCMDestOverviewTree.config";
        //    public const string StorageOptimizationTree = "Etc/TreeSettings/ControlStorageOptimizationTree.config";
        //    public const string GranularRestoreOutOfPlaceTree = "Etc/TreeSettings/ControlGranularRestoreOutOfPlaceTree.config";
        //    public const string ReplicatorSrcTree = "Etc/TreeSettings/ControlReplicatorSrcTree.config";
        //    public const string ReplicatorDestTree = "Etc/TreeSettings/ControlReplicatorDestTree.config";
        //    public const string PlatformBackupTree = "Etc/TreeSettings/ControlPlatformBackupTree.config";
        //    public const string PlatformRestoreTree = "Etc/TreeSettings/ControlPlatformRestoreTree.config";
        //    public const string PlatformRestoreOutOfPlaceTree = "Etc/TreeSettings/ControlPlatformRestoreOutOfPlaceTree.config";
        //    public const string ContentManagerImportTree = "Etc/TreeSettings/ControlCMImportTree.config";
        //    public const string CMDeleteContentTree = "Etc/TreeSettings/ControlCMDeleteContentTree.config";
        //    public const string ReplicatorFSTree = "Etc/TreeSettings/ControlReplicatorFSTree.config";
        //    public const string ReplicatorImportTree = "Etc/TreeSettings/ControlReplicatorImportTree.config";
        //}

        public class NodeConfig
        {
            public const int PerPage = 10;
        }

        public class JobSummaryKey
        {
            public const string SourceFarm = "SourceFarm";
            public const string TargetFarm = "TargetFarm";

            public const string WebAppCount = "WebAppCount";
            public const string FailedWebAppCount = "FailedWebAppCount";
            public const string SkippedWebAppCount = "SkippedWebAppCount";
            public const string FilteredWebAppCount = "FilteredWebAppCount";

            public const string SiteCollectionCount = "SiteCollectionCount";
            public const string FailedSiteCollectionCount = "FailedSiteCollectionCount";
            public const string SkippedSiteCollectionCount = "SkippedSiteCollectionCount";
            public const string FilteredSiteCollectionCount = "FilteredSiteCollectionCount";

            //For Exchange Online
            public const string ExChangeMailBoxCount = "MailBoxCount";
            public const string ExChangeFailedMailBoxCount = "FailedMailBoxCount";
            public const string ExChangeSkippedMailBoxCount = "SkippedMailBoxCount";

            public const string ExChangeFolderCount = "FolderCount";
            public const string ExChangeFailedFolderCount = "FailedFolderCount";
            public const string ExChangeSkippedFolderCount = "SkippedFolderCount";

            public const string ExChangeItemCount = "ItemCount";
            public const string ExChangeFailedItemCount = "FailedItemCount";
            public const string ExChangeSkippedItemCount = "SkippedItemCount";
            public const string SearchResultType = "SearchResultType";

            //For File Migration Folder Count
            public const string FolderCount = "FolderCount";
            public const string FailedFolderCount = "FailedFolderCount";
            public const string SkippedFolderCount = "SkippedFolderCount";
            public const string FilteredFolderCount = "FilteredFolderCount";

            //For Archive Folder Count
            public const string ArchiveFolderCount = "FolderCount";
            public const string ArchiveFailedFolderCount = "FailedFolderCount";
            public const string ArchiveSkippedFolderCount = "SkippedFolderCount";
            public const string ArchiveFilteredFolderCount = "FilteredFolderCount";

            public const string SiteCount = "SiteCount";
            public const string FailedSiteCount = "FailedSiteCount";
            public const string SkippedSiteCount = "SkippedSiteCount";
            public const string FilteredSiteCount = "FilteredSiteCount";

            public const string ListCount = "ListCount";
            public const string FailedListCount = "FailedListCount";
            public const string SkippedListCount = "SkippedListCount";
            public const string FilteredListCount = "FilteredListCount";

            public const string ProjectCount = "ProjectCount";
            public const string FailedProjectCount = "FailedProjectCount";
            public const string SkippedProjectCount = "SkippedProjectCount";
            public const string FilteredProjectCount = "FilteredProjectCount";

            public const string AppCount = "AppCount";
            public const string FailedAppCount = "FailedAppCount";
            public const string SkippedAppCount = "SkippedAppCount";
            public const string FilteredAppCount = "FilteredAppCount";
            public const string SuccessfulAppCount = "SuccessfulAppCount";
            public const string UpdateAvailableAppCount = "UpdateAvailableAppCount";

            public const string ItemCount = "ItemCount";
            public const string FailedItemCount = "FailedItemCount";
            public const string SkippedItemCount = "SkippedItemCount";
            public const string FilteredItemCount = "FilteredItemCount";
            public const string SuccessfulItemCount = "SuccessfulItemCount";
            public const string TotalSize = "TotalSize";
            public const string DataSize = "DataSize";
            public const string TransferredSize = "TransferredSize";
            public const string TotalCount = "TotalCount";


            public const string VersionCount = "VersionCount";
            public const string FailedVersionCount = "FailedVersionCount";

            #region spmigration user and group count
            public const string UserCount = "UserCount";
            public const string FailedUserCount = "FailedUserCount";
            public const string SkippedUserCount = "SkippedUserCount";
            public const string FilteredUserCount = "FilteredUserCount";

            public const string GroupCount = "GroupCount";
            public const string FailedGroupCount = "FailedGroupCount";
            public const string SkippedGroupCount = "SkippedGroupCount";
            public const string FilteredGroupCount = "FilteredGroupCount";
            #endregion

            public const string Comments = "Comments";
            public const string CommentForSubJob = "Comment";

            public const string SuccessfulSolutionCount = "SuccessfulSolutionCount";
            public const string FailedSolutionCount = "FailedSolutionCount";
            public const string SolutionSize = "SolutionSize";

            //EBS Stub upgrade
            public const string SuccessfullyUpgradedEBSStubs = "SuccessfullyUpgradedEBSStubsCount";
            public const string FailedUpgradedEBSStubs = "FailedUpgradedEBSStubsCount";

            /// <summary>
            /// 多个comment词条显示时做换行处理标记
            /// </summary>
            public const string Gui_NewLine = "Gui_NewLine";

            public const string FailedMessage = "FailedMessage";

            public const string DefragmenterStartSize = "DefragmenterStartSize";

            public const string DefragmenterEndSize = "DefragmenterEndSize";

            
            //DPM Compare Report
            public const string SourceURL = "SourceURL";
            public const string DestinationURL = "DestinationURL";
            public const string SourceOnlyObjectCount = "SourceOnlyObjectCount";
            public const string DestinationOnlyObjectCount = "DestinationOnlyObjectCount";
            public const string BothDifferenceObjectCount = "BothDifferenceObjectCount";
            public const string SameCount = "SameCount";
            public const string TotalSubJobDetails = "TotalSubJobDetails";

            //SO Comment
            public const string NodeNotExistingComment = "NodeNotExistingComment";
            public const string AssociatedJobID = "AssociatedJobID";

            //Report Center 
            public const string ExportLocation = "ExportLocation";

            //PhysicalRecordsJob
            public const string LocationCount = "LocationCount";
            public const string FailedLocationCount = "FailedLocationCount";
            public const string SkipLocationCount = "SkipLocationCount";
            public const string PhysicalBoxCount = "PhysicalBoxCount";
            public const string FailedPhysicalBoxCount = "FailedPhysicalBoxCount";
            public const string SkipPhysicalBoxCount = "SkipPhysicalBoxCount";
            public const string PhysicalFileCount = "PhysicalFileCount";
            public const string FailedPhysicalFileCount = "FailedPhysicalFileCount";
            public const string SkipPhysicalFileCount = "SkipPhysicalFileCount";
            public const string PhysicalRecordCount = "PhysicalRecordCount";
            public const string FailedPhysicalRecordCount = "FailedPhysicalRecordCount";
            public const string SkipPhysicalRecordCount = "SkipPhysicalRecordCount";
        }

        public class JobDetailInfoKeys
        {
            public const string Message = "JobDetail_Message";
            public const string SrcURL = "JobDetail_SrcURL";
            public const string DestURL = "JobDetail_DestURL";
            public const string PhysicalDevice = "JobDetail_PhysicalDevice";
            public const string SrcAgentHost = "JobDetail_SrcAgentHost";
            public const string DestAgentHost = "JobDetail_DestAgentHost";
            public const string MediaHost = "JobDetail_MediaHost";
            public const string Type = "JobDetail_Type";
            public const string Operator = "JobDetail_Operator";
            public const string Option = "JobDetail_Option";
            public const string Remark3 = "JobDetail_Remark3";
            public const string Remark4 = "JobDetail_Remark4";
            public const string Remark5 = "JobDetail_Remark5";
            public const string Remark6 = "JobDetail_Remark6";
        }
        #region Time dependent
        public class TimeFormatTemplate
        {
            public const string DATEPATTERN = "yyyy-MM-dd";
            public const string TIMEPATTERN = "HH:mm:ss";
            public const string DATETIMEPATERN = "yyyyMMddHHmmss";
            public const string LOCATION = "English(United States)";
            public const string LONGDATETIMEPATERN = "yyyyMMddHHmmssfff";
            public const string ShortDatePattern = "yyyyMMdd";

            public const string AuditorTimePattern = "yyyyMMddHHmmss";
            public const string AuditorIISLogTimePattern = "yyyy_MM_dd";
        }

        #endregion

        public class UserRegisterConstants
        {
            public const string RegsiterID = "RegisterID";
            public const string VerificationCode = "VerificationCode";
            public const string VerificationCodeRequest = "VerificationCodeRequest";
            public const string NewEditionVerificationCode = "NewEditionVerificationCode";
            public const string InviteID = "InviteID";
            public const int TrialDay = 30;
            public static readonly int[] MailNotificatedDays = { 10, 1, 0 };
            //一共提示11天
            public const int LoginShowInfoDay = 11;
            public const int MailNotificatedDay = 10;
        }

        #region SolutionName
        public class SolutionName
        {
            public const string ContentLibrary2010 = "SP2010ConnectorContentLibrary.wsp";
            public const string MediaLibrary2010 = "SP2010ConnectorMediaLibrary.wsp";
            public const string EndUserArchiver2010 = "SP2010EndUserArchiver.wsp";
            public const string SecurityManagement2010 = "SP2010SecurityManagement.wsp";
            public const string DocumentAuditing2010 = "SP2010DocumentAuditing.wsp";
            public const string AuditorMonitor2010 = "SP2010AuditorMonitor.wsp";
            #region SP2013
            public const string ContentLibrary2013 = "SP2013ConnectorContentLibrary.wsp";
            public const string MediaLibrary2013 = "SP2013ConnectorMediaLibrary.wsp";
            public const string EndUserArchiver2013 = "SP2013EndUserArchiver.wsp";
            public const string ErrorPageforArchivedData2013 = "SP2013ErrorPageforArchivedData.wsp";
            public const string SecurityManagement2013 = "SP2013SecurityManagement.wsp";
            #endregion
        }
        #endregion

        public class I18NConstants
        {
            public const string SystemOptionServiceKey = "AvePoint.ControlPanel.API.Service.SystemOption.SystemOptionService";

            public const string CentralAdminGui = "CentralAdmin.Gui";
            public const string CentralAdminService = "CentralAdmin.Service";
            public const string CentrlAdminGui = "CentrlAdmin.Gui";
            public const string Common = "Common";
            public const string CommonGui = "Common.Gui";
            public const string CommonGuiControls = "Common.GuiControls";
            //            public const string ComplianceGui = "Compliance.Gui";
            public const string ContentManagerGui = "ContentManager.Gui";
            public const string ContentManagerService = "ContentManager.Service";
            public const string ControlGui = "Control.Gui";
            public const string ControlPanel = "ControlPanel";
            public const string ControlPanelGui = "ControlPanel.Gui";
            public const string ControlPanelService = "ControlPanel.Service";
            public const string ControlPanelAPI = "ControlPanel.API";
            public const string ControlPanelWeb = "ControlPanel.Web";
            public const string DeploymentManagerGui = "DeploymentManager.Gui";
            public const string DeploymentManagerService = "DeploymentManager.Service";
            public const string GuiCommon = "Gui.Common";
            public const string GuiGui = "Gui.Gui";
            public const string ItemGui = "Item.Gui";
            public const string ItemService = "Item.Service";
            public const string MigrationGui = "Migration.Gui";
            public const string MigrationService = "Migration.Service";
            public const string PlatformRecoveryGui = "PlatformRecovery.Gui";
            public const string PlatformRecoveryService = "PlatformRecovery.Service";
            public const string ReplicatorGui = "Replicator.Gui";
            public const string ReplicatorService = "Replicator.Service";
            public const string ReportCenterCommon = "ReportCenter.Common";
            public const string ReportCentergui = "ReportCenter.gui";
            public const string ReportCenterGui = "ReportCenter.Gui";
            public const string ReportCenterService = "ReportCenter.Service";
            public const string ReportCenterServiceReportor = "ReportCenter.Service.Report";
            public const string ReportCenterServiceAuditor = "ReportCenter.Service.Auditor";
            public const string StorageOptimizationGui = "StorageOptimization.Gui";
            public const string StorageOptimizationGUI = "StorageOptimization.GUI";
            public const string StorageOptimizationService = "StorageOptimization.Service";
            public const string VAULT_SERVICE = "Vault.Service";
            public const string ServiceCommon = "Service.Common";
            public const string EDiscoveryService = "EDiscovery.Service";
            public const string EDiscoveryGui = "EDiscovery.Gui";
            public const string ExchangeOnlineGui = "ExchangeOnline.Gui";
            public const string ExchangeOnlineService = "ExchangeOnline.Service";
            public const string CloudAppAdminGui = "AzureAdManager.Gui";
            public const string CloudAppAdminService = "CloudAppAdmin.Service";

            
        }

        #region Clould
        public class ClouldConstants
        {
            public const string WebRoleName_DataProtection = "DataProtection.Web";
            public const string WebRoleName_Administration = "Administration.Web";
            public const string WebRoleName_ReportCenter = "ReportCenter.Web";
            public const string WebRoleName_Control = "Control.Web";
            public const string WebRoleName_Agent = "Control.Agent";
            public const string ApiRoleName_Agent = "DocAveApi.Agent";
        }
        #endregion
    }
}
