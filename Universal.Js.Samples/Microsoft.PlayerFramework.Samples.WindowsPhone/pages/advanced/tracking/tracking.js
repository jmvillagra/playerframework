﻿(function () {
    "use strict";

    var mediaPlayer = null;

    WinJS.UI.Pages.define("/pages/advanced/tracking/tracking.html", {
        // This function is called whenever a user navigates to this page.
        // It populates the page with data and initializes the media player control.
        ready: function (element, options) {
            var item = options && options.item ? Data.resolveItemReference(options.item) : Data.items.getAt(0);
            element.querySelector(".titlearea .pagetitle").textContent = item.title;
            if (WinJS.Utilities.isPhone) {
                document.getElementById("header").style.display = "none";
            }

            var mediaPlayerElement = element.querySelector("[data-win-control='PlayerFramework.MediaPlayer']");
            mediaPlayer = mediaPlayerElement.winControl;
            mediaPlayer.playTimeTrackingPlugin.addEventListener("eventtracked", onPlayTimeEventTracked, false);
            mediaPlayer.positionTrackingPlugin.addEventListener("eventtracked", onPositionEventTracked, false);
            mediaPlayer.focus();
        },

        // This function is called whenever a user navigates away from this page.
        // It resets the page and disposes of the media player control.
        unload: function () {
            if (mediaPlayer) {
                mediaPlayer.playTimeTrackingPlugin.removeEventListener("eventtracked", onPlayTimeEventTracked, false);
                mediaPlayer.positionTrackingPlugin.removeEventListener("eventtracked", onPositionEventTracked, false);
                mediaPlayer.dispose();
                mediaPlayer = null;
            }
        }
    });

    function onPlayTimeEventTracked(e) {
        if (!isNaN(e.detail.trackingEvent.playTimePercentage)) {
            console.log("Type: " + e.type + ", Timestamp: " + e.detail.timestamp + ", Play Time Percentage: " + e.detail.trackingEvent.playTimePercentage);
        } else {
            console.log("Type: " + e.type + ", Timestamp: " + e.detail.timestamp + ", Play Time: " + e.detail.trackingEvent.playTime);
        }
    }

    function onPositionEventTracked(e) {
        if (!isNaN(e.detail.trackingEvent.positionPercentage)) {
            console.log("Type: " + e.type + ", Timestamp: " + e.detail.timestamp + ", Position Percentage: " + e.detail.trackingEvent.positionPercentage + ", Skipped Past: " + e.detail.skippedPast);
        } else {
            console.log("Type: " + e.type + ", Timestamp: " + e.detail.timestamp + ", Position: " + e.detail.trackingEvent.position + ", Skipped Past: " + e.detail.skippedPast);
        }
    }
})();