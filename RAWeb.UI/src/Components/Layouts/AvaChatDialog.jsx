import { useState, useEffect, useRef } from 'react'
import { useSelector } from "react-redux";
import './index.less'
// import { FloatingChatDialog  } from "@gui/chat-dialog";
import { getTerm, initialize, ProductType } from '@gui/common-i18n-terms';

const supportLanguages = ['en-US', 'fr-FR', 'ja-JP', 'zh-CN', 'ko-KR'];

const AvaChatDialog = () => {

    const [isFirstMeetAva, setIsFirstMeetAva] = React.useState(!localStorage.getItem('isFirstMeetAva'));

    const lastRequestIdRef = useRef("");

    const externalActionRequest = useSelector(
        (state) => state.avaDialog?.externalActionRequest || null
    );

    useEffect(() => {
        initialize();
    }, []);

    useEffect(() => {
        const requestId = externalActionRequest?.id;
        if (!requestId) {
            return;
        }

        if (lastRequestIdRef.current === requestId) {
            return;
        }

        lastRequestIdRef.current = requestId;
    }, [externalActionRequest]);

    if (RM.gData.diableChatBot || !RM.gData.chatBotApiURL) {
        return (<></>);
    }
    const options = {
        serviceUrl: RM.gData.chatBotApiURL,
        portalUrl: RM.gData.chatBotPortalURL,
        cdnUrl: RM.gData.resCdnURL,
        userId: RM.gData.userId,
        userName: RM.gData.emailAddress,
        productType: ProductType.AvePointRecords,
        getToken: async () => {
            let data = await fetchUtility({
                url: `/api/HomeApi/GetChatBotToken`,
                method: "POST",    
                data: {Token: RM.gData.accessToken}
            });
            if (data.opus) {
                RM.gData.accessToken = data.opus;
            }
            return Promise.resolve(data.chatbot);
        },
        privacyPolicyUrl: 'https://www.avepoint.com/privacy-policy', // Optional: Privacy agency link
        // sendIcon: <R.Icon icon="fia-paper-plane" />,
        // stopIcon: <R.Icon icon="fia-stop-square" />,
        inputLimit: 4000,
        feedbackInputLimit: 4000,
        floatingLayerIndex: 1000,
        floatingPanelWidth: 440,
        setHasUsed: ()=> {
            localStorage.setItem('isFirstMeetAva', '1');
            setIsFirstMeetAva(false);
        }
        // defaultContent: <div className="message completion-message">{getTerm('I18NShared_AVAbot_hello_message')}</div>,
        // contentBelowInput: (
        //     <div tabIndex={0} style={{ textAlign: "center", fontSize: "12px" }}>
        //         <span style={{ marginRight: 2, fontSize: 12 }}>{getTerm("I18NShared_AVAbot_can-make-mistake")}</span>
        //         <a tabIndex={0} target="_blank" rel="noopener noreferrer" className="chat-link" href="https://www.avepoint.com/company/privacy-notice">
        //             {getTerm("I18NShared_AVAbot_acceptable-use")}
        //         </a>
        //     </div>
        // ),
    };
    return (
        <div className="ava-chat-dialog-wrapper">
            {/* <FloatingChatDialog 
                options={options} 
                externalActionRequest={externalActionRequest}
                buttonOptions={{
                    isNewUserFirstMeetAva: !RM.gData.existAVAUser,
                    isFirstMeetAva: isFirstMeetAva,
                    onRenderContainer: (dom, useOptions) => {
						const hideTooltip = useOptions?.isOpenPanel || isFirstMeetAva;
                        return <div 
                            className="ava-chat-dialog-container" 
                            data-tooltip="true"
                            data-tooltip-wrap="force"
                            aria-label={hideTooltip ? "" : useOptions.i18nStrings?.askAvaTooltip}
                            >{dom}</div>
					},
                }}
            /> */}
        </div>
    )
}

export default AvaChatDialog