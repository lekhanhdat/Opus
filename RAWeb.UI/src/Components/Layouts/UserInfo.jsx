
export default class UserInfo extends R.Component {
    constructor(props) {
        super(props);
    }
    
    render() {
        return (
            <div>
                <div 
                    id="rmUserOptions"
                    data-tooltip="diffneed"
                    aria-label={RM.Encoding.htmlDecode(RM.gData.userName)}
                    tabIndex='0'
                >
                    <R.Avatar name={RM.gData.userName} tooltip={true} shape="square"/>
                </div>
                <R.Popup
                    id="raUserInfoPopup"
                    of="#rmUserOptions"
                    width={200}
                    height={42}
                    triggerEvent="click"
                    global={true}
                >
                    <a href="/Account/LogOut" onClick={()=>{sessionStorage.clear();} }>
                        <div className="ra-suitbar-popupContent">
                            {RMResx.RM_LogOut}
                        </div>
                    </a>
                </R.Popup>
            </div>
        );
    }
}