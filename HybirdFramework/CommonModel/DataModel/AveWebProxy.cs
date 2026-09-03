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
using System.Net;

namespace HybridCommonModel.DataModel
{
    public class AveWebProxy
    {
        private AveWebProxyOptions _options;
        public AveWebProxy(AveWebProxyOptions options)
        {
            _options = options;
        }

        public IWebProxy Create()
        {
            if (!_options.Enabled)
            {
                return null;
            }

            WebProxy proxy = null;
            if (string.IsNullOrEmpty(_options.UserName))
            {
                proxy = new WebProxy(_options.Host, _options.Port) { UseDefaultCredentials = true };
            }
            else
            {
                var index = _options.UserName.IndexOf('\\');
                if (index > 0)
                {
                    proxy = new WebProxy(_options.Host, _options.Port)
                    {
                        Credentials = new NetworkCredential(_options.UserName.Substring(index + 1), _options.Password, _options.UserName.Substring(0, index))
                    };
                }
                else
                {
                    proxy = new WebProxy(_options.Host, _options.Port)
                    {
                        Credentials = new NetworkCredential(_options.UserName, _options.Password)
                    };
                }
            }

            if (proxy != null) proxy.BypassProxyOnLocal = _options.BypassProxyOnLocal;
            return proxy;
        }
    }

    public class AveWebProxyOptions
    {
        public string Host;
        public int Port;
        public string UserName;
        public string Password;
        public bool Enabled { get; set; }

        public bool BypassProxyOnLocal { get; set; } = true;
    }
}
