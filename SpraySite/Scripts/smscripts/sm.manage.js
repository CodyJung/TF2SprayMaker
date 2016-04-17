function approve(id)
{
    $.post("/Manage/Approve/" + id).success(function () {
        $("#" + id).show();
    });
}

function approvensfw(id) {
    $.post("/Manage/ApproveNSFW/" + id).success(function () {
        $("#" + id).show();
    });
}

function approvesketchy(id) {
    $.post("/Manage/ApproveSketchy/" + id).success(function () {
        $("#" + id).show();
    });
}

function deny(id) {
    $.post("/Manage/Deny/" + id).success(function () {
        $("#" + id).show();
    });
}