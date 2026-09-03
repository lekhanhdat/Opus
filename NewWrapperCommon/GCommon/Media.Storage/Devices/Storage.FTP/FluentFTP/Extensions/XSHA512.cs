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
    using AvePoint.Media.Storage.FTP.Wrapper;
    using System;
    using System.Collections.Generic; 
    #endregion

    /// <summary>
    /// Implementation of the non-standard XSHA512 command
    /// </summary>
    public static class XSHA512 {
        delegate string AsyncGetXSHA512(string path);
        static Dictionary<IAsyncResult, AsyncGetXSHA512> m_asyncmethods = new Dictionary<IAsyncResult, AsyncGetXSHA512>();

        /// <summary>
        /// Gets the SHA-512 hash of the specified file using XSHA512. This is a non-standard extension
        /// to the protocol and may or may not work. A FtpCommandException will be
        /// thrown if the command fails.
        /// </summary>
        /// <param name="client">FtpClient Object</param>
        /// <param name="path">Full or relative path to remote file</param>
        /// <returns>Server response, presumably the SHA-512 hash.</returns>
        public static string GetXSHA512(this WrapperFtpClient client, string path) {
            FtpReply reply;

            if (!(reply = client.Execute("XSHA512 {0}", path)).Success)
                throw new FtpCommandException(reply);

            return reply.Message;
        }

        /// <summary>
        /// Asynchronusly retrieve a SHA512 hash. The XSHA512 command is non-standard
        /// and not guaranteed to work.
        /// </summary>
        /// <param name="client">FtpClient Object</param>
        /// <param name="path">Full or relative path to remote file</param>
        /// <param name="callback">AsyncCallback</param>
        /// <param name="state">State Object</param>
        /// <returns>IAsyncResult</returns>
        public static IAsyncResult BeginGetXSHA512(this WrapperFtpClient client, string path, AsyncCallback callback, object state) {
            AsyncGetXSHA512 func = new AsyncGetXSHA512(client.GetXSHA512);
            IAsyncResult ar = func.BeginInvoke(path, callback, state); ;
            
            lock (m_asyncmethods) {
                m_asyncmethods.Add(ar, func);
            }

            return ar;
        }

        /// <summary>
        /// Ends an asynchronous call to BeginGetXSHA512()
        /// </summary>
        /// <param name="ar">IAsyncResult returned from BeginGetXSHA512()</param>
        /// <returns>The SHA-512 hash of the specified file.</returns>
        public static string EndGetXSHA512(IAsyncResult ar) {
            AsyncGetXSHA512 func = null;

            lock (m_asyncmethods) {
                if (!m_asyncmethods.ContainsKey(ar))
                    throw new InvalidOperationException("The specified IAsyncResult was not found in the collection.");

                func = m_asyncmethods[ar];
                m_asyncmethods.Remove(ar);
            }

            return func.EndInvoke(ar);
        }
    }
}