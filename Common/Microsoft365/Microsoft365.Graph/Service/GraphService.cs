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
namespace Microsoft365.Graph.Service;
public class GraphService
{
    internal GraphServiceClient Client { get; private set; }
    internal GraphServiceClient SecurityClient { get; private set; }
    internal GraphBeta.GraphServiceClient BetaClient { get; private set; }
    internal HttpClient HttpClient { get; private set; }

    public GraphService(string apiRoot, IATokenProviderBase? provider) : this(apiRoot, provider, null)
    {
    }
    public GraphService(string apiRoot, IATokenProviderBase? provider, MiddlewareBuilder? middlewareBuilder)
    {
        apiRoot.ThrowIfNullOrEmpty();

        //NEVER call client.HttpProvider.Dispose()
        var builder = new GraphClientBuilder(new Uri(apiRoot)).
            WithMiddleware(middlewareBuilder).
            WithUserAgent("opus");//tudo:
        //WithUserAgent(M365Configuration.UserAgent);
        Client = builder.WithTokenProvider(provider).Create();
        BetaClient = builder.WithTokenProvider(provider).CreateBeta();
        //httpclient is used to download file content with a preauthenticated download URL
        HttpClient = builder.CreateHttpClient(preAuthenticated: false);
        SecurityClient = builder.WithTokenProviderForSecurityService(provider).Create();
        Init(Client, BetaClient, HttpClient,SecurityClient);
    }

    [MemberNotNull(nameof(Drives), nameof(Sites), nameof(Users), nameof(Groups), nameof(Mails), nameof(Teams), nameof(Lists), nameof(Chats), nameof(Security))]
    private void Init(GraphServiceClient client, GraphBeta.GraphServiceClient betaClient, HttpClient httpClient, GraphServiceClient securityClient)
    {
        Drives = new GraphDriveService(client, httpClient);
        Sites = new GraphSiteService(client);
        Lists = new GraphListService(client);
        Users = new GraphUserService(client, betaClient);
        Groups = new GraphGroupService(client);
        Mails = new GraphMailService(client, betaClient);
        Teams = new GraphTeamService(client);
        Chats = new GraphChatService(client);
        Security = new GraphSecurityService(securityClient);
    }

    public GraphDriveService Drives { get; private set; }
    public GraphSiteService Sites { get; private set; }
    public GraphListService Lists { get; private set; }

    public GraphUserService Users { get; private set; }

    public GraphGroupService Groups { get; private set; }

    public GraphMailService Mails { get; private set; }
    public GraphTeamService Teams { get; private set; }
    public GraphChatService Chats { get; private set; }

    public GraphSecurityService Security { get; private set; }
}