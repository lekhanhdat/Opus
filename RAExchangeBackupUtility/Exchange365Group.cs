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
using AvePoint.RA.CommonUtil;
using System;
using System.Reflection;

namespace ExchangeUtility
{
    public static class Office365GroupFacotry
    {
        public static Office365GroupService CreateOffice365GroupService(AuthObject authObj)
        {
            var ewsServiceUrl = string.IsNullOrEmpty(authObj.EWSServiceUrl) ? Util.AutoDiscoverServiceUrl(authObj, authObj.UserName).OriginalString : authObj.EWSServiceUrl;
            return new Office365GroupServiceWithSA2AppToken(authObj, ewsServiceUrl);
        }
        public abstract class Office365GroupService : ExchangeObjectBase, IDisposable
        {
            protected static RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
            //protected ExchangeSoapService Office365GroupSerice = null;
            public Office365GroupService(AuthObject authObj)
                : base(authObj)
            {
            }

            //public virtual Office365GroupEntityV2 GetO365GroupDetails(string o365GroupName)
            //{
            //    logger.Info("Get office 365 group details.[{0}]", o365GroupName);
            //    var groupDetails = Office365GroupSerice.GetGroup(o365GroupName);
            //    var groupDetailsEntity = GlobalExchangeSetting.ConvertClassBySameClassStructure<UnifiedGroupDetails, Office365GroupEntityV2>(groupDetails);
            //    var groupMemberEntity = this.GetO365GroupOwnerAndMembers(o365GroupName);
            //    groupDetailsEntity.GroupMemberList = groupMemberEntity;
            //    return groupDetailsEntity;
            //}
            //public virtual List<GroupMemberV2> GetO365GroupOwnerAndMembers(string o365GroupName)
            //{
            //    logger.Info("Get office 365 group owners and members.[{0}]", o365GroupName);
            //    var totalMemberCount = 0;
            //    var groupMemberItems = Office365GroupSerice.GetMembers(o365GroupName, 500, 0, out totalMemberCount);
            //    var groupMembers = groupMemberItems.Select(item => new GroupMemberV2 { IsOwner = item.IsOwner, UserName = item.Persona.EmailAddress.EmailAddress }).ToList();
            //    return groupMembers;
            //}
            //public virtual string GetO365GroupOwnerOrMember(string o365GroupName)
            //{
            //    logger.Info("Get office 365 group owners or members.[{0}]", o365GroupName);
            //    var totalMemberCount = 0;
            //    var groupMemberItems = Office365GroupSerice.GetMembers(o365GroupName, 500, 0, out totalMemberCount);
            //    if (totalMemberCount > 0)
            //    {
            //        return groupMemberItems.First(m => !m.IsGuest).Persona.EmailAddress.EmailAddress;
            //    }
            //    else
            //    {
            //        throw new AccessdeniedException(ExchangeConstants.ERRORMESSAGE_GROUP_NONEUSER);
            //        //var groupDetails = Office365GroupSerice.GetGroup(o365GroupName);
            //        //if (groupDetails.AccessType == ExchangeService.GroupAccessType.Private)
            //        //{
            //        //    throw new AccessdeniedException(ExchangeConstants.ERRORMESSAGE_GROUP_NONEUSER);
            //        //}
            //        //return this.UserName;
            //    }
            //}
            /// <summary>
            /// Owners count maybe is 0
            /// </summary>
            /// <param name="o365GroupName"></param>
            /// <returns></returns>
            //public virtual List<string> GetO365GroupOwners(string o365GroupName)
            //{
            //    logger.Info("Get office 365 group owners.[{0}]", o365GroupName);
            //    var totalMemberCount = 0;
            //    var groupMembers = Office365GroupSerice.GetMembers(o365GroupName, 500, 0, out totalMemberCount);
            //    var owners = groupMembers.Where(member => member.IsOwner).Select(member => member.Persona.EmailAddress.EmailAddress).ToList();
            //    return owners;
            //}
            /// <summary>
            /// Member count maybe is 0
            /// </summary>
            /// <param name="o365GroupName"></param>
            /// <returns></returns>
            //public virtual List<string> GetO365GroupMembers(string o365GroupName)
            //{
            //    logger.Info("Get office 365 group members.[{0}]", o365GroupName);
            //    var totalMemberCount = 0;
            //    var groupMembers = Office365GroupSerice.GetMembers(o365GroupName, 500, 0, out totalMemberCount);
            //    var members = groupMembers.Where(member => !member.IsOwner).Select(member => member.Persona.EmailAddress.EmailAddress).ToList();
            //    return members;
            //}
            //public virtual Boolean IsO365GroupExist(string o365GroupName)
            //{
            //    try
            //    {
            //        Office365GroupSerice.GetGroup(o365GroupName);
            //        return true;
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error("An error occurred when get group info.{0}", ex);
            //        return false;
            //    }
            //}
            //public virtual Boolean CreateO365Group(Office365GroupEntityV2 office365GroupEntity)
            //{
            //    logger.Info("Create office 365 group.");
            //    var exchangeServiceGroupEntity = GlobalExchangeSetting.ConvertClassBySameClassStructure<Office365GroupEntityV2, UnifiedGroupDetails>(office365GroupEntity);
            //    var groupInfo = new GroupCreateInfomation()
            //    {
            //        AccessType = exchangeServiceGroupEntity.AccessType,
            //        AutoSubscribeNewMembers = exchangeServiceGroupEntity.MailboxSettings.AutoSubscribeNewMembers,
            //        Description = exchangeServiceGroupEntity.Description,
            //        Name = exchangeServiceGroupEntity.DisplayName,
            //        Alias = exchangeServiceGroupEntity.SmtpAddress.Substring(0, exchangeServiceGroupEntity.SmtpAddress.LastIndexOf('@')),
            //        IsGroupMembershipHidden = exchangeServiceGroupEntity.AdditionalProperties.IsGroupMembershipHidden,
            //        Language = exchangeServiceGroupEntity.MailboxSettings.MailboxCultureName
            //    };
            //    return !string.IsNullOrEmpty(Office365GroupSerice.CreateGroup(groupInfo).Email);
            //}
            //public virtual void AddO365GroupMember(Office365GroupEntityV2 office365GroupEntity, string o365GroupName)
            //{
            //    try
            //    {
            //        logger.Info("Add office 365 group members.[{0}]", o365GroupName);
            //        var memberList = office365GroupEntity.GroupMemberList.Select(member => member.UserName).ToList();
            //        var ownerList = office365GroupEntity.GroupMemberList.Where(member => member.IsOwner).Select(member => member.UserName).ToList();
            //        var addMemberSuccess = Office365GroupSerice.AddMember(o365GroupName, memberList);
            //        if (addMemberSuccess) Office365GroupSerice.MakeOwnerStatus(o365GroupName, ownerList);
            //    }
            //    catch (Exception ex)
            //    {
            //        logger.Error("An error occurred when get group members.{0}", ex);
            //    }
            //}
            public void Dispose()
            {
                this.Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            { }
        }

        public class Office365GroupServiceWithSA2AppToken : Office365GroupService
        {
            public Office365GroupServiceWithSA2AppToken(AuthObject authObj, string serviceUrl)
                : base(authObj)
            {
                var accessToken = this.tokenAuthObj.GetAccessToken();
                //Office365GroupSerice = new ExchangeSoapService(authObj.UserName, accessToken, () => this.tokenAuthObj.GetAccessToken(), serviceUrl);
            }
            protected IAppTokenAuthObject tokenAuthObj
            {
                get
                {
                    return this.AuthObject as IAppTokenAuthObject;
                }
            }
        }
        //public class Office365GroupServiceWithAppToken : Office365GroupService
        //{
        //    public Office365GroupServiceWithAppToken(AppTokenAuthObject authObj, string serviceUrl)
        //        : base(authObj)
        //    {
        //        var accessToken = this.AppTokenAuthObject.GetAccessToken();
        //        Office365GroupSerice = new ExchangeSoapService(authObj.UserName, accessToken, () => this.AppTokenAuthObject.GetAccessToken(), serviceUrl);//() => { return "you new access token"; });//
        //    }

        //    protected AppTokenAuthObject AppTokenAuthObject
        //    {
        //        get
        //        {
        //            return this.AuthObject as AppTokenAuthObject;
        //        }
        //    }
        //}

    }
}
