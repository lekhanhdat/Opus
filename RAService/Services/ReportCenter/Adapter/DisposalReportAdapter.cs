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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ReportCenter.Adapter
{
    public class DisposalReportAdapter
    {
        public static RMProfile ConvertToDbModel(DisposalReportModel reportInfo)
        {
            var profile = new RMProfile
            {
                Source = reportInfo.Source,
                Id = reportInfo.Id,
                Name = reportInfo.ProfileName,
                Description = reportInfo.Description,
                Modified = reportInfo.Modified,
                CreateProfileLogonUserId = reportInfo.CreateBy,
                Extension1 = reportInfo.ApplyRuleBeforeTime,
                Extension2 = reportInfo.CheckedTreeStructure,
            };

            return profile;
        }

        public static DisposalReportModel ConvertToReportModel(RMProfile profile)
        {
            var reportInfo = new DisposalReportModel
            {
                Source = profile.Source,
                Id = profile.Id,
                ProfileName = profile.Name,
                Description = profile.Description,
                CreateBy = profile.CreateProfileLogonUserId,
                ApplyRuleBeforeTime = profile.Extension1,
                Modified = profile.Modified,
                CheckedTreeStructure = profile.Extension2
            };

            return reportInfo;
        }
    }
}
