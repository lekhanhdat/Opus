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
using System.IO;

namespace k8s
{
    public partial class KubernetesClientConfiguration
    {
        private const string ServiceAccountPath = "/var/run/secrets/kubernetes.io/serviceaccount/";
        private const string ServiceAccountTokenKeyFileName = "token";
        private const string ServiceAccountRootCAKeyFileName = "ca.crt";

        public static Boolean IsInCluster()
        {
            var host = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_HOST");
            var port = Environment.GetEnvironmentVariable("KUBERNETES_SERVICE_PORT");
            if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(port))
            {
                return false;
            }
            var tokenPath = Path.Combine(ServiceAccountPath, ServiceAccountTokenKeyFileName);
            if (!File.Exists(tokenPath))
            {
                return false;
            }
            var certPath = Path.Combine(ServiceAccountPath, ServiceAccountRootCAKeyFileName);
            return File.Exists(certPath);
        }
    }
}