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


namespace Microsoft365.Authentication.ServiceEndPoint.DomainRealm
{
    using System;
    using System.Collections.Generic;
    using System.Text.RegularExpressions;
    using Microsoft365.Authentication;
    using Microsoft365.Common.Cache;
    using Microsoft365.Common.HttpUtil;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;

    internal class AzureDomainRealmInfoProvider //: IAzureDomainRealmProvider
    {
        private static IMicrosoft365Logger logger = Microsoft365LoggerManager.CreateLogger(typeof(AzureDomainRealmInfoProvider));
        static Regex GUIDRegex = new Regex(@"(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}");

        private static Dictionary<string, AzureDomainRealm> defaultDomainUserRealm = new Dictionary<string, AzureDomainRealm>(StringComparer.OrdinalIgnoreCase)
        {
            {
                MicrosoftOnlineInstance.AzureChinaCloud.InitialDomainNameSuffix,
                new AzureDomainRealm()
                {
                    Environment = AveAzureEnvironment.AzureChinaCloud,
                    Domain = MicrosoftOnlineInstance.AzureChinaCloud.InitialDomainNameSuffix,
                }
            },
            // ## Remove this one because the US GOV and Commerical are using the same default domain
            // ## https://docs.microsoft.com/en-us/azure/azure-government/documentation-government-developer-guide 
            //{
            //    MicrosoftOnlineInstance.AzureCloud.InitialDomainNameSuffix,
            //    new AzureDomainRealm()
            //    {
            //        Environment = AveAzureEnvironment.AzureCloud,
            //        Domain = MicrosoftOnlineInstance.AzureCloud.InitialDomainNameSuffix,
            //    }
            //},
            {
                MicrosoftOnlineInstance.AzureGermanyCloud.InitialDomainNameSuffix,
                new AzureDomainRealm()
                {
                    Environment = AveAzureEnvironment.AzureGermanyCloud,
                    Domain = MicrosoftOnlineInstance.AzureGermanyCloud.InitialDomainNameSuffix,
                }
            },
            {
                MicrosoftOnlineInstance.AzurePPE.InitialDomainNameSuffix,
                new AzureDomainRealm()
                {
                    Environment = AveAzureEnvironment.AzurePPE,
                    Domain = MicrosoftOnlineInstance.AzurePPE.InitialDomainNameSuffix,
                }
            },
            {
                MicrosoftOnlineInstance.AzureUSGovernmentCloud.InitialDomainNameSuffix,
                new AzureDomainRealm()
                {
                    Environment = AveAzureEnvironment.USGovernment,
                    Domain = MicrosoftOnlineInstance.AzureUSGovernmentCloud.InitialDomainNameSuffix,
                }
            }
        };

        private static Dictionary<string, AveAzureEnvironment> environmentMapping = new Dictionary<string, AveAzureEnvironment>(StringComparer.OrdinalIgnoreCase)
        {
            {
                MicrosoftOnlineInstance.AzureChinaCloud.MsodsEndpointDomainNameSuffix,
                AveAzureEnvironment.AzureChinaCloud
            },
            {
                MicrosoftOnlineInstance.AzureCloud.MsodsEndpointDomainNameSuffix,
                AveAzureEnvironment.AzureCloud
            },
            {
                MicrosoftOnlineInstance.AzureGermanyCloud.MsodsEndpointDomainNameSuffix,
                AveAzureEnvironment.AzureGermanyCloud
            },
            {
                MicrosoftOnlineInstance.AzurePPE.MsodsEndpointDomainNameSuffix,
                AveAzureEnvironment.AzurePPE
            },
            {
                MicrosoftOnlineInstance.AzureUSGovernmentCloud.MsodsEndpointDomainNameSuffix,
                AveAzureEnvironment.USGovernment
            },
        };

        private static Dictionary<AveAzureEnvironment, MicrosoftOnlineInstanceDetail> environmentInfoMapping = new Dictionary<AveAzureEnvironment, MicrosoftOnlineInstanceDetail>()
        {
            {
                AveAzureEnvironment.AzureChinaCloud, MicrosoftOnlineInstance.AzureChinaCloud
            },
            {
                AveAzureEnvironment.AzureCloud, MicrosoftOnlineInstance.AzureCloud
            },
            {
                AveAzureEnvironment.AzureGermanyCloud, MicrosoftOnlineInstance.AzureGermanyCloud
            },
            {
                AveAzureEnvironment.AzurePPE, MicrosoftOnlineInstance.AzurePPE
            },
            {
                AveAzureEnvironment.None, MicrosoftOnlineInstance.AzureCloud
            },
            {
                AveAzureEnvironment.USGovernment, MicrosoftOnlineInstance.AzureUSGovernmentCloud
            },
            {
                AveAzureEnvironment.USGovernmentDOD, MicrosoftOnlineInstance.AzureUSGovernmentDODCloud
            },
        };

        private IKeyValueCache<string, AzureDomainRealm> cache = new KeyValueCache<string, AzureDomainRealm>(Microsoft365Configuration.AuthenticationConfiguration.TokenSetting.MaxCacheInstance,0, int.MaxValue);

        public void CleanCache()
        {
            cache.Clear();
        }

        public string GetTenantId(string userName) => GetDomainRealmFromCache(userName).TenantId;

        public string GetTenantRegionScope(string username) => GetDomainRealmFromCache(username).TenantRegionScope;

        public string GetTenantRegionSubScope(string username) => GetDomainRealmFromCache(username).TenantRegionSubScope;

        public AveAzureEnvironment GetEnvironment(string userName)
        {
            string domainName = GetDomainName(userName);
            if (SpecialDomainCache.TryGetValue(domainName, out AveAzureEnvironment environment))
            {
                logger.Info($"User:{userName}-> environment:{environment} by SpecialDomainCache");
                return environment;
            }
            var domainRealm = GetDomainRealm(domainName);
            logger.Info($"User:{userName}-> environment:{domainRealm.Environment}");
            return domainRealm.Environment;
        }

        private static Dictionary<string, AveAzureEnvironment> SpecialDomainCache = new Dictionary<string, AveAzureEnvironment>(StringComparer.OrdinalIgnoreCase)
        {
            {"pwcus.com",AveAzureEnvironment.USGovernment },
            {"guidehouse.onmicrosoft.com",AveAzureEnvironment.USGovernment }
        };

        /// <summary>
        /// only get environment, this will check user subfix first, if it is special environment, will return the environment that match the subfix directly. 
        /// If domain subfix match global environment,or is not any environment subfix, will use openidconfiguration to query.
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        private AzureDomainRealm GetDomainRealm(string domainName)
        {
          
            var domainRealm = cache.Get(domainName);

            if (domainRealm == null)
            {
                var index = domainName.IndexOf('.');

                if (index >= 0)
                {
                    var domainNameSuffix = domainName.Substring(index + 1);

                    domainRealm = cache.Get(domainNameSuffix);

                    if (domainRealm == null)
                    {
                        domainRealm = GenerateDefaultDomainRealm(domainNameSuffix);

                        if (domainRealm != null)
                        {
                            cache.AddOrUpdate(domainNameSuffix, domainRealm);
                        }
                    }
                }

                if (domainRealm == null)
                {
                    domainRealm = RetrieveAzureDomainInformation(domainName);

                    cache.AddOrUpdate(domainName, domainRealm);
                }
            }

           

            return domainRealm;
        }

        private static string GetDomainName(string userName)
        {
            var index = userName.IndexOf('@');
            if (index <= 0)
            {
                return userName;
                //throw new ArgumentException("Invalid username:" + userName);
            }

            var domainName = userName.Substring(index + 1);
            return domainName;
        }

        public AzureDomainRealm GenerateDefaultDomainRealm(string domainName)
        {
            AzureDomainRealm userRealm = null;

            defaultDomainUserRealm.TryGetValue(domainName, out userRealm);

            return userRealm;
        }

        private AzureDomainRealm RetrieveAzureDomainInformation(string domainName)
        {
            string requestUrl = string.Format("https://login.microsoftonline.com/{0}/.well-known/openid-configuration", domainName);
            string result = "";
            using (var client = RestClientFactory.CreateSharePointRestClient("Office365Discover"))
            {
                result = client.GetStringAsync(requestUrl).ConfigureAwait(false).GetAwaiter().GetResult();
            }
             
            logger.Info("Retrieve the domain information of {0} -> {1}", domainName, result);

            var info = Newtonsoft.Json.JsonConvert.DeserializeObject<AzureDomainRealmInfo>(result);

            AveAzureEnvironment environment;

            if (SpecialDomainCache.TryGetValue(domainName, out environment))
            {
                logger.Info($"User:{domainName}-> environment:{environment} by SpecialDomainCache");
            }
            else
            {
                if (!environmentMapping.TryGetValue(info.CloudInstanceName, out environment))
                {
                    environment = AveAzureEnvironment.None;
                }
            }

            return new AzureDomainRealm()
            {
                Domain = domainName,
                TenantId = ResolveTenantId(info.AuthorizationEndpoint),
                TenantRegionScope = info.TenantRegionScope,
                TenantRegionSubScope = info.TenantRegionSubScope,
                Environment = environment
            };
        }

        private static string ResolveTenantId(string input)
        {
            if (!string.IsNullOrEmpty(input))
            {
                var match = GUIDRegex.Match(input);

                if (match != null)
                {
                    return match.Value;
                }
            }
            return null;
        }

        private AzureDomainRealm GetDomainRealmFromCache(string username)
        {
            var domainName = GetDomainName(username);
            var domainRealm = cache.Get(domainName);
            if (domainRealm == null)
            {
                domainRealm = RetrieveAzureDomainInformation(domainName);
                cache.AddOrUpdate(domainName, domainRealm);
            }
            return domainRealm;
        }

        public MicrosoftOnlineInstanceDetail GetEnvironmentInfo(AveAzureEnvironment environment)
        {
            return environmentInfoMapping[environment];
        }

        public AveAzureEnvironment GetEnvironmentByMsodsEndpointDomainNameSuffix(string msodsEndpointDomainNameSuffix)
        {
            return environmentMapping[msodsEndpointDomainNameSuffix];
        }
    }
}