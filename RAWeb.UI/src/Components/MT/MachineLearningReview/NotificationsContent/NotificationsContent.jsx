import {ProgressStatus, ProgressStatusIcon} from "./Contains";
import "./Index.less";

const NotificationsContent = (notifications, callback) =>{

    const deleteItem = (index) => {
        callback({
            type: "DELETE",
            index: index
        });
    };

    return <div className="raNotifications">
        {
            RM.deepcopy(notifications).map((item, index)=>{
                return <div key={index}>
                    <div className="ra-notifications-space"></div>
                    <div className="ra-notifications-title">
                        <div className="ra-notifications-title-info">{`${item.actionText} ${ProgressStatus[item.status]}`}</div>
                        <div className='fia-searchbox-close' onClick={()=>{ deleteItem(index); }}></div>
                    </div>
                    <div className="ra-notifications-file-list">
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
                    <div className="ra-notifications-foot"> 
                        <div className={ProgressStatusIcon[item.status]}></div>
                        <div className="ra-notifications-time">{item.showTime} {item.startTime}</div>
                    </div>
                </div>;
            })
        }
    </div>;
};

export default NotificationsContent;