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
using AngleSharp.Dom;
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.GCommon.Utility;
using AvePoint.RA.Common;
using AvePoint.RA.Common.Aos;
using AvePoint.RA.Common.Util;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Aos;
using AvePoint.RA.Contract.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Explorer.Dao;
using AvePoint.RA.RACommonUtility.Browser;
using AvePoint.RA.Service.Services;
using RAGoogle.GoogleObjDiscover;
using RAGoogle.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Common
{
    public class BrowseTreeService : RMServiceBase, IBrowseTreeService
    {
        private RALogger logger = RALogger.GetInstance(typeof(BrowseTreeService));

        private IRMRemoteNodeService RemoteNodeService => PlatformWindsorManager.GetService<IRMRemoteNodeService>();

        private IRMRemoteGoogleNodeService RemoteGoogleNodeService => PlatformWindsorManager.GetService<IRMRemoteGoogleNodeService>();

        private Dictionary<string, string> _getI18NDictionary = new()
        {
            {
                RMConstants.DEFAULT_GOOGLE_USER_GROUP,
                I18N.Core.I18NEntity.GetString("RM_GoogleUser_Default_Container")
            },
            {
                RMConstants.DEFAULT_GOOGLE_SHARED_DRIVE_GROUP,
                I18N.Core.I18NEntity.GetString("RM_GoogleSharedDrive_Default_Container")
            }
        };


        #region Browse Tree: SharePoint Online & OneDrive & Teams
        public async Task<RMSPSampleTreeNode> BrowseSPOTreeAsync(RMSPSampleTreeNode parentNode, RMBrowseTreeNodeSourceType type, bool checkPermission)
        {
            var searchKey = parentNode.SearchKey;
            var isSearch = parentNode.IsSearch;
            if (!isSearch)
            {
                parentNode.SearchKey = string.Empty;
                switch ((NodeLevel)parentNode.Level)
                {
                    case NodeLevel.Farm:
                    case NodeLevel.Root:
                        parentNode = await RemoteNodeService.GetWebApplicationsAsync(parentNode, type, checkPermission);
                        break;
                    case NodeLevel.WebApplication:
                        parentNode = await RemoteNodeService.GetSiteCollectionsAsync(parentNode, checkPermission, parentNode.IsArchiverTree);
                        break;
                    case NodeLevel.SiteCollections:
                        parentNode = RemoteNodeService.GetSiteCollectionsUnderTeamsAsync(parentNode);
                        break;
                    default:
                        parentNode = BrowseSPOTreeBelowWebApplication(parentNode, type);
                        break;
                }
            }
            else
            {
                logger.Info($"Start to search tree. SeachKey: {parentNode?.SearchKey}, SourceType: {type}");
                if (parentNode.Level == (int)NodeLevel.WebApplication)
                {
                    // container that contains searched children
                    try
                    {
                        logger.Info($"Start to search under container, Name: {parentNode?.Name}, FullPath: {parentNode?.FullPath}");
                        parentNode = await RemoteNodeService.GetSiteCollectionsAsync(parentNode, checkPermission, parentNode.IsArchiverTree);
                    }
                    catch (Exception e)
                    {
                        logger.Error($"An error occured while browser container that contains searched children. Ex: {e}");
                        throw;
                    }
                }
                else
                {
                    try
                    {
                        var isSupportContainerSearch = type == RMBrowseTreeNodeSourceType.SharepointOnline || type == RMBrowseTreeNodeSourceType.Teams;
                        var isExactlySearch = isSupportContainerSearch && !string.IsNullOrEmpty(searchKey) && searchKey.StartsWith('"') && searchKey.EndsWith('"');
                        if(isExactlySearch)
                        {
                            return await RemoteNodeService.GetWebApplicationsForExactlySearchAsync(parentNode, type, checkPermission, parentNode.IsArchiverTree);
                        }

                        parentNode = await RemoteNodeService.GetWebApplicationsForSearchAsync(parentNode, type, checkPermission);
                        parentNode.Expanded = true;
                        if (parentNode.Children == null || parentNode.Children.Count == 0)
                        {
                            return parentNode;
                        }

                        logger.Info($"containers found count: {parentNode.Children.Count}");
                        var availableWebs = new List<RMSPSampleTreeNode>();
                        foreach (var webApplication in parentNode.Children)
                        {
                            try
                            {
                                var needAddResult = false;
                                if (isSupportContainerSearch && webApplication.Loaded.HasValue && webApplication.Loaded.Value)
                                {
                                    logger.Info($"The container matches searchKey. Fullpath: {webApplication.FullPath}");
                                    webApplication.Loaded = null;
                                    if (isExactlySearch)
                                    {
                                        availableWebs.Add(webApplication);
                                        continue; // the web application is already found by search key in exact search mode, skip it
                                    }
                                    needAddResult = true;
                                }
                                webApplication.SearchKey = searchKey;
                                webApplication.PageIndex = 0;
                                webApplication.PageSize = 15;
                                var web = await RemoteNodeService.GetSiteCollectionsAsync(webApplication, checkPermission, parentNode.IsArchiverTree);
                                logger.Info($"sites in container found. container: {webApplication.FullPath}, count: {web.Children?.Count}");

                                if (web.Children != null && web.Children.Count > 0)
                                {
                                    web.Expanded = true;
                                    if (isSupportContainerSearch)
                                    {
                                        web.Loaded = true;
                                        web.IsSearch = true;
                                    }
                                    web.Children.ForEach(n => {
                                        n.ParentId = web.Id;
                                        n.Parent = web;
                                    });
                                    //availableWebs.Add(web);
                                    needAddResult = true;
                                }

                                if (needAddResult)
                                {
                                    availableWebs.Add(web);
                                }
                            }
                            catch (Exception e)
                            {
                                logger.Error($"An error occured while searching sites under web {webApplication.FullPath}. Ex: {e}");
                                throw;
                            }
                        }
                        parentNode.Children = availableWebs;
                        parentNode.ChildrenCount = availableWebs.Count;
                    }
                    catch (Exception e)
                    {
                        logger.Error($"An error occured while searching. Ex: {e}");
                        throw;
                    }
                }
            }

            parentNode.Children?.ForEach(n => {
                n.ParentId = parentNode.Id;
                n.Parent = parentNode;
            });

            return parentNode;
        }

        private RMSPSampleTreeNode BrowseSPOTreeBelowWebApplication(RMSPSampleTreeNode parentNode, RMBrowseTreeNodeSourceType type)
        {
            parentNode.Children = new List<RMSPSampleTreeNode>();
            logger.Info($"Start to browse tree node, Name: {parentNode?.Name}, FullPath: {parentNode?.FullPath}");
            try
            {
                var currentNode = RMDtoConverter.ConvertRMSampleTree2SPTree(parentNode);
                RMDtoConverter.ConvertSPTreeBeforeToJSON(currentNode);
                SPTreeMessage msg = type switch { 
                    
                    RMBrowseTreeNodeSourceType.Teams => RABrowserClient.BrowseTeams(new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = currentNode }, type),
                    _ => RABrowserClient.BrowseSharePoint(
                    new SPTreeMessage() { TreeType = TreeType.SOArchiverTree, Node = currentNode },
                    type)
                };
                if (msg != null && msg.NodeList != null)
                {
                    foreach (SPTreeNodeDto sp in msg.NodeList)
                    {
                        RMSPSampleTreeNode child = RMDtoConverter.ConvertSPTree2RMSampleTree(sp);
                        child.ParentId = parentNode.Id;
                        child.SourceType = parentNode.SourceType;
                        parentNode.Children.Add(child);
                    }
                }
                parentNode.ChildrenCount = parentNode.Children.Count;
            }
            catch (Exception e)
            {
                logger.Error(e.Message, e);
                throw new AveException($"Error occured while Name: {parentNode?.Name}, FullPath: {parentNode?.FullPath}");
            }

            return parentNode;
        }

        #endregion

        #region Browse Tree: Google Drive

        public async Task<RMSampleGoogleTreeNode> BrowseGoogleDriveTreeForRuleAsync(RMSampleGoogleTreeNode parentNode, bool checkPermission)
        {
            try
            {
                parentNode = parentNode.Level switch
                {
                    (int)NodeLevel.Root => await RemoteGoogleNodeService
                        .GetContainersForRuleAsync(parentNode, checkPermission, NodeLevel.GoogleSharedDriveContainer),
                    (int)NodeLevel.GoogleSharedDriveContainer => await RemoteGoogleNodeService
                        .GetDrivesForRuleAsync(parentNode, checkPermission),
                    _ => await GetFolderLevel(parentNode)
                };
                parentNode.Children?.ForEach(children =>
                {
                    children.Parent = null;
                    if (parentNode.Level == (int)NodeLevel.Root && children.DisplayName.IsNotNullOrEmpty())
                    {
                        if (_getI18NDictionary.TryGetValue(children.DisplayName, out var i18nKey))
                        {
                            children.DisplayName = i18nKey;
                        }
                    }
                });
                return parentNode;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new AveException($"Error occured while browse node Name: {parentNode?.Name}, Id: {parentNode?.Id}");
            }
        }

        public async Task<RMSampleGoogleTreeNode> BrowseGoogleDriveTreeAsync(RMSampleGoogleTreeNode parentNode, bool checkPermission)
        {
            try
            {
                parentNode = parentNode.IsSearch switch
                {
                    false when parentNode.Level == (int)NodeLevel.Root => await RemoteGoogleNodeService
                        .GetContainersAsync(parentNode, checkPermission),
                    false when parentNode.Level is (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer => await RemoteGoogleNodeService
                        .GetDrivesAsync(parentNode, checkPermission),
                    true => await RemoteGoogleNodeService.GetContainersForSearchAsync(parentNode, checkPermission),
                    _ => parentNode
                };
                await RemoteGoogleNodeService.LoadGoogleSettingIconAsync(parentNode.Children);
                parentNode.Children?.ForEach(children =>
                {
                    children.Parent = null;
                    if (parentNode.Level == (int)NodeLevel.Root && children.DisplayName.IsNotNullOrEmpty())
                    {
                        if (_getI18NDictionary.TryGetValue(children.DisplayName, out var i18nKey))
                        {
                            children.DisplayName = i18nKey;
                        }
                    }
                });
                return parentNode;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new AveException($"Error occured while browse node Name: {parentNode?.Name}, Id: {parentNode?.Id}");
            }
        }

        #region BrowseGoogleDriveTree for full level
        public async Task<RMSampleGoogleTreeNode> BrowseGoogleDriveTreeForFullLevelAsync(RMSampleGoogleTreeNode parentNode, bool checkPermission)
        {
            try
            {
                parentNode = parentNode.Level switch
                {
                    (int)NodeLevel.Root => await RemoteGoogleNodeService
                        .GetContainersForRuleAsync(parentNode, checkPermission, NodeLevel.GoogleMyDriveContainer, NodeLevel.GoogleSharedDriveContainer),
                    (int)NodeLevel.GoogleMyDriveContainer or (int)NodeLevel.GoogleSharedDriveContainer => await RemoteGoogleNodeService
                        .GetDrivesForRuleAsync(parentNode, checkPermission),
                    //_ => await GetFolderLevel(parentNode)
                };
                parentNode.Children?.ForEach(children =>
                {
                    children.Parent = null;
                    if (parentNode.Level == (int)NodeLevel.Root && children.DisplayName.IsNotNullOrEmpty())
                    {
                        if (_getI18NDictionary.TryGetValue(children.DisplayName, out var i18nKey))
                        {
                            children.DisplayName = i18nKey;
                        }
                    }
                });
                return parentNode;
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new AveException($"Error occured while browse node Name: {parentNode?.Name}, Id: {parentNode?.Id}");
            }
        }
        #endregion

        private async Task<RMSampleGoogleTreeNode> GetFolderLevel(RMSampleGoogleTreeNode parentNode)
        {
            RMAosGoogleAppProfile googleAppProfile =
                RMAosApiClient.GetGoogleAppProfile(TenantLocalValue.LogonGroupId, parentNode.GoogleTenantId, true);
            RMGoogleDiscoverBase discoverBase = new(null);
            discoverBase.Init(googleAppProfile);
            var driveNode = GetDriveLevel(parentNode);
            var isSharedDrive = driveNode.Level is (int)NodeLevel.GoogleSharedDrive;
            GoogleDriveService googleDriveService = await discoverBase.GetDriveService(
                isSharedDrive ? driveNode.ObjectId : driveNode.DisplayName);
            var folders = isSharedDrive
                ? await googleDriveService.PageFoldersByDriveIdAsync(driveNode.ObjectId, parentNode.Level == (int) NodeLevel.GoogleFolder ? parentNode.ObjectId : driveNode.ObjectId)
                : await googleDriveService.PageMyDriveFoldersAsync(parentNode.Level == (int) NodeLevel.GoogleFolder ? parentNode.ObjectId : "root");
            parentNode.Children = folders.Select(folder => new RMSampleGoogleTreeNode
            {
                Id = folder.Id,
                Name = folder.Name,
                DisplayName = folder.Name,
                NodeType = (int) NodeLevel.GoogleFolder,
                Level = (int) NodeLevel.GoogleFolder,
                ParentId = parentNode.Id,
                ObjectId = folder.Id,
                Parent = parentNode,
                FullPath = folder.Parents.IsNotNullOrEmpty() ? folder.Parents[0] : string.Empty,
                GoogleTenantId = parentNode.GoogleTenantId,
                DriveId = folder.DriveId,
            }).ToList();
            parentNode.ChildrenCount = folders.Count;
            return parentNode;
        }

        private RMSampleGoogleTreeNode GetDriveLevel(RMSampleGoogleTreeNode node)
        {
            while (node != null && (NodeLevel) node.Level is not (NodeLevel.GoogleMyDrive or NodeLevel.GoogleSharedDrive))
            {
                node = node.Parent;
            }
            return node;
        }

        #endregion

        #region Support GoogleOne
        public async Task<RMSampleGoogleTreeNode> BrowseGoogleNodesByPagerAsync(RMSampleGoogleTreeNode parentNode, bool checkPermission)
        {
            try
            {
                return await RemoteGoogleNodeService.BrowseGoogleNodesByPagerAsync(parentNode, checkPermission);
            }
            catch (Exception ex)
            {
                logger.Error(ex.Message, ex);
                throw new AveException($"Error occured while browse node Name: {parentNode?.Name}, Id: {parentNode?.Id}");
            }
        }

        public Task<SearchSiteCollectionLazyLoadResponse> SearchSiteCollectionLazyLoad(SearchSiteCollectionLazyLoadRequest condition, bool checkPermission)
        {
            return RemoteNodeService.SearchSiteCollectionLazyLoad(condition, checkPermission);
        }

        public async Task<RMSPSampleTreeNode> SearchContainerByPage(RMSPSampleTreeNode parentNode, RMBrowseTreeNodeSourceType type, bool checkPermission = true)
        {
            var searchKey = parentNode.SearchKey;
            var isExactlySearch = !string.IsNullOrEmpty(searchKey) && searchKey.StartsWith('"') && searchKey.EndsWith('"');
            if (isExactlySearch)
            {
                if (type == RMBrowseTreeNodeSourceType.SharepointOnline || type == RMBrowseTreeNodeSourceType.Teams)
                {
                    return await RemoteNodeService.GetWebApplicationsOnlyForExactlySearchAsync(parentNode, type, checkPermission, parentNode.IsArchiverTree);
                }
                else
                {
                    throw new NotSupportedException("Exactly search is only supported in SharePoint Online and Teams & Groups source type");
                }
            }
            else
            {
                if (type == RMBrowseTreeNodeSourceType.SharepointOnline || type == RMBrowseTreeNodeSourceType.Teams)
                {
                    parentNode = await RemoteNodeService.GetWebApplicationsOnlyForSearchAsync(parentNode, type, checkPermission);
                }
                else
                {
                    throw new NotSupportedException("Partial search is only supported in SharePoint Online and Teams & Groups source type");
                }
            }
            parentNode.Expanded = true;

            return parentNode;
        }

        #endregion
    }
}
