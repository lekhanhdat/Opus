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
using System.Collections.Generic;
using System.Text;

namespace AvePoint.Wrapper.Common
{
    public interface IAveConfigurationDatabase : IAveDatabase, IAvePersistedStoreProvider, IDisposable
    {
        Dictionary<AveJobType, List<AveTimerJobStatus>> GetWebApplicationTimerJobs(AveJobsPageInfo pageInfo);
        Dictionary<AveJobType, List<AveTimerJobStatus>> GetServiceTimerJobs(AveJobsPageInfo pageInfo);
        Dictionary<AveJobType, List<AveTimerJobStatus>> GetServerTimerJobs(AveJobsPageInfo pageInfo);
        Dictionary<AveJobType, List<AveTimerJobStatus>> GetJobDefinitionTimerJobs(AveJobsPageInfo pageInfo);
        Dictionary<AveJobType, List<AveTimerJobStatus>> GetAllTimerJobs(AveJobsPageInfo pageInfo);
        Dictionary<AveJobType, int> GetTotalJobsCount(AveJobsPageInfo pageInfo);

        /// <summary>
        /// Update the info of the site collection in the SharePoint_Config DB,the method is implemented in the 07 codes but not in the 10 codes
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="webappId"></param>
        /// <param name="databaseId"></param>
        /// <param name="siteRelativeUrl"></param>
        /// <param name="hostHeaderIsSiteName"></param>
        /// <returns></returns>
        void AddSite(Guid siteId, Guid webappId, Guid databaseId, string siteRelativeUrl, bool hostHeaderIsSiteName);
        IAveConfigurationDatabase Local { get; }
    }

    public class AveTimerJobStatus
    {
        public string JobTitle { get; set; }
        public string Server { get; set; }
        public int Status { get; set; }
        public int Progress { get; set; }
        public string Started { get; set; }
        public string Ended { get; set; }
        public string WebApplication { get; set; }
    }

    public class AveJobsPageInfo
    {
        public AveJobType JobType { get; set; }
        public int PageSize { get; set; }
        public int CurPage { get; set; }
        public string WebAppId { get; set; }
        public string ServiceId { get; set; }
        public string ServerId { get; set; }
        public string JobDefinitionId { get; set; }
        public int Status { get; set; }
    }

    public enum AveJobType
    {
        ScheduledJob = 0,
        RunningJob = 1,
        HistoryJob = 2,
        AllJob = 3
    }
}
