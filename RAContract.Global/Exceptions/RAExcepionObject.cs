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

namespace AvePoint.RA.Contract.Global.Exceptions
{
    [Serializable]
    public class SkipItemException : Exception
    {
        public SkipItemException(string errorMsg)
            : base(errorMsg)
        {
        }
    }

    [Serializable]
    public class InputParameterException : Exception
    {
        public InputParameterException(string errorMsg)
            : base(errorMsg)
        {
        }
    }

    [Serializable]
    public class TableNotExistsException : Exception
    {
        public TableNotExistsException(string errorMsg)
            : base(errorMsg)
        {
        }
    }

    [Serializable]
    public class ExportConfigZipIllegalException : Exception
    {
        public ExportConfigZipIllegalException(string errorMsg)
            : base(errorMsg)
        {

        }
    }

    [Serializable]
    public class GetSiteFromDAException : Exception
    {
        public GetSiteFromDAException(string errorMsg)
            : base(errorMsg)
        {

        }
    }

    [Serializable]
    public class RelatedRecordsAppDisableExcetion : Exception
    {
        public RelatedRecordsAppDisableExcetion() : base() { }
        public RelatedRecordsAppDisableExcetion(string message) : base(message) { }
    }

    [Serializable]
    public class DenyAddAndCustomizePagesEnableExcetion : Exception
    {
        public DenyAddAndCustomizePagesEnableExcetion() : base() { }
        public DenyAddAndCustomizePagesEnableExcetion(string message) : base(message) { }
    }

    [Serializable]
    public class CancelSuiteAssociationInUsingExcetion : Exception
    {
        public CancelSuiteAssociationInUsingExcetion() : base() { }
        public CancelSuiteAssociationInUsingExcetion(string message) : base(message) { }
    }

    [Serializable]
    public class WorkflowNameConflictException : Exception
    {
        public WorkflowNameConflictException() : base() { }
        public WorkflowNameConflictException(string message) : base(message) { }
    }

    [Serializable]
    public class WorkflowNoConfigReviewerException : Exception
    {
        public WorkflowNoConfigReviewerException() : base() { }
        public WorkflowNoConfigReviewerException(string message) : base(message) { }
    }

    [Serializable]
    public class NotAvailableAgentException : Exception
    {
        public NotAvailableAgentException() : base() { }
        public NotAvailableAgentException(string message) : base(message) { }
    }

    [Serializable]
    public class AgentProcessException : Exception
    {
        public AgentProcessException() : base() { }
        public AgentProcessException(string message) : base(message) { }
        public AgentProcessException(string message, Exception innerException)
           : base(message, innerException) { }
    }

    [Serializable]
    public class AgentNotifyWebApiException : Exception
    {
        public AgentNotifyWebApiException() : base() { }
        public AgentNotifyWebApiException(string message) : base(message) { }
    }

    [Serializable]
    public class SameNameExistException : Exception
    {
        public SameNameExistException() : base() { }
        public SameNameExistException(string message) : base(message) { }
    }
}
