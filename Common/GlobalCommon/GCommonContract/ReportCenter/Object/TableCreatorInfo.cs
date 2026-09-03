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




namespace AvePoint.GCommon.Contract.ReportCenter.Object
{
    #region using directives
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    /// <summary>
    /// 
    /// TableCreator参数类
    /// 
    /// 约定大于配置原则： table的行列默认等于TableDatas的长宽，
    ///                   TableDatas第一列为表格head列
    ///                   
    /// 
    /// </summary>
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class TableCreatorInfo
    {
        [DataMember]
        public string Title { get; set; }

        [DataMember]
        public string[,] TableDatas { get; set; }

        //每列宽度，单位px，与TableDatas列一一对应
        [DataMember]
        public int[] CellWidth{ get; set; }
    }
}