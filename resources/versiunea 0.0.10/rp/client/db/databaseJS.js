var RegisterForm = null;
var LoginForm = null;
var CaracterMenu = null;

var nume = null;
var prenume = null;
var varsta = null;
var tara = null;
var PREPARE_CAMERA = null;

function login(Password) {
    API.triggerServerEvent("CheckLogin", Password);
}
function register(Email, Password) {
    if (Email.includes("@") && Email.includes("."))
    {
        if (Password.length >= 6)
        {
            API.triggerServerEvent("CheckRegister", Email, Password);
        }
        else
        {
            RegisterForm.eval("document.getElementById('wrong').innerHTML = 'Parola este prea scurta!';");
        }
    }
    else
    {
        RegisterForm.eval("document.getElementById('wrong').innerHTML = 'Emailul este invalid';");
    }
}

API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName == "CreateAuthCam")
    {
        player = API.getLocalPlayer();
        API.showCursor(true);
        API.setCanOpenChat(false);
        API.setHudVisible(false);
        PREPARE_CAMERA = API.createCamera(API.getEntityPosition(player).Add(new Vector3(0, 0, 100)), new Vector3());
        API.setActiveCamera(PREPARE_CAMERA);
    }
    if (eventName == "DestroyAuthCam")
    {
        PREPARE_CAMERA = null;
        API.setActiveCamera(null);
    }
    if (eventName == "CreateMenu")
    {
        let menu = API.createMenu("Caracter", "Informatii despre tine", 0, 0, 6);
        let numeitem = API.createColoredItem("Nume", "Numele dvs.", "#771616", "#d11919");
        let prenumeitem = API.createColoredItem("Prenume", "Prenumele dvs.", "#771616", "#d11919");
        let varstaitem = API.createColoredItem("Varsta", "Varsta dvs.", "#771616", "#d11919");
        let taraitem = API.createColoredItem("Tara", "Tara de origine.", "#771616", "#d11919");

        let nextitem = API.createColoredItem("Next", "Completeaza toate campurile.", "#771616", "#d11919");
        menu.ResetKey(menuControl.Back);
        function activated() {
            if (nume != null && prenume != null && varsta != null && tara != null) {
                menu.RemoveItemAt(4);
                let nextitem = API.createColoredItem("Next", "Spre spawn.", "#0d47a1", "#1976d2");

                nextitem.Activated.connect(function (menu, item) {
                    API.triggerServerEvent("RegisterCaracter", nume, prenume, varsta, tara); // de aici o sa se trimita mai multe variabile
                    menu.Visible = false;

                });
                menu.AddItem(nextitem);
            }
        }
        //verde deschis 53d125
        //verde inchis 2a770e

        numeitem.Activated.connect(function (menu, item) {
            if (nume == null) {
                nume = API.getUserInput("", 25);
                if (nume.length < 2)
                {
                    nume = null;
                }
        /*        menu.RemoveItemAt(0);
                let numeitem = API.createColoredItem("Nume", "Nume introdus", "#2a770e", "#53d125");
                menu.AddItem(numeitem); daca o sa vreau sa fie mai colorat */
                activated();
            }
        });

        prenumeitem.Activated.connect(function (menu, item) {
            if (prenume == null) {
                prenume = API.getUserInput("", 25);
                if (prenume.length < 2) {
                    prenume = null;
                }
                activated();
            }
        });

        varstaitem.Activated.connect(function (menu, item) {
            if (varsta == null) {
                varsta = API.getUserInput("", 25);
                if (varsta.length < 2) {
                    varsta = null;
                }
                activated();
            }
        });

        taraitem.Activated.connect(function (menu, item) {
            if (tara == null) {
                tara = API.getUserInput("", 25);
                if (tara.length < 2) {
                    tara = null;
                }
                activated();
            }
        });


        menu.AddItem(numeitem);
        menu.AddItem(prenumeitem);
        menu.AddItem(varstaitem);
        menu.AddItem(taraitem);
        menu.AddItem(nextitem);
       
        menu.Visible = true;  
    }
    if (eventName == "CreateCaracter")
    {
        API.triggerServerEvent("CCreator");
    }
    if (eventName == "loggedin")
    {
        API.showCursor(false);
        API.destroyCefBrowser(LoginForm);
        API.setCanOpenChat(true);
        API.setHudVisible(true);
    }

    if (eventName == "registred") {
        PREPARE_CAMERA = null;
        API.setActiveCamera(null);
        API.destroyCefBrowser(RegisterForm);
    }

    if (eventName == "wrongpw")
    {
        LoginForm.eval("document.getElementById('wrong').innerHTML = 'Parola este incorecta!';");
    }

    if (eventName == "loginCallBack") {
        const resolution = API.getScreenResolution();
        LoginForm = API.createCefBrowser(resolution.Width, resolution.Height);
        API.waitUntilCefBrowserInit(LoginForm);
        API.setCefBrowserPosition(LoginForm, 0, 0); 
        API.loadPageCefBrowser(LoginForm, "client/db/login/login.html");
       
    }

    if (eventName == "registerCallBack") {
        const resolution = API.getScreenResolution();
        RegisterForm = API.createCefBrowser(resolution.Width, resolution.Height, true);
        API.waitUntilCefBrowserInit(RegisterForm);
        API.setCefBrowserPosition(RegisterForm, 0, 0);
        API.setCefBrowserHeadless(RegisterForm, false);
        API.loadPageCefBrowser(RegisterForm, "client/db/register/register.html");
    }

});

