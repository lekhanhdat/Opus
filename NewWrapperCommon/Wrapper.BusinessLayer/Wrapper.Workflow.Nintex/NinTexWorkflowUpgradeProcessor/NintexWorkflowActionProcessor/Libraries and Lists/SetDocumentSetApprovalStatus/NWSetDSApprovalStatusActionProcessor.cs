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
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Native13NinTexWorkflowEntity;

namespace LS.SPWorkflowProcessor
{
    class NWSetDSApprovalStatusActionProcessor : NWLibariesAndListsActionProcessor
    {
        public NWSetDSApprovalStatusActionProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "#NintexLive";
        }

        private const string serviceId = "4253e551-7df0-49d8-9237-81d8a7452d5f";

        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.Live = new Live
            {
                ServiceId = serviceId,
                VersionId = "20141001062414",
                ProductId = "Office365SetDocumentSetApprovalStatus"
            };
            configuration.SubscriptionInfo = new SubscriptionInfo
            {
                EndDate = DateTime.Now.AddYears(1),
                Type = "Free",
                ProductId = "Office365SetDocumentSetApprovalStatus"
            };
            configuration.HelpKey = "NL4253E5517DF049D8923781D8A7452D5F";
            configuration.Id = serviceId;
            return configuration;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = string.Format("https://ec.nintex.com/EXT/V1/Icons?type=primary&serviceId={0}", serviceId),
                x49x49 = 0,
                y49x49 = 0,
                x30x30 = 0,
                y30x30 = 0,
                x16x16 = 0,
                y16x16 = 0,
                PreLoadedKey = string.Format("nw_i_{0}", serviceId.Replace("-", "")),
            };
        }

        protected override List<Property> CreateProperties()
        {
            ActivityParameter status = null;
            ActivityParameter message = null;

            foreach (var para in sourceConfig.Parameters)
            {
                if (string.Equals(para.Name, "Status", StringComparison.OrdinalIgnoreCase))
                {
                    status = para;
                }
                else if (string.Equals(para.Name, "Message", StringComparison.OrdinalIgnoreCase))
                {
                    message = para;
                }
            }

            Debug.Assert(status != null, "status != null");
            Debug.Assert(message != null, "message != null");

            //Local 的SetApprovalStatus和365差别比较大，Local只更新CurrentItem的ModerationStatus，365更新指定item(s)的status
            //由于local比较简单，因此我们只在365端创建一个跟local的表现行为相同的action
            //在这里写死action

            var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("Wrapper.Workflow.Nintex.NinTexWorkflowUpgradeProcessor.NintexWorkflowActionProcessor.Libraries_and_Lists.SetDocumentSetApprovalStatus.DocumentSetApprovalStatusTemplate.xml");

            Debug.Assert(stream != null, "stream != null");

            var properties = SerializerHelper.DeserializeObjectFromStream<List<Property>>(stream);

            var userNameProperty = properties[6];
            userNameProperty.Parameters[0].Value.PrimitiveValue.Value.StringValue = this.workflowActionProcessor.Web.Site.UserAccountInfo.UserName;
            userNameProperty.Parameters[0].Value.PrimitiveValue.FormatValues = null;

            properties[7] = base.CreatePasswordProperty("p7", base.workflowActionProcessor.Web.Site.UserAccountInfo.Password, string.Empty);

            var moderationStatus = properties[3];
            moderationStatus.Parameters[0].Value.PrimitiveValue.Value.StringValue = status.PrimitiveValue.Value;
            moderationStatus.Parameters[0].Value.PrimitiveValue.FormatValues = null;

            var comments = properties[4];
            comments.Parameters[0].Value.PrimitiveValue = base.ConvertToPrimitiveValue(message);

            return properties;
        }
    }
}
