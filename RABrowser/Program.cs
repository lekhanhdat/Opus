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
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Browser.Browser.EndUser;
using AvePoint.RA.Browser.Browser.SPO;
using AvePoint.RA.Browser.Handler;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Configurations;
using AvePoint.RA.Common.Pipe;
using AvePoint.RA.Common.Security;
using AvePoint.RA.Common.SharePointBrowser;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.SharePoint.Common;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Newtonsoft.Json;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading;

namespace AvePoint.RA.Browser
{
    public class Program
    {

        private static DateTime LastRunTime = DateTime.UtcNow;

        private static readonly TimeSpan ExpireTime = TimeSpan.FromMinutes(30);

        private static readonly TimeSpan SleepTime = TimeSpan.FromMinutes(1);

        private static readonly string DefaultPipeName = "ranpcs";

        private static RANamedPipeServerStream server;

        private static RALogger Logger = null;

        public static void Main(string[] args)
        {
            try
            {

#if DEBUG
                RALogger.ConfigFile = "Config/BrowserLog4net.dev.config";
#else
                RALogger.ConfigFile = "Config/BrowserLog4net.config";
#endif

                Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
                Logger.Info("Starting browser process");

#if DEBUG
                while (File.Exists("c:/RABrowser.sleep"))
                {
                    Thread.Sleep(1000);
                }
#endif
                InitEnv();
                KeepRun();
            }
            catch (Exception ex)
            {
                if(Logger !=null)
                {
                    Logger?.Error($"Start browser process failed. {ex}");
                }
                else
                {
                    throw;
                }
            }
        }

        private static void InitEnv()
        {
            try
            {
                Logger.Info("Begin initial env.");
                InitCastle();
                RMGlobalConfiguration.Init();
                RMServiceManagerUtil.Init();
                AosApiUtility.Init(RMCertificateHelper.GetCertificate(RMCertNames.AvePointRecords));
                PoolUserUtil.Init(true);
                InitPipeServer();
                Logger.Info("Initial env successful.");
            }
            catch (Exception e)
            {
                Logger.Error("An error occur while Initial env. error: {0}", e);
                throw;
            }
        }

        private static void InitPipeServer()
        {
            try
            {
                Logger.Info("Begin initial pipe server.");
                server = new RANamedPipeServerStream(DefaultPipeName);
                server.RegisterClientOnConnectCallBack(ClientOnConnect);
                server.Connect();
                Logger.Info("Initial pipe server successful.");
            }
            catch (Exception e)
            {
                Logger.Error("An error occur while initial pipe server. error: {0}", e);
                throw;
            }
        }

        private static void InitCastle()
        {
            try
            {
                Logger.Info("Begin initial castle.");
                string installPath = WebUtil.GetInstallPath();
                WindsorContainer windsorContainer = new WindsorContainer();
                windsorContainer.Register(
                    Component.For<IWindsorContainer>().Instance(windsorContainer)
                );
                windsorContainer.Install(Castle.Windsor.Installer.Configuration.FromXmlFile(
                    Path.Combine(installPath, "Config/Castle/BrowserServiceCastle.config")));
                AppDomain.CurrentDomain.SetData("CoreIOCContainerIdentifier", windsorContainer);
                PlatformWindsorManager.SetUp(windsorContainer);
                Logger.Info("Initial castle successful.");
            }
            catch (Exception e)
            {
                Logger.Error("An error occur while initial castle. error: {0}", e);
                throw;
            }
        }

        private static void KeepRun()
        {
            while (LastRunTime + ExpireTime > DateTime.UtcNow)
            {
                Logger.Info("Keep running....");
                Thread.Sleep(SleepTime);
            }
            server?.Dispose();
            Logger.Info($"Exit because no request was made within {ExpireTime} minutes.");
        }

        private static void ClientOnConnect(RANamedPipeServerStreamInner serverStream)
        {
            try
            {
                Logger.Info("Client on connect.");
                LastRunTime = DateTime.UtcNow;
                var contract = serverStream.ReadMessage<RABrowserContract>();
                TenantLocalValue.LogonUserEmail = contract.LogonUserEmail;
                TenantLocalValue.LogonGroupId = contract.LogonGroupId;
                TenantLocalValue.LogonUserId = contract.LogonUserId;
                Logger.Info($"Request browser type: {contract.Type}, user email: {StrConvertBase64(contract.LogonUserEmail)}, tenant id: {StrConvertBase64(contract.LogonGroupId)}, user id: {StrConvertBase64(contract.LogonUserId)}.");
                AveTreeMessage result = null;
                if (contract.Type == BrowserType.SharePointOnline || contract.Type == BrowserType.OneDrive)
                {
                    var message = JsonConvert.DeserializeObject<SPTreeMessage>(contract.Message);
                    result =  SPOBaseBrowser.BrowseAsync(message, contract.Type).Result;
                    serverStream.SendMessage(result);
                }
                else if (contract.Type == BrowserType.CheckEndUserPermission)
                {
                    var message = JsonConvert.DeserializeObject<BrowserMessage>(contract.Message);
                    var checkResult = EndUserCheckPermission.HandlMessageAsync(message, contract.Type).Result;
                    serverStream.SendMessage(checkResult);
                }
                #region 此段代码不会被调用到，保留是为了以后扩展用
                else if (contract.Type == BrowserType.ExchangeOnline)
                {
                    var message = JsonConvert.DeserializeObject<ExchangeOnlineTreeMessage>(contract.Message);
                }
                #endregion
                
            }
            catch (Exception e)
            {
                Logger.Error("An error occur while processing the request. error: {0}", e);
            }
        }

        private static string StrConvertBase64(string str)
        {
            if (string.IsNullOrEmpty(str)) return "";
            var byteArr = Encoding.UTF8.GetBytes(str);
            return Convert.ToBase64String(byteArr);
        }

        //private void DummyFunctionToMakeSureReferencesGetCopy_DO_NOT_DELETE_THIS_CODE()
        //{
        //    var dummyType = typeof(AvePoint.GCommon.MicroKernel.CoreBindingBuilder);
        //    Console.WriteLine(dummyType.FullName);
        //}
    }
}
