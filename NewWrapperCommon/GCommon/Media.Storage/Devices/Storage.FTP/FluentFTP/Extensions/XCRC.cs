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

namespace AvePoint.Media.Storage.FTP.Wrapper.Extensions
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using AvePoint.Media.Storage.FTP.Wrapper;
    #endregion

    /// <summary>
    /// Implementation of the non-standard XCRC command
    /// </summary>
    public static class XCRC
    {
        delegate string AsyncGetXCRC(string path);
        static Dictionary<IAsyncResult, AsyncGetXCRC> m_asyncmethods = new Dictionary<IAsyncResult, AsyncGetXCRC>();

        /// <summary>
        /// Get the CRC value of the specified file. This is a non-standard extension of the protocol 
        /// and may throw a FtpCommandException if the server does not support it.
        /// </summary>
        /// <param name="client">FtpClient object</param>
        /// <param name="path">The path of the file you'd like the server to compute the CRC value for.</param>
        /// <returns>The response from the server, typically the CRC value. FtpCommandException thrown on error</returns>
        public static string GetXCRC(this WrapperFtpClient client, string path)
        {
            FtpReply reply;

            if (!(reply = client.Execute("XCRC {0}", path)).Success)
                throw new FtpCommandException(reply);

            return reply.Message;
        }

        /// <summary>
        /// Asynchronusly retrieve a CRC hash. The XCRC command is non-standard
        /// and not guaranteed to work.
        /// </summary>
        /// <param name="client">FtpClient Object</param>
        /// <param name="path">Full or relative path to remote file</param>
        /// <param name="callback">AsyncCallback</param>
        /// <param name="state">State Object</param>
        /// <returns>IAsyncResult</returns>
        public static IAsyncResult BeginGetXCRC(this WrapperFtpClient client, string path, AsyncCallback callback, object state)
        {
            AsyncGetXCRC func = new AsyncGetXCRC(client.GetXCRC);
            IAsyncResult ar = func.BeginInvoke(path, callback, state); ;

            lock (m_asyncmethods)
            {
                m_asyncmethods.Add(ar, func);
            }

            return ar;
        }

        /// <summary>
        /// Ends an asynchronous call to BeginGetXCRC()
        /// </summary>
        /// <param name="ar">IAsyncResult returned from BeginGetXCRC()</param>
        /// <returns>The CRC hash of the specified file.</returns>
        public static string EndGetXCRC(IAsyncResult ar)
        {
            AsyncGetXCRC func = null;

            lock (m_asyncmethods)
            {
                if (!m_asyncmethods.ContainsKey(ar))
                    throw new InvalidOperationException("The specified IAsyncResult was not found in the collection.");

                func = m_asyncmethods[ar];
                m_asyncmethods.Remove(ar);
            }

            return func.EndInvoke(ar);
        }
    }
}