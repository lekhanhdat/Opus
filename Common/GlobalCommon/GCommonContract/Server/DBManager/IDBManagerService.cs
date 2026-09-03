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
using System.ServiceModel;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.Server.DBManager
{
    [ServiceContract(Namespace = ContractConstants.Namespace)]
    public interface IDBManagerService
    {
        [OperationContract]
        List<string> GetAllTables(string code);
        //[OperationContract]
        //List<Dictionary<string, string>> GetTableContent(string tableName, int pageSize, int currentPage);
        [OperationContract]
        List<Dictionary<string, string>> ExecuteForList(string sql, string code);
        [OperationContract]
        List<Dictionary<string, string>> GetTablePropertys(string tableName, string code);
        [OperationContract]
        Dictionary<string, List<string>> GetRelationOfTables(int moduleId, string code);
        [OperationContract]
        Dictionary<string, Dictionary<int, string>> GetColumnMappingOfTalbe(string tableName, string code);
        [OperationContract]
        int ExecuteNonQuery(string sql, string code);
        [OperationContract]
        int GetTableRowCount(string tableName, string code);
        [OperationContract]
        string LoginConfirm(string code);
        [OperationContract]
        byte[] ConverterHelperEncrypt(byte[] bytes, int type, string code);
        [OperationContract]
        byte[] ConverterHelperDecrypt(byte[] bytes, int type, string code);

    }
}
