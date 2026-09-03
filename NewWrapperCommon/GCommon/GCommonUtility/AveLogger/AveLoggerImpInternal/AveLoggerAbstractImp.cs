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

namespace AvePoint.GCommon
{
    internal class AveLoggerAbstractImp : IAveLoggerImp
    {
        protected Type loggingType;
        protected string loggerName;

        public virtual AveLogLevel CurrentLogLevel
        {
            get { return AveLogLevel.DEBUG; }
        }

        public virtual bool IsErrorEnabled
        {
            get { return true; }
        }

        public virtual bool IsWarnEnabled
        {
            get { return true; }
        }

        public virtual bool IsInfoEnabled
        {
            get { return true; }
        }

        public virtual bool IsDebugEnabled
        {
            get { return true; }
        }

        public virtual bool IsAgentContext()
        {
            return false;
        }

        public virtual bool IsMediaContext()
        {
            return false;
        }

        public virtual void SetLoggingType(Type type)
        {
            this.loggingType = type;
        }

        public virtual void SetLoggerName(string loggerName)
        {
            this.loggerName = loggerName;
        }

        public virtual void SetJobId(string jobId, bool mergeOldFile)
        {
        }

        public virtual void SetThreadJobId(string jobId)
        {
        }

        public virtual void InitializeInstance()
        {
        }

        public virtual void WaitForAllLogsFlush()
        {
        }

        public virtual void WriteEntry(string msg, AveLogLevel level, int eventId, ushort taskCategory, string eventSource, Exception e)
        {
        }

        public virtual void SetLogLevel(AveLogLevel logLevel)
        {
        }

        public virtual void SeparateLogByTenant(string jobId, string tenantAccountId, string tenantAccountName)
        {
 
        }

        public virtual void SeparateLogByTenant(string tenantAccountId, string tenantAccountName)
        {

        }

        public virtual void SetTreadLogTenantAndJobId(string jobId, string tenantAccountId, string tenantAccountName)
        {
 
        }

        public virtual void SetDeployId(string deployId)
        {
            
        }
    }
}
