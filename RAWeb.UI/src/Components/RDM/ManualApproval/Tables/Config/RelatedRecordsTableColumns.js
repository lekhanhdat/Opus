import { Columns, OrderOptions, ColumnIds} from "../../Constants/index";

const RelatedRecordsTableColumns = [
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
        id: ColumnIds.DisposalClass,
        header: Columns.DisposalClass,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.RelatedRecords,
        header: Columns.RelatedRecords,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.DisposalAction,
        header: Columns.DisposalAction,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.RecordReviewer,
        header: Columns.RecordReviewer,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.ModifiedTime,
        header: Columns.ModifiedTime,
        width: [150],
        resizeable: true
    },
];

export default RelatedRecordsTableColumns;