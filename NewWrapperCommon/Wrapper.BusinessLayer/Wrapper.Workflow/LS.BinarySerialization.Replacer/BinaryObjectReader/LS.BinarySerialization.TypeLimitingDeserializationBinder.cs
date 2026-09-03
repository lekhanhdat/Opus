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
using System.Configuration.Assemblies;
using System.Reflection;
using System.Resources;
using System.Runtime.Serialization;

namespace LS.BinarySerialization
{
    internal sealed class TypeLimitingDeserializationBinder : SerializationBinder
    {
        // Fields
        private LSObjectReader _objectReader;
        private Type _typeToDeserialize;

        internal LSObjectReader ObjectReader
        {
            get
            {
                return this._objectReader;
            }
            set
            {
                this._objectReader = value;
            }
        }

        public override Type BindToType(string assemblyName, string typeName)
        {
            AssemblyName name = new AssemblyName();
            Assembly assembly = this._typeToDeserialize.Assembly;
            name.Init(assembly.nGetSimpleName(), assembly.nGetPublicKey(), null, null, assembly.GetLocale(), AssemblyHashAlgorithm.None, AssemblyVersionCompatibility.SameMachine, null, AssemblyNameFlags.PublicKey, null);
            bool flag = false;
            foreach (string str in LSResourceReader.TypesSafeForDeserialization)
            {
                if (LSResourceManager.CompareNames(str, typeName, name))
                {
                    flag = true;
                    break;
                }
            }
            if (this.ObjectReader.FastBindToType(assemblyName, typeName).IsEnum)
            {
                flag = true;
            }
            if (!flag)
            {
                throw new BadImageFormatException("BadImageFormat_ResType&SerBlobMismatch");
            }
            return null;
        }

 

    }
}
