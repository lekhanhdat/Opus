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

using AvePoint.Wrapper.Core.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AvePoint.Wrapper.Core.SPRestore
{
    class BuiltInUserMapping : IUserMapping
    {
        private string placeHolderAccount;
        private string defaultUserAccount;
        private Dictionary<string, string> userMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private Dictionary<string, string> domainMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// 先通过User Mapping，如果找不到，再看看是不是FBA，需要进行转换
        /// </summary>
        /// <param name="userName"></param>
        /// <returns></returns>
        public string GetMappingLoginName(string userName)
        {
            string loginName = null;
            SPUserLoginNameDecoder decoder = null;

            if (userMappings.Count > 0)
            {
                lock (userMappings)
                {
                    if (!userMappings.TryGetValue(userName, out loginName))
                    {
                        decoder = new SPUserLoginNameDecoder(userName);

                        if (decoder.IsClaims && (decoder.OriginalIssueType == SPUserOriginalIssuerType.Forms ||
                            decoder.OriginalIssueType == SPUserOriginalIssuerType.ASPNETMemberShip ||
                            decoder.OriginalIssueType == SPUserOriginalIssuerType.ASPNETRole))
                        {
                            var formName = decoder.GetFormattedClaimValue();

                            if (userMappings.TryGetValue(formName, out loginName))
                            {
                                loginName = SPUserLoginNameDecoder.Encode(decoder.Header, loginName, true);
                            }
                        }
                    }
                }
            }

            if(loginName == null)
            {
                if(domainMappings.Count > 0)
                {
                    if (decoder == null) { decoder = new SPUserLoginNameDecoder(userName); }

                    decoder.ReplaceDomain(TryGetMappingDomainName, out loginName);
                }
            }

            if(loginName == null)
            {
                loginName = userName;
            }

            return loginName;
        }

        public bool IsPlaceHolderEnabled
        {
            get { return !string.IsNullOrEmpty(placeHolderAccount); }
        }

        public string PlaceHolderAccount
        {
            get { return placeHolderAccount; }
            internal set { placeHolderAccount = value; }
        }

        public bool IsDefaultUserEnabled
        {
            get { return !string.IsNullOrEmpty(defaultUserAccount); }
        }

        internal void AddUserMapping(string sourceUserName, string destUserName)
        {
            lock (userMappings)
            {
                userMappings[sourceUserName] = destUserName;
            }
        }

        public string DefaultUserAccount
        {
            get { return defaultUserAccount; }
            internal set { defaultUserAccount = value; }
        }

        internal void AddDomainMapping(string sourceDomainName, string destDomainName)
        {
            lock(domainMappings)
            {
                domainMappings[sourceDomainName] = destDomainName;
            }
        }

        private bool TryGetMappingDomainName(string sourceDomainName, out string destDomainName)
        {
            destDomainName = null;

            lock(domainMappings)
            {
                return domainMappings.TryGetValue(sourceDomainName, out destDomainName);
            }
        }


        public Dictionary<string, string> ExportUserMapping()
        {
            lock(userMappings)
            {
                return new Dictionary<string, string>(userMappings, StringComparer.OrdinalIgnoreCase);
            }
        }

        public Dictionary<string, string> ExportDomainMapping()
        {
            lock(domainMappings)
            {
                return new Dictionary<string, string>(domainMappings, StringComparer.OrdinalIgnoreCase);
            }
        }


        public Common.SPUserLoginNameDecoder GetMappingLoginNameDecoder(string userName)
        {
            return new SPUserLoginNameDecoder(GetMappingLoginName(userName));
        }
    }
}
