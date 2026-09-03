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



namespace AvePoint.GCommon.Media.StorageService
{
    #region using directives
    using System;

    #endregion

    internal class DefaultConcordanceCompatibleFormatGenerator
        : ConcordanceCompatibleFormatGeneratorBase
    {
        /// <summary>
        /// Comma ASCII code is 20
        /// </summary>
        protected override Char Comma { get { return '\u0014'; } }

        /// <summary>
        /// Quote ASCII code is 254
        /// </summary>
        protected override Char Quote { get { return '\u00FE'; } }

        /// <summary>
        /// NewLine ASCII code is 174
        /// </summary>
        protected override Char NewLine { get { return '\u00AE'; } }
    }
}