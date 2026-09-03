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

namespace AutoInstallationCommon.ActiveDirectory
{
    public class ActiveDirectoryPropertyNames
    {
        public const string OBJECT_SID = "objectSid";
        public const string COMMON_NAME = "cn";
        public const string MANAGER = "manager";
        public const string DISTINGUISHED_NAME = "distinguishedname";
        public const string MEMBER_OF = "memberOf";
        public const string MEMBER = "member";
        public const string SAMACCOUNTNAME = "samaccountname";
        public const string OBJECTCLASS_USER = "user";
        public const string OBJECTCLASS_GROUP = "group";
        public const string NAME = "name";
        public const string ADSPATH = "adsPath";
        public const string DEFAULT_GROUP = "tokenGroups";
        public const string USER_PRINCIPAL_NAME = "userprincipalname";
        public const string MSDS_PRINCIPAL_NAME = "msDS-PrincipalName";
        public const string DISPLAY_NAME = "displayName";
        public const string DEPARTMENT = "department";
        public const string USER_ACCOUNT_CONTROL = "userAccountControl";
        public const string MAIL = "mail";
        public const string GROUP_TYPE = "grouptype";

        #region Naming Context

        public class NamingContext
        {
            public const string ROOTDSE_FORMAT = "LDAP://{0}/RootDSE";
            public const string DEFAULT_NAMING_CONTEXT = "defaultNamingContext";
            public const string CONFIGURATION_NAMING_CONTEXT = "configurationNamingContext";
            public const string N_CNAME = "nCNAME";
            public const string NETBIOSNAME = "netBiosName";
        }

        #endregion
    }

    public enum ActiveDirectoryObjectType
    {
        User = 0,
        Group = 1
    }

    public class ObjectClasses
    {
        public const string USER = "user";
        public const string TOP = "top";
        public const string GROUP = "group";
        public const string FOREIGN_SECURITY_PRINCIPAL = "foreignSecurityPrincipal";

        #region Naming Context

        public class NamingContext
        {
            public const string CROSS_REF = "crossRef";
        }

        #endregion
    }

    public class ObjectCategories
    {
        public const string PERSON = "person";
        public const string GROUP = "group";
    }
}