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
using AvePoint.RA.DB.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.Dao
{
    public interface IRMScheduleDao : IBaseDao<RMSchedule>
    {
        Task<string> CreateScheduleAsync(RMSchedule rmSchedule);

        Task UpdateScheduleAsync(RMSchedule rmSchedule);

        int BatchUpdateScheduleAsync(List<RMSchedule> rmSchedules);

        void DeleteSchedule(string rmScheduleId);
        Task MarkScheduleRemovedAsync(string rmScheduleId);

        RMSchedule GetSchedule(string rmScheduleId);

        RMSchedule GetSchedule(string profileId, ScheduleType type);
        RMSchedule GetAncestrySchedule(string profileId, ScheduleType type);

        List<RMSchedule> GetScheduleByType(ScheduleType type);
        List<RMSchedule> GetScheduleByTypeAndGroupId(string groupId, ScheduleType type);

        List<RMSchedule> GetScheduleByProfileId(string profileId);
        List<string> GetDisposalBreakNodes(string parentId);
        List<RMSchedule> GetRunableSchedule();
        List<RMSchedule> GetRunableSchedule(ScheduleType type);

        bool CheckIsContainScheduleForOwnAndChildNodes(string nodeId, string groupId);
        Task UpdateExtentionsByTypeAsync(ScheduleType type, string extension);
        void RemoveNodeInfo(ScheduleType type);
        void DeleteSchedules(ScheduleType type, string profileId);

        RMSchedule GetPhysicalScheduleByLocationId(Guid locationId);
        List<string> GetScheduleBreakNodes(string parentId);
        List<RMSchedule> GetRunableScheduleByTypes(List<ScheduleType> scheduleTypes);
    }
}
