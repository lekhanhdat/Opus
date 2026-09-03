import {bindEvents} from '../../Utilities/CommonUtil';
import {ElecStatusEnum} from "../BCM/Constants";

export default class NotificationMenu extends R.Component {
    idAttr = true;
    constructor(props) {
        super(props);
        this.state = {
            isOpenNotificationMenu: false,
            statusType: ''
        };

        bindEvents(this, 'closeNotificationMenu');
    }

    componentReceive(data, statusType) {
        let isOpenNotificationMenu = true;
        if (data == 'close') {
            isOpenNotificationMenu = false;
        }
        this.setState({
            content: data,
            isOpenNotificationMenu: isOpenNotificationMenu,
            statusType: statusType
        });
        if(isOpenNotificationMenu){
            setTimeout(()=>{
                this.closeNotificationMenu();
            },5000);
        }
    }

    closeNotificationMenu() {
        this.setState({
            isOpenNotificationMenu: false
        });
    }

    renderActionIcon() {
        let statusType = this.state.statusType;
        if (statusType == ElecStatusEnum.InProgress) {
            return <div className="inprogress-icon"></div>;
        }
        if (statusType == ElecStatusEnum.Failed || statusType == ElecStatusEnum.Exception) {
            return <div className="error-icon"></div>;
        }
        if (statusType == ElecStatusEnum.Completed) {
            return <div className="finished-icon"></div>;
        }
    }

    onActionMouseDown = (e) =>{
        e.stopPropagation();
    }

    renderNotificationMenu() {
        let isOpenNotificationMenu = this.state.isOpenNotificationMenu;
        return <div
            id="raNotificationMenu"
            style={{display: (isOpenNotificationMenu) ? 'block' : 'none'}}
            onMouseDown={this.onActionMouseDown}
        >
            <div className="close_btn" onClick={this.closeNotificationMenu} tabIndex="0">
                <div className="close_btn_img"></div>
            </div>
            <div className="jobMsg">
                <div className="left">
                    {this.renderActionIcon()}
                </div>
                {this.state.content}
            </div>
        </div>;
    }

    render() {
        return <React.Fragment>
            {this.renderNotificationMenu()}
        </React.Fragment>;
    }
}
