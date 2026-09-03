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
using AvePoint.GCommon;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365Account.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common.Common.Utility
{
    public class AveAppProfileUtility
    {
        private static IAveLogger logger = AveLogger.GetInstance(typeof(AveAppProfileUtility));
        private readonly static object mLock = new object();
        //key is tenant id, value is Dictionary(key is app profile client id) for app profiles
        private static Dictionary<Guid, Dictionary<string, AppProfileObject>> mProfileList = new Dictionary<Guid, Dictionary<string, AppProfileObject>>();
        private static int mTicks = 5;
        //key is tenant id, value is current app profile client id
        private static Dictionary<Guid, string> mCurrentAppProfileId = new Dictionary<Guid, string>();
        //key is tenant id, value is enable status
        private static Dictionary<Guid, bool> mTenantEnabledDic = new Dictionary<Guid, bool>();
        public static bool HasInit(Guid tenantId)
        {
            if (mTenantEnabledDic.ContainsKey(tenantId))
            {
                return mTenantEnabledDic[tenantId];
            }
            return false;
        }
        

        public static void SetCurrentAppProfileId(Guid tenantId, string appId)
        {
            if (mCurrentAppProfileId == null)
            {
                mCurrentAppProfileId = new Dictionary<Guid, string>();
            }
            mCurrentAppProfileId[tenantId] = appId;
        }

        //提供一个方法，可以在执行query时定期切换app profile。目前只支持cloud archiver，其他模块请不要调用!
        public static void Init(Guid tenantId, List<BposInfo> profiles)
        {
            try
            {
                lock (mLock)
                {
                    if (mProfileList.ContainsKey(tenantId))
                    {
                        mProfileList[tenantId] = new Dictionary<string, AppProfileObject>();
                    }
                    else
                    {
                        mProfileList.Add(tenantId, new Dictionary<string, AppProfileObject>());
                    }
                    profiles.ForEach(p =>
                    {
                        var currentTenantAppProfiles = mProfileList[tenantId];
                        if (p.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.AppToken
                        || p.ConnectionType == GCommon.Contract.CentralAdmin.Object.BposConnectionType.Modern)
                        {
                            var clientId = p.ConvertToAveBPOSAccountInfo().ClientId;
                            if (!string.IsNullOrWhiteSpace(clientId))
                            {
                                if (!currentTenantAppProfiles.ContainsKey(clientId))
                                {
                                    currentTenantAppProfiles.Add(clientId, new AppProfileObject
                                    {
                                        UsedTimes = 0,
                                        BposInfo = p
                                    });
                                }
                                else
                                {
                                    logger.Info("App profile with the same client id has already been added.");
                                }
                            }
                            else
                            {
                                logger.Warn("Client id is null");
                            }
                        }
                        else
                        {
                            logger.Warn("ConnectionType is not AppToken, user account name:" + p.UserAccountInfo?.Username);
                        }
                    });
                    if (mProfileList.ContainsKey(tenantId))
                    {
                        if (mProfileList[tenantId].Count > 1)
                        {
                            logger.Info("App profile count:" + mProfileList[tenantId].Count);
                            mTenantEnabledDic[tenantId] = true;
                        }
                        else
                        {
                            logger.Info("App profile count:" + mProfileList[tenantId].Count);
                            mTenantEnabledDic[tenantId] = false; ;
                        }
                    }
                }
            }
            catch (Exception e)
            {
                logger.Error("An error occurred while init multiple app profile utility, error:{0}", e.ToString());
                mTenantEnabledDic[tenantId] = false;
            }
            logger.Info("AveAppProfileUtil HasInit:{0} TenantId:{1}", mTenantEnabledDic[tenantId], tenantId);
        }

        public static BposInfo ChangeAppProfile(Guid tenantId)
        {
            if (HasInit(tenantId))
            {
                lock (mLock)
                {
                    try
                    {
                        //get app profile with best performance
                        var profileObjectList = mProfileList[tenantId].Values.OrderBy(p => p.Weight).ToList();
                        AppProfileObject profileObject = null;
                        if(mCurrentAppProfileId?.ContainsKey(tenantId) == true)
                        {
                            profileObject = profileObjectList.Where(profile => profile?.BposInfo?.ConvertToAveBPOSAccountInfo()?.ClientId != mCurrentAppProfileId[tenantId]).First();
                        }
                        else
                        {
                            profileObject = profileObjectList.First();
                        }
                        mCurrentAppProfileId[tenantId] = profileObject.BposInfo.ConvertToAveBPOSAccountInfo().ClientId;
                        profileObject.UsedTimes++;
                        logger.Debug($"Wrapper used multiple app [{profileObject.BposInfo?.AppType} - {profileObject.BposInfo?.UserAccountInfo?.AppId}].");
                        return profileObject.BposInfo;
                    }
                    catch (Exception e)
                    {
                        logger.Warn("An error occurred while changing app profile. Error:{0}", e.ToString());
                        return null;
                    }
                }
            }
            return null;
        }

        //if 429 issue occurrs while using current app profile, should set this app profile blocked
        public static void SetBlockStatus(Guid tenantId, int afterTime)
        {
            if (HasInit(tenantId))
            {
                lock (mLock)
                {
                    try
                    {
                        DateTime estimatedAvailableTime = DateTime.Now.AddSeconds(afterTime);
                        if (mProfileList.ContainsKey(tenantId) && mCurrentAppProfileId.ContainsKey(tenantId))
                        {
                            if (mProfileList[tenantId].ContainsKey(mCurrentAppProfileId[tenantId]))
                            {
                                mProfileList[tenantId][mCurrentAppProfileId[tenantId]].EstimatedAvailableTime = estimatedAvailableTime;
                                logger.Info("Block current app profile, Id:{0} EstimatedAvailableTime:{1}", mProfileList[tenantId][mCurrentAppProfileId[tenantId]], estimatedAvailableTime);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Info("An error occurred while setting app profile block status. Profile id:{0} Error:{1}", mCurrentAppProfileId, e.ToString());
                    }
                }
            }
        }

        //if execute query successully with current app profile, should set this app profile available
        public static void ClearBlockStatus(Guid tenantId)
        {
            if (HasInit(tenantId))
            {
                lock (mLock)
                {
                    try
                    {
                        if (mProfileList.ContainsKey(tenantId) && mCurrentAppProfileId.ContainsKey(tenantId))
                        {
                            if (mProfileList[tenantId].ContainsKey(mCurrentAppProfileId[tenantId]))
                            {
                                mProfileList[tenantId][mCurrentAppProfileId[tenantId]].EstimatedAvailableTime = DateTime.Now;
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        logger.Info("An error occurred while clearing app profile block status. Profile id:{0} Error:{1}", mCurrentAppProfileId, e.ToString());
                    }
                }
            }
        }
    }

    public class AppProfileObject
    {
        //record how many times this app profile has been used.
        public long UsedTimes { get; set; }

        public BposInfo BposInfo { get; set; }

        public DateTime EstimatedAvailableTime { get; set; }

        public string Weight
        {
            get
            {
                return string.Format(
                    "{0}{1}",
                    Blocked ? 1 : 0,
                    Blocked ? EstimatedAvailableTime.Ticks.ToString() : UsedTimes.ToString().PadLeft(18, '0')
                );
            }
        }
        public bool Blocked
        {
            get
            {
                return this.EstimatedAvailableTime > DateTime.Now;
            }
        }

    }
}
