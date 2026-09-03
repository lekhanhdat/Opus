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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.Configurations
{
    public class RMSecretNames
    {
        //Databases
        public static readonly string DB_Control = "Database--Control";
        public static readonly string DB_Cosmos_URI = "Database--Cosmos--URI";
        public static readonly string DB_Cosmos_Secret = "Database--Cosmos--Secret";
        
        //KeyVault
        public static readonly string DAO_KeyVault_CertThumbprint = "DAO--KeyVault--CertThumbprint";
        public static readonly string DAO_KeyVault_ClientId = "DAO--KeyVault--ClientId";
        public static readonly string DAO_KeyVault_Url = "DAO--KeyVault--Url";

        //Service Bus
        public static readonly string JobQueue = "JobQueue--ConnectionString";
        
        //Storage
        public static readonly string LogStorage = "Storage--Log";
        public static readonly string ReportStorage = "Storage--Report";
        public static readonly string JobContextStorage = "Storage--JobContextStorage";
        public static readonly string AzureTableStorage = "Storage--AzureTable";
        public static readonly string RecordsHistoryStorage = "Storage--RecordsHistory";

        //Others
        public static readonly string NotificationSetting = "Notification--Setting";
        public static readonly string ClientSecret = "RelatedApp--ClientSecret";
        public static readonly string SecondaryClientSecret = "RelatedApp--SecondaryClientSecret";

        public static readonly string RedisConnectionString = "Redis--connection";

        public static readonly string PortalTopicConnectionString = "PortalTopic--connection";

        //Proudct certs
        public static readonly string Cert_Records = "CertificateName--Records";
        public static readonly string Cert_DAO = "CertificateName--DAO";
        public static readonly string Cert_OfficeConnect = "CertificateName--OfficeConnect";
        public static readonly string Cert_Records_Encryption = "CertificateName--Records--Encryption";


    }

    public class RMCertNames
    {
        public static readonly string AvePointRecordsEncryption = "AvePointRecordsEncryption";
        public static readonly string AvePointRecords = "AvePointRecords";
        public static readonly string OfficeConnect = "OfficeConnect";
        public static readonly string DocAveOnline = "DocAveOnline";
    }

    
}
