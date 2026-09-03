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
using AvePoint.RA.Contract.ReportCenter.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RAReportCenter.Model
{
    public class TermTreeNode
    {
        [JsonProperty("key")]
        public string Key { get; set; }

        [JsonProperty("id")]
        public int Id { get; set; }

        [JsonProperty("checkable")]
        public bool Checkable { get; set; }

        [JsonProperty("checkStatus")]
        public CheckStatus CheckStatus { get; set; }

        [JsonProperty("type")]
        public string Type { get; set; }

        [JsonProperty("children")]
        public List<TermTreeNode> Children { get; set; } = new List<TermTreeNode>();
    }
}
