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
using System.Linq;
using System.Text;


namespace AvePoint.StorageOptimization.Common.Object
{
    public interface IAveFileMigrationToExtender : IDisposable
    {
        /// <summary>
        /// 将参数提供的信息对应的一个文件做FileMigrationToExtender, 异步函数, 调用之后立即返回.
        /// </summary>
        /// <param name="fileInfo">目的端SPFile的信息</param>
        /// <param name="filePath">源端文件相对路径</param>
        /// <param name="callback">回调函数委托</param>
        /// <param name="status">用户自定义的一个对象, 用来区分异步调用, 开始时在异步调用时传入API,结果中将包含这个对象.</param>
        void FileMigrationToExtender(SPFileInfo fileInfo, string filePath,BlobUtilityAsyncCallBack callback, object status);

        /// <summary>
        /// 等待FileMigrationToExtender方法所有线程结束
        /// </summary>
        void WaitComplete();
    }

    public class SPFileInfo
    {
        public Guid WebApplicationId { get; set; }

        public string WebApplicationUrl { get; set; }

        public Guid ContentDatabaseId { get; set; }

        /// <summary>
        /// ContentDatabase name
        /// </summary>
        public string CDBName { get; set; }

        public string ConnectionString { get; set; }

        public Guid SiteId { get; set; }

        public Guid WebId { get; set; }

        public Guid ListId { get; set; }

        public int UIVersion { get; set; }

        public int Level { get; set; }

        public Guid UniqueId { get; set; }

    }
     
    public class UnknowProviderException : Exception
    {
        public UnknowProviderException() { }
        public UnknowProviderException(string msg) : base(msg) { }
    }


    public class UnavailableLogicalDeviceException : Exception
    {
        public UnavailableLogicalDeviceException() { }
        public UnavailableLogicalDeviceException(string msg) : base(msg) { }
    }

    /// <summary>
    /// 回调函数委托
    /// </summary>
    /// <param name="ar">回调函数参数</param>
    /// <returns></returns>
    public delegate void BlobUtilityAsyncCallBack(IBlobUtilityAsyncObj ar);
    /// <summary>
    /// 调用异步多线程的API执行操作的结果状态.
    /// </summary>
    public enum CompleteStatus : byte { Complete, CompleteWithException, Failed, NotDefined }
    /// <summary>
    /// 调用异步多线程的API的执行结果.
    /// </summary>
    public interface IBlobUtilityAsyncObj
    {
        /// <summary>
        /// 执行结果的状态.
        /// </summary>
        CompleteStatus Completed { get; }
        /// <summary>
        /// 用户自定义的一个对象, 用来区分异步调用, 开始时在异步调用时传入API,结果中将包含这个对象.
        /// </summary>
        object AsyncState { get; }
        /// <summary>
        /// 异步执行时内部异常.
        /// </summary>
        Exception InnerException { get; }
    }
}
