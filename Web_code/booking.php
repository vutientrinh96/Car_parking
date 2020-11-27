<?php
$title = 'booking';
$css = [
    'public/css/booking.css',
    'public/css/animate.css',
    'public/css/hamburgers.min.css',
    'public/css/main.css',
    'public/css/select2.min.css',
    'public/css/util.css'
];
$js = [
    'public/js/jquery-3.2.1.min.js',
    'public/js/popper.js',
    'public/js/select2.min.js',
    'public/js/tilt.jquery.min.js',
    'public/js/bootstrap.min.js',
    'public/js/main.js'
];

$images = 'public\images\img-01.png';

ob_start();
include __DIR__ .'\template\booking.html.php';

$content = ob_get_clean();

ob_start();
include __DIR__ .'\template\booking.js.html.php';
$script = ob_get_clean();

include  __DIR__ . '\template\layout.html.php';