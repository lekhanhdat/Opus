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
namespace StandaloneTool.Model.Verify
{
    public enum VerifyResult
    {
        Success = 0,
        FailedWithException = 1,
        AccountNameEmpty = 2,
        PasswordEmpty = 3,
        IncorrectPwdError = 4,
        ZipFilePathError = 5,
        ZipFileContentInvalid = 6,
        PathEmpty = 7,
        PathInvalid = 8,
        AccessPointEmpty = 9,
        ContainerNameEmpty = 10,
        AccountKeyEmpty = 11,
        AzureError = 12,
        SFTPIPEmpty = 13,
        SFTPPortEmpty = 14,
        SFTPFolderEmpty = 15,
        SFTPUsernameEmpty = 16,
        SFTPPasswordEmpty = 17,
        SFTPIPError = 18,
        SFTPPortError = 19,
        SFTPPrivateKeyFileEmpty = 20,
        SFTPPrivateKeyFileInvalid = 21,
        SFTPPrivateKeyPasswordEmpty = 22,
        SFTPPrivateKeyPasswordInvalid = 23,
        SFTPAuthenticationException = 24,
        SFTPFolderPathInvalid = 25,
        NetShareError = 26,
        NetSharePathInvalid = 27,
    }
}
