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
namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Globalization;
    using AvePoint.GCommon;
    using Microsoft.Azure.ActiveDirectory.Client.Framework;

    class BecWebServiceInstance
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(BecWebServiceInstance), false);
        private static readonly Uri BecWebService = new Uri("https://provisioningapi.microsoftonline.com/provisioningwebservice.svc");
        private MicrosoftOnlineInstanceDetail onlineInstanceDetail;

        public BecWebServiceInstance(AveBPOSAccountInfo account)
        {
            onlineInstanceDetail = FromDomainOrPrincipalName(account.UserName);
        }

        public string GetBecWebServiceLogonSiteName()
        {
            string text;
            if (onlineInstanceDetail != null)
            {
                text = onlineInstanceDetail.ProvisioningServiceSiteName;
            }
            else
            {
                text = BecWebService.Host;
            }
            text = text.Replace("provisioningapi.", "ps.");
            return text;
        }

        public string GetBecWebServiceUri()
        {
            if (onlineInstanceDetail != null)
            {
                return onlineInstanceDetail.ProvisioningServiceEndpointUrl;
            }
            else
            {
                return BecWebService.ToString();
            }
        }

        private static MicrosoftOnlineInstanceDetail FromDomainOrPrincipalName(string userName)
        {
            MicrosoftOnlineInstanceDetail detail = null;
            if (!string.IsNullOrEmpty(userName))
            {
                string message = null;
                try
                {
                    try
                    {
                        detail = MicrosoftOnlineInstance.FromDomainOrPrincipalName(userName);
                        message = "Service instance autodiscovery succeeded.";
                    }
                    catch (DnsResolverException ex)
                    {
                        message = string.Format(CultureInfo.InvariantCulture, "Service instance autodiscovery failed with '{0}'.", ex);
                    }
                    catch (NotSupportedException ex2)
                    {
                        message = string.Format(CultureInfo.InvariantCulture, "Service instance autodiscovery failed with '{0}'.", ex2);
                    }
                }
                finally
                {
                    logger.Info(message);
                }
            }

            return detail;
        }
    }
}
