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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.GCommon.Utility;

namespace AvePoint.Wrapper.Restore
{
    public class AveSPProject: RestoreableObject<AveSPProject>
    {
        private static AveLogger mLog = AveLogger.GetInstance(typeof(AveSPProject));
        private AveSPSite mSite;
        private Guid mId;
        private string mName;
        private AveProjectConfig mConfig = new AveProjectConfig(true);

        public AveSPProject(AveSPSite site, string name)
        {
            this.mSite = site;
            this.mName = name;
            DisableDraftData();
        }

        public AveSPProject(AveSPSite site, Guid id)
        {
            this.mSite = site;
            this.mId = id;
            DisableDraftData();
        }

        private void DisableDraftData()
        {
            this.mConfig.IncludeDraftProjects = false;
            this.mConfig.IncludeDraftTasks = false;
        }

        public AveProjectConfig RestoreProjectConfig
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

        public void Import(IAveRestoreStream stream, AveProjectInfo projInfo, AveRestoreOption restoreOption)
        {
            HandleFieldValues(projInfo);
            CreateProjectData(projInfo);
            using (var projectReader = new AveProjectReader(stream))
            {
                projectReader.FindEvent += new AveProjectReader.FindMember(FindMember);
                this.mSite.SPSite.Projects.ProjectSerializer.SetObjectData(projInfo, projectReader, this.mConfig, restoreOption.mAveRestoreMode);
            }
        }

        private int FindMember(int originalId)
        {
            return mSite.SPMembers.FindMemberId(originalId);
        }

        private void HandleFieldValues(AveProjectInfo projInfo)
        {
            if (projInfo.CheckedOutById != 0)
            {
                projInfo.CheckedOutById = mSite.SPMembers.FindMemberId(projInfo.CheckedOutById);
            }
            projInfo.OwnerId = mSite.SPMembers.FindMemberId(projInfo.OwnerId);
            //mapping custom field
            Dictionary<string, object> tempFieldValues = new Dictionary<string, object>();
            foreach (KeyValuePair<string, object> pair in projInfo.FieldValues)
            {
                //更新fomular类型的fields，会抛CustomFieldCannotSetValueOnFormulaFields异常
                string mappingKey = mSite.MappingManager.ProjectMappingManager.GetCustomFieldNameMapping(pair.Key);
                if (!string.IsNullOrEmpty(mappingKey))
                {
                    Guid fieldId = new Guid(mappingKey.Split(new char[] { '_' })[1]);
                    IAveProjectCustomField customField = mSite.SPSite.ProjectCustomFields.GetByGuid(fieldId);
                    if (customField != null && !string.IsNullOrEmpty(customField.Formula))
                    {
                        continue;
                    }
                    tempFieldValues[mappingKey] = pair.Value;
                }
                else
                {
                    mLog.Warn("get mapping custom field failed. internalName:{0}", pair.Key);
                }
            }
            projInfo.FieldValues = tempFieldValues;
            projInfo.EnterpriseProjectTypeId = mSite.MappingManager.ProjectMappingManager.GetEnterpriseTypeIdMapping(projInfo.EnterpriseProjectTypeId);
            //replace project site url
            if (projInfo.ProjectSiteInfo != null)
            {
                AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                ReplaceOption replaceOption = new ReplaceOption(true, true);
                projInfo.ProjectSiteInfo.Url = AveReplaceProcessor.UrlReplace(projInfo.ProjectSiteInfo.Url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                projInfo.ProjectSiteUrl = AveReplaceProcessor.UrlReplace(projInfo.ProjectSiteInfo.Url, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
            }
        }

        private void CreateProjectData(AveProjectInfo projInfo)
        {
            if (projInfo.ProjectSiteInfo != null)
            {
                IAveWeb projSite = mSite.SPSite.OpenWeb(projInfo.ProjectSiteInfo.Url);
                if (!projSite.Exists)
                {
                    projSite = mSite.SPSite.AddWeb(projInfo.ProjectSiteInfo.Url, projInfo.ProjectSiteInfo.Title, string.Empty, (uint)projInfo.ProjectSiteInfo.Language, projInfo.ProjectSiteInfo.WebTemplate, false, false);
                }
                if (!string.IsNullOrEmpty(projInfo.TaskListTitle))
                {
                    AveTuple<Guid, string> taskInfo = CreateTaskList(projSite, projInfo.TaskListTitle, 1);
                    projInfo.TaskListId = taskInfo.ItemA;
                    projInfo.TaskListTitle = taskInfo.ItemB;
                }
            }
        }

        private AveTuple<Guid, string> CreateTaskList(IAveWeb projSite, string taskListTitle, int index)
        {
            IAveList task = projSite.Lists.GetByTitle(taskListTitle);
            if (task != null)
            {
                if (task.BaseTemplate != AveListTemplateType.TasksWithTimelineAndHierarchy)
                {
                    taskListTitle = taskListTitle + index.ToString();
                    return CreateTaskList(projSite, taskListTitle, ++index);
                }
                return new AveTuple<Guid, string>(task.ID, taskListTitle);
            }
            else
            {
                Guid listId = projSite.Lists.Add(taskListTitle, string.Empty, AveListTemplateType.TasksWithTimelineAndHierarchy);
                return new AveTuple<Guid, string>(listId, taskListTitle);
            }
        }

        //public void ImportTaskListProject(AveProjectInfo projectInfo, IAveRestoreStream stream, Guid taskListId, AveRestoreOption restoreOption)
        //{
        //    if (taskListId == Guid.Empty)
        //    {
        //        throw new ArgumentNullException("List Id cannot be Empty.", "List Id");
        //    }
        //    if (projectInfo.EnterpriseProjectTypeId != ProjectSerializerTag.SHAREPOINTTASKLISTPROJECTTYPEID)
        //    {
        //        throw new ArgumentException("It is not a SharePoint Task List Project.", "AveProjectInfo");
        //    }

        //    if (restoreOption.mAveRestoreMode == AveRestoreMode.Default)
        //    {
        //        //if (this.mSite.SPSite.Projects.GetByName(projectInfo.Name) != null)
        //        //{
        //        //    //[Project]
        //        //    return;
        //        //}
        //    }

        //    mLog.Info("Replace Task List Project TaskListId Property. Old value:{0}, new value:{1}.", projectInfo.TaskListId.ToString(), taskListId.ToString());
        //    projectInfo.TaskListId = taskListId;            

        //    using (var detailReader = new AveProjectReader(stream))
        //    {
        //        this.mSite.SPSite.Projects.ProjectSerializer.SetObjectData(projectInfo, detailReader, this.mConfig, restoreOption.mAveRestoreMode);
        //    }
        //}
    }
}
