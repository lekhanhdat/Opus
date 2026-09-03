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

using AvePoint.GCommon.Contract.Server.Job.Object;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.Service.Services.JobMonitor.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.JobMonitor.Detail
{
    public class ArchiverRestoreJobJobDetailWorker : AbstractDaoMigrationJobDetailWorker
    {

        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, ref int totalCount, string conditionFilter, BaseJobDto jobInfo)
        {
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            totalCount = base.GetCountForDetail(reportFilePath, base.SELECT_DETAIL_COUNT_SQL, jobInfo);
            return GetData(PageSize, StartPage, conditionFilter, jobInfo);
        }
        public override IEnumerable<JMJobDetails> GetData(int PageSize, int StartPage, string conditionFilter, BaseJobDto jobInfo)
        {
            IEnumerable<JMJobDetails> result = null;
            string reportFilePath = DownloadReports(jobInfo);
            TABLE_NAME = JobMonitorConstants.JOBDETAIL;
            InitGetDataSQLString(PageSize, StartPage, conditionFilter);
            bool isRPTExist = CheckFileExist(reportFilePath);
            bool isTableInRPTExist = JobDetailDao.IsExistTable(reportFilePath, TABLE_NAME);
            if (!isRPTExist || !isTableInRPTExist)
            {
                logger.Debug("about {0} database exist:{1},table exist{2}", jobInfo.Id, isRPTExist, isTableInRPTExist);
                return result;
            }
            result = JobDetailDao.GetData(reportFilePath, base.SELECT_DATA_SQL, jobInfo);

            foreach (JMDisposalJobDetails jobDetail in result.Cast<JMDisposalJobDetails>())
            {
                jobDetail.DetailsTab = (JobReportDetailEntityType)jobDetail.EntityType switch
                {
                    JobReportDetailEntityType.Export => "RM_JS_JM_EntityType_Export",
                    JobReportDetailEntityType.NormalInfo => "RM_JS_JM_EntityType_Backup",
                    JobReportDetailEntityType.ArchiveDeletion => "RM_JS_JMD_Grid_Action",//RECO-3973 Action
                    JobReportDetailEntityType.RecordManager => "RM_JS_JM_EntityType_RecordDeclaration",
                    _ => ""
                };
                jobDetail.DetailsTab = I18NEntity.GetString(jobDetail.DetailsTab);
                jobDetail.Size = JobDetailHelper.GetDataSizeToView(jobDetail.SizeNumber);
                jobDetail.Action = GetDataOperation(jobDetail.Action);
                jobDetail.Comment = ConvertXmlToI18NString(jobDetail.Comment);
            }

            return result;
        }

        private string GetDataOperation(string operation)
        {
            switch (operation)
            {
                case SOConstants.Operation_Delete:
                    return I18NEntity.GetString("StorageOptimization.Service_61150B8C-0DFD-4EB6-AD78-5A56C93205F1");//"Delete it"
                case SOConstants.Operation_Keep:
                    return I18NEntity.GetString("StorageOptimization.Service_1FAA997A-D777-45C4-8D8C-64D42478B481");//, "Keep it"
                case SOConstants.LeaveLinkInSharePoint:
                    return I18NEntity.GetString("StorageOptimization.Service_d70b6481-8aad-498f-a14f-611180c4a9b9");//, "Leave a stub in SharePoint"
                case SOConstants.Operation_PhysicalDelete:
                    return I18NEntity.GetString("Replicator.Service_afba914c-ef35-430e-bd22-16916e74f941");//, "Delete"
                //Only Records has this action so that I18N in records side.
                case SOConstants.Operation_DeleteOnly:
                    return "RM_JS_JM_DataOperation_DeleteOnlyFromSharePoint";
                case SOConstants.Operation_DeleteOnlyAndKeepVersion:
                    return I18NEntity.GetString("StorageOptimization.Service_7F9BB2D9-69B8-5FC1-32B3-7A3B2A1DF70C");//, "Delete Only And Keep Version"
                case SOConstants.Operation_DeleteOnlyAndDoesNotKeepVersion:
                    return I18NEntity.GetString("StorageOptimization.Service_1FEDF518-E810-4F7D-8272-7026C4417FEB");//, "Delete Only And Does Not Keep Version"
                case SOConstants.Operation_Move:
                    return "RM_JS_JM_DataOperation_SharePointMoveAction";
                case SOConstants.Operation_Archive:
                    return "RM_RDM_CreateRule_ArchiveToAzureBlobStorage";
                case SOConstants.Operation_ArchiveLeaveStub:
                    return "RM_JS_JM_DataOperation_ArchiveLeaveStub";
                default:
                    return operation;
            }
        }
      
    }
}
