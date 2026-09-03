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



using System.Collections.Generic;
using System.Runtime.Serialization;
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.SingleSignOn.Object;
using AvePoint.GCommon.Contract.Wcf;



namespace AvePoint.GCommon.Contract.Server.Common.Certificate
{
    
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMCertificateService
    {
        /// <summary>
        /// 获取Server中证书Tree
        /// </summary>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        List<CertificateInformationDto> GetCertificatesInServer();

        /// <summary>
        /// 证书查找
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="type">查找类型</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        CertificateInformationDto FindCertficateInServer(string value, CertificatieFindTypeEnum type);


        /// <summary>
        /// 在指定的Store中查找证书
        /// </summary>
        /// <param name="value">值</param>
        /// <param name="type">查找类型</param>
        /// <param name="soreName">Store</param>
        /// <returns></returns>
        [OperationContract]
        [FaultContract(typeof(WcfException))]
        CertificateInformationDto FindCertficateByStore(string value, string storeName, CertificatieFindTypeEnum type);
    }

    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum CertificatieFindTypeEnum
    {
        [EnumMember]
        IssuedBy,

        [EnumMember]
        IssuedTo,

        [EnumMember]
        MD5Hash,

        [EnumMember]
        SerialNumber,

        [EnumMember]
        SHA1Hash,

        [EnumMember]
        SubjectName
    }
}
