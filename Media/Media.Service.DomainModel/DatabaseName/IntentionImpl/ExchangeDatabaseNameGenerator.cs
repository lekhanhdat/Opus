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




namespace AvePoint.Media.Service.DomainModel
{
    using AvePoint.GCommon.Contract.ExchangeOnline.ExchangeOnlineMailbox.Object;
    using AvePoint.GCommon.Utility;
    #region using directives
    using System;

    #endregion
    public class ExchangeDatabaseNameGenerator
        : IDatabaseNameGenerator
    {
        public string Generate(DatabaseInfoBase databaseInfo)
        {
            string hashCode = GenerateHashCode(databaseInfo);
            return "index" + hashCode + ".db";
        }

        public string GenerateHashCode(DatabaseInfoBase databaseInfo, bool complex = false)
        {
            var caseSensitive = ExchangeConfigInfo.IsMailboxNameCaseSensitive;
            var exchangeDatabaseInfo = databaseInfo as ExchangeDataBaseInfo;
            var uesrAddress = exchangeDatabaseInfo.UesrAddress;
            var url = uesrAddress.EndsWith("/", StringComparison.OrdinalIgnoreCase) ? uesrAddress.Substring(uesrAddress.Length - 1) : uesrAddress;
            //AOSBR-2601: Add for public folder since pf folder id is case sensitive
            if (!caseSensitive)
            {
                url = url.ToUpper();
            }
            if (complex)
            {
                return HashCodeHelper.ToMD5HashCode(HashCodeHelper.ToMD5HashCode(url) + url);
            }
            if (exchangeDatabaseInfo.MailboxType == MailboxType.PublicFolder ||
                exchangeDatabaseInfo.MailboxType == MailboxType.PublicFolderMetadata)
            {
                return HashCodeHelper.ToMD5HashCode(url);
            }
            var hashCode = (uint)(exchangeDatabaseInfo.Is64BitProcess ? url.GetHashCodeIn64BitProcess() : url.GetHashCodeIn32BitProcess());
            return hashCode.ToString();
        }

        // 使用2次MD5生成Index DB名称
        public string GenerateDatabaseName(DatabaseInfoBase databaseInfo)
        {
            string hashCode = GenerateHashCode(databaseInfo, true);
            return $"index{hashCode}.db";
        }
    }
}
