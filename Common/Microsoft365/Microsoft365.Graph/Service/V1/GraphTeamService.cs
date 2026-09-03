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
using System.Text.RegularExpressions;

namespace Microsoft365.Graph.Service;

public class GraphTeamService
{
    private readonly GraphServiceClient client;
    private static readonly string[] PARA_TEAM_CHANNEL_FILES_FOLDER_SELECT = new string[] { "id", "name", "webUrl", "parentReference" };
    private static readonly string[] PARA_TEAM_CHANNEL_SELECT = new string[] { "id", "displayName","membershipType" };
    private static readonly string[] PARA_TEAM_CHANNEL_TAB_SELECT = new string[] { "id", "configuration", "displayName" };

    internal GraphTeamService(GraphServiceClient client)
    {
        this.client = client;
    }

    [GraphAPI("/teams/{GroupTeamId}/channels")]
    public IAsyncEnumerable<Channel> ListChannelsAsync(string groupTeamId, CancellationToken cancellationToken = default)
    {

        return client.GetAllAsync<Channel, ChannelCollectionResponse>(
          () =>
            client.
            Teams[groupTeamId].
            Channels.
            GetAsync(config =>
                {
                    config.Headers.Add("Prefer", "include-unknown-enum-members");
                    config.QueryParameters.Select = PARA_TEAM_CHANNEL_SELECT;
                }, cancellationToken),
          cancellationToken);
    }

    [GraphAPI("/teams/{GroupTeamId}/channels/{ChannelId}/filesFolder")]
    public async ValueTask<DriveItem?> GetChanelFilesFolderAsync(string groupTeamId, String ChannelId, CancellationToken cancellationToken = default)
    {
        return await client.Teams[groupTeamId].Channels[ChannelId].FilesFolder.GetAsync(config =>
        {
            config.QueryParameters.Select = PARA_TEAM_CHANNEL_FILES_FOLDER_SELECT;
        }, cancellationToken);
    }

    [GraphAPI("/teams/{GroupTeamId}/channels/{ChannelId}/tabs?$expand=teamsApp")]
    public IAsyncEnumerable<TeamsTab> ListTabsAsync(string groupTeamId, string channelId, CancellationToken cancellationToken = default)
    {
        return client.GetAllAsync<TeamsTab, TeamsTabCollectionResponse>(
          () =>
            client.
            Teams[groupTeamId].
            Channels[channelId].
            Tabs.
            GetAsync(config =>
            {
                config.Headers.Add("Prefer", "include-unknown-enum-members");
                config.QueryParameters.Select = PARA_TEAM_CHANNEL_TAB_SELECT;
                config.QueryParameters.Expand = new string[] { "teamsApp" };
            }, cancellationToken),
          cancellationToken);
    }

    [GraphAPI("/teams/{GroupTeamId}/channels/{ChannelId}/tabs/{TabId}")]
    public async ValueTask<TeamsTab?> UpdateTabAsync(string groupTeamId, string channelId, TeamsTab tab, CancellationToken cancellationToken = default)
    {
        return await client
            .Teams[groupTeamId]
            .Channels[channelId]
            .Tabs[tab.Id]
            .PatchAsync(tab, null, cancellationToken);
    }

    [GraphAPI("/teams")]
    public IAsyncEnumerable<Team> ListTeamsAsync(CancellationToken cancellationToken = default)
    {
        return client.GetAllAsync<Team, TeamCollectionResponse>(() => client.Teams.GetAsync(null, cancellationToken), cancellationToken);
    }

    [GraphAPI("/teams")]
    public async Task<string> CreateTeamsAsync(Team teams)
    {
        var nativeResponseHandler = new NativeResponseHandler();
        var result = await client.Teams.PostAsync(teams, requestConf =>
        {
            requestConf.Options.Add(new ResponseHandlerOption { ResponseHandler = nativeResponseHandler });
        });

        using var responseMessage = nativeResponseHandler.Value as HttpResponseMessage;
        if (responseMessage != null)
        {
            if (responseMessage.IsSuccessStatusCode)
            {
                var location = responseMessage.Headers.Location;
                if (location != null)
                {
                    var match = Regex.Match(location.ToString(), @"/teams\('([0-9a-fA-F-]{36})'\)");
                    if (match.Success)
                    {
                        return match.Groups[1].Value;
                    }
                }
            }
            else
            {
                var err = await responseMessage.Content.ReadAsStringAsync();
                throw new ApiException(err);
            }
        }
        return string.Empty;
    }

    [GraphAPI("/teams?$top=1")]
    public async Task<Team?> GetOneTeamAsync(CancellationToken cancellationToken = default)
    {
        var response = await client.Teams.GetAsync((request) =>
        {
            request.QueryParameters.Top = 1;
        }, cancellationToken);
        return response?.Value?.FirstOrDefault();
    }
}
