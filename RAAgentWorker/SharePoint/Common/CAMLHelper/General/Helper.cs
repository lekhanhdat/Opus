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
using AvePoint.RA.Contract.CodeView;
using System;
using System.Text.RegularExpressions;

namespace AvePoint.RA.SharePoint.Common.CAMLHelper.General
{
    
    /// <summary>
    /// Helper class that provides utility methods.
    /// </summary>
    [RACodeReview("Allen Yin")]
    internal class Helper
    {
        /// <summary>
        /// Regular expression for validating a string as a Guid.
        /// </summary>
        private static readonly Regex isGuid = new Regex(@"^(\{){0,1}[0-9a-fA-F]{8}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{4}\-[0-9a-fA-F]{12}(\}){0,1}$", RegexOptions.Compiled);

        /// <summary>
        /// Check if passed string is a valid Guid.
        /// </summary>
        /// <param name="candidate">String to test as a valid Guid.</param>
        /// <param name="output">Guid variable.</param>
        /// <returns>bool: If the candidate string is a valid Guid</returns>
        internal static bool IsGuid(string candidate, out Guid output)
        {
            bool isValid = false;
            output = Guid.Empty;

            if (!string.IsNullOrEmpty(candidate))
            {
                if (isGuid.IsMatch(candidate))
                {
                    output = new Guid(candidate);
                    isValid = true;
                }
            }
            return isValid;
        }
    }

    [RACodeReview("Allen Yin")]
    internal class SPBuiltInFieldName
    {
        public const string Name = "FileLeafRef";
        public const string DocumentSize = "FileSizeDisplay";
        public const string ModifiedTime = "Modified";
        public const string CreatedTime = "Created";
        public const string CreatedBy = "Author";
        public const string ModifiedBy = "Editor";
        public const string ContentType = "ContentType";
        public const string Title = "Title";
    }
}
