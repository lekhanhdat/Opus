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
using System.Net;
using System.IO;

using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;
using Microsoft365.SharePoint.CSOM.Extension;
using AvePoint.Wrapper.Common;
namespace AvePoint.ObjectModel.ClientOM
{
    public partial class AveClientOM2013Request
    {
        #region common

        public bool TestProjectLicense()
        {
            using (var context = CreateProjectContext())
            {
                context.Load(context.Projects, ps => ps.Include(p => p.Id));
                context.ExecuteQuery();
            }
            return true;
        }

        #endregion

        #region PSI

        public string ReadServerTimeLine()
        {
            return mPSIRequest.ReadServerTimeLine();
        }
        public void UpdateTimeLineByPSI(string tlViewData)
        {
            mPSIRequest.UpdateTimeLine(tlViewData);
        }

        public List<AveProjectDetailPageInfo> GetDetailPages(Guid eptId)
        {
            return mPSIRequest.ReadEnterpriseTypePDPs(eptId);
        }

        public void UpdateEnterpriseTypeByPSI(Guid projId, AveProjectEnterpriseProjectTypeInfo eptInfo)
        {
            mPSIRequest.UpdateEnterpriseTypeByPSI(projId, eptInfo);
        }

        #endregion

        #region get

        public List<Dictionary<string, object>> QueryProjects(bool includeDetails)
        {
            using (var context = CreateProjectContext())
            {
                if (includeDetails)
                {
                    //context.Load(context.Projects, ps => ps.Include(p => p.HasMppPendingImport, p => p.IsEnterpriseProject, p => p.EnterpriseProjectType.Id, p => p.ProjectSiteUrl, p => p.TaskListId));
                    //context.Load(context.Projects, ps => ps.Include(p => p.CheckedOutBy.Id, p => p.Owner.Id));
                    //context.Load(context.Projects);
                    context.Load(context.Projects, ps => ps.IncludeWithDefaultProperties(
                    p => p.HasMppPendingImport,
                    p => p.IsEnterpriseProject,
                    p => p.ProjectSiteUrl,
                    p => p.TaskListId,
                    p => p.EnterpriseProjectType.Id,
                    p => p.CheckedOutBy.Id,
                    p => p.Owner.Id
                    ));
                }
                else
                {
                    context.Load(context.Projects, ps => ps.Include(p => p.CreatedDate, p => p.Name, p => p.Id, p => p.IsEnterpriseProject, p => p.LastPublishedDate, p => p.LastSavedDate, p => p.TaskListId, p => p.ProjectSiteUrl));
                }
                context.ExecuteQuery();

                var results = new List<Dictionary<string, object>>();
                foreach (var p in context.Projects)
                {
                    var props = new Dictionary<string, object>();
                    CopyProperty(props, p);

                    if (includeDetails)
                    {
                        context.Load(p.IncludeCustomFields);
                        context.ExecuteQuery();
                        props["FieldValues"] = p.IncludeCustomFields.FieldValues;

                        props["EnterpriseProjectTypeId"] = p.EnterpriseProjectType.Id;
                        if (VerifyServerObject(p.CheckedOutBy))
                        {
                            try
                            {
                                props["CheckedOutBy" + AveObjectModelConstant.ObjectPropertySuffix] = p.CheckedOutBy.Id;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Get project check out user failed. Error:{0}", ex);
                            }
                        }
                        if (VerifyServerObject(p.Owner))
                        {
                            try
                            {
                                props["Owner" + AveObjectModelConstant.ObjectPropertySuffix] = p.Owner.Id;
                            }
                            catch (Exception ex)
                            {
                                mLogger.Warn("Get project owner failed. Error:{0}", ex);
                            }
                        }
                    }
                    results.Add(props);
                }
                return results;
            }
        }

        public List<Dictionary<string, object>> QueryProjectTasks(Guid projectId, bool isPublished)
        {
            using (var context = CreateProjectContext())
            {
                var project = context.Projects.GetByGuid(projectId);
                if (isPublished)
                {
                    context.Load(project, p => p.Tasks);
                    context.Load(project, p => p.Tasks.Include(t => t.StatusManager.Id, t => t.Parent.Id));
                }
                else
                {
                    context.Load(project.Draft, p => p.Tasks);
                    context.Load(project.Draft, p => p.Tasks.Include(t => t.StatusManager.Id, t => t.Parent.Id));
                }

                context.ExecuteQuery();

                if (isPublished)
                {
                    return AssembleTasksProps(project.Tasks);
                }
                else
                {
                    return AssembleTasksProps(project.Draft.Tasks);
                }
            }
        }

        public Dictionary<string, object> QueryDraftProject(Guid projectId)
        {
            using (var context = CreateProjectContext())
            {
                var project = context.Projects.GetByGuid(projectId);
                context.Load(project.Draft);
                context.Load(project.Draft, 
                    p => p.HasMppPendingImport, 
                    p => p.IncludeCustomFields,
                    p => p.ProjectSiteUrl, 
                    p => p.TaskListId, 
                    p => p.EnterpriseProjectType.Id, 
                    p => p.CheckedOutBy.Id, 
                    p => p.Owner.Id);
                context.ExecuteQuery();
                var props = new Dictionary<string, object>();
                CopyProperty(props, project.Draft);
                props["FieldValues"] = project.Draft.IncludeCustomFields.FieldValues;
                if (VerifyServerObject(project.Draft.CheckedOutBy))
                {
                    try
                    {
                        props["CheckedOutBy" + AveObjectModelConstant.ObjectPropertySuffix] = project.Draft.CheckedOutBy.Id;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Get draft project check out user failed. Error:{0}", ex);
                    }
                }
                if (VerifyServerObject(project.Draft.Owner))
                {
                    try
                    {
                        props["Owner" + AveObjectModelConstant.ObjectPropertySuffix] = project.Draft.Owner.Id;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Get draft project owner failed. Error:{0}", ex);
                    }
                }

                return props;
            }
        }

        public List<Dictionary<string, object>> QueryProjectCalendars()
        {
            var result = new List<Dictionary<string, object>>();

            using (var context = CreateProjectContext())
            {
                context.Load(context.Calendars, calendars => calendars.IncludeWithDefaultProperties(
                    c => c.BaseCalendarExceptions));

                context.ExecuteQuery();

                foreach (var calendar in context.Calendars)
                {
                    var calProp = AssembleProjectCalendar(calendar);
                    result.Add(calProp);
                }
            }
            return result;
        }

        public List<Dictionary<string, object>> QueryProjectCustomFields()
        {
            var result = new List<Dictionary<string, object>>();

            using (var context = CreateProjectContext())
            {
                context.Load(context.CustomFields, cfs => cfs.IncludeWithDefaultProperties(
                    cf => cf.LookupTable.Name,
                    cf => cf.EntityType,
                    cf => cf.LookupEntries));

                context.ExecuteQuery();

                foreach (var field in context.CustomFields)
                {
                    var fieldProp = AssembleProjectCustomField(field);
                    result.Add(fieldProp);
                }
            }
            return result;
        }

        public List<Dictionary<string, object>> QueryProjectLookupTables()
        {
            var result = new List<Dictionary<string, object>>();

            using (var context = CreateProjectContext())
            {
                context.Load(context.LookupTables, lts => lts.IncludeWithDefaultProperties(
                    lt => lt.Entries,
                    lt => lt.Masks));

                context.ExecuteQuery();

                foreach (var table in context.LookupTables)
                {
                    var tableProp = AssembleProjectLookupTable(table);
                    result.Add(tableProp);
                }
            }
            return result;
        }

        public List<Dictionary<string, object>> QueryProjectEnterpriseProjectTypes()
        {
            var result = new List<Dictionary<string, object>>();

            using (var context = CreateProjectContext())
            {
                context.Load(context.EnterpriseProjectTypes, epts => epts.IncludeWithDefaultProperties(
                    ept => ept.WorkflowAssociationId,
                    ept => ept.WorkflowAssociationName,
                    ept => ept.ProjectDetailPages.IncludeWithDefaultProperties(
                        p => p.Item.Id,
                        p => p.Item.FileSystemObjectType,
                        p => p.Item.File.Exists,
                        p => p.Item.File.ServerRelativeUrl,
                        p => p.Item.File.Name)));

                context.ExecuteQuery();

                foreach (var ept in context.EnterpriseProjectTypes)
                {
                    var props = AssembleEnterpriseType(ept);
                    result.Add(props);
                }
            }
            return result;
        }

        public List<Dictionary<string, object>> QueryProjectEnterpriseResources()
        {
            var result = new List<Dictionary<string, object>>();

            using (var context = CreateProjectContext())
            {
                context.Load(context.EnterpriseResources, ers => ers.IncludeWithDefaultProperties(
                    er => er.DefaultAssignmentOwner.Id,
                    er => er.TimesheetManager.Id,
                    er => er.User.Id,
                    er => er.Assignments.IncludeWithDefaultProperties(ass => ass.CustomFields),
                    er => er.BaseCalendar,
                    er => er.BaseCalendar.BaseCalendarExceptions,
                    er => er.CustomFields,
                    er => er.ResourceCalendarExceptions));

                context.ExecuteQuery();

                foreach (var resource in context.EnterpriseResources)
                {
                    var resourceProp = AssembleProjectEnterpriseResource(resource);
                    result.Add(resourceProp);
                }
            }
            return result;
        }

        public List<Dictionary<string, object>> QueryProjectPhases()
        {
            var result = new List<Dictionary<string, object>>();

            using (var context = CreateProjectContext())
            {
                #region Reserve
                //context.Load(context.Phases, ps => ps.IncludeWithDefaultProperties(p => p.Stages.IncludeWithDefaultProperties(
                //    s => s.CustomFields,
                //    s => s.ProjectDetailPages.IncludeWithDefaultProperties(
                //        dp => dp,
                //        dp => dp.Page,
                //        dp => dp.Page.Item.Id,
                //        dp => dp.Page.Item.FileSystemObjectType,
                //        dp => dp.Page.Item.File.Exists,
                //        dp => dp.Page.Item.File.ServerRelativeUrl,
                //        dp => dp.Page.Item.File.Name),
                //    s => s.WorkflowStatusPage,
                //    s => s.WorkflowStatusPage.Item.Id,
                //    s => s.WorkflowStatusPage.Item.FileSystemObjectType,
                //    s => s.WorkflowStatusPage.Item.File.Exists,
                //    s => s.WorkflowStatusPage.Item.File.ServerRelativeUrl,
                //    s => s.WorkflowStatusPage.Item.File.Name)));
                #endregion
                context.Load(context.Phases);

                context.ExecuteQuery();

                foreach (var phase in context.Phases)
                {
                    var phasePorp = new Dictionary<string, object>();
                    CopyProperty(phasePorp, phase);

                    result.Add(phasePorp);
                }
            }

            return result;
        }

        public List<Dictionary<string, object>> QueryProjectStages()
        {
            var result = new List<Dictionary<string, object>>();

            using (var context = CreateProjectContext())
            {
                context.Load(context.Stages, ss => ss.IncludeWithDefaultProperties(
                    s => s.CustomFields,
                    s => s.Phase,
                    s => s.ProjectDetailPages.IncludeWithDefaultProperties(
                        p => p,
                        p => p.Page,
                        p => p.Page.Item.Id,
                        p => p.Page.Item.FileSystemObjectType,
                        p => p.Page.Item.File.Exists,
                        p => p.Page.Item.File.ServerRelativeUrl,
                        p => p.Page.Item.File.Name),
                    s => s.WorkflowStatusPage,
                    s => s.WorkflowStatusPage.Item.Id,
                    s => s.WorkflowStatusPage.Item.FileSystemObjectType,
                    s => s.WorkflowStatusPage.Item.File.Exists,
                    s => s.WorkflowStatusPage.Item.File.ServerRelativeUrl,
                    s => s.WorkflowStatusPage.Item.File.Name));

                context.ExecuteQuery();

                foreach (var stage in context.Stages)
                {
                    var stageProp = AssembleProjectStage(stage);
                    result.Add(stageProp);
                }
            }

            return result;
        }

        public List<Dictionary<string, object>> QueryProjectDetailPages()
        {
            using (var context = CreateProjectContext())
            {
                context.Load(context.ProjectDetailPages, ps => ps.IncludeWithDefaultProperties(
                    pdp => pdp.Item.Id,
                    pdp => pdp.Item.FileSystemObjectType,
                    pdp => pdp.Item.File.Exists,
                    pdp => pdp.Item.File.ServerRelativeUrl,
                    pdp => pdp.Item.File.Name));
                context.ExecuteQuery();
                var pagesProps = new List<Dictionary<string, object>>();
                foreach (ProjectDetailPage page in context.ProjectDetailPages)
                {
                    var pageProp = AssembleProjectDetailPage(page);
                    pagesProps.Add(pageProp);
                }
                return pagesProps;
            }
        }

        public Dictionary<string, object> GetProjectById(Guid id)
        {
            using (var context = CreateProjectContext())
            {
                PublishedProject proj = context.Projects.GetByGuid(id);
                context.Load(proj);
                context.Load(proj,
                p => p.HasMppPendingImport,
                p => p.IsEnterpriseProject,
                p => p.ProjectSiteUrl,
                p => p.TaskListId,
                p => p.IncludeCustomFields,
                p => p.EnterpriseProjectType.Id,
                p => p.CheckedOutBy.Id,
                p => p.Owner.Id
                );
                context.ExecuteQuery();

                var props = new Dictionary<string, object>();
                CopyProperty(props, proj);
                props["FieldValues"] = proj.IncludeCustomFields.FieldValues;
                props["EnterpriseProjectTypeId"] = proj.EnterpriseProjectType.Id;
                if (VerifyServerObject(proj.CheckedOutBy))
                {
                    try
                    {
                        props["CheckedOutBy" + AveObjectModelConstant.ObjectPropertySuffix] = proj.CheckedOutBy.Id;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Get project check out user failed. Error:{0}", ex);
                    }
                }
                if (VerifyServerObject(proj.Owner))
                {
                    try
                    {
                        props["Owner" + AveObjectModelConstant.ObjectPropertySuffix] = proj.Owner.Id;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Get project owner failed. Error:{0}", ex);
                    }
                }
                return props;
            }
        }

        #endregion

        #region add

        public Dictionary<string, object> AddLookupTable(AveProjectLookupTableInfo tableInfo)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                context.Load(context.LookupTables, lts => lts.Include(lt => lt.Name));
                context.ExecuteQuery();
                LookupTable table = AddLookupTable(tableInfo, context);
                return AssembleProjectLookupTable(table);
            }
        }

        private LookupTable AddLookupTable(AveProjectLookupTableInfo info, ProjectContext context)
        {
            mLogger.Info("Adding a lookup table with name:{0}.", info.Name);
            Dictionary<string, Guid> lookupTextMapping = new Dictionary<string, Guid>();

            var entries = new List<LookupEntryCreationInformation>(info.Entries.Count);
            foreach (var entryInfo in info.Entries)
            {
                var entry = new LookupEntryCreationInformation
                {
                    Description = entryInfo.Description,
                    Id = entryInfo.Id,
                    SortIndex = entryInfo.SortIndex,
                };

                Guid parentId = Guid.Empty;
                if (info.FieldType == (int)CustomFieldType.TEXT)
                {
                    parentId = GetLookupTextParentId(entryInfo, lookupTextMapping);
                }
                if (parentId != Guid.Empty)
                {
                    entry.ParentId = parentId;
                }
                var value = new LookupEntryValue();
                SetLookupEntryValue(value, info.FieldType, entryInfo);
                entry.Value = value;
                entries.Add(entry);
            }
            var masks = new List<LookupMask>(info.Masks.Count);
            foreach (var maskInfo in info.Masks)
            {
                var mask = new LookupMask
                {
                    Length = maskInfo.Length,
                    MaskType = (LookupTableMaskSequence)maskInfo.MaskType,
                    Separator = maskInfo.Separator
                };
                masks.Add(mask);
            }

            var ltci = new LookupTableCreationInformation
            {
                Entries = entries,
                Id = info.Id,
                Masks = masks,
                Name = info.Name,
                SortOrder = (LookupTableSortOrder)info.SortOrder
            };
            var table = context.LookupTables.Add(ltci);
            context.LookupTables.Update();
            context.Load(table);
            context.Load(table, t => t.Entries, t => t.Masks);
            context.ExecuteQuery();
            return table;
        }

        private void SetLookupEntryValue(LookupEntryValue entryValue, int fieldType, AveProjectLookupEntryInfo info)
        {
            switch (fieldType)
            {
                case (int)CustomFieldType.COST:
                case (int)CustomFieldType.NUMBER:
                    decimal value = Decimal.Parse(info.Value);
                    entryValue.NumberValue = value;
                    break;
                case (int)CustomFieldType.DATE:
                case (int)CustomFieldType.FINISHDATE:
                    var ticks = long.Parse(info.Value);
                    entryValue.DateValue = new DateTime(ticks);
                    break;
                case (int)CustomFieldType.DURATION:
                    entryValue.DurationValue = info.Value;
                    break;
                case (int)CustomFieldType.TEXT:
                    entryValue.TextValue = info.Value;
                    break;
                default:
                    mLogger.Warn("Use default method set lookup entry value. FieldType:{0}", fieldType.ToString());
                    break;
            }
        }

        private Guid GetLookupTextParentId(AveProjectLookupEntryInfo info, Dictionary<string, Guid> mapping)
        {
            Guid result = Guid.Empty;
            foreach (var kv in mapping)
            {
                string endValue = kv.Key + info.Value;
                if (info.FullValue.EndsWith(endValue, StringComparison.OrdinalIgnoreCase))
                {
                    result = kv.Value;
                    break;
                }
            }

            if (info.HasChildren)
            {
                mapping[info.Value + info.MaskSeparator] = info.Id;
            }

            return result;
        }

        public Dictionary<string, object> AddCustomField(AveProjectCustomFieldInfo customFieldInfo)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (!context.LookupTables.AreItemsAvailable)
                {
                    context.Load(context.LookupTables, lts => lts.Include(lt => lt.Name));
                }
                context.ExecuteQuery();
                CustomField field = AddCustomField(customFieldInfo, context);
                return AssembleProjectCustomField(field);
            }
        }

        private CustomField AddCustomField(AveProjectCustomFieldInfo info, ProjectContext context)
        {
            mLogger.Info("Adding a custom field with name:{0}.", info.Name);
            var cfci = new CustomFieldCreationInformation
            {
                Description = info.Description,
                EntityType = GetEntityType(info.EntityType.Name, context),
                FieldType = (CustomFieldType)info.FieldType,
                Formula = info.Formula,
                Id = info.Id,
                IsEditableInVisibility = info.IsEditableInVisibility,
                IsMultilineText = info.IsMultilineText,
                IsRequired = info.IsRequired,
                IsWorkflowControlled = info.IsWorkflowControlled,
                LookupAllowMultiSelect = info.LookupAllowMultiSelect,
                LookupDefaultValue = info.LookupDefaultValue,
                Name = info.Name
            };
            if (!string.IsNullOrEmpty(info.LookupTable))
            {
                cfci.LookupTable = context.LookupTables.First(t => string.Equals(info.LookupTable, t.Name, StringComparison.OrdinalIgnoreCase));
            }
            var field = context.CustomFields.Add(cfci);
            context.CustomFields.Update();
            context.Load(field);
            context.Load(field, f => f.LookupTable.Name, f => f.EntityType, f => f.LookupEntries);
            context.ExecuteQuery();
            return field;
        }

        public Dictionary<string, object> AddEnterpriseType(AveProjectEnterpriseProjectTypeInfo eptInfo)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                try
                {
                    EnterpriseProjectType ept = CreateEPT(eptInfo, context);
                    return AssembleEnterpriseType(ept);
                }
                catch (Exception ex)
                {
                    mLogger.Error("An error occurred while restoring enterprise project type data. Name:{0}. Error:{1}", eptInfo.Name, ex);
                }
                return new Dictionary<string, object>();
            }
        }

        private EnterpriseProjectType CreateEPT(AveProjectEnterpriseProjectTypeInfo info, ProjectContext context)
        {
            mLogger.Info("Adding a enterprise project type with name:{0}.", info.Name);

            var eptci = new EnterpriseProjectTypeCreationInformation
            {
                Description = info.Description,
                Id = info.Id,
                ImageUrl = info.ImageUrl,
                IsDefault = info.IsDefault,
                IsManaged = info.IsManaged,
                Name = info.Name,
                Order = info.Order,
                PermissionSyncEnable = info.PermissionSyncEnable,
                TaskListSyncEnable = info.TaskListSyncEnable,
                SiteCreationOption = (EnterpriseProjectTypeSiteCreationOptions)info.SiteCreationOption,
                SiteCreationURL = info.SiteCreationURL,
                //ProjectPlanTemplateId = info.ProjectPlanTemplateId,
                WorkspaceTemplateLCID = (uint)info.WorkspaceTemplateLCID,
                WorkspaceTemplateName = info.WorkspaceTemplateName,
                WorkflowAssociationId = info.WorkflowAssociationId,
                //WorkflowAssociationName = info.WorkflowAssociationName
            };
            #region Set project detail page
            var pdpcis = new List<ProjectDetailPageCreationInformation>();
            foreach (var pdp in info.ProjectDetailPages)
            {
                var pci = new ProjectDetailPageCreationInformation
                {
                    Id = pdp.Id,
                    IsCreate = pdp.IsCreatePDP,
                    Position = pdp.Position
                };
                pdpcis.Add(pci);
            }
            eptci.ProjectDetailPages = pdpcis;
            #endregion

            var ept = context.EnterpriseProjectTypes.Add(eptci);
            
            context.EnterpriseProjectTypes.Update();
            context.Load(ept);
            context.Load(ept,
                e => e.WorkflowAssociationId,
                e => e.WorkflowAssociationName,
                e => e.ProjectDetailPages.IncludeWithDefaultProperties(
                        p => p.Item.Id,
                        p => p.Item.FileSystemObjectType,
                        p => p.Item.File.Exists,
                        p => p.Item.File.ServerRelativeUrl,
                        p => p.Item.File.Name));
            context.ExecuteQuery();
            return ept;
        }

        public Dictionary<string, object> AddEnterpriseResource(AveProjectEnterpriseResourceInfo resourceInfo)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (!context.Calendars.AreItemsAvailable)
                {
                    context.Load(context.Calendars, cs => cs.Include(c => c.Name));
                }
                context.ExecuteQuery();
                Dictionary<string, object> prop = new Dictionary<string, object>();
                try
                {
                    var resource = AddEnterpriseResource(resourceInfo, context);
                    //EnsureCustomField(resourceInfo, context);
                    UpdateEnterpriseResource(resource, resourceInfo, context);
                    prop =AssembleProjectEnterpriseResource(resource);
                }
                catch (Exception ex)
                {
                    mLogger.Error("An error occurred while restoring enterprise resource data. Name:{0}. Error:{1}", resourceInfo.Name, ex);
                }
                return prop;
            }
        }

        private EnterpriseResource AddEnterpriseResource(AveProjectEnterpriseResourceInfo info, ProjectContext context)
        {
            mLogger.Info("Adding an enterprise resource with name:{0}", info.Name);
            var erci = new EnterpriseResourceCreationInformation
            {
                Id = info.Id,
                IsBudget = info.IsBudget,
                IsGeneric = info.IsGeneric,
                IsInactive = !info.IsActive,
                Name = info.Name,
                ResourceType = (EnterpriseResourceType)info.ResourceType
            };

            var resource = context.EnterpriseResources.Add(erci);
            context.EnterpriseResources.Update();
            context.Load(resource, r => r.Name, r => r.IsCheckedOut, r => r.CustomFields);
            context.ExecuteQuery();

            return resource;
        }

        private void UpdateEnterpriseResource(EnterpriseResource resource, AveProjectEnterpriseResourceInfo info, ProjectContext context)
        {
            var baseCalendar = GetCalendarByName(info.BaseCalendar.Name, context);
            if (baseCalendar != null)
            {
                resource.BaseCalendar = baseCalendar;
            }
            if (info.FieldValues != null && info.FieldValues.Count > 0)
            {
                foreach (KeyValuePair<string, object> pair in info.FieldValues)
                {
                    resource[pair.Key] = pair.Value;
                }
                //foreach (var cfInfo in info.CustomFields)
                //{
                //    var field = GetCustomFieldByName(cfInfo.Name, context);
                //    if (field != null)
                //    {
                //        object obj;
                //        if (info.FieldValues.TryGetValue(cfInfo.InternalName, out obj))
                //        {
                //            resource[field.InternalName] = obj;
                //        }
                //    }
                //    else
                //    {
                //        throw new ArgumentNullException("CustomField", string.Format("Cannot find the custom field associated with this enterprise resource [{0}]", info.Name));
                //    }
                //}
            }

            resource.CanLevel = info.CanLevel;
            resource.Code = info.Code;
            //resource.CostAccrual
            resource.CostCenter = info.CostCenter;
            if (info.DefaultAssignmentOwnerId != 0)
            {
                var owner = context.Web.SiteUsers.GetById(info.DefaultAssignmentOwnerId);
                resource.DefaultAssignmentOwner = owner;
            }
            resource.DefaultBookingType = (Microsoft.ProjectServer.Client.BookingType)info.DefaultBookingType;
            resource.Email = info.Email;
            //resource.Engagements
            resource.ExternalId = info.ExternalId;
            resource.Group = info.Group;
            if (info.HireDate != DateTime.MinValue)
            {
                resource.HireDate = info.HireDate;
            }
            resource.Initials = info.Initials;
            resource.IsActive = info.IsActive;
            resource.MaterialLabel = info.MaterialLabel;
            resource.Name = info.Name;
            resource.Phonetics = info.Phonetics;
            resource.RequiresEngagements = info.RequiresEngagements;
            foreach (var rceInfo in info.ResourceCalendarExceptions)
            {
                var rceci = new CalendarExceptionCreationInformation
                {
                    Finish = rceInfo.Finish,
                    Name = rceInfo.Name,
                    //RecurrenceDays
                    RecurrenceFrequency = rceInfo.RecurrenceFrequency,
                    RecurrenceMonth = rceInfo.RecurrenceMonth,
                    RecurrenceMonthDay = rceInfo.RecurrenceMonthDay,
                    RecurrenceType = (CalendarRecurrenceType)rceInfo.RecurrenceType,
                    RecurrenceWeek = (CalendarRecurrenceWeek)rceInfo.RecurrenceWeek,
                    Shift1Finish = rceInfo.Shift1Finish,
                    Shift1Start = rceInfo.Shift1Start,
                    Shift2Finish = rceInfo.Shift2Finish,
                    Shift2Start = rceInfo.Shift2Start,
                    Shift3Finish = rceInfo.Shift3Finish,
                    Shift3Start = rceInfo.Shift3Start,
                    Shift4Finish = rceInfo.Shift4Finish,
                    Shift4Start = rceInfo.Shift4Start,
                    Shift5Finish = rceInfo.Shift5Finish,
                    Shift5Start = rceInfo.Shift5Start,
                    Start = rceInfo.Start
                };
                resource.ResourceCalendarExceptions.Add(rceci);
            }
            if (info.TerminationDate != DateTime.MinValue)
            {
                resource.TerminationDate = info.TerminationDate;
            }
            if (info.TimesheetManagerId != 0)
            {
                var manager = context.Web.SiteUsers.GetById(info.TimesheetManagerId);
                resource.TimesheetManager = manager;
            }
            if (info.UserId != 0)
            {
                var user = context.Web.SiteUsers.GetById(info.UserId);
                resource.User = user;
            }
            if (resource.IsCheckedOut)
            {
                resource.ForceCheckIn();
            }
            context.EnterpriseResources.Update();
            context.Load(resource);
            context.Load(resource,
                    er => er.DefaultAssignmentOwner.Id,
                    er => er.TimesheetManager.Id,
                    er => er.User.Id,
                    er => er.Assignments.IncludeWithDefaultProperties(ass => ass.CustomFields),
                    er => er.BaseCalendar,
                    er => er.BaseCalendar.BaseCalendarExceptions,
                    er => er.CustomFields,
                    er => er.ResourceCalendarExceptions);
            context.ExecuteQuery();
        }

        public Dictionary<string, object> AddPhase(AveProjectPhaseInfo info)
        {
            Dictionary<string, object> phaseProp = new Dictionary<string,object>();
            using (ProjectContext context = CreateProjectContext())
            {
                context.Load(context.Phases, ps => ps.Include(p => p.Name, p => p.Id));
                context.ExecuteQuery();
                Phase phase = AddPhase(info, context);
                //phase.stages如何处理？
                CopyProperty(phaseProp, phase);
                return phaseProp;
            }
        }

        private Phase AddPhase(AveProjectPhaseInfo info, ProjectContext context)
        {
            mLogger.Info("Adding a phase with name:{0}.", info.Name);
            var pci = new PhaseCreationInformation
            {
                Id = info.Id,
                Name = info.Name,
                Description = info.Description
            };

            var phase = context.Phases.Add(pci);
            context.Phases.Update();
            context.Load(phase);
            context.ExecuteQuery();
            return phase;
        }

        public Dictionary<string, object> AddStage(AveProjectStageInfo info)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (!context.Phases.AreItemsAvailable)
                {
                    context.Load(context.Phases, ps => ps.Include(p => p.Name, p => p.Id));
                }
                if (!context.ProjectDetailPages.AreItemsAvailable)
                {
                    context.Load(context.ProjectDetailPages, pdps => pdps.Include(pdp => pdp.Name, pdp => pdp.Id));
                }
                context.Load(context.Stages, ss => ss.Include(s => s.Name));
                context.ExecuteQuery();
                Stage stage = AddStage(info, context);
                return AssembleProjectStage(stage);
            }
        }

        public Stage AddStage(AveProjectStageInfo info, ProjectContext context)
        {
            mLogger.Info("Adding a stage with name:{0}.", info.Name);
            var phase = context.Phases.First(p => string.Equals(p.Name, info.Phase, StringComparison.OrdinalIgnoreCase));
            if (phase == null)
            {
                throw new ArgumentNullException("Phase", string.Format("Cannot find the specified workflow phase [{0}]", info.Phase));
            }

            var tci = new StageCreationInformation
            {
                Behavior = (StrategicImpactBehavior)info.Behavior,
                CheckInRequired = info.CheckInRequired,
                Description = info.Description,
                Id = info.Id,
                Name = info.Name,
                PhaseId = phase.Id,
                SubmitDescription = info.SubmitDescription,
            };

            #region Set Stage Custom Field
            var scfcis = new List<StageCustomFieldCreationInformation>(info.CustomFields.Count);
            foreach (var fieldInfo in info.CustomFields)
            {
                //var field = GetCustomFieldByName(fieldInfo.Name, context);
                //if (field != null)
                //{
                var scfci = new StageCustomFieldCreationInformation
                {
                    Id = fieldInfo.Id,
                    ReadOnly = fieldInfo.ReadOnly,
                    Required = fieldInfo.Required
                };
                scfcis.Add(scfci);
                //}
                //else
                //{
                //    mLogger.Warn("Cannot find the specified stage custom field for {0}. Name:{1}", info.Name, fieldInfo.Name);
                //}
            }
            tci.CustomFields = scfcis;
            #endregion

            #region Set stage detail page
            var sdpcis = new List<StageDetailPageCreationInformation>(info.ProjectDetailPages.Count);
            foreach (var sdpInfo in info.ProjectDetailPages)
            {
                var sdpci = new StageDetailPageCreationInformation();
                sdpci.Description = sdpInfo.Description;
                var page = GetPageByName(sdpInfo.Name, context);
                if (page != null)
                {
                    sdpci.Id = page.Id;
                }
                else
                {
                    mLogger.Warn("Cannot find the specified stage detail page of {0}. Name:{1}", info.Name, sdpInfo.Name);
                }
                sdpci.RequiresAttention = sdpInfo.RequiresAttention;
                sdpcis.Add(sdpci);
            }
            tci.ProjectDetailPages = sdpcis;
            #endregion

            var workflowStatusPage = GetPageByName(info.WorkflowStatusPage.Name, context);
            if (workflowStatusPage != null)
            {
                tci.WorkflowStatusPageId = workflowStatusPage.Id;
            }
            else
            {
                mLogger.Warn("Cannot find the specified workflow status page of {0}. Name:{1}", info.Name, info.WorkflowStatusPage.Name);
            }

            var stage = context.Stages.Add(tci);
            context.Stages.Update();
            context.Load(stage);
            context.Load(stage, 
                s => s.CustomFields, 
                s => s.Phase, 
                s => s.ProjectDetailPages.IncludeWithDefaultProperties(
                    p => p,
                    p => p.Page,
                    p => p.Page.Item.Id,
                    p => p.Page.Item.FileSystemObjectType,
                    p => p.Page.Item.File.Exists,
                    p => p.Page.Item.File.ServerRelativeUrl,
                    p => p.Page.Item.File.Name),
                s =>s.WorkflowStatusPage,
                s => s.WorkflowStatusPage.Item.Id,
                s => s.WorkflowStatusPage.Item.FileSystemObjectType,
                s => s.WorkflowStatusPage.Item.File.Exists,
                s => s.WorkflowStatusPage.Item.File.ServerRelativeUrl,
                s => s.WorkflowStatusPage.Item.File.Name);
            context.ExecuteQuery();
            return stage;
        }

        #endregion

        #region delete

        public void DeleteProject(Guid id, string siteUrl)
        {
            using (var context = CreateProjectContext())
            {
                QueueJob job;
                Web projectSite = null;
                if (siteUrl != null)
                {
                    string prjectSiteServerUrl = AveUrlUtility.GetSiteServerRelativeUrl(siteUrl);
                    projectSite = context.Site.OpenWeb(prjectSiteServerUrl);
                }

                var project = context.Projects.GetByGuid(id);
                context.Load(project, p => p.IsCheckedOut, p => p.IsEnterpriseProject);
                context.ExecuteQuery();
                if (!project.IsEnterpriseProject && projectSite != null)
                {
                    try
                    {
                        mLogger.Info("switch sharepoint task to enterprise");
                        projectSite.Features.Remove(AveProjectConstants.PWSVisibilityFeatureUid, true);
                        projectSite.Features.Add(AveProjectConstants.PWSManagedFeatureUid, true, FeatureDefinitionScope.Farm);
                        projectSite.Update();
                        context.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("switch sharepoint task to enterprise failed, error:{0}.", e);
                    }
                }
                //can not delete checkout project, so we should check in first
                if (project.IsCheckedOut)
                {
                    job = project.Draft.Publish(true);
                    WaitForJob(job, context);
                }
                job = project.DeleteObject();
                WaitForJob(job, context);
                //context.ExecuteQuery();
            }
        }

        #endregion

        #region update

        public Dictionary<string, object> UpdateLookupTable(Guid id, Dictionary<string, object> updateProp)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                Dictionary<string, object> tableProp = new Dictionary<string, object>();
                LookupTable table = context.LookupTables.GetByGuid(id);
                AveObjectCopy.UpdateObjectBasicProperties(updateProp, table);
                if (Convert.ToInt32(updateProp["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    context.LookupTables.Update();
                    context.Load(table);
                    context.Load(table, t => t.Entries, t => t.Masks);
                    context.ExecuteQuery();
                    tableProp = AssembleProjectLookupTable(table);
                }
                return tableProp;
            }
        }

        public Dictionary<string, object> UpdateCustomField(Guid id, Dictionary<string,object> updateProp)
        {
            using (var context = CreateProjectContext())
            {
                Dictionary<string, object> fieldProp = new Dictionary<string, object>();
                CustomField field = context.CustomFields.GetByGuid(id);
                AveObjectCopy.UpdateObjectBasicProperties(updateProp, field);
                if (Convert.ToInt32(updateProp["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    context.CustomFields.Update();
                    context.Load(field);
                    context.Load(field, f => f.LookupTable.Name, f => f.EntityType, f => f.LookupEntries);
                    context.ExecuteQuery();
                    fieldProp = AssembleProjectCustomField(field);
                }
                return fieldProp;
            }
        }

        public Dictionary<string, object> UpdateEnterpriseProjectType(Guid id, Dictionary<string, object> updateProp)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                Dictionary<string, object> eptProp = new Dictionary<string, object>();
                EnterpriseProjectType ept = context.EnterpriseProjectTypes.GetByGuid(id);
                AveObjectCopy.UpdateObjectBasicProperties(updateProp, ept);
                if (Convert.ToInt32(updateProp["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    context.EnterpriseProjectTypes.Update();
                    context.Load(ept);
                    context.Load(ept,
                        e => e.WorkflowAssociationId,
                        e => e.WorkflowAssociationName,
                        e => e.ProjectDetailPages.IncludeWithDefaultProperties(
                                p => p.Item.Id,
                                p => p.Item.FileSystemObjectType,
                                p => p.Item.File.Exists,
                                p => p.Item.File.ServerRelativeUrl,
                                p => p.Item.File.Name));
                    context.ExecuteQuery();
                    eptProp = AssembleEnterpriseType(ept);
                }

                return eptProp;
            }
        }

        public Dictionary<string, object> UpdateEnterpriseResource(Guid id, Dictionary<string, object> updateProp)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                Dictionary<string, object> resourceProp = new Dictionary<string, object>();
                EnterpriseResource resource = context.EnterpriseResources.GetByGuid(id);
                AveObjectCopy.UpdateObjectBasicProperties(updateProp, resource);
                if (Convert.ToInt32(updateProp["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    context.EnterpriseResources.Update();
                    context.Load(resource);
                    context.Load(resource,
                            er => er.DefaultAssignmentOwner.Id,
                            er => er.TimesheetManager.Id,
                            er => er.User.Id,
                            er => er.Assignments.IncludeWithDefaultProperties(ass => ass.CustomFields),
                            er => er.BaseCalendar,
                            er => er.BaseCalendar.BaseCalendarExceptions,
                            er => er.CustomFields,
                            er => er.ResourceCalendarExceptions);
                    context.ExecuteQuery();
                    resourceProp = AssembleProjectEnterpriseResource(resource);
                }
                return resourceProp;
            }
        }

        public Dictionary<string, object> UpdateStage(Guid id, Dictionary<string, object> updateProp)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                Dictionary<string, object> stageProp = new Dictionary<string, object>();
                Stage stage = context.Stages.GetByGuid(id);
                AveObjectCopy.UpdateObjectBasicProperties(updateProp, stage);
                if (Convert.ToInt32(updateProp["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    context.Stages.Update();
                    context.Load(stage);
                    context.Load(stage,
                        s => s.CustomFields,
                        s => s.Phase,
                        s => s.ProjectDetailPages.IncludeWithDefaultProperties(
                            p => p,
                            p => p.Page,
                            p => p.Page.Item.Id,
                            p => p.Page.Item.FileSystemObjectType,
                            p => p.Page.Item.File.Exists,
                            p => p.Page.Item.File.ServerRelativeUrl,
                            p => p.Page.Item.File.Name),
                        s => s.WorkflowStatusPage,
                        s => s.WorkflowStatusPage.Item.Id,
                        s => s.WorkflowStatusPage.Item.FileSystemObjectType,
                        s => s.WorkflowStatusPage.Item.File.Exists,
                        s => s.WorkflowStatusPage.Item.File.ServerRelativeUrl,
                        s => s.WorkflowStatusPage.Item.File.Name);
                    context.ExecuteQuery();
                    stageProp = AssembleProjectStage(stage);
                }
                return stageProp;
            }
        }

        public Dictionary<string, object> UpdatePhase(Guid id, Dictionary<string, object> updateProp)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                Dictionary<string, object> phaseProp = new Dictionary<string, object>();
                Phase phase = context.Phases.GetByGuid(id);
                AveObjectCopy.UpdateObjectBasicProperties(updateProp, phase);
                if (Convert.ToInt32(updateProp["ValidPropertiesCount" + AveObjectModelConstant.ObjectPropertySuffix]) > 0)
                {
                    context.Phases.Update();
                    context.Load(phase);
                    context.ExecuteQuery();
                    CopyProperty(phaseProp, phase);
                }
                return phaseProp;
            }
        }

        #endregion

        #region restore pwa settings

        public void RestoreCalendar(List<AveProjectCalendarInfo> calendarInfo)
        {
            using (ProjectContext context = CreateProjectContext())
            {

            }
        }

        public void RestoreLookupTable(List<AveProjectLookupTableInfo> lookupTableInfos)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (lookupTableInfos.Count <= 0)
                {
                    mLogger.Info("There is no lookup table data need to be restored.");
                    //return result;
                }

                context.Load(context.LookupTables, lts => lts.Include(lt => lt.Name));
                context.ExecuteQuery();

                foreach (var table in lookupTableInfos)
                {
                    try
                    {
                        if (context.LookupTables.Any(t => string.Equals(t.Name, table.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring lookup table [{0}] since a same name existed.", table.Name);
                            continue;
                        }
                        AddLookupTable(table, context);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring lookup table data. Name:{0}. Error:{1}", table.Name, ex);
                    }
                }
            }
        }

        public void RestoreCustomFields(List<AveProjectCustomFieldInfo> customFieldInfos)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (customFieldInfos.Count <= 0)
                {
                    mLogger.Info("There is no custom field data need to be restored.");
                    //return result;
                }
                if (!context.LookupTables.AreItemsAvailable)
                {
                    context.Load(context.LookupTables, lts => lts.Include(lt => lt.Name));
                }
                context.Load(context.CustomFields, cfs => cfs.Include(cf => cf.Name, cf => cf.InternalName));
                context.ExecuteQuery();

                foreach (var field in customFieldInfos)
                {
                    try
                    {
                        if (context.CustomFields.Any(cf => string.Equals(cf.Name, field.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring custom field [{0}] since a same name existed.", field.Name);
                            continue;
                        }
                        AddCustomField(field, context);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring custom field data. Name:{0}. Error:{1}", field.Name, ex);
                    }
                }
            }
        }

        private EntityType GetEntityType(string name, ProjectContext context)
        {
            name = name.ToUpper();
            switch (name)
            {
                case "PROJECT":
                    return context.EntityTypes.ProjectEntity;
                case "TASK":
                    return context.EntityTypes.TaskEntity;
                case "ASSIGNMENT":
                    return context.EntityTypes.AssignmentEntity;
                case "RESOURCE":
                    return context.EntityTypes.ResourceEntity;
                default:
                    throw new Exception(string.Format("Unkown entity type string [{0}]", name));
            }
        }

        public void RestoreEnterpriseResource(List<AveProjectEnterpriseResourceInfo> resourceInfos)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (resourceInfos.Count <= 0)
                {
                    mLogger.Info("There is no enterprise resource data need to be restored.");
                    //return result;
                }

                if (!context.Calendars.AreItemsAvailable)
                {
                    context.Load(context.Calendars, cs => cs.Include(c => c.Name));
                }
                if (!context.CustomFields.AreItemsAvailable)
                {
                    context.Load(context.CustomFields, cfs => cfs.Include(cf => cf.InternalName, cf => cf.Name));
                }
                context.Load(context.EnterpriseResources, ers => ers.Include(er => er.Name));
                context.ExecuteQuery();

                foreach (var resourceInfo in resourceInfos)
                {
                    try
                    {
                        if (context.EnterpriseResources.Any(er => string.Equals(er.Name, resourceInfo.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring enterprise resource [{0}] since a same name existed.", resourceInfo.Name);
                            continue;
                        }
                        var resource = AddEnterpriseResource(resourceInfo, context);
                        UpdateEnterpriseResource(resource, resourceInfo, context);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring enterprise resource data. Name:{0}. Error:{1}", resourceInfo.Name, ex);
                    }
                }
            }
        }

       /* private void EnsureCustomField(AveProjectEnterpriseResourceInfo info, ProjectContext context)
        {
            foreach (var cfInfo in info.CustomFields)
            {
                var field = GetCustomFieldByName(cfInfo.Name, context);
                if (field == null)
                {
                    AddCustomField(cfInfo, context);
                }
            }
        }*/

        public void RestorePhase(List<AveProjectPhaseInfo> phaseInfos)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (phaseInfos.Count <= 0)
                {
                    mLogger.Info("There is no phase data need to be restored.");
                    //return result;
                }
                context.Load(context.Phases, ps => ps.Include(p => p.Name, p => p.Id));
                context.ExecuteQuery();

                foreach (var phaseInfo in phaseInfos)
                {
                    try
                    {
                        if (context.Phases.Any(p => string.Equals(p.Name, phaseInfo.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring phase [{0}] since a same name existed.", phaseInfo.Name);
                            continue;
                        }
                        AddPhase(phaseInfo, context);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring phase data. Name:{0}. Error:{1}", phaseInfo.Name, ex);
                    }
                }
            }
        }

        public void RestoreStage(List<AveProjectStageInfo> stageInfos)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (stageInfos.Count <= 0)
                {
                    mLogger.Info("There is no stage data need to be restore.");
                    //return result;
                }
                if (!context.Phases.AreItemsAvailable)
                {
                    context.Load(context.Phases, ps => ps.Include(p => p.Name, p => p.Id));
                }
                if (!context.ProjectDetailPages.AreItemsAvailable)
                {
                    context.Load(context.ProjectDetailPages, pdps => pdps.Include(pdp => pdp.Name, pdp => pdp.Id));
                }
                context.Load(context.Stages, ss => ss.Include(s => s.Name));
                context.ExecuteQuery();

                foreach (var stageInfo in stageInfos)
                {
                    try
                    {
                        if (context.Stages.Any(s => string.Equals(s.Name, stageInfo.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring stage [{0}] because a same name stage existed.", stageInfo.Name);
                            continue;
                        }
                        AddStage(stageInfo, context);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring stage data. Name:{0}. Error:{1}", stageInfo.Name, ex);
                    }
                }
            }
        }
        
        public void RestoreEnterpriseProjectTypes(List<AveProjectEnterpriseProjectTypeInfo> eptInfos)
        {
            using (ProjectContext context = CreateProjectContext())
            {
                if (eptInfos.Count <= 0)
                {
                    mLogger.Info("There is no enterprise project type data need to be restored.");
                }
                if (!context.ProjectDetailPages.AreItemsAvailable)
                {
                    context.Load(context.ProjectDetailPages, pdps => pdps.Include(pdp => pdp.Name, pdp => pdp.Id));
                }
                context.Load(context.EnterpriseProjectTypes, epts => epts.Include(ept => ept.Name));
                context.ExecuteQuery();

                var eptNames = new List<string>(context.EnterpriseProjectTypes.Count);
                foreach (var ept in context.EnterpriseProjectTypes)
                {
                    eptNames.Add(ept.Name);
                }

                foreach (var eptInfo in eptInfos)
                {
                    try
                    {
                        if (eptNames.Any(name => string.Equals(name, eptInfo.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring enterprise project type [{0}] since a same name existed.", eptInfo.Name);
                            continue;
                        }
                        CreateEPT(eptInfo, context);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring enterprise project type data. Name:{0}. Error:{1}", eptInfo.Name, ex);
                    }
                }
            }
        }

        private Calendar GetCalendarByName(string name, ProjectContext context)
        {
            return context.Calendars.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }



        private ProjectDetailPage GetPageByName(string name, ProjectContext context)
        {
            return context.ProjectDetailPages.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region Browser

       
        #endregion

        #region Private

        private List<Dictionary<string, object>> AssembleTasksProps(PublishedTaskCollection tasks)
        {
            var results = new List<Dictionary<string, object>>();
            foreach (var task in tasks)
            {
                var props = new Dictionary<string, object>();
                CopyProperty(props, task);
                if (VerifyServerObject(task.Parent))
                {
                    props["ParentId"] = task.Parent.Id;
                }
                if (VerifyServerObject(task.StatusManager))
                {
                    try
                    {
                        props["StatusManager" + AveObjectModelConstant.ObjectPropertySuffix] = task.StatusManager.Id;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Get published task status manager failed. Error:{0}", ex);
                    }
                }
                results.Add(props);
            }

            return results;
        }

        private List<Dictionary<string, object>> AssembleTasksProps(DraftTaskCollection tasks)
        {
            var results = new List<Dictionary<string, object>>();
            foreach (var task in tasks)
            {
                var props = new Dictionary<string, object>();
                CopyProperty(props, task);
                if (VerifyServerObject(task.Parent))
                {
                    props["ParentId"] = task.Parent.Id;
                }
                if (VerifyServerObject(task.StatusManager))
                {
                    try
                    {
                        props["StatusManager" + AveObjectModelConstant.ObjectPropertySuffix] = task.StatusManager.Id;
                    }
                    catch (Exception ex)
                    {
                        mLogger.Warn("Get draft task status manager failed. Error:{0}", ex);
                    }
                }
                results.Add(props);
            }

            return results;
        }

        private Dictionary<string, object> AssembleProjectCalendar(Calendar calendar)
        {
            var calProp = new Dictionary<string, object>();
            CopyProperty(calProp, calendar);

            var baseCalProps = new List<Dictionary<string, object>>();
            foreach (var baseCal in calendar.BaseCalendarExceptions)
            {
                var baseCalProp = new Dictionary<string, object>();
                CopyProperty(baseCalProp, baseCal);
                baseCalProps.Add(baseCalProp);
            }
            calProp["BaseCalendarExceptions"] = baseCalProps;

            return calProp;
        }

        private Dictionary<string, object> AssembleProjectCustomField(CustomField field)
        {
            var fieldProp = new Dictionary<string, object>();

            CopyProperty(fieldProp, field);
            var entityTypeProp = new Dictionary<string, object>();
            CopyProperty(entityTypeProp, field.EntityType);
            fieldProp["EntityType"] = entityTypeProp;
            string name = string.Empty;
            if (VerifyServerObject(field.LookupTable))
            {
                name = field.LookupTable.Name;
            }
            fieldProp["LookupTable"] = name;

            var entriesProps = new List<Dictionary<string, object>>();
            if (VerifyServerObject(field.LookupEntries))
            {
                foreach (var entry in field.LookupEntries)
                {
                    var entryProp = new Dictionary<string, object>();
                    CopyProperty(entryProp, entry);
                    entriesProps.Add(entryProp);
                }
            }
            fieldProp["LookupEntries"] = entriesProps;

            return fieldProp;
        }

        private Dictionary<string, object> AssembleProjectLookupTable(LookupTable table)
        {
            var tableProp = new Dictionary<string, object>();

            CopyProperty(tableProp, table);
            tableProp["SortOrder"] = (int)table.SortOrder;
            tableProp["FieldType"] = (int)table.FieldType;

            var entriesInfos = new List<Dictionary<string, object>>();
            foreach (var entry in table.Entries)
            {
                var entryInfo = new Dictionary<string, object>();
                CopyProperty(entryInfo, entry);
                FillExtraValue(table.FieldType, entry, entryInfo);
                entriesInfos.Add(entryInfo);
            }
            tableProp["Entries"] = entriesInfos;

            var masksInfos = new List<Dictionary<string, object>>();
            foreach (var mask in table.Masks)
            {
                var maskInfo = new Dictionary<string, object>();
                maskInfo["Length"] = mask.Length;
                maskInfo["MaskType"] = (int)mask.MaskType;
                maskInfo["Separator"] = mask.Separator;
                masksInfos.Add(maskInfo);
            }
            tableProp["Masks"] = masksInfos;

            return tableProp;
        }

        private void FillExtraValue(CustomFieldType type, LookupEntry entry, Dictionary<string, object> prop)
        {
            switch (type)
            {
                case CustomFieldType.TEXT:
                    var text = entry as LookupText;
                    if (text != null)
                    {
                        prop["Value"] = text.Value;
                        prop["MaskSeparator"] = text.Mask.Separator;
                        prop["HasChildren"] = text.HasChildren;
                    }
                    break;
                case CustomFieldType.DATE:
                case CustomFieldType.FINISHDATE:
                    var date = entry as LookupDate;
                    if (date != null)
                    {
                        prop["Value"] = date.Value.Ticks.ToString();
                    }
                    break;
                case CustomFieldType.COST:
                    var cost = entry as LookupCost;
                    if (cost != null)
                    {
                        prop["Value"] = cost.Value.ToString();
                    }
                    break;
                case CustomFieldType.DURATION:
                    var duration = entry as LookupDuration;
                    if (duration != null)
                    {
                        prop["Value"] = duration.Value;
                        prop["ValueTimeSpan"] = duration.ValueTimeSpan;
                    }
                    break;
                case CustomFieldType.NUMBER:
                    var number = entry as LookupNumber;
                    if (number != null)
                    {
                        prop["Value"] = number.Value.ToString();
                    }
                    break;
            }

        }

        private Dictionary<string, object> AssembleProjectDetailPage(ProjectDetailPage page)
        {
            var pageProp = new Dictionary<string, object>();
            CopyProperty(pageProp, page);
            var itemProp = new Dictionary<string, object>();
            CopyProperty(itemProp, page.Item);
            if (page.Item.File.Exists)
            {
                itemProp["Name"] = page.Item.File.Name;
                itemProp["ServerRelativeUrl"] = page.Item.File.ServerRelativeUrl;
            }
            pageProp["Item"] = itemProp;

            return pageProp;
        }

        private Dictionary<string, object> AssembleProjectEnterpriseResource(EnterpriseResource resource)
        {
            var resourceProp = new Dictionary<string, object>();

            CopyProperty(resourceProp, resource);
            resourceProp["ResourceType"] = (int)resource.ResourceType;
            if (VerifyServerObject(resource.DefaultAssignmentOwner))
            {
                resourceProp["DefaultAssignmentOwner" + AveObjectModelConstant.ObjectPropertySuffix] = resource.DefaultAssignmentOwner.Id;
            }
            if (VerifyServerObject(resource.TimesheetManager))
            {
                resourceProp["TimesheetManager" + AveObjectModelConstant.ObjectPropertySuffix] = resource.TimesheetManager.Id;
            }
            if (VerifyServerObject(resource.User))
            {
                resourceProp["User" + AveObjectModelConstant.ObjectPropertySuffix] = resource.User.Id;
            }

            var assignmentsInfos = new List<Dictionary<string, object>>(resource.Assignments.Count);
            foreach (var assignment in resource.Assignments)
            {
                var assignmentProp = AssembleStatusAssignment(assignment);
                assignmentsInfos.Add(assignmentProp);
            }
            resourceProp["Assignments"] = assignmentsInfos;
            resourceProp["BaseCalendar"] = AssembleProjectCalendar(resource.BaseCalendar);

            var fieldsInfos = new List<Dictionary<string, object>>();
            foreach (var field in resource.CustomFields)
            {
                var fieldInfo = AssembleProjectCustomField(field);
                fieldsInfos.Add(fieldInfo);
            }
            resourceProp["CustomFields"] = fieldsInfos;

            var calExceptionsInfos = new List<Dictionary<string, object>>();
            foreach (var excep in resource.ResourceCalendarExceptions)
            {
                var excepInfo = new Dictionary<string, object>();
                CopyProperty(excepInfo, excep);
                excepInfo["RecurrenceType"] = (int)excep.RecurrenceType;
                calExceptionsInfos.Add(excepInfo);
            }
            resourceProp["ResourceCalendarExceptions"] = calExceptionsInfos;

            return resourceProp;
        }

        private Dictionary<string, object> AssembleStatusAssignment(StatusAssignment assignment)
        {
            var assignmentProp = new Dictionary<string, object>();
            CopyProperty(assignmentProp, assignment);
            List<Dictionary<string, object>> fields = new List<Dictionary<string, object>>();
            foreach (CustomField field in assignment.CustomFields)
            {
                Dictionary<string, object> fieldProp = AssembleProjectCustomField(field);
                fields.Add(fieldProp);
            }
            assignmentProp["CustomFields" + AveObjectModelConstant.ObjectPropertySuffix] = fields;
            return assignmentProp;
        }

        private Dictionary<string, object> AssembleProjectStage(Stage stage)
        {
            var stageProp = new Dictionary<string, object>();
            CopyProperty(stageProp, stage);
            stageProp["Behavior"] = (int)stage.Behavior;
            stageProp["Phase"] = stage.Phase.Name;

            var fieldsInfos = new List<Dictionary<string, object>>(stage.CustomFields.Count);
            foreach (var field in stage.CustomFields)
            {
                var fieldProp = new Dictionary<string, object>();
                CopyProperty(fieldProp, field);
                fieldsInfos.Add(fieldProp);
            }
            stageProp["CustomFields"] = fieldsInfos;

            var stageDetailPagesInfos = new List<Dictionary<string, object>>(stage.ProjectDetailPages.Count);
            foreach (var page in stage.ProjectDetailPages)
            {
                var stagePageProp = new Dictionary<string, object>();
                CopyProperty(stagePageProp, page);
                var projectDetailPageProp = AssembleProjectDetailPage(page.Page);
                stagePageProp["Page"] = projectDetailPageProp;
                stageDetailPagesInfos.Add(stagePageProp);
            }
            stageProp["ProjectDetailPages"] = stageDetailPagesInfos;
            stageProp["WorkflowStatusPage"] = AssembleProjectDetailPage(stage.WorkflowStatusPage);

            return stageProp;
        }

        private Dictionary<string, object> AssembleEnterpriseType(EnterpriseProjectType ept)
        {
            var props = new Dictionary<string, object>();
            CopyProperty(props, ept);

            var pagesProps = new List<Dictionary<string, object>>();
            foreach (var page in ept.ProjectDetailPages)
            {
                var pageProp = AssembleProjectDetailPage(page);
                pagesProps.Add(pageProp);
            }
            props["ProjectDetailPages" + AveObjectModelConstant.ObjectPropertySuffix] = pagesProps;
            return props;
        }

        private AveProjectBrowserInfo SetProjectBrowserInfo(PublishedProject project)
        {
            AveProjectBrowserInfo projInfo = new AveProjectBrowserInfo();
            projInfo.Name = project.Name;
            projInfo.ID = project.Id;
            projInfo.IsEnterpriseProject = project.IsEnterpriseProject;
            projInfo.EnterpriseProjectTypeId = project.EnterpriseProjectType.Id;
            projInfo.IsCheckedOut = project.IsCheckedOut;
            if (!string.IsNullOrEmpty(project.ProjectSiteUrl))
            {
                projInfo.Url = project.ProjectSiteUrl;
            }
            return projInfo;
        }

        #endregion

        private ProjectContext CreateProjectContext()
        {
            var context = new AveRetryProjectContext(mWebUrl, mTenantId.ToString(), ChangeTokenProvider, GetTenantIdAndDefaultAppIdFunc);
            context.RequestTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout;
            context.SetTokenProvider(tokenProvider);
            return context;
        }

        private bool WaitForJob(QueueJob job, ProjectContext context)
        {
            var jobState = context.WaitForQueue(job, WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout);
            if (job.ServerObjectIsNull.HasValue && !job.ServerObjectIsNull.Value)
            {
                if (job.JobState != JobState.Success)
                {
                    mLogger.Warn("jobType:{0}, jobState:{1}", job.MessageType, job.JobState);
                }
                else
                {
                    mLogger.Info("jobType:{0}, jobState:{1}", job.MessageType, job.JobState);
                }
                return (jobState == JobState.Success) && (job.JobState == JobState.Success);
            }
            return false;
        }

        private bool VerifyServerObject(ClientObject obj)
        {
            return obj != null && obj.ServerObjectIsNull.HasValue && !obj.ServerObjectIsNull.Value;
        }

        public Dictionary<string, object> RestoreProject(AveProjectInfo info, AveProjectReader projectDetails, AveProjectConfig config, AveRestoreMode option)
        {
            var manager = new AveProjectManager(mPSIRequest, this.mWebUrl, this.mUserAccountInfo, tokenProvider, config);

            //manager.RestoreProjectGlobalData(projectDetails, option);

            //if (exist)
            //{
                return manager.RestoreProject(info, projectDetails, option);
            //}
            //else
            //{
            //    return manager.CreateProject(info, projectDetails);
            //}
        }

        #region TODO:TimeSheet
        //public List<Dictionary<string, object>> QueryProjectTimeSheet()
        //{
        //    var result = new List<Dictionary<string, object>>();
        //    using (var context = CreateProjectContext())
        //    {
        //        context.Load(context.TimeSheetPeriods, tsp => tsp.IncludeWithDefaultProperties(
        //                p => p.TimeSheet,
        //                p => p.TimeSheet.Creator,
        //                p => p.TimeSheet.Manager,
        //                p => p.TimeSheet.Period,
        //                p => p.TimeSheet.Lines.IncludeWithDefaultProperties(l => l.Assignment,
        //                                                                    l => l.Work,
        //                                                                    l => l.ValidationType,
        //                                                                    l => l.TotalWorkTimeSpan,
        //                                                                    l => l.TotalWork,
        //                                                                    l => l.Status,
        //                                                                    l => l.LineClass,
        //                                                                    l => l.Id,
        //                                                                    l => l.TimeSheet
        //            )
        //        ));
        //        context.ExecuteQuery();
        //        foreach (var period in context.TimeSheetPeriods)
        //        {
        //            try
        //            {
        //                TimeSheet ts = period.TimeSheet;
        //                Dictionary<string, object> timeSheetProp = AssembleProjectTimeSheet(ts);
        //                result.Add(timeSheetProp);
        //            }
        //            catch (Exception e)
        //            {
        //                if (e.Message.Contains("Object reference not set to an instance of an object on server. The object is associated with property TimeSheet."))
        //                {
        //                    mLogger.Error("The time sheet had not been created");
        //                }
        //                else
        //                {
        //                    throw;
        //                }
        //            }
        //        }
        //    }

        //    return result;
        //}

        //private Dictionary<string, object> AssembleProjectTimeSheet(TimeSheet ts)
        //{
        //    var timeSheetProp = new Dictionary<string, object>();
        //    CopyProperty(timeSheetProp, ts);
        //    timeSheetProp["EntryMode"] = (int)ts.EntryMode;
        //    if (ts.Period != null)
        //    {
        //        timeSheetProp["Period"] = ts.Period.ToString();
        //    }
        //    timeSheetProp["Status"] = (int)ts.Status;

        //    //var linesInfos = new List<Dictionary<string, object>>(ts.Lines.Count);
        //    //foreach (var line in ts.Lines)
        //    //{
        //    //    var lineProp = new Dictionary<string, object>();
        //    //    CopyProperty(lineProp, line);
        //    //    lineProp["LineClass"] = (int)line.LineClass;
        //    //    line.Status
        //    //    linesInfos.Add(lineProp);
        //    //}
        //    //stageProp["CustomFields"] = fieldsInfos;

        //    //var stageDetailPagesInfos = new List<Dictionary<string, object>>(stage.ProjectDetailPages.Count);
        //    //foreach (var page in stage.ProjectDetailPages)
        //    //{
        //    //    var stagePageProp = new Dictionary<string, object>();
        //    //    CopyProperty(stagePageProp, page);
        //    //    var projectDetailPageProp = AssembleProjectDetailPage(page.Page);
        //    //    stagePageProp["Page"] = projectDetailPageProp;
        //    //    stageDetailPagesInfos.Add(stagePageProp);
        //    //}
        //    //stageProp["ProjectDetailPages"] = stageDetailPagesInfos;
        //    //stageProp["WorkflowStatusPage"] = AssembleProjectDetailPage(stage.WorkflowStatusPage);

        //    return timeSheetProp;
        //}

        ///// <summary>
        ///// 目前在project online的页面中，仅能创建出一个time sheet，所以在inplace还原时，仅对这个time sheet的属性做update
        ///// 包括其中的time line和work
        ///// </summary>
        ///// <param name="timeSheetInfos"></param>
        //public void RestoreTimeSheet(List<AveProjectTimeSheetInfo> timeSheetInfos)
        //{
        //    using (var context = CreateProjectContext())
        //    {
        //        if (timeSheetInfos.Count < 0)
        //        {
        //            mLogger.Warn("There is no timesheet to be restore");
        //            return;
        //        }
        //        context.Load(context.TimeSheetPeriods, tsp => tsp.IncludeWithDefaultProperties(
        //                p => p.TimeSheet,
        //                p => p.TimeSheet.Creator,
        //                p => p.TimeSheet.Manager,
        //                p => p.TimeSheet.Period,
        //                p => p.TimeSheet.Lines.IncludeWithDefaultProperties(l => l.Assignment,
        //                                                                    l => l.Work,
        //                                                                    l => l.ValidationType,
        //                                                                    l => l.TotalWorkTimeSpan,
        //                                                                    l => l.TotalWork,
        //                                                                    l => l.Status,
        //                                                                    l => l.LineClass,
        //                                                                    l => l.Id,
        //                                                                    l => l.TimeSheet
        //            )
        //        ));
        //        context.ExecuteQuery();
        //        //如果目的端没有timesheetperiod，则无法创建timesheet
        //        if (context.TimeSheetPeriods.Count <= 0)
        //        {
        //            mLogger.Warn("There is no time sheet period in destination, we can not create or update timesheet in this situation.");
        //            return;
        //        }
        //        var period = context.TimeSheetPeriods[0];
        //        var timeSheet = period.TimeSheet;
        //        if (timeSheet != null)
        //        {
        //            foreach (var timeSheetInfo in timeSheetInfos)
        //            {
        //                try
        //                {
        //                    if (timeSheetInfo.Name.Equals(timeSheet.Name, StringComparison.OrdinalIgnoreCase))
        //                    {
        //                        UpdateTimeSheetProperties(timeSheet, timeSheetInfo, context);
        //                        timeSheet.Update();
        //                        context.Load(timeSheet);
        //                        context.ExecuteQuery();
        //                    }
        //                    else
        //                    {
        //                        //create a new time sheet
        //                        //使用period.CreateTimeSheet()不能创建出新的timesheet，目前只能update已存在的timesheet属性.
        //                    }
        //                }
        //                catch (Exception e)
        //                {
        //                    mLogger.Error("Update timeSheet properties failed,timeSheet name:{0}, error:{1}", timeSheet.Name, e);
        //                }

        //            }
        //        }
        //    }
        //}

        //private void UpdateTimeSheetProperties(TimeSheet timeSheet, AveProjectTimeSheetInfo timeSheetInfo, ProjectContext context)
        //{
        //    //timeSheet.Name = timeSheetInfo.Name;
        //    timeSheet.EntryMode = (TimeSheetEntryMode)timeSheetInfo.EntryMode;
        //    timeSheet.IsControlledByOwner = timeSheetInfo.IsControlledByOwner;
        //    timeSheet.IsProcessed = timeSheetInfo.IsProcessed;
        //    timeSheet.Status = (TimeSheetStatus)timeSheetInfo.Status;
        //    # region read only properties
        //    //timeSheet.TotalActualWork = timeSheetInfo.TotalActualWork;
        //    //timeSheet.TotalActualWorkTimeSpan = timeSheetInfo.TotalActualWorkTimeSpan;
        //    //timeSheet.TotalNonBillableOvertimeWork = timeSheetInfo.TotalNonBillableOvertimeWork;
        //    //timeSheet.TotalNonBillableOvertimeWorkTimeSpan = timeSheetInfo.TotalNonBillableOvertimeWorkTimeSpan;
        //    //timeSheet.TotalNonBillableWork = timeSheetInfo.TotalNonBillableWork;
        //    //timeSheet.TotalNonBillableWorkTimeSpan = timeSheetInfo.TotalNonBillableWorkTimeSpan;
        //    //timeSheet.TotalOvertimeWork = timeSheetInfo.TotalOvertimeWork;
        //    //timeSheet.TotalOvertimeWorkTimeSpan = timeSheetInfo.TotalOvertimeWorkTimeSpan;
        //    //timeSheet.TotalWork = timeSheetInfo.TotalWork;
        //    //timeSheet.TotalWorkTimeSpan = timeSheetInfo.TotalWorkTimeSpan;
        //    # endregion
        //    if (timeSheetInfo.Lines != null)
        //    {
        //        for (int i = 0; i < timeSheetInfo.Lines.Count; i++)
        //        {
        //            var lineInfo = timeSheetInfo.Lines[i];
        //            var line = GetTimeSheetLineById(timeSheet.Lines, lineInfo);
        //            if (line != null)
        //            {
        //                UpdateTimeSheetLineProperties(line, lineInfo);
        //            }
        //            else
        //            {
        //                AddTimeSheetLine(timeSheet, lineInfo, context);
        //            }
        //        }
        //    }
        //}

        //public void AddTimeSheetLine(TimeSheet timeSheet, AveProjectTimeSheetLineInfo lineInfo, ProjectContext context)
        //{
        //    TimeSheetLineCreationInformation tslCreatInfo = new TimeSheetLineCreationInformation()
        //    {
        //        #region 创建时如果给这些属性赋值，会抛Administrative line already exists and class is not set to allow multiple entries异常
        //        //Comment = lineInfo.Comment,
        //        //Id = lineInfo.Id,
        //        //LineClass = (Microsoft.ProjectServer.Client.TimeSheetLineClass)lineInfo.LineClass,
        //        //ProjectId = lineInfo.ProjectId
        //        #endregion
        //        TaskName = lineInfo.TaskName
        //    };
        //    TimeSheetLine line = timeSheet.Lines.Add(tslCreatInfo);
        //    for (int i = 0; i < lineInfo.Works.Count; i++)
        //    {
        //        var work = lineInfo.Works[i];
        //        AddTimeSheetWork(line, work);
        //    }
        //}

        //private void AddTimeSheetWork(TimeSheetLine line, AveProjectTimeSheetWorkInfo workInfo)
        //{
        //    TimeSheetWorkCreationInformation tswCreateInfo = new TimeSheetWorkCreationInformation()
        //    {
        //        //ActualWork = workInfo.ActualWork,
        //        Comment = workInfo.Comment,
        //        End = workInfo.End,
        //        NonBillableWork = workInfo.NonBillableWork,
        //        NonBillableOvertimeWork = workInfo.NonBillableOvertimeWork,
        //        OvertimeWork = workInfo.OvertimeWork,
        //        PlannedWork = workInfo.PlannedWork,
        //        Start = workInfo.Start
        //    };
        //    line.Work.Add(tswCreateInfo);
        //}

        //private void UpdateTimeSheetLineProperties(TimeSheetLine line, AveProjectTimeSheetLineInfo lineInfo)
        //{
        //    line.Comment = lineInfo.Comment;
        //    line.LineClass = (Microsoft.ProjectServer.Client.TimeSheetLineClass)lineInfo.LineClass;
        //    line.Status = (TimeSheetLineStatus)lineInfo.Status;

        //    for (int i = 0; i < line.Work.Count; i++)
        //    {
        //        var workInfo = lineInfo.Works[i];
        //        var work = GetTimeSheetWorkById(line.Work, workInfo);
        //        if (work != null)
        //        {
        //            UpdateTimeSheetWorkProperties(work, workInfo);
        //        }
        //        else
        //        {
        //            AddTimeSheetWork(line, workInfo);
        //        }
        //    }
        //}

        //private void UpdateTimeSheetWorkProperties(TimeSheetWork work, AveProjectTimeSheetWorkInfo workInfo)
        //{
        //    //TimeSpan 类型，如果带有double（0.0）,就会更新失败（Cannot cast DBNull.Value to type 'System.Decimal'. Please use a nullable type.）
        //    work.ActualWork = workInfo.ActualWork;
        //    //work.ActualWorkTimeSpan = workInfo.ActualWorkTimeSpan;
        //    work.Comment = workInfo.Comment;
        //    work.End = workInfo.End;
        //    work.NonBillableOvertimeWork = workInfo.NonBillableOvertimeWork;
        //    //work.NonBillableOvertimeWorkTimeSpan = workInfo.NonBillableOvertimeWorkTimeSpan;
        //    work.NonBillableWork = workInfo.NonBillableWork;
        //    //work.NonBillableWorkTimeSpan = workInfo.NonBillableWorkTimeSpan;
        //    work.OvertimeWork = workInfo.OvertimeWork;
        //    //work.OvertimeWorkTimeSpan = workInfo.OvertimeWorkTimeSpan;
        //    work.PlannedWork = workInfo.PlannedWork;
        //    //work.PlannedWorkTimeSpan = workInfo.PlannedWorkTimeSpan;
        //    work.Start = workInfo.Start;
        //}

        //private TimeSheetWork GetTimeSheetWorkById(TimeSheetWorkCollection works, AveProjectTimeSheetWorkInfo workInfo)
        //{
        //    return works.FirstOrDefault(w => w.Id == workInfo.Id);
        //}

        //private TimeSheetLine GetTimeSheetLineById(TimeSheetLineCollection lines, AveProjectTimeSheetLineInfo lineInfo)
        //{
        //    return lines.FirstOrDefault(l => l.Id == lineInfo.Id);
        //}

        #endregion

    }
}
