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




namespace AvePoint.GCommon.MicroKernel
{
    #region using directives
    using System;
    using System.Diagnostics;
    #endregion

    #region Attribute

    [DebuggerNonUserCode]
    #endregion

    ///<Summary>
    /// Provide a abstract implement of the ITraceSoure interface.
    /// 
    /// <remarks>
    ///     There is a cached weak reference list in tracesource class may lead to memory
    ///     leak, you can see the tracesource class's constructor implementation as follows:
    ///     
    ///  <code>
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
    ///     
    ///  </code> 
    ///     Also in the Microsoft page, from the .net community has some comment about this issue.
    ///     http://msdn.microsoft.com/en-us/library/system.diagnostics.tracesource(VS.80).aspx
    /// </remarks>
    ///</Summary>
    public abstract class TraceSourceBase : ITraceSource
    {
        TraceSource internalTraceSource;
        protected abstract TraceSource TraceSource { get; }
        protected abstract Int32 EventId { get; }

        #region ITraceSource Members

        #region Trace Infromation

        public void TraceInformation(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceInformation(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Information, eventId, format, data);
        }

        public void TraceInformation(String format, params Object[] data)
        {
            this.TraceInformation(this.EventId, format, data);
        }

        public void TraceInformation(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceInformation(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Information, this.EventId, message);
        }

        #endregion

        #region Trace Warning

        public void TraceWarning(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceWarning(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Warning, eventId, format, data);
        }

        public void TraceWarning(String format, params Object[] data)
        {
            this.TraceWarning(this.EventId, format, data);
        }

        public void TraceWarning(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceWarning(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Warning, this.EventId, message);
        }

        #endregion

        #region Trace Error

        public void TraceError(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceError(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Error, eventId, format, data);
        }

        public void TraceError(String format, params Object[] data)
        {
            this.TraceError(this.EventId, format, data);
        }

        public void TraceError(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceError(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Error, this.EventId, message);
        }

        #endregion

        #region Trace Verbose

        public void TraceVerbose(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceInformation(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Verbose, eventId, format, data);
        }

        public void TraceVerbose(String format, params Object[] data)
        {
            this.TraceVerbose(this.EventId, format, data);
        }

        public void TraceVerbose(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceInformation(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Verbose, this.EventId, message);
        }

        #endregion

        #region Trace Critical

        public void TraceCritical(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceError(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Critical, eventId, format, data);
        }

        public void TraceCritical(String format, params Object[] data)
        {
            this.TraceCritical(this.EventId, format, data);
        }

        public void TraceCritical(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceError(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Critical, this.EventId, message);
        }

        #endregion

        #endregion

        TraceSource IntializeTraceSource()
        {
            if (this.internalTraceSource == null)
            {
                this.internalTraceSource = this.TraceSource;
            }
            return internalTraceSource;
        }
    }
}