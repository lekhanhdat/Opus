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
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Reflection;

    #endregion

    /// <summary>
    /// This class is used to handle the weak reference memory leak of creating lot of trace source object
    /// , this issue is reported by Victor Wang, as workaround solution, we add this class, the prune policy
    /// is from the .net 4.0 framework.  for the detail information, please mail to 
    /// <seealso cref="mailto://yhzhang@avepoint.com">Baron</seealso>
    ///    <code>
    ///     static List<WeakReference> tracesources = new List<WeakReference>();
    ///     public TraceSource(string name, SourceLevels defaultLevel)
    ///     {
    ///         this.manager = new TraceEventCache();
    ///         if (name == null)
    ///         {
    ///             throw new ArgumentNullException("name");
    ///         }
    ///         if (name.Length == 0)
    ///         {
    ///             throw new ArgumentException("name");
    ///         }
    ///         this.sourceName = name;
    ///         this.switchLevel = defaultLevel;
    ///         lock (tracesources)
    ///         {
    ///              tracesources.Add(new WeakReference(this));
    ///         }
    ///     }     
    ///   </code>
    /// <remarks>
    ///     if you not create a lot of TraceSource objects, you do not need to use this class, currently,
    ///     I only know the microkernel, ave network, full text engine and media use the trace source
    ///     
    ///     also in the following Microsoft page, from the .net community has some comment about this issue.
    ///     http://msdn.microsoft.com/en-us/library/system.diagnostics.tracesource(VS.80).aspx
    /// </remarks>
    /// </summary>
    public class TraceSourceGarbageCollector
    {
        static Int32 lastCollectionCount;
        static List<WeakReference> tracesources;

        static TraceSourceGarbageCollector()
        {
            var traceSourceInternalField = typeof(TraceSource).GetField("tracesources", BindingFlags.NonPublic | BindingFlags.Static);
            if (traceSourceInternalField != null)
                tracesources = traceSourceInternalField.GetValue(null) as List<WeakReference>;
        }

        /// <summary>
        /// collect the weak reference object cached by trace source object
        /// </summary>
        public static void Collect()
        {
            if (tracesources != null)
            {
                lock (tracesources)
                {
                    if (lastCollectionCount != GC.CollectionCount(2))
                    {
                        var collection = new List<WeakReference>(tracesources.Count);
                        for (int i = 0; i < tracesources.Count; i++)
                        {
                            if (((TraceSource)tracesources[i].Target) != null)
                            {
                                collection.Add(tracesources[i]);
                            }
                        }
                        if (collection.Count < tracesources.Count)
                        {
                            tracesources.Clear();
                            tracesources.AddRange(collection);
                            tracesources.TrimExcess();
                        }
                        lastCollectionCount = GC.CollectionCount(2);
                    }
                }
            }      
        }
    }
}
