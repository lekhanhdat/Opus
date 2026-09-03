import { useEffect, useState } from "react";
import { BrowserRouter, Switch, Route, Redirect } from "react-router-dom";
import RouteConfig from "../Components/Base/RouteConfig";
import Layout from "../Components/Layouts";
import { RoleType, HomePageType } from "../Constants/Constants";
import { LicenseHelper } from "../Utilities/CommonUtil";
import RouterUrls from "../Constants/RouterUrls";
import { checkPermission } from "../Utilities/permissionManager";
import RARoute from "../Components/Base/RARoute";
// import Dashboard from "../Components/Home/Dashboard";
import CPRouteConfig from "../Routers/CPRouter";
import RCRouterConfig from "../Routers/RCRouter";
import PRMRouterConfig from "../Routers/PRMRouter";
import JMRouterConfig from "../Routers/JMRouter";
import MTRouteConfig from "../Routers/MTRouter";
import MANRouteConfig from "../Routers/MANRouter";
import RERouteConfig from "../Routers/RERouter";
import CRRouteConfig from "../Routers/CRRouter";
import DCRouterConfig from "./DCRouter";
import MLRouterConfig from "./MLRouter";
import ArchiveRCRouterConfig from "./ArchiveRCRouter";
import DBRouteConfigs from "./DBRouter";
import FileAnalysisRouterConfig from "./AnalysisRouter";
import HelpRouterConfig from "./HelpRouter";
import LocationsRouterConfig from "./LocationsRouter";
import TemplatesRouterConfig from "./TemplatesRouter";

const activeNavConfigs = [
    {
        type: HomePageType.OpusAll,
        navId: "Home",
        expandNavId: "",
        AdditionalRouteConfigs: DBRouteConfigs,
        bottomRoute: <Route path="*"><Redirect to="/"/></Route>
    },
    {
        type: HomePageType.OpusSOOnly,
        navId: "RDM_RuleManagement",
        expandNavId: "Manage",
        AdditionalRouteConfigs: [],
        bottomRoute: <Route path="*"><Redirect to={RouterUrls.RDM_RuleManagement}/></Route>
    },
    {
        type: HomePageType.OpusDiscoveryOnly,
        navId: "FA_Discovery",
        expandNavId: "discovery",
        AdditionalRouteConfigs: [],
        bottomRoute: <Route path="*"><Redirect to={RouterUrls.FA_Discovery}/></Route>
    },
    {
        type: HomePageType.RestoreCenterOnly,
        navId: "AR_Restore",
        expandNavId: "restore",
        AdditionalRouteConfigs: [],
        bottomRoute: <Route path="*"><Redirect to={RouterUrls.Archiver_RestoreCenter}/></Route>
    },
    {
        type: HomePageType.HoldOnly,
        navId: "Hold_Management",
        expandNavId: "hold",
        AdditionalRouteConfigs: [],
        bottomRoute: <Route path="*"><Redirect to={RouterUrls.PRM_HybridSearch}/></Route>
    },
];

let allRouterConfigs = [
    RERouteConfig,
    MTRouteConfig,
    FileAnalysisRouterConfig,
    MANRouteConfig,
    CRRouteConfig,
    RCRouterConfig,
    MLRouterConfig,
    PRMRouterConfig,
    LocationsRouterConfig,
    TemplatesRouterConfig,
    ArchiveRCRouterConfig,
    DCRouterConfig,
    JMRouterConfig,
    CPRouteConfig,
    HelpRouterConfig,
];

var item = InitActiveNavInfo();
const navItems = splitAllNavItems(allRouterConfigs);
const allRouters = splitAllRouters(allRouterConfigs);

function RootRouter() {
    
    let [activeNavId, setActiveNav] = useState(item.navId);
    return (
        <BrowserRouter getUserConfirmation={getConfirmation}>
            <Layout navItems={navItems} activeNav={setActiveNav} activeId={activeNavId}>
                <Switch>
                    {allRouters.map((rc, idx) => {
                        return (
                            <RARoute
                                exact={rc.exact}
                                path={rc.url}
                                key={idx}
                                setActiveNav={setActiveNav}
                                routeConfig={rc}
                            />
                        );
                    })}
                    {item.bottomRoute}
                </Switch>
            </Layout>
        </BrowserRouter>
    );


    function getConfirmation(message, callback) {
        if (RARoute.PromptMsg == message) {
            callback(true);
        } else {
            $$.messagedialog(true, {
                // classify: "info",
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: message,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_Cancel,
                        onClick: () => {
                            $$.messagedialog(false);
                            callback(false);
                        },
                    },
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: () => {
                            $$.messagedialog(false);
                            callback(true);
                        },
                    },
                ],
            });
        }
    }
}



function splitAllRouters(routerConfigs) {
    let routers = [];
    for (const routeCfg of routerConfigs) {
        if (routeCfg.component) {
            routers.push(routeCfg);
        }
        for (const child of routeCfg.children) {
            if (child.component) {
                routers.push(child);
            }
        }
    }
    return routers;
}

function splitAllNavItems(routerConfigs) {
    let isStandardUser = RM.RoleType == RoleType.StandardUser;
    let navList = null;
    navList = splitNormalNavItems(routerConfigs);

    return filterNavItems(navList);
}

function splitNormalNavItems(routerConfigs) {
    let navList = [];
    for (const routeCfg of routerConfigs) {
        if (routeCfg.showInNav) {
            navList.push(routeCfg.cloneNavObject());
        }
    }
    for(let navItem of navList){
        if(navItem.children.length > 0){
            navItem.url = "";
        }
    }
    return navList;
}

function filterNavItems(navList) {
    let filteredNavs = [];
    for (const navItem of navList) {
        if (navItem.children && navItem.children.length > 0) {
            let filteredChildren = [];
            for (const child of navItem.children) {
                if (checkPermission(child.url)) {
                    filteredChildren.push(child);
                }
            }
            if (filteredChildren.length > 0) {
                navItem.children = filteredChildren;
                filteredNavs.push(navItem);
            }
        } else if (checkPermission(navItem.url)) {
            filteredNavs.push(navItem);
        }
    }
    return filteredNavs;
}

function InitActiveNavInfo()
{
    let isSOAdmin = checkPermission("Archiver_CP_Schedule_Settings", RM.UserResources);
    let homePageType = LicenseHelper.HasDiscoveryLicenseOnly() ? HomePageType.OpusDiscoveryOnly : (LicenseHelper.HasOpusSOLicenseOnly() && !isSOAdmin) ? HomePageType.OpusSOOnly : HomePageType.OpusAll;
    if(LicenseHelper.HasDiscoveryLicense() && LicenseHelper.HasOpusILLicense() && !LicenseHelper.HasOpusSOLicense()){
        if(!LicenseHelper.EnableRecordsArchiver()){
            homePageType = HomePageType.OpusAll;
        }else{
            homePageType = HomePageType.OpusDiscoveryOnly;
        }
    }
    if (homePageType != HomePageType.OpusDiscoveryOnly && checkPermission(RouterUrls.Archiver_RestoreCenter) && !checkPermission(RouterUrls.Home)) {

        homePageType = HomePageType.RestoreCenterOnly;
    }
    else if (!isSOAdmin && !LicenseHelper.HasOpusILLicense() && LicenseHelper.HasOpusGoogleAndSOLicense() && !checkPermission(RouterUrls.BCM_ContentRepositoryManagement_GoogleDrive)) {
        if (checkPermission(RouterUrls.Home)) {
            homePageType = HomePageType.OpusAll;
        } else {
            homePageType = HomePageType.OpusSOOnly;
        }
    }

    if(RM.RoleType === RoleType.ManageHoldUser && checkPermission(RouterUrls.PRM_HybridSearch)){
        homePageType = HomePageType.HoldOnly;
    }

    let item = activeNavConfigs.find(o => o.type == homePageType);
    if(item.expandNavId)
    {
        ExpandSpecifiedNav(item.expandNavId);
    }

    if(item.AdditionalRouteConfigs)
    {
        allRouterConfigs = [...item.AdditionalRouteConfigs, ...allRouterConfigs];
    }
    return item;
}

function ExpandSpecifiedNav(navId)
{
    var item = allRouterConfigs.find(o => o.navId == navId);
    if(item)
    {
        item.expand = true;
    }
}

export default RootRouter;