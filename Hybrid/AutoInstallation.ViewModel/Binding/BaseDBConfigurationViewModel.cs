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
using AvePoint.Labs.AutoInstallation.Common.Contract;
using AvePoint.Labs.AutoInstallation.Common.Contract.DataBase;
using AvePoint.Labs.AutoInstallation.Common.Contract.Interface;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using WEBRESX = AvePoint.Labs.AutoInstallation.Common.Resource.WebSiteInstallation.Resource;
using COMMRESX = AvePoint.Labs.AutoInstallation.Common.Resource.Common.Resource;
using APPRESX = AvePoint.Labs.AutoInstallation.Common.Resource.AppInstallation.Resource;
using AvePoint.Labs.AutoInstallation.Common.Utility.GlobalFunctions;
using AvePoint.Labs.AutoInstallation.Common.Utility;
using System.Reflection;
using AvePoint.Labs.AutoInstallation.Common.Contract.Message;
using AvePoint.Labs.AutoInstallation.Common.ViewModel.Handler;
using System.Windows.Threading;
using System.Data.SqlClient;
using Avepoint.Labs.Meetings.Business.SqlServer;
using Avepoint.Labs.Meetings.Business.SqlServer.Impl;
using Avepoint.Labs.Meetings.Business.SqlServer.Entity;
using AvePoint.Labs.AutoInstallation.Common.Contract.Navigation;

namespace AvePoint.Labs.AutoInstallation.Common.ViewModel.Binding
{
    public class BaseDBConfigurationViewModel : NotifyPropertyChanged, IDBConfigurationViewModel
    {
        private static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);
        private string dbServer = string.Empty;
        public string DBServer
        {
            get { return dbServer; }
            set
            {
                dbServer = value;
                OnPropertyChanged("DBServer");
            }
        }
        public virtual void AddReportItem(IPreviewViewModel preview)
        {
            throw new NotImplementedException();
        }
        public void ResetPrompt()
        {
            DBServerMsg = string.Empty;
            DBNameMsg = string.Empty;
            UserNameMsg = string.Empty;
            IsPasswordMsgVis = Visibility.Collapsed;
            PasswordErrorMsg = string.Empty;
        }

        private bool isChecking = false;
        public bool IsChecking
        {
            get { return isChecking; }
            set
            {
                isChecking = value;
                OnPropertyChanged("IsChecking");
            }
        }
        private string dbServerMsg = string.Empty;
        public string DBServerMsg
        {
            get { return dbServerMsg; }
            set
            {
                dbServerMsg = value;
                OnPropertyChanged("DBServerMsg");
            }
        }
        private string dbName = string.Empty;
        public string DBName
        {
            get { return dbName; }
            set
            {
                dbName = value;
                OnPropertyChanged("DBName");
            }
        }
        private string dbNameMsg = string.Empty;
        public string DBNameMsg
        {
            get { return dbNameMsg; }
            set
            {
                dbNameMsg = value;
                OnPropertyChanged("DBNameMsg");
            }
        }
        private Authentication authenticationType = Authentication.WindowsAuthentication;
        public Authentication AuthenticationType
        {
            get { return authenticationType; }
            set
            {
                authenticationType = value;
                OnPropertyChanged("AuthenticationType");
            }
        }
        private Dictionary<string, Authentication> authenticationSource = new Dictionary<string, Authentication>();
        public Dictionary<string, Authentication> AuthenticationSource
        {
            get
            {
                if (authenticationSource.Count == 0)
                {
                    authenticationSource.Add(COMMRESX.COMMON_TEXT_WINDOWSAUTHENTICATION, Authentication.WindowsAuthentication);
                    authenticationSource.Add(COMMRESX.COMMON_TEXT_SQLAUTHENTICATION, Authentication.SQLAuthentication);
                }
                return authenticationSource;
            }
        }
        private string passwordAtGUI = string.Empty;
        public string PasswordAtGUI
        {
            get { return passwordAtGUI; }
            set
            {
                passwordAtGUI = value;
                OnPropertyChanged("PasswordAtGUI");
            }
        }
        private string passwordErrorMsg = string.Empty;
        public string PasswordErrorMsg
        {
            get { return passwordErrorMsg; }
            set
            {
                passwordErrorMsg = value;
                OnPropertyChanged("PasswordErrorMsg");
            }
        }
        private Visibility isPasswordMsgVis = Visibility.Collapsed;
        public Visibility IsPasswordMsgVis
        {
            get { return isPasswordMsgVis; }
            set
            {
                isPasswordMsgVis = value;
                OnPropertyChanged("IsPasswordMsgVis");
            }
        }
        public string Password { get; set; }
        private string userName = string.Empty;
        public string UserName
        {
            get { return userName; }
            set
            {
                userName = value;
                OnPropertyChanged("UserName");
            }
        }
        private string userNameMsg = string.Empty;
        public string UserNameMsg
        {
            get { return userNameMsg; }
            set
            {
                userNameMsg = value;
                OnPropertyChanged("UserNameMsg");
            }
        }
        public string ConnectString
        {
            get
            {
                SqlConnectionStringBuilder connectionStringBuilder = new SqlConnectionStringBuilder();
                connectionStringBuilder.DataSource = dbServer;
                connectionStringBuilder.InitialCatalog = dbName;
                connectionStringBuilder.Pooling = true;
                if (authenticationType == Authentication.SQLAuthentication)
                {
                    connectionStringBuilder.IntegratedSecurity = false;
                    connectionStringBuilder.UserID = UserName;
                    connectionStringBuilder.Password = AvePoint.Labs.Utility.Utilities.Encode(Password, AvePoint.Labs.Utility.EncodingBlob.ProtectKeyType.Null);
                }
                else
                {
                    connectionStringBuilder.IntegratedSecurity = true;
                }
                return connectionStringBuilder.ConnectionString;
            }
        }
        public bool CheckValue()
        {
            bool ret = true;
            if (string.IsNullOrEmpty(DBServer))
            {
                DBServerMsg = WEBRESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBSERVEREMPTY;
                ret = false;
            }
            else if (string.IsNullOrEmpty(DBName))
            {
                DBNameMsg = WEBRESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBNAMEEMPTY;
                ret = false;
            }
            else if (string.IsNullOrWhiteSpace(DBName))
            {
                DBNameMsg = WEBRESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBNAMEEMWP;
                ret = false;
            }
            else if (DBName.IndexOf('*') > -1 || DBName.IndexOf('|') > -1 || DBName.IndexOf('?') > -1)
            {
                DBNameMsg = WEBRESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBNAMEEMIC;
                ret = false;
            }
            //else if (string.IsNullOrEmpty(UserName))
            //{
            //    UserNameMsg = WEBRESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBUSERNAMEEMPTY;
            //    ret = false;
            //}
            else if (AuthenticationType == Authentication.WindowsAuthentication)
            {
                ret = CheckDB();
            }
            else if (AuthenticationType == Authentication.SQLAuthentication)
            {
                if (string.IsNullOrEmpty(UserName))
                {
                        UserNameMsg = WEBRESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBUSERNAMEEMPTY;
                        ret = false;
                }
                else if (string.IsNullOrEmpty(PasswordAtGUI))
                {
                    PasswordErrorMsg = WEBRESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBPASSWORDEMPTY;
                    IsPasswordMsgVis = Visibility.Visible;
                    ret = false;
                }
                else
                {
                    ret = CheckDB();
                }
            }
            Password = PasswordAtGUI;
            return ret;
        }
        private bool CheckDB()
        {
            bool ret = true;
            DatabaseUtility dbUtility = new DatabaseUtility(AuthenticationType, DBServer, DBName, UserName, PasswordAtGUI);
            string str = dbUtility.InitializeMasterDBConnectionString();
            bool connected = false;
            try
            {
                connected = dbUtility.OpenConnection(str);
            }
            catch (Exception ex)
            {
                //string pat = @"(\w+)(;Password=)(\w+)(;\w+)";
                //Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                //string[] u = r.Split(str);
                DatabaseUtility temp = new DatabaseUtility(AuthenticationType, DBServer, DBName, UserName, AvePoint.Labs.Utility.Utilities.Encode(PasswordAtGUI, AvePoint.Labs.Utility.EncodingBlob.ProtectKeyType.Null));
                string sql = temp.InitializeMasterDBConnectionString();
                logger.Error("Can not connect db server.ConnectionString:{0},Error:{1}", sql, ex.ToString());
                connected = false;
            }
            if (!connected)
            {
                DBServerMsg = WEBRESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBCONNECTERROR;
                ret = false;
            }
            else
            {
                try
                {
                    if (!dbUtility.IsDBExist())
                    {
                        MessageResult result = MessageResult.None;
                        System.Windows.Application.Current.Dispatcher.Invoke(new Action(() =>
                        {
                            result = PopupMessageBox.GetInstance().ShowMessageBox(COMMRESX.COMMON_MESSAGEBOX_DB_CREATEPROMPT, APPRESX.MeetingsAppInstallation_Key_GUI_Btn_Create, COMMRESX.COMMON_BTN_CANCEL);

                        }), DispatcherPriority.Normal);
                        if (result == MessageResult.OK)
                        {
                            //if (!dbUtility.CreateDatabase())
                            try
                            {
                                string meetingdb = dbUtility.InitializeSpecifiedDBConnectionString();
                                OnPremiseDBContext context = new OnPremiseDBContext(meetingdb);
                                context.Sites.Find(Guid.NewGuid());
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Create DB Failed.Error:{0}", ex.ToString());
                                    DBNameMsg = COMMRESX.COMMON_MESSAGEBOX_DB_CREATEERROR;
                                    ret = false;
                                
                            }
                        }
                        else
                        {
                            ret = false;
                        }
                    }
                }
                catch (Exception ex)
                {
                    DBNameMsg = COMMRESX.COMMON_MESSAGEBOX_DB_CREATEERROR;
                    ret = false;
                    logger.Error("Can not get or create a db.DBName:{0},Error:{1}", dbUtility.DBName, ex.ToString());
                }
            }
            return ret;
        }
    }
}
