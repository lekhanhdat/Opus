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

namespace AvePoint.Wrapper.Common
{
    public interface IAveUtility
    {
        string GetLocalizedString(string source, string defaultResourceFile, uint language);
        IAvePrincipalInfo ResolvePrincipal(IAveWeb web, string input, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, bool inputIsEmailOnly, bool ignoreDomainDiff = false);
        IAvePrincipalInfo ResolvePrincipal(IAveWebApplication webApp, AveUrlZone? urlZone, string input, AvePrincipalType scopes, AvePrincipalSource sources, bool inputIsEmailOnly);
        IAvePrincipalInfo ResolveWindowsPrincipal(IAveWebApplication webApp, string input, AvePrincipalType scopes, bool inputIsEmailOnly);
        IList<IAvePrincipalInfo> SearchWindowsPrincipals(IAveWebApplication webApp, string input, AvePrincipalType scopes, int maxCount, out bool reachMaxCount);
        Guid GetWeb(IAveSite spSite, string webUrl);
        string CreateISO8601DateTimeFromSystemDateTime(DateTime dtValue);
        DateTime CreateSystemDateTimeFromXmlDataDateTimeFormat(string strDT);
        string GregorianISOToIntlISODate(IAveWeb web, string strISODate, int iCalType);
        bool IsEmailServerSet(IAveWeb web);
        bool SendEmail(IAveWeb web, bool fAppendHtmlTag, bool fHtmlEncode, string to, string subject, string htmlBody, bool appendFooter);
        IList<IAvePrincipalInfo> SearchPrincipals(IAveWeb web, string input, AvePrincipalType scopes, AvePrincipalSource sources, IAveUserCollection usersContainer, int maxCount, out bool reachMaxCount);
        IList<IAvePrincipalInfo> SearchPrincipals(IAveWebApplication webApp, AveUrlZone? urlZone, string input, AvePrincipalType scopes, AvePrincipalSource sources, int maxCount, out bool reachMaxCount);
        IAveListItem CreateNewDiscussion(IAveList list, string title);
        IAveListItem CreateNewDiscussion(IAveListItemCollection items, string title);
        IAveListItem CreateNewDiscussionReply(IAveListItem parent);
        IAveFile CreateNewWikiPage(IAveList wikiList, string url);
        bool ValidateFormDigest();
        bool IfServiceAvailable(IAveWebApplication webApp, AveServiceApplicationType type);
        string HexStringFromBytes(byte[] buffer);
        string GetGenericSetupPath(string strSubdir);
        string FormatDate(IAveWeb web, DateTime date, AveDateFormat fmt);
        byte[] GetBinaryUserId(string fullName);
        IAvePrincipalInfo[] GetPrincipalsInGroup(IAveWeb web, string input, int maxCount, out bool reachedMaxCount);
        AzureRegions GetAzureTypeAndTanentID(string userName, ref string tanentID);
    }
}
