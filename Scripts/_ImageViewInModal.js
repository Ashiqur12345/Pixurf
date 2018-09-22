function viewImage(imgUrl, title, description, contentId) {
    //console.log(imgUrl);
    var modal = document.getElementById('myModal');
    var modalImg = document.getElementById("modalImg");
    var captionText = document.getElementById("caption");

    modal.style.display = "block";
    modalImg.src = imgUrl;
    captionText.innerHTML = '<h3>' +
        title +
        '</h3>' +
        '<p>' +
        description +
        '</p>' +
        '<a href="../../Content/View/' + contentId + '">More</a>';


    // Get the <span> element that closes the modal
    var span = document.getElementsByClassName("close-modal")[0];

    // When the user clicks on <span> (x), close the modal
    span.onclick = function () {
        modal.style.display = "none";
    }
}