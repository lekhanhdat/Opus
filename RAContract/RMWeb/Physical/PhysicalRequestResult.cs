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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb.Physical
{
    public class PhysicalRequestResult
    {
        public bool HasError { set; get; }

        public string ErrorMsg { set; get; }

        public int TotalCount { set; get; }

        public List<PhysicalRequestDto> RequestList { set; get; }

        public List<int> FailedIdList { set; get; }

        public List<Guid> FailedGuidIdList { set; get; }

        public bool NeedConfirmIgnoreReturnDate { set; get; }

        public bool StartLoanBoxJob { set; get; }

        public EPhysicalRequestType FailedType { set; get; } = EPhysicalRequestType.None;

        public MoveResult MoveResult { set; get; }
    }
    public class MoveResult
    {
        public string JobId { set; get; }
        public bool IsStartJob { set; get; }
    }
}
