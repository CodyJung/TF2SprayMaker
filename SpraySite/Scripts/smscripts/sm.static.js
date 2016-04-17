$(document).ready(function () {

    $("#fileUpload1").fileupload();

    $('#submitButton').click(function () {
        resetBars();

        var file = document.getElementById('imageInput').files[0];
        if (file == undefined)
        { return; }

        var fd = new FormData();

        fd.append('key', 'uploads/${filename}');
        fd.append('acl', 'public-read');
        fd.append('success_action_status', '201');
        fd.append('Content-Type', file.type);
        /* CHANGEME - AWS Access Key, policy, and signature. You'll want this to be limited to images only, and of a certain size. */
        fd.append('AWSAccessKeyId', 'Access Key goes here');
        fd.append('policy', 'Policy goes here');
        fd.append('signature', 'Sig goes here');

        fd.append("file", file);

        var xhr = new XMLHttpRequest();

        xhr.upload.addEventListener("progress", uploadProgress, false);

        xhr.onreadystatechange = function (evt) {
            if (xhr.readyState == 4) {
                if (xhr.status == 201) {
                    uploadComplete(evt);
                } else {
                    uploadFailed(evt);
                }
            }
        }

        /* CHANGEME - Change this to a temporary bucket for user uploads. */
        xhr.open('POST', 'https://tempsprays.s3-us-west-1.amazonaws.com', true);

        xhr.send(fd);
        $("#fileUpload1").fileupload('clear');
    });

    function uploadProgress(evt) {
        if (evt.lengthComputable) {
            var percentComplete = Math.round(evt.loaded * 100 / evt.total);
            $('#uploadProgressBar').css("width", function () { return percentComplete + "%"; });
        }
    }

    function uploadComplete(evt) {
        /* This event is raised when the server send back a response */
        var response = evt.target.response;
        $.post("/Create/UploadStatic", { s3Response: response, transparency: $("#transparency").is(":checked"), progressId: $.connection.hub.id }, function (data) {
            if(data == true) {
                $('#uploadProgress').addClass('progress-success').removeClass('progress-striped active');
            } else {
                $('#uploadProgress').addClass('progress-danger').removeClass('progress-striped active');
                $('#uploadProgressBar').text(data);
            }
        });
    }

    function uploadFailed(evt) {
        var response = evt.target.response;
        $('#uploadProgress').addClass('progress-danger').removeClass('progress-striped active');
        $.post("/Error/GetAmazonError", { s3Response: response }, function (data) {
            $('#uploadProgressBar').text(data);
        });
    }

    function resetBars() {
        // Reset progress bar
        $('#uploadProgress, #processProgress, #savingProgress').removeClass('progress-danger progress-success').addClass('progress-striped active');
        $('#uploadProgressBar, #processProgressBar, #savingProgressBar').css("width", "0%");
        $('.imageDiv img').attr("src", "/Content/images/ajax_loader.gif");
        $('#uploadProgressBar').text("");
        $("#resultsDiv").hide();
    }

    function loadPreviewImage(url, seconds) {
        var img = new Image();

        // wrap our new image in jQuery, then:
        $(img)
          // once the image has loaded, execute this code
          .load(function () {
              // set the image hidden by default    
              $(this).hide();

              // with the holding div #loader, apply:
              $('.imageDiv')
                .empty()
                // then insert our image
                .append(this);

              // fade our image in to create a nice effect
              $(this).fadeIn();
          })

          // if there was an error loading the image, react accordingly
          .error(function () {
              setTimeout(loadPreviewImage, seconds * 1000, url, seconds + 1);
          })

          // *finally*, set the src attribute of the new image to our image
          .attr('src', url);
    }

    //Signalr stuff
    var updates = $.connection.progressHub;
    updates.client.changePercent = function (percentage, bar) {
        if (bar == "Processing") {
            $('#processProgressBar').css("width", function () { return percentage + "%"; });

            if (percentage == "100") {
                $("#processProgress").removeClass("progress-striped active").addClass("progress-success");
            }
        } else if (bar == "Upload2S3") {
            $('#savingProgressBar').css("width", function () { return percentage + "%"; });

            if (percentage == "100") {
                $("#savingProgress").removeClass("progress-striped active").addClass("progress-success");
            }
        }
    };

    updates.client.showImage = function (imageUrl, downloadUrl, vmtUrl, animated, link) {
        $("#linksDiv #downloadLink").attr("href", downloadUrl);
        $("#linksDiv #vmtLink").attr("href", vmtUrl);
        $("#detailsLink").attr("href", link);
        $("#shareLink").attr("href", link);
        loadPreviewImage(imageUrl + "?nocache", 1);
        $("#resultsDiv").show();

        if (animated) {
            $(".imageWrapper .animatedFlag").show();
        } else {
            $(".imageWrapper .animatedFlag").hide();
        }
    };

    $.connection.hub.start();
});