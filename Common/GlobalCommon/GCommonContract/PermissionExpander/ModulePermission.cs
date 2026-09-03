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
using AvePoint.GCommon.Contract.AveModuleContract;
using AvePoint.GCommon.Contract.Common;

namespace AvePoint.GCommon.Contract.PermissionExpander
{
    [DataContract(Namespace = ContractConstants.Namespace)]
    public class ModulePermission
    {
        [DataMember]
        public Dictionary<string, List<PermissionType>> dictionary = new Dictionary<string, List<PermissionType>>();
        public ModulePermission()
        {
            //control panel
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.ExportLocation.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.SystemOptions.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.AccountManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.Office365.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.MappingManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.FilterPolicy.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.Monitor.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.AuthenticationManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.LicenseManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.UpdateManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.AgentGroup.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.UserNotificationSettings.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.JobPruning.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.LogManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.SolutionManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });
            dictionary.Add(ModuleContract.DocAvePlatform.ControlPanel.StorageManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write });


            //Data Protection
            dictionary.Add(ModuleContract.DocAvePlatform.DataProtection.GranularBackup.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.DataProtection.ExchangeOnlineBackup.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.DataProtection.PlatformBackup.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });

            //Migration
            dictionary.Add(ModuleContract.DocAvePlatform.Migration.eRoomMigration.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.Migration.FileMigration.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.Migration.LivelinkMigration.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.Migration.NotesMigration.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.Migration.SPMigration.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });

            //Compliance
            dictionary.Add(ModuleContract.DocAvePlatform.Compliance.EDiscovery.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });

            //Report Center
            dictionary.Add(ModuleContract.DocAvePlatform.ReportCenter.RCAdministration.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.ReportCenter.RCComplianceReports.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });

            //Storage Optimization
            dictionary.Add(ModuleContract.DocAvePlatform.StorageOptimization.Extender.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.StorageOptimization.Archiver.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.StorageOptimization.Connector.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });

            //Administration
            dictionary.Add(ModuleContract.DocAvePlatform.Administration.CentralAdmin.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.Administration.ContentManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.Administration.Replicator.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
            dictionary.Add(ModuleContract.DocAvePlatform.Administration.DeploymentManager.Name, new List<PermissionType> { PermissionType.Read, PermissionType.Write, PermissionType.Control });
        }
    }
    [DataContract(Namespace = ContractConstants.Namespace)]
    public enum PermissionType
    {
        [EnumMember]
        Write = 0,
        [EnumMember]
        Read = 1,
        [EnumMember]
        Control
    }
}
