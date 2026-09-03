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
    #endregion

    /// <summary>
    /// Implementation of the non-standard MD5 command
    /// </summary>
    public static class MD5
    {
        delegate string AsyncGetMD5(string path);
        static Dictionary<IAsyncResult, AsyncGetMD5> m_asyncmethods = new Dictionary<IAsyncResult, AsyncGetMD5>();

        /// <summary>
        /// Gets the MD5 hash of the specified file using MD5. This is a non-standard extension
        /// to the protocol and may or may not work. A FtpCommandException will be
        /// thrown if the command fails.
        /// </summary>
        /// <param name="client">FtpClient Object</param>
        /// <param name="path">Full or relative path to remote file</param>
        /// <returns>Server response, presumably the MD5 hash.</returns>
        public static string GetMD5(this WrapperFtpClient client, string path)
        {
            // http://tools.ietf.org/html/draft-twine-ftpmd5-00#section-3.1
            FtpReply reply;
            string response;

            if (!(reply = client.Execute("MD5 {0}", path)).Success)
                throw new FtpCommandException(reply);

            response = reply.Message;
            if (response.StartsWith(path))
            {
                response = response.Remove(0, path.Length).Trim();
            }

            return response;
        }

        /// <summary>
        /// Asynchronusly retrieve a MD5 hash. The MD5 command is non-standard
        /// and not guaranteed to work.
        /// </summary>
        /// <param name="client">FtpClient Object</param>
        /// <param name="path">Full or relative path to remote file</param>
        /// <param name="callback">AsyncCallback</param>
        /// <param name="state">State Object</param>
        /// <returns>IAsyncResult</returns>
        public static IAsyncResult BeginGetMD5(this WrapperFtpClient client, string path, AsyncCallback callback, object state)
        {
            AsyncGetMD5 func = new AsyncGetMD5(client.GetMD5);
            IAsyncResult ar = func.BeginInvoke(path, callback, state); ;

            lock (m_asyncmethods)
            {
                m_asyncmethods.Add(ar, func);
            }

            return ar;
        }

        /// <summary>
        /// Ends an asynchronous call to BeginGetMD5()
        /// </summary>
        /// <param name="ar">IAsyncResult returned from BeginGetMD5()</param>
        /// <returns>The MD5 hash of the specified file.</returns>
        public static string EndGetMD5(IAsyncResult ar)
        {
            AsyncGetMD5 func = null;

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