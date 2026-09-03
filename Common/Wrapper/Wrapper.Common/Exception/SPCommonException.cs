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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Common
{
    [Serializable]
    public class SPCommonException : Exception
    {
        private string m_serverStackTrace;
        private int m_serverErrorCode;
        private string m_serverErrorValue;
        private string m_serverErrorTypeName;
        private object m_serverErrorDetails;
        private string m_serverErrorTraceCorrelationId;
        public string ServerStackTrace
        {
            get
            {
                return this.m_serverStackTrace;
            }
        }
        public int ServerErrorCode
        {
            get
            {
                return this.m_serverErrorCode;
            }
        }
        public string ServerErrorValue
        {
            get
            {
                return this.m_serverErrorValue;
            }
        }
        public string ServerErrorTypeName
        {
            get
            {
                return this.m_serverErrorTypeName;
            }
        }
        public object ServerErrorDetails
        {
            get
            {
                return this.m_serverErrorDetails;
            }
        }
        public string ServerErrorTraceCorrelationId
        {
            get
            {
                return this.m_serverErrorTraceCorrelationId;
            }
        }

        public SPCommonException(string message, string serverStackTrace, int serverErrorCode) : this(message, serverStackTrace, serverErrorCode, null, null, null, null)
        {
        }

        public SPCommonException(string message, string serverStackTrace, int serverErrorCode, string serverErrorValue, string serverErrorTypeName, object serverErrorDetails, string serverErrorTraceCorrelationId) : base(message)
        {
            this.m_serverStackTrace = serverStackTrace;
            this.m_serverErrorCode = serverErrorCode;
            this.m_serverErrorValue = serverErrorValue;
            this.m_serverErrorTypeName = serverErrorTypeName;
            this.m_serverErrorDetails = serverErrorDetails;
            this.m_serverErrorTraceCorrelationId = serverErrorTraceCorrelationId;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(this.ServerStackTrace))
            {
                return base.ToString();
            }
            return this.Message + Environment.NewLine + this.ServerStackTrace;
        }

        public static SPCommonException CreateFromErrorInfo(string message, string serverStackTrace, int serverErrorCode, string serverErrorValue, string serverErrorTypeName, object serverErrorDetails, string serverErrorTraceCorrelationId)
        {
            //need to support more
            //ErrorCode = -2147024809
            //ErrorMessage = Feature '448e1394-5e76-44b4-9e1c-269b7a389a1b' for list template '1101' is not installed in this farm.The operation could not be completed.

            //ErrorCode = -2130239231
            //ErrorMessage = Cannot complete this action. Please try again.

            switch (serverErrorCode)
            {
                case -2147024891:
                    return new SPUnauthorizedAccessException(message, serverStackTrace, serverErrorCode, serverErrorValue, serverErrorTypeName, serverErrorDetails, serverErrorTraceCorrelationId);
                case -2130575300:
                    return new SPUniqueListInstanceException(message, serverStackTrace, serverErrorCode, serverErrorValue, serverErrorTypeName, serverErrorDetails, serverErrorTraceCorrelationId);
                case -2130575342:
                    return new SPListExistException(message, serverStackTrace, serverErrorCode, serverErrorValue, serverErrorTypeName, serverErrorDetails, serverErrorTraceCorrelationId);
            }

            return new SPCommonException(message, serverStackTrace, serverErrorCode, serverErrorValue, serverErrorTypeName, serverErrorDetails, serverErrorTraceCorrelationId);
        }
    }

    [Serializable]
    public class SPUnauthorizedAccessException : SPCommonException
    {
        public SPUnauthorizedAccessException(string message, string serverStackTrace, int serverErrorCode, string serverErrorValue, string serverErrorTypeName, object serverErrorDetails, string serverErrorTraceCorrelationId) : base(message, serverStackTrace, serverErrorCode, serverErrorValue, serverErrorTypeName, serverErrorDetails, serverErrorTraceCorrelationId)
        {
        }
    }

    /// <summary>
    /// 只能创建一个instance
    /// ErrorCode=-2130575300
    /// ErrorMessage=There can only be one instance of this list type in a web. An instance already exists.
    /// ErrorMessage=网站上只能有一个此列表类型的实例。已存在一个实例。
    /// </summary>
    [Serializable]
    public class SPUniqueListInstanceException : SPCommonException
    {
        public SPUniqueListInstanceException(string message, string serverStackTrace, int serverErrorCode, string serverErrorValue, string serverErrorTypeName, object serverErrorDetails, string serverErrorTraceCorrelationId) : base(message, serverStackTrace, serverErrorCode, serverErrorValue, serverErrorTypeName, serverErrorDetails, serverErrorTraceCorrelationId)
        {
        }
    }

    /// <summary>
    /// ErrorCode=-2130575342
    /// ErrorMessage=A list, survey, discussion board, or document library with the specified title already exists in this Web site.  Please choose another title.
    /// </summary>
    [Serializable]
    public class SPListExistException: SPCommonException
    {
        public SPListExistException(string message, string serverStackTrace, int serverErrorCode, string serverErrorValue, string serverErrorTypeName, object serverErrorDetails, string serverErrorTraceCorrelationId) : base(message, serverStackTrace, serverErrorCode, serverErrorValue, serverErrorTypeName, serverErrorDetails, serverErrorTraceCorrelationId)
        {
        }
    }
}
