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
using System.IO;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using NLog;

namespace CloudRecordDownloadManager.Utils.Http {

    public abstract class HttpExecutor {

        readonly static Logger _logger = LogManager.GetCurrentClassLogger();

        public delegate void UpdateMaxFileSize(long contentLength);

        public delegate void UpdateFileSize(long contentLength);

        public static async Task<DownloadResult> Download(string url, string destination, UpdateMaxFileSize updateMaxFileSize, UpdateFileSize updateFileSize = null) {
            try {
                using (var httpClient = new HttpClient() {Timeout = TimeSpan.FromSeconds(10)}) {
                    //if (!string.IsNullOrEmpty(token))
                    //{
                    //    httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
                    //}

                    /*var url = requestOptions.url.s*/
                    ;
                    var request = new HttpRequestMessage {RequestUri = new Uri(url)};
                    var httpResponseMessage =
                        await httpClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

                    if (httpResponseMessage.StatusCode >= HttpStatusCode.BadRequest) {
                        _logger.Warn($"Cannot download ${url}, status ${httpResponseMessage.StatusCode}, Reason: ${httpResponseMessage.ReasonPhrase}");

                        return new DownloadResult {
                            Status = 0,
                            Msg = $"Cannot download ${url}, status ${httpResponseMessage.StatusCode}"
                        };
                    }

                    var contentLength = httpResponseMessage.Content.Headers.ContentLength;
                    if (contentLength != null) {
                        updateMaxFileSize((long) contentLength);
                    }

                    long downloadSize = 0;

                    using (var stream = await httpResponseMessage.Content.ReadAsStreamAsync()) {
                        const int readLength = 1024 * 1024 * 10;
                        var bytes = new byte[readLength];
                        using (var fs = new FileStream(destination, FileMode.Append, FileAccess.Write)) {
                            int writeLength;
                            while ((writeLength = await stream.ReadAsync(bytes, 0, readLength)) > 0) {
                                try {
                                    fs.Write(bytes, 0, writeLength);
                                    downloadSize += writeLength;
                                } catch (Exception e) {
                                    Console.WriteLine(e);
                                    throw;
                                }

                                updateFileSize?.Invoke(downloadSize);
                            }
                        }
                    }

                    if (downloadSize == contentLength) {
                        _logger.Warn($"Download ${url} successfully.");
                        return new DownloadResult {
                            Status = 1,
                            Destination = destination,
                            ContentLength = (long) contentLength,
                            DownloadSize = downloadSize,
                        };
                    }
                }

                return new DownloadResult {
                    Status = 0,
                    Destination = destination
                };
            } catch (Exception e) {
                _logger.Error(url);
                _logger.Error(e);
                return new DownloadResult {
                    Status = 0,
                    Destination = destination,
                    Msg = e.Message
                };
            }
        }

    }

}