import {Component} from "react";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import RouterUrls from "../../Constants/RouterUrls";
import {bindEvents} from "../../Utilities/CommonUtil";
import "../../Less/PRM/UpdateRecordLocation.less";


export default class UpdateRecordLocation extends Component {
    constructor(props) {
        super(props);
        this.changeStatus = false;
        this.tempSchedule = {};
        this.JMTitle = RMResx.RM_JS_JM_Title;
        this.state = {
            tipStatus: {show: false},
            tipType: "success",
            tipMsg: 1
        };
        bindEvents(this, "onCancelClick",
            "onSaveBtnClick", "onSyncBtnClick", "onSave",
            "onChangeTipStatusShow", "onChangeTipStatusHide",
            "onChangeTipStatusError", "onChangeTipStatusInfo"
        );
    }

    componentDidMount() {

    }

    getJobDetail() {

    }

    componentWillUnmount() {

    }

    showMessageTip(type, msg) {
        let tipOption = {
            tipStatus: {show: true},
            tipType: type,
            tipMsg: msg
        };
        this.setState(tipOption);
    }

    onChangeTipStatusInfo() {
        this.showMessageTip(
            "success",
            <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
            </$g.I18NProvider>);
    }

    onSaveBtnClick() {
        this.setState({
            tipStatus: {show: false}
        });

        this.scheduleConponment.saveOnly((success, msg) => {
            if (success) {
                this.showMessageTip("success", RMResx.RM_JS_BCM_TermSync_SaveSuccessMessage);
            } else {
                this.showMessageTip("error", RMResx.RM_JS_BCM_TermSync_SaveFailMessage);
            }
        });
    }

    onSyncBtnClick() {
        this.setState({
            tipStatus: {show: false}
        });
        this.scheduleConponment.saveAndRun((success, msg) => {
            if (success) {
                this.showMessageTip("success", <$g.I18NProvider msg={RMResx.RM_JS_BCM_TermSync_SyncSuccessMessage}>
                    <a className="ra-link-a" href="/Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
                </$g.I18NProvider>);
            } else {
                this.showMessageTip("error", RMResx.RM_JS_BCM_TermSync_SyncFailMessage);
            }
        });
    }

    onCancelClick(e) {
        this.props.history.push({
            pathname: RouterUrls.Home
        });
    }

    render() {
        return <div id="rmUpdateRecordLocation">
            <$g.SiteMap data={[SiteMapLinks.PRM_UpdateRecordLocation]}/>
            <R.Messagebar
                message={this.state.tipMsg}
                classify={this.state.tipType}
                onClose={this.hideMessageTip}
                status={{show: this.state.tipStatus.show}}
            />
            <div className="term-sync-box" style={{display: "block"}}>
                <h2 className="term-sync-tips">
                    <span tabIndex="0">
                        {RMResx.RM_URL_ContentTitle}
                    </span>
                </h2>
                <form id="termSyncForm">
                    <h3 className="term-sync-option">
                        <span className="term-sync-mustFill-icon">* </span>
                        <span tabIndex="0" style={{fontWeight: "600"}}>
                            {RMResx.RM_URL_Update_Options} </span>
                    </h3>
                    <$g.ScheduleSetting
                        ref={r => this.scheduleConponment = r}
                        fromTimerJobPage={true}
                        getJobDetail={this.getJobDetail}
                        type={4}
                    />

                </form>
            </div>
            <div className="term-sync-bottom-btns">
                <R.Button
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.onCancelClick}/>
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_URL_SaveAndUpdate_btn}
                    onClick={this.onSyncBtnClick}/>
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.onSaveBtnClick}/>
            </div>
        </div>;
    }
}
