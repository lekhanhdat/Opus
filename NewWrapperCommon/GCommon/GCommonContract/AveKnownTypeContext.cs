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
using System.Xml;
using System.Reflection;
using System.Diagnostics.CodeAnalysis;

namespace AvePoint.GCommon.Contract
{

    public interface KnownTypeResolver
    {
        void Resovle(Dictionary<Type, List<Type>> knownTypeMap, string filePath);
    }


    /// <summary>
    /// 作为自定义KnownType的解析器，负责管理所有通过配置文件配置的KnownType映射
    /// 在程序启动时需要传入配置文件路径，会通过反射将所有Type从各程序集中载入
    /// 配置文件为标准.net dataContractSerializer config 文件，在支持dataContractSerializer
    /// 自定义配置的地方可以直接使用
    /// </summary>
    public static class AveKnownTypeContext
    {

        private static Dictionary<Type, List<Type>> knownTypeMap = new Dictionary<Type, List<Type>>();
        public static KnownTypeResolver Resolver
        {
            get;
            set;
        }

        private static bool isEnabled = false;
        static AveKnownTypeContext()
        {

        }

        public static void AddKnownType(Type baseType, List<Type> knownTypeList)
        {

            knownTypeMap.Add(baseType, knownTypeList);

        }


        public static void init(string filePath)
        {

            knownTypeMap.Clear();
            if (Resolver != null)
            {
                Resolver.Resovle(knownTypeMap, filePath);

            }
            isEnabled = true;
        }



        [SuppressMessage("FxCopCustomRules", "C100013:DoNotMissExceptionHandlingInCatchBlocks")]
        public static IEnumerable<Type> GetKnonwTypes(Type c)
        {


            if (!isEnabled)
            {
                try
                {
                    Resolver = (KnownTypeResolver)Assembly.GetExecutingAssembly().CreateInstance("AvePoint.GCommon.Contract.DefaultKnownTypeResolver");
                    init(null);

                }
                catch (Exception)
                {

                }
            }


            if (isEnabled)
            {
                List<Type> result;
                try
                {
                    result = knownTypeMap[c];

                }

                catch (Exception)
                {
                    result = new List<Type>();
                }
                return result;
            }
            else
            {

                return new List<Type>();

            }
        }


    }






}
