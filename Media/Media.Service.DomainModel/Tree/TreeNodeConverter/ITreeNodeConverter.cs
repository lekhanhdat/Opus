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




namespace AvePoint.Media.Service.DomainModel
{
    using AvePoint.GCommon.Contract.Tree;
    #region using directives

    using AvePoint.GCommon.Contract.Tree.Object;
    using System;
    using System.Collections.Generic;

    #endregion using directives

    public interface ITreeNodeConverter
    {
        SPTreeNodeDto ConvertTreeNodeToSPTreeNode(TreeNode treeNode, NodeLevel level,Boolean ignoreChildren = default(Boolean));

        List<SPTreeNodeDto> ConvertTreeNodeListToSPTreeNodeList(List<TreeNode> treeNodeList, NodeLevel level);

        PRTreeNodeDto ConvertTreeNodeToPRTreeNode(TreeNode treeNode);

        List<PRTreeNodeDto> ConvertTreeNodeListToPRTreeNodeList(List<TreeNode> treeNodeList);

        TreeNode ConvertSPTreeNodeToTreeNode(SPTreeNodeDto spTreeNode);

        List<HistoryVersion> ConvertTreeNodeListToHistoryVersionList(List<TreeNode> treeNodeList);

        List<TreeNode> ConvertEITreeNodeListToTreeNodeList(List<EITreeNodeDto> eiTreeNodeList);

        List<ExchangeOnlineTreeNodeDto> ConvertTreeNodeListToExchangeTreeNodeList(List<TreeNode> treeNodeList);

        TreeNode ConvertExchangeOnlineTreeNodeToTreeNode(ExchangeOnlineTreeNodeDto exchangeTreeNode);

        List<SPTreeNodeDto> ConvertTreeNodeListToTeamsTreeNodeList(List<TreeNode> treeNodeList, NodeLevel level);

        List<GoogleDriveTreeNodeDto> ConvertTreeNodeListToGDriveTreeNodeList(List<TreeNode> treeNodeList, NodeLevel level);

    }
}