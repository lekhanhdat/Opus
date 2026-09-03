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
using AvePoint.GCommon;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.Common.ActiveDirectoryWrapper
{
    public class ActiveDirectorySearcher :IDisposable
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(ActiveDirectorySearcher));
        public const string LDAP_MATCHING_RULE_BIT_AND = "1.2.840.113556.1.4.803";
        public const string LDAP_MATCHING_RULE_BIT_OR = "1.2.840.113556.1.4.804";
        public const string LDAP_MATCHING_RULE_IN_CHAIN = "1.2.840.113556.1.4.1941";
        public int PageSize = 200;
        public int SizeLimit = 0;
        public string[] PropertiesToLoad { get; set; }
        public string Filter { get; set; }       
        public SearchScope Scope = SearchScope.Subtree;
        public DirectorySearcher Searcher { get; set; }
        public ActiveDirectoryDomain Checker { get; set; }
        public bool UseLDAP { get; set; } 

        public ActiveDirectorySearcher(ActiveDirectoryDomain checker, bool useLDAP =false) 
        {
            this.Checker = checker;
            this.UseLDAP = useLDAP;
            if (!UseLDAP)
            {
                this.Searcher = new DirectorySearcher(checker.ConnectGlobalCatalog().ConnectLDAP().Entry);
            }
            else 
            {
                this.Searcher = new DirectorySearcher(checker.ConnectGlobalCatalog().ConnectLDAP().EntryForExtend);
            }
        }

        /// <summary>
        /// Re-create properties array to load
        /// </summary>
        /// <param name="propertiesToLoad"></param>
        /// <returns></returns>
        public ActiveDirectorySearcher ToLoad(params string[] propertiesToLoad)
        {
            this.PropertiesToLoad = propertiesToLoad;
            return this;
        }

        /// <summary>
        /// Load more properties after ToLoad() method
        /// </summary>
        /// <param name="propertiesToLoad"></param>
        /// <returns></returns>
        public ActiveDirectorySearcher LoadMore(params string[] propertiesToLoad) 
        {
            if (this.PropertiesToLoad != null)
            {
                string[] newPropertiesToLoad = new string[this.PropertiesToLoad.Length + propertiesToLoad.Length];
                Array.Copy(propertiesToLoad, 0, newPropertiesToLoad, 0, propertiesToLoad.Length);
                Array.Copy(this.PropertiesToLoad, 0, newPropertiesToLoad, propertiesToLoad.Length, this.PropertiesToLoad.Length);
                this.PropertiesToLoad = newPropertiesToLoad;
                return this;
            }
            else 
            {
                this.ToLoad(propertiesToLoad);
                return this;
            }
        }

        /// <summary>
        /// Set page size
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetPageSize(int pageSize) 
        {
            this.PageSize = pageSize;
            return this;
        }

        /// <summary>
        /// Set Size limit per page
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetPageSizeLimit(int limit) 
        {
            this.SizeLimit = limit;
            return this;
        }

        /// <summary>
        /// Prepare a Scope to search, if you don't set it,the default value is SearchScope.SubTree
        /// </summary>
        /// <param name="scope">Scope</param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetScope(SearchScope scope) 
        {
            this.Scope = scope;
            return this;
        }

        /// <summary>
        /// Prepare a filter to search.
        /// </summary>
        /// <param name="filter">Filter string. 
        /// For example to search a people named someone: "(&(objectClass=user)(objectCategory=person)(cn=someone))"
        /// You can get more Filter from ADSI EDITOR.</param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetFilter(string filter) 
        {
            this.Filter = filter;
            return this;
        }

        /// <summary>
        /// Search an user by its common name or login name 
        /// </summary>
        /// <param name="name">common name or login name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchUser(params string[] name)
        {
            this.SetFilter(string.Format("(&{0}(|{1}))",this.UserMask, MakeupSingleSearchParamters(name)));
            return this.SingleSearch();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        private string MakeupSingleSearchParamters(params string[] names) 
        {
            StringBuilder builder = new StringBuilder();
            if (names != null) 
            {
                foreach (string n in names) 
                {
                    builder.AppendFormat("(cn={0})(samaccountname={0})", n);
                }
            }

            return builder.ToString();
        }

        /// <summary>
        /// Search a group by its common name or login name 
        /// </summary>
        /// <param name="name">common name or login name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchGroup(params string[]  name)
        {
            this.SetFilter(string.Format("(&{0}(|{1}))",this.GroupMask, MakeupSingleSearchParamters(name)));
            return this.SingleSearch();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public ActiveDirectoryObject SingleSearch(string name)
        {
            this.SetFilter(string.Format("(&(|(&{2})(&{1}))(|(cn={0})(samaccountname={0})))",  name, this.UserMask, this.GroupMask));
            return this.SingleSearch();
        }

        /// <summary>
        /// Load an object by its ObjectSID
        /// </summary>
        /// <param name="sid">ObjectSID</param>
        /// <returns></returns>
        public ActiveDirectoryObject LoadByObjectSid(string sid) 
        {
            this.SetFilter(string.Format("(&(objectSID={0}))", sid));
            return this.SingleSearch();
        }

        /// <summary>
        /// Search a user by its Common name
        /// </summary>
        /// <param name="cn">common name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchUserByCommonName(string cn) 
        {
            this.SetFilter(string.Format("(&{0}(cn={1}))",this.UserMask, cn));
            return this.SingleSearch();
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        /// <summary>
        /// Search a user by its SAMACCOUNTNAME
        /// </summary>
        /// <param name="cn">common name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchUserByLoginName(string loginName)
        {
            this.SetFilter(string.Format("(&{0}({2}={1}))", this.UserMask, ActiveDirectoryPropertyNames.SAMACCOUNTNAME));
            return this.SingleSearch();
        }

        /// <summary>
        /// Search a group by its common name
        /// </summary>
        /// <param name="cn">common name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchGroupByCommonName(string cn)
        {
            this.SetFilter(string.Format("(&{0}(cn={1}))",this.GroupMask, cn));
            return this.SingleSearch();
        }
       
        /// <summary>
        /// Set Search Filter.
        /// </summary>
        /// <param name="type">User or Group</param>
        /// <param name="value">Search Value</param>
        /// <param name="propertiesNames">Select attributes to be matched.</param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetFilter(ActiveDirectoryObjectType type, string value, params string[] propertiesNames) 
        {
            string finalFormat = "(&{0}(|{1}))";
            if (propertiesNames == null) 
            {
                throw new Exception("At least one property name is needed.");
            }
            string conditions = string.Join("", propertiesNames.Select(a => { return string.Format("({0}={1})", a, value); }).ToArray());
            this.Filter = string.Format(finalFormat, type == ActiveDirectoryObjectType.User ? this.UserMask: this.GroupMask, conditions);
            if (this.PropertiesToLoad != null)
            {
                this.LoadMore(propertiesNames);
            }
            else 
            {
                this.ToLoad(propertiesNames);
            }
            return this;
        }

        /// <summary>
        /// Search an single result base the FilterString set in SetFilter() method
        /// </summary>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearch()
        {
            
            this.BuildArguments();
            this.Searcher.PageSize = 1;
            this.Searcher.SizeLimit = 1;
            //logger.Debug("Searching in Global Catalog: {0}. Filter: {1}",this.Checker.Entry.Name, this.Filter);

            SearchResult sr = this.Searcher.FindOne();
            if (sr == null) 
            {
                return null;
            }
            if (!UseLDAP)
            {
                return this.Checker.CreateObject(sr);
            }
            else
            {
                return this.Checker.CreateObject(sr, "LDAP://");
            }
        }

        /// <summary>
        /// Search Objects in LDAP/GC base the FilterString set in SetFilter method
        /// </summary>
        /// <returns></returns>
        public List<ActiveDirectoryObject> Search()
        {
            this.BuildArguments();
            this.Searcher.PageSize = this.PageSize;
            this.Searcher.SizeLimit = this.SizeLimit;
            List<ActiveDirectoryObject> results = new List<ActiveDirectoryObject>();
            logger.Debug("Searching in Global Catalog: {0}. Filter: {1}", this.Checker.Entry.Name, this.Filter);
            SearchResultCollection searchResults = this.Searcher.FindAll();
            foreach (SearchResult r in searchResults)
            {
                results.Add(this.Checker.CreateObject(r));
            }
            return results;
        }

        /// <summary>
        /// Short proxy of WildcardYieldSearch
        /// </summary>
        /// <param name="anyValue"></param>
        /// <returns></returns>
        public SearchResultCollection Find(params string[] anyvalues)
        {

            return WildcardYieldSearch(anyvalues);
        }

        /// <summary>
        /// Search Objects in LDAP/GC, you can find any thing like 'anyvalue'
        /// Yield: The results will return in more than one time
        /// </summary>
        /// <param name="anyvalue">Any attribute value</param>
        /// <returns></returns>
        public SearchResultCollection WildcardYieldSearch(params string[] anyvalues)
        {
            this.SetFilter((string.Format("(&(|(&{1})(&{2}))(|{0}))",MakeupFilter(anyvalues), this.UserMask, this.GroupMask)));
            return YieldSearch();
        }

        /// <summary>
        /// Search Objects in LDAP/GC, you can find any thing like 'anyvalue'
        /// </summary>
        /// <param name="anyvalue">Any attribute value</param>
        /// <returns></returns>
        public List<ActiveDirectoryObject> WildcardSearch(params string[] anyvalues)
        {
            SearchResultCollection results = WildcardYieldSearch(anyvalues);
            List<ActiveDirectoryObject> realResult = new List<ActiveDirectoryObject>();
            if (results != null) 
            {
                foreach (SearchResult sr in results) 
                {
                    realResult.Add(this.Checker.CreateObject(sr));
                }
            }
            return realResult;
        }

        private string MakeupFilter(params string[] anyvalues) 
        {
            StringBuilder builder = new StringBuilder();
            foreach (string anyValue in anyvalues)
            {
                foreach (string attr in ActiveDirectoryDomain.SearchObject_SupportedAttributes)
                {
                    builder.AppendFormat("({0}={1}*)", attr, anyValue);
                }
            }
            return builder.ToString();
        }

        /// <summary>
        /// Search Objects in LDAP/GC base the FilterString set in SetFilter method
        /// Yield: The results will return in more than one time
        /// </summary>
        /// <returns></returns>
        public SearchResultCollection YieldSearch()
        {
            this.BuildArguments();
            this.Searcher.PageSize = this.PageSize;
            this.Searcher.SizeLimit = this.SizeLimit;
            logger.Debug("Searching in Global Catalog: {0}. Filter: {1}", this.Checker.Entry.Name, this.Filter);
            return this.Searcher.FindAll();
        }

        public void Dispose()
        {
            this.Searcher.Dispose();
        }

        /// <summary>
        /// Build all arguments .
        /// </summary>
        private void BuildArguments() 
        {
            this.Searcher.Filter = this.Filter;
            this.Searcher.SearchScope = this.Scope;
            if (this.PropertiesToLoad != null)
            {
                this.Searcher.PropertiesToLoad.AddRange(this.PropertiesToLoad);
            }
            this.Searcher.PageSize = this.PageSize;
            this.Searcher.SizeLimit = this.SizeLimit;
        }

        private string CreateMask(string objectClass, string objectCategory)
        {
            return string.Format("(objectClass={0})(objectCategory={1})", objectClass, objectCategory);
        }

        private string GroupMask
        {
            get
            {
                return this.CreateMask(ObjectClasses.GROUP, ObjectCategories.GROUP);
            }
        }

        private string UserMask
        {
            get
            {
                return this.CreateMask(ObjectClasses.USER, ObjectCategories.PERSON);
            }
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public List<ActiveDirectoryObject> GetGroupMembers(string group)
        {
            string searchString = string.Format("(&(&(objectCategory=group)(objectClass=group)(|({0}={1})({2}={1}))))", "CN", group, "samaccountname");
            this.SetFilter(searchString);
            List<ActiveDirectoryObject> members = new List<ActiveDirectoryObject>();
            try
            {

                uint rangeStep = 1000;
                uint rangeLow = 0;
                uint rangeHigh = rangeLow + (rangeStep - 1);
                bool lastQuery = false;
                bool quitLoop = false;

                do
                {
                    string attributeWithRange;
                    if (!lastQuery)
                    {
                        attributeWithRange = String.Format("member;range={0}-{1}", rangeLow, rangeHigh);
                    }
                    else
                    {
                        attributeWithRange = String.Format("member;range={0}-*", rangeLow);
                    }
                    this.PropertiesToLoad = new string[] { attributeWithRange, ActiveDirectoryPropertyNames.DISTINGUISHED_NAME };
                    this.BuildArguments();
                    SearchResult results = this.Searcher.FindOne();
                    //ActiveDirectoryObject results = this.SingleSearch();
                    if (results.Properties.Contains(attributeWithRange))
                    {
                        foreach (object obj in results.Properties[attributeWithRange])
                        {
                            //Console.WriteLine(obj.GetType());
                            if (obj.GetType().Equals(typeof(System.String)))
                            {
                                ActiveDirectoryObject member = this.Checker.CreateObject(obj.ToString());
                                members.Add(member);
                            }
                            else if (obj.GetType().Equals(typeof(System.Int32)))
                            {
                            }
                            // Console.WriteLine(obj.ToString());
                        }
                        if (lastQuery)
                        {
                            quitLoop = true;
                        }
                    }
                    else
                    {
                        lastQuery = true;
                    }
                    if (!lastQuery)
                    {
                        rangeLow = rangeHigh + 1;
                        rangeHigh = rangeLow + (rangeStep - 1);
                    }
                }
                while (!quitLoop);
            }
            catch (Exception ex)
            {
                logger.Warn("GetGroupMembers.Error:{0}", ex.ToString());
            }
            return members;
        }

        [SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues")]
        public List<ActiveDirectoryObject> GetUserMemberOf(string userDistingishedName)
        {
            string searchString = string.Format("(&(objectCategory=person)(objectClass=user)({0}={1}))", "distinguishedname", userDistingishedName);
            this.SetFilter(searchString);
            List<ActiveDirectoryObject> memberof = new List<ActiveDirectoryObject>();
            this.BuildArguments();
            SearchResult results = this.Searcher.FindOne();
            foreach (object obj in results.Properties["memberof"])
            {
                if (obj.GetType().Equals(typeof(System.String)))
                {
                    try
                    {
                        ActiveDirectoryObject member = this.Checker.CreateObject(obj.ToString());
                        memberof.Add(member);
                    }
                    catch(Exception e)
                    {
                        logger.Warn("Get a parent node failed. Parent: {0}, children: {1}, Error: {2}", obj, userDistingishedName, e);
                    }
                }
            }
            return memberof;
        }
    }
}
