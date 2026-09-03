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
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.ReportCenter.Adapter
{
    public class CreateAndDestryoedReportAdapter
    {
        public static RMProfile ConvertToDbModel(CreateAndDestryoedReportModel reportInfo)
        {
            var profile = new RMProfile
            {
                Source = reportInfo.Source,
                Id = reportInfo.Id,
                Name = reportInfo.ProfileName,
                Description = reportInfo.Description,
                Modified = reportInfo.Modified,
                CreateProfileLogonUserId = reportInfo.CreateBy,
                Extension2 = reportInfo.CheckedTreeStructure,
            };

            reportInfo.CheckedTreeStructure = null;
            profile.Extension1 = JsonConvert.SerializeObject(reportInfo);

            return profile;
        }

        public static CreateAndDestryoedReportModel ConvertToReportModel(RMProfile profile)
        {
            var reportInfo = new CreateAndDestryoedReportModel
            {
                Source = profile.Source,
                Id = profile.Id,
                ProfileName = profile.Name,
                Description = profile.Description,
                CreateBy = profile.CreateProfileLogonUserId,
                Modified = profile.Modified,
                CheckedTreeStructure = profile.Extension2
            };

            var otherInfo = JsonConvert.DeserializeObject<CreateAndDestryoedReportModel>(profile.Extension1);
            reportInfo.ActionType = otherInfo.ActionType;
            reportInfo.DateFrameType = otherInfo.DateFrameType;
            reportInfo.CustomStartDate = otherInfo.CustomStartDate;
            reportInfo.CustomEndDate = otherInfo.CustomEndDate;

            return reportInfo;
        }
    }
}
