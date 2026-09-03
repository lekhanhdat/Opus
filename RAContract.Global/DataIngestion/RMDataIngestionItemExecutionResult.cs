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

namespace AvePoint.RA.Contract.DataIngestion
{
    public class RMDataIngestionAgentWorkItemExecutionResult
    {
        public Guid Id { get; set; }

        public int NodeType { get; set; }

        public string LeafName { get; set; }

        public string DirPath { get; set; }

        public bool Succeed { get; set; }

        public string Message { get; set; }

        public long Size { get; set; }

        //public long StartTime { get; set; }

        public long FinishTime { get; set; }

        public int RuleAction { get; set; }

        public string RuleName { get; set; }

        public int Status { get; set; }

        public long Depth { get; set; }

        public bool HasRuleChanged { get; set; }

        public bool HasTermChanged { get; set; }
    }
}
