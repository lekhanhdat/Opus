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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.FunctionSetting;
using AvePoint.RA.Contract.ManualApproval;
using AvePoint.RA.Contract.ManualApproval.Enums;
using AvePoint.RA.Contract.ManualApproval.Model;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Extension;
using AvePoint.RA.DB.Dao.Impl;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.DB.Explorer.Dao.CosmosImp;
using AvePoint.RA.DB.SecurityTrimming;
using AvePoint.RA.Service.Services.AccountManager;
using AvePoint.RA.Service.Services.ManualApproval.Model;
using AvePoint.RA.Service.Services.ManualApproval.Queriers;
using DocumentFormat.OpenXml.InkML;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace AvePoint.RA.Web.Common.Filters
{
    public class ValidManualApprovalParameterFilter : BaseActionFilterAsync
    {

        private static RALogger logger = RALogger.GetInstance(typeof(ValidManualApprovalParameterFilter));
        private static IRMFunctionSettingDao FunctionSettingDao => PlatformWindsorManager.GetService<IRMFunctionSettingDao>();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private static IRMManualApprovalService ManualApprovalService => PlatformWindsorManager.GetService<IRMManualApprovalService>();

        private static IOneDriveSettingDao OneDriveSettingDao => PlatformWindsorManager.GetService<IOneDriveSettingDao>();

        private static IEXOSettingDao EXOSettingDao => PlatformWindsorManager.GetService<IEXOSettingDao>();

        private static IRMGoogleSettingDao RMGoogleSettingDao => PlatformWindsorManager.GetService<IRMGoogleSettingDao>();

        private static ITenantService TenantService => PlatformWindsorManager.GetService<ITenantService>();

        private static IRMKeyValueDao RMKeyValueDao => PlatformWindsorManager.GetService<IRMKeyValueDao>();

        private static IRMFileSystemSettingsService RMFileSystemSettingsService => PlatformWindsorManager.GetService<IRMFileSystemSettingsService>();

        private static IUserService UserService => PlatformWindsorManager.GetService<IUserService>();

        private static IAccountDao AccountDao => PlatformWindsorManager.GetService<IAccountDao>();

        private static IRMSecurityTrimmingHelper SecurityTrimmingHelper => PlatformWindsorManager.GetService<IRMSecurityTrimmingHelper>();

        private static ManualApprovalRecordRepository ManualApprovalRecordRepository => new ManualApprovalRecordRepository();

        private static readonly int MaxProcessItemLimit = 5000;

        private static readonly int MaxCharacterLimit = 20000;
        public ManualApprovalActionType ActionType { get; set; }

        public ValidManualApprovalParameterFilter(ManualApprovalActionType actionType)
        {
            ActionType = actionType;
        }

        public override async Task OnActionExecutionAsync(ActionExecutingContext actionContext, ActionExecutionDelegate next)
        {
            bool validateRes;
            switch (ActionType)
            {
                case ManualApprovalActionType.Escalate:
                case ManualApprovalActionType.Reassign:
                    validateRes = await EsclateOrReassignValid(actionContext);
                    break;
                case ManualApprovalActionType.Approve:
                case ManualApprovalActionType.Reject:
                    validateRes = await ApproveOrRejectValid(actionContext, ActionType);
                    break;
                case ManualApprovalActionType.Extend:
                    validateRes = await ExtendValid(actionContext);
                    break;
                case ManualApprovalActionType.RestoreExtend:
                    validateRes = await RestoreExtendValid(actionContext);
                    break;
                case ManualApprovalActionType.ChangeDisposalAction:
                    validateRes = await ChangeDisposalActionValid(actionContext);
                    break;
                case ManualApprovalActionType.ResetManualWorkflow:
                    validateRes = await ResetManualWorkflow(actionContext);
                    break;
                case ManualApprovalActionType.UpdateSetting:
                    {
                        var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
                        if (parameter is not ManualApprovalSettings settings)
                        {
                            actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                            validateRes = false;
                            break;
                        }
                        validateRes = UpdateSettingValid(settings, actionContext);
                    }
                    break;
                case ManualApprovalActionType.SaveConfigOption:
                    {
                        var parameter = actionContext.ActionArguments.Values.First() as ManualApprovalCommentInfos;
                        validateRes = SaveConfigOption(parameter, actionContext);
                    }
                    break;
                case ManualApprovalActionType.SaveApprovalSetting:
                    validateRes = SaveApprovalSetting(actionContext);
                    break;
                case ManualApprovalActionType.ChangeTerm:
                    validateRes = await ChangeTermValidate(actionContext);
                    break;
                default:
                    actionContext.Result = new ObjectResult("Illegal Operation") { StatusCode = (int)HttpStatusCode.Forbidden };
                    validateRes = false;
                    break;
            }

            if(validateRes)
            {
                await next();
            }
        }

        private bool SaveApprovalSetting(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not ManualApprovalSettingInfo settings)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }
            return UpdateSettingValid(settings.ApprovalProcessSetting, actionContext) && SaveConfigOption(settings.CommentSettingInfo, actionContext);
        }

        private async Task<bool> EsclateOrReassignValid(ActionExecutingContext actionContext)
        {

            if(ActionType == ManualApprovalActionType.Escalate && await ManualApprovalService.DisabledEscalateAsync())
            {
                actionContext.Result = new ObjectResult("Illegal Operation") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not ManualAprovalEscalateDefinition definition || definition.ItemIds?.Count == 0 || definition.ToUsers?.Count == 0)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var itemIds = definition.ItemIds;
            if (itemIds.Count > MaxProcessItemLimit)
            {
                actionContext.Result = new ObjectResult("Limit Exceeded") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var queryDefinition = new ManualApprovalQueryDefinition
            {
                PageSize = 100,
                NeedCalculationCount = false,
            };
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ItemId,
                Value = JsonConvert.SerializeObject(itemIds)
            });
            var count = await ManualApprovalQuerier.CountAsync(queryDefinition);
            if (count != itemIds.Count)
            {
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static async Task<bool> ApproveOrRejectValid(ActionExecutingContext actionContext, ManualApprovalActionType actionType)
        {
            var parameter = actionContext.ActionArguments.Values.First() as ManualApprovalActionParams;
           
            if (parameter.ManualFromTab == ManualApprovalTab.UnderReview)
            {
                //check quick reason
                var inputQuickReason = parameter.QuickReason;
                var QuickReasonSettingInfo = FunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualApprovalCommentSetting).GetAwaiter().GetResult();
                var QucikInfo = SerializerHelper.DeserializeByJsonConvert<ManualApprovalCommentSetting>(QuickReasonSettingInfo);
                if (inputQuickReason != string.Empty && actionType == ManualApprovalActionType.Reject)
                {
                    if (!QucikInfo.ManualApprovalQuickReasonInfo.NeedQuickReason)
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return false;
                    }
                    if (inputQuickReason.Length > 255)
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return false;
                    }
                    if (!QucikInfo.ManualApprovalQuickReasonInfo.QuickReasonInfo.Contains(inputQuickReason))
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return false;
                    }
                    if (QucikInfo.ManualApprovalQuickReasonInfo?.IncativeRejectBool != null)
                    {
                        List<string> quickReasonInfoList = QucikInfo.ManualApprovalQuickReasonInfo.QuickReasonInfo.ToList();
                        List<bool> incativeRejectBoolList = QucikInfo.ManualApprovalQuickReasonInfo.IncativeRejectBool.ToList();
                        List<string> result = new List<string>();
                        for (int i = 0; i < incativeRejectBoolList.Count; i++)
                        {
                            if (!incativeRejectBoolList[i])
                            {
                                result.Add(quickReasonInfoList[i]);
                            }
                        }
                        if (!result.Contains(inputQuickReason))
                        {
                            actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                            return false;
                        }
                    }
                }
                if (inputQuickReason == string.Empty && actionType == ManualApprovalActionType.Reject && QucikInfo.ManualApprovalQuickReasonInfo.NeedQuickReason)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
                if (actionType == ManualApprovalActionType.Approve && !string.IsNullOrEmpty(parameter.QuickReason))
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
                //check comment Option
                var inputCommentOption = parameter.ApprovalComment.Trim();
                var ManualApproveSettingInfo = FunctionSettingDao.GetSettingInfo(FunctionSettingType.ManualApprovalCommentOption).GetAwaiter().GetResult(); //  1.Approve Reject 必須     2.Approve  必須    3.Reject必須    4. 都可以填或者不填
                if (ManualApproveSettingInfo.Equals("1") && inputCommentOption == string.Empty)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
                if (ManualApproveSettingInfo.Equals("2") && actionType == ManualApprovalActionType.Approve && inputCommentOption == string.Empty)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
                if (ManualApproveSettingInfo.Equals("3") && actionType == ManualApprovalActionType.Reject && inputCommentOption == string.Empty)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
                //check extend type and extend time
                if (actionType == ManualApprovalActionType.Reject)
                {
                    var inputExtendType = parameter.ExtendType;
                    var inputCustomeExtendDate = parameter.CustomeExtendDate;

                    var settingJson = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.ManualSetting);
                    var setting = JsonConvert.DeserializeObject<ManualApprovalSettings>(settingJson);   // 120 month    10 year 
                    var latestExtendNumber = setting.DisposalExtentionSetting.LatestExtendNumber;
                    var lastetExtendTypeNumber = setting.DisposalExtentionSetting.LatestExtendType switch
                    {
                        ManualApprovalExtendType.Month => 1,
                        ManualApprovalExtendType.Year => 12,
                        ManualApprovalExtendType.After1Year => 12, 
                        ManualApprovalExtendType.After1Month => 1,
                        ManualApprovalExtendType.After3Month => 3,
                        ManualApprovalExtendType.After6Month => 6,
                        _ => 0.5
                    };
                    var intputExtendTypeNumber  = inputExtendType switch
                    {
                        ManualApprovalExtendType.After1Month => 1,
                        ManualApprovalExtendType.After3Month => 3,
                        ManualApprovalExtendType.After6Month => 6,
                        ManualApprovalExtendType.After1Year => 12,
                        ManualApprovalExtendType.Custom => 120,
                        _ => 0.5
                    };
                    //是否比数据库存入的大
                    if ((intputExtendTypeNumber > latestExtendNumber * lastetExtendTypeNumber ||
                            inputExtendType <= ManualApprovalExtendType.None) &&
                            inputExtendType != ManualApprovalExtendType.Custom)
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return false;
                    }

                    //判断custom最大的选择时间
                    if (inputExtendType == ManualApprovalExtendType.Custom)
                    {
                        var maxDateTime = DateTime.UtcNow.AddMonths((int)lastetExtendTypeNumber * latestExtendNumber);
                        if (inputCustomeExtendDate > maxDateTime)
                        {
                            actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                            return false;
                        }
                    }
                }
            }                       
            if (parameter.NeedActionIds is not List<Guid> itemIds || itemIds?.Count == 0)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if (itemIds.Count > MaxProcessItemLimit)
            {
                actionContext.Result = new ObjectResult("Limit Exceeded") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static async Task<bool> ExtendValid(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not ManualApprovalExtendDefinition definition || definition.ItemIds?.Count == 0)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var settingJson = await FunctionSettingDao.GetSettingInfo(Contract.FunctionSetting.FunctionSettingType.ManualSetting);
            var setting = JsonConvert.DeserializeObject<ManualApprovalSettings>(settingJson);

            if ((definition.ExtendType > setting.DisposalExtentionSetting.LatestExtendType || 
                definition.ExtendType <= ManualApprovalExtendType.None) && 
                definition.ExtendType != ManualApprovalExtendType.Custom)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            DateTime CalculationMaxExtendTime()
            {
                var now = DateTime.UtcNow;
                if (setting.DisposalExtentionSetting.LatestExtendType == ManualApprovalExtendType.After3Month)
                {
                    return now.AddMonths(3);
                }
                else if (setting.DisposalExtentionSetting.LatestExtendType == ManualApprovalExtendType.After6Month)
                {
                    return now.AddMonths(6);
                }
                else if (setting.DisposalExtentionSetting.LatestExtendType == ManualApprovalExtendType.After1Year)
                {
                    return now.AddYears(1);
                }

                return DateTime.UtcNow;
            }

            if (definition.ExtendType == ManualApprovalExtendType.Custom)
            {
                var customeDateTime = await GeneralSettingService.ConvertDateTimeToUtcAsync(definition.CustomeExtendDate);
                var maxDateTime = CalculationMaxExtendTime();
                if(customeDateTime > maxDateTime)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
            }

            var itemIds = definition.ItemIds;
            if (itemIds.Count > MaxProcessItemLimit)
            {
                actionContext.Result = new ObjectResult("Limit Exceeded") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var queryDefinition = new ManualApprovalQueryDefinition
            {
                PageSize = 100,
                NeedCalculationCount = false,
            };
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ItemId,
                Value = JsonConvert.SerializeObject(itemIds)
            });
            var items = (await ManualApprovalQuerier.CosmosDBQueryAsync(queryDefinition)).Items;
            if (items.Count != itemIds.Count)
            {
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if(items.Any(item => item.ExtendCount >= setting.DisposalExtentionSetting.MaxDelayTimes))
            {
                actionContext.Result = new ObjectResult("Illegal Operation") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static async Task<bool> RestoreExtendValid(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not List<Guid> itemIds || itemIds.Count == 0)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if (itemIds.Count > MaxProcessItemLimit)
            {
                actionContext.Result = new ObjectResult("Limit Exceeded") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var queryDefinition = new ManualApprovalQueryDefinition
            {
                PageSize = 100,
                NeedCalculationCount = false,
            };
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Rejected , SOApproveDBStatus.Approved , SOApproveDBStatus.WaitingApprove })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "true"
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ItemId,
                Value = JsonConvert.SerializeObject(itemIds)
            });
            var count = await ManualApprovalQuerier.CountAsync(queryDefinition);
            if (count != itemIds.Count)
            {
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static async Task<bool> ChangeDisposalActionValid(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not ManualApprovalRelatedRecordsDisposalDefinition definition || definition.ItemIds?.Count == 0)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var itemIds = definition.ItemIds;
            if (itemIds.Count > MaxProcessItemLimit)
            {
                actionContext.Result = new ObjectResult("Limit Exceeded") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var queryDefinition = new ManualApprovalQueryDefinition
            {
                PageSize = 100,
                NeedCalculationCount = false,
            };
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.WaitingApprove })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ExtendTime,
                Value = "false"
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.IsRelatedRecords,
                Value = "true"
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ItemId,
                Value = JsonConvert.SerializeObject(itemIds)
            });
            var count = await ManualApprovalQuerier.CountAsync(queryDefinition);
            if (count != itemIds.Count)
            {
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static async Task<bool> ResetManualWorkflow(ActionExecutingContext actionContext)
        {
            var parameter = actionContext.ActionArguments.Values.FirstOrDefault();
            if (parameter is not List<Guid> itemIds || itemIds?.Count == 0)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if (itemIds.Count > MaxProcessItemLimit)
            {
                actionContext.Result = new ObjectResult("Limit Exceeded") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            var queryDefinition = new ManualApprovalQueryDefinition
            {
                PageSize = 100,
                NeedCalculationCount = false,
            };
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ApprovalStatus,
                Value = JsonConvert.SerializeObject(new List<SOApproveDBStatus> { SOApproveDBStatus.Approved, SOApproveDBStatus.Rejected })
            });
            queryDefinition.Filters.Add(new ManualApprovalFilterDefinition
            {
                FilterOption = ManualApprovalFilterOptions.ItemId,
                Value = JsonConvert.SerializeObject(itemIds)
            });
            var count = await ManualApprovalQuerier.CountAsync(queryDefinition);
            if (count != itemIds.Count)
            {
                actionContext.Result = new ObjectResult("Access Denied") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static bool UpdateSettingValid(ManualApprovalSettings settings, ActionExecutingContext actionContext)
        {
            bool EmailNotificationSettingValid()
            {
                var emailNotification = settings.EmailNotificationSetting;
                if (emailNotification == null)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }

                if (emailNotification.Interval < 1 || emailNotification.Interval > 100)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }

                if (emailNotification.IntervalType <= ManualApprovalIntervalType.None || emailNotification.IntervalType > ManualApprovalIntervalType.Weeks)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }

                if (emailNotification.EndType <= ManualApprovalEndType.None || emailNotification.EndType > ManualApprovalEndType.EndOccurrences)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }

                if (emailNotification.EndType == ManualApprovalEndType.EndOccurrences && (emailNotification.OccurrencesTimes < 1 || emailNotification.OccurrencesTimes > 100))
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }

                return true;
            }

            bool EscalationSettingValid()
            {
                var escalationSetting = settings.EscalationSetting;
                if (escalationSetting.EscalateSettingType <= ManualApprovalEscalateSettingType.None || escalationSetting.EscalateSettingType > ManualApprovalEscalateSettingType.NoAction)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }

                if (escalationSetting.EscalateSettingType == ManualApprovalEscalateSettingType.WorkflowNextStep && (escalationSetting.ApprovalStatus != SOApproveDBStatus.Approved && escalationSetting.ApprovalStatus != SOApproveDBStatus.Rejected))
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }

                if (escalationSetting.EscalateSettingType == ManualApprovalEscalateSettingType.ReassignSpecificUsers)
                {
                    var reassignUsers = escalationSetting.ReassignUsers;
                    if (reassignUsers == null || reassignUsers.Count == 0)
                    {
                        actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                        return false;
                    }
                }

                return true;
            }

            bool ExtendDisposalValid()
            {
                var extendDisposal = settings.DisposalExtentionSetting;
                if (extendDisposal.MaxDelayTimes < 1 || extendDisposal.MaxDelayTimes > 10)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
                if (extendDisposal.LatestExtendNumber < 1 || extendDisposal.LatestExtendNumber > 120)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
                if (extendDisposal.LatestExtendType <= ManualApprovalExtendType.None || extendDisposal.LatestExtendType > ManualApprovalExtendType.After1Month)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }
                if (extendDisposal.LatestExtendNumber > 10 && extendDisposal.LatestExtendType == ManualApprovalExtendType.Year)
                {
                    actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                    return false;
                }

                return true;
            }

            if (!EscalationSettingValid())
            {
                return false;
            }

            if (!EmailNotificationSettingValid())
            {
                return false;
            }

            if (!ExtendDisposalValid())
            {
                return false;
            }

            return true;
        }

        private static bool SaveConfigOption(ManualApprovalCommentInfos parameter, ActionExecutingContext actionContext)
        {
            if (!Enum.IsDefined(typeof(ManualApprovalCommentOptions), parameter.Option))
            {
                return false;
            }

            var ManualApprovalModifiedButtonNamesList = parameter.ModifyButtonName.ManualApprovalModifyButton.ModifiedButtonNames;
            if (ManualApprovalModifiedButtonNamesList.Count != 2)
            {
                return false;
            }
            var approvalList = ManualApprovalModifiedButtonNamesList[0];
            var rejectList = ManualApprovalModifiedButtonNamesList[1];

            var approvalNames = new[] { approvalList.ChineseName, approvalList.EnglishName, approvalList.JapaneseName };
            var rejectNames = new[] { rejectList.ChineseName, rejectList.EnglishName, rejectList.JapaneseName };

            if (approvalNames.Any(name => name == null || name.Trim() == string.Empty) ||
                rejectNames.Any(name => name == null || name.Trim() == string.Empty))
            {
                return false;
            }

            var quickReasonInfo = parameter.CommentSetting.ManualApprovalQuickReasonInfo.QuickReasonInfo;
            if (quickReasonInfo.Count > 1 && quickReasonInfo.Any(s => string.IsNullOrEmpty(s.Trim()) || s.Length > 255))
            {
                return false;
            }

            var duration = parameter.Duration;
            if (duration < 1 || duration > 366)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static async Task<bool> ChangeTermValidate(ActionExecutingContext actionContext)
        {
            if (actionContext.ActionArguments.Values.FirstOrDefault() is not ChangeTermDto dto)
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if (string.IsNullOrWhiteSpace(dto.Comment))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if (RMKeyValueDao.IsEnableJPMCFileSystemFeature())
            {
                logger.Warn($"This feature is not available for JPMC");
                actionContext.Result = new ObjectResult("Invalid permission") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if(!TenantService.IsNewOpusTenant())
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if (await IsDisableClassificationByOpus(dto))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if(!ValidFSFolderClassification(dto))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            if(!await ValidReviewerUser(dto))
            {
                actionContext.Result = new ObjectResult("Illegal Parameter") { StatusCode = (int)HttpStatusCode.Forbidden };
                return false;
            }

            return true;
        }

        private static async Task<bool> ValidReviewerUser(ChangeTermDto dto)
        {
            try
            {
                var isAdmin = await SecurityTrimmingHelper.DoesUserHasThisPermissionAsync(Contract.RoleAssignments.RMPermissionMasks.ManualReviewAdmin);

                if (isAdmin) return true;

                var reviewerIds = UserService.GetUserWithRemovedAndGroupIds(TenantLocalValue.LogonUserId);
                var accounts = await AccountDao.GetUserWithRemovedByIds(reviewerIds);
                var userPrincipalNames = accounts.Select(item => item.UserPrincipalName);
                accounts = AccountDao.GetUserWithRemovedByPrincipalNames(userPrincipalNames);
                reviewerIds = accounts.Select(item => item.Id).ToList();
                IExplorerDao explorerDao = new ExplorerDao();
                List<Guid> recordIds = new List<Guid>();
                recordIds.AddRange(dto.OneDriveRecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.EXORecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.GoogleDriveRecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.SPOnPremRecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.RecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.BoxRecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.TeamsRecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.FSRecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.PhyRecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.AzureFileShareRecordIds ?? new List<Guid>());
                recordIds.AddRange(dto.CustomizeConnectorRecordIds ?? new List<Guid>());
                var records = explorerDao.GetRecordByIds(recordIds);
                bool isPassCurrentRecord = false;
                foreach(var record in records)
                {
                    isPassCurrentRecord = false;
                    foreach(var reviewerId in reviewerIds)
                    {
                        if(record.ManualReviewer.Contains(reviewerId))
                        {
                            isPassCurrentRecord = true;
                            break;
                        }
                    }
                    if (isPassCurrentRecord) continue;
                    return false;
                }
                return true;
            }
            catch(Exception e)
            {
                logger.Error($"Vaid reviewer has errors: {e}");
                return false;
            }

        }

        private static bool ValidFSFolderClassification(ChangeTermDto dto)
        {
            if(dto.FSRecordIds?.Any() == true)
            {
                var fsLevel = RMFileSystemSettingsService.GetClassificationLevel();
                return (int)NodeLevel.FSFolder != fsLevel;
            }
            return true;
        }

        private static async Task<bool> IsDisableClassificationByOpus(ChangeTermDto dto)
        {
            if (dto.OneDriveRecordIds?.Any() == true && await IsODDisableClassification(dto.OneDriveRecordIds)) return true;

            if (dto.EXORecordIds?.Any() == true && await IsEXODisableClassification(dto.EXORecordIds)) return true;

            if (dto.GoogleDriveRecordIds?.Any() == true && await IsGoogleDisableClassification(dto.GoogleDriveRecordIds)) return true;

            return false;
        }

        private static Task<bool> IsODDisableClassification(List<Guid> ids) => IsDisableClassification(ids, containerId =>
        {
            var setting = OneDriveSettingDao.GetSettingInfoByScope(containerId, Guid.Empty, containerId);

            return setting == null || setting.IsNullClassificationSetting;
        });

        private static Task<bool> IsEXODisableClassification(List<Guid> ids) => IsDisableClassification(ids, containerId =>
        {
            var setting = EXOSettingDao.GetSettingInfoByScope(containerId, Guid.Empty, containerId);

            return setting == null || setting.IsNullClassificationSetting;
        });

        private static Task<bool> IsGoogleDisableClassification(List<Guid> ids) =>  IsDisableClassification(ids, containerId =>
        {
            var setting = RMGoogleSettingDao.GetSettingInfoByScope(containerId, containerId, Guid.Empty);

            return setting == null || setting.IsNullClassificationSetting;
        });

        private static async Task<bool> IsDisableClassification(List<Guid> ids, Func<Guid, bool> isDisableByContainer)
        {
            if (ids == null || ids.Count == 0) return false;

            var containerIds = (await ManualApprovalRecordRepository.QueryItemsAsync(record => ids.Contains(record.Id))).Select(item => item.ContainerId).Distinct();

            foreach (var containerId in containerIds)
            {
                if (!Guid.TryParse(containerId, out var guidContainerId)) continue;

                if (isDisableByContainer(guidContainerId)) return true;
            }

            return false;
        }
    }
}