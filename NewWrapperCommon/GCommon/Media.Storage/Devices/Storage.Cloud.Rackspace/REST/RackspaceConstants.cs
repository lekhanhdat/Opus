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



namespace AvePoint.Media.Storage.Cloud.Rackspace
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    static class RackspaceConstants
    {
        /** HTTP Header token that identifies the username to Cloud Files **/
        public const string X_STORAGE_USER = "x-auth-user";
        /** HTTP header token that identifies the password to Cloud Files **/
        public const string X_STORAGE_PASS = "x-auth-key";
        /** HTTP header token that identifies the Storage URL after a successful user login to Cloud Files **/
        public const string X_STORAGE_URL = "X-Storage-Url";
        /** HTTP header that identifies the CDN Management URL after a successful login to Cloud Files **/
        public const string X_CDN_MANAGEMENT_URL = "X-CDN-Management-URL";
        /** HTTP header token that identifies the Storage Token after a successful user login to Cloud Files **/
        public const string X_AUTH_TOKEN = "X-Auth-Token";
        /** HTTP header token that is returned on a HEAD request against a Container.  The value of this header is the number of Objects in the Container **/
        public const string X_CONTAINER_OBJECT_COUNT = "X-Container-Object-Count";
        /** HTTP header token that is returned on a HEAD request against a Container.  The value of this header is the number of Objects in the Container **/
        public const string X_CONTAINER_BYTES_USED = "X-Container-Bytes-Used";
        /** HTTP header token that is returned on a HEAD request against an Account.  The value of this header is the number of Containers in the Account **/
        public const string X_ACCOUNT_CONTAINER_COUNT = "X-Account-Container-Count";
        /** HTTP header token that is returned on a HEAD request against an Account.  The value of this header is the total size of the Objects in the Account **/
        public const string X_ACCOUNT_BYTES_USED = "X-Account-Bytes-Used";
        /** HTTP header token that is returned by calls to the CDN Management API **/
        public const string X_CDN_URI = "X-CDN-URI";
        /** HTTP header token that is returned by calls to the CDN Management API **/
        public const string X_CDN_TTL = "X-TTL";
        /** HTTP header token that is returned by calls to the CDN Management API **/
        public const string X_CDN_RETAIN_LOGS = "X-Log-Retention";
        /** HTTP header token that is returned by calls to the CDN Management API **/
        public const string X_CDN_ENABLED = "X-CDN-Enabled";
        /** HTTP header token that is returned by calls to the CDN Management API **/
        public const string X_CDN_USER_AGENT_ACL = "X-User-Agent-ACL";
        /** HTTP header token that is returned by calls to the CDN Management API **/
        public const string X_CDN_REFERRER_ACL = "X-Referrer-ACL ";
        /** HTTP Header used by Cloud Files for the MD5Sum of the object being created in a Container **/
        public const string E_TAG = "ETag";
        /** These constants are used for performing queries on the content of a container **/
        public const string LIST_ROOT_NAME_QUERY = "delimiter";
        public const string LIST_CONTAINER_NAME_QUERY = "path";//"prefix";
        public const string LIST_CONTAINER_LIMIT_OBJ_COUNT_QUERY = "limit";
        public const string LIST_CONTAINER_START_OFFSET_QUERY = "offset";
        public const string LIST_CONTAINER_MARKER_QUERY = "marker";

        public const int CONTAINER_NAME_LENGTH = 256;
        public const int OBJECT_NAME_LENGTH = 1024;
        public const int METADATA_NAME_LENGTH = 1024;
        public const int METADATA_VALUE_LENGTH = 1024;

        /** Prefix Cloud Files expects on all Meta data headers on Objects **/
        public const string X_OBJECT_META = "X-Object-Meta-";
        public const string CONTENT_TYPE = X_OBJECT_META + "Content-Type";
        public const string LAST_MODIFIED = X_OBJECT_META + "Last-Modified-Time";

        public const string PSEUDO_CONTAINER_MIME_TYPE = "application/directory";
        //for config httpwebrequest
        public const string STREAM_CONTENT_TYPE = "application/octet-stream";
        public const string SEPARATOR = ",";
        public const string CONTENT_LENGTH = "Content-Length";
        //for config delete data
        public const string META = "meta";
        public const string DATA_ARCHIVE = "data_archive:DataVolume";
    }
}
