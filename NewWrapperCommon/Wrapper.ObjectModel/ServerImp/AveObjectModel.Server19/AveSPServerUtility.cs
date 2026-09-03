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
using Microsoft.SharePoint;
using System.Xml;
using AvePoint.GCommon;

namespace AvePoint.ObjectModel.Server19
{
    class AveSPServerUtility
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AveSPServerUtility));
        //internal static void LoadAssemblyByInit()
        //{
        //    AveServerAssemblyInit.LoadAssembly();
        //}

        //internal static IAveListItem CreateNewDiscussion(IAveList list, string title)
        //{
        //    return new AveListItem(list.Items as AveListItemCollection, SPUtility.CreateNewDiscussion((list as AveList).List, title));
        //}

        //internal static IAveListItem CreateNewDiscussionReply(IAveListItem aveItem)
        //{
        //    return new AveListItem(aveItem.ParentList.Items as AveListItemCollection,SPUtility.CreateNewDiscussionReply((aveItem as AveListItem).ListItem));
        //}
        //internal static IAveListItem CreateNewDiscussion(IAveListItemCollection iAveListItemCollection, string mName)
        //{
        //    return new AveListItem(iAveListItemCollection as AveListItemCollection, SPUtility.CreateNewDiscussion((iAveListItemCollection.List as AveList).List, mName));
        //}
                     

        //internal static IAveFile CreateNewWikiPage(IAveList wikiList, string url)
        //{
        //    return new AveFile((wikiList as AveList).ParentWeb as AveWeb,SPUtility.CreateNewWikiPage((wikiList as AveList).List, url));            
        //}

        //internal static string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        //{
        //    return SPUtility.CreateISO8601DateTimeFromSystemDateTime(dtValue);
        //}

        //internal static DateTime CreateSystemDateTimeFromXmlDataDateTimeFormat(string strDT)
        //{
        //    return SPUtility.CreateSystemDateTimeFromXmlDataDateTimeFormat(strDT);
        //}

        //internal static string GregorianISOToIntlISODate(IAveWeb web, string strISODate, int iCalType)
        //{
        //    return SPUtility.GregorianISOToIntlISODate((web as AveWeb).Web, strISODate, iCalType);
        //}

        //public static string GetLocalizedString(string source, string defaultResourceFile, uint language)
        //{
        //    return SPUtility.GetLocalizedString(source, defaultResourceFile, language);
        //}

        //internal static IAvePrincipalInfo ResolveWindowsPrincipal(IAveWebApplication webApp, string input, AvePrincipalType scopes, bool inputIsEmailOnly)
        //{
        //    SPPrincipalInfo principalInfo = SPUtility.ResolveWindowsPrincipal((webApp as AveWebApplication).WebApplication, input, (SPPrincipalType)scopes, inputIsEmailOnly);
        //    if (principalInfo == null)
        //    {
        //        return null;
        //    }
        //    return new AvePrincipalInfo(principalInfo);
        //}

        //internal static IList<IAvePrincipalInfo> SearchWindowsPrincipals(IAveWebApplication webApp, string input, AvePrincipalType scopes, int maxCount, out bool reachMaxCount)
        //{
        //    IList<SPPrincipalInfo> principalInfos = SPUtility.SearchWindowsPrincipals((webApp as AveWebApplication).WebApplication, input, (SPPrincipalType)scopes, maxCount, out reachMaxCount);
        //    IList<IAvePrincipalInfo> avePrincipalInfos = new List<IAvePrincipalInfo>();
        //    foreach (SPPrincipalInfo principalInfo in principalInfos)
        //    {
        //        avePrincipalInfos.Add(new AvePrincipalInfo(principalInfo));
        //    }
        //    return avePrincipalInfos;
        //}

        //internal static bool IsEmailServerSet(IAveWeb web)
        //{
        //    return SPUtility.IsEmailServerSet((web as AveWeb).Web);
        //}

        //internal static bool SendEmail(IAveWeb web, bool fAppendHtmlTag, bool fHtmlEncode, string to, string subject, string htmlBody, bool appendFooter)
        //{
        //    return SPUtility.SendEmail((web as AveWeb).Web, fAppendHtmlTag, fHtmlEncode, to, subject, htmlBody, appendFooter);
        //}

        //the same as AveSPUtility.IsOrInSystemFormsFolder(IAveFolder folder), need to remove one
        internal static bool IsOrInSystemFormsFolder(SPFolder spFolder)
        {
            if (spFolder.ParentListId == Guid.Empty) return false;

            SPList list = spFolder.ParentWeb.Lists[spFolder.ParentListId];
            if (list.BaseType != SPBaseType.DocumentLibrary)
            {
                return false;
            }
            return spFolder.ServerRelativeUrl.StartsWith(list.RootFolder.ServerRelativeUrl + "/Forms/", StringComparison.OrdinalIgnoreCase)
                || spFolder.ServerRelativeUrl.Equals(list.RootFolder.ServerRelativeUrl + "/Forms", StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsOrInListRootFolder(SPFolder spFolder)
        {
            if (spFolder.ParentListId == Guid.Empty) return false;

            SPList list = spFolder.ParentWeb.Lists[spFolder.ParentListId];
            if (list.BaseType == SPBaseType.DocumentLibrary)
            {
                return false;
            }
            return spFolder.ServerRelativeUrl.StartsWith(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase)
                || spFolder.ServerRelativeUrl.Equals(list.RootFolder.ServerRelativeUrl, StringComparison.OrdinalIgnoreCase);
        }

        internal static bool IsFormsFile(SPFile file) 
        {
            if(file.ParentFolder== null || file.ParentFolder.ParentListId == Guid.Empty)
            {
                return false;
            }

            string xPath = ".//*[@Url='" + file.ServerRelativeUrl + "']";
            SPList list = file.Web.Lists[file.ParentFolder.ParentListId];
            if (!string.IsNullOrEmpty(list.Forms.SchemaXml))
            {
                XmlDocument formsDoc = new XmlDocument();
                formsDoc.LoadXml(list.Forms.SchemaXml);
                try
                {
                    XmlNode node = formsDoc.SelectSingleNode(xPath);
                    return node != null;
                }
                catch (Exception ex) 
                {
                    logger.Warn("Error when finds forms file through server relative url. Error Message: {0}",ex.ToString());
                }
            }
            return false;
        }

        internal static bool UnderWebRootFolder(SPFile file)
        {
            return (file.ParentFolder != null && file.ParentFolder.ParentListId == Guid.Empty);
        }
        //internal static IAvePrincipalInfo ResolvePrincipal(SPWeb web, string input, SPPrincipalType scopes, SPPrincipalSource sources, SPUserCollection usersContainer, bool inputIsEmailOnly)
        //{
        //    SPPrincipalInfo info = SPUtility.ResolvePrincipal(web, input, scopes, sources, usersContainer, inputIsEmailOnly);
        //    if(info==null)
        //    {
        //        return null;
        //    }
        //    return new AvePrincipalInfo(info);
        //}


        //internal static void ValidateFormDigest()
        //{
        //    SPUtility.ValidateFormDigest();
        //}
    }
}
