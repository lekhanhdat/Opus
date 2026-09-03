
//import RouterUrls from "../../../../Constants/RouterUrls";
export default class ConnectionGroupSettings extends R.Component {
    idAttr = true;

    componentCreate() {
        this.getGroupUrl = '/api/ConnectionRegisterApi/GetGroupById';
        this.getConnByGroupUrl = '/api/ConnectionRegisterApi/GetConnectionByGroupId';
        this.state = {
            isSaving:false,
            showTip: false,
            showMessageTip: this.showMessageTip,
            haveChange: false,
            groupName: '',
            groupDesc: '',
            groupNameValidate: false,
        };
    }

    componentInit() {

    }

    componentReceive(action, ...args) {
        switch (action) {
            case "onEditInit":
                this.showEditPanelInit(args[0], args[1]);
                break;
            case "onSave":
                this.saveGroup(args[0]);
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

    saveGroup(callback){
        let validateFailed = false;
        validateFailed = this.state.groupName == '';
        this.setState({
            isSaving: true,
            groupNameValidate:validateFailed
        });
        if (validateFailed) {
            return;
        }

        if(!callback({ Id: this.groupId, Name: this.state.groupName, Description: this.state.groupDesc })){
            this.showMessageTip("error", RMResx.RM_FS_Register_SameGroupNameErrorMessage);
        }
    }

    showEditPanelInit(callback, group) {
        this.groupId = group.Id;
        this.setState({ groupName: group.Name, groupDesc: group.Description });
    }

    onChangeGroupName = (value) => {
        this.setState({
            groupName: $.trim(value),
            haveChange: true,
        });
    }

    onChangeGroupDesc = (value) => {
        this.setState({
            groupDesc: $.trim(value),
            haveChange: true
        });
    }
    render() {
        return <div id={this.props.id}>
            <R.Messagebar
                message={this.state.tipMsg} classify={this.state.tipType}
                onClose={this.hideMessageTip} status={{ show: this.state.showTip }} />
            <div className="panel-description-form">
                <div className="ra-form-label" >
                    <div className='input-label require' tabIndex='0'>{RMResx.RM_FS_Register_GroupName}</div>
                </div>
                <R.Input
                    name='iptConnGroupName'
                    type='text'
                    width={500}
                    value={this.state.groupName}
                    onChange={this.onChangeGroupName}
                    aria={{ariaLabel:RMResx.RM_FS_Register_GroupName}}
                />
                <$g.ValidationMsg show={this.state.groupNameValidate}>
                    {RMResx.RM_FS_Register_NameInputValidateMessage}
                </$g.ValidationMsg>


                <div className="ra-form-label" >
                    <div className='input-label' tabIndex='0'>{RMResx.RM_FS_Register_Description}</div>
                </div>
                <div className="ra-form-content">
                    <R.Input
                        name='iptConnGroupDesc'
                        type='textarea'
                        width={500}
                        height={100}
                        value={this.state.groupDesc}
                        onChange={this.onChangeGroupDesc}
                        aria={{ariaLabel:RMResx.RM_FS_Register_Description}}
                    />
                </div>
            </div>
        </div>;
    }
}