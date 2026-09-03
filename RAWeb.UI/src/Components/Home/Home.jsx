import { Component } from 'react';
import { Link } from 'react-router-dom';
import '../../Less/Home/home.less';
import { TelemetryEventType, TelemetryModule } from '../../Constants/Constants';
import {addTelemetryRecord}  from '../../Utilities/TelemetryUtil';


export default class Home extends Component {
    constructor(props) {
        super(props);
        this.state = {
            modules: []
        };
        window.document.title = RMResx.RM_Home_PageTitle;
    }
    componentDidMount() {
        this.getData();
        this.bindEvents();
        this.initPageSize();
        addTelemetryRecord(TelemetryModule.HomePage, TelemetryEventType.HomepageLoaded);
    }
    componentWillUnmount() {
        //RM.clearBaseFontChangeCallback();
    }
    componentDidUpdate(prevProps, prevState, snapshot) {
        if (this.state.modules !== prevState.modules) {
            this.modulesResize();
        }
        return true;
    }
    bindEvents() {
        $(window).on('resize', this.modulesResize);
        //RM.setBaseFontChangeCallback(this.modulesResize);
    }
    initPageSize() {
        var sizeStr = RM.Cookie.UserInfo.getFontSize();
        var fsize = parseInt(sizeStr, 10);
        var mainWidth = $("body").width(),
            moduleWidth = 14 * fsize + 30 + 50,
            rowCount = Math.floor(mainWidth / moduleWidth);

        $("#rmInnerModules").width(moduleWidth * rowCount);
    }
    modulesResize() {
        var $main = $("body"),
            $innerModule = $("#rmInnerModules"),
            $modules = $(".ra-home-module"),
            moduleWidth = $modules.outerWidth() + 30,
            mainWidth = $main.width(),
            rowCount = Math.floor(mainWidth / moduleWidth),
            totalCount = $modules.length;
        if ($modules.length == 1)
        {
            $(".ra-home-module").width(1160);
            $(".ra-home-module").height(634);
            moduleWidth = $modules.outerWidth() + 30;
        }
        if (rowCount > totalCount) {
            rowCount = totalCount;
        }
        var tempWidth = moduleWidth * rowCount;
        if (tempWidth > $innerModule.width()) {
            $innerModule.width(tempWidth);
        }
        //compute top(css) of the elements which class = 'ra-home-moudule-link-icon'
        var sizeStr = RM.Cookie.UserInfo.getFontSize();
        sizeStr = sizeStr.substr(0, sizeStr.length - 2);
        var fsize = parseInt(sizeStr, 10);
        $(".ra-home-moudule-link-icon").css("top", (fsize * 0.75 - 18) / 2);

        var minHeight = 260,
            tempHeight = minHeight,
            curHeight,
            preRowNum = 0,
            curRowNum = 0,
            maxIdx = $modules.length - 1;
        $modules.removeAttr("style");
        $modules.each(function (idx) {
            curHeight = $(this).height();
            curRowNum = Math.floor(idx / rowCount);

            if (curHeight > tempHeight) {
                tempHeight = curHeight;
            }
            if ((maxIdx == idx || (idx%rowCount == rowCount-1))) {
                if(tempHeight > minHeight) {
                    for (var i = curRowNum * rowCount; i <= idx; i++) {
                        $modules.eq(i).height(tempHeight);
                    }
                }
                tempHeight = minHeight;
            }
        });
        if ($modules.length == 1) {
            $("#rmModules").css("padding-top", "60px");
            $(".ra-home-module").width(1160).height(634);
            $(".ra-home-module-icon").css("display", "none");
            $(".ra-home-moudule-title")
                .css("padding", "0 0 0 0")
                .css("font-weight", "normal")
                .css("line-height", "normal");
            $(".ra-home-moudule-title")
                .css("text-align", "center")
                .css("font-family", "Segoe UI")
                .css("font-size", "1.7rem")
                .css("color", "#707070;");
            $(".ra-home-moudule-description")
                .css("font-size", "0.8rem")
                .css("height", "auto")
                .css("margin-left", "64px")
                .css("margin-top", "64px");
            $(".ra-home-moudule-links")
                .css("margin-top", "30px")
                .css("margin-left", "64px");
            $(".ra-home-moudule-link")
                .css("margin-bottom", "15px");
        }
        if (tempWidth < $innerModule.width()) {
            $innerModule.width(tempWidth);
        }
    }

    getData() {
        let obj = {
        };
        let option = {
            url: '/api/HomeApi/GetModules',
            data: JSON.stringify(obj)
        };
        fetchUtility(option).then((res) => {
            if (res) {
                this.setState({ modules: res });
            }
        }).catch((e) => {
            //console.log(e);
        });
    }

    render() {
        return <div id="rmHome"><div id="rmModules">
            <div id="rmInnerModules">
                {
                    this.state.modules.map((item, index) =>
                        <div className="ra-home-module ra-box-shadow" key={index}>
                            <div className="ra-home-module-head">
                                <div className={"ra-home-module-icon " + item.iconClass}></div>
                                <div className="ra-home-moudule-title" title={item.title} tabIndex="0">
                                    {item.title}
                                </div>
                            </div>
                            <div className="ra-home-moudule-description" title={item.description} tabIndex="0">
                                {item.description}
                            </div>
                            <div className="ra-home-moudule-links">
                                {
                                    item.links.map((subLink, linkIdx) =>
                                        <div className="ra-home-moudule-link" key={linkIdx}>
                                            <div className="ra-home-moudule-link-icon"></div>
                                            {
                                                subLink.href != "_blank" && subLink.href.indexOf('/Root/') == 0 ?
                                                    <Link className="ra-home-moudule-link-a" to={{ pathname: subLink.href }}>
                                                        {subLink.text}
                                                    </Link>
                                                    :
                                                    <a className="ra-home-moudule-link-a" href={subLink.href} target={subLink.target}>
                                                        {subLink.text}
                                                    </a>
                                            }
                                        </div>
                                    )
                                }
                            </div>
                        </div>
                    )
                }
                <div className="ra-clearboth"></div>
            </div>
            <div className="ra-clearboth"></div>
        </div></div>;
    }
}
                      