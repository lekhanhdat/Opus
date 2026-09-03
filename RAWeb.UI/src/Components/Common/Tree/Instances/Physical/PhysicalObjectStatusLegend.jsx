import { Component } from "react";
import { PhysicalObjectStatus } from "../../../../../Constants/Constants";

export const PhysicalObjectStatusColor = {
    Open: "#28CC74",
    Destroyed: "#F7941D",
    Closed: "#686868",
    Missing: "#EF0500",
    Loaned: "#0080FF"
};

export const PhysicalObjectStatusIcon = {
    Inherit: "fia-radiobutton-bg-device",
    BreakInherit: "fia-lock"
};

let getIconScaleStyle = function(){
    //本方法为了解决浏览器设置最小font的问题
    let remFontSizeNum = window.getComputedStyle(document.documentElement)["fontSize"].replace("px","")*1;
    let iconScaleNum = (7 / remFontSizeNum).toFixed(2);
    let iconScaleStyle = {transform: `scale(${iconScaleNum})`};
    return iconScaleStyle;
};

export const PhysicalObjectStatusInherit = [
    { name: RMResx.RM_PRM_PRE_Column_Status_Open, color: PhysicalObjectStatusColor.Open, iconClass: PhysicalObjectStatusIcon.Inherit, statusKey: PhysicalObjectStatus.Open, iconScaleStyle: getIconScaleStyle()},
    { name: RMResx.RM_PRM_PRE_Column_Status_Closed, color: PhysicalObjectStatusColor.Closed, iconClass: PhysicalObjectStatusIcon.Inherit, statusKey: PhysicalObjectStatus.Closed, iconScaleStyle:  getIconScaleStyle()},
    { name: RMResx.RM_PRM_PRE_Column_Status_Missing, color: PhysicalObjectStatusColor.Missing, iconClass: PhysicalObjectStatusIcon.Inherit, statusKey: PhysicalObjectStatus.Missing ,iconScaleStyle:  getIconScaleStyle()},
    { name: RMResx.RM_PRM_PRE_Column_Status_Destroyed, color: PhysicalObjectStatusColor.Destroyed, iconClass: PhysicalObjectStatusIcon.Inherit, statusKey: PhysicalObjectStatus.Destroyed ,iconScaleStyle: getIconScaleStyle()},
    { name: RMResx.RM_PRM_PRE_Column_Status_Loaned, color: PhysicalObjectStatusColor.Loaned, iconClass: PhysicalObjectStatusIcon.Inherit, statusKey: 'loaned', iconScaleStyle: getIconScaleStyle() },
];

export const PhysicalObjectStatusBreakInherit = [
    { name: RMResx.RM_PRM_PRE_PermissionEditedAndOpen, color: PhysicalObjectStatusColor.Open, iconClass: PhysicalObjectStatusIcon.BreakInherit, statusKey: PhysicalObjectStatus.Open, iconScaleStyle: getIconScaleStyle()},
    { name: RMResx.RM_PRM_PRE_PermissionEditedAndClosed, color: PhysicalObjectStatusColor.Closed, iconClass: PhysicalObjectStatusIcon.BreakInherit, statusKey: PhysicalObjectStatus.Closed, iconScaleStyle: getIconScaleStyle() },
    { name: RMResx.RM_PRM_PRE_PermissionEditedAndMissing, color: PhysicalObjectStatusColor.Missing, iconClass:  PhysicalObjectStatusIcon.BreakInherit, statusKey: PhysicalObjectStatus.Missing, iconScaleStyle: getIconScaleStyle()},
    { name: RMResx.RM_PRM_PRE_PermissionEditedAndDestroyed, color: PhysicalObjectStatusColor.Destroyed, iconClass: PhysicalObjectStatusIcon.BreakInherit, statusKey: PhysicalObjectStatus.Destroyed, iconScaleStyle: getIconScaleStyle() },
    { name: RMResx.RM_PRM_PRE_PermissionEditedAndLoaned, color: PhysicalObjectStatusColor.Loaned, iconClass: PhysicalObjectStatusIcon.BreakInherit,statusKey: 'loaned', iconScaleStyle: getIconScaleStyle()},
];

class PhysicalObjectStatusLegend extends Component {
    constructor(props) {
        super(props);
        this.statusIconInfo = [...PhysicalObjectStatusInherit,...PhysicalObjectStatusBreakInherit];
        this.state = {
            show: false
        };
    }
    componentDidMount () {
        document.addEventListener('click', this.hidePopup);
    }

    componentWillUnmount () {
        document.removeEventListener('click', this.hidePopup);
    }

    hidePopup = (e) => {
        this.setState({
            show: false,
        });
    }

    onPopupClick = (e) => {
        e.nativeEvent.stopImmediatePropagation();
    }

    onTriggerClick = (e) => {
        this.setState({ show: !this.state.show });
        e.nativeEvent.stopImmediatePropagation();
    }

    onTriggerKeyDown = (e) => {
        if (e.keyCode == 13) {
            this.onTriggerClick(e);
        }
    }

    render () {
        return <React.Fragment>
            <div className={"ra-tree-status"}>
                <div
                    tabIndex={0}
                    className={"ra-tree-status-trigger"}
                    onClick={this.onTriggerClick} onKeyDown={this.onTriggerKeyDown}>
                    {RMResx.RM_PRM_PRE_Status_Legend}
                </div>
                {this.state.show &&
                    <div className={"ra-tree-status-list ra-box-shadow"} onClick={this.onPopupClick}>
                        {this.statusIconInfo.map((sc, idx) => {
                            return <div key={idx} className={"ra-tree-status-item"} tabIndex="0">
                                <div className={sc.iconClass} style={{color: sc.color, ...sc.iconScaleStyle}}></div>
                                <div className="margin-left-s">{sc.name}</div>
                            </div>;
                        })}
                    </div>
                }
            </div>
        </React.Fragment>;
    }
}

export default PhysicalObjectStatusLegend;