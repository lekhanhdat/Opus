import PropTypes from "prop-types";
import "../../../../Less/BCM/ContentRepositoryManagement/manualApprovalSetting.less";
import CRMCommonUtil from "../Common/CRMCommonUtil";
import { LicenseHelper, showToast } from "../../../../Utilities/CommonUtil";

export const SelectProcessType = {
    SelectNoneApprovalType: 0,
    SelectApprovalProcess: 1,
    SelectOwnerRecords: 2,
    SelectAutoApprove: 3,
};

export default class ManualApprovalSettingPanel extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            items: props.items || [],
            workflowList: [],
            userList: [],
            enableApprovalItem: parseInt(this.props.data.ApprovalType.toString(), 10),
            mailToOwner: this.props.data.EMailToRecordOwner,
            workflowReferenceId: this.props.data.WorkflowReferenceId,
            isSaving: false,
        };
        this.addUserChanged = [];
    }

    componentInit() {
        this.initWorkflowCombobox();
        this.initPeopleCombobox();
    }

    initPeopleCombobox() {
        let users = this.props.data.RecordOwner;
        if (users) {
            let newUsers = CRMCommonUtil.convertUsersToRichCombobox(users);
            this.addUserChanged = newUsers;
            this.setState({
                userList: newUsers,
            });
        }
    }

    // convertUsersToRichCombobox(users) {
    //     let newUsers = [];
    //     users.forEach(user => {
    //         newUsers.push({
    //             name: user.DisplayName,
    //             // sub: user.DisplayName,
    //             value: user.UserId,
    //             disabled: false,
    //             tooltip: user.UserPrincipalName,
    //             readonly: false,
    //             invalid: false,
    //             conflict: false,
    //             data: user,
    //         });
    //     });
    //     return newUsers;
    // }

    initWorkflowCombobox() {
        let option = {
            url: '/api/RuleApi/GetAllWorkflows',
            method: "GET"
        };
        fetchUtility(option).then((res) => {
            let data = JSON.parse(res);
            data.forEach(item => {
                if (item.ReferenceId == this.props.data.WorkflowReferenceId) {
                    item.Checked = true;
                } else {
                    item.Checked = false;
                }
            });

            this.setState({
                workflowList: data
            });
        }).catch((e) => {
        });
    }

    componentReceive(type, args) {
        switch (type) {
            case "onSave":
                this.onSave(args);
                break;
        }
    }

    onSave(callback, e) {
        if (!$$.verify(this.allValidation)) {
            return false;
        }
        $$.loading(true);
        let approvalSettingNode = this.props.data;
        if (this.state.enableApprovalItem == SelectProcessType.SelectApprovalProcess) {
            if (this.state.workflowReferenceId == null) {
                return false;
            } else {
                approvalSettingNode.ApprovalType = this.state.enableApprovalItem;
                approvalSettingNode.WorkflowReferenceId = this.state.workflowReferenceId;
                approvalSettingNode.EMailToRecordOwner = this.state.mailToOwner;
                approvalSettingNode.RecordOwner = [];
            }
        } else if (this.state.enableApprovalItem == SelectProcessType.SelectOwnerRecords) {
            let newRoList = [];
            this.addUserChanged.forEach(user => {
                newRoList.push(user.data);
            });
            if (newRoList.length == 0) {
                return false;
            } else {
                approvalSettingNode.RecordOwner = newRoList;
                approvalSettingNode.EMailToRecordOwner = this.state.mailToOwner;
                approvalSettingNode.ApprovalType = this.state.enableApprovalItem;
                approvalSettingNode.WorkflowReferenceId = null;
            }
        } else if (this.state.enableApprovalItem == SelectProcessType.SelectAutoApprove) {
            approvalSettingNode.ApprovalType = this.state.enableApprovalItem;
            approvalSettingNode.WorkflowReferenceId = null;
            approvalSettingNode.RecordOwner = [];
            approvalSettingNode.EMailToRecordOwner = false;
        } else {
            approvalSettingNode.RecordOwner = [];
            approvalSettingNode.EMailToRecordOwner = false;
            approvalSettingNode.ApprovalType = 0;
            approvalSettingNode.WorkflowReferenceId = null;
        }
        let option = {
            url: this.props.context.saveManualApprovalUrl,
            method: "Post",
            data: approvalSettingNode
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                const res = typeof result === "string" ? JSON.parse(result) : result;

                // Success case
                if(res.MessageType === 0 || res.isSuccessful){
                    callback(true);
                } else {
                    showToast.error(res.ErrorMessage);
                }
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onApprovalSwitchChange = (args) => {
        let approvalType = parseInt(this.props.data.ApprovalType.toString(), 10);
        if (args) {
            if (approvalType == SelectProcessType.SelectNoneApprovalType) {
                this.setState({ enableApprovalItem: SelectProcessType.SelectApprovalProcess });
            } else {
                this.setState({ enableApprovalItem: approvalType });
            }
        } else {
            this.setState({ enableApprovalItem: SelectProcessType.SelectNoneApprovalType });
        }
    }

    onAddUserSelectionChanged = (args) => {
        let selections = RM.deepcopy(args.newValue);
        this.addUserChanged = selections;
    }

    onApprovalChanged = (args) => {
        this.setState({ enableApprovalItem: args });
    }

    onAddUserApprovalChanged = (args) => {
        this.setState({ addUser: args });
    }

    onEMailToRecordOwnerChanged = (args) => {
        this.setState({ mailToOwner: args });
    }

    onApprovalSelectionChanged = (args) => {
        let selectedWorkflowId = args.newValue.ReferenceId;
        this.setState({ workflowReferenceId: selectedWorkflowId });
    }

    onSearch = (args) => {
        let searchValue = args.key;
        let urlData = `/api/BCMCommonSettingApi/SearchAADUsers?tenantId=&key=${searchValue}`;
        let option = {
            url: urlData,
            method: "get"
        };
        if (searchValue) {
            return fetchUtility(option).then((res) => {
                let users = RM.deepcopy(res.Users);
                return CRMCommonUtil.convertUsersToRichCombobox(users);
            }).catch((e) => {

            });
        }
    }

    render() {
        return <div id={this.props.id}>
            <R.Validation>
                <div ref={r => this.allValidation = r}>
                    <div className="ra-crm-form-content ra-setting-panel-containerEnable">
                        <span className="ra-containerEnable-span" tabIndex="0">
                            {RMResx.RM_BCM_ManualApproval_Title_EnableApproval}
                            <$g.Popover>{RMResx.RM_BCM_ManualApproval_Title_EnableApprovalTips}</$g.Popover>
                        </span>
                        <span className="ra-setting-panel-containerSwitch">
                            <R.Switch
                                checked={this.state.enableApprovalItem != SelectProcessType.SelectNoneApprovalType}
                                onChange={this.onApprovalSwitchChange}
                            />
                        </span>
                    </div>
                    {this.state.enableApprovalItem != SelectProcessType.SelectNoneApprovalType && <div>
                        <div className="ra-crm-form-content">
                            <div className="ra-setting-panel-radio">
                                <R.Radio
                                    name="radioManualApproval"
                                    text={RMResx.RM_BCM_ManualApproval_Title_SelectProcess}
                                    value={SelectProcessType.SelectApprovalProcess}
                                    checked={this.state.enableApprovalItem == SelectProcessType.SelectApprovalProcess}
                                    onChange={this.onApprovalChanged} />
                                {this.state.enableApprovalItem == SelectProcessType.SelectApprovalProcess && <div>
                                    <div className="ra-setting-panel-radio">
                                        <R.Validation
                                            element="Combobox"
                                            require={RMResx.RM_RDM_CreateRule_Msg_NoSelectProcess}
                                        >
                                            <R.Combobox
                                                id="raCrmMaProcess"
                                                items={this.state.workflowList}
                                                tooltipField="Name"
                                                width='100%'
                                                textField="Name"
                                                valueField="ReferenceId"
                                                checkedField="Checked"
                                                linkMode={false}
                                                searchable
                                                onChange={this.onApprovalSelectionChanged} />
                                        </R.Validation>
                                    </div>
                                </div>}
                            </div>
                            <div className="ra-setting-panel-radio">
                                <R.Radio
                                    name="radioManualApproval"
                                    text={RMResx.RM_BCM_ManualApproval_Title_AddUser}
                                    value={SelectProcessType.SelectOwnerRecords}
                                    checked={this.state.enableApprovalItem == SelectProcessType.SelectOwnerRecords}
                                    onChange={this.onApprovalChanged}
                                />
                                {this.state.enableApprovalItem == SelectProcessType.SelectOwnerRecords && <div>
                                    <div className="ra-setting-panel-radio">
                                        <R.Validation
                                            element="RichCombobox"
                                            require={RMResx.RM_JS_CP_AM_Owner_Require} >
                                            <R.RichCombobox
                                                asyncSearch
                                                id={"raCrmUserManualApproval"}
                                                height={80}
                                                value={this.state.userList}
                                                searchPlaceholder={RMResx.RM_Common_PeoplePicker_Watermark}
                                                disabled={false}
                                                textField="name"
                                                valueField="value"
                                                template="profile"
                                                itemTemplate="profile"
                                                checkedField="checked"
                                                tooltipField="tooltip"
                                                disabledField="disabled"
                                                readonlyField="readonly"
                                                invalidField="invalid"
                                                groupField={null}
                                                matchFields={{ 'name': false }}
                                                searchable={true}
                                                singleMode={false}
                                                silence={false}
                                                excludeChecked={true}
                                                doLoad={this.onSearch}
                                                onChange={this.onAddUserSelectionChanged}
                                            />
                                        </R.Validation>
                                    </div>
                                </div>}
                            </div>
                            {LicenseHelper.EnableRecordsArchiver() && this.props.context.showAutoApproveOption && <div className="ra-setting-panel-radio">
                                <R.Radio
                                    name="radioManualApproval"
                                    text={RMResx.RM_BCM_ManualApproval_Detail_AutoApprove}
                                    value={SelectProcessType.SelectAutoApprove}
                                    checked={this.state.enableApprovalItem == SelectProcessType.SelectAutoApprove}
                                    onChange={this.onApprovalChanged} />
                            </div>}
                        </div>
                        {this.state.enableApprovalItem != SelectProcessType.SelectAutoApprove && <div className="ra-setting-panel-checkbox">
                            <R.Checkbox
                                id="raCrmMaSendMailToOwner"
                                text={RMResx.RM_SPS_MASendMailToOwner}
                                title={RMResx.RM_SPS_MASendMailToOwner}
                                checked={this.state.mailToOwner}
                                onChange={this.onEMailToRecordOwnerChanged}
                            />
                        </div>}
                    </div>}
                </div>
            </R.Validation>
        </div>;
    }
}

ManualApprovalSettingPanel.propTypes = {
    selectionChanged: PropTypes.func,
};
