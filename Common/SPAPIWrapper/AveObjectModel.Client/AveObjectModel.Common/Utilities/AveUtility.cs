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
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;

namespace AvePoint.ObjectModel.Common
{
    class AveUtility : AveClientObject, IAveUtility
    {
        public AveUtility()
        { }

        public string GetLocalizedString(string source, string defaultResourceFile, uint language)
        {
            //throw new NotImplementedException();
            return source;
        }

        private string TrimSearchName(string searchName)
        {
            int index = -1;
            if (searchName.Contains(":"))//for CM
            {
                index = searchName.IndexOf(':');
                searchName = searchName.Substring(index + 1);
            }
            if (searchName.Contains("|"))//for CA
            {
                index = searchName.LastIndexOf('|');
                searchName = searchName.Substring(index + 1);
            }
            return searchName;
        }

        public Dictionary<string, IAvePrincipalInfo> ResolvePrincipals(IAveWeb web, List<string> searchNames, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, bool inputIsEmailOnly, bool ignoreDomainDiff = true)
        {
            Dictionary<string, IAvePrincipalInfo> resolvedPrincipals = new Dictionary<string, IAvePrincipalInfo>();
            Dictionary<string, string> loginNameAndSearchNameMap = new Dictionary<string, string>();
            for (int i = 0; i < searchNames.Count; i++)
            {
                string searchName = TrimSearchName(searchNames[i]);
                loginNameAndSearchNameMap[searchName]= searchNames[i];
                searchNames[i] = searchName;
            }
            searchNames = searchNames.Distinct().ToList();
            Dictionary<string, Dictionary<string, object>> principalInfos = ((web.Site as AveSite).Request as IAveRequest).ResolvePrincipals(web.ServerRelativeUrl, searchNames, (int)scopes, (int)sources, inputIsEmailOnly, ignoreDomainDiff);
            foreach (KeyValuePair<string, Dictionary<string, object>> principalInfo in principalInfos)
            {
                resolvedPrincipals.Add(loginNameAndSearchNameMap[principalInfo.Key], (principalInfo.Value != null && principalInfo.Value.Count > 0) ? new AvePrincipalInfo(principalInfo.Value) : null);
            }
            return resolvedPrincipals;
        }

        public IAvePrincipalInfo ResolvePrincipal(IAveWeb web, string searchName, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, bool inputIsEmailOnly, bool ignoreDomainDiff = true)
        {
            searchName = TrimSearchName(searchName);
            Dictionary<string, object> principalInfo = (web.Site as AveSite).Request.ResolvePrincipal(web.ServerRelativeUrl, searchName, (int)scopes, (int)sources, inputIsEmailOnly, ignoreDomainDiff);
            if (principalInfo != null && principalInfo.Count > 0)
            {
                return new AvePrincipalInfo(principalInfo);
            }
            return null;
        }

        public IList<IAvePrincipalInfo> SearchPrincipals(IAveWeb web, string input, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, int maxCount, out bool reachMaxCount)
        {
            reachMaxCount = false;
            List<IAvePrincipalInfo> searchList = new List<IAvePrincipalInfo>();
            Dictionary<string, object> infos = (web.Site as AveSite).Request.SearchPrincipals(web.ServerRelativeUrl, input, (int)scopes, (int)sources, maxCount);
            if (infos.ContainsKey("Principals"))
            {
                List<Dictionary<string, object>> infoList = infos["Principals"] as List<Dictionary<string, object>>;
                foreach (Dictionary<string, object> infoDic in infoList)
                {
                    AvePrincipalInfo info = new AvePrincipalInfo(infoDic);
                    searchList.Add(info);
                }
            }
            return searchList;
        }

        public IAvePrincipalInfo ResolvePrincipal(IAveWebApplication webApp, AveUrlZone? urlZone, string input, AvePrincipalType scopes, AvePrincipalSource sources, bool inputIsEmailOnly)
        {
            throw new NotImplementedException();
        }

        public IAvePrincipalInfo ResolveWindowsPrincipal(IAveWebApplication webApp, string input, AvePrincipalType scopes, bool inputIsEmailOnly)
        {
            throw new NotImplementedException();
        }

        public IList<IAvePrincipalInfo> SearchWindowsPrincipals(IAveWebApplication webApp, string input, AvePrincipalType scopes, int maxCount, out bool reachMaxCount)
        {
            throw new NotImplementedException();
        }

        public Guid GetWeb(IAveSite spSite, string webUrl)
        {
            using (IAveWeb web = spSite.OpenWeb(webUrl))
            {
                return web.ID;
            }
        }

        public string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        {
            throw new NotImplementedException();
        }

        public DateTime CreateSystemDateTimeFromXmlDataDateTimeFormat(string strDT)
        {
            throw new NotImplementedException();
        }

        public string GregorianISOToIntlISODate(IAveWeb web, string strISODate, int iCalType)
        {
            throw new NotImplementedException();
        }

        public bool IsEmailServerSet(IAveWeb web)
        {
            return false;//暂时无法获取
        }

        public bool SendEmail(IAveWeb web, bool fAppendHtmlTag, bool fHtmlEncode, string to, string subject, string htmlBody, bool appendFooter)
        {
            throw new NotImplementedException();
        }

        public IList<IAvePrincipalInfo> SearchPrincipals(IAveWebApplication webApp, AveUrlZone? urlZone, string input, AvePrincipalType scopes, AvePrincipalSource sources, int maxCount, out bool reachMaxCount)
        {
            throw new NotImplementedException();
        }

        public IAveListItem CreateNewDiscussion(IAveList list, string title)
        {
            throw new NotImplementedException();
        }

        public IAveListItem CreateNewDiscussion(IAveListItemCollection items, string title)
        {
            throw new NotImplementedException();
        }

        public IAveListItem CreateNewDiscussionReply(IAveListItem parent)
        {
            throw new NotImplementedException();
        }

        public IAveFile CreateNewWikiPage(IAveList wikiList, string url)
        {
            throw new NotImplementedException();
        }

        public bool ValidateFormDigest()
        {
            throw new NotImplementedException();
        }

        public void ShareObject(IAveWeb web, string url, string peoplePickerInput, string roleValue, int groupId, bool propagateAcl, bool sendEmail, bool includeAnonymousLinkInEmail, string emailSubject, string emailBody, bool useSimplifiedRoles)
        {
            ((web.Site as AveSite).Request as IAveRequest).ShareObject(web.Url, url, peoplePickerInput, roleValue, groupId, propagateAcl, sendEmail, includeAnonymousLinkInEmail, emailSubject, emailBody, useSimplifiedRoles);
        }

        public string CreateAnonymousLinkWithExpiration(IAveWeb web, string fileFullPath, bool isEditLink, long expirationTicks = default(long))
        {
            return ((web.Site as AveSite).Request as IAveRequest).CreateAnonymousLinkWithExpiration(web.Url, fileFullPath, isEditLink, expirationTicks);
        }

        public string CreateOrganizationSharingLink(IAveWeb web, string fileFullPath, bool isEditLink)
        {
            return ((web.Site as AveSite).Request as IAveRequest).CreateOrganizationSharingLink(web.Url, fileFullPath, isEditLink);
        }
    }
}
