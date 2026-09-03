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

namespace AvePoint.RA.Contract.CloudService
{
    public class StorageInfo
    {
        public string AccountName { get; set; }
        public string AccessKey { get; set; }
        public StorageAccountType AccountType { get; set; }

        /// <summary>
        /// GB
        /// </summary>
        public int MaxSize { get; set; }

        
    }
    public enum StorageAccountType
    {
        Regular = 0,
        Government = 1,
        AzureChina = 2
    }

    public class TenantStorageSetting
    {
        public TenantStorageType StorageType { get; set; }

        public string AccountName { get; set; }
        public string AccessKey { get; set; }
        public string ContainerName { get; set; }
    }

    public enum TenantStorageType
    {
        Default = 0,
        AzureStorage = 1
    }
}
