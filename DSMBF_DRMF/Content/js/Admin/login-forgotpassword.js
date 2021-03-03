$(document).ready(function () {

    jQuery.extend(jQuery.validator.messages, {
        required: "This field is required."
    });

    $("form").validate({
        errorPlacement: function (error, element) {
            $("span[data-valmsg-for='" + $(element).attr("name") + "']").html($(error));
        }
    });

    $("#Email").on("keyup", function () {
        $("#AccountNo").val('');
        $("span[data-valmsg-for='" + $("#AccountNo").attr("name") + "']").html('');
    });

    $("#AccountNo").on("keyup", function () {
        $("#Email").val('');
        $("span[data-valmsg-for='" + $("#Email").attr("name") + "']").html('');
    });

    $("#emailContinue").click(function () {
        $("#AccountNo").removeClass("required");
        $("#Email").addClass("required");
        $("form").submit();
    });

    $("#accountNoContinue").click(function () {
        $("#AccountNo").addClass("required");
        $("#Email").removeClass("required");
        $("form").submit();
    });

});