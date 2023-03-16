const options = {
    hour: '2-digit',
    minute: '2-digit',
    second: '2-digit',
    day: '2-digit',
    month: '2-digit',
    year: 'numeric'
};

var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

function send(url, data, callback) {
    const securityToken = $('input[name="__RequestVerificationToken"]').val();
    $.ajax({
        url: url,
        data: data,
        headers: { "RequestVerificationToken": securityToken },
        cache: false,
        processData: false,
        type: 'POST',
        success: function (result) {
            callback(result);
        },
        error: function (err) {
            console.error(err.toString());
        }
    });
}