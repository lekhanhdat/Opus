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
using System.Text;
using System.IO;
using AvePoint.GCommon.Contract.Tree.Object;
using System.Xml;
using AvePoint.GCommon.Utility;
using AvePoint.GCommon;

namespace AvePoint.Common
{
    /// <summary>
    /// 用于转换Import Tree
    /// </summary>
    public static class AveImportTreeUtil
    {
        private static AveLogger mLogger = AveLogger.GetInstance(typeof(AveImportTreeUtil));

        ///// <summary>
        ///// 从文件中获取XML，分析TreeType，如果TreeType相同，然后反序列化对应的对象
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="filePath"></param>
        ///// <param name="treeType"></param>
        ///// <returns></returns>
        //public static T GetImportTree<T>(string filePath, TreeType treeType)
        //{
        //    T obj = default(T);

        //    try
        //    {
        //        if (File.Exists(filePath))
        //        {
        //            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
        //            {
        //                obj = GetImportTree<T>(stream, treeType);
        //                stream.Close();
        //            }
        //        }
        //        else
        //        {
        //            mLogger.Info("File:{0} is not found.", filePath);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        mLogger.Error("Get import tree from file:{0} failed:{1}", filePath, ex.ToString());
        //    }

        //    return obj;
        //}

        /// <summary>
        /// 从流中获取XML，分析TreeType，如果TreeType相同，然后反序列化对应的对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="filePath"></param>
        /// <param name="treeType"></param>
        /// <returns></returns>
        public static T GetImportTree<T>(Stream fileStream, TreeType treeType)
        {
            T obj = default(T);

            try
            {
                XmlDocument document = new XmlDocument();
                document.Load(fileStream);
                XmlElement rootElement = document.DocumentElement;
                if (rootElement != null && rootElement.HasAttribute("TreeType"))
                {
                    string treeTypeAttri = rootElement.GetAttribute("TreeType");

                    TreeType treeTypeFromFile = (TreeType)Enum.Parse(typeof(TreeType), treeTypeAttri);

                    if (treeTypeFromFile == treeType)
                    {
                        obj = SerializerHelper.DeserializeFromXmlString<T>(rootElement.InnerText);
                    }
                    else
                    {
                        mLogger.Info("The require type:{0} is not match with the file type:{1} from the file stream.", treeType, treeTypeFromFile);
                    }
                }
            }
            catch (Exception ex)
            {
                mLogger.Error("Get import tree from file stream failed:{0}", ex.ToString());
            }

            return obj;
        }

        /// <summary>
        /// 通过序列化，把TreeType和Obj写到Xml中，返回String
        /// 会抛异常
        /// </summary>
        /// <param name="treeType"></param>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string ConvertImportTree(TreeType treeType, object obj)
        {
            string importTree = string.Empty;

            XmlDocument document = new XmlDocument();
            document.LoadXml("<AgentImportTree />");
            XmlElement rootElement = document.DocumentElement;
            rootElement.SetAttribute("TreeType", treeType.ToString());
            rootElement.InnerText = SerializerHelper.SerializeToXmlString(obj);

            importTree = document.OuterXml;

            return importTree;
        }

        ///// <summary>
        ///// 把Tree写到文件中。 会抛异常，请使用前，模拟用户，可以访问该路径
        ///// </summary>
        ///// <param name="treeType"></param>
        ///// <param name="obj"></param>
        ///// <param name="folder"></param>
        //public static void ConvertImportTreeToFile(TreeType treeType, object obj, string folder)
        //{
        //    string xml = ConvertImportTree(treeType, obj);
        //    File.WriteAllText(Path.Combine(folder, AgentConstants.AgentConfigurationFileName.AgentImportTreeFile), xml, Encoding.UTF8);
        //}
    }
}
