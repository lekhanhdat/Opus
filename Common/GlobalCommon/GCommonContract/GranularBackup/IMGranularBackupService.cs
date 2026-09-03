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




namespace AvePoint.GCommon.Contract.GranularBackup
{
    #region == using directives ==
    using System;
    using System.Collections.Generic;
    using System.ServiceModel;
    using AvePoint.GCommon.Contract.Common;
    using AvePoint.GCommon.Contract.GranularBackup.Object;
    using AvePoint.GCommon.Contract.GranularRestore.Object;
    using AvePoint.GCommon.Contract.Media.Object;
    using AvePoint.GCommon.Contract.Server.ControlPanel.PlanGroup.Object;
    using AvePoint.GCommon.Contract.Server.GranularBackup.Object;
    #endregion ==

    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMGranularBackupService
    {
        /// <summary> 用来创建SiteMasterIndex(主记录).</summary>
        /// <param name="siteMasterIndex"></param>
        /// <param name="subInfo"></param>
        IndexOperationResult CreateSiteMasterIndexInfo(SiteMasterIndexDto siteMasterIndex);

        /// <summary> 用来创建SiteMasterIndexSubDto(子记录). </summary>
        /// <param name="subInfo"></param>
        void CreateSiteMasterIndexSubInfo(SiteMasterIndexSubDto subInfo);

        /// <summary> 更新SiteMasterIndexSubDto(子记录)记录. </summary>
        /// <param name="siteMasterIndex"></param>
        void UpdateSiteMasterIndexSubInfo(SiteMasterIndexSubDto siteMasterIndex);

        /// <summary>删除SiteMasterIndex(主记录)或者SiteMasterIndexSub(子记录)记录. </summary>
        /// <param name="id"></param>
        void DeleteSiteMasterIndexInfo(string id);

        /// <summary> 更新SiteMasterIndex(主记录)的FullTextState值和主Job表的IndexStatus值. </summary>
        /// <param name="jobId"></param>
        /// <param name="status"></param>
        void UpdateFullTextState(string jobId, ItemFullTextState status);

        ///// <summary> According to the subJob Id, update the subJob performance value. </summary>
        ///// <param name="subJobId"></param>
        ///// <param name="jobPerformance"></param>
        //void UpdateSubJobPerformance(string subJobId, string jobPerformance);

        void UpdateRestoreCacheTree(RestoreCacheTreeContract treeContent);

        /// <summary> This method mainly provide external calls(Content Manager,Replicator module).</summary>
        /// <param name="simplePlan"></param>
        /// <param name="planId">backup planId</param>
        /// <param name="jobId">backup jobId</param>
        void GenerateBackupJobBySimplePlan(GranularBackupSimplePlanDto simplePlan, out string planId, out string jobId);
        /// <summary>
        /// This method mainly provide external calls(Content Manager,Replicator module).
        /// </summary>
        /// <param name="simplePlan"></param>
        /// <param name="parentPlanId"></param>
        /// <param name="userId">the id of user who create plan</param>
        /// <param name="planId"></param>
        /// <param name="jobId"></param>
        void GenerateBackupJobBySimplePlan(GranularBackupSimplePlanDto simplePlan, string parentPlanId, string userId, out string planId, out string jobId);

        GranularBackupJobDto GetGranularBackupJobByJobId(string backupJobId);

        /// <summary> Migration export SP07 to SP10. </summary>
        /// <param name="migrationPlanSettings"></param>
        String GenerateMigrationJob(MigrationBackupPlanDto migrationPlanSettings);

        PlanGroupJobResult CreatePlanGroupJob(MigrationBackupPlanDto migrationPlanSettings);

        void RunBackupJobById(string jobId);

        void PruningBackupDataByJobIds(List<string> jobIds, bool deleteJob);

        void HandelRetentionJob(GranularPruningResult result, GranularPruningMessage pruningMsg);
    }
}
