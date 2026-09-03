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
using System.Linq;
using System.Text;
using AvePoint.GCommon.Contract.Server.ControlPanel.Object;

namespace AvePoint.GCommon.Contract.Server.ControlPanel
{
    public interface IAgentUseJobController
    {
        List<ServiceDto> FilterAgent(List<ServiceDto> agents, FilterMode filterMode = FilterMode.Available);
        List<ServiceDto> FilterAgentForQuick(List<ServiceDto> agents, FilterMode filterMode = FilterMode.Available);
        List<ServiceDto> FilterAgentForQuickForDPM(List<ServiceDto> agents, FilterMode filterMode = FilterMode.Available);
    }

    public enum FilterMode
    {
        Available,
        JobControl,
        JobControlWithWaitJob,
    }

    /// <summary>
    /// 枚举类型，用来表示Filter方法中使用哪种查询逻辑，过滤超出run job 数量限制的agent
    /// 通过修改ControlServiceProperties.config 中的filterExceedRestrictionAgent 配置(默认值是1，旧逻辑)
    /// 1，FilterAgentOldFunction 表示使用旧查询逻辑； 2，FilterAgentQuickFunction 表示使用新查询逻辑；
    /// </summary>
    public enum FilterFunctionMode
    {
        FilterAgentOldFunction = 1,
        FilterAgentQuickFunction = 2,
    }
}
