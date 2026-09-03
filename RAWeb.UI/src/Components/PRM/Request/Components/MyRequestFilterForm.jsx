import { getMulticomboboxAllItems } from "../../../../Utilities/CommonUtil";

const ColumnHeadNames = {
    RequestID: RMResx.RM_PRM_MyRequest_RequestId,
    ItemName: RMResx.RM_PRM_MyRequest_ItemName,
    UniqueId: RMResx.RM_PRM_RequestManagement_UniqueId,
    RequestType: RMResx.RM_PRM_MyRequest_RequestType,
    Status: RMResx.RM_PRM_RequestManagement_Status,
    CreatedTime:RMResx.RM_PRM_PRE_Column_CreatedTime,
    RequestBy: RMResx.RM_PRM_MyRequest_RequestBy,
};
export class MyRequestFilterForm extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        this.state = {
            timeRange:props.timeRange,
            requestTypeItems: this.props.filterOptionsInfo.Type || RM.deepcopy(this.props.requestTypeItems),
            requestByItems: this.props.filterOptionsInfo.RequestBy || RM.deepcopy(this.props.requestByItems),
            approvalStatusItems: this.props.filterOptionsInfo.Status || RM.deepcopy(this.props.approvalStatusItems),
        
        };
        this.isAdmin = this.props.isAdmin;
        this.filterOptionsInfo = RM.deepcopy(this.props.filterOptionsInfo);
        this.bind([ "onSelectTime"]);
    }

    componentReceive(callback) {
        callback(this.filterOptionsInfo,this.state.timeRange);
    }

    onChangerequestType = (args) => {
        this.filterOptionsInfo.Type = getMulticomboboxAllItems(args.newValue, this.state.requestTypeItems, "id", "checked");
    }

    onChangerequestBy = (args) => {
        this.filterOptionsInfo.RequestBy = getMulticomboboxAllItems(args.newValue, this.state.requestByItems, "id", "checked");
    }

    onChangeapprovalStatus = (args) =>{
        this.filterOptionsInfo.Status = getMulticomboboxAllItems(args.newValue, this.state.approvalStatusItems, "id", "checked");
    }

    onClear = () => {
        this.filterOptionsInfo.Type = this.props.requestTypeItems;
        this.filterOptionsInfo.RequestBy = this.props.requestByItems;
        this.filterOptionsInfo.Status = this.props.approvalStatusItems;
        this.setState({
            timeRange:{
                start: null,
                end: null,
            },
            requestTypeItems: RM.deepcopy(this.props.requestTypeItems),
            requestByItems: RM.deepcopy((this.props.requestByItems)),
            approvalStatusItems:RM.deepcopy((this.props.approvalStatusItems))
        });
    }

    getDefaultStartTime() { //默认时间为一个月前到今天
        let startTime = new Date("2000/1/1");
        return startTime;
    }

    onSelectTime(args) {
        let timeRange = {
            start: args.newValue.start,
            end: args.newValue.end
        };
        this.setState({timeRange: timeRange});
    }

    renderDateRangDatePicker() {
        return (
            <R.Rangepicker
                selectedDate={this.state.timeRange}
                data-part="vtWidget"
                width={"100%"}
                dateTimeFormat={RM.TimeSettingModel.DateFormat}
                onChange={this.onSelectTime}
            />
        );
    }

    render() {
        return <div id={this.props.id}>
            <div className="ra-flex-justify-end">
                <a className="ra-main-filter-clear fia-funnel-clear" 
                    tabIndex={0} 
                    onClick={this.onClear} 
                    aria-label={RMResx.RM_Common_ClearFilter}
                > {RMResx.RM_Common_ClearFilter}</a>
            </div>
            <$g.FormRow label={RMResx.RM_MA_TimeRange.replace(":", "")}>
                {this.renderDateRangDatePicker()}
            </$g.FormRow>
            <$g.FormRow label={ColumnHeadNames.RequestType}>
                <R.Multicombobox
                    width={"100%"}
                    checkedField="checked"
                    textField="name"
                    valueField="id"
                    searchable={false}
                    clearable={true}
                    required={true}
                    items={this.state.requestTypeItems}
                    noneText={RMResx.RM_PRM_MyRequest_RequestType}
                    onChange={this.onChangerequestType}
                />
            </$g.FormRow>
            {this.isAdmin && <div> 
                <$g.FormRow label={ColumnHeadNames.RequestBy}>
                    <R.Multicombobox
                        width={"100%"}
                        checkedField="checked"
                        textField="name"
                        valueField="id"
                        required={true}
                        clearable={true}
                        tooltipField="tooltip"
                        items={this.state.requestByItems}
                        noneText={RMResx.RM_PRM_MyRequest_RequestBy}
                        onChange={this.onChangerequestBy}
                    />
                </$g.FormRow>
            </div>}
            <$g.FormRow label={ColumnHeadNames.Status}>
                <R.Multicombobox
                    width={"100%"}
                    checkedField="checked"
                    textField="name"
                    valueField="id"
                    searchable={false}
                    clearable={true}
                    required={true}
                    items={this.state.approvalStatusItems}
                    noneText={RMResx.RM_PRM_RequestManagement_Status}
                    onChange={this.onChangeapprovalStatus}
                />
            </$g.FormRow>
        </div>;
    }
}