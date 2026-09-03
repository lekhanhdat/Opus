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
using AvePoint.RA.Contract.RMWeb;
using ExportSetting = AvePoint.GCommon.Contract.StorageOptimization.Object.ExportSetting;

namespace AvePoint.RA.ArchiverMigration.JobStage
{
    internal class MigrateExportSettingStage : AbstractArchiverMigrationStage
    {

        public override string StageType => "Export Setting Stage";

        /* Fortify Issue Type: Insecure Randomness 
        * Sink Details:  AvePoint.RA.ArchiverMigration ArchiverMigrationJobExecutor  ResetJobProgressUpdaterAsync
        * Ignore Reason: random用于job进程参数，不涉及安全问题
        */
        public override int JobProgressWeight => new Random().Next(1, 3);

        public override string JobDetailType => throw new NotImplementedException();

        private IExportSettingService ExportSettingService => PlatformWindsorManager.GetService<IExportSettingService>();


        protected override async Task InnerExecuteAsync()
        {
            var templateInfo = await GetVEOTempateAsync();
            if(templateInfo != null && !string.IsNullOrEmpty(templateInfo.TemplateName) && templateInfo.Template != null)
            {
                await ExportSettingService.MigrateVEOTemplateAsync(templateInfo.Template, templateInfo.TemplateName);
            }

            JobProgressUpdater.Increase(1);
        }

        public override Task<int> GetStageProgressBaseSizeAsync()
        {
            return Task.FromResult(1);
        }

        private async Task<ExportSetting> GetVEOTempateAsync()
        {
            return await GetArchiverMigrationDataAsync<ExportSetting>((service) =>
            {
                return service.GetVEOTemplate();
            });
        }
    }
}
