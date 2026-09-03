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



#define CODE_ANALYSIS
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Diagnostics.CodeAnalysis;

// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("AgentCommonUtility.dll")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("AvePoint, Inc.")]
[assembly: AssemblyProduct("AvePoint RevIM")]
[assembly: AssemblyCopyright("Copyright © 2021 AvePoint® Inc. All Rights Reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("520a4b9f-23ef-4bfc-96e1-a3712991db02")]

// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Build and Revision Numbers 
// by using the '*' as shown below:
// [assembly: AssemblyVersion("1.0.*")]
[assembly: AssemblyVersion("1.0.0.0")]
[assembly: AssemblyFileVersion("2.0.0.0")]
[assembly: AssemblyInformationalVersion("2.0.0.0")]

[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.AgentRegisterUtil.#IsSQLInstalled()", MessageId = "AvePoint.Common.AgentRegisterUtil.IsSQLInstalled")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "type", Target = "AvePoint.Common.SQLServer.SQLServerInstance", MessageId = "SQLServerInstance")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.SQLServer.SQLServerInstance.#sqlDataRootFolder")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.SQLServer.SQLServerInstance.#get_sqlDataRootFolder()", MessageId = "AvePoint.Common.SQLServer.SQLServerInstance.get_sqlDataRootFolder")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.SQLServer.SQLServerInstance.#set_sqlDataRootFolder(System.String)", MessageId = "AvePoint.Common.SQLServer.SQLServerInstance.set_sqlDataRootFolder")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "type", Target = "AvePoint.Common.SQLServer.SQLServerInstanceCollection", MessageId = "SQLServerInstanceCollection")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "type", Target = "AvePoint.Common.SQLServer.SQLServerUtility", MessageId = "SQLServerUtility")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.SQLServer.SQLServerUtility.#CheckSQLServerInstanceCluster(System.String)", MessageId = "AvePoint.Common.SQLServer.SQLServerUtility.CheckSQLServerInstanceCluster")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.SQLServer.SQLServerUtility.#GetSQLLocationPath(System.String)", MessageId = "AvePoint.Common.SQLServer.SQLServerUtility.GetSQLLocationPath")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.SQLServer.SQLServerUtility.#CheckSQLServerInstanceClusterName(System.String)", MessageId = "AvePoint.Common.SQLServer.SQLServerUtility.CheckSQLServerInstanceClusterName")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.SQLServer.SQLServerUtility.#SearchSQLDataRootPath(System.String,System.String)", MessageId = "AvePoint.Common.SQLServer.SQLServerUtility.SearchSQLDataRootPath")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.SQLServer.SQLServerUtility.#GetSQLServerInstanceVersion(System.String)", MessageId = "AvePoint.Common.SQLServer.SQLServerUtility.GetSQLServerInstanceVersion")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "type", Target = "AvePoint.Common.AveSqlConnection", MessageId = "AveSqlConnection")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.AveSqlConnection.#Database")]
[module: SuppressMessage("FxCopCustomRules", "C100001:DoNotNameSensitiveWords", Scope = "member", Target = "AvePoint.Common.AveSqlConnection.#get_Database()", MessageId = "AvePoint.Common.AveSqlConnection.get_Database")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Common.AgentConstants+AgentFolderName.#.cctor()", MessageId = "Documentum")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Common.AgentConstants+AgentBinaryName.#.cctor()", MessageId = "Lun")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Common.AgentConstants+AgentBinaryName.#.cctor()", MessageId = "Livelink")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Common.AgentConstants+AgentBinaryName.#.cctor()", MessageId = "Documentum")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Service.Impl.PlatformRecovery.CommonLunMonitorServiceImpl.#IsServiceAlive(AvePoint.GCommon.Contract.Server.ControlPanel.Object.ServiceDto,System.String&)", MessageId = "Lun")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.Service.Impl.PlatformRecovery.CommonLunMonitorServiceImpl.#ProcessMessage(AvePoint.GCommon.Contract.PlatformRecovery.Object.LunMonitorMessage)", MessageId = "Lun")]
