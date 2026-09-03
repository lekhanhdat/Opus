import React from "react";
import { SearchViewTypes, SearchViewActions, BuildInViewActions, SpecialSearchViewIds } from "../../Constants";
export default class HSSearchView extends R.Component {
    idAttr = true;
    componentCreate() {
        this.state = {
            selectedViewText: "",
            searchViewActions: SearchViewActions,
            buildInViewActions: BuildInViewActions,
            buildInViews: "",
            allSearchViews: [],
            savedSearchViewName: "",
            isShowSetViewNameDialog: false,
            isShowShareViewDialog: false,
            validViewNameEmpty: false,
            isShowPopup: false,
            groupItems: [],
            isCanShareView: false,
            currentViewIsShared: false,
            showOfflineSearchTip: false,
        };
        this.selectedSearchView = {};
        this.allSearchViews = [];
        this.sharedGroupIds = [];
    }

    componentInit() {
        if(this.props.isExpireReturnDateSearch){
            this.loadSearchView(SpecialSearchViewIds.ReturnDate);
        }else{
            this.loadSearchView();
        }
        this.setIsHasShareViewPermission();
    }

    componentReceive(action, data) {
        if(action == "loadSearchView"){
            this.loadSearchView(data);
        }
        if(action == "initSearchProfiles"){
            this.selectedSearchView.IsOffline = data.IsOffline;
            this.setState({ allSearchViews: RM.deepcopy(this.state.allSearchViews)});
        }
        if(action == "showOfflineSearchTip"){
            this.setState({showOfflineSearchTip: data});
        }
        if (action === "refreshSearchViews") {
            // Reset to standard profile always, not default profile.
            this.onSelectViewChanged(this.allSearchViews[0]);
        }
    }

    onOpenSearchView = ()=>{
        this.props.onOpenSearchView();
    }

    setIsHasShareViewPermission(){
        $$.loading(true);
        let option = {
            url: "/api/PersonalSettinggApi/CanShare",
            method: "POST",
            data: ""
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.setState({ isCanShareView: res });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    loadSearchView(currentId) {
        $$.loading(true);
        let option = {
            url: "/api/PersonalSettinggApi/GetAllGlobalSearchCriteria",
            method: "POST",
            data: ""
        };
        fetchUtility(option).then((res) => {
            $$.loading(false);
            this.allSearchViews = RM.deepcopy(res);
            let selectedSearchView = [];
            let isExpireReturnDateSearch = currentId == SpecialSearchViewIds.ReturnDate;
            if(!isExpireReturnDateSearch){
                if (currentId) {
                    selectedSearchView = this.allSearchViews.filter((item) => { return item.Id == currentId; });
                } else {
                    selectedSearchView = this.allSearchViews.filter((item) => { return item.IsDefault; });
                }
            }
            this.onSelectViewChanged(selectedSearchView[0] || this.allSearchViews[0]);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    getPrivateViewsInfo(isBuildIn) {
        let allSearchViews = RM.deepcopy(this.state.allSearchViews);
        let searchViews = allSearchViews.filter((item) => { return item.IsBuiltIn == isBuildIn && !item.IsSharedBy; });
        return searchViews;
    }

    getSharedViewsInfo(){
        let allSearchViews = RM.deepcopy(this.state.allSearchViews);
        let searchViews = allSearchViews.filter((item) => { return item.IsSharedBy; });
        return searchViews;
    }

    onSelectViewChanged = (args) => {
        let allSearchViews = RM.deepcopy(this.allSearchViews);
        for (let item of allSearchViews) {
            item.Checked = args.Id == item.Id;
            if (item.Checked) {
                this.setState({ selectedViewText: item.Name });
                this.selectedSearchView = item;
            }
        }
        this.setState({
            allSearchViews: allSearchViews,
            isShowPopup: false
        });
        let operateViewParam = {
            actionType: SearchViewTypes.View,
            selectedViewInfo: this.selectedSearchView,
        };
        this.props.onOperate(operateViewParam);
    }

    onOperateSearchView = (actionType) => {
        let args = {};
        this.actionType = actionType;
        switch (actionType) {
            case SearchViewTypes.Save:
            case SearchViewTypes.SaveAs:
                this.setState({
                    isShowSetViewNameDialog: true,
                    savedSearchViewName: actionType == SearchViewTypes.Save ? this.selectedSearchView.Name : "",
                });
                break;
            case SearchViewTypes.SaveAsDefaut:
                args = {
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: RMResx.RM_HS_Criteria_View_MsgBox_SetDefaultView_Notice,
                    buttons: [
                        { text: RMResx.RM_JS_Common_Cancel, onClick: () => { $$.messagedialog(false, args); } },
                        { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onSaveAsDefaut },
                        
                    ],
                };
                $$.messagedialog(true, args);
                break;
            case SearchViewTypes.Delete:
                args = {
                    hideActions: false,
                    title: RMResx.RM_JS_Common_Confirmation,
                    content: RMResx.RM_HS_Criteria_View_MsgBox_DeleteView_Notice,
                    buttons: [
                        { text: RMResx.RM_JS_Common_Cancel, onClick: () => { $$.messagedialog(false, args); } },
                        { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onDeleteSearchView }, 
                    ],
                };
                $$.messagedialog(true, args);
                break;
            case SearchViewTypes.Share:
                this.setState({ isShowShareViewDialog: true});
                if(this.state.groupItems.length == 0){
                    this.loadGroupsAndContainers();
                }else{
                    this.initShareViewGroups();
                }
                break;
            default:
        }
    }

    loadGroupsAndContainers(){
        $$.loading(true);
        let option = {
            url: "/api/CPApi/LoadGroupsAndContainers",
            method: "Get",
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            if (result) {
                let groupItems = JSON.parse(result).GroupItems;
                for(let item of groupItems){
                    item.Name = RMResx[item.Name] || item.Name;
                }
                this.setState({
                    groupItems: groupItems,
                },()=>{
                    this.initShareViewGroups();
                });
            }
        }).catch((e) => {
            $$.loading(false);
        });
    }

    initShareViewGroups(){
        $$.loading(true);
        let option = {
            url: "/api/PersonalSettinggApi/GetGlobalSearchShareSetting",
            method: "Post",
            data: this.selectedSearchView.Id
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            let groupItems = RM.deepcopy(this.state.groupItems);
            for(let item of groupItems){
                item.Checked = result.SecurityGroups.includes(item.Id);
            }
            this.setState({
                groupItems: groupItems,
                currentViewIsShared: result.SecurityGroups.length > 0
            });
        }).catch((e) => {
            $$.loading(false);
        });
    }

    onChangeViewName = (value) => {
        this.setState({
            savedSearchViewName: value,
            validViewNameEmpty: !value
        });
    }

    onHideSetViewNameDialog() {
        this.setState({
            savedSearchViewName: "",
            isShowSetViewNameDialog: false,
            validViewNameEmpty: false
        });
    }

    onSaveSearchView (){
        if (!this.state.savedSearchViewName) {
            this.setState({ validViewNameEmpty: true });
            return;
        }
        let operateViewParam = {
            actionType: this.actionType,
            selectedViewInfo: this.selectedSearchView,
            savedViewNewName: this.state.savedSearchViewName
        };
        this.props.onOperate(operateViewParam);
        this.onHideSetViewNameDialog();
    }

    onSaveAsDefaut = () => {
        let operateViewParam = {
            actionType: this.actionType,
            selectedViewInfo: this.selectedSearchView,
        };
        this.props.onOperate(operateViewParam);
        $$.messagedialog(false);
    }

    onDeleteSearchView = () => {
        let operateViewParam = {
            actionType: this.actionType,
            selectedViewInfo: this.selectedSearchView,
            savedViewNewName: this.state.savedSearchViewName
        };
        this.props.onOperate(operateViewParam);
        $$.messagedialog(false);
    }

    onChangeGroup = (args) =>{
        this.sharedGroupIds = args.newValue.map((item)=>{return item.Id;});
    }

    onHideShareViewDialog = () =>{
        this.setState({ isShowShareViewDialog: false });
    }

    onShareSearchView = (isCancelShare) =>{
        if(!isCancelShare && !$$.verify("#raHsSearchShareViewDialog")){
            return false;
        }
        if(isCancelShare){
            $$.messagedialog(false);
        }
        let operateViewParam = {
            actionType: this.actionType,
            selectedViewInfo: this.selectedSearchView,
            sharedGroupIds: this.sharedGroupIds,
            isCancelShare: isCancelShare
        };
        this.props.onOperate(operateViewParam);
        this.onHideShareViewDialog();
    }

    onCloseCancelShareSearchViewMsgbox = () =>{
        $$.messagedialog(false);
    }

    openCancelShareSearchViewMsgbox = () =>{
        $$.messagedialog(true, {
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_HS_SearchView_CancelShareNotice,
            buttons: [
                { text: RMResx.RM_JS_Common_No, onClick: this.onCloseCancelShareSearchViewMsgbox },
                { text: RMResx.RM_JS_Common_Yes, primary: true, classify: "theme", onClick: this.onShareSearchView.bind(this, true) }
            ],
        });

    }

    getSearchViewActions() {
        let actionBtns = this.selectedSearchView.IsBuiltIn ? this.state.buildInViewActions : this.state.searchViewActions;
        let sharedViewActionTypes = [SearchViewTypes.SaveAs, SearchViewTypes.SaveAsDefaut];
        let offLineViewActionTypes = [SearchViewTypes.SaveAs, SearchViewTypes.Delete, SearchViewTypes.SaveAsDefaut,SearchViewTypes.Share];
        if (this.selectedSearchView.IsDefault) {
            actionBtns = actionBtns.filter((item) => { return item.ActionType != SearchViewTypes.SaveAsDefaut ; });
        }
        if(this.selectedSearchView.IsSharedBy){
            actionBtns = actionBtns.filter((item) => { return sharedViewActionTypes.includes(item.ActionType); });
        }
        //当前user假如没有权限share view，隐藏share按钮。
        if(!this.state.isCanShareView){
            actionBtns = actionBtns.filter((item) => { return item.ActionType != SearchViewTypes.Share; });
        }
        if(this.selectedSearchView.IsOffline){
            actionBtns = actionBtns.filter((item)=>{ return offLineViewActionTypes.includes(item.ActionType); });
        }
        return actionBtns;
    }

    onKeyDown = (e) => {
        e.keyCode === 13 && e.target.click();
    }

    renderViewsCriteria(searchViewsTitle, searchViews) {
        return <div className="hs-view-selection">
            <div className="hs-view-title" tabIndex="0">{searchViewsTitle}</div>
            <div className="hs-view-criterias">
                {
                    searchViews.map((item, index) => {
                        return <div
                            key={index}
                            tabIndex="0"
                            className="hs-view-criteria"
                            onClick={this.onSelectViewChanged.bind(this, item)}
                            onKeyDown={this.onKeyDown}
                        >
                            <div className="hs-view-option-content">
                                <span className="hs-view-option-text" data-tooltip aria-label={item.Name}>{item.Name}</span>
                            </div>
                            <div className="hs-view-default">
                                {item.IsDefault && <span>{RMResx.RM_HS_Criteria_DefaultViewFlag}</span>}
                                {item.Checked && <div className="hs-view-option-icon fia-checkbox-device"></div>}
                            </div>
                        </div>;
                    })
                }
            </div>
        </div>;
    }

    renderSearchViewAction() {
        let searchViewActions = this.getSearchViewActions();
        return <div className="hs-view-action">
            {
                searchViewActions.map((item, key) => {
                    return <a className="ra-link-a" tabIndex="0" key={key} onClick={this.onOperateSearchView.bind(this, item.ActionType)} onKeyDown={this.onKeyDown}>{item.Name}</a>;
                })
            }
        </div>;
    }

    renderSearchViewPopup() {
        let buildInViews = this.getPrivateViewsInfo(true);
        let customViews = this.getPrivateViewsInfo(false);
        let sharedViews = this.getSharedViewsInfo();
        return <React.Fragment>
            <div className="aui-comboboxshell" data-tooltip aria-label={this.state.selectedViewText} onClick={this.onOpenSearchView} onKeyDown={this.onKeyDown} tabIndex="0" role="button">
                <div className="aui-comboboxshell-flex">
                    <div className="aui-comboboxshell-content aui-comboboxshell-ellipsis aui-comboboxshell-center">
                        <div className="hs-selected-combobox">
                            <span className="hs-selected-option-icon fia-view"></span>
                            <span className="hs-selected-option-text">{this.state.selectedViewText}</span>
                        </div>
                    </div>
                    <div className="aui-comboboxshell-icon-box">
                        <div className="aui-comboboxshell-icon fia-triangle-down"></div>
                    </div>
                </div>
            </div>
            <R.Popup
                of="#raHSSearchView"
                triggerEvent="click"
                status={{ show: this.state.isShowPopup }}
            >
                <div id="raHsSaveViewContent">
                    <div className="hs-views-content">
                        {this.renderViewsCriteria(RMResx.RM_HS_Criteria_Builtin_View_Title, buildInViews)}
                        {this.renderViewsCriteria(RMResx.RM_HS_ShareProfiles, sharedViews)}
                        {this.renderViewsCriteria(RMResx.RM_HS_Criteria_Custom_View_Title, customViews)}
                    </div>
                    {this.renderSearchViewAction()}
                </div>
            </R.Popup>
        </React.Fragment>;
    }

    renderSetViewNameDialogContent() {
        return <React.Fragment>
            {
                this.state.showOfflineSearchTip && <div className="margin-bottom-l">
                    {RMResx.RM_HS_Offline_SaveDialogTip}
                </div>
            }
            <$g.FormRow label={RMResx.RM_HS_Criteria_View_Dialog_ViewNameTitle} require={true}>
                <R.Input
                    type="text"
                    width=""
                    value={this.state.savedSearchViewName}
                    onChange={this.onChangeViewName}
                    placeholder={RMResx.RM_HS_Criteria_View_Dialog_ViewNameWatermark}
                />
                <$g.ValidationMsg show={this.state.validViewNameEmpty}>
                    {RMResx.RM_HS_Criteria_View_Dialog_ViewNameValid}
                </$g.ValidationMsg>
            </$g.FormRow>
        </React.Fragment>;
    }

    renderSetViewNameDialog() {
        let dialogTitle = this.actionType == SearchViewTypes.Save
            ? RMResx.RM_HS_Criteria_View_Btn_Save : RMResx.RM_HS_Criteria_View_Btn_SaveAs;
        return <R.Dialog
            id="raHsSearchSetViewNameDialog"
            header={dialogTitle}
            status={{ show: this.state.isShowSetViewNameDialog }}
            struct={{ foot: false }}
            destroy={true}
            onClose={this.onHideSetViewNameDialog.bind(this)}
        >
            <div>
                {this.renderSetViewNameDialogContent()}
            </div>
            <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onHideSetViewNameDialog.bind(this)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_JS_Common_Save} onClick={this.onSaveSearchView.bind(this)} />
            </>
        </R.Dialog>;
    }

    renderShareDialogContent(){
        return <$g.FormRow label={RMResx.RM_HS_SelectedGroupsTitle} require>
            <R.Validation element="Multicombobox" require={RMResx.RM_HS_SearchView_NotSelectedGroupsTip}>
                <R.Multicombobox
                    width={400}
                    searchable={true}
                    textField='Name'
                    valueField='Id'
                    checkedField='Checked'
                    tooltipField='Name'
                    noneText={RMResx.RM_HS_SelectedGroupsWatermark}
                    items={this.state.groupItems}
                    onChange={this.onChangeGroup}
                />
            </R.Validation> 
        </$g.FormRow>;
    }

    renderShareViewDialog(){
        let dialogTitle = RMResx.RM_HS_Dialog_ShareViewTitle;
        let shareDialogActionBtns = <>
            <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onHideShareViewDialog} />
            <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_HS_ShareDialog_ShareBtn} onClick={this.onShareSearchView.bind(this, false)} />
        </>;
        if (this.state.currentViewIsShared) {
            shareDialogActionBtns = <>
                <R.Button slot="buttons" text={RMResx.RM_JS_Common_Cancel} onClick={this.onHideShareViewDialog} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_HS_ShareDialog_ShareBtn} onClick={this.onShareSearchView.bind(this, false)} />
                <R.Button slot="buttons" primary classify="theme" text={RMResx.RM_HS_SearchView_CancelShareBtn} onClick={this.openCancelShareSearchViewMsgbox} />
            </>
        }
        return <R.Validation ref={r => this.allValidation = r}>
            <R.Dialog
                id="raHsSearchShareViewDialog"
                header={dialogTitle}
                status={{ show: this.state.isShowShareViewDialog }}
                struct={{ foot: false }}
                destroy={true}
                onClose={this.onHideShareViewDialog}
            >
                {this.renderShareDialogContent()}
                {shareDialogActionBtns}
            </R.Dialog>
        </R.Validation>;
    }

    render() {
        return <div id="raHSSearchView" className="ra-hs-search-view">
            {this.renderSearchViewPopup()}
            {this.renderSetViewNameDialog()}
            {this.renderShareViewDialog()}
        </div>;
    }
}