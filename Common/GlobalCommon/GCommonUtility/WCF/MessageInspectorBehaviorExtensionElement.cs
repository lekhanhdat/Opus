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





namespace AvePoint.GCommon
{
    #region using directives
    using System;
    using System.ServiceModel.Configuration;
    #endregion

    /// <summary>
    /// 这是一个service behavior extension,增加这个extension可以把server接到的soap requst打印出来，同时还可以打印出回复的soap response
    /// <extensions>
    ///     <behaviorExtensions>
    ///         <add name="messageInspectorBehavior" type="AvePoint.GCommon.MessageInspectorBehaviorExtensionElement, CommonUtility, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" />
    ///     </behaviorExtensions>
    /// </extensions>
    /// Please refer to : AgentCommonWcfConfigurations.config
    /// </summary>
    public class MessageInspectorBehaviorExtensionElement : BehaviorExtensionElement
    {
        public override Type BehaviorType
        {
            get { return typeof(MessageInspectorServiceBehavior); }
        }
        protected override object CreateBehavior()
        {
            return new MessageInspectorServiceBehavior();
        }
    }
}
