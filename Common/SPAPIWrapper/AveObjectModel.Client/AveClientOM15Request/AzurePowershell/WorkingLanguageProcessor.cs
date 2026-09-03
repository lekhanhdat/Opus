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
namespace AvePoint.ObjectModel.ClientOM
{
    using AvePoint.GCommon;
    using AvePoint.Wrapper.Common;
    using Microsoft.SharePoint.Client;
    using Microsoft.SharePoint.Client.UserProfiles;
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using AvePoint.Wrapper.Common.Graph;

    class WorkingLanguageProcessor
    {
        static WorkingLanguageProcessor()
        {
            UserLanguageCache = new Dictionary<string, string> { };
        }
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(WorkingLanguageProcessor));
        private static Dictionary<string,string> UserLanguageCache { get; set; }

        private readonly static object mLock = new object();

        public int GetWorkingLanguage(ClientContext context,Web web,AveBPOSAccountInfo account)
        {
            context.Load(web, w => w.Language, w => w.IsMultilingual, w => w.SupportedUILanguageIds);
            context.ExecuteQuery();
            if (!web.IsMultilingual)
            {
                mLogger.Info("IsMultilingual is not enabled for current site.");
                return (int)web.Language;
            }
            string cultureName;

            lock (mLock)
            {
                if (UserLanguageCache.ContainsKey(account.UserName))
                {
                    cultureName = UserLanguageCache[account.UserName];
                    mLogger.Info($"Find cached culture {cultureName} for user {account.UserName}");
                }
                else
                {
                    if (!TryGetCurrentUserLanguageFromUPS(context, out cultureName))
                    {
                        if (!TryGetCurrentUserLanguage(account, out cultureName))
                        {
                            //error while get user culture, use web default language
                            return (int)web.Language;
                        }
                    }
                    UserLanguageCache[account.UserName] = cultureName;
                }
            }

            CultureInfo userCulture = null;
            if (!string.IsNullOrEmpty(cultureName))
            {
                userCulture = new CultureInfo(cultureName);
                if (web.SupportedUILanguageIds.Contains(userCulture.LCID))
                {
                    return userCulture.LCID;
                }
                else
                {
                    return (int)web.Language;
                }
            }
            else
            {
                if (web.SupportedUILanguageIds.Contains(1033))
                {
                    //return secondary, because if user is no language preference,
                    //site has English as secondary language,the site will display as English
                    return 1033;
                }
                else
                {
                    //return secondary, because if user is no language preference,
                    //site will display as the site's secondary langauge
                    return web.SupportedUILanguageIds.FirstOrDefault();
                }
                
            }
        }

        private static bool TryGetCurrentUserLanguage(AveBPOSAccountInfo account, out string language)
        {
            bool success = false;
            language = "";
            try
            {
                language = GraphHelper.GetCurrentUserPreferedLanguage(account);
                mLogger.Error($"Get CurrentUser Language success.User:{account?.UserName},Language:{language}");
                success = true;
            }
            catch (Exception e)
            {
                mLogger.Error("Get current web working language failed.Error:{0}", e);
            }
            return success;
        }

        private static bool TryGetCurrentUserLanguageFromUPS(ClientContext context, out string language)
        {
            bool success = false;
            language = string.Empty;
            string preferLanguageKey = "SPS-MUILanguages";
            try
            {
                PeopleManager peopleManager = new PeopleManager(context);
                PersonProperties personProperties = peopleManager.GetMyProperties();
                context.Load(personProperties, p => p.AccountName, p => p.UserProfileProperties);
                context.ExecuteQuery();

                var preferLang = personProperties.UserProfileProperties.ContainsKey(preferLanguageKey) ? personProperties.UserProfileProperties[preferLanguageKey] : string.Empty;
                if (!string.IsNullOrEmpty(preferLang))
                {
                    int isMulti = preferLang.IndexOf(",");
                    language = isMulti > 0 ? preferLang.Substring(0, isMulti) : preferLang;
                    success = true;
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Get current web working language from UPS failed.Error:{0}", ex);
            }
            mLogger.Info($"TryGetCurrentUserLanguageFromUPS.Result:{success},Language:{language}");
            return success;
        }
    }
}
