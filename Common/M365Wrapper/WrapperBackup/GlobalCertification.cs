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
    using System.Collections.Concurrent;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;

    using ExchangeCommonWrapper;

    using AvePoint.RA.CommonUtil;

    public class GlobalExchangeSetting
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(GlobalExchangeSetting));

        #region O365 Group Mailbox

        public static bool IsO365GroupMailBox = false;

        #endregion

        #region Get Impersonate Id

        /// <summary>
        /// 此Dictionary保存本次Job中MailBox和ImpersonateId对应关系
        /// </summary>
        private static ConcurrentDictionary<string, string> impersinateIdDictionary = new ConcurrentDictionary<string, string>();

        private static DelayMode dalayMode = DelayMode.Random;

        /// <summary>
        /// 此方法依赖IsO365GroupMailBox属性，请使用前给该属性赋值，有待改进
        /// </summary>
        /// <param name="mailbox"></param>
        /// <param name="authObj"></param>
        /// <returns></returns>
        ///
        public static void SetImpersonateIdToDictionary(string mailbox, AuthorizationManager authorizationManager)
        {
            var ewsAuthObj = authorizationManager.GetAuthObjectForEWS(mailbox);//ews
            var graphAuthObj = authorizationManager.GetAuthObjectForGraph(mailbox);//graph
            SetImpersonateIdToDictionary(mailbox, ewsAuthObj, graphAuthObj);
        }
        public static void SetCurrentGroupDisplayName(string groupAddress, string groupDisPlayName)
        {
            try
            {
                if (ExchangeGlobalConfig.MailboxDisplayNameDic.ContainsKey(groupAddress)) return;
                ExchangeGlobalConfig.MailboxDisplayNameDic[groupAddress] = groupDisPlayName;
            }
            catch (Exception ex)
            {
                logger.Info("Failed to get group[{0}] displayName. Reason:{1}", groupAddress, ex.ToString());
            }
        }

        public static void SetImpersonateIdToDictionary(string mailbox, AuthObject ewsAuthObj, IAuthObject graphAuthObj)
        {
            var impersinateId = string.Empty;
            var haveImpersinateId = impersinateIdDictionary.TryGetValue(mailbox, out impersinateId);
            if (!haveImpersinateId)
            {
                if (IsO365GroupMailBox)
                {
                    try
                    {
                        //SA方式：使用白名单筛选； APP profile方式： 第一次使用白名单筛选，第二次使用黑名单筛选。
                        //由于黑名单的存在漏洞，这次改动是为了保证具有已知license的user不被掩盖。
                        using (var MS365GroupService = ExchangeServiceFactory.CreateMicrosoft365Group(graphAuthObj))
                        {
                            var groupInfo = GetGroupInfo(MS365GroupService, mailbox);
                            SetCurrentGroupDisplayName(mailbox, groupInfo.DisplayName);
                            impersinateId = MS365GroupService.GetGroupUser(mailbox);
                        }
                        if (string.IsNullOrEmpty(impersinateId) && graphAuthObj.AuthType == AuthObjectType.AccessToken)
                        {
                            logger.Warn("Set impersonate by whitelist filter failed, we will use group owner or member.");
                            using (ExchangeUser exchangeUser = ExchangeServiceFactory.CreateExchangeUser(graphAuthObj))
                            {
                                //var retry = new RetryCommon(5, 2, WaitMode.Random, DelayMode.None);
                                //impersinateId = retry.Retry<string, string>(mailbox, exchangeUser.GetO365GroupOwnerOrMember);
                                impersinateId = exchangeUser.GetO365GroupOwnerOrMember(mailbox);
                            }
                        }
                    }
                    catch (ObjectNotFoundException obe)
                    {
                        logger.Error("An error occurred when to get office 365 group info. Exception:{0}.", obe.ToString());
                        throw;
                    }
                    catch (AccessdeniedException ade)
                    {
                        logger.Error("An error occurred when to get office 365 private group users. Exception:{0}.", ade.ToString());
                        throw;
                    }
                    catch (Exception ex)
                    {
                        logger.Error("An error occurred when to get office 365 group users. Exception:{0}.", ex.ToString());
                    }
                    finally
                    {
                        if (string.IsNullOrEmpty(impersinateId))
                        {
                            impersinateId = graphAuthObj.UserName;
                            impersinateIdDictionary.TryAdd(mailbox, impersinateId);
                            logger.Warn($"Get O365 group owner or member with Exception and set service account: {impersinateId} as default");
                        }
                    }
                }
                else
                {
                    impersinateId = mailbox;
                }
                impersinateIdDictionary.TryAdd(mailbox, impersinateId);
            }
        }

        private static Office365GroupEntityV2 GetGroupInfo(Microsoft365GroupServiceBase mS365GroupService, string groupAddress)
        {
            var groupInfo = mS365GroupService.FindGroup(groupAddress);
            if (groupInfo == null) throw new ObjectNotFoundException("Agent.Office365Group.GroupNotExsit_3a71e5f2-2a8a-471a-aad9-9aa4c98ece34");
            return groupInfo;
        }

        /// <summary>
        /// use for brower tree
        /// </summary>
        public static void ClearImpersonateIdDictionary()
        {
            impersinateIdDictionary.Clear();
        }

        public static string GetImpersonateIdByMailbox(string mailbox)
        {
            var impersinateId = string.Empty;
            impersinateIdDictionary.TryGetValue(mailbox, out impersinateId);
            return string.IsNullOrEmpty(impersinateId) ? mailbox : impersinateId;
        }

        /// <summary>
        /// use for brower tree
        /// </summary>
        public static void SetNoneDalayMode()
        {
            dalayMode = DelayMode.None;
        }

        #endregion

        public static ConcurrentDictionary<string, string> archiveMailboxSmtpAddressDictionary = new ConcurrentDictionary<string, string>();

        public static V ConvertClassBySameClassStructure<T, V>(T a) where T : class, new() where V : class, new()
        {
            Newtonsoft.Json.JsonSerializerSettings jSetting = new Newtonsoft.Json.JsonSerializerSettings();
            jSetting.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(a, jSetting);
            logger.Info("[{0}]--->[{1}], Details:[{2}]", typeof(T).Name, typeof(V).Name, json);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<V>(json);
        }

        public static string JsonSerializer<T>(T value)
        {
            Newtonsoft.Json.JsonSerializerSettings jSetting = new Newtonsoft.Json.JsonSerializerSettings();
            jSetting.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            return Newtonsoft.Json.JsonConvert.SerializeObject(value, jSetting);
        }
    }
}