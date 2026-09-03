const sessionManagement = {
    timeout: 30 * 1000,
    checkSessionResult: {
        Success: 1,
        SessionTimeout: 2,
        ForcedLogout: 3
    },
    init() {
        //check is session timeout while page loaded
        this.setSessionTimer();
    },
    setSessionTimer() {
        let that = this;
        setTimeout(() => {
            $.ajax({
                url: "/Account/CheckSession",
                async: true,
                cache: false,
                success: (data) => {
                    if(data == that.checkSessionResult.Success)
                    {
                        that.setSessionTimer();
                    }
                    else 
                    {
                        that.showSessionTipMsgBox(that.getCheckSessionResult(data));
                    }
                },
                error: (data) => {
                    console.log("Check session failed.");
                    that.setSessionTimer();
                    //that.showTimoutMsg();
                }
            });
        }, that.timeout);
    },
    getCheckSessionResult(sessionCheckResult)
    {
        if(sessionCheckResult == this.checkSessionResult.SessionTimeout)
        {
            return RMResx.RM_JS_Login_SessionTimeOut_Warn;
        }
        if(sessionCheckResult == this.checkSessionResult.ForcedLogout)
        {
            return RMResx.RM_JS_Login_ForcedLogout_Warn;
        }
        return "";
    },
    showSessionTipMsgBox(msg) {
        if(!msg) {return;}
        $$.messagedialog(true, {
            // classify: "warn",
            width: '550px',
            hideActions: false,
            title: RMResx.RM_JS_Common_Confirmation,
            content: <div tabIndex="0"><p id="rmSesionTImeOutWarn">{msg}</p></div>,
            buttons: [
                { text: RMResx.RM_JS_Common_OK, primary: true, classify: "theme", onClick: this.refreshPage }
            ],
            willClose: this.refreshPage
        });
    },
    refreshPage() {
        sessionStorage.clear();
        window.location.href = "/Account/LogOut";
    }
};

function initSessionManagement() {
    sessionManagement.init();
}

export { initSessionManagement };