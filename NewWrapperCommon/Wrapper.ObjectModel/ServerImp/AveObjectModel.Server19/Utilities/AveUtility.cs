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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Utilities;
using Microsoft.SharePoint.Administration;

namespace AvePoint.ObjectModel.Server19
{
    //interface for Microsoft.SharePoint.Utilities.SPUtility
    class AveUtility : IAveUtility
    {
        public AveUtility()
        { }

        public string GetLocalizedString(string source, string defaultResourceFile, uint language)
        {
            return SPUtility.GetLocalizedString(source, defaultResourceFile, language);
        }

        public IAvePrincipalInfo ResolvePrincipal(IAveWeb web, string input, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, bool inputIsEmailOnly, bool ignoreDomainDiff = false)
        {
            SPUserCollection users = usersContainer == null ? null : (usersContainer as AveUserCollection).Users;
            SPPrincipalInfo info = SPUtility.ResolvePrincipal((web as AveWeb).Web, input, (SPPrincipalType)scopes, (SPPrincipalSource)sources, users, inputIsEmailOnly);
            if (info != null)
            {
                return new AvePrincipalInfo(info);
            }
            return null;
        }

        public IAvePrincipalInfo ResolvePrincipal(IAveWebApplication webApp, AveUrlZone? urlZone, string input, AvePrincipalType scopes, AvePrincipalSource sources, bool inputIsEmailOnly)
        {
            SPPrincipalInfo principalInfo = SPUtility.ResolvePrincipal((webApp as AveWebApplication).WebApplication, (SPUrlZone?)urlZone, input, (SPPrincipalType)scopes, (SPPrincipalSource)sources, inputIsEmailOnly);
            if (principalInfo != null)
            {
                return new AvePrincipalInfo(principalInfo);

            }
            return null;
        }

        public IAvePrincipalInfo ResolveWindowsPrincipal(IAveWebApplication webApp, string input, AvePrincipalType scopes, bool inputIsEmailOnly)
        {
            SPPrincipalInfo principalInfo = SPUtility.ResolveWindowsPrincipal((webApp as AveWebApplication).WebApplication, input, (SPPrincipalType)scopes, inputIsEmailOnly);
            if (principalInfo == null)
            {
                return null;
            }
            return new AvePrincipalInfo(principalInfo);
        }

        public IList<IAvePrincipalInfo> SearchWindowsPrincipals(IAveWebApplication webApp, string input, AvePrincipalType scopes, int maxCount, out bool reachMaxCount)
        {
            IList<SPPrincipalInfo> principalInfos = SPUtility.SearchWindowsPrincipals((webApp as AveWebApplication).WebApplication, input, (SPPrincipalType)scopes, maxCount, out reachMaxCount);
            IList<IAvePrincipalInfo> avePrincipalInfos = new List<IAvePrincipalInfo>();
            foreach (SPPrincipalInfo principalInfo in principalInfos)
            {
                avePrincipalInfos.Add(new AvePrincipalInfo(principalInfo));
            }
            return avePrincipalInfos;
        }

        public Guid GetWeb(IAveSite spSite, string webUrl)
        {
            return (spSite as AveSite).QueryService.GetWebId(spSite.ID, webUrl);
        }

        public string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        {
            return SPUtility.CreateISO8601DateTimeFromSystemDateTime(dtValue);
        }

        public DateTime CreateSystemDateTimeFromXmlDataDateTimeFormat(string strDT)
        {
            return SPUtility.CreateSystemDateTimeFromXmlDataDateTimeFormat(strDT);
        }

        public string GregorianISOToIntlISODate(IAveWeb web, string strISODate, int iCalType)
        {
            return SPUtility.GregorianISOToIntlISODate((web as AveWeb).Web, strISODate, iCalType);
        }

        public bool IsEmailServerSet(IAveWeb web)
        {
            return SPUtility.IsEmailServerSet((web as AveWeb).Web);
        }

        public bool SendEmail(IAveWeb web, bool fAppendHtmlTag, bool fHtmlEncode, string to, string subject, string htmlBody, bool appendFooter)
        {
            return SPUtility.SendEmail((web as AveWeb).Web, fAppendHtmlTag, fHtmlEncode, to, subject, htmlBody, appendFooter);
        }

        public IList<IAvePrincipalInfo> SearchPrincipals(IAveWeb web, string input, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, int maxCount, out bool reachMaxCount)
        {
            IList<SPPrincipalInfo> principalInfos = SPUtility.SearchPrincipals((web as AveWeb).Web, input, (SPPrincipalType)scopes, (SPPrincipalSource)sources, usersContainer == null ? null : (usersContainer as AveUserCollection).Users, maxCount, out reachMaxCount);
            IList<IAvePrincipalInfo> avePrincipalInfos = new List<IAvePrincipalInfo>();
            foreach (SPPrincipalInfo principalInfo in principalInfos)
            {
                avePrincipalInfos.Add(new AvePrincipalInfo(principalInfo));
            }
            return avePrincipalInfos;
        }

        public IList<IAvePrincipalInfo> SearchPrincipals(IAveWebApplication webApp, AveUrlZone? urlZone, string input, AvePrincipalType scopes, AvePrincipalSource sources, int maxCount, out bool reachMaxCount)
        {
            IList<SPPrincipalInfo> principalInfos = SPUtility.SearchPrincipals((webApp as AveWebApplication).WebApplication, (SPUrlZone?)urlZone, input, (SPPrincipalType)scopes, (SPPrincipalSource)sources, maxCount, out reachMaxCount);
            IList<IAvePrincipalInfo> avePrincipalInfos = new List<IAvePrincipalInfo>();
            foreach (SPPrincipalInfo principalInfo in principalInfos)
            {
                avePrincipalInfos.Add(new AvePrincipalInfo(principalInfo));
            }
            return avePrincipalInfos;
        }

        public IAveListItem CreateNewDiscussion(IAveList list, string title)
        {
            return new AveListItem(list.Items as AveListItemCollection, SPUtility.CreateNewDiscussion((list as AveList).List, title));
        }

        public IAveListItem CreateNewDiscussion(IAveListItemCollection items, string title)
        {
            return new AveListItem(items as AveListItemCollection, SPUtility.CreateNewDiscussion((items as AveListItemCollection).ListItemCollection, title));
        }

        public IAveListItem CreateNewDiscussionReply(IAveListItem parent)
        {
            return new AveListItem(parent.ParentList.Items as AveListItemCollection, SPUtility.CreateNewDiscussionReply((parent as AveListItem).ListItem));
        }

        public IAveFile CreateNewWikiPage(IAveList wikiList, string url)
        {
            return new AveFile((wikiList as AveList).ParentWeb as AveWeb, SPUtility.CreateNewWikiPage((wikiList as AveList).List, url));
        }

        public bool ValidateFormDigest()
        {
            return SPUtility.ValidateFormDigest();
        }

        public bool IfServiceAvailable(IAveWebApplication webApp, AveServiceApplicationType serviceType)
        {
            string assemblyQualifiedName = GetServiceApplicationType(serviceType);
            Type type = Type.GetType(assemblyQualifiedName);
            if (type != null)
            {
                if (webApp.ServiceApplicationProxyGroup.ContainsType(type))
                {
                    foreach (IAveServiceApplicationProxy p in webApp.ServiceApplicationProxyGroup.Proxies)
                    {
                        if (p.CheckAssemblyQualifiedName(assemblyQualifiedName)
                            && p.Status == AveObjectStatus.Online)
                        {
                            return true;
                        }
                    }
                }
            }

            return false;
        }

        private string GetServiceApplicationType(AveServiceApplicationType type)
        {
            var map = new Dictionary<AveServiceApplicationType, string>(6)
            {
                {AveServiceApplicationType.UserProfileService,"Microsoft.Office.Server.Administration.UserProfileApplicationProxy, Microsoft.Office.Server.UserProfiles, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"},
                {AveServiceApplicationType.BDCService,"Microsoft.SharePoint.BusinessData.SharedService.BdcServiceApplicationProxy, Microsoft.SharePoint, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"},
                {AveServiceApplicationType.ManagedMetadataService,"Microsoft.SharePoint.Taxonomy.MetadataWebServiceApplicationProxy, Microsoft.SharePoint.Taxonomy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"},
                {AveServiceApplicationType.ManagedMetadataServiceApplication, "Microsoft.SharePoint.Taxonomy.MetadataWebServiceApplication, Microsoft.SharePoint.Taxonomy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"},
                {AveServiceApplicationType.ManagedMetadataServiceApplicationUtilities,"Microsoft.Office.Server.Utilities.SPServiceApplicationUtilities,Microsoft.Office.Server, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"},
                {AveServiceApplicationType.PartionSettings,"Microsoft.SharePoint.Taxonomy.PartitionSettings,Microsoft.SharePoint.Taxonomy, Version=16.0.0.0, Culture=neutral, PublicKeyToken=71e9bce111e9429c"},
            };
            string typeString;
            map.TryGetValue(type, out typeString);
            return typeString;
        }

        public string HexStringFromBytes(byte[] buffer)
        {
            return AveAssemblyUtility.InvokeStaticMethod(typeof(SPUtility), "HexStringFromBytes", new Type[] { typeof(byte[]) }, new object[] { buffer }) as string;
        }

        public string GetGenericSetupPath(string strSubdir)
        {
            return SPUtility.GetGenericSetupPath(strSubdir);
        }

        public string FormatDate(IAveWeb web, DateTime date, AveDateFormat fmt)
        {
            AveWeb aveWeb = web as AveWeb;
            return SPUtility.FormatDate(aveWeb == null ? null : aveWeb.Web, date, (SPDateFormat)fmt);
        }

        public byte[] GetBinaryUserId(string fullName)
        {
            return SPUtility.GetBinaryUserId(fullName);
        }


        public IAvePrincipalInfo[] GetPrincipalsInGroup(IAveWeb web, string input, int maxCount, out bool reachedMaxCount)
        {
            var info = SPUtility.GetPrincipalsInGroup((web as AveWeb).Web, input, maxCount, out reachedMaxCount);
            if (info == null) { return null; }
            return info.Select(spInfo => spInfo == null ? null : new AvePrincipalInfo(spInfo)).ToArray();
       
        }
        
        public AzureRegions GetAzureTypeAndTanentID(string userName, ref string tanentID)
        {
            throw new NotImplementedException();
        }
    }
}
