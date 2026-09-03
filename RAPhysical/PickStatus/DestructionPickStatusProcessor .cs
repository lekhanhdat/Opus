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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Explorer;
using AvePoint.RA.Contract.RMWeb.JobMonitor;
using AvePoint.RA.Contract.RMWeb.Physical;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using AvePoint.RA.Contract.TemplateManagement;
using AvePoint.RA.DB.Explorer.Model;
using DocumentFormat.OpenXml.Wordprocessing;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAPhysical.PickStatus
{
    public class DestructionPickStatusProcessor : BasePickStatusProcessor
    {
        public DestructionPickStatusProcessor(JobType jobType, string jobId) : base(jobType, jobId)
        {
        }

        protected override ExplorerQueryV3Dto GetQueryDto(CompleteActionParam jobParam, string pageIndex)
        {
            var queryDto = PickListService.GetDestructionQueryDto(pageIndex, PageSize, jobParam.SearchText, jobParam.FilterOptions);
            if ((jobParam.SelectedItemIds?.Any() ?? false) && jobParam.IsContainerLevel)
            {
                queryDto.QueryOption.Values.Add(new ExplorerSearchOptionV3
                {
                    Column = new ExplorerQueryColumn { Id = QueryCloumnIds.Id },
                    Value = JsonConvert.SerializeObject(jobParam.SelectedItemIds)
                });
            }
            return queryDto;
        }

        protected override async Task ProcessRecordsAsync(BaseRecordDto rec)
        {
            if (rec.DestructionPickStatus == (int)PickStatusType.Pendding)
            {
                successIds.Add(rec.Id);
                ExplorerDao.UpdateAll(s => s.Id == rec.Id, r => { r.DestructionPickStatus = (int)PickStatusType.Complete; });
                SendDetails(rec);
            }
            else
            {
                SendDetails(rec, JobDetailsStatus.Skipped);
            }

            IEnumerable<Record> children = null;
            if (rec.NodeType == (int)RMNodeType.PhyBox)
            {
                var physicalTypes = new List<int>() { (int)RMRecordStatus.Active, (int)RMRecordStatus.Closed, (int)RMRecordStatus.Missing, (int)RMRecordStatus.Destroyed };
                children = ExplorerDao.QueryAll(r => (physicalTypes.Contains(r.RecordStatus)) && r.SourceFlag == (int)SourceFlag.Physical && (r.BoxId == rec.Id || r.ParentId == rec.Id) && r.NodeType == (int)RMNodeLevel.PhysicalFile);
                if (children != null && children.Any())
                {
                    foreach (var subRecord in children)
                    {
                        if (subRecord.DestructionPickStatus == (int)PickStatusType.Pendding)
                        {
                            successIds.Add(subRecord.Id);
                            ExplorerDao.UpdateAll(s => s.Id == subRecord.Id, r => { r.DestructionPickStatus = (int)PickStatusType.Complete; });
                            SendDetails(subRecord);
                        }
                    }
                }
            }
        }
        protected override Task PrepareProcessAsync() { return Task.CompletedTask; }
        protected override async Task AfterProcessAsync() { }
    }
}
