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

namespace Office365GroupRestore
{
    #region directory

    using System;
    using System.Runtime.Serialization;
    
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    using AvePoint.GCommon.Contract.Server.Job.Object;
    using AvePoint.GCommon.Utility;
    using AvePoint.Media.Service.DomainModel;
    using ExchangeCommonWrapper;
    using Job.ModernManagement.Report;

    #endregion

    internal class RestoreCommonUtility
    {
        public static ReportDto CreateReportDto(string title, char type, long size, string path) =>
            new ReportDto()
            {
                Title = title,
                Status = ReportStatus.Success,
                Type = type,
                Size = size,
                Path = path,
                EntityType = JobReportDetailEntityType.Objects,
                Name = title,
                Option = RestoreOption.NewCreated.GetEnumDescription(),
                SourcePath = title,
            };

        public static string GetAgentIndexName(string mailboxAddress, MailboxType mailboxType, bool Is64BitProcess, bool complex = false) =>
            new ExchangeDatabaseNameGenerator().GenerateHashCode(new ExchangeDataBaseInfo { UesrAddress = mailboxAddress, MailboxType = mailboxType, Is64BitProcess = Is64BitProcess }, complex);

        public static Byte[] HexStringToByteArray(String HexString)
        {
            Byte[] ByteArray = new Byte[HexString.Length / 2];
            for (int i = 0; i < HexString.Length; i += 2)
            {
                ByteArray[i / 2] = Convert.ToByte(HexString.Substring(i, 2), 16);
            }
            return ByteArray;
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
    }
}