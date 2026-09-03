import { LicenseHelper } from "../../../../Utilities/CommonUtil";
import PeoplePicker from "../../../Common/PeoplePicker";

const GuidEmpty = "00000000-0000-0000-0000-000000000000";
export default class ConnectionSettings extends R.Component {
    idAttr = true;
    componentCreate() {
        const browserLanguage = (navigator.language || '').toLowerCase();
        this.getGroupUrl = '/api/ConnectionRegisterApi/GetGroupById';
        this.getConnByGroupUrl = '/api/ConnectionRegisterApi/GetConnectionByGroupId';
        this.getConnectionUrl = '/api/ConnectionRegisterApi/GetConnectionById';
        this.validationUrl = '/api/ConnectionRegisterApi/ValidationConnection';
        this.isUNCPathPattern = /\\\\[\w.-]+\\[^\\?<>*:|/]+/;
        this.connectionTableID = 'ra-conn-table';
        this.tableColumns = this.getColums();
        this.connectionId = GuidEmpty;
        this.connectionListLoaded = false;
        this.needValidateTestConn = true;
        this.isJPMCConnectionData = false;
        this.pathValidationMessageStyle = browserLanguage.startsWith('ko') ? { fontFamily: 'Consolas, "Courier New", monospace' } : undefined;
        this.state = {
            isSaving: false,
            showTip: false,
            showMessageTip: this.showMessageTip,
            haveChange: false,
            connectionName: '',
            connectionDesc: '',
            uncPath: '',
            agentSelected: GuidEmpty,
            addToGroupSelected: GuidEmpty,
            agentOptions: [],
            groupOptions: [],
            validateTestSuccess: -1,
            validateTestFailed: '',
            validateGroupFailed: false,
            pathValidateGroupFailed: false,
            pathValidateFailed: false,
            informationValidateFailed: false,
            recordsValidateFailed: false,
            showErrorMessage: '',
            infoOwner: [],
            recordsOwner: [],
            JPMCConnectionId: '',
            JPMCIdValidateFailed: false,
            clonnedConnection: null,
            isExceedJPMCIdLength: false,
        };
        this.isEnableJPMC = LicenseHelper.EnableJPMCFileSystemFeature();
    }

    componentInit() {

    }

    componentReceive(action, ...args) {
        switch (action) {
            case "onSaveInit":
                this.showSavePanelInit(args[0], args[1], args[2], args[3]);
                this.setState({
                    clonnedConnection: args[3] ?? null,
                });
                break;
            case "onSave":
                this.saveConnection(args[0]);
                break;
        }
    }

    showMessageTip = (type, msg) => {
        let tipOption = {
            showTip: true,
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    hideMessageTip = () => {
        this.setState({
            showTip: false
        });
    }

    saveConnection(callback) {
        const nameValidateFailed = this.state.connectionName == '';
        const pathValidateFailed = this.state.uncPath == '' || !this.isUNCPathPattern.test(this.state.uncPath);
        const informationValidateFailed = this.state.infoOwner.length === 0;
        const recordsValidateFailed = false; //update this field to optional for JPMC connection
        const JPMCIdValidateFailed = this.state.clonnedConnection ? false : this.state.JPMCConnectionId == "";
        const hasSupportJPMC = this.isEnableJPMC && (this.isJPMCConnectionData || this.connectionId === GuidEmpty);
        const isExceedJPMCIdLength = this.state.JPMCConnectionId.length > 255;
        let JPMCValidationFailed = false;

        if (hasSupportJPMC) {
            JPMCValidationFailed = informationValidateFailed || recordsValidateFailed || JPMCIdValidateFailed || isExceedJPMCIdLength;
        }
        // let groupValidateFailed = this.state.addToGroupSelected === GuidEmpty;
        // let agentValidateFailed = this.state.agentSelected == GuidEmpty;
        this.setState({
            isSaving: true,
            nameValidateFailed: nameValidateFailed,
            pathValidateFailed: pathValidateFailed,
            informationValidateFailed,
            recordsValidateFailed,
            validateTestFailed: false,
            JPMCIdValidateFailed,
            isExceedJPMCIdLength,
            // validateGroupFailed: groupValidateFailed
            // agentValidateFailed: agentValidateFailed,
        });
        if (nameValidateFailed || pathValidateFailed || JPMCValidationFailed) {
            return;
        }
        let saveConnectionFunc = () => {
            let showMessageFunc = (message) => {
                this.showMessageTip("error", message);
            };
            let payload = {
                Id: this.connectionId,
                GroupId: this.state.addToGroupSelected,
                Name: this.state.connectionName,
                Description: this.state.connectionDesc,
                UNCPath: this.state.uncPath,
                // AgentId: this.state.agentSelected,
            };
            if(hasSupportJPMC) {
                payload.InformationOwners = this.state.infoOwner;
                payload.RecordOwners = this.state.recordsOwner;
                payload.JPMCConnectionId = this.state.JPMCConnectionId;
                payload.IsEditConnectionPage = !this.props.isEditPermission;
            }
            
            callback(payload, showMessageFunc);
        };

        saveConnectionFunc();
        // if (this.needValidateTestConn) {
        //     this.validateConnection(saveConnectionFunc);
        // }
        // else {
        // }
    }

    showSavePanelInit(callback, agentOptions, groupOptions, connection) {
        //get group by id from server;
        //GetGroupById
        if (connection) {
            this.isJPMCConnectionData = connection?.InformationOwners && connection?.RecordOwners;
            this.connectionId = connection.Id;
            // let tempAgentOptions = agentOptions.map(op => {
            //     let nop = {};
            //     nop.title = op.text;
            //     nop.text = op.text;
            //     nop.value = op.value;
            //     nop.checked = (connection.AgentId && connection.AgentId.toLowerCase()) == op.value.toLowerCase();
            //     return nop;
            // });

            let tempGroupOptions = groupOptions.map(op => {
                let nop = {};
                nop.title = op.Name;
                nop.text = op.Name;
                nop.value = op.Id;
                nop.checked = (connection.GroupId && connection.GroupId.toLowerCase()) == op.Id.toLowerCase();
                return nop;
            });
            this.setState({
                connectionName: connection.Name,
                connectionDesc: connection.Description,
                uncPath: connection.UNCPath,
                agentSelected: connection.AgentId,
                infoOwner: connection.InformationOwners || [],
                recordsOwner: connection.RecordOwners || [],
                JPMCConnectionId: connection.JPMCConnectionId || '',
                // agentOptions: tempAgentOptions,
                groupOptions: tempGroupOptions,
                addToGroupSelected: connection.GroupId
            });
        } else {
            // let tempAgentOptions = agentOptions.map(op => {
            //     let nop = {};
            //     nop.title = op.text;
            //     nop.text = op.text;
            //     nop.value = op.value;
            //     return nop;
            // });
            this.isJPMCConnectionData = false;
            let tempGroupOptions = groupOptions.map(op => {
                let nop = {};
                nop.title = op.Name;
                nop.text = op.Name;
                nop.value = op.Id;
                return nop;
            });
            this.setState({
                // agentOptions: tempAgentOptions,
                groupOptions: tempGroupOptions
            });
        }
    }
    onSelectInfoOwner = (args) => {
        this.setState({ infoOwner: args, informationValidateFailed: args.length === 0 });
    }

    onSelectRecordsOwner = (args) => {
        this.setState({ recordsOwner: args });
    }
    getColums() {
        return [
            {
                header: RMResx.RM_FS_Register_ConnectionName,
                width: 150,
                resizeable: true
            }, {
                header: this.isEnableJPMC ? RMResx.RM_FS_Register_Path : RMResx.RM_FS_Register_UNCPath,
                width: 260,
                resizeable: true
            }, {
                header: RMResx.RM_FS_Register_LastModifiedTime,
                resizeable: true,
                width: 260
            }];
    }

    onChangeConnectionName = (value) => {
        this.setState({
            connectionName: $.trim(value),
            haveChange: true,
        });
    }

    onChangeJPMCConnectionId = (value) => {
        this.setState({
            JPMCConnectionId: $.trim(value),
            haveChange: true,
        });
    }

    onChangeUNCPath = (value) => {
        this.setState({
            uncPath: $.trim(value),
            haveChange: true,
        });
        this.needValidateTestConn = true;
    }

    onChangeConnectionDesc = (value) => {
        this.setState({
            connectionDesc: $.trim(value),
            haveChange: true
        });
    }

    onShowNewConnectionPanel = () => {

    }

    // handleAgentChanged = (e, args) => {
    //     this.setState({
    //         agentSelected: args.newValue.value
    //     });
    // }

    handleAddToGroupChanged = (args) => {
        this.setState({
            addToGroupSelected: args.newValue.value,
            pathValidateGroupFailed: false,
            validateGroupFailed: false,
        });
    }

    onValidationAgentAndConnection = () => {
        this.validateConnection();
    }

    validateConnection(callback) {
        this.hideMessageTip();
        this.setState({
            validateTestSuccess: -1,
            validateTestFailed: '',
        });
        let pathValidateFailed = this.state.uncPath == '' || !this.isUNCPathPattern.test(this.state.uncPath);
        if (pathValidateFailed) {
            this.setState({
                pathValidateFailed: true,
            });
            return;
        }
        else {
            this.setState({
                pathValidateFailed: false,
            });
        }

        if (this.state.addToGroupSelected === GuidEmpty) {
            this.setState({
                pathValidateGroupFailed: true
            });
            return;
        }

        $$.loading(true);
        let option = {
            url: this.validationUrl,
            method: "POST",
            data: {
                Name: this.state.connectionName,
                UNCPath: this.state.uncPath,
                AgentId: this.state.agentSelected,
                GroupId: this.state.addToGroupSelected
            }
        };
        fetchUtility(option).then((res) => {
            let result = JSON.parse(res);
            if (result.MessageType == 0) {
                this.needValidateTestConn = false;
                if (callback) {
                    callback(result);
                } else {
                    this.setState({
                        validateTestSuccess: result.MessageType,
                    });
                }
            } else {
                this.setState({
                    validateTestFailed: result.MessageType,
                    showErrorMessage: result.ErrorMessage || RMResx.RM_FS_Register_FSConnectionValidationTestFailed,
                });
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    render() {
        return <div id={this.props.id}>
            <R.Messagebar
                message={this.state.tipMsg} classify={this.state.tipType}
                onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />

            <div className="panel-description-form">
                { !this.props.isEditPermission &&
                    <>
                        <div className="ra-form-label" >
                            <div className='input-label require' tabIndex='0'>{RMResx.RM_FS_Register_ConnectionName}</div>
                        </div>
                        <R.Input
                            id="raConnSettingGroupName"
                            name='iptConnectionName'
                            type='text'
                            width={500}
                            value={this.state.connectionName}
                            onChange={this.onChangeConnectionName}
                            aria={{ ariaLabel: RMResx.RM_FS_Register_ConnectionName }}
                        />

                        <$g.ValidationMsg show={this.state.nameValidateFailed}>
                            {RMResx.RM_FS_Register_NameInputValidateMessage}
                        </$g.ValidationMsg>

                        <div className="ra-form-label" >
                            <div className='input-label' tabIndex='0'>{RMResx.RM_FS_Register_Description}</div>
                        </div>
                        <div className="ra-form-content">
                            <R.Input
                                id="raConnSettingGroupDesc"
                                name='iptConnectionDesc'
                                type='textarea'
                                width={500}
                                height={100}
                                value={this.state.connectionDesc}
                                onChange={this.onChangeConnectionDesc}
                                aria={{ ariaLabel: RMResx.RM_FS_Register_Description }}
                            />
                        </div>
                        {this.isEnableJPMC && (this.isJPMCConnectionData || this.connectionId === GuidEmpty) && (
                            <>
                                <div className="ra-form-label" >
                                    <div className='input-label require' tabIndex='0'>{RMResx.RM_FS_Register_JPMCId}</div>
                                </div>
                                <R.Input
                                    id="raConnSettingGroupId"
                                    name='iptConnectionId'
                                    type='text'
                                    width={500}
                                    value={this.state.JPMCConnectionId}
                                    onChange={this.onChangeJPMCConnectionId}
                                    aria={{ ariaLabel: RMResx.RM_FS_Register_JPMCId }}
                                    disabled={this.connectionId !== GuidEmpty}
                                />

                                <$g.ValidationMsg show={this.state.JPMCIdValidateFailed}>
                                    {RMResx.RM_FS_Register_JPMCIdValidateMessage}
                                </$g.ValidationMsg>
                                <$g.ValidationMsg show={this.state.isExceedJPMCIdLength}>
                                    {RMResx.RM_JS_Common_Msg_CannotExceed255}
                                </$g.ValidationMsg>
                            </>
                        )}
                        <div className="ra-form-label" >
                            <div className='input-label require' tabIndex='0'>{this.isEnableJPMC ? RMResx.RM_FS_Register_Path : RMResx.RM_FS_Register_UNCPath}</div>
                        </div>
                        <div style={{ width: "500px" }}>
                            <R.Input
                                id="raConnSettingGroupUncPath"
                                name='iptUNCPath'
                                type='text'
                                width={"100%"}
                                value={this.state.uncPath}
                                onChange={this.onChangeUNCPath}
                                aria={{ ariaLabel: this.isEnableJPMC ? RMResx.RM_FS_Register_Path : RMResx.RM_FS_Register_UNCPath }}
                            />
                        </div>
                        <$g.ValidationMsg show={this.state.pathValidateFailed}>
                            <span style={this.pathValidationMessageStyle}>
                                {this.isEnableJPMC ? RMResx.RM_FS_Register_PathInputValidateMessage : RMResx.RM_FS_Register_UNCPathInputValidateMessage}
                            </span>
                        </$g.ValidationMsg>
                        <$g.ValidationMsg show={this.state.pathValidateGroupFailed}>
                            {RMResx.RM_FS_Register_UNCPathGroupValidateMessage}
                        </$g.ValidationMsg>
                    </>
                }

                {this.isEnableJPMC && (this.isJPMCConnectionData || this.connectionId === GuidEmpty) && (
                    <>
                        <div className="ra-form-label" >
                            <div className='input-label require' tabIndex='0'>{RMResx.RM_FS_Register_Information_Owner}</div>
                        </div>
                        <div style={{ width: "500px" }}>
                            <PeoplePicker
                                id="raConnSettingGroupInformation"
                                width={"100%"}
                                items={this.state.infoOwner}
                                selectionChanged={this.onSelectInfoOwner}
                                onlyIncludeAAdUser={true}
                            />
                        </div>
                        <$g.ValidationMsg show={this.state.informationValidateFailed}>
                            {RMResx.RM_FS_Register_InformationOwnerInputValidateMessage}
                        </$g.ValidationMsg>

                        <div className="ra-form-label" >
                            <div className='input-label' tabIndex='0'>{RMResx.RM_FS_Register_Records_Owner}</div>
                        </div>
                        <div style={{ width: "500px" }}>
                            <PeoplePicker
                                id="raConnSettingGroupRecords"
                                width={"100%"}
                                items={this.state.recordsOwner}
                                selectionChanged={this.onSelectRecordsOwner}
                                onlyIncludeAAdUser={true}
                            />
                        </div>
                        <$g.ValidationMsg show={this.state.recordsValidateFailed}>
                            {RMResx.RM_FS_Register_RecordsOwnerInputValidateMessage}
                        </$g.ValidationMsg>
                    </>
                )}
                <div className="ra-validation">
                    {this.state.validateTestSuccess == 0 && <span className="validate-test-success-span">{RMResx.RM_FS_Register_FSConnectionValidationTestSuccessfully}</span>}
                    {this.state.validateTestFailed == 1 && <span className="validate-test-failed-span">{this.state.showErrorMessage}</span>}
                </div>
            </div>            
        </div>
    
    }
}