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
using AvePoint.Common.Portal;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.GCommon.Utility.TransientFault;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Encryption;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.Service.Services.Tenant.SyncNodes.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Tenant
{
    public class RMMailboxService : RMServiceBase, IRMMailboxService
    {
        private RALogger logger = RALogger.GetInstance(typeof(RMMailboxService));
        private static readonly AveRetryPolicy RetryPolicy = new AveRetryPolicy(new AveTransientErrorCatchAllStrategy(), new FixedIntervalRetryStrategy(3, TimeSpan.FromSeconds(10)));
        private static readonly object lockObj = new object();

        private IRMMailboxDao MailBoxDao => PlatformWindsorManager.GetService<IRMMailboxDao>();
        //public IRMRemoteO365AccountService RemoteO365AccountService { get; set; }

        public List<SyncRemoteNodePara> GetAllMailboxNodesByPage(int pageIndex, int pageSize)
        {
            return MailBoxDao.GetAllMailboxNodesByPage(pageIndex, pageSize);
        }

        public List<SyncRemoteNodePara> GetAllMailboxNodes()
        {
            return MailBoxDao.GetAllMailboxNodes();
        }
        public List<EmailAccountDto> GetAllMailboxNodesWithId()
        {
            return MailBoxDao.GetAllMailboxNodesWithId();
        }
        public int GetMailboxNodesCount()
        {
            return MailBoxDao.GetMailboxNodesCount();
        }

        public Dictionary<string, string> GetMailboxNamesByParentIds(List<string> parentIds)
        {
            return MailBoxDao.GetMailboxNamesByParentIds(parentIds);
        }

        public Dictionary<string, string> GetParentNamesByMailboxes(IEnumerable<string> mailboxNames, bool includeO365Group = false)
        {
            return MailBoxDao.GetParentNamesByMailboxes(mailboxNames, includeO365Group);
        }

        public List<RemoteNodePara> GetRemoteMailGroupNodes()
        {
            return this.MailBoxDao.GetRemoteMailGroupNodes();
        }

        public RemoteNodePara GetMailGroupByNameAndNodeLevel(string name, int nodeLevel)
        {
            return MailBoxDao.GetMailGroupByNameAndNodeLevel(name, nodeLevel);
        }

        public RemoteNodePara GetMailGroupByAosIdAndNodeLevel(string aosId, int nodeLevel)
        {
            return MailBoxDao.GetMailGroupByAosIdAndNodeLevel(aosId, nodeLevel);
        }

        /// <summary>
        ///  Username不会被解密
        /// </summary>
        public List<EmailAccountDto> GetMailboxesByEmailAddressNameWithoutEncryption(List<string> addressNameList)
        {
            return MailBoxDao.GetMailboxesByEmailAddressName(addressNameList);
        }

        public void CreateMailboxGroups(List<EmailAccountGroupDto> mailGroups)
        {
            MailBoxDao.CreateEmailGroups(mailGroups);
        }

        public void UpdateEmailGroups(List<EmailAccountGroupDto> emailGroups)
        {
            MailBoxDao.UpdateEmailGroups(emailGroups);
        }

        public void DeleteMailboxByNames(List<string> names)
        {
            MailBoxDao.DeleteMailboxByNames(names);
        }

        public void DeleteMailboxByParentIds(List<string> parentIds)
        {
            MailBoxDao.DeleteMailboxByParentIds(parentIds);
        }

        public void DeleteMailboxGroup(List<string> ids)
        {
            MailBoxDao.DeleteEmailGroups(ids);
        }

        public void SyncMailboxs(List<EmailAccountDto> mails)
        {
            //DatabaseEncrypt(mails);
            MailBoxDao.AddEmailsForAutoScan(mails);
        }

        public void UpdateSyncMails(List<SyncRemoteNodePara> mails)
        {
            MailBoxDao.UpdateSyncMails(mails);
        }

        public List<EmailAccountDto> GetEmailsByEmailGroupIdForBrowse(string emailGroupId)
        {
            return MailBoxDao.GetEmailsByEmailGroupIdForBrowse(emailGroupId);
        }
        
        public EmailAccountDto GetMailboxById(string mailId)
        {
            var result = MailBoxDao.GetEmailById(mailId);
            DatabaseDecrypt(result);
            return result;
        }

        public List<EmailAccountDto> GetMailboxesByEmailAddressName(List<string> addressNameList)
        {
            var mails = MailBoxDao.GetMailboxesByEmailAddressName(addressNameList);
            DatabaseDecrypt(mails);
            return mails;
        }

        public ExchangeOnlineTreeNodeDto GetExchangeNodeByIdAndAddress(string id, string address)
        {
            ExchangeOnlineTreeNodeDto result = null;
            try
            {
                EmailAccountDto accountDtoById = GetMailboxById(id);
                EmailAccountDto accountDtoByAddress = GetMailboxesByEmailAddressName(new List<string>() { address }).FirstOrDefault();
                //第一步先通过DAO TreeNodeID获取Mailbox
                if (accountDtoById != null && accountDtoById.State == EmailAccountState.AccessAll)
                {
                    logger.Info($"Current nodeID:{id} can be found in DAO Mailbox.");
                    result = new ExchangeOnlineTreeNodeDto
                    {
                        ID = accountDtoById.Id,
                        Level = accountDtoById.NodeLevel,
                        Name = accountDtoById.Email,
                        DisplayName = accountDtoById.Email,
                        CanChildrenBeLoaded = true,
                        EmailAddress = accountDtoById.Email,
                        FullPath = accountDtoById.Email,
                        MailboxType = accountDtoById.MailboxType,
                        ParentId = accountDtoById.ParentId,
                    };
                }
                //第二步通过DAO Address获取获取MailBox
                else if (accountDtoByAddress != null && accountDtoByAddress.State == EmailAccountState.AccessAll)
                {
                    //logger.Info($"Current address:{address} can be found in DAO Mailbox.");
                    result = new ExchangeOnlineTreeNodeDto
                    {
                        ID = accountDtoByAddress.Id,
                        Level = accountDtoByAddress.NodeLevel,
                        Name = accountDtoByAddress.Email,
                        DisplayName = accountDtoByAddress.Email,
                        CanChildrenBeLoaded = true,
                        EmailAddress = accountDtoByAddress.Email,
                        FullPath = accountDtoByAddress.Email,
                        MailboxType = accountDtoByAddress.MailboxType,
                        ParentId = accountDtoByAddress.ParentId,
                    };
                }
                //第三步通过AOSObjectID获取到对应的Address，然后再通过Address获取AOS的Mailbox
                else
                {
                    logger.Info($"Current nodeID:{id} can not be found in DAO Mailbox and get nodeId from AOS.");
                    string mailBoxAddress = RMAosApiClient.GetMailBoxAddressByAOSObjectID(id);
                    if (!string.IsNullOrEmpty(mailBoxAddress))
                    {
                        EmailAccountDto accountDtoByAOSIDAndAddress = GetMailboxesByEmailAddressName(new List<string>() { mailBoxAddress }).FirstOrDefault();
                        if (accountDtoByAOSIDAndAddress != null && accountDtoByAOSIDAndAddress.State == EmailAccountState.AccessAll)
                        {
                            result = new ExchangeOnlineTreeNodeDto
                            {
                                ID = accountDtoByAOSIDAndAddress.Id,
                                Level = accountDtoByAOSIDAndAddress.NodeLevel,
                                Name = accountDtoByAOSIDAndAddress.Email,
                                DisplayName = accountDtoByAOSIDAndAddress.Email,
                                CanChildrenBeLoaded = true,
                                EmailAddress = accountDtoByAOSIDAndAddress.Email,
                                FullPath = accountDtoByAOSIDAndAddress.Email,
                                MailboxType = accountDtoByAOSIDAndAddress.MailboxType,
                                ParentId = accountDtoByAOSIDAndAddress.ParentId,
                            };
                        }
                    }
                    else
                    {
                        logger.Info($"Current nodeID:{id} can not be found in DAO and AOS.");
                        return null;
                    }
                }
                return result;
            }
            catch (Exception e)
            {
                logger.Error("GetExchangeOnlineTreeNodeDtoByIDAndAddress failed, {0}", e.ToString());
                return null;
            }
        }

        public EmailAccountDto GetO365GroupById(string id)
        {
            var result = MailBoxDao.GetO365GroupById(id);
            DatabaseDecrypt(result);
            return result;
        }

        public BposInfo GetBPOSInfoByExchangeNode(ExchangeOnlineTreeNodeDto treeDto)
        {
            try
            {
                return IsO365(treeDto) ? GetBposInfoByO365GroupNode(treeDto) : GetBposInfoByEmailNode(treeDto);
            }
            catch (NotSupportedException ex)
            {
                logger.Error("Browse exchange tree failed, {0}", ex.ToString());
                throw;
            }

            catch (Exception e)
            {
                logger.Error("Browse exchange tree failed, {0}", e.ToString());
                return null;
            }
        }
        public BposInfo GetBPOSInfoById(string tenantId)
        {
            try
            {
                return GetBposInfoById(tenantId);
            }
            catch (Exception e)
            {
                logger.Error("GetBPOSInfoByExchangeId failed, {0}", e.ToString());
                return null;
            }
        }
        private BposInfo GetBposInfoByO365GroupNode(ExchangeOnlineTreeNodeDto currentNode)
        {
            while(currentNode.Level > NodeLevel.ExchangeOnlineMailbox)
            {
                currentNode = currentNode.Parent;
            }
            var bposInfo = new BposInfo();
            var emailAccount = GetO365GroupById(currentNode.ID);
            if(emailAccount == null)
            {
                logger.Error("Can not find email info by node {0}, nodeId is {1}.", currentNode.Name, currentNode.ID);
                return null;
            }
            bposInfo.ConnectionType = emailAccount.ConnectionType;
            bposInfo.SiteUrl = emailAccount.ServiceUrl;
            var dict = RMAosApiClient.GetMailboxNameToAppProfileDict(new List<EmailAccountDto> { emailAccount }, TenantLocalValue.LogonGroupId);
            bposInfo.UserAccountInfo = new BposUserAccountInfo()
            {
                Username = emailAccount.Username,
                Password = string.Empty,
                TenantId = emailAccount.TenantId
            };
            RMAosApiClient.UpdateBposInfoPasswordForServiceAccount(bposInfo);
            RMAosApiClient.AddBposInfoCertInfoByEmailNode(bposInfo, emailAccount, dict);
            bposInfo.TenantGroupId = TenantLocalValue.LogonGroupId;
            return bposInfo;
        }

        private BposInfo GetBposInfoByEmailNode(ExchangeOnlineTreeNodeDto currentNode)
        {
            while (currentNode.Level > NodeLevel.ExchangeOnlineMailbox)
            {
                currentNode = currentNode.Parent;
            }
            var bposInfo = new BposInfo();
            var emailAccount = GetMailboxById(currentNode.ID);
            if (emailAccount == null)
            {
                logger.Error("Can not find email info by node {0}, nodeId is {1}.", currentNode.Name, currentNode.ID);
                return null;
            }
            var dict = RMAosApiClient.GetMailboxNameToAppProfileDict(new List<EmailAccountDto> { emailAccount }, TenantLocalValue.LogonGroupId, currentNode.UsingModernApp);
            bposInfo.ConnectionType = emailAccount.ConnectionType;
            bposInfo.SiteUrl = emailAccount.ServiceUrl;
            bposInfo.MailboxType = emailAccount.MailboxType;
            bposInfo.UserAccountInfo = new BposUserAccountInfo()
            {
                Username = emailAccount.Username,
                Password = emailAccount.Password,
                TenantId = emailAccount.TenantId
            };
            bposInfo.CustomerId = TenantLocalValue.LogonGroupId;
            RMAosApiClient.UpdateBposInfoPasswordForServiceAccount(bposInfo);
            RMAosApiClient.AddBposInfoCertInfoByEmailNode(bposInfo, emailAccount, dict);
            return bposInfo;
        }
        private BposInfo GetBposInfoById(string tenantId)
        {
            var bposInfo = new BposInfo();
            var appProfile = RMAosApiClient.GetAppProfileForEXOArchiver(tenantId, TenantLocalValue.LogonGroupId);
            bposInfo.UserAccountInfo = new BposUserAccountInfo()
            {
                TenantId = tenantId
            };
            RMAosApiClient.AddBposInfoCertInfoById(bposInfo, appProfile);
            return bposInfo;
        }

        private bool IsO365(ExchangeOnlineTreeNodeDto node)
        {
            var temp = node;
            while (temp != null && temp.Level != NodeLevel.ExchangeOnlineMailbox)
            {
                temp = temp.Parent;
            }
            return temp != null && temp.Type == NodeType.EOO365GroupGroup;
        }

        private void DatabaseDecrypt(EmailAccountDto mail)
        {
            if(mail == null)
            {
                logger.Error("The mailbox is null.");
                return;
            }
            if(mail.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount)
            {
                if (string.IsNullOrEmpty(mail.ServiceAccountId))
                {
                    logger.Error("The service account id of mail is null.");
                    return;
                }
                ConvertToEmailAccount(mail);
            }
            else
            {
                if (string.IsNullOrEmpty(mail.Username))
                {
                    logger.Info("The username column is null.");
                    return;
                }
                mail.Username = RMDatabaseDefaultEncryptor.DecryptToString(mail.Username);
            }
        }

        private void DatabaseDecrypt(List<EmailAccountDto> mails)
        {
            var dicCache = new Dictionary<string, string>();
            if (mails != null)
            {
                var ids = mails.Where(s => !string.IsNullOrEmpty(s.ServiceAccountId)).Select(s => s.ServiceAccountId).Distinct().ToList();
                if (ids == null || ids.Count == 0)
                {
                    logger.Debug("exo with ServiceAccountId is null");
                    return;
                }
                logger.Debug("ServiceAccountIds is {0}", string.Join(",", ids));
                Dictionary<string, ServiceAccount> serviceAccountId2ServiceAccountDic = GetO365ServiceAccounts();
                mails.ForEach(mail =>
                {
                    if (mail.ConnectionType == BposConnectionType.ServiceAccount)
                    {
                        if (string.IsNullOrEmpty(mail.ServiceAccountId))
                        {
                            logger.Error("The service account is null, mailbox is {0}", mail.Email);
                            return;
                        }
                        else
                        {
                            if (serviceAccountId2ServiceAccountDic.ContainsKey(mail.ServiceAccountId))
                            {
                                var serviceAccount = serviceAccountId2ServiceAccountDic[mail.ServiceAccountId];
                                mail.Username = serviceAccount.UserName;
                            }
                            return;
                        }
                    }
                    else
                    {
                        if (string.IsNullOrEmpty(mail.Username))
                        {
                            logger.Error("The user name is null. Auth type is AppProfile.");
                            return;
                        }
                        else
                        {
                            var reg = Regex.Match(mail.Username, @"<CheckSum>.*</CheckSum>");
                            if (reg.Success && !string.IsNullOrEmpty(reg.Value))
                            {
                                var checkSum = reg.Value;
                                if (dicCache.ContainsKey(checkSum))
                                {
                                    mail.Username = dicCache[checkSum];
                                }
                                else
                                {
                                    mail.Username = RMDatabaseDefaultEncryptor.DecryptToString(mail.Username);
                                    lock (lockObj)
                                    {
                                        dicCache.Add(checkSum, mail.Username);
                                    }
                                }
                            }
                            else
                            {
                                mail.Username = RMDatabaseDefaultEncryptor.DecryptToString(mail.Username);
                            }
                        }
                    }
                });
            }
        }

        private void ConvertToEmailAccount(EmailAccountDto mail)
        {
            var serviceAccountId2ServiceAccountDic = GetO365ServiceAccounts();
            if (serviceAccountId2ServiceAccountDic.ContainsKey(mail.ServiceAccountId))
            {
                var serviceAccount = serviceAccountId2ServiceAccountDic[mail.ServiceAccountId];
                mail.Username = serviceAccount.UserName;
            }
        }

        private Dictionary<string, ServiceAccount> GetO365ServiceAccounts()
        {
            if (string.IsNullOrEmpty(TenantLocalValue.LogonGroupId))
            {
                return new Dictionary<string, ServiceAccount>();
            }
            List<Cloud.Sdk.Data.AosModern.ServiceAccount> accounts = new List<Cloud.Sdk.Data.AosModern.ServiceAccount>();
            RetryPolicy.ExecuteAction(() =>
            {
                accounts  = AosApiUtility.GetAosModernClient(TenantLocalValue.LogonGroupId).ServiceAccountService.GetAllAsync().Result;
            });
            //var accounts = PortalUtil.GetServiceAccounts(TenantLocalValue.LogonGroupId);
            return accounts.ToDictionary(account => HashCodeHelper.ToMD5HashCode(account.UserName.ToLowerInvariant()), account => new ServiceAccount() {
                Id = account.Id,
                Name = account.Name,
                UserName = account.UserName,
                Status = (int)account.Status,
                TenantId = account.TenantId,
                AdminUrl = RMAosApiClient.GetO365TenantInfoByIdAsync(account.TenantId).GetAwaiter().GetResult().AdminUrl,
                DomainName = account.DomainName,
            });
        }
    }
}
