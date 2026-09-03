namespace AvePoint.Item.Restore.ScanStaging
{
    internal enum OpusScanContainerType
    {
        SiteCollection = 0,
        Site = 1,
        List = 2,
        Folder = 3,
    }

    internal sealed class OpusScanContainer
    {
        internal string ContainerUrl { get; set; }
        internal string ParentUrl { get; set; }
        internal string SiteUrl { get; set; }
        internal string WebUrl { get; set; }
        internal OpusScanContainerType ContainerType { get; set; }
        internal string Name { get; set; }
        internal string DisplayName { get; set; }
        internal string FullPathForUI { get; set; }
    }

    internal sealed class OpusScanItem
    {
        internal long RowId { get; set; }
        internal string ItemId { get; set; }
        internal string UniqueId { get; set; }
        internal string SiteUrl { get; set; }
        internal string WebUrl { get; set; }
        internal string ListUrl { get; set; }
        internal string ParentUrl { get; set; }
        internal string FileUrl { get; set; }
        internal string FileName { get; set; }
        internal string Extension { get; set; }
        internal long Size { get; set; }
    }
}
