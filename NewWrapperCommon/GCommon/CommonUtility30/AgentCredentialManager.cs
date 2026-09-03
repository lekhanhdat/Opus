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



namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using AvePoint.GCommon.Contract.PlatformRecovery;
    using AvePoint.GCommon.Contract.Server.ControlPanel;
    using AvePoint.GCommon.Contract.Server.ControlPanel.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.GCommon.Utility.Cryptography;
    using System.Xml;

    #endregion

    public class AgentCredentialManager
    {
        static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static Tuple<string, string, string> inner = null;
        private static DateTime cacheTime = DateTime.MinValue;
        private static object innerLocker = new object();
        //public static Tuple<string, string, string> GetAgentCredential(bool encryptPasswordUsingCommunicationKey = true)
        //{
        //    if(NeedRefresh())
        //    {
        //        lock(innerLocker)
        //        {
        //            if (NeedRefresh())
        //            {
        //                RefreshCache();
        //            }
        //        }
        //    }

        //    return inner;
        //}
        public static void ClearAgentCredentialCache()
        {
            //inner = null;
            //cacheTime = DateTime.MinValue;
            AgentCacheManager.ClearCachedAgentCredential();
            logger.Debug("Agent credential cache cleared.");
        }


        //private static bool NeedRefresh()
        //{
        //    if(inner == null || cacheTime.AddMinutes(15) < DateTime.Now)
        //    {
        //        return true;
        //    }else
        //    {
        //        return false;
        //    }
        //}

        //private static void RefreshCache(bool encryptPasswordUsingCommunicationKey = true)
        //{
        //    logger.Debug("Start to retrive credential information from manager.");
        //    IMAgentService agentControlService = WcfUtility.GetManagerService<IMAgentService>();//CustomizeChannelFactory<IMAgentService>.CreateManagerChannel();
        //    var agent = agentControlService.GetAgent(new AgentQueryDto { AgentName = AveEnv.AgentName, AgentAddress = AveEnv.AgentAddress });
        //    if (agent == null) throw new Exception(String.Format("can't find agent, Name: {0} Address: {1}", AveEnv.AgentName, AveEnv.AgentAddress));

        //    var password = agent.Password;
        //    if (!encryptPasswordUsingCommunicationKey)
        //    {
        //        password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(password));
        //    }

        //    inner = new Tuple<string, string, string>(agent.Domain, agent.UserName, agent.Password);
        //    cacheTime = DateTime.Now;
        //}

        public static Tuple<string, string, string> GetAgentCredential(bool encryptPasswordUsingCommunicationKey = true)
        {
            var domain = String.Empty;
            var username = String.Empty;
            var password = String.Empty;
            try
            {
                Tuple<string, string, string> credential = AgentCacheManager.GetCachedAgentCredential(encryptPasswordUsingCommunicationKey);
                domain = credential.Item1;
                username = credential.Item2;
                password = credential.Item3;
            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while retrieving agent account from cache file. Exception details: {0}", ex.ToString());
                IMAgentService agentControlService = WcfUtility.GetManagerService<IMAgentService>();//CustomizeChannelFactory<IMAgentService>.CreateManagerChannel();
                var agent = agentControlService.GetAgent(new AgentQueryDto { AgentName = AveEnv.AgentName, AgentAddress = AveEnv.AgentAddress });
                if (agent == null) throw new Exception(String.Format("can't find agent, Name: {0} Address: {1}", AveEnv.AgentName, AveEnv.AgentAddress));
                domain = agent.Domain;
                username = agent.UserName;
                password = agent.Password;
                if (!string.IsNullOrEmpty(domain)
                    && !string.IsNullOrEmpty(username)
                    && !string.IsNullOrEmpty(password))
                {
                    AgentCacheManager.PersistAgentCredential(domain, username, password, true);
                }
                if (!encryptPasswordUsingCommunicationKey)
                {
                    password = CryptoUtil.ConvertBytesToString(CspCommunicationWrapper.UnWrapKey(password));
                }
            }
            return new Tuple<string, string, string>(domain, username, password);
        }

    }

}

