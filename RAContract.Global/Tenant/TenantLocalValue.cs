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
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Account.Security;
using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Threading;
using AvePoint.RA.Contract.ControlPlus;

namespace AvePoint.RA.Contract.Tenant
{
    public class TenantLocalValue
    {
        public static string CurrentCulture
        {
            get { return _currentCulture.Value; }
            set { _currentCulture.Value = value; }
        }
        
        public static string LogonGroupId
        {
            get { return _logonGroupId.Value; }
            set { _logonGroupId.Value = value; }
        }

        public static string LogonUserId
        {
            get { return _logonUserId.Value; }
            set { _logonUserId.Value = value; }
        }
        
        public static string LogonGoogleUserId
        {
            get { return _logonGoogleUserId.Value; }
            set { _logonGoogleUserId.Value = value; }
        }

        public static string LogonUserEmail
        {
            get { return _logonUserEmail.Value; }
            set { _logonUserEmail.Value = value; }
        }

        public static string DisplayName
        {
            get { return _displayName.Value; }
            set { _displayName.Value = value; }
        }

        public static string PartnerUser
        {
            get { return _partnerUser.Value; }
            set { _partnerUser.Value = value; }
        }

        public static string CallerType
        {
            get { return _callerType.Value; }
            set { _callerType.Value = value; }
        }

        public static string PartnerOwner
        {
            get { return _partnerOwner.Value; }
            set { _partnerOwner.Value = value; }
        }

        public static RMAccountType AccountType
        {
            get { return _accountType.Value; }
            set { _accountType.Value = value; }
        }

        public static string LogonGroupEmail
        {
            get { return _logonGroupEmail.Value; }
            set { _logonGroupEmail.Value = value; }
        }

        public static string LogonGroupDisplayName
        {
            get { return _logonGroupDisplayName.Value; }
            set { _logonGroupDisplayName.Value = value; }
        }

        public static string RecordsUrl
        {
            get { return _recordsUrl.Value; }
            set { _recordsUrl.Value = value; }
        }

        public static List<AzureADGroupInfo> UserGroups
        {
            get { return _userGroups.Value; }
            set { _userGroups.Value = value; }
        }

        public static string AccountNumber
        {
            get { return _accountNumber.Value; }
            set { _accountNumber.Value = value; }
        }

        public static string Company
        {
            get { return _company.Value; }
            set { _company.Value = value; }
        }

        public static string TimezoneId
        {
            get { return _timezoneId.Value; }
            set { _timezoneId.Value = value; }
        }
        
        public static RequesterTypeEnum RequesterType
        {
            get { return _requesterType.Value; }
            set { _requesterType.Value = value; }
        }

        public static string TraceId
        {
            get { return _traceId.Value; }
            set { _traceId.Value = value; }
        }

        public static string ClientName
        {
            get { return _clientName.Value; }
            set { _clientName.Value = value; }
        }

        public static string MultiGeoIP
        {
            get { return _multigeoIp.Value; }
            set { _multigeoIp.Value = value; }
        }

        public static void Init(RMIdentity identity)
        {
            LogonGroupId = identity.TenantGroupId;
            LogonUserEmail = identity.Name;
            LogonUserId = identity.AccountId;
            DisplayName = identity.DisplayName;
            PartnerUser = identity.PartnerUser;
            PartnerOwner = identity.PartnerOwner;
            AccountType = identity.AccountType;
            RecordsUrl = identity.Url;
            AccountNumber = identity.AccountNumber;
            Company = identity.Company;
        }

        public static void Clear()
        {
            LogonGroupId = null;
            LogonUserEmail = null;
            LogonUserId = null;
            LogonGoogleUserId = null;
            RequesterType = RequesterTypeEnum.Opus;
            DisplayName = null;
            AccountType = RMAccountType.None;
            RecordsUrl = null;
            AccountNumber = null;
            Company = null;
            PartnerUser = null;
            PartnerOwner = null;
            CallerType = null;
            TraceId = null;
            ClientName = null;
        }


        private static AsyncLocal<string> _currentCulture = new AsyncLocal<string>();
        private static AsyncLocal<string> _logonGroupId = new AsyncLocal<string>();
        private static AsyncLocal<string> _logonUserId = new AsyncLocal<string>();
        private static AsyncLocal<string> _logonGoogleUserId = new AsyncLocal<string>();
        private static AsyncLocal<string> _logonUserEmail = new AsyncLocal<string>();
        private static AsyncLocal<string> _displayName = new AsyncLocal<string>();
        private static AsyncLocal<string> _partnerUser = new AsyncLocal<string>();
        private static AsyncLocal<string> _partnerOwner = new AsyncLocal<string>();
        private static AsyncLocal<string> _callerType = new AsyncLocal<string>();

        private static AsyncLocal<RMAccountType> _accountType = new AsyncLocal<RMAccountType>();
        private static AsyncLocal<string> _logonGroupEmail = new AsyncLocal<string>();
        private static AsyncLocal<string> _logonGroupDisplayName = new AsyncLocal<string>();
        private static AsyncLocal<string> _recordsUrl = new AsyncLocal<string>();
        private static AsyncLocal<List<AzureADGroupInfo>> _userGroups = new AsyncLocal<List<AzureADGroupInfo>>();
        private static AsyncLocal<string> _accountNumber = new AsyncLocal<string>();
        private static AsyncLocal<string> _company = new AsyncLocal<string>();
        private static AsyncLocal<string> _timezoneId = new AsyncLocal<string>();
        private static AsyncLocal<RequesterTypeEnum> _requesterType = new AsyncLocal<RequesterTypeEnum>();
        private static AsyncLocal<string> _traceId = new AsyncLocal<string>();
        private static AsyncLocal<string> _clientName = new AsyncLocal<string>();
        private static AsyncLocal<string> _multigeoIp = new AsyncLocal<string>();
    }
}
