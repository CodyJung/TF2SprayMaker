$(document).ready(function () {
    $(".imageWrapper").on("mouseenter", null, function () {
        if ($(this).has(".nearImage").length != 0) {
            $(this).children(".farImage").hide();
            $(this).children(".nearImage").show();
        }
    });

    $(".imageWrapper").on("mouseleave", null, function () {
        if ($(this).has(".nearImage").length != 0) {
            $(this).children(".nearImage").hide();
            $(this).children(".farImage").show();
        }
    });
});