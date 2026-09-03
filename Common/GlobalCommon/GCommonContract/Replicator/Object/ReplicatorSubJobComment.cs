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



using System.Runtime.Serialization;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Replicator.Object
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum ReplicatorSubJobComment : int
    {
        [EnumMember]
        Unknown = 0,

        //Cannot find the web application in source.
        [EnumMember]
        CannotFindSourceWebApp = 1,

        //Cannot find the site collection in source.
        [EnumMember]
        CannotFindSourceSite = 2,

        //Cannot find the web in source.
        [EnumMember]
        CannotFindSourceWeb = 3,

        //Cannot find list in source.
        [EnumMember]
        CannotFindSourceList = 4,

        //Cannot find the folder in source.
        [EnumMember]
        CannotFindSourceFolder = 5,

        //The current web was not changed in the job, so no need to run this job.
        [EnumMember]
        NoChangeForWebJob = 6,

        //Discovered the web application failed.
        [EnumMember]
        DiscoverWebAppFailed = 7,

        //Discovered the site collection failed.
        [EnumMember]
        DiscoverSiteFailed = 8,

        //Discovered the web failed.
        [EnumMember]
        DiscoverWebFailed = 9,

        //Cannot find a usable exported job folder, so skip to import.
        [EnumMember]
        CanNotFindUsefulJobfolderInImport = 10,

        //Data transfer failed.
        [EnumMember]
        DataTransferFailed = 11,

        //An error occurred while running the mapping.
        [EnumMember]
        MappingWorkerError = 12,

        //Job completed successfully.
        [EnumMember]
        MappingSuccess = 13,

        //Job failed. Please view details for further information.
        [EnumMember]
        MappingFailed = 14,

        //Job completed with some unexpected errors. Please view details for further information.
        [EnumMember]
        MappingCompletedWithException = 15,

        [EnumMember]
        MappingLicenseExpired = 16,

        //UTC time is not consensus between primary and secondary
        [EnumMember]
        UTCTimeIsNotValidBetweenPrimaryAndSecondary = 17,

        //Start secondary process failed
        [EnumMember]
        StartSecondaryProcessFailed = 18,

        //There is no space available for replicator job in device location
        [EnumMember]
        NoSpaceAvailableError = 19,

        //Replicator job can not start because backup job failed  for roll back
        [EnumMember]
        MappingBackupBeforeFailed = 20,

        //Mapping job failed for no connnection to replicator configDb
        [EnumMember]
        MappingConfigDbFailed = 21,
        //Skip running this mapping as backup failed
        [EnumMember]
        GranularBackupFailedForManualInput = 22,

        //Access to this siteCollection has been blocked
        [EnumMember]
        AccessSiteBlocked = 23,
        //Replicator job can not start because the sitecollection Level node has been moved from ControlPanel
        [EnumMember]
        SCLevelNodeNotExist = 24,

        //The sign-in credentials does not match one in the Microsoft account system
        [EnumMember]
        LoginSharepointOnlineFailed = 25,

        [EnumMember]
        BackupBeforeStopped = 26,

        [EnumMember]
        BackupBeforeJobHasFailed = 27,
    }
}
