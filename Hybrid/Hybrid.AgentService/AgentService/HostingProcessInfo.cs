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



namespace AvePoint.Hybrid.AgentService
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Text;
    #endregion

    internal class HostingProcessInfo
    {
        public List<String> agentTypes;
        public String exePath;
        public String exeName;
        public String workingDir;
        public String args;

        public delegate void OnStopHandler();
        public event OnStopHandler PreStop;
        public bool LazyStart;
        public bool NeedMonitoring;

        /// <summary>
        /// 由于存在其他模块需要两个Agent Type任一一种都能起这个Process，所以需要加一个List集合，用于下面的判断
        /// </summary>
        /// <param name="agentTypes"></param>
        /// <param name="exePath"></param>
        /// <param name="exeName"></param>
        /// <param name="workingDir"></param>
        /// <param name="args"></param>
        /// <param name="onStopHanler"></param>
        public HostingProcessInfo(
            List<String> agentTypes,
            String exePath,
            String exeName,
            String workingDir,
            String args,
            OnStopHandler onStopHanler,
            Boolean lazyStart,
            Boolean needMonitoring)
        {
            this.agentTypes = agentTypes;
            this.exePath = exePath;
            this.exeName = exeName;
            this.workingDir = workingDir;
            this.args = args;
            this.PreStop += onStopHanler;
            this.LazyStart = lazyStart;
            this.NeedMonitoring = needMonitoring;
        }

        public void PeacefulStop()
        {
            if (PreStop != null)
                PreStop();
        }
    }
}
