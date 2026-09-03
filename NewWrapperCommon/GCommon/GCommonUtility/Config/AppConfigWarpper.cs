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
using System.Configuration;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Xml;
using System.Data;
using System.Xml.Linq;

namespace AvePoint.GCommon.Utility.Config
{
    public static class AppConfigWarpper
    {
        private static AveLogger logger = AveLogger.GetInstance(typeof(AppConfigWarpper));

        #region Connection String

        ///<summary> 
        ///依据连接串名字connectionName返回数据连接字符串  
        ///</summary> 
        ///<param name="connectionName"></param> 
        ///<returns></returns> 
        public static string GetConnectionStringsConfig(string connectionName)
        {
            return ConfigurationManager.ConnectionStrings[connectionName].ConnectionString.ToString(CultureInfo.InvariantCulture);
        }

        ///<summary> 
        ///更新连接字符串  
        ///</summary> 
        ///<param name="newName">连接字符串名称</param> 
        ///<param name="newConString">连接字符串内容</param> 
        ///<param name="newProviderName">数据提供程序名称</param> 
        public static void UpdateConnectionStringsConfig(string newName, string newConString, string newProviderName)
        {
            //记录该连接串是否已经存在  
            bool isModified = ConfigurationManager.ConnectionStrings[newName] != null;
            //如果要更改的连接串已经存在  

            //新建一个连接字符串实例  
            ConnectionStringSettings mySettings = new ConnectionStringSettings(newName, newConString, newProviderName);
            // 打开可执行的配置文件*.exe.config  
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            // 如果连接串已存在，首先删除它  
            if (isModified)
            {
                config.ConnectionStrings.ConnectionStrings.Remove(newName);
            }
            // 将新的连接串添加到配置文件中.  
            config.ConnectionStrings.ConnectionStrings.Add(mySettings);
            // 保存对配置文件所作的更改  
            config.Save(ConfigurationSaveMode.Modified);
            // 强制重新载入配置文件的ConnectionStrings配置节  
            ConfigurationManager.RefreshSection("ConnectionStrings");
        }
        #endregion

        #region App Setting

        ///<summary> 
        ///返回＊.exe.config文件中appSettings配置节的value项  
        ///</summary> 
        ///<param name="strKey"></param> 
        ///<returns></returns> 
        public static string GetAppSettings(string strKey)
        {
            foreach (string key in ConfigurationManager.AppSettings)
            {
                if (key == strKey)
                {
                    return ConfigurationManager.AppSettings[strKey];
                }
            }
            return null;
        }

        ///<summary>  
        ///在＊.exe.config文件中appSettings配置节增加一对键、值对  
        ///</summary>  
        ///<param name="newKey"></param>  
        ///<param name="newValue"></param>  
        public static void UpdateAppSettings(string newKey, string newValue)
        {
            bool isModified = false;
            foreach (string key in ConfigurationManager.AppSettings)
            {
                if (key == newKey)
                {
                    isModified = true;
                }
            }


            // Open App.Config of executable  
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            // You need to remove the old settings object before you can replace it  
            if (isModified)
            {
                config.AppSettings.Settings.Remove(newKey);
            }

            // Add an Application Setting.  
            config.AppSettings.Settings.Add(newKey, newValue);
            // Save the changes in App.config file.  
            config.Save(ConfigurationSaveMode.Modified);
            // Force a reload of a changed section.  
            ConfigurationManager.RefreshSection("appSettings");
        }

        #endregion

        /// <summary>
        /// 更新system.data信息
        /// </summary>
        /// <param name="name"></param>
        /// <param name="invariant"></param>
        /// <param name="description"></param>
        /// <param name="type"></param>
        public static void UpdateSystemDataSettings(string name, string invariant, string description, string type)
        {
            var dataSet = ConfigurationManager.GetSection("system.data") as System.Data.DataSet;
            dataSet.Tables[0].Rows.Add(name, description, invariant, type);

            ConfigurationManager.RefreshSection("system.data");
        }

        public static void UpdateOtherSettings(string section, string key, string value)
        {
            Configuration config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
            ConfigurationSection configSection = config.GetSection(section);

            TimerServiceConfigurationSection configSectionC = new TimerServiceConfigurationSection();


            System.Data.DataSet data = ConfigurationManager.GetSection(section) as System.Data.DataSet;

            bool findColumn = false;

            if (data != null)
            {
                foreach (DataTable table in data.Tables)
                {
                    logger.Debug("table: " + table.TableName);
                    foreach (DataRow row in table.Rows)
                    {
                        logger.Debug("row: " + row);
                        foreach (DataColumn column in table.Columns)
                        {
                            if (column.ColumnName.Equals(key, StringComparison.OrdinalIgnoreCase))
                            {
                                row[column] = key;
                                findColumn = true;
                                break;
                            }
                            logger.Debug("column: " + column.ColumnName + " : " + row[column]);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Add sqlLite configuration to config
        /// </summary>
        /// <param name="filePath"></param>
        /// <returns></returns>
        public static bool AddSqlLiteConfiguration(string filePath = null)
        {
            try
            {
                if (filePath == null || filePath.Equals(string.Empty))
                {
                    filePath = AppDomain.CurrentDomain.SetupInformation.ConfigurationFile;
                }
                if (filePath == null || filePath.Equals(string.Empty))
                {
                    filePath = "ControlTimerService.exe.config";
                }
                XElement doc = XElement.Load(filePath);
                bool needSave = false;
                //system.data:
                XElement data = new XElement("system.data",
                    new XElement("DbProviderFactories",
                        new XElement("remove", new XAttribute("invariant", "System.Data.SQLite")),
                        new XElement("add",
                            new XAttribute("name", "SQLite Data Provider"),
                            new XAttribute("invariant", "System.Data.SQLite"),
                            new XAttribute("description", ".Net Framework Data Provider for SQLite"),
                            new XAttribute("type", "System.Data.SQLite.SQLiteFactory, System.Data.SQLite")
                            )
                    )
                );
                //query:
                var system = doc.Element("system.data");
                if (system == null)
                {
                    doc.Add(data);
                    needSave = true;
                }
                else
                {
                    var factories = system.Elements("DbProviderFactories").FirstOrDefault();
                    if (factories == null)
                    {
                        system.Add(data.Element("DbProviderFactories"));
                        needSave = true;
                    }
                    else
                    {
                        var remove = factories.Element("remove");
                        var add = factories.Element("add");
                        if (remove == null || add == null)
                        {
                            if (remove != null) { remove.Remove(); }
                            if (add != null) { add.Remove(); }
                            factories.Add(data.Element("DbProviderFactories").Element("remove"));
                            factories.Add(data.Element("DbProviderFactories").Element("add"));
                            needSave = true;
                        }
                    }
                }
                if (needSave)
                {
                    doc.Save(filePath);
                }
            }
            catch (Exception e)
            {
                logger.Error("Failed to update sqlite configuration in timer service config file" + e.ToString());
                return false;
            }

            ConfigurationManager.RefreshSection("system.data");
            return true;
        }
    }

    public class TimerServiceConfigurationSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _Properties;
        private static readonly ConfigurationProperty _FileName = new ConfigurationProperty("fileName", typeof(string), "default.txt", ConfigurationPropertyOptions.IsRequired);

        public TimerServiceConfigurationSection()
        {
            {
                // Property initialization
                _Properties = new ConfigurationPropertyCollection { _FileName };
            }
        }
    }
}
