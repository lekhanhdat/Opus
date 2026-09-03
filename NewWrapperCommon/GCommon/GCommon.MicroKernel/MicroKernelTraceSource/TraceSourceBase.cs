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
    ///<Summary>
    /// Provide a abstract implement of the ITraceSoure interface.
    /// 
    /// <remarks>
    ///     There is a cached weak reference list in tracesource class may lead to memory
    ///     leak, you can see the tracesource class's constructor implementation as follows:
    ///     
    ///  <code>
    ///     static List<WeakReference/> tracesources = new List<WeakReference/>();
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
    [DebuggerNonUserCode]
    #endregion


    public abstract class TraceSourceBase : ITraceSource
    {
        TraceSource internalTraceSource;

        /// <summary>
        /// 
        /// </summary>
        protected abstract TraceSource TraceSource { get; }

        /// <summary>
        /// 
        /// </summary>
        protected abstract Int32 EventId { get; }

        #region ITraceSource Members

        #region Trace Infromation

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceInformation(Int32 eventId, String format, params Object[] data)
        {
            try
            {
                this.IntializeTraceSource();
                Trace.TraceInformation(String.Format(format, data));
                this.internalTraceSource.TraceEvent(TraceEventType.Information, eventId, format, data);
            }
            catch (Exception e)
            {
                Trace.TraceInformation(String.Format("An error occurred while trace info,details:{0}.", e.Message));
            }
        }


        // ReSharper disable MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceInformation(String format, params Object[] data)
        // ReSharper restore MethodOverloadWithOptionalParameter
        {
            this.TraceInformation(this.EventId, format, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void TraceInformation(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceInformation(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Information, this.EventId, message);
        }

        #endregion

        #region Trace Warning

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceWarning(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceWarning(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Warning, eventId, format, data);
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceWarning(String format, params Object[] data)
        // ReSharper restore MethodOverloadWithOptionalParameter
        {
            this.TraceWarning(this.EventId, format, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void TraceWarning(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceWarning(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Warning, this.EventId, message);
        }

        #endregion

        #region Trace Error

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceError(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceError(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Error, eventId, format, data);
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceError(String format, params Object[] data)
        // ReSharper restore MethodOverloadWithOptionalParameter
        {
            this.TraceError(this.EventId, format, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void TraceError(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceError(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Error, this.EventId, message);
        }

        #endregion

        #region Trace Verbose

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceVerbose(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceInformation(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Verbose, eventId, format, data);
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceVerbose(String format, params Object[] data)
        // ReSharper restore MethodOverloadWithOptionalParameter
        {
            this.TraceVerbose(this.EventId, format, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void TraceVerbose(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceInformation(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Verbose, this.EventId, message);
        }

        #endregion

        #region Trace Critical

        /// <summary>
        /// 
        /// </summary>
        /// <param name="eventId"></param>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceCritical(Int32 eventId, String format, params Object[] data)
        {
            this.IntializeTraceSource();
            Trace.TraceError(String.Format(format, data));
            this.internalTraceSource.TraceEvent(TraceEventType.Critical, eventId, format, data);
        }

        // ReSharper disable MethodOverloadWithOptionalParameter
        /// <summary>
        /// 
        /// </summary>
        /// <param name="format"></param>
        /// <param name="data"></param>
        public void TraceCritical(String format, params Object[] data)
        // ReSharper restore MethodOverloadWithOptionalParameter
        {
            this.TraceCritical(this.EventId, format, data);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="message"></param>
        public void TraceCritical(String message)
        {
            this.IntializeTraceSource();
            Trace.TraceError(message);
            this.internalTraceSource.TraceEvent(TraceEventType.Critical, this.EventId, message);
        }

        #endregion

        #endregion

        /// <summary>
        /// 
        /// </summary>
        void IntializeTraceSource()
        {
            if (this.internalTraceSource == null)
            {
                this.internalTraceSource = this.TraceSource;
            }
        }
    }
}