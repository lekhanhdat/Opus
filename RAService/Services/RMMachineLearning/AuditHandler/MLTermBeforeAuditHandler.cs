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
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.TermManagement;
using System;
using System.Collections.Generic;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.RMMachineLearning.AuditHandler
{
    public class MLTermBeforeAuditHandler : IBeforeAuditHandler
    {
        private static IRMMLTermDao RMMLTermDao => PlatformWindsorManager.GetService<IRMMLTermDao>();
        private static IRMMLTrainingModelDao RMMLTrainingModelDao => PlatformWindsorManager.GetService<IRMMLTrainingModelDao>();
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        public async Task<RMAuditInfo> CollectAsync(int model, int category, int action, object[] args, object target)
        {
            AuditAction act = (AuditAction)action;
            if (act == AuditAction.SetAutoApply)
            {
                var info = new RMAuditInfo();
                info.Module = (AuditModule)model;
                info.Category = (AuditCategory)category;
                info.Action = (AuditAction)action;

                if (Guid.TryParse(args[0].ToString(), out Guid termId))
                {
                    var dbModel = RMMLTermDao.Find(a => a.Id == termId);
                    info.ModifyContent = new List<AuditItem>();
                    info.ModifyContent.Add(new AuditItem() { TargetSetting = AuditConstants.Audit_MachineLearning_EnableAutoApply_Title, OldValue = YesOrNoString(dbModel.AutoApply) });
                }
                return info;
            }
            if(act == AuditAction.UpdateTermDescription)
            {
                var info = new RMAuditInfo();
                info.ModifyContent = new List<AuditItem>();
                var param = args[0] as MLTermDto;
                var dbModel = RMMLTermDao.Find(a => a.Id == param.Id);
                AuditItem termDescription = new AuditItem();
                termDescription.TargetSetting = AuditConstants.Audit_MachineLearning_Update_Description;
                termDescription.OldValue = dbModel.Description;
                info.ModifyContent.Add(termDescription);
                return info;
            }
            else
            { 
                switch(act)
                {
                    case AuditAction.SwitchMode:
                        {
                            var info = GenerateAuditInfo();
                            var currentMode = RMMLTrainingModelDao.GetDefaultModel()?.Mode ?? TrainingMode.ZeroShot;
                            info.ModifyContent =
                            [
                                new AuditItem() { TargetSetting = AuditConstants.Audit_MachineLearning_SwitchMode, OldValue = GetCurrentModeString(currentMode)},
                            ];
                            return info;
                        }
                    case AuditAction.ChangeTrainingScopeOption:
                        {
                            var info = GenerateAuditInfo();
                            var currentMode = RMMLTrainingModelDao.GetDefaultModel()?.Mode ?? (RMKeyValueDao.EnableZeroShotFeature() ? TrainingMode.ZeroShot : TrainingMode.MLTraining);
                            if(currentMode == TrainingMode.MLTraining)
                            {
                                var currentScopeOption = RMMLTrainingModelDao.GetTrainingScopeOption();
                                info.ModifyContent =
                                [
                                    new AuditItem() { TargetSetting = AuditConstants.Audit_MachineLearning_ChangeTrainingOption, OldValue = GetTrainingScopeOption((TrainingScopeOption)currentScopeOption.TrainingScopeOption)}
                                ];
                                if (currentScopeOption.TrainingScopeOption == (int)TrainingScopeOption.FromLocation)
                                {
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = AuditConstants.Audit_MachineLearning_FromLocation_SourceFlag, OldValue = GetSourceFlag(currentScopeOption.SourceFlag)});
                                    info.ModifyContent.Add(new AuditItem() { TargetSetting = AuditConstants.Audit_MachineLearning_FromLocation_Location, OldValue = currentScopeOption.Location });
                                }
                            }
                            return info;
                        }
                }
                return null;
            }

            RMAuditInfo GenerateAuditInfo()
            {
                return new RMAuditInfo()
                {
                    Module = (AuditModule)model,
                    Category = (AuditCategory)category,
                    Action = (AuditAction)action,
                };
            }
        }

        private string GetSourceFlag(MTSSourceFlag sourceFlag) => sourceFlag switch
        {
            MTSSourceFlag.Google => "RM_ML_TrainingScope_ManagePanel_Location_GoogleDrive",
            MTSSourceFlag.SPO => "RM_ML_TrainingScope_ManagePanel_Location_SPO",
            _ => "RM_ML_TrainingScope_ManagePanel_Location_SPO"
        };

        private string GetTrainingScopeOption(TrainingScopeOption currentScopeOption) => currentScopeOption switch
        {
            TrainingScopeOption.Manual => "RM_ML_TrainingScope_ManagePanel_Option03",
            TrainingScopeOption.FromLocation => "RM_ML_TrainingScope_ManagePanel_Option02",
            TrainingScopeOption.Auto500Laster => "RM_ML_TrainingScope_ManagePanel_Option01",
            _ => "RM_ML_TrainingScope_ManagePanel_Option01"
        };


        private string GetCurrentModeString(TrainingMode currentMode) => currentMode switch
        {
            TrainingMode.MLTraining => "RM_ML_IntelligentTerm_Tab_ML",
            TrainingMode.ZeroShot => "RM_ML_IntelligentTerm_Tab_Zero",
            _ => "RM_ML_IntelligentTerm_Tab_Zero"
        };

        private string YesOrNoString(bool boolValue)
        {
            return boolValue ? I18NEntity.GetString("RM_JS_Common_Yes") : I18NEntity.GetString("RM_JS_Common_No");
        }
    }
}
