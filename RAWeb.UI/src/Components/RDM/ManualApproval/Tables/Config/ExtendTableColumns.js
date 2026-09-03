import { Columns, OrderOptions, ColumnIds} from "../../Constants/index";

const ExtendTableColumns = [
    {
        id: ColumnIds.RecordName,
        header: Columns.RecordName,
        width: [150],
        resizeable: true,
        sortable: true,
        orderOption: OrderOptions.LeafName,
        disabled: true
    },
    {
        id: ColumnIds.RecordsId,
        header: Columns.RecordsId,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.FullPath,
        header : Columns.FullPath,
        width: [200],
        resizeable : true
    },
    {
        id: ColumnIds.FolderPath,
        header : Columns.FolderPath,
        width: [150],
        resizeable : true,
    },
    {
        id: ColumnIds.Type,
        header: Columns.Type,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.Rule,
        header: Columns.Rule,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.LastReasonForRejection,
        header: Columns.LastReasonForRejection,
        width: [280],
        resizeable: true,
        sortable: true,
        orderOption: OrderOptions.QuickReason,
        disabled: false
    },
    {
        id: ColumnIds.LastApproveRejectComment,
        header: Columns.LastApproveRejectComment,
        width: [280],
        resizeable: true,
    },
    {
        id: ColumnIds.DisposalClass,
        header: Columns.DisposalClass,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.ExtendTime,
        header: Columns.ExtendTime,
        width: [200],
        resizeable: true
    },
    {
        id: ColumnIds.RecordReviewer,
        header: Columns.RecordReviewer,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.LastReviewedBy,
        header: Columns.LastReviewedBy,
        width: [200],
        resizeable: true,
    },
    {
        id: ColumnIds.LastReviewTime,
        header: Columns.LastReviewTime,
        width: [200],
        resizeable: true,
    },
    {
        id: ColumnIds.ModifiedTime,
        header: Columns.ModifiedTime,
        width: [150],
        resizeable: true
    },
];

export default ExtendTableColumns;