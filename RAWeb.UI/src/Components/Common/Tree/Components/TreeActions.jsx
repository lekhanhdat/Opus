import { createPortal } from 'react-dom';
import { bindEvents } from '../../../../Utilities/CommonUtil';

class TreeContextMenuTrigger extends React.Component {
    constructor(props) {
        super(props);
        bindEvents(this, "onNodeContentMenu", "setContextMenuRef");
    }

    onNodeContentMenu(e) {
        e.preventDefault();
        if (this.props.itemComponent) {
            let refMenu = this.props.itemComponent.contextMenu;
            let shown = refMenu && refMenu.state && refMenu.state.showAtions;
            if (refMenu && e.button == 2 && !shown) {
                refMenu.showMenu();
            }
        }
        return false;
    }

    render() {
        return (
            <div
                className={"ra-tree-menu-trigger"}
                onContextMenu={this.onNodeContentMenu} >
                {this.props.children}
            </div>
        );
    }
}

class TreeActionsPopup extends React.Component {
    constructor(props) {
        super(props);

        bindEvents(this, "onPopupClick", "onMenuBtnClick", "onMenuBtnKeyDown", "onMenuBtnFocus",
            "onMenuBtnBlur", "calculatePosition", "showMenu", "hideMenu", "showPopupPopover", "hidePopupPopover");
        this.popopPostion = {
            top: "0",
            left: "0"
        };

        if (props.itemComponent) {
            props.itemComponent.contextMenu = this;
        }
        
        this.initPopupContainer();

        this.state = {
            showAtions: false
        };
    }

    componentDidMount() {
        window.addEventListener('scroll', this.hideMenu, true);
    }

    componentWillUnmount() {
        this.hidePopupPopover();
        this.popupContainer.removeChild(this.popupElement);
        window.removeEventListener('scroll', this.hideMenu, true);
    }

    UNSAFE_componentWillReceiveProps(nextProps) {
        
    }

    initPopupContainer() {
        const doc = window.document;
        let container = doc.getElementById('raTreeActionsPopups');
        if(!container) {
            container = doc.createElement('div');
            container.id = 'raTreeActionsPopups';
            doc.body.appendChild(container);
        } 
        this.popupElement = doc.createElement('div');
        this.popupElement.setAttribute('popover', 'manual');
        this.popupElement.className = 'ra-tree-actions-popover';
        container.appendChild(this.popupElement);
        this.popupContainer = container;
    }

    showPopupPopover() {
        if (this.popupElement && this.popupElement.showPopover && !this.popupElement.matches(':popover-open')) {
            this.popupElement.showPopover();
        }
    }

    hidePopupPopover() {
        if (this.popupElement && this.popupElement.hidePopover && this.popupElement.matches(':popover-open')) {
            this.popupElement.hidePopover();
        }
    }

    onPopupClick(e) {
        e.stopPropagation();
    }

    onMenuBtnClick(e) {
        e.stopPropagation();
        if (!this.state.showAtions) {
            this.showMenu();
        } else {
            this.hideMenu();
        }
    }

    onMenuBtnKeyDown(e) {
        if (e.keyCode == 13) {
            this.onMenuBtnClick(e);
        }
    }

    onMenuBtnFocus(e) {
        $(this.showActionsBtn).closest(".ra-tree-menu-trigger").addClass("ra-tree-menu-iconFocus");
    }

    onMenuBtnBlur(e) {
        $(this.showActionsBtn).closest(".ra-tree-menu-trigger").removeClass("ra-tree-menu-iconFocus");
    }

    showMenu() {
        if (this.props.disabled) {
            return;
        }
        this.setState({ showAtions: true }, this.showPopupPopover);
        $(this.showActionsBtn).closest(".ra-tree-menu-trigger").addClass("ra-tree-menu-shown");
        let popup = this.popupElement;
        setTimeout(()=>{
            this.showPopupPopover();
            let windowHeight = $(window).height(); 
            let actionsMenuPopupHeight = ($(".ra-tree-menu-list").height() * -1 + 18) ;
            let offsetBottom = windowHeight - $(this.showActionsBtn).closest(".ra-tree-menu-trigger").offset().top;
            if( offsetBottom < $(".ra-tree-menu-list").height() + 20){
                $(".ra-tree-menu-list").css("top",actionsMenuPopupHeight + "px");
            }
        },0);
        setTimeout(() => {
            $(popup).find(".ra-tree-menu-item").each((idx, item) => {
                if (!$(item).hasClass("ra-tree-menu-disabled")) {
                    item.focus();
                    return false;
                }
            });
        }, 100);
    }

    hideMenu() {
        this.hidePopupPopover();
        this.setState({ showAtions: false });
        $(this.showActionsBtn).closest(".ra-tree-menu-trigger").removeClass("ra-tree-menu-shown");
    }

    calculatePosition() {
        if (this.state.showAtions) {
            let $node = $(this.showActionsBtn).closest(".ra-tree-menu-trigger");
            let nodeOffset = $node.offset();
            let nodeWidth = $node.outerWidth();
            //ra-tree-node-content元素距离.ra-tree左边的距离，16是expand按钮所占的宽度
            let leftOtherWidth = parseInt($node.closest(".ra-tree-node").css("padding-left").replace("px", ""), 10) + 16;
            let $treeview = $node.closest(".ra-treeview");
            let treeviewElement = $treeview[0];
            let hasHorizontalScroll = treeviewElement.scrollWidth > treeviewElement.clientWidth;
            let treeviewOffset = $treeview.offset();
            let treeviewWidth = $treeview.width();

            this.popopPostion.top = (nodeOffset.top + ($node.outerHeight() / 2)) + "px";
            // if (hasHorizontalScroll && nodeWidth + leftOtherWidth >= treeviewWidth) {
            //     let hasVerticalScroll = treeviewElement.scrollHeight > treeviewElement.clientHeight;
            //     let tempLeft = treeviewOffset.left + treeviewWidth;
            //     if (hasVerticalScroll) {
            //         tempLeft -= 20; //滚动条按20px宽计算
            //     }
            //     this.popopPostion.left = tempLeft + "px";
            // } else {
            
            // nodeWidth > (treeviewWidth + 320 - nodeOffset.left): Terry's code
            if (this.props.recalculatePosition && nodeWidth > (treeviewWidth + 320 - nodeOffset.left)) {
                this.popopPostion.left = (treeviewWidth + 285) + "px";
            } else {
                this.popopPostion.left = (nodeOffset.left + nodeWidth) + "px";
            }
            // }
        }
    }

    renderActionItems(node, treeContext) {
        if (!node) {
            return null;
        } else if (node.type == TreeActionItem) {
            return React.cloneElement(node, {
                treeContext: treeContext,
                contextMenu: this,
            });
        } else if (node.props && node.props.children) {
            return React.Children.map(node.props.children, (child, i) => {
                return this.renderActionItems(child, treeContext);
            });
        } else {
            return node;
        }
    }

    render() {
        if(!this.props.itemComponent || !this.props.itemComponent.props.item.enableContextMenu 
            || this.props.treeContext.readonly) {
            return null;
        }
        this.calculatePosition();
        return <React.Fragment>
            {!this.props.disabled &&
                <div
                    className="ra-tree-menu-expand" tabIndex="0" aria-label={RMResx.RM_DAM_ActionDropdown}
                    ref={r => this.showActionsBtn = r}
                    onClick={this.onMenuBtnClick} onKeyDown={this.onMenuBtnKeyDown}
                    onFocus={this.onMenuBtnFocus} onBlur={this.onMenuBtnBlur}>
                    <div className={"ra-tree-menu-expand-icon fia-triangle-down"}></div>
                </div>
            }
            
            {!this.props.disabled && this.state.showAtions && createPortal(
                <div
                    style={{ ...this.popopPostion }}
                    className="ra-tree-menu-popup"
                    onClick={this.onPopupClick}
                    title="">
                    <ul className="ra-tree-menu-list">
                        {this.renderActionItems(this, this.props.treeContext)}
                    </ul>
                    <div className="ra-tree-menu-triangle-wrap">
                        <span className="ra-tree-menu-triangle-border"></span>
                        <span className="ra-tree-menu-triangle"></span>
                    </div>
                </div>,
                this.popupElement)
            }
        </React.Fragment>;
    }
}

class TreeActionItem extends React.Component {
    constructor(props) {
        super(props);
        bindEvents(this, "onActionClick", "onActionKeyUp", "onActionBlur");
    }

    onActionClick(e) {
        if (!this.props.disabled) {
            if (!this.props.onActionClick(e)) {
                this.props.contextMenu.hideMenu();
            }
        }
        e.stopPropagation();
    }

    onActionKeyUp(e) {
        if (e.keyCode == 13) {
            this.onActionClick(e);
        }
        else if (e.keyCode == 9) {
            let hasEnableItem = false;
            if (e.shiftKey) {
                $(e.target).prevAll().each((idx, item) => {
                    if (!$(item).hasClass("ra-tree-menu-disabled")) {
                        hasEnableItem = true;
                        return false;
                    }
                });
            } else {
                $(e.target).nextAll().each((idx, item) => {
                    if (!$(item).hasClass("ra-tree-menu-disabled")) {
                        hasEnableItem = true;
                        return false;
                    }
                });
            }
            let showActionsBtn = this.props.contextMenu.showActionsBtn;
            if (!hasEnableItem && showActionsBtn) {
                e.preventDefault();
                $(showActionsBtn).focus();
            }
        }
    }

    onDoubleClick(e) {
        e.stopPropagation();
    }

    onActionMouseDown(e){
        e.stopPropagation();
    }

    onActionBlur(e) {
        setTimeout(() => {
            let activeElement = $(document.activeElement).closest(".ra-tree-menu-popup");
            if (activeElement.length == 0 || activeElement[0] != $(this.actionElement).closest(".ra-tree-menu-popup")[0]) {
                this.props.contextMenu.hideMenu();
            }
        }, 200);
    }

    render() {
        let disabled = this.props.disabled === true;
        let actionClass = this.props.actionClass ? ` ${this.props.actionClass}` : ""; //actionClass用于QA自动化区分dom使用。
        let liProps = { className: "ra-tree-menu-item" + actionClass};
        if (disabled) {
            liProps.className += " ra-tree-menu-disabled";
        } else {
            liProps.tabIndex = "0";
        }
        return <React.Fragment>
            <li {...liProps} data-tooltip aria-label={this.props.text} ref={r => this.actionElement=r}
                onClick={this.onActionClick} onKeyDown={this.onActionKeyUp} onDoubleClick={this.onDoubleClick}
                onBlur={this.onActionBlur} onMouseDown={this.onActionMouseDown} role="button">
                <span className={"ra-tree-menu-icon " + this.props.iconClass}></span>
                <span className="ra-tree-menu-text">{this.props.text}</span>
            </li>
        </React.Fragment>;
    }
}

export { TreeContextMenuTrigger, TreeActionsPopup, TreeActionItem };