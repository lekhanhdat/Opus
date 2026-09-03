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
namespace AvePoint.GCommon.Utility.Cryptography
{
    using Microsoft.Win32;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("Interoperability", "CA1416:Validate platform compatibility", Justification = "Reviewed by wbhu,This method have check and run it only on windows.")]
    internal class FipsCheckerWindows : FipsCheckerEmpty, IFipsChecker
    {
        /// <summary>
        /// read sub key from local machine registry sub key
        /// </summary>
        /// <param name="subKey">subkey name</param>
        /// <param name="valueName">value name</param>
        /// <returns>result string value</returns>
        public static string ReadLocalMachine(string subKey, string valueName)
        {
            var result = string.Empty;
            using (var key = Registry.LocalMachine.OpenSubKey(subKey))
            {
                if (key != null)
                {
                    var value = key.GetValue(valueName);
                    result = value == null ? string.Empty : value.ToString();
                }
            }
            return result;
        }
        public override CryptoMode GetCryptoModeFromRegistry()
        {
            string FIPS_Key = @"System\CurrentControlSet\Control\Lsa\FIPSAlgorithmPolicy";
            string reg = ReadLocalMachine(FIPS_Key, "Enabled");
            if (reg.Equals(string.Empty))
            {
                string FIPS_Key_03 = @"System\CurrentControlSet\Control\Lsa";
                reg = ReadLocalMachine(FIPS_Key_03, "FIPSAlgorithmPolicy");
            }
            return reg.Equals("1") ? CryptoMode.FIPS : CryptoMode.NoneFIPS;
        }
    }
}