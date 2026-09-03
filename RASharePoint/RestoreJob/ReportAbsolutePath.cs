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

namespace AvePoint.Item.Common
{

    public static class ReportAbsolutePath
    {
        public static string GetWebAP(string siteUrl, string webName)
        {
            return siteUrl.TrimEnd('/') + "/" + webName;
        }

        public static string GetFolderVersionAP(string siteUrl, string siteSRUrl, string folerSRUrl, string listViewSRUrl, int id, int version)
        {
            return GetListItemAP(siteUrl, siteSRUrl, folerSRUrl, listViewSRUrl, id, version);
        }

        public static string GetDocumentAP(string siteUrl, string siteSRUrl, string folderSRUrl, string docName)
        {
            return GetUrlBySR(siteUrl, siteSRUrl, folderSRUrl) + '/' + docName;
        }

        public static string GetDocumentVersionAP(string siteUrl, string siteSRUrl, string webUrl, int version, string folderUrl, string folderSRUrl, string docName
            )
        {
            if (!docName.Contains('.'))
            {
                return GetDocumentVersionAP(webUrl, version, folderUrl, docName);
            }
            else
            {
                switch (docName.Substring(docName.LastIndexOf('.')).ToUpper())
                {
                    case ".ASPX":
                        return GetPageVersionAP(siteUrl, siteSRUrl, folderSRUrl, version, docName);
                    default:
                        return GetDocumentVersionAP(webUrl, version, folderUrl, docName);
                }
            }
        }

        public static string GetDocumentVersionAP(string webUrl, int version, string folderUrl, string docName)
        {
            return webUrl.TrimEnd('/') + "/_vti_history/" + version.ToString() + "/" + folderUrl + "/" + docName;
        }

        public static string GetPageVersionAP(string siteUrl, string siteSRUrl, string folderSRUrl, int version, string docName)
        {
            return GetUrlBySR(siteUrl, siteSRUrl, folderSRUrl) + '/' + docName + "?PageVersion=" + version;
        }

        public static string GetListItemAP(string siteUrl, string siteSRUrl, string parentSRUrl, string listViewSRUrl, int id, int version)
        {
            if (string.IsNullOrEmpty(listViewSRUrl))
            {
                return GetUrlBySR(siteUrl, siteSRUrl, parentSRUrl);
            }
            return GetUrlBySR(siteUrl, siteSRUrl, listViewSRUrl) + "?ID=" + id.ToString() + "&VersionNo=" + version.ToString();
        }

        public static string GetAttachmentAP(string siteUrl, string siteSRUrl, string listSRUrl, int id, string attName)
        {
            return GetAttachmentAP(GetUrlBySR(siteUrl, siteSRUrl, listSRUrl), id, attName);
        }
        public static string GetAttachmentAP(string listUrl, int id, string attName)
        {
            return listUrl.TrimEnd('/') + "/Attachments/" + id + "/" + attName;
        }

        public static string GetUrlBySR(string siteUrl, string siteSRUrl, string objSRUrl)
        {
            if (string.IsNullOrEmpty(siteUrl))
            {
                return null;
            }
            if (string.IsNullOrEmpty(siteSRUrl) || !siteUrl.Contains(siteSRUrl) || string.IsNullOrEmpty(objSRUrl))
            {
                return siteUrl;
            }
            else if (string.Equals(siteSRUrl, "/", StringComparison.Ordinal))  //rootSiteCollection
            {
                return !objSRUrl.Contains(siteUrl) ? (siteUrl.TrimEnd('/') + '/' + objSRUrl.TrimStart('/')).TrimEnd('/') : objSRUrl.TrimEnd('/');
            }
            return !objSRUrl.Contains(siteUrl) ? (siteUrl.Remove(siteUrl.LastIndexOf(siteSRUrl, StringComparison.OrdinalIgnoreCase)).TrimEnd('/') + '/' + objSRUrl.TrimStart('/')).TrimEnd('/') : objSRUrl.TrimEnd('/');
        }

        public static string GetTitle(string objSRUrl)
        {
            if (string.IsNullOrEmpty(objSRUrl))
            {
                return objSRUrl;
            }
            return objSRUrl.Contains("\\") ? objSRUrl.Substring(objSRUrl.LastIndexOf('\\') + 1) : objSRUrl;
        }

        public static string GetReportTitle(string objUrl)
        {
            if (string.IsNullOrEmpty(objUrl))
            {
                return objUrl;
            }
            return objUrl.Contains("/") ? objUrl.Substring(objUrl.LastIndexOf('/') + 1) : objUrl;
        }
    }
}
