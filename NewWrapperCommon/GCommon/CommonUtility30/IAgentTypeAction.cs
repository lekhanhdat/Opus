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

namespace AvePoint.Service.AgentType
{
    /// <summary>
    /// 不同AgentType都有两种状态。
    /// </summary>
    public interface IAgentTypeAction
    {
        /// <summary>
        /// called when installing
        /// </summary>
        void Install();
        /// <summary>
        /// called when checking agent type
        /// </summary>
        void Check();
        /// <summary>
        /// called when uncheck agent type
        /// </summary>
        void UnCheck();
        /// <summary>
        /// called when uninstalling
        /// </summary>
        void Uninstall();
        /// <summary>
        /// called after patching
        /// </summary>
        void Patch(InstallOption installOption);
        /// <summary>
        /// Agent type value
        /// </summary>
        string AgentTypeValue { get; }
    }

    [Flags]
    public enum InstallOption : long
    {
        None = 0L,

        UpgradeSolution = 1 << 0,
    }
}
