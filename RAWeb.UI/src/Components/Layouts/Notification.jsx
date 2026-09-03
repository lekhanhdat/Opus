import RouterUrls from "../../Constants/RouterUrls";
import { bindEvents } from "../../Utilities/CommonUtil";
import { checkPermission } from "../../Utilities/permissionManager";
import "../Layouts/index.less";

export default class Notification extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            content: "",
            isShowEmptySpace: true,
            agentReminderContent: "",
            agentLatestVersion: "",
        };

        bindEvents(
            this,
            "handleCloseKeydown",
            "handleCloseClick",
            "handleNotify",
            "handleDismissAll"
        );
    }

    componentInit() {
        let isAllowShowAgentManagementCard =
        checkPermission("Source_FS", RM.UserResources) ||
        checkPermission("CP_Schedule_Settings_On_Prem", RM.UserResources);
        
        if(isAllowShowAgentManagementCard && RM.RoleType == 1){
            let option = {
                url: "/api/CPAgentMgmtApi/GetAllAgents",
                method: "POST",
            };
            this.setAgentLatestVersion();
            fetchUtility(option).then((res) => {
                let needUpdateAgentVersion = false;
                if ((res != null) && (res.length > 0)) {
                    res.map((item) => {
                        if (item.Status == 5) {
                            needUpdateAgentVersion = true;
                        }
                    });
                    if(!needUpdateAgentVersion && res.some(o => this.isLowAgentMinorVersion(o.Version)))
                    {
                        needUpdateAgentVersion = true;
                    }
                    
                    if(needUpdateAgentVersion){
                        this.setState({
                            isShowEmptySpace : false,
                            agentReminderContent: this.getAgentReminderContent(RMResx.RM_Notification_Agent_NeedUpdatedVersion)}
                        )
                    }
                }
            });
        }
    }

    componentReceive(element) {
        let isShowEmptySpace = true;
        if (element.props && element.props.children.length > 0) {
            isShowEmptySpace = false;
        }
        if(this.state.agentReminderContent){
            isShowEmptySpace = false;
        }
        this.setState({
            content: element,
            isShowEmptySpace: isShowEmptySpace,
        });
    }

    setAgentLatestVersion(){
        $$.loading(true);
        let option = {
            url: "/api/CPAgentMgmtApi/GetAgentLatestVersion",
            method: "POST",
        };
        fetchUtility(option).then((res) => {
            if(res){
                this.setState({agentLatestVersion: res});
            }
            $$.loading(false);
        }).catch((e) => {
            $$.loading(false);
        });
    }

    isLowAgentMinorVersion = (agentVersion) => {
        const agentLatestVersion = this.state.agentLatestVersion;
        const agentLatestVersionArr = agentLatestVersion.substr(agentLatestVersion.indexOf('.') + 1).split('.');
        const agentVersionArr = agentVersion.substr(agentVersion.indexOf('.') + 1).split('.');
        if(Number(agentVersionArr[0]) < Number(agentLatestVersionArr[0])) {
            return true;
        }

        if(Number(agentVersionArr[1]) < Number(agentLatestVersionArr[1])) {
            return true;
        }

        if(Number(agentVersionArr[2]) < Number(agentLatestVersionArr[2])) {
            return true;
        }

        return false;
    }

    deleteNotificationItem = () => {
        let isShowEmptySpace = true;
        if (this.state.content.props && this.state.content.props.children.length > 0) {
            isShowEmptySpace = false;
        }
        this.setState({
            agentReminderContent : "",
            isShowEmptySpace : isShowEmptySpace
        });
    }

    onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    getAgentReminderContent(msg) {
        let startTime = new Date();
        let showTime = RM.TimeUtil.dateToStringSimplifyTimeZone(startTime, RM.TimeUtil.getGlobalTimezoneInfo())
        return <div className="raNotifications">
        {
            <div>
                <div className="ra-notifications-space"></div>
                <div className="ra-notifications-title">
                    <div className="ra-notifications-title-info" tabIndex="0">{msg}</div>
                    <div className='fia-searchbox-close' onClick={()=>{ this.deleteNotificationItem() }} onKeyDown={this.onKeyDown}></div>
                </div>
                <div className="ra-notifications-file-list" tabIndex="0">
                    <$g.I18NProvider msg={RMResx.RM_Notification_Agent_GoTo}>
                        <a className="ra-link-a" href="/Root/CP/AgentManagement" tabIndex="0">{RMResx.RM_CP_Agent_Management}</a>
                    </$g.I18NProvider>
                </div>
                <div className="ra-notifications-foot"> 
                    <div className="ra-agent-notifications-time" tabIndex="0">{showTime}</div>
                </div>
            </div>
        }
        </div> 
    }

    handleNotify(e) {
        e.nativeEvent.stopImmediatePropagation();
    }

    handleDismissAll() {
        this.setState({
            agentReminderContent: "",
            content: "",
            isShowEmptySpace: true,
        });
    }

    render() {
        return (
            <div id={this.props.id}>
                {!this.state.isShowEmptySpace && <div className="notification-tip"></div>}
                <div
                    className="notification_suitbar_container"
                    style={{ display: "none" }}
                >
                    <div
                        className="notification_suitbar"
                        onClick={this.handleNotify}
                    >
                        <div className="notification_suitbar_body">
                            <R.Popup
                                id="raGlobalNotificationPopup"
                                of={this.props.stringClassName}
                                width={480}
                                // closeable={true}
                                triggerEvent="click"
                                arrow={true}
                                position="bottom"
                                title={RMResx.RM_Notification_MainTitle}
                                modalable
                            >
                                <div className="ra-notification_suitbar">
                                    <div className="ra-suitbar-more gap-xs">
                                        {checkPermission(RouterUrls.RC_AuditReportManagement, RM.UserResources) && <a
                                            style={{ flex: 1 }}
                                            className="ra-suitbar-more-a"
                                            href="/Root/RC/AuditReport/Management"
                                            tabIndex="0"
                                        >
                                            {RMResx.RM_SuiteBar_Notification_ViewAll}
                                        </a>}

                                        <a
                                            className="ra-suitbar-more-a ignore-link"
                                            onClick={this.handleDismissAll}
                                            tabIndex="0"
                                        >
                                            {RMResx.RM_SuiteBar_Notification_IgnoreAll}
                                        </a>
                                    </div>
                                    {this.state.isShowEmptySpace && <div className="notification-space"></div>}
                                    {this.state.agentReminderContent}
                                    <div className="rm-notification-content">
                                        {this.state.content}
                                    </div>
                                    <div className="notification_body">
                                        {this.state.isShowEmptySpace && (
                                            <div className="notification_body_empty">
                                                {RMResx.RM_Notification_NoNotifications}
                                            </div>
                                        )}
                                    </div>
                                </div>
                            </R.Popup>
                        </div>
                    </div>
                </div>
            </div>
        );
    }
}
