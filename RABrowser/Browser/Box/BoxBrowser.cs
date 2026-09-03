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
using AvePoint.RA.Common;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Box;
using AvePoint.RA.Contract.BoxBrowser;
using AvePoint.RA.Contract.Schedule;
using AvePoint.RA.DB.Dao;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RMNodeLevel = AvePoint.RA.Contract.RMWeb.Tree.Base.RMNodeLevel;


namespace AvePoint.RA.Browser.Browser.Box
{
    public class BoxBrowser
    {
        private static readonly RALogger logger = RALogger.GetInstance(typeof(BoxBrowser));

        private static IRMBoxBrowser BoxBrowserService = PlatformWindsorManager.GetService<IRMBoxBrowser>();

        private static IBoxSettingDao BoxSettingDao => PlatformWindsorManager.GetService<IBoxSettingDao>();

        private static IScheduleService ScheduleService => PlatformWindsorManager.GetService<IScheduleService>();

        public static RABoxBrowserContract GetRootNode()
        {
            try
            {
                var rootNode = BoxBrowserService.GetRootNode();
                return ConvertToBoxBrowserContract(rootNode);
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while attempting to browse box tree. Error: {ex}");
                return null;
            }
        }


        public static async Task<IEnumerable<RABoxBrowserContract>> GetChildrenWithSettingIcon(RABoxBrowserContract contract)
        {
            try
            {
                var boxTreeNode = ConvertToBoxTreeNode(contract);
                var childrenNodes = await BoxBrowserService.BrowseAsync(boxTreeNode);
                return childrenNodes.ConvertAll(node =>
                {
                    UpdateNodeIconStatus(contract, node);
                    return ConvertToBoxBrowserContract(node);
                });
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while attempting to browse box tree. Error: {ex}");
                return null;
            }
        }

        public static async Task<RABoxBrowserContract> BBrowserTreeByPager(RABoxBrowserContract contract)
        {
            try
            {
                List<RABoxBrowserContract> children = new List<RABoxBrowserContract>();
                var boxTreeNode = ConvertToBoxTreeNode(contract);
                var childrenNodes = await BoxBrowserService.BrowseAsync(boxTreeNode);
                children = childrenNodes.ConvertAll(node => ConvertToBoxBrowserContract(node)).ToList();
                contract.ChildrenCount = children.Count();
                var resultChild = children;
                contract.ChildrenIds = children.Select(r => r.Id.ToString()).ToList();
                if(contract.Level == Contract.BoxBrowser.RMNodeLevel.Root)
                {
                    resultChild = children.Skip(contract.PageIndex * 10).Take(10).ToList();
                }
                contract.Children = resultChild.ToList();
            }
            catch (Exception ex)
            {
                logger.Error($"An error occurred while attempting to browse box tree. Error: {ex}");
            }
            return contract;
        }

        public static RABoxBrowserContract ConvertToBoxBrowserContract(BoxTreeNode contract)
        {
            if (contract == null)
            {
                return null;
            }

            return new RABoxBrowserContract
            {
                Id = contract.Id,
                RealId = contract.RealId,
                ConnectionId = contract.ConnectionId,
                ContainerId = contract.ContainerId,
                OwnerId = contract.OwnerId,
                Level = (Contract.BoxBrowser.RMNodeLevel)contract.Level,
                DisplayName = contract.DisplayName,
                LeafName = contract.LeafName,
                FullPath = contract.FullPath,
                Parent = ConvertToBoxBrowserContract(contract.Parent),
                IconStatus = (IconStatus)contract.IconStatus,
                PageIndex = contract.PageIndex,
                ChildrenIds = contract.ChildrenIds,
                Children = contract.Children == null ? null : contract.Children.ConvertAll(_ => ConvertToBoxBrowserContract(_)),
                ChildrenCount = contract.ChildrenCount,
                CheckNumber = contract.CheckNumber,
                Name = contract.Name,
                Expanded = contract.Expanded,
            };
        }

        public static BoxTreeNode ConvertToBoxTreeNode(RABoxBrowserContract contract)
        {
            if (contract == null)
            {
                return null;
            }

            return new BoxTreeNode
            {
                Id = contract.Id,
                RealId = contract.RealId,
                ConnectionId = contract.ConnectionId,
                ContainerId = contract.ContainerId,
                OwnerId = contract.OwnerId,
                Level = (RMNodeLevel)contract.Level,
                DisplayName = contract.DisplayName,
                LeafName = contract.LeafName,
                FullPath = contract.FullPath,
                Parent = ConvertToBoxTreeNode(contract.Parent),
                IconStatus = (Contract.Object.IconStatus)contract.IconStatus,
                PageIndex = contract.PageIndex,
                ChildrenIds= contract.ChildrenIds,
                Children = contract.Children == null ? null : contract.Children.ConvertAll(_ => ConvertToBoxTreeNode(_)),
                ChildrenCount = contract.ChildrenCount,
                CheckNumber = contract.CheckNumber,
                Name = contract.Name,
                Expanded = contract.Expanded,
            };
        }

        private static bool HasSetting(BoxTreeNode node)
        {
            var groupId = node.Level == RMNodeLevel.BoxConnectionGroup ? node.Id : node.ContainerId;
            var profileId = ScheduleService.GetProfileId(node);
            return BoxSettingDao.GetSettingByScopeIdAndGroupId(node.Id, groupId) != null || ScheduleService.GetScheduleAsync(profileId, ScheduleType.BoxDisposalSchedule).GetAwaiter().GetResult() != null;
        }

        private static void UpdateNodeIconStatus(RABoxBrowserContract contract, BoxTreeNode node)
        {
            if (HasSetting(node) && node.Level != RMNodeLevel.Root)
            {
                node.IconStatus = (Contract.Object.IconStatus)IconStatus.Break;
            }
            else if (contract.IconStatus == IconStatus.Break || contract.IconStatus == IconStatus.Inhert)
            {
                node.IconStatus = (Contract.Object.IconStatus)IconStatus.Inhert;
            }
        }
    }
}
