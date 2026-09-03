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
// filepath: c:\work\Code\RECO\Opus\reco\RAContract\AzureService\AzureResourceModels.cs
using System;
using System.Collections.Generic;
using System.Text;

namespace AvePoint.RA.Contract.AzureService
{
    public class AzureResourceModels
    {
        public class Database
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public string ElasticPoolId { get; set; }
            public Properties Properties { get; set; }
            public Sku Sku { get; set; }
        }

        public class ElasticPool
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Location { get; set; }
            public Properties Properties { get; set; }
        }
        public class Properties
        {
            public string Status { get; set; }
            public string State { get; set; }
            public string ElasticPoolId { get; set; }
        }

        public class CreateElasticPoolRequest
        {
            public string Location { get; set; }
            public Sku Sku { get; set; }
            public ElasticPoolProperties Properties { get; set; }
        }

        public class Sku
        {
            public string Name { get; set; }
            public string Tier { get; set; }
            public int Capacity { get; set; }
        }
        public class ElasticPoolProperties
        {
            public ElasticPoolPerDatabaseSettings PerDatabaseSettings { get; set; }
        }

        public class UpdateDatabaseRequest
        {
            public DatabaseProperties Properties { get; set; }
        }

        public class FailoverGroup
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public FailoverGroupProperties Properties { get; set; }
        }

        public class FailoverGroupProperties
        {
            public List<PartnerServer> PartnerServers { get; set; }
            public string ReadWriteEndpoint { get; set; }
            public string ReadOnlyEndpoint { get; set; }
            public List<string> DatabaseIds { get; set; }
        }

        public class PartnerServer
        {
            public string Id { get; set; }
            public string Location { get; set; }
        }

        public class ElasticPoolPerDatabaseSettings
        {
            public int MaxCapacity { get; set; }
        }

        public class DatabaseProperties
        {
            public string ElasticPoolId { get; set; }
        }
    }

    // Moved from DBPoolTaskExecutor.cs
    public static class DBPoolTaskExecutorConstants
    {
        public const int MaxDatabaseInPool = 50;
        public const string ElasticPoolNamePrefix = "elasticpool";
        public const int MaxDatabaseSize = 50; // Size in GB
    }

}
