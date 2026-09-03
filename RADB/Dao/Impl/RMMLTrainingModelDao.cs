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
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.Objects;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.Contract.MachineLearning;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Model;

namespace AvePoint.RA.DB.Dao.Impl
{
    public class RMMLTrainingModelDao : BaseDao<RMMLTrainingModel>, IRMMLTrainingModelDao
    {
        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();
        private ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();


        public RMMLTrainingModel GetDefaultModel(bool createIfNotExists = false)
        {
            using (var context = GetNewContext())
            {
                RMMLTrainingModel model;
                //if(RMKeyValueDao.EnableZeroShotFeature())
                //{
                //    model = context.RMMLTrainingModels.Where(m => m.IsModeUsing == null || m.IsModeUsing == true).FirstOrDefault();
                //}
                //else
                //{
                //    model = context.RMMLTrainingModels.Where(m => m.Mode == TrainingMode.MLTraining).FirstOrDefault();
                //}
                var enableMaestroAI = LicenseHelperService.IsEnableMaestroAI().GetAwaiter().GetResult();
                var enableZeroShot = RMKeyValueDao.EnableZeroShotFeature();
                if (!enableMaestroAI && enableZeroShot)
                {
                    model = context.RMMLTrainingModels.Where(m => m.Mode == TrainingMode.ZeroShot).FirstOrDefault();
                }
                else if (enableMaestroAI && !enableZeroShot)
                {
                    model = context.RMMLTrainingModels.Where(m => m.Mode == TrainingMode.MLTraining).FirstOrDefault();
                }
                else
                {
                    model = context.RMMLTrainingModels.Where(m => m.IsModeUsing == null || m.IsModeUsing == true).FirstOrDefault();
                }
                if (model == null && createIfNotExists)
                {
                    var createModel = new RMMLTrainingModel()
                    {
                        Id = Guid.NewGuid(),
                        Accuracy = 0,
                        CreateTime = DateTime.UtcNow.Ticks,
                        ModifedTime = DateTime.UtcNow.Ticks,
                        Mode = RMKeyValueDao.EnableZeroShotFeature() ? TrainingMode.ZeroShot : TrainingMode.MLTraining,
                        IsModeUsing = true,
                    };
                    var addModel = context.RMMLTrainingModels.Add(createModel);
                    context.SaveChanges();
                    return addModel;
                }
                else
                {
                    return model;
                }
            }
        }

        public long GetLastUpdatedTime()
        {
            using var context = GetNewContext();
            var model = context.RMMLTrainingModels.FirstOrDefault(m => m.Mode == TrainingMode.MLTraining);
            if (model != null)
            {
                return model.LastTrainedTime;
            }
            return 0;
        }

        public async Task SwitchModeAsync(TrainingMode mode)
        {
            using var context = GetNewContext();
            var trainingModels = await context.RMMLTrainingModels.ToListAsync();
            bool isExistModeRecord = false;
            foreach (var trainingModel in trainingModels) {
                if(trainingModel.Mode == mode)
                {
                    isExistModeRecord = true;
                    trainingModel.IsModeUsing = true;
                    continue;
                }
                trainingModel.IsModeUsing = false;
            }
            //Update training model mode
            this.BatchUpdate(trainingModels);
            if (!isExistModeRecord)
            {
                var newModel = new RMMLTrainingModel()
                {
                    Id = Guid.NewGuid(),
                    Accuracy = 0,
                    CreateTime = DateTime.UtcNow.Ticks,
                    ModifedTime = DateTime.UtcNow.Ticks,
                    Mode = mode,
                    IsModeUsing = true,
                };
                context.RMMLTrainingModels.Add(newModel);
                await context.SaveChangesAsync();
            }
        }

        public void ChangeTrainingScopeOption(MLTrainingScopeManage manage)
        {
            using var context = GetNewContext();
            var trainingModel = context.RMMLTrainingModels.Where(m => m.Mode == TrainingMode.MLTraining).FirstOrDefault();
            if (trainingModel != null)
            {
                trainingModel.TrainingScopeOption = (TrainingScopeOption)manage.TrainingScopeOption;
                if(manage.TrainingScopeOption == (int)TrainingScopeOption.FromLocation)
                {
                    trainingModel.Extension = SerializerHelper.SerializeByDataContractSerializer(manage);
                }
                else
                {
                    trainingModel.Extension = string.Empty;
                }
                context.SaveChanges();
            }
            else
            {
                var zeroModel = context.RMMLTrainingModels.Where(m => m.Mode == TrainingMode.ZeroShot).FirstOrDefault();
                var createModel = new RMMLTrainingModel()
                {
                    Id = Guid.NewGuid(),
                    Accuracy = 0,
                    CreateTime = DateTime.UtcNow.Ticks,
                    ModifedTime = DateTime.UtcNow.Ticks,
                    Mode = TrainingMode.MLTraining,
                    IsModeUsing = zeroModel == null ? true : !(zeroModel.IsModeUsing ?? true),
                    TrainingScopeOption = (TrainingScopeOption)manage.TrainingScopeOption,
                    Extension = manage.TrainingScopeOption == (int)TrainingScopeOption.FromLocation ? SerializerHelper.SerializeByDataContractSerializer(manage) : string.Empty,
                };
                context.RMMLTrainingModels.Add(createModel);
                context.SaveChanges();
            }
        }

        public MLTrainingScopeManage GetTrainingScopeOption()
        {
            using var context = GetNewContext();
            var trainingModel = context.RMMLTrainingModels.Where(m => m.Mode == TrainingMode.MLTraining).FirstOrDefault();
            if (trainingModel.TrainingScopeOption == TrainingScopeOption.FromLocation)
            {
                var trainingScopeManage = SerializerHelper.DeserializeByDataContractSerializer<MLTrainingScopeManage>(trainingModel.Extension);
                return new MLTrainingScopeManage
                {
                    TrainingScopeOption = (int)trainingModel.TrainingScopeOption,
                    Location = trainingScopeManage.Location,
                    SourceFlag = trainingScopeManage.SourceFlag,
                    LocationId = trainingScopeManage.LocationId
                };
            }
            return new MLTrainingScopeManage
            {
                TrainingScopeOption = (int)trainingModel.TrainingScopeOption
            };
        }
    }
}
