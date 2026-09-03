

export function showMsgBox(type, msg, btns, onClose) {
    if(btns) {
        btns = btns.map((btn) => {
            return {
                text: btn.name, 
                className: `button button-${!btn.isPrimary?"default":"primary"}`,
                onClick: () => {
                    if(btn.onClick) {
                        btn.onClick();
                    }
                    $$.messagedialog(false);
                }
            }
        });
    }
    $$.messagedialog(true, {
        type: type,
        width: '550px',
        hideActions: false,
        title: RMResx.RM_JS_Common_Confirmation,
        content: msg,
        buttons: btns,
        willClose: () => {
            if(onClose) {
                onClose();
            }
        }
    });
}