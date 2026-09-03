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
using Microsoft.WindowsAzure.ServiceRuntime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ManualScaleIn
{
    class ServiceUrls
    {
        static ServiceUrls()
        {
            try
            {
                BaseUrl = RoleEnvironment.GetConfigurationSettingValue("AzureServiceManagementUrl");
            }
            catch(Exception) {
                //WriteLog(string.Format("{0} - Error:{1}", DateTime.UtcNow.ToString(), ex));
            }
            BaseUrl = BaseUrl ?? "https://management.core.windows.net";
            BaseUrl = BaseUrl.TrimEnd('/'); //Quality Issue
            GetHostedServicePropertyOperationUrlTemplate = BaseUrl + "/{0}/services/hostedservices/{1}?embed-detail=true";
            GetHostedServicesOperationUrlTemplate = BaseUrl + "/{0}/services/hostedservices";
            DeleteRoleInstancesUrlTemplate = BaseUrl + "/{0}/services/hostedservices/{1}/deploymentslots/production/roleinstances/?comp=delete";
        }
        //http://azurespeed.com/Information/AzureEnvironments
        public static string BaseUrl;
        public static string GetHostedServicePropertyOperationUrlTemplate;
        public static string GetHostedServicesOperationUrlTemplate;
        public static string DeleteRoleInstancesUrlTemplate;

    }
}
