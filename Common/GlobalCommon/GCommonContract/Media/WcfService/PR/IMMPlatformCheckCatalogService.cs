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





namespace AvePoint.GCommon.Contract.Media.WcfService
{
    #region using directives
    using AvePoint.GCommon.Contract.Media.Object;

    #endregion

    public interface IMMPlatformCheckCatalogService
    {
        /// <summary>
        /// 检查catalog，看是否备份了config db
        /// </summary>
        /// <param name="param">定位catalog所需参数</param>
        /// <returns>如果备份了config db，返回true，反之 返回false</returns>
        bool CheckConfigDBState(PlatformCheckCatalogParam param);
    }
}
