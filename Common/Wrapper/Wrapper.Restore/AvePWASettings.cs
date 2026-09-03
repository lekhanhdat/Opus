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
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Newtonsoft.Json;

namespace AvePoint.Wrapper.Restore
{
    public class AvePWASettings
    {
        private static AveLogger mLog = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private AveSPSite mParentSite = null;
        private AveServerTimelineInfo mServerTimeLineInfo;

        private List<AveProjectStageInfo> mStageInfos = new List<AveProjectStageInfo>();
        private List<AveProjectEnterpriseProjectTypeInfo> mEptInfos = new List<AveProjectEnterpriseProjectTypeInfo>();
        private List<Guid> mRequiredCustomFieldList = new List<Guid>();

        public AvePWASettings(AveSPSite site)
        {
            mParentSite = site;
        }

        #region cache

        public void CacheStageInfo(List<AveProjectStageInfo> stageInfos)
        {
            mStageInfos = stageInfos;
        }

        public void CacheEnterpriseProjectType(List<AveProjectEnterpriseProjectTypeInfo> eptInfos)
        {
            mEptInfos = eptInfos;
        }

        public void CacheTimeline(string tlViewData)
        {
            int index = tlViewData.IndexOf('*');
            if (index > 0)
            {
                // Timeline Name Placeholder**!**
                tlViewData = tlViewData.Substring(index + 5);
                List<Dictionary<string, object>> jsonObj = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(tlViewData);
                if (jsonObj.Count > 1)
                {
                    mServerTimeLineInfo = new AveServerTimelineInfo();
                    string xml = jsonObj[0]["Formatting"].ToString();
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(xml);
                    List<AveProjectTimelineInfo> timelineInfos = new List<AveProjectTimelineInfo>();
                    for (int i = 1; i < jsonObj.Count; i++)
                    {
                        Dictionary<string, object> taskData = jsonObj[i];
                        AveProjectTimelineInfo timelineInfo = new AveProjectTimelineInfo();
                        timelineInfo.ProjUID = new Guid(taskData["PROJ_UID"].ToString());
                        timelineInfo.TaskCheckUID = new Guid(taskData["TASK_CHECK_UID"].ToString());
                        timelineInfo.ProjName = taskData["PROJ_NAME"].ToString();
                        XmlNode node = doc.DocumentElement.SelectSingleNode("./mlSet/m[@id='" + timelineInfo.ProjUID + "']");
                        if (node != null)
                        {
                            timelineInfo.onTL = node.Attributes["onTL"].Value;
                            timelineInfo.barid = node.Attributes["barid"].Value;
                        }
                        else
                        {
                            node = doc.DocumentElement.SelectSingleNode("./tskSet/t[@id='" + timelineInfo.ProjUID + "']");
                            if (node != null)
                            {
                                timelineInfo.onTL = node.Attributes["onTL"].Value;
                                timelineInfo.barid = node.Attributes["barid"].Value;
                            }
                        }
                        timelineInfos.Add(timelineInfo);
                    }
                    mServerTimeLineInfo.Timelines = timelineInfos;
                    XmlNode mlSet = doc.DocumentElement.SelectSingleNode("./mlSet");
                    if (mlSet.ChildNodes.Count > 1)
                    {
                        while (mlSet.ChildNodes.Count != 1)
                        {
                            mlSet.RemoveChild(mlSet.ChildNodes[1]);
                        }

                    }
                    XmlNode taskSet = doc.DocumentElement.SelectSingleNode("./tskSet");
                    if (taskSet.ChildNodes.Count > 1)
                    {
                        while (taskSet.ChildNodes.Count != 1)
                        {
                            taskSet.RemoveChild(taskSet.ChildNodes[1]);
                        }
                    }
                    mServerTimeLineInfo.BaseViewData = doc.OuterXml;
                }
            }
        }

        #endregion

        #region restore

        public void RestoreLookupTable(List<AveProjectLookupTableInfo> lookupTableInfos)
        {
            foreach (AveProjectLookupTableInfo info in lookupTableInfos)
            {
                try
                {
                    IAveProjectLookupTable table = Find(info);
                    if (table == null)
                    {
                        mLog.Info("add lookup table, table name:{0}, table id:{1}", info.Name, info.Id);
                        table = mParentSite.SPSite.ProjectLookupTables.Add(info);
                    }
                    else
                    {
                        mLog.Warn("The lookup table already exists. table name:{0}, table id:{1}", info.Name, info.Id);
                        //the rest properties are all read-only
                        table.Name = info.Name;
                        table.SortOrder = info.SortOrder;
                        table.Update();
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("restore lookup table failed. lookup table name:{0}, error:{1}", info.Name, e);
                }
            }
            lookupTableInfos.Clear();
        }

        public void RestoreCustomFields(List<AveProjectCustomFieldInfo> customFieldInfos)
        {
            foreach (AveProjectCustomFieldInfo info in customFieldInfos)
            {
                try
                {
                    IAveProjectCustomField field = Find(info);
                    if (field == null)
                    {
                        mLog.Info("add custom field, field name:{0}, internl name:{1}, field id:{2}", info.Name, info.InternalName, info.Id);
                        field = mParentSite.SPSite.ProjectCustomFields.Add(info);
                    }
                    else
                    {
                        mLog.Warn("The custom field already exists. field name:{0}, internl name:{1}, field id:{2}", info.Name, info.InternalName, info.Id);
                        //update properties
                        //the rest properties are all read-only
                        field.Name = info.Name;
                        field.Description = info.Description;
                        field.Formula = info.Formula;
                        field.IsEditableInVisibility = info.IsEditableInVisibility;
                        field.IsMultilineText = info.IsMultilineText;
                        field.IsRequired = info.IsRequired;
                        field.IsWorkflowControlled = info.IsWorkflowControlled;
                        field.LookupDefaultValue = info.LookupDefaultValue;
                        field.RollsDownToAssignments = info.RollsDownToAssignments;
                        field.Update();
                    }
                    mParentSite.MappingManager.ProjectMappingManager.AddCustomFieldNameMapping(info.InternalName, field.InternalName);
                    mParentSite.MappingManager.ProjectMappingManager.AddCustomFieldIdMapping(info.Id, field.Id);
                    if (field.IsRequired)
                    {
                        field.IsRequired = false;
                        field.Update();
                        mRequiredCustomFieldList.Add(field.Id);
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("restore custom field failed. field name:{0}, internl name:{1}, error:{2}", info.Name, info.InternalName, e);
                }
            }
            customFieldInfos.Clear();
        }

        public void RevertRequiredCustomField()
        {
            foreach (Guid id in mRequiredCustomFieldList)
            {
                try
                {
                    IAveProjectCustomField field = mParentSite.SPSite.ProjectCustomFields.GetByGuid(id);
                    field.IsRequired = true;
                    field.Update();
                }
                catch (Exception e)
                {
                    mLog.Warn("revert required custom field falied. error message:{0}", e.ToString());
                }
            }
            mRequiredCustomFieldList.Clear();
        }

        public void RestoreEnterpriseResource(List<AveProjectEnterpriseResourceInfo> resourceInfos)
        {
            foreach (AveProjectEnterpriseResourceInfo info in resourceInfos)
            {
                try
                {
                    if (info.ResourceType == 0)
                    {
                        continue; //NotSpecified的resource在界面不显示，不能还原，如果做添加操作会被还原成work类型的
                    }

                    IAveProjectEnterpriseResource resource = Find(info);
                    if (info.DefaultAssignmentOwnerId != 0)
                    {
                        info.DefaultAssignmentOwnerId = mParentSite.SPMembers.FindMemberId(info.DefaultAssignmentOwnerId);
                    }
                    if (info.TimesheetManagerId != 0)
                    {
                        info.TimesheetManagerId = mParentSite.SPMembers.FindMemberId(info.TimesheetManagerId);
                    }
                    if (info.UserId != 0)
                    {
                        info.UserId = mParentSite.SPMembers.FindMemberId(info.UserId);
                    }
                    if (info.FieldValues != null && info.FieldValues.Count > 0)
                    {
                        Dictionary<string, object> tempFieldValues = new Dictionary<string, object>();
                        foreach (KeyValuePair<string, object> pair in info.FieldValues)
                        {
                            string mappingKey = mParentSite.MappingManager.ProjectMappingManager.GetCustomFieldNameMapping(pair.Key);
                            tempFieldValues[mappingKey] = pair.Value;
                        }
                        info.FieldValues = tempFieldValues;
                    }

                    if (resource == null || resource.ResourceType == 0)
                    {
                        mLog.Info("add resource, name:{0}, id:{1}", info.Name, info.Id);
                        resource = mParentSite.SPSite.ProjectEnterpriseResources.Add(info);
                    }
                    else
                    {
                        mLog.Warn("resource already exist, name:{0}, id:{1}", info.Name, info.Id);
                        resource.Name = info.Name;
                        resource.CanLevel = info.CanLevel;
                        resource.Code = info.Code;
                        resource.CostCenter = info.CostCenter;
                        resource.DefaultBookingType = info.DefaultBookingType;
                        resource.Email = info.Email;
                        resource.ExternalId = info.ExternalId;
                        resource.Group = info.Group;
                        resource.Initials = info.Initials;
                        resource.IsActive = info.IsActive;
                        resource.MaterialLabel = info.MaterialLabel;
                        resource.Phonetics = info.Phonetics;
                        resource.RequiresEngagements = info.RequiresEngagements;

                        #region TODO
                        //datetime 类型的属性去更新时，有异常。
                        //resource.HireDate = info.HireDate;
                        //resource.TerminationDate = info.TerminationDate;
                        //read-only
                        //resource.BaseCalendar = (IAveProjectCalendar)info.BaseCalendar;
                        //string——>user
                        //resource.DefaultAssignmentOwner = info.DefaultAssignmentOwner;
                        //resource.TimesheetManager = info.TimesheetManager;
                        //resource.User = info.User;
                        #endregion

                        resource.Update();
                    }
                }
                catch(Exception e)
                {
                    mLog.Warn("restore EnterpriseResource failed. resource name:{0}, error:{2}", info.Name, e);
                }
            }
            resourceInfos.Clear();
        }

        public void RestorePhase(List<AveProjectPhaseInfo> phaseInfos)
        {
            foreach (var info in phaseInfos)
            {
                try
                {
                    IAveProjectPhase phase = Find(info);
                    if (phase == null)
                    {
                        mLog.Info("add phase, name:{0}, id:{1}", info.Name, info.Id);
                        phase = mParentSite.SPSite.ProjectPhases.Add(info);
                    }
                    else
                    {
                        mLog.Warn("phase already exist, name:{0}, id:{1}", info.Name, info.Id);
                        phase.Name = info.Name;
                        phase.Description = info.Description;
                        phase.Update();
                    }
                }
                catch (Exception e)
                {
                    mLog.Warn("restore phase failed. phase name:{0}, error:{1}", info.Name, e);
                }
            }
            phaseInfos.Clear();
        }

        public void RestoreStage()
        {
            if (mStageInfos.Count > 0)
            {
                foreach (AveProjectStageInfo info in mStageInfos)
                {
                    try
                    {
                        IAveProjectStage stage = Find(info);

                        foreach (AveProjectStageCustomFieldInfo fInfo in info.CustomFields)
                        {
                            fInfo.Id = mParentSite.MappingManager.ProjectMappingManager.GetCustomFieldIdMapping(fInfo.Id);
                        }

                        if (stage == null)
                        {
                            mLog.Info("add stage, name:{0}, id:{1}", info.Name, info.Id);
                            stage = mParentSite.SPSite.ProjectStages.Add(info);
                        }
                        else
                        {
                            mLog.Warn("stage alreaddy exist, name:{0}, id:{1}", info.Name, info.Id);
                            stage.Name = info.Name;
                            stage.CheckInRequired = info.CheckInRequired;
                            stage.Description = info.Description;
                            stage.SubmitDescription = info.SubmitDescription;
                            #region TODO
                            //read-only
                            //stage.Phase = sInfo.Phase;
                            //stage.Id = sInfo.Id;
                            //stage.Behavior = sInfo.Behavior;
                            //stage.CustomFields = sInfo.CustomFields;

                            //特殊类型的properties，需要考虑如何支持
                            //stage.WorkflowStatusPage = sInfo.WorkflowStatusPage;
                            //stage.ProjectDetailPages = sInfo.ProjectDetailPages;
                            #endregion
                            stage.Update();
                        }
                        mParentSite.MappingManager.ProjectMappingManager.AddStageIdMapping(info.Id, stage.Id);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("restore stage failed. stage name:{0}, error:{1}", info.Name, e);
                    }
                }
                mStageInfos.Clear();
            }
        }

        public void RestoreEnterpriseProjectType()
        {
            if (mEptInfos.Count > 0)
            {
                foreach (AveProjectEnterpriseProjectTypeInfo info in mEptInfos)
                {
                    try
                    {
                        if (info.WorkflowAssociationId != Guid.Empty)
                        {
                            info.WorkflowAssociationId = mParentSite.MappingManager.ProjectMappingManager.GetWorkflowSubscriptionIdMapping(info.WorkflowAssociationId);
                        }
                        AveSiteMappingManager siteMappingManager = WrapperRuntime.CurrentContext.MappingManager.SiteMappingManager;
                        ReplaceOption replaceOption = new ReplaceOption(true, true);
                        info.SiteCreationURL = mParentSite.SPSite.Url;//AveReplaceProcessor.UrlReplace(info.SiteCreationURL, siteMappingManager.SiteManagedMappings, replaceOption, siteMappingManager.SourceSiteInfo, siteMappingManager.DestSiteInfo.ServerRelativeUrl);
                        
                        for (int i=0; i< info.ProjectDetailPages.Count; i++ )
                        {
                            AveProjectDetailPageInfo pdpInfo = info.ProjectDetailPages[i];
                            IAveProjectDetailPage page = mParentSite.SPSite.ProjectServer.ProjectDetailPages.GetByName(pdpInfo.Name);
                            if (page != null)
                            {
                                pdpInfo.Id = page.Id;
                            }
                            else
                            {
                                mLog.Warn("detail page doesn't exist, page name:{0}, page id:{1}, isNewCreate:{2}", pdpInfo.Name, pdpInfo.Id, pdpInfo.IsCreatePDP);
                                info.ProjectDetailPages.Remove(pdpInfo);
                            }
                        }

                        IAveProjectEnterpriseProjectType ept = Find(info);
                        if (ept == null)
                        {
                            mLog.Info("add enterpriseType, name:{0}, id:{1}", info.Name, info.Id);
                            ept = mParentSite.SPSite.ProjectEnterpriseProjectTypes.Add(info);
                        }
                        else
                        {
                            mLog.Warn("enterpriseType alread exists in destination, name:{0}, id:{1}", info.Name, info.Id);
                            ept.Name = info.Name;
                            ept.Description = info.Description;
                            ept.ImageUrl = info.ImageUrl;
                            ept.IsDefault = info.IsDefault;
                            ept.IsManaged = info.IsManaged;
                            ept.Order = info.Order;
                            ept.WorkspaceTemplateLCID = info.WorkspaceTemplateLCID;
                            ept.WorkspaceTemplateName = info.WorkspaceTemplateName;
                            ept.TaskListSyncEnable = info.TaskListSyncEnable;
                            ept.PermissionSyncEnable = info.PermissionSyncEnable;
                            ept.SiteCreationOption = (AveEnterpriseProjectTypeSiteCreationOptions)info.SiteCreationOption;
                            ept.SiteCreationURL = info.SiteCreationURL;
                            #region TODO
                            //throw : "EnterpriseProjectTypeInvalidProjectPlanTemplateUid"
                            //ept.ProjectPlanTemplateId = eptInfo.ProjectPlanTemplateId;
                            //throw : "invalid assciation uid" 
                            //ept.WorkflowAssociationId = eptInfo.WorkflowAssociationId;
                            //ept.WorkflowAssociationName = info.WorkflowAssociationName;
                            #endregion
                            //ept.UpdateEnterpriseTypeByPSI(info);
                            ept.Update();
                        }
                        mParentSite.MappingManager.ProjectMappingManager.AddEnterpriseTypeIdMapping(info.Id, ept.Id);
                    }
                    catch (Exception e)
                    {
                        mLog.Warn("restore ept failed, ept name:{0}, error message:{1}", info.Name, e);
                    }
                }
                mEptInfos.Clear();
            }
        }

        public void RestoreTimeline()
        {
            if (mServerTimeLineInfo != null)
            {
                mLog.Info("restore project timeline");
                string jsonData = mParentSite.SPSite.ProjectServer.ReadServerTimeLine();
                int index = jsonData.IndexOf('*');
                if (index > 0)
                {
                    // Timeline Name Placeholder**!**
                    jsonData = jsonData.Substring(index + 5);
                    List<Dictionary<string, object>> jsonObj = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(jsonData);
                    string tlViewData = string.Empty;
                    if (jsonObj.Count > 1)  //如果目的端有，就用目的端的timeline加上还原过的project timeline
                    {
                        tlViewData = jsonObj[0]["Formatting"].ToString();
                    }
                    else //如果目的端没有，就用源端的base timeline加上还原过的project timeline
                    {
                        tlViewData = mServerTimeLineInfo.BaseViewData;
                    }
                    XmlDocument doc = new XmlDocument();
                    doc.LoadXml(tlViewData);
                    XmlNode mlSet = doc.DocumentElement.SelectSingleNode("./mlSet");
                    XmlNode taskSet = doc.DocumentElement.SelectSingleNode("./tskSet");
                    mParentSite.SPSite.ProjectServer.CleanCache();
                    foreach (AveProjectTimelineInfo timelineInfo in mServerTimeLineInfo.Timelines )
                    {
                        Guid projId = mParentSite.MappingManager.ProjectMappingManager.GetProjectTaskIdMapping(timelineInfo.TaskCheckUID);
                        if (projId != Guid.Empty) //说明当前job成功还原了该project
                        {
                            IAveProject project = mParentSite.SPSite.Projects.GetById(projId);
                            if (project != null)
                            {
                                Guid taskId = mParentSite.MappingManager.ProjectMappingManager.GetProjectTaskIdMapping(timelineInfo.ProjUID);
                                if (taskId == Guid.Empty) //当前timeline关联的是task而不是project自己
                                {
                                    if (project.IsEnterpriseProject) //enterprise type的task会跟随project一起还原，是keep id的
                                    {
                                        taskId = timelineInfo.ProjUID;
                                    }
                                    else //sharepoint task project的task是在project site task list还原task item，而且item id和task id不一致，无法匹配，所以只能通过name找，如果name有同名的可能出现问题
                                    {
                                        IAveProjectTask task = project.Tasks.GetByName(timelineInfo.ProjName);
                                        taskId = task.Id;
                                    }
                                }

                                XmlNode node = doc.DocumentElement.SelectSingleNode("./mlSet/m[@id='" + taskId + "']");
                                if (node != null)
                                {
                                    node.Attributes["onTL"].Value = timelineInfo.onTL;
                                    node.Attributes["barid"].Value = timelineInfo.barid;
                                }
                                else
                                {
                                    XmlElement ml = doc.CreateElement("m");
                                    ml.Attributes["y"].Value = "35";
                                    ml.Attributes["x"].Value = "0";
                                    ml.Attributes["fmt"].Value = "2";
                                    ml.Attributes["onTL"].Value = timelineInfo.onTL;
                                    ml.Attributes["id"].Value = taskId.ToString();
                                    ml.Attributes["barid"].Value = timelineInfo.barid;
                                    mlSet.AppendChild(ml);
                                }

                                node = doc.DocumentElement.SelectSingleNode("./tskSet/t[@id='" + taskId + "']");
                                if (node != null)
                                {
                                    node.Attributes["onTL"].Value = timelineInfo.onTL;
                                    node.Attributes["barid"].Value = timelineInfo.barid;
                                }
                                else
                                {
                                    XmlElement tsk = doc.CreateElement("t");
                                    tsk.Attributes["fmt"].Value = "0";                                    
                                    tsk.Attributes["onTL"].Value = timelineInfo.onTL;
                                    tsk.Attributes["id"].Value = taskId.ToString();
                                    tsk.Attributes["barid"].Value = timelineInfo.barid;
                                    tsk.Attributes["ch"].Value = "4294967295";
                                    taskSet.AppendChild(tsk);
                                }
                            }
                        }
                    }
                    mParentSite.SPSite.ProjectServer.UpdateTimeLine(doc.OuterXml);
                }
                mLog.Info("restore project timeline");
            }
        }

        #endregion

        #region Find

        private IAveProjectLookupTable Find(AveProjectLookupTableInfo info)
        {
            IAveProjectLookupTable table = mParentSite.SPSite.ProjectLookupTables.GetByName(info.Name);
            if (table == null)
            {
                table = mParentSite.SPSite.ProjectLookupTables.GetByGuid(info.Id);
            }
            return table;
        }

        private IAveProjectCustomField Find(AveProjectCustomFieldInfo info)
        {
            IAveProjectCustomField field = mParentSite.SPSite.ProjectCustomFields.GetByName(info.Name);
            if (field == null)
            {
                field = mParentSite.SPSite.ProjectCustomFields.GetByGuid(info.Id);
            }
            return field;
        }

        private IAveProjectPhase Find(AveProjectPhaseInfo info)
        {
            IAveProjectPhase phase = mParentSite.SPSite.ProjectPhases.GetByName(info.Name);
            if (phase == null)
            {
                phase = mParentSite.SPSite.ProjectPhases.GetByGuid(info.Id);
            }
            return phase; 
        }

        private IAveProjectStage Find(AveProjectStageInfo info)
        {
            IAveProjectStage stage = mParentSite.SPSite.ProjectStages.GetByName(info.Name);
            if (stage == null)
            {
                stage = mParentSite.SPSite.ProjectStages.GetByGuid(info.Id);
            }
            return stage;
        }

        private IAveProjectEnterpriseResource Find(AveProjectEnterpriseResourceInfo info)
        {
            IAveProjectEnterpriseResource resource = mParentSite.SPSite.ProjectEnterpriseResources.GetByName(info.Name);
            if (resource == null)
            {
                resource = mParentSite.SPSite.ProjectEnterpriseResources.GetByGuid(info.Id);
            }
            return resource;
        }

        private IAveProjectEnterpriseProjectType Find(AveProjectEnterpriseProjectTypeInfo info)
        {
            IAveProjectEnterpriseProjectType ept = mParentSite.SPSite.ProjectEnterpriseProjectTypes.GetByName(info.Name);
            if (ept == null)
            {
                ept = mParentSite.SPSite.ProjectEnterpriseProjectTypes.GetByGuid(info.Id);
            }
            return ept;
        }

        #endregion

    }
}
