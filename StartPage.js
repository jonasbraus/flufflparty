var menuActive = false;

function showMobileMenuBar()
{
    if(!menuActive) {
        document.getElementById("mobileMenuBar").style.display = "block";
        menuActive = true;
    }
    else
    {
        document.getElementById("mobileMenuBar").style.display = "none";
        menuActive = false;
    }
}