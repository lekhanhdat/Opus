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
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Text;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;
using AvePoint.ObjectModel.WebService;
//using AvePoint.ObjectModel.ClientExtension;
using System.Net;
using System.ServiceModel;
//using AvePoint.GCommon.Utility.Cryptography;
//using AvePoint.GCommon.Utility.I18N;
//using AvePoint.GCommon.Utility.Exceptions.Authentication;
using System.Diagnostics.CodeAnalysis;
using AvePoint.Common;
using AvePoint.ObjectModel.ClientOM;
using System.IO;
using AvePoint.Wrapper.Resource;
using Microsoft365.Authentication;

namespace AvePoint.ObjectModel.Common
{
    class AveClientRequest : IDisposable
    {
        private const int DefaultHttpConnectionLimit = 80;
        private static AveLogger Logger = AveLogger.GetInstance(typeof(AveClientRequest));
        private IAveRequest mRequest;
        private string mSiteUrl;
        private string mOriginalUrl;
        private AveBPOSAccountInfo mUserAccountInfo = new AveBPOSAccountInfo();
        //private HttpClientCredentialType mCredentialType;
        private object mObj;
        private ITokenProvider tokenProvider;

        public AveClientRequest(string url, AveBPOSAccountInfo userAccountInfo)
        {
            mSiteUrl = url;
            //mUserAccountInfo = userAccountInfo;
            InitAccountUser(userAccountInfo);
            InitHttpSettings();
            //InitHttpsSettings();
            InitClientMode();
        }

        private void InitAccountUser(AveBPOSAccountInfo accountInfo)
        {
            //Logger.Info("Account:{0}", accountInfo);
            accountInfo.CopyTo(mUserAccountInfo);
            if (!string.IsNullOrEmpty(mUserAccountInfo.UserName))
            {
                string[] account = mUserAccountInfo.UserName.Split(new char[] { '\\' });
                if (account.Length > 1)
                {
                    mUserAccountInfo.Domain = account[0];
                    mUserAccountInfo.UserName = account[1];
                }
            }
            //else if (string.IsNullOrEmpty(mUserAccountInfo.Domain))
            //{
            //    mUserAccountInfo.Domain = ".";
            //}
        }



        private void InitHttpSettings()
        {
            System.Net.ServicePointManager.DefaultConnectionLimit = DefaultHttpConnectionLimit;
        }

        private void InitClientMode()
        {
            tokenProvider = mUserAccountInfo.Convert2TokenProvider();

            try
            {
                OnlineAuthentication();
            }
            catch (IncorrectUserNameOrPasswordException)
            {
                throw;
            }
            catch (PasswordExpiredException)
            {
                throw;
            }
            catch (Office365SiteExpiredException)
            {
                throw;
            }
            catch (AccountDisableException)
            {
                throw;
            }

        }


        private void OnlineAuthentication()
        {
            //add retry for login. sometimes login fails.
            for (int i = 0; i < WrapperConfiguration.WrapperConfigurationForBPOS.LoginRetryCount;)
            {
                try
                {
                    i++;
                    //EnsureCurrentUserIsSiteAdmin();
                    SPOnlineAuthentication onlineAuth = new SPOnlineAuthentication(mSiteUrl);
                    onlineAuth.Login(tokenProvider);
                    mSiteUrl = onlineAuth.SiteUrl;
                    if (mObj != null)
                    {
                        Logger.Info("login successfully using office365 authentication");
                    }

                    break;
                }
                catch (IncorrectUserNameOrPasswordException e1)
                {
                    Logger.Warn("failed to login site: {0} due to: {1}, retry {2}", mSiteUrl, e1.ToString(), i);
                    if (i < WrapperConfiguration.WrapperConfigurationForBPOS.LoginRetryCount)
                    {
                        System.Threading.Thread.Sleep(WrapperConfiguration.WrapperConfigurationForBPOS.LoginRetryInterval);
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (PasswordExpiredException)
                {
                    throw;
                }
                catch (NonOffice365AccountException)
                {
                    throw;
                }
                catch (Office365SiteExpiredException)
                {
                    throw;
                }
                catch (AccountDisableException)
                {
                    throw;
                }
                catch (Exception e2)
                {
                    Logger.Warn("failed to login site: {0} due to: {1}, retry {2}", mSiteUrl, e2.ToString(), i);
                    if (i < WrapperConfiguration.WrapperConfigurationForBPOS.LoginRetryCount)
                    {
                        System.Threading.Thread.Sleep(WrapperConfiguration.WrapperConfigurationForBPOS.LoginRetryInterval);
                    }
                    else
                    {
                        throw;
                    }
                }
            }
        }


        internal IAveRequest InitRequest()
        {
            var request = CreateRequest(mSiteUrl, mUserAccountInfo);
            mRequest = request;
            return request;
        }

        private IAveRequest CreateRequest(string siteUrl, AveBPOSAccountInfo accountInfo)
        {

            var request = new AveClientOM2013Request(siteUrl, accountInfo)
            {
                OriginalUrl = mOriginalUrl
            };
            return request;
        }


        #region IDisposable Members

        public void Dispose()
        {
        }

        #endregion
    }
}

