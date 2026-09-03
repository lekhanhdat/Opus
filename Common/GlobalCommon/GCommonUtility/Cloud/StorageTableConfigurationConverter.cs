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
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using System;

namespace AvePoint.GCommon.Contract.Server.Audit.Object
{
    public static class StorageTableConfigurationConverter
    {
        public static CloudTable ConvertToCloudTable(this StorageTableConfigurationSetting setting)
        {
            CloudTable table = null;
            if (setting != null)
            {
                string connectionString = null;
                 if (!string.IsNullOrEmpty(setting.EndPoint))
                {
                    connectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};TableEndpoint={2}", setting.AccountName, setting.AccountKey, setting.EndPoint);
                }
                else
                {
                    connectionString = string.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1};", setting.AccountName, setting.AccountKey);
                }
                 CloudStorageAccount storageAccount = CloudStorageAccount.Parse(connectionString);
                 CloudTableClient tableClient = storageAccount.CreateCloudTableClient();
                 if (string.IsNullOrEmpty(setting.TableName))
                 {
                     tableClient.GetServiceProperties();
                 }
                 else
                 {
                     table = tableClient.GetTableReference(setting.TableName);
                 }
            }
            return table;
        }
    }
}
