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
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Retrying;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using AvePoint.RA.Contract.Task;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.Contract.TenantUpgrade;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.Model;
using AvePoint.RA.DB.SecurityTrimming;
using Cloud.Sdk.Data.Amls.Ics.Category;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.Extension;

namespace AvePoint.RA.Service.RMTasks
{
    public class MachineLearningModelStatusTaskExecutor : ITaskExecutor
    {
        private static readonly RALogger Logger = RALogger.GetInstance(typeof(MachineLearningModelStatusTaskExecutor));
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();

        private IRMKeyValueDao KeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        public async System.Threading.Tasks.Task ExecutorAsync(TaskBase task)
        {
            var tenantService = PlatformWindsorManager.GetService<ITenantService>();
            var tenants = tenantService.GetAllAvailableTenantInfo();
            foreach (var tenant in tenants)
            {
                await TenantUtil.RunUnderTenantAsync(tenant.TenantId, tenant.RegisterEmail, async () =>
                {
                    Logger.Info($"MachineLearningModelStatusTaskExecutor Start, Current tenant: [{tenant.TenantId}]");
                    var trainingTermService = PlatformWindsorManager.GetService<IRMMLTermService>();
                    var trainingScopeService = PlatformWindsorManager.GetService<ITrainingScopeService>();
                    var securityTrimmingHelper = PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();
                    var trainingModelDao = PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
                    var trainingTermDao = PlatformWindsorManager.GetService<IRMMLTermDao>();
                    var jobMonitorDao = PlatformWindsorManager.GetService<IJobMonitorDao>();
                    var tenantService = PlatformWindsorManager.GetService<ITenantService>();
                    var explorerDao = new ExplorerDao();

                    if (!await LicenseHelperService.IsEnableMaestroAI() && !KeyValueDao.EnableZeroShotFeature())
                    {
                        Logger.Warn($"Current tenant: [{tenant.TenantId}] is disable intelligent. [EnableIntelligent] is False. Need to execute deletion logic");
                        await trainingScopeService.DeleteAIRelatedResourcesAsync(tenant.TenantId);
                        return;
                    }
                   
                    var dbTrainingModel = trainingModelDao.GetDefaultModel();
                    if (dbTrainingModel == null || dbTrainingModel.TrainStatus == (int)MLModelStatus.None)
                    {
                        Logger.Info($"The training hasn't started yet. Current tenant: [{tenant.TenantId}]");
                        return;
                    }

                    if (dbTrainingModel.ExpiredResourcesDeleted)
                    {
                        dbTrainingModel.ExpiredResourcesDeleted = false;
                        await trainingModelDao.UpdateAsync(dbTrainingModel);
                    }

                    Logger.Info($"Get ics client:{tenant.TenantId}");
                    try
                    {
                        var client = AosApiUtility.GetIcsClient(tenant.TenantId);
                        var finailStatus = new int[] { (int)MLModelStatus.Succeeded, (int)MLModelStatus.Failed, (int)MLModelStatus.Exception };
                        var runningStatus = new int[] { (int)MLModelStatus.None, (int)MLModelStatus.Running };
                        if (runningStatus.Contains(dbTrainingModel.TrainStatus))
                        {
                            Logger.Info($"Invoke ics api: {tenant.TenantId}");
                            TrainResult trainResult = await client.TrainingService.GetStateAsync(dbTrainingModel.Id);
                            if (trainResult != null)
                            {
                                Logger.Info($"{GetLogBaseInfo(tenant, dbTrainingModel)}, DB/Client train status: {(MLModelStatus)dbTrainingModel.TrainStatus}/{trainResult.State}");
                                if (trainResult.State != (OperationState)dbTrainingModel.TrainStatus)
                                {
                                    dbTrainingModel.TrainStatus = (int)trainResult.State;
                                    await trainingModelDao.UpdateAsync(dbTrainingModel);
                                }

                                if (trainResult.State == OperationState.Succeeded && dbTrainingModel.PublishStatus == (int)MLModelStatus.None)
                                {
                                    OperationState deployState = OperationState.None;
                                    Logger.Info($"Invoke ics api: {tenant.TenantId}");
                                    
                                    int retryTimes = 0;
                                    while (retryTimes < 3)
                                    {
                                        deployState = await client.EndpointService.DeployAsync(dbTrainingModel.Id);
                                        if (deployState != OperationState.Failed)
                                        {
                                            break;
                                        }
                                        Logger.Info($"{GetLogBaseInfo(tenant, dbTrainingModel)}, EndPoint deploy retry times {retryTimes}.");
                                        retryTimes++;
                                    }

                                    dbTrainingModel.PublishStatus = (int)deployState;
                                    // dbTrainingModel.LastTrainedTime = DateTime.UtcNow.Ticks;
                                    await trainingModelDao.UpdateAsync(dbTrainingModel);

                                    //DeleteBlobData(tenant, dbTrainingModel);
                                }
                                if (trainResult.State == OperationState.Failed || trainResult.State == OperationState.Exception)
                                {
                                    //udpate term and file ing -> not
                                    //UpdateDataToNotTrain(tenant, trainingTermDao, explorerDao);
                                    Logger.Info($"{GetLogBaseInfo(tenant, dbTrainingModel)}, start analyse job.");
                                    jobMonitorDao.UpdateJob(dbTrainingModel.CurrentTrainingJobId, JobStatus.Failed, "RM_MachineLearning_ModelTrainFailed");
                                    trainingTermService.StartAnalyseJob();
                                }
                            }
                        }
                        else if (runningStatus.Contains(dbTrainingModel.PublishStatus))
                        {
                            Logger.Info($"Invoke ics api: {tenant.TenantId}");
                            var publishState = await client.EndpointService.GetDeployStateAsync(dbTrainingModel.Id);
                            Logger.Info($"{GetLogBaseInfo(tenant, dbTrainingModel)}, DB train status: {(MLModelStatus)dbTrainingModel.TrainStatus}; DB/Client publish status: {(MLModelStatus)dbTrainingModel.PublishStatus}/{publishState}");
                            if (publishState != OperationState.None)
                            {
                                dbTrainingModel.PublishStatus = (int)publishState;
                            }

                            if (publishState == OperationState.Succeeded)
                            {
                                //udpate term and file ing -> ed
                                //UpdateDataToTrained(tenant, trainingTermDao, explorerDao);
                                Logger.Info($"{GetLogBaseInfo(tenant, dbTrainingModel)}, start analyse job.");
                                jobMonitorDao.UpdateJob(dbTrainingModel.CurrentTrainingJobId, JobStatus.Finished);
                                trainingTermService.StartAnalyseJob();
                                dbTrainingModel.LastTrainedTime = DateTime.UtcNow.Ticks;
                            }
                            else if (publishState == OperationState.Failed || publishState == OperationState.Exception || publishState == OperationState.None)
                            {
                                //udpate term and file ing -> not
                                //UpdateDataToNotTrain(tenant, trainingTermDao, explorerDao);
                                jobMonitorDao.UpdateJob(dbTrainingModel.CurrentTrainingJobId, JobStatus.Failed, "RM_MachineLearning_EndPointPublishFailed");
                                Logger.Info($"{GetLogBaseInfo(tenant, dbTrainingModel)}, start analyse job.");
                                trainingTermService.StartAnalyseJob();
                            }
                            await trainingModelDao.UpdateAsync(dbTrainingModel);
                        }
                        else
                        {
                            Logger.Info($"Invoke ics api: {tenant.TenantId}");
                            var publishState = await client.EndpointService.GetDeployStateAsync(dbTrainingModel.Id);
                            var warningMessage = "";
                            if ((MLModelStatus)dbTrainingModel.PublishStatus == MLModelStatus.Succeeded && publishState != OperationState.Succeeded)
                            {
                                warningMessage = ", The endpoint may have been deleted in the system.";
                            }
                            Logger.Info($"{GetLogBaseInfo(tenant, dbTrainingModel)}, DB train status: {(MLModelStatus)dbTrainingModel.TrainStatus}; DB/Client publish status: {(MLModelStatus)dbTrainingModel.PublishStatus}/{publishState}{warningMessage}");
                            //Logger.Info($"{GetLogBaseInfo(tenant, dbTrainingModel)}, DB train status: {(MLModelStatus)dbTrainingModel.TrainStatus}; DB publish status: {(MLModelStatus)dbTrainingModel.PublishStatus}");
                        }

                    }
                    catch (Exception e)
                    {
                        if (jobMonitorDao.GetJob(dbTrainingModel.CurrentTrainingJobId).Status != (int)JobStatus.Finished)
                        {
                            jobMonitorDao.UpdateJob(dbTrainingModel.CurrentTrainingJobId, JobStatus.Failed);
                        }
                        Logger.Error($"Sync model status error:{e}");
                    }
                });
            }
        }

        private string GetLogBaseInfo(TenantInfoDto tenant, RMMLTrainingModel dbTrainingModel)
        {
            return $"Current tenant: [{tenant.TenantId}], train model: {dbTrainingModel.Id}, current job:{dbTrainingModel.CurrentTrainingJobId}";
        }

        /*private static async System.Threading.Tasks.Task UpdateDataToTrainedAsync(TenantInfoDto tenant, IRMMLTermDao trainingTermDao, ExplorerDao explorerDao)
        {
            Logger.Info($"Current tenant: [{tenant.TenantId}], update data status trained.");
            List<RMMLTerm> trainingTerms = await trainingTermDao.FindListAsync(t => t.Status == (int)MLTermStatus.Training);
            foreach (var term in trainingTerms)
            {
                term.Status = (int)MLTermStatus.Trained;
            }
            trainingTermDao.BatchUpdate(trainingTerms);
            explorerDao.UpdateAll(r => r.TrainingScope == (int)MLFileStatus.Training, r => { r.TrainingScope = (int)MLFileStatus.Trained; });
        }*/
    }
}
