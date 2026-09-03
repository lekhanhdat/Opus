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
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cloud;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Contract.AAD;
using AvePoint.RA.Contract.Aos;
using Cloud.Sdk.Data.Amls.Ics.Contracts;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using Cloud.Sdk.Amls.Ics;

namespace AosApiUnitTest
{
    [TestClass]
    public class UnitTest1
    {
        [TestMethod]
        public  void TestMethod1()
        {          
            var identityServiceUrl = "https://identity.sharepointguild.com";
            var clientId = "3354bc67-9d2b-deeb-de3b-66f8552937da";
            var portalApiUrl = "https://graph.sharepointguild.com/aos";
            var modernPortalApiUrl = "https://graph.sharepointguild.com/aos-modern-internal";
            var certificate = GetCertFromLocalByThumbprint("958B6B5093959E9B3886BC19D5B10A19C417973D"); 
            string customerId = "6baa622f-215f-43e8-8ca4-26b786865b66";
            RMCloudSdk.InitForUnitTest(certificate, identityServiceUrl, clientId, portalApiUrl, modernPortalApiUrl);
            RMAosApiClient.GetLicenseInfo(customerId);
            var test = RMAosApiClient.GetTenantRemoteNodes(customerId, "c767fca9-e8a7-406c-9689-a6ecf5d389e6");
            var pools = RMAosApiClient.GetAccountPoolUsers(customerId);
            RMAosApiClient.GetLicenseInfo(customerId);
            var accounts = RMAosApiClient.GetServiceAccountsWithPassword(customerId);
            var users = RMAosApiClient.GetGroupAndUsers(customerId);
            var u1 = RMAosApiClient.GetUserByUserId(customerId, users.FirstOrDefault().UserId);
            var u = users.Where(m => m.UserId == "2ebe78ee-337b-439e-9790-b60a5998b71a").FirstOrDefault();
           
            var all = RMAosApiClient.GetAllProfile(customerId);
            //var app = RMAosApiClient.GetO365AccessTokenFromAOS("53ad416f-d679-4b03-814f-30b874189b31", "814c0b46-87dc-4b23-8714-94b8f33c2a96");
            RMAosApiClient.VerifySignature(RMAosProductType.COP.Name, "", "");
            var url = RMAosApiClient.GetRecordsServiceUrl(customerId);            
            var ids = RMAosApiClient.GetO365TenantIds(customerId);

            var tenant = RMAosApiClient.GetTenantInfo(customerId);

            //admin@M365x73442106.onmicrosoft.com
            var ss = RMAosApiClient.SearchUser(customerId, "jychu@163.com");


            #region AI Predict Term
            var trainingModelId = new Guid("E76B9686-009B-4960-94BE-3A014123D097");
            var response = RMAosApiClient.GetPredictResult("3a1b3d1d-10a7-4a59-b507-fa64534f7f76", trainingModelId);

            foreach (var item in response)
            {
                Console.WriteLine($"{item.Name}");
                item.Results.ForEach(o =>
                {
                    Console.WriteLine($"label: {o.Label}, score: {o.Score}");
                });
            }
            #endregion

        }

        private static X509Certificate2 GetCertFromLocalByThumbprint(string thumbprint)
        {
            if (string.IsNullOrEmpty(thumbprint)) return null;

            var certificate = Get509Cert(StoreLocation.LocalMachine, thumbprint);
            if (certificate == null)
            {
                certificate = Get509Cert(StoreLocation.CurrentUser, thumbprint);
            }
            if (certificate == null)
            {
                throw new Exception(string.Format("Can't find certificate by thumbprint {0}.", thumbprint));
            }
            else
            {
                return certificate;
            }
        }
        private static X509Certificate2 Get509Cert(StoreLocation location, string thumbprint)
        {
            var store = new X509Store(StoreName.My, location);
            store.Open(OpenFlags.ReadOnly);
            var x509cerCollection = store.Certificates.Find(X509FindType.FindByThumbprint, thumbprint, false);
            if (x509cerCollection.Count == 0)
            {
                return null;
            }
            X509Certificate2 cer = x509cerCollection[0];
            store.Close();
            return cer;
        }
    }
}
