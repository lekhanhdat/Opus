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




namespace AvePoint.ObjectModel.Server13
{
    public class AveColumn
    {
        private string mBackupName; // when backup the column, we use this name instead of the sql column name.
        private string mColumnName;
        private bool mIsDisplayColumn;
        private bool mIsUser;   // This field user or not.

        public bool IsDisplayColumn
        {
            get { return this.mIsDisplayColumn; }
            set { this.mIsDisplayColumn = value; }
        }

        public string ColumnName
        {
            get { return this.mColumnName; }
            set { this.mColumnName = value; }
        }

        public string BackupName
        {
            get { return this.mBackupName; }
            set { this.mBackupName = value; }
        }

        public bool IsUser
        {
            get { return mIsUser; }
        }

        public AveColumn()
        { }

        public AveColumn(string backupName, string columnName, bool isDisplayColumn)
            : this(backupName, columnName, isDisplayColumn, false)
        { }

        public AveColumn(string backupName, string columnName, bool isDisplayColumn, bool isUser)
        {
            mBackupName = backupName;
            mColumnName = columnName;
            mIsDisplayColumn = isDisplayColumn;
            mIsUser = isUser;
        }
    }
}
