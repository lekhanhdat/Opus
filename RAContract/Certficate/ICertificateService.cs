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
using AvePoint.Hybrid.Contract.SignalR;
using AvePoint.RA.Contract.Object;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract.Certficate
{
    public interface ICertificateService
    {
        Guid Create(RMCertificateDto dto);
        Task<Guid> CreateReplicaCertificateAsync(RMCertificateDto dto);
        Task<bool> SetAsDefaultCertificateAsync(Guid certificateId);
        RMCertificateDto Get(Guid id, bool includeBinaryData = true);

        /// <summary>
        /// Return certificates which value only contains fields of Id, Name, Thumbprint, ValidateFrom and ValidateTo.
        /// </summary>
        /// <param name="includeExpired">indicate if including expired certificates</param>
        /// <returns></returns>
        Task<IList<RMCertificateDto>> GetAllWithoutBinaryDataAsync(bool includeExpired = false);

        string GetCertificatePulicKeyString(Guid id);

        bool Delete(Guid id);
        /// <summary>
        /// check if there are active agents need to update the certificate
        /// </summary>
        /// <param name="certificateId"></param>
        /// <returns></returns>
        Task<bool> NeedUpdateCertificate2AgentsAsync(Guid certificateId);

        /// <summary>
        /// update the certificate for active agents.
        /// </summary>
        /// <param name="certificateId"></param>
        /// <returns>if no agents need to be updated, return null</returns>
        Task<List<AgentCertificateUpdateResult>> UpdateCertificate2AgentsAsync(Guid certificateId);
        string ReadEncyptKey();
    }
}
