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
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.RADataBroker;
using AvePoint.RA.RAExchange.Authorization;
using ExchangeBackupUtility;
using ExchangeUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using IExchangeFolder = ExchangeBackupUtility.Graph.IExchangeFolder;
using NewAuthorizationManager = ExchangeUtility.Graph.AuthorizationManager;
using AuthScope = ExchangeUtility.Graph.AuthScope;
using ExchangeFactoryProvider = ExchangeBackupUtility.ExchangeFactoryProvider;
using NewGlobalExchangeSetting = ExchangeUtility.Graph.GlobalExchangeSetting;
using NewExchangeMailbox = ExchangeUtility.Graph.ExchangeMailbox;
using NewExchangeMailboxType = ExchangeUtility.Graph.ExchangeMailboxType;

namespace AvePoint.RA.RAExchange.Common
{
    public class TreeManagement
    {
        private static readonly AvePoint.RA.Contract.Services.IRALogger logger = RALogger.GetInstance(typeof(TreeManagement));
        private readonly IRMMailboxDao MailBoxDao = PlatformWindsorManager.GetService<IRMMailboxDao>();
        public static ExchangeOnlineTreeNodeDto GetMailboxNode(ExchangeOnlineTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.ExchangeOnlineMailbox 
                && node.Level != NodeLevel.ExchangeOnlineO365Group
                && node.Level != NodeLevel.Office365GroupEntire)
            {
                node = node.Parent;
            }
            return node;
        }
        public static ExchangeOnlineTreeNodeDto GetGroupNode(ExchangeOnlineTreeNodeDto curnode)
        {
            var node = curnode;
            while (node.Level != NodeLevel.ExchangeOnlineMailboxGroup 
                && node.Level != NodeLevel.ExchangeOnlineO365GroupGroup
                && node.Level != NodeLevel.WebApplication)
            {
                node = node.Parent;
            }
            return node;
        }

        public ExchangeFolder GetExchangeFolderFromTreeNode(ExchangeOnlineTreeNodeDto treeNodeDto)
        {
            using (var performance = new PerformanceScope("EXO.TreeManagement.Init", "", true))
            {
                var emailBposInfoMap = GetBPOSInfo(treeNodeDto);
                AuthorizationManager.Instance.Init(emailBposInfoMap);
                var mailboxNode = TreeManagement.GetMailboxNode(treeNodeDto);
                var address = mailboxNode.Name;
                logger.Info($"Current mailbox address is : {mailboxNode.ID}.");
                //目前DAO Tree上的MailboxType  不准，即使修改也会存在老数据问题，所以外围自己通过可用的属性Type 重新确认了类型
                var mailboxType = ExchangeMailboxType.User;
                if (mailboxNode.Type == NodeType.EOO365Group)
                {
                    mailboxType = ExchangeMailboxType.Group;
                }
                EnableEWSNonitor();
                SetGlobalSetting(mailboxType == ExchangeMailboxType.Group);
                GlobalExchangeSetting.SetImpersonateIdToDictionary(address, AuthorizationManager.Instance.GetAuthObjectWindowsGraph(address));

                var CurrentFolder = treeNodeDto.Level == NodeLevel.ExchangeOnlineMailbox ?
                          new ExchangeRootFolder(new ExchangeMailbox(address, mailboxType), AuthorizationManager.Instance.GetAuthObject(address)) :
                            new ExchangeFolder(new ExchangeMailbox(address, mailboxType), treeNodeDto.ID, AuthorizationManager.Instance.GetAuthObject(address));
                CurrentFolder.DisplayFolderPath = treeNodeDto.FullPath;
                CurrentFolder.InternalFolderPath = treeNodeDto.InternalFolderPath;
                CurrentFolder.Open();
                CurrentFolder.GenerateCurrentSyncState();
                CurrentFolder.GenerateCurrentItemSyncState();
                if (treeNodeDto.Level == NodeLevel.ExchangeOnlineMailbox || treeNodeDto.Level == NodeLevel.ExchangeOnlineO365Group)
                {
                    CurrentFolder.InternalFolderPath = CurrentFolder.DisplayFolderPath;
                    //AccessConfig.UseImpersonateList[treeNode.EmailAddress] = useImpersonate;
                }
                return CurrentFolder;
            }
        }

        public IExchangeFolder GetExchangeFolderFromTreeNodeV2(ExchangeOnlineTreeNodeDto treeNodeDto, string mailboxId,bool supportGraphApi)
        {
            using var performance = new PerformanceScope("EXO.TreeManagement.Init", "", true);
            var emailBposInfoMap = GetBPOSInfo(treeNodeDto);
            NewAuthorizationManager.Instance.Init(emailBposInfoMap, authScopes: [AuthScope.MicrosoftGraph, AuthScope.EWS]);
            var mailboxNode = GetMailboxNode(treeNodeDto);
            var address = mailboxNode.Name;
            logger.Info($"Current mailbox address is : {mailboxNode.ID}.");
            //目前DAO Tree上的MailboxType  不准，即使修改也会存在老数据问题，所以外围自己通过可用的属性Type 重新确认了类型
            var mailboxType = NewExchangeMailboxType.User;
            if (mailboxNode.Type == NodeType.EOO365Group)
            {
                mailboxType = NewExchangeMailboxType.Group;
            }

            EnableEWSNonitor();
            SetGlobalSetting(mailboxType == NewExchangeMailboxType.Group);
            NewGlobalExchangeSetting.SetImpersonateIdToDictionary(address, NewAuthorizationManager.Instance);

            var factory = ExchangeFactoryProvider.Create(supportGraphApi);

            var authObject = supportGraphApi
                ? NewAuthorizationManager.Instance.GetAuthObjectForGraph(address)
                : NewAuthorizationManager.Instance.GetAuthObjectForEWS(address);
            var mailbox = new NewExchangeMailbox(address, mailboxType, mailboxId);
            var currentFolder = treeNodeDto.Level == NodeLevel.ExchangeOnlineMailbox
                ? factory.CreateRootFolder(mailbox, supportGraphApi ? authObject : AuthObjectConverter.ToEwsAuthObject(authObject, emailBposInfoMap.Values.First()))
                : factory.CreateFolder(mailbox, treeNodeDto.ID, supportGraphApi ? authObject : AuthObjectConverter.ToEwsAuthObject(authObject, emailBposInfoMap.Values.First()));
            currentFolder.DisplayFolderPath = treeNodeDto.FullPath;
            currentFolder.InternalFolderPath = treeNodeDto.InternalFolderPath;
            currentFolder.Open();
            currentFolder.GenerateCurrentSyncState();
            currentFolder.GenerateCurrentItemSyncState();
            if (treeNodeDto.Level is NodeLevel.ExchangeOnlineMailbox or NodeLevel.ExchangeOnlineO365Group)
            {
                currentFolder.InternalFolderPath = currentFolder.DisplayFolderPath;
            }

            return currentFolder;
        }

        public string GetRealMailboxGuid(ExchangeOnlineTreeNodeDto treeNodeDto)
        {
            string mailboxGuid = string.Empty;
            var emailBposInfoMap = GetBPOSInfo(treeNodeDto);
            var mailboxNode = TreeManagement.GetMailboxNode(treeNodeDto);
            var address = mailboxNode.Name;
            logger.Info($"GetRealMailboxGuid.Current mailbox address is : {mailboxNode.ID}.");
            if (AvePoint.RA.SharePoint.ArchiverCommon.ArchiverCommonStaticMethod.IsNestleCustomize)
            {
               var result= MailBoxDao.GetEmailByEmailAddress(address);
                if (result != null)
                {
                    mailboxGuid = result.ObjectId;
                    if (mailboxGuid.IndexOf("(Archive)") != -1)
                    {
                        mailboxGuid = mailboxGuid.Substring(0, mailboxGuid.IndexOf("(Archive)"));
                    }
                    logger.Info("NestleCustomize.Mailbox real mailBoxGuid is:{0}", mailboxGuid);
                }
            }
            else if (emailBposInfoMap.Count > 0)
            {
                var results = RMAosApiClient.GetModernTenantRemoteNodes(string.IsNullOrEmpty(emailBposInfoMap.FirstOrDefault().Value.TenantGroupId) ? TenantLocalValue.LogonGroupId : emailBposInfoMap.FirstOrDefault().Value.TenantGroupId, emailBposInfoMap.FirstOrDefault().Value.UserAccountInfo.TenantId);
                var findResult = results.Mailboxes.Find(r => r.Name.Equals(address, StringComparison.OrdinalIgnoreCase));
                if (findResult != null)
                {
                    mailboxGuid = findResult.ObjectId;
                    if (mailboxGuid.IndexOf("(Archive)") != -1)
                    {
                        mailboxGuid = mailboxGuid.Substring(0, mailboxGuid.IndexOf("(Archive)"));
                    }
                    logger.Info("Mailbox real mailBoxGuid is:{0}", mailboxGuid);
                }
            }
            return mailboxGuid;
        }

        //return aos mailbox id, if it is a inplace archive mailbox, id will be guid+(Archive)
        public string GetRealMailboxStringId(ExchangeOnlineTreeNodeDto treeNodeDto)
        {
            string mailboxGuid = string.Empty;
            var emailBposInfoMap = GetBPOSInfo(treeNodeDto);
            var mailboxNode = TreeManagement.GetMailboxNode(treeNodeDto);
            var address = mailboxNode.Name;
            logger.Info($"GetRealMailboxStringId.Current mailbox Id is : {mailboxNode.ID}.");
            if (AvePoint.RA.SharePoint.ArchiverCommon.ArchiverCommonStaticMethod.IsNestleCustomize)
            {
                var result = MailBoxDao.GetEmailByEmailAddress(address);
                if (result != null)
                {
                    mailboxGuid = result.ObjectId;
                    logger.Info("NestleCustomize.Mailbox real mailbox string id is:{0}", mailboxGuid);
                }
            }
            else if (emailBposInfoMap.Count > 0)
            {
                var results = RMAosApiClient.GetModernTenantRemoteNodes(string.IsNullOrEmpty(emailBposInfoMap.FirstOrDefault().Value.TenantGroupId) ? TenantLocalValue.LogonGroupId : emailBposInfoMap.FirstOrDefault().Value.TenantGroupId, emailBposInfoMap.FirstOrDefault().Value.UserAccountInfo.TenantId);
                var findResult = results.Mailboxes.Find(r => r.Name.Equals(address, StringComparison.OrdinalIgnoreCase));
                if (findResult != null)
                {
                    mailboxGuid = findResult.ObjectId;                    
                    logger.Info("Mailbox real mailbox string id is:{0}", mailboxGuid);
                }
            }
            return mailboxGuid;
        }

        public string GetAOSObjectId(ExchangeOnlineTreeNodeDto treeNodeDto)
        {
            string mailboxGuid = string.Empty;
            var emailBposInfoMap = GetBPOSInfo(treeNodeDto);
            var mailboxNode = TreeManagement.GetMailboxNode(treeNodeDto);
            var address = mailboxNode.Name;
            logger.Info($"GetAOSObjectId.Current mailbox address is : {mailboxNode.ID}.");
            if (AvePoint.RA.SharePoint.ArchiverCommon.ArchiverCommonStaticMethod.IsNestleCustomize)
            {
                var result = MailBoxDao.GetEmailByEmailAddress(address);
                if (result != null)
                {
                    mailboxGuid = result.ObjectId;
                    logger.Info("NestleCustomize.Mailbox real mailbox string id is:{0}", mailboxGuid);
                }
            }
            else if (emailBposInfoMap.Count > 0)
            {
                var results = RMAosApiClient.GetModernTenantRemoteNodes(string.IsNullOrEmpty(emailBposInfoMap.FirstOrDefault().Value.TenantGroupId) ? TenantLocalValue.LogonGroupId : emailBposInfoMap.FirstOrDefault().Value.TenantGroupId, emailBposInfoMap.FirstOrDefault().Value.UserAccountInfo.TenantId);
                var findResult = results.Mailboxes.Find(r => r.Name.Equals(address, StringComparison.OrdinalIgnoreCase));
                if (findResult != null)
                {
                    mailboxGuid = findResult.ObjectId;
                    logger.Info("Mailbox AOSObjectId is:{0}", mailboxGuid);
                }
            }
            return mailboxGuid;
        }

        private Dictionary<string, BposInfo> GetBPOSInfo(ExchangeOnlineTreeNodeDto treeNode)
        {
            using (var performance = new PerformanceScope("EXO.TreeManagement.GetBPOSInfo", "", true))
            {
                if (treeNode.Level == NodeLevel.ExchangeOnlineMailbox)
                {
                    //DAOAPIClientV1 client = new DAOAPIClientV1();

                    var bposInfo = new Dictionary<string, BposInfo>();
                    //bposInfo.Add(treeNode.Name, client.GetBPOSInfoByEXONode(treeNode));
                    var info = RABrowserClient.GetBPOSInfoByEXONode(treeNode);
                    bposInfo.Add(treeNode.Name, info);
                    return bposInfo;
                }
                else
                {
                    return GetBPOSInfo(treeNode.Parent);
                }
            }
        }

        private void EnableEWSNonitor()
        {
            EWSMonitor.Mode = EWSMonitorMode.RequesRate | EWSMonitorMode.RequestDetails;
            EWSMonitor.IntervalInSecond = 300;
            logger.Info("Set EWSMonitorMode to {0}, set EWSMonitorIntervalInSecond to {1}.", EWSMonitor.Mode, EWSMonitor.IntervalInSecond);
        }


        private void SetGlobalSetting(bool isO365Group)
        {
            GlobalExchangeSetting.IsO365GroupMailBox = isO365Group;
#if !DEBUG
            if (isO365Group)
            {
                var randomCount = GetRandomCount();
                logger.Info("Group Waiting time: {0} * 40s.", randomCount);
                Thread.Sleep(randomCount * 1000 * 40);
            }
#endif
            logger.Info("Start to set global setting.");
            using (new PerformanceScope("EXO.TreeManagement.SetGlobalSetting"))
            {
                GlobalExchangeSetting.SetServicePointManager();
            }
            logger.Info("Is O365 Group Mailbox:{0}", isO365Group);
        }

        private int GetRandomCount()
        {
            var tick = DateTime.Now.Ticks;
            var ran = new Random((int)(tick & 0xffffffffL) | (int)(tick >> 32));
            return ran.Next(0, 9);
        }
    }
}
