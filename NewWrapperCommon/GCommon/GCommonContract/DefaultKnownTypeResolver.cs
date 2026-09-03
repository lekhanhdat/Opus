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
using System.Resources;
using System.IO;
using System.Diagnostics.CodeAnalysis;

[module: SuppressMessage("Microsoft.Naming", "CA1715:IdentifiersShouldHaveCorrectPrefix", Scope = "type", Target = "AvePoint.GCommon.Contract.KnownTypeResolver", MessageId = "I")]
namespace AvePoint.GCommon.Contract
{
    public class DefaultKnownTypeResolver : KnownTypeResolver
    {

        [SuppressMessage("FxCopCustomRules", "C100013:DoNotMissExceptionHandlingInCatchBlocks")]
        private Type GetTypeFromString(string typeName)
        {
            try
            {
                string[] knownStringList = typeName.Split(',');
                Assembly dll = null;
                dll = Assembly.Load(knownStringList[1]);
                Type result = dll.GetType(knownStringList[0]);
                return result;
            }
            catch (Exception)
            {
                try
                {
                    string[] knownStringList = typeName.Split(',');
                    Assembly dllFallback = this.GetType().Assembly;
                    Type result = dllFallback.GetType(knownStringList[0]);
                    return result;

                }
                catch (Exception)
                {
                    return null;

                }
            }
        }


        public void Resovle(Dictionary<Type, List<Type>> knownTypeMap, string filePath)
        {

            Stream m = Assembly.GetExecutingAssembly().GetManifestResourceStream("AvePoint.GCommon.Contract.KnownType.config");
            using (XmlReader reader = XmlReader.Create(m))
            {

                XmlDocument doc = new XmlDocument();
                doc.Load(reader);
                foreach (XmlNode node in doc)
                {
                    if (node.Name.Equals("dataContractSerializer", StringComparison.CurrentCultureIgnoreCase))
                    {
                        foreach (XmlNode declaredTypes in node.ChildNodes)
                        {

                            if (declaredTypes.Name.Equals("declaredTypes", StringComparison.CurrentCultureIgnoreCase))
                            {
                                foreach (XmlNode add in declaredTypes.ChildNodes)
                                {
                                    if (add.Name.Equals("add", StringComparison.CurrentCultureIgnoreCase))
                                    {
                                        string baseName = add.Attributes["type"].Value;

                                        Type baseType = GetTypeFromString(baseName);
                                        if (baseType != null)
                                        {
                                            List<Type> knownTypeList = new List<Type>();
                                            foreach (XmlNode knownNode in add.ChildNodes)
                                            {
                                                if (knownNode.Name.Equals("knownType", StringComparison.CurrentCultureIgnoreCase))
                                                {
                                                    string knownString = knownNode.Attributes["type"].Value;
                                                    Type result = GetTypeFromString(knownString);
                                                    if (result != null && (!knownTypeList.Contains(result)))
                                                    {

                                                        knownTypeList.Add(result);
                                                    }
                                                }
                                            }
                                            if (!knownTypeMap.ContainsKey(baseType))
                                            {
                                                knownTypeMap.Add(baseType, knownTypeList);
                                            }
                                        }
                                    }

                                }

                            }
                        }
                    }
                }

            }

        }

    }
}
