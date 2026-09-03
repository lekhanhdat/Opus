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
namespace AvePoint.Media.Storage.FTP.Wrapper.Proxy
{
    /// <summary>
    /// Abstraction of an FtpClient with a proxy
    /// </summary>
    public abstract class FtpClientProxy : WrapperFtpClient
    {
		private ProxyInfo _proxy;
        /// <summary> The proxy connection info. </summary>
		protected ProxyInfo Proxy { get { return _proxy;  } }

        /// <summary> A FTP client with a HTTP 1.1 proxy implementation </summary>
        /// <param name="proxy">Proxy information</param>
		protected FtpClientProxy(ProxyInfo proxy)
        {
			_proxy = proxy;
        }

	    /// <summary> Redefine connect for FtpClient : authentication on the Proxy  </summary>
        /// <param name="stream">The socket stream.</param>
        protected override void Connect(FtpSocketStream stream)
        {
            stream.Connect(Proxy.Host, Proxy.Port, InternetProtocolVersions);
        }
    }
}