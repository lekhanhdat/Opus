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
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.Hybrid.ClientCore;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.RACommonUtility.Email.Model;
using Cloud.Sdk.Nexus.Governance;
using GOneGlobal.GlobalDomain;
using EmailTemplateType = Cloud.Sdk.Data.Nexus.Foundation.EmailTemplateType;

namespace RADiscoveryUnitTest.GControlPlatformTests.GControlEmail;

[TestClass]
public class GControlEmailUnitTests : GControlPlatformInitializeTest
{
    [TestMethod]
    public async Task SendEmail_ShouldBeSuccessful()
    {
        var references = new Dictionary<string, object>()
        {
            [nameof(EmailTemplateReferenceType.TaskName)] = "TestTask",
            [nameof(EmailTemplateReferenceType.TaskCreatedTime)] = $"{DateTime.UtcNow}",
            [nameof(EmailTemplateReferenceType.TaskLink)] = $"<a href=\"{GCommonRoleConfiguration.GCONTROL_MYHUB_TASK_URL ?? ""}\" title=\"Vist MyHub\">Records review for disposal => Review</a>",
            ["Request"] = new 
            {
                Reviewer = "TestTask",
                Link = GCommonRoleConfiguration.GCONTROL_MYHUB_TASK_URL ?? "",
                LinkText = "Records review for disposal => Review",
                Comment = ""
            }
        };
        var locale = await NexusGovernancePersonalSettingService.GetPersonalSettingLanguage("103687575894702885469");
        var result = await GControlPlatformEmailService.SendEmailAsync(
            new Guid("dccccf33-8f8c-4693-ba64-f438fbac00ef"), "",
            EmailTemplateType.NewTask, references, locale);
        Assert.IsTrue(result);
    }
}