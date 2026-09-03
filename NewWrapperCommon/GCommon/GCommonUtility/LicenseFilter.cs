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
using System.IO;
using AvePoint.GCommon.Utility.Cryptography;
using System.Xml;
using AvePoint.GCommon.Contract.AveLicense.Detail;
using AvePoint.GCommon.Contract.AveLicense;

namespace AvePoint.GCommon.Utility
{
    public class LicenseFilter
    {
        #region Constants
        private const string AttributeAddDate = "addDate";
        private const string AttributeLicenseGuid = "blackGuid";
        private const string AttributeCompanyName = "companyName";
        private const string AttributeLicenseId = "licenseId";
        private const string ElementName = "item";
        public static readonly string FileName = "ControlMiscInfo.bla";
        #endregion
        private static readonly AveLogger logger = AveLogger.GetInstance(typeof(LicenseFilter));
        private List<InvalidLicense> invalidLicenses = new List<InvalidLicense>();

        public static HashSet<ModuleName> FilteredModules
        {
            get
            {
                HashSet<ModuleName> hideInGUI = new HashSet<ModuleName>()
                {
                        ModuleName.RC_Customize2010, ModuleName.RC_Infrastructure2010 ,
                        ModuleName.RC_StorageOptimization2010,ModuleName.RC_RealtimeMonidtoring2010,
                        ModuleName.RC_ActivityHistory2010,ModuleName.RC_AuditorReports2010, 
                };
                return hideInGUI;
            }
        }

        public static HashSet<ModuleName> SMSPModules
        {
            get
            {
                HashSet<ModuleName> smspModules = new HashSet<ModuleName>()
                {
                    ModuleName.SO_Archiver2010,ModuleName.SO_Archiver2013,
                    ModuleName.SO_Connector2010,ModuleName.SO_Connector2013,
                    ModuleName.SO_RealTimeStorageManager2010,ModuleName.SO_RealTimeStorageManager2013,
                    ModuleName.SO_ScheduledStorageManager2010,ModuleName.SO_ScheduledStorageManager2013,
                    ModuleName.DP_PlatformBackup2010,ModuleName.DP_PlatformBackup2013,
                    //ModuleName.DP_HighAvailability2010,ModuleName.DP_HighAvailability2013,
                    //ModuleName.OF_ArchiverOnline,
                };
                return smspModules;
            }
        }

        public LicenseFilter(string path)
        {
            string xml = ReadThenDecryptContent(path);
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(xml);
            var nodeList = doc.GetElementsByTagName(ElementName);
            foreach (XmlNode node in nodeList)
            {
                invalidLicenses.Add(new InvalidLicense
                    {
                        AddedTime = node.Attributes[AttributeAddDate].Value,
                        CompanyName = node.Attributes[AttributeCompanyName].Value,
                        LicenseGuid = node.Attributes[AttributeLicenseGuid].Value,
                        LicenseId = node.Attributes[AttributeLicenseId].Value
                    });
            }

        }

        private string ReadThenDecryptContent(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException(string.Format("Can not find the file {0}.", path));
            }
            FileStream fs = File.OpenRead(path);
            byte[] buffer = new byte[fs.Length];
            fs.Read(buffer, 0, buffer.Length);
            fs.Close();
            //byte[] decryptedData =
            //    EncryptionFactory.GetDefaultKeyEncryption(EncryptionAlgorithm.AES_ENCRYPTION).DecryptBinary(buffer);
            byte[] decryptedData = CspCrossPlatformExchangeWrapper.UnWrapKeyFromByte(buffer);
            return Encoding.UTF8.GetString(decryptedData);
        }

        public bool IsLicenseValid(string licenseGuid)
        {
            foreach (var invalidLicense in invalidLicenses)
            {
                if (invalidLicense.LicenseGuid == licenseGuid)
                {
                    logger.Warn(invalidLicense.ToString());
                    return false;
                }
            }
            return true;
        }

        public static LicenseDetail FilterLicense(LicenseDetail detail)
        {
            LicenseDetail result = new LicenseDetail(){
                LastModifyTime = detail.LastModifyTime,
                Maintenance = detail.Maintenance,
                ModuleDetails = new Dictionary<ModuleName, LicenseModuleDetail>(),
                PrimaryInfo = detail.PrimaryInfo,
                Status = detail.Status,
                UserSeat = detail.UserSeat
            };
            foreach (var pair in detail.ModuleDetails)
            {
                if (!FilteredModules.Contains(pair.Key))
                    result.ModuleDetails.Add(pair.Key, pair.Value);
            }
            return result;
        }

        class InvalidLicense
        {
            public string LicenseId { get; set; }
            public string LicenseGuid { get; set; }
            public string AddedTime { get; set; }
            public string CompanyName { get; set; }

            public override string ToString()
            {
                return string.Format("Invalid license ID: [{0}], Guid: [{1}], Company: [{2}].", LicenseId, LicenseGuid, CompanyName);
            }
        }
    }
}
