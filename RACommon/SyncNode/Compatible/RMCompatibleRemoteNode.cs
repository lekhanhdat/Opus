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
using AvePoint.GCommon.Contract.Server.Common.Monitor.Object.Detail;
using Cloud.Sdk.Data.AosModern;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common.SyncNode.Compatible
{
    public class RMCompatibleRemoteNode
    {
        public string Id { get; set; }

        public string DomainName { get; set; }

        public string UserName { get; set; }

        public string ProfileUserName { get; set; }

        public string Name { get; set; }

        public string Url { get; set; }

        public string ParentId { get; set; }

        public string ParentName { get; set; }

        public string TemplateName { get; set; }

        public string TemplateTitle { get; set; }

        public bool IsPublicWebSite { get; set; }

        public string SPVersion { get; set; }

        public ConnectionType ConnectionType { get; set; }

        public SiteCollectionType SiteCollectionType { get; set; }

        public RemoteNodeType NodeType { get; set; }

        public IdentityProviderType AppProfileType { get; set; }

        public RemoteNodeStatus State { get; set; }

        public O365GroupType GroupType { get; set; }

        public string TenantId { get; set; }

        public string AdminUrl { get; set; }

        public string ObjectId { get; set; }

        public string ExternalId { get; set; }

        public string DisplayName { get; set; }

        public ChannelType ChannelType { get; set; }

        public MailboxType MailType { get; set; }

        public RemoteObjectType ObjectType { get; set; }
    }

    public class RMCompatibleRemoteNodeConverter
    {

        public static List<RMCompatibleRemoteNode> Convert(RemoteNodesResult nodes)
        {
            var res = new List<RMCompatibleRemoteNode>();

            var o365GroupNodes = Convert(nodes.O365Groups);
            res.AddRange(o365GroupNodes);

            var spSiteNodes = Convert(nodes.SPSites);
            res.AddRange(spSiteNodes);

            var oneDriveNodes = Convert(nodes.OneDrives);
            res.AddRange(oneDriveNodes);

            var projectNodes = Convert(nodes.ProjectSites);
            res.AddRange(projectNodes);

            var channelNodes = Convert(nodes.Channels);
            res.AddRange(channelNodes);

            var mailboxNodes = Convert(nodes.Mailboxes);
            res.AddRange(mailboxNodes);

            return res;
        }

        public static List<RMCompatibleRemoteNode> Convert(IEnumerable<Cloud.Sdk.Data.Aos.Tenant.RemoteNode> nodes)
        {
            return nodes.ToList().ConvertAll(item => new RMCompatibleRemoteNode
            {
                Id = item.Id,
                Name = item.Name,
                DomainName = item.DomainName,
                UserName = item.UserName,
                Url = item.Url,
                ParentId = item.ParentId,
                ParentName = item.ParentName,
                State = (RemoteNodeStatus)item.State,
                TemplateName = item.TemplateName,
                TemplateTitle = item.TemplateTitle,
                IsPublicWebSite = item.IsPublicWebSite,
                SPVersion = item.SPVersion,
                SiteCollectionType = (SiteCollectionType)item.SiteCollectionType,
                NodeType = (RemoteNodeType)item.NodeType,
                TenantId = item.TenantId,
                AdminUrl = item.AdminUrl,
                ObjectType = (RemoteObjectType)item.ObjectType,
                AppProfileType = (IdentityProviderType)item.AppProfileType,
                ConnectionType = (ConnectionType)item.ConnectionType,
                ProfileUserName = item.ProfileUserName,
                ExternalId = item.ExternalId,
                MailType = (MailboxType)item.MailType,
                ObjectId = item.ObjectId,
                DisplayName = item.DisplayName,
                ChannelType = (ChannelType)item.ChannelType,
                GroupType = (O365GroupType)item.O365GroupType,
            });
        }

        public static List<RMCompatibleRemoteNode> Convert(IEnumerable<RemoteNode> nodes)
        {
            var res = new List<RMCompatibleRemoteNode>();

            nodes.ForEach(item =>
            {
                if (item is O365GroupRemoteNode o365GroupNode)
                {
                    res.Add(Convert(new List<O365GroupRemoteNode>() { o365GroupNode }).First());
                }
                else if (item is SiteRemoteNode spSiteNode)
                {
                    res.Add(Convert(new List<SiteRemoteNode> { spSiteNode }).First());
                }
                else if (item is ChannelRemoteNode channelNode)
                {
                    res.Add(Convert(new List<ChannelRemoteNode> { channelNode }).First());
                }
                else if (item is MailboxRemoteNode mailboxNode)
                {
                    res.Add(Convert(new List<MailboxRemoteNode> { mailboxNode }).First());
                }
            });

            return res;
        }

        public static List<RMCompatibleRemoteNode> Convert(IEnumerable<O365GroupRemoteNode> nodes)
        {
            return nodes.ConvertAll(item => new RMCompatibleRemoteNode
            {
                Id = item.Id,
                UserName = item.ServiceAccountUsername,
                Name = item.DisplayName,
                ProfileUserName = item.AppProfileUsername,
                Url = item.SiteUrl,
                ParentId = item.ParentId,
                ParentName = item.ContainerName,
                TemplateName = item.TemplateName,
                TemplateTitle = item.TemplateTitle,
                SPVersion = item.SPVersion,
                ConnectionType = item.ConnectionType,
                NodeType = RemoteNodeType.Office365Group,
                AppProfileType = item.AppProfileType,
                TenantId = item.TenantId,
                AdminUrl = item.AdminUrl,
                ObjectId = item.ObjectId,
                ExternalId = item.ObjectId,
                DisplayName = item.DisplayName,
                GroupType = item.GroupType
            }).ToList();
        }

        public static List<RMCompatibleRemoteNode> Convert(IEnumerable<SiteRemoteNode> nodes)
        {
            return nodes.ConvertAll(item => new RMCompatibleRemoteNode
            {
                Id = item.Id,
                DomainName = item.DomainName,
                UserName = item.ServiceAccountUsername,
                ProfileUserName = item.AppProfileUsername,
                Name = item.Name,
                Url = item.Url,
                ParentId = item.ParentId,
                ParentName = item.ContainerName,
                TemplateName = item.TemplateName,
                TemplateTitle = item.TemplateTitle,
                SPVersion = item.SPVersion,
                ConnectionType = item.ConnectionType,
                NodeType = item.NodeType,
                AppProfileType = item.AppProfileType,
                TenantId = item.TenantId,
                AdminUrl = item.AdminUrl,
                ObjectId = item.ObjectId,
                ExternalId = string.Empty,
                State = item.State,
                IsPublicWebSite = item.IsPublicWebSite,
            }).ToList();
        }

        public static List<RMCompatibleRemoteNode> Convert(IEnumerable<ChannelRemoteNode> nodes)
        {

            return nodes.ConvertAll(item => new RMCompatibleRemoteNode
            {
                Id = item.Id,
                UserName = item.ServiceAccountUsername,
                ProfileUserName = item.AppProfileUsername,
                Name = item.Name,
                Url = item.SiteUrl,
                ParentId = item.ParentId,
                ParentName = item.ContainerName,
                ConnectionType = item.ConnectionType,
                NodeType = RemoteNodeType.Channel,
                AppProfileType = item.AppProfileType,
                TenantId = item.TenantId,
                AdminUrl = item.AdminUrl,
                ObjectId = item.ChannelId,
                ExternalId = item.ChannelId,
                ChannelType = item.ChannelType
            }).ToList();
        }

        public static List<RMCompatibleRemoteNode> Convert(IEnumerable<MailboxRemoteNode> nodes)
        {
            return nodes.ConvertAll(item => new RMCompatibleRemoteNode
            {
                Id = item.Id,
                UserName = item.ServiceAccountUsername,
                ProfileUserName = item.AppProfileUsername,
                Name = item.Name,
                Url = item.Name,
                ParentId = item.ParentId,
                ParentName = item.ContainerName,
                ConnectionType = item.ConnectionType,
                NodeType = RemoteNodeType.Mailbox,
                AppProfileType = item.AppProfileType,
                TenantId = item.TenantId,
                AdminUrl = item.AdminUrl,
                ObjectId = item.ObjectId,
                ExternalId = string.Empty,
                State = item.State,
                MailType = item.MailboxType,
                ObjectType = item.ObjectType,
            }).ToList();
        }

    }
}
