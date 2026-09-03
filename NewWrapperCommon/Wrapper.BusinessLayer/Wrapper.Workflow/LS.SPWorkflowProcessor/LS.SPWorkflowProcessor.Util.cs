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

using AvePoint.Wrapper.Common;
namespace LS.SPWorkflowProcessor
{
    public enum SPWFAssociationParentType
    {
        Invalid,
        List,
        Web,
        ListContentType,
        WebContentType,
    }

    [Flags]
    public enum SPWFProcessorType
    {
        API,
        API13Model,
        Native,
        Native13Model,
    }

    public enum SharePointVersion
    {
        Invalid = 0,
        SharePoint2007 = 1,
        SharePoint2010 = 2,
        SharePoint2013 = 3,
        SharePoint2016 = 4,
    }

    [Flags]
    public enum LogLevel
    {
        Monitorable,
        Medium,
        High,
        Exception,
        Critical,

    }

    public enum LanguageMappingScopeEnum
    {
        ListTitle,
        FieldName,
        Permission,
    }

    public class LogDirectory
    {
        public const string WFTemplateFile = "Workflow Template File";
    }

    public class Environment
    {
        public static SharePointVersion SharePointVersion
        {
            get
            {
                switch (SPWorkflowProcessorRuntime.ObjectModelFactory.ContextKind)
                {
                    case AveContextKind.Server07ObjectModel:
                        return SharePointVersion.SharePoint2007;
                    case AveContextKind.Server10ObjectModel:
                    case AveContextKind.ServerObjectModel:
                        return SharePointVersion.SharePoint2010;
                    case AveContextKind.Server13ObjectModel:
                        return SharePointVersion.SharePoint2013;
                    case AveContextKind.Server16ObjectModel:
                        return SharePointVersion.SharePoint2016;
                    default:
                        return SharePointVersion.Invalid;
                }
            }
        }
    }

    public enum SPWFInternalPlatform
    {
        Default,
        WF2010PlatformType,
        WF2013PlatformType,
        WFExportedNintex
    }
}
