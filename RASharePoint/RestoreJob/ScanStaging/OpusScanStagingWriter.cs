using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SQLite;

namespace AvePoint.Item.Restore.ScanStaging
{
    internal sealed class OpusScanStagingWriter : IDisposable
    {
        private readonly SQLiteConnection _connection;
        private readonly SQLiteCommand _containerCommand;
        private readonly SQLiteCommand _itemCommand;
        private bool _disposed;

        internal OpusScanStagingWriter(OpusScanStagingDatabase database)
        {
            _connection = database?.Connection ?? throw new ArgumentNullException(nameof(database));
            _containerCommand = CreateContainerCommand(_connection);
            _itemCommand = CreateItemCommand(_connection);
        }

        internal void WriteContainers(IEnumerable<OpusScanContainer> containers)
        {
            ExecuteBatch(containers, _containerCommand, SetContainerParameters);
        }

        internal void WriteItems(IEnumerable<OpusScanItem> items)
        {
            ExecuteBatch(items, _itemCommand, SetItemParameters);
        }

        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _containerCommand.Dispose();
            _itemCommand.Dispose();
        }

        private static void ExecuteBatch<T>(
            IEnumerable<T> values,
            SQLiteCommand command,
            Action<SQLiteCommand, T> setParameters)
        {
            if (values == null)
            {
                return;
            }

            using SQLiteTransaction transaction = command.Connection.BeginTransaction();
            command.Transaction = transaction;
            bool hasValues = false;
            try
            {
                foreach (T value in values)
                {
                    setParameters(command, value);
                    command.ExecuteNonQuery();
                    hasValues = true;
                }

                if (hasValues)
                {
                    transaction.Commit();
                }
                else
                {
                    transaction.Rollback();
                }
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
            finally
            {
                command.Transaction = null;
            }
        }

        private static SQLiteCommand CreateContainerCommand(SQLiteConnection connection)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ScanContainers
                    (ContainerUrl, ParentUrl, SiteUrl, WebUrl, ContainerType, Name, DisplayName, FullPathForUI)
                VALUES
                    (@ContainerUrl, @ParentUrl, @SiteUrl, @WebUrl, @ContainerType, @Name, @DisplayName, @FullPathForUI)
                ON CONFLICT(ContainerUrl) DO UPDATE SET
                    ParentUrl = excluded.ParentUrl,
                    SiteUrl = excluded.SiteUrl,
                    WebUrl = excluded.WebUrl,
                    ContainerType = excluded.ContainerType,
                    Name = excluded.Name,
                    DisplayName = excluded.DisplayName,
                    FullPathForUI = excluded.FullPathForUI;";
            AddParameter(command, "@ContainerUrl", DbType.String);
            AddParameter(command, "@ParentUrl", DbType.String);
            AddParameter(command, "@SiteUrl", DbType.String);
            AddParameter(command, "@WebUrl", DbType.String);
            AddParameter(command, "@ContainerType", DbType.Int32);
            AddParameter(command, "@Name", DbType.String);
            AddParameter(command, "@DisplayName", DbType.String);
            AddParameter(command, "@FullPathForUI", DbType.String);
            command.Prepare();
            return command;
        }

        private static SQLiteCommand CreateItemCommand(SQLiteConnection connection)
        {
            SQLiteCommand command = connection.CreateCommand();
            command.CommandText = @"
                INSERT INTO ScanResults
                    (ItemId, UniqueId, SiteUrl, WebUrl, ListUrl, ParentUrl, FileUrl, FileName, Size, Extension)
                VALUES
                    (@ItemId, @UniqueId, @SiteUrl, @WebUrl, @ListUrl, @ParentUrl, @FileUrl, @FileName, @Size, @Extension)
                ON CONFLICT(ListUrl, ItemId) DO UPDATE SET
                    UniqueId = excluded.UniqueId,
                    SiteUrl = excluded.SiteUrl,
                    WebUrl = excluded.WebUrl,
                    ParentUrl = excluded.ParentUrl,
                    FileUrl = excluded.FileUrl,
                    FileName = excluded.FileName,
                    Size = excluded.Size,
                    Extension = excluded.Extension;";
            AddParameter(command, "@ItemId", DbType.String);
            AddParameter(command, "@UniqueId", DbType.String);
            AddParameter(command, "@SiteUrl", DbType.String);
            AddParameter(command, "@WebUrl", DbType.String);
            AddParameter(command, "@ListUrl", DbType.String);
            AddParameter(command, "@ParentUrl", DbType.String);
            AddParameter(command, "@FileUrl", DbType.String);
            AddParameter(command, "@FileName", DbType.String);
            AddParameter(command, "@Size", DbType.Int64);
            AddParameter(command, "@Extension", DbType.String);
            command.Prepare();
            return command;
        }

        private static void SetContainerParameters(SQLiteCommand command, OpusScanContainer container)
        {
            SetValue(command, "@ContainerUrl", container.ContainerUrl);
            SetValue(command, "@ParentUrl", container.ParentUrl);
            SetValue(command, "@SiteUrl", container.SiteUrl);
            SetValue(command, "@WebUrl", container.WebUrl);
            SetValue(command, "@ContainerType", (int)container.ContainerType);
            SetValue(command, "@Name", container.Name);
            SetValue(command, "@DisplayName", container.DisplayName);
            SetValue(command, "@FullPathForUI", container.FullPathForUI);
        }

        private static void SetItemParameters(SQLiteCommand command, OpusScanItem item)
        {
            SetValue(command, "@ItemId", item.ItemId);
            SetValue(command, "@UniqueId", item.UniqueId);
            SetValue(command, "@SiteUrl", item.SiteUrl);
            SetValue(command, "@WebUrl", item.WebUrl);
            SetValue(command, "@ListUrl", item.ListUrl);
            SetValue(command, "@ParentUrl", item.ParentUrl);
            SetValue(command, "@FileUrl", item.FileUrl);
            SetValue(command, "@FileName", item.FileName);
            SetValue(command, "@Size", item.Size);
            SetValue(command, "@Extension", item.Extension);
        }

        private static void AddParameter(SQLiteCommand command, string name, DbType type)
        {
            command.Parameters.Add(new SQLiteParameter(name, type));
        }

        private static void SetValue(SQLiteCommand command, string name, object value)
        {
            command.Parameters[name].Value = value ?? DBNull.Value;
        }
    }
}
