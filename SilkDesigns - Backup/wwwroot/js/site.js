// Please see documentation at https://docs.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
var keepSessionAlive = false;
var keepSessionAliveUrl = null;

function SetupSessionUpdater(actionUrl) {
    keepSessionAliveUrl = actionUrl;
    var container = $("#body");
    container.mousemove(function () { keepSessionAlive = true; });
    container.keydown(function () { keepSessionAlive = true; });
    CheckToKeepSessionAlive();
}

function CheckToKeepSessionAlive() {
    setTimeout("KeepSessionAlive()", 5 * 60 * 1000);
}

function KeepSessionAlive() {
    if (keepSessionAlive && keepSessionAliveUrl != null) {
        $.ajax({
            type: "POST",
            url: keepSessionAliveUrl,
            success: function () { keepSessionAlive = false; }
        });
    }
    CheckToKeepSessionAlive();
}


