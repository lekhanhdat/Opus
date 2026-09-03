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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMMachineLearning.AuditHandler
{
    public class MLTermAfterAuditHandler : IAfterAuditHandler
    {
        private static ITermDao TermDao => PlatformWindsorManager.GetService<ITermDao>();
        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = new RMAuditInfo();
            auditInfo.Module = (AuditModule)model;
            auditInfo.Category = (AuditCategory)category;
            auditInfo.Action = (AuditAction)action;
            switch (auditInfo.Action)
            {
                case AuditAction.AddTerms:
                    return CollectAddTerms(auditInfo, args, target, returnValue);
                case AuditAction.DeleteTerms:
                    return CollectDeleteTerms(auditInfo, args, target, returnValue);
                case AuditAction.SetAutoApply:
                    return CollectSetAutoApply(auditInfo, info, args, target, returnValue);
                case AuditAction.StartTrainingJob:
                    return CollectStartTrainingJob(auditInfo, args, target, returnValue);
                case AuditAction.MLExportReportJob:
                    return CollectExportTrainingReportJob(auditInfo, args, target, returnValue);
                case AuditAction.UpdateTermDescription:
                    return CollectUpdateTermDescription(auditInfo, info, args, target, returnValue);
                case AuditAction.SwitchMode:
                    return CollectSwitchMode(auditInfo, info, args, target, returnValue);
                case AuditAction.AddTrainingFileManual:
                    return CollectAddTrainingFileManual(auditInfo, args, target, returnValue);
                case AuditAction.DeleteTrainingScopeFile:
                    return CollectDeleteTrainingFileManual(auditInfo, args, target, returnValue);
                case AuditAction.ChangeTrainingScopeOption:
                    return CollectChangeTrainingScopeOption(auditInfo, info, args, target, returnValue);
                default:
                    return null;
                    //return await System.Threading.Tasks.Task.FromResult<RMAuditInfo>(null);
            }
        }

        private RMAuditInfo CollectDeleteTrainingFileManual(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            var param = args[0] as List<MLTrainScopeDto>;
            RAReturnMessage result = returnValue as RAReturnMessage;
            auditInfo.Object = string.Join(" ;", param?.Where(_ => !string.IsNullOrEmpty(_.FileName)).Select(_ => _.FileName).ToList() ?? []);
            auditInfo.Status = result == null || result.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectChangeTrainingScopeOption(RMAuditInfo auditInfo, RMAuditInfo compareInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            RAReturnMessage result = returnValue as RAReturnMessage;

            if (args[0] is MLTrainingScopeManage)
            {
                var newTrainingScope = args[0] as MLTrainingScopeManage;
                if (compareInfo != null && compareInfo.ModifyContent != null)
                {
                    auditInfo.ModifyContent = new List<AuditItem>();
                    var trainingScopeOptionAudit = compareInfo.ModifyContent.Where(_ => _.TargetSetting.Equals(AuditConstants.Audit_MachineLearning_ChangeTrainingOption)).FirstOrDefault();
                    if (trainingScopeOptionAudit != null)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = trainingScopeOptionAudit.TargetSetting,
                            OldValue = trainingScopeOptionAudit.OldValue,
                            NewValue = GetTrainingScopeOption((TrainingScopeOption)newTrainingScope.TrainingScopeOption)
                        });
                    }
                    else
                    {
                        auditInfo.ModifyContent.Add(new AuditItem()
                        {
                            TargetSetting = trainingScopeOptionAudit.TargetSetting,
                            NewValue = GetTrainingScopeOption((TrainingScopeOption)newTrainingScope.TrainingScopeOption)
                        });
                    }
                    bool isNeedAddAudit = false;
                    var locationScopeOptionAudit = compareInfo.ModifyContent.Where(_ => _.TargetSetting.Equals(AuditConstants.Audit_MachineLearning_FromLocation_Location)).FirstOrDefault();
                    if(locationScopeOptionAudit == null)
                    {
                        locationScopeOptionAudit = new AuditItem()
                        {
                            TargetSetting = AuditConstants.Audit_MachineLearning_FromLocation_Location,
                        };
                    }
                    else
                    {
                        isNeedAddAudit = true;
                    }
                    var sourceScopeOptionAudit = compareInfo.ModifyContent.Where(_ => _.TargetSetting.Equals(AuditConstants.Audit_MachineLearning_FromLocation_SourceFlag)).FirstOrDefault();
                    if (sourceScopeOptionAudit == null)
                    {
                        sourceScopeOptionAudit = new AuditItem()
                        {
                            TargetSetting = AuditConstants.Audit_MachineLearning_FromLocation_SourceFlag,
                        };
                    }
                    else
                    {
                        isNeedAddAudit = true;
                    }
                    if (newTrainingScope.TrainingScopeOption == (int)TrainingScopeOption.FromLocation)
                    {
                        isNeedAddAudit = true;
                        locationScopeOptionAudit.NewValue = newTrainingScope.Location;
                        sourceScopeOptionAudit.NewValue = GetSourceFlag(newTrainingScope.SourceFlag);
                    }
                    if (isNeedAddAudit)
                    {
                        auditInfo.ModifyContent.AddRange([sourceScopeOptionAudit, locationScopeOptionAudit]);
                    }

                }
                auditInfo.Status = result == null || result.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            }
            return auditInfo;
        }

        private RMAuditInfo CollectSwitchMode(RMAuditInfo auditInfo, RMAuditInfo compareInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            RAReturnMessage result = returnValue as RAReturnMessage;
            int.TryParse(args[0].ToString(), out int mode);
            if (compareInfo != null && compareInfo.ModifyContent != null)
            {
                auditInfo.ModifyContent = new List<AuditItem>();
                foreach (AuditItem item in compareInfo.ModifyContent)
                {
                    if (item.TargetSetting == AuditConstants.Audit_MachineLearning_SwitchMode)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem()
                        {
                            OldValue = item.OldValue,
                            NewValue = GetCurrentModeString((TrainingMode)mode)
                        });
                    }
                }
            }
            auditInfo.Status = result == null || result.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectAddTrainingFileManual(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            var param = args[0] as List<MLTrainScopeDto>;
            RAReturnMessage result = returnValue as RAReturnMessage;
            auditInfo.Object = string.Join(" ;", param?.Where(_ => !string.IsNullOrEmpty(_.FullPath)).Select(_ => _.FullPath).ToList() ?? []);
            auditInfo.Status = result == null || result.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectAddTerms(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            var param = args[0] as List<MLTermDto>;
            MLTermResponseResult result = returnValue as MLTermResponseResult;
            auditInfo.Object = GetTermNames(param?.Select(o => o.Id).ToList());
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectUpdateTermDescription(RMAuditInfo auditInfo, RMAuditInfo compareInfo, object[] args, object target, object returnValue)
        {
            string title = string.Empty;
            var param = args[0] as MLTermDto;
            MLTermResponseResult result = returnValue as MLTermResponseResult;
            auditInfo.Object = TermDao.GetTermNamesPathByTermId(param.Id);
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            if (compareInfo != null && compareInfo.ModifyContent != null)
            {
                auditInfo.ModifyContent = new List<AuditItem>();
                foreach (AuditItem item in compareInfo.ModifyContent)
                {
                    if (item.TargetSetting == AuditConstants.Audit_MachineLearning_Update_Description)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem()
                        {
                            OldValue = item.OldValue,
                            NewValue = param.Description
                        });
                    }
                }
            }
            return auditInfo;
        }

        private RMAuditInfo CollectDeleteTerms(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            var param = args[0] as List<Guid>;
            MLTermResponseResult result = returnValue as MLTermResponseResult;
            auditInfo.Object = GetTermNames(param);
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectSetAutoApply(RMAuditInfo auditInfo, RMAuditInfo compareInfo, object[] args, object target, object returnValue)
        {
            if (Guid.TryParse(args[0].ToString(), out Guid termId))
            {
                auditInfo.Object = TermDao.GetTermNamesPathByTermId(termId);
            }
            bool.TryParse(args[1].ToString(), out bool autoApply);
            if (compareInfo != null && compareInfo.ModifyContent != null)
            {
                auditInfo.ModifyContent = new List<AuditItem>();
                foreach (AuditItem item in compareInfo.ModifyContent)
                {
                    if (item.TargetSetting == AuditConstants.Audit_MachineLearning_EnableAutoApply_Title)
                    {
                        auditInfo.ModifyContent.Add(new AuditItem()
                        {
                            OldValue = item.OldValue,
                            NewValue = YesOrNoString(autoApply)
                        });
                    }
                }
            }
            MLTermResponseResult result = returnValue as MLTermResponseResult;
            auditInfo.Status = result == null || result.HasError ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
            return auditInfo;
        }

        private RMAuditInfo CollectStartTrainingJob(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            var jobId = returnValue as string;
            auditInfo.Object = jobId;
            return auditInfo;
        }

        private RMAuditInfo CollectExportTrainingReportJob(RMAuditInfo auditInfo, object[] args, object target, object returnValue)
        {
            var jobId = returnValue as string;
            auditInfo.Object = jobId;
            return auditInfo;
        }

        private string GetTermNames(List<Guid> termIds)
        {
            string result = "";
            var terms = TermDao.GetRMTermsByTermIds(termIds);
            if (terms != null && terms.Count > 0)
            {
                var termFullPathDic = TermDao.GetTermFullPathByTermIds(terms.Select(o => o.Id).ToList());
                result = string.Join(';', termFullPathDic.Values.ToArray());
            }
            return result;
        }

        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
        }

        private string GetCurrentModeString(TrainingMode currentMode) => currentMode switch
        {
            TrainingMode.MLTraining => "RM_ML_IntelligentTerm_Tab_ML",
            TrainingMode.ZeroShot => "RM_ML_IntelligentTerm_Tab_Zero",
            _ => "RM_ML_IntelligentTerm_Tab_Zero"
        };

        private string GetTrainingScopeOption(TrainingScopeOption currentScopeOption) => currentScopeOption switch
        {
            TrainingScopeOption.Manual => "RM_ML_TrainingScope_ManagePanel_Option03",
            TrainingScopeOption.FromLocation => "RM_ML_TrainingScope_ManagePanel_Option02",
            TrainingScopeOption.Auto500Laster => "RM_ML_TrainingScope_ManagePanel_Option01",
            _ => "RM_ML_TrainingScope_ManagePanel_Option01"
        };

        private string GetSourceFlag(MTSSourceFlag sourceFlag) => sourceFlag switch
        {
            MTSSourceFlag.Google => "RM_ML_TrainingScope_ManagePanel_Location_GoogleDrive",
            MTSSourceFlag.SPO => "RM_ML_TrainingScope_ManagePanel_Location_SPO",
            _ => "RM_ML_TrainingScope_ManagePanel_Location_SPO"
        };
    }
}
