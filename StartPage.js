var menuActive = false;

function showMobileMenuBar()
{
    if(!menuActive) {
        document.getElementById("mobileMenuBar").style.display = "block";
        document.getElementById("mobileMenuBarButton").style.backgroundColor = "#B6E2A1";
        menuActive = true;
    }
    else
    {
        hideMobileMenuBar();
    }
}

function hideMobileMenuBar()
{
    document.getElementById("mobileMenuBar").style.display = "none";
    document.getElementById("mobileMenuBarButton").style.backgroundColor = "rgba(0, 0, 0, 0)";
    menuActive = false;
}