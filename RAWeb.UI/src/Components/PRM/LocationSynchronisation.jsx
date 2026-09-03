import {Component} from "react";
import SiteMapLinks from "../../Constants/SiteMapLinks";
import RouterUrls from "../../Constants/RouterUrls";
import {bindEvents} from "../../Utilities/CommonUtil";
import "../../Less/PRM/LocationSynchronization.less";


export default class LocationSynchronisation extends Component {
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

    componentWillUnmount() {

    }

    getJobDetail() {

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
        this.scheduleConponment.saveAndRun((success, msg) => {
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
                    <a className="ra-link-a" href="Root/JM/Index">{RMResx.RM_JS_JM_Title}</a>
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
        return <div id="rmLocationSynchronisation">
            <$g.SiteMap data={[SiteMapLinks.PRM_LocationSynchronisation]}/>
            <R.Messagebar
                message={this.state.tipMsg}
                classify={this.state.tipType}
                onClose={this.hideMessageTip}
                status={{ show: this.state.tipStatus.show }} />
            <div className="term-sync-box" style={{display: "block"}}>
                <h2 className="term-sync-tips">
                    <span tabIndex="0">
                        {RMResx.RM_PRM_LS_ContentTitle}
                    </span>
                </h2>

                <form id="termSyncForm">
                    <h3 className="term-sync-option">
                        <span className="term-sync-mustFill-icon">* </span>
                        <span tabIndex="0" style={{fontWeight: "600"}}>
                            {RMResx.RM_BCM_TermSync_ContentTitle} </span>
                    </h3>
                    <div className="scheduleSetting">
                        <$g.ScheduleSetting
                            ref={r => this.scheduleConponment = r}
                            fromTimerJobPage={false}
                            type={3}
                            getJobDetail={this.getJobDetail}
                        />
                    </div>
                </form>
            </div>
            <div className="term-sync-bottom-btns">
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_JS_Common_Save}
                    onClick={this.onSaveBtnClick}/>
                <R.Button
                    primary={true}
                    classify="theme"
                    text={RMResx.RM_BCM_TermSync_SyncBtn}
                    onClick={this.onSyncBtnClick}/>
                <R.Button
                    text={RMResx.RM_JS_Common_Cancel}
                    onClick={this.onCancelClick}/>
            </div>
        </div>;
    }
}
