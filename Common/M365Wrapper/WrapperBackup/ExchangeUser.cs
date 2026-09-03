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
    using System;

    using AvePoint.GCommon.GraphAPI;

    using AvePoint.RA.CommonUtil;
    using M365.Wrapper.Backup.Auth.Common;

    public abstract class ExchangeUser : IDisposable
    {
        protected static RALogger logger = RALogger.GetInstance(typeof(ExchangeUser));

        public IAuthObject AuthObject { get; private set; }

        public string UserName { get { return this.AuthObject.UserName; } }

        public ExchangeUser(IAuthObject authObj)
        {
            this.AuthObject = authObj;
        }

        public abstract string GetO365GroupOwner(string o365GroupMailBox);

        public abstract string GetO365GroupMember(string o365GroupMailBox);

        public abstract bool IsO365GroupPrivate(string o365GroupMailBox);

        public virtual string GetO365GroupOwnerOrMember(string o365GroupMailBox)
        {
            var owner = GetO365GroupOwner(o365GroupMailBox);
            if (owner.IsNotNullOrEmpty())
            {
                return owner;
            }
            var member = GetO365GroupMember(o365GroupMailBox);
            if (member.IsNotNullOrEmpty())
            {
                return member;
            }
            if (IsO365GroupPrivate(o365GroupMailBox) || UserName.IsNullOrEmpty())
            {
                throw new AccessdeniedException(ExchangeReportMessage.CreateReportMessage("Agent.Office365Group.GroupNoUser_EF38F303-A038-4456-AECB-28146241C321", ExchangeGlobalConfig.MailboxDisplayNameDic.TryGetValue(o365GroupMailBox, out string displayName) ? $"{displayName}({o365GroupMailBox})" : o365GroupMailBox));
            }
            return UserName;
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        { }
    }

    public class ExchangeUserWithGraph : ExchangeUser
    {
        private MicrosoftGraphAPIService msGraphAPIService;

        public ExchangeUserWithGraph(IAppTokenAuthObject authObj)
            : base(authObj)
        {
            this.msGraphAPIService = new MicrosoftGraphAPIService(authObj.ResourceUrl, authObj.GetAccessToken, new GraphLogger());
            this.msGraphAPIService.RetryController = new GraphAPIRetry();
        }

        protected IAppTokenAuthObject AppTokenAuthObject
        {
            get
            {
                return this.AuthObject as IAppTokenAuthObject;
            }
        }

        public GraphUser GetUser(string idOrUserPrincipalName)
        {
            try
            {
                return msGraphAPIService.GetUser(idOrUserPrincipalName);
            }
            catch (GraphAPIException ex) when (ex.Error.Code.EqualsIgnoreCase("Request_ResourceNotFound"))
            {
                return null;
            }
        }
        public override bool IsO365GroupPrivate(string o365GroupMailBox)
        {
            try
            {
                return GetO365GroupVisibility(o365GroupMailBox);
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred when judging group is private,Error:{0}", ex.ToString());
                return false;
            }
        }

        public override string GetO365GroupMember(string o365GroupMailBox)
        {
            try
            {
                var groupId = GetGroupIdByName(o365GroupMailBox);
                return GetGroupMemberById(groupId);
            }
            catch (Exception ex)
            {
                logger.Error("An error occurred when get group member. Error:{0}", ex);
            }
            return string.Empty;
        }

        private string GetGroupMemberById(string groupId)
        {
            string groupMember = null;
            try
            {
                groupMember = GetGroupMember(groupId);
                logger.Info("Group member: {0}", groupMember);
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred when get group member.Retry Time: 1 ,Error:{0}", ex.ToString());
                groupMember = GetGroupMember(groupId);
                logger.Info("Group member: {0}", groupMember);
            }
            return groupMember;
        }

        public override string GetO365GroupOwner(string o365GroupMailBox)
        {
            try
            {
                var groupId = GetGroupIdByName(o365GroupMailBox);
                return GetGroupOwnerById(groupId);
            }
            catch (Exception ex)
            {
                logger.Error("An Error occurred when get group owner. Error:{0}", ex);
            }
            return string.Empty;
        }

        private string GetGroupOwnerById(string groupId)
        {
            string groupOwner = null;
            try
            {
                groupOwner = GetGroupOwner(groupId);
                logger.Info("Group Owner: {0}", groupOwner);
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred when get group owner.Retry Time: 1 ,Error:{1}", ex.ToString());
                groupOwner = GetGroupOwner(groupId);
                logger.Info("Group Owner: {0}", groupOwner);
            }
            return groupOwner;
        }

        private string GetGroupIdByName(string o365GroupMailBox)
        {
            var groupId = string.Empty;
            try
            {
                groupId = GetGroupId(o365GroupMailBox);
                logger.Info("Group Id: {0}", groupId);
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred when get group id.Retry Time: 1 ,Error:{0}", ex.ToString());
                groupId = GetGroupId(o365GroupMailBox);
                logger.Info("Group Id: {0}", groupId);
            }
            return groupId;
        }

        private bool GetO365GroupVisibility(string o365GroupMailBox)
        {
            var groupVisibility = string.Empty;
            try
            {
                groupVisibility = GetGroupVisibility(o365GroupMailBox);
                logger.Info("Group Visibility Value: {0}", groupVisibility);
            }
            catch (Exception ex)
            {
                logger.Warn("An Error occurred when get group visibility.Retry Time: 1 ,Error:{0}", ex.ToString());
                groupVisibility = GetGroupVisibility(o365GroupMailBox);
                logger.Info("Group Visibility Value: {0}", groupVisibility);
            }
            return groupVisibility.Equals("Private", StringComparison.OrdinalIgnoreCase) || groupVisibility.Equals("HiddenMembership", StringComparison.OrdinalIgnoreCase);
        }

        private string GetGroupId(string o365GroupMailBox)
        {
            return msGraphAPIService.GetGroupIdByAddress(o365GroupMailBox);
        }

        private string GetGroupOwner(string groupId)
        {
            return msGraphAPIService.GetLicensedUser(groupId, findowner: true)?.UserPrincipalName;
        }

        private string GetGroupMember(string groupId)
        {
            return msGraphAPIService.GetLicensedUser(groupId, findowner: false)?.UserPrincipalName;
        }

        private string GetGroupVisibility(string o365GroupMailBox)
        {
            return msGraphAPIService.GetGroupInfoByAddress(o365GroupMailBox).Visibility;
        }
    }
}