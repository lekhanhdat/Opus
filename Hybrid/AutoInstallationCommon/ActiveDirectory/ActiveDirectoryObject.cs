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
using System.DirectoryServices;
using System.Linq;
using System.Security.Principal;
using System.Text;

namespace AutoInstallationCommon.ActiveDirectory
{
    public class ActiveDirectoryObject : IDisposable
    {
        private static readonly Logs logger = Logs.CreateUniformLog();

        public List<ActiveDirectoryObject> Members
        {
            get
            {
                var members = new List<ActiveDirectoryObject>();
                foreach (PropertyValueCollection sid in Properties["tokenGroups"])
                    members.Add(Domain.CreateObjectBySid(sid.Value.ToString()));

                return members;
            }
        }

        public void Dispose()
        {
            if (Entry != null) Entry.Dispose();
        }

        public ActiveDirectoryObject ToDetailViaLDAP()
        {
            Domain = Domain.ConnectGlobalCatalog();
            Entry = Domain.CreateEntry(DistinguishedName, "LDAP://");
            return this;
        }

        public List<ActiveDirectoryObject> IsMemeberOf(List<ActiveDirectoryObject> others, bool aliasIsCN = true)
        {
            ///Domain Local/Global Groups Checker
            ///Include internal forest-trusts
            ///Include direct external-trust users (via ObjectSID)
            var matchNames = new HashSet<ActiveDirectoryObject>();
            var groups = Domain.CreateDefaultSearcher()
                .SetScope(SearchScope.Subtree)
                .SetFilter(string.Format("(&(objectClass=group)(&(member:{0}:={1})(|{2})))",
                    ActiveDirectorySearcher.LDAP_MATCHING_RULE_IN_CHAIN,
                    DistinguishedName,
                    GetCNQueryString(others)))
                .Search();

            if (groups != null)
                foreach (var group in groups)
                {
                    matchNames.Add(group);
                    others.Remove(group);
                }


            ///Try special groups

            foreach (var other in others)
            {
                ///Local Groups Checker
                ///Only local groups

                if (string.Equals(Domain.RealDomainName, other.Domain.RealDomainName,
                        StringComparison.OrdinalIgnoreCase) ||
                    Domain.IsInternalTrusted(other.Domain.RealDomainName) ||
                    other.Domain.IsInternalTrusted(Domain.RealDomainName))
                    if (InInDefaultGroups(other, aliasIsCN))
                    {
                        matchNames.Add(other);
                        continue;
                    }

                //If alias is not commonname, it means from external trust domain validation
                //then do not go into IsInTrustedDomainGroup_New again.
                if (aliasIsCN && (other.Domain.IsExternalTrusted(Domain.RealDomainName) ||
                                  other.Domain.IsForestTrusted(Domain.RealDomainName)))
                    if (IsInTrustedDomainGroup(other))
                        matchNames.Add(other);
            }

            return matchNames.ToList();
        }

        public bool IsMemeberOf(ActiveDirectoryObject otherObject, bool aliasIsCN = true)
        {
            var sr = otherObject.Domain.CreateDefaultSearcher()
                .SetScope(SearchScope.Subtree)
                .SetPageSize(1)
                .SetPageSizeLimit(1)
                .SetFilter(string.Format("(&(objectClass=group)(&(member:{0}:={1})(cn={2})))",
                    ActiveDirectorySearcher.LDAP_MATCHING_RULE_IN_CHAIN,
                    DistinguishedName,
                    otherObject.CommonName))
                .SingleSearch();
            if (sr != null) return true;

            //Try Local Group or anyothers(include crossdomain) Default Group
            var isInDefaltGroups = false;
            if (string.Equals(Domain.RealDomainName, otherObject.Domain.RealDomainName,
                    StringComparison.OrdinalIgnoreCase) ||
                Domain.IsInternalTrusted(otherObject.Domain.RealDomainName) ||
                otherObject.Domain.IsInternalTrusted(Domain.RealDomainName))
                isInDefaltGroups = InInDefaultGroups(otherObject, aliasIsCN);
            if (isInDefaltGroups) return true;

            if (aliasIsCN &&
                (otherObject.Domain.IsExternalTrusted(Domain.RealDomainName) ||
                 otherObject.Domain.IsForestTrusted(Domain.RealDomainName)))
            {
                //Try external Trusted-domain
                if (IsInTrustedDomainGroup(otherObject)) return true;
                return false;
            }

            return false;
        }

        /// <summary>
        ///     Default Support nested group
        /// </summary>
        /// <param name="otherObject"></param>
        /// <returns></returns>
        private bool InInDefaultGroups(ActiveDirectoryObject otherObject, bool aliasIsCN = true)
        {
            var tokenGroups = new List<string>();
            Entry.RefreshCache(new[] {ActiveDirectoryPropertyNames.DEFAULT_GROUP});
            var pc = Entry.Properties[ActiveDirectoryPropertyNames.DEFAULT_GROUP];
            if (pc != null)
            {
                foreach (var propertyValue in pc)
                    tokenGroups.Add(new SecurityIdentifier((byte[]) propertyValue, 0).ToString());
                var result = tokenGroups.Contains(otherObject.ObjectSID) ||
                             aliasIsCN && IsDefaultGroupsMemberOf(tokenGroups, otherObject, aliasIsCN);
                if (result)
                    return result;
                return false;
            }

            return false;
        }

        private bool IsDefaultGroupsMemberOf(List<string> tokenGroups, ActiveDirectoryObject other,
            bool aliasIsCN = true)
        {
            var tokenGroupsObj = new List<ActiveDirectoryObject>();
            foreach (var sid in tokenGroups)
            {
                var sidSplits = sid.Split('-');
                if (sidSplits != null && sidSplits.Length > 0)
                {
                    var rid = sidSplits[sidSplits.Length - 1];
                    var ridNumber = int.Parse(rid);
                    //Microsoft RIDs for LOCAL, DOMAIN and BUILTIN, static 500-999
                    //Just check rid 0-1000
                    if (ridNumber > 1000) continue;
                }

                var tmp = Domain.CreateObjectBySid(sid);
                if (tmp.IsMemeberOf(other, aliasIsCN)) tokenGroupsObj.Add(tmp);
            }

            if (tokenGroupsObj != null && tokenGroupsObj.Count() > 0)
                return true;
            return false;
        }


        public override bool Equals(object obj)
        {
            if (obj is ActiveDirectoryObject)
            {
                var other = obj as ActiveDirectoryObject;
                return DistinguishedName.Equals(other.DistinguishedName);
            }

            return false;
        }


        public override int GetHashCode()
        {
            return DistinguishedName.GetHashCode();
        }

        #region ==属性==

        private string objectSID;

        public string ObjectSID
        {
            get
            {
                if (string.IsNullOrEmpty(objectSID))
                    objectSID = new SecurityIdentifier(
                        Entry.GetProperties(ActiveDirectoryPropertyNames.OBJECT_SID).First<byte[]>(), 0).ToString();
                return objectSID;
            }
        }

        private string distinguishedName;

        public string DistinguishedName
        {
            get
            {
                if (string.IsNullOrEmpty(distinguishedName))
                    distinguishedName = GetPropertySingleValue(ActiveDirectoryPropertyNames.DISTINGUISHED_NAME);
                return distinguishedName;
            }
            set { distinguishedName = value; }
        }

        private string commonName;

        public string CommonName
        {
            get
            {
                if (string.IsNullOrEmpty(commonName))
                    commonName = GetPropertySingleValue(ActiveDirectoryPropertyNames.COMMON_NAME);
                return commonName;
            }
            set { commonName = value; }
        }

        private string mail;

        public string Mail
        {
            get
            {
                if (string.IsNullOrEmpty(mail)) mail = GetPropertySingleValue(ActiveDirectoryPropertyNames.MAIL);
                return mail;
            }
            set { mail = value; }
        }

        private string domainName;

        public string DomainName
        {
            get
            {
                if (string.IsNullOrEmpty(domainName)) domainName = ActiveDirectoryDomain.GetFullDomainName(Properties);
                return domainName;
            }
            set { domainName = value; }
        }

        public string GetPropertySingleValue(string propertyName)
        {
            var result = string.Empty;
            if (Entry.Properties.Contains(propertyName))
            {
                var values = Entry.GetProperties(propertyName);
                if (values != null && values.ValueCount > 0) result = Entry.GetProperties(propertyName).First<string>();
            }

            return result;
        }

        private string samAccountName;

        public string SamAccountName
        {
            get
            {
                if (string.IsNullOrEmpty(samAccountName))
                    samAccountName = GetPropertySingleValue(ActiveDirectoryPropertyNames.SAMACCOUNTNAME);
                return samAccountName;
            }
            set { samAccountName = value; }
        }

        private string displayName;

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(displayName))
                {
                    Entry.RefreshCache(new[] {ActiveDirectoryPropertyNames.DISPLAY_NAME});
                    displayName = GetPropertySingleValue(ActiveDirectoryPropertyNames.DISPLAY_NAME);
                }

                return displayName;
            }
            set { displayName = value; }
        }

        private string department;

        public string Department
        {
            get
            {
                if (string.IsNullOrEmpty(department))
                    department = GetPropertySingleValue(ActiveDirectoryPropertyNames.DEPARTMENT);
                return department;
            }
            set { department = value; }
        }

        private bool isGroup;

        public bool IsGroup
        {
            get
            {
                if (Properties.Contains("grouptype"))
                    return true;
                return false;
            }
            set { isGroup = value; }
        }

        private bool isActived;

        public bool IsActived
        {
            get
            {
                try
                {
                    var uac = 0;
                    uac = int.Parse(Properties[ActiveDirectoryPropertyNames.USER_ACCOUNT_CONTROL][0].ToString());
                    if ((uac & 0x0002) == 0x0002)
                        isActived = false;
                    else
                        isActived = true;
                }
                catch (Exception e)
                {
                    //Cannot found that value
                    //But if current obj is a group, this value should be True
                    logger.Debug("User Account Control can not get value. Exception: {0}", e.Message);
                    isActived = IsGroup;
                }

                return isActived;
            }
        }

        private string manager = string.Empty;

        public string Manager
        {
            get
            {
                if (string.IsNullOrEmpty(this.manager))
                    try
                    {
                        var manager = Domain.CreateObject(ManagerSource);
                        this.manager = manager.MSDS_PrincipalName;
                    }
                    catch (Exception e)
                    {
                        logger.Debug("manager can not get value. Exception: {0}", e.Message);
                        return manager;
                    }

                return this.manager;
            }
            set { manager = value; }
        }

        private string upn;

        public string UPN
        {
            get
            {
                if (string.IsNullOrEmpty(upn))
                {
                    Entry.RefreshCache(new[] {ActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME});
                    var values = Entry.GetProperties(ActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME);
                    if (values != null && values.ValueCount > 0)
                        upn = values.First<string>();
                    else
                        upn = string.Empty;
                }

                return upn;
            }
            set { upn = value; }
        }

        private string msDS_PrincipalName;

        public string MSDS_PrincipalName
        {
            get
            {
                if (string.IsNullOrEmpty(msDS_PrincipalName))
                {
                    Entry.RefreshCache(new[] {ActiveDirectoryPropertyNames.MSDS_PRINCIPAL_NAME});
                    msDS_PrincipalName = Entry.GetProperties(ActiveDirectoryPropertyNames.MSDS_PRINCIPAL_NAME)
                        .First<string>();
                }

                return msDS_PrincipalName;
            }
            set { msDS_PrincipalName = value; }
        }

        private string managerSource;

        public string ManagerSource
        {
            get
            {
                if (string.IsNullOrEmpty(managerSource))
                {
                    var values = Entry.GetProperties(ActiveDirectoryPropertyNames.MANAGER);
                    if (values.ValueCount > 0)
                        managerSource = values.First<string>();
                    else
                        managerSource = string.Empty;
                }

                return managerSource;
            }
        }

        public string DomainId { get; set; }

        /// <summary>
        ///     Every Object comes from same Domain, use the same-single Checker in normally
        /// </summary>
        public ActiveDirectoryDomain interChecker;

        public ActiveDirectoryDomain Domain
        {
            get
            {
                if (interChecker == null)
                {
                    //DomainDto dto = service.CreateInstance(this.DomainId);
                    //this.checker = new ActiveDirectoryChecker(this.path, domaindto.username, domaindto.password);
                }

                return interChecker;
            }
            set { interChecker = value; }
        }

        /// <summary>
        ///     Every object has its own Entry
        /// </summary>
        private ActiveDirectoryEntry entry;

        /// <summary>
        ///     Every object has its own Entry
        /// </summary>
        public ActiveDirectoryEntry Entry
        {
            get
            {
                if (entry == null) entry = interChecker.CreateEntry(DistinguishedName);
                return entry;
            }
            set { entry = value; }
        }

        public PropertyCollection Properties => Entry.Properties;

        #endregion

        #region IN BATCH

        private string GetCNQueryString(List<ActiveDirectoryObject> others)
        {
            var builder = new StringBuilder();
            foreach (var obj in others)
                builder.AppendFormat("({0}={1})",
                    ActiveDirectoryPropertyNames.COMMON_NAME,
                    obj.CommonName);
            return builder.ToString();
        }

        private string GetMEMBERQueryString(List<ActiveDirectoryObject> memberDNs)
        {
            var builder = new StringBuilder();
            foreach (var obj in memberDNs)
                builder.AppendFormat("((member:{0}:={1}))",
                    ActiveDirectorySearcher.LDAP_MATCHING_RULE_IN_CHAIN,
                    obj.DistinguishedName);
            return builder.ToString();
        }


        private bool IsInTrustedDomainGroup(ActiveDirectoryObject otherObject)
        {
            ///External forest trust groups
            ///Only External
            //Get all groups current user in
            var src = Domain.CreateDefaultSearcher()
                .SetFilter(string.Format("(&(objectClass=group)(member:{0}:={1}))",
                    ActiveDirectorySearcher.LDAP_MATCHING_RULE_IN_CHAIN,
                    DistinguishedName))
                .Search();
            //Append current user self
            src.Add(this);
            //Get these group in external forest trust Domain DN
            var objectSIDBuilder = new StringBuilder();
            foreach (var obj in src)
                objectSIDBuilder.AppendFormat("({0}={1})", ActiveDirectoryPropertyNames.COMMON_NAME, obj.ObjectSID);
            var inForestDomainDN = otherObject.Domain.CreateDefaultSearcher()
                .SetFilter(string.Format("(&(objectClass=foreignSecurityPrincipal)(|{0}))", objectSIDBuilder))
                .Search();
            foreach (var dn in inForestDomainDN)
                if (dn.IsMemeberOf(new List<ActiveDirectoryObject> {otherObject}, false).Count != 0)
                    return true;
            return false;
        }

        #endregion
    }
}