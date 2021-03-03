$(document).ready(function () {

    jQuery.extend(jQuery.validator.messages, {
        required: "This is required."
    });

    $("form").validate({
        errorPlacement: function (error, element) {
            $("span[data-valmsg-for='" + $(element).attr("name") + "']").html($(error));
        }
    });

    $("#submitBtn").click(function () {
        $("form").submit();
    });

});