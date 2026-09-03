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
[assembly: AssemblyTitle("CommonDataTransfer.dll")]
[assembly: AssemblyDescription("")]
[assembly: AssemblyConfiguration("")]
[assembly: AssemblyCompany("AvePoint, Inc.")]
[assembly: AssemblyProduct("DocAve 6")]
[assembly: AssemblyCopyright("Copyright © 2021 AvePoint® Inc. All Rights Reserved.")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("04d0d5fb-5965-4431-a130-fd2fbf4b0e92")]

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
[assembly: AssemblyFileVersion("6.7.0.1139")]
[assembly: AssemblyInformationalVersion("6.7.0.1139")]
[assembly: System.Runtime.CompilerServices.InternalsVisibleTo("DataTransferUnitTest, PublicKey=0024000004800000940000000602000000240000525341310004000001000100AD6E36FC036FB0F655658DCC959D16A912B081EC9C9E371E451EFDBEC11BC2A6A7F0131F085899E57EF02F369F074BCDBBC215F8524A1BC325DF2AFB5DAA35072282C0BF464CBBA8F1BBC04629ECE7F47E317ED853CE259B2A4DFA265DFBFC64F181DA7B44549B25C01373F44A76FA939B08F651DC7A4F359A644D4AD199CF8B")]

[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.XmlConfiguration.#ApplyConfiguration(System.String,System.String,System.ServiceModel.Configuration.BindingsSection)", MessageId = "ws")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.HttpModeTest.#ReceiverThread(System.Object)", MessageId = "iso")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.HttpModeTest.#ReceiverThread(System.Object)", MessageId = "dvd")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.HttpModeTest.#SenderThread(System.Object)", MessageId = "iso")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.HttpModeTest.#SenderThread(System.Object)", MessageId = "dvd")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.DataTransferTestCase.#ReceiverThread(System.Object)", MessageId = "iso")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.DataTransferTestCase.#ReceiverThread(System.Object)", MessageId = "mu")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.DataTransferTestCase.#ReceiverThread(System.Object)", MessageId = "dvd")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.XmlConfiguration.#InitiateSystemServiceModel(System.Xml.XmlElement)", MessageId = "teredo")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.XmlConfiguration.#InitiateSystemServiceModel(System.Xml.XmlElement)", MessageId = "ws")]
[module: SuppressMessage("CheckHardCode", "Z100009:CheckString", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.FileTransferTestCase.#SenderThread()")]
[module: SuppressMessage("CheckHardCode", "Z100009:CheckString", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.FileTransferTestCase.#ReceiverThread()")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.DataTransferUpgradeV1.#UpgradeDataTransfer(System.Xml.XmlElement,System.Xml.XmlElement)", MessageId = "Mq")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.DataTransferUpgradeV1.#UpgradeDataTransfer(System.Xml.XmlElement,System.Xml.XmlElement)", MessageId = "mq")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.DataTransferSection.#get_MqConfig()", MessageId = "mq")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.DataTransferSection.#get_EnableSsl()", MessageId = "Ssl")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.DataTransferSection.#set_EnableSsl(System.Boolean)", MessageId = "Ssl")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.DataTransferSection.#set_MqUriSchema(System.String)", MessageId = "mq")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Common.DataTransferSection.#get_MqUriSchema()", MessageId = "mq")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.MQ.Channel.MessageChannelFactory.#GetMessageChannel(System.String,System.Int32,System.String,System.String,AvePoint.GCommon.Transfer.MQ.Interface.IMQClientCallback,System.Boolean)", MessageId = "Mq")]
[module: SuppressMessage("Microsoft.Naming", "CA1708:IdentifiersShouldDifferByMoreThanCase", Scope = "type", Target = "AvePoint.GCommon.Transfer.Factory.WcfChannelFactory`1")]
[module: SuppressMessage("Microsoft.Globalization", "CA1304:SpecifyCultureInfo", Scope = "member", Target = "AvePoint.GCommon.Transfer.Factory.WcfConfigurationChannelUtility.#LoadBehaviors`1(System.ServiceModel.Configuration.ServiceModelExtensionCollectionElement`1<System.ServiceModel.Configuration.BehaviorExtensionElement>,System.Collections.Generic.KeyedByTypeCollection`1<!!0>,System.Boolean)", MessageId = "System.Type.InvokeMember(System.String,System.Reflection.BindingFlags,System.Reflection.Binder,System.Object,System.Object[])")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Factory.WcfConfigurationChannelUtility.#LoadIdentity(System.ServiceModel.Configuration.IdentityElement)", MessageId = "rsa")]
[module: SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "AvePoint.GCommon.Transfer.Data.Multiple.AveMultiTaskThread")]
[module: SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable", Scope = "type", Target = "AvePoint.GCommon.Transfer.Data.Multiple.AveTaskHierarchyScheduler")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Data.Multiple.Util.SegmentedStream.#InitStream()", MessageId = "avetemp")]
[module: SuppressMessage("FxCopCustomRules", "C100007:SpellCheckStringValues", Scope = "member", Target = "AvePoint.GCommon.Transfer.Data.Service.StreamModeService.#PutTransferStream(AvePoint.GCommon.Transfer.Data.Interface.HttpModeServiceStream)", MessageId = "seession")]

[module: SuppressMessage("CheckHardCode", "Z100009:CheckString", Scope = "member", Target = "AvePoint.GCommon.Transfer.TestCase.FileTransferTestCase.#SenderThread()")]
