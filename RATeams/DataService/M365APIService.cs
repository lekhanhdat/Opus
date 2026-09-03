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
using AvePoint.GCommon.Contract.CentralAdmin.Object;
using ExchangeUtility.Graph;
using ExchangeUtility.Graph.Teams;
using M365.Wrapper.Backup.Auth.Common;
using Util.MSAzure;

namespace M365GroupTeam
{
    public class M365APIService
    {
        public AuthorizationManager authorizationManager { get; set; }

        public IAppTokenAuthObject graphApplicationAuthObj;

        public IAppTokenAuthObject graphDelegateAuthObj;

        public BposInfo BposInfo { get; set; }

        private string currentMailbox;

        public AzureEnvironment Environment { get { return graphApplicationAuthObj.Environment; } }

        public bool IsGovernmentEnvironment => Environment is AzureEnvironment.USGovGCCHigh or AzureEnvironment.USGovDoD;

        public M365APIService(BposInfo bposInfo, string currentMailbox)
        {
            this.BposInfo = bposInfo;
            authorizationManager = AuthorizationManager.Instance;
            this.currentMailbox = currentMailbox;

            graphApplicationAuthObj = AuthObjectFactory4TeamsJob.GetGraphAuthObjectForDelegateCustomApp(bposInfo, TokenPermissionType.Application);
            graphDelegateAuthObj = AuthObjectFactory4TeamsJob.GetGraphAuthObjectForDelegateCustomApp(bposInfo, TokenPermissionType.Delegated);
        }

        private MicrosoftTeamsAPIBase _m365TeamsService;
        public MicrosoftTeamsAPIBase TeamsService
        {
            get
            {
                if (_m365TeamsService == null)
                {
                    //var graphAuthObj = authorizationManager.GetAuthObjectForGraph(currentMailbox);//graph

                    _m365TeamsService = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(graphApplicationAuthObj);
                }

                return _m365TeamsService;
            }
        }

        private MicrosoftTeamsAPIBase _m365TeamsServiceForDelegate;
        public MicrosoftTeamsAPIBase TeamsServiceForDelegate
        {
            get
            {
                if (_m365TeamsServiceForDelegate == null)
                {
                    //var graphAuthObj = authorizationManager.GetAuthObjectForGraph(currentMailbox);//graph

                    _m365TeamsServiceForDelegate = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(graphDelegateAuthObj);
                }

                return _m365TeamsServiceForDelegate;
            }
        }

        private MicrosoftTeamsAPIBase _m365TeamsService4ServiceAccount;
        public MicrosoftTeamsAPIBase TeamsService4ServiceAccount
        {
            get
            {
                if (_m365TeamsService4ServiceAccount == null)
                {
                    //var authObj = authorizationManager.GetAuthObjectForGraph(currentMailbox);
                    //var appTokenAuthObj = authObj as AppTokenAuthObject;
                    //var azureEnvironment = appTokenAuthObj?.Environment;
                    //var authObj4ServiceAccount = authorizationManager.GetAuthObjectForGraph(currentMailbox, AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount);
                    //var hasSA = authObj4ServiceAccount.AuthType == AuthObjectType.PasswordAccessToken;
                    ////logger.Info($"Create Ms365TenantContext. HasSa: {hasSA}.");
                    //_m365TenantContent = new Ms365TenantContext()
                    //{
                    //    TeamService = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObj),
                    //    TeamService4ServiceAccount = hasSA ? ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObj4ServiceAccount) : null,
                    //    Environment = azureEnvironment,
                    //    IsGovernmentEnvironment = azureEnvironment is AzureEnvironment.USGovGCCHigh or AzureEnvironment.USGovDoD
                    //};
                }

                return _m365TeamsService4ServiceAccount;
            }
        }

        private ExchangePlannerService _m365PlannerService;
        public ExchangePlannerService PlannerService
        {
            get
            {
                if (_m365PlannerService == null)
                {
                    if (graphApplicationAuthObj != null)
                    {
                        _m365PlannerService = ExchangeServiceFactory.CreateOffice365Planner(graphApplicationAuthObj);
                    }
                    else
                    {
                        var authObj = authorizationManager.GetPlannerAuth(currentMailbox);
                        if (authObj is ServiceAccout2AppTokenAuthObject || authObj is AOSTokenAuthObjectV2)
                        {
                            var saAuthObj = authObj as ServiceAccout2AppTokenAuthObject;
                            if (saAuthObj.CloudType is AzureEnvironment.China)
                            {
                                return null;//此修改是因为21V不支持Planner
                            }
                        }

                        _m365PlannerService = ExchangeServiceFactory.CreateOffice365Planner(authObj);
                    }
                }
                return _m365PlannerService;
            }
        }

        private ExchangePlannerService _m365PlannerServiceForDelegate;
        public ExchangePlannerService PlannerServiceForDelegate
        {
            get
            {
                if (_m365PlannerServiceForDelegate == null)
                {
                    _m365PlannerServiceForDelegate = ExchangeServiceFactory.CreateOffice365Planner(graphDelegateAuthObj);
                }
                return _m365PlannerServiceForDelegate;
            }
        }

        private Microsoft365GroupServiceBase _365GroupService;
        public Microsoft365GroupServiceBase GroupService
        {
            get
            {
                if (_365GroupService == null)
                {
                    if (graphApplicationAuthObj != null)
                    {
                        _365GroupService = ExchangeServiceFactory.CreateMicrosoft365Group(graphApplicationAuthObj);
                    }
                    else
                    {
                        var authObjSA = authorizationManager.GetAuthObjectForGraph(currentMailbox);
                        _365GroupService = ExchangeServiceFactory.CreateMicrosoft365Group(authObjSA);
                    }
                }
                return _365GroupService;
            }
        }

        private Microsoft365GroupServiceBase _delegateAuth_GroupService;
        public Microsoft365GroupServiceBase DelegateAuth_GroupService
        {
            get
            {
                if(_delegateAuth_GroupService == null)
                {
                    _delegateAuth_GroupService = ExchangeServiceFactory.CreateMicrosoft365Group(graphDelegateAuthObj);
                }
                return _delegateAuth_GroupService;
            }
        }

        private Microsoft365GroupServiceBase _m365GroupService4ServiceAccount;
        public Microsoft365GroupServiceBase GroupService4ServiceAccount
        {
            get
            {
                if (_m365GroupService4ServiceAccount == null)
                {
                    //var authObj = authorizationManager.GetAuthObjectForGraph(currentMailbox);
                    //var appTokenAuthObj = authObj as AppTokenAuthObject;
                    //var azureEnvironment = appTokenAuthObj?.Environment;
                    //var authObj4ServiceAccount = authorizationManager.GetAuthObjectForGraph(currentMailbox, AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount);
                    //var hasSA = authObj4ServiceAccount.AuthType == AuthObjectType.PasswordAccessToken;
                    ////logger.Info($"Create Ms365TenantContext. HasSa: {hasSA}.");
                    //_m365TenantContent = new Ms365TenantContext()
                    //{
                    //    TeamService = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObj),
                    //    TeamService4ServiceAccount = hasSA ? ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObj4ServiceAccount) : null,
                    //    Environment = azureEnvironment,
                    //    IsGovernmentEnvironment = azureEnvironment is AzureEnvironment.USGovGCCHigh or AzureEnvironment.USGovDoD
                    //};
                }

                return _m365GroupService4ServiceAccount;
            }
        }


        private Ms365TenantContext _m365TenantContent;
        public Ms365TenantContext M365TenantContext
        {
            get
            {
                if (_m365TenantContent == null)
                {
                    var authObj = authorizationManager.GetAuthObjectForGraph(currentMailbox);
                    var appTokenAuthObj = authObj as AppTokenAuthObject;
                    var azureEnvironment = appTokenAuthObj?.Environment;
                    var authObj4ServiceAccount = authorizationManager.GetAuthObjectForGraph(currentMailbox, AvePoint.GCommon.Contract.CentralAdmin.Object.BposConnectionType.ServiceAccount);
                    var hasSA = authObj4ServiceAccount.AuthType == AuthObjectType.PasswordAccessToken;
                    //logger.Info($"Create Ms365TenantContext. HasSa: {hasSA}.");
                    _m365TenantContent = new Ms365TenantContext()
                    {
                        TeamService = ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObj),
                        TeamService4ServiceAccount = hasSA ? ExchangeServiceFactory.CreateExchangeMicrosoftTeams(authObj4ServiceAccount) : null,
                        Environment = azureEnvironment,
                        IsGovernmentEnvironment = azureEnvironment is AzureEnvironment.USGovGCCHigh or AzureEnvironment.USGovDoD
                    };
                }
                return _m365TenantContent;
            }
        }
    }
}
