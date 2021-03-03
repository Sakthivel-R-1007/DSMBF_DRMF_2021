

//jQuery(document).ready(function ($) {
//	$('.header').load('includes/menunav.html');
//	$('.leftsidebar').load('includes/menunav.html .mainContents');
//	$('.footer').load('includes/footer.html');
//})


$(window).scroll(function() {
if ($(this).scrollTop() > 200) {
$('.go-top').fadeIn(200);
} else {
$('.go-top').fadeOut(200);
}
});

// Animate the scroll to top
$('.go-top').click(function(event) {
event.preventDefault();

$('html, body').animate({scrollTop: 0}, 200);
})

