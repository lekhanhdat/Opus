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

namespace Microsoft365.Graph.Core;

public class GraphClientBuilder
{
    //private static readonly ICloudBackupLogger logger = CloudBackupLogManager.Get(typeof(GraphClientBuilder));
    private readonly Uri baseUri;
    private IAuthenticationProvider? provider;
    private IAccessTokenProvider? tokenProvider;
    private MiddlewareBuilder middlewareBuilder;
    private TimeSpan timeout;
    private string? userAgent;
    private static readonly HttpMessageHandler finalHandler;

    static GraphClientBuilder()
    {
        finalHandler = new SocketsHttpHandler()
        {
            AllowAutoRedirect = false,
            AutomaticDecompression = System.Net.DecompressionMethods.None,
            PooledConnectionLifetime = TimeSpan.FromMinutes(60),
            ConnectTimeout = TimeSpan.FromMinutes(6),
#if DEBUG
            PlaintextStreamFilter = (context, token) =>
            {
                //logger.Info($"Tcp connection established,InitialRequest:{context.InitialRequestMessage.RequestUri},InitRequestVersion:{context.InitialRequestMessage?.Version},{context.InitialRequestMessage?.VersionPolicy},NegotiatedHttpVersion:{context.NegotiatedHttpVersion}");
                return ValueTask.FromResult(context.PlaintextStream);
            },
            //Talk to qinglong.luo@avepoint.com if you want to set custom callback
            //ConnectCallback = async (ctx, ct) =>
            //{
            //    var s = new Socket(SocketType.Stream, ProtocolType.Tcp) { NoDelay = true };
            //    try
            //    {
            //        s.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            //        s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 60);
            //        s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 5);
            //        //s.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 5);

            //        //Currently the callback does not aware if it is a async request or not, and always call async connect
            //        //https://github.com/dotnet/runtime/issues/44876
            //        //if (async)
            //        //{
            //        await s.ConnectAsync(ctx.DnsEndPoint, ct).ConfigureAwait(false);
            //        //}
            //        //else
            //        //{
            //        //    using (ct.UnsafeRegister(static s => ((Socket)s!).Dispose(), s))
            //        //    {
            //        //        s.Connect(ctx.DnsEndPoint);
            //        //    }
            //        //}
            //        return new NetworkStream(s, ownsSocket: true);
            //    }
            //    catch
            //    {
            //        s.Dispose();
            //        throw;
            //    }
            //}
#endif
            //EnableMultipleHttp2Connections = true,
        }.ConfigureCallBack();
    }

    /// <summary>
    /// Initializes a new instance of the GraphClientBuilder class with the specified base URI.
    /// </summary>
    /// <param name="baseUri">The base URI for the Graph API endpoint.</param>
    /// <remarks>
    /// The GraphVersion.V1 will be appended to the base URI automatically.
    /// Default timeout is set to 1 hour, and default middleware is GraphMiddlewareBuilder.Default.
    /// </remarks>
    public GraphClientBuilder(Uri baseUri)
    {
        this.baseUri = baseUri;
        middlewareBuilder = MiddlewareBuilder.Default;
        timeout = TimeSpan.FromHours(1);
    }

    /// <summary>
    /// Sets the timeout for requests made by the GraphServiceClient.
    /// </summary>
    /// <param name="timeout">The timeout duration for HTTP requests. If null, the current timeout is not changed.</param>
    /// <returns>The current GraphClientBuilder instance to enable method chaining.</returns>
    public GraphClientBuilder WithTimeout(TimeSpan? timeout)
    {
        if (timeout is not null)
        {
            this.timeout = timeout.Value;
        }
        return this;
    }

    /// <summary>
    /// Sets the User-Agent header for requests made by the GraphServiceClient.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to be included in request headers. If null, the current User-Agent is not changed.</param>
    /// <returns>The current GraphClientBuilder instance to enable method chaining.</returns>
    public GraphClientBuilder WithUserAgent(string? userAgent)
    {
        if (userAgent is not null)
        {
            this.userAgent = userAgent;
        }
        return this;
    }

    /// <summary>
    /// Add middlewares
    /// </summary>
    /// <param name="middlewareBuilder">
    /// null:  no middlewares
    /// GraphMiddlewareBuilder.Default: default middlewares
    /// other: custom middlewares
    /// </param>
    /// <returns></returns>
    public GraphClientBuilder WithMiddleware(MiddlewareBuilder? middlewareBuilder)
    {
        if (middlewareBuilder is not null)
        {
            this.middlewareBuilder = middlewareBuilder;
        }
        return this;
    }

    /// <summary>
    /// Sets the token provider for authenticating requests to the Microsoft Graph API.
    /// </summary>
    /// <param name="provider">The token provider that will supply authentication tokens. If null, the current provider is not changed.</param>
    /// <returns>The current GraphClientBuilder instance to enable method chaining.</returns>
    public GraphClientBuilder WithTokenProvider(IATokenProviderBase? provider)
    {
        if (provider is not null)
        {
            var tmp = provider.ToAuthenticationProvider();
            this.tokenProvider = tmp.AccessTokenProvider;
            this.provider = tmp;
        }
        return this;
    }
    
    public GraphClientBuilder WithTokenProviderForSecurityService(IATokenProviderBase? provider)
    {
        if (provider is not null)
        {
            var tmp = provider.ToAuthenticationProviderForSecurityService();
            this.tokenProvider = tmp.AccessTokenProvider;
            this.provider = tmp;
        }
        return this;
    }

    /// <summary>
    /// Creates a GraphServiceClient instance with the configured settings.
    /// </summary>
    /// <param name="preAuthenticated">
    /// When true, automatically includes authentication in requests via the AuthenticationHandler middleware.
    /// When false, authentication must be handled separately.
    /// </param>
    /// <returns>A configured GraphServiceClient instance ready for use.</returns>
    /// <remarks>
    /// CAUTION: Never call client.HttpProvider.Dispose() as it will dispose the shared HTTP handler.
    /// </remarks>
    public GraphServiceClient Create(bool preAuthenticated = true)
    {
        var client = CreateHttpClient(preAuthenticated);
        //client.HttpProvider.Dispose() will dispose finalHandler, GraphClientFactory does not provide a way to construct httpclient with disposeHandler=false 
        return new GraphServiceClient(client, provider, new Uri(baseUri, GraphVersion.V1).ToString());
    }

    public GraphBeta.GraphServiceClient CreateBeta(bool preAuthenticated = true)
    {
        var client = CreateHttpClient(preAuthenticated);
        //client.HttpProvider.Dispose() will dispose finalHandler, GraphClientFactory does not provide a way to construct httpclient with disposeHandler=false 
        return new GraphBeta.GraphServiceClient(client, provider, new Uri(baseUri, GraphVersion.Beta).ToString());
    }

    /// <summary>
    /// Create HttpClient with predefined settings.
    /// </summary>
    /// <param name="preAuthenticated">
    /// True: add AuthenticationHandler middleware which include the bearer token in request header.
    /// False: not pre authentication, you need to do it by yourself.
    /// </param>
    /// <returns></returns>
    public HttpClient CreateHttpClient(bool preAuthenticated = true)
    {
        var handlers = middlewareBuilder.GetMiddlewares(preAuthenticated ? this.tokenProvider : null);
        var client = GraphClientFactory.Create(handlers, "v1.0", "Global", null, finalHandler, false);
        client.Timeout = timeout;
        if (userAgent is not null)
        {
            client.DefaultRequestHeaders.UserAgent.TryParseAdd(userAgent);
        }
        return client;
    }
}