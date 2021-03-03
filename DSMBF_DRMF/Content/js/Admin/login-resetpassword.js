$(document).ready(function () {

    $.validator.addMethod("pwcheck", function (value) {
        return /[a-z]/.test(value) && /[0-9]/.test(value) && /[A-Z]/.test(value);
    }, "Invalid password");
    

    var validate = $("form").validate({
        errorPlacement: function (error, element) {
            $("span[data-valmsg-for='" + $(element).attr("name") + "']").html($(error));
        },
        rules: {
            ConfirmPassword: {
                equalTo: "#Password"
            },
            "UserAccount.Password": {
                pwcheck: true
            }
        },
        messages: {
            ConfirmPassword: "Please enter the same value again."
        }
    });

    $("#submit_btn").click(function () {
        $("form").submit();
    });
});