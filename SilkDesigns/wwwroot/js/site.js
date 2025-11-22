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

// --- statusAnchor helpers ---
function findStatusAnchor() {
    // Preferred: ID. If you can modify the view, add id="statusAnchor"
    var selectors = [
        "#statusAnchor",            // preferred
        "a.status-anchor",          // class fallback
        "a[data-status]",           // data-attribute fallback
        "a[data-role='status']",    // alternate data-attr
        "a[name='statusAnchor']",   // name fallback
        "a[data-status-anchor]",    // another possible attr
        "a[href*='/Status']"        // href pattern fallback (use carefully)
    ];
    for (var i = 0; i < selectors.length; i++) {
        var el = document.querySelector(selectors[i]);
        if (el) return el;
    }
    return null;
}

function getStatusAnchorHref() {
    var el = findStatusAnchor();
    if (!el) {
        console.warn("statusAnchor not found — skipping behavior that requires it.");
        return null;
    }
    return el.getAttribute("href");
}

// Example safe usage inside jQuery ready:
$(function () {
    var statusHref = getStatusAnchorHref();
    if (statusHref) {
        // safe to use statusHref
        console.log("Found status anchor href:", statusHref);
    }
});


