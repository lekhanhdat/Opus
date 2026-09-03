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
    /// <summary>
    /// 当前这个类所对应的Nintex action：copy to file share，在Office365中没有对应的action，所以这个类暂时不要使用，待以后有对应action后，再完善当前类
    /// </summary>
    class NWCopyFileToFileShareActionProcessor : NWActionProcessorBase
    {
        public NWCopyFileToFileShareActionProcessor(NintexWFActionProcessor workflowActionProcessor) : base(workflowActionProcessor)
        {
            CLASSNAME = "#NintexLive";
        }

        private string serviceId = "";

        protected override Configuration CreateConfiguration()
        {
            var configuration = base.CreateConfiguration();
            configuration.Live = new Live
            {
                ServiceId = "5e1889f7-d8de-47c9-848a-9be58c5cc901",
                VersionId = "20140929034511",
                ProductId = "Office365DownloadFile"
            };
            configuration.SubscriptionInfo = new SubscriptionInfo
            {
                EndDate = DateTime.Now.AddYears(1),
                Type = "Free",
                ProductId = "Office365DownloadFile"
            };
            configuration.HelpKey = "NL0AAF609F0F03425B83F948781BE320EF";
            return configuration;
        }

        protected override Image CreateImage()
        {
            return new Image
            {
                Src = "https://ec.nintex.com/EXT/V1/Icons?type=primary&serviceId=5e1889f7-d8de-47c9-848a-9be58c5cc901",
                x49x49 = 0,
                y49x49 = 0,
                x30x30 = 0,
                y30x30 = 0,
                x16x16 = 0,
                y16x16 = 0,
                PreLoadedKey = string.Format("nw_i_{0}", serviceId.Replace("-", ""))
            };
        }

        protected override List<Property> CreateProperties()
        {
            //Local 的SetApprovalStatus和365差别比较大，Local只更新CurrentItem的ModerationStatus，365更新指定item(s)的status
            //由于local比较简单，因此我们只在365端创建一个跟local的表现行为相同的action
            //在这里写死action

            var stream = Assembly.GetCallingAssembly().GetManifestResourceStream("Wrapper.Workflow.Nintex.NinTexWorkflowUpgradeProcessor.NintexWorkflowActionProcessor.Libraries_and_Lists.CopyFileToFileShare.Office365DownloadFileTemplate.xml");

            Debug.Assert(stream != null, "stream != null");

            var properties = SerializerHelper.DeserializeObjectFromStream<List<Property>>(stream);

            var userNameProperty = properties[2];
            userNameProperty.Parameters[0].Value.PrimitiveValue.Value.StringValue = this.workflowActionProcessor.Web.Site.UserAccountInfo.UserName;
            userNameProperty.Parameters[0].Value.PrimitiveValue.FormatValues = null;

            var passwordProperty = properties[3];
            passwordProperty.Parameters[0].Value.PrimitiveValue.Value.StringValue = this.workflowActionProcessor.Web.Site.UserAccountInfo.Password;
            passwordProperty.Parameters[0].Value.PrimitiveValue.FormatValues = null;

            properties[5].Parameters[0].Value.PrimitiveValue.FormatValues = null;

            return properties;
        }
    }
}
