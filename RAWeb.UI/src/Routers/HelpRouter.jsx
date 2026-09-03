import RouteConfig from "../Components/Base/RouteConfig";

function getChildren() {
    let children = [];
    children.push(
         new RouteConfig(
            "InviteSupport",
            "",
            RMResx.RM_SuiteBar_Tips_Invite_Group,
        )
            .setShowInNav(true),
        new RouteConfig(
            "UserGuide",
            "",
            RMResx.RM_SuiteBar_Tips_Guide,
        )
            .setShowInNav(true),
        new RouteConfig(
            "AboutOpus",
            "",
            RMResx.RM_SuiteBar_Tips_About
        )
            .setShowInNav(true),
    );
    return children;
}

const HelpRouterConfig = new RouteConfig(
    "Help",
    "",
    RMResx.RM_Home_Help_Title,
    RMResx.RM_NavPanel_Group_System,
    ".fia-help-nav"
).addChildren(...getChildren());

export default HelpRouterConfig;