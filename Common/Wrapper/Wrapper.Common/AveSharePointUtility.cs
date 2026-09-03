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
using System.Globalization;

namespace AvePoint.Wrapper.Common
{
    /// <summary>
    /// Call SharePoint SPUtility
    /// Provides tools for converting date and time formats, for obtaining information from user names, for modifying access to sites, and for various other tasks in managing deployments of Microsoft SharePoint Foundation.
    /// </summary>
    public class AveSharePointUtility
    {
        //private static AveContextKind mContextKind;
        private static string mAssemblyName;
        private static string mNameSpace;
        private static string mUtilityName;
        private const string mWebRelativeUrlPrefix = "~site/";
        private const string mSiteRelativeUrlPrefix = "~sitecollection/";        
    
        internal static string AssmeblyName
        {
            set
            {
                mAssemblyName = value;
            }
        }

        internal static string NameSpace
        {
            set
            {
                mNameSpace = value;
            }
            get
            {
                return mNameSpace;
            }
        }

        internal static string UtilityName
        {
            set
            {
                mUtilityName = value;
            }
        }

        public static string WebRelativeUrlPrefix
        {
            get
            {
                return mWebRelativeUrlPrefix;
            }
        }

        public static string SiteRelativeUrlPrefix
        {
            get
            {
                return mSiteRelativeUrlPrefix;
            }
        }

        public static void LoadAssembly()
        {
            AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "LoadAssemblyByInit", new object[] { });
        }

        public static bool IsEmailServerSet(IAveWeb web)
        {
            return Convert.ToBoolean(AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "IsEmailServerSet", new object[] { web }));
        }

        public static IAveListItem CreateNewDiscussion(IAveList list, string title)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "CreateNewDiscussion", new object[] { list, title }) as IAveListItem;
        }

        public static IAveListItem CreateNewDiscussionReply(IAveListItem list)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "CreateNewDiscussionReply", new object[] { list }) as IAveListItem;
        }

        public static IAveFile CreateNewWikiPage(IAveList wikiList, string url)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "CreateNewWikiPage", new object[] { wikiList, url }) as IAveFile;
        }

        public static string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "CreateISO8601DateTimeFromSystemDateTime", new object[] { dtValue }) as string;
        }

        public static DateTime CreateSystemDateTimeFromXmlDataDateTimeFormat(string strDT)
        {
            return Convert.ToDateTime(AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "CreateSystemDateTimeFromXmlDataDateTimeFormat", new object[] { strDT }));
        }

        public static string GregorianISOToIntlISODate(IAveWeb web, string strISODate, int iCalType)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "GregorianISOToIntlISODate", new object[] { web, strISODate, iCalType }) as string;
        }

        public static string GetLocalizedString(string source, string defaultResourceFile, uint language)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "GetLocalizedString", new object[] { source, defaultResourceFile, language }) as string;
        }

        public static IAvePrincipalInfo ResolveWindowsPrincipal(IAveWebApplication webApp, string input, AvePrincipalType scopes, bool inputIsEmailOnly)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "ResolveWindowsPrincipal", new object[] { webApp, input, scopes, inputIsEmailOnly }) as IAvePrincipalInfo;
        }

        public static IList<IAvePrincipalInfo> SearchWindowsPrincipals(IAveWebApplication webApp, string input, AvePrincipalType scopes, int maxCount, out bool reachMaxCount)
        {
            bool outReachMaxCount = false;
            object obj = AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "SearchWindowsPrincipals", new object[] { webApp, input, scopes, maxCount, outReachMaxCount });
            reachMaxCount = outReachMaxCount;
            return (IList<IAvePrincipalInfo>)obj;
        }

        public static bool StsCompareStrings(string str1, string str2)
        {
            CompareInfo compareInfo = CultureInfo.InvariantCulture.CompareInfo;
            return (0 == compareInfo.Compare(str1, str2, CompareOptions.IgnoreCase));
        }

        public static void HandleAccessDenied(Exception ex)
        {
            AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "HandleAccessDenied", new object[] { ex });
        }

        public static bool SendEmail(IAveWeb web, bool fAppendHtmlTag, bool fHtmlEncode, string to, string subject, string htmlBody, bool appendFooter)
        {
            return Convert.ToBoolean(AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "SendEmail", new Type[] { web.GetType(), typeof(bool), typeof(bool), typeof(string), typeof(string), typeof(string), typeof(bool), }, new object[] { web, fAppendHtmlTag, fHtmlEncode, to, subject, htmlBody, appendFooter }));
        }

        public static IAveListItem CreateNewDiscussion(IAveListItemCollection iAveListItemCollection, string mName)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "CreateNewDiscussion",new Type[]{typeof(IAveListItemCollection),typeof(string)}, new object[] { iAveListItemCollection, mName }) as IAveListItem;
        }

        public static Guid GetWeb(IAveSite spSite, string webUrl)
        {
            Guid id = Guid.Empty;
            using (IAveWeb web = spSite.OpenWeb(webUrl))
            {
                id = web.ID;
            }
            return id;
        }

        public static IAvePrincipalInfo ResolvePrincipal(IAveWeb web, string input, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, bool inputIsEmailOnly)
        {
            return AveAssemblyUtility.InvokeStaticMethod(AveAssemblyUtility.GetType(mAssemblyName, mNameSpace + mUtilityName), "ResolvePrincipal", new Type[] { typeof(IAveWeb), typeof(string), typeof(AvePrincipalType), typeof(AvePrincipalSource), typeof(IAveUserCollection), typeof(bool) }, new object[] { web, input, scopes, sources, usersContainer, inputIsEmailOnly }) as IAvePrincipalInfo;
        }

    }
}
