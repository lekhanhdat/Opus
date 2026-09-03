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
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.Physical;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Explorer
{
    public class BoxLoanJobMessage
    {
        public LoanAction LoanAction { get; set; }
        public PhysicalRequestParam RequestsParam { get; set; }
        public List<PhysicalReturnObject> Returns { get; set; }
    }

    public class PhysicalReturnObject
    {
        public Guid UniqueId { get; set; }
        public int NodeType { get; set; }
    }
    public enum LoanAction
    {
        Loan = 1,
        Reutrn = 2
    }

    public class ItemActionResult
    {
        public ActionResultStatus Status { get; set; }
        public string Comment { get; set; }
    }

    public enum ActionResultStatus
    {
        Successful = 0,
        Failed = 1,
        Skipped = 2,
    }
}
