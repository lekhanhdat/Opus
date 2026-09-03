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

namespace ExchangeUtility.Graph
{
    //using AvePoint.Application.Serializer;
    //using AvePoint.GCommon.Contract.Context;
    using System.Collections.Generic;

    public class ExchangeReportMessage
    {
        public static string CreateReportMessage(string messageKey, params string[] parameters)
        {
            return ToCreateJobReportMessage(messageKey, GetMessageValue(messageKey), parameters);
        }

        private static string ToCreateJobReportMessage(string messageKey, string messageValue, params string[] parameters)
        {
            //studo:var jobReportMessage = new JobReportMessage { Key = messageKey, DefaultValue = messageValue, Parameters = parameters };
            //studo:return jobReportMessage.SerializeByJsonConvert();
            return "";
        }

        public static string GetMessageValue(string comment)
        {
            if (i18nSourceMessage.ContainsKey(comment))
            {
                return i18nSourceMessage[comment];
            }
            return null;
        }

        private static Dictionary<string, string> i18nSourceMessage
        {
            get
            {
                Dictionary<string, string> dics = new Dictionary<string, string>();
                dics.Add("Agent.Office365Group.GroupNoUser_EF38F303-A038-4456-AECB-28146241C321", "{0} does not have any owners or members,please add it and try again");
                dics.Add("Agent.Office365Group.GroupExist_F125D124-6A2A-4C57-8364-F9964F0CA07C", "Group:[{0}] already exists in the destination.");
                dics.Add("Agent.Office365Group.SameNameUserMailbox_D8F898C3-02B2-400E-B380-29B1016C47DE", "There is the same name user mailbox.[{0}]");
                dics.Add("Agent.Office365Group.TeamsGroupExist_8A957D25-AACD-4A3D-8216-C46DEE598C12", "Teams Group:[{0}] already exists in the destination.");
                dics.Add("Agent.Teams.TeamsExist_43414301-E295-4734-89FF-FE4047B63CDA", "Teams:[{0}] already exists in the destination.");
                dics.Add("Agent.Teams.ChannelExist_F125D124-6A2A-4C57-8364-F9964F0CA07C", "Channel:[{0}] already exists in the destination.");
                dics.Add("Agent.Office365Group.GroupSystemFolder_6D49F2C0-7C27-42F4-99A8-057115D99A36", "This folder is skipped because it is system folder.");
                dics.Add("Agent.Teams.TeamsExistButNotMember_525D7CDD-00AA-42EA-903F-76A39D746AE2", "Teams:[{0}] already exists in the destination, but the service account is not the team owner or member.");
                dics.Add("Agent.Teams.AliasBeOccupied_19083F0C-6EF3-4701-A997-78CAC2BBCA15", "The alias [{0}] used to create team has already been occupied by others. ");
                dics.Add("Agent.Teams.FailedGetTeamGroup_C0D9D010-8E65-435D-A6B3-2874CC3AED03", "Failed to get the team group or the team group is not the new one. Team:[{0}]. ");
                dics.Add("Agent.Teams.FailedGetTeamSite_FE69A800-35AD-45B0-A766-4E2BDFD4F8ED", "Failed to get the team site. Team:[{0}]. ");
                dics.Add("Agent.Teams.FailedUpdateTeamAddress_E5FE21A0-E10A-4E55-B38B-2E03D54975CB", "Failed to update team address. Team:[{0}]. ");
                dics.Add("Agent.Teams.FailedUpdateTeamAddress_D9EED4BD-CD89-4D5F-8290-BBDDCA319F5B", "App Profile doesn't support to update team address. Team:[{0}]. ");
                dics.Add("Agent.Office365Group.NoPermissionsAccessItem_0CA9D403-ADB9-4348-9F58-6F84B6472333", "The account used by this service ({0}) must be the owner and member of the Office 365 Group/Team where this Planner information is stored");
                dics.Add("Agent.Office365Group.VisitPlannerFailed_072E474D-101F-4EA6-A526-B767871A8600", "You do not have permission to view this directory or page using the credentials that you supplied.");
                dics.Add("Agent.Exchange.CommitFailed_89C03B89-6205-433E-8248-5D0171EFDB93", "The pst file [{0}] was generated successfully, but failed to commit, if want to get the file directly, please contact the support, or rerun the export job");
                dics.Add("Agent.Office365Group.TemporarilyUnavailable_575956E5-1C1C-420D-840B-91896F037EA4", "The resource you are looking for might have been removed, had its name changed, or is temporarily unavailable.");
                dics.Add("Agent.Exchange.MFA_DBC3DD47-C31A-4F59-B252-FEC624CAFB14", "Unable to use {0} to access this resource. You may have enabled Multi-Factor Authentication for this account.");
                dics.Add("Agent.Teams.CannotGetBackupData_E919719C-31E4-482F-B459-F4C9D420E1EE", "We can't find the successful backup data, please check the backup status of job:{0}");
                dics.Add("Agent.Exchange.ImpersonateFailed_6EB6670C-0894-07AA-8B7E-158EF71E4B76", "The account[{0}] does not have permission to impersonate the requested user. Please add ApplicationImpersonation permission for this account in Exchange admin center(permission>admin roles>add) and try again.");
                dics.Add("Wrapper_IncorrectUserNameOrPasswordError", "The service account credentials may have been updated. Username: {0}.");
                dics.Add("Agent.Exchange.NoLicense_4047EB60-E71A-BCC7-905D-0A1C32A740C9", "There is no license, or the license is invalid.");
                dics.Add("Agent.Exchange.OrganizationRemoved_246C3598-92EC-C1C1-539D-E26EB13A863E", "Your organization doesn't have full Exchange Online functionality or  your organization is marked for removal. If this state is unexpected then please contact Office 365 Support.");
                dics.Add("Agent.Exchange.MailboxAddressNotMatch_B51E37F5-87E3-4BBA-BDC5-DF7458BDD290", "The actual mailbox address retrieved by using the object Id does not match the registered one in aos. This situation is usually caused by rename mailbox, please scan again.");
                //使用sp的词条
                dics.Add("Wrapper_DeviceNotAvailable", "Cannot connect to the device successfully. Please check if the device is available or if there is any firewall rule block the connection, fix it and try again later.");
                dics.Add("Wrapper_DeviceAuthenticationFailed", "Cannot connect to the device successfully. Please check if the credential of your device has been changed, fix it and try again later.");
                dics.Add("Agent.PowerBI.SkipRestoreIncomingChannel_B8815F3E-16EF-447D-9FDA-9305D1EF947E", "Skip restoring this incoming channel, which is hosted at Other teams ({0})");
                dics.Add("Agent.Yammer.GroupNotExist_48D9F21B-1C94-445C-AE8D-7572A3353495", "Yammer group doesn't exist in the destination.");
                dics.Add("Agent.Exchange.NoRoleAssigned_435555BB-6BeD-4FC8-AB95-09B815E62C6E", "For the successful of Public folder backup, please go to Microsoft Entra admin center (or Microsoft Azure portal) to assign the Exchange Administrator role to the application {0}.");
                return dics;
            }
        }
    }
}