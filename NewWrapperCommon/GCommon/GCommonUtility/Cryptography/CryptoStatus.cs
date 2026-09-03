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
using System.Text;

namespace AvePoint.GCommon.Utility.Cryptography
{
    public enum CryptoState
    {
        PowerOn = 0,
        PowerOff = 1,
        CryptoOfficer = 2,
        KeyEntry = 3,
        User = 4,
        SelfTest = 5,
        Error = 6,
        Public =7,
        Backward = 8,
        Any = 9
    }

    public enum CryptoEvent
    {

        PowerOnSelfTestFailed = 0,
        PowerOnSelfTestSuccess = 1,
        InitSuccess = 2,
        FinalizeSuccess = 3,
        ConditionalSelfTestFailed = 4,
        ConditionalSelfTestSuccess = 5,
        UserLogonSuccess = 6,
        UserLogonFailed = 7,
        UserLogoffSuccess = 8,
        UserLogoffFailed = 9,
        CryptoOfficerLogonSuccess = 10,
        CryptoOfficerLogonFailed = 11,
        CryptoOfficerLogoffSuccess = 12,
        CryptoOfficerLogoffFailed = 13,
        EnterKeyBegin = 14,
        EnterKeySuccess = 15,
        EnterKeyFailed = 16,
        CryptoReSelfTest = 17


    }

    public enum CryptoMode
    {
        NoneFIPS = 0,
        FIPS = 1,
    }

    public enum EncryptionMode
    {
        ENCRYPTION = 0,
        DECRYPTION = 1,
    }

    public enum EncryptionAlgorithm
    {
        NONE= -1,
        BLOWFISH_ENCRYPTION = 0,
        AES_ENCRYPTION = 1,
        DES_ENCRYPTION = 2,
    }


    public enum HashAlgorithm
    {
        SHA1 = 0,
        HMACSHA1 = 1,
        MD5 = 2,
        HMASHA256 = 3,
        SHA256 = 4,
        SHA384 = 5,
        SHA512 = 6,
    }

    public enum AsymmetricEncryptionAlgorithm
    {
        RSA = 0,
        

    }
}
