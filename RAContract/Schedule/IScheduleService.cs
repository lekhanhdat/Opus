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
using AvePoint.GCommon.Contract.ReportCenter.Object;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.SourceTreeQuery.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Schedule
{
    public interface IScheduleService
    {
        Task<string> CreateScheduleServiceAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "");
        Task<string> CreateScheduleServiceForFSAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "");
        System.Threading.Tasks.Task UpsertScheduleServiceAsyncForGoogleOne(List<ScheduleInfo> scheduleInfo, bool checkStartTime = true, string nodeFullPath = "");

        Task<string> CreateScheduleServiceForGoogleAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "");
        Task<string> CopyCreateScheduleServiceAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "");
        Task<string> UpdateScheduleServiceAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "");
        Task<string> UpdateScheduleServiceForFSAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "");
        Task<List<string>> UpdateScheduleServiceAsyncForGoogleOne(List<ScheduleInfo> scheduleInfo, string nodeFullPath = "");


        int UpdateTeamsScheduleService(List<ScheduleInfo> scheduleInfoes);
        Task<string> UpdateScheduleServiceForGoogleAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "");

        void DeleteScheduleService(string scheduleId, string nodeFullPath = "");
        void DeleteScheduleServiceForFS(string scheduleId, string nodeFullPath = "");
        void DeleteScheduleServiceForGoogle(string scheduleId, string nodeFullPath = "");

        ScheduleInfo GetScheduleService(string scheduleId);

        Task<List<ScheduleInfo>> GetScheduleByTypeServiceAsync(ScheduleType type);
        Task<List<ScheduleInfo>> GetScheduleByTypeServiceAsyncForGoogleOne();
        List<ScheduleInfo> GetScheduleByTypeAndGroupIdService(string groupId , ScheduleType type);

        Task<List<ScheduleInfo>> GetRunableScheduleAsync();
        Task<List<ScheduleInfo>> GetRunableScheduleByTypeAsync(List<ScheduleType> types);
        Task<ScheduleInfo> GetScheduleByIdAsync(string id);
        Task<ScheduleInfo> GetScheduleAsync(string profileId, ScheduleType type);
        Task<ScheduleInfo> GetAncestryScheduleAsync(string profileId, ScheduleType type);
        Task<ScheduleInfo> GetScheduleByProfileIdAsync(string profileId);
        System.Threading.Tasks.Task UpdateExtentionsByTypeAsync(ScheduleType type, string extension);
        bool CheckIsContainScheduleForOwnAndChildNodes(string nodeId, string groupId);
        bool NeedRunSchedule();
        bool DeleteScheduleByType(ScheduleType type);
        System.Threading.Tasks.Task CreateCustomScheduleAsync(bool isOperationGeneralSetting, ScheduleType scheduleType);

        System.Threading.Tasks.Task UpdateDashboardNextRunTimeAsync();

        System.Threading.Tasks.Task UpdateManualApprovalEmailScheduleNextRunTimeAsync();

        void RemoveScheduleNodeInfo(ScheduleType type);
        void CreateNoSchedule(SettingScheduleType type, string nodeFullPath = "");
        void CreateNoScheduleForFS(SettingScheduleType type, string nodeFullPath = "");

        string GetProfileId(RMSPTreeNode tree);
        string GetProfileId(RMSPSampleTreeNode tree);
        string GetProfileId(RMEXOTreeNode node);
        string GetProfileId(RMSampleEXOTreeNode node);
        string GetProfileId(RMFSTreeNode tree);
        string GetProfileId(BoxTreeNode tree);
        string GetProfileId(RMSampleGoogleTreeNode tree);
        string GetProfileId(RMGoogleTreeNode tree);
        string GetProfileId(List<Guid> ids);
        

        void DeleteSchedules(ScheduleType type, string profileIdPath);
        string GetProfileId(AzureFileShareTreeNode node);

        #region Control plus
        void ConvertScheduleByTimezone(ScheduleInfo scheduleInfo, bool isNeedCompare = true, string timeFormat = null);
        #endregion
        System.Threading.Tasks.Task CreateScheduleNotificationAsync(ScheduleType scheduleType);


        System.Threading.Tasks.Task<string> CreateScheduleWithoutAuditAsync(ScheduleInfo scheduleInfo, bool checkStartTime = true, string nodeFullPath = "");
        System.Threading.Tasks.Task<string> UpdateScheduleWithoutAuditAsync(ScheduleInfo scheduleInfo, string nodeFullPath = "");
        void DeleteScheduleWithoutAudit(string scheduleId, string nodeFullPath = "");
    }
}
