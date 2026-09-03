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


using System;

namespace AutoInstallationCommon.Utility.Handler
{
    public class Impersonator
    {
        private Impersonator()
        {
        }

        public Impersonator(string username, string domain, string password)
        {
            Username = username;
            Domain = domain;
            Password = password;
        }

        public string Username { get; set; }
        public string Domain { get; set; }
        public string Password { get; set; }

        public bool LogonUser()
        {
            var handle = IntPtr.Zero;
            var result = LogonUser(ref handle);
            return result;
        }

        private bool LogonUser(ref IntPtr handle)
        {
            var logonSucceeded = false;
            logonSucceeded = Win32Wrapper.LogonUserW(Username, Domain, Password, 4, 0, ref handle);
            if (!logonSucceeded) logonSucceeded = Win32Wrapper.LogonUserW(Username, Domain, Password, 2, 0, ref handle);
            return logonSucceeded;
        }
    }
}