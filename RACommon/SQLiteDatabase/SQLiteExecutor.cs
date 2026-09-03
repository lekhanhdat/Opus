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
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.IO;
using System.Threading.Tasks;
using RACommon.SQLiteDatabase.Extensions;
using Util;

namespace RACommon.SQLiteDatabase;

internal sealed class SQLiteExecutor : DatabaseHelperBase, IDisposable, IAsyncDisposable
{
    protected override DbProviderFactory DbProviderFactory => SQLiteFactory.Instance;

    protected String FilePath { get; set; }

    private readonly bool withPassword;
    private string WalFile => $"{FilePath}-wal";
    private string ShmFile => $"{FilePath}-shm";

    private bool withWal;

    public SQLiteExecutor(String filePath, String? password = null, bool withWal = true)
    {
        filePath.ThrowIfNullOrEmpty();

        if (password.IsNotNullOrEmpty())
        {
            withPassword = true;
        }
        this.withWal = withWal;
        connectionString = withWal
            ? SQLiteConnectionStringExtensions.BuildWalModeConnectionString(filePath, password)
            : SQLiteConnectionStringExtensions.BuildConnectionString(filePath, password);
        FilePath = filePath;
    }


    /// <summary>
    /// Don't use it
    /// </summary>
    public override DataSet ExecuteQuery(String commandText, Dictionary<String, Object> parameters, TransactionContext transaction = null!, int commandTimeout = 300) => throw new NotImplementedException();

    /// <summary>
    /// Don't use it
    /// </summary>
    public override Task<DataSet> ExecuteQueryAsync(String commandText, Dictionary<String, Object> parameters, TransactionContext transaction = null!, int commandTimeout = 300) => throw new NotImplementedException();

    public void Close()
    {
        if (!withWal) return;
        if ((!String.IsNullOrEmpty(FilePath)) && File.Exists(FilePath))
        {
            if (withPassword)
            {
                EncryptionClose();
            }
            else
            {
                OrdinaryClose();
            }
        }
    }

    public async ValueTask CloseAsync()
    {
        Close();
        await Task.CompletedTask;
    }

    public DbConnection GetConnection() => SQLiteConnectionStringExtensions.OpenConnection(connectionString);

    protected override DbConnection GetConnection(Boolean isRead = false, String cmdTxt = "") => GetConnection();

    public async Task<DbConnection> GetConnectionAsync() => await SQLiteConnectionStringExtensions.OpenConnectionAsync(connectionString);

    protected override async Task<DbConnection> GetConnectionAsync(Boolean isRead = false, String cmdTxt = "") => await GetConnectionAsync();

    public void Dispose() => Close();

    public async ValueTask DisposeAsync() => await CloseAsync();

    private void OrdinaryClose()
    {
        SQLiteConnection.ConnectionPool.ClearPool(FilePath);

        if (!Closed())
        {
            PollyRetry.Handle<Exception>(() =>
            {
                CheckPoint();

                SQLiteConnection.ConnectionPool.ClearPool(FilePath);

                if (!Closed())
                {
                    throw new CloseException();
                }
            }, null!, 3, 3000);
        }

        bool Closed() => !File.Exists(WalFile) && !File.Exists(ShmFile);
    }

    private void EncryptionClose()
    {
        CheckPoint();

        if (!Closed())
        {
            PollyRetry.Handle<Exception>(() =>
            {
                CheckPoint();

                SQLiteConnection.ConnectionPool.ClearPool(FilePath);

                if (!Closed())
                {
                    throw new CloseException();
                }
            }, null!, 3, 3000);
        }

        bool Closed() => (!File.Exists(WalFile) && !File.Exists(ShmFile)) || (File.Exists(WalFile) && new FileInfo(WalFile).Length == 0);
    }

    // https://www.sqlite.org/pragma.html#pragma_wal_checkpoint
    private void CheckPoint() => ExecuteNonQuery("PRAGMA wal_checkpoint(TRUNCATE)", null);
}