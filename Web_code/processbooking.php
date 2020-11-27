<?php
try {
    session_start();
    $_SESSION['name']= $_POST['name'];
    $_SESSION['phone']= $_POST['phone'];
    $_SESSION['checkin']= $_POST['checkin'];
    $_SESSION['carnumber']= $_POST['carnumber'];

    header('Location: chooseseat.php');
}
catch (PDOException $e) {
    echo $e->getMessage();
}
