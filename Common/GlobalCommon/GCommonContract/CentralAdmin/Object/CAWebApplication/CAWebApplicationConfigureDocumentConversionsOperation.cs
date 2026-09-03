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





namespace AvePoint.GCommon.Contract.CentralAdmin.Object
{
    #region using directives
    using System.Collections.Generic;
    using System.Runtime.Serialization;
    using AvePoint.GCommon.Contract.Common;
    #endregion

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebApplicationConfigureDocumentConversionsOperation : CAOperation
    {
        [DataMember]
        public bool DocumentConversionsEnabled { get; set; }

        /// <summary>
        ///     Load Balancer Server  <id, name>
        /// </summary>
        [DataMember]
        public Dictionary<string, string> LoadBalances { get;set;}

        /// <summary>
        ///     Load Balancer Server, this is a key value of  LoadBalances
        /// </summary>
        [DataMember]
        public string CurrentLoadBalanceId { get; set; }

        [DataMember]
        public List<CAWebApplicationDocumentConverterOperation> DocumentConverters { get; set; }

        /// <summary>
        ///     daily between 03:00:00 and 11:00:00   or  daily between 15:00:00 and 19:00:00
        ///     hourly between 3 and 6
        ///     every 18 minutes between 0 and 0
        /// </summary>
        [DataMember]
        public string ScheduleString { get; set; }


        /// <summary>
        ///     false : get all the info need in Configure Document Conversions and Document Converter Settings
        ///             set Configure Document Conversions
        ///     true :  set Document Converter Settings, the list DocumentConverters should have 1 value, and in CAWebApplicationDocumentConverterOperation, the Id should not be null.
        /// </summary>
        [DataMember]
        public bool IsDocumentConverterSetting { get; set; }
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public class CAWebApplicationDocumentConverterOperation
    {
        [DataMember]
        public string DisplayName { get; set; }

        [DataMember]
        public string Id { get; set; }

        [DataMember]
        public string ConvertFrom { get; set; }

        [DataMember]
        public string ConvertTo { get; set; }

        /// <summary>
        ///     Make this document converter available for all document libraries on the server.
        /// </summary>
        [DataMember]
        public bool DisplayInUI { get; set; }

        /// <summary>
        ///     The specified value for time-out length is not valid. Type a whole number between 3 and 1800.
        ///     can not be null
        /// </summary>
        [DataMember]
        public int Timeout { get; set; }

        /// <summary>
        ///     The specified value for maximum retries is not valid. Type a whole number between 0 and 5.
        ///     can not be null
        /// </summary>
        [DataMember]
        public int MaxRetries { get; set; }

        /// <summary>
        ///     The specified value for maximum file size is not valid. Type a whole number between 1 and 999999.
        ///     if is null, the value is -1.
        /// </summary>
        [DataMember]
        public int MaxFileSize { get; set; }


        /// <summary>
        ///     Page to display to the user when the converter is invoked: 
        /// </summary>
        [DataMember]
        public string ConverterUIPage { get; set; }

        /// <summary>
        ///     Custom control to display to the user when the converter is invoked: 
        /// </summary>
        [DataMember]
        public string ConverterSpecificSettingsUI { get; set; }

        /// <summary>
        ///     Page to display to the user when the converter is configured for a content type:
        /// </summary>
        [DataMember]
        public string ConverterSettingsForContentType { get; set; }
    }
}
