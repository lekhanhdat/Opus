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
using System.Net;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using AutoInstallation.Contract.WebClientRest;
using GUIRESX = AutoInstallation.Records.App.Resources.Resource;
using LOGRESX = AutoInstallation.Records.App.Resources.LogResource;

namespace AutoInstallationCommon.Utility.Handler
{
    public class TrustAllCertificatePolicy : ICertificatePolicy
    {
        public bool CheckValidationResult(ServicePoint sp,
            X509Certificate cert,
            WebRequest req, int problem)
        {
            return true;
        }
    }

    public class WebClientHandler
    {
        public delegate void CheckFailed(string msg);

        private static readonly AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        public event CheckFailed OnCheckFailed;

        public bool UploadString(Uri address, string type)
        {
            var ret = false;
            try
            {
                using (var _client = new ClientRest(30))
                {
                    _client.Headers[HttpRequestHeader.ContentType] = "application/json";
                    if (!_client.IsBusy)
                    {
                        ServicePointManager.CertificatePolicy = new TrustAllCertificatePolicy();
                        var returnValue = _client.UploadString(address, "POST");
                        ret = true;
                    }
                    else
                    {
                        //logger.Error(GUIRESX.COMMONUTILITY_WEBCLIENTBUSY);
                        //if (OnCheckFailed != null) OnCheckFailed(GUIRESX.COMMONUTILITY_WEBCLIENTBUSY);
                    }
                }
            }
            catch (Exception ex)
            {
                logger.Error(LOGRESX.COMMONUTILITYLOG_CONNECTPLATFORMERROR, ex.ToString());
                //if (OnCheckFailed != null) OnCheckFailed(GUIRESX.COMMONUTILITY_CONNECTPLATFORMERROR);
            }

            return ret;
        }
    }
}