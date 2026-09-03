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
using AvePoint.RA.Common.Configurations;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.AzureService
{
    public class ListResult<T>
    {
        public List<T> Value { get; set; }
    }

    public class FailoverGroup
    {
        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

        [JsonProperty(PropertyName = "properties")]
        public FailoverGroupProperties Properties { get; set; }

    }

    public class GroupPartnerServers
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "replicationRole")]
        public string ReplicationRole { get; set; }
    }

    public class FailoverGroupProperties
    {
        [JsonProperty(PropertyName = "databases")]
        public IList<string> Databases { get; set; }

        [JsonProperty(PropertyName = "readWriteEndpoint")]
        public FailoverGroupReadWriteEndpoint ReadWriteEndpoint { get; set; }

        [JsonProperty(PropertyName = "partnerServers")]
        public IList<GroupPartnerServers> PartnerServers { get; set; }
    }

    public class FailoverGroupReplicationRole 
    {
        public static string Primary = "Primary";
        public static string Secondary = "Secondary";
    }

    public class FailoverGroupReadWriteEndpoint
    {
        [JsonProperty(PropertyName = "failoverPolicy")]
        public string FailoverPolicy { get; set; }

        [JsonProperty(PropertyName = "failoverWithDataLossGracePeriodMinutes")]
        public int? FailoverWithDataLossGracePeriodMinutes { get; set; }
    }

    public class Database
    {
        public string Id { get; set; }

        public string Name { get; set; }
    }

    public class FailoverParameters
    {
        public string ResourceManager { get; private set; }

        public string SubscriptionId { get; private set; }

        public string ResourceGroupName { get; private set; }

        public string ServerName { get; set; }

        private static readonly Dictionary<string, string> ResourceManagerMapping = new Dictionary<string, string>
        {
            { ".database.windows.net",       "https://management.azure.com" },
            { ".database.usgovcloudapi.net", "https://management.usgovcloudapi.net" },
            { ".database.chinacloudapi.cn",  "https://management.chinacloudapi.cn" },
            { ".database.cloudapi.de",       "https://management.microsoftazure.de" },
        };

        public FailoverParameters(string primaryServer)
        {
            var primaryDBServer = RMGlobalConfiguration.AppConfig.DatabasePrimaryServerName;
            var parts = primaryServer.Split('/');
            SubscriptionId = parts[0];
            ResourceGroupName = parts[1];
            int index = parts[2].IndexOf(".database.");
            ServerName = parts[2].Substring(0, index);
            //优先使用Primary DB
            ServerName = string.IsNullOrEmpty(primaryDBServer) ? ServerName : primaryDBServer.Split('.').First();
            ResourceManager = ResourceManagerMapping[parts[2].Substring(index)];
        }

        public string ToUri()
        {
            return $"{ResourceManager}/subscriptions/{SubscriptionId}/resourceGroups/{ResourceGroupName}/providers/Microsoft.Sql/servers/{ServerName}";
        }
    }
}
