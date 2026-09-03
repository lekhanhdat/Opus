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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Google;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Model;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace AvePoint.RA.Api.Web.Controllers.GoogleOne;

[Route("api/googleone/nodesettings")]
public class GoogleOneNodeSettingsApiController : GoogleOneApiBaseController
{
    private static RALogger s_logger = RALogger.GetInstance(typeof(GoogleOneNodeSettingsApiController));
    private IRMGoogleSettingsService SettingManagerService => PlatformWindsorManager.GetService<IRMGoogleSettingsService>();
    private IManualProcessManagementService ManualProcessManagementService => PlatformWindsorManager.GetService<IManualProcessManagementService>();
    private IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

    [HttpPost("load")]
    public async Task<string> LoadGoogleNodeSettings([FromBody] RMSampleGoogleTreeNode currentNode)
    {
        try
        {
            var settings = await SettingManagerService.LoadGoogleNodeSettingsAsync(currentNode);
            ScheduleService.ConvertScheduleByTimezone(settings.DisposeScheduleInfo);
            if (settings.ApprovalType == (int)ApprovalType.ApprovalProcess &&
                Guid.TryParse(settings.WorkflowReferenceId, out var referenceId))
            {
                var workflow = ManualProcessManagementService.GetWorkflow(referenceId);
                settings.WorkflowReferenceName = workflow?.Name;
            }
            return JsonConvert.SerializeObject(settings);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to load Google node settings for node [{currentNode?.FullPath}]. Ex: {ex.Message}.", ex);
            return ex.Message;
        }
    }

    [HttpPost("inherit")]
    public async Task<string> InheritParentSetting([FromBody] RMGoogleTreeNode curSetting)
    {
        try
        {
            var result = await SettingManagerService.InheritParentSettingAsync(curSetting);
            return result.ErrorMessage;
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to inherit parent settings for node [{curSetting?.FullPath}]. Ex: {ex.Message}.", ex);
            throw;
        }
    }

    [HttpPost("inherit/bulk")]
    public async Task<string> InheritParentSettings([FromBody] List<RMGoogleTreeNode> settings)
    {
        try
        {
            return await SettingManagerService.BulkInheritParentSettingAsync(settings);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to inherit parent settings for multiple nodes. Ex: {ex.Message}.", ex);
            return ex.Message;
        }
    }

    [HttpPost("save")]
    public async Task<string> SaveLabelSetting([FromBody] RMGoogleTreeNode curSetting)
    {
        try
        {
            return await SettingManagerService.AddGoogleNodeSettingsAsync(curSetting);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to save Google node settings for node [{curSetting?.FullPath}]. Ex: {ex.Message}.", ex);
            return ex.Message;
        }
    }

    [HttpPost("save/bulk")]
    public async Task<string> SaveLabelSettings([FromBody] List<RMGoogleTreeNode> settings)
    {
        try
        {
            return await SettingManagerService.BulkAddGoogleNodeSettingsAsync(settings);
        }
        catch (Exception ex)
        {
            s_logger.Error($"Failed to save Google node settings for multiple nodes. Ex: {ex.Message}.", ex);
            return ex.Message;
        }
    }
}