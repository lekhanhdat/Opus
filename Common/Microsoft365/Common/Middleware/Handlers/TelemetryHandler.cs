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
#nullable enable
using System.Text.Json;
using System;

namespace Microsoft365.Common.Middleware.Handlers;
public class TelemetryHandler : DelegatingHandler
{
    //private static readonly ICloudBackupLogger logger = CloudBackupLogManager.Get(typeof(TelemetryHandler));
    private static readonly bool logSuccessResponse =
#if DEBUG
    string.Equals(Environment.GetEnvironmentVariable("GRAPH_TELEMETRY_LOG_SUCCESS_RESPONSE"), "true", StringComparison.OrdinalIgnoreCase);
#else
    false;
#endif

    public TelemetryOption Option { get; set; }

    public TelemetryHandler() : this(new()) { }

    public TelemetryHandler(TelemetryOption option)
    {
        Option = option;
    }

    protected override HttpResponseMessage Send(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = new Context(request)
        {
            OnSuccessResponse = Option.OnSuccessResponse,
            OnErrorResponse = Option.OnErrorResponse,
        };
        try
        {
            return context.Response = base.Send(request, cancellationToken);
        }
        catch (System.Exception ex)
        {
            context.Error = ex;
            throw;
        }
        finally
        {
            Log(context);
        }
    }

    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var context = new Context(request)
        {
            OnSuccessResponse = Option.OnSuccessResponse,
            OnErrorResponse = Option.OnErrorResponse,
        };
        try
        {
            return context.Response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
        catch (System.Exception ex)
        {
            context.Error = ex;
            throw;
        }
        finally
        {
            Log(context);
        }
    }

    private void Log(Context context)
    {
        if (!context.IsSuccessResponse)//Failed respnse status code >=400
        {
            //logger.Warn($"FailingRequestTelemetry {context}");
            return;
        }
        if (context.IsRedrectResponse)//Redrect response, status code [300,400)
        {
            //logger.Warn($"RedrectRequestTelemetry {context}");
            return;
        }
        if (string.IsNullOrEmpty(context.Request.GetClassification()))
        {
            //logger.Info($"UnClassifiedRequestTelemetry {context}");
            return;
        }
        if (logSuccessResponse) //others
        {
            //logger.Info($"SuccessRequestTelemetry {context}");
            return;
        }
    }

    class Context
    {


        private static readonly bool logRequestBody =
#if DEBUG
            string.Equals(Environment.GetEnvironmentVariable("GRAPH_TELEMETRY_LOG_REQUEST_BODY"), "true", StringComparison.OrdinalIgnoreCase);
#else
            false;
#endif
        public Action<HttpResponseMessage>? OnSuccessResponse { get; set; }
        public Action<HttpResponseMessage?, System.Exception?>? OnErrorResponse { get; set; }

        public bool IsSuccessResponse { get; private set; } = false;
        public bool IsRedrectResponse { get; private set; } = false;

        internal bool LogDetailInfo =>
#if DEBUG
            string.Equals(Environment.GetEnvironmentVariable("GRAPH_TELEMETRY_LOG_RESPONSE_BODY"), "true", StringComparison.OrdinalIgnoreCase) ||
#endif
            IsRedrectResponse || !IsSuccessResponse;


        private HttpResponseMessage? response;
        public HttpResponseMessage? Response
        {
            get { return response; }
            set
            {
                if (value is not null)
                {
                    response = value;
                    if ((int)response.StatusCode < 400)//set redirect as success
                    {
                        if ((int)response.StatusCode >= 300) //[300,400)
                        {
                            IsRedrectResponse = true;
                        }
                        IsSuccessResponse = true;
                        OnSuccessResponse?.Invoke(response);
                    }
                    else
                    {
                        OnErrorResponse?.Invoke(response, null);
                    }
                }

            }
        }

        private System.Exception? error;
        public System.Exception? Error
        {
            get { return error; }
            set
            {
                error = value;
                if (error is not null)
                {
                    OnErrorResponse?.Invoke(null, error);
                }
            }
        }
        public HttpRequestMessage Request { get; set; }
        private readonly Stopwatch watch;
        private readonly DateTime start;

        public Context(HttpRequestMessage request)
        {
            start = DateTime.UtcNow;
            Request = request;
            watch = Stopwatch.StartNew();
        }

        public override string ToString()
        {
            var builder = new StringBuilder();
            builder.AppendLine(FirstLine);
            var responseHeader = GetResponseHeaders(!LogDetailInfo);
            if (logRequestBody && "application/json".EqualsIgnoreCase(Request.Content?.Headers?.ContentType?.MediaType))
            {
                try
                {
                    var body = Request.Content!.ReadAsStringAsync().ExecuteAsyncTask();
                    builder.AppendLine(body.ToIndentedJson());
                }
                catch (InvalidOperationException)
                { // The stream was already consumed. It cannot be read again.
                }
            }
            if (!string.IsNullOrEmpty(responseHeader))
            {
                builder.Append(responseHeader);
            }
            if (Error is not null)
            {
                builder.Append(Error);
            }
            if (LogDetailInfo && response != null)
            {
                builder.AppendLine("Response Content:");
                try
                {
                    builder.AppendLine(DecompressContent(response).ConfigureAwait(false).GetAwaiter().GetResult());
                }
                catch (System.Exception ex)
                {
                    builder.Append($"Read response failed.Error:{ex}");
                }
            }
            return builder.ToString();
        }

        internal string FirstLine => string.Join(' ',
                Request.Method.ToString().PadRight(5),
                Request.RequestUri.RemoveSensitiveInfo(),
                "HTTP/" + (Response?.Version ?? Request.Version),
                (int?)Response?.StatusCode,
                Response?.ReasonPhrase,
                start.ToString("o"),
                watch.ElapsedMilliseconds + "ms",
                Error?.GetType().Name);

        internal string GetResponseHeaders(bool removeSensitive) =>
            Response?.Headers.ToFormatedString(removeSensitive) +
            Response?.TrailingHeaders.ToFormatedString(removeSensitive) +
            Response?.Content?.Headers.ToFormatedString(removeSensitive);

        private static async Task<string> DecompressContent(HttpResponseMessage response)
        {
            if (response.Content.IsCompress())
            {
                var bytes = await response.Content.ReadAsByteArrayAsync().ConfigureAwait(false);
                using var buffer = new MemoryStream();
                await new GZipStream(new MemoryStream(bytes), CompressionMode.Decompress).CopyToAsync(buffer).ConfigureAwait(false);
                buffer.Seek(0, SeekOrigin.Begin);
                return Encoding.UTF8.GetString(buffer.ToArray());
            }
            else
            {
                return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            }
        }
    }
}