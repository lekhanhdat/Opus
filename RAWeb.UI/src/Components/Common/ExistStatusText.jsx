const Status = new Map([
    [ "Info", "ra-info-color"],
    [ "Success", "ra-success-color" ],
    [ "Warn", "ra-warn-color" ],
    [ "Error", "ra-error-color" ],
    [ "Disabled", "ra-disabled-color"],
]);

const ExistStatusText = ({status, name}) =>{
    const iconClass = `fia-radiobutton-bg-device ${Status.get(status)}`;
    return  <div className="ra-exist-status-text">
        <div className={iconClass}></div>
        <div className="margin-left-xs">{name}</div>
    </div>;
};

export default ExistStatusText;