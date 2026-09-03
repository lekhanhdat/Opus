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

namespace AvePoint.Media.Storage.FTP.Wrapper
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Diagnostics; 
    #endregion

    /// <summary>
    /// Used for transaction logging and debug information.
    /// </summary>
    /// <example>The following example illustrates how to assist in debugging
    /// AvePoint.Media.Storage.FTP.Wrapper by getting a transaction log from the server.
    /// <code source="..\Examples\Debug.cs" lang="cs" />
    /// </example>
    public static class FtpTrace
    {
        static List<TraceListener> m_listeners = new List<TraceListener>();

        static bool m_flushOnWrite = false;

        /// <summary>
        /// Gets or sets whether the trace listeners should be flushed or not
        /// after writing to them. Default value is false.
        /// </summary>
        public static bool FlushOnWrite
        {
            get
            {
                return m_flushOnWrite;
            }
            set
            {
                m_flushOnWrite = value;
            }
        }

        /// <summary>
        /// Add a TraceListner to the collection. You can use one of the predefined
        /// TraceListeners in the System.Diagnostics namespace, such as ConsoleTraceListener
        /// for logging to the console, or you can write your own deriving from 
        /// System.Diagnostics.TraceListener.
        /// </summary>
        /// <param name="listener">The TraceListener to add to the collection</param>
        public static void AddListener(TraceListener listener)
        {
            lock (m_listeners)
            {
                m_listeners.Add(listener);
            }
        }

        /// <summary>
        /// Remove the specified TraceListener from the collection
        /// </summary>
        /// <param name="listener">The TraceListener to remove from the collection.</param>
        public static void RemoveListener(TraceListener listener)
        {
            lock (m_listeners)
            {
                m_listeners.Remove(listener);
            }
        }

        /// <summary>
        /// Write to the TraceListeners.
        /// </summary>
        /// <param name="message">The message to write</param>
        /// <param name="args">Optional variables if using a format string similar to string.Format()</param>
        public static void Write(string message, params object[] args)
        {
            Write(string.Format(message, args));
        }

        /// <summary>
        /// Write to the TraceListeners
        /// </summary>
        /// <param name="message">The message to write</param>
        public static void Write(string message)
        {
            TraceListener[] listeners;

            lock (m_listeners)
            {
                listeners = m_listeners.ToArray();
            }

#if DEBUG
            Debug.Write(message);
            Console.WriteLine(message);
#endif

            foreach (TraceListener t in listeners)
            {
                t.Write(message);

                if (m_flushOnWrite)
                {
                    t.Flush();
                }
            }
        }

        /// <summary>
        /// Write to the TraceListeners.
        /// </summary>
        /// <param name="message">The message to write</param>
        /// <param name="args">Optional variables if using a format string similar to string.Format()</param>
        public static void WriteLine(string message, params object[] args)
        {
            Write(string.Format("{0}{1}", string.Format(message, args), Environment.NewLine));
        }

        /// <summary>
        /// Write to the TraceListeners
        /// </summary>
        /// <param name="message">The message to write</param>
        public static void WriteLine(string message)
        {
            Write(string.Format("{0}{1}", message, Environment.NewLine));
        }
    }
}