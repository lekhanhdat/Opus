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
using AvePoint.GCommon.Contract.Tree.Object;
using AvePoint.RA.CommonUtil;
using System.Collections.Generic;
using System;
using System.Reflection;
using AvePoint.GCommon.Contract.Tree;
using AvePoint.RA.I18N.Core;
using AvePoint.RA.DB.Dao.GoogleSyncNodeDao.Contract;
using AvePoint.RA.Common;
using AvePoint.RA.Contract.Object;
using AvePoint.RA.Common.Util;

namespace AvePoint.RA.RACommonUtility.Browser;

public class GoogleBrowser
{
    private static readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

    private static readonly Dictionary<NodeLevel, Func<GoogleDriveTreeNodeDto, List<GoogleDriveTreeNodeDto>>> NodeLevelMapping = new ()
        {
            {NodeLevel.Root, RootBrowse },
        };
    private static IRMGoogleRemoteNodeDao RemoteNodeDao => PlatformWindsorManager.GetService<IRMGoogleRemoteNodeDao>();

    public static GoogleDriveTreeMessage Browse(GoogleDriveTreeMessage message)
    {
        Logger.Info($"Start browse google tree, level: {message.Node.Level}.");
        if (!NodeLevelMapping.TryGetValue(message.Node.Level, out var browseFunc))
        {
            Logger.Error($"Nonsupport {message.Node.Level} exchange node browse children.");
            return null;
        }
        var children = browseFunc(message.Node);
        Logger.Info($"End browse exchange tree, level: {message.Node.Level}, children count: {children.Count}.");
        return new GoogleDriveTreeMessage
        {
            NodeList = children,
            ChildrenCount = children.Count,
            TreeType = TreeType.GoogleDriveArchiverTree
        };
    }

    private static List<GoogleDriveTreeNodeDto> RootBrowse(GoogleDriveTreeNodeDto node)
    {
        const string RootName = "My Google Drive";
        var FarmDisplayName = I18NEntity.GetString("RM_Google_Drive_RootNode");
        return
            [
                new GoogleDriveTreeNodeDto
                {
                    ID = Guid.NewGuid().ToString(),
                    Level = NodeLevel.Root,
                    Name = RootName,
                    DisplayName = FarmDisplayName,
                    CanChildrenBeLoaded = true,
                    Offset = 0
                }
            ];
    }

    public static List<GoogleDriveTreeNodeDto> BrowserTreeNode(GoogleDriveTreeNodeDto node)
    {
        List<GoogleDriveTreeNodeDto> children = [];
        if (node.Level == NodeLevel.Root)
        {
            children.AddRange(BrowserRootNode());
        }
        else if (node.Level == NodeLevel.GoogleMyDriveContainer || node.Level == NodeLevel.GoogleSharedDriveContainer)
        {
            children.AddRange(BrowserContainerNode(node));
        }

        return children;
    }

    private static List<GoogleDriveTreeNodeDto> BrowserRootNode()
    {
        List<GoogleDriveTreeNodeDto> childNodeDtos = [];
        List<RMSampleGoogleTreeNode> childNodes = RemoteNodeDao.GetAllGoogleContainers();
        if (childNodes != null && childNodes.Count > 0)
        {
            childNodeDtos.AddRange(childNodes.ConvertAll(x => RMDtoConverter.ConvertSampleTree2Dto(x)));
        }
        return childNodeDtos;
    }

    private static List<GoogleDriveTreeNodeDto> BrowserContainerNode(GoogleDriveTreeNodeDto node)
    {
        List<GoogleDriveTreeNodeDto> childNodeDtos = [];
        List<RMSampleGoogleTreeNode> childNodes = RemoteNodeDao.GetGoogleDrivesByParentId(node.ID);
        if(childNodes != null && childNodes.Count > 0)
        {
            childNodeDtos.AddRange(childNodes.ConvertAll(x => RMDtoConverter.ConvertSampleTree2Dto(x)));
        }
        return childNodeDtos;
    }
}
