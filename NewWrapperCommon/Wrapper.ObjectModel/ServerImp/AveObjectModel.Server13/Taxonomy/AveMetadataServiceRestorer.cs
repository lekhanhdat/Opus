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
using AvePoint.Wrapper.Common;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server13
{
    class AveMetadataServiceRestorer : IAveMetadataServiceRestorer
    {
        private Guid serviceApplicationId;

        public AveMetadataServiceRestorer(Guid serviceAppId)
        {
            this.serviceApplicationId = serviceAppId;
        }

        #region IAveMetadataServiceRestorer Members

        public void Restore(AveManagedMetadataServiceApplicationInfo serviceAppInfo)
        {
            //Get the managed metadata service application object
            Type serviceAppType = Type.GetType(ServiceApplicationType.ManagedMetadataServiceApplication);

            object[] tmpParams = new object[] { this.serviceApplicationId };
            object serviceAppObj = AveAssemblyUtility.InvokeStaticMethod(serviceAppType, "GetApplicationById", tmpParams);

            //Judge if the service application with the source name exists
            object tmpTargetServiceAppObj = AveAssemblyUtility.InvokeStaticMethod(serviceAppType, "GetApplicationByName", serviceAppInfo.Name);

            Type[] paramTypes = new Type[15];
            paramTypes[0] = typeof(string);//service app name
            paramTypes[1] = typeof(string);//db name
            paramTypes[2] = typeof(string);//db server
            paramTypes[3] = typeof(bool);//doSetAuthenticationMode; always "true"
            paramTypes[4] = typeof(string);//db user name
            paramTypes[5] = typeof(string);//db password
            paramTypes[6] = typeof(bool);//doSetFailoverServer; always "true"
            paramTypes[7] = typeof(string);//fail over server
            paramTypes[8] = typeof(SPIisWebServiceApplicationPool);//app pool
            paramTypes[9] = typeof(bool);//unpublishAllPackages
            paramTypes[10] = typeof(string);//hub uri
            paramTypes[11] = typeof(bool);//doSetErrorReport
            paramTypes[12] = typeof(bool);//isErrorReportEnabled
            paramTypes[13] = typeof(int);//cacheCheckInterval
            paramTypes[14] = typeof(int);//maxChannelCache

            //Get application pool object
            Type appPoolType = typeof(SPIisWebServiceApplicationPool);
            SPIisWebServiceApplicationPool appPool = (SPIisWebServiceApplicationPool)AveAssemblyUtility.InvokeStaticMethod(appPoolType, "GetInstance", new object[] { SPFarm.Local, serviceAppInfo.ApplicationPool.Name });

            object[] parameters = new object[15];
            if (tmpTargetServiceAppObj == null)//The service application with the source name does not exist
            {
                parameters[0] = serviceAppInfo.Name;
            }
            else
            {
                string targetServiceAppName = (string)AveAssemblyUtility.GetPropertyValue(serviceAppObj, "Name");
                parameters[0] = targetServiceAppName;
            }

            //Get the target service application DB name            
            string targetServiceAppDBName = (string) AveAssemblyUtility.GetPropertyValue(serviceAppObj, "DatabaseName");
            parameters[1] = targetServiceAppDBName;
            parameters[2] = (string.IsNullOrEmpty(serviceAppInfo.DatabaseServer)) ? string.Empty : serviceAppInfo.DatabaseServer;
            parameters[3] = true;
            parameters[4] = (string.IsNullOrEmpty(serviceAppInfo.SqlAuthenticationUserName)) ? string.Empty : serviceAppInfo.SqlAuthenticationUserName;
            parameters[5] = (string.IsNullOrEmpty(serviceAppInfo.SqlAuthenticationUserPassword)) ? string.Empty : serviceAppInfo.SqlAuthenticationUserPassword;
            parameters[6] = true;
            parameters[7] = (string.IsNullOrEmpty(serviceAppInfo.FailoverDatabaseServer)) ? string.Empty : serviceAppInfo.FailoverDatabaseServer;
            parameters[8] = appPool;
            parameters[9] = false;
            parameters[10] = (string.IsNullOrEmpty(serviceAppInfo.ContentTypeHub)) ? string.Empty : serviceAppInfo.ContentTypeHub;
            parameters[11] = true;
            parameters[12] = serviceAppInfo.IsErrorReportEnabled;

            //Get partion settings            

            object partionSettings = AveAssemblyUtility.InvokeMethod(serviceAppObj, "GetServiceApplicationPartitionSettings");

            object cacheCheckInterval = AveAssemblyUtility.InvokeMethod(partionSettings, "cacheCheckIntervalInSeconds");

            object maxChannelCache = AveAssemblyUtility.GetFieldValue(partionSettings, "maxChannelCacheSize");
            parameters[13] = cacheCheckInterval;
            parameters[14] = maxChannelCache;

            AveAssemblyUtility.InvokeMethod(serviceAppObj, "Update", parameters);
        }

        #endregion
    }
}