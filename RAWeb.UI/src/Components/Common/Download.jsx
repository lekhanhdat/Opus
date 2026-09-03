import { getRequestVerificationToken } from "../../Utilities/CommonUtil";

export default class Download extends R.Component {
    idAttr = true;
    
    componentReceive(downloadUrl,data) {
        this.downLoad(downloadUrl, data);
    }

    downLoad(downloadUrl, data) {
        let requestVerificationToken = getRequestVerificationToken();
        let divElement = document.getElementById(this.props.id);
        ReactDOM.render(
            <form action={downloadUrl} method='post'>
                <input name='RequestVerificationToken' type='text' value={requestVerificationToken} readOnly />
                {
                    data && data.map((item, index) => {
                        return <input key={index} name={item.name} type='text' value={item.value} readOnly />;
                    })
                }
            </form>,
            divElement
        );
        divElement.querySelector("form").submit();
        ReactDOM.unmountComponentAtNode(divElement);
    }

    render() {
        return <div id={this.props.id} style={{ display: "none" }}></div>;
    }
}
