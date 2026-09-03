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
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.JobMonitor;
using AvePoint.RA.Contract.RMWeb.ReportCenter;
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IProfileDao : IBaseDao<RMProfile>
    {

        bool DeleteProfiles(List<int> ids);
        //bool RealDeleteProfiles(List<int> ids);
        Task<bool> RealDeleteProfilesAndJobsAsync(List<int> ids);
        bool EditProfile(RMProfile profile);

        /// <summary>
        /// 后台分页查询Profile
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <param name="pageSize"></param>
        /// <param name="totalRecord"></param>
        /// <param name="orderKey"></param>
        /// <param name="isAsc"></param>
        /// <param name="whereLambda"></param>
        /// <returns></returns>
        List<RMProfile> GetProfiles(int pageIndex, int pageSize, out int totalRecord, string orderKey, bool isAsc, Expression<Func<RMProfile, bool>> whereLambda = null);
        int SaveProfile(RMProfile profile);
        RMProfile GetProfileById(int id);
        RMProfile GetProfileByScheduleId(string scheduleId);
        IEnumerable<RMProfile> GetProfilesByTypes(List<JobType> jobTypes, List<SourceFlag> sources, string logonUserId = "");


        bool CheckProfileNameExist(RMProfileDto profile);
        List<RMProfile> GetProfileByIds(List<int> ids);

        int GetPageIndexByProfileId(int profileId);

        List<int> GetValidProfileTypesByUserId(string userId);

        List<RMProfile> GetJobNotificationProfiles();
    }
}
