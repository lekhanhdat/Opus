import { bindEvents } from '../../Utilities/CommonUtil';

let onlyClose = false;
export default class VPAT extends React.Component {
    constructor(props) {
        super(props);
        bindEvents(this, 'onFocus');
    }
    componentDidMount() {
        // $$.messagedialog(false);
        $(".reco-layout-content").focus();
        $(".skip_navigation").on("focus", this.onFocus);
    }
    onFocus(e) {
        if (onlyClose) {
            onlyClose = false;}
        else {
            this.showTimoutMsg();
        } 
    }
    showTimoutMsg() {
        let args = {
            // classify: "warn",
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: RMResx.RM_Common_SkipNavigation,
            buttons: [
                { text: RMResx.RM_JS_Common_Cancel, onClick: this.onNoClick },
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.onYesClick },
                
            ],
            willClose: () => onlyClose = true,
        };
        $$.messagedialog(true, args);
    }
    onYesClick() {
        $$.messagedialog(false);
        setTimeout(() => {
            $(".reco-layout-content").focus();       
        }, 300);
    }
    onNoClick() {
        $$.messagedialog(false);
        $("#rmUserManager_Content").focus();
    }
    render() {
        return <div tabIndex="1" className="skip_navigation"></div>;
    }
}