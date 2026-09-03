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
using AvePoint.RA.DB.AzureTable.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.DB.AzureTable
{
    public class RMArchiverStorageAzureTableContext : RMAzureTableContext
    {

        private static RMArchiverStorageAzureTableContext Context;

        private static readonly object Locker = new();

        private RMArchiverStorageAzureTableContext(string connectionString) : base(connectionString) { }

        public static RMArchiverStorageAzureTableContext GetInstance(
            string accountName,
            string accountKey,
            string endpoint
        )
        {
            if (Context == null)
            {
                lock (Locker)
                {
                    if(Context == null)
                    {
                        if (string.IsNullOrEmpty(accountKey) || string.IsNullOrEmpty(accountName))
                        {
                            Context = new(endpoint);
                        }
                        else
                        {
                            var connectionString = $"DefaultEndpointsProtocol=https;AccountName={accountName};AccountKey={accountKey};";
                            if (endpoint != null)
                            {
                                connectionString += $"TableEndpoint={endpoint}";
                            }

                            Context = new(connectionString);
                        }
                    }
                }
            }

            return Context;
        }

        public RMAzureTableDataSet<RMManualArchiverSharePointOnlineTableEntity> ManualArchiverSharePointOnlineItems
            => new(this, "SOArchiverDB", true);

        public RMAzureTableDataSet<RMManualArchiverExchangeTableEntity> ManualArchiverExchangeItems
            => new(this, "SOExchangeOnlineDB", true);
    }
}
