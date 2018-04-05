var bani;
var player_menu;
var player_menu_transferbani;
var player_acte;

API.onKeyDown.connect(function (sender, e) {
    if (e.KeyCode === Keys.I && !API.isChatOpen()) {
        var Player = API.getLocalPlayer();

        if (API.isPlayerInAnyVehicle(Player)) {
            if (API.getVehicleEngineStatus(API.getPlayerVehicle(Player))) {
                API.setVehicleEngineStatus(API.getPlayerVehicle(Player), false);
            }
            else {
                API.setVehicleEngineStatus(API.getPlayerVehicle(Player), true);
            }
        }
    }
    if (e.KeyCode === Keys.K && !API.isChatOpen()) {
        API.triggerServerEvent("Player_OpenMenu");
    }
});
//nota1 atunci cand il aresteaza sa ii inchida meniul la jucator
//nota2 cand fac magazinele, sa adaug daca are telefon sau nu.
API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName == "Player_CreateMenu") {

        var jucator = args[0];
        player_menu = API.createMenu(jucator, "", 0, 0, 6);
        let acte = API.createMenuItem("Acte", "Aici gasesti toate actele tale de pe server.");
        let ajutor = API.createMenuItem("Ajutor", "Ai nevoie de ajutor? Atunci pune o intrebare aici si iti va raspunde un helper.");
        let raporteaza = API.createMenuItem("Asistenta", "Daca ai nevoie de un admin, cere aici.");
        let inventar = API.createMenuItem("Ghiozdan", "In ghiozdan gasesti toate lucrurile tale.");
        let sarme = API.createMenuItem("Stocare arme", "Pune toate armele si munitia in ghiozdan.");
        let telefon = API.createMenuItem("Telefon", "Acesta este telefonul tau.");
        let transferbani = API.createMenuItem("Transfera bani", "Transfera bani celui mai apropiat jucator.");
        let inchide = API.createMenuItem("Inchide", "Inchide meniul.");


        acte.Activated.connect(function (player_menu, item) {
            player_menu.Visible = false;
            API.triggerServerEvent("Player_Menu_Acte");
        });

        ajutor.Activated.connect(function (player_menu, item) {
            player_menu.Visible = false;
            var textajutor = API.getUserInput("", 200);
            API.triggerServerEvent("Player_Menu_Ajutor", textajutor);
        });

        raporteaza.Activated.connect(function (player_menu, item) {
            player_menu.Visible = false;
            var textasistenta = API.getUserInput("", 200);
            API.triggerServerEvent("Player_Menu_Asistenta", textasistenta);
        });

        transferbani.Activated.connect(function (player_menu, item) {
            player_menu.Visible = false;
            var suma = API.getUserInput("", 30);
            API.triggerServerEvent("Player_Menu_TransferBani", suma);
        });

        inchide.Activated.connect(function (player_menu, item) {
            player_menu.Visible = false;
        });

        player_menu.AddItem(acte);
        player_menu.AddItem(ajutor);
        player_menu.AddItem(raporteaza);
        player_menu.AddItem(inventar);
        player_menu.AddItem(sarme);
        player_menu.AddItem(telefon);
        player_menu.AddItem(transferbani);
        player_menu.AddItem(inchide);
    }
    if (eventName == "Player_Menu_Acte")
    {
        jucator = args[0];
        player_acte = API.createMenu(jucator, "", 0, 0, 6);
        let brevetavion = API.createMenuItem("Brevet avion - SOON", "Brevet avion.");
        let buletin = API.createMenuItem("Buletin - SOON", "Buletinul tau.");
        let licentarme = API.createMenuItem("Licenta arme - SOON", "Licenta pentru arme.");
        let licentabarca = API.createMenuItem("Licenta barca - SOON", "Licenta barcagiu.");
        let licentapescuit = API.createMenuItem("Licenta pescuit - SOON", "Licenta pentru pescuit.");
        let permisdeconducere = API.createMenuItem("Permis de conducere - SOON", "Permisul de conducere.");
        let inchide = API.createMenuItem("Inapoi", "Inapoi la meniu.");


        inchide.Activated.connect(function (player_acte, item) {
            player_acte.Visible = false;
            player_menu.Visible = true;
        });

        player_acte.AddItem(brevetavion);
        player_acte.AddItem(buletin);
        player_acte.AddItem(licentarme);
        player_acte.AddItem(licentabarca);
        player_acte.AddItem(licentapescuit);
        player_acte.AddItem(permisdeconducere);
        player_acte.AddItem(inchide);
        player_acte.Visible = true;
    }
    if (eventName == "Player_Menu_TransferBani") {

        var jucator = args[0];
        var suma = args[1];
        player_menu_transferbani = API.createMenu("Transfer bani", "Transfer bani", 0, 0, 6);
        let numejucator = API.createMenuItem("Nume jucator: " + jucator, "Numele jucatorului care va primi banii.");
        let sumatransferat = API.createMenuItem("Suma: " + suma, "Suma pentru transfer.");
        let transfera = API.createMenuItem("Transfera", "Sunt de acord.");
        let anuleaza = API.createMenuItem("Anuleaza", "Anuleaza tranzactia.");



        transfera.Activated.connect(function (player_menu_transferbani, item) {
            API.triggerServerEvent("Player_Menu_TransferBani_Accept", jucator, suma);
            player_menu_transferbani.Visible = false;
        });

        anuleaza.Activated.connect(function (player_menu_transferbani, item) {
            player_menu_transferbani.Visible = false;
        });

        player_menu_transferbani.AddItem(numejucator);
        player_menu_transferbani.AddItem(sumatransferat);
        player_menu_transferbani.AddItem(transfera);
        player_menu_transferbani.AddItem(anuleaza);
        player_menu_transferbani.Visible = true;

    }
    if (eventName == "Player_OpenMenu") {
        if (player_menu_transferbani != null && player_menu_transferbani.Visible == true)
            player_menu_transferbani.Visible = false;

        if (player_acte != null && player_acte.Visible == true)
            player_acte.Visible = false;

        if (player_menu.Visible == false)
            player_menu.Visible = true;
        else
            player_menu.Visible = false;
    }
    if (eventName == "ScoalaDeSoferi_Exam")
    {
        bani = args[0];

        scoaladesoferi = API.createMenu("Scoala de soferi", "Examene licenta condus", 0, 0, 6);

        let inainte = API.createColoredItem("Incepe - 200$", "Odata ce platesti, nu te mai poti intoarce.", "#2a770e", "#53d125");
        let anuleaza = API.createColoredItem("Anuleaza", "Reprogramezi examenul.", "#2a770e", "#53d125");


        scoaladesoferi.ResetKey(menuControl.Back);

        inainte.Activated.connect(function (scoaladesoferi, item) {
            API.triggerServerEvent("ScoalaDeSoferi_APlatit");
            scoaladesoferi.Visible = false;
        });

        anuleaza.Activated.connect(function (scoaladesoferi, item) {
            scoaladesoferi.Visible = false;
        });


        scoaladesoferi.AddItem(inainte);
        scoaladesoferi.AddItem(anuleaza);
        scoaladesoferi.Visible = true;
    }
    if (eventName == "destroy_scoaladesoferi")
    {
        scoaladesoferi.Visible = false;
    }
});