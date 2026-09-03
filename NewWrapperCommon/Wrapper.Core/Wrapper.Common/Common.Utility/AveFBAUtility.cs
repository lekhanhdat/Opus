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
using System.Text;
using AvePoint.GCommon;
using System.Configuration.Provider;
using System.Configuration;
using System.Web.Security;
using System.Web.Configuration;
using System.Reflection;

namespace AvePoint.Wrapper.Common
{
    public class AveFBAUtility
    {
        private static AveLogger mLog = AveLogger.GetInstance(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static object mLock = new object();
        private static string mCurrentConfigPath;

        public static void InitProvider(string configPath)
        {
            if (!string.IsNullOrEmpty(mCurrentConfigPath) && mCurrentConfigPath.Equals(configPath, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }
            lock (mLock)
            {
                if (!string.IsNullOrEmpty(mCurrentConfigPath) && mCurrentConfigPath.Equals(configPath, StringComparison.OrdinalIgnoreCase))
                {
                    return;
                }
                mCurrentConfigPath = configPath;
                mLog.Info(configPath);
                InitProvider();
            }

        }

        public static ProviderBase GetMemberShipProvider(string providerName, string configPath, bool membershipOrRole)
        {
            InitProvider(configPath);
            ProviderCollection providers = membershipOrRole ? (ProviderCollection)Membership.Providers : Roles.Providers;
            return providers[providerName];
        }

        #region Init Provider

        private static void InitProvider()
        {
            try
            {
                ExeConfigurationFileMap fileMap = new ExeConfigurationFileMap() { ExeConfigFilename = mCurrentConfigPath };
                Configuration configuration = ConfigurationManager.OpenMappedExeConfiguration(fileMap, ConfigurationUserLevel.None);
                Log(configuration);
                InitMembershipProviders(configuration);
                InitRoleProviders(configuration);
            }
            catch (Exception e)
            {
                mLog.Error("error occurred when init membership from :" + mCurrentConfigPath + " ; reason:" + e.ToString());
            }
        }

        private static void Log(Configuration configuration)
        {
            MembershipSection membership = configuration.GetSection("system.web/membership") as MembershipSection;
            RoleManagerSection roleManager = configuration.GetSection("system.web/roleManager") as RoleManagerSection;
            ConnectionStringsSection connectionStringsSection = configuration.ConnectionStrings;
            string mString = ToString(membership);
            string rString = ToString(roleManager);
            string connString = ToString(connectionStringsSection);
            mLog.Info("Membership:{0}. \nRole Manger:{1}. \nConnection Strings:{2}.", mString, rString, connString);
        }

        private static string ToString(MembershipSection membership)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Default Provider:{0}. \n", membership.DefaultProvider);
            foreach (ProviderSettings provider in membership.Providers)
            {
                sb.AppendFormat("[Name:{0}. ", provider.Name);
                sb.AppendFormat("Type:{0}. ", provider.Type);
                foreach (string key in provider.Parameters.Keys)
                {
                    if (key.IndexOf("password", StringComparison.OrdinalIgnoreCase) < 0 && key.IndexOf("pwd", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        sb.AppendFormat("{0}={1}. ", key, provider.Parameters[key]);
                    }
                }
                sb.AppendLine("].");
            }
            return sb.ToString();
        }

        private static string ToString(RoleManagerSection roleManager)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Default Provider:{0}. Enable:{1}. Domain:{2}. \n", roleManager.DefaultProvider, roleManager.Enabled, roleManager.Domain);
            foreach (ProviderSettings provider in roleManager.Providers)
            {
                sb.AppendFormat("[Name:{0}. ", provider.Name);
                sb.AppendFormat("Type:{0}. ", provider.Type);
                foreach (string key in provider.Parameters.Keys)
                {
                    if (key.IndexOf("password", StringComparison.OrdinalIgnoreCase) < 0 && key.IndexOf("pwd", StringComparison.OrdinalIgnoreCase) < 0)
                    {
                        sb.AppendFormat("{0}={1}. ", key, provider.Parameters[key]);
                    }
                }
                sb.AppendLine("].");
            }
            return sb.ToString();
        }

        private static string ToString(ConnectionStringsSection conns)
        {
            StringBuilder sb = new StringBuilder();
            foreach (ConnectionStringSettings conn in conns.ConnectionStrings)
            {
                sb.AppendFormat("[Name:{0}. ", conn.Name);
                sb.AppendFormat("ProviderName:{0}. ", conn.ProviderName);
                string connectionString = conn.ConnectionString;
                int index = connectionString.IndexOf("password", StringComparison.OrdinalIgnoreCase);
                if (index < 0)
                {
                    index = connectionString.IndexOf("pwd", StringComparison.OrdinalIgnoreCase);
                }
                if (index >= 0)
                {
                    int endIndex = connectionString.IndexOf(';', index);
                    connectionString = connectionString.Remove(index, (endIndex > 0 ? endIndex : connectionString.Length) - index);
                }
                sb.AppendFormat("ConnectionString:{0}]. ", connectionString);
            }
            return sb.ToString();
        }

        private static void InitMembershipProviders(Configuration configuration)
        {
            MembershipSection membership = configuration.GetSection("system.web/membership") as MembershipSection;
            ConnectionStringsSection connectionStringsSection = configuration.ConnectionStrings;
            Dictionary<string, object> membershipFields = new Dictionary<string, object>();
            membershipFields["s_InitializeException"] = null;
            //membershipFields["s_Initialized"] = true;
            string hashAlgorithmType = membership.HashAlgorithmType;
            if (string.IsNullOrEmpty(hashAlgorithmType))
            {
                membershipFields["s_HashAlgorithmFromConfig"] = false;
                membershipFields["s_HashAlgorithmType"] = "SHA1";
                MachineKeySection machineKey = configuration.GetSection("system.web/machineKey") as MachineKeySection;
                if ((machineKey != null) && (machineKey.Validation == MachineKeyValidation.MD5))
                {
                    membershipFields["s_HashAlgorithmType"] = "MD5";
                }
            }
            else
            {
                membershipFields["s_HashAlgorithmType"] = hashAlgorithmType;
                membershipFields["s_HashAlgorithmFromConfig"] = true;
            }
            membershipFields["s_UserIsOnlineTimeWindow"] = (int)membership.UserIsOnlineTimeWindow.TotalMinutes;
            SetFields(typeof(Membership), null, membershipFields);
            membershipFields.Clear();
            ProviderCollection membershipProviders = GetProviders(true, membership.Providers, connectionStringsSection);
            membershipFields["s_Providers"] = membershipProviders;
            if (((membership.DefaultProvider == null)))// || (membership.Providers == null)) || (membership.Providers.Count < 1))
            {
                mLog.Error("Default membership provider is not specified.");
            }
            else
            {
                membershipFields["s_Provider"] = membershipProviders[membership.DefaultProvider];
                if (membershipFields["s_Provider"] == null)
                {
                    mLog.Error("Default membership {0} cannot be found.", membership.DefaultProvider);
                    membershipFields.Remove("s_Provider");
                }
            }
            SetFields(typeof(Membership), null, membershipFields);
            membershipFields.Clear();
        }

        private static void InitRoleProviders(Configuration configuration)
        {
            RoleManagerSection roleManager = configuration.GetSection("system.web/roleManager") as RoleManagerSection;
            ConnectionStringsSection connectionStringsSection = configuration.ConnectionStrings;
            Dictionary<string, object> rolesFields = new Dictionary<string, object>();
            rolesFields["s_InitializeException"] = null;
            rolesFields["s_Initialized"] = true;
            rolesFields["s_InitializedDefaultProvider"] = true;
            rolesFields["s_Enabled"] = roleManager.Enabled;
            if (roleManager.Enabled)
            {
                rolesFields["s_EnabledSet"] = true;
            }
            rolesFields["s_CookieName"] = roleManager.CookieName;
            rolesFields["s_CacheRolesInCookie"] = roleManager.CacheRolesInCookie;
            rolesFields["s_CookieTimeout"] = (int)roleManager.CookieTimeout.TotalMinutes;
            rolesFields["s_CookiePath"] = roleManager.CookiePath;
            rolesFields["s_CookieRequireSSL"] = roleManager.CookieRequireSSL;
            rolesFields["s_CookieSlidingExpiration"] = roleManager.CookieSlidingExpiration;
            rolesFields["s_CookieProtection"] = roleManager.CookieProtection;
            rolesFields["s_Domain"] = roleManager.Domain;
            rolesFields["s_CreatePersistentCookie"] = roleManager.CreatePersistentCookie;
            rolesFields["s_MaxCachedResults"] = roleManager.MaxCachedResults;
            SetFields(typeof(Roles), null, rolesFields);
            rolesFields.Clear();
            if (roleManager.Enabled)
            {
                if (roleManager.MaxCachedResults < 0)
                {
                    mLog.Error("maxCachedResults value must be non negative integer.");
                }
                ProviderCollection roleProviders = GetProviders(false, roleManager.Providers, connectionStringsSection);
                rolesFields["s_Providers"] = roleProviders;
                if (roleManager.DefaultProvider == null)
                {
                    mLog.Error("Default role provider is not specified.");
                }
                else
                {
                    rolesFields["s_Provider"] = roleProviders[roleManager.DefaultProvider];
                    if (rolesFields["s_Provider"] == null)
                    {
                        mLog.Error("Default membership {0} cannot be found.", roleManager.DefaultProvider);
                        rolesFields.Remove("s_Provider");
                    }
                }
                SetFields(typeof(Roles), null, rolesFields);
                rolesFields.Clear();
            }
        }

        private static ProviderCollection GetProviders(bool isMembershipOrRole, ProviderSettingsCollection providerSettingsCollection, ConnectionStringsSection connectionStringsSection)
        {
            ProviderCollection existProviders = isMembershipOrRole ? (ProviderCollection)Membership.Providers : Roles.Providers;
            ProviderCollection providers = isMembershipOrRole ? (ProviderCollection)new MembershipProviderCollection() : new RoleProviderCollection();
            if (existProviders != null)
            {
                foreach (ProviderBase provider in existProviders)
                {
                    providers.Add(provider);
                }
            }
            Type type = isMembershipOrRole ? typeof(MembershipProvider) : typeof(RoleProvider);
            foreach (ProviderSettings settings in providerSettingsCollection)
            {
                Type c = Type.GetType(settings.Type, false, true);
                if (c == null || !type.IsAssignableFrom(c))
                {
                    mLog.Error("{0} cannot be found or it is a invalid provider type of {1}.", settings.Type, type.Name);
                    continue;
                }
                ProviderBase provider = (ProviderBase)Activator.CreateInstance(c);
                AveFBAProviderInitialize.ProviderInitialize(provider, settings, connectionStringsSection.ConnectionStrings[settings.Parameters["connectionStringName"]]);
                providers.Remove(provider.Name);
                providers.Add(provider);
            }
            providers.SetReadOnly();
            return providers;
        }

        private static void SetFields(Type type, object instance, Dictionary<string, object> values)
        {
            BindingFlags flags = BindingFlags.NonPublic | BindingFlags.SetField;
            if (instance == null)
            {
                flags |= BindingFlags.Static;
            }
            else
            {
                flags |= BindingFlags.Instance;
            }
            foreach (var value in values)
            {
                FieldInfo field = type.GetField(value.Key, flags);
                if (field != null)
                {
                    field.SetValue(instance, value.Value);
                }
            }
        }

        #endregion
    }

    internal class AveFBAProviderInitialize
    {
        public static void ProviderInitialize(ProviderBase provider, ProviderSettings providerSettings, ConnectionStringSettings connectionStringSettings)
        {
            if (provider is SqlMembershipProvider)
            {
                SqlMemberShipProviderInitialize(provider as SqlMembershipProvider, providerSettings, connectionStringSettings);
            }
            else if (provider is SqlRoleProvider)
            {
                SqlRoleProviderInitialize(provider as SqlRoleProvider, providerSettings, connectionStringSettings);
            }
            else if (provider is ActiveDirectoryMembershipProvider)
            {
                ActiveDirectoryMembershipProviderInitialize(provider as ActiveDirectoryMembershipProvider, providerSettings, connectionStringSettings);
            }
            else
            {
                provider.Initialize(providerSettings.Name, providerSettings.Parameters);
            }
        }

        private static void SqlMemberShipProviderInitialize(SqlMembershipProvider provider, ProviderSettings providerSettings, ConnectionStringSettings connectionStringSettings)
        {
            try
            {
                provider.Initialize(providerSettings.Name, providerSettings.Parameters);
            }
            catch (ProviderException)
            {
                // in the process config file, there is no connectiongstrings node, the  Initialize method will search the config file to get the connectiongstring, 
                // so throw a ProviderException.
                FieldInfo f1 = typeof(SqlMembershipProvider).GetField("_sqlConnectionString", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField);
                f1.SetValue(provider, connectionStringSettings.ConnectionString);
            }
        }

        private static void SqlRoleProviderInitialize(SqlRoleProvider provider, ProviderSettings providerSettings, ConnectionStringSettings connectionStringSettings)
        {
            try
            {
                provider.Initialize(providerSettings.Name, providerSettings.Parameters);
            }
            catch (ProviderException)
            {
                // in the process config file, there is no connectiongstrings node, the  Initialize method will search the config file to get the connectiongstring, 
                // so throw a ProviderException.
                FieldInfo f1 = typeof(SqlRoleProvider).GetField("_sqlConnectionString", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField);
                f1.SetValue(provider, connectionStringSettings.ConnectionString);

                // in SqlRoleProvider, the attribute _AppName's value is set after _sqlConnectionString.
                FieldInfo appNameField = typeof(SqlRoleProvider).GetField("_AppName", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField);
                appNameField.SetValue(provider, providerSettings.Parameters["applicationName"]);
            }
        }

        private static void ActiveDirectoryMembershipProviderInitialize(ActiveDirectoryMembershipProvider provider, ProviderSettings providerSettings, ConnectionStringSettings connectionStringSettings)
        {

            Type runtimeConfigType = Type.GetType("System.Web.Configuration.RuntimeConfig, System.Web, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a");
            MethodInfo method = runtimeConfigType.GetMethod("GetAppConfig", BindingFlags.NonPublic | BindingFlags.Static);
            object runtimeConfig = method.Invoke(null, new object[] { });

            PropertyInfo runtimeConnectionStringsSectionProperty = runtimeConfigType.GetProperty("ConnectionStrings", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.GetProperty);

            ConnectionStringsSection runtimeConnectionStringsSection = (ConnectionStringsSection)runtimeConnectionStringsSectionProperty.GetValue(runtimeConfig, null);
            ConnectionStringSettingsCollection runtimeConnectionStrings = runtimeConnectionStringsSection.ConnectionStrings;

            if (runtimeConnectionStrings[connectionStringSettings.Name] != null)
            {
                provider.Initialize(providerSettings.Name, providerSettings.Parameters);
            }
            else
            {
                Type configurationElementCollectionType = typeof(ConfigurationElementCollection);
                FieldInfo readOnlyField = configurationElementCollectionType.GetField("bReadOnly", BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.SetField);

                readOnlyField.SetValue(runtimeConnectionStrings, false);
                runtimeConnectionStrings.Add(connectionStringSettings);
                provider.Initialize(providerSettings.Name, providerSettings.Parameters);
                runtimeConnectionStrings.Remove(connectionStringSettings.Name);
                readOnlyField.SetValue(runtimeConnectionStrings, true);
                //MethodInfo resetModified = configurationElementCollectionType.GetMethod("ResetModified", BindingFlags.NonPublic | BindingFlags.Instance);
                //resetModified.Invoke(runtimeConnectionStrings, new object[] { });
            }
        }
    }
}
