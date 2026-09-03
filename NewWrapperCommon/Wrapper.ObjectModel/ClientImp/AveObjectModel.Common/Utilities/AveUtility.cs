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
using System.Globalization;
using System.Linq;
using System.Text;
using AvePoint.Wrapper.Common;
using DnsClient;
using DnsClient.Protocol;

namespace AvePoint.ObjectModel.Common
{
    class AveUtility : AveClientObject, IAveUtility
    {
        public AveUtility()
        { }

        public string GetLocalizedString(string source, string defaultResourceFile, uint language)
        {
            return source;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="web"></param>
        /// <param name="input"></param>
        /// <param name="scopes"></param>
        /// <param name="sources"></param>
        /// <param name="usersContainer"></param>
        /// <param name="inputIsEmailOnly"></param>
        /// <param name="ignoreDomainDiff">当前默认值为false，个人认为这个参数没有必要，考虑68去掉该参数[xluo]</param>
        /// <returns></returns>
        public IAvePrincipalInfo ResolvePrincipal(IAveWeb web, string input, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, bool inputIsEmailOnly, bool ignoreDomainDiff = false)
        {
            //input是空格是可以匹配到All Windows User这个用户。
            if (input == null || string.IsNullOrEmpty(input.Trim()))
            {
                return null;
            }
            int index = -1;
            if (web.Site.IsOnlineSite)
            {
                index = input.IndexOf(':'); //for CM
                if (index >= 0)
                {           
                    input = input.Substring(index + 1);
                }
                index = input.LastIndexOf('|');
                if (index >= 0) //for CA
                {   
                    input = input.Substring(index + 1);
                }
            }
            Dictionary<string, object> infoDic = (web.Site as AveSite).Request.ResolvePrincipal(web.ServerRelativeUrl, input, (int)scopes, (int)sources, inputIsEmailOnly, ignoreDomainDiff);
            if (infoDic.Count > 0)
            {
                //int principalId = (int)infoDic["PrincipalId"];
                //if (principalId < 0)
                //{
                //    infoDic["PrincipalId"] = 1;
                //}
                return new AvePrincipalInfo(infoDic);
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
            throw new NotImplementedException();
        }

        public string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(dtValue.Year.ToString("0000"));
            builder.Append("-");
            builder.Append(dtValue.Month.ToString("00"));
            builder.Append("-");
            builder.Append(dtValue.Day.ToString("00"));
            builder.Append("T");
            builder.Append(dtValue.Hour.ToString("00"));
            builder.Append(":");
            builder.Append(dtValue.Minute.ToString("00"));
            builder.Append(":");
            builder.Append(dtValue.Second.ToString("00"));
            builder.Append("Z");
            return builder.ToString();
        }

        public DateTime CreateSystemDateTimeFromXmlDataDateTimeFormat(string strDT)
        {
            return CreateSystemDateTimeFromXmlDataDateTimeFormat(strDT, false);
        }

        internal DateTime CreateSystemDateTimeFromXmlDataDateTimeFormat(string strDT, bool fPreserveMilliseconds)
        {
            return CreateSystemDateTimeFromXmlDataDateTimeFormat(strDT, fPreserveMilliseconds, false);
        }

        internal DateTime CreateSystemDateTimeFromXmlDataDateTimeFormat(string strDT, bool fPreserveMilliseconds, bool fPreserveDateTimeKind)
        {
            if (string.IsNullOrEmpty(strDT))
            {
                throw new ArgumentNullException(strDT);
            }
            int year = Convert.ToInt32(strDT.Substring(0, 4), CultureInfo.InvariantCulture);
            int month = Convert.ToInt32(strDT.Substring(5, 2), CultureInfo.InvariantCulture);
            int day = Convert.ToInt32(strDT.Substring(8, 2), CultureInfo.InvariantCulture);
            int hour = Convert.ToInt32(strDT.Substring(11, 2), CultureInfo.InvariantCulture);
            int minute = Convert.ToInt32(strDT.Substring(14, 2), CultureInfo.InvariantCulture);
            int second = Convert.ToInt32(strDT.Substring(0x11, 2), CultureInfo.InvariantCulture);
            int millisecond = 0;
            if (fPreserveMilliseconds && (strDT.Length >= 0x17))
            {
                millisecond = Convert.ToInt32(strDT.Substring(20, 3), CultureInfo.InvariantCulture);
            }
            bool flag = false;
            if (fPreserveDateTimeKind)
            {
                flag = (strDT.Length > 0x17) && (strDT.Substring(0x17, 1).ToUpper(CultureInfo.InvariantCulture) == "Z");
            }
            return new DateTime(year, month, day, hour, minute, second, millisecond, new GregorianCalendar(), flag ? DateTimeKind.Utc : DateTimeKind.Unspecified);
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
            IAveRequest request = (list as AveList).Request;
            Dictionary<string, object> itemProps = new Dictionary<string, object>();
            itemProps["folderUrl"] = list.RootFolder.ServerRelativeUrl;
            itemProps["FileSystemObjectType"] = 1;
            itemProps["leafName"] = title;
            itemProps["isDiscussion"] = true;
            IAveListItem topic = new AveListItem(request, list.ParentWeb, list, itemProps, true);
            topic["Title"] = title;
            return topic;
        }

        public IAveListItem CreateNewDiscussion(IAveListItemCollection items, string title)
        {
            return CreateNewDiscussion(items.List, title);
        }

        public IAveListItem CreateNewDiscussionReply(IAveListItem parent)
        {
            IAveRequest request = (parent.ParentList as AveList).Request;
            Dictionary<string, object> itemProps = new Dictionary<string, object>();
            itemProps["parentId"] = parent.ID;
            itemProps["isDiscussion"] = true;
            IAveListItem reply = new AveListItem(request, parent.Web, parent.ParentList, itemProps, true);
            return reply;
        }

        public IAveFile CreateNewWikiPage(IAveList wikiList, string url)
        {
            throw new NotImplementedException();
        }

        public bool ValidateFormDigest()
        {
            throw new NotImplementedException();
        }

        public bool IfServiceAvailable(IAveWebApplication webApp, AveServiceApplicationType type)
        {
            throw new NotImplementedException();
        }


        public string HexStringFromBytes(byte[] buffer)
        {
            throw new NotImplementedException();
        }

        public string GetGenericSetupPath(string strSubdir)
        {
            throw new NotImplementedException();
        }

        public string FormatDate(IAveWeb web, DateTime date, AveDateFormat fmt)
        {
            return null;
        }

        public byte[] GetBinaryUserId(string fullName)
        {
            return null;
        }


        public IAvePrincipalInfo[] GetPrincipalsInGroup(IAveWeb web, string input, int maxCount, out bool reachedMaxCount)
        {
            throw new NotImplementedException();
        }

        public AzureRegions GetAzureTypeAndTanentID(string userName, ref string tanentID)
        {
            return RegionValidation.LoadTenantRegionWithUserName(userName, ref tanentID);
        }
    }
}
