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
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace AvePoint.GCommon
{
    public class CertificateManagementUtil
    {
        public static string GetCertificateChainStatus(string certThumbprint)
        {
            StringBuilder chainStatus = new StringBuilder();
            try
            {
                X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                try
                {
                    store.Open(OpenFlags.OpenExistingOnly);
                    X509Certificate2Collection col = store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, false);
                    if (col.Count > 0)
                    {
                        X509Certificate2 cert = col[0];
                        X509Chain certChain = new X509Chain();
                        certChain.ChainPolicy.RevocationMode = X509RevocationMode.Online;
                        bool buildResult = certChain.Build(cert);
                        chainStatus.AppendLine("Build result:" + buildResult.ToString());
                        foreach (var status in certChain.ChainStatus)
                        {
                            chainStatus.AppendLine(string.Format("Status: {0} Description: {1}", status.Status.ToString(), status.StatusInformation));
                        }
                    }
                    else
                    {
                        chainStatus.AppendLine("Cannot find certificate by thumbprint: " + certThumbprint);
                    }
                }
                finally
                {
                    store.Close();
                }
            }
            catch (Exception ex)
            {
                chainStatus.AppendLine("An error occurred while checking certificate chain status: " + ex.ToString());
            }
            return chainStatus.ToString();
        }

    }
}
