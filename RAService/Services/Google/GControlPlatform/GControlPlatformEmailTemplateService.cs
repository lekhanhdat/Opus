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
using System.Threading.Tasks;
using AvePoint.RA.Contract.Google.GControlPlatform;
using AvePoint.RA.Contract.RMWeb.CP;
using Cloud.Sdk.Data.Nexus.Common;
using Cloud.Sdk.Data.Nexus.Foundation;
using EmailTemplateType = Cloud.Sdk.Data.Nexus.Foundation.EmailTemplateType;

namespace AvePoint.RA.Service.Services.Google.GControlPlatform;

public class GControlPlatformEmailTemplateService : GControlPlatformBaseService, IGControlPlatformEmailTemplateService
{
    public async Task<EmailTemplateDto> GetEmailTemplate(Guid id)
    {
        Logger.Info($"GetEmailTemplate: Fetching template with ID {id}.");
        return ConvertToEmailTemplateDto(await GControlPlatformApiClient.EmailTemplateService.GetEmailTemplate(id));
    }

    public async Task<List<EmailTemplateDto>> SearchEmailTemplate(CommonRequest request)
    {
        var templates = await GControlPlatformApiClient.EmailTemplateService.SearchEmailTemplate(request);
        Logger.Debug($"SearchEmailTemplate: {templates.Count} templates found.");

        return templates.ConvertAll(ConvertToEmailTemplateDto);
    }

    public async Task<List<EmailTemplateDto>> GetEmailTemplateByType(EmailTemplateType type)
    {
        var templates = await GControlPlatformApiClient.EmailTemplateService.GetEmailTemplateByType(type);
        Logger.Debug($"GetEmailTemplate: {templates.Count} templates found.");

        return templates.ConvertAll(ConvertToEmailTemplateDto);
    }

    private EmailTemplateDto ConvertToEmailTemplateDto(EmailTemplate template)
    {
        if (template == null)
        {
            Logger.Warn("ConvertToEmailTemplateDto: Received null template.");
            return null;
        }

        return new EmailTemplateDto
        {
            UniqueId = template.TemplateId,
            Name = template.Name,
            Type = 3,
            Subject = template.Name,
            IsNewTemplate = false,
            IsCustomTemplate = !template.IsBuiltIn,
        };
    }
}