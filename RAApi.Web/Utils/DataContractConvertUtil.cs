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
using AvePoint.RA.Api.Contract;
using AvePoint.RA.Contract.Security;
using AvePoint.RA.Contract.TaxonomyModel;
using Cloud.sdk.Data.Records.Classification;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace AvePoint.RA.Web.Common.Utils
{
    public class DataContractConvertUtil
    {
        public static TermTreeNode Convert2TermTreeNode(RMTermInfo dto)
        {
            return new TermTreeNode()
            {
                Id = dto.UniqueId,
                Name = dto.Name,
                Level = (TermLevel)dto.Type
            };
        }
        public static TermTreeNode ConvertTermTreeNode(Contract.RMReport.TermTreeNode treeNode)
        {
            TermTreeNode termInfo = new TermTreeNode();
            termInfo.Id = treeNode.ID;
            termInfo.Name = treeNode.Name;
            termInfo.Level = (TermLevel)treeNode.Type;
            termInfo.Children = treeNode.Children == null ? null : ConvertTermTreeNodeList(treeNode.Children.Values.ToList());
            return termInfo;
        }

        private static List<TermTreeNode> ConvertTermTreeNodeList(List<Contract.RMReport.TermTreeNode> treeNodeList)
        {
            List<TermTreeNode> terms = new List<TermTreeNode>();
            foreach (var item in treeNodeList)
            {
                terms.Add(ConvertTermTreeNode(item));
            }
            return terms;
        }


        public static DocAveOnline.WebApi.Contracts.JobStatus ConvertToStatus(RA.Contract.RMWeb.JobMonitor.JobStatus status)
        {
            switch (status)
            {
                case RA.Contract.RMWeb.JobMonitor.JobStatus.Wait:
                    return DocAveOnline.WebApi.Contracts.JobStatus.Waiting;
                case RA.Contract.RMWeb.JobMonitor.JobStatus.InProgress:
                    return DocAveOnline.WebApi.Contracts.JobStatus.InProgress;
                case RA.Contract.RMWeb.JobMonitor.JobStatus.Finished:
                    return DocAveOnline.WebApi.Contracts.JobStatus.Finished;
                case RA.Contract.RMWeb.JobMonitor.JobStatus.Failed:
                    return DocAveOnline.WebApi.Contracts.JobStatus.Failed;
                case RA.Contract.RMWeb.JobMonitor.JobStatus.FinishWithException:
                    return DocAveOnline.WebApi.Contracts.JobStatus.FinishedWithException;
                case RA.Contract.RMWeb.JobMonitor.JobStatus.Stopped:
                    return DocAveOnline.WebApi.Contracts.JobStatus.Stopped;
                case RA.Contract.RMWeb.JobMonitor.JobStatus.Skipped:
                    return DocAveOnline.WebApi.Contracts.JobStatus.Skipped;
                case RA.Contract.RMWeb.JobMonitor.JobStatus.Stopping:
                    return DocAveOnline.WebApi.Contracts.JobStatus.InProgress;//recenter not surpport stopping status
                default:
                    return DocAveOnline.WebApi.Contracts.JobStatus.Waiting;
            }
        }
    }
}