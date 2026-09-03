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
using Azure.Core;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using HybridServer.Log;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace HybridServer.Utils
{
    
    class KeyVaultUtil
    {
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(KeyVaultUtil));
        public static SecretClient GetClient(string keyVaultUrl, string tenantId, string clientId, string certThumb)
        {
            SecretClient client = null;
            try
            {
                TokenCredential cred;
                if (!string.IsNullOrEmpty(clientId))
                {
                    using (var store = new X509Store(StoreLocation.LocalMachine))
                    {
                        store.Open(OpenFlags.ReadOnly);
                        var certs = store.Certificates
                            .Find(X509FindType.FindByThumbprint,
                                certThumb, false);
                        cred = new ClientCertificateCredential(tenantId, clientId, certs.OfType<X509Certificate2>().Single());
                    }
                    logger.Info($"use clientcertficate:{keyVaultUrl}.");
                }
                else
                {
                    cred = new ManagedIdentityCredential();
                    logger.Info($"use managedIdentity:{keyVaultUrl}.");
                }
                client = new SecretClient(new Uri(keyVaultUrl), cred);
                
            }
            catch (Exception ex)
            {
                logger.Error($"Get client error:{ex.ToString()}");
            }
            return client;
        }

        public static void TestConnection(string keyVaultUrl, string tenantId, string clientId, string certThumb) 
        {
            try
            {
                logger.Info("begin to test connect.");
                var client = GetClient(keyVaultUrl, tenantId, clientId, certThumb);
                RSAHelper rasHelper = null;
                try
                {
                    var certContent = client.GetSecret("CertificateName--Records")?.Value?.Value;
                    logger.Info($"test get cert key:{certContent}.");
                    var certByte = System.Convert.FromBase64String(certContent);
                    var cert = new X509Certificate2(certByte, string.Empty, X509KeyStorageFlags.MachineKeySet | X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
                    rasHelper = new RSAHelper(cert);
                }
                catch (Exception ex)
                {
                    logger.Error($"Get cert error:{ex.ToString()}");
                    return;
                }
               
                var db = client.GetSecret("Hybrid--Database")?.Value?.Value;
                logger.Info($"test get db key:{db}.");

                var redis = client.GetSecret("Redis--connection")?.Value?.Value;
                logger.Info($"test get redis key:{redis}.");

                var redisConn = rasHelper.Decrypt(redis);
                logger.Info($"test get redis decrypt key:{redisConn}.");
            }
            catch (Exception ex)
            {
                logger.Error($"error test connect:{ex.ToString()}.");
            }
        }
    }

}

