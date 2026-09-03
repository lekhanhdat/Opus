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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Audit;
using AvePoint.RA.Contract.CloudService;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Exceptions;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.RMMachineLearning;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.DB.Model;
using AvePoint.RA.Service.Services.RMMachineLearning.AuditHandler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.VectorDataCenter.Embedding;
using AvePoint.RA.VectorDataCenter.Storage;
using AvePoint.RA.VectorDataCenter.Services;
using AvePoint.RA.VectorDataCenter.Models;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.Hybrid.ClientLibrary.Data;
using AvePoint.GCommon.Utility;
using Microsoft.Graph.Models;
using Aspose.Pdf.Operators;
using static AvePoint.GCommon.Utility.I18N.EventIds.Configuration;

namespace AvePoint.RA.Service.Services.RMMachineLearning
{
    [Audit]
    public class RMMLTermService : RMServiceBase, IRMMLTermService
    {
        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();
        private static IJobMonitorService RMJobService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private static IJobQueueService JobQueueService => PlatformWindsorManager.GetService<IJobQueueService>();
        private static ITrainingScopeService TrainingScopeService => PlatformWindsorManager.GetService<ITrainingScopeService>();
        private static IRMMLTermDao RMMLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private static IRMMLTrainingModelDao RMMLTrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        //private static IDashboardTermUsageDao DashboardTermUsageDao => PlatformWindsorManager.GetService<IDashboardTermUsageDao>();
        private static ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private static readonly int TrainingJobIntervalLimit = 24;
        private static readonly string TrainingJobIntervalMins = "TrainingJobIntervalMins";
        private IJobMonitorService JobMonitorService => PlatformWindsorManager.GetService<IJobMonitorService>();
        private IRMSettingJobDao SettingJobDao => PlatformWindsorManager.GetService<IRMSettingJobDao>();

        private IFeatureUsageLimitDao FeatureUsageLimitDao => PlatformWindsorManager.GetService<IFeatureUsageLimitDao>();
        private IExplorerService ExplorerService => PlatformWindsorManager.GetService<IExplorerService>();

        private static readonly int MaxTermCount = 500;

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.AddTerms, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public async Task<MLTermResponseResult> AddTerms(List<MLTermDto> dtos)
        {
            if (int.TryParse(RMKeyValueDao.GetValueByKey(RMKeyValuesConstants.AISmartTermMaxCount)?.Value, out int setAISmartTermMaxCount))
            {
                Logger.Info($"Get AI smart term max count is {setAISmartTermMaxCount}");
            }
            else
            {
                setAISmartTermMaxCount = MaxTermCount;
            }
            var result = new MLTermResponseResult();
            if (!await FeatureUsageLimitDao.CheckUsageLimit(FeatureType.Embedding))
            {
                return new MLTermResponseResult
                {
                    HasError = true,
                    ErrorMsg = I18NEntity.GetString("RM_ML_Zero_CheckUsageLimit_Msg")
                };
            }
            try
            {
                var defaultModel = RMMLTrainingModelDao.GetDefaultModel(true);
                RMMLTermDao.AddOrUpdateTerms(dtos, defaultModel.Id, setAISmartTermMaxCount);
                if(RMKeyValueDao.EnableZeroShotFeature() && RMMLTrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot)
                {
                    IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                    var vectorizationService = await VectorizationService.CreateWithRAIProvider(vectorStore);
                    foreach (var item in dtos)
                    {
                        try
                        {
                            if (!item.Description.IsNullOrWhiteSpace())
                            {
                                FeatureUsageLimitDao.AddOrUpdate(FeatureType.Embedding);
                            }
                            await vectorizationService.StoreTermAsync(new TermDescription
                            {
                                Id = item.Id,
                                Name = item.Name,
                                Description = item.Description
                            });
                        }
                        catch (Exception e)
                        {
                            Logger.Error($"An error while add vector for term {item.Id}, message: {e}");
                        }
                    }
                }
            }
            catch (MLTermMaxCountExceededException ex)
            {
                result.HasError = true;
                result.ErrorMsg = I18NEntity.GetString(ex.Message, setAISmartTermMaxCount);
            }
            catch (Exception ex)
            {
                Logger.Error($"An error while add ml terms, message: {ex}");
                result.HasError = true;
            }
            return result;
        }

        public async Task<RAReturnMessage> CheckPredictionJobRunning(int action)
        {
            List<BaseJobDto> importJobs = JobMonitorService.GetRunningJobs([JobType.ApplySharePointSettings, JobType.GoogleApplySettings, JobType.ApplyTeamsSettings, JobType.OneDriveDataSynchronisation]);
            foreach (var importJob in importJobs)
            {
                var jobSetting = SettingJobDao.GetRMSettingJob(importJob.Id);
                if (jobSetting != null)
                {
                    if(HasExistingJob(jobSetting))
                    {
                        if(action == (int)PredictionJobRunningAction.ChangePredictionMode)
                        {
                            return new RAReturnMessage()
                            {
                                MessageType = RAMessageType.Failed,
                                ErrorMessage = I18NEntity.GetString("RM_ML_Exist_Running_Job_SwitchMode")
                            };
                        }
                        return new RAReturnMessage()
                        {
                            MessageType = RAMessageType.Failed,
                            ErrorMessage = I18NEntity.GetString("RM_ML_Exist_Running_Job")
                        };
                    }
                }
            }
            return new RAReturnMessage()
            {
                MessageType = RAMessageType.Successful
            };
        }

        private bool HasExistingJob(RMSettingJobInfo jobSetting)
        {
            Logger.Info($"Job setting count: {jobSetting.JobInfos.Length}");
            if (!jobSetting.JobInfos.Any()) { return false; }

            switch (jobSetting.JobType)
            {
                case (int)JobType.ApplySharePointSettings:
                    var spSettings = (SerializerHelper.DeserializeByDataContractSerializer<List<RMSharePointSetting>>(jobSetting.JobInfos));
                    return spSettings.Any(s => s.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification || s.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault);
                case (int)JobType.GoogleApplySettings:
                    var ggSettings = (SerializerHelper.DeserializeByDataContractSerializer<List<RMGoogleSetting>>(jobSetting.JobInfos));
                    return ggSettings.Any(s => s.DeployLabelMethod == (int)DeployLabelMethod.UseIntelligenceClassification || s.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault);
                case (int)JobType.ApplyTeamsSettings:
                    var teamsSettings = (SerializerHelper.DeserializeByDataContractSerializer<List<RMTeamsSetting>>(jobSetting.JobInfos));
                    return teamsSettings.Any(s => s.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification || s.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault);
                case (int)JobType.OneDriveDataSynchronisation:
                    var odSettings = (SerializerHelper.DeserializeByDataContractSerializer<List<RMOneDriveSetting>>(jobSetting.JobInfos));
                    return odSettings.Any(s => s.DeployTermMethod == (int)DeployTermMethod.UseIntelligenceClassification || s.AITermUseType == ArtificialIntelligenceTermUseType.AutoDefault);
            }
            return false;
        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.UpdateTermDescription, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public async Task<MLTermResponseResult> UpdateDescription(MLTermDto dto)
        {
            var result = new MLTermResponseResult();
            if(!await FeatureUsageLimitDao.CheckUsageLimit(FeatureType.Embedding))
            {
                return new MLTermResponseResult
                {
                    HasError = true,
                    ErrorMsg = I18NEntity.GetString("RM_ML_Zero_CheckUsageLimit_Msg")
                };
            }
            try
            {
                await RMMLTermDao.UpdateDescription(dto);
                FeatureUsageLimitDao.AddOrUpdate(FeatureType.Embedding);
                IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                var vectorizationService = await VectorizationService.CreateWithRAIProvider(vectorStore);
                await vectorizationService.StoreTermAsync(new TermDescription
                {
                    Id = dto.Id,
                    Name = dto.Name,
                    Description = dto.Description
                });
            }
            catch (Exception ex)
            {
                Logger.Error($"An error while update ml term, message: {ex}");
                result.HasError = true;
                result.ErrorMsg = I18NEntity.GetString(ex.Message);
            }
            return result;
        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.DeleteTerms, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public async Task<MLTermResponseResult> DeleteTerms(List<Guid> ids)
        {
            var result = new MLTermResponseResult();
            try
            {
                bool hasRunningJob = RMJobService.GetRunningJobsCount(JobType.MachineLearningTraining) > 0;
                if (hasRunningJob)
                {
                    result.HasError = true;
                    result.ErrorMsg = I18NEntity.GetString("RM_ML_HasRunningJob_RemoveTerm_Message");
                    return result;
                }
                RMMLTermDao.MarkTermRemoveStatus(ids);
                var resetResult = ExplorerService.ResetMARecordsForRemovedMLTerms(ids);
                if (!resetResult)
                {
                    Logger.Error($"Failed to reset MA records for removed ML terms, term ids: {string.Join(",", ids)}");
                }
                IVectorStore vectorStore = VectorStoreFactory.CreateVectorStore();
                var vectorizationService = await VectorizationService.CreateWithRAIProvider(vectorStore);
                foreach (var id in ids)
                {
                    try
                    {
                        await vectorizationService.DeleteTermAsync(id);
                    }
                    catch (Exception e)
                    {
                        Logger.Error($"An error while remove the vector term {id} in cosmos, message: {e}");
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"An error while mark terms is removed/will removed, message: {ex}");
                result.HasError = true;
                result.ErrorMsg = I18NEntity.GetString(ex.Message);
            }
            return result;
        }


        public MLTermResponseResult LoadUsageTerms(UsageTermQueryParam param)
        {
            var result = new MLTermResponseResult();
            try
            {
                var data = TermDao.GetWillTrainingTerms(param.SearchValue, param.PageIndex, param.PageSize, out int totalCount);
                //var data = DashboardTermUsageDao.GetTermUsages(param, out int totalCount);
                result.TotalCount = totalCount;
                result.UsageTerms = data.Select(o => new UsageTermDto { Id = o.UniqueId, Name = o.Name, Description = o.Description, FullPath = o.FullPath }).ToList();
            }
            catch (Exception ex)
            {
                Logger.Error($"An error while load usage terms, message: {ex}");
                result.HasError = true;
            }
            return result;
        }

        public MLTermResponseResult LoadTerms(MLTermQueryParam param)
        {
            var result = new MLTermResponseResult();
            try
            {
                var isZeroShot = RMKeyValueDao.EnableZeroShotFeature() && RMMLTrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
                result.MLTerms = RMMLTermDao.Query(param, out int totalCount, isZeroShot);
                result.MLTerms?.ForEach(o =>
                {
                    o.FullPath = TermDao.GetTermNamesPathByTermId(o.Id);
                });
                result.TotalCount = totalCount;
            }
            catch (Exception ex)
            {
                Logger.Error($"An error while load ml terms, message: {ex}");
                result.HasError = true;
            }
            return result;

        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.SetAutoApply, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public async Task<MLTermResponseResult> SetAutoApplyAsync(Guid termId, bool autoApply)
        {
            var result = new MLTermResponseResult();
            try
            {
                await RMMLTermDao.SetAutoApplyAsync(termId, autoApply);
            }
            catch (Exception ex)
            {
                Logger.Error($"An error while set auto apply, term id: {termId}, message: {ex}");
                result.HasError = true;
            }
            return result;
        }

        public MLTermResponseResult StartTrain()
        {
            throw new NotImplementedException();
        }

        public async Task<string> GetLastUpdatedTimeAsync()
        {
            var lastUpdatedTimeTicks = RMMLTrainingModelDao.GetLastUpdatedTime();
            if (lastUpdatedTimeTicks > 0)
            { 
                return (await GeneralSettingService.ConvertTiksToDateTimeAsync(lastUpdatedTimeTicks, false)).FormaTime;
            }
            return string.Empty;
        }

        public async Task<RAReturnMessage> StartTrainingJobAsync()
        {
            RAReturnMessage returnMessage = new();
            var trainingModel = RMMLTrainingModelDao.GetDefaultModel();
            if (trainingModel == null)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_MachineLearning_JobTermCountLimit");
                return returnMessage;
            }
            //Use key value table, config interval for qa testing
            var invervalTicks = TimeSpan.FromHours(TrainingJobIntervalLimit).Ticks;
            var keyValue = RMKeyValueDao.GetValueByKey(TrainingJobIntervalMins);
            if (int.TryParse(keyValue?.Value, out int minIntValue))
            {
                invervalTicks = TimeSpan.FromMinutes(minIntValue).Ticks;
            }

            if (DateTime.UtcNow.Ticks - trainingModel.LastTrainedTime < invervalTicks)
            {
                GeneralSettingModel gls = await GeneralSettingService.GetGeneralSettingAsync();
                var afterTime = GeneralSettingService.ConvertTiksToDateTime(gls, trainingModel.LastTrainedTime + invervalTicks, true).SimplifyFormatTime;
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = string.Format(I18NEntity.GetString("RM_MachineLearning_JobAfterTime"), afterTime);
                return returnMessage;
            }

            var modelStatus = TrainingScopeService.GetTrainingModelStatus();
            if (modelStatus == MLModelStatus.Running)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_MachineLearning_TrainRunningSkip");
                return returnMessage;
            }

            var hasRunningJobs = RMJobService.GetRunningJobs(JobType.MachineLearningTraining).Any();
            if (hasRunningJobs)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_MachineLearning_JobRunningSkip");
                return returnMessage;
            }

            var activeStatus = new int[] { (int)MLTermStatus.NotTrain, (int)MLTermStatus.Training, (int)MLTermStatus.Trained };
            var activeTerms = await RMMLTermDao.FindListAsync(t => Enumerable.Contains(activeStatus, t.Status));
            // var exceptDefaultTermIds = SharePointSettingDao.FindList(o => !o.IsRemoved && o.EnableRecordManagement == 1 && o.DeployTermMethod == (int)DeployTermMethod.UseDefaultTerm).Select(o => o.DefaultTermId).Distinct().ToList();
            // activeTerms = activeTerms.Where(term => !exceptDefaultTermIds.Contains(term.Id)).ToList();
            if (activeTerms.Count < RecordsConstants.TrainingTerm_MinimumNumber)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = I18NEntity.GetString("RM_MachineLearning_JobTermCountLimit");
                return returnMessage;
            }
            try
            {
                var jobType = JobType.MachineLearningTraining;
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.StartTrainingJob, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public string RealRunTrainingJob(JobType jobType)
        {
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            try
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                List<string> runningJobs = RMJobService.GetRunningJobs(jobType);
                bool isSkip = runningJobs.Any(j => j != jobId);
                if (isSkip)
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                    Logger.Info("Machine learning training has job running job, so this job is skip"); ;
                }
                else
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType.ToString(), jobId),
                    });
                    Logger.Info(string.Format("Finished add job to job queue, job id is : {0}", jobId));
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in real run ML job, reason : {ex}.");
            }
            return jobId;
        }


        public RAReturnMessage StartAnalyseJob()
        {
            RAReturnMessage returnMessage = new();
            try
            {
                var jobType = JobType.MachineLearningAnalyse;
                var groupId = TenantLocalValue.LogonGroupId;
                var loginName = TenantLocalValue.LogonUserEmail;
                JobQueueDto jqDto = new JobQueueDto()
                {
                    JobType = jobType,
                    JobRunType = JobRunBy.Control,
                    TenantGroupId = groupId,
                    JobRunByUser = loginName
                };
                returnMessage.MessageType = RAMessageType.Successful;
                returnMessage.Extension = JobQueueService.AddToDBJobQueue(jqDto);
            }
            catch (Exception ex)
            {
                returnMessage.MessageType = RAMessageType.Failed;
                returnMessage.ErrorMessage = ex.Message;
            }
            return returnMessage;
        }

        public string RealRunAnalyseJob(JobType jobType)
        {
            string jobId = string.Empty;
            string jobRunByUser = TenantLocalValue.LogonUserEmail;
            try
            {
                jobId = RMJobService.CreateJob(jobType, jobRunByUser);
                List<string> runningJobs = RMJobService.GetRunningJobs(jobType);
                bool isSkip = runningJobs.Any(j => j != jobId);
                if (isSkip)
                {
                    RMJobService.UpdateJobStatus(jobId, JobStatus.Skipped, "RM_EF_JobSkip");
                    Logger.Info("Machine learning training has job running job, so this job is skip"); ;
                }
                else
                {
                    JobQueueService.HandleMessage(new JobQueueMessage()
                    {
                        JobId = jobId,
                        RunBy = JobRunBy.Control,
                        JobType = jobType,
                        CommandLine = string.Format("{0} {1}", jobType.ToString(), jobId),
                    });
                    Logger.Info(string.Format("Finished add job to job queue, job id is : {0}", jobId));
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in real run ML job, reason : {ex}.");
            }
            return jobId;
        }

        public ValidateDefaultTermResult ValidateDefaultTerm(List<Guid> termIds)
        {
            var isZeroShotFeature = RMKeyValueDao.EnableZeroShotFeature() && RMMLTrainingModelDao.GetDefaultModel()?.Mode == TrainingMode.ZeroShot;
            if (isZeroShotFeature)
            {
                return new ValidateDefaultTermResult()
                {
                    IsExists = false
                };
            }
            var result = new ValidateDefaultTermResult();
            if (termIds != null && termIds.Any())
            {
                var defaultTermNames = TermDao.GetSettingDefaultTermNames(termIds);
                if (defaultTermNames != null && defaultTermNames.Any())
                {
                    result.IsExists = true;
                    result.DefaultTermNames = defaultTermNames;
                }
            }
            return result;
        }

        public int GetCurrentMode()
        {
            if(!RMKeyValueDao.EnableZeroShotFeature())
            {
                return (int)TrainingMode.MLTraining;
            }
            var defaultModel = RMMLTrainingModelDao.GetDefaultModel();
            if (defaultModel == null) return (int)TrainingMode.ZeroShot;
            return (int)defaultModel.Mode;
        }

        [Audit(Module = AuditModule.MachineLearning, Category = AuditCategory.MachineLearning, Action = AuditAction.SwitchMode, BeforeHandler = typeof(MLTermBeforeAuditHandler), AfterHandler = typeof(MLTermAfterAuditHandler))]
        public async Task<RAReturnMessage> SwitchModeAsync(int mode)
        {
            RAReturnMessage message = new()
            {
                MessageType = RAMessageType.Successful
            };
            try
            {
                if (RMKeyValueDao.EnableZeroShotFeature())
                {
                    await RMMLTrainingModelDao.SwitchModeAsync((TrainingMode)mode);
                }
                else
                {
                    Logger.Error($"Current account does not enable the zero shot feature.");
                }
            }
            catch (Exception ex)
            {
                Logger.Error($"Error in switch mode, reason : {ex}.");
                message.MessageType = RAMessageType.Failed;
            }
            return message;
        }
    }
}
