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




namespace AvePoint.Media.Service
{
    #region using directives
    using System.Collections.Generic;
    using AvePoint.Media.Service.DomainModel;
using System;
    #endregion

    public interface IAdvancedConditionsHandler
    {
        /// <summary>
        /// 这个方法实现了根据AdvancedConditions合并branches的功能
        /// </summary>
        /// <param name="paramDictionary">key是用户设置的filter的序号，value是有待合并的branches</param>
        /// <param name="advancedConditions">用户输入的AdvancedConditions字符串，例如“1and（2or3）”</param>
        /// <returns>合并之后，要显示在界面上的trees（用户选中结点以下的部分）</returns>
        List<TreeNode> AssembleTreeByAdvancedConditions(List<TreeNode> paramDictionary, string advancedConditions,Boolean isObjectSearch =false);
    }
}
