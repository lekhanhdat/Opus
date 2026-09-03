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




namespace AvePoint.Common
{
    #region using directives
    using System;
    using System.Reflection;
using AvePoint.GCommon;
    #endregion

    /// <summary>
    /// Class that provide a generic way to implement singleton design pattern 
    /// </summary>
    /// <typeparam name="T">Class type which is want to implement singleton design pattern</typeparam>
    /// <example>
    /// <code>
    ///      var dbHelper = Singleton{DbHelper}.SingletonInstance
    /// </code>
    /// </example>
    public class Singleton<T> where T : class, ISingleton
    {
        /// <summary>
        /// To get a singleton class instance of a class which has default 
        /// private constructor and implement ISingleton interface 
        /// </summary>
        public static T SingletonInstance { get { return GetSingletonInstance(null); } }

        /// <summary>
        /// To get a singleton class instance of a class which has  
        /// private constructor and implement ISingleton interface 
        /// </summary>
        /// <param name="args"></param>
        /// <returns>The singleton instance of the object</returns>
        public static T GetSingletonInstance(Object[] args) { return NestedClass.GetInstance(args); }

        /// <summary>
        /// This kind of way is use Dot net infrastructure to implement multiple thread security singleton 
        /// other than Double checked locker.
        /// But at last i change the code like this,because last version code may lead some initialize
        /// problems ,also i did not use the static constructor method.
        /// </summary>
        private class NestedClass
        {
            private static AveLogger logger = AveLogger.GetInstance(typeof(NestedClass));
            readonly static Object syncRoot = new Object();
            static T instance;

            /// <summary>
            /// Currently we just catch all the exception and to make the failed invoke as a null reference object
            /// </summary>
            /// <param name="args">the constructor parameters</param>
            /// <returns>the object instance in type of T</returns>
            public static T GetInstance(Object[] args)
            {
                if (instance == null)
                {
                    lock (syncRoot)
                    {
                        if (instance == null)
                        {
                            try
                            {
                                instance = typeof(T).InvokeMember(typeof(T).Name, BindingFlags.CreateInstance | BindingFlags.Instance | BindingFlags.NonPublic, null, null, args) as T;
                            }
                            catch(Exception ex)
                            {
                                logger.Error(ex.ToString());
                            }
                        }
                    }
                }
                return instance;
            }
        }
    }
}
