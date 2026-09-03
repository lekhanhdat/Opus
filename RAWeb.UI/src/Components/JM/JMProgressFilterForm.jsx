import { getMulticomboboxAllItems } from "../../Utilities/CommonUtil";

const statusColumns = [
    { Id: 0, value: RMResx.RM_JS_JM_Status_Wait, isChecked: true },
    { Id: 1, value: RMResx.RM_JS_JM_Progress_Status_Scan, isChecked: true },
    { Id: 2, value: RMResx.RM_JS_JM_Progress_Status_Export, isChecked: true },
    { Id: 3, value: RMResx.RM_JS_JM_Progress_Status_Archive, isChecked: true },
    { Id: 4, value: RMResx.RM_JS_JM_Progress_Status_Others, isChecked: true },
    { Id: 5, value: RMResx.RM_JS_JM_Status_Finished, isChecked: true },
    { Id: 6, value: RMResx.RM_JS_JM_Status_Failed, isChecked: true },
    { Id: 7, value: RMResx.RM_JS_JM_Status_FinishWithException, isChecked: true },
    { Id: 8, value: RMResx.RM_JS_JM_Status_Stopped, isChecked: true },
    { Id: 9, value: RMResx.RM_JS_JM_Status_Skipped, isChecked: true },
]

const getColumnsByStatusFilter = (statusFilter) => {
    const hasStatusFilter = Array.isArray(statusFilter) && statusFilter.length > 0;
    return statusColumns.map(item => ({
        ...item,
        isChecked: hasStatusFilter ? statusFilter.includes(item.Id) : true,
    }));
}

export class JMProgressFilterForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        const initialColumns = getColumnsByStatusFilter(props.statusFilter);
        this.state = {
            columns: initialColumns,
            filterData: initialColumns,
        }
    }

    componentUpdate(prevProps) {
        if (prevProps.statusFilter !== this.props.statusFilter) {
            const nextColumns = getColumnsByStatusFilter(this.props.statusFilter);
            this.setState({
                columns: nextColumns,
                filterData: nextColumns,
            });
        }
    }

    componentReceive(type, callback) {
        if (type === "onFilter") {
            callback(this.state.filterData.filter(item => item.isChecked));
        }
    }

    statusFilterChanged = (args) => {
        this.setState({ filterData: getMulticomboboxAllItems(args.newValue, this.state.columns, "Id", "isChecked") });
    }

    onClear = () => {
        this.setState({ filterData: [], columns: statusColumns.map(item => ({ ...item, isChecked: true })) });
    }

    onKeyDown(e) {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    render() {
        return (
            <div id={this.props.id}>
                <div className="ra-flex-justify-end">
                    <a className="ra-main-filter-clear fia-funnel-clear" onClick={this.onClear} tabIndex="0" onKeyDown={this.onKeyDown}> {RMResx.RM_Common_ClearFilter}</a>
                </div>
                <$g.FormRow label={RMResx.RM_JS_JMD_Progress_Status}>
                    <R.Multicombobox
                        checkedField="isChecked"
                        textField="value"
                        valueField="Id"
                        width={"100%"}
                        hasSelectAll={true}
                        searchable={false}
                        items={this.state.columns}
                        noneText={RMResx.RM_JS_JMD_Status_Filter}
                        onChange={this.statusFilterChanged}
                    />
                </$g.FormRow>
            </div>
        )
    }
}