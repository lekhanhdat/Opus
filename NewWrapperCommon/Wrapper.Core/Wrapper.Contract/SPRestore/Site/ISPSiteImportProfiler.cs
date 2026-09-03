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

namespace AvePoint.Wrapper.Core.SPRestore
{
    /// <summary>
    /// ISPRestore Site Profiler
    /// </summary>
    public interface ISPSiteImportProfiler : ISPImportProfiler
    {
        
    }

    /// <summary>
    /// ISPRestore Web Profiler
    /// </summary>
    public interface ISPWebImportProfiler : ISPImportProfiler
    { 
        
    }

    /// <summary>
    /// ISPRestore List Profiler
    /// </summary>
    public interface ISPListImportProfiler : ISPImportProfiler
    {

    }

    /// <summary>
    /// Event Args for sp import
    /// </summary>
    public class SPImportEventArgs
    {
        /// <summary>
        /// Url
        /// </summary>
        public string Url { get; internal set; }

        /// <summary>
        /// Title
        /// </summary>
        public string Title { get; internal set; }

        /// <summary>
        /// Message
        /// </summary>
        public WrapperInternationalMessage Message { get; internal set; }

        /// <summary>
        /// Status
        /// </summary>
        public WrapperRestoreStatus Status { get; internal set; }

        /// <summary>
        /// Type
        /// </summary>
        public SPObjectType Type { get; internal set; }

        /// <summary>
        /// Level
        /// </summary>
        public SPObjectLevel Level { get; internal set; }
    }
}
