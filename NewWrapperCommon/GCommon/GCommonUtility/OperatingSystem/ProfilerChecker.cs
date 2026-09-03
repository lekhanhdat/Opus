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

namespace AvePoint.GCommon.Utility
{
    #region using directives
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    using AvePoint.GCommon.Configurations;

    #endregion

    internal class ProfilerChecker : IProfilerChecker
    {
        static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        public Dictionary<String, String> Check(String imageName, Dictionary<String, String> environmentVariable)
        {
            var result = environmentVariable;
            try
            {
                var profiler = DocAveCongiguration.Diagnostics.Profiler;
                if (profiler.IsActive)
                {
                    var traces =
                        from tracer in profiler.Tracers.Cast<Tracer>()
                        where tracer.IsEnabled && imageName.ToLowerInvariant().Contains(tracer.ProcessName.ToLowerInvariant())
                        select tracer;
                    var isTraceProcess = traces.Any();
                    if (isTraceProcess)
                    {
                        result = this.AddProfilingEnvironmentVariables(environmentVariable);
                    }
                }
            }
            catch (Exception e)
            {
                string msg = e.ToString();
                logger.Debug("There is no profiler configuration, Process will be started in non-profiler status.");
            }
            return result;
        }

        Dictionary<String, String> AddProfilingEnvironmentVariables(Dictionary<String, String> environmentVariable)
        {
            var result = environmentVariable;
            var profilerEnvironmentVariables = new ProcessStartInfo().EnvironmentVariables;
            profilerEnvironmentVariables["Cor_Enable_Profiling"] = "0x1";
            profilerEnvironmentVariables["COR_PROFILER"] = "{8782F5A0-E8B0-49af-B9D2-D0BE025D5D3E}";
            result = result ?? new Dictionary<String, String>();
            foreach (DictionaryEntry entry in profilerEnvironmentVariables)
            {
                if (!result.ContainsKey(entry.Key.ToString()))
                    result.Add(entry.Key.ToString(), entry.Value.ToString());
            }
            return result;
        }
    }
}