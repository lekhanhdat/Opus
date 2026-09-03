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
using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.RACommonUtility.Browser
{
    public class ExchangeBrowser
    {

        private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        private static readonly IRMMailboxService MailboxService = PlatformWindsorManager.GetService<IRMMailboxService>();

        private static readonly Dictionary<NodeLevel, Func<ExchangeOnlineTreeNodeDto, List<ExchangeOnlineTreeNodeDto>>> NodeLevelMapping = new Dictionary<NodeLevel, Func<ExchangeOnlineTreeNodeDto, List<ExchangeOnlineTreeNodeDto>>>
        {
            {NodeLevel.Root, RootBrowse },
            {NodeLevel.ExchangeOnlineFarm, FarmBrowse },
            {NodeLevel.ExchangeOnlineMailboxGroup, WebApplicationBrowse }
        };

        public static ExchangeOnlineTreeMessage Browse(ExchangeOnlineTreeMessage message)
        {
            Logger.Info($"Start browse exchange tree, level: {message.Node.Level}.");
            if(!NodeLevelMapping.TryGetValue(message.Node.Level, out var browseFunc))
            {
                Logger.Error($"Nonsupport {message.Node.Level} exchange node browse children.");
                return null;
            }
            var children = browseFunc(message.Node);
            Logger.Info($"End browse exchange tree, level: {message.Node.Level}, children count: {children.Count}.");
            return new ExchangeOnlineTreeMessage
            {
                NodeList = children,
                ChildrenCount = children.Count,
                TreeType = TreeType.ExchangeOnlineArchiverTree
            };
        }

        private static List<ExchangeOnlineTreeNodeDto> RootBrowse(ExchangeOnlineTreeNodeDto node)
        {
            const string FarmName = "My Registered Mailboxes";
            var FarmDisplayName = I18NEntity.GetString("RM_JS_SPS_EXO_RootNode");
            return new List<ExchangeOnlineTreeNodeDto>
            {
                new ExchangeOnlineTreeNodeDto
                {
                    ID = Guid.NewGuid().ToString(),
                    Level = NodeLevel.ExchangeOnlineFarm,
                    Name = FarmName,
                    DisplayName = FarmDisplayName,
                    CanChildrenBeLoaded = true,
                    Offset = 0
                }
            };
        }

        private static List<ExchangeOnlineTreeNodeDto> FarmBrowse(ExchangeOnlineTreeNodeDto node)
        {
            var res = new List<ExchangeOnlineTreeNodeDto>();
            try
            {
                Logger.Info("Begin browse exchange all container.");
                var groups = MailboxService.GetRemoteMailGroupNodes();
                groups.Sort((node1, node2) => String.Compare(node1.NodeName, node2.NodeName, StringComparison.CurrentCultureIgnoreCase));
                int offset = 0;
                foreach (var group in groups)
                {
                    res.Add(new ExchangeOnlineTreeNodeDto
                    {
                        ID = group.NodeId,
                        Level = NodeLevel.ExchangeOnlineMailboxGroup,
                        Type = group.NodeLevel == NodeLevel.ExchangeOnlineO365GroupGroup ? NodeType.EOO365GroupGroup : NodeType.GenericList,
                        Name = group.NodeName,
                        DisplayName = GetDefaultGroupName(group.NodeName),
                        CanChildrenBeLoaded = true,
                        Offset = offset++
                    });
                }
                Logger.Info($"End browse exchange all container count: {res.Count}.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while browse exchange all container. Error: {e}");
            }
            return res;
        }

        private static List<ExchangeOnlineTreeNodeDto> WebApplicationBrowse(ExchangeOnlineTreeNodeDto node)
        {
            var res = new List<ExchangeOnlineTreeNodeDto>();
            try
            {
                Logger.Info($"Begin browse exchange email address by container id: {node.ID}");
                var emails = MailboxService.GetEmailsByEmailGroupIdForBrowse(node.ID);
                foreach (var email in emails)
                {
                    if (email.State == EmailAccountState.AccessAll)
                    {
                        res.Add(new ExchangeOnlineTreeNodeDto()
                        {
                            ObjectId = email.ObjectId,
                            ID = email.Id,
                            Name = email.Email,
                            DisplayName = email.Email,
                            MailboxType = email.MailboxType,
                            Level = NodeLevel.ExchangeOnlineMailbox,
                            Type = NodeType.EOMailBox,
                            O365TenantId = email.TenantId,
                        });
                    }
                }
                Logger.Info($"End browse exchange email address, count: {res.Count}.");
            }
            catch (Exception e)
            {
                Logger.Error($"An error occurred while browse exchange email address. Error: {e}");
            }
            return res;
        }

        private static string GetDefaultGroupName(string groupName)
        {
            if (string.Equals(groupName, RMConstants.DEFAULT_MAILBOX_GROUP))
            {
                return I18NEntity.GetString("RM_EXO_Default_Container");
            }
            else if (string.Equals(groupName, RMConstants.DEFAULT_O365_GROUPS_GROUP))
            {
                return "Default Microsoft 365 Group Mailbox Container";
            }
            return groupName;
        }
    }
}
