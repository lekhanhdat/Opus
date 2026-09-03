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
using System.Reflection;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.ProjectServer.Client;
using Microsoft.SharePoint.Client;

using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using Microsoft365.Authentication;
using AvePoint.Wrapper.Resource;
using AvePoint.ObjectModel.PSI;
using System.Threading;
using Microsoft365.SharePoint.CSOM.Extension;

namespace AvePoint.ObjectModel.ClientOM
{
    public class AveProjectManager
    {
        private static AveLogger mLogger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string mWebUrl;
        private AvePSIRequest mPSIRequest;
        private AveBPOSAccountInfo mAccount;
        private ITokenProvider mTokenProvider;
        private ProjectContext mContext;
        private AveProjectConfig mConfig;
        private PublishedProject mPublishedProj;
        private Web mProjectSite;


        public AveProjectManager(AvePSIRequest psi, string webUrl, AveBPOSAccountInfo account, ITokenProvider tokenProvider, AveProjectConfig config)
        {
            mPSIRequest = psi;
            this.mWebUrl = webUrl;
            this.mAccount = account;
            mConfig = config;
            this.mTokenProvider = tokenProvider;
            //[Project] memory issue
            mContext = CreateProjectContext();
        }

        private ProjectContext CreateProjectContext()
        {
            var context = new AveRetryProjectContext(mWebUrl);
            context.RequestTimeout = WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout;
            context.SetTokenProvider(this.mTokenProvider);
            return context;
        }

        private bool WaitForJob(QueueJob job)
        {
            try
            {
                var jobState = mContext.WaitForQueue(job, WrapperConfiguration.WrapperConfigurationForBPOS.HttpWebRequestTimeout);
                if (job.ServerObjectIsNull.HasValue && !job.ServerObjectIsNull.Value)
                {
                    if (job.JobState != JobState.Success)
                    {
                        mLogger.Warn("jobType:{0}, jobState:{1}", job.MessageType, job.JobState);
                        return mPSIRequest.WaitForQueue(job.Id);
                    }
                    else
                    {
                        mLogger.Info("jobType:{0}, jobState:{1}", job.MessageType, job.JobState);
                    }
                    return (jobState == JobState.Success) && (job.JobState == JobState.Success);
                }
                else
                {
                    mLogger.Warn("job failed");
                }
            }
            /*reivew-qlluo*/catch (ServerException se)
            {
                mLogger.Warn("job failed.server error code:{0}, error message:{1}", se.ServerErrorCode, se.ToString());
                throw;
            }
            return false;
        }

        //需要和备份顺序保持一致
        /*public void RestoreProjectGlobalData(AveProjectReader projectDetails, AveRestoreMode conflictOption)
        {
            //mLogger.Info("start restoring project global data");

            //try
            //{
            //    RestoreProjectCalendars(projectDetails);
            //    RestoreProjectLookupTables(projectDetails);
            //    RestoreProjectCustomFields(projectDetails);
            //    RestoreProjectEnterpriseResources(projectDetails);
            //    RestoreProjectPhases(projectDetails);
            //    RestoreProjectStages(projectDetails);
            //    RestoreProjectEnterpriseProjectTypes(projectDetails, conflictOption);
            //}
            //finally
            //{
            //    RefreshProjectContext();
            //}
        }*/

        #region pwa settings
        /*
        private Dictionary<string, object> RestoreProjectEnterpriseProjectTypes(AveProjectReader projectDetails, AveRestoreMode conflictOption)
        {
            var result = new Dictionary<string, object>();

            var eptInfos = projectDetails.GetProjectEnterpriseProjectTypes();

            if (this.mConfig.IncludeEnterpriseProjectTypes)
            {
                if (eptInfos.Count <= 0)
                {
                    mLogger.Info("There is no enterprise project type data need to be restored.");
                    return result;
                }
                if (!mContext.ProjectDetailPages.AreItemsAvailable)
                {
                    mContext.Load(mContext.ProjectDetailPages, pdps => pdps.Include(pdp => pdp.Name));
                }
                mContext.Load(mContext.EnterpriseProjectTypes, epts => epts.Include(ept => ept.Name));
                mContext.ExecuteQuery();

                var eptNames = new List<string>(mContext.EnterpriseProjectTypes.Count);
                foreach (var ept in mContext.EnterpriseProjectTypes)
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
                        CreateEPT(eptInfo);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring enterprise project type data. Name:{0}. Error:{1}", eptInfo.Name, ex);
                    }
                }
            }
            else
            {
                //add result
                mLogger.Warn("Skip restoring enterprise project type data due to configuration.");
            }

            return result;
        }

        private EnterpriseProjectType CreateEPT(AveProjectEnterpriseProjectTypeInfo info)
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
                ProjectPlanTemplateId = info.ProjectPlanTemplateId,
                WorkspaceTemplateName = info.WorkspaceTemplateName,
                //WorkflowAssociationId = info.WorkflowAssociationId,
                //WorkflowAssociationName = info.WorkflowAssociationName
            };
            #region Set project detail page
            var pdpcis = new List<ProjectDetailPageCreationInformation>();
            foreach (var pdp in info.ProjectDetailPages)
            {
                var page = GetPageByName(pdp.Name);
                if (page != null)
                {
                    var pci = new ProjectDetailPageCreationInformation
                    {
                        Id = page.Id,
                        IsCreate = pdp.PageType == AveProjectDetailPageType.NewProject,
                        //Position
                    };
                    pdpcis.Add(pci);
                    if (info.ProjectDetailPages.Count == 1)
                    {
                        var pci1 = new ProjectDetailPageCreationInformation
                        {
                            Id = page.Id,
                            IsCreate = !pci.IsCreate,
                            //Position
                        };
                        pdpcis.Add(pci1);
                    }
                }
                else
                {
                    mLogger.Warn("Cannot find the specified project detail page for {0}. Name:{1}", info.Name, pdp.Name);
                }
            }
            eptci.ProjectDetailPages = pdpcis;
            #endregion

            var ept = mContext.EnterpriseProjectTypes.Add(eptci);
            mContext.EnterpriseProjectTypes.Update();
            mContext.ExecuteQuery();
            return ept;
        }

        private Dictionary<string, object> RestoreProjectCalendars(AveProjectReader projectDetails)
        {
            var result = new Dictionary<string, object>();

            var calendarInfos = projectDetails.GetProjectCalendars();

            mLogger.Warn("Skip restoring project calendars data duo to API limitation.");
            return result;
        }

        private Dictionary<string, object> RestoreProjectLookupTables(AveProjectReader projectDetails)
        {
            var result = new Dictionary<string, object>();

            var lookupTableInfos = projectDetails.GetProjectLookupTables();

            if (this.mConfig.IncludeLookupTables)
            {
                if (lookupTableInfos.Count <= 0)
                {
                    mLogger.Info("There is no lookup table data need to be restored.");
                    return result;
                }
                mContext.Load(mContext.LookupTables, lts => lts.Include(lt => lt.Name));
                mContext.ExecuteQuery();

                foreach (var table in lookupTableInfos)
                {
                    try
                    {
                        if (mContext.LookupTables.Any(t => string.Equals(t.Name, table.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring lookup table [{0}] since a same name existed.", table.Name);
                            continue;
                        }
                        AddLookupTable(table);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring lookup table data. Name:{0}. Error:{1}", table.Name, ex);
                    }
                }
            }
            else
            {
                //add result
                mLogger.Warn("Skip restoring lookup table data due to configuration.");
            }

            return result;
        }

        private LookupTable AddLookupTable(AveProjectLookupTableInfo info)
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
                Id = Guid.NewGuid(),
                Masks = masks,
                Name = info.Name,
                SortOrder = (LookupTableSortOrder)info.SortOrder
            };
            var table = mContext.LookupTables.Add(ltci);
            mContext.LookupTables.Update();
            mContext.Load(table, t => t.Name);
            mContext.ExecuteQuery();
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

        private Guid GetLookupTextParentId(AveProjectLookupEntryInfo info, Dictionary<string,Guid> mapping)
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

        private Dictionary<string, object> RestoreProjectCustomFields(AveProjectReader projectDetails)
        {
            var result = new Dictionary<string, object>();

            var customFieldInfos = projectDetails.GetProjectCustomFields();

            if (this.mConfig.IncludeCustomFields)
            {
                if (customFieldInfos.Count <= 0)
                {
                    mLogger.Info("There is no custom field data need to be restored.");
                    return result;
                }
                if (!mContext.LookupTables.AreItemsAvailable)
                {
                    mContext.Load(mContext.LookupTables, lts => lts.Include(lt => lt.Name));
                }
                mContext.Load(mContext.CustomFields, cfs => cfs.Include(cf => cf.Name, cf => cf.InternalName));
                mContext.ExecuteQuery();

                foreach (var field in customFieldInfos)
                {
                    try
                    {
                        if (mContext.CustomFields.Any(cf => string.Equals(cf.Name, field.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring custom field [{0}] since a same name existed.", field.Name);
                            continue;
                        }
                        AddCustomField(field);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring custom field data. Name:{0}. Error:{1}", field.Name, ex);
                    }
                }
            }
            else
            {
                //add result
                mLogger.Warn("Skip restoring custom field data due to configuration.");
            }

            return result;
        }

        private CustomField AddCustomField(AveProjectCustomFieldInfo info)
        {
            mLogger.Info("Adding a custom field with name:{0}.", info.Name);
            var cfci = new CustomFieldCreationInformation
            {
                Description = info.Description,
                EntityType = GetEntityType(info.EntityType.Name),
                FieldType = (CustomFieldType)info.FieldType,
                Formula = info.Formula,
                //Id = info.Id,
                Id = Guid.NewGuid(),
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
                cfci.LookupTable = mContext.LookupTables.First(t => string.Equals(info.LookupTable, t.Name, StringComparison.OrdinalIgnoreCase));
            }
            var field = mContext.CustomFields.Add(cfci);
            mContext.CustomFields.Update();
            mContext.Load(field, f => f.InternalName, f => f.Name);
            mContext.ExecuteQuery();
            return field;
        }

        private EntityType GetEntityType(string name)
        {
            name = name.ToUpper();
            switch (name)
            {
                case "PROJECT":
                    return mContext.EntityTypes.ProjectEntity;
                case "TASK":
                    return mContext.EntityTypes.TaskEntity;
                case "ASSIGNMENT":
                    return mContext.EntityTypes.AssignmentEntity;
                case "RESOURCE":
                    return mContext.EntityTypes.ResourceEntity;
                default:
                    throw new Exception(string.Format("Unkown entity type string [{0}]", name));
            }
        }

        private Dictionary<string, object> RestoreProjectEnterpriseResources(AveProjectReader projectDetails)
        {
            var result = new Dictionary<string, object>();

            var resourceInfos = projectDetails.GetProjectEnterpriseResources();

            if (this.mConfig.IncludeEnterpriseResources)
            {
                if (resourceInfos.Count <= 0)
                {
                    mLogger.Info("There is no enterprise resource data need to be restored.");
                    return result;
                }

                if (!mContext.Calendars.AreItemsAvailable)
                {
                    mContext.Load(mContext.Calendars, cs => cs.Include(c => c.Name));
                }
                if (!mContext.CustomFields.AreItemsAvailable)
                {
                    mContext.Load(mContext.CustomFields, cfs => cfs.Include(cf => cf.InternalName, cf => cf.Name));
                }
                mContext.Load(mContext.EnterpriseResources, ers => ers.Include(er => er.Name));
                mContext.ExecuteQuery();

                foreach (var resourceInfo in resourceInfos)
                {
                    try
                    {
                        if (mContext.EnterpriseResources.Any(er => string.Equals(er.Name, resourceInfo.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring enterprise resource [{0}] since a same name existed.", resourceInfo.Name);
                            continue;
                        }
                        var resource = AddEnterpriseResource(resourceInfo);
                        EnsureCustomField(resourceInfo);
                        UpdateEnterpriseResource(resource, resourceInfo);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring enterprise resource data. Name:{0}. Error:{1}", resourceInfo.Name, ex);
                    }
                }
            }
            else
            {
                //add result
                mLogger.Warn("Skip restoring enterprise resource data due to configuration.");
            }

            return result;
        }

        private EnterpriseResource AddEnterpriseResource(AveProjectEnterpriseResourceInfo info)
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

            var resource = mContext.EnterpriseResources.Add(erci);
            mContext.EnterpriseResources.Update();
            mContext.Load(resource, r => r.Name, r => r.IsCheckedOut);
            mContext.ExecuteQuery();

            return resource;
        }

        private void EnsureCustomField(AveProjectEnterpriseResourceInfo info)
        {
            foreach (var cfInfo in info.CustomFields)
            {
                var field = GetCustomFieldByName(cfInfo.Name);
                if (field == null)
                {
                    AddCustomField(cfInfo);
                }
            }
        }

        private void UpdateEnterpriseResource(EnterpriseResource resource, AveProjectEnterpriseResourceInfo info)
        {
            var baseCalendar = GetCalendarByName(info.BaseCalendar.Name);
            if (baseCalendar != null)
            {
                resource.BaseCalendar = baseCalendar;
            }
            if (info.FieldValues != null && info.FieldValues.Count > 0)
            {
                foreach (var cfInfo in info.CustomFields)
                {
                    var field = GetCustomFieldByName(cfInfo.Name);
                    if (field != null)
                    {
                        object obj;
                        if (info.FieldValues.TryGetValue(cfInfo.InternalName, out obj))
                        {
                            resource[field.InternalName] = obj;
                        }
                    }
                    else
                    {
                        throw new ArgumentNullException("CustomField", string.Format("Cannot find the custom field associated with this enterprise resource [{0}]", info.Name));
                    }
                }
            }
            resource.CanLevel = info.CanLevel;
            resource.Code = info.Code;
            //resource.CostAccrual
            resource.CostCenter = info.CostCenter;
            if (!string.IsNullOrEmpty(info.DefaultAssignmentOwner))
            {
                var owner = CachedSiteUsers.GetByLoginName(info.DefaultAssignmentOwner);
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
            if (!string.IsNullOrEmpty(info.TimesheetManager))
            {
                var manager = CachedSiteUsers.GetByLoginName(info.TimesheetManager);
                resource.TimesheetManager = manager;
            }
            if (!string.IsNullOrEmpty(info.User))
            {
                var user = CachedSiteUsers.GetByLoginName(info.User);
                resource.User = user;
            }
            if (resource.IsCheckedOut)
            {
                resource.ForceCheckIn();
            }
            mContext.EnterpriseResources.Update();
            mContext.ExecuteQuery();
        }

        private Dictionary<string, object> RestoreProjectPhases(AveProjectReader projectDetails)
        {
            var result = new Dictionary<string, object>();

            var phaseInfos = projectDetails.GetProjectPhases();
            if (this.mConfig.IncludePhases)
            {
                if (phaseInfos.Count <= 0)
                {
                    mLogger.Info("There is no phase data need to be restored.");
                    return result;
                }
                mContext.Load(mContext.Phases, ps => ps.Include(p => p.Name, p => p.Id));
                mContext.ExecuteQuery();

                foreach (var phaseInfo in phaseInfos)
                {
                    try
                    {
                        if (mContext.Phases.Any(p => string.Equals(p.Name, phaseInfo.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring phase [{0}] since a same name existed.", phaseInfo.Name);
                            continue;
                        }
                        AddPhase(phaseInfo);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring phase data. Name:{0}. Error:{1}", phaseInfo.Name, ex);
                    }
                }
            }
            else
            {
                //add result
                mLogger.Warn("Skip restoring phase data due to configuration.");
            }
            return result;
        }

        private Phase AddPhase(AveProjectPhaseInfo info)
        {
            mLogger.Info("Adding a phase with name:{0}.", info.Name);
            var pci = new PhaseCreationInformation
            {
                Id = info.Id,
                Name = info.Name,
                Description = info.Description
            };

            var phase = mContext.Phases.Add(pci);
            mContext.Phases.Update();
            mContext.Load(phase, p => p.Name, p => p.Id);
            mContext.ExecuteQuery();
            return phase;
        }

        private Dictionary<string, object> RestoreProjectStages(AveProjectReader projectDetails)
        {
            var result = new Dictionary<string, object>();

            var stageInfos = projectDetails.GetProjectStages();

            if (this.mConfig.IncludeStages)
            {
                if (stageInfos.Count <= 0)
                {
                    mLogger.Info("There is no stage data need to be restore.");
                    return result;
                }
                if (!mContext.Phases.AreItemsAvailable)
                {
                    mContext.Load(mContext.Phases, ps => ps.Include(p => p.Name, p => p.Id));
                }
                if (!mContext.ProjectDetailPages.AreItemsAvailable)
                {
                    mContext.Load(mContext.ProjectDetailPages, pdps => pdps.Include(pdp => pdp.Name, pdp => pdp.Id));
                }
                mContext.Load(mContext.Stages, ss => ss.Include(s => s.Name));
                mContext.ExecuteQuery();

                foreach (var stageInfo in stageInfos)
                {
                    try
                    {
                        if (mContext.Stages.Any(s => string.Equals(s.Name, stageInfo.Name, StringComparison.OrdinalIgnoreCase)))
                        {
                            mLogger.Warn("Skip restoring stage [{0}] because a same name stage existed.", stageInfo.Name);
                            continue;
                        }
                        AddStage(stageInfo);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("An error occurred while restoring stage data. Name:{0}. Error:{1}", stageInfo.Name, ex);
                    }
                }
            }
            else
            {
                //add result
                mLogger.Warn("Skip restoring stage data due to configuration.");
            }
            return result;
        }

        public Stage AddStage(AveProjectStageInfo info)
        {
            mLogger.Info("Adding a stage with name:{0}.", info.Name);
            var phase = mContext.Phases.First(p => string.Equals(p.Name, info.Phase, StringComparison.OrdinalIgnoreCase));
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
                var field = GetCustomFieldByName(fieldInfo.Name);
                if (field != null)
                {
                    var scfci = new StageCustomFieldCreationInformation
                    {
                        Id = field.Id,
                        ReadOnly = fieldInfo.ReadOnly,
                        Required = fieldInfo.Required
                    };
                    scfcis.Add(scfci);
                }
                else
                {
                    mLogger.Warn("Cannot find the specified stage custom field for {0}. Name:{1}", info.Name, fieldInfo.Name);
                }
            }
            tci.CustomFields = scfcis;
            #endregion

            #region Set stage detail page
            var sdpcis = new List<StageDetailPageCreationInformation>(info.ProjectDetailPages.Count);
            foreach (var sdpInfo in info.ProjectDetailPages)
            {
                var sdpci = new StageDetailPageCreationInformation();
                sdpci.Description = sdpInfo.Description;
                var page = GetPageByName(sdpInfo.Name);
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
            
            var workflowStatusPage = GetPageByName(info.WorkflowStatusPage.Name);
            if (workflowStatusPage != null)
            {
                tci.WorkflowStatusPageId = workflowStatusPage.Id;
            }
            else
            {
                mLogger.Warn("Cannot find the specified workflow status page of {0}. Name:{1}", info.Name, info.WorkflowStatusPage.Name);
            }
            
            var stage = mContext.Stages.Add(tci);
            mContext.Stages.Update();
            mContext.Load(stage, s => s.Name);
            mContext.ExecuteQuery();
            return stage;
        }

        private ProjectDetailPage GetPageByName(string name)
        {
            return mContext.ProjectDetailPages.FirstOrDefault(p => string.Equals(p.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private CustomField GetCustomFieldByName(string name)
        {
            return mContext.CustomFields.FirstOrDefault(f => string.Equals(f.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        private Calendar GetCalendarByName(string name)
        {
            return mContext.Calendars.FirstOrDefault(c => string.Equals(c.Name, name, StringComparison.OrdinalIgnoreCase));
        }

        */

        #endregion

        public Dictionary<string, object> RestoreProject(AveProjectInfo info, AveProjectReader projectReader, AveRestoreMode conflictOption)
        {
            mLogger.Info("Start restoring project:[{0}]", info.Name);
            var result = new Dictionary<string, object>();
            if (info.NewId != Guid.Empty)
            {
                mPublishedProj = mContext.Projects.GetByGuid(info.NewId);
                mContext.Load(mPublishedProj);
                mContext.Load(mPublishedProj, p => p.IsCheckedOut, p => p.IsEnterpriseProject, p => p.TaskListId, p => p.Owner, p => p.ProjectSiteUrl);
                mContext.ExecuteQuery();
            }
            if (mPublishedProj == null)
            {
                CreateProject(info, projectReader);
                info.NewId = mPublishedProj.Id;
                info.NewSummaryTaskId = mPublishedProj.SummaryTaskId;
                info.IsNewCreated = true;
            }
            //如果存在tasklist，则创建出的project为sharepoint task类型，修改对应的feature后，会变为enterprise类型
            if (!mPublishedProj.IsEnterpriseProject
                && mProjectSite != null)
            {
                try
                {
                    mProjectSite.Features.Remove(AveProjectConstants.PWSVisibilityFeatureUid, true);
                    mProjectSite.Features.Add(AveProjectConstants.PWSManagedFeatureUid, true, FeatureDefinitionScope.Farm);
                    mProjectSite.Update();
                    mContext.ExecuteQuery();
                }
                catch (Exception e)
                {
                    mLogger.Error("switch sharepoint task to enterprise failed, error:{0}.", e);
                }
            }
            try
            {
                var draftProject = mPublishedProj.IsCheckedOut ? mPublishedProj.Draft : mPublishedProj.CheckOut();
                RestoreProjectProperties(draftProject, info);

                List<AveProjectTaskInfo> tasks = projectReader.GetPublishedTasks();
                if (this.mConfig.IncludePublishedTasks)
                {
                    RestoreProjectTasks(projectReader, draftProject, tasks);
                }
                PublishProject(draftProject, info.IsCheckedOut);

                if (!info.IsCheckedOut)
                {
                    //need reload?
                    draftProject = mPublishedProj.CheckOut();
                }

                bool changed = false;
                var draftInfo = projectReader.GetDraftProject();
                if (this.mConfig.IncludeDraftProjects)
                {
                    changed |= RestoreProjectProperties(draftProject, draftInfo);
                }

                var draftTasks = projectReader.GetDraftTasks();
                if (this.mConfig.IncludeDraftTasks)
                {
                    changed |= RestoreProjectTasks(projectReader, draftProject, draftTasks);
                }

                if (changed)
                {
                    draftProject.Update();
                    PublishProject(draftProject, info.IsCheckedOut);
                }
            }
            finally
            {
                //应根据源端是否为enterprise project来决定是否revert对应的feature
                if (!info.IsEnterpriseProject && mProjectSite != null)
                {
                    try
                    {
                        mProjectSite.Features.Remove(AveProjectConstants.PWSManagedFeatureUid, true);
                        mProjectSite.Features.Add(AveProjectConstants.PWSVisibilityFeatureUid, true, FeatureDefinitionScope.Farm);
                        mProjectSite.Update();
                        mContext.ExecuteQuery();
                    }
                    catch (Exception e)
                    {
                        mLogger.Error("revert enterprise to sharepoint task failed, error:{0}.", e);
                    }
                }
            }
            return result;
        }

        public void CreateProject(AveProjectInfo info, AveProjectReader projectDetails)
        {
            mLogger.Info("Start adding project:[{0}], Project Type Id:{1}", info.Name, info.EnterpriseProjectTypeId);
            List taskList = null;
            if (info.ProjectSiteInfo != null && !string.IsNullOrEmpty(info.TaskListTitle))
            {
                taskList = GetDestTaskList(info);
                if (taskList == null)
                {
                    string msg = string.Format("Cannot find project task list. Destination Task List title:{0}, project site url:{1}", info.TaskListTitle, info.ProjectSiteInfo.Url);
                    throw new Exception(msg);
                }
            }
            AddProject(info, taskList);
        }

        private List GetDestTaskList(AveProjectInfo projInfo)
        {
            List taskList = null;
            try
            {
                mProjectSite = mContext.Site.OpenWeb(projInfo.ProjectSiteInfo.Url);
                taskList = mProjectSite.Lists.GetByTitle(projInfo.TaskListTitle);
                mContext.Load(taskList);
                mContext.ExecuteQuery();
                if (taskList.BaseTemplate != (int)AveListTemplateType.TasksWithTimelineAndHierarchy)
                {
                    mLogger.Warn("task list has the same title but different template.");
                    return null;
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Cannot get destination task list using both id and title. Error:{0}", ex);
            }
            return taskList;
        }

        private void PublishProject(DraftProject draftProject, bool isCheckOut)
        {
            var publishJob = draftProject.Publish(!isCheckOut);
            WaitForJob(publishJob);
        }

        private void AddProject(AveProjectInfo info, List taskList)
        {
            var pci = new ProjectCreationInformation
            {
                Name = info.Name,
                Description = info.Description,
                EnterpriseProjectTypeId = info.EnterpriseProjectTypeId,
                Id = info.OriginalId,
                Start = info.StartDate,
            };
            if (taskList != null)
            {
                pci.TaskList = taskList;
            }
            mPublishedProj = mContext.Projects.Add(pci);
            mContext.Load(mPublishedProj, p => p.Id);
            var job = mContext.Projects.Update();
            WaitForJob(job);
            List<object> dataCache = AveAssemblyUtility.GetPropertyValue(mContext.Projects, "Data") as List<object>;
            if (dataCache != null)
            {
                dataCache.Clear();
            }
            //AOSBR-4746 当OriginalId为0时，使用add project新建出的Guid去获取project。
            DateTime startTime = DateTime.UtcNow;
            while (startTime.AddMinutes(5) > DateTime.UtcNow)
            {
                mPublishedProj = mContext.Projects.GetByGuid(mPublishedProj.Id);
                mContext.Load(mPublishedProj);
                mContext.Load(mPublishedProj, p => p.IsCheckedOut, p => p.Owner.Id, p => p.IsEnterpriseProject);
                mContext.ExecuteQuery();
                mLogger.Info("mPublishedProj {0} isCheckOut:{1}, mPublishedProj IsEnterpriseProject:{2}", info.Name,mPublishedProj.IsCheckedOut, mPublishedProj.IsEnterpriseProject);
                //默认使用API新创建出来的project都是check-in & published状态的
                if (mPublishedProj.IsCheckedOut || (taskList != null && mPublishedProj.IsEnterpriseProject)) //有个时候重新获取状态也不对，这里加一次retry
                {
                    Thread.Sleep(5 * 1000);
                    continue;
                }
                else
                {
                    break;
                }
            }
        }

        private bool RestoreProjectProperties(DraftProject draftProject, AveProjectInfo info)
        {
            bool changed = false;
            //owner 与 resource有关联，如果resource中的对应user还原失败，或者没有还原，此处设置时会抛CSOMUnkounUser异常
            //if (mPublishedProj.Owner.ServerObjectIsNull.HasValue && mPublishedProj.Owner.ServerObjectIsNull.Value || info.OwnerId != mPublishedProj.Owner.Id)
            //{
            //    var owner = mContext.Web.SiteUsers.GetById(info.OwnerId);
            //    draftProject.Owner = owner;
            //    changed = true;
            //}
            #region not support update
            //read-only
            //draftProject.WinprojVersion = info.WinprojVersion;
            //draftProject.CurrencyPosition 
            //draftProject.DefaultFixedCostAccrual 
            //draftProject.DefaultTaskType
            //draftProject.DefaultWorkFormat
            //draftProject.UtilizationType
            //draftProject.TrackingMode
            
            #endregion
            draftProject.CurrencyCode = info.CurrencyCode;
            draftProject.CurrencyDigits = info.CurrencyDigits;
            draftProject.Description = info.Description;
            draftProject.Name = info.Name;
            draftProject.ProjectIdentifier = info.ProjectIdentifier;
            draftProject.DaysPerMonth = info.DaysPerMonth;
            draftProject.DefaultEffortDriven = info.DefaultEffortDriven;
            draftProject.DefaultEstimatedDuration = info.DefaultEstimatedDuration;
            draftProject.DefaultOvertimeRate = info.DefaultOvertimeRate;
            draftProject.DefaultStandardRate = info.DefaultStandardRate;
            draftProject.FiscalYearStartMonth = info.FiscalYearStartMonth;
            draftProject.MinutesPerDay = info.MinutesPerDay;
            draftProject.MinutesPerWeek = info.MinutesPerWeek;
            draftProject.NewTasksAreManual = info.NewTasksAreManual;
            draftProject.NumberFiscalYearFromStart = info.NumberFiscalYearFromStart;
            draftProject.ProtectedActualsSynch = info.ProtectedActualsSynch;
            draftProject.ShowEstimatedDurations = info.ShowEstimatedDurations;
            draftProject.WeekStartDay = info.WeekStartDay;
            if (info.CurrentDate != DateTime.MinValue)
            {
                draftProject.CurrentDate = info.CurrentDate;
            }
            if (info.StatusDate != DateTime.MinValue)
            {
                draftProject.StatusDate = info.StatusDate;
            }
            if (info.UtilizationDate != DateTime.MinValue)
            {
                draftProject.UtilizationDate = info.UtilizationDate;
            }
            if (info.FinishDate != DateTime.MinValue)
            {
                draftProject.FinishDate = info.FinishDate;
            }
            if(info.StartDate != DateTime.MinValue)
            {
                draftProject.StartDate = info.StartDate;
            }
            foreach (KeyValuePair<string, object> pair in info.FieldValues)
            {
                draftProject[pair.Key] = pair.Value;
            }
            changed = true;

            var projectUpdateJob = draftProject.Update();
            WaitForJob(projectUpdateJob);
            return changed;
        }

        private bool RestoreProjectTasks(AveProjectReader reader, DraftProject draftProject, List<AveProjectTaskInfo> tasks)
        {
            if (tasks.Count == 0) return false;

            bool changed = false;
            foreach (var taskInfo in tasks)
            {
                if(taskInfo.IsActive)
                {
                    try
                    {
                        mLogger.Info("restore task name:{0}, task id:{1}", taskInfo.Name, taskInfo.Id);
                        taskInfo.StatusManagerId = reader.FindUser(taskInfo.StatusManagerId);
                        DraftTask task = null;
                        task = GetTaskById(draftProject, taskInfo.Id);
                        if (task != null)
                        {
                            changed |= RestoreTaskProperties(task, taskInfo, false);
                            draftProject.Update();
                            continue;
                        }
                        else
                        {
                            AddTask(draftProject, taskInfo);
                            changed = true;
                        }
                        mLogger.Info("finish restore task name:{0}, task id:{1}", taskInfo.Name, taskInfo.Id);
                    }
                    catch (Exception ex)
                    {
                        mLogger.Error("Restore project task failed. Task Name:{0}. Error:{1}", taskInfo.Name, ex);
                    }
                }
                
            }

            return changed;
        }

        private DraftTask GetTaskById(DraftProject draftProject, Guid id)
        {
            try
            {
                var task = draftProject.Tasks.GetByGuid(id);
                mContext.Load(task);
                mContext.ExecuteQuery();
                if (task != null && task.ServerObjectIsNull.HasValue
                    && !task.ServerObjectIsNull.Value)
                {
                    return task;
                }
            }
            catch (Exception ex)
            {
                mLogger.Warn("Cannot get the specified task by id. Id:{0}. Error:{1}", id.ToString(), ex);
            }
            return null;
        }

        private DraftTask AddTask(DraftProject draftProject, AveProjectTaskInfo taskInfo)
        {
            var tci = new TaskCreationInformation
            {
                //AddAfterId = 
                //Duration = taskInfo.Duration,
                //Start = taskInfo.Start,
                //Finish = taskInfo.Finish,
                Id = taskInfo.Id,
                IsManual = taskInfo.IsManual,
                Name = taskInfo.Name,
                Notes = taskInfo.Notes,
                //StatusManager = mContext.Web.SiteUsers.GetById(taskInfo.StatusManagerId),
            };
            if (taskInfo.ActualStart != DateTime.MinValue)
            {
                tci.Start = taskInfo.ActualStart;
            }
            else
            {
                tci.Start = taskInfo.Start;
            }
            if (taskInfo.ActualFinish != DateTime.MinValue)
            {
                tci.Finish = taskInfo.ActualFinish;
            }
            else
            {
                tci.Finish = taskInfo.Finish;
            }

            if (taskInfo.ParentId != Guid.Empty)
            {
                tci.ParentId = taskInfo.ParentId;
            }
            var newTask = draftProject.Tasks.Add(tci);

            RestoreTaskProperties(newTask, taskInfo, true);
            draftProject.Update();

            return newTask;
        }

        private bool RestoreTaskProperties(DraftTask task, AveProjectTaskInfo info, bool isNewAddTask)
        {
            bool changed = false;
            if (!isNewAddTask)
            {
                //if (info.StatusManagerId != 0)
                //{
                //    var manager = mContext.Web.SiteUsers.GetById(info.StatusManagerId);
                //    task.StatusManager = manager;
                //}
                task.Name = info.Name;
                if (info.ActualStart != DateTime.MinValue)
                {
                    task.ActualStart = info.ActualStart;
                }
                else
                {
                    task.Start = info.Start;
                }
                if (info.ActualFinish != DateTime.MinValue)
                {
                    task.ActualFinish = info.ActualFinish;
                }
                else
                {
                    task.Finish = info.Finish;
                }
                task.IsManual = info.IsManual;
                changed = true;
            }
            if (Math.Abs(info.ActualCost - 0.0d) > 0)
            {
                task.ActualCost = info.ActualCost;
            }
            else
            {
                task.Cost = info.Cost;
            }
            if (!string.Equals(info.Work, "0h"))
            {
                task.Work = info.Work;
            }
            else
            {
                task.ActualWork = info.ActualWork;
            }

            task.ActualWorkTimeSpan = info.ActualWorkTimeSpan;
            task.LevelingAdjustsAssignments = info.LevelingAdjustsAssignments;
            task.LevelingCanSplit = info.LevelingCanSplit;
            task.OutlineLevel = info.OutlineLevel;
            task.PercentPhysicalWorkComplete = info.PercentPhysicalWorkComplete;
            task.Priority = info.Priority;
            //task.RemainingDuration = info.RemainingDuration;
            //task.RemainingDurationTimeSpan = info.RemainingDurationTimeSpan;
            //task.StartText = info.StartText;//StartText为null时，更新抛unknown error
            task.UsePercentPhysicalWorkComplete = info.UsePercentPhysicalWorkComplete;
            
            //task.WorkTimeSpan = info.WorkTimeSpan;
            task.PercentComplete = info.PercentComplete;
            
            task.Deadline = info.Deadline;
            task.Duration = info.Duration;
            task.DurationTimeSpan = info.DurationTimeSpan;
            //task.FinishText = info.FinishText;
            task.FixedCost = info.FixedCost;
            task.IsActive = info.IsActive;
            task.IsLockedByManager = info.IsLockedByManager;
            task.IsMarked = info.IsMarked;
            task.IsMilestone = info.IsMilestone;
            #region not support update
            //readonly
            //task.BudgetWork = info.BudgetWork;
            //task.BudgetWorkTimeSpan = info.BudgetWorkTimeSpan;
            //task.Calendar
            //readonly
            //task.Completion = info.Completion;
            //task.ConstraintStartEnd = info.ConstraintStartEnd;
            //task.ConstraintType
            //task.TaskType
            #endregion
            changed = true;
            return changed;
        }
    }
}
