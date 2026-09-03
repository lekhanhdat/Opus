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
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.Threads;
using System;

namespace AvePoint.RA.Contract.Workflow
{
    public class BaseReviewRequestInfo
    {
        /// <summary>
        /// related object data id
        /// </summary>
        public Guid RequestId { get; set; }

        /// <summary>
        /// workflow instance id
        /// </summary>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// workflow definition id
        /// </summary>
        public Guid DefinitionId { get; set; }

        public ThreadSetting ThreadSetting { get; set; }
    }

    //Define the request info, will add more properties later.
    public class DisposalReviewRequestInfo : BaseReviewRequestInfo
    {
        public DisposalReviewActionEnum Action { get; set; }
        /// <summary>
        /// send email to reviewers
        /// </summary>
        public bool IsSendEmail { get; set; }
        public AzureTableConnectContract ArchiverTableConnInfo { get; set; }
        public string TenantGroupId  { get; set; }
        public string PartionKey { get; set; }
        public string RowKey { get; set; }
        public SourceFlag Source { get; set; }
        /// <summary>
        /// history display
        /// </summary>
        public string ActionBy { get; set; }

        /// <summary>
        /// Manual Review Display
        /// </summary>
        public string ActionUserId { get; set; }
    }
}
