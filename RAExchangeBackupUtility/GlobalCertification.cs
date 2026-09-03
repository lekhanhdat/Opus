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



namespace ExchangeUtility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Net;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using ExchangeUtility;
    using System.Collections.Concurrent;
    using AvePoint.GCommon;
    using System.Reflection;
    using AvePoint.RA.CommonUtil;

    public class GlobalExchangeSetting
    {
        private static readonly RALogger logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        #region Certification
       /* private static bool CertificateValidationCallBack(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors sslPolicyErrors)
        {
            // If the certificate is a valid, signed certificate, return true.
            if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
            {
                return true;
            }

            // If there are errors in the certificate chain, look at each error to determine the cause.
            if ((sslPolicyErrors & System.Net.Security.SslPolicyErrors.RemoteCertificateChainErrors) != 0)
            {
                if (chain != null && chain.ChainStatus != null)
                {
                    foreach (System.Security.Cryptography.X509Certificates.X509ChainStatus status in chain.ChainStatus)
                    {
                        if ((certificate.Subject == certificate.Issuer) &&
                           (status.Status == System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.UntrustedRoot))
                        {
                            // Self-signed certificates with an untrusted root are valid. 
                            continue;
                        }
                        else
                        {
                            if (status.Status != System.Security.Cryptography.X509Certificates.X509ChainStatusFlags.NoError)
                            {
                                // If there are any other errors in the certificate chain, the certificate is invalid,
                                // so the method returns false.
                                return false;
                            }
                        }
                    }
                }

                // When processing reaches this line, the only errors in the certificate chain are 
                // untrusted root errors for self-signed certificates. These certificates are valid
                // for default Exchange server installations, so return true.
                return true;
            }
            else
            {
                // In all other cases, return false.
                return false;
            }
        }*/

        public static void SetServicePointManager()
        {
            //ServicePointManager.ServerCertificateValidationCallback = CertificateValidationCallBack;
        }
        #endregion

        #region Permission
        //public static void CheckAndSetPermission(AuthObject authObj, string targetMailbox, string url)
        //{
        //    using (ExchangeUser exchangeUser = ExchangeUserFacotry.CreateExchangeUser(authObj))
        //    {
        //        if (!exchangeUser.CheckPermission(targetMailbox, ref url))
        //        {
        //            exchangeUser.AddPermission(targetMailbox);
        //        }
        //    }
        //}
        #endregion

        #region O365 Group Mailbox
        public static bool IsO365GroupMailBox = false;
        #endregion

        #region Get Impersonate Id
        /// <summary>
        /// 此Dictionary保存本次Job中MailBox和ImpersonateId对应关系
        /// </summary>
        private static ConcurrentDictionary<string, string> impersinateIdDictionary = new ConcurrentDictionary<string, string>();
        private static DelayMode dalayMode = DelayMode.Random;

        /// <summary>
        /// 此方法依赖IsO365GroupMailBox属性，请使用前给该属性赋值，有待改进
        /// keep original retry times and wait time.
        /// </summary>
        /// <param name="mailbox"></param>
        /// <param name="authObj"></param>
        /// <returns></returns>
        public static void SetImpersonateIdToDictionary(string mailbox, AuthObject authObj)
        {
            var retryPolicy = new RetryCommon(5, 2, WaitMode.Random, DelayMode.None);
            SetImpersonateIdToDictionary(mailbox, authObj, retryPolicy);
        }

        /// <summary>
        /// new method, construst retry policy outside
        /// </summary>
        /// <param name="mailbox"></param>
        /// <param name="authObj"></param>
        /// <param name="retryPolicy"></param>
        public static void SetImpersonateIdToDictionary(string mailbox, AuthObject authObj, RetryCommon retryPolicy)
        {
            var impersinateId = string.Empty;
            var haveImpersinateId = impersinateIdDictionary.TryGetValue(mailbox, out impersinateId);
            if (!haveImpersinateId)
            {
                if (IsO365GroupMailBox)
                {
                    using (var Office365GroupService = Office365GroupFacotry.CreateOffice365GroupService(authObj))
                    {
                        // NET6Upgrade 需要检查是否可以改为Graph API获取
                        throw new Exception("GetO365GroupOwnerOrMember use graph api?");
                        try
                        {
                            //impersinateId = Office365GroupService.GetO365GroupOwnerOrMember(mailbox);
                        }
                        catch (Exception ex)
                        {
                            logger.Error("An error occurred when get office 365 group owners,we will use classic method.Exception:{0}.", ex.ToString());
                            using (ExchangeUser exchangeUser = ExchangeUserFacotry.CreateExchangeUser(authObj))
                            {
                                var retry = new RetryCommon(5, 2, WaitMode.Random, DelayMode.None);
                                impersinateId = retry.Retry<string, string>(mailbox, exchangeUser.GetO365GroupOwnerOrMember);
                            }

                        }
                        finally
                        {
                            if (string.IsNullOrEmpty(impersinateId))
                            {
                                impersinateId = authObj.UserName;
                                impersinateIdDictionary.TryAdd(mailbox, impersinateId);
                                logger.Warn($"Get O365 group owner or member with Exception and set service account: {impersinateId} as default");
                            }
                        }
                    }
                }
                else
                {
                    impersinateId = mailbox;
                }
                impersinateIdDictionary.TryAdd(mailbox, impersinateId);
            }
        }

        /// <summary>
        /// use for brower tree
        /// </summary>
        public static void ClearImpersonateIdDictionary()
        {
            impersinateIdDictionary.Clear();
        }

        public static string GetImpersonateIdByMailbox(string mailbox)
        {
            var impersinateId = string.Empty;
            impersinateIdDictionary.TryGetValue(mailbox, out impersinateId);
            return string.IsNullOrEmpty(impersinateId) ? mailbox : impersinateId;
        }

        /// <summary>
        /// use for brower tree
        /// </summary>
        public static void SetNoneDalayMode()
        {
            dalayMode = DelayMode.None;
        }
        #endregion

        public static V ConvertClassBySameClassStructure<T, V>(T a) where T : class, new() where V : class, new()
        {
            Newtonsoft.Json.JsonSerializerSettings jSetting = new Newtonsoft.Json.JsonSerializerSettings();
            jSetting.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(a, jSetting);
            logger.Info("[{0}]--->[{1}], Details:[{2}]", typeof(T).Name, typeof(V).Name, json);
            return Newtonsoft.Json.JsonConvert.DeserializeObject<V>(json);
        }
    }
}
