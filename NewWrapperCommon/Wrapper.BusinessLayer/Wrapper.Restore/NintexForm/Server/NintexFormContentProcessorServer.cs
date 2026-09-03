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
using System.Reflection;
using System.Text;
using System.Xml;
using AvePoint.GCommon;
using AvePoint.Wrapper.Common;

namespace AvePoint.Wrapper.Restore.NintexForm
{
    internal class NintexFormContentProcessorServer : NintexFormContentProcessorBase
    {
        private static AveLogger log = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private readonly Dictionary<Guid, AveNintexFormControlType> nintexFormControlType = new Dictionary<Guid, AveNintexFormControlType>
        {
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
            {new Guid("7733d5bf-11c6-4bdc-a430-79c3065a796c"),AveNintexFormControlType.SqlRequest},
            {new Guid("aeada2b6-24ad-46e2-894f-562c2a01d38a"),AveNintexFormControlType.WebRequest},
            {new Guid("ff9f65fe-f979-4312-a35b-50f0d3769069"),AveNintexFormControlType.ChangeContentType},
            {new Guid("c0a89c70-0781-4bd4-8623-f73675005e21"),AveNintexFormControlType.ExternalDataColumn},
            {new Guid("2c285c16-d4e6-49eb-8a6a-d9aa41e9e71b"),AveNintexFormControlType.ListItem},
            {new Guid("4420d111-8869-49bb-8685-c1b6cdec4873"),AveNintexFormControlType.ListView},
            {new Guid("b612705d-96ee-4824-90e2-4f37ee78a36c"),AveNintexFormControlType.ManagedMetadata},
            {new Guid("2212c7db-a29d-4666-86dd-14e8ad4b3fc9"),AveNintexFormControlType.WorkflowDiagram},
            {new Guid("6eff501c-eebf-43e1-b25c-638a2a6d8791"),AveNintexFormControlType.PageBreakGuide},
        };

        public NintexFormContentProcessorServer(IAveSPWeb web, IAveList list) : base(web, list)
        {
        }
        public NintexFormContentProcessorServer(IAveSPWeb web, IAveList list, bool needContinue) : base(web, list, needContinue)
        { }

        protected override Dictionary<Guid, AveNintexFormControlType> NintexFormControlTypeMapping { get { return nintexFormControlType; } }

        protected override string RemoveUnsupportedFormControl(string formXml)
        {
            return formXml;
        }

        protected override string ReplaceNintexFormContent(XmlDocument xd, string contentTypeId)
        {
            ReplaceContent(xd, UrlReplace);
            return xd.InnerXml;
        }


        private void ReplaceContent(XmlNode node, Func<string, string> UrlReplace)
        {
            XmlElement nodeElement = node as XmlElement;
            if (nodeElement == null)
            {
                return;
            }
            DecodeNodeValue(nodeElement);
            ReplaceUrl(nodeElement, UrlReplace);

            foreach (XmlNode child in nodeElement.ChildNodes)
            {
                ReplaceContent(child, UrlReplace);
            }
        }

        private void DecodeNodeValue(XmlElement nodeElement)
        {
            if (string.Equals("d2p1:Name", nodeElement.Name, StringComparison.OrdinalIgnoreCase)
                || string.Equals("d2p1:Text ", nodeElement.Name, StringComparison.OrdinalIgnoreCase))
            {
                nodeElement.InnerText = System.Web.HttpUtility.HtmlDecode(nodeElement.InnerText);
            }
        }

        private void ReplaceUrl(XmlElement nodeElement, Func<string, string> UrlReplace)
        {
            if (string.Equals("d2p1:ImageUrl", nodeElement.Name, StringComparison.OrdinalIgnoreCase))
            {
                nodeElement.InnerText = UrlReplace(nodeElement.InnerText);
            }
        }

    }
}