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
using AvePoint.RA.Common.Audit;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Audit;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.PickList.AuditHandler
{
    public class PickListAfterAuditHandler : IAfterAuditHandler
    {
        private RA.DB.Explorer.Dao.IExplorerDao _explorerDao;
        public RA.DB.Explorer.Dao.IExplorerDao ExplorerDao
        {
            get
            {
                if (_explorerDao == null)
                {
                    _explorerDao = new RA.DB.Explorer.Dao.CosmosImp.ExplorerDao();
                }
                return _explorerDao;
            }
        }

        public async Task<RMAuditInfo> CollectAsync(RMAuditInfo info, int model, int category, int action, object[] args, object target, object returnValue)
        {
            RMAuditInfo auditInfo = new()
            {
                Module = (AuditModule)model,
                Category = (AuditCategory)category,
                Action = (AuditAction)action
            };

            if ((AuditAction)action == AuditAction.PhysicalLoanPickComplete)
            {
                CompleteActionParam param = args[0] as CompleteActionParam;
                PickObjectType? objectType = args[1] as PickObjectType?;
                RAReturnMessage result = returnValue as RAReturnMessage;

                var records = ExplorerDao.QueryAll(r => param.SelectedItemIds != null && param.SelectedItemIds.Contains(r.Id));
                auditInfo.Action = objectType switch
                {
                    PickObjectType.Loan => AuditAction.PhysicalLoanPickComplete,
                    PickObjectType.Destruction => AuditAction.PhysicalDestructionPickComplete,
                    _ => AuditAction.PhysicalLoanPickComplete,
                };
                if(auditInfo.Action == AuditAction.PhysicalLoanPickComplete)
                {
                    auditInfo.Category = AuditCategory.ReturnHistoryExport;
                }

                auditInfo.Object = string.Join("; ", records.Select(r => r.LeafName));
                auditInfo.Status = result?.MessageType == RAMessageType.Failed ? (int)AuditStatus.Failed : (int)AuditStatus.Successful;
                auditInfo.ModifyContent = new List<AuditItem>
                {
                    new AuditItem()
                    {
                        TargetSetting = I18NEntity.GetString("RM_MT_PickList_Column_Status"),
                        OldValue = I18NEntity.GetString("RM_MT_PickList_Status_PendingLoan"),
                        NewValue = I18NEntity.GetString("RM_MT_PickList_Status_Loaned")
                    }
                };

                auditInfo.ModifyContent = objectType switch
                {
                    PickObjectType.Loan => new List<AuditItem>
                                            {
                                                new AuditItem()
                                                {
                                                    TargetSetting = I18NEntity.GetString("RM_MT_PickList_Column_Status"),
                                                    OldValue = I18NEntity.GetString("RM_MT_PickList_Status_PendingLoan"),
                                                    NewValue = I18NEntity.GetString("RM_MT_PickList_Status_Loaned")
                                                }
                                            },
                    PickObjectType.Destruction => new List<AuditItem>
                                                {
                                                    new AuditItem()
                                                    {
                                                        TargetSetting = I18NEntity.GetString("RM_MT_PickList_Column_Status"),
                                                        OldValue = I18NEntity.GetString("RM_MT_PickList_Status_PendingDestroy"),
                                                        NewValue = I18NEntity.GetString("RM_MT_PickList_Status_Destroyed")
                                                    }
                                                },
                    _ => new List<AuditItem>()
                };
            }
            else if ((AuditAction)action == AuditAction.PhysicalLoanPickCompleteJob)
            {
                JobType? jobType = args[0] as JobType?;
                var jobId = returnValue as string;
                auditInfo.Object = jobId;
                auditInfo.Action = jobType switch
                {
                    JobType.PhysicalLoanPick => AuditAction.PhysicalLoanPickCompleteJob,
                    JobType.PhysicalDestructionPick => AuditAction.PhysicalDestructionPickCompleteJob,
                    JobType.PhysicalLoanPickExportJob => AuditAction.PhysicalLoanPickExportJob,
                    JobType.PhysicalDestructionPickExportJob => AuditAction.PhysicalDestructionPickExportJob,
                    JobType.PhysicalReturnHistoryExport => AuditAction.PhysicalReturnHistoryExportJob,
                    JobType.PhysicalMovePickExportJob => AuditAction.PhysicalMovePickExportJob,
                    _ => AuditAction.Unknown
                };
                auditInfo.Category = jobType switch
                {
                    JobType.PhysicalLoanPick => AuditCategory.ReturnHistoryExport,
                    JobType.PhysicalDestructionPick => AuditCategory.DestructionPickList,
                    JobType.PhysicalLoanPickExportJob => AuditCategory.LoanPickList,
                    JobType.PhysicalDestructionPickExportJob => AuditCategory.DestructionPickList,
                    JobType.PhysicalReturnHistoryExport => AuditCategory.ReturnHistoryExport,
                    JobType.PhysicalMovePickExportJob => AuditCategory.MovePickListExport,
                    _ => AuditCategory.Unknown
                };
            }
            return auditInfo;
        }
    }
}
