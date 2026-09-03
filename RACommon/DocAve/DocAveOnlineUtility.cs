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
using AvePoint.GCommon.Contract.Server.Common;
using AvePoint.GCommon.Contract.Server.ControlPanel.Office365;
using AvePoint.GCommon.Contract.SharePointBrowser;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Contract.Tree.Object;
using System;
using System.Collections.Generic;

namespace AvePoint.RA.Common.DocAve
{

    /// <summary>
    /// 暂时替代来自Online的数据方法
    /// </summary>
    public class DocAveOnlineUtility //for building
    {
        //unfinished===
        //GetRAGlobalSetting
        //GetArchiverConfigFromAgent
        //GetIndexDeviceSetting

       //不需要
        public static Office365TestResult TestForOffice365(Office365MessageContract message, RemoteWebApplication webapp, string other)
        {
            throw new NotImplementedException("TestForOffice365");
        }

        //Online是否支持Move 
        public static SOReturnMessage ValidationInputUrl(string farmId, DestinationLocationInfo destinationInfo, SPType spType, bool isProfileRule, SPTreeNodeDto virtualNode)
        {
            return null;
        }
    }
}
