import { getMulticomboboxAllItems } from "../../../Utilities/CommonUtil";
import { ActionTypeCol, AuditEventType } from "../Constants";

export default class ShowReportFilter extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);

        let actionTypeCols = RM.deepcopy(ActionTypeCol);
        for (let index = 0; index < actionTypeCols.length; index++) {
            const cols = actionTypeCols[index];
            if (this.props.selectedActionTypes == AuditEventType.All) {
                cols.checked = true;
            } else {
                if ((this.props.selectedActionTypes & cols.value) == cols.value) {
                    cols.checked = true;
                } else {
                    cols.checked = false;
                }
            }
        }
        
        this.state = {
            actionCol: actionTypeCols,
            userList: [],
            actionTypes: this.props.selectedActionTypes,
        };
        this.filterUsers = [];
    }

    componentInit() {
        this.loadFilterData();
    }

    loadFilterData() {
        let param = {
            ReportJobType: this.props.reportJobType,
            JobId: this.props.jobId,
        };
        let option = {
            url: "/API/ActionAuditReportApi/GetReportJobFilterData",
            method: "POST",
            data: param
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            for (let index = 0; index < data.Filters.User.length; index++) {
                const element = data.Filters.User[index];
                if (this.props.selectedUsers.length == 0 || this.props.selectedUsers.find(u => u == element.Name)) {
                    element.Checked = true;
                } else {
                    element.Checked = false;
                }
                element.Value = element.Name;
            }
            this.filterUsers = data.Filters.User;

            this.setState({
                userList: data.Filters.User
            });
        }).catch((e) => {
        });
    }

    getActionFilterData() {
        return {
            userList: this.filterUsers,
            actionTypes: this.state.actionTypes,
        };
    }

    setActionItemsClear() {
        let userItems = RM.deepcopy(this.state.userList);
        for (let index = 0; index < userItems.length; index++) {
            userItems[index].Checked = true;
        }
        this.filterUsers = [];
        this.setState({ userList: userItems, actionCol: RM.deepcopy(ActionTypeCol), actionTypes: AuditEventType.All });
    }

    onActionTypeChanged = (actionType) => {
        let action = this.state.actionTypes;
        let actionTypeResult = 0;
        if (actionType.isSelectAll) {
            action = AuditEventType.All;
        } else {
            let actionTypeLists = actionType.newValue.map(v => v.value);
            for (let index = 0; index < actionTypeLists.length; index++) {
                const element = actionTypeLists[index];
                actionTypeResult = actionTypeResult | element;
            }
            action = actionTypeResult;
        }
        this.setState({ actionTypes: action });
    }

    onUserChanged = (user) => {
        this.allUserValue = getMulticomboboxAllItems(user.newValue, this.state.userList, "Value", "Checked");
        if (user.isSelectAll) {
            this.filterUsers = [];
        } else {
            this.filterUsers = user.newValue;
        }
        this.setState({ userList: this.allUserValue });
    }

    render() {
        return <div>
            <$g.FormRow label={RMResx.RM_JS_RC_ActionAudit_ShowReportFilter_User}>
                <R.Multicombobox
                    id="raRcUserCbb"
                    width={"100%"}
                    items={this.state.userList}
                    disabled={false}
                    textField="Name"
                    valueField="Value"
                    checkedField="Checked"
                    tooltipField="tooltip"
                    disabledField="disabled"
                    required={true}
                    linkMode={false}
                    onChange={this.onUserChanged}
                />
            </$g.FormRow>
            <$g.FormRow label={RMResx.RM_JS_RC_ActionAudit_ShowReportFilter_Action}>
                <R.Multicombobox
                    id="raRcActionCbb"
                    width={"100%"}
                    items={this.state.actionCol}
                    disabled={false}
                    textField="name"
                    valueField="value"
                    checkedField="checked"
                    tooltipField="tooltip"
                    disabledField="disabled"
                    required={true}
                    linkMode={false}
                    onChange={this.onActionTypeChanged}
                />
            </$g.FormRow>
        </div>;
    }
}