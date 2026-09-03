using System;
using System.Collections.Generic;
using System.Data.SQLite;

namespace AvePoint.Item.Restore.ScanStaging
{
    internal sealed class OpusScanBatch
    {
        internal OpusScanBatch(IReadOnlyList<OpusScanItem> items, IReadOnlyList<OpusScanContainer> containers, long lastRowId)
        {
            Items = items;
            Containers = containers;
            LastRowId = lastRowId;
        }

        internal IReadOnlyList<OpusScanItem> Items { get; }
        internal IReadOnlyList<OpusScanContainer> Containers { get; }
        internal long LastRowId { get; }
    }

    internal sealed class OpusScanStagingReader
    {
        private readonly SQLiteConnection _connection;

        internal OpusScanStagingReader(OpusScanStagingDatabase database)
        {
            _connection = database?.Connection ?? throw new ArgumentNullException(nameof(database));
        }

        internal IEnumerable<OpusScanBatch> ReadBatches(int pageSize)
        {
            if (pageSize <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(pageSize));
            }

            long cursor = 0;
            while (true)
            {
                List<OpusScanItem> items = ReadItems(cursor, pageSize);
                if (items.Count == 0)
                {
                    yield break;
                }

                yield return new OpusScanBatch(items, ReadRequiredContainers(items), items[items.Count - 1].RowId);
                cursor = items[items.Count - 1].RowId;
            }
        }

        private List<OpusScanItem> ReadItems(long cursor, int pageSize)
        {
            List<OpusScanItem> items = new List<OpusScanItem>(pageSize);
            using SQLiteCommand command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT RowId, ItemId, UniqueId, SiteUrl, WebUrl, ListUrl, ParentUrl, FileUrl, FileName, Size, Extension
                FROM ScanResults
                WHERE RowId > @Cursor
                ORDER BY RowId
                LIMIT @PageSize;";
            command.Parameters.AddWithValue("@Cursor", cursor);
            command.Parameters.AddWithValue("@PageSize", pageSize);
            using SQLiteDataReader reader = command.ExecuteReader();
            while (reader.Read())
            {
                items.Add(new OpusScanItem
                {
                    RowId = Convert.ToInt64(reader["RowId"]),
                    ItemId = Convert.ToString(reader["ItemId"]),
                    UniqueId = Convert.ToString(reader["UniqueId"]),
                    SiteUrl = Convert.ToString(reader["SiteUrl"]),
                    WebUrl = Convert.ToString(reader["WebUrl"]),
                    ListUrl = Convert.ToString(reader["ListUrl"]),
                    ParentUrl = Convert.ToString(reader["ParentUrl"]),
                    FileUrl = Convert.ToString(reader["FileUrl"]),
                    FileName = Convert.ToString(reader["FileName"]),
                    Size = Convert.ToInt64(reader["Size"]),
                    Extension = Convert.ToString(reader["Extension"]),
                });
            }

            return items;
        }

        private IReadOnlyList<OpusScanContainer> ReadRequiredContainers(IReadOnlyList<OpusScanItem> items)
        {
            Dictionary<string, OpusScanContainer> containers = new Dictionary<string, OpusScanContainer>(StringComparer.OrdinalIgnoreCase);
            foreach (OpusScanItem item in items)
            {
                AddContainerAndAncestors(item.ListUrl, containers);
                AddContainerAndAncestors(item.ParentUrl, containers);
            }

            return new List<OpusScanContainer>(containers.Values);
        }

        private void AddContainerAndAncestors(string containerUrl, IDictionary<string, OpusScanContainer> containers)
        {
            string currentUrl = containerUrl;
            while (!string.IsNullOrWhiteSpace(currentUrl) && !containers.ContainsKey(currentUrl))
            {
                OpusScanContainer container = ReadContainer(currentUrl);
                if (container == null)
                {
                    return;
                }

                containers.Add(container.ContainerUrl, container);
                currentUrl = container.ParentUrl;
            }
        }

        private OpusScanContainer ReadContainer(string containerUrl)
        {
            using SQLiteCommand command = _connection.CreateCommand();
            command.CommandText = @"
                SELECT ContainerUrl, ParentUrl, SiteUrl, WebUrl, ContainerType, Name, DisplayName, FullPathForUI
                FROM ScanContainers
                WHERE ContainerUrl = @ContainerUrl;";
            command.Parameters.AddWithValue("@ContainerUrl", containerUrl);
            using SQLiteDataReader reader = command.ExecuteReader();
            if (!reader.Read())
            {
                return null;
            }

            return new OpusScanContainer
            {
                ContainerUrl = Convert.ToString(reader["ContainerUrl"]),
                ParentUrl = reader["ParentUrl"] == DBNull.Value ? null : Convert.ToString(reader["ParentUrl"]),
                SiteUrl = Convert.ToString(reader["SiteUrl"]),
                WebUrl = Convert.ToString(reader["WebUrl"]),
                ContainerType = (OpusScanContainerType)Convert.ToInt32(reader["ContainerType"]),
                Name = Convert.ToString(reader["Name"]),
                DisplayName = Convert.ToString(reader["DisplayName"]),
                FullPathForUI = reader["FullPathForUI"] == DBNull.Value ? null : Convert.ToString(reader["FullPathForUI"]),
            };
        }
    }
}
