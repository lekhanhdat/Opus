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




using System.Collections.ObjectModel;
using AvePoint.GCommon.Contract.Tree.Object;
using System.Collections.Generic;

namespace AvePoint.GCommon.Contract.Tree.Object
{
    public interface IFilter<T>
    {
        bool IsMatch(T value);
        bool IsMatch(T value, List<NodeFilterPolicy> filters);
    }

    public class CommonTreeNodeFilter : IFilter<IAveTreeNodeDto>
    {

        private IFilterPolicy<int> _intFilterPolicy = FilterPolicyManager.GetPolicy<int>();
        private IFilterPolicy<string> _stringFilterPolicy = FilterPolicyManager.GetPolicy<string>();
        private IFilterPolicy<bool> _boolFilterPolicy = FilterPolicyManager.GetPolicy<bool>();

        public CommonTreeNodeFilter()
        {

        }

        /// <summary>
        /// 判断节点是否应该被过滤掉。
        /// </summary>
        /// <param name="value">需要验证的节点</param>
        /// <returns>true表示过滤掉</returns>
        public bool IsMatch(IAveTreeNodeDto value, List<NodeFilterPolicy> filters)
        {
            if (filters == null)
            {
                return true;

            }
            bool hasInclude = false;
            bool isInclude = false;
            foreach (NodeFilterPolicy nodePolicy in filters)
            {
                if (nodePolicy is IncludeNode)
                {
                    hasInclude = true;
                }
                if (_intFilterPolicy.IsMatch(GetLevelValue(nodePolicy.Level), (int)value.Level) &&
                    _intFilterPolicy.IsMatch(nodePolicy.Type, (int)value.Type) &&
                    _stringFilterPolicy.IsMatch(nodePolicy.DisplayName, value.Name) &&
                        (
                        !(value is SPTreeNodeDto) ||
                        _boolFilterPolicy.IsMatch(nodePolicy.Hidden, ((SPTreeNodeDto)value).Hidden) &&
                        _intFilterPolicy.IsMatch(nodePolicy.Template, ((SPTreeNodeDto)value).Template)
                        )
                    )
                {

                    if (nodePolicy is IncludeNode)
                    {
                        isInclude = true;
                        break;


                    }
                    else
                    {

                        return false;

                    }
                }


            }

            isInclude = !(isInclude ^ hasInclude);

            if (!isInclude)
            {
                return false;
            }
            else
            {

                return true;
            }
        }

        /// <summary>
        /// 获取当前节点的级别值。
        /// 默认类型的Level和*都表示所有类型。
        /// </summary>
        private string GetLevelValue(NodeLevel nodeLevel)
        {
            return (nodeLevel == NodeLevel.Undefined) ? "*" : ((int)nodeLevel).ToString();
        }

        public bool IsMatch(IAveTreeNodeDto value)
        {
            throw new System.NotImplementedException();
        }
    }
}
