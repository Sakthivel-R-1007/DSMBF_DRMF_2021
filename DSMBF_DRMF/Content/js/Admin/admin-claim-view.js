$(document).ready(function () {

    $(document).on("click", ".paidicon", PaidPopup);

    $(document).on("click", "#btnconfirm_paid", submitPaid);

});

function PaidPopup() {
    $('#btnconfirm_paid').attr('data-value', $(this).attr("data-encDetail"));
    $('#PaidClaim').modal('show');
}

function submitPaid() {
	
	$("#dynamic_container").html('').append($("<input>", {
		id: "EncDetail",
		name: "EncDetail",
		type: "hidden",
        value: $('#btnconfirm_paid').data('value')
	}));

	$("#Form").attr({
        action: apppath + "/Admin/Claim/Paid"
	}).submit();
}