"use strict";

var connection = null;
var user = null;
function addMessage(user, msg) {
    let chatwindow = document.getElementById("chat-content");
    chatwindow.contentDocument.write(user + ": " + msg);
}
function connect() {
    user = document.getElementById("chat-content").value;
    addMessage(user, "Connecting...");

    let serverUrl = (document.location.protocol === "https:" ? "wss" : ws) + "://" + document.location.hostname + ":6502";

    connection = new WebSocket(serverUrl, "json");
}

function onsend() {
}