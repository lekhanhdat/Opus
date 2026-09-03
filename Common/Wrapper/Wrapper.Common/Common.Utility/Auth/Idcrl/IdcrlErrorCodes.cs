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

namespace AvePoint.Wrapper.Common
{
    using System;
    using System.Collections.Generic;
    using System.Runtime.InteropServices;

    internal static class IdcrlErrorCodes
    {
        public const int PPCRL_AUTHREQUIRED_E_PASSWORD = -2147186672;
        public const int PPCRL_AUTHREQUIRED_E_UNKNOWN = -2147186668;
        public const int PPCRL_AUTHSTATE_E_EXPIRED = -2147186687;
        public const int PPCRL_AUTHSTATE_E_UNAUTHENTICATED = -2147186688;
        public const int PPCRL_AUTHSTATE_S_AUTHENTICATED_OFFLINE = 0x48802;
        public const int PPCRL_AUTHSTATE_S_AUTHENTICATED_PARTNER = 0x48804;
        public const int PPCRL_AUTHSTATE_S_AUTHENTICATED_PASSWORD = 0x48803;
        public const int PPCRL_E_AUTH_SERVICE_UNAVAILABLE = -2147186583;
        public const int PPCRL_E_AUTHBLOB_INVALID = -2147186552;
        public const int PPCRL_E_AUTHBLOB_NOT_FOUND = -2147186553;
        public const int PPCRL_E_AUTHBLOB_TOO_LARGE = -2147186554;
        public const int PPCRL_E_BUILD_CERT_REQUEST_FAILED = -2147186556;
        public const int PPCRL_E_BUSY = -2147186558;
        public const int PPCRL_E_CALLBACK_REQUIRED = -2147186579;
        public const int PPCRL_E_CALLER_NOT_SIGNED = -2147186559;
        public const int PPCRL_E_CERT_CA_ROLLOVER = -2147186540;
        public const int PPCRL_E_CERT_INVALID_ISSUER = -2147186570;
        public const int PPCRL_E_CERT_INVALID_POP = -2147186560;
        public const int PPCRL_E_CERT_NOT_VALID_FOR_MINTTL = -2147186572;
        public const int PPCRL_E_CERTIFICATE_NOT_FOUND = -2147186555;
        public const int PPCRL_E_CREDINFO_CORRUPTED = -2147186544;
        public const int PPCRL_E_CREDPROP_NOTFOUND = -2147186543;
        public const int PPCRL_E_CREDTARGETNAME_INVALID = -2147186545;
        public const int PPCRL_E_DOWNLOAD_FILE_FAILED = -2147186557;
        public const int PPCRL_E_EXTPROP_NOTFOUND = -2147186551;
        public const int PPCRL_E_FORBIDDEN_NAMESPACE = -2147186537;
        public const int PPCRL_E_IDENTITY_NOCID = -2147186535;
        public const int PPCRL_E_IDENTITY_NOT_AUTHENTICATED = -2147186591;
        public const int PPCRL_E_IE_MISCONFIGURED = -2147186534;
        public const int PPCRL_E_ILLEGAL_LOGONIDENTITY_FLAG = -2147186573;
        public const int PPCRL_E_INVALID_AUTH_SERVICE_RESPONSE = -2147186582;
        public const int PPCRL_E_INVALID_RPS_TOKEN = -2147186530;
        public const int PPCRL_E_INVALID_RSTPARAMS = -2147186575;
        public const int PPCRL_E_INVALID_URL = -2147186528;
        public const int PPCRL_E_INVALIDFLAGS = -2147186577;
        public const int PPCRL_E_MISSING_FILE = -2147186574;
        public const int PPCRL_E_NO_CERTSTORE_FOR_ISSUERS = -2147186569;
        public const int PPCRL_E_NO_LINKEDACCOUNTS = -2147186542;
        public const int PPCRL_E_NO_LINKEDHANDLE = -2147186541;
        public const int PPCRL_E_NO_MEMBER_NAME_SET = -2147186580;
        public const int PPCRL_E_NO_UI = -2147186532;
        public const int PPCRL_E_NOT_UI_ERROR = -2147186529;
        public const int PPCRL_E_OFFLINE_AUTH = -2147186568;
        public const int PPCRL_E_REALM_LOOKUP = -2147186539;
        public const int PPCRL_E_RESPONSE_TOO_LARGE = -2147186550;
        public const int PPCRL_E_SIGCHECK_FAILED = -2147186547;
        public const int PPCRL_E_SIGN_POP_FAILED = -2147186567;
        public const int PPCRL_E_UNABLE_TO_INITIALIZE_CRYPTO_PROVIDER = -2147186581;
        public const int PPCRL_E_UNABLE_TO_RETRIEVE_CERT = -2147186576;
        public const int PPCRL_E_UNABLE_TO_RETRIEVE_SERVICE_TOKEN = -2147186590;
        public const int PPCRL_E_USER_NOTFOUND = -2147186548;
        public const int PPCRL_REQUEST_E_ACCOUNT_CONVERSION_NEEDED = -2147186447;
        public const int PPCRL_REQUEST_E_AUTH_EXPIRED = -2147186631;
        public const int PPCRL_REQUEST_E_AUTH_SERVER_ERROR = -2147186656;
        public const int PPCRL_REQUEST_E_BAD_MEMBER_NAME_OR_PASSWORD = -2147186655;
        public const int PPCRL_REQUEST_E_BAD_MEMBER_NAME_OR_PASSWORD_FED = -2147186445;
        public const int PPCRL_REQUEST_E_CANCELLED = -2147186462;
        public const int PPCRL_REQUEST_E_CERT_PARSE_ERROR = -2147186452;
        public const int PPCRL_REQUEST_E_CLIENT_DEPRECATED = -2147186463;
        public const int PPCRL_REQUEST_E_DUPLICATE_SERVICETARGET = -2147186460;
        public const int PPCRL_REQUEST_E_EMAIL_VALIDATION_REQUIRED = -2147186634;
        public const int PPCRL_REQUEST_E_FLOWDISABLED = -2147186449;
        public const int PPCRL_REQUEST_E_FORCE_CHANGE_PASSWORD_REQUIRED = -2147186649;
        public const int PPCRL_REQUEST_E_FORCE_CHANGE_SQSA = -2147186640;
        public const int PPCRL_REQUEST_E_FORCE_RENAME_REQUIRED = -2147186650;
        public const int PPCRL_REQUEST_E_FORCE_SIGNIN = -2147186459;
        public const int PPCRL_REQUEST_E_HIP_ON_FIRST_LOGIN = -2147186444;
        public const int PPCRL_REQUEST_E_INVALID_CARDSPACE_TOKEN = -2147186443;
        public const int PPCRL_REQUEST_E_INVALID_MEMBER_NAME = -2147186643;
        public const int PPCRL_REQUEST_E_INVALID_PKCS10 = -2147186594;
        public const int PPCRL_REQUEST_E_INVALID_PKCS10_KEYLEN = -2147186461;
        public const int PPCRL_REQUEST_E_INVALID_PKCS10_TIMESTAMP = -2147186595;
        public const int PPCRL_REQUEST_E_INVALID_POLICY = -2147186644;
        public const int PPCRL_REQUEST_E_INVALID_SERVICE_TIMESTAMP = -2147186596;
        public const int PPCRL_REQUEST_E_KID_HAS_NO_CONSENT = -2147186605;
        public const int PPCRL_REQUEST_E_MISSING_HASHED_PASSWORD = -2147186464;
        public const int PPCRL_REQUEST_E_MISSING_PRIMARY_CREDENTIAL = -2147186642;
        public const int PPCRL_REQUEST_E_NO_NETWORK = -2147186616;
        public const int PPCRL_REQUEST_E_PARTNER_AUTHENTICATION_BAD_ELEMENTS = -2147186471;
        public const int PPCRL_REQUEST_E_PARTNER_BAD_MEMBER_NAME_OR_PASSWORD = -2147186446;
        public const int PPCRL_REQUEST_E_PARTNER_BAD_REQUEST = -2147186470;
        public const int PPCRL_REQUEST_E_PARTNER_EXPIRED_DATA = -2147186469;
        public const int PPCRL_REQUEST_E_PARTNER_HAS_NO_ASYMMETRIC_KEY = -2147186645;
        public const int PPCRL_REQUEST_E_PARTNER_INVALID_REQUEST = -2147186474;
        public const int PPCRL_REQUEST_E_PARTNER_INVALID_SCOPE = -2147186467;
        public const int PPCRL_REQUEST_E_PARTNER_INVALID_SECURITY_TOKEN = -2147186472;
        public const int PPCRL_REQUEST_E_PARTNER_INVALID_TIME_RANGE = -2147186468;
        public const int PPCRL_REQUEST_E_PARTNER_LOGIN = -2147186450;
        public const int PPCRL_REQUEST_E_PARTNER_NEED_CERTIFICATE = -2147186458;
        public const int PPCRL_REQUEST_E_PARTNER_NEED_PASSWORD = -2147186456;
        public const int PPCRL_REQUEST_E_PARTNER_NEED_PIN = -2147186457;
        public const int PPCRL_REQUEST_E_PARTNER_NEED_STRONGPW = -2147186633;
        public const int PPCRL_REQUEST_E_PARTNER_NEED_STRONGPW_EXPIRY = -2147186632;
        public const int PPCRL_REQUEST_E_PARTNER_NOT_FOUND = -2147186646;
        public const int PPCRL_REQUEST_E_PARTNER_RENEW_NEEDED = -2147186466;
        public const int PPCRL_REQUEST_E_PARTNER_REQUEST_FAILED = -2147186473;
        public const int PPCRL_REQUEST_E_PARTNER_SERVER_ERROR = -2147186451;
        public const int PPCRL_REQUEST_E_PARTNER_UNABLE_TO_RENEW = -2147186465;
        public const int PPCRL_REQUEST_E_PASSWORD_EXPIRED = -2147186639;
        public const int PPCRL_REQUEST_E_PASSWORD_LOCKED_OUT = -2147186653;
        public const int PPCRL_REQUEST_E_PASSWORD_LOCKED_OUT_BAD_PASSWORD_OR_HIP = -2147186652;
        public const int PPCRL_REQUEST_E_PENDING_NETWORK_REQUEST = -2147186641;
        public const int PPCRL_REQUEST_E_PROFILE_ACCRUE_REQUIRED = -2147186636;
        public const int PPCRL_REQUEST_E_RSTR_FAULT = -2147186603;
        public const int PPCRL_REQUEST_E_RSTR_MISSING_BASE64CERT = -2147186601;
        public const int PPCRL_REQUEST_E_RSTR_MISSING_PRIVATE_KEY = -2147186597;
        public const int PPCRL_REQUEST_E_RSTR_MISSING_TOKENTYPE = -2147186600;
        public const int PPCRL_REQUEST_E_SCHANNEL_ERROR = -2147186453;
        public const int PPCRL_REQUEST_E_STRONG_PASSWORD_REQUIRED = -2147186648;
        public const int PPCRL_REQUEST_E_UNKNOWN = -2147186615;
        public const int PPCRL_REQUEST_E_USER_CANCELED = -2147186622;
        public const int PPCRL_REQUEST_E_USER_FORGOT_PASSWORD = -2147186623;
        public const int PPCRL_REQUEST_E_USER_NOT_LINKED = -2147186448;
        public const int PPCRL_REQUEST_S_IO_PENDING = 0x48847;
        public const int PPCRL_REQUEST_S_IO_PENDING_NO_SLC = 0x488ea;
        public const int PPCRL_REQUEST_S_OK_NO_SLC = 0x488e9;
        public const int PPCRL_S_NO_AUTHENTICATION_REQUIRED = 0x48863;
        public const int PPCRL_S_NO_MORE_IDENTITIES = 0x48860;
        public const int PPCRL_S_NO_SUCH_CREDENTIAL = 0x48862;
        public const int PPCRL_S_OK_CLIENTTIME = 0x48875;
        public const int PPCRL_S_TOKEN_TYPE_DOES_NOT_SUPPORT_SESSION_KEY = 0x48861;
        public const int CUSTOM_MFA_REQUIRE_STRONG_PASSWORD = -2147207980;
        private static Dictionary<int, string> s_errorMap;

        static IdcrlErrorCodes()
        {
            Dictionary<int, string> dictionary = new Dictionary<int, string>();
            dictionary.Add(0x48802, "PPCRL_AUTHSTATE_S_AUTHENTICATED_OFFLINE");
            dictionary.Add(0x48803, "PPCRL_AUTHSTATE_S_AUTHENTICATED_PASSWORD");
            dictionary.Add(0x48804, "PPCRL_AUTHSTATE_S_AUTHENTICATED_PARTNER");
            dictionary.Add(0x48847, "PPCRL_REQUEST_S_IO_PENDING");
            dictionary.Add(0x48860, "PPCRL_S_NO_MORE_IDENTITIES");
            dictionary.Add(0x48861, "PPCRL_S_TOKEN_TYPE_DOES_NOT_SUPPORT_SESSION_KEY");
            dictionary.Add(0x48862, "PPCRL_S_NO_SUCH_CREDENTIAL");
            dictionary.Add(0x48863, "PPCRL_S_NO_AUTHENTICATION_REQUIRED");
            dictionary.Add(0x48875, "PPCRL_S_OK_CLIENTTIME");
            dictionary.Add(0x488e9, "PPCRL_REQUEST_S_OK_NO_SLC");
            dictionary.Add(0x488ea, "PPCRL_REQUEST_S_IO_PENDING_NO_SLC");
            dictionary.Add(-2147186688, "PPCRL_AUTHSTATE_E_UNAUTHENTICATED");
            dictionary.Add(-2147186687, "PPCRL_AUTHSTATE_E_EXPIRED");
            dictionary.Add(-2147186672, "PPCRL_AUTHREQUIRED_E_PASSWORD");
            dictionary.Add(-2147186668, "PPCRL_AUTHREQUIRED_E_UNKNOWN");
            dictionary.Add(-2147186656, "PPCRL_REQUEST_E_AUTH_SERVER_ERROR");
            dictionary.Add(-2147186655, "PPCRL_REQUEST_E_BAD_MEMBER_NAME_OR_PASSWORD");
            dictionary.Add(-2147186653, "PPCRL_REQUEST_E_PASSWORD_LOCKED_OUT");
            dictionary.Add(-2147186652, "PPCRL_REQUEST_E_PASSWORD_LOCKED_OUT_BAD_PASSWORD_OR_HIP");
            dictionary.Add(-2147186650, "PPCRL_REQUEST_E_FORCE_RENAME_REQUIRED");
            dictionary.Add(-2147186649, "PPCRL_REQUEST_E_FORCE_CHANGE_PASSWORD_REQUIRED");
            dictionary.Add(-2147186648, "PPCRL_REQUEST_E_STRONG_PASSWORD_REQUIRED");
            dictionary.Add(-2147186646, "PPCRL_REQUEST_E_PARTNER_NOT_FOUND");
            dictionary.Add(-2147186645, "PPCRL_REQUEST_E_PARTNER_HAS_NO_ASYMMETRIC_KEY");
            dictionary.Add(-2147186644, "PPCRL_REQUEST_E_INVALID_POLICY");
            dictionary.Add(-2147186643, "PPCRL_REQUEST_E_INVALID_MEMBER_NAME");
            dictionary.Add(-2147186642, "PPCRL_REQUEST_E_MISSING_PRIMARY_CREDENTIAL");
            dictionary.Add(-2147186641, "PPCRL_REQUEST_E_PENDING_NETWORK_REQUEST");
            dictionary.Add(-2147186640, "PPCRL_REQUEST_E_FORCE_CHANGE_SQSA");
            dictionary.Add(-2147186639, "PPCRL_REQUEST_E_PASSWORD_EXPIRED");
            dictionary.Add(-2147186636, "PPCRL_REQUEST_E_PROFILE_ACCRUE_REQUIRED");
            dictionary.Add(-2147186634, "PPCRL_REQUEST_E_EMAIL_VALIDATION_REQUIRED");
            dictionary.Add(-2147186633, "PPCRL_REQUEST_E_PARTNER_NEED_STRONGPW");
            dictionary.Add(-2147186632, "PPCRL_REQUEST_E_PARTNER_NEED_STRONGPW_EXPIRY");
            dictionary.Add(-2147186631, "PPCRL_REQUEST_E_AUTH_EXPIRED");
            dictionary.Add(-2147186623, "PPCRL_REQUEST_E_USER_FORGOT_PASSWORD");
            dictionary.Add(-2147186622, "PPCRL_REQUEST_E_USER_CANCELED");
            dictionary.Add(-2147186616, "PPCRL_REQUEST_E_NO_NETWORK");
            dictionary.Add(-2147186615, "PPCRL_REQUEST_E_UNKNOWN");
            dictionary.Add(-2147186605, "PPCRL_REQUEST_E_KID_HAS_NO_CONSENT");
            dictionary.Add(-2147186603, "PPCRL_REQUEST_E_RSTR_FAULT");
            dictionary.Add(-2147186601, "PPCRL_REQUEST_E_RSTR_MISSING_BASE64CERT");
            dictionary.Add(-2147186600, "PPCRL_REQUEST_E_RSTR_MISSING_TOKENTYPE");
            dictionary.Add(-2147186597, "PPCRL_REQUEST_E_RSTR_MISSING_PRIVATE_KEY");
            dictionary.Add(-2147186596, "PPCRL_REQUEST_E_INVALID_SERVICE_TIMESTAMP");
            dictionary.Add(-2147186595, "PPCRL_REQUEST_E_INVALID_PKCS10_TIMESTAMP");
            dictionary.Add(-2147186594, "PPCRL_REQUEST_E_INVALID_PKCS10");
            dictionary.Add(-2147186591, "PPCRL_E_IDENTITY_NOT_AUTHENTICATED");
            dictionary.Add(-2147186590, "PPCRL_E_UNABLE_TO_RETRIEVE_SERVICE_TOKEN");
            dictionary.Add(-2147186583, "PPCRL_E_AUTH_SERVICE_UNAVAILABLE");
            dictionary.Add(-2147186582, "PPCRL_E_INVALID_AUTH_SERVICE_RESPONSE");
            dictionary.Add(-2147186581, "PPCRL_E_UNABLE_TO_INITIALIZE_CRYPTO_PROVIDER");
            dictionary.Add(-2147186580, "PPCRL_E_NO_MEMBER_NAME_SET");
            dictionary.Add(-2147186579, "PPCRL_E_CALLBACK_REQUIRED");
            dictionary.Add(-2147186577, "PPCRL_E_INVALIDFLAGS");
            dictionary.Add(-2147186576, "PPCRL_E_UNABLE_TO_RETRIEVE_CERT");
            dictionary.Add(-2147186575, "PPCRL_E_INVALID_RSTPARAMS");
            dictionary.Add(-2147186574, "PPCRL_E_MISSING_FILE");
            dictionary.Add(-2147186573, "PPCRL_E_ILLEGAL_LOGONIDENTITY_FLAG");
            dictionary.Add(-2147186572, "PPCRL_E_CERT_NOT_VALID_FOR_MINTTL");
            dictionary.Add(-2147186570, "PPCRL_E_CERT_INVALID_ISSUER");
            dictionary.Add(-2147186569, "PPCRL_E_NO_CERTSTORE_FOR_ISSUERS");
            dictionary.Add(-2147186568, "PPCRL_E_OFFLINE_AUTH");
            dictionary.Add(-2147186567, "PPCRL_E_SIGN_POP_FAILED");
            dictionary.Add(-2147186560, "PPCRL_E_CERT_INVALID_POP");
            dictionary.Add(-2147186559, "PPCRL_E_CALLER_NOT_SIGNED");
            dictionary.Add(-2147186558, "PPCRL_E_BUSY");
            dictionary.Add(-2147186557, "PPCRL_E_DOWNLOAD_FILE_FAILED");
            dictionary.Add(-2147186556, "PPCRL_E_BUILD_CERT_REQUEST_FAILED");
            dictionary.Add(-2147186555, "PPCRL_E_CERTIFICATE_NOT_FOUND");
            dictionary.Add(-2147186554, "PPCRL_E_AUTHBLOB_TOO_LARGE");
            dictionary.Add(-2147186553, "PPCRL_E_AUTHBLOB_NOT_FOUND");
            dictionary.Add(-2147186552, "PPCRL_E_AUTHBLOB_INVALID");
            dictionary.Add(-2147186551, "PPCRL_E_EXTPROP_NOTFOUND");
            dictionary.Add(-2147186550, "PPCRL_E_RESPONSE_TOO_LARGE");
            dictionary.Add(-2147186548, "PPCRL_E_USER_NOTFOUND");
            dictionary.Add(-2147186547, "PPCRL_E_SIGCHECK_FAILED");
            dictionary.Add(-2147186545, "PPCRL_E_CREDTARGETNAME_INVALID");
            dictionary.Add(-2147186544, "PPCRL_E_CREDINFO_CORRUPTED");
            dictionary.Add(-2147186543, "PPCRL_E_CREDPROP_NOTFOUND");
            dictionary.Add(-2147186542, "PPCRL_E_NO_LINKEDACCOUNTS");
            dictionary.Add(-2147186541, "PPCRL_E_NO_LINKEDHANDLE");
            dictionary.Add(-2147186540, "PPCRL_E_CERT_CA_ROLLOVER");
            dictionary.Add(-2147186539, "PPCRL_E_REALM_LOOKUP");
            dictionary.Add(-2147186537, "PPCRL_E_FORBIDDEN_NAMESPACE");
            dictionary.Add(-2147186535, "PPCRL_E_IDENTITY_NOCID");
            dictionary.Add(-2147186534, "PPCRL_E_IE_MISCONFIGURED");
            dictionary.Add(-2147186532, "PPCRL_E_NO_UI");
            dictionary.Add(-2147186530, "PPCRL_E_INVALID_RPS_TOKEN");
            dictionary.Add(-2147186529, "PPCRL_E_NOT_UI_ERROR");
            dictionary.Add(-2147186528, "PPCRL_E_INVALID_URL");
            dictionary.Add(-2147186474, "PPCRL_REQUEST_E_PARTNER_INVALID_REQUEST");
            dictionary.Add(-2147186473, "PPCRL_REQUEST_E_PARTNER_REQUEST_FAILED");
            dictionary.Add(-2147186472, "PPCRL_REQUEST_E_PARTNER_INVALID_SECURITY_TOKEN");
            dictionary.Add(-2147186471, "PPCRL_REQUEST_E_PARTNER_AUTHENTICATION_BAD_ELEMENTS");
            dictionary.Add(-2147186470, "PPCRL_REQUEST_E_PARTNER_BAD_REQUEST");
            dictionary.Add(-2147186469, "PPCRL_REQUEST_E_PARTNER_EXPIRED_DATA");
            dictionary.Add(-2147186468, "PPCRL_REQUEST_E_PARTNER_INVALID_TIME_RANGE");
            dictionary.Add(-2147186467, "PPCRL_REQUEST_E_PARTNER_INVALID_SCOPE");
            dictionary.Add(-2147186466, "PPCRL_REQUEST_E_PARTNER_RENEW_NEEDED");
            dictionary.Add(-2147186465, "PPCRL_REQUEST_E_PARTNER_UNABLE_TO_RENEW");
            dictionary.Add(-2147186464, "PPCRL_REQUEST_E_MISSING_HASHED_PASSWORD");
            dictionary.Add(-2147186463, "PPCRL_REQUEST_E_CLIENT_DEPRECATED");
            dictionary.Add(-2147186462, "PPCRL_REQUEST_E_CANCELLED");
            dictionary.Add(-2147186461, "PPCRL_REQUEST_E_INVALID_PKCS10_KEYLEN");
            dictionary.Add(-2147186460, "PPCRL_REQUEST_E_DUPLICATE_SERVICETARGET");
            dictionary.Add(-2147186459, "PPCRL_REQUEST_E_FORCE_SIGNIN");
            dictionary.Add(-2147186458, "PPCRL_REQUEST_E_PARTNER_NEED_CERTIFICATE");
            dictionary.Add(-2147186457, "PPCRL_REQUEST_E_PARTNER_NEED_PIN");
            dictionary.Add(-2147186456, "PPCRL_REQUEST_E_PARTNER_NEED_PASSWORD");
            dictionary.Add(-2147186453, "PPCRL_REQUEST_E_SCHANNEL_ERROR");
            dictionary.Add(-2147186452, "PPCRL_REQUEST_E_CERT_PARSE_ERROR");
            dictionary.Add(-2147186451, "PPCRL_REQUEST_E_PARTNER_SERVER_ERROR");
            dictionary.Add(-2147186450, "PPCRL_REQUEST_E_PARTNER_LOGIN");
            dictionary.Add(-2147186449, "PPCRL_REQUEST_E_FLOWDISABLED");
            dictionary.Add(-2147186448, "PPCRL_REQUEST_E_USER_NOT_LINKED");
            dictionary.Add(-2147186447, "PPCRL_REQUEST_E_ACCOUNT_CONVERSION_NEEDED");
            dictionary.Add(-2147186446, "PPCRL_REQUEST_E_PARTNER_BAD_MEMBER_NAME_OR_PASSWORD");
            dictionary.Add(-2147186445, "PPCRL_REQUEST_E_BAD_MEMBER_NAME_OR_PASSWORD_FED");
            dictionary.Add(-2147186444, "PPCRL_REQUEST_E_HIP_ON_FIRST_LOGIN");
            dictionary.Add(-2147186443, "PPCRL_REQUEST_E_INVALID_CARDSPACE_TOKEN");
            s_errorMap = dictionary;
        }

        internal static bool TryGetErrorStringId(int hr, out string stringId)
        {
            return s_errorMap.TryGetValue(hr, out stringId);
        }
    }
}

