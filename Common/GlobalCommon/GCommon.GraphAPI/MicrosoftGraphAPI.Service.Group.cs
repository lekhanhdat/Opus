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


namespace AvePoint.GCommon.GraphAPI
{
    using AvePoint.GCommon.GraphAPI.JsonToObject.Teams;
    using Microsoft.Graph;
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public partial class MicrosoftGraphAPIService
    {
        private readonly string[] SelectProperties_GroupMember = { "id", "displayName", "mail", "userPrincipalName", "userType" };
        private readonly string[] SelectProperties_GroupMember_beta = { "id", "displayName", "mail", "userPrincipalName", "userType", "assignedPlans" };
		private readonly string[] SelectProperties_UserDetailForDefinedGroup = { "id", "businessPhones", "displayName", "givenName", "jobTitle", "mail", "mobilePhone", "officeLocation", "preferredLanguage", "surname", "userPrincipalName", "userType", "assignedPlans", "accountEnabled", "department" };
        public ListGroupsObj GetUnifiedGroups()
        {
            var request = new ListGroups(resourceUrl, refreshAccessToken, RetryController);
            request.QueryParameters.Filter("groupTypes/any(c:c+eq+'Unified')");
            return request.GetApiResult();
        }

        public Group GetGroupById(string groupId)
        {
            var group = new GetGroupByGroupId(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            var groupObjList = group.GetApiResult();
            return groupObjList.Value.FirstOrDefault();
        }

        public AssignedLabels[] GetGroupAssignedLabelById(string groupId)
        {
            var assignedLabel = new GetGroupAssignedLabel(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            var result = assignedLabel.GetApiResult();
            return result.AssignedLabels;
        }

        public string GetGroupIdByAddress(string o365GroupMailBox)
        {
            var group = new GetGroupByMail(this.resourceUrl, this.refreshAccessToken, o365GroupMailBox, this.RetryController);
            var listGroupObj = group.GetApiResult();
            return listGroupObj.Value.FirstOrDefault()?.Id;
        }

        public Group GetGroupByMailNickName(string o365GroupMailBox)
        {
            string mailNickName = o365GroupMailBox.Substring(0, o365GroupMailBox.LastIndexOf("@"));
            var group = new GetGroupByMailNickName(this.resourceUrl, this.refreshAccessToken, mailNickName, this.RetryController);
            var listGroupObj = group.GetApiResult();
            return listGroupObj.Value.FirstOrDefault();
        }

        public Group GetGroupInfoByAddress(string o365GroupMailBox)
        {
            var group = new GetGroupByMail(this.resourceUrl, this.refreshAccessToken, o365GroupMailBox, this.RetryController);
            var listGroupObj = group.GetApiResult();
            return listGroupObj.Value.FirstOrDefault();
        }
        
        public string GetGroupIdByDisplayName(string displayName)
        {
            var group = new GetGroupByDisplayName(this.resourceUrl, this.refreshAccessToken, displayName, this.RetryController);
            var listGroupObj = group.GetApiResult();
            return listGroupObj.Value[0].Id;
        }
        public string GetEmailByUPNName(string displayName)
        {
            var email = new GetEmailByUPNName(this.resourceUrl, this.refreshAccessToken, displayName, this.RetryController);
            var reuslt = email.GetApiResult();
            return reuslt.Value;
        }
        public Group GetGroupByDisplayName(string displayName)
        {
            var group = new GetGroupByDisplayName(this.resourceUrl, this.refreshAccessToken, displayName, this.RetryController);
            var listGroupObj = group.GetApiResult();
            return listGroupObj.Value[0];
        }

        public IList<Domain> GetAllDomains()
        {
            var domains = new GetAllDomains(this.resourceUrl, this.refreshAccessToken, this.RetryController);
            return domains.GetApiResult();
        }

        public IList<GraphUser> ListGroupMembers(string groupId)
        {
            var request = new ListGroupMembers(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            request.QueryParameters.Select(SelectProperties_GroupMember);
            return request.GetApiResult();
        }

        public IList<GraphUser> ListGroupOwners(string groupId)
        {
            var request = new ListGroupOwners(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            request.QueryParameters.Select(SelectProperties_GroupMember);
            return request.GetApiResult();
        }

        public IList<GraphUser> ListGroupUsersWithBetaApi(string groupId, bool findOwner)
        {
            var request = new ListGroupUsersWithBetaApi(this.resourceUrl, this.refreshAccessToken, groupId, findOwner, this.RetryController);
            request.QueryParameters.Select(SelectProperties_GroupMember_beta);
            return request.GetApiResult();
        }

        public IList<GraphUser> ListGroupMembersByGroupDisplayName(string displayName)
        {
            var request = new ListGroupMembersByGroupDisplayName(this.resourceUrl, this.refreshAccessToken, displayName, this.RetryController);
            request.QueryParameters.Select(SelectProperties_GroupMember);
            return request.GetApiResult();
        }

        public Group GetGroup(string groupId)
        {
            var request = new GetGroup(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            var group = request.GetApiResult();
            return group;
        }

        public IList<Group> ListGroup(bool isIncludeDetail = false)
        {
            var request = new ListGroup(this.resourceUrl, this.refreshAccessToken, this.RetryController);
            if (isIncludeDetail)
            {
                request.QueryParameters.Select(SelectProperties_UserDetailForDefinedGroup);
            }
            var groups = request.GetApiResult();
            return groups;
        }

        public IList<GraphUser> ListUser(bool isIncludeDetail = false)
        {
            var request = new ListUser(this.resourceUrl, this.refreshAccessToken, this.RetryController);
            if (isIncludeDetail)
            {
                request.QueryParameters.Select(SelectProperties_UserDetailForDefinedGroup);
            }
            var users = request.GetApiResult();
            return users;
        }

        public IList<GraphUser> ListGroupUsers(string groupId, bool findOwner)
        {
            var request = new ListGroupUsers(this.resourceUrl, this.refreshAccessToken, groupId, findOwner, this.RetryController);
            return request.GetApiResult();
        }

        public void AddGroupOwner(string groupId, string userId, bool addAsMember = true)
        {
            //add member first, then add owner, otherwise it may cause sync delay. AOSBR-8946
            if (addAsMember)
            {
                try
                {
                    // We recommend that when you add an owner, you also add that user as a member. If a team has an owner who is not also a member, ownership and membership changes might not show up immediately in Microsoft Teams.
                    // In addition, different apps and APIs will handle that differently. For example, Microsoft Teams will show teams that the user is either a member or an owner of, while the Microsoft Teams PowerShell cmdlets and the /me/joinedTeams API will only show teams the user is a member of.
                    // To avoid confusion, add all owners to the members list as well.
                    AddGroupMember(groupId, userId);
                }
                catch (Exception e)
                {
                    Logger.Warn($"Error occured while add group member. Error message : {e.Message}");
                }
            }
            var request = new AddGroupOwner(this.resourceUrl, this.refreshAccessToken, groupId, userId, this.RetryController);
            request.GetApiResult();
        }
        public void AddGroupMember(string groupId, string userId)
        {
            var request = new AddGroupMember(this.resourceUrl, this.refreshAccessToken, groupId, userId, this.RetryController);
            request.GetApiResult();
        }
        public GetGroupSiteObj GetGroupSiteByGroupId(string groupId)
        {
            var ggSite = new GetGroupSite(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            return ggSite.GetApiResult();
        }

        public void UpdateGroup(Group group)
        {
            new UpdateGroup(this.resourceUrl, this.refreshAccessToken, group, this.RetryController).GetApiResult();
        }

        /// <summary>
        /// Default document library URL
        /// </summary>
        /// <param name="groupId"></param>
        /// <returns></returns>
        public Microsoft.Graph.Models.Drive GetGroupDrive(string groupId)
        {
            return graphServiceClient.Groups[groupId].Drive.GetAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }
        public Group CreateUnifiedGroup(Group groupToCreate)
        {
            return CreateUnifiedGroup(groupToCreate, TemplateType.NotDefined);
        }
        public Group CreateUnifiedGroup(Group groupToCreate, TemplateType template)
        {
            groupToCreate.GroupTypes = new string[] { "Unified" };
            groupToCreate.MailEnabled = true;
            groupToCreate.SecurityEnabled = false;
            SetGroupPropertiesBasedOnTemplate(groupToCreate, template);
            var g = new CreateGroup(this.resourceUrl, this.refreshAccessToken, groupToCreate, this.RetryController).GetApiResult();
            if(String.IsNullOrEmpty(g.Id)) g.Id = g.ObjectId;//AOSBR-16858 global 环境返回 objectId ; GCCH 环境返回 Id
            return g;
        }

        private static void SetGroupPropertiesBasedOnTemplate(Group group, TemplateType template)
        {
            switch (template)
            {
                case TemplateType.EDU_Class:
                    group.Visibility = "HiddenMembership";
                    group.CreationOptions = new string[]
                    {
                        "ExchangeProvisioningFlags:461",
                        "classAssignments",
                    };
                    group.EducationObjectType = "Section";
                    return;
                case TemplateType.EDU_PLC:
                    group.CreationOptions = new string[]
                    {
                        "PLC",
                    };
                    return;
            }
            group.CreationOptions = new string[] { "ExchangeProvisioningFlags:481" };
        }

        public void DeleteGroup(string groupId)
        {
            new DeleteGroup(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController).GetApiResult();
        }
        public string GetGroupVisibility(string groupId)
        {
            var group = new GetGroupVisibility(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController);
            var gouopObj = group.GetApiResult();
            return gouopObj.Visibility;
        }
        public GroupExtraInfo GetGroupExtraSettings(string groupId)
        {
            return new GetGroupExtraSettings(this.resourceUrl, this.refreshAccessToken, groupId, this.RetryController).GetApiResult();
        }
        public void UpdateGroupExtraSettings(GroupExtraInfo extraSettings)
        {
            new UpdateGroupExtraSettings(this.resourceUrl, this.refreshAccessToken, extraSettings, this.RetryController).GetApiResult();
        }
    }

    public enum TemplateType
    {
        NotDefined,
        EDU_Class,
        EDU_PLC
    }
}