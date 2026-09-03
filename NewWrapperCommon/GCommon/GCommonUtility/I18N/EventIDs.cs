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
using AvePoint.I18N;
using AvePoint.GCommon.Contract.Server.Audit;

namespace AvePoint.GCommon.Utility.I18N
{
    public abstract class AveEventMessage
    {
        private Exception eventException = null;
        private Dictionary<Enum, string> contexts = new Dictionary<Enum, string>();
        internal Dictionary<Enum, string> Contexts { get { return contexts; } }

        public abstract AveLogLevel LogLevel { get; }
        public int EventId { get { return string.IsNullOrEmpty(GetValue("EventId")) ? 0 : int.Parse(GetValue("EventId")); } }
        public string EventMessage { get { return GetValue("EventMsg") + "\n" + ContextKeys.GetAllContexts(Contexts); } }
        public Exception EventException { get { return eventException; } }

        private string GetValue(string prefix)
        {
            string temp = this.GetType().FullName;
            temp = temp.Substring(temp.IndexOf("EventIds", StringComparison.OrdinalIgnoreCase) + "EventIds".Length);
            temp = temp.Replace("+", "_");
            temp = prefix + temp;
            return EventViewerResources.ResourceManager.GetString(temp);
        }

        public AveEventMessage()
        {
            //if (EventSourcesUtil.IsSMSP())
            //{
            //    this.Contexts.Add(ContextKeys.Common.MoreInformation, "https://kb.netapp.com/support/index?page=content&id=S:2017061&actp=LIST");
            //}
            //else
            //{
            //    this.Contexts.Add(ContextKeys.Common.MoreInformation, "http://www.avepoint.com/community/event-id/?id=" + EventId);
            //}
            if (this.eventException != null)
            {
                this.Contexts.Add(ContextKeys.Common.MoreInformation, "http://www.avepoint.com/community/event-id/?id=" + EventId + "&exception=" + this.eventException.GetType().Name);
            }
            else
            {
                this.Contexts.Add(ContextKeys.Common.MoreInformation, "http://www.avepoint.com/community/event-id/?id=" + EventId);
            }
        }

        public AveEventMessage(Exception e)
            : this()
        {
            if (e != null)
            {
                this.eventException = e;
                this.Contexts.Add(ContextKeys.Common.Cause, GetCustomizedMessage(e));
            }
        }

        private string GetCustomizedMessage(Exception e)
        {
            if (e.InnerException == null)
            {
                return e.GetType().Name + ": " + e.Message;
            }
            string innerMessage = GetCustomizedMessage(e.InnerException);
            string currentMessage = e.GetType().Name + ": " + e.Message;
            return innerMessage + " <--- \n" + currentMessage;
        }
    }

    public class EventIds
    {
        public class Authentication
        {
            public class LoginSuccessfullyEventMessage : AveEventMessage
            {
                public LoginSuccessfullyEventMessage(string loginAddress, string loginTime, ContextValues.Authentication.LoginType loginType, string userName)
                {
                    Contexts.Add(ContextKeys.Authentication.LoginAddress, loginAddress);
                    Contexts.Add(ContextKeys.Authentication.LoginTime, loginTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class LoginFailedEventMessage : AveEventMessage
            {
                public LoginFailedEventMessage(string loginAddress, string loginTime, ContextValues.Authentication.LoginType loginType, string userName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Authentication.LoginAddress, loginAddress);
                    Contexts.Add(ContextKeys.Authentication.LoginTime, loginTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class AutoLogoffSuccessfullyEventMessage : AveEventMessage
            {
                public AutoLogoffSuccessfullyEventMessage(string logoffTime, ContextValues.Authentication.LoginType loginType, string sessionStartTime, string userName)
                {
                    Contexts.Add(ContextKeys.Authentication.LogoffTime, logoffTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.SessionStartTime, sessionStartTime);
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }
            }

            public class ManualLogoffSuccessfullyEventMessage : AveEventMessage
            {
                public ManualLogoffSuccessfullyEventMessage(string logoffTime, ContextValues.Authentication.LoginType loginType, string userName)
                {
                    Contexts.Add(ContextKeys.Authentication.LogoffTime, logoffTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class ForceLogoffSuccessfullyEventMessage : AveEventMessage
            {
                public ForceLogoffSuccessfullyEventMessage(string logoffTime, ContextValues.Authentication.LoginType loginType, string operatingUserName, string userName)
                {
                    Contexts.Add(ContextKeys.Authentication.LogoffTime, logoffTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.OperatingUserName, operatingUserName);
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }
            }

            public class ValidateAccountFailedEventMessage : AveEventMessage
            {
                public ValidateAccountFailedEventMessage(string userName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
        }

        public class Communication
        {
            public class HandleRequestFailedEventMessage : AveEventMessage
            {
                public HandleRequestFailedEventMessage(string requestMessage, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Communication.RequestMessage, requestMessage);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class ConnectToServiceFailedEventMessage : AveEventMessage
            {
                public ConnectToServiceFailedEventMessage(string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class DataTransferFailedEventMessage : AveEventMessage
            {
                public DataTransferFailedEventMessage(string destinationAddress, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Communication.DestinationAddress, destinationAddress);
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class SendDataFailedEventMessage : AveEventMessage
            {
                public SendDataFailedEventMessage(string destinationAddress, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Communication.DestinationAddress, destinationAddress);
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class ReceiveDataFailedEventMessage : AveEventMessage
            {
                public ReceiveDataFailedEventMessage(Exception e)
                    : base(e)
                {
                }

                public ReceiveDataFailedEventMessage(string sourceAddress, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Communication.SourceAddress, sourceAddress);
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
        }

        public class Configuration
        {
            public class Plan
            {
                public class AddPlanSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                    public AddPlanSuccessfullyEventMessage(string planID, string planName, ContextValues.Configuration.Plan.PlanType planType)
                    {
                        Contexts.Add(ContextKeys.Configuration.PlanID, planID);
                        Contexts.Add(ContextKeys.Configuration.PlanName, planName);
                        Contexts.Add(ContextKeys.Configuration.PlanType, ContextValues.GetContextValue(planType));
                    }
                }
                public class ModifyPlanSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                    public ModifyPlanSuccessfullyEventMessage(string planID, string planName, ContextValues.Configuration.Plan.PlanType planType)
                    {
                        Contexts.Add(ContextKeys.Configuration.PlanID, planID);
                        Contexts.Add(ContextKeys.Configuration.PlanName, planName);
                        Contexts.Add(ContextKeys.Configuration.PlanType, ContextValues.GetContextValue(planType));
                    }
                }
                public class DeletePlanSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }

                    public DeletePlanSuccessfullyEventMessage(string planID, string planName, ContextValues.Configuration.Plan.PlanType planType)
                    {
                        Contexts.Add(ContextKeys.Configuration.PlanID, planID);
                        Contexts.Add(ContextKeys.Configuration.PlanName, planName);
                        Contexts.Add(ContextKeys.Configuration.PlanType, ContextValues.GetContextValue(planType));
                    }
                }
                public class AddPlanFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public AddPlanFailedEventMessage(string planID, string planName, ContextValues.Configuration.Plan.PlanType planType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.PlanID, planID);
                        Contexts.Add(ContextKeys.Configuration.PlanName, planName);
                        Contexts.Add(ContextKeys.Configuration.PlanType, ContextValues.GetContextValue(planType));
                    }
                }
                public class ModifyPlanFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public ModifyPlanFailedEventMessage(string planID, string planName, ContextValues.Configuration.Plan.PlanType planType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.PlanID, planID);
                        Contexts.Add(ContextKeys.Configuration.PlanName, planName);
                        Contexts.Add(ContextKeys.Configuration.PlanType, ContextValues.GetContextValue(planType));
                    }
                }
                public class DeletePlanFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public DeletePlanFailedEventMessage(string planID, string planName, ContextValues.Configuration.Plan.PlanType planType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.PlanID, planID);
                        Contexts.Add(ContextKeys.Configuration.PlanName, planName);
                        Contexts.Add(ContextKeys.Configuration.PlanType, ContextValues.GetContextValue(planType));
                    }
                }
            }

            public class Profile
            {
                public class AddProfileSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                    public AddProfileSuccessfullyEventMessage(string profileName, ContextValues.Configuration.Profile.ProfileType profileType)
                    {
                        Contexts.Add(ContextKeys.Configuration.ProfileName, profileName);
                        Contexts.Add(ContextKeys.Configuration.ProfileType, ContextValues.GetContextValue(profileType));
                    }
                }
                public class ModifyProfileSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                    public ModifyProfileSuccessfullyEventMessage(string profileName, ContextValues.Configuration.Profile.ProfileType profileType)
                    {
                        Contexts.Add(ContextKeys.Configuration.ProfileName, profileName);
                        Contexts.Add(ContextKeys.Configuration.ProfileType, ContextValues.GetContextValue(profileType));
                    }
                }
                public class DeleteProfileSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }

                    public DeleteProfileSuccessfullyEventMessage(string profileName, ContextValues.Configuration.Profile.ProfileType profileType)
                    {
                        Contexts.Add(ContextKeys.Configuration.ProfileName, profileName);
                        Contexts.Add(ContextKeys.Configuration.ProfileType, ContextValues.GetContextValue(profileType));
                    }
                }
                public class AddProfileFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public AddProfileFailedEventMessage(string profileName, ContextValues.Configuration.Profile.ProfileType profileType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.ProfileName, profileName);
                        Contexts.Add(ContextKeys.Configuration.ProfileType, ContextValues.GetContextValue(profileType));
                    }
                }
                public class ModifyProfileFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public ModifyProfileFailedEventMessage(string profileName, ContextValues.Configuration.Profile.ProfileType profileType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.ProfileName, profileName);
                        Contexts.Add(ContextKeys.Configuration.ProfileType, ContextValues.GetContextValue(profileType));
                    }
                }
                public class DeleteProfileFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public DeleteProfileFailedEventMessage(string profileName, ContextValues.Configuration.Profile.ProfileType profileType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.ProfileName, profileName);
                        Contexts.Add(ContextKeys.Configuration.ProfileType, ContextValues.GetContextValue(profileType));
                    }
                }

                public class OperateProfileSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                    public OperateProfileSuccessfullyEventMessage(string profileName, ContextValues.Configuration.Profile.ProfileType profileType, ContextValues.Configuration.Profile.OperationType operationType)
                    {
                        Contexts.Add(ContextKeys.Configuration.ProfileName, profileName);
                        Contexts.Add(ContextKeys.Configuration.ProfileType, ContextValues.GetContextValue(profileType));
                        Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    }
                }

                public class OperateProfileFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public OperateProfileFailedEventMessage(string profileName, ContextValues.Configuration.Profile.ProfileType profileType, ContextValues.Configuration.Profile.OperationType operationType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.ProfileName, profileName);
                        Contexts.Add(ContextKeys.Configuration.ProfileType, ContextValues.GetContextValue(profileType));
                        Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    }
                }
            }

            public class Setting
            {
                public class AddSettingSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                    public AddSettingSuccessfullyEventMessage(ContextValues.Configuration.Setting.SettingType settingType)
                    {
                        Contexts.Add(ContextKeys.Configuration.SettingType, ContextValues.GetContextValue(settingType));
                    }
                }
                public class ModifySettingSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                    public ModifySettingSuccessfullyEventMessage(ContextValues.Configuration.Setting.SettingType settingType)
                    {
                        Contexts.Add(ContextKeys.Configuration.SettingType, ContextValues.GetContextValue(settingType));
                    }
                }
                public class DeleteSettingSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }

                    public DeleteSettingSuccessfullyEventMessage(ContextValues.Configuration.Setting.SettingType settingType)
                    {
                        Contexts.Add(ContextKeys.Configuration.SettingType, ContextValues.GetContextValue(settingType));
                    }
                }
                public class AddSettingFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public AddSettingFailedEventMessage(ContextValues.Configuration.Setting.SettingType settingType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.SettingType, ContextValues.GetContextValue(settingType));
                    }
                }
                public class ModifySettingFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public ModifySettingFailedEventMessage(ContextValues.Configuration.Setting.SettingType settingType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.SettingType, ContextValues.GetContextValue(settingType));
                    }
                }
                public class DeleteSettingFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public DeleteSettingFailedEventMessage(ContextValues.Configuration.Setting.SettingType settingType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Configuration.SettingType, ContextValues.GetContextValue(settingType));
                    }
                }

                public class OperateSettingSuccessfullyEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                    public OperateSettingSuccessfullyEventMessage(ContextValues.Configuration.Setting.SettingType settingType, ContextValues.Configuration.Setting.OperationType operationType)
                    {
                        Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                        Contexts.Add(ContextKeys.Configuration.SettingType, ContextValues.GetContextValue(settingType));
                    }
                }
                public class OperateSettingFailedEventMessage : AveEventMessage
                {
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                    public OperateSettingFailedEventMessage(ContextValues.Configuration.Setting.SettingType settingType, ContextValues.Configuration.Setting.OperationType operationType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                        Contexts.Add(ContextKeys.Configuration.SettingType, ContextValues.GetContextValue(settingType));
                    }
                }
            }
        }

        public class Database
        {
            public class AddDatabaseSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AddDatabaseSuccessfullyEventMessage(string databaseName, ContextValues.Database.DatabaseType databaseType)
                {
                    Contexts.Add(ContextKeys.Database.Name, databaseName);
                    Contexts.Add(ContextKeys.Database.Type, ContextValues.GetContextValue(databaseType));
                }
            }
            public class ModifyDatabaseSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public ModifyDatabaseSuccessfullyEventMessage(string databaseName, ContextValues.Database.DatabaseType databaseType)
                {
                    Contexts.Add(ContextKeys.Database.Name, databaseName);
                    Contexts.Add(ContextKeys.Database.Type, ContextValues.GetContextValue(databaseType));
                }
            }
            public class DeleteDatabaseSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }

                public DeleteDatabaseSuccessfullyEventMessage(string databaseName, ContextValues.Database.DatabaseType databaseType)
                {
                    Contexts.Add(ContextKeys.Database.Name, databaseName);
                    Contexts.Add(ContextKeys.Database.Type, ContextValues.GetContextValue(databaseType));
                }
            }
            public class AddDatabaseFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public AddDatabaseFailedEventMessage(string databaseName, ContextValues.Database.DatabaseType databaseType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Database.Name, databaseName);
                    Contexts.Add(ContextKeys.Database.Type, ContextValues.GetContextValue(databaseType));
                }
            }
            public class ModifyDatabaseFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public ModifyDatabaseFailedEventMessage(string databaseName, ContextValues.Database.DatabaseType databaseType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Database.Name, databaseName);
                    Contexts.Add(ContextKeys.Database.Type, ContextValues.GetContextValue(databaseType));
                }
            }
            public class DeleteDatabaseFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public DeleteDatabaseFailedEventMessage(string databaseName, ContextValues.Database.DatabaseType databaseType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Database.Name, databaseName);
                    Contexts.Add(ContextKeys.Database.Type, ContextValues.GetContextValue(databaseType));
                }
            }

            public class OperateDatabaseSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public OperateDatabaseSuccessfullyEventMessage(string databaseName, ContextValues.Database.DatabaseType databaseType, ContextValues.Database.OperationType operationType)
                {
                    Contexts.Add(ContextKeys.Database.Name, databaseName);
                    Contexts.Add(ContextKeys.Database.Type, ContextValues.GetContextValue(databaseType));
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                }
            }

            public class OperateDatabaseFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public OperateDatabaseFailedEventMessage(string databaseName, ContextValues.Database.DatabaseType databaseType, ContextValues.Database.OperationType operationType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Database.Name, databaseName);
                    Contexts.Add(ContextKeys.Database.Type, ContextValues.GetContextValue(databaseType));
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                }
            }
        }

        public class Driver
        {
            public class OperateDriverFailedEventMessage : AveEventMessage
            {
                public OperateDriverFailedEventMessage(ContextValues.Driver.OperationType operationType, string driverName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    Contexts.Add(ContextKeys.Driver.Name, driverName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class OperateDriverSuccessfullyEventMessage : AveEventMessage
            {
                public OperateDriverSuccessfullyEventMessage(ContextValues.Driver.OperationType operationType, string driverName)
                {
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    Contexts.Add(ContextKeys.Driver.Name, driverName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
        }

        public class File
        {
            public class WriteFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public WriteFailedEventMessage(string filePath, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.File.Path, filePath);
                }
            }

            public class ReadFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public ReadFailedEventMessage(string filePath, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.File.Path, filePath);
                }
            }
        }

        public class IIS
        {
            public class StartIISServiceSuccessfullyEventMessage : AveEventMessage
            {
                public StartIISServiceSuccessfullyEventMessage(string iisAddress)
                {
                    Contexts.Add(ContextKeys.Socket.Address, iisAddress);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class StartIISServiceFailedEventMessage : AveEventMessage
            {
                public StartIISServiceFailedEventMessage(string iisAddress, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Socket.Address, iisAddress);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class StopIISServiceSuccessfullyEventMessage : AveEventMessage
            {
                public StopIISServiceSuccessfullyEventMessage(string iisAddress)
                {
                    Contexts.Add(ContextKeys.Socket.Address, iisAddress);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class StopIISServiceFailedEventMessage : AveEventMessage
            {
                public StopIISServiceFailedEventMessage(string iisAddress, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Socket.Address, iisAddress);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
        }

        public class Job
        {
            public class JobReport
            {
                public class OperateJobReportFailedEventMessage : AveEventMessage
                {
                    public OperateJobReportFailedEventMessage(string jobID, ContextValues.Job.JobReport.OperationType operationType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Job.JobID, jobID);
                        Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }

                public class OperateJobReportSuccessfullyEventMessage : AveEventMessage
                {
                    public OperateJobReportSuccessfullyEventMessage(string jobID, ContextValues.Job.JobReport.OperationType operationType)
                    {
                        Contexts.Add(ContextKeys.Job.JobID, jobID);
                        Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.INFO; }
                    }
                }
            }

            public class StartedEventMessage : AveEventMessage
            {
                public StartedEventMessage(string jobID)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.INFO; }
                }
            }

            public class CompletedEventMessage : AveEventMessage
            {
                public CompletedEventMessage(string jobID)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.INFO; }
                }
            }

            public class CompletedWithExceptionEventMessage : AveEventMessage
            {
                public CompletedWithExceptionEventMessage(string jobID)
                    : this(jobID, new Utility.Exceptions.Job.CompletedWithExceptionException())
                {
                }

                public CompletedWithExceptionEventMessage(string jobID, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class SkippedEventMessage : AveEventMessage
            {
                public SkippedEventMessage(string jobID, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class FailedEventMessage : AveEventMessage
            {

                public FailedEventMessage(string jobID, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class StoppedEventMessage : AveEventMessage
            {
                public StoppedEventMessage(string jobID)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class PausedEventMessage : AveEventMessage
            {
                public PausedEventMessage(string jobID)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class DeletedJobSuccessfullyEventMessage : AveEventMessage
            {
                public DeletedJobSuccessfullyEventMessage(string jobID)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class DeletedJobFailedEventMessage : AveEventMessage
            {
                public DeletedJobFailedEventMessage(string jobID, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class ConvertJobTypeSuccessfullyEventMessage : AveEventMessage
            {
                public ConvertJobTypeSuccessfullyEventMessage(string originalJobType, string finalJobType, string jobID)
                {
                    Contexts.Add(ContextKeys.Job.JobID, jobID);
                    Contexts.Add(ContextKeys.Job.OriginalJobType, originalJobType);
                    Contexts.Add(ContextKeys.Job.FinalJobType, finalJobType);
                }
                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }
        }

        public class License
        {
            public class LoadLicenseFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public LoadLicenseFailedEventMessage(Exception e)
                    : base(e)
                {
                }
            }

            public class ApplyLicenseFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public ApplyLicenseFailedEventMessage(Exception e)
                    : base(e)
                {
                }
            }

            public class ApplyLicenseSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public ApplyLicenseSuccessfullyEventMessage(Exception e)
                    : base(e)
                {
                }
            }
        }

        public class Process
        {
            public class InvokeProcessFailedEventMessage : AveEventMessage
            {
                public InvokeProcessFailedEventMessage(string parameter, string processName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Process.Parameter, parameter);
                    Contexts.Add(ContextKeys.Process.ProcessName, processName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class InvokeProcessSuccessfullyEventMessage : AveEventMessage
            {
                public InvokeProcessSuccessfullyEventMessage(string parameter, string processName)
                {
                    Contexts.Add(ContextKeys.Process.Parameter, parameter);
                    Contexts.Add(ContextKeys.Process.ProcessName, processName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class ProcessStartEventMessage : AveEventMessage
            {
                public ProcessStartEventMessage(string parameter, string processName)
                {
                    Contexts.Add(ContextKeys.Process.Parameter, parameter);
                    Contexts.Add(ContextKeys.Process.ProcessName, processName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class InvokeCommandFailedEventMessage : AveEventMessage
            {
                public InvokeCommandFailedEventMessage(string parameter, string commandName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Process.Parameter, parameter);
                    Contexts.Add(ContextKeys.Process.CommandName, commandName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class InvokeCommandSuccessfullyEventMessage : AveEventMessage
            {
                public InvokeCommandSuccessfullyEventMessage(string parameter, string commandName)
                {
                    Contexts.Add(ContextKeys.Process.Parameter, parameter);
                    Contexts.Add(ContextKeys.Process.CommandName, commandName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class CommandStartEventMessage : AveEventMessage
            {
                public CommandStartEventMessage(string parameter, string commandName)
                {
                    Contexts.Add(ContextKeys.Process.Parameter, parameter);
                    Contexts.Add(ContextKeys.Process.CommandName, commandName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
        }

        public class Packaging
        {
            public class InstallationStartEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public InstallationStartEventMessage(ContextValues.Packaging.PackageType packageType)
                {
                    Contexts.Add(ContextKeys.Packaging.PackageType, ContextValues.GetContextValue(packageType));
                }
            }

            public class UninstallationStartEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public UninstallationStartEventMessage(ContextValues.Packaging.PackageType packageType)
                {
                    Contexts.Add(ContextKeys.Packaging.PackageType, ContextValues.GetContextValue(packageType));
                }
            }

            public class InstallPackageSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public InstallPackageSuccessfullyEventMessage(ContextValues.Packaging.PackageType packageType)
                {
                    Contexts.Add(ContextKeys.Packaging.PackageType, ContextValues.GetContextValue(packageType));
                }
            }

            public class InstallPackageFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public InstallPackageFailedEventMessage(ContextValues.Packaging.PackageType packageType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Packaging.PackageType, ContextValues.GetContextValue(packageType));
                }
            }

            public class UninstallPackageSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public UninstallPackageSuccessfullyEventMessage(ContextValues.Packaging.PackageType packageType)
                {
                    Contexts.Add(ContextKeys.Packaging.PackageType, ContextValues.GetContextValue(packageType));
                }
            }

            public class UninstallPackageFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public UninstallPackageFailedEventMessage(ContextValues.Packaging.PackageType packageType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Packaging.PackageType, ContextValues.GetContextValue(packageType));
                }
            }
        }

        public class Service
        {
            public class AddServiceSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AddServiceSuccessfullyEventMessage(string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType)
                {
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }
            public class ModifyServiceSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public ModifyServiceSuccessfullyEventMessage(string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType)
                {
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }
            public class DeleteServiceSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }

                public DeleteServiceSuccessfullyEventMessage(string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType)
                {
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }
            public class AddServiceFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public AddServiceFailedEventMessage(string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }
            public class ModifyServiceFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public ModifyServiceFailedEventMessage(string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }
            public class DeleteServiceFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public DeleteServiceFailedEventMessage(string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }

            public class OperateServiceFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public OperateServiceFailedEventMessage(ContextValues.Service.OperationType operationType, string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }

            public class OperateServiceSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public OperateServiceSuccessfullyEventMessage(ContextValues.Service.OperationType operationType, string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType)
                {
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }

            public class ServiceSelfCheckWithExceptionEventMessage : AveEventMessage
            {
                public ServiceSelfCheckWithExceptionEventMessage(string serviceAddress, int servicePort, ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RuntimeErrorEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public RuntimeErrorEventMessage(ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }

            public class StartedSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public StartedSuccessfullyEventMessage(ContextValues.Service.ServiceType serviceType)
                {
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }

            public class StartedFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public StartedFailedEventMessage(ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }

                public StartedFailedEventMessage(ContextValues.Service.ServiceType serviceType, int servicePort, Exception e)
                    : this(serviceType, e)
                {
                    Contexts.Add(ContextKeys.Service.ServicePort, servicePort.ToString());
                }

                public StartedFailedEventMessage(ContextValues.Service.ServiceType serviceType, string managerAddress, int managerPort, Exception e)
                    : this(serviceType, e)
                {
                    Contexts.Add(ContextKeys.Service.ManagerAddress, managerAddress);
                    Contexts.Add(ContextKeys.Service.ManagerPort, managerPort.ToString());
                }
            }

            public class StoppedSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public StoppedSuccessfullyEventMessage(ContextValues.Service.ServiceType serviceType)
                {
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }

            public class StoppedFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public StoppedFailedEventMessage(ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }

            public class ExitedAbnormallyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public ExitedAbnormallyEventMessage(ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }
            }
        }

        public class SharePoint
        {
            public class Common
            {
                public class BrowseSharePointFailedEventMessage : AveEventMessage
                {
                    public BrowseSharePointFailedEventMessage(Exception e)
                        : base(e)
                    {

                    }
                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }

                public class DiscoverSharePointFailedEventMessage : AveEventMessage
                {
                    public DiscoverSharePointFailedEventMessage(Exception e)
                        : base(e)
                    {

                    }
                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }
            }

            public class Index
            {
                public class BackupIndexFailedEventMessage : AveEventMessage
                {
                    public BackupIndexFailedEventMessage(string indexName, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.IndexName, indexName);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }

                public class RestoreIndexFailedEventMessage : AveEventMessage
                {
                    public RestoreIndexFailedEventMessage(string indexName, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.IndexName, indexName);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }
            }

            public class Blob
            {
                public class BackupBlobFailedEventMessage : AveEventMessage
                {
                    public BackupBlobFailedEventMessage(string blobName, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.BlobName, blobName);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }

                public class RestoreBlobFailedEventMessage : AveEventMessage
                {
                    public RestoreBlobFailedEventMessage(string blobName, string destinationPath, Guid listID, string sourcePath, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.BlobName, blobName);
                        Contexts.Add(ContextKeys.SharePoint.DestinationPath, destinationPath);
                        Contexts.Add(ContextKeys.SharePoint.ListID, listID.ToString());
                        Contexts.Add(ContextKeys.SharePoint.SourcePath, sourcePath);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }

                public class EnabledRBSSuccessfullyEventMessage : AveEventMessage
                {
                    public EnabledRBSSuccessfullyEventMessage(string databaseName)
                    {
                        Contexts.Add(ContextKeys.Database.Name, databaseName);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.INFO; }
                    }
                }

                public class EnabledRBSFailedEventMessage : AveEventMessage
                {
                    public EnabledRBSFailedEventMessage(string databaseName, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Database.Name, databaseName);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }

                public class EnabledEBSSuccessfullyEventMessage : AveEventMessage
                {
                    public EnabledEBSSuccessfullyEventMessage(string farmName)
                    {
                        Contexts.Add(ContextKeys.SharePoint.FarmName, farmName);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.INFO; }
                    }
                }

                public class EnabledEBSFailedEventMessage : AveEventMessage
                {
                    public EnabledEBSFailedEventMessage(string farmName, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.FarmName, farmName);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }
            }

            public class Database
            {
                public class BackupDatabaseFailedEventMessage : AveEventMessage
                {
                    public BackupDatabaseFailedEventMessage(string databaseName, ContextValues.SharePoint.Database.DatabaseType databaseType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.DatabaseName, databaseName);
                        Contexts.Add(ContextKeys.SharePoint.DatabaseType, ContextValues.GetContextValue(databaseType));
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }

                public class RestoreDatabaseFailedEventMessage : AveEventMessage
                {
                    public RestoreDatabaseFailedEventMessage(string databaseName, ContextValues.SharePoint.Database.DatabaseType databaseType, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.DatabaseName, databaseName);
                        Contexts.Add(ContextKeys.SharePoint.DatabaseType, ContextValues.GetContextValue(databaseType));
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }
            }

            public class ContentDatabase
            {
                public class BackupContentDatabaseFailedEventMessage : AveEventMessage
                {
                    public BackupContentDatabaseFailedEventMessage(string contentDatabaseName, string webApplicationURL, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.ContentDatabaseName, contentDatabaseName);
                        Contexts.Add(ContextKeys.SharePoint.WebApplicationURL, webApplicationURL);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }

                public class RestoreContentDatabaseFailedEventMessage : AveEventMessage
                {
                    public RestoreContentDatabaseFailedEventMessage(string contentDatabaseName, string webApplicationURL, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.ContentDatabaseName, contentDatabaseName);
                        Contexts.Add(ContextKeys.SharePoint.WebApplicationURL, webApplicationURL);
                    }

                    public override AveLogLevel LogLevel
                    {
                        get { return AveLogLevel.ERROR; }
                    }
                }
            }

            public class Solution
            {
                public class OperateSolutionSuccessfullyEventMessage : AveEventMessage
                {
                    public OperateSolutionSuccessfullyEventMessage(ContextValues.SharePoint.Solution.OperationType operationType, string solutionName)
                    {
                        Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                        Contexts.Add(ContextKeys.SharePoint.SolutionName, solutionName);
                    }
                    public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
                }

                public class OperateSolutionFailedEventMessage : AveEventMessage
                {
                    public OperateSolutionFailedEventMessage(ContextValues.SharePoint.Solution.OperationType operationType, string solutionName, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                        Contexts.Add(ContextKeys.SharePoint.SolutionName, solutionName);
                    }
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
                }
            }

            public class List
            {
                public class SyncListFailedEventMessage : AveEventMessage
                {
                    public SyncListFailedEventMessage(string listTitle, Exception e)
                        : base(e)
                    {
                        Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
                    }
                    public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
                }
            }


            public class UploadDocumentFailedEventMessage : AveEventMessage
            {
                public UploadDocumentFailedEventMessage(string documentName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.DocumentName, documentName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            #region <<Restore>>
            public class RestoreSiteCollectionFailedEventMessage : AveEventMessage
            {
                public RestoreSiteCollectionFailedEventMessage(string siteCollectionURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.SiteCollectionURL, siteCollectionURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class RestoreWebFailedEventMessage : AveEventMessage
            {
                public RestoreWebFailedEventMessage(string siteURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.SiteURL, siteURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class RestoreListFailedEventMessage : AveEventMessage
            {
                public RestoreListFailedEventMessage(string listTitle, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class RestoreItemFailedEventMessage : AveEventMessage
            {
                public RestoreItemFailedEventMessage(string itemName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ItemName, itemName);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class RestoreContentTypeFailedEventMessage : AveEventMessage
            {
                public RestoreContentTypeFailedEventMessage(string contentTypeName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ContentTypeName, contentTypeName);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreColumnFailedEventMessage : AveEventMessage
            {
                public RestoreColumnFailedEventMessage(string columnTitle, string dependencyTitle, ContextValues.SharePoint.ObjectType dependencyType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ColumnTitle, columnTitle);
                    Contexts.Add(ContextKeys.SharePoint.DependencyTitle, dependencyTitle);
                    Contexts.Add(ContextKeys.SharePoint.DependencyType, ContextValues.GetContextValue(dependencyType));
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class ActivateFeatureFailedEventMessage : AveEventMessage
            {
                public ActivateFeatureFailedEventMessage(Guid featureID, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.FeatureID, featureID.ToString());
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreUserFailedEventMessage : AveEventMessage
            {
                public RestoreUserFailedEventMessage(string username, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Authentication.UserName, username);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreUserProfileFailedEventMessage : AveEventMessage
            {
                public RestoreUserProfileFailedEventMessage(string siteCollectionURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.SiteCollectionURL, siteCollectionURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreDocumentTagFailedEventMessage : AveEventMessage
            {
                public RestoreDocumentTagFailedEventMessage(string itemURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ItemURL, itemURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreWorkflowInstanceFailedEventMessage : AveEventMessage
            {
                public RestoreWorkflowInstanceFailedEventMessage(string workflowDefinationName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.WorkflowDefinationName, workflowDefinationName);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestorePermissionFailedEventMessage : AveEventMessage
            {
                public RestorePermissionFailedEventMessage(Exception e)
                    : base(e)
                { }
                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreMetadataServiceFailedEventMessage : AveEventMessage
            {
                public RestoreMetadataServiceFailedEventMessage(Exception e)
                    : base(e)
                { }
                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreAlertFailedEventMessage : AveEventMessage
            {
                public RestoreAlertFailedEventMessage(string username, string scopeURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Authentication.UserName, username);
                    Contexts.Add(ContextKeys.SharePoint.ScopeURL, scopeURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreAudienceMappingFailedEventMessage : AveEventMessage
            {
                public RestoreAudienceMappingFailedEventMessage(string siteURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.SiteURL, siteURL);
                }
                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreWebPartFailedEventMessage : AveEventMessage
            {
                public RestoreWebPartFailedEventMessage(string webPartDisplayName, string webPartType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.WebPartDisplayName, webPartDisplayName);
                    Contexts.Add(ContextKeys.SharePoint.WebPartType, webPartType);
                }
                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class RestoreItemPropertyFailedEventMessage : AveEventMessage
            {
                public RestoreItemPropertyFailedEventMessage(string itemName, string propertyName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ItemName, itemName);
                    Contexts.Add(ContextKeys.SharePoint.PropertyName, propertyName);
                }
                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            #endregion

            #region <<Backup>>
            public class BackupSiteCollectionFailedEventMessage : AveEventMessage
            {
                public BackupSiteCollectionFailedEventMessage(string siteCollectionURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.SiteCollectionURL, siteCollectionURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class BackupWebFailedEventMessage : AveEventMessage
            {
                public BackupWebFailedEventMessage(string siteURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.SiteURL, siteURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class BackupListFailedEventMessage : AveEventMessage
            {
                public BackupListFailedEventMessage(string listTitle, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ListTitle, listTitle);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class BackupItemFailedEventMessage : AveEventMessage
            {
                public BackupItemFailedEventMessage(string itemName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ItemName, itemName);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.ERROR; }
                }
            }

            public class BackupUserProfileFailedEventMessage : AveEventMessage
            {
                public BackupUserProfileFailedEventMessage(string siteCollectionURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.SiteCollectionURL, siteCollectionURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class BackupDocumentTagFailedEventMessage : AveEventMessage
            {
                public BackupDocumentTagFailedEventMessage(string itemURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.ItemURL, itemURL);
                }

                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class BackupMetadataServiceFailedEventMessage : AveEventMessage
            {
                public BackupMetadataServiceFailedEventMessage(Exception e)
                    : base(e)
                { }
                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            public class BackupAudienceMappingFailedEventMessage : AveEventMessage
            {
                public BackupAudienceMappingFailedEventMessage(string siteURL, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.SharePoint.SiteURL, siteURL);
                }
                public override AveLogLevel LogLevel
                {
                    get { return AveLogLevel.WARN; }
                }
            }

            #endregion
        }

        public class Snapshot
        {
            public class OperateSnapMirrorFailedEventMessage : AveEventMessage
            {
                public OperateSnapMirrorFailedEventMessage(string volumeName, ContextValues.Snapshot.OperationType operationType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Snapshot.VolumeName, volumeName);
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class OperateSnapMirrorSuccessfullyEventMessage : AveEventMessage
            {
                public OperateSnapMirrorSuccessfullyEventMessage(string volumeName, ContextValues.Snapshot.OperationType operationType)
                {
                    Contexts.Add(ContextKeys.Snapshot.VolumeName, volumeName);
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class OperateSnapVaultFailedEventMessage : AveEventMessage
            {
                public OperateSnapVaultFailedEventMessage(string volumeName, ContextValues.Snapshot.OperationType operationType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Snapshot.VolumeName, volumeName);
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class OperateSnapVaultSuccessfullyEventMessage : AveEventMessage
            {
                public OperateSnapVaultSuccessfullyEventMessage(string volumeName, ContextValues.Snapshot.OperationType operationType)
                {
                    Contexts.Add(ContextKeys.Snapshot.VolumeName, volumeName);
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class OperateSnapshotFailedEventMessage : AveEventMessage
            {
                public OperateSnapshotFailedEventMessage(
                    string agentName,
                    int currentSnapshotsCount,
                    int maxSnapshotCount,
                    ContextValues.Snapshot.OperationType operationType,
                    string volumeName,
                    Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Snapshot.AgentName, agentName);
                    Contexts.Add(ContextKeys.Snapshot.CurrentSnapshotCount, currentSnapshotsCount.ToString());
                    Contexts.Add(ContextKeys.Snapshot.MaxSnapshotCount, maxSnapshotCount.ToString());
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    Contexts.Add(ContextKeys.Snapshot.VolumeName, volumeName);
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class OperateSnapshotSuccessfullyEventMessage : AveEventMessage
            {
                public OperateSnapshotSuccessfullyEventMessage(
                    string agentName,
                    int currentSnapshotsCount,
                    int maxSnapshotCount,
                    ContextValues.Snapshot.OperationType operationType,
                    string volumeName)
                {
                    Contexts.Add(ContextKeys.Snapshot.AgentName, agentName);
                    Contexts.Add(ContextKeys.Snapshot.CurrentSnapshotCount, currentSnapshotsCount.ToString());
                    Contexts.Add(ContextKeys.Snapshot.MaxSnapshotCount, maxSnapshotCount.ToString());
                    Contexts.Add(ContextKeys.Common.OperationType, ContextValues.GetContextValue(operationType));
                    Contexts.Add(ContextKeys.Snapshot.VolumeName, volumeName);
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
        }

        public class Storage
        {
            public class VerifyFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public VerifyFailedEventMessage(string path, ContextValues.Storage.StorageType storageType, Exception e)
                    : base(e)
                {
                    this.Contexts.Add(ContextKeys.Storage.Path, path);
                    this.Contexts.Add(ContextKeys.Storage.StorageType, ContextValues.GetContextValue(storageType));
                }
            }

            public class VerifySuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public VerifySuccessfullyEventMessage(string path, ContextValues.Storage.StorageType storageType)
                {
                    this.Contexts.Add(ContextKeys.Storage.Path, path);
                    this.Contexts.Add(ContextKeys.Storage.StorageType, ContextValues.GetContextValue(storageType));
                }
            }

            public class WriteFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public WriteFailedEventMessage(string path, ContextValues.Storage.StorageType storageType, Exception e)
                    : base(e)
                {
                    this.Contexts.Add(ContextKeys.Storage.Path, path);
                    this.Contexts.Add(ContextKeys.Storage.StorageType, ContextValues.GetContextValue(storageType));
                }
            }

            public class ReadFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public ReadFailedEventMessage(string path, ContextValues.Storage.StorageType storageType, Exception e)
                    : base(e)
                {
                    this.Contexts.Add(ContextKeys.Storage.Path, path);
                    this.Contexts.Add(ContextKeys.Storage.StorageType, ContextValues.GetContextValue(storageType));
                }
            }
        }

        public class Update
        {
            public class InstallHotfixSuccessfullyEventMessage : AveEventMessage
            {
                public InstallHotfixSuccessfullyEventMessage(string hotfixName, string serviceAddress, ContextValues.Service.ServiceType serviceType)
                {
                    Contexts.Add(ContextKeys.Update.HostfixName, hotfixName);
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class InstallHotfixFailedEventMessage : AveEventMessage
            {
                public InstallHotfixFailedEventMessage(string hotfixName, string serviceAddress, ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Update.HostfixName, hotfixName);
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }

            public class UninstallHotfixSuccessfullyEventMessage : AveEventMessage
            {
                public UninstallHotfixSuccessfullyEventMessage(string hotfixName, string serviceAddress, ContextValues.Service.ServiceType serviceType)
                {
                    Contexts.Add(ContextKeys.Update.HostfixName, hotfixName);
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }

            public class UninstallHotfixFailedEventMessage : AveEventMessage
            {
                public UninstallHotfixFailedEventMessage(string hotfixName, string serviceAddress, ContextValues.Service.ServiceType serviceType, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Update.HostfixName, hotfixName);
                    Contexts.Add(ContextKeys.Service.ServiceAddress, serviceAddress);
                    Contexts.Add(ContextKeys.Service.ServiceType, ContextValues.GetContextValue(serviceType));
                }

                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
        }

        public class Audit
        {
            public class AuditSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditSuccessfullyEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditFailedEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }

                public AuditFailedEventMessage() : base()
                {
                }
            }

            public class AuditSessionTimeoutEventMessage : AveEventMessage
            {
                public AuditSessionTimeoutEventMessage(string userName, string lastLoginTime, string timeOut)
                {
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                    Contexts.Add(ContextKeys.Audit.LastLoginTime, lastLoginTime);
                    Contexts.Add(ContextKeys.Audit.SessionTimeOut, timeOut);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditAutoLogoffSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditAutoLogoffSuccessfullyEventMessage(string logoffTime, ContextValues.Authentication.LoginType loginType, string sessionStartTime, string userName)
                {
                    Contexts.Add(ContextKeys.Authentication.LogoffTime, logoffTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.SessionStartTime, sessionStartTime);
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }

            }
            public class AuditManualLogoffSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditManualLogoffSuccessfullyEventMessage(string logoffTime, ContextValues.Authentication.LoginType loginType, string userName)
                {
                    Contexts.Add(ContextKeys.Authentication.LogoffTime, logoffTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }

            }
            public class AuditForceLogoffSuccessfullyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditForceLogoffSuccessfullyEventMessage(string logoffTime, ContextValues.Authentication.LoginType loginType, string operatingUserName, string userName)
                {
                    Contexts.Add(ContextKeys.Authentication.LogoffTime, logoffTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.OperatingUserName, operatingUserName);
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }
            }
            public class AuditAddAccountEventMessage : AveEventMessage
            {
                public AuditAddAccountEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditAddAccountFailedEventMessage : AveEventMessage
            {
                public AuditAddAccountFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditEditAccountEventMessage : AveEventMessage
            {
                public AuditEditAccountEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditEditAccountFailedEventMessage : AveEventMessage
            {
                public AuditEditAccountFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeleteAccountEventMessage : AveEventMessage
            {
                public AuditDeleteAccountEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; ; } }
            }
            public class AuditDeleteAccountFailedEventMessage : AveEventMessage
            {
                public AuditDeleteAccountFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDisableAccountEventMessage : AveEventMessage
            {
                public AuditDisableAccountEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditDisableAccountFailedEventMessage : AveEventMessage
            {
                public AuditDisableAccountFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditEnableAccountEventMessage : AveEventMessage
            {
                public AuditEnableAccountEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditEnableAccountFailedEventMessage : AveEventMessage
            {
                public AuditEnableAccountFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditAddPermissionLevelEventMessage : AveEventMessage
            {
                public AuditAddPermissionLevelEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditAddPermissionLevelFailedEventMessage : AveEventMessage
            {
                public AuditAddPermissionLevelFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditEditPermissionLevelEventMessage : AveEventMessage
            {
                public AuditEditPermissionLevelEventMessage(string profileName, string role, string oldcontent, string newcontent, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.OldContent, oldcontent);
                    Contexts.Add(ContextKeys.Audit.NewContent, newcontent);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditEditPermissionLevelFailedEventMessage : AveEventMessage
            {
                public AuditEditPermissionLevelFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeletePermissionLevelsEventMessage : AveEventMessage
            {
                public AuditDeletePermissionLevelsEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }
            }
            public class AuditDeletePermissionLevelsFailedEventMessage : AveEventMessage
            {
                public AuditDeletePermissionLevelsFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditAddGroupEventMessage : AveEventMessage
            {
                public AuditAddGroupEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditAddGroupFailedEventMessage : AveEventMessage
            {
                public AuditAddGroupFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditEditGroupEventMessage : AveEventMessage
            {
                public AuditEditGroupEventMessage(string profileName, string role, string content, string newContent, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.OldContent, content);
                    Contexts.Add(ContextKeys.Audit.NewContent, newContent);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditEditGroupFailedEventMessage : AveEventMessage
            {
                public AuditEditGroupFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeleteGroupsEventMessage : AveEventMessage
            {
                public AuditDeleteGroupsEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.WARN; } }
            }
            public class AuditDeleteGroupsFailedEventMessage : AveEventMessage
            {
                public AuditDeleteGroupsFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditAddUsertoGivenGroupEventMessage : AveEventMessage
            {
                public AuditAddUsertoGivenGroupEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditAddUsertoGivenGroupFailedEventMessage : AveEventMessage
            {
                public AuditAddUsertoGivenGroupFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditRemoveUsersFromGroupEventMessage : AveEventMessage
            {
                public AuditRemoveUsersFromGroupEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditDeleteNotificationMessageEventMessage : AveEventMessage
            {
                public AuditDeleteNotificationMessageEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditLoginEventMessage : AveEventMessage
            {
                public AuditLoginEventMessage(string loginAddress, string loginTime, ContextValues.Authentication.LoginType loginType, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Authentication.LoginAddress, loginAddress);
                    Contexts.Add(ContextKeys.Authentication.LoginTime, loginTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditLoginFailedEventMessage : AveEventMessage
            {
                public AuditLoginFailedEventMessage(string loginAddress, string loginTime, ContextValues.Authentication.LoginType loginType, string userName, Exception e)
                    : base(e)
                {
                    Contexts.Add(ContextKeys.Authentication.LoginAddress, loginAddress);
                    Contexts.Add(ContextKeys.Authentication.LoginTime, loginTime);
                    Contexts.Add(ContextKeys.Authentication.LoginType, ContextValues.GetContextValue(loginType));
                    Contexts.Add(ContextKeys.Authentication.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditRemoveUsersFromGroupFailedEventMessage : AveEventMessage
            {
                public AuditRemoveUsersFromGroupFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditCreateAccountProfileFailedEventMessage : AveEventMessage
            {
                public AuditCreateAccountProfileFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditUpdateAccountProfileFailedEventMessage : AveEventMessage
            {
                public AuditUpdateAccountProfileFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeleteAccountProfileFailedEventMessage : AveEventMessage
            {
                public AuditDeleteAccountProfileFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditSaveSystemSettingEventMessage : AveEventMessage
            {
                public AuditSaveSystemSettingEventMessage(string profileName, string role, string oldcontent, string newcontent, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.OldContent, oldcontent);
                    Contexts.Add(ContextKeys.Audit.NewContent, newcontent);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditUpdateSystemSecurityPolicyEventMessage : AveEventMessage
            {
                public AuditUpdateSystemSecurityPolicyEventMessage(string profileName, string role, string oldcontent, string newcontent, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.OldContent, oldcontent);
                    Contexts.Add(ContextKeys.Audit.NewContent, newcontent);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditUpdateSystemPasswordPolicyEventMessage : AveEventMessage
            {
                public AuditUpdateSystemPasswordPolicyEventMessage(string profileName, string role, string oldcontent, string newcontent, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.OldContent, oldcontent);
                    Contexts.Add(ContextKeys.Audit.NewContent, newcontent);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditChangePassphrsaseCharEventMessage : AveEventMessage
            {
                public AuditChangePassphrsaseCharEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditUpdateNotificationMessageEventMessage : AveEventMessage
            {
                public AuditUpdateNotificationMessageEventMessage(string profileName, string role, string oldcontent, string newcontent, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.OldContent, oldcontent);
                    Contexts.Add(ContextKeys.Audit.NewContent, newcontent);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditCreateNotificationMessageEventMessage : AveEventMessage
            {
                public AuditCreateNotificationMessageEventMessage(string profileName, string role, string oldcontent, string newcontent, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.OldContent, oldcontent);
                    Contexts.Add(ContextKeys.Audit.NewContent, newcontent);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditSaveNotificationSettingEventMessage : AveEventMessage
            {
                public AuditSaveNotificationSettingEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditSaveAgentProxyEventMessage : AveEventMessage
            {
                public AuditSaveAgentProxyEventMessage(string profileName, string role, string oldcontent, string newcontent, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.OldContent, oldcontent);
                    Contexts.Add(ContextKeys.Audit.NewContent, newcontent);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditApplyLicenseEventMessage : AveEventMessage
            {
                public AuditApplyLicenseEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }
            }
            public class AuditUpdateAccountProfileEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditUpdateAccountProfileEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditCreateAccountProfileEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditCreateAccountProfileEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditDeleteAccountProfileEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditDeleteAccountProfileEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditCreatePhysicalDeviceEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditCreatePhysicalDeviceEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditUpdatePhysicalDeviceEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditUpdatePhysicalDeviceEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditDeletePhysicalDeviceEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditDeletePhysicalDeviceEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditCreateLogicalDeviceEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditCreateLogicalDeviceEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditUpdateLogicalDeviceEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditUpdateLogicalDeviceEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditDeleteLogicalDeviceEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditDeleteLogicalDeviceEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditCreateStoragePolicyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditCreateStoragePolicyEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditUpdateStoragePolicyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditUpdateStoragePolicyEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditDeleteStoragePolicyEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditDeleteStoragePolicyEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditCreateSystemProfileEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditCreateSystemProfileEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditUpdateSystemProfileEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditUpdateSystemProfileEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditDeleteSystemProfileEventMessage : AveEventMessage
            {
                public override AveLogLevel LogLevel { get { return AveLogLevel.INFO; } }

                public AuditDeleteSystemProfileEventMessage(string profileName, string role, string content, AveAction action, string userName) : base()
                {
                    Contexts.Add(ContextKeys.Audit.Role, role);
                    Contexts.Add(ContextKeys.Audit.Content, content);
                    Contexts.Add(ContextKeys.Audit.Action, action.ToString());
                    Contexts.Add(ContextKeys.Audit.ObjectName, profileName);
                    Contexts.Add(ContextKeys.Audit.UserName, userName);
                }
            }
            public class AuditCreatePhysicalDeviceFailedEventMessage : AveEventMessage
            {
                public AuditCreatePhysicalDeviceFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditUpdatePhysicalDeviceFailedEventMessage : AveEventMessage
            {
                public AuditUpdatePhysicalDeviceFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeletePhysicalDeviceFailedEventMessage : AveEventMessage
            {
                public AuditDeletePhysicalDeviceFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditCreateLogicalDeviceFailedEventMessage : AveEventMessage
            {
                public AuditCreateLogicalDeviceFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditUpdateLogicalDeviceFailedEventMessage : AveEventMessage
            {
                public AuditUpdateLogicalDeviceFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeleteLogicalDeviceFailedEventMessage : AveEventMessage
            {
                public AuditDeleteLogicalDeviceFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditCreateStoragePolicyFailedEventMessage : AveEventMessage
            {
                public AuditCreateStoragePolicyFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditUpdateStoragePolicyFailedEventMessage : AveEventMessage
            {
                public AuditUpdateStoragePolicyFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeleteStoragePolicyFailedEventMessage : AveEventMessage
            {
                public AuditDeleteStoragePolicyFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditCreateSystemProfileFailedEventMessage : AveEventMessage
            {
                public AuditCreateSystemProfileFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditUpdateSystemProfileFailedEventMessage : AveEventMessage
            {
                public AuditUpdateSystemProfileFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeleteSystemProfileFailedEventMessage : AveEventMessage
            {
                public AuditDeleteSystemProfileFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditSaveSystemSettingFailedEventMessage : AveEventMessage
            {
                public AuditSaveSystemSettingFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditDeleteNotificationMessageFailedEventMessage : AveEventMessage
            {
                public AuditDeleteNotificationMessageFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditUpdateNotificationMessageFailedEventMessage : AveEventMessage
            {
                public AuditUpdateNotificationMessageFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditCreateNotificationMessageFailedEventMessage : AveEventMessage
            {
                public AuditCreateNotificationMessageFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
            public class AuditApplyLicenseFailedEventMessage : AveEventMessage
            {
                public AuditApplyLicenseFailedEventMessage() : base()
                {
                }
                public override AveLogLevel LogLevel { get { return AveLogLevel.ERROR; } }
            }
        }
    }

    //    [Obsolete("Using EventIds instead")]
    //    public class EventID
    //    {
    //
    //        public const int MediaService = 10000;
    //        public const int ReportService = 11000;
    //        public const int Packging = 11800; //11800 - 11999
    //        public const int MigrationRestoreWrapper = 20000;
    //        public const int eRoomMigrationBackup = 21000;
    //        public const int eRoomMigraitonRestore = 22000;
    //    }
    //
    //    [Obsolete("Using EventIds instead")]
    //    public class ServiceEventIds
    //    {
    //        public const int StartedSucceed = 5401;
    //        public const int StartedFailed = 5402;
    //        public const int StoppedSucceed = 5403;
    //        public const int ExitedUnexpected = 5404;
    //
    //
    //
    //    }
    //
    //    [Obsolete("Using EventIds instead")]
    //    public class RCProfileEventIds
    //    {
    //        public const int CreateProfile = 5405;
    //    }
    //
    //    [Obsolete("Using EventIds instead")]
    //    public class StorageOperationEventIds
    //    {
    //        public const int DeviceCanNotConnect = 2201;
    //        public const int AccessDenied = 2202;
    //        public const int DataWriteFailed = 2203;
    //        public const int DeviceSpaceFull = 2204;
    //        public const int DataNotFound = 2205;
    //        public const int DataReadFailed = 2206;
    //    }
    //
    //    [Obsolete("Using EventIds instead")]
    //    public class JobEventIds
    //    {
    //        public const int JobStarted = 2401;
    //        public const int JobCompleteWithException = 2402;
    //        public const int JobComplete = 2403;
    //        public const int JobFailed = 2404;
    //        public const int JobPauseOrResume = 2405;
    //        public const int JobRestartOrStop = 2406;
    //        public const int JobSkipped = 2407;
    //    }
    //
    //    [Obsolete("Using EventIds instead")]
    //    public class LicenseEventIds
    //    {
    //        public const int NoLicense = 1101;
    //        public const int LicenseExpired = 1102;
    //    }
    //
    //    [Obsolete("Using EventIds instead")]
    //    public class PlanConfigurationEventIds
    //    {
    //        public const int OperateProfileOrPlanFailed = 4401;
    //        public const int OperateMappingFailed = 4402;
    //        public const int OperateJobScheduleFailed = 4403;
    //    }
    //
    //    [Obsolete("Using EventIds instead")]
    //    public class DataOperationEventIds
    //    {
    //        public const int CreateStubDatabaseFailed = 1201;
    //        public const int CreateStubDatabaseSucceed = 1202;
    //        public const int OperateStubFailed = 1203;
    //        public const int InitStubDatabaseFailed = 1204;
    //        public const int ConfigStubDatabaseFailed = 1205;
    //        public const int ConfigStubDatabaseSucceed = 1206;
    //
    //        public const int InitReportDataBaseSucceed = 1207;
    //        public const int InitReportDataBaseFailed = 1208;
    //        public const int InitAuditorDataBaseSucceed = 1209;
    //        public const int InitAuditorDataBaseFailed = 1210;
    //
    //    }
}

