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

namespace Microsoft365.SharePoint.Rest
{
    using Microsoft.SharePoint.Client;
    using Microsoft365.Authentication.TokenProvider;
    using System;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;

    public class FileService
    {
        public string SiteUrl { get; private set; }
        private readonly SharePointRestExecutor executor;
        const int TimeoutInHours = 24;

        /// <summary>
        /// </summary>
        /// <param name="siteUrl">Target site url</param>
        /// <param name="tokenProvider">Token provider to obtain access token. If user token is used, it requires SharePoint admin and site admin permission.</param>
        internal FileService(string siteUrl, IATokenProvider tokenProvider)
        {
            this.SiteUrl = siteUrl.TrimEnd('/');
            this.executor = new SharePointRestExecutor(this.SiteUrl, tokenProvider, true);
            this.executor.MaxDataServiceVersion = "3.0";
        }

        public async Task DownloadFileAsync(Guid fileId, Stream target, OpenBinaryOptions options = OpenBinaryOptions.None)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromHours(TimeoutInHours));
            await this.executor.DownloadStreamRequestAsync(ToRestUri(fileId, options), target, null, cts.Token);
        }
        //Does not work well for large file, use DownloadFileAsync instead
        //public async Task<Stream> OpenFileAsync(Guid fileId, SPOpenBinaryOptions options = SPOpenBinaryOptions.None)
        //{
        //    return await this.executor.GetStreamRequestAsync(ToRestUri(fileId, options));
        //}

        public async Task DownloadFileVersionAsync(Guid fileId, int version, Stream target, OpenBinaryOptions options = OpenBinaryOptions.None)
        {
            using var cts = new CancellationTokenSource(TimeSpan.FromHours(TimeoutInHours));
            await this.executor.DownloadStreamRequestAsync(ToRestUri(fileId, version, options), target, null, cts.Token);
        }
        
        //Does not work well for large file, use DownloadFileAsync instead
        //public async Task<Stream> OpenFileVersionAsync(Guid fileId, int version, SPOpenBinaryOptions options = SPOpenBinaryOptions.None)
        //{
        //    return await this.executor.GetStreamRequestAsync(ToRestUri(fileId, version, options));
        //}

        private Uri ToRestUri(Guid fileId, OpenBinaryOptions options)
        {
            return new Uri($"{this.SiteUrl}/_api/web/getfilebyid('{fileId}')/OpenBinaryStreamWithOptions({(int)options})");
        }
        private Uri ToRestUri(Guid fileId, int version, OpenBinaryOptions options)
        {
            return new Uri($"{this.SiteUrl}/_api/web/getfilebyid('{fileId}')/versions({version})/OpenBinaryStreamWithOptions({(int)options})");
        }

    }

    public enum OpenBinaryOptions
    {
        None = 0x0,
        Unprotected = 0x2,
        SkipVirusScan = 0x4,
        MinimizeProcessing = 0x1,
        GetAsZipWithAltStreamsIfAvailable = 0x100000
    }
}
