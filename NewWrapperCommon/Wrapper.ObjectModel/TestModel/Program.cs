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
using AvePoint.Wrapper.Common;

namespace TestModeWebService
{

    class Program
    {
        private static string SiteUrl = "https://M365x126499.sharepoint.com/sites/site1";
        private static AveBPOSAccountInfo user = new AveBPOSAccountInfo()
        {
            //Domain = "M365x126499.onmicrosoft.com",
            UserName = "admin@M365x126499.onmicrosoft.com",
            Password = "0WAUUgoghV",
            //AdminUrl = siteAdminUrl,
            ConnectionType = BposConnectionType.ServiceAccount,
            //TenantId = "6622a2b6-e5b5-45f1-a74a-e688919cd837"
        };

        //[DllImport("kernel32.dll")]
        static void Main(string[] args)
        {
            SiteTest();
        }

        public static void SiteTest()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12 | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls;
            AveObjectModelFactory factory = AveObjectModelFactory.CreateObjectModelFactory(SiteUrl, user, AveContextKind.ClientObjectModel);
            var site = factory.CreateSite(SiteUrl);
            site.OpenWeb();
            Console.WriteLine("Over");
            Console.ReadKey();
        }
    }
}
