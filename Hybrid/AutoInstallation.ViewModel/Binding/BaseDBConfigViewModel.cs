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
using System.Data.SqlClient;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
using AutoInstallation.Contract;
using AutoInstallation.Contract.DataBase;
using AutoInstallation.Contract.Interface;
using AutoInstallation.Contract.Message;
using AutoInstallation.ViewModel.Handler;
using AutoInstallationCommon.Utility;
using AutoInstallationUtility;
using RESX = AutoInstallation.Records.App.Resources.Resource;


namespace AutoInstallation.ViewModel.Binding
{
    public abstract class BaseDBConfigViewModel : NotifyPropertyChanged, IPageViewModel
    {
        protected static AveLogger logger = AveLogger.GetInstance(MethodBase.GetCurrentMethod().DeclaringType);

        protected virtual bool CheckDB()
        {
            var ret = true;
            var dbUtility = new DatabaseUtility(AuthenticationType, DBServer, DBName, UserName, PasswordAtGUI);
            var str = dbUtility.InitializeMasterDBConnectionString();
            var connected = false;
            try
            {
                connected = dbUtility.OpenConnection(str);
            }
            catch (Exception ex)
            {
                //string pat = @"(\w+)(;Password=)(\w+)(;\w+)";
                //Regex r = new Regex(pat, RegexOptions.IgnoreCase);
                //string[] u = r.Split(str);
                var temp = new DatabaseUtility(AuthenticationType, DBServer, DBName, UserName,
                    Utilities.Encode(PasswordAtGUI, EncodingBlob.ProtectKeyType.Null));
                var sql = temp.InitializeMasterDBConnectionString();
                logger.Error("Can not connect db server.ConnectionString:{0},Error:{1}", sql, ex.ToString());
                connected = false;
            }

            if (!connected)
            {
                DBServerMsg = RESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBCONNECTERROR;
                ret = false;
            }
            else
            {
                try
                {
                    if (!dbUtility.IsDBExist())
                    {
                        var result = MessageResult.None;
                        Application.Current.Dispatcher.Invoke(
                            () =>
                            {
                                result = PopupMessageBox.GetInstance().ShowMessageBox(
                                    RESX.COMMON_MESSAGEBOX_DB_CREATEPROMPT,
                                    RESX.RecordsAppInstallation_Key_GUI_Btn_Create, RESX.COMMON_BTN_CANCEL);
                            }, DispatcherPriority.Normal);
                        if (result == MessageResult.OK)
                            try
                            {
                                if (!dbUtility.CreateDatabase())
                                {
                                    logger.Error("Create DB Failed.Error.");
                                    DBNameMsg = RESX.COMMON_MESSAGEBOX_DB_CREATEERROR;
                                    ret = false;
                                }

                                //string Recorddb = dbUtility.InitializeSpecifiedDBConnectionString();
                                //OnPremiseDBContext context = new OnPremiseDBContext(Recorddb);
                                //context.Sites.Find(Guid.NewGuid());
                            }
                            catch (Exception ex)
                            {
                                logger.Error("Create DB Failed.Error:{0}", ex.ToString());
                                DBNameMsg = RESX.COMMON_MESSAGEBOX_DB_CREATEERROR;
                                ret = false;
                            }
                        else
                            ret = false;
                    }
                }
                catch (Exception ex)
                {
                    DBNameMsg = RESX.COMMON_MESSAGEBOX_DB_CREATEERROR;
                    ret = false;
                    logger.Error("Can not get or create a db.DBName:{0},Error:{1}", dbUtility.DBName, ex.ToString());
                }
            }

            return ret;
        }

        #region Property

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

        private readonly Dictionary<string, Authentication> authenticationSource =
            new Dictionary<string, Authentication>();

        public Dictionary<string, Authentication> AuthenticationSource
        {
            get
            {
                if (authenticationSource.Count == 0)
                {
                    authenticationSource.Add(RESX.COMMON_TEXT_WINDOWSAUTHENTICATION,
                        Authentication.WindowsAuthentication);
                    authenticationSource.Add(RESX.COMMON_TEXT_SQLAUTHENTICATION, Authentication.SQLAuthentication);
                }

                return authenticationSource;
            }
        }

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

        public SqlConnectionStringBuilder ConnectString
        {
            get
            {
                var connectionStringBuilder = new SqlConnectionStringBuilder();
                connectionStringBuilder.DataSource = dbServer;
                connectionStringBuilder.InitialCatalog = dbName;
                connectionStringBuilder.Pooling = true;
                connectionStringBuilder.MultipleActiveResultSets = true;
                if (authenticationType == Authentication.SQLAuthentication)
                {
                    connectionStringBuilder.IntegratedSecurity = false;
                    connectionStringBuilder.UserID = UserName;
                    connectionStringBuilder.Password = Utilities.Encode(Password, EncodingBlob.ProtectKeyType.Null);
                }
                else
                {
                    connectionStringBuilder.IntegratedSecurity = true;
                }

                return connectionStringBuilder;
            }
        }

        public SqlConnectionStringBuilder GetConnectString(bool encodePassword)
        {
            var connectionStringBuilder = new SqlConnectionStringBuilder();
            connectionStringBuilder.DataSource = dbServer;
            connectionStringBuilder.InitialCatalog = dbName;
            connectionStringBuilder.Pooling = true;
            connectionStringBuilder.MultipleActiveResultSets = true;
            if (authenticationType == Authentication.SQLAuthentication)
            {
                connectionStringBuilder.IntegratedSecurity = false;
                connectionStringBuilder.UserID = UserName;
                if (encodePassword)
                    connectionStringBuilder.Password = Utilities.Encode(Password, EncodingBlob.ProtectKeyType.Null);
                else
                    connectionStringBuilder.Password = Password;
            }
            else
            {
                connectionStringBuilder.IntegratedSecurity = true;
            }

            return connectionStringBuilder;
        }

        #endregion

        #region Interface Impl

        private bool isChecking;

        public bool IsChecking
        {
            get { return isChecking; }
            set
            {
                isChecking = value;
                OnPropertyChanged("IsChecking");
            }
        }

        public virtual void ResetPrompt()
        {
            DBServerMsg = string.Empty;
            DBNameMsg = string.Empty;
            UserNameMsg = string.Empty;
            IsPasswordMsgVis = Visibility.Collapsed;
            PasswordErrorMsg = string.Empty;
        }

        public abstract void LoadPage();

        public virtual bool CheckValue()
        {
            var ret = true;
            if (string.IsNullOrEmpty(DBServer))
            {
                DBServerMsg = RESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBSERVEREMPTY;
                ret = false;
            }
            else if (string.IsNullOrEmpty(DBName))
            {
                DBNameMsg = RESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBNAMEEMPTY;
                ret = false;
            }
            else if (string.IsNullOrWhiteSpace(DBName))
            {
                DBNameMsg = RESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBNAMEEMWP;
                ret = false;
            }
            else if (DBName.IndexOf('*') > -1 || DBName.IndexOf('|') > -1 || DBName.IndexOf('?') > -1)
            {
                DBNameMsg = RESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBNAMEEMIC;
                ret = false;
            }
            else if (AuthenticationType == Authentication.WindowsAuthentication)
            {
                ret = CheckDB();
            }
            else if (AuthenticationType == Authentication.SQLAuthentication)
            {
                if (string.IsNullOrEmpty(UserName))
                {
                    UserNameMsg = RESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBUSERNAMEEMPTY;
                    ret = false;
                }
                else if (string.IsNullOrEmpty(PasswordAtGUI))
                {
                    PasswordErrorMsg = RESX.WEBSITEINSTALLATION_ADVANCE_LB_PROMPT_DBPASSWORDEMPTY;
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

        #endregion
    }
}