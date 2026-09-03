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
using Aspose.Pdf.Operators;
using AvePoint.Common.Portal;
using AvePoint.Cryptography;
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.RA.Common;
using AvePoint.RA.Contract;
using AvePoint.RA.Contract.Archiver;
using AvePoint.RA.Contract.RMWeb;
using AvePoint.RA.Contract.RMWeb.Setting;
using AvePoint.RA.Contract.Tenant;
using AvePoint.RA.DB.Dao;
using AvePoint.RA.DB.Dao.Utility;
using AvePoint.RA.RACommonUtility.Encryption;
using RAExportCommon.VEOExportV2;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AvePoint.RA.Service.Services.Settings
{
    public class SettingProfileService : RMServiceBase, ISettingProfileService
    {
        private ISettingProfilesDao SettingProfileDao => PlatformWindsorManager.GetService<ISettingProfilesDao>();
        private static ILicenseHelperService LicenseHelperService => PlatformWindsorManager.GetService<ILicenseHelperService>();
        private RMAesEncryptorWrapper AesEncryptorWrapper => new();
        public byte[] GetCommunicationEncryptionKey()
        {
            byte[] applicationKey;
            string temp = string.Empty;
            temp = SettingProfileDao.LoadByType(SettingProfilesType.CommunicationEncryptionKey)?.Settings;
            if (string.IsNullOrEmpty(temp))
            {
                applicationKey = KeyGenerateProviderFactory.CreateProvider().GenerateKeyBytes(32);
                string tempSecureString = AesEncryptorWrapper.Encrypt(Convert.ToBase64String(applicationKey));
                temp = SettingProfileDao.GetCommunicationEncryptionKey(tempSecureString);
            }
            applicationKey = Convert.FromBase64String(AesEncryptorWrapper.Decrypt(temp));

            return applicationKey;
        }

        public string GetDBSEEMasterKey()
        {
            string temp = SettingProfileDao.LoadByType(SettingProfilesType.DBSEEMasterKey)?.Settings;
            if (string.IsNullOrEmpty(temp))
            {
                temp = SettingProfileDao.GetDBSEEMasterKey(AesEncryptorWrapper.Encrypt(string.Format("aes256:{0}", new NetworkCredential("", KeyGenerateProviderFactory.CreateProvider().GenerateVisibleKeyString(20)).Password)));
            }
            //mMasterKeyString
            return AesEncryptorWrapper.Decrypt(temp);
        }

        public Task<int> BatchCreateAsync(IEnumerable<SettingProfileDto> profiles)
        {
            return SettingProfileDao.BatchCreateAsync(profiles);
        }

        public Task<int> DeleteMigratedSettingProfilesAsync()
        {
            return SettingProfileDao.DeleteMigratedProfilesAsync();
        }
        public ExportSignatureInfo GetExportSignature()
        {
            ExportSignatureInfo result = new ExportSignatureInfo();
            var exportSignature = SettingProfileDao.LoadByType((int)SettingProfilesType.ExportSignatureInfo);
            if (exportSignature == null)
            {
                ExportSignatureInfo info = new ExportSignatureInfo();
                RSAParameters sharedParameters;
                string publicKey;
                string privateKey;
                using (RSA rsa = RSA.Create())
                {
                    publicKey = rsa.ExportRSAPublicKeyPem();
                    privateKey = rsa.ExportRSAPrivateKeyPem();
                    sharedParameters = rsa.ExportParameters(true);
                }
                string sharedParametersJsonString = JsonSerializer.Serialize(new RsaParametersSerializable(sharedParameters));
                info.PrivateKey = AesEncryptorWrapper.Encrypt(privateKey);
                info.PublicKey = AesEncryptorWrapper.Encrypt(publicKey);
                info.SharedParametersJson = sharedParametersJsonString;
                info.EnableExportSignature = false;

                SettingProfileDto profileDto = new SettingProfileDto();
                profileDto.Id = Guid.NewGuid().ToString();
                profileDto.Name = SettingProfilesType.ExportSignatureInfo.ToString();
                profileDto.Type = (int)SettingProfilesType.ExportSignatureInfo;
                profileDto.Settings = JsonSerializer.Serialize(info);
                SettingProfileDao.Create(profileDto);
                return info;
            }
            else
            {
                ExportSignatureInfo info = new ExportSignatureInfo();
                info = JsonSerializer.Deserialize<ExportSignatureInfo>(exportSignature.Settings);
                return info;
            }
        }
        public async Task<string> UpdateSiteMappingIsOverrideInfo(string isOverride)
        {
            SettingProfileDto profileDto = new SettingProfileDto();
            var getIsOverrideSettingProfile = SettingProfileDao.LoadByType((int)SettingProfilesType.ImportSiteMappingOverrideInfo);
            profileDto.Id = Guid.NewGuid().ToString();
            profileDto.Name = SettingProfilesType.ImportSiteMappingOverrideInfo.ToString();
            profileDto.Type = (int)SettingProfilesType.ImportSiteMappingOverrideInfo;
            profileDto.Settings = isOverride;
            var oldSetting = await UpdateSettingAsync(profileDto);
            return oldSetting.ToString();
        }
        public ExportSignatureInfo GetExportSignatureForVEO()
        {
            var exportSignature = SettingProfileDao.LoadByType((int)SettingProfilesType.ExportSignatureForVEOInfo);
            if (exportSignature == null)
            {
                ExportSignatureInfo info = new ExportSignatureInfo();
                var cert = SHA512WithRSASignature.CreateSelfSignedCertificateForTenant(TenantLocalValue.LogonGroupId);
                var pass = SHA512WithRSASignature.GenerateRandomPassword().ToSecureString();
                var certBytes = cert.Export(X509ContentType.Pfx, pass);
                (string publicKey, string privateKey) = SHA512WithRSASignature.GenerateKeys(certBytes, pass.ToPlainString());
                info.Certificate = AesEncryptorWrapper.Encrypt(certBytes);
                info.Password = AesEncryptorWrapper.Encrypt(pass.ToPlainString());
                info.Thumbprint = cert.Thumbprint;
                info.PrivateKey = AesEncryptorWrapper.Encrypt(privateKey);
                info.PublicKey = AesEncryptorWrapper.Encrypt(publicKey);

                SettingProfileDto profileDto = new SettingProfileDto();
                profileDto.Id = Guid.NewGuid().ToString();
                profileDto.Name = SettingProfilesType.ExportSignatureForVEOInfo.ToString();
                profileDto.Type = (int)SettingProfilesType.ExportSignatureForVEOInfo;
                profileDto.Settings = JsonSerializer.Serialize(info);
                SettingProfileDao.Create(profileDto);
                return info;
            }
            else
            {
                ExportSignatureInfo info = new ExportSignatureInfo();
                info = JsonSerializer.Deserialize<ExportSignatureInfo>(exportSignature.Settings);
                return info;
            }
        }
        public async Task<string> UpdateSettingAsync(SettingProfileDto dto)
        {
            return await SettingProfileDao.UpdateAsync(dto);
        }
        public SettingProfileDto GetProfileDtoByType(SettingProfilesType type)
        {
            var profile = SettingProfileDao.LoadByType(type);
            if (profile != null)
            {
                return StorageDeviceConvert.ConvertSettingProfileToIndexDeviceDto(profile);
            }
            else
            {
                GetExportSignature();
                return null;
            }
        }

        public Task<int> DeleteOverrideProfilesAfterMigrationAsync()
        {
            return SettingProfileDao.DeleteOverrideProfilesAfterMigrationAsync(new int[] { 
                (int)SettingProfilesType.IndexDevice,
                (int)SettingProfilesType.ExportLocationDevice,
                (int)SettingProfilesType.EndUserStubLinkMasterKey,
                (int)SettingProfilesType.DBSEEMasterKey,
                51
            });
        }

        public bool IsEnableArchiverDeduplication()
        {
            if (LicenseHelperService.IsNewOpus().GetAwaiter().GetResult() && LicenseHelperService.HasOpusSPILOrSOLicense)
            {
                var dto = SettingProfileDao.LoadByType(SettingProfilesType.ShowDeduplicateSetting);
                return bool.TryParse(dto?.Settings, out var flag) ? flag : false;
            }
            return false;
        }
    }
}
