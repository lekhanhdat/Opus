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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Service.Services.ControlPanel;
using AvePoint.RA.Service.Services.RMMachineLearning;
using AvePoint.RA.VectorDataCenter.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.RMTasks
{
    public class ZeroshotCheckLicenseTaskExcutor : ITaskExecutor
    {
        private RALogger logger = RALogger.GetInstance(typeof(ZeroshotCheckLicenseTaskExcutor));
        private ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();
        private readonly RMTenantVectorPostgreMappingDao _mappingDao = new RMTenantVectorPostgreMappingDao();
        private readonly RMTenantVectorCosmosMappingDao _mappingDaoCosmosDb = new RMTenantVectorCosmosMappingDao();

        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            try
            {
                var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
                var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
                var tInfos = TenantService.GetAllAvailableTenantInfo();
                logger.Debug($"Tenant infos count: [{tInfos.Count}]");
                foreach (var tInfo in tInfos)
                {
                    await TenantUtil.RunUnderTenantAsync(tInfo.TenantId, tInfo.RegisterEmail, async () =>
                    {
                        var enableZeroShot = KeyValueDao.EnableZeroShotFeature();
                        logger.Debug($"Current tenant: [{tInfo.TenantId}], enable ZeroShot : {enableZeroShot}, IsGCP env: {isGCP}");
                        if (!enableZeroShot)
                        {
                            if (isGCP) // delete PostgreSQL
                            {
                                IVectorStore _vectorStore = new PostgresVectorStore(false);
                                var dbName = _mappingDao.GetOrCreateDatabaseName(tInfo.TenantId, false);
                                if (!string.IsNullOrEmpty(dbName))
                                {
                                    logger.Warn($"Current tenant: [{tInfo.TenantId}] is disable Zeroshot feature. [EnableZeroshot] is False. Need to execute deletion schema");
                                    await _vectorStore.DropVectorDbIfExist(dbName, $"s_{SanitizeIdentifier(tInfo.TenantId)}");
                                    _mappingDao.DeleteMapping(tInfo.TenantId);
                                    return;
                                }
                            }
                            else // delete CosmosDb
                            {
                                var (dbName, containerName) = _mappingDaoCosmosDb.GetOrCreateDatabaseAndContainerName(new Guid(tInfo.TenantId), false);
                                if(!string.IsNullOrEmpty(dbName))
                                {
                                    IVectorStore _vectorStore = new CosmosDbVectorStore(false);
                                    logger.Warn($"Current tenant: [{tInfo.TenantId}] is disable Zeroshot feature. [EnableZeroshot] is False. Need to execute deletion document");
                                    await _vectorStore.DropVectorDbIfExist(dbName, containerName);
                                    _mappingDaoCosmosDb.DeleteMapping(tInfo.TenantId);
                                    return;
                                }

                            }
                        }
                        
                    });
                }

            }
            catch (Exception ex)
            {
                logger.Error("Error occurred while delete db vector usage, ERROR:{0}", ex.ToString());
            }
        }
        private string SanitizeIdentifier(string identifier)
        {
            var sanitized = identifier.ToLower().Replace("-", "_");
            return Regex.Replace(sanitized, @"[^a-z0-9_]", "");
        }
    }
}
