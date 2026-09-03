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
using AvePoint.RA.Contract.FileSystem;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.OnPremiseSharePoint;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Contract.Global.JobMessage
{
    public class EnforceRuleActionJobMessage
    {
        public List<RMSPTreeNode> TreeNodes { set; get; }
        public Dictionary<Guid, RMSharePointOnPremiseSetting> GroupSettingMapping { set; get; }
        public List<RMSharePointOnPremiseSetting> AllSettings { get; set; }
        public List<FSTermDto> AllTerms { get; set; }
        public List<AgentTermSetMembershipDto> AllTermSetMemberships { get; set; }
        public List<AgentTermSetDto> AllTermSets { get; set; }
        //term-->rule id mapping
        public Dictionary<Guid, List<Guid>> TermIDRuleIDMapping { get; set; }
        public Dictionary<Guid, RMRuleItemCollection> TermAndRulesMapping { get; set; }
        public string AllRecordsRule { set; get; }
        public List<string> RunningJobNodeUrls { get; set; }
        public List<string> BreakTreeNodeUrls { get; set; }
        public string GeneralSettingModel { get; set; }
        public string TimeFormat { get; set; }
    }
}
