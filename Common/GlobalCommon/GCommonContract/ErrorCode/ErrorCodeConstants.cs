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
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.GCommon.Contract.ErrorCode
{
    public enum SourceType
    {
        ReportCommentKey,

        ErrorMessage,

        /// <summary>
        /// If the error code need to parse based on the I18N exception, please add it to the ErrorCode attribute of I18N exception directly 
        /// </summary>
        I18NException
    }

    /// <summary>
    /// For the ErrorCode display name, the length need less than 100 characters, 
    /// and the format is: ModelPrefix + "-" + ErrorDescription, the ModelPrefix as below: 
    ///     Common => CO; 
    ///     Site(include SharePoint, OneDrive, Project, Group/Team site) => SP; 
    ///     Exchange => EX; 
    ///     Group => TG; 
    ///     Teams => TM;
    /// </summary>
    public enum TroubleshootingErrorCode
    {
        [DisplayName("")]
        Default,

        #region Common error code
        [DisplayName("CO-Throttling")]
        CO_Throttling,

        [DisplayName("CO-NotFound")]
        CO_NotFound,

        [DisplayName("CO-IncorrectUserNameOrPassword")]
        CO_IncorrectUserNameOrPassword,
        #endregion

        #region Site error code
        [DisplayName("SP-SiteLocked")]
        SP_SiteLocked,

        [DisplayName("SP-SiteReadOnly")]
        SP_SiteReadOnly,

        [DisplayName("SP-PDFBackupFailedDueToIRM")]
        SP_PDFBackupFailedDueToIRM,

        [DisplayName("SP-FileBackupFailedDueToVirusScanner")]
        SP_FileBackupFailedDueToVirusScanner,

        [DisplayName("SP-WebPartNotExist")]
        SP_WebPartNotExist,

        [DisplayName("SP-SiteNotExist")]
        SP_SiteNotExist,

        [DisplayName("SP-CannotCreateSubsite")]
        SP_CannotCreateSubsite,

        [DisplayName("SP-SkipBackupRecordingsFolder")]
        SP_SkipBackupRecordingsFolder,

        [DisplayName("SP-IRMProtectedFileFailed")]
        SP_IRMProtectedFileFailed,
        #endregion
    }

    public static class ErrorCodeConstants
    {
        internal static Dictionary<TroubleshootingErrorCode, Dictionary<SourceType, List<string>>> ErrorCodeSources
        {
            get
            {
                return ErrorCodeSource.ErrorCodeSource_Common
                    .Concat(ErrorCodeSource.ErrorCodeSource_Site)
                    .Concat(ErrorCodeSource.ErrorCodeSource_Exchange)
                    .Concat(ErrorCodeSource.ErrorCodeSource_Group)
                    .Concat(ErrorCodeSource.ErrorCodeSource_Teams)
                    .ToDictionary(kv => kv.Key, kv => kv.Value);
            }
        }

    }

    /// <summary>
    /// The actual displayed troubleshooting error code
    /// </summary>
    public class DisplayNameAttribute : Attribute
    {
        public string DisplayName { get; set; }
        public DisplayNameAttribute(string displayName)
        {
            this.DisplayName = displayName;
        }
    }

    public static class ErrorCodeHelper
    {
        public static string ToDisplayName(this TroubleshootingErrorCode errorCode)
        {
            FieldInfo field = errorCode.GetType().GetField(errorCode.ToString());
            object[] objs = field.GetCustomAttributes(typeof(DisplayNameAttribute), false);
            if (objs == null || objs.Length == 0)
                return errorCode.ToString();

            return (objs[0] as DisplayNameAttribute).DisplayName;
        }
    }

}