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



using System;
using System.Collections.Generic;
using System.Text;
using AvePoint.GCommon.Contract.Tree.Object;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    public class AveTreeUtil
    {
        public static NodeLevel GetChildrenLevel(NodeLevel parentLevel)
        {
            switch (parentLevel)
            {
                case NodeLevel.Farm: return NodeLevel.WebApplication;
                case NodeLevel.WebApplication: return NodeLevel.SiteCollection;
                case NodeLevel.SiteCollection: return NodeLevel.Site;
                case NodeLevel.Site: return NodeLevel.List;
                case NodeLevel.Lists: return NodeLevel.List;
                case NodeLevel.Sites: return NodeLevel.Site;
                case NodeLevel.List: return NodeLevel.Folder;
                case NodeLevel.Folder: return NodeLevel.Folder;
                default: return NodeLevel.Root;
            }
        }

        /// <summary>
        /// (注意：此方法原计划用于从数据库读取tree之后添加parent属性使用，但是由于采用DataContractSerializer进行了序列化，Parent属性在重数据库读取之后已经包含，因此该方法以后不需要使用)处理只有子节点没有父节点的Tree，此方法将会在节点中加入父节点的引用
        /// </summary>
        /// <param name="node"></param>
        public static void AddParentReference(IAveTreeNodeDto node)
        {
            foreach (IAveTreeNodeDto child in node.Children)
            {
                child.Parent = node;
                AddParentReference(child);
            }
        }

        /// <summary>
        /// SharePoint节点的默认比较器
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public static int SPTreeNodeComparision(SPTreeNodeDto node1, SPTreeNodeDto node2)
        {
            if (node1.Level == NodeLevel.Farm)
            {
                if (node1.SPType == SPType.Moss && node2.SPType == SPType.BPOS)
                {
                    return -1;
                }
                if (node1.SPType == SPType.BPOS && node2.SPType == SPType.Moss)
                {
                    return 1;
                }
                if (node1.SPVersion != node2.SPVersion)
                {
                    return node1.SPVersion - node2.SPVersion;
                }
            }
            return string.Compare(node1.DisplayName, node2.DisplayName, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// SharePoint节点的默认比较器
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        public static int FSTreeNodeComparision(FSTreeNodeDto node1, FSTreeNodeDto node2)
        {
            if (node1.Level == NodeLevel.Farm)
            {
                if (node1.SPType == SPType.Moss && node2.SPType == SPType.BPOS)
                {
                    return -1;
                }
                if (node1.SPType == SPType.BPOS && node2.SPType == SPType.Moss)
                {
                    return 1;
                }
                if (node1.SPVersion != node2.SPVersion)
                {
                    return node1.SPVersion - node2.SPVersion;
                }
            }
            return string.Compare(node1.DisplayName, node2.DisplayName, StringComparison.CurrentCulture);
        }

        /// <summary>
        /// New DocAve: 前台向后台发送消息时，new docave用的是json消息，会引起循环引用问题。
        /// 所以前台tree dto调用了toJson()方法，去掉了Parent引用。
        /// 发到后台方法里时，如果需要用到Parent属性，需要调用此方法加上Parent属性引用。
        /// </summary>
        /// <param name="node"></param>
        public static void AddParentJsonReference(IAveTreeNodeDto node)
        {
            if (node != null && node.Children != null)
            {
                foreach (IAveTreeNodeDto item in node.Children)
                {
                    item.Parent = node;
                    AddParentReference(item);
                }
            }
        }

        /// <summary>
        /// New DocAve: 前台向后台发送消息时，
        /// 由于IncludeNew和SelectAll都加了[DataMember(EmitDefaultValue = false)]标签，并且在构造函数中初始化的值并不是默认值"0"，而是"-1"。
        /// 所以在前台向后台发消息json序列化时，如果前台值是"0"，则在后台获取时却是"-1"。
        /// 机制和旧代码(获取的是默认值"0")中不一致，所以暂时添加此公共方法，需要各模块所有向后台发送tree dto数据的地方，在Controller中调用此方法转换下。
        /// 可以参考Granular backup controller代码，已添加此逻辑，代码：BackupPlanModelConverter.cs。
        /// 比如save\edit plan时，可以在Controller中，将plan module convert to plan dto的时候，调用此方法先convert下tree dto。
        /// </summary>
        /// <param name="node"></param>
        public static void ConvertTreeDtoDefaultValue(SPTreeNodeDto node)
        {
            if (node != null)
            {
                #region 方案1
                //方案1：前台将值存放在ExtraOptions里，然后再在后台取出赋值到属性上，最后删除ExtraOptions项，但是考虑ExtraOptions属性会xml序列化到DB中，风险比较大。
                /*
                if (node.ExtraOptions != null)
                {
                    var ep = node.ExtraOptions.Find(item => item.Key == "IncludeNew");
                    if (ep != null)
                    {
                        if (node.IncludeNew == IncludeNewState.Undefined)
                        {
                            int enumInt = 0;
                            if (int.TryParse(ep.value, out enumInt))
                            {
                                node.IncludeNew = (IncludeNewState)enumInt;
                            }
                            node.ExtraOptions.Remove(ep);
                        }
                    }
                }
                */
                #endregion

                //方案2：直接将默认值赋值给对应属性，简单看了下整体代码，发现两个Undefined枚举值并没有实际意义，用到的地方也很少，都是用Unchecked和Checked枚举值。
                if (node.IncludeNew == IncludeNewState.Undefined)
                {
                    node.IncludeNew = IncludeNewState.Unchecked;
                }
                if (node.SelectAll == SelectAllState.Undefined)
                {
                    node.SelectAll = SelectAllState.Unchecked;
                }
                if (node.Children != null)
                {
                    foreach (var item in node.Children)
                    {
                        ConvertTreeDtoDefaultValue(item);
                    }
                }
            }
        }

        public static void RemoveParentReference(IAveTreeNodeDto node)
        {
            if (node != null && node.Children != null)
            {
                foreach (IAveTreeNodeDto child in node.Children)
                {
                    child.Parent = null;
                    RemoveParentReference(child);
                }
            }
        }

    }

}
