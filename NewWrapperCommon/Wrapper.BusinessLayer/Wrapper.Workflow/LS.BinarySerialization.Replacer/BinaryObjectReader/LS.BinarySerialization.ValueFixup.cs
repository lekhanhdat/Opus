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
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Remoting.Messaging;
namespace LS.BinarySerialization
{
    internal sealed class ValueFixup
    {
        // Fields
        internal Array arrayObj;
        internal object header;
        internal int[] indexMap;
        internal string memberName;
        internal object memberObject;
        internal ReadObjectInfo objectInfo;
        internal ValueFixupEnum valueFixupEnum;
        internal static MemberInfo valueInfo;

        internal ValueFixup(Array arrayObj, int[] indexMap)
        {
            this.valueFixupEnum = ValueFixupEnum.Array;
            this.arrayObj = arrayObj;
            this.indexMap = indexMap;
        }

        internal ValueFixup(object memberObject, string memberName, ReadObjectInfo objectInfo)
        {
            this.valueFixupEnum = ValueFixupEnum.Member;
            this.memberObject = memberObject;
            this.memberName = memberName;
            this.objectInfo = objectInfo;
        }

        internal void Fixup(ParseRecord record, ParseRecord parent)
        {
            object pRnewObj = record.PRnewObj;
            switch (this.valueFixupEnum)
            {
                case ValueFixupEnum.Array:
                    this.arrayObj.SetValue(pRnewObj, this.indexMap);
                    return;

                case ValueFixupEnum.Header:
                    {
                        Type type = typeof(Header);
                        if (valueInfo == null)
                        {
                            MemberInfo[] member = type.GetMember("Value");
                            if (member.Length != 1)
                            {
                                throw new SerializationException(string.Format(CultureInfo.CurrentCulture, LSEnvironment.GetResourceString("Serialization_HeaderReflection"), new object[] { member.Length }));
                            }
                            valueInfo = member[0];
                            break;
                        }
                        break;
                    }
                case ValueFixupEnum.Member:
                    if (!this.objectInfo.isSi)
                    {
                        MemberInfo memberInfo = this.objectInfo.GetMemberInfo(this.memberName);
                        if (memberInfo != null)
                        {
                            this.objectInfo.objectManager.RecordFixup(parent.PRobjectId, memberInfo, record.PRobjectId);
                        }
                        return;
                    }
                    this.objectInfo.objectManager.RecordDelayedFixup(parent.PRobjectId, this.memberName, record.PRobjectId);
                    return;

                default:
                    return;
            }
            //Modified
            //FormatterServices.SerializationSetValue(valueInfo, this.header, pRnewObj);
            LSInvoker.CallStaticMethod(Type.GetType("System.Runtime.Serialization.FormatterServices"), "SerializationSetValue",
                new Type[]{typeof(MemberInfo),typeof(object),typeof(object)},
                new object[]{valueInfo,this.header,pRnewObj});
            //
        }



    }
}
