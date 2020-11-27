<?php
session_start();
$title = 'home';
$css = __DIR__ . '\public\css\home.css';
$js = '';

ob_start();
include __DIR__ .'\template\home.html.php';
if($_SESSION['submited']) {
    echo "<script>alert('Đặt chỗ thành công')</script>";
    $_SESSION['submited']= false;
}
$content = ob_get_clean();

include  __DIR__ . '\template\layout.html.php';

