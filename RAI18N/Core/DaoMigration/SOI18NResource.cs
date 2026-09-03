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

namespace AvePoint.RA.I18N.Core.DaoMigration
{
    public class SOI18NResource
    {
        public static string Execution(string key, params object[] args)
        {
            return GetString(key, args);
        }

        static string GetString(string key, params object[] args)
        {

            switch (key)
            {
                #region  Common
                #region Job Monitor header  不全
                case RestoreType:
                    return Get("StorageOptimization.Service_FBBA6E2E-F048-4398-95FB-F8E496258606", "Restore Type");
                case RestoreSettings:
                    return Get("StorageOptimization.Service_0E5C0ACA-6895-4C61-8207-EFF22EB6E938", "Restore Settings");
                case StoragePolicy:
                    return Get("StorageOptimization.Service_F2D76A65-9F80-4DCB-82AF-A77F57E64B11", "Storage Policy");
                case StartTime:
                    return Get("StorageOptimization.Service_8D941EEC-F1BD-47E3-A2F2-1F170DFE921D", "Start Time");
                case SecurityProfile:
                    return Get("StorageOptimization.Service_73DEDABD-4DCB-43B8-AC2C-1F1BEF243347", "Security Profile");
                case WorkflowInstance:
                    return Get("StorageOptimization.Service_BDB17DD6-9CE9-40FC-BB93-B53F900E7FFE", "Include Workflow Instance");
                case WorkflowDefinition:
                    return Get("StorageOptimization.Service_B2C0A50C-BF2F-404B-963C-09F23D15F217", "Include Workflow Definition");
                case TheJobHasTimedOutComment:
                    return Get("StorageOptimization.Service_3DA64E2D-5C0F-421F-945F-0226CC58D3D8", "The job has timed out. The connections between the Control Service and Media Service or Agent are disconnected.");
                #endregion

                #region Job detail header
                // 这部分词条在JobDetailHeaderContainer里国际化
                #endregion
                #endregion

                #region Archiver
                #region Comments From Agent
                case ErrorBackupSiteCollection:
                    return Get("StorageOptimization.Service_F92B3313-5B1E-469C-90FB-D7AAF4867A72", "An error occurred while backing up site collection.");
                case ErrorBackupSite:
                    return Get("StorageOptimization.Service_F229B91D-E6F1-4672-A67D-9BE4DB7802C7", "An error occurred while backing up site.");
                case ErrorBackupList:
                    return Get("StorageOptimization.Service_8652676A-2EED-42D7-BF07-DCFFBFCA213B", "An error occurred while backing up list.");
                case ErrorBackupItem:
                    return Get("StorageOptimization.Service_5B3FB832-EC32-4457-B8D3-34A5987A83C0", "An error occurred while backing up item.");
                case ErrorDiscoveringData:
                    return Get("StorageOptimization.Service_9E55EEFB-73BE-486B-B04F-0F03982FDCF4", "An error occurred while discovering the data.");
                case ErrorDeletingItem:
                    return Get("StorageOptimization.Service_D1CF4044-9105-4506-A7A2-3C2AB7E36EBB", "An error occurred while deleting item.");
                case ErrorDeletingList:
                    return Get("StorageOptimization.Service_53C0BF12-530B-4FFA-BB36-E366B66C7530", "An error occurred while deleting list.");
                case ErrorDeletingSite:
                    return Get("StorageOptimization.Service_EE9CE868-B609-46E6-BC9C-4E5EFD9A89E4", "An error occurred while deleting site.");
                case ErrorDeletingSiteCollection:
                    return Get("StorageOptimization.Service_DCB24708-C38D-43B1-A6AB-A7D54ADA32C7", "An error occurred while deleting site collection.");
                case ErrorInMedia:
                    return Get("StorageOptimization.Service_A4A57B2F-8E30-412C-A088-8F1FF061837A", "An error occurred in the media.");
                case ListCantDelete:
                    return Get("StorageOptimization.Service_AB092717-1746-4043-B5E6-00540877C57A", "The list cannot be deleted.");
                case SeeDetailsForBackup:
                    return Get("StorageOptimization.Service_6851F097-3C78-4EAD-A59A-AAB9500F932A", "Please see details in \"Details for Backup\" tab.");
                case SeeDetailForDeletion:
                    return Get("StorageOptimization.Service_5F1C1DE3-896C-489C-9F9C-39508D42B4D4", "Please see details in \"Details for Deletion\" tab.");
                case RuleNotAvailableForScope:
                    return Get("StorageOptimization.Service_41CD2B66-2BA2-4B64-A8ED-68C2E915AFEF", "The applied rule {0} is not available.");
                case NoArchiverDB:
                    return Get("StorageOptimization.Service_3E44A53B-AD79-4D6F-AFCE-09C67767E151", "There is no available archiver database, please configure one first.");
                case BackupOnly:
                    return Get("StorageOptimization.Service_505E4E00-BE00-4A1F-A4C1-DDA58ADBCE1A", "Backup Only");
                case Archive:
                    return Get("StorageOptimization.Service_4C34D236-CD77-4BAD-9BEA-4028DDB7D381", "Archive");
                case SiteCollectionReadOnlyInCentralAdmin:
                    return Get("StorageOptimization.Service_9C73A4B4-02F9-4E8A-9370-963AB6414361", "The site collection has been setup to read-only in Central Admin");
                case SiteCollectionLockedInCentralAdmin:
                    return Get("StorageOptimization.Service_5B2C485E-7B8E-413B-A9F5-A86D0ABD8444", "The site collection has been locked in Central Admin");
                case ConfigsNodeIsNull:
                    return Get("StorageOptimization.Service_91F72CAE-F1F4-45E7-802E-086D661A58C9", "ConfigNodes is null or empty.");
                case NoEnabledRuleFound:
                    return Get("StorageOptimization.Service_24C79833-91D5-4A65-9023-81FEBEBD80E3", "No enabled rule found.");
                case NoDestination:
                    return Get("StorageOptimization.Service_5F1BBBF9-8721-4F38-9838-F63D8B8FCBF3", "The destination library doesn't exist.");
                case DestinationLibraryError:
                    return Get("StorageOptimization.Service_4CE11A1C-94E8-4605-8CA1-4F697509475B", "The destination shouldn't be the parent of the source scope or the source itself.");
                case CannotExecute:
                    return Get("StorageOptimization.Service_4635085E-DA5E-4A18-9591-08763C31FC74", "Cannot execute this library because this is the destination library.");
                case ThereIsAJobCurrently:
                    return Get("StorageOptimization.Service_01c69435-8aba-4212-bf6f-e98cd64dd43b", "There is a job currently running for the specified node, and this job is skipped.");
                #endregion

                #region Comments From Media
                case TheUserDoesNotHaveThePermissionForTheLogicalDevice:
                    return Get("StorageOptimization.Service_0BB29AC5-C839-486D-8B87-EB9589D271D9", "The user does not have the permission for the logical device.");
                case ThereIsNoEnoughSpaceInTheSpecifiedDevice:
                    return Get("StorageOptimization.Service_CCE4C046-0FF2-4B16-A89F-57047CBF5F4D", "There is no enough space in the specified device.");
                case AnErrorOccurredWhileTransferringDataToTheControlDatabase:
                    return Get("StorageOptimization.Service_6B40B088-0D9F-497D-ADE6-47CE382CEADB", "An error occurred while transferring data to the control database.");
                case SuccessfullyUpgradedTheIndex:
                    return Get("StorageOptimization.Service_4F030132-3EFE-4BE9-AD65-6A4EB0EEB1AD", "Successfully upgraded the index.");
                case FailedToUpgradeTheIndex:
                    return Get("StorageOptimization.Service_92FD5E69-40EE-4D4D-B206-4DD09AB0C62B", "Failed to upgrade the index.");
                case FailedToDeleteTheItem:
                    return Get("StorageOptimization.Service_2E0C0627-857A-4FB1-AA89-A73C6E3859C1", "Failed to delete the item.");
                case SuccessfullyDeletedTheItem:
                    return Get("StorageOptimization.Service_A390D0EF-DA20-40D2-AF63-EB267BB96BE3", "Successfully deleted the item.");
                case AnErrorOccurredWhileRunningTheBackupJob:
                    return Get("StorageOptimization.Service_F521E693-55C0-48C8-BB33-D81A7EACC328", "An error occurred while running the backup job.");
                case SuccessfullyRanTheBackupJob:
                    return Get("StorageOptimization.Service_F601B0F4-D788-4E46-A6B4-862E3FE9DFC3", "Successfully ran the backup job.");
                case CannotFindTheDataThatIsUsedToUpgrade:
                    return Get("StorageOptimization.Service_C031CD90-6003-4449-99FD-1E47047EF58D", "Cannot find the data that is used to upgrade.");
                case TheDataThatIsUsedToUpgradeAlreadyExists:
                    return Get("StorageOptimization.Service_B9C8034E-1860-4E08-BF47-749E553623AB", "The data that is used to upgrade already exists.");
                case AnErrorOccurredWhileRunningTheUpgradeJob:
                    return Get("StorageOptimization.Service_E560FD68-E975-41F7-89F8-F2BB262C7C35", "An error occurred while running the upgrade job.");
                case SuccessfullyRanTheUpgradeJob:
                    return Get("StorageOptimization.Service_3ED2D6E9-C0E4-4C9C-AB6F-76681B352107", "Successfully ran the upgrade job.");
                case AnErrorOccurredWhileRunningTheMaintenanceIob:
                    return Get("StorageOptimization.Service_73E07B56-C2DB-4248-A173-43A05C9CA323", "An error occurred while running the maintenance job.");

                case MergeSucessfully:
                    return Get("StorageOptimization.Service_B6775122-1015-4E00-8CE0-3D44BBAB237E", "Successfully merged the index.");
                case MergeFailed:
                    return Get("StorageOptimization.Service_FFBB3B63-312E-48A2-A359-83F789171A94", "Failed to merge the index.");
                case ArchiverDeviceReadOnly:
                    return Get("StorageOptimization.Service_1DF920DC-B158-41D6-A69E-8CE904B2EC8A", "The device is set to read-only.");
                case ArchiverMaintenanceSuccessfully:
                    return Get("StorageOptimization.Service_12D2337E-90E8-4487-AD47-FED09B35C2C4", "Successfully ran the maintenance job.");
                case ArchiverMaintenanceFailed:
                    return Get("StorageOptimization.Service_CBDE9B38-ABF8-4ECD-90BE-5264BFF290B7", "Failed to run the maintenance job.");
                case ArchiverRestoreFSSuccessfully:
                case ArchiverRestoreFSSuccessfully2:
                    return Get("StorageOptimization.Service_406EF91A-2836-403F-BD2D-184575E45169", "Successfully restored the data to file system.");
                case ArchiverRestoreToFSServiceErrorMessage:
                case ArchiverRestoreFSFailed:
                    return Get("StorageOptimization.Service_E671CAC6-21C0-4C50-8203-69663D025B3E", "Failed to restore the data to file system.");
                case ArchiverUpgradeSuccessfully:
                    return Get("StorageOptimization.Service_372F78EB-B8C6-4A17-95DC-D03A9093D668", "Successfully upgraded the Archiver data.");
                case ArchiverUpgradeFailed:
                    return Get("StorageOptimization.Service_3B199DE5-2587-446A-8210-2D5DABAC54C5", "Failed to upgrade the Archiver data.");
                case MapArchiveContentSuccessfully:
                    return Get("StorageOptimization.Service_6DD5A055-701A-4358-A1E1-391C52F963FF", "Successfully mapped the archived content.");
                case MapArchiveMetadataSuccessfully:
                    return Get("StorageOptimization.Service_F178E37A-E70B-4A57-988D-EDD224E53BDD", "Successfully mapped the metadata of the archived content.");
                case MapArchiveContentFailed:
                    return Get("StorageOptimization.Service_ECE5ADBB-0D4E-4E1E-B090-B9B6AEEA86AA", "The archived content has not been mapped.");
                case MapArchiveMetadataFailed:
                    return Get("StorageOptimization.Service_872F1DBB-F849-4DE0-954D-7CD6026FFBA2", "The metadata of the archived content has not been mapped.");
                case MapArchiveDataSuccessfully:
                    return Get("StorageOptimization.Service_2153BAE8-D1FD-4B92-BCC3-95A0F32C8243", "The archived data has not been mapped.");
                case FarmCanntUseThisPhysical:
                    return Get("StorageOptimization.Service_8DC6809C-53C7-4316-8F91-5D76E56C3B69", "The farm currently cannot be used by the physical device.");

                case NoDataAvailable:
                    return Get("StorageOptimization.Service_0DBF28F1-2BA2-4AD2-9011-A8EACAE47E82", "No data is available.");
                #endregion
                #endregion

                #region Job Settings

                case GeneralSettings:
                    return Get("StorageOptimization.Service_9F77C2FC-20A6-48DB-93A9-52F4210FED7D", "General Settings");
                case ProfileName:
                    return Get("StorageOptimization.Service_980EB10B-4BE7-4670-B3CA-C18982A87443", "Profile Name");
                case Rules:
                    return Get("StorageOptimization.Service_9B9C81A9-6D8F-4023-A194-CF13BE3CFF12", "Rules");
                case AdvancedSettings:
                    return Get("StorageOptimization.Service_DF70545E-84B5-4756-B960-F9BA732D6A31", "Advanced Settings");
                case IncludeWorkflowDefinition:
                    return Get("StorageOptimization.Service_1076287B-85C5-467E-8894-1443907FDB78", "Include workflow definition");
                case IncludeWorkflowInstance:
                    return Get("StorageOptimization.Service_74C6A112-85A5-455A-B7B4-7C1BEF10E28C", "Include workflow instance");
                case NotificationProfile:
                    return Get("StorageOptimization.Service_D94CC180-C1FB-4BD1-AD11-D95165D01BCB", "Notification Profile");

                case Destination:
                    return Get("StorageOptimization.Service_69C22AAE-19CE-4128-9E98-E269EBE9E7AD", "Destination");
                case ConflictResolution:
                    return Get("StorageOptimization.Service_948BC2E3-DFAC-4D35-B1AA-5D53B514F350", "Conflict Resolution");

                #endregion

                #region Job Report (Table Header)

                case JOB_REPORT_SUMMARY:
                    return Get("StorageOptimization.Service_028CC58E-C2F3-470B-AF60-0C49D782BDF1", "Summary");
                case JOB_REPORT_SETTINGS:
                    return Get("StorageOptimization.Service_76708d46-2c6b-44a2-a717-cc05e89daba9", "Job Settings");
                case JOB_REPORT_DELETION_DETAILS:
                    return Get("StorageOptimization.Service_AA57D206-0BFD-414D-861B-C62343BCA282", "Deletion Details");
                case JOB_REPORT_BACKUP_DETAILS:
                    return Get("StorageOptimization.Service_CFCDD07B-6A40-40C9-B45D-D2DF7E325870", "Backup Details");
                case JOB_REPORT_RECORD_DECLARATION_DETAILS:
                    return Get("StorageOptimization.Service_54093093-0aa1-4b87-a065-824d575b0041", "Record Declaration Details");
                case JOB_REPORT_EXPORT_DETAILS:
                    return Get("StorageOptimization.Service_57D69E5A-BF27-489E-8A8D-99D6E8379FD8", "Export Details");
                case JOB_REPORT_FILERETETION_DETAILS:
                    return Get("StorageOptimization.Service_69377537-0160-4BD3-A02B-374FDE862958", "File Retention Details");
                case JOB_REPORT_REMOVEDSTUB_DETAILS:
                    return Get("StorageOptimization.Service_0C738F86-169C-479B-8900-275351848E10", "Removed Stub Details");
                #endregion

                #region ==动态 job monitor summary==
                case SO_JOB_INFORMATION:
                    return Get("StorageOptimization.Service_a766b582-1eaf-467f-bcbf-dcca13fe4a64", "Job Information");
                case SO_SCOPE:
                    return Get("StorageOptimization.Service_e07f0714-82b8-4b84-a9fe-22be557ee5bf", "Scope");
                case SO_JOB_ID:
                    return Get("StorageOptimization.Service_6a86c596-68bc-4d35-849d-7b164486bde3", "Job ID");
                case SO_ORIGINAL_JOB_ID:
                    return Get("StorageOptimization.Service_b84231d1-cbb0-440a-b5ac-0f4f3f06cea5", "Original Job ID");
                case SO_PLAN_TYPE:
                    return Get("StorageOptimization.Service_89647092-ff08-46dc-8696-302b2f68d0e9", "Plan Type");
                case SO_START_TIME:
                    return Get("StorageOptimization.Service_8f544075-f0f4-4632-a1dd-1168bbe89709", "Start Time");
                case SO_END_TIME:
                    return Get("StorageOptimization.Service_61ba9d11-127e-4706-9830-d208b360881e", "Finish Time");
                case SO_JOB_OPERATED_BY:
                    return Get("StorageOptimization.Service_d8a66012-890c-4905-af05-7c61b5970742", "Job Operated By");
                case SO_DATA_TYPE:
                    return Get("StorageOptimization.Service_a12e6785-0746-4efc-85fe-48b4e051a0fb", "Data Type");
                case SO_SCHEDULED_RULE_ENABLED:
                    return Get("StorageOptimization.Service_dc0064ac-f746-483c-a668-ab0592cf4856", "Scheduled Rule Enabled");
                case SO_STATISTICS:
                    return Get("StorageOptimization.Service_4f0bd5df-04eb-432b-9c0f-a908f0d005da", "Statistics");
                case SO_STATUS:
                    return Get("StorageOptimization.Service_955d4d30-3430-462f-a541-aa5b253e1aee", "Status");
                case SO_COMMENTS:
                    return Get("StorageOptimization.Service_4d50cfce-02fb-4d7c-9674-75db9602a021", "Comment");
                case SO_NUMBER_OF_SUCCEEDED_OBJECTS:
                    return Get("StorageOptimization.Service_b3335749-3177-477b-9cfb-b0399735d375", "The Number of Successful Objects");
                case SO_NUMBER_OF_FAILED_OBJECTS:
                    return Get("StorageOptimization.Service_5ba95ea5-fe8d-4127-9de0-c2d8f1b47fcb", "The Number of Failed Objects");
                case SO_NUMBER_OF_SKIPPED_OBJECTS:
                    return Get("StorageOptimization.Service_844ccb69-243c-4cc4-93e0-cbaa7af0aa1a", "The Number of Skipped Objects");
                case SO_TOTALSIZE:
                    return Get("StorageOptimization.Service_e722bc3c-1964-4bcf-8679-1e7964686429", "Total Size");
                case HasRunningMoveIndexJob:
                    return Get("ControlPanel.Service_866ab185-e7d9-4f3e-9eca-4773a395d430", "The job skipped because there is a move index device running.");
                case DataSizeOutofLimit:
                    //return I18NRespository.Get(string.Empty, key, args);
                    return Get("ControlPanel.Service_a213cb5b-544c-4936-ba56-528d7e79442f", "Datasize in device is already out of limit.");
                case ArchiverRehydrationAzureBlobComments:
                    return Get("StorageOptimization.Service_9C1BEB52-81AB-4198-94A6-C8EFFBFC10A4", "There are some backup data in the Azure archive tier, It takes a lot of time to rehydrate it from the archive tier.");
                case BlockedArchiverRehydrationAzureBlobComments:
                    return Get("StorageOptimization.Service_16a1ef5b-5403-4ac7-b040-bbf9c058c1b9", "The current job contains data in the Azure archive tier, and the current setting disables endUser to restore data in the Archive tier.");
                case ExportOutLimit:
                    return Get("StorageOptimization.Service_CAE6D9E5-054E-4A06-B6A3-D3D96C966C7B", "The total size of exported and/or restored data has exceeded the allowed storage limit.");
                #endregion
                default:
                    return string.Empty;
            }
        }

        public static string GetMessageByKey(string messageKey, object[] args = null)
        {
            switch (messageKey)
            {
                case SO_JOB_ID:
                    return Get("StorageOptimization.Service_6a86c596-68bc-4d35-849d-7b164486bde3", "Job ID");
                case SO_DATA_TYPE:
                    return Get("StorageOptimization.Service_a12e6785-0746-4efc-85fe-48b4e051a0fb", "Data Type");
                case SO_START_TIME:
                    return Get("StorageOptimization.Service_8f544075-f0f4-4632-a1dd-1168bbe89709", "Start Time");
                case SO_END_TIME:
                    return Get("StorageOptimization.Service_61ba9d11-127e-4706-9830-d208b360881e", "Finish Time");
                case SO_JOB_OPERATED_BY:
                    return Get("StorageOptimization.Service_d8a66012-890c-4905-af05-7c61b5970742", "Job Operated By");
                default:
                    return messageKey;
            }
        }

        #region Constants String Definition

        #region  Archiver

        #region  Job Comment,  Message from Agent
        public const string ErrorBackupSiteCollection = "An error occurred while backing up site collection.";
        public const string ErrorBackupSite = "An error occurred while backing up site.";
        public const string ErrorBackupList = "An error occurred while backing up list.";
        public const string ErrorBackupItem = "An error occurred while backing up item.";
        public const string ErrorDiscoveringData = "An error occurred while discovering the data.";
        public const string ErrorDeletingItem = "An error occurred while deleting item.";
        public const string ErrorDeletingList = "An error occurred while deleting list.";
        public const string ErrorDeletingSite = "An error occurred while deleting site.";
        public const string ErrorDeletingSiteCollection = "An error occurred while deleting site collection.";
        public const string ErrorInMedia = "An error occurred in the media.";
        public const string ListCantDelete = "The list cannot be deleted.";
        public const string SeeDetailsForBackup = "Please see details in \"Details for Backup\" tab.";
        public const string SeeDetailForDeletion = "Please see details in \"Details for Deletion\" tab.";
        public const string RuleNotAvailableForScope = "The applied rule {0} is not available.";
        public const string NoArchiverDB = "There is no available archiver database, please configure one first.";
        public const string BackupOnly = "Backup Only";
        public const string Archive = "Archive";
        public const string SiteCollectionReadOnlyInCentralAdmin = "The site collection has been setup to read-only in Central Admin";
        public const string SiteCollectionLockedInCentralAdmin = "The site collection has been locked in Central Admin";
        public const string ConfigsNodeIsNull = "ConfigNodes is null or empty.";
        public const string NoEnabledRuleFound = "No enabled rule found.";
        public const string NoDestination = "The destination library doesn't exist";
        public const string DestinationLibraryError = "The destination shouldn't be the parent of the source scope or the source itself.";
        public const string CannotExecute = "Cannot execute this library because this is the destination library.";
        #endregion

        #region Job Comments from Media

        public const string TheUserDoesNotHaveThePermissionForTheLogicalDevice = "The user does not have the permission for the logical device.";
        public const string ThereIsNoEnoughSpaceInTheSpecifiedDevice = "There is no enough space in the specified device.";
        public const string AnErrorOccurredWhileTransferringDataToTheControlDatabase = "An error occurred while transferring data to the control database.";
        public const string SuccessfullyUpgradedTheIndex = "Successfully upgraded the index.";
        public const string FailedToUpgradeTheIndex = "Failed to upgrade the index.";
        public const string SuccessfullyDeletedTheItem = "Successfully deleted the item.";
        public const string FailedToDeleteTheItem = "Failed to delete the item.";
        public const string AnErrorOccurredWhileRunningTheBackupJob = "An error occurred while running the backup job.";
        public const string SuccessfullyRanTheBackupJob = "Successfully ran the backup job.";
        public const string CannotFindTheDataThatIsUsedToUpgrade = "Cannot find the data that is used to upgrade.";
        public const string TheDataThatIsUsedToUpgradeAlreadyExists = "The data that is used to upgrade already exists.";
        public const string AnErrorOccurredWhileRunningTheUpgradeJob = "An error occurred while running the upgrade job.";
        public const string SuccessfullyRanTheUpgradeJob = "Successfully ran the upgrade job.";
        public const string AnErrorOccurredWhileRunningTheMaintenanceIob = "An error occurred while running the maintenance job.";

        public const string MergeSucessfully = "Successfully merged the index.";
        public const string MergeFailed = "Failed to merge the index.";
        public const string ArchiverDeviceReadOnly = "The device is set to read-only.";
        public const string ArchiverMaintenanceSuccessfully = "Successfully ran the maintenance job.";
        public const string ArchiverMaintenanceFailed = "Failed to run the maintenance job.";
        public const string ArchiverRestoreFSSuccessfully = "Successfully restored the data to file system.";
        public const string ArchiverRestoreFSSuccessfully2 = "ArchiverRestoreToFSSuccessfulMessage";
        public const string ArchiverRestoreFSFailed = "Failed to restore the data to file system.";
        public const string ArchiverUpgradeSuccessfully = "Successfully upgraded the Archiver data.";
        public const string ArchiverUpgradeFailed = "Failed to upgrade the Archiver data.";
        public const string MapArchiveContentSuccessfully = "Successfully mapped the archived content.";
        public const string MapArchiveMetadataSuccessfully = "Successfully mapped the metadata of the archived content.";
        public const string MapArchiveContentFailed = "The archived content has not been mapped.";
        public const string MapArchiveMetadataFailed = "The metadata of the archived content has not been mapped.";
        public const string MapArchiveDataSuccessfully = "The archived data has not been mapped.";
        public const string FarmCanntUseThisPhysical = "The farm currently cannot be used by the physical device.";
        public const string ArchiverRestoreToFSServiceErrorMessage = "ArchiverRestoreToFSServiceErrorMessage";
        public const string NoDataAvailable = "No data is available.";
        #endregion

        #region Job Settings
        public const string GeneralSettings = "General Settings";
        public const string ProfileName = "Profile Name";
        public const string Rules = "Rules";
        public const string AdvancedSettings = "Advanced Settings";
        public const string IncludeWorkflowDefinition = "Include workflow definition";
        public const string IncludeWorkflowInstance = "Include workflow instance";
        public const string NotificationProfile = "Notification Profile";
        public const string Destination = "Destination";
        public const string ConflictResolution = "Conflict Resolution";
        #endregion

        #endregion

        #region Common
        public const string Rule = "Rule";
        public const string RestoreType = "RestoreType";
        public const string RestoreSettings = "RestoreSettings";
        public const string StoragePolicy = "StoragePolicy";
        public const string StartTime = "StartTime";
        public const string SecurityProfile = "SecurityProfile";
        public const string WorkflowInstance = "WorkflowInstance";
        public const string WorkflowDefinition = "WorkflowDefinition";
        public const string TheJobHasTimedOutComment = "The job has timed out. The connections between the Control Service and Media Service or Agent are disconnected.";
        public const string DataSizeOutofLimit = "Datasize in device is already out of limit";
        public const string HasRunningMoveIndexJob = "Has running move index job";
        public const string ThereIsAJobCurrently = "There is a job currently running for the specified node, and this job is skipped.";
        public const string ArchiverRehydrationAzureBlobComments = "ArchiverRehydrationAzureBlobComments";
        public const string BlockedArchiverRehydrationAzureBlobComments = "BlockedArchiverRehydrationAzureBlobComments";
        public const string ExportOutLimit = "ExportOutLimit";
        #endregion

        #region  job detail header
        // 这部分词条在JobDetailHeaderContainer里国际化
        #endregion

        #endregion

        #region ==动态 job monitor summary==
        public const string SO_JOB_INFORMATION = "SO_JobInformation";
        public const string SO_SCOPE = "SO_Scope";
        public const string SO_JOB_ID = "SO_JobId";
        public const string SO_PLAN_TYPE = "SO_PLAN_TYPE";
        public const string SO_ORIGINAL_JOB_ID = "SO_OriginalJobId";
        public const string SO_START_TIME = "SO_StartTime";
        public const string SO_END_TIME = "SO_EndTime";
        public const string SO_JOB_OPERATED_BY = "SO_JobOperatedBy";
        public const string SO_DATA_TYPE = "SO_DataType";
        public const string SO_SCHEDULED_RULE_ENABLED = "SO_ScheduledRuleEnabled";
        public const string SO_STATISTICS = "SO_Statistics";
        public const string SO_STATUS = "SO_Status";
        public const string SO_COMMENTS = "SO_Comments";
        public const string SO_NUMBER_OF_SUCCEEDED_OBJECTS = "SO_NumberOfSucceededObjects";
        public const string SO_NUMBER_OF_FAILED_OBJECTS = "SO_NumberOfFailedObjects";
        public const string SO_NUMBER_OF_SKIPPED_OBJECTS = "SO_NumberOfSkippedObjects";
        public const string SO_TOTALSIZE = "SO_TotalSize";

        //public static string I18NJobStatistics(int allCount, int webAppCount, int siteCollectionCount, int siteCount, int listCount, int itemCount)
        //{
        //    return Get("StorageOptimization.Service_9325a3c2-c855-4ecb-a131-66817d511270", "{0}(Site Collection: {1}; Site: {2}; List: {3}; Item: {4})",
        //        allCount, siteCollectionCount, siteCount, listCount, itemCount);
        //}
        #endregion

        public const string JOB_REPORT_SETTINGS = " Job Settings";

        public const string JOB_REPORT_PLAN_SETTINGS = "Plan Setting";

        public const string JOB_REPORT_DETAIL = " Details";

        public const string JOB_REPORT_SUMMARY = " Summary";

        public const string JOB_REPORT_DELETION_DETAILS = "Deletion Details";

        public const string JOB_REPORT_BACKUP_DETAILS = "Backup Details";

        public const string JOB_REPORT_EXPORT_DETAILS = "Export Details";

        public const string JOB_REPORT_FILERETETION_DETAILS = "File Retention Details";

        public const string JOB_REPORT_REMOVEDSTUB_DETAILS = "Removed Stub Details";

        public const string JOB_REPORT_RECORD_DECLARATION_DETAILS = "Record Declaration Details";

        public static string Get(string key, string defaultValue, params object[] args)
        {
            var value = I18NEntity.GetString(key, args);
            if (string.IsNullOrEmpty(key))
            {
                return defaultValue;
            }
            return value;
        }
    }
}
