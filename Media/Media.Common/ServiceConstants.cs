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




namespace AvePoint.Media.Common
{
    #region using directives

    using System;
    using System.Configuration;
    using System.Diagnostics.CodeAnalysis;
    using AvePoint.GCommon.Contract.CodeReview;

    #endregion using directives

    #region CodeReview

    [AveCodeReview(
        "2011/12/23",
        "yhzhang@avepoint.com",
        "yhzhang@avepoint.com",
        new string[] { },
        null,
        true)]

    #endregion CodeReview

    public static class ServiceConstants
    {
        public static readonly String ServiceName = ConfigurationManager.AppSettings.Get("ServiceName");
        public static readonly String ServiceDisplayName = ConfigurationManager.AppSettings.Get("ServiceDisplayName");
        public static readonly String ServiceDescription = ConfigurationManager.AppSettings.Get("ServiceDescription");
        public static readonly String ServiceDependOnServiceNetTcpPortSharing = "NetTcpPortSharing";

        public static readonly String ServiceTraceSourceName = "MediaService";
        public static readonly Int32 ServiceEventId = 6001;

        public static readonly String JobStatusUpdateServiceThreadName = "MediaJobStatusUpdateThreadIdentifier";
        public static readonly String IocPropertiesConfigurations = "MediaIocPropertiesConfigurations.config";
        public static readonly String Log4NetCongfigurations = "MediaLog4Net.config";
        public static readonly String ServerHostOrIpAddress = "mediaServerHostOrIpAddress";
        public static readonly String ServerControlPort = "mediaServerControlPort";
        public static readonly String ServerDataPort = "mediaServerDataPort";

        public static readonly String IndexDatabaseInitialScriptPathTemplate = "AvePoint.Media.Core.Index.CoreIndexScripts.InitialScripts.{0}IndexDatabaseScript.sqlite";
        public static readonly String IndexDatabaseUpgradeScriptPathTemplate = "AvePoint.Media.Core.Index.CoreIndexScripts.UpgradeScripts.{0}IndexDatabaseScript.config";

        public static readonly Char ExtraChar = (Char)0x13;
        public static readonly Char Delimiter = (Char)0x12;

        public static readonly String NetAppArchiverPath = "data_archiver";
        public static readonly String NetAppRootPath = "SMMOSS";
        public static readonly String ArchiverPath = "data_archive";    // SPO,OneDrive
        public static readonly String FSArchiverPath = "data_fs_archive";
        public static readonly String TeamsArchiverPath = "data_teams_archive";
        public static readonly String EXOArchiverPath = "data_exo_archive"; // mailbox
        public static readonly String GoogleArchiverPath = "data_google_archive"; // Google Drive
        public static readonly String ExtenderPath = "Data_Extender";
        public static readonly String RealtimeArchiverPath = "data_realtime_archive";
        public static readonly String VaultPath = "data_vault";
        public static readonly String PlatformPath = "data_platform";
        public static readonly String GranularPath = "data_granular";
        public static readonly String GeneralPath = "data_general";
        public static readonly String SolutionPath = "data_solution";
        public static readonly String SolutionPath5X = "BackupSolutions";
        public static readonly String ReplicatorPath = "Replicator2010";
        public static readonly String IndexDBName = "index.db";
        public static readonly String DedupIndexDBName = "dedup_index.db";
        public static readonly String Granular58MainIndexDBName = "mainIndex.db";
        public static readonly String ArchiverIndexDBName = "archiver_index.db";
        public static readonly String GranularIndexDBName = "granular_index.db";
        public static readonly String PlatformIndexDBName = "platform_index.db";
        public static readonly String MetaDataCacheName = "meta_cache.dat";
        public static readonly String ContentDataCacheName = "content_cache.dat";
        public static readonly String DBPropertiesName = ".properties";
        public static readonly String ModifyTimeHeader = "ModifyTime#";

        public static readonly String DefaultIndexPath = "index";
        public static readonly String DefaultIndexVolume = "IndexVolume";
        public static readonly String DefaultDataVolume = "DataVolume";
        public static readonly String MetaDataHeader = "meta";
        public static readonly String ContentDataHeader = "content";
        public static readonly String ArchiverFullTextIndexPath = "index_archive";
        public static readonly String GranularFullTextIndexPath = "index_granular";

        public static readonly String SecurityKeyName = "SecurityKey50";
        public static readonly String VersionPropertyName = "version";
        public static readonly String WorkingSite = "WorkingSite";
        public static readonly String Order = "Order";

        public static readonly String StringSendToAgent = "<items/>";
        public static readonly String CatalogName = "catalog.idx";
        public static readonly String NetAppHeadIdxName = "head.idx";
        public static readonly String NetAppJobPropertyName = "job.properties";

        public static readonly String Document = "Document";
        public static readonly String DocumentVersion = "Document Version";
        public static readonly String Item = "Item";
        public static readonly String ItemVersion = "Item Version";

        public static readonly Int32 MergeIndexLimit = 32775;
        public static readonly Int32 FullTextIndexLimit = 1000;
        public static readonly String GranularFulltextIndexMessage = "<GranularFullTextIndex />";
        public static readonly String ArchiverFulltextIndexMessage = "<ArchiverFullTextIndex />";
        public static readonly String ArchiverRetentionSuccessfulMessage = "Successfully ran the maintenance job.";
        public static readonly String ArchvierRetentionFailedMessage = "Failed to run the maintenance job.";
        public static readonly String MergeReportSuccessfulMessage = "Successfully merged the index.";
        public static readonly String MergeReportFailedMessage = "Failed to merge the index.";
        public static readonly String ArchiverRestoreToFSSuccessfulMessage = "Successfully restored the data to file system.";
        public static readonly String ArchiverRestoreToFSFailedMessage = "Failed to restore the data to file system.";
        public static readonly String MergeDetailType = "Site Collection";
        public static readonly String NotMappedContentMessage = "The archived content has not been mapped.";
        public static readonly String MappedContentMessage = "Successfully mapped the archived content.";
        public static readonly String NotMappedMetaDataMessage = "The metadata of the archived content has not been mapped.";
        public static readonly String MappedMetaDataMessage = "Successfully mapped the metadata of the archived content.";
        public static readonly String ArchiverUpgradeDataSuccessedMessage = "Successfully upgraded the Archiver data.";
        public static readonly String ArchiverUpgradeDataFailedMessage = "Failed to upgrade the Archiver data.";
        public static readonly String UpgradeDataSuccessedMessage = "Successfully upgraded the index.";
        public static readonly String UpgradeDataFailedMessage = "Failed to upgrade the index.";
        public static readonly String RemoveDataSuccessedMessage = "Successfully deleted the item.";
        public static readonly String RemoveDataFailedMessage = "Failed to delete the item.";
        public static readonly String UpgradeDataNotMappedAllMessage = "The archived data has not been mapped.";
        public static readonly String PlatformBackupRetetionMessage = "The specified destination device does not have enough space.";
        public static readonly String PlatformBackupUpgradeServiceMessage = "An error occurred while transferring data to the control database.";
        public static readonly String FarmCannotBeUsedByPhysicalDevice = "The farm currently cannot be used by the physical device";
        public static readonly String GranularImportWithoutData = "Cannot find the data that is used to upgrade.";
        public static readonly String GranularImportDataImported = "The data has been imported.";
        public static readonly String GranularUpgradeErrorOccurred = "An error occurred while transferring data to the control database.";


        public static readonly String EdiscoveryExportSuccessfulMessage = "Successfully ran the export job.";
        public static readonly String EdiscoveryExportFailedMessage = "Failed to run the export job.";
        public static readonly String EdiscoveryHoldFSuccessfulMessage = "Successfully ran the hold job.";
        public static readonly String EdiscoveryHoldFailedMessage = "Failed to run the hold job.";
        public static readonly String EdiscoveryReleaseSuccessfulMessage = "Successfully ran the release job.";
        public static readonly String EdiscoveryReleaseFailedMessage = "Failed to run the release job.";
        public static readonly String EdiscoverySearchSuccessfulMessage = "Successfully ran the search job.";
        public static readonly String EdiscoverySearchFailedMessage = "Failed to run the search job.";

        public static readonly String SkipStatus = "Skip";
        public static readonly String NewCreateStatus = "New Created";
        public static readonly String OverwriteStatus = "Overwritten";

        public static readonly Int32 EntrySize = 64 * 1024;
        public static readonly String SharedSearchSetting = "SharedSearchSetting";
        public static readonly String SspCatalogFile = "SSPCatalog.idx";
        public static readonly String FewListName = "few_list.idx";

        public static readonly String VersionGuid = "AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA";
        public static readonly String VersionTemplateValue = "6.0.0.0";
        public static readonly String UpgradeConfigurationSectionHandlerName = "upgradeConfigurationSectionHandler";

        public static readonly String EncryptionInfoKey = "EncryptionInfo";
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public static readonly String MediaServiceCommandLinePassphase = "b181fc16163b389c76aa15f786ce753e";

        public static readonly String DocAve = "DocAve";

        public const String DocAveProduct = "DocAve";
        public const String NetAppProduct = "NetApp";
        public const String IBMOemProduct = "IBMOem";

        public const String DocAveProductIdPrefix6X = "6X";
        public const String DocAveProductIdPrefix5X = "5X";
        public const String DocAveProductIdPrefix4X = "4X";

        //Sample
        public const String NetAppProductIdPrefixCifs = "Cifs";
        public const String NetAppProductIdPrefix6X = "6X";
        public const String NetAppProductIdPrefix5X = "5X";
        public const String NetAppProductIdPrefix4X = "4X";

        public static readonly String SQLiteX86ResourcePath = "AvePoint.Media.SupportAssembly.X86.System.Data.SQLite.DLL";
        public static readonly String SQLiteX64ResourcePath = "AvePoint.Media.SupportAssembly.X64.System.Data.SQLite.DLL";
        public static readonly String SQLiteDll = "System.Data.SQLite.DLL";

        public static readonly String MediaGarbageCollectionThreadIdentifier = "MediaGarbageCollectionThread";

        public static readonly String DataFileNameExtention = ".dat";

        //job detail job summary
        public static readonly Int32 HandleItemSucceeded = 0;
        public static readonly Int32 HandleItemFailed = 1;
        public static readonly Int32 HandleItemSkiped = 2;

        //Solution Remove job summary
        public static readonly string ComponentCount = "ComponentCount";
        public static readonly string FailedComponent = "FailedComponent";
        public static readonly string FailedMessage = "FailedMessage";

        public const Int32 JobRunning = 0;
        public const Int32 JobFinished = 2;
        public const Int32 JobFailed = 3;
        public const Int32 JobStopped = 4;
        public const Int32 JobPaused = 5;
        public const Int32 JobSkipped = 6;
        public const Int32 JobFinishedWithException = 7;
        public const Int32 JobStopping = 9;
        public const Int32 JobPausing = 10;

        public static readonly Int32 MemoryStreamLimit = 100;

        public static readonly String IdentityTypeJobId = "JobId";
        public static readonly String IdentityTypeGroupId = "GroupId";
        public static readonly String IdentityTypeTenant = "Tenant";
        public const string ExchangeBlankSubject = AvePoint.GCommon.Contract.Agent.ExchangeBrowser.Object.ExchangeOnlineBrowserConstants.EXCHANGE_ONLINE_EMPTY_MAIL_SUBJECT;
    }
}