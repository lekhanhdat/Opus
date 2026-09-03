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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives

    using System;
    using System.Collections.Generic;

    #endregion using directives

    public abstract class StorageInfoMetaDataBuilderBase<TBackupJob>
        : IStorageInfoMetaDataBuilder
        where TBackupJob : BackupJobBase
    {
        public String MetaDataKeyNamePlatform { get { return "Platform"; } }

        public String MetaDataKeyNameComponent { get { return "Component"; } }

        public String MetaDataKeyNameOriginalFileName { get { return "OriginalFileName"; } }

        public String MetaDataKeyNameGranularFarmName { get { return "Granular-FarmName"; } }

        public String MetaDataKeyNameGranularWebAppName { get { return "Granular-WebAppName"; } }

        public String MetaDataKeyNameGranularSiteCollectionName { get { return "Granular-SiteCollectionName"; } }

        public String MetaDataKeyNameGranularPlanId { get { return "Granular-PlanId"; } }

        public String MetaDataKeyNameGranularCycleId { get { return "Granular-CycleId"; } }

        public String MetaDataKeyNameGranularJobId { get { return "Granular-JobId"; } }

        public String MetaDataKeyNameGranularDataMode { get { return "Granular-DataMode"; } }

        public String MetaDataKeyNameGeneralPlanId { get { return "General-PlanId"; } }

        public String MetaDataKeyNameGeneralJobId { get { return "General-JobId"; } }

        public String MetaDataKeyNameGeneralDataMode { get { return "General-DataMode"; } }

        public String MetaDataKeyNamePlatformFarmName { get { return "Platform-FarmName"; } }

        public String MetaDataKeyNamePlatformPlanId { get { return "Platform-PlanId"; } }

        public String MetaDataKeyNamePlatformCycleId { get { return "Platform-CycleId"; } }

        public String MetaDataKeyNamePlatformJobId { get { return "Platform-JobId"; } }

        public String MetaDataKeyNamePlatformDataMode { get { return "Platform-DataMode"; } }

        public String MetaDataKeyNameArchiverFarmName { get { return "Archive-FarmName"; } }

        public String MetaDataKeyNameArchiverPlanId { get { return "Archive-PlanId"; } }

        public String MetaDataKeyNameArchiverWebAppName { get { return "Archive-WebAppName"; } }

        public String MetaDataKeyNameArchiverSiteCollectionName { get { return "Archive-SiteCollectionName"; } }

        public String MetaDataKeyNameArchiverJobId { get { return "Archive-JobId"; } }

        public String MetaDataKeyNameArchiverKeepTime { get { return "Archive-KeepTime"; } }

        public String MetaDataKeyNameArchiverBackupTime { get { return "Archive-BackupTime"; } }

        public String MetaDataKeyNameArchiverDataMode { get { return "Archive-DataMode"; } }

        public String MetaDataKeyNameArchiverSnapLock { get { return "Archive-SnapLock"; } }

        public String MetaDataKeyNameVaultFarmName { get { return "Vault-FarmName"; } }

        public String MetaDataKeyNameVaultWebAppName { get { return "Vault-WebAppName"; } }

        public String MetaDataKeyNameVaultSiteCollectionName { get { return "Vault-SiteCollectionName"; } }

        public String MetaDataKeyNameVaultPlanId { get { return "Vault-PlanId"; } }

        public String MetaDataKeyNameVaultKeepTime { get { return "Vault-KeepTime"; } }

        public String MetaDataKeyNameVaultJobId { get { return "Vault-JobId"; } }

        public String MetaDataKeyNameVaultDataMode { get { return "Vault-DataMode"; } }

        public String MetaDataKeyNameExchangeUserName { get { return "Exchange-UserName"; } }

        public String MetaDataKeyNameExchangePlanId { get { return "Exchange-PlanId"; } }

        public String MetaDataKeyNameExchangeCycleId { get { return "Exchange-CycleId"; } }

        public String MetaDataKeyNameExchangeJobId { get { return "Exchange-JobId"; } }

        public String MetaDataKeyNameExchangeDataMode { get { return "Exchange-DataMode"; } }

        public Dictionary<String, String> BuildMetaData(BackupJobBase backupJob)
        {
            var detailJob = backupJob as TBackupJob;
            return this.BuildMetaData(detailJob);
        }

        protected abstract Dictionary<String, String> BuildMetaData(TBackupJob backupJob);
    }
}