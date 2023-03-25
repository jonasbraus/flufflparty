var menuActive = false;

function showMobileMenuBar()
{
    if(!menuActive) {
        document.getElementById("mobileMenuBar").style.display = "block";
        document.getElementById("mobileMenuBarButton").style.backgroundColor = "rgba(198, 119, 70, 0.5)";
        menuActive = true;
    }
    else
    {
        document.getElementById("mobileMenuBar").style.display = "none";
        document.getElementById("mobileMenuBarButton").style.backgroundColor = "rgba(198, 119, 70, 0)";
        menuActive = false;
    }
}