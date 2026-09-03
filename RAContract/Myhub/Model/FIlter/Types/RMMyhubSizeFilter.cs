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
using Amazon.S3.Model.Internal.MarshallTransformations;
using AvePoint.RA.Contract.Explorer;
using FluentFTP.Helpers;
using Microsoft.Data.SqlClient;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.MyHub.Model.FIlter.Types
{
    public class RMMyhubSizeFilter : IRMMyhubFilter
    {
        //如果是metainfo的filesize：现在获取size的逻辑存在问题，SQL不支持JSON_VALUE函数，所以无法直接在SQL里获取到metaInfo的FileSize进行比较
        //如果是jpmcfilesize，实现如下：
        public string Name => "Size";

        public RMMyhubSQLInfo GetSQL(string valueJson)
        {
            var value = JsonConvert.DeserializeObject<RMMyhubDriveSizeFilterValue>(valueJson);
            long unitPower = 1;
            if ((int)value.Unit != (int)RMMyhubDriveSizeFilterUnit.Byte) {
                unitPower = Convert.ToInt64(Math.Pow(1024, (int)value.Unit));
            }
            switch (value.Operator)
            {
                case RMMyhubDriveSizeFilterOperator.LessThan:
                    {
                        var sizeForByte = unitPower * value.Size;
                        return new()
                        {
                            SQL = " AND c.jpmcFileSize < @fileSize ",
                            SQLParameters = [new SqlParameter("@fileSize", sizeForByte)]
                        };
                    }
                case RMMyhubDriveSizeFilterOperator.GreaterThan:
                    {
                        var sizeForByte = unitPower * value.Size;
                        return new()
                        {
                            SQL = " AND c.jpmcFileSize > @fileSize ",
                            SQLParameters = [new SqlParameter("@fileSize", sizeForByte)]
                        };
                    }
                case RMMyhubDriveSizeFilterOperator.BetWeen:
                    {
                        var leftSizeForByte = unitPower * (value.LeftSize - 1);
                        var rightSizeForByte = unitPower * (value.RightSize + 1);
                        return new()
                        {
                            SQL = " AND c.jpmcFileSize > @leftSize AND c.jpmcFileSize < @rightSize ",

                            SQLParameters =
                        [
                            new SqlParameter("@leftSize", leftSizeForByte),
                            new SqlParameter("@rightSize", rightSizeForByte)
                        ]
                        };
                    }
            }
            throw new NotSupportedException($"Unsupported filter option: {value}");
        }

        public RMMyhubPageResult LoadAvaliableValues(RMMyhubPageInfo pageInfo)
        {
            throw new NotImplementedException();
        }

        public enum RMMyhubDriveSizeFilterOperator
        {
            None = 0,
            LessThan = 1,
            GreaterThan = 2,
            BetWeen = 3,
        }
        public enum RMMyhubDriveSizeFilterUnit
        {
            KB = 1,
            MB = 2,
            GB = 3,
            TB = 4,
            PB = 5,
            Byte = 6
        }
        public class RMMyhubDriveSizeFilterValue
        {
            public RMMyhubDriveSizeFilterOperator Operator { get; set; }
            public RMMyhubDriveSizeFilterUnit Unit { get; set; }
            public long Size { get; set; }
            public long LeftSize { get; set; }
            public long RightSize { get; set; }
        }
    }
}
