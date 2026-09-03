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
using System.IO;
using System.Xml;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Backup
{
    public class AveSPProject
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private Guid mId;
        //private string mTaskListTitle;

        private AveSPSite mAveSPSite;
        private IAveProject mProject;
        private AveListDataCache mDataCache = new AveListDataCache();
        private AveProjectConfig mConfig = new AveProjectConfig(true);

        //SharePoint Task List Project Type Id: f4066fec-bd67-4db9-8e6f-9cb3d3b297a6
        //Enterprise Project Type Id: 09fa52b4-059b-4527-926e-99f9be96437a

        public AveSPProject(AveSPSite site, Guid id)
        {
            if (id == Guid.Empty) throw new ArgumentException("Guid is invalid", "id");

            this.mAveSPSite = site;
            this.mId = id;
            mProject = this.mAveSPSite.SPSite.ProjectServer.GetProjectById(this.mId); //this.mAveSPSite.SPSite.Projects.GetById(this.mId);
            if (mProject == null)
            {
                throw new ArgumentNullException("Project", "Cannot find the specified project");
            }
        }

        //public AveSPProject(AveSPSite site, string name)
        //{
        //    if (string.IsNullOrEmpty(name)) throw new ArgumentException("Name is invalid", "name");

        //    this.mAveSPSite = site;
        //    this.mName = name;
        //    mProject = this.mAveSPSite.SPSite.Projects.GetByName(this.mName);
        //    if (mProject == null)
        //    {
        //        throw new ArgumentNullException("Project", "Cannot find the specified project");
        //    }
        //}
        
        //public AveSPProject(AveSPSite site, IAveProject project, string taskListTitle)
        //{
        //    if (project == null)
        //    {
        //        throw new ArgumentNullException("Project", "Project cannot be null");
        //    }
        //    if (!project.EnterpriseProjectTypeId.Equals(ProjectSerializerTag.SHAREPOINTTASKLISTPROJECTTYPEID))
        //    {
        //        throw new ArgumentException("Project is not SharePoint Task List project.", "project");
        //    }
            
        //    this.mAveSPSite = site;
        //    this.mProject = project;
        //    this.mTaskListTitle = taskListTitle;
        //}

        public bool IsEnterpriseProject
        {
            get
            {
                return mProject.IsEnterpriseProject;
                //if (!this.mIsSharePointTaskListProject.HasValue)
                //{
                //    this.mIsSharePointTaskListProject = mProject.EnterpriseProjectTypeId.Equals(ProjectSerializerTag.SHAREPOINTTASKLISTPROJECTTYPEID) || !mProject.EnterpriseProjectType.IsManaged;
                //}
                //return this.mIsSharePointTaskListProject.Value;
            }
        }

        public AveProjectConfig BackupProjectConfig
        {
            get
            {
                return this.mConfig;
            }
            set
            {
                this.mConfig = value;
            }
        }

        public AveProjectInfo GetBasicInfo()
        {
            mLog.Info("Start to backup project [{0}] data", mProject.Name);
            var projInfo = mProject.ConvertToProjectInfo();
            if (!string.IsNullOrEmpty(projInfo.ProjectSiteUrl) && projInfo.TaskListId != Guid.Empty)
            {
                string psUrl = AveUrlUtility.GetSiteServerRelativeUrl(projInfo.ProjectSiteUrl);
                IAveWeb projSite = mAveSPSite.SPSite.OpenWeb(psUrl);
                if (projSite.Exists)
                {
                    AveWebCreationInformation webInfo = new AveWebCreationInformation();
                    webInfo.Url = psUrl;
                    webInfo.Title = projSite.Title;
                    webInfo.Language = (int)projSite.Language;
                    webInfo.WebTemplate = projSite.WebTemplateName;
                    projInfo.ProjectSiteInfo = webInfo;

                    IAveList taskList = projSite.GetList(projInfo.TaskListId);
                    projInfo.TaskListTitle = taskList.Title;
                }
            }
            CacheUserFromSettings(projInfo);
            return projInfo;
        }

        public void ExportBasic(IAveBackupStream output, AveProjectInfo projInfo)
        {
            output.WriteMetadata(AveMetadataType.ProjectBasic, projInfo);
        }

        public Stream GetTaskProp()
        {
            Stream result = new AveCoordinatedStream("ProjectDetails");
            var actions = GetPushActions();
            using (var writer = new AveProjectWriter(result))
            {
                foreach (var action in actions)
                {
                    try
                    {
                        action(writer);
                    }
                    catch (Exception ex)
                    {
                        //[Project]
                        //report status
                        mLog.Warn("An error occurred while backing up project details. Error:{0}", ex);
                    }
                }
            }

            if (result != null)
            {
                result.Position = 0;
            }

            return result;
        }

        public void ExportDetails(IAveBackupStream output, Stream content)
        {
            if (content != null)
            {
                try
                {
                    long readSize = 0;
                    output.FlushMetadata(content.Length);
                    byte[] buffer = new byte[1024 * 1024];
                    int length;
                    while (readSize < content.Length)
                    {
                        length = content.Read(buffer, 0, buffer.Length);
                        if (length == 0)
                        {
                            break;
                        }
                        readSize += length;
                        output.WriteContent(buffer, 0, length);
                    }
                }
                finally
                {
                    content.Dispose();
                }
            }
            else
            {
                output.FlushMetadata(0);
            }
        }

        public void CacheUserFromSettings(AveProjectInfo projInfo)
        {
            AddUserToCache(projInfo.CheckedOutById);
            AddUserToCache(projInfo.OwnerId);
        }

        private void AddUserToCache(int principalId)
        {
            if (principalId != 0 && !mDataCache.principalIdAlreadyExists(principalId))
            {
                object obj = mAveSPSite.DataCache.GetPrincipalInfo(principalId);

                if (obj is AveUserInfo)
                {
                    mDataCache.AddToCache(principalId, (AveUserInfo)obj);
                }
                else if (obj is AveGroupInfo)
                {
                    mDataCache.AddToCache(principalId, (AveGroupInfo)obj);
                }
            }
        }

        public void ExportUser(IAveBackupStream output)
        {
            output.WriteMetadata(AveMetadataType.UserCache, mDataCache.UserList);
        }

        //根据还原顺序决定备份顺序, 先备份的先还原
        private List<Action<AveProjectWriter>> GetPushActions()
        {
            var actions = new List<Action<AveProjectWriter>>();

            //actions.Add(PushProjectCalendar);
            //actions.Add(PushProjectLookupTable);
            //actions.Add(PushProjectCustomField);
            //actions.Add(PushProjectEnterpriseResource);
            //actions.Add(PushProjectPhase);
            //actions.Add(PushProjectStage);
            //actions.Add(PushProjectEnterpriseProjectType);

            actions.Add(PushPublishedTasks);
            actions.Add(PushDraftProject);
            actions.Add(PushDraftTasks);

            return actions;
        }
        private void PushPublishedTasks(AveProjectWriter writer)
        {
            List<AveProjectTaskInfo> tasksInfo = new List<AveProjectTaskInfo>();
            if (this.mConfig.IncludePublishedTasks)
            {
                mLog.Info("Start to backup published task data");
                foreach (var task in mProject.Tasks)
                {
                    AveProjectTaskInfo info = task.CovertToTaskInfo();
                    AddUserToCache(info.StatusManagerId);
                    tasksInfo.Add(info);
                }
            }
            writer.WritePublishedTasks(tasksInfo);
        }

        private void PushDraftProject(AveProjectWriter write)
        {
            AveProjectInfo draftProjectInfo = null;
            if (this.mConfig.IncludeDraftProjects)
            {
                mLog.Info("Start to backup draft project data");
                draftProjectInfo = mProject.Draft.ConvertToProjectInfo();
                AddUserToCache(draftProjectInfo.OwnerId);
                AddUserToCache(draftProjectInfo.CheckedOutById);
            }
            write.WriteDraftProject(draftProjectInfo);
        }

        private void PushDraftTasks(AveProjectWriter writer)
        {
            var tasksInfo = new List<AveProjectTaskInfo>();
            if (this.mConfig.IncludeDraftTasks)
            {
                mLog.Info("Start to backup draft task data");
                foreach (var task in mProject.Draft.Tasks)
                {
                    AveProjectTaskInfo info = task.CovertToTaskInfo();
                    AddUserToCache(info.StatusManagerId);
                    tasksInfo.Add(info);
                }
            }
            writer.WriteDraftTasks(tasksInfo);
        }

        #region pwa
        //private void PushProjectEnterpriseProjectType(AveProjectWriter writer, Stream stream)
        //{
        //    var typeInfos = new List<AveProjectEnterpriseProjectTypeInfo>();
        //    if (this.mConfig.IncludeEnterpriseProjectTypes)
        //    {
        //        mLog.Info("Start to backup enterprise project type data");
        //        foreach (var ept in this.mAveSPSite.SPSite.ProjectEnterpriseProjectTypes)
        //        {
        //            typeInfos.Add(ept.ConvertToEPTInfo());
        //        }
        //    }
        //    writer.WriteProjectEnterpriseTypes(typeInfos);
        //}

        //private void PushProjectCalendar(AveProjectWriter writer, Stream stream)
        //{
        //    var calendarInfos = new List<AveProjectCalendarInfo>();
        //    if (this.mConfig.IncludeCalendars)
        //    {
        //        mLog.Info("Start to backup project calendar data.");
        //        foreach (var cal in this.mAveSPSite.SPSite.ProjectCalendars)
        //        {
        //            calendarInfos.Add(cal.ConvertToCalendarInfo());
        //        }
        //    }
        //    writer.WriteProjectCalendars(calendarInfos);
        //}

        //private void PushProjectLookupTable(AveProjectWriter writer, Stream stream)
        //{
        //    var tableInfos = new List<AveProjectLookupTableInfo>();
        //    if (this.mConfig.IncludeLookupTables)
        //    {
        //        mLog.Info("Start to backup lookup table data");
        //        foreach (var table in this.mAveSPSite.SPSite.ProjectLookupTables)
        //        {
        //            tableInfos.Add(table.ConvertToLookupTableInfo());
        //        }
        //    }
        //    writer.WriteProjectLookupTables(tableInfos);
        //}

        //private void PushProjectEnterpriseResource(AveProjectWriter writer, Stream stream)
        //{
        //    var resourceInfos = new List<AveProjectEnterpriseResourceInfo>();
        //    if (this.mConfig.IncludeEnterpriseResources)
        //    {
        //        mLog.Info("Start to backup enterprise resource data");
        //        foreach (var resource in this.mAveSPSite.SPSite.ProjectEnterpriseResources)
        //        {
        //            resourceInfos.Add(resource.ConvertToEnterpriseResourceInfo());
        //        }
        //    }
        //    writer.WriteProjectEnterpriseResources(resourceInfos);
        //}

        //private void PushProjectCustomField(AveProjectWriter writer, Stream stream)
        //{
        //    var fieldInfos = new List<AveProjectCustomFieldInfo>();
        //    if (this.mConfig.IncludeCustomFields)
        //    {
        //        mLog.Info("Start to backup custom field data");
        //        foreach (var field in this.mAveSPSite.SPSite.ProjectCustomFields)
        //        {
        //            fieldInfos.Add(field.ConvertToCustomFieldInfo());
        //        }
        //    }
        //    writer.WriteProjectCustomFields(fieldInfos);
        //}

        //private void PushProjectPhase(AveProjectWriter writer, Stream stream)
        //{
        //    var phaseInfos = new List<AveProjectPhaseInfo>();
        //    if (this.mConfig.IncludePhases)
        //    {
        //        mLog.Info("Start to backup phase data");
        //        foreach (var phase in this.mAveSPSite.SPSite.ProjectPhases)
        //        {
        //            phaseInfos.Add(phase.ConvertToPhaseInfo());
        //        }
        //    }
        //    writer.WriteProjectPhases(phaseInfos);
        //}

        //private void PushProjectStage(AveProjectWriter writer, Stream stream)
        //{
        //    var stageInfos = new List<AveProjectStageInfo>();
        //    if (this.mConfig.IncludeStages)
        //    {
        //        mLog.Info("Start to backup stage data");
        //        foreach (var stage in this.mAveSPSite.SPSite.ProjectStages)
        //        {
        //            stageInfos.Add(stage.ConvertToStageInfo());
        //        }
        //    }
        //    writer.WriteProjectStages(stageInfos);
        //}       

        //private void PushProjectTimeSheet(AveProjectWriter writer, Stream stream)
        //{

        //}

        #endregion
    }
}
