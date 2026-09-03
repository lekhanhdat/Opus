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




namespace AvePoint.Media.Core.Index
{
    #region using directives
    using System;
    using System.Collections.Generic;
    using System.Data;
    using AvePoint.Media.Service.DomainModel;
    #endregion

    public interface IIndexProcessor<TIndexProcessorParameter>
        where TIndexProcessorParameter : IndexProcessorParameter
    {
        /// <summary>
        /// 打开默认数据库链接
        /// </summary>
        /// <param name="param"></param>
        void Open(TIndexProcessorParameter param);

        /// <summary>
        /// 检查数据库完整性.
        /// </summary>
        String CheckIntegrity();

        /// <summary>
        /// 关闭数据库连接.
        /// </summary>
        void Close(Boolean isCheckIntegrity = false);

        /// <summary>
        /// 可执行插入、更新、修改操作,如果不使用参数化查询，请把parameters设为null.
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        void Execute(String commandText, Dictionary<String, Object> parameters);

        /// <summary>
        /// 可执行插入、更新、修改操作,将dataTable填充到数据源中。
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="dataTable"></param>
        void Execute(String tableName, DataTable dataTable);

        /// <summary>
        /// 查询方法，如果不使用参数化查询，请把parameters设为null。
        /// 注意：使用时一定要把T设置成数据库表对应的类。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        List<TIndexable> ExecuteQuery<TIndexable>(String commandText, Dictionary<String, Object> parameters)
            where TIndexable : IIndexable;

        /// <summary>
        /// 查询方法，如果不使用参数化查询，请把parameters设为null。
        /// </summary>
        /// <param name="commandText"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        DataTable ExecuteQuery(String commandText, Dictionary<String, Object> parameters);

        /// <summary>
        /// 执行ExecuteScalar查询操作，如果不使用参数化查询，请把parameters设为null.
        /// 注意:这条sql应该且只应该返回一条结果。
        /// </summary>
        /// <param name="sql"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        Object ExecuteScalar(String commandText, Dictionary<String, Object> parameters);

        /// <summary>
        ///  执行ExecuteQuery查询操作，注意:这条sql应该且只应该返回一列结果。
        /// </summary>
        /// <typeparam name="T">要查询的列的数据类型</typeparam>
        /// <param name="commandText">sql语句</param>
        /// <param name="parameters">参数列表，如果不使用参数化查询，请把parameters设为null</param>
        /// <returns>查询结果</returns>
        List<T> ExecuteQueryForOneColume<T>(String commandText, Dictionary<String, Object> parameters)
           where T : class;
        /// <summary>
        ///  执行ExecuteQuery查询操作，注意:这条sql应该且只应该返回一列结果。
        /// </summary>
        /// <typeparam name="T">要查询的列的数据类型</typeparam>
        /// <param name="commandText">sql语句</param>
        /// <param name="parameters">参数列表，如果不使用参数化查询，请把parameters设为null</param>
        /// <returns>查询结果</returns>
        List<Int64> ExecuteQueryForOneColumeInt64(String commandText, Dictionary<String, Object> parameters);

        /// <summary>
        /// 执行ExecuteQuery查询操作.泛型T可以不实现IIndexable接口,所传类型的属性名要和sql语句中要取的数据名一致
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="commandText">sql语句</param>
        /// <param name="parameters">，如果不使用参数化查询，请把parameters设为null</param>
        /// <returns></returns>
        List<T> ExecuteQueryForAllClass<T>(String commandText, Dictionary<String, Object> parameters)
           where T : class;
        /// <summary>
        /// 批量插入方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="indexs"></param>
        void Insert<TIndexable>(List<TIndexable> indexs) where TIndexable : IIndexable;

        /// <summary>
        /// 插入方法。
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sql"></param>
        /// <param name="indexs"></param>
        void Insert<TIndexable>(TIndexable index) where TIndexable : IIndexable;
    }
}