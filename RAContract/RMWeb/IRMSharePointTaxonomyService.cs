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
using AvePoint.RA.Contract.Global.JobMessage;
using AvePoint.RA.Contract.Global.Object;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.RMWeb
{
    public interface IRMSharePointTaxonomyService
    {
        Task<RAReturnMessage> RunSyncRMTermTreeToSharePointAsync(JobRunBy jobRunBy, bool fromTimerJobPage, bool fromGoogleOne = false);
        string RealRunSyncJob(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage, bool fromGoogleOne);
        Task<string> RealRunSyncJobForSPOnpremAsync(JobRunBy jobRunBy, string jobRunByUser, bool fromTimerJobPage);
        List<GRMTermGroup> LoadTermNodes();
        List<GRMTermGroupMembership> GetAllTermGroupMembership();
        Task<string> GetTermSyncJobMessageAsync(string jobId);
    }

    public enum JobRunBy
    {
        Control = 1,
        Schedule = 2,
        ChangeTab = 3,
    }
    [DataContract]
    public enum RunApplySettingMethod
    {
        /// <summary>
        /// run settingTime等于0的scope
        /// </summary>
        [EnumMember]
        UpdatedScope = 1,
        /// <summary>
        /// run full job
        /// </summary>
        [EnumMember] 
        AllScope = 2,
        /// <summary>
        /// 没有SettingTime等于0的数据则跑full job,否则只跑settingTime等于0的scope
        /// </summary>

        [EnumMember] 
        Auto = 3,
        [EnumMember]
        SelectedNode = 4

    }
    [DataContract]
    public class RunApplySettingjobParam
    {
        [DataMember]
        public bool FromTimerJobPage { get; set; }
        [DataMember]
        public RunApplySettingMethod RunJobMethod { get; set; }
    }
}
