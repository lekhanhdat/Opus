import NotificationsContent from "./NotificationsContent";
import NotificationsRecordsList from "./NotificationsRecordsList";
import { StatusEnum } from "./Contains";

export default class Timer extends R.Component{
    constructor(props) {
        super(props);
        this.notificationCacheData = [];
    }

    updateNotificationTimer(jobId, actionText) {
        if ($(".rm-notification-content").children().length == 0) {
            this.notificationCacheData = [];
        }
        this.updateNotification(jobId, StatusEnum.InProgress, actionText);
        let timerCount = 0;
        let updateChangeTerm = setInterval(() => {
            ++timerCount;
            if (jobId) {
                let option = {
                    url: `/api/RecordsExplorerApi/GetRealTimeJobStatusInfo?jobId=${jobId}`,
                    method: "GET"
                };
                fetchUtility(option).then((res) => {
                    let result = JSON.parse(res);
                    let stopTimer = false;
                    if (timerCount == 60 * 10) {
                        stopTimer = true;
                    }
                    if (result.MessageType == 1) {
                        stopTimer = true;
                        this.updateNotification(jobId, StatusEnum.Failed, actionText, result.Items);
                    }else{
                        if(result.Status == 4 && result.Items){
                            stopTimer = true;
                            this.updateNotification(jobId, StatusEnum.Completed, actionText, result.Items);
                        }
                    }
                    if (stopTimer) {
                        clearInterval(updateChangeTerm);
                        this.props.callback();
                    }
                });
            }
        }, 1000);
    }

    deleteNotificationItem = (args) => {
        this.notificationCacheData = this.notificationCacheData.filter((item, idx) => {
            return args.index != idx;
        });
        let notificationsContent = NotificationsContent(this.notificationCacheData, this.deleteNotificationItem);
        this.dispatch('raNotification', notificationsContent);
    }

    updateNotification(jobId, status, actionText, checkedItems){
        let currentNotificationItem = this.notificationCacheData.find( item => item.jobId === jobId );
        let recordsItems = checkedItems || this.props.checkedItems.map(item => item.leafName);
        let notificationItem = {
            jobId: jobId,
            status: status,
            actionText: actionText,
            recordItems: recordsItems,
            startTime: new Date().toLocaleTimeString(),
            showTime: new Date().toLocaleDateString(),
        };
        if(currentNotificationItem){
            Object.assign(currentNotificationItem, notificationItem);
        }else{
            this.notificationCacheData.push(notificationItem); 
        }
        let notificationsContent = NotificationsContent(this.notificationCacheData, this.deleteNotificationItem);
        let notificationsMsgContent = NotificationsRecordsList(notificationItem);
        this.dispatch('rmSuiteBar');
        this.dispatch('raNotification', notificationsContent);
        this.dispatch('raNotificationMenu', notificationsMsgContent, status);
    }

    render(){
        return <div></div>;
    }
}