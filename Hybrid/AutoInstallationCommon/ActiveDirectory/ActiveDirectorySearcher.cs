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
using System.Collections;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;

namespace AutoInstallationCommon.ActiveDirectory
{
    public class ActiveDirectorySearcher : IDisposable
    {
        public const string LDAP_MATCHING_RULE_BIT_AND = "1.2.840.113556.1.4.803";
        public const string LDAP_MATCHING_RULE_BIT_OR = "1.2.840.113556.1.4.804";
        public const string LDAP_MATCHING_RULE_IN_CHAIN = "1.2.840.113556.1.4.1941";
        public int PageSize = 200;
        public SearchScope Scope = SearchScope.Subtree;
        public int SizeLimit;

        public ActiveDirectorySearcher(ActiveDirectoryDomain checker, bool useLDAP = false)
        {
            Checker = checker;
            UseLDAP = useLDAP;
            if (!UseLDAP)
                Searcher = new DirectorySearcher(checker.ConnectGlobalCatalog().ConnectLDAP().Entry);
            else
                Searcher = new DirectorySearcher(checker.ConnectGlobalCatalog().ConnectLDAP().EntryForExtend);
        }

        public string[] PropertiesToLoad { get; set; }
        public string Filter { get; set; }
        public DirectorySearcher Searcher { get; set; }
        public ActiveDirectoryDomain Checker { get; set; }
        public bool UseLDAP { get; set; }
        public string BaseFilter { get; set; }

        private string GroupMask => CreateMask(ObjectClasses.GROUP, ObjectCategories.GROUP);

        private string UserMask => CreateMask(ObjectClasses.USER, ObjectCategories.PERSON);

        public void Dispose()
        {
            Searcher.Dispose();
        }

        /// <summary>
        ///     Re-create properties array to load
        /// </summary>
        /// <param name="propertiesToLoad"></param>
        /// <returns></returns>
        public ActiveDirectorySearcher ToLoad(params string[] propertiesToLoad)
        {
            PropertiesToLoad = propertiesToLoad;
            return this;
        }

        /// <summary>
        ///     Load more properties after ToLoad() method
        /// </summary>
        /// <param name="propertiesToLoad"></param>
        /// <returns></returns>
        public ActiveDirectorySearcher LoadMore(params string[] propertiesToLoad)
        {
            if (PropertiesToLoad != null)
            {
                var newPropertiesToLoad = new string[PropertiesToLoad.Length + propertiesToLoad.Length];
                Array.Copy(propertiesToLoad, 0, newPropertiesToLoad, 0, propertiesToLoad.Length);
                Array.Copy(PropertiesToLoad, 0, newPropertiesToLoad, propertiesToLoad.Length, PropertiesToLoad.Length);
                PropertiesToLoad = newPropertiesToLoad;
                return this;
            }

            ToLoad(propertiesToLoad);
            return this;
        }

        /// <summary>
        ///     Set page size
        /// </summary>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetPageSize(int pageSize)
        {
            PageSize = pageSize;
            return this;
        }

        /// <summary>
        ///     Set Size limit per page
        /// </summary>
        /// <param name="limit"></param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetPageSizeLimit(int limit)
        {
            SizeLimit = limit;
            return this;
        }

        /// <summary>
        ///     Prepare a Scope to search, if you don't set it,the default value is SearchScope.SubTree
        /// </summary>
        /// <param name="scope">Scope</param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetScope(SearchScope scope)
        {
            Scope = scope;
            return this;
        }

        public List<ActiveDirectoryObject> ExactSearch(params string[] anyvalues)
        {
            SetFilter(string.Format("(&(|(&{1})(&{2}))(|{0}))", MakeupFilterWithoutWildcard(anyvalues), UserMask,
                GroupMask));
            var results = YieldSearch();
            var realResult = new List<ActiveDirectoryObject>();
            if (results != null)
                foreach (SearchResult sr in results)
                    realResult.Add(Checker.CreateObject(sr));
            return realResult;
        }

        private string MakeupFilterWithoutWildcard(params string[] anyvalues)
        {
            var builder = new StringBuilder();
            foreach (var anyValue in anyvalues)
            foreach (var attr in ActiveDirectoryDomain.SearchObject_SupportedAttributes)
                builder.AppendFormat("({0}={1})", attr, anyValue);
            return builder.ToString();
        }

        /// <summary>
        ///     Prepare a filter to search.
        /// </summary>
        /// <param name="filter">
        ///     Filter string.
        ///     For example to search a people named someone: "(&(objectClass=user)(objectCategory=person)(cn=someone))"
        ///     You can get more Filter from ADSI EDITOR.
        /// </param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetFilter(string filter)
        {
            Filter = filter;
            return this;
        }

        /// <summary>
        ///     Search an user by its common name or login name
        /// </summary>
        /// <param name="name">common name or login name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchUser(params string[] name)
        {
            SetFilter(string.Format("(&{0}(|{1}){2})", UserMask, MakeupSingleSearchParamters(name), BaseFilter));
            return SingleSearch();
        }

        private string MakeupSingleSearchParamters(params string[] names)
        {
            var builder = new StringBuilder();
            if (names != null)
                foreach (var n in names)
                    builder.AppendFormat("(cn={0})(samaccountname={0})", n);

            return builder.ToString();
        }

        /// <summary>
        ///     Search a group by its common name or login name
        /// </summary>
        /// <param name="name">common name or login name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchGroup(params string[] name)
        {
            SetFilter(string.Format("(&{0}(|{1}){2})", GroupMask, MakeupSingleSearchParamters(name), BaseFilter));
            return SingleSearch();
        }

        public ActiveDirectoryObject SingleSearch(string name)
        {
            SetFilter(string.Format("(&(|(&{2})(&{1}))(|(cn={0})(samaccountname={0})){3})", name, UserMask, GroupMask,
                BaseFilter));
            return SingleSearch();
        }

        /// <summary>
        ///     Load an object by its ObjectSID
        /// </summary>
        /// <param name="sid">ObjectSID</param>
        /// <returns></returns>
        public ActiveDirectoryObject LoadByObjectSid(string sid)
        {
            SetFilter(string.Format("(&(objectSID={0}){1})", sid, BaseFilter));
            return SingleSearch();
        }

        /// <summary>
        ///     Search a user by its Common name
        /// </summary>
        /// <param name="cn">common name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchUserByCommonName(string cn)
        {
            SetFilter(string.Format("(&{0}(cn={1}){2})", UserMask, cn, BaseFilter));
            return SingleSearch();
        }

        /// <summary>
        ///     Search a user by its SAMACCOUNTNAME
        /// </summary>
        /// <param name="cn">common name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchUserByLoginName(string loginName)
        {
            SetFilter(string.Format("(&{0}({1}={2}){3})", UserMask, ActiveDirectoryPropertyNames.SAMACCOUNTNAME,
                loginName, BaseFilter));
            return SingleSearch();
        }

        /// <summary>
        ///     Search a group by its common name
        /// </summary>
        /// <param name="cn">common name</param>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearchGroupByCommonName(string cn)
        {
            SetFilter(string.Format("(&{0}(cn={1})(2))", GroupMask, cn, BaseFilter));
            return SingleSearch();
        }

        /// <summary>
        ///     Set Search Filter.
        /// </summary>
        /// <param name="type">User or Group</param>
        /// <param name="value">Search Value</param>
        /// <param name="propertiesNames">Select attributes to be matched.</param>
        /// <returns></returns>
        public ActiveDirectorySearcher SetFilter(ActiveDirectoryObjectType type, string value,
            params string[] propertiesNames)
        {
            var finalFormat = "(&{0}(|{1}))";
            if (propertiesNames == null) throw new Exception("At least one property name is needed.");
            var conditions = string.Join("",
                propertiesNames.Select(a => { return string.Format("({0}={1})", a, value); }).ToArray());
            Filter = string.Format(finalFormat, type == ActiveDirectoryObjectType.User ? UserMask : GroupMask,
                conditions);
            if (PropertiesToLoad != null)
                LoadMore(propertiesNames);
            else
                ToLoad(propertiesNames);
            return this;
        }

        /// <summary>
        ///     Search an single result base the FilterString set in SetFilter() method
        /// </summary>
        /// <returns></returns>
        public ActiveDirectoryObject SingleSearch()
        {
            BuildArguments();
            Searcher.PageSize = 1;
            Searcher.SizeLimit = 1;

            var sr = Searcher.FindOne();
            if (sr == null) return null;
            if (!UseLDAP)
                return Checker.CreateObject(sr);
            return Checker.CreateObject(sr, "LDAP://");
        }

        /// <summary>
        ///     Search Objects in LDAP/GC base the FilterString set in SetFilter method
        /// </summary>
        /// <returns></returns>
        public List<ActiveDirectoryObject> Search()
        {
            BuildArguments();
            Searcher.PageSize = PageSize;
            Searcher.SizeLimit = SizeLimit;
            var results = new List<ActiveDirectoryObject>();
            var searchResults = Searcher.FindAll();
            foreach (SearchResult r in searchResults)
                if (!UseLDAP)
                    results.Add(Checker.CreateObject(r));
                else
                    results.Add(Checker.CreateObject(r, "LDAP://"));
            return results;
        }

        public List<ActiveDirectoryObject> Search(int index, int count)
        {
            BuildArguments();
            Searcher.PageSize = PageSize;
            Searcher.SizeLimit = SizeLimit;
            var results = new List<ActiveDirectoryObject>();
            var searchResults = Searcher.FindAll();
            var skipCount = -1;
            var skipper = searchResults.GetEnumerator();
            while (true)
            {
                skipCount++;
                var hasNext = skipper.MoveNext();
                if (skipCount == index && hasNext) break;
                if (!hasNext) return null;
            }

            var currentCount = 0;
            while (true)
            {
                currentCount++;
                if (currentCount > count) break;

                var sr = (SearchResult) skipper.Current;
                if (!UseLDAP)
                    results.Add(Checker.CreateObject(sr));
                else
                    results.Add(Checker.CreateObject(sr, "LDAP://"));
            }

            return results;
        }

        /// <summary>
        ///     Short proxy of WildcardYieldSearch
        /// </summary>
        /// <param name="anyValue"></param>
        /// <returns></returns>
        public SearchResultCollection Find(params string[] anyvalues)
        {
            return WildcardYieldSearch(anyvalues);
        }

        /// <summary>
        ///     Search Objects in LDAP/GC, you can find any thing like 'anyvalue'
        ///     Yield: The results will return in more than one time
        /// </summary>
        /// <param name="anyvalue">Any attribute value</param>
        /// <returns></returns>
        public SearchResultCollection WildcardYieldSearch(params string[] anyvalues)
        {
            SetFilter(string.Format("(&(|(&{1})(&{2}))(|{0}){3})", MakeupFilterSingleReturnVlaue(anyvalues), UserMask,
                GroupMask, BaseFilter));
            return YieldSearch();
        }

        public IEnumerator WildcardYieldSearch(int index, params string[] anyvalues)
        {
            SetFilter(string.Format("(&(|(&{1})(&{2}))(|{0}){3})", MakeupFilterSingleReturnVlaue(anyvalues), UserMask,
                GroupMask, BaseFilter));
            return YieldSearch(index);
        }

        /// <summary>
        ///     Search Objects in LDAP/GC, you can find any thing like 'anyvalue'
        /// </summary>
        /// <param name="anyvalue">Any attribute value</param>
        /// <returns></returns>
        public List<ActiveDirectoryObject> WildcardSearch(params string[] anyvalues)
        {
            var results = WildcardYieldSearch(anyvalues);
            var realResult = new List<ActiveDirectoryObject>();
            if (results != null)
                foreach (SearchResult sr in results)
                    if (!UseLDAP)
                        realResult.Add(Checker.CreateObject(sr));
                    else
                        realResult.Add(Checker.CreateObject(sr, "LDAP://"));
            return realResult;
        }

        public List<ActiveDirectoryObject> WildcardSearch(int index, int count, params string[] anyvalues)
        {
            var results = WildcardYieldSearch(index, anyvalues);
            var realResult = new List<ActiveDirectoryObject>();
            if (results != null)
            {
                var currentCount = 0;
                while (true)
                {
                    currentCount++;
                    if (currentCount > count) break;

                    var sr = (SearchResult) results.Current;
                    if (!UseLDAP)
                        realResult.Add(Checker.CreateObject(sr));
                    else
                        realResult.Add(Checker.CreateObject(sr, "LDAP://"));
                }
            }

            return realResult;
        }

        private string MakeupFilterMultipleReturnVlaue(params string[] anyvalues)
        {
            var builder = new StringBuilder();
            foreach (var anyValue in anyvalues)
            foreach (var attr in ActiveDirectoryDomain.SearchObject_SupportedAttributes)
                builder.AppendFormat("({0}={1}*)", attr, anyValue);
            return builder.ToString();
        }

        private string MakeupFilterSingleReturnVlaue(params string[] anyvalues)
        {
            var builder = new StringBuilder();
            foreach (var anyValue in anyvalues)
            foreach (var attr in ActiveDirectoryDomain.SearchObject_SupportedAttributes)
                builder.AppendFormat("({0}={1})", attr, anyValue);
            return builder.ToString();
        }

        /// <summary>
        ///     Search Objects in LDAP/GC base the FilterString set in SetFilter method
        ///     Yield: The results will return in more than one time
        /// </summary>
        /// <returns></returns>
        public SearchResultCollection YieldSearch()
        {
            BuildArguments();
            Searcher.PageSize = PageSize;
            Searcher.SizeLimit = SizeLimit;
            return Searcher.FindAll();
        }

        /// <summary>
        ///     Search Objects in LDAP/GC base the FilterString set in SetFilter method
        ///     Yield: The results will return in more than one time
        /// </summary>
        /// <returns></returns>
        public IEnumerator YieldSearch(int index)
        {
            BuildArguments();
            Searcher.PageSize = PageSize;
            Searcher.SizeLimit = SizeLimit;
            var collection = Searcher.FindAll();
            var skipCount = -1;
            var skipper = collection.GetEnumerator();
            while (true)
            {
                skipCount++;
                var hasNext = skipper.MoveNext();
                if (skipCount == index && hasNext) break;
                if (!hasNext) return null;
            }

            return skipper;
        }

        /// <summary>
        ///     Build all arguments .
        /// </summary>
        private void BuildArguments()
        {
            Searcher.Filter = Filter;
            Searcher.SearchScope = Scope;
            if (PropertiesToLoad != null) Searcher.PropertiesToLoad.AddRange(PropertiesToLoad);
            Searcher.PageSize = PageSize;
            Searcher.SizeLimit = SizeLimit;
        }

        private string CreateMask(string objectClass, string objectCategory)
        {
            return string.Format("(objectClass={0})(objectCategory={1})", objectClass, objectCategory);
        }

        public List<ActiveDirectoryObject> GetGroupMembers(string group)
        {
            var searchString = string.Format("(&(&(objectCategory=group)(objectClass=group)(|({0}={1})({2}={1}))){3})",
                "CN", group, "samaccountname", BaseFilter);
            SetFilter(searchString);
            var members = new List<ActiveDirectoryObject>();
            try
            {
                uint rangeStep = 1000;
                uint rangeLow = 0;
                var rangeHigh = rangeLow + (rangeStep - 1);
                var lastQuery = false;
                var quitLoop = false;

                do
                {
                    string attributeWithRange;
                    if (!lastQuery)
                        attributeWithRange = string.Format("member;range={0}-{1}", rangeLow, rangeHigh);
                    else
                        attributeWithRange = string.Format("member;range={0}-*", rangeLow);
                    PropertiesToLoad = new[] {attributeWithRange, ActiveDirectoryPropertyNames.DISTINGUISHED_NAME};
                    BuildArguments();
                    var results = Searcher.FindOne();
                    //ActiveDirectoryObject results = this.SingleSearch();
                    if (results.Properties.Contains(attributeWithRange))
                    {
                        foreach (var obj in results.Properties[attributeWithRange])
                            //Console.WriteLine(obj.GetType());
                            if (obj.GetType().Equals(typeof(string)))
                            {
                                var member = Checker.CreateObject(obj.ToString());
                                members.Add(member);
                            }
                            else if (obj.GetType().Equals(typeof(int)))
                            {
                            }
                        // Console.WriteLine(obj.ToString());

                        if (lastQuery) quitLoop = true;
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
                } while (!quitLoop);
            }
            catch (Exception ex)
            {
                // Handle exception ex.
            }

            return members;
        }

        public List<ActiveDirectoryObject> GetUserMemberOf(string userDistingishedName)
        {
            var searchString = string.Format("(&(objectCategory=person)(objectClass=user)({0}={1}){2})",
                "distinguishedname", userDistingishedName, BaseFilter);
            SetFilter(searchString);
            var memberof = new List<ActiveDirectoryObject>();
            try
            {
                BuildArguments();
                var results = Searcher.FindOne();
                foreach (var obj in results.Properties["memberof"])
                    if (obj.GetType().Equals(typeof(string)))
                    {
                        var member = Checker.CreateObject(obj.ToString());
                        memberof.Add(member);
                    }
            }
            catch (Exception ex)
            {
                throw ex;
            }

            return memberof;
        }
    }
}