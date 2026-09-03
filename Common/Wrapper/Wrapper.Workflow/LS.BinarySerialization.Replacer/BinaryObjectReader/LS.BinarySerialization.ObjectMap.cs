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
using System.Collections;
using System.Collections.Generic;

namespace LS.BinarySerialization
{
    internal sealed class ObjectMap
    {
        // Fields
        internal BinaryAssemblyInfo assemblyInfo;
        internal BinaryTypeEnum[] binaryTypeEnumA;
        internal bool isInitObjectInfo;
        internal string[] memberNames;
        internal Type[] memberTypes;
        internal int objectId;
        internal ReadObjectInfo objectInfo;
        internal string objectName;
        internal object objectReader;
        internal Type objectType;
        internal object[] typeInformationA;

        internal ObjectMap(string objectName, Type objectType, 
            string[] memberNames, LSObjectReader objectReader, 
            int objectId, BinaryAssemblyInfo assemblyInfo)
        {
            this.isInitObjectInfo = true;
            this.objectName = objectName;
            this.objectType = objectType;
            this.memberNames = memberNames;
            this.objectReader = objectReader;
            this.objectId = objectId;
            this.assemblyInfo = assemblyInfo;
            //modified
            //this.objectInfo = objectReader.CreateReadObjectInfo(objectType);
            //this.memberTypes = this.objectInfo.GetMemberTypes(memberNames, objectType);
            this.objectInfo = objectReader.CreateReadObjectInfo(objectType);
            this.memberTypes = (Type[])LSInvoker.CallMethod(this.objectInfo, "GetMemberTypes", new Type[] { typeof(string[]), typeof(Type) }, new object[] { memberNames, objectType });
            //
            this.binaryTypeEnumA = new BinaryTypeEnum[this.memberTypes.Length];
            this.typeInformationA = new object[this.memberTypes.Length];
            for (int i = 0; i < this.memberTypes.Length; i++)
            {
                object typeInformation = null;
                this.binaryTypeEnumA[i] = BinaryConverter.GetParserBinaryTypeInfo(this.memberTypes[i], out typeInformation);
                this.typeInformationA[i] = typeInformation;
            }
        }

        internal ObjectMap(string objectName, string[] memberNames, 
            BinaryTypeEnum[] binaryTypeEnumA, object[] typeInformationA, 
            int[] memberAssemIds, LSObjectReader objectReader, 
            int objectId, BinaryAssemblyInfo assemblyInfo,
            SizedArray assemIdToAssemblyTable)
        {
            this.isInitObjectInfo = true;
            this.objectName = objectName;
            this.memberNames = memberNames;
            this.binaryTypeEnumA = binaryTypeEnumA;
            this.typeInformationA = typeInformationA;
            this.objectReader = objectReader;
            this.objectId = objectId;
            this.assemblyInfo = assemblyInfo;
            if (assemblyInfo == null)
            {
                throw new Exception("Serialization_Assembly");
            }
            this.objectType = objectReader.GetType(assemblyInfo, objectName);
            this.memberTypes = new Type[memberNames.Length];
            for (int i = 0; i < memberNames.Length; i++)
            {
                InternalPrimitiveTypeE ee;
                string str;
                Type type=null;
                bool flag;
                BinaryConverter.TypeFromInfo(binaryTypeEnumA[i], typeInformationA[i], objectReader, (BinaryAssemblyInfo)assemIdToAssemblyTable[memberAssemIds[i]], out ee, out str, out type, out flag);
                this.memberTypes[i] = type;
            }
            //Modified
            //this.objectInfo = objectReader.CreateReadObjectInfo(this.objectType, memberNames, null);
            //if (!this.objectInfo.isSi)
            //{
            //    this.objectInfo.GetMemberTypes(memberNames, this.objectInfo.objectType);
            //}
            this.objectInfo =objectReader.CreateReadObjectInfo(objectType, memberNames,null);
            if (!(bool)LSInvoker.GetRawProperty(objectInfo, "isSi"))
            {
                //LSInvoker.CallMethod(objectInfo, "GetMemberTypes", new Type[] { typeof(string[]), typeof(Type) }, new object[] { memberNames, LSInvoker.GetRawProperty(objectInfo, "objectType") });
            }
            //
        }

        internal static ObjectMap Create(string name, Type objectType, string[] memberNames, LSObjectReader objectReader, int objectId, BinaryAssemblyInfo assemblyInfo)
        {
            return new ObjectMap(name, objectType, memberNames, objectReader, objectId, assemblyInfo);
        }

        internal static ObjectMap Create(string name, string[] memberNames, BinaryTypeEnum[] binaryTypeEnumA, object[] typeInformationA, int[] memberAssemIds, LSObjectReader objectReader, int objectId, BinaryAssemblyInfo assemblyInfo, SizedArray assemIdToAssemblyTable)
        {
            return new ObjectMap(name, memberNames, binaryTypeEnumA, typeInformationA, memberAssemIds, objectReader, objectId, assemblyInfo, assemIdToAssemblyTable);
        }

        internal ReadObjectInfo CreateObjectInfo(ref SerializationInfo si, ref object[] memberData)
        {
            if (this.isInitObjectInfo)
            {
                this.isInitObjectInfo = false;
                this.objectInfo.InitDataStore(ref si, ref memberData);
                return this.objectInfo;
            }
            this.objectInfo.PrepareForReuse();
            this.objectInfo.InitDataStore(ref si, ref memberData);
            return this.objectInfo;
        }

 

    }
}
