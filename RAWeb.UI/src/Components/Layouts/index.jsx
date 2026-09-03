import React, { Fragment, useState, useEffect, useRef } from "react";
import "./index.less";
import "../../Less/Layouts/global.less";

import { withRouter } from "react-router-dom";
import SuiteBar from "../Layouts/SuiteBar";
import Notification from "../Layouts/Notification";
import NotificationMenu from "../Layouts/NotificationMenu";
import VPAT from "../Base/VPAT";
// import AvaChatDialog from "./AvaChatDialog";
import Enviroments from "../../Constants/Enviroments";
import ScopeSourceMenu from "./ScopeSourceMenu";
import { getUserGuildTagPage } from "../../Utilities/CommonUtil";
import { productKeys } from "../../Utilities/Constant";

const Layout = ({ children, navItems, activeNav, activeId }) => {

    // const opusProductType = 16;
    const opusActiveId = "Opus";

    const [siderExpaned, setSiderExpaned] = useState(true);

    const [navLinkInfos, setNavLinkInfos] = useState([]);

    const [aosProductData, setAosProductData] = useState([]);

    const layoutContentEl = useRef(null);

    useEffect(() => {

        var interval = setInterval(() => {
            const hasScroll = (layoutContentEl.current.scrollHeight - layoutContentEl.current.clientHeight) > 0;
            let newClassName = "reco-layout-content-wrapper ";
            if (hasScroll) {
                newClassName += siderExpaned ? "reco-layout-expaned-scroll" : "reco-layout-collapsed-scroll";

            }
            else {
                newClassName += siderExpaned ? "reco-layout-expaned" : "reco-layout-collapsed";
            }

            if (layoutContentEl.current.className !== newClassName) {
                layoutContentEl.current.className = newClassName;
            }
        }, 150);
        
        initNavLinkInfos();

        return () => {
            clearInterval(interval);
        };
    }, []);

    const initNavLinkInfos = () => {
        $$.loading(true);
        let option = {
            url: "/api/HomeApi/GetSwitchBar",
            method: "GET",
        };
        fetchUtility(option).then((result) => {
            $$.loading(false);
            const aosData = result.filter(item => item.productType === -1);
            setAosProductData(aosData);
            
            const categorizedResult = {};
            const othersData = result.filter(item => item.productType !== -1 && !item.isExpired);
            othersData.forEach(item => {
                let category = item.categoryName;
                if (!category) {
                    category = item.navProductName;
                    item.isOwnGroup = true;
                }
                if (!categorizedResult[category]) {
                    categorizedResult[category] = [];
                }
                categorizedResult[category].push(item);
            });

            const resultArray = Object.keys(categorizedResult).map(category => ({
                categoryName: category,
                categoryIcon: categorizedResult[category][0].isOwnGroup ? categorizedResult[category][0].navProductIcon : categorizedResult[category][0].categoryIcon,
                productItems: categorizedResult[category],
                isOwnGroup: categorizedResult[category][0].isOwnGroup,
            }));

            const navLinkList = [];
            resultArray.forEach(item => {
                item.isOwnGroup ? navLinkList.push({
                    // id: item.productItems[0].productType,
                    id: item.productItems[0].categoryName,
                    content: item.categoryName,
                    icon: `.${item.categoryIcon}`,
                    url: item.productItems[0].url,
                }) : navLinkList.push({
                    id: item.categoryName,
                    content: item.categoryName,
                    icon: `.${item.categoryIcon}`,
                    children: getNavChildren(item.productItems),
                });
            });
            setNavLinkInfos(navLinkList);
        });
    };

    const getNavChildren = (childrens) => {
        const childrenList = [];
        childrens.forEach(children => {
            let childrenObj = {
                // id: children.productType,
                id: children.navProductName,
                content: children.navProductName,
                url: children.url,
            };
            childrenList.push(childrenObj);
        });
        return childrenList;
    };

    const onNavPanelChange = (args) => {
        window.location.href = args.url;
    };

    const toggle = (expaned) => {
        setSiderExpaned(expaned);
        let pageIndex = document.querySelector(".reco-layout-content");
        pageIndex.focus();
    };

    const change = (nav) => {
        // 该段代码是为了处理 Menu Item 跳转问题，不要修改。
        const userGuideLink = getUserGuideLink();
        const inviteSupportLink = RM.gData.aosPortalURL + "#/home?invite-support=16";
        let oldActiveId = activeId;
        if (!nav.isInternal) {
            let pageIndex = document.querySelector(".reco-layout-content");
            pageIndex.focus();
            window.open(nav.url, "_blank");
            activeNav(nav.id);
            activeNav(oldActiveId);
        } else {
            let pageIndex = document.querySelector(".reco-layout-content");
            pageIndex.focus();
            activeNav(nav.id);
            activeNav(oldActiveId);
            if (nav.id === "user-guide") {
                window.open(userGuideLink, "_blank");
                return false;
            } else if (nav.id === "about-opus") {
                handleHelpAboutShow();
                return false;
            } else if(nav.id === "invite-support") {
                window.open(inviteSupportLink, "_blank");
                return false;
            }
            else {
                return nav.url;
            }
        }

        function getUserGuideLink() {
            let guideLink = "";
            if(RM.gData.enviromentName == Enviroments.ChinaNorth) {
                guideLink = "https://cdn.avepoint.com/pdfs/cn/user_guides/AvePoint_Opus_User_Guide.pdf";
            } else if(navigator.language == "ja") {
                guideLink =  "https://cdn.avepoint.com/assets/jp/webhelp/avepoint-opus/index.htm"
            } else {
                guideLink = getUserGuildTagPage();
            }
            return guideLink;
        }
    };

    const handleHelpAboutShow = (e) => {
        $(".ra-help-about-popup,.ra-help-about-placeholder").toggleClass(
            "show"
        );
    };

    const onKeyDown = (e) => {
        if (e.keyCode == 13) {
            e.target.click();
        }
    }

    const renderAboutOpusCard = () => {
        const backgroundImageUrl = `${RM.gData.resCdnURL}/cloud%20records/about_RA.png`;
        return <div>
            <div
                className="ra-help-about-popup"
                style={{ backgroundImage: `url(${backgroundImageUrl})` }}
                tabIndex="-1"
                onKeyDown={onKeyDown}
                onClick={handleHelpAboutShow}>
                <div className="ra-help-about-popup-product-name">
                    {RMResx.RM_JS_Common_RecourdAutomation}
                    <br />
                    {RMResx.RM_JS_Common_ReleaseDate +
                        RM.gData.productVersion}
                </div>
                <div className="ra-help-about-popup-copyright">
                    {RM.gData.copyright}
                </div>
            </div>
            <div
                className="ra-help-about-placeholder"
                tabIndex="-1"
                onKeyDown={onKeyDown}
                onClick={handleHelpAboutShow}
            ></div>
        </div>
    };

    return (
        <Fragment>
            <div className="reco-layout-wrapper">
                <VPAT />
                <div className={siderExpaned ? "reco-layout-sider-expaned" : "reco-layout-sider-collapsed"}></div>
                <aside className="reco-layout-sider">
                    <div className="reco-nav">
                        <div id="productNavPanel">
                            {aosProductData.length > 0 && <R.NavPanel
                                id="NavPanelLeft"
                                key="NavPanelLeft"
                                skin="Allure"
                                colorScheme="dark"
                                iconClassify="ai"
                                items={navLinkInfos}
                                // activeId={opusProductType}
                                activeId={opusActiveId}
                                onChange={onNavPanelChange}
                                expanded={false}
                                toggleable={false}
                                arrow={true}
                            >
                                <a className="always-show" href={aosProductData[0].url} data-tooltip aria-label={aosProductData[0].navProductName}>
                                    <img className="product-img" src={aosProductData[0].navProductIcon} alt={aosProductData[0].navProductName} />
                                </a>
                            </R.NavPanel>}
                        </div>
                        <div className="reco-nav-right">
                            <R.NavPanel
                                id="NavPanelRight"
                                key="NavPanelRight"
                                skin="Allure"
                                colorScheme="dark"
                                classify="ai"
                                items={navItems}
                                activeId={activeId}
                                onToggle={toggle}
                                onChange={change}
                                arrow={true}
                            >
                                <div>
                                    <img id="rmlogo" tabIndex="0" src={`${RM.gData.resCdnURL}/cloud%20records/information-management-opus.svg`}
                                        alt={RMResx.RM_JS_Common_RecourdAutomation} />
                                </div>
                            </R.NavPanel>
                        </div>
                    </div>
                </aside>
                <div className="reco-layout">
                    <ScopeSourceMenu id="raScopeSource"/>
                    <div className="reco-layout-container">
                        <header className="reco-layout-header">
                            <SuiteBar />
                        </header>
                        <div ref={layoutContentEl} className={["reco-layout-content-wrapper", siderExpaned ? "reco-layout-expaned-scroll" : "reco-layout-collapsed-scroll"].join(" ")} >
                            <main className="reco-layout-content" tabIndex= "-1">
                                {
                                    children
                                }
                            </main>
                            <div className="reco-layout-placeholder"></div>
                        </div>
                    </div>
                </div>
                {renderAboutOpusCard()}
                {/* <AvaChatDialog /> */}
                <NotificationMenu id="raNotificationMenu"/>
            </div>
        </Fragment>
    );
};

export default withRouter(Layout);