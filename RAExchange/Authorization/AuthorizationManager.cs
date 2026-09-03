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
using AvePoint.Common;
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using ExchangeUtility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RAExchange.Authorization
{
    public class AuthorizationManager : ISingleton
    {
        private volatile bool inited = false;

        private Dictionary<string, AuthObject> authorizationInfo;

        private Dictionary<string, AuthObject> authorizationInfoWindowsGraph;

        private AuthorizationManager() { }

        public void Init(Dictionary<string, BposInfo> emailBposInfoMap)
        {
            if (emailBposInfoMap == null) throw new ArgumentNullException("emailBposInfoMap");

            this.authorizationInfo = emailBposInfoMap.ToDictionary(kv => kv.Key, kv => AuthObjectFactory.CreateAuthObject(kv.Value, AuthResourceType.EWS));
            this.authorizationInfoWindowsGraph = emailBposInfoMap.ToDictionary(kv => kv.Key, kv => AuthObjectFactory.CreateAuthObject(kv.Value, AuthResourceType.MicrosoftGraph));
            this.inited = true;
        }

        public IEnumerable<AuthObject> AuthObjects
        {
            get
            {
                return this.authorizationInfo.Values;
            }
        }

        public AuthObject GetAuthObject(string mailboxAddress)
        {
            if (!this.inited) throw new InvalidOperationException("Current AuthorizationManager instance is not init, please call AuthorizationManager.Init before access other method.");

            return this.authorizationInfo[mailboxAddress];
        }

        public AuthObject GetAuthObjectWindowsGraph(string mailboxAddress)
        {
            if (!this.inited) throw new InvalidOperationException("Current AuthorizationManager instance is not init, please call AuthorizationManager.Init before access other method.");

            return this.authorizationInfoWindowsGraph[mailboxAddress];
        }

        public static AuthorizationManager Instance
        {
            get
            {
                return Singleton<AuthorizationManager>.SingletonInstance;
            }
        }
    }
}
