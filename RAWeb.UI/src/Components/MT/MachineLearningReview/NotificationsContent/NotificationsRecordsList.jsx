import {ProgressStatus} from "./Contains";

const NotificationsRecordsList = (item) => {
    return <div className="right">
        <div className="nTitle">
            <div>{`${item.actionText} ${ProgressStatus[item.status]}`}</div>
        </div>
        <div className="nBody">
            {
                item.recordItems.slice(0, 3).map((recordName, index)=>{
                    return <div className="ra-notifications-file-item" key={index}>
                        {recordName}
                    </div>;
                })
            }
            {
                item.recordItems.length > 3 && <div className="ra-notifications-file-item">
                    {`...(${item.recordItems.length - 3})`}
                </div>
            }
        </div>
    </div>;
};

export default NotificationsRecordsList;