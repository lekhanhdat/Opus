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

namespace AvePoint.Wrapper.Common
{
    public static class AveFieldNameCollection
    {
        public const string Guid_Field = "GUID";
        public const string UniqueId_Field = "UniqueId";
        public const string FileDirRef_Field = "FileDirRef";
        public const string Id_Field = "Id";
    }
    public static class OnlineFieldId
    {
        public readonly static List<Guid> NeedSkipOnlineField = new List<Guid>
        {
            new Guid("ccc1037f-f65e-434a-868e-8c98af31fe29"),
            new Guid("14ee99cd-bed9-474a-bf99-8f753fbad6b4"),
            new Guid("0b16648a-daff-47d4-9fda-c6038b75ed27"),
            new Guid("d48268e5-c65d-486c-bbf1-874cf986d7d3"),
            new Guid("d4b6480a-4bed-4094-9a52-30181ea38f1d"),
            new Guid("92be610e-ddbb-49f4-b3b1-5c2bc768df8f"),
            new Guid("418d7676-2d6f-42cf-a16a-e43d2971252a"),
            new Guid("052d75ed-7afd-4818-a346-d7e413073907"),
            new Guid("142b1fa5-b93b-42d8-a37c-813c9d3e3f3a"),
            new Guid("d6d3555c-2f57-4b19-bd39-4582f88adfe5"),
            new Guid("d1faf7fe-868c-4748-bfee-e608a37721fb"),
            new Guid("b4cb04e8-622e-4c7d-8e87-b558a1bb907b"),
            new Guid("df7ffe41-81d6-46eb-8777-444d1613c803"),
            new Guid("32d407ed-15e1-4ccc-b1d4-c56f5799b256"),
            new Guid("c4b1727e-aca8-4bd8-ae83-f554ae3c08eb"),
            new Guid("c274cbfd-084a-4017-925f-cce50c9e3eec"),
            new Guid("d307dff3-340f-44a2-9f4b-fbfe1ba07459"),
            new Guid("db8d9d6d-dc9a-4fbd-85f3-4a753bfdc58c"),
            new Guid("4df6bfaf-f887-424e-8ea3-fd050113e7a9"),
            new Guid("d340fca5-f503-4baa-bae9-90f1447ebff6"),
            new Guid("1faa4902-9115-44b9-bba7-791441ca1d6f"),
            new Guid("a261b12a-8ca2-47fa-a117-05861d637c7e"),
            new Guid("3a6b296c-3f50-445c-a13f-9c679ea9dda3"),
            new Guid("8382d247-72a9-44b1-9794-7b177edc89f3"),
            new Guid("2662ad77-2410-4938-b01c-e5e43321bad4"),
            new Guid("e8fea999-553d-4f45-be52-d941627e9fe5"),
            new Guid("47b1b86f-9f8a-4dbe-a75e-ca5d9b0f566c"),
            new Guid("e2a3861f-c216-47d7-820f-7cb638862ab2"),
            new Guid("786099e5-d20a-4232-86e5-cfc3d6face96"),
            new Guid("1FAA4902-9115-44B9-BBA7-791441CA1D6F"),//此field 不是365 特有，这种built in 的column 也不应该还原。在365 to local 时，更改此field 会导致library 不可用，暂时将逻辑放到这。如果有其他问题，可以针对365 to local 的case 再添加新的逻辑判断过滤
            new Guid("A261B12A-8CA2-47FA-A117-05861D637C7E"),
            new Guid("aa445880-c723-44f1-ab01-61083d60cb2e")
        };
    }
}
