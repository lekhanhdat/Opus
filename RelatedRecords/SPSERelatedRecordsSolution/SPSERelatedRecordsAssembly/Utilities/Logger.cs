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
using Microsoft.SharePoint;
using Microsoft.SharePoint.Administration;

namespace AvePoint.Opus.RelatedRecords.Utilities 
{ 
    public static class Logger
    {
        private static readonly SPDiagnosticsCategory _defaultCategory =
            new SPDiagnosticsCategory("RelatedRecord",
            TraceSeverity.Medium,
            EventSeverity.Information);

        #region basic funtions

        private static void WriteTrace(string message, TraceSeverity severity = TraceSeverity.Medium)
        {
            WriteTrace(message, _defaultCategory, severity);
        }

        private static void WriteTrace(string message, SPDiagnosticsCategory category, TraceSeverity severity)
        {
            try
            {
                SPDiagnosticsService.Local.WriteTrace(0, category, severity, message, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"ULS Write Failed: {message}. Error: {ex.Message}");
            }
        }

        private static void WriteTrace(uint id, string message, TraceSeverity severity = TraceSeverity.Medium)
        {
            WriteTrace(id, message, _defaultCategory, severity);
        }

        private static void WriteTrace(uint id, string message, SPDiagnosticsCategory category, TraceSeverity severity)
        {
            try
            {
                SPDiagnosticsService.Local.WriteTrace(id, category, severity, message, null);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine($"ULS Write Failed (ID:{id}): {message}. Error: {ex.Message}");
            }
        }

        #endregion

        #region public functions

        public static void LogInfo(string message)
        {
            WriteTrace(message, TraceSeverity.Medium);
        }

        public static void LogWarning(string message)
        {
            WriteTrace(message, TraceSeverity.High);
        }

        public static void LogError(string message)
        {
            WriteTrace(message, TraceSeverity.Unexpected);
        }

        public static void LogError(string message, Exception ex)
        {
            string errorMessage = $"{message}. Exception: {ex.Message}. StackTrace: {ex.StackTrace}";
            WriteTrace(errorMessage, TraceSeverity.Unexpected);
        }

        public static void LogVerbose(string message)
        {
            WriteTrace(message, TraceSeverity.Verbose);
        }

        #endregion

        #region public functions with execution context

        public static void LogWebOperation(SPWeb web, string operation, string details = "")
        {
            string message = $"Web: {web.Url}, Operation: {operation}";
            if (!string.IsNullOrEmpty(details))
                message += $", Details: {details}";

            WriteTrace(message, TraceSeverity.Medium);
        }

        public static void LogListOperation(SPList list, string operation, string details = "")
        {
            string message = $"List: {list.Title}, Web: {list.ParentWeb.Url}, Operation: {operation}";
            if (!string.IsNullOrEmpty(details))
                message += $", Details: {details}";

            WriteTrace(message, TraceSeverity.Medium);
        }

        public static void LogFeatureOperation(SPFeatureReceiverProperties properties, string operation)
        {
            string scope = properties.Feature.Definition.Scope.ToString();
            string message = $"Feature: {properties.Feature.Definition.DisplayName}, Scope: {scope}, Operation: {operation}";
            WriteTrace(message, TraceSeverity.Medium);
        }

        #endregion
    }
}