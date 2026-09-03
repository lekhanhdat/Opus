//sso logout
(function () {
    document.querySelector("#sso_logout_form")?.submit();
})()


//Accept LA
var $btnAccept = $("#btnAccept");
var $btnReject = $("#btnReject");
$btnAccept.off("click").on("click", () => {
    $("#sso_la_form").submit();
});

$btnReject.off("click").on("click", () => {
    $("#sso_la_logout_form").submit();
});