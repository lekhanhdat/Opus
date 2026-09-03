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
using AvePoint.Wrapper.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.Wrapper.Restore.NintexForm
{
    static class NintexFormControlTypeMapping
    {
        public static readonly Dictionary<Guid, AveNintexFormControlType> NintexFormControlType = new Dictionary<Guid, AveNintexFormControlType>
        {
            {new Guid("5f8b447a-4195-485b-9a04-477d7f24be73"),AveNintexFormControlType.Attachment},
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e11"),AveNintexFormControlType.Border },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e09"),AveNintexFormControlType.Button },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e17"),AveNintexFormControlType.CalculatedValue },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e02"),AveNintexFormControlType.Choice },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e03"),AveNintexFormControlType.DateTime },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e19"),AveNintexFormControlType.Geolocation },
            {new Guid("a0c89d70-0781-4bd4-8623-a73675005a05"),AveNintexFormControlType.Hyperlink },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e08"),AveNintexFormControlType.Image },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e00"),AveNintexFormControlType.Label },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e06"),AveNintexFormControlType.MultiLineTextbox },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e13"),AveNintexFormControlType.PageViewer },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e14"),AveNintexFormControlType.Panel },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e04"),AveNintexFormControlType.RichText },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e07"),AveNintexFormControlType.Html },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e12"),AveNintexFormControlType.People },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e16"),AveNintexFormControlType.RepeatingSection },
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e05"),AveNintexFormControlType.SingleLineTextbox },
            {new Guid("6eff501c-eebf-43e1-b25c-638a2a6d8791"),AveNintexFormControlType.PageBreakGuide },
            {new Guid("b612705d-96ee-4824-90e2-4f37ee78a36c"),AveNintexFormControlType.ManagedMetadata},
            {new Guid("{C0A89C70-0781-4BD4-8623-F73675005E15}"),AveNintexFormControlType.SharePointLookup},
        };
     
    }
}
