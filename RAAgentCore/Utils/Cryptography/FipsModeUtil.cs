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

using AvePoint.GCommon;
using AvePoint.Hybrid.Utility.Cryptography.Registry;
using AvePoint.RA.CommonUtil;
using AvePoint.RA.Contract.Services;
using System;
using System.Configuration;
using System.Reflection;

namespace AvePoint.Hybrid.Utility.Cryptography
{
    public class FipsModeUtil
    {
        private static AveLogger logger = new AveLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Initialize ControlService CryptoMode.
        /// </summary>
        public static void InitControlCryptoMode()
        {
            try
            {
                if (GetCryptoModeFromRegistry() == CryptoMode.FIPS || GetCryptoModeFromConfig() == CryptoMode.FIPS)
                {
                    CryptographyManagement.CryptoMode = CryptoMode.FIPS;
                }
                else
                {
                    CryptographyManagement.CryptoMode = CryptoMode.NoneFIPS;
                }
            }
            catch (Exception e)
            {
                logger.Warn(string.Format("Init CryptoMode Error, Exception:{0}", e.ToString()));
                CryptographyManagement.CryptoMode = CryptoMode.NoneFIPS;
            }
        }

        /// <summary>
        /// Read FIPS Status in Registry.
        /// </summary>
        /// <returns></returns>
        public static CryptoMode GetCryptoModeFromRegistry()
        {
            string FIPS_Key = @"System\CurrentControlSet\Control\Lsa\FIPSAlgorithmPolicy";
            string reg = RegistryManager.ReadLocalMachine(FIPS_Key, "Enabled");
            if (reg.Equals(string.Empty))
            {
                string FIPS_Key_03 = @"System\CurrentControlSet\Control\Lsa";
                reg = RegistryManager.ReadLocalMachine(FIPS_Key_03, "FIPSAlgorithmPolicy");
            }
            return reg.Equals("1") ? CryptoMode.FIPS : CryptoMode.NoneFIPS;
        }

        /// <summary>
        /// Read FIPS Status in config file.
        /// </summary>
        /// <returns></returns>
        public static CryptoMode GetCryptoModeFromConfig()
        {
            string config = ConfigurationManager.AppSettings["FipsMode"];

            if(config != null && config.Equals("true", StringComparison.OrdinalIgnoreCase))
            {
                return CryptoMode.FIPS;
            }
            else
            {
                return CryptoMode.NoneFIPS;
            }
        }
    }
}
