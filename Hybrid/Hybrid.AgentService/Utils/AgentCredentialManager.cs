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



namespace AvePoint.Hybrid.AgentService
{
    #region using directives
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Reflection;
    using System.Threading;
    using AvePoint.GCommon;
    using System.Xml;
    using AvePoint.RA.CommonUtil;
    using AvePoint.Hybrid.Utility;

    #endregion

    public class AgentCredentialManager
    {
        static AvePoint.GCommon.AveLogger logger = new AvePoint.GCommon.AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        private static AveTuple<string, string, string> inner = null;
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


        public static AveTuple<string, string, string> GetAgentCredential(bool encryptPasswordUsingCommunicationKey = true)
        {
            var domain = String.Empty;
            var username = String.Empty;
            var password = String.Empty;
            try
            {
                AveTuple<string, string, string> credential = AgentCacheManager.GetCachedAgentCredential(encryptPasswordUsingCommunicationKey);
                if(credential != null)
                {
                    domain = credential.ItemA;
                    username = credential.ItemB;
                    password = credential.ItemC;
                }

            }
            catch (Exception ex)
            {
                logger.Warn("An error occurred while retrieving agent account from cache file. Exception details: {0}", ex.ToString());
                //IMAgentService agentControlService = WcfUtility.GetManagerService<IMAgentService>();//CustomizeChannelFactory<IMAgentService>.CreateManagerChannel();
                //var agent = null;//agentControlService.GetAgent(new AgentQueryDto { AgentName = AveEnv.AgentName, AgentAddress = AveEnv.AgentAddress });
                throw ex;

            }
            return new AveTuple<string, string, string>(domain, username, password);
        }

    }

}

