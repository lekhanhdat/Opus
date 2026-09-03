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


namespace Microsoft365.SharePoint.CSOM
{
    using Microsoft.SharePoint.Client;
    using Microsoft365.Authentication.TokenProvider;
    using Microsoft365.Common.Logger;
    using Microsoft365.Configuration;
    using System.IO;
    using System.Threading.Tasks;

    public class FileService
    {

        private const int RETRYCOUNT_DEFAULT = 3;
        private const int RETRYINTERVAL_DEFAULT = 5000;
        protected string USERAGENT { get; set; } = Microsoft365Configuration.CommonConfiguration.UserAgent;
        public string WebUrl { get; private set; }
        protected IATokenProvider TokenProvider { get; set; }
        protected RetryableClientContextFactory ClientContextFactory { get; set; }
        public FileService(string webUrl, IATokenProvider tokenProvider)
        {
            this.WebUrl = webUrl;
            this.TokenProvider = tokenProvider;
            this.ClientContextFactory = new RetryableClientContextFactory(USERAGENT, tokenProvider, RETRYCOUNT_DEFAULT, RETRYINTERVAL_DEFAULT);
        }

        private RetryableClientContext CreateContext()
        {
            return ClientContextFactory.GetClientContext(this.WebUrl);
        }

        public async Task DownloadFileAsync(string serverRelativeUrl, Stream target, SPOpenBinaryOptions options = SPOpenBinaryOptions.None)
        {
            using (var context = CreateContext())
            {
                var file = context.Web.GetFileByServerRelativeUrl(serverRelativeUrl);
                var stream = file.OpenBinaryStreamWithOptions(options);
#if DEBUG
                context.Load(file);
#endif
                await context.ExecuteQueryAsync();
                await stream.Value.CopyToAsync(target);
            }
        }

        public async Task DownloadFileVersionAsync(string serverRelativeUrl, int version, Stream target, SPOpenBinaryOptions options = SPOpenBinaryOptions.None)
        {
            using (var context = CreateContext())
            {
                var file = context.Web.GetFileByServerRelativeUrl(serverRelativeUrl);
                var versionFile = file.Versions.GetById(version);
                var stream = versionFile.OpenBinaryStreamWithOptions(options);
#if DEBUG
                context.Load(file, f => f.Versions);
                context.Load(versionFile);
#endif
                await context.ExecuteQueryAsync();
                await stream.Value.CopyToAsync(target);
            }
        }
    }
}
