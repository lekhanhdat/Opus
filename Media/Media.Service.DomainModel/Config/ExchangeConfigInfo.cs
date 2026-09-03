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




namespace AvePoint.Media.Service.DomainModel
{
    #region using directives
    using System;
    #endregion

    public class ExchangeConfigInfo
    {

        #region Add for public folder to make case senistive hashcode 
        private static bool? isMailboxNameCaseSensitiveOnProcessLevel;

        [ThreadStatic]
        private static bool? isMailboxNameCaseSensitiveOnThreadLevel;


        /// <summary>
        /// Set CaseSenitiveLevel.Process will overwrite any setting on thread level.
        /// </summary>
        /// <param name="level"></param>
        public static void SetMailboxNameCaseSensitive(bool caseSenitive, CaseSenitiveLevel level)
        {
            switch (level)
            {
                case CaseSenitiveLevel.Thread:
                    isMailboxNameCaseSensitiveOnThreadLevel = caseSenitive;
                    return;
                case CaseSenitiveLevel.Process:
                default:
                    isMailboxNameCaseSensitiveOnProcessLevel = caseSenitive;
                    return;
            }
        }

        public static bool IsMailboxNameCaseSensitive
        {
            get
            {
                if (isMailboxNameCaseSensitiveOnProcessLevel.HasValue) return isMailboxNameCaseSensitiveOnProcessLevel.Value;
                if (isMailboxNameCaseSensitiveOnThreadLevel.HasValue) return isMailboxNameCaseSensitiveOnThreadLevel.Value;
                return false;
            }
        }
        #endregion
    }

    public enum CaseSenitiveLevel
    {
        Process = 0,
        Thread = 1,
    }
}
