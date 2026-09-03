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



using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Reflection;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.Wrapper.Common.Office;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using AvePoint.Wrapper.Resource.ServerAPI2010;
using Microsoft.Office.Server.SocialData;
using Microsoft.Office.Server.UserProfiles;

namespace AvePoint.ObjectModel.Server19.Office
{
    abstract class AveOSocialDataManager : IAveOSocialDataManager
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private SocialDataManager mSocialDataManager;
        private AveOProfileLoader mProfileLoader;

        public AveOSocialDataManager(SocialDataManager socialDataManager)
        {
            mSocialDataManager = socialDataManager;
        }

        public bool IsSocialAdmin
        {
            get { return mSocialDataManager.IsSocialAdmin; }
        }

        internal SocialDataManager SocialDataManager
        {
            get { return mSocialDataManager; }
        }

        public Guid PartitionID
        {
            get { return (Guid)AveAssemblyUtility.GetPropertyValue(mSocialDataManager, "PartitionID"); }
        }

        public IAveOUserProfileApplicationProxy UserProfileApplicationProxy
        {
            get
            {
                object userProfileApplicationProxy = AveAssemblyUtility.GetPropertyValue(mSocialDataManager, "UserProfileApplicationProxy");
                if (userProfileApplicationProxy == null)
                {
                    return null;
                }
                return new AveOUserProfileApplicationProxy(userProfileApplicationProxy);
            }
        }

        public IAveOProfileLoader ProfileLoader
        {
            get
            {
                if (mProfileLoader == null)
                {
                    mProfileLoader = new AveOProfileLoader(AveAssemblyUtility.GetPropertyValue(mSocialDataManager, typeof(SocialDataManager), "ProfileLoader"));
                }
                return mProfileLoader;
            }
        }

        protected void GetBulkUserProfiles(List<long> rgUserRecordIds, Dictionary<long, string> nameDic)
        {
            int i = 0;
            int j = 0;
            Dictionary<int, int> indexDic = new Dictionary<int, int>();
            List<long> ids = new List<long>();
            foreach (long id in rgUserRecordIds)
            {
                if (!nameDic.ContainsKey(id))
                {
                    indexDic.Add(j, i);
                    ids.Add(id);
                    j++;
                }
                i++;
            }
            List<UserProfile> userProfiles = AveAssemblyUtility.InvokeMethod(mSocialDataManager, typeof(SocialDataManager), "GetBulkUserProfiles", new Type[] { typeof(List<long>) }, new object[] { ids }) as List<UserProfile>;
            for (int index = 0; index < userProfiles.Count; index++)
            {
                try
                {
                    nameDic[rgUserRecordIds[indexDic[index]]] = userProfiles[index].MultiloginAccounts[0];
                }
                catch (Exception e)
                {
                    log.Log(AveLogLevel.DEBUG, ServerAPIResource.GetUserProfPropertyError, e.ToString());
                }
            }
        }

        public void ExecuteNonQuery(SqlCommand command)
        {
            this.UserProfileApplicationProxy.SocialDBSqlSession.ExecuteNonQuery(command);
        }

        public SqlDataReader ExecuteReader(SqlCommand command)
        {
            return this.UserProfileApplicationProxy.SocialDBSqlSession.ExecuteReader(command);
        }
    }
}
