import { withRouter } from 'react-router-dom';
import { Link } from 'react-router-dom';
import { bindEvents } from '../../Utilities/CommonUtil';
import RouterUrls from '../../Constants/RouterUrls';
import { RoleType } from '../../Constants/Constants';
import { checkPermission } from '../../Utilities/permissionManager';

export default withRouter(class RibbonRow extends React.Component {
    constructor(props) {
        super(props);
        this.state = {
            showNavPopup: false,
            navData: []
        };
        this.navDelay = null;
        this.delayTime = 150;
        this.lv2LeaveDelay = null;
        this.lv2Delay = null;
        this.isAdmin = RM.RoleType == RoleType.SupAdmin || RM.RoleType == RoleType.DelegateAdmin;
        this.isStandardUser = RM.RoleType == RoleType.StandardUser;
        bindEvents(this, 'returnHome', 'handleHomeKeydown', 'handleJMKeydown', 'handleCPKeydown',
            'handleNavClick', 'handleNavKeydown', 'handleNavFocus', 'handleNavBlur', 'handleJMFocus',
            'handleNavPopupKeydown', 'handleLv1TabMouseEnter', 'handleLv1TabMouseLeave',
            'hideNavPopup');
        this.getNavData();
        RM.hideNavPopup = this.hideNavPopup;
    }

    returnHome() {
        if (this.isAdmin) {
            this.routerTo(RouterUrls.Home);
        }
    }
    handleHomeKeydown(e) {
        if (e.keyCode == 13) {
            this.returnHome();
            e.stopPropagation();
        }
    }
    handleNavClick(e) {
        clearTimeout(this.navDelay);
        if (e.target.id == "rmNav_group" || e.target.id == "rmNav_icon"
            || e.target.className == "ra-nav-name" || e.target.id == "rmNav_downIcon") {
            $(".ra-nav-lv1").slideToggle(200);
            this.setState({ showNavPopup: !this.state.showNavPopup });
        }
    }
    handleNavKeydown(e) {
        if (e.keyCode == 13) {
            $(".ra-nav-lv1").slideToggle(200);
            e.stopPropagation();
        }
    }
    handleNavPopupKeydown(e) {
        if (e.keyCode == 27) {
            this.setState({ showNavPopup: false });
            $(".ra-nav-lv1").hide(200);
        }
        e.stopPropagation();
    }
    handleNavFocus(e) {
        if (e.target.id == "rmNav_group" || e.target.id == "rmNav_icon"
            || e.target.className == "ra-nav-name" || e.target.id == "rmNav_downIcon") {
            this.hideNavPopup();
        }

    }
    handleNavBlur(e) {
        this.navDelay = setTimeout(() => {
            if (document.activeElement) {
                var $focusEl = $(document.activeElement);
                if (document.activeElement.id != "rmNav_group" && $focusEl.closest("#rmNav_group").length == 0) {
                    this.setState({ showNavPopup: false });
                    $(".ra-nav-lv1").hide(200);
                }
            }
        }, 400);
    }
    handleLv1TabMouseEnter(e) {
        var nav1Tab = $(e.currentTarget);
        clearTimeout(this.lv2LeaveDelay);
        this.lv2Delay = setTimeout(function () {
            nav1Tab.addClass("ra-nav-lv1Hover");
            nav1Tab.siblings(".ra-nav-lv1Tab").removeClass("ra-nav-lv1Hover");
        }, this.delayTime);
    }
    handleLv1TabMouseLeave(e) {
        //var nav1Tab = $(e.currentTarget);
        clearTimeout(this.lv2Delay);
        this.lv2LeaveDelay = setTimeout(() => {
            //nav1Tab.removeClass("ra-nav-lv1Hover");
            this.removeLv1TabHoverState();
        }, this.delayTime);
    }
    handleLv1TabKeyDown(e) {
        //var nav1Tab = $(e.currentTarget);
        //var keyCode = e.which || event.keyCode || e.keyCode;
        //if (keyCode == 13) {
        //    nav1Tab.find(".ra-nav-lv2").focus();
        //    e.stopPropagation();
        //}
        //else if (keyCode == 9) {
        //    nav1Tab.addClass("ra-nav-lv1Hover");
        //}
    }
    handleLv1TabFocus(e) {
        var nav1Tab = $(e.currentTarget);
        nav1Tab.addClass("ra-nav-lv1Hover");
        nav1Tab.siblings(".ra-nav-lv1Tab").removeClass("ra-nav-lv1Hover");
    }
    handleLv2TabKeydown(e) {
        var $curEl = $(e.currentTarget);
        var $parent = $curEl.parent(".ra-nav-lv2Tab");
        if (e.keyCode == 9 && !e.shiftKey) {
            if ($parent.index() == $parent.closest(".ra-nav-lv2").find("li").length - 1) {
                $(".ra-nav-lv2").hide();
                if ($parent.closest(".ra-nav-lv1Tab").index() == $(".ra-nav-lv1Tab").length - 1) {
                    $(".ra-nav-jm").focus();
                    $(".ra-nav-lv2").hide();
                } else {
                    $curEl.closest(".ra-nav-lv1Tab").next().focus();
                    $(".ra-nav-lv2").hide();
                }
            } else {
                $parent.next().find("a").focus();
            }
            e.stopPropagation();
            e.preventDefault();
        } else if (e.shiftKey && e.keyCode == 9) {
            $parent.prev().find("a").focus();
            e.stopPropagation();
            e.preventDefault();
        }
    }
    handleJMFocus(e) {
        this.removeLv1TabHoverState();
    }
    handleJMKeydown(e) {
        if (e.keyCode == 13) {
            this.routerTo(RouterUrls.Home);
            e.stopPropagation();
        }
    }
    handleCPKeydown(e) {
        if (e.keyCode == 13) {
            this.routerTo(RouterUrls.CP_Index);
            e.stopPropagation();
        }
    }


    routerTo(routerUrl) {
        this.props.history.push({
            pathname: routerUrl
        });
        this.hideNavPopup();
    }
    removeLv1TabHoverState() {
        $(".ra-nav-lv1Tab").removeClass("ra-nav-lv1Hover");
    }
    hideNavPopup() {
        this.setState({ showNavPopup: false });
        $(".ra-nav-lv1").hide(200);
        this.removeLv1TabHoverState();
    }
    getNavData() {
        let obj = {
        };
        let option = {
            url: '/api/HomeApi/GetModules',
            data: JSON.stringify(obj)
        };
        fetchUtility(option).then((res) => {
            if (res) {
                this.setState({ navData: res });
            }
        }).catch((e) => {
            //console.log(e);
        });
    }

    /**
     * 显示导航菜单
     */
    renderNav(isAdmin) {
        var navData = this.state.navData;
        if (isAdmin) {
            return navData.map((item, index) => {
                return <li className="ra-nav-lv1Tab" key={index} tabIndex="0"
                    onMouseEnter={this.handleLv1TabMouseEnter} onMouseLeave={this.handleLv1TabMouseLeave}
                    onKeyDown={this.handleLv1TabKeyDown} onFocus={this.handleLv1TabFocus}>
                    <div className="ra-nav-lv1TabName">{item.title} </div>
                    <ul className="ra-nav-lv2">
                        {this.renderSubNav(item.links)}
                    </ul>
                </li>;
            });
        }
        else {
            if (navData && navData.length > 0) {
                return navData[0].links.map((sublink, index) => {
                    return (
                        <li className="ra-nav-lv1Tab" key={index} tabIndex="0"
                            onMouseEnter={this.handleLv1TabMouseEnter} onMouseLeave={this.handleLv1TabMouseLeave}
                            onKeyDown={this.handleLv1TabKeyDown} onFocus={this.handleLv1TabFocus}>
                            <div className="ra-nav-lv1TabName">
                                <a className="ra-link-a" href={sublink.href} target={sublink.target} onKeyDown={this.handleLv1TabKeyDown}>
                                    {sublink.text}
                                </a>
                            </div>
                        </li>
                    );
                });
            }
        }
    }


    renderSubNav(subNavs) {
        return subNavs.map((subLink, index) => {
            return <li className="ra-nav-lv2Tab" key={index}>
                {
                    subLink.target != "_blank" && subLink.href.indexOf('/Root/') == 0 ?
                        <Link to={{ pathname: subLink.href }} onClick={this.hideNavPopup.bind(this)} onKeyDown={this.handleLv2TabKeydown}>
                            {subLink.text}
                        </Link>
                        :
                        <a className="ra-link-a" href={subLink.href} target={subLink.target} onClick={this.hideNavPopup.bind(this)} onKeyDown={this.handleLv2TabKeydown}>
                            {subLink.text}
                        </a>
                }
            </li>;
        });
    }

    /**
     * 是否显示JobMonitor和Control菜单
     * @param {*} isAdmin 是否是管理员
     */
    renderJobMonitorAndControlPanel() {
        return (
            <li className="ra-nav-jmcp">
                {checkPermission(RouterUrls.JM, RM.UserResources) && <div className="ra-nav-jm" tabIndex="0" onFocus={this.handleJMFocus}
                    onClick={() => this.routerTo(RouterUrls.JM_Index)} onKeyDown={this.handleJMKeydown}>
                    <span className="rmRibbonRow-right-groupImg ra-nav-jmIcon"></span>
                    <span className="ra-nva-jmlable">{RMResx.RM_JS_JM_Title}</span>
                </div>}
                {checkPermission(RouterUrls.CP, RM.UserResources) && <div className="ra-nav-cp" tabIndex="0"
                    onClick={() => this.routerTo(RouterUrls.CP_Index)} onKeyDown={this.handleCPKeydown}>
                    <span className="rmRibbonRow-right-groupImg ra-nav-cpIcon"></span>
                    <span className="ra-nva-cplable">{RMResx.RM_CP_ControlPanel}</span>
                </div>}
            </li>
        );
        
    }

    render() {
        return <React.Fragment>
            <div id="rmRibbonRow">
                <div id="rmRibbonRow_left_group">
                    <img id="rmlogo" tabIndex="0" src="/Images/Home/logo_nav.png"
                        alt={RMResx.RM_JS_Common_RecourdAutomation} onClick={this.returnHome} />
                </div>
                {
                    <div id="rmRibbonRow_right_group">
                        <div className="rmRibbonRow-right-item" tabIndex="0" id="rmHome_group"
                            onClick={() => this.routerTo(RouterUrls.Home)} onKeyDown={this.handleHomeKeydown}>
                            <span className="rmHome-item rmRibbonRow-right-groupImg" id="rmHome_icon"></span>
                            <span className="rmHome-item" id="rmHome_label">
                                {RMResx.RM_Home_PageTitle}
                            </span>
                        </div>
                        <div className="rmRibbonRow-right-item" tabIndex="0" id="rmNav_group"
                            onClick={this.handleNavClick} onKeyDown={this.handleNavKeydown}
                            onBlur={this.handleNavBlur} onFocus={this.handleNavFocus} >
                            <div className="rmNav-item rmRibbonRow-right-groupImg" id="rmNav_icon"></div>
                            <div className="rmNav-item" id="rmNav_label">
                                <div className="ra-nav-name">{RMResx.RM_Header_MenuBtn}</div>
                                <ul className="ra-nav-lv1" onKeyDown={this.handleNavPopupKeydown}>
                                    {
                                        this.renderNav(this.isAdmin)
                                    }
                                    {
                                        this.renderJobMonitorAndControlPanel()
                                    }
                                </ul>
                            </div>
                            <div id="rmNav_downIcon" className={"rmNav-item rmRibbonRow-right-groupImg"
                                + (this.state.showNavPopup ? ' rmNav_downIcon_hover' : '')}></div>
                        </div>
                    </div>
                }
            </div>
        </React.Fragment>;
    }
});
