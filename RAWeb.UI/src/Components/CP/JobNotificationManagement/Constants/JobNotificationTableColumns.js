import {ColumnIds, ColumnNames} from "./index";

const JobNotificationTableColumns = [
    {
        id: ColumnIds.Name,
        header: ColumnNames.Name,
        width: [150],
        resizeable: true,
        disabled: true
    },
    {
        id: ColumnIds.Description,
        header: ColumnNames.Description,
        width: [150],
        resizeable: true,
        disabled: true
    },
    {
        id: ColumnIds.EmailReceiver,
        header: ColumnNames.EmailReceiver,
        width: [150],
        resizeable: true,
        disabled: true
    },
    {
        id: ColumnIds.Interval,
        header: ColumnNames.Interval,
        width: [150],
        resizeable: true,
        disabled: true
    },
    {
        id: ColumnIds.CreateTime,
        header: ColumnNames.CreateTime,
        width: [150],
        resizeable: true,
        disabled: true
    },
];

export default JobNotificationTableColumns;