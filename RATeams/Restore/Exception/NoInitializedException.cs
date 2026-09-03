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


namespace Office365GroupRestore.Worker
{
    #region
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
    #endregion

    public class NoInitializedException : Exception
    {
        public NoInitializedException() :
            this(RestoreConstants.NOINITIALIZEMESSAGE)
        { }
        public NoInitializedException(string message) :
            base(message)
        { }
        public NoInitializedException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }

    public class JobFailedException : Exception
    {
        public JobFailedException(string message) :
            base(message)
        { }
        public JobFailedException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }

    public class DataBlockException : Exception
    {
        public DataBlockException(string message) :
            base(message)
        { }
        public DataBlockException(string message, Exception innerException) :
            base(message, innerException)
        { }
    }

}