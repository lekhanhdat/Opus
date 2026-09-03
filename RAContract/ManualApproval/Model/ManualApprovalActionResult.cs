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
using AvePoint.RA.Contract.Schedule;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.ManualApproval.Model
{
    public class ManualApprovalActionResult
    {
        [JsonProperty("completedStatus")]
        public ActionCompletedStatus CompletedStatus { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("effectItems")]
        public List<ManualApprovalItemActionResult> EffectItems { get; set; } = new List<ManualApprovalItemActionResult>();
    }

    public class ManualApprovalItemActionResult
    {
        [JsonProperty("id")]
        public Guid Id { get; set; }

        [JsonProperty("isSucceed")]
        public bool IsSucceed { get; set; }

        [JsonProperty("message")]
        public string Message { get; set; }

        [JsonProperty("oldValue")]
        public object OldValue { get; set; }

        [JsonProperty("effectItemFullPath")]
        public string EffectItemFullPath { get; set; }

        [JsonProperty("extendType")]
        public ManualApprovalExtendType ExtendType { get; set; }

        [JsonProperty("extendTime")]
        public DateTime ExtendTime { get; set; }
    }

    public enum ActionCompletedStatus
    {
        Succeed,
        Failed,
        HasException,
    }
}
