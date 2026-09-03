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


using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;
using AvePoint.GCommon.Contract.Server.CreateContainer.Object;
using AvePoint.GCommon.Contract.SharePointBrowser;

namespace AvePoint.GCommon.Contract.Server.CreateContainer
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IMCreateSiteContainerService
    {
        [OperationContract]
        Office365CreateMessageContract ConnectForCreateContainer(Office365MessageContract message);
        [OperationContract]
        Office365CreateMessageContract GetCreateContainerSiteCollectionMessage(Office365MessageContract message);
        //[OperationContract]
        //SCContainerResult CreateSiteContainer(Office365CreateMessageContract createMessage, Office365MessageContract accountMessage);
        [OperationContract]
        SCContainerResult CreateSiteContainerByThread(Office365CreateMessageContract createMessage, Office365MessageContract accountMessage);
        [OperationContract]
        SCContainerResult CheckCreateResult(string fileName, long timeOut);
        [OperationContract]
        Office365CustomSolutionContract BrowseUserUpload(Office365CustomSolutionContract browseMessage,Office365MessageContract BrowseUserUpload);
    }
}
