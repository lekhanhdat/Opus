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
using AvePoint.RA.I18N.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Common
{
    public static class RecordBuildInColumnIds
    {
        public const string NameOrUniqueId = "38f015c0-f507-4925-a855-d1546dc0b0f9";
        public const string NameOrTitle = "de5e99cb-4fb4-4e25-b732-a1dce71dd048";
        public const string UniqueId = "c980eb95-ea92-4f07-9f97-1a8ab2a053fa";
        public const string SourceFlag = "edbac887-d4cc-ed92-ad0d-0e68ceb336a0";
        public const string Type = "90c0f7ce-ad79-4a9d-a5eb-3b097006b03d";
        public const string Classification = "ce693d2c-ab58-4d29-9db5-3191bfc5c81a";
        public const string RuleName = "da9dcebc-5628-45b7-9dff-37ca8a601e31";
        public const string RuleAction = "4de03a10-4b33-4091-8929-68be1f7d2325";
        public const string Owners = "38e1e287-4077-44a5-ba57-3de64561c51f";
        public const string HoldStatus = "f9806a66-1be8-4f85-867e-f0de4fa4c073";
        public const string HoldBy = "8499e388-9c52-4366-a7b3-df77c70e648f";
        public const string ActionDueDate = "9117fd6b-4171-4405-b881-cbe139e6ced7";
        public const string CreatedDateInfo = "c55a2cc4-2825-42ff-b1d4-fb72b7be7dc5";
        public const string CreatedBy = "91a08d45-c5dd-43da-b6c4-670f11ac273e";
        public const string ModifiedTime = "3ec9a488-90fa-4d62-835f-0df0cd2e9f97";
        public const string ArchivedTime = "1f525384-e4bf-ed14-78fe-ca9ef9f0d930";
        public const string ModifiedBy = "1f2e8c3f-e49a-473c-bd16-8647258cf15c";
        public const string DeclaredRecord = "bf4e131c-1d9b-403b-8a9f-a1fa3b63cd15";
        public const string FileSystem = "becf61cd-bd6b-440c-8e33-4b6300be58d5";
        public const string LoanBy = "df21d79c-bc37-fdfd-f59e-641f7d630488";
        public const string OnLoan = "b3512f95-198e-c3c9-c2d6-ec21c81e0bae";
        public const string SPOLocation = "ee86426d-488f-4bdb-a63b-2ef6a61c7bef";
        public const string HoldTitle = "3667dc37-36ee-40fd-aee3-7bfe0f80a123";
        public const string HoldUntil = "7e60d9c2-c833-4831-80c3-66c8c36e75fa";
        public const string LockedByRecordLabel = "a8b3c9d1-e2f4-4a5c-9b8d-7e6f5a4c3b21";
        public const string PlaceOnHoldBy = "e2e2e7e2-1c2a-4b7a-9b2e-2e2e7e2e7e2e";

        public static  List<string> BuildInColumns = new List<string>()
        {
            "38f015c0-f507-4925-a855-d1546dc0b0f9",
            "de5e99cb-4fb4-4e25-b732-a1dce71dd048",
            "c980eb95-ea92-4f07-9f97-1a8ab2a053fa",
            "edbac887-d4cc-ed92-ad0d-0e68ceb336a0",
            "90c0f7ce-ad79-4a9d-a5eb-3b097006b03d",
            "ce693d2c-ab58-4d29-9db5-3191bfc5c81a",
            "da9dcebc-5628-45b7-9dff-37ca8a601e31",
            "4de03a10-4b33-4091-8929-68be1f7d2325",
            "38e1e287-4077-44a5-ba57-3de64561c51f",
            "f9806a66-1be8-4f85-867e-f0de4fa4c073",
            "8499e388-9c52-4366-a7b3-df77c70e648f",
            "3667DC37-36EE-40FD-AEE3-7BFE0F80A123",
            "7E60D9C2-C833-4831-80C3-66C8C36E75FA",
            "9117fd6b-4171-4405-b881-cbe139e6ced7",
            "c55a2cc4-2825-42ff-b1d4-fb72b7be7dc5",
            "91a08d45-c5dd-43da-b6c4-670f11ac273e",
            "3ec9a488-90fa-4d62-835f-0df0cd2e9f97",
            "1f2e8c3f-e49a-473c-bd16-8647258cf15c",
            "bf4e131c-1d9b-403b-8a9f-a1fa3b63cd15",
            "becf61cd-bd6b-440c-8e33-4b6300be58d5",
            "df21d79c-bc37-fdfd-f59e-641f7d630488",
            "b3512f95-198e-c3c9-c2d6-ec21c81e0bae",
            "ee86426d-488f-4bdb-a63b-2ef6a61c7bef",
            "1f525384-e4bf-ed14-78fe-ca9ef9f0d930",
            "a8b3c9d1-e2f4-4a5c-9b8d-7e6f5a4c3b21",
            "e2e2e7e2-1c2a-4b7a-9b2e-2e2e7e2e7e2e"
       };


        

        


        

        

        
    }
}
