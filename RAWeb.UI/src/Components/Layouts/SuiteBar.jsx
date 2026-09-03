import { bindEvents } from "../../Utilities/CommonUtil";
import "../../Less/Layouts/suiteBar.less";
import Notification from "./Notification";
// import Notification from "../Layouts/Notification";
import UserInfo from './UserInfo';
import AvaChatDialog from "./AvaChatDialog";
import GlobalDC from "./GlobalDC";
export default class SuiteBar extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            content: "",
            isShowEmptySpace: true,
            showUserMenu: false,
        };
        this.hidePopupTimer = null;

        bindEvents(
            this,
            "handleHelpAboutKeydown",
            "handleHelpMenuClick",
            "handleHelpMenuKeydown",
            "tryHideHelpMenu",
            "handleHelpAboutShow",
            "handleUserMenuClick",
            "tryHideUserMenu",
            "handleUserMenuKeydown",
            "handleNotificationClick",
            "tryHideNotiMenu",
            "handleNotificationKeydown",
            "handleDismissAll"
        );
    }

    componentInit() {
        document.addEventListener("click", this.tryHideNotiMenu);
    }
    handleCloseClick(e) {
        $(".notification_suitbar_container").toggle();
        $(".notification_suitbar").toggle(50);
        $(".rmSuitBar-notification-Img").toggleClass(
            "rmSuitBar-notification_selected-Img"
        );
        $(".rmSuitBar-notification-Img")
            .parent()
            .toggleClass("rm_notification_selected");
    }
    handleUserMenuClick(e) {
        if (
            e.target.id == "rmUserManager_Content" ||
            e.target.id == "rmUserManager_Icon" ||
            e.target.id == "rmUserManager_Arrow" ||
            e.target.id == "rmUserManager_Name"
        ) {
            this.setState({ showUserMenu: !this.state.showUserMenu });
            $("#rmUserManager_DropDown").slideToggle(200);
        }
    }

    tryHideUserMenu(e) {
        this.hidePopupTimer = setTimeout(() => {
            if (
                $(document.activeElement).closest("#rmUserManager_Content")
                    .length == 0
            ) {
                this.setState({ showUserMenu: false });
                $("#rmUserManager_DropDown").hide(200);
            }
        }, 200);
    }

    handleUserMenuKeydown(e) {
        if (e.keyCode == 13) {
            this.handleUserMenuClick(e);
        }
    }

    handleLogoutKeydown(e) {
        if (e.keyCode == 9) {
            //tab
            $("#rmUserManager_DropDown").hide(200);
        }
    }

    handleHelpMenuClick(e) {
        if (e.target.id == "rm_helpContext" || e.target.id == "rmHelp") {
            this.setState({ showHelpMenu: !this.state.showHelpMenu });
            $("#rmHelp_DropDownList").slideToggle(200);
        }
    }

    tryHideHelpMenu(e) {
        this.hidePopupTimer = setTimeout(() => {
            if (
                $(document.activeElement).closest("#rm_helpContext").length == 0
            ) {
                this.setState({ showHelpMenu: false });
                $("#rmHelp_DropDownList").hide(200);
            }
        }, 200);
    }

    handleHelpMenuKeydown(e) {
        if (e.keyCode == 13) {
            this.handleHelpMenuClick(e);
        }
    }

    handleNotificationClick(e) {
        $(".notification_suitbar_container").toggle();
        $(".notification_suitbar").toggle(50);
        $(".rmSuitBar-notification-Img").toggleClass(
            "rmSuitBar-notification_selected-Img"
        );
        $(".rmSuitBar-notification-Img").removeClass(
            "rmSuitBar-notification-alert-Img"
        );
        $(".rmSuitBar-notification-Img")
            .parent()
            .toggleClass("rm_notification_selected");
        e.nativeEvent.stopImmediatePropagation();
    }

    tryHideNotiMenu(e) {
        this.hidePopupTimer = setTimeout(() => {
            //console.log(document.activeElement);
            if (
                $(document.activeElement).closest(".notification_suitbar")
                    .length == 0
            ) {
                //this.setState({ showNotiMenu: false });
                $(".notification_suitbar").hide(50);
                $(".notification_suitbar_container").hide();
                $(".rmSuitBar-notification-Img").removeClass(
                    "rmSuitBar-notification_selected-Img"
                );
                $(".rmSuitBar-notification-Img")
                    .parent()
                    .removeClass("rm_notification_selected");
            }
        }, 200);
    }

    handleNotificationKeydown(e) {
        if (e.keyCode == 13) {
            this.handleNotificationClick(e);
        }
        if (e.keyCode == 9) {
            if ($(".notification_suitbar_container").is(":visible")) {
                e.preventDefault();
                $(".head-title span").focus();
            }
        }
    }

    handleDismissAll() {
        this.setState({
            content: "",
            isShowEmptySpace: true,
        });
    }

    clearTimer() {
        //if (this.hidePopupTimer != null) {
        //    clearTimeout(this.hidePopupTimer);
        //    this.hidePopupTimer = null;
        //}
    }

    render() {
        // eslint-disable-next-line no-unused-vars
        let avepointLink =
            RM.gData.currentLanguage == "ja" ||
                RM.gData.currentLanguage == "ja-JP"
                ? "https://www.avepoint.com/jp"
                : "https://www.avepoint.com/";
        return (
            <React.Fragment>
                <div id="rmSuiteBar">
                        <div className="ra-suitbar-group">
                            <div className="ra-suitbar-item">
                                <GlobalDC id="raGlobalDcSelector"/>
                            </div>
                            <div className="ra-suitbar-item">
                                <AvaChatDialog />
                            </div>
                            <div className="ra-suitbar-item">
                                <R.Button
                                    className="ra-suitbar-btn2"
                                    type="icon"
                                    icon="fia-notification"
                                    text="button"
                                    tooltip={RMResx.RM_Notification_MainTitle}
                                />
                                <Notification
                                    id="raNotification"
                                    stringClassName=".ra-suitbar-btn2"
                                />
                            </div>
                            <div className="ra-suitbar-item">
                                <UserInfo />
                            </div>
                        </div>
                </div>
            </React.Fragment>
        );
    }
}
