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
using System.Diagnostics.CodeAnalysis;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Common.ActiveDirectoryWrapper
{
    public class ActiveDirectoryObject : IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(ActiveDirectoryObject));

        public ActiveDirectoryObject ToDetailViaLDAP()
        {
            this.Domain = this.Domain.ConnectGlobalCatalog();
            this.Entry = this.Domain.CreateEntry(this.DistinguishedName, "LDAP://");
            return this;
        }

        #region ==属性==

        private string objectSID = null;
        public string ObjectSID
        {
            get
            {
                if (string.IsNullOrEmpty(this.objectSID))
                {
                    this.objectSID = new SecurityIdentifier(Entry.GetProperties(ActiveDirectoryPropertyNames.OBJECT_SID).First<byte[]>(), 0).ToString();
                }
                return this.objectSID;
            }
        }

        private string distinguishedName = null;
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public string DistinguishedName
        {
            get
            {
                if (string.IsNullOrEmpty(this.distinguishedName))
                {
                    this.distinguishedName = this.GetPropertySingleValue(ActiveDirectoryPropertyNames.DISTINGUISHED_NAME);
                }
                return this.distinguishedName;
            }
            set
            {
                this.distinguishedName = value;
            }
        }

        private string commonName = null;
        public string CommonName
        {
            get
            {
                if (string.IsNullOrEmpty(this.commonName))
                {
                    this.commonName = this.GetPropertySingleValue(ActiveDirectoryPropertyNames.COMMON_NAME);
                }
                return this.commonName;
            }
            set
            {
                this.commonName = value;
            }
        }

        private string mail = null;
        public string Mail
        {
            get
            {
                if (string.IsNullOrEmpty(this.mail))
                {
                    this.mail = this.GetPropertySingleValue(ActiveDirectoryPropertyNames.MAIL);
                }
                return this.mail;
            }
            set
            {
                this.mail = value;
            }
        }

        private string domainName = null;
        public string DomainName
        {
            get
            {
                if (string.IsNullOrEmpty(this.domainName))
                {

                    this.domainName = ActiveDirectoryDomain.GetFullDomainName(this.Properties);
                }
                return this.domainName;
            }
            set
            {
                this.domainName = value;
            }
        }

        public string GetPropertySingleValue(string propertyName)
        {
            string result = string.Empty;
            if (this.Entry.Properties.Contains(propertyName))
            {
                ActiveDirectoryProperty values = this.Entry.GetProperties(propertyName);
                if (values != null && values.ValueCount > 0)
                {
                    result = this.Entry.GetProperties(propertyName).First<string>();
                }
            }
            return result;
        }

        private string samAccountName = null;
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public string SamAccountName
        {
            get
            {
                if (string.IsNullOrEmpty(this.samAccountName))
                {
                    this.samAccountName = this.GetPropertySingleValue(ActiveDirectoryPropertyNames.SAMACCOUNTNAME);
                }
                return this.samAccountName;
            }
            set
            {
                this.samAccountName = value;
            }
        }

        private string displayName = null;
        public string DisplayName
        {
            get
            {
                if (string.IsNullOrEmpty(this.displayName))
                {
                    this.Entry.RefreshCache(new string[] { ActiveDirectoryPropertyNames.DISPLAY_NAME });
                    this.displayName = this.GetPropertySingleValue(ActiveDirectoryPropertyNames.DISPLAY_NAME);

                }
                return this.displayName;
            }
            set
            {
                this.displayName = value;
            }
        }

        private string department = null;
        public string Department
        {
            get
            {
                if (string.IsNullOrEmpty(this.department))
                {

                    this.department = this.GetPropertySingleValue(ActiveDirectoryPropertyNames.DEPARTMENT);

                }
                return this.department;
            }
            set
            {
                this.department = value;
            }
        }

        private bool isGroup = false;
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Justification = "grouptype")]
        public bool IsGroup
        {
            get
            {
                if (this.Properties.Contains("grouptype"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                this.isGroup = value;
            }
        }

        private bool isActived = false;
        public bool IsActived
        {
            get
            {

                try
                {
                    int uac = 0;
                    uac = int.Parse(this.Properties[ActiveDirectoryPropertyNames.USER_ACCOUNT_CONTROL][0].ToString());
                    if ((uac & 0x0002) == 0x0002)
                    {
                        this.isActived = false;
                    }
                    else
                    {
                        this.isActived = true;
                    }
                }
                catch (Exception e)
                {
                    //Cannot found that value
                    //But if current obj is a group, this value should be True
                    logger.Debug("User Account Control can not get value. Exception: {0}", e.Message);
                    this.isActived = this.IsGroup;
                }

                return this.isActived;

            }
        }

        private string manager = string.Empty;
        public string Manager
        {
            get
            {
                if (string.IsNullOrEmpty(this.manager))
                {
                    try
                    {
                        ActiveDirectoryObject manager = this.Domain.CreateObject(this.ManagerSource);
                        this.manager = manager.MSDS_PrincipalName;
                    }
                    catch (Exception e)
                    {
                        logger.Debug("manager can not get value. Exception: {0}", e.Message);
                        return this.manager;
                    }
                }
                return this.manager;
            }
            set
            {
                this.manager = value;
            }
        }

        private string upn = null;
        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public string UPN
        {
            get
            {

                if (string.IsNullOrEmpty(this.upn))
                {
                    Entry.RefreshCache(new string[] { ActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME });
                    var values = Entry.GetProperties(ActiveDirectoryPropertyNames.USER_PRINCIPAL_NAME);
                    if (values != null && values.ValueCount > 0)
                    {
                        this.upn = values.First<string>();
                    }
                    else
                    {
                        this.upn = string.Empty;
                    }
                }
                return this.upn;
            }
            set
            {
                this.upn = value;
            }
        }

        private string msDS_PrincipalName = null;
        public string MSDS_PrincipalName
        {
            get
            {
                if (string.IsNullOrEmpty(this.msDS_PrincipalName))
                {
                    Entry.RefreshCache(new string[] { ActiveDirectoryPropertyNames.MSDS_PRINCIPAL_NAME });
                    this.msDS_PrincipalName = Entry.GetProperties(ActiveDirectoryPropertyNames.MSDS_PRINCIPAL_NAME).First<string>();
                }
                return this.msDS_PrincipalName;
            }
            set
            {
                this.msDS_PrincipalName = value;
            }
        }

        private string managerSource = null;
        public string ManagerSource
        {
            get
            {
                if (string.IsNullOrEmpty(this.managerSource))
                {
                    var values = Entry.GetProperties(ActiveDirectoryPropertyNames.MANAGER);
                    if (values.ValueCount > 0)
                    {
                        this.managerSource = values.First<string>();
                    }
                    else
                    {
                        this.managerSource = string.Empty;
                    }
                }
                return this.managerSource;
            }
        }

        public string DomainId { get; set; }

        /// <summary>
        /// Every Object comes from same Domain, use the same-single Checker in normally
        /// </summary>
        public ActiveDirectoryDomain interChecker = null;
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
            set
            {
                this.interChecker = value;
            }
        }

        /// <summary>
        /// Every object has its own Entry
        /// </summary>
        private ActiveDirectoryEntry entry = null;
        /// <summary>
        /// Every object has its own Entry
        /// </summary>
        public ActiveDirectoryEntry Entry
        {
            get
            {
                if (this.entry == null)
                {
                    this.entry = this.interChecker.CreateEntry(this.DistinguishedName);
                }
                return this.entry;
            }
            set
            {
                this.entry = value;
            }
        }

        public PropertyCollection Properties
        {
            get
            {
                return this.Entry.Properties;
            }
        }
        #endregion

        #region IN BATCH

        private string GetCNQueryString(List<ActiveDirectoryObject> others)
        {
            StringBuilder builder = new StringBuilder();
            foreach (ActiveDirectoryObject obj in others)
            {
                builder.AppendFormat("({0}={1})",
                    ActiveDirectoryPropertyNames.COMMON_NAME,
                    obj.CommonName);
            }
            return builder.ToString();
        }

        private string GetMEMBERQueryString(List<ActiveDirectoryObject> memberDNs)
        {
            StringBuilder builder = new StringBuilder();
            foreach (ActiveDirectoryObject obj in memberDNs)
            {
                builder.AppendFormat("((member:{0}:={1}))",
                    ActiveDirectorySearcher.LDAP_MATCHING_RULE_IN_CHAIN,
                    obj.DistinguishedName);
            }
            return builder.ToString();
        }

    

        private bool IsInTrustedDomainGroup(ActiveDirectoryObject otherObject)
        {
            ///External forest trust groups
            ///Only External
            //Get all groups current user in
            List<ActiveDirectoryObject> src = this.Domain.CreateDefaultSearcher()
               .SetFilter(string.Format("(&(objectClass=group)(member:{0}:={1}))",
                           ActiveDirectorySearcher.LDAP_MATCHING_RULE_IN_CHAIN,
                           this.DistinguishedName))
               .Search();
            //Append current user self
            src.Add(this);
            //Get these group in external forest trust Domain DN
            StringBuilder objectSIDBuilder = new StringBuilder();
            foreach (ActiveDirectoryObject obj in src)
            {
                objectSIDBuilder.AppendFormat("({0}={1})", ActiveDirectoryPropertyNames.COMMON_NAME, obj.ObjectSID);
            }
            List<ActiveDirectoryObject> inForestDomainDN = otherObject.Domain.CreateDefaultSearcher()
                .SetFilter(string.Format("(&(objectClass=foreignSecurityPrincipal)(|{0}))", objectSIDBuilder.ToString()))
                .Search();
            foreach (ActiveDirectoryObject dn in inForestDomainDN)
            {
                if (dn.IsMemeberOf(new List<ActiveDirectoryObject>() { otherObject }, false).Count != 0)
                {
                    return true;
                }
            }
            return false;
        }

        #endregion

        public List<ActiveDirectoryObject> IsMemeberOf(List<ActiveDirectoryObject> others, bool aliasIsCN = true)
        {
            ///Domain Local/Global Groups Checker
            ///Include internal forest-trusts
            ///Include direct external-trust users (via ObjectSID)
            HashSet<ActiveDirectoryObject> matchNames = new HashSet<ActiveDirectoryObject>();
            List<ActiveDirectoryObject> groups = this.Domain.CreateDefaultSearcher()
                .SetScope(SearchScope.Subtree)
                .SetFilter(string.Format("(&(objectClass=group)(&(member:{0}:={1})(|{2})))",
                        ActiveDirectorySearcher.LDAP_MATCHING_RULE_IN_CHAIN,
                        this.DistinguishedName,
                        GetCNQueryString(others)))
                .Search();

            if (groups != null)
            {
                foreach (ActiveDirectoryObject group in groups)
                {
                    matchNames.Add(group);
                    others.Remove(group);
                }
            }

            

            ///Try special groups

            foreach (ActiveDirectoryObject other in others)
            {
                ///Local Groups Checker
                ///Only local groups

                if (string.Equals(this.Domain.RealDomainName, other.Domain.RealDomainName, StringComparison.OrdinalIgnoreCase) || 
                    this.Domain.IsInternalTrusted(other.Domain.RealDomainName) ||
                    other.Domain.IsInternalTrusted(this.Domain.RealDomainName))
                {
                    if (InInDefaultGroups(other, aliasIsCN))
                    {
                        matchNames.Add(other);
                        continue;
                    }
                }

                //If alias is not commonname, it means from external trust domain validation
                //then do not go into IsInTrustedDomainGroup_New again.
                if (aliasIsCN &&( other.Domain.IsExternalTrusted(this.Domain.RealDomainName)||other.Domain.IsForestTrusted(this.Domain.RealDomainName)))
                {
                    ///Try External Trust Domains
                    if (IsInTrustedDomainGroup(other))
                    {
                        matchNames.Add(other);
                    }
                }
            }
            return matchNames.ToList();
        }

        public bool IsMemeberOf(ActiveDirectoryObject otherObject, bool aliasIsCN = true)
        {
            ActiveDirectoryObject sr = otherObject.Domain.CreateDefaultSearcher()
                .SetScope(SearchScope.Subtree)
                .SetPageSize(1)
                .SetPageSizeLimit(1)
                .SetFilter(string.Format("(&(objectClass=group)(&(member:{0}:={1})(cn={2})))",
                        ActiveDirectorySearcher.LDAP_MATCHING_RULE_IN_CHAIN,
                        this.DistinguishedName,
                        otherObject.CommonName))
                .SingleSearch();
            if (sr != null)
            {
                return true;
            }
            else
            {
                //Try Local Group or anyothers(include crossdomain) Default Group
                bool isInDefaltGroups = false;
                if (string.Equals(this.Domain.RealDomainName, otherObject.Domain.RealDomainName, StringComparison.OrdinalIgnoreCase)||
                    this.Domain.IsInternalTrusted(otherObject.Domain.RealDomainName) ||
                    otherObject.Domain.IsInternalTrusted(this.Domain.RealDomainName))
                {
                    isInDefaltGroups = InInDefaultGroups(otherObject, aliasIsCN);
                }
                if (isInDefaltGroups)
                {
                    return true;
                }
                else if (aliasIsCN &&
                    (otherObject.Domain.IsExternalTrusted(this.Domain.RealDomainName)||otherObject.Domain.IsForestTrusted(this.Domain.RealDomainName)))
                {
                    //Try external Trusted-domain
                    if (IsInTrustedDomainGroup(otherObject))
                    {
                        return true;
                    }
                    return false;
                }
              /*  else if (WrapperConfiguration.MasForceCheckWithAPI)
                {
                    if (IsInTrustedDomainGroup(otherObject))
                    {
                        return true;
                    }
                    return false;
                }*/
                else
                {
                    return false;
                }
            }
        }

        /// <summary>
        /// Default Support nested group
        /// </summary>
        /// <param name="otherObject"></param>
        /// <returns></returns>
        private bool InInDefaultGroups(ActiveDirectoryObject otherObject, bool aliasIsCN = true)
        {
            List<string> tokenGroups = new List<string>();
            this.Entry.RefreshCache(new string[] { ActiveDirectoryPropertyNames.DEFAULT_GROUP });
            PropertyValueCollection pc = this.Entry.Properties[ActiveDirectoryPropertyNames.DEFAULT_GROUP];
            if (pc != null)
            {
                foreach (object propertyValue in pc)
                {
                    tokenGroups.Add(new SecurityIdentifier((byte[])propertyValue, 0).ToString());
                }
                bool result = tokenGroups.Contains(otherObject.ObjectSID) || IsDefaultGroupsMemberOf(tokenGroups, otherObject, aliasIsCN);
                if (result)
                {
                    return result;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        private bool IsDefaultGroupsMemberOf(List<string> tokenGroups, ActiveDirectoryObject other, bool aliasIsCN = true) 
        {
            List<ActiveDirectoryObject> tokenGroupsObj = new List<ActiveDirectoryObject>();
            foreach(string sid in tokenGroups)
            {
                string[] sidSplits = sid.Split('-');
                if(sidSplits!=null && sidSplits.Length>0)
                {
                    string rid = sidSplits[sidSplits.Length-1];
                    int ridNumber = int.Parse(rid);
                    //Microsoft RIDs for LOCAL, DOMAIN and BUILTIN, static 500-999
                    //Just check rid 0-1000
                    if (ridNumber > 1000) 
                    {
                        continue;
                    }
                }
                ActiveDirectoryObject tmp = this.Domain.CreateObjectBySid(sid);
                if (tmp.IsMemeberOf(other, aliasIsCN))
                {
                    tokenGroupsObj.Add(tmp);
                }
            }
           
            if (tokenGroupsObj != null && tokenGroupsObj.Count() > 0)
            {
                return true;
            }
            else 
            {
                return false;
            }
        }


        public override bool Equals(object obj)
        {
            if (obj is ActiveDirectoryObject)
            {
                ActiveDirectoryObject other = obj as ActiveDirectoryObject;
                return this.DistinguishedName.Equals(other.DistinguishedName);
            }
            else
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return this.DistinguishedName.GetHashCode();
        }

        public void Dispose()
        {
            if (this.Entry != null)
            {
                this.Entry.Dispose();
            }
        }
    }
}
