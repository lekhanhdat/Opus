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
using AvePoint.RA.Contract.TemplateManagement;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Object
{
    public class PatchRecord
    {
        /// <summary>
        /// 单次partial update操作时的最大Property更新个数，此数字是Cosmos DB中的限制
        /// </summary>
        private static int maxOperationNumber = 10;
        /// <summary>
        /// id in cosmos db
        /// </summary>
        public Guid Id;
        /// <summary>
        /// create date of the record, it is the partition key in cosmos db
        /// </summary>
        public int CreateDate;

        [CosmosPath(Path = CosmosFieldPath.ContainerId)]
        public string ContainerId { get; set; }

        [CosmosPath(Path = CosmosFieldPath.TemplateId)]
        public int? TemplateId { get; set; }

        [CosmosPath(Path = CosmosFieldPath.LoanedBy, ColumnType = ColumnType.PeopleOrGroup)]
        public List<AOSUserDto> LoanedBy { get; set; }


        /// <summary>
        /// fields to be updated, should contains no more than 10 elements
        /// </summary>
        public List<PatchRecordField> Fields { get; set; }

        /// <summary>
        /// check by id, partition key and PatchRecordField.
        /// </summary>
        public void AssertByFields()
        {
            AssertKey();
            int fieldCount = this.Fields != null? this.Fields.Count: 0;
            AssertFieldCount(fieldCount);
        }

        /// <summary>
        /// check by id, partition key and Properties.
        /// </summary>
        public void Assert()
        {
            AssertKey();
            int fieldCount = GetPathAndValues().Count();
            AssertFieldCount(fieldCount);

        }

        private void AssertKey()
        {
            if (this.Id == Guid.Empty) throw new ArgumentException("Id can't be empty");
            if (this.CreateDate <= 0) throw new ArgumentException("CreateDate is wrong");
        }
        /// <summary>
        /// 检查要更新的Property的个数是否超出了限制
        /// </summary>
        /// <param name="count"></param>
        private void AssertFieldCount(int count)
        {
            if (count == 0) throw new ArgumentException("Fields is empty");
            if (count > maxOperationNumber) throw new ArgumentException($"Fields count exceeds the maximum size: {maxOperationNumber}");
        }

        /// <summary>
        /// 得到设置了CosmosPathAttribute并且有赋值的Property的list，
        /// 每个list元素是个三元组，Item1是在Property在Cosmos DB中对于的path，Item2是对应的值, 如果是custom column，那么Item3是对应的Custom column type
        /// </summary>
        /// <returns></returns>
        public List<(string, object, Contract.TemplateManagement.ColumnType)> GetPathAndValues()
        {
            var paths = new List<(string, object, Contract.TemplateManagement.ColumnType)>();
            var t = this.GetType();
            var props = t.GetProperties();
            foreach(var p in props)
            {
                var att = p.GetCustomAttributes(typeof(CosmosPathAttribute), true).FirstOrDefault();
                if (att == null) continue;
                var v = p.GetValue(this);
                if (v == null) continue;
                var cosmosAttr = (att as CosmosPathAttribute);
                var path = cosmosAttr.Path;
                if (!string.IsNullOrEmpty(path?.Trim())) paths.Add((path, v, cosmosAttr.ColumnType));
            }
            return paths;
        }
    }

    public enum PatchRecordOperation
    {
        Set
    }

    public class PatchRecordField
    {
        public RecordFieldName FieldName { get; set; }
        public object FieldValue { get; set; }

        public PatchRecordOperation Operation { get; set; } = PatchRecordOperation.Set;

        /// <summary>
        /// this is only valid for custom column
        /// </summary>
        public ColumnType? CustomColumnType { get;set; }
    }
}
