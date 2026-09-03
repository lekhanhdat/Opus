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
    using ExchangeUtility.Graph.PowerShellRestAPI;
    using M365.Wrapper.Backup.Auth.Common;

    public class ExchangeServiceFactory
    {
        /// <summary>
        /// use Graph Api
        /// </summary>
        public static Microsoft365GroupServiceBase CreateMicrosoft365Group(IAuthObject authObj)
        {
            var tokenObj = authObj as IAppTokenAuthObject;
            if (tokenObj != null)//sa or app profile
            {
                return new Microsoft365GroupServiceWithGraph(tokenObj);
            }
            throw AssemblyNotSupportException(authObj);
        }
        public static MicrosoftTeamsAPIBase CreateExchangeMicrosoftTeams(IAuthObject authObj)
        {
            var tokenObj = authObj as IAppTokenAuthObject;
            if (tokenObj != null)//sa or app profile
            {
                return new MicrosoftTeamsWithGraph(tokenObj);
            }
            throw AssemblyNotSupportException(authObj);
        }
        public static ExchangePlannerService CreateOffice365Planner(IAuthObject authObj)
        {
            var tokenObj = authObj as IAppTokenAuthObject;
            if (tokenObj.PermissionType == TokenPermissionType.Delegated)
            {
                return new ExchangePlannerDelegateService(tokenObj);
            }
            else
            {
                return new ExchangePlannerAppService(tokenObj);
            }
        }
        public static OutlookService CreateOutlookService(IAuthObject authObj)
        {
            return new OutlookService(authObj);
        }
        public static ExchangeUser CreateExchangeUser(IAuthObject authObj)
        {
            return new ExchangeUserWithGraph(authObj as IAppTokenAuthObject);
        }

        /// <summary>
        /// Use Yammer API
        /// </summary>
        /// <param name="authObj"></param>
        /// <param name="exportLocation"></param>
        /// <returns></returns>
        public static YammerGroupServiceBase CreateYammerGroup(IAuthObject authObj, string exportLocation)
        {
            var tokenObj = authObj as YammerAppTokenAuthObject;
            if (tokenObj != null) //yammer app profile
            {
                return new YammerGroupSericeWithYammerApp(tokenObj, exportLocation);
            }
            throw AssemblyNotSupportException(authObj);
        }

        private static Exception AssemblyNotSupportException(IAuthObject authObj)
        {
            return new ArgumentException(string.Format("Unsupport AuthType: {0}", authObj.AuthType));
        }

    }
}