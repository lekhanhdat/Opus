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
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Explorer;
using AvePoint.RA.Contract.RMWeb.Tree.Base;
using RASourceTreeQuery.Exceptions;
using RASourceTreeQuery.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RASourceTreeQuery.Queriers
{
    public abstract class SourceTreeQuerier<T> where T : SourceTreeNode, IParentSourceTreeNode<T>, new()
    {

        protected readonly RALogger Logger = RALogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public abstract SourceFlag Flag { get; }

        protected abstract Dictionary<RMNodeLevel, Func<T, IEnumerable<T>>> LevelContainerQueries { get; }

        protected abstract Dictionary<RMNodeLevel, Func<PagingSourceTreeNode<T>, PagingSourceTreeNode<T>>> LevelContainerPagingQueries { get; }

        protected abstract Dictionary<RMNodeLevel, Func<PagingSourceTreeNode<T>, PagingSourceTreeNode<T>>> LevelItemPagingQueriers { get; }

        protected abstract bool HasSetting(T node);

        public abstract T GetRootNode();

        public IEnumerable<T> GetChildrenContainer(T node)
        {
            if (!LevelContainerQueries.TryGetValue(node.Level, out var queryFunc))
            {
                throw new SourceTreeQuerierException($"The [{Flag}] querier can't find [{node.Level}] level container query function.");
            }

            return queryFunc(node);
        }

        public PagingSourceTreeNode<T> GetChildrenContainer(PagingSourceTreeNode<T> node)
        {
            if (!LevelContainerPagingQueries.TryGetValue(node.Node.Level, out var queryFunc))
            {
                throw new SourceTreeQuerierException($"The [{Flag}] querier can't find [{node.Node.Level}] level container query function.");
            }

            return queryFunc(node);
        }

        public IEnumerable<T> GetChildrenContainerWithSettingIcon(T node)
        {
            var children = GetChildrenContainer(node);
            AddNodeSettingIcon(node, children);
            return children;
        }

        public PagingSourceTreeNode<T> GetChildrenContainerWithSettingIcon(PagingSourceTreeNode<T> node)
        {
            var pagingNode = GetChildrenContainer(node);
            AddNodeSettingIcon(node.Node, pagingNode.Children);
            return pagingNode;
        }

        public PagingSourceTreeNode<T> GetChildrenItem(PagingSourceTreeNode<T> node)
        {
            if (!LevelItemPagingQueriers.TryGetValue(node.Node.Level, out var queryFunc))
            {
                throw new SourceTreeQuerierException($"The [{Flag}] querier can't find [{node.Node.Level}] level item query function.");
            }

            return queryFunc(node);
        }

        private void AddNodeSettingIcon(T node, IEnumerable<T> children)
        {
            foreach (var child in children)
            {
                var hasSetting = HasSetting(child);
                if (hasSetting)
                {
                    child.IconStatus = AvePoint.RA.Contract.Object.IconStatus.Break;
                }
                else if (node.IconStatus == AvePoint.RA.Contract.Object.IconStatus.Break ||
                    node.IconStatus == AvePoint.RA.Contract.Object.IconStatus.Inhert)
                {
                    child.IconStatus = AvePoint.RA.Contract.Object.IconStatus.Inhert;
                }
            }
        }
    }
}
