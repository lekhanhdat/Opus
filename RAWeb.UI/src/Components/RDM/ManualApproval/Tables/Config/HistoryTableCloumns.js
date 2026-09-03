import { Columns, ColumnIds } from "../../Constants/index";

const HistoryTableCloumns = [
    {
        id: ColumnIds.RecordName,
        header: Columns.RecordName,
        width: [150],
        resizeable: true,
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
        id: ColumnIds.ApprovalStatus,
        header: Columns.ApprovalStatus,
        width: [150],
        resizeable: true,
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
        id: ColumnIds.ReassignedFrom,
        header: Columns.ReassignedFrom,
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
        id: ColumnIds.ApprovedBy,
        header: Columns.ApprovedBy,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.QuickReason,
        header: Columns.QuickReason,
        width: [230], 
        resizeable: true
    },
    {
        id: ColumnIds.ApprovalCommnent,
        header: Columns.ApprovalComment,
        width: [230], 
        resizeable: true
    },
    {
        id: ColumnIds.ReassignComment,
        header: Columns.ReassignComment,
        width: [200],
        resizeable: true
    },
    {
        id: ColumnIds.ModifiedBy,
        header: Columns.ModifiedBy,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.CreatedBy,
        header: Columns.CreatedBy,
        width: [150],
        resizeable: true
    },
    {
        id: ColumnIds.ActionTime,
        header: Columns.ActionTime,
        width: [150],
        resizeable: true,
    },
    {
        id: ColumnIds.ModifiedTime,
        header: Columns.ModifiedTime,
        width: [150],
        resizeable: true
    }
];

export default HistoryTableCloumns;