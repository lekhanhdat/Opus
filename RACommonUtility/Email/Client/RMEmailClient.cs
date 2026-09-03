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
using AvePoint.GCommon.Utility.I18N;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.CP;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Email.Client.Compilers;
using AvePoint.RA.RACommonUtility.Email.Client.Config;
using AvePoint.RA.RACommonUtility.Email.Client.Server;
using AvePoint.RA.RACommonUtility.Email.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Email.Client
{
    public class RMEmailClient
    {

        private static readonly RALogger s_logger = RALogger.GetInstance(typeof(RMEmailClient));

        private static readonly Dictionary<RMEmailTemplateType, IRMEmailCompiler> s_compilers = new();

        private static IGeneralSettingService GeneralSettingService => PlatformWindsorManager.GetService<IGeneralSettingService>();

        private readonly IRMEmailServer _emailSender;

        static RMEmailClient()
        {
            try
            {
                var compilerType = typeof(IRMEmailCompiler);
                var assembly = Assembly.GetAssembly(compilerType);

                foreach (var type in assembly.GetTypes())
                {
                    if (type.IsInterface) continue;
                    if (type.GetInterfaces().Contains(compilerType))
                    {
                        var instance = Activator.CreateInstance(type) as IRMEmailCompiler;
                        s_compilers.Add(instance.TemplateType, instance);
                    }
                }
                s_logger.Info($"Succeed initial email template compilers.");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while initial email template compilers. Error: {e}");
                throw;
            }
        }

        public RMEmailClient() :
            this(GeneralSettingService.GetGeneralSettingAsync().GetAwaiter().GetResult().EmailSenderDefinition)
        { }

        public RMEmailClient(EmailSenderDefinition emailSenderDefinition)
        {
            if (emailSenderDefinition.EmailSenderType == EmailSenderType.Default)
            {
                _emailSender = new RMDefaultEmailServer();
            }
            else if (emailSenderDefinition.EmailSenderType == EmailSenderType.O365)
            {
                _emailSender = new RMO365EmailServer(emailSenderDefinition);
            }
        }

        public async Task SendAsync(EmailTemplateDto emailTemplate, RMEmailTemplateParameters parameters)
        {
            var message = new RMEmailMessage
            {
                ToUsers = new List<string> { parameters.ToUser },
                CcUsers = emailTemplate.CC.Split(";", StringSplitOptions.RemoveEmptyEntries).ToList(),
                Subject = s_compilers[parameters.TemplateType].CompileSubject(emailTemplate.Subject, parameters),
                Body = CompileBody(emailTemplate, parameters),
            };

            if (emailTemplate.IsNewTemplate)
            {
                s_logger.Debug($"Current template [{emailTemplate.Id}] already modified. Need assembly image.");
                message.Images = GetImages(emailTemplate);
                _emailSender.AssemblyImages(message);
            }

            try
            {
                await _emailSender.SendAsync(message);
                s_logger.Info($"Succeed send template: [{emailTemplate.Id}] email. To: [{parameters.ToUser.LogBase64()}].");
            }
            catch (Exception e)
            {
                s_logger.Error($"An error occurred while send template: [{emailTemplate.Id}] email. To: [{parameters.ToUser.LogBase64()}]. Error: {e}");
            }
        }

        private static string CompileBody(EmailTemplateDto emailTemplate, RMEmailTemplateParameters parameters)
        {
            var body = emailTemplate.Body;
            body = body.Replace("&lt;", "&#60;").Replace("&gt;", "&#62;");
            if (!emailTemplate.IsNewTemplate)
            {
                body = body.Replace("<", "&#60;").Replace(">", "&#62;");
                body = $"<div>{body}</div>";  ////class='eContent'
            }
            
            body = s_compilers[parameters.TemplateType].CompileBody(body, parameters);

            body = body.Replace("\n", "<br/>");

            body = RMEmailTemplateHtml.BASIC_TEMPLATE.Replace("@Body", body);

            if (emailTemplate.IsUseDefaultFooter == (int)DefaultFooterStatus.UseDefaultFooter)
            {
                var copyright = RMEmailTemplateHtml.COPYRIGHT.Replace("@EndYear", DateTime.UtcNow.Year.ToString());
                body = body.Replace("@Copyright", copyright);
            }
            else
            {
                body = body.Replace("@Copyright", "");
            }

            if (I18NUtility.curCulture.Equals(RMEmailTemplateJapanCulture.JAPAN_CULTURE, StringComparison.OrdinalIgnoreCase))
            {
                RMEmailTemplateJapanCulture.JAPAN_FONT_FAMILY_MAPPING.ForEach(entry =>
                {
                    body = body.Replace(entry.Key, entry.Value);
                });
            }

            return body;
        }

        private static List<RMEmailImage> GetImages(EmailTemplateDto emailTemplate)
        {
            var res = new List<RMEmailImage>();

            var id = emailTemplate.IsCustomTemplate ? emailTemplate.UniqueId.ToString() : emailTemplate.Id.ToString();
            var imageFileNames = RAStorageUtil.AllBlobNames($"{TenantLocalValue.LogonGroupId}/{id}");
            foreach (var imageFileName in imageFileNames)
            {
                var imageName = imageFileName.Replace($"{TenantLocalValue.LogonGroupId}/{id}/", "");

                var imageInfo = imageName.Remove(imageName.LastIndexOf(".")).Split("_");
                var imageId = imageInfo[0];

                if (emailTemplate.Body.Contains(imageId))
                {
                    var imageType = imageInfo[1];
                    var content = RAStorageUtil.DownloadImageBlobToText(imageFileName);
                    res.Add(new RMEmailImage
                    {
                        Id = imageId,
                        Type = imageType,
                        Content = content,
                    });
                }
            }

            return res;
        }
    }
}
