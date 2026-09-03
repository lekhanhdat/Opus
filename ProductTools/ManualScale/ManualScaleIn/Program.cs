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
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.WindowsAzure.ServiceRuntime;
using System.Security.Cryptography.X509Certificates;
using System.Net;
using System.Collections.Generic;
using System.Xml.Linq;
using ManualScaleIn;
using System.Threading;

namespace AvePoint.AutoScale.ManualScaleIn
{
    class Program
    {
        private static XNamespace mNameSpace = "http://schemas.microsoft.com/windowsazure";
        private const string EMPTY = "null";
        private const string BASEINSTANCENAME = "RAScheduleJobWorkerRole_IN_{0}";
        private static string LogFilePath;
        static int Main(string[] args)
        {
            //try
            //{
            //    var deployedId = RoleEnvironment.DeploymentId;
            //    var CertificateThumbprint = RoleEnvironment.GetConfigurationSettingValue("CertificateThumbprint");
            //    Console.WriteLine($"deployedId:{deployedId}, CertificateThumbprint:{CertificateThumbprint}");
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine(ex.ToString());
            //}
            //Console.ReadKey();
            if (args.Length < 1 || string.IsNullOrEmpty(args[0]))
            {
                return 1;
            }

            try
            {
#if DEBUG
                while (File.Exists("c:\\manualScaleIn.sleep"))
                {
                    Thread.Sleep(1000);
                }
#endif
                LogFilePath = args[0];
                EnsureLogDirectory();
                
                var certThumbprint = RoleEnvironment.GetConfigurationSettingValue("SubscriptionCertThumbprint");
                if (string.IsNullOrEmpty(certThumbprint) || string.Equals(EMPTY, certThumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DisableException("Cannot find the CertificateThumbprint value in configuration file.");
                }

                var subscriptionId = RoleEnvironment.GetConfigurationSettingValue("SubscriptionId");
                if (string.IsNullOrEmpty(subscriptionId) || string.Equals(EMPTY, subscriptionId, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DisableException("Cannot find the SubscriptionId value in configuration file.");
                }

                var cert = GetCertificate(certThumbprint);
                if (cert == null)
                {
                    throw new Exception(string.Format("Cannot find the specified certificate. Thumbprint:{0}", certThumbprint));
                }

                string cloudServiceName = GetCloudServiceName(subscriptionId, cert, RoleEnvironment.DeploymentId);
                //string cloudServiceName = GetCloudServiceName(subscriptionId, cert, "9b793eaac062417cba55029f254c617e");
                if (string.IsNullOrEmpty(cloudServiceName))
                {
                    throw new Exception(string.Format("Cannot find the specified cloud service name. Thumbprint:{0}, SubscriptionId:{1}", certThumbprint, subscriptionId));
                }

                EnsureDeleteInstance(RoleEnvironment.CurrentRoleInstance.Id);

                WriteLog(string.Format("{0} -Start to delete instance. CloudSerivceName:{1}, RoleInstanceId:{2}", DateTime.UtcNow.ToString(), cloudServiceName, RoleEnvironment.CurrentRoleInstance.Id));
                System.Threading.Thread.Sleep(1000);
                if (!DeleteRoleInstances(subscriptionId, cloudServiceName, cert, new string[] { RoleEnvironment.CurrentRoleInstance.Id }))
                {
                    return 2;
                }
            }
            catch (DisableException dex)
            {
                WriteLog(string.Format("{0} - Error:{1}", DateTime.UtcNow.ToString(), dex));
                System.Threading.Thread.Sleep(1000);
                return -1;
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("{0} - Error:{1}", DateTime.UtcNow.ToString(), ex));
                System.Threading.Thread.Sleep(1000);
                return 1;
            }
            WriteLog(string.Format("{0} - Delete Role Instance {1} Successfully", DateTime.UtcNow.ToString(), RoleEnvironment.CurrentRoleInstance.Id));
            System.Threading.Thread.Sleep(1000);
            return 0;
        }

        private static void EnsureLogDirectory()
        {
            var path = Path.GetDirectoryName(LogFilePath);
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
            }
        }

        private static void EnsureDeleteInstance(string instanceName)
        {
            int defaultCount = 0;
            var reservedCount = RoleEnvironment.GetConfigurationSettingValue("ReservedInstanceCount");
            if (string.IsNullOrEmpty(reservedCount) || string.Equals(EMPTY, reservedCount, StringComparison.OrdinalIgnoreCase) || !int.TryParse(reservedCount, out defaultCount) || defaultCount <= 0)
            {
                throw new DisableException(string.Format("reservedCount not validate:{0}.", reservedCount));
            }

            for (int i = 0; i < defaultCount; i++)
            {
                string reservedName = string.Format(BASEINSTANCENAME, i);
                //WriteLog(string.Format("{0}:{1}", reservedName, instanceName));
                if (instanceName.Equals(reservedName, StringComparison.OrdinalIgnoreCase))
                {
                    throw new DisableException(string.Format("current cloud service need reserved:{0}.", instanceName));
                }
            }

        }

        private static X509Certificate2 GetCertificate(string certThumbprint)
        {
            X509Store x509Store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
            x509Store.Open(OpenFlags.OpenExistingOnly);
            X509Certificate2Collection x509Certificate2Collection = x509Store.Certificates.Find(X509FindType.FindByThumbprint, certThumbprint, false);
            if (x509Certificate2Collection.Count == 0)
            {
                throw new Exception(string.Format("Cannot find the specified certificate. Thumbprint:{0}", certThumbprint));
            }
            X509Certificate2 cert = x509Certificate2Collection[0];

            return cert;
        }

        private static string GetCloudServiceName(string subscriptionId, X509Certificate2 cert, string deploymentId)
        {
            var names = GetHostedServices(subscriptionId, cert);
            foreach (var name in names)
            {
                try
                {
                    var properties = GetHostedServiceProperties(subscriptionId, cert, name);
                    if (properties != null)
                    {
                        var deploymentXElements = properties.Elements(XName.Get("Deployments", mNameSpace.ToString())).Elements(XName.Get("Deployment", mNameSpace.ToString())).ToList();
                        if (deploymentXElements != null && deploymentXElements.Count > 0)
                        {
                            foreach (var deployment in deploymentXElements)
                            {
                                string currentDeploymentId = deployment.Element(XName.Get("PrivateID", mNameSpace.ToString())).Value;
                                if (string.Equals(currentDeploymentId, deploymentId, StringComparison.OrdinalIgnoreCase))
                                {
                                    return name;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    WriteLog(string.Format("{0} - Error:{1}", DateTime.UtcNow.ToString(), ex));
                    System.Threading.Thread.Sleep(1000);
                }
            }

            return string.Empty;
        }

        private static bool DeleteRoleInstances(string subscriptionId, string cloudServiceName, X509Certificate2 cert, string[] roleInstanceNames)
        {
            try
            {
                var uri = string.Format(ServiceUrls.DeleteRoleInstancesUrlTemplate, subscriptionId, cloudServiceName);

                var requestBodyFormat = @"<RoleInstances xmlns=""http://schemas.microsoft.com/windowsazure"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"">{0}</RoleInstances>";
                var namesXml = string.Join("", roleInstanceNames.Select(x => string.Format("<Name>{0}</Name>", x)));

                PerformPostOperation(uri, cert, string.Format(requestBodyFormat, namesXml));
            }
            catch (Exception ex)
            {
                WriteLog(string.Format("{0} - Error:{1}", DateTime.UtcNow.ToString(), ex));
                System.Threading.Thread.Sleep(1000);
                return false;
            }
            return true;
        }

        private static string PerformPostOperation(string uri, X509Certificate2 certificate, string body)
        {
            var requestUri = new Uri(uri);
            var httpWebRequest = CreateHttpWebRequest(requestUri, certificate, "POST");

            var requestBody = Encoding.UTF8.GetBytes(body);
            using (var stream = httpWebRequest.GetRequestStream())
            {
                stream.Write(requestBody, 0, requestBody.Length);
            }

            using (var resp = (HttpWebResponse)httpWebRequest.GetResponse())
            using (var sr = new StreamReader(resp.GetResponseStream()))
            {
                return sr.ReadToEnd();
            }
        }

        private static HttpWebRequest CreateHttpWebRequest(Uri uri, X509Certificate2 certificate, string httpWebRequestMethod)
        {
            var httpWebRequest = (HttpWebRequest)HttpWebRequest.Create(uri);
            httpWebRequest.Method = httpWebRequestMethod;
            httpWebRequest.Headers.Add("x-ms-version", "2013-08-01");
            httpWebRequest.ClientCertificates.Add(certificate);
            httpWebRequest.ContentType = "application/xml";
            return httpWebRequest;
        }

        private static IList<string> GetHostedServices(string subscriptionId, X509Certificate2 certificate)
        {
            List<string> hostedServiceNames = new List<string>();
            string uri = string.Format(ServiceUrls.GetHostedServicesOperationUrlTemplate, subscriptionId);
            XElement xe = PerformGetOperation(uri, certificate);
            if (xe != null)
            {
                var serviceNameElements = xe.Elements().Elements(XName.Get("ServiceName", mNameSpace.ToString()));
                foreach (var serviceElement in serviceNameElements)
                {
                    hostedServiceNames.Add(serviceElement.Value);
                }
            }
            return hostedServiceNames;
        }

        private static XElement GetHostedServiceProperties(string subscriptionId, X509Certificate2 certificate, string hostedServiceName)
        {
            string uri = string.Format(ServiceUrls.GetHostedServicePropertyOperationUrlTemplate, subscriptionId, hostedServiceName);
            return PerformGetOperation(uri, certificate);
        }

        private static XElement PerformGetOperation(string uri, X509Certificate2 certificate)
        {
            XElement responseBody = null;
            Uri requestUri = new Uri(uri);
            HttpWebRequest httpWebRequest = CreateHttpWebRequest(requestUri, certificate, "GET");
            using (HttpWebResponse response = (HttpWebResponse)httpWebRequest.GetResponse())
            {
                Stream responseStream = response.GetResponseStream();
                responseBody = XElement.Load(responseStream);
            }
            return responseBody;
        }

        private static void WriteLog(string message)
        {
            using (StreamWriter sw = new StreamWriter(LogFilePath, true))
            {
                sw.WriteLine(message);
            }
        }
    }
}
