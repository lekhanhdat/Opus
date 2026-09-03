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


namespace AvePoint.Media.Storage.Cloud.Azure
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    using System.Globalization;
    #endregion

    class MSAzureConstants
    {
        public const string BlockList = "BlockList";
        public const string Block = "Block";
        public const string EnumerationResults = "EnumerationResults";
        public const string Prefix = "prefix";
        public const string Marker = "marker";
        public static readonly string MaxResults = "MAXRESULTS".ToLower(CultureInfo.InvariantCulture);
        public const string Delimiter = "delimiter";
        public const string NextMarker = "NextMarker";
        public const string Containers = "Containers";
        public const string Container = "Container";
        public const string ContainerName = "Name";
        public const string ContainerNameAttribute = "ContainerName";
        public const string AccountNameAttribute = "AccountName";
        public const string LastModified = "Last-Modified";
        public const string Etag = "Etag";
        public const string Url = "Url";
        public const string CommonPrefixes = "CommonPrefixes";
        public const string ContentType = "Content-Type";
        public const string ContentEncoding = "Content-Encoding";
        public const string ContentLanguage = "Content-Language";
        public const string ContentLength = "Content-Length";
        public const string ContentSize = "x-ms-content-size";
        public const string ContentMD5 = "Content-MD5";
        public const string Range = "Range";
        public const string Size = "Size";
        public const string Blobs = "Blobs";
        public const string Blob = "Blob";
        public const string BlobName = "Name";
        public const string BlobProperties = "Properties";
        public const string BlobPrefix = "BlobPrefix";
        public const string BlobPrefixName = "Name";
        public const string BlobType = "BlobType";
        public const string LeaseStatus = "LeaseStatus";
        public const string Name = "Name";

        public const string Metadata = "Metadata";
        public const string MetaName = "name";

        // Error specific constants
        public const string ErrorRootElement = "Error";
        public const string ErrorCode = "Code";
        public const string ErrorMessage = "Message";
        public const string ErrorException = "ExceptionDetails";
        public const string ErrorExceptionMessage = "ExceptionMessage";
        public const string ErrorExceptionStackTrace = "StackTrace";
        public const string AuthenticationErrorDetail = "AuthenticationErrorDetail";

        public const string SharedKeyAuthSchemeName = "SharedKey";
        public const string RangeHeaderFormat = "bytes={0}-{1}";
        public const string RangeHeader = "x-ms-range";
        public const string MetaNameHeader = "x-ms-meta-name";
        public const string VersionHeader = "x-ms-version";
        public const string ApiVersion = "2019-02-02";
        public const String DateHeader = "x-ms-date";

        //URL Type
        public const int EndPoint = 1;
        public const int CDN = 2;
        public const int DOMAIN = 3;

        //
        public const int KB = 1024;
        public const int MB = 1024 * KB;
        public const int MaxBlobSize = 64 * MB;
        public const int BlockSize = 4 * MB;

        public const string IS_ADVANCED = "advanced";

        public static readonly string BLOCK_ID = "BLOCKID".ToLower(CultureInfo.InvariantCulture);

        public static readonly string RESTYPE = "RESTYPE".ToLower(CultureInfo.InvariantCulture);

    }

    class MSAzureKeyValueParams
    {
        public static readonly string RESTYPE = "RESTYPE".ToLower(CultureInfo.InvariantCulture);

        //List Containers Key
        public static readonly string COMP = "COMP".ToLower(CultureInfo.InvariantCulture);

        //List Containers value
        public static readonly string LIST = "LIST".ToLower(CultureInfo.InvariantCulture);

        //List MaxResults
        public static readonly string MAXRESULTS = "MAXRESULTS".ToLower(CultureInfo.InvariantCulture);
    }
}
