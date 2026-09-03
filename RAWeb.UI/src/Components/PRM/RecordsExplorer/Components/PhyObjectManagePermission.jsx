import PeoplePicker from "../../../Common/PeoplePicker";
import { NodeType } from "../../../../Constants/DAEnums";

export default class PhyObjectManagePermission extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            showTip: false,
            tipType: "success",
            tipMsg: "",
            phyObjItems: RM.deepcopy(this.props.data),
            searchedUser: [],
            userList: [],
            inheritanceInfo: [],
            isBreakInheritance: false,
            superiorIsBreakInheritance: false,
            isGlobalSearch: !!this.props.globalSearch,
            selConflictType: "1"
        };
        this.bind(["onSearchUser", "deleteUserToList", "inheritanceClick", "breakInheritance", "onConflictChange"]);
    }

    componentInit() {
        this.initFromGlobalSearch();
        this.setUsersToList();
        this.setInheritanceInfo();
    }

    componentReceive(type, callback) {
        switch (type) {
            case "onSave":
                this.saveSetting(callback);
                break;
        }
    }

    initFromGlobalSearch()
    {
        if(this.state.isGlobalSearch)
        {
            this.setState({ isBreakInheritance: true });
        }
    }

    saveSetting(callback) {
        //去重
        let permissionInfo = {};
        let userList = this.getRealPermissionUsers();
        permissionInfo.userList = userList.length == 0 ? null : userList;
        permissionInfo.IsInherit = !this.state.isBreakInheritance;
        permissionInfo.selConflictType = this.state.selConflictType;
        callback(permissionInfo, true);
    }

    getRealPermissionUsers() {
        //去重，将增加到userList的重复user去掉。
        let selectedUsers = this.state.userList || [];
        let userList = [];
        let obj = {};
        for (let item of selectedUsers) {
            if (item.UserId) {
                if (!obj[item.UserId]) {
                    userList.push(item);
                    obj[item.UserId] = true;
                }
            } else {
                if (!obj[item.Id]) {
                    userList.push(item);
                    obj[item.Id] = true;
                }
            }
        }
        return userList;
    }

    //当前节点回显的permission info.
    setUsersToList() {
        let phyObjItems = this.state.phyObjItems;
        if (phyObjItems.length > 0) {
            $$.loading(true);
            let paramStr = phyObjItems[0].Id;
            let option = {
                url: `/api/PhysicalRecordApi/GetBreakOrInheritPermission?scopeId=${paramStr}&includeSelf=${true}`,
                method: "GET",
            };
            fetchUtility(option).then((result) => {
                $$.loading(false);
                let res = JSON.parse(result);
                this.setState({
                    userList: res.Accounts || [],
                    isBreakInheritance: res.BreakInheritStatus
                });
            }).catch((e) => {
                $$.loading(false);
            });
        }
    }

    //继承时permission info.
    setInheritanceInfo() {
        let phyObjItems = this.state.phyObjItems;
        if (phyObjItems.length > 0) {
            $$.loading(true);
            let paramStr = phyObjItems[0].Id;
            let option = {
                url: `/api/PhysicalRecordApi/GetBreakOrInheritPermission?scopeId=${paramStr}&includeSelf=${false}`,
                method: "GET",
            };
            fetchUtility(option).then((result) => {
                $$.loading(false);
                let res = JSON.parse(result);
                this.inheritanceInfo = res.Accounts ? RM.deepcopy(res.Accounts) : [];
                this.setState({
                    superiorIsBreakInheritance: res.BreakInheritStatus
                });
            }).catch((e) => {
                $$.loading(false);
            });
        }
    }

    onSearchUser(args) {
        if(args.find(o => o.invalid) === undefined) {
            this.setState({ searchedUser: args }, () => {
                this.addUserToList();
            });
        }
    }

    addUserToList() {
        let userList = this.state.userList;
        userList = [...userList, ...this.state.searchedUser];
        this.setState({
            userList: userList,
            searchedUser: []
        });
    }

    deleteUserToList(index) {
        let userList = this.state.userList;
        userList.splice(index, 1);
        this.setState({ userList: RM.deepcopy(userList) });
    }

    inheritanceClick() {
        this.setState({
            showTip: false,
            isBreakInheritance: false,
            userList: RM.deepcopy(this.inheritanceInfo)
        });
    }

    breakInheritance() {
        this.setState({
            isBreakInheritance: true,
            userList: RM.deepcopy(this.inheritanceInfo)
        });
    }

    renderMessageTip() {
        return <R.Messagebar
            message={this.state.tipMsg}
            classify={this.state.tipType}
            status={{ show: this.state.showTip }}
            onClose={this.hideMessageTip}
        />;
    }

    renderInheritanceBtns() {
        //nodeType room和room以下显示不同的词条。
        //打破继承和继承也要显示不同词条。
        let permissionIntroduce = "";
        if(this.state.isGlobalSearch)
        {
            permissionIntroduce = RMResx.RM_PRM_GS_Permission_Panel_Desc;
        }
        else
        {
            permissionIntroduce = this.state.isBreakInheritance ?
                RMResx.RM_PRM_PRE_BreakInheritance_PermissIntroForLocation : RMResx.RM_PRM_PRE_PermissionIntroduce;
            if (this.state.phyObjItems.length > 0 && (this.state.phyObjItems[0].NodeType > NodeType.PhysicalRootLocation))
            {
                permissionIntroduce = this.state.isBreakInheritance ?
                    RMResx.RM_PRM_PRE_BreakInheritance_PermissIntroForBoxsAndFolders : RMResx.RM_PRM_PRE_PermissionIntroduceForBoxsAndFolders;
            }
        }

        return <div className='manage-permission-header'>
            <div className='inheritance-introduce' tabIndex='0'>
                {permissionIntroduce}
            </div>
            <div className='inheritance-button'>
                {
                    !this.state.isBreakInheritance &&
                    <R.Button
                        type="link"
                        text={RMResx.RM_PRM_PRE_BreakInheriteance}
                        icon='fia-lock-open'
                        onClick={this.breakInheritance} />
                }
                {
                    this.state.isBreakInheritance && !this.state.isGlobalSearch &&
                    <R.Button
                        type="link"
                        text={RMResx.RM_PRM_PRE_Inheritance}
                        onClick={this.inheritanceClick}
                        icon='fia-lock'
                    />
                }
            </div>
            <div className='dividing_line'></div>
        </div>;
    }

    renderUserSearch() {
        if (this.state.isBreakInheritance) {
            return <div>
                <div className='search-users-title' tabIndex='0'>{RMResx.RM_PRM_PRE_EnterUserOrGroup.replace(":","")}</div>
                <div className='search-users-content'>
                    <div className='pull-left'>
                        <PeoplePicker
                            width={530}
                            items={this.state.searchedUser}
                            selectionChanged={this.onSearchUser}
                        />
                    </div>
                </div>
            </div>;
        }
    }

    renderUserList() {
        this.state.userList = this.state.userList || [];
        if (this.state.userList.length == 0 && !this.state.isBreakInheritance && !this.state.superiorIsBreakInheritance) {
            return <div className='user-content'>
                <div className='inherit-parent-text' tabIndex='0'>{RMResx.RM_PRM_PRE_AllUsersHasPermission}</div>
            </div>;
        } else {
            return <div className='user-content'>
                <div className='user-content-title' tabIndex='0'>{RMResx.RM_PRM_PRE_UserGroup}</div>
                <div className='user-list'>
                    {
                        this.state.userList.map((item, index) => {
                            return <div key={index}>
                                <div className='user-list-info'>
                                    <div className='user-list-left' tabIndex='0'>
                                        <div className='user-list-info-name'>{item.DisplayName}</div>
                                        <div className='user-list-info-email'>{item.UserPrincipalName}</div>
                                    </div>
                                    {
                                        this.state.isBreakInheritance && <div className='user-list-right'>
                                            <R.Button
                                                type="bald"
                                                icon="fia-delete"
                                                tooltip={RMResx.RM_JS_Common_Delete}
                                                onClick={this.deleteUserToList.bind(this, index)} />
                                        </div>
                                    }
                                </div>
                                {/* <div className="dividing_line"></div> */}
                            </div>;
                        })
                    }
                </div>
            </div>;
        }
    }

    renderConflictOptions()
    {
        return this.state.isGlobalSearch && <div id="conflict_setting_container">
            <div className="section-title">{RMResx.RM_PRM_GS_Permission_Resolution_Title}</div>
            <div className="section-desc">{RMResx.RM_PRM_GS_Permission_Resolution_Desc}</div>
            <R.Radio.Group
                name="conflict-option-radiogroup"
                items={this.getConflictOptions()}
                onChange={this.onConflictChange}
                block={true}
            />
        </div>;
    }

    onConflictChange(value) {
        this.setState({
            selConflictType: value,
        });
    }

    getConflictOptions() {
        let options = [
            { text: RMResx.RM_PRM_GS_Permission_ConflictOption_Append, value: "1", disabled: false },
            { text: RMResx.RM_PRM_GS_Permission_ConflictOption_Overwrite, value: "0", disabled: false }
        ];
        return options.map(op => {
            op.title = op.text;
            op.checked = this.state.selConflictType == op.value;
            return op;
        });
    }

    render() {
        return <div id='raPhyObjectManagePermission'>
            {this.renderMessageTip()}
            {this.renderInheritanceBtns()}
            {this.renderUserSearch()}
            {this.renderUserList()}
            {this.renderConflictOptions()}
        </div>;
    }
}