$(document).ready(function () {

    var responses = [];

    $("#fileUpload1").fileupload();
    $("#fileUpload2").fileupload();

    $('#submitButton').click(function () {
        resetBars();

        var nearFile = document.getElementById('imageInput').files[0];
        if (nearFile == undefined)
        { return; }

        var farFile = document.getElementById('imageInput2').files[0];
        if (farFile == undefined)
        { return; }


        // TODO Pass in near/far as a boolean instead of doubling the methods.
        uploadToS3(nearFile, uploadProgressNear, uploadCompleteNear, uploadFailedNear);
        uploadToS3(farFile, uploadProgressFar, uploadCompleteFar, uploadFailedFar);

        $("#fileUpload1").fileupload('clear');
        $("#fileUpload2").fileupload('clear');
    });    

    function uploadToS3(file, progress, complete, failure)
    {
        var fd = new FormData();

        fd.append('key', 'uploads/${filename}');
        fd.append('acl', 'public-read');
        fd.append('success_action_status', '201');
        fd.append('Content-Type', file.type);
        /* CHANGEME - AWS Access Key, policy, and signature */
        fd.append('AWSAccessKeyId', 'Access Key goes here');
        fd.append('policy', 'Policy goes here - see notes in sm.static.js');
        fd.append('signature', 'Sig goes here');

        fd.append("file", file);

        var xhr = new XMLHttpRequest();

        xhr.upload.addEventListener("progress", progress, false);

        xhr.onreadystatechange = function (evt) {
            if (xhr.readyState == 4) {
                if (xhr.status == 201) {
                    complete(evt);
                } else {
                    failure(evt);
                }
            }
        }

        /* CHANGEME - Change this bucket to one of your own */
        xhr.open('POST', 'https://tempsprays.s3-us-west-1.amazonaws.com', true);

        xhr.send(fd);
    }

    function uploadProgressNear(evt) {
        if (evt.lengthComputable) {
            var percentComplete = Math.round(evt.loaded * 100 / evt.total);
            $('#uploadProgressBar').css("width", function () { return percentComplete + "%"; });
        }
    }

    function uploadProgressFar(evt) {
        if (evt.lengthComputable) {
            var percentComplete = Math.round(evt.loaded * 100 / evt.total);
            $('#uploadProgressBar2').css("width", function () { return percentComplete + "%"; });
        }
    }

    function uploadCompleteNear(evt) {
        /* This event is raised when the server sends back a response */
        var response = evt.target.response;
        responses.unshift(response);
        checkBothComplete();
    }

    function uploadCompleteFar(evt) {
        /* This event is raised when the server sends back a response */
        var response = evt.target.response;
        responses.push(response);
        checkBothComplete();
    }

    function checkBothComplete() {
        if (responses.length == 2)
        {
            $.post("/Create/UploadFading", { s3NearResponse: responses[0], s3FarResponse: responses[1], transparency: $("#transparency").is(":checked"), progressId: $.connection.hub.id }, function (data) {
                if(data[1] == true) {
                    $('#uploadProgress').addClass('progress-success').removeClass('progress-striped active');
                    $('#uploadProgress2').addClass('progress-success').removeClass('progress-striped active');
                } else if(data[0] == 0) {
                    $('#uploadProgressBar').text(data[1]);
                    $('#uploadProgress').addClass('progress-danger').removeClass('progress-striped active');
                } else if (data[0] == 1) {
                    $('#uploadProgressBar2').text(data[1]);
                    $('#uploadProgress2').addClass('progress-danger').removeClass('progress-striped active');
                }
            });
        }
    }

    function uploadFailedNear(evt) {
        var response = evt.target.response;
        $('#uploadProgress').addClass('progress-danger').removeClass('progress-striped active');
        $.post("/Error/GetAmazonError", { s3Response: response }, function (data) {
            $('#uploadProgressBar').text(data);
        });
    }

    function uploadFailedFar(evt) {
        var response = evt.target.response;
        $('#uploadProgress2').addClass('progress-danger').removeClass('progress-striped active');
        $.post("/Error/GetAmazonError", { s3Response: response }, function (data) {
            $('#uploadProgressBar2').text(data);
        });
    }

    function resetBars() {
        // Reset count
        responses.length = 0;

        // Reset progress bar
        $('#uploadProgress, #uploadProgress2, #processProgress, #savingProgress').removeClass('progress-danger progress-success').addClass('progress-striped active');
        $('#uploadProgressBar, #uploadProgressBar2, #processProgressBar, #savingProgressBar').css("width", "0%");
        $('.imageDiv img').attr("src", "/Content/images/ajax_loader.gif");
        $('#uploadProgressBar').text("");
        $('#uploadProgressBar2').text("");
        $("#resultsDiv").hide();
    }

    function loadPreviewImage(url, selector, seconds) {
        var img = new Image();

        // wrap our new image in jQuery, then:
        $(img)
          // once the image has loaded, execute this code
          .load(function () {
              // set the image hidden by default    
              $(this).hide();

              // with the holding div #loader, apply:
              $(selector)
                .empty()
                // then insert our image
                .append(this);

              // fade our image in to create a nice effect
              $(this).fadeIn();
          })

          // if there was an error loading the image, react accordingly
          .error(function () {
              setTimeout(loadPreviewImage, seconds * 1000, url, selector, seconds + 1);
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

    updates.client.showImage = function (imageUrl, nearUrl, downloadUrl, vmtUrl, animated, link) {
        $("#linksDiv #downloadLink").attr("href", downloadUrl);
        $("#linksDiv #vmtLink").attr("href", vmtUrl);
        $("#detailsLink").attr("href", link);
        $("#shareLink").attr("href", link);
        loadPreviewImage(imageUrl + "?nocache", '.farImage', 1);
        loadPreviewImage(nearUrl + "?nocache", '.nearImage', 1);
        $("#resultsDiv").show();
        $(".imageWrapper .fadingFlag").show();

        if (animated) {
            $(".imageWrapper .animatedFlag").show();
        } else {
            $(".imageWrapper .animatedFlag").hide();
        }
    };

    $.connection.hub.start();
});