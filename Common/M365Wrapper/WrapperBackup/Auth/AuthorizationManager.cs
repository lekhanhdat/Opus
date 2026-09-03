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
    //using AvePoint.Application.TokenManager.TokenManagement;
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.RA.CommonUtil;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AvePoint.Common;
    using M365.Wrapper.Backup.Auth.Common;

    public class AuthorizationManager:ISingleton
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(AuthObjectFactory));
        public static AuthorizationManager Instance
        {
            get
            {
                return Singleton<AuthorizationManager>.SingletonInstance;
            }
        }
        private AuthorizationManager() { }
        private volatile bool inited = false;
        private Dictionary<string, AuthObject> serviceAccountAuthInfo4Ews;
        private Dictionary<string, AuthObject> appTokenAuthInfo4Ews;
        private Dictionary<string, AuthObject> serviceAccountAuthInfo4Graph;
        private Dictionary<string, AuthObject> appTokenAuthInfo4Graph;
        private List<YammerAppTokenAuthObject> authInfo4Yammer;
        private Dictionary<string, List<AuthObject>> delegateAppAuthInfo4ServiceAccount;
        private Dictionary<string, List<AuthObject>> delegateAppAuthInfo4AppToken;
        private Dictionary<string, AuthObject> exchangePSAuthInfo;

        public AuthObjectFactory AuthObjectFactory = new AuthObjectFactory();

        //public ITokenManagementService TokenManagementService { get; set; }

        //public void Init(Dictionary<string, BposInfo> emailBposInfoMap, int primaryImpersonateUserIndex = 0, params AuthScope[] authScopes)
        //{

          

        //}
        public void Init(Dictionary<string, BposInfo> emailBposInfoMap, int primaryImpersonateUserIndex = 0, params AuthScope[] authScopes)
        {
            var useDefault = (!authScopes?.Any()) ?? true;
            ArgumentNullException.ThrowIfNull(emailBposInfoMap);
            logger.Info($"Initialize the auth object.");
            InitializeAuthDic(emailBposInfoMap.Comparer);

            if (useDefault || authScopes.Contains(AuthScope.Yammer))
            {
                //InitYammerAppAuth(emailBposInfoMap);
            }

            var impersonateUsersCount = emailBposInfoMap.First().Value.UserAccountInfo.ExchangeUserNames?.Count ?? 0;
            foreach (var emailBposInfo in emailBposInfoMap)
            {
                var impersonateUser = new ImpersonateUserInfo { ImpersonateUsers = emailBposInfo.Value.UserAccountInfo.ExchangeUserNames, PrimaryImpersonateUserIndex = primaryImpersonateUserIndex };
                if (string.IsNullOrEmpty(emailBposInfo.Value.TenantGroupId))
                {
                    logger.Info("The current object should not enable the AOS token service. Address:[{0}].", emailBposInfo.Key);
                    throw new ArgumentNullException("CustomerId", "Missing customerId when using token service.");
                }
                if (useDefault || authScopes.Contains(AuthScope.EWS))
                {
                    var ewsAuthObjects = AuthObjectFactory.CreateAOSAuthObjects(emailBposInfo.Value, AuthResourceType.EWS, impersonateUser);
                    AddAuthInfo(ewsAuthObjects, emailBposInfo.Key, AuthResourceType.EWS);
                    RecordEmailBposInfoMap(emailBposInfo.Key, ewsAuthObjects);
                }

                if (useDefault || authScopes.Contains(AuthScope.MicrosoftGraph))
                {
                    var graphAuthObjects = AuthObjectFactory.CreateAOSAuthObjects(emailBposInfo.Value, AuthResourceType.MicrosoftGraph, impersonateUser);
                    AddAuthInfo(graphAuthObjects, emailBposInfo.Key, AuthResourceType.MicrosoftGraph);
                    RecordEmailBposInfoMap(emailBposInfo.Key, graphAuthObjects);
                }

                #region For PS
                if (useDefault || authScopes.Contains(AuthScope.ExchangePS))
                {
                    var psAuthObject = AuthObjectFactory.CreateAOSAuthObjectsForExchagnePS(emailBposInfo.Value, impersonateUser);
                    exchangePSAuthInfo.Add(emailBposInfo.Key, psAuthObject);
                    logger.Info("Backup object for PS: {0}. Account user: {1}.", emailBposInfo.Key, psAuthObject.UserName);
                }
                #endregion

                primaryImpersonateUserIndex++;
                if (primaryImpersonateUserIndex >= impersonateUsersCount) primaryImpersonateUserIndex = 0;
            }
            inited = true;
        }

        private void InitYammerAppAuth(Dictionary<string, BposInfo> emailBposInfoMap)
        {
            //var bposInfo = emailBposInfoMap.FirstOrDefault().Value;
            //var yammerBPOSInfo = bposInfo.ExternalBposInfos?.Find(b => b.AppType is AppType.YammerApp);
            //if (yammerBPOSInfo is null)
            //{
            //    logger.Info("Not found the yammer BPOS info, no need to init the yammer app.");
            //}
            //else
            //{
            //    logger.Info($"Yammer App Info. Admin User: [{yammerBPOSInfo.AppProfileUsername}].");
            //    authInfo4Yammer.Add(new YammerAppTokenAuthObject(TokenManagementService.CreateTokenProvider(bposInfo), yammerBPOSInfo, emailBposInfoMap.Values.First().CustomerId));
            //}
        }

        private void InitializeAuthDic(IEqualityComparer<string> comparer)
        {
            serviceAccountAuthInfo4Ews = new Dictionary<string, AuthObject>(comparer);
            appTokenAuthInfo4Ews = new Dictionary<string, AuthObject>(comparer);
            serviceAccountAuthInfo4Graph = new Dictionary<string, AuthObject>(comparer);
            appTokenAuthInfo4Graph = new Dictionary<string, AuthObject>(comparer);
            authInfo4Yammer = new List<YammerAppTokenAuthObject>();
            delegateAppAuthInfo4ServiceAccount = new Dictionary<string, List<AuthObject>>(comparer);
            delegateAppAuthInfo4AppToken = new Dictionary<string, List<AuthObject>>(comparer);
            exchangePSAuthInfo = new Dictionary<string, AuthObject>(comparer);
        }

        private void AddAuthInfo((AuthObject AppTokenAuthObject, AuthObject ServiceAccountAuthObject, List<AuthObject> DelegateAppAuthObject4ServiceAccounts, List<AuthObject> DelegateAppAuthObject4AppTokens) authInfo, string key, AuthResourceType type)
        {
            switch (type)
            {
                case AuthResourceType.EWS:
                    if (authInfo.AppTokenAuthObject != null)
                    {
                        appTokenAuthInfo4Ews.Add(key, authInfo.AppTokenAuthObject);
                    }
                    if (authInfo.ServiceAccountAuthObject != null)
                    {
                        serviceAccountAuthInfo4Ews.Add(key, authInfo.ServiceAccountAuthObject);
                    }
                    break;
                case AuthResourceType.MicrosoftGraph:
                    if (authInfo.AppTokenAuthObject != null)
                    {
                        appTokenAuthInfo4Graph.Add(key, authInfo.AppTokenAuthObject);
                    }
                    if (authInfo.ServiceAccountAuthObject != null)
                    {
                        serviceAccountAuthInfo4Graph.Add(key, authInfo.ServiceAccountAuthObject);
                    }
                    if (authInfo.DelegateAppAuthObject4ServiceAccounts?.Count > 0)
                    {
                        delegateAppAuthInfo4ServiceAccount.Add(key, authInfo.DelegateAppAuthObject4ServiceAccounts);
                    }
                    if (authInfo.DelegateAppAuthObject4AppTokens?.Count > 0)
                    {
                        delegateAppAuthInfo4AppToken.Add(key, authInfo.DelegateAppAuthObject4AppTokens);
                    }
                    break;
            }
        }

        public void RecordEmailBposInfoMap(string emailAddress, (AuthObject AppTokenAuthObject, AuthObject ServiceAccountAuthObject, List<AuthObject> DelegateAppAuthObject4ServiceAccounts, List<AuthObject> DelegateAppAuthObject4AppTokens) authObjects)
        {
            if (authObjects.AppTokenAuthObject != null && authObjects.ServiceAccountAuthObject != null) logger.Info("Backup object email has two authorize mode.");
            if (authObjects.AppTokenAuthObject != null) logger.Info("Backup object: email. Auth type: app token. Account user: {0}.", authObjects.AppTokenAuthObject.UserName);
            if (authObjects.ServiceAccountAuthObject != null) logger.Info("Backup object: email. Auth type: service account. Account user: {0}.", authObjects.ServiceAccountAuthObject.UserName);
            if (authObjects.DelegateAppAuthObject4ServiceAccounts?.Count > 0) logger.Info("Backup object: email. Auth type: delegate app for service account.");
            if (authObjects.DelegateAppAuthObject4AppTokens?.Count > 0) logger.Info("Backup object: email. Auth type: delegate app for app token.");
        }

        public AuthObject GetAuthObjectForEWS(string mailboxAddress, BposConnectionType connectionType = BposConnectionType.AppToken)
        {
            if (!inited) throw new InvalidOperationException("Current AuthorizationManager instance is not init, please call AuthorizationManager.Init before access other method.");
            if (serviceAccountAuthInfo4Ews.Count == 0 || !serviceAccountAuthInfo4Ews.ContainsKey(mailboxAddress)) return appTokenAuthInfo4Ews[mailboxAddress];
            if (appTokenAuthInfo4Ews.Count == 0 || !appTokenAuthInfo4Ews.ContainsKey(mailboxAddress)) return serviceAccountAuthInfo4Ews[mailboxAddress];
            switch (connectionType)
            {
                case BposConnectionType.ServiceAccount:
                    return serviceAccountAuthInfo4Ews[mailboxAddress];
                case BposConnectionType.AppToken:
                    return appTokenAuthInfo4Ews[mailboxAddress];
                default:
                    return appTokenAuthInfo4Ews[mailboxAddress];
            }
        }

        public AuthObject GetAuthObjectForGraph(string mailboxAddress, BposConnectionType connectionType = BposConnectionType.AppToken)
        {
            if (!inited) throw new InvalidOperationException("Current AuthorizationManager instance is not init, please call AuthorizationManager.Init before access other method.");
            if (serviceAccountAuthInfo4Graph.Count == 0 || !serviceAccountAuthInfo4Graph.ContainsKey(mailboxAddress)) return appTokenAuthInfo4Graph[mailboxAddress];
            if (appTokenAuthInfo4Graph.Count == 0 || !appTokenAuthInfo4Graph.ContainsKey(mailboxAddress)) return serviceAccountAuthInfo4Graph[mailboxAddress];
            switch (connectionType)
            {
                case BposConnectionType.ServiceAccount:
                    return serviceAccountAuthInfo4Graph[mailboxAddress];
                case BposConnectionType.AppToken:
                    return appTokenAuthInfo4Graph[mailboxAddress];
                default:
                    return appTokenAuthInfo4Graph[mailboxAddress];
            }
        }

        public AuthObject GetAuthObjectForExchangePS(string mailboxAddress)
        {
            AuthMonitor.Instance.IncreaseTokenAuthNumber();
            return exchangePSAuthInfo[mailboxAddress];
        }
        public YammerAppTokenAuthObject GetAuthObjectForYammer()
        {
            if (authInfo4Yammer != null && authInfo4Yammer.Count > 0)
            {
                return authInfo4Yammer.FirstOrDefault();
            }
            else
            {
                logger.Error($"Failed to get Yammer Auth Info.");
                return null;
            }
        }

        public AuthObject GetDelegateAppAuthObject(string mailboxAddress, DelegateAppCloudBackupModuleType type, BposConnectionType connectionType = BposConnectionType.AppToken)
        {
            if (!delegateAppAuthInfo4ServiceAccount.ContainsKey(mailboxAddress) && !delegateAppAuthInfo4AppToken.ContainsKey(mailboxAddress))
                return null;
            if (delegateAppAuthInfo4ServiceAccount.Count == 0 || !delegateAppAuthInfo4ServiceAccount.ContainsKey(mailboxAddress))
                return delegateAppAuthInfo4AppToken[mailboxAddress].FirstOrDefault(a => a is AppTokenAuthObject o && o.DelegateAppCloudBackupModuleType.HasFlag(type));
            if (delegateAppAuthInfo4AppToken.Count == 0 || !delegateAppAuthInfo4AppToken.ContainsKey(mailboxAddress))
                return delegateAppAuthInfo4ServiceAccount[mailboxAddress].FirstOrDefault(a => a is AppTokenAuthObject o && o.DelegateAppCloudBackupModuleType.HasFlag(type));
            return connectionType switch
            {
                BposConnectionType.ServiceAccount => delegateAppAuthInfo4ServiceAccount[mailboxAddress].FirstOrDefault(a => a is AppTokenAuthObject o && o.DelegateAppCloudBackupModuleType.HasFlag(type)),
                BposConnectionType.AppToken => delegateAppAuthInfo4AppToken[mailboxAddress].FirstOrDefault(a => a is AppTokenAuthObject o && o.DelegateAppCloudBackupModuleType.HasFlag(type)),
                _ => delegateAppAuthInfo4AppToken[mailboxAddress].FirstOrDefault(a => a is AppTokenAuthObject o && o.DelegateAppCloudBackupModuleType.HasFlag(type)),
            };
        }

        public AuthObject GetPlannerAuth(string groupName)
        {
            var authObj = GetDelegateAppAuthObject(groupName, DelegateAppCloudBackupModuleType.Planner);
            if (authObj != null)
            {
                logger.Info("Delegate app of planner is not null,use delegate app to backup Planner");
                return authObj;
            }
            authObj = GetAuthObjectForGraph(groupName, connectionType: AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount);
            var token = authObj as AOSTokenAuthObjectV2;
            if (token.PermissionType == TokenPermissionType.Delegated)
            {
                logger.Info("Auth permission type is delegated,use service account to backup Planner");
                return authObj;
            }
            else
            {
                logger.Info("Auth permission type is application,use app profile to backup Planner");
                return authObj;
            }
        }
    }
}
