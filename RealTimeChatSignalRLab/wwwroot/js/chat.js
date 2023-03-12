//"use strict";

//var connection = new signalR.HubConnectionBuilder().withUrl("/chat").build();

//connection.start().then(function () {
//    document.getElementById("sendButton").disable = false;
//}
//).catch(function (err) {
//    return console.error(err.toString());
//});

//connection.on("ReceiveMessage", function (user, message) {
//    var li = document.createElement("li");
//    document.getElementById("messageList").appendChild(li);
//    li.textContent = user + " says " + message;
//});

//document.getElementById("sendButton").addEventListener("click", function (event) {
//    //var user = document.getElementById("userInput").value;
//    var message = document.getElementById("messageInput").value;
//    connection.invoke("SendMessageToUser", "", message)
//    .catch(function (err) {
//        return console.error(err.toString());
//    });
//    event.preventDefault();
//});