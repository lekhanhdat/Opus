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
using AvePoint.Common;
using AvePoint.GCommon.Contract.Server.Job.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.SharePoint.RecordsUniqueIdSetting.JobReport
{
    public class SPUniqueIdSettingReportEntry 
    {
        private string mObjName;
        private string mUrl;
        private string mColumnName;
        private string mUniqueID;
        private JobReportDetailStatus mStatus;
        private string mAction;
        private string mMessage;
        public SPUniqueIdSettingReportEntry(string name, string action,string url, string columnName,
            string uniqueID, JobReportDetailStatus status, string message)
        {
            mObjName = name;
            mUrl = url;
            mColumnName = columnName;
            mAction = action;
            mUniqueID = uniqueID;
            mMessage = message;
            mStatus = status;
        }
        //public override JobDetail ToJobDetail()
        //{
        //    return new JobDetail
        //    {
        //        //Type = Type.ToString(),
        //        SrcAgentHost = AveEnv.AgentAddress,
        //        Date = DateTime.UtcNow.Ticks,
        //        Message = mMessage,
        //        SrcURL = mUrl,
        //        Status = (int)mStatus,
        //        Title = mObjName,
        //        Remark3 = mColumnName,
        //        Remark4 = mUniqueID,
        //        Remark5 = mAction,
        //    };
        //}
    }
}
