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
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Xml;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.Cloud.OpenStack.OpenStackConstants.#.cctor()", MessageId = "Slo")]
namespace AvePoint.Media.Storage.Cloud.OpenStack
{
    class OpenStackUtil
    {
        public static string Convert2StorageInfo(OpenStackStorageInfo info)
        {
            return string.Format("<StorageInfo metaId=\"{0}\" contentId=\"{1}\"/>", info.MetaId, info.ContentId);
        }

        public static OpenStackStorageInfo Convert2CAStorStorageInfo(string storageInfo)
        {
            OpenStackStorageInfo info = new OpenStackStorageInfo();
            if (!string.IsNullOrEmpty(storageInfo))
            {
                XmlDocument xmlDoc = new XmlDocument();
                xmlDoc.LoadXml(storageInfo);
                XmlElement node = (XmlElement)xmlDoc.SelectSingleNode("StorageInfo");
                info.MetaId = node.GetAttribute("metaId");
                info.ContentId = node.GetAttribute("contentId");
            }
            return info;
        }

        public static string ParseSpaceField(string jsonString, string pattern)
        {
            Match m = Regex.Match(jsonString, pattern);
            if (!m.Success)
            {
                throw new Exception("Match space field failed.");
            }
            string[] tempStrs = m.Groups[0].Value.Split(':');
            return tempStrs[1];
        }

        public static string GetCheckCtnErrorMsg(string xSetName)
        {
            return string.Format("Check container failed, container : {0}.", xSetName);
        }

        public static string GetCreateCtnErrorMsg(string xSetName)
        {
            return string.Format("Create container failed, container : {0}.", xSetName);
        }

        public static string GetDeleteCtnErrorMsg(string xSetName)
        {
            return string.Format("Delete container failed, container : {0}.", xSetName);
        }

        public static string GetListObjErrorMsg(string xSetName)
        {
            return string.Format("List object failed, container : {0}.", xSetName);
        }

        public static string GetCheckObjErrorMsg(string xSetName, string xStreamName)
        {
            return string.Format("Check object failed, object : {0}, container : {1}.", xStreamName, xSetName);
        }

        public static string GetCreateObjErrorMsg(string xSetName, string xStreamName)
        {
            return string.Format("Create object failed, object : {0}, container : {1}.", xStreamName, xSetName);
        }

        public static string GetOpenObjErrorMsg(string xSetName, string xStream)
        {
            return string.Format("Open object failed, object : {0}, container : {1}.", xStream, xSetName);
        }

        public static string GetDeleteObjErrorMsg(string xSetName, string xStream)
        {
            return string.Format("Delete object failed, object : {0}, container : {1}.", xStream, xSetName);
        }
    }

    class OpenStackStorageInfo
    {
        public string MetaId { get; set; }
        public string ContentId { get; set; }
    }

    class OpenStackConstants
    {
        public static readonly string HttpMethod_PUT = "PUT";
        public static readonly string HttpMethod_GET = "GET";
        public static readonly string HttpMethod_DELETE = "DELETE";
        public static readonly string HttpMethod_POST = "POST";
        public static readonly string HttpMethod_HEAD = "HEAD";
        public static readonly string HttpMethod_COPY = "COPY";


        public static readonly string X_STORAGE_USER = "x-auth-user";
        public static readonly string X_STORAGE_PASS = "x-auth-key";
        public static readonly string X_STORAGE_URL = "X-Storage-Url";
        public static readonly string X_AUTH_TOKEN = "X-Auth-Token";
        public static readonly string X_CDN_URI = "X-CDN-URI";
        public static readonly string X_CDN_ENABLED = "X-CDN-Enabled";
        public static readonly string X_CDN_MANAGEMENT_URL = "X-CDN-Management-URL";

        public static readonly string TENANTNAME_KEY = "tenantName".ToLower(CultureInfo.InvariantCulture);
        public static readonly string TENANTID_KEY = "tenantID".ToLower(CultureInfo.InvariantCulture);
        public static readonly string USERNAME_KEY = "username";
        public static readonly string PASSWORD_KEY = "secret";
        public static readonly string AUTHENTICATION_URL_KEY = "authenticationUrl".ToLower(CultureInfo.InvariantCulture);
        public static readonly string AUTHENTICATION_TYPE_KEY = "authenticationType".ToLower(CultureInfo.InvariantCulture);
        public static readonly string AUTHENTICATION_VERSION_KEY = "authenticationVersion".ToLower(CultureInfo.InvariantCulture);
        public static readonly string SystemLocationKeyName = "containerName".ToLower(CultureInfo.InvariantCulture);
        public static readonly string CREATE_IF_NOT_EXISTS = "creation";
        public static readonly string ENABLESLO_KEY = "enableSlo".ToLower(CultureInfo.InvariantCulture);

        public static readonly string SingleUploadMaxSize_KEY = "SingleUploadMaxSize".ToLower(CultureInfo.InvariantCulture);
        public static readonly string SegmentMinSize_KEY = "SegmentMinSize".ToLower(CultureInfo.InvariantCulture);
        public static readonly string MaxFileSize_KEY = "MaxFileSize".ToLower(CultureInfo.InvariantCulture);
        public static readonly string EnableBulkDelete_KEY = "enableBulkDelete".ToLower(CultureInfo.InvariantCulture);

    }
}
