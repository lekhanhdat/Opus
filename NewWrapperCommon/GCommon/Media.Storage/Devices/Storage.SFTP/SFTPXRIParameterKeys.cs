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

using System.Diagnostics.CodeAnalysis;
using System.Globalization;

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPXRIParameterKeys.#.cctor()", MessageId = "privatekeysecret")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Media.Storage.SFTP.SFTPXRIParameterKeys.#.cctor()", MessageId = "privatekeypasswordsecret")]
namespace AvePoint.Media.Storage.SFTP
{
    class SFTPXRIParameterKeys
    {
        /**************************For SFTP*********************************/
        public static readonly string SFTP_HOST = "host";

        public static readonly string SFTP_PORT = "port";

        public static readonly string SFTP_PRIVATE_KEY = "privatekeysecret";

        public static readonly string SFTP_PRIVATE_KEY_PASSWORD = "privatekeypasswordsecret";

        public static readonly int SFTP_DEFAULT_PORT = 22;

        public static readonly string SFTPTypekey = "ftpType".ToLower(CultureInfo.InvariantCulture);

        public static readonly string SFTP_RootFolder = "SFTPRootFolder".ToLower(CultureInfo.InvariantCulture);

        public static readonly string SFTP_BufferSize = "SFTPBufferSize".ToLower(CultureInfo.InvariantCulture);

        /**************************For SFTP*********************************/
    }
}
