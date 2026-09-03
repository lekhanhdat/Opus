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
using AvePoint.GCommon.Contract.Storage.Entity;
using AvePoint.GCommon.Contract.StorageOptimization.Object;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon.Utility.Cryptography;
using AvePoint.Media.Service.DomainModel;
using DataExportCore.Discover.Node;
using DataExportCore.Enum;
using DataExportCore.Report;
using ExchangeCommonWrapper;
using System.Runtime.Serialization;
using System.Text;

namespace DataExportCore.Utils
{
    public static class ConvertUtil
    {
        public static LogicalDeviceDto ConvertStorageDeviceExportDtoToLogicalDeviceDto(RMStorageDeviceInfoExportDto domain)
        {
            // Process decrypt google storage connection string
            domain.ConnectionString = ExportUtility.CustomAesEncryptorWrapper.Decrypt(domain.ConnectionString);
            if(domain.Type == (int)StorageDeviceType.Google)
                DecryptGoogleStorageSecret(domain);
            var physical = new PhysicalDeviceDto()
            {
                Id = domain.Id,
                ConnectionString = domain.ConnectionString,
                ModifyTime = domain.ModifiedTime,
                Type = domain.Type,
                IsSystemStorage = domain.Id.Equals(ExportUtility.AVEPOINT_STORAGE_ID, StringComparison.OrdinalIgnoreCase) || domain.IsSystemStorage
            };

            var logical = new LogicalDeviceDto
            {
                Name = domain.Name,
                PhysicalDrives = [physical],
            };
            return logical;
        }

        private static void DecryptGoogleStorageSecret(RMStorageDeviceInfoExportDto dto)
        {
            string connectionString = dto.ConnectionString;
            string begin = "-----BEGIN PRIVATE KEY-----";
            string end = "-----END PRIVATE KEY-----";
            string[] keyValue = dto.Password[0].Split(new char[] { '=' });
            if (!keyValue[0].EndsWith("tokensecret") && !(keyValue[1].StartsWith(begin) && keyValue[1].Contains(end)))
            {
                var result = CspCrossPlatformExchangeWrapper.UnWrapKey(PhysicalDeviceDto.XRIUtil.ValueDecode(keyValue[1]));
                keyValue[1] = PhysicalDeviceDto.XRIUtil.ValueEncode(Encoding.UTF8.GetString(result, 0, result.Length));
            }
            connectionString = connectionString.Replace(dto.Password[0], keyValue[0] + "=" + keyValue[1]);
            dto.ConnectionString = connectionString;
        }

        public static ArchiverSiteMasterIndexContract ConvertSiteMasterDtoToContract(ArchiverSiteMasterIndexExportDto domain)
        {
            ArchiverSiteMasterIndexContract contract = null;
            if (domain != null)
            {
                contract = new ArchiverSiteMasterIndexContract();
                contract.ArchiverTime = domain.ArchiverTime;
                contract.Id = domain.Id;
                contract.JobId = domain.JobId;
                contract.SiteId = domain.SiteId;
                contract.SiteURL = domain.SiteURL;
            }
            return contract;
        }

        public static ArchiverSiteMasterIndexContract ConvertSiteMasterDtoToContract(CommonSiteMasterIndexExportDto domain)
        {
            ArchiverSiteMasterIndexContract contract = null;
            if (domain != null)
            {
                contract = new ArchiverSiteMasterIndexContract();
                contract.ArchiverTime = domain.ArchiverTime;
                contract.Id = domain.Id;
                contract.JobId = domain.JobId;
                contract.SiteId = domain.SiteId;
                contract.SiteURL = domain.SiteURL;
            }
            return contract;
        }

        public static ExportDetailEntity ConvertToExportDetail(this DiscoverNode node, ExportStatus status, DataType dataType, string comment = "")
        {
            if (node == null) return new();

            return new ExportDetailEntity
            {
                DataType = GetI18NDataTypeValue(dataType),
                Name = node.Index.ItemName,
                SourceURL = node.Url,
                TargetURL = node.ExportPath,
                ObjectLevel = node.ConvertNodeTypeToLevel(),
                ObjectSize = node.ExportedFileSize.ToKBSize().ToString(),
                Status = status.ToString(),
                Comment = comment,
            };
        }

        public static ExportDetailEntity ConvertToExportDetail(this ExchangeDiscoverNode node, ExportStatus status, DataType dataType, string comment = "", string teamsGroupAddress = "", string mailBoxName = "")
        {
            if (node == null) return new();

            return new ExportDetailEntity
            {
                DataType = GetI18NDataTypeValue(dataType),
                Name = node.Index.Name,
                SourceURL = Path.Combine(teamsGroupAddress, mailBoxName, node.Index.Name),
                TargetURL = node.ExportPath,
                ObjectLevel = node.Level.ConvertNodeTypeToLevel(),
                ObjectSize = node.ExportedFileSize.ToKBSize().ToString(),
                Status = status.ToString(),
                Comment = comment,
            };
        }

        private static string GetI18NDataTypeValue(DataType dataType) => dataType switch
        {
            DataType.SharePointOnline => I18NEntity.GetString("SATool_DataType_SharePointOnline"),
            DataType.OneDrive => I18NEntity.GetString("SATool_DataType_OneDrive"),
            DataType.Teams => I18NEntity.GetString("SATool_DataType_Teams"),
            _ => dataType.ToString(),
        };

        public static ExportDetailEntity ConvertToExportDetail(this TeamsDiscoveryNode node, ExportStatus status, DataType dataType, string comment = "", string teamsGroupAddress = "")
        {
            if (node == null) return new();

            return new ExportDetailEntity
            {
                DataType = GetI18NDataTypeValue(dataType),
                Name = node.Index.Name,
                SourceURL = Path.Combine(teamsGroupAddress, node.Index.Name),
                TargetURL = node.ExportPath,
                ObjectLevel = node.Level.ConvertNodeTypeToLevel(),
                ObjectSize = node.ExportedFileSize.ToKBSize().ToString(),
                Status = status.ToString(),
                Comment = comment,
            };
        }

        public static MetadataEntity ConvertToBaseEntity(string entityString)
        {
            try
            {
                return SerializerHelper.DeserializeByDataContractSerializer<MetadataEntity>(entityString);
            }
            catch (SerializationException)
            {
                var eV2 = SerializerHelper.DeserializeByDataContractSerializer<BaseEntityV2>(entityString);
                return eV2.ToBaseEntity();
            }
        }

        public static string ConvertNodeTypeToLevel(this DiscoverNode node)
        {
            switch (node.Type)
            {
                case "Y":
                    return I18NEntity.GetString("SATool_ObjectLevel_App");
                case "E":
                    return I18NEntity.GetString("SATool_ObjectLevel_SiteCollection");
                case "W":
                    return I18NEntity.GetString("SATool_ObjectLevel_Site");
                case "D":
                    return I18NEntity.GetString("SATool_ObjectLevel_Document");
                case "V":
                    return I18NEntity.GetString("SATool_ObjectLevel_DocumentVersion");
                case "I":
                    return I18NEntity.GetString("SATool_ObjectLevel_Item");
                case "U":
                    return I18NEntity.GetString("SATool_ObjectLevel_ItemVersion");
                case "F":
                    return I18NEntity.GetString("SATool_ObjectLevel_Folder");
                case "L":
                    return I18NEntity.GetString("SATool_ObjectLevel_List");
                case "A":
                    return I18NEntity.GetString("SATool_ObjectLevel_Attachment");
                default:
                    return I18NEntity.GetString("SATool_ObjectLevel_None");
            }
        }

        public static string ConvertNodeTypeToLevel(this NodeType type) => type switch
        {
            NodeType.ExchangeOnlineMailbox => I18NEntity.GetString("SATool_ObjectLevel_ExchangeMailBox"),
            NodeType.Mail => I18NEntity.GetString("SATool_ObjectLevel_ExchangeMail"),
            NodeType.Attachment => I18NEntity.GetString("SATool_ObjectLevel_Attachment"),
            NodeType.O365GroupSitesGroup => I18NEntity.GetString("SATool_ObjectLevel_Teams"),
            NodeType.TeamsChannel => I18NEntity.GetString("SATool_ObjectLevel_TeamsChannel"),
            NodeType.Conversation => I18NEntity.GetString("SATool_ObjectLevel_Conversation"),
            _ => I18NEntity.GetString("SATool_ObjectLevel_None")
        };
    }
}
