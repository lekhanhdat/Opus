export class MessageBox {
    static show(content, okCallback) {
        $$.messagedialog(true,
            {
                width: "550px",
                hideActions: false,
                title: RMResx.RM_JS_Common_Confirmation,
                content: content,
                buttons: [
                    {
                        text: RMResx.RM_JS_Common_Cancel,
                        onClick: () => {
                            $$.messagedialog(false);
                        },
                    },
                    {
                        text: RMResx.RM_JS_Common_OK,
                        primary: true,
                        classify: "theme",
                        onClick: okCallback,
                    },
                ],
            }
        );
    }
}
