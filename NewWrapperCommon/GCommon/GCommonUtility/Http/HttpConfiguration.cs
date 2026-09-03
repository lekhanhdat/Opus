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
    using System.Net.Configuration;
    using System.Reflection;
    using System.Diagnostics;
    #endregion
    /// <summary>
    /// The class intend to config some globle settings of http protocol of .net implementation.
    /// <remarks>
    ///  
    /// </remarks>
    /// </summary>
    public class HttpConfiguration
    {
        /// <summary>
        /// allow the unsafe header parsing.
        /// <remarks>
        /// In some cases, the version 1.1 of HTTP protocol is not implemented strictly follow the RFC 
        /// 2616 or related standards. so maybe you can get an exception message something like below:
        /// HttpWebRequestError: The server committed a protocol violation. Section=ResponseHeader
        /// Detail=CR must be followed by LF.
        /// 
        /// In dot net 1.1, this behavior is allowed, then in dot net 2.0+, there is a enhanced security 
        /// feature for the http protocol, in order to compatible with some web servers, we should 
        /// get a workround for the issue.
        /// 
        /// Besides the hard code for the configuration, you also can use the below config in you app.config
        /// file, such as exe.config and web.config
        /// 
        ///  <configuration> 
        ///    <system.net> 
        ///      <settings> 
        ///       <httpWebRequest useUnsafeHeaderParsing="true" /> 
        ///      </settings> 
        ///    </system.net> 
        ///  </configuration>
        ///</remarks>
        /// </summary>
        public static void AllowUnsafeHeaderParsing()
        {
            var netSettingType = typeof(SettingsSection).Assembly.GetType("System.Net.Configuration.SettingsSectionInternal");
            var settingSectionInternalInstance = netSettingType.GetProperty("Section", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null, null);
            var useUnsafeHeaderParsingField = netSettingType.GetField("useUnsafeHeaderParsing", BindingFlags.NonPublic | BindingFlags.Instance);
            if (useUnsafeHeaderParsingField != null)
            {
                useUnsafeHeaderParsingField.SetValue(settingSectionInternalInstance, true);
                Trace.WriteLine("Http request allow unsafe header parsing by DocAve");
            }
        }
    }
}
