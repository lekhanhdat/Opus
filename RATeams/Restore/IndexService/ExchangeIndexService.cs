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

namespace Office365GroupRestore
{
    #region using directives

    using System;
    using System.IO;

    using AvePoint.Metadata;
    using AvePoint.Media.Core.Index;
    using AvePoint.Media.Service.DomainModel;
    using AvePoint.RA.CommonUtil;
    using Storage.Util;
    using Storage;
    using AvePoint.Media.Common;
    using MediaDataIO;
    using AvePoint.Media.Service.DomainModel.DocAve6x;
    using Office365Group;
    using AvePoint.RA.Contract.Tenant;

    #endregion

    public class ExchangeIndexService : IIndexService<ExchangeIndexServiceOpenParameter>
    {
        private static RALogger logger = RALogger.GetInstance(typeof(ExchangeIndexService));
        private ExchangeIndexServiceOpenParameter openParam;

        public IDatabaseNameGenerator DatabaseNameGenerator = new ExchangeDatabaseNameGenerator();

        public MediaDataPathGenerator DataPathGenerator { get; set; }

        public string CurrentOpenIndexDatabaseName { get; set; }

        public IIndexDatabaseSynchronizer IndexSynchronizer { get; set; }

        public IIndexProcessor<ExchangeIndexProcessorParameter> IndexProcessor { get; set; }

        public void Open(ExchangeIndexServiceOpenParameter openParam)
        {
            logger.Info("Begin opening exchange index service");
            this.openParam = openParam;
            
            DataPathGenerator = new TeamsMediaDataPathGenerator(DataModule.TeamsPlatform, openParam.BackupJobId, RestoreConfig.CurrentMailboxAddress);
            if (openParam.IndexCacheDeviceSystem != null && !openParam.IndexCacheDeviceSystem.IsDirectSystem)
            {
                logger.Info("Exchange index service is working on non-local system.");
            }
            IndexSynchronizer.Initialize(openParam);
            var param = new ExchangeIndexProcessorParameter(TenantLocalValue.LogonGroupId);
            var dbInfo = new IndexDatabaseInfo(openParam);
            try
            {
                this.DownloadIndex(openParam, param, dbInfo);
            }
            catch (FileNotFoundException e)
            {
                logger.Warn("Exchange index {0} was not found. Changing index name to index.db, details: {1}.", dbInfo.DbFileName, e.ToString());
            }
            param.IndexWorkingSystem = (!openParam.IndexLogicalDeviceSystem.IsDirectSystem || MediaConfigInfo.CommonConfigInfo.ForceUseCache) ?
                openParam.IndexCacheDeviceSystem : openParam.IndexLogicalDeviceSystem;
            logger.Info("Start opening exchange index processor.");
            IndexProcessor.Open(param);
            logger.Info("Opened exchange index service successfully.");
        }

        public void Open(ExchangeIndexServiceOpenParameter openParam, DataModule dataModule)
        {
            logger.Info("Begin opening exchange index service");
            this.openParam = openParam;

            DataPathGenerator = new TeamsMediaDataPathGenerator(dataModule, openParam.BackupJobId, RestoreConfig.CurrentMailboxAddress);
            if (openParam.IndexCacheDeviceSystem != null && !openParam.IndexCacheDeviceSystem.IsDirectSystem)
            {
                logger.Info("Exchange index service is working on non-local system.");
            }
            IndexSynchronizer.Initialize(openParam);
            var param = new ExchangeIndexProcessorParameter(TenantLocalValue.LogonGroupId);
            var dbInfo = new IndexDatabaseInfo(openParam);
            try
            {
                this.DownloadIndex(openParam, param, dbInfo);
            }
            catch (FileNotFoundException e)
            {
                logger.Warn("Exchange index {0} was not found. Changing index name to index.db, details: {1}.", dbInfo.DbFileName, e.ToString());
            }
            param.IndexWorkingSystem = (!openParam.IndexLogicalDeviceSystem.IsDirectSystem || MediaConfigInfo.CommonConfigInfo.ForceUseCache) ?
                openParam.IndexCacheDeviceSystem : openParam.IndexLogicalDeviceSystem;
            logger.Info("Start opening exchange index processor.");
            IndexProcessor.Open(param);
            logger.Info("Opened exchange index service successfully.");
        }

        public void Close()
        {
            if (IndexProcessor != null)
            {
                IndexProcessor.Close();
            }
        }

        private void DownloadIndex(ExchangeIndexServiceOpenParameter openParam, ExchangeIndexProcessorParameter param, IndexDatabaseInfo dbInfo)
        {
            dbInfo.DbFileName = ServiceConstants.IndexDBName;
            CurrentOpenIndexDatabaseName = dbInfo.DbFileName;
            param.DownLoadResult = IndexSynchronizer.Download(dbInfo);
        }

        public void DeleteFileForJob(ExchangeIndexServiceOpenParameter openParam)
        {
            throw new NotImplementedException();
        }
    }
}