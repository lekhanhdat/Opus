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
using AvePoint.Hybrid.AgentService.Utils;
using AvePoint.Hybrid.Contract.Object;
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.Hybrid.Utility;
using AvePoint.Hybrid.Utility.ConfigurationFile;
using System;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;

namespace AvePoint.Hybrid.AgentService.ServiceEndpoint
{
    public interface IEPCertificateUpdateService
    {
        void Update(AgentCertificateUpdateArgs args);
    }
    public class EPCertificateUpdateService : IEPCertificateUpdateService
    {
        /// <summary>
        /// will update the certificate in Registry and memory
        /// </summary>
        /// <param name="args"></param>
        public void Update(AgentCertificateUpdateArgs args)
        {
            try
            {
                if (string.IsNullOrEmpty(args.AgentConfigurtionContent)) throw new ArgumentException($"AgentConfigurtionContent is null, agent id : {args.AgentId}");

                var bytes = Convert.FromBase64String(args.AgentConfigurtionContent);
                var installationCode = AgentConfigurationFileHelper.ReadInstallationCode();
                if (string.IsNullOrEmpty(installationCode)) throw new Exception($"No installation code found in registry, agent id : {args.AgentId}");
                var config = AgentConfigurationFileHelper.ReadFromEncryptBytes(bytes, installationCode);

                if (null == config) throw new Exception($"new configuration content can't be read, agent id : {args.AgentId}");

                var appCert = new X509Certificate2(Convert.FromBase64String(config.CertificateContent), config.CertificatePWD);
                if (appCert.NotAfter < DateTime.Now) throw new Exception($"certificate is expired, agent id : {args.AgentId}");

                UpdateRegistry(config, installationCode);
                UpdateMemory(appCert);
            }
            catch (Exception)
            {
                throw;
            }
            StopAgentBrowserAsync(); //no need to wait.
        }

        private void UpdateRegistry(AgentConfigurtion config, string installationCode)
        {
            var existingConfig = AgentConfigurationFileHelper.ReadFromRegistry();
            existingConfig.CertificateContent = config.CertificateContent;
            existingConfig.CertificatePWD = config.CertificatePWD;
            AgentConfigurationFileHelper.WriteConfig2Registry(existingConfig, installationCode);
        }

        private void UpdateMemory(X509Certificate2 appCert)
        {
            CommonConfiguration.SetAppCert(appCert);
        }

        /// <summary>
        /// stop the agent browser process async
        /// </summary>
        /// <returns></returns>
        private Task StopAgentBrowserAsync()
        {
            return Task.Run(() => ProcessHelper.StopProcess(Constants.RecordsBrowserExe));
        }
    }
}
