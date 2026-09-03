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
namespace Microsoft365.Graph.Util
{
    public static class GraphCommonUtil
    {
        public static List<IMapiExtendedPropertyDefinition> CommonExtendedProperties = new List<IMapiExtendedPropertyDefinition>
        {
            OutlookExtendedProperties.PidTagSubject,
            OutlookExtendedProperties.PidTagInternetMessageId,
            OutlookExtendedProperties.PidTagHasAttachments,
            OutlookExtendedProperties.PidTagSenderName,
            OutlookExtendedProperties.PidTagSenderSmtpAddress,
            OutlookExtendedProperties.PidTagReceiverSmtpAddress,
            OutlookExtendedProperties.PidTagDisplayTo,
            OutlookExtendedProperties.PidTagDisplayCc,
            OutlookExtendedProperties.PidTagImportance,
            OutlookExtendedProperties.PidDeliveryTime,
            OutlookExtendedProperties.PidTagModifiedBy,
            OutlookExtendedProperties.PidTagClientSubmitTime,
            OutlookExtendedProperties.PidTagMessageFlags,
            OutlookExtendedProperties.PidNameMSIPLabels,
            OutlookExtendedProperties.PidComplianceTag,
            OutlookExtendedProperties.PidTagRetentionId,
            OutlookExtendedProperties.PidTagRetentionPeriod,
            OutlookExtendedProperties.PidTagRetentionDate,
            OutlookExtendedProperties.PidTagRetentionFlags,
            OutlookExtendedProperties.PidTagSize,
            OutlookExtendedProperties.CustomPidItemTermId,
            OutlookExtendedProperties.PidTagConversationTopic,
            OutlookExtendedProperties.PidTagReceivedRepresentingName,
            OutlookExtendedProperties.PidTagReceivedRepresentingSmtpAddress,
            OutlookExtendedProperties.PidTagSensitivity,
        };
    }
}