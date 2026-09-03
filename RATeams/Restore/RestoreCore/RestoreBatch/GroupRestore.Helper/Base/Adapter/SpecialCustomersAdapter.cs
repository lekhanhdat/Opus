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


namespace Office365GroupRestore
{
    using AvePoint.GCommon.Contract.CentralAdmin.Object;
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineRestore.Object;
    using ExchangeBackupUtility;
    using ExchangeBackupUtility.Graph;
    using ExchangeCommonWrapper;
    using System;
    using System.Collections.Generic;

    public class SpecialCustomersAdapter
    {
        private RestoreConfig config;
        private bool IsSpecialCustomer;
        private Dictionary<string, string> domainMap;
        public string SourceDomain { get; private set; }
        public string SourceSmtpAddress { get; private set; }
        private string targetDomain;
        private string targetSmtpAddress; 
        public bool IsSpecialTeam { get; private set; }
        public SpecialCustomersAdapter(RestoreConfig config, string smtpAddress)
        {
            if (config.RestoreType != EORestoreType.InPlace || !config.IsMicrosoftTeams) return;
            if (!VerifyAddressFormat(smtpAddress)) return;
            this.config = config;
            this.SourceSmtpAddress = smtpAddress;
            this.SourceDomain = smtpAddress.Substring(smtpAddress.LastIndexOf('@') + 1);
            domainMap = new SpecialCustomers().GetDomainMapping(RestoreConfig.TenantGroupId);
            if (domainMap == null) return;
            IsSpecialCustomer = true;
            if (domainMap.TryGetValue(SourceDomain, out string domain))
            {
                targetDomain = domain;
                targetSmtpAddress = SourceSmtpAddress.Replace(SourceDomain, domain);
                IsSpecialTeam = true;
            }
        }
        public string RegenerateTeamAddress()
        {
            return IsSpecialTeam ? targetSmtpAddress : SourceSmtpAddress;
        }
        public void AdaptToTeamMetadata()
        {
            RegenerateBponsInfo();
            RegenerateRestoreMapping();
        }
        private void RegenerateBponsInfo()
        {
            if (IsSpecialTeam)
            {
                if (RestoreConfig.EmailBposInfoMap.TryGetValue(SourceSmtpAddress, out BposInfo bposInfo))
                {
                    RestoreConfig.EmailBposInfoMap[RegenerateTeamAddress()] = bposInfo;
                }
            }
        }
        private void RegenerateRestoreMapping()
        {
            if (IsSpecialTeam)
            {
                config.UserMapping = new Dictionary<string, string>();
                config.DomainMapping = domainMap;
            }
        }

        private bool VerifyAddressFormat(string address)
        {
            if (string.IsNullOrEmpty(address)) return false;
            var index = address.Trim('@').LastIndexOf('@');
            return index >=0;
        }
    }
}