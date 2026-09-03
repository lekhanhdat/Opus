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
using Microsoft.AspNetCore.Hosting;
//using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
//using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HybridServer.Log;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Microsoft.Extensions.Configuration;
using Azure.Extensions.AspNetCore.Configuration.Secrets;
using Azure.Identity;
using HybridServer.Utils;
using HybridServer.Configuration;

namespace HybridServer
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args).
               ConfigureWebHostDefaults(webBuilder =>
              {

                  webBuilder.ConfigureKestrel((env, options) =>
                  {
                      options.AddServerHeader = false;
                      if (!env.HostingEnvironment.IsDevelopment())
                      {
                          var certFile = "tls.crt";
                          var keyFile = "tls.key";
                          options.ConfigureHttpsDefaults(listenOptions =>
                          {
                              using var privateKey = RSA.Create();
                              privateKey.ImportRSAPrivateKey(PemBytes(keyFile), out var bytesRead);
                              X509Certificate2 certificate = new X509Certificate2(PemBytes(certFile));
                              listenOptions.ServerCertificate =
                                  new X509Certificate2(certificate.CopyWithPrivateKey(privateKey).Export(X509ContentType.Pkcs12));
                          });
                      }
                  });
                  webBuilder.ConfigureLog().UseStartup<Startup>();
              })
              .ConfigureAppConfiguration((hostingContext, configBuilder) =>
              {
                  GlobalConfiguration.Init(configBuilder.Build(), hostingContext.HostingEnvironment.IsProduction());
              });
        }

        public static byte[] PemBytes(string fileName) =>
            Convert.FromBase64String(
                File.ReadAllLines(fileName)
                .Where(l => !l.Contains('-'))
                .Where(l => !l.Contains(' '))
                .Aggregate("", (current, next) => current + next)
            );
    }
}
