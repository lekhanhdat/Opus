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
using System.Text;
using System.Threading.Tasks;

namespace AvePoint.RA.Contract
{
    public enum SettingProfilesType
    {
        #region 0-19 base settings
        IndexDevice = 0,
        ExportLocationDevice = 1,
        #endregion

        #region 20-49 archive background settings
        //备份线程数
        TotalMultiBackupThreadNumber = 20,
        //删除线程数
        TotalMultiDeleteThreadNumber = 21,
        TotalTransferQueueNumber = 22,
        //stub restore link 加密的master key
        EndUserStubLinkMasterKey = 23,
        //index db see加密的master key
        DBSEEMasterKey = 24,
        DataEncrptionProfile= 25,
        //Archiver backup 输出的数据块格式
        ArchiverBackupOutputStreamLevel = 26,
        //Records backup 输出的数据块格式
        RecordsBackupOutputStreamLevel = 27,
        SOSkipDeletionForTest = 28,

        ArchiverExtendSetting = 29,
        //Archiver还原数据时是否走Cache逻辑，针对FTP/SFTP等Storage类型
        ArchiverMediaConfigReadDataViaCache = 30,
        //WPP Batch lmport
        ArchiverImport = 31,
        //用来控制是否计算CRC，目前只有WPP默认计算CRC
        ArchiverIsCalculateCRC =32,
        //Wrapper和Media交互Cache File数量，默认4
        ArchiverTransferQueueNumber= 33,
        //WPP Dedicate VM 是否走单独的Job Oueue
        DedicatedJobQueueSetting = 34,
        //控制是否处理PHL里面的数据，默认不处理
        IsScanPreservationHoldLibrary = 35,
        //备份删除多线程数量，默认4
        ArchiverMultiThreadSetting = 36,
        //根据Office365 Tenant user seats计算SubJob，默认日subjob计算方式
        SubJobControlSetting = 37,
        //Restore
        ItemDependencyOption = 38,
        //Enable Archiver Dedup
        ShowDeduplicateSetting = 39,
        #endregion

        #region 50-89 global settings
        CommunicationEncryptionKey = 50,
        ExportSignatureInfo = 51,
        ExportSignatureForVEOInfo = 52,
        ImportSiteMappingOverrideInfo = 53,
        // M365 Records label for locking
        RecordsLabelSetting = 54,
        #endregion
    }
}
