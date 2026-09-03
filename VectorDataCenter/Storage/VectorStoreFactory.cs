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
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Configurations;

namespace AvePoint.RA.VectorDataCenter.Storage;

public class VectorStoreFactory
{
    private static readonly RALogger s_logger = RALogger.GetInstance(typeof(VectorStoreFactory));

    public static IVectorStore CreateVectorStore()
    {
        var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
        var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
        s_logger.Info($"Creating vector store for environment: {envName} (GCP: {isGCP})");
        if (isGCP)
        {
            return new PostgresVectorStore();
        }

        return new CosmosDbVectorStore();
    }

    public static bool CheckGCPEnv()
    {
        var envName = RMGlobalConfiguration.EnvSetting[RMEnvSettingKey.ENVIRONMENT_NAME];
        var isGCP = ContractConstants.ENVIRONMENT_NAME_GCP.Contains(envName?.ToLower());
        if (isGCP)
        {
            return true;
        }
        return false;

    }
}
