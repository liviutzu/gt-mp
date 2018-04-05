var bani;
var player_menu;
var player_menu_transferbani;
var player_acte;
var player_telefon;
var player_telefonnosim;
var telefon_servicii;
var telefon_urgente;
var player_inventory;
var inventory_arme;
var inventory_mancare;
var inventory_baut;
var inventory_munitie;
var inventory_folosit;
var inventory_echipat;
var inventory_senditem;

function telefonservicii() {
    telefon_servicii.Visible = true;
}
function telefonurgente() {
    telefon_urgente.Visible = true;
}

API.onResourceStart.connect(function () {
    telefon_urgente = API.createMenu("Urgente", "iFon 5.", 0, 0, 6);

    let Politie = API.createMenuItem("Politie - SOON", "Daca esti talharit, suna la politie.");
    let Pompieri = API.createMenuItem("Pompieri - SOON", "Ai preparat prea mult meth si ti-a luat casa foc? Atunci cheama pompierii.");
    let Salvare = API.createMenuItem("Salvare - SOON", "Mai ai putin si mori? Cheama o amulanta si se rezolva.");
    let inapoii = API.createMenuItem("Inapoi", "Inapoi in meniu.");


    inapoii.Activated.connect(function (telefon_urgente, item) {
        telefon_urgente.Visible = false;
        player_menu.Visible = true;
    });


    telefon_urgente.AddItem(Politie);
    telefon_urgente.AddItem(Pompieri);
    telefon_urgente.AddItem(Salvare);
    telefon_urgente.AddItem(inapoii);

    //servicii
    telefon_servicii = API.createMenu("Servicii", "iFon 5.", 0, 0, 6);

    let mecanic = API.createMenuItem("Mecanic - SOON", "Daca ti s-a stricat masina, cheama un mecanic.");
    let uber = API.createMenuItem("Uber - SOON", "Ai ramas pe jos? Cheama un uber.");
    let avocat = API.createMenuItem("Avocat - SOON", "Ai intrat in belele? Cheama un avocat.");
    let pizza = API.createMenuItem("Pizza - SOON", "Ti-e foame? Comanda o pizza.");
    let detectiv = API.createMenuItem("Detectiv - SOON", "Vrei sa spionezi pe cineva? Atunci cheama un detectiv.");
    let inapoi = API.createMenuItem("Inapoi", "Inapoi in meniu.");


    inapoi.Activated.connect(function (telefon_servicii, item) {
        telefon_servicii.Visible = false;
        player_telefon.Visible = true;
    });


    telefon_servicii.AddItem(mecanic);
    telefon_servicii.AddItem(uber);
    telefon_servicii.AddItem(avocat);
    telefon_servicii.AddItem(pizza);
    telefon_servicii.AddItem(detectiv);
    telefon_servicii.AddItem(inapoi);
});

API.onKeyDown.connect(function (sender, e) {
    if (e.KeyCode === Keys.I && !API.isChatOpen()) {
        var Player = API.getLocalPlayer();

        if (API.isPlayerInAnyVehicle(Player) == true) {
            if (API.getVehicleHealth(API.getPlayerVehicle(Player)) > 1) {
                if (API.getVehicleEngineStatus(API.getPlayerVehicle(Player)) == true) {
                    API.setVehicleEngineStatus(API.getPlayerVehicle(Player), false);
                }
                else {
                    API.setVehicleEngineStatus(API.getPlayerVehicle(Player), true);
                }
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
        let inventar = API.createMenuItem("Ghiozdan - WORKING", "In ghiozdan gasesti toate lucrurile tale.");
        let sarme = API.createMenuItem("Stocare arme", "Pune toate armele si munitia in ghiozdan.");
        let telefon = API.createMenuItem("Telefon - WORKING", "Acesta este telefonul tau.");
        let transferbani = API.createMenuItem("Transfera bani", "Transfera bani celui mai apropiat jucator.");
        let vehicul = API.createMenuItem("Vehicul - SOON", "Tot ce apartine de vehicul gasesti aici.");
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

        inventar.Activated.connect(function (player_menu, item) {
            player_menu.Visible = false;
            API.triggerServerEvent("Player_Menu_Inventory");
        });

        sarme.Activated.connect(function (player_menu, item) {
            player_menu.Visible = false;
            API.triggerServerEvent("Player_Menu_Sarme");
        });

        telefon.Activated.connect(function (player_menu, item) {
            player_menu.Visible = false;
            API.triggerServerEvent("Player_Menu_Telefon");
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
        player_menu.AddItem(vehicul);
        player_menu.AddItem(inchide);
    } // chestia cu k //daca e inputul gol sa nu faca alt cv
    if (eventName == "inventory_transferitem") {
        inventory_arme.Visible = false;
        var jucator = args[0];
        var trimite = args[1];
        var numeitem = args[2];
        var tipitem = args[3];

        inventory_senditem = API.createMenu("Transfer iteme", "Transfer itmeme", 0, 0, 6);
        let numejucator = API.createMenuItem("Nume jucator: " + jucator.toString(), "Numele jucatorului care va primi itemele.");
        let sumatransferat = API.createMenuItem("Iteme pentru expediat: " + trimite.toString(), "Cantiatea itemelor.");
        let transfera = API.createMenuItem("Transfera", "Sunt de acord.");
        let anuleaza = API.createMenuItem("Anuleaza", "Anuleaza tranzactia.");

        transfera.Activated.connect(function (inventory_senditem, item) {
            API.triggerServerEvent("inventory_transferitem_accept", jucator.toString(), trimite.toString(), numeitem.toString(), tipitem.toString());
            inventory_senditem.Visible = false;
        });

        anuleaza.Activated.connect(function (inventory_senditem, item) {
            inventory_senditem.Visible = false;
        });

        inventory_senditem.AddItem(numejucator);
        inventory_senditem.AddItem(sumatransferat);
        inventory_senditem.AddItem(transfera);
        inventory_senditem.AddItem(anuleaza);
        inventory_senditem.Visible = true;

    }
    if (eventName == "Player_Menu_Inventory")
    {
        var iteme = args[0];
        var itemestr = args[1];
        var itemenr = args[2];
        var size = args[3];
        let itemer = [];

        player_inventory = API.createMenu("Ghiozdan", "Aici este inventarul tau.", 0, 0, 6);

        let testi = API.createMenuItem("Obiecte in inventar " + iteme.toString() + " / " + size.toString(), "Totalul obiectelor din inventar");
        player_inventory.AddItem(testi);


        for (var i = 0; i < iteme; i++) {
            API.sendChatMessage(i.toString());
            itemer[i] = API.createMenuItem(itemestr[i] + " (" + itemenr[i] + ")", "");
            player_inventory.AddItem(itemer[i]);


        }
        let inapoi = API.createMenuItem("Inapoi", "Inapoi la inventar");

        if (itemer[0] != null) {
            itemer[0].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[0], itemestr[0]);
            });
        }
        if (itemer[1] != null) {
            itemer[1].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[1], itemestr[1]);
            });
        } if (itemer[2] != null) {
            itemer[2].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[2], itemestr[2]);
            });
        } if (itemer[3] != null) {
            itemer[3].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[3], itemestr[3]);
            });
        } if (itemer[4] != null) {
            itemer[4].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[4], itemestr[4]);
            });
        } if (itemer[5] != null) {
            itemer[5].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[5], itemestr[5]);
            });
        }if (itemer[6] != null) {
            itemer[6].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[6], itemestr[6]);
            });
        } if (itemer[7] != null) {
            itemer[7].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[7], itemestr[7]);
            });
        } if (itemer[8] != null) {
            itemer[8].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[8], itemestr[8]);
            });
        } if (itemer[9] != null) {
            itemer[9].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[9], itemestr[9]);
            });
        } if (itemer[10] != null) {
            itemer[10].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[10], itemestr[10]);
            });
        }if (itemer[11] != null) {
            itemer[11].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[11], itemestr[11]);
            });
        } if (itemer[12] != null) {
            itemer[12].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[12], itemestr[12]);
            });
        } if (itemer[13] != null) {
            itemer[13].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[13], itemestr[13]);
            });
        } if (itemer[14] != null) {
            itemer[14].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[14], itemestr[14]);
            });
        } if (itemer[15] != null) {
            itemer[15].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[15], itemestr[15]);
            });
        }if (itemer[16] != null) {
            itemer[16].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[16], itemestr[16]);
            });
        } if (itemer[17] != null) {
            itemer[17].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[17], itemestr[17]);
            });
        } if (itemer[18] != null) {
            itemer[18].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[18], itemestr[18]);
            });
        } if (itemer[19] != null) {
            itemer[19].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[19], itemestr[19]);
            });
        } if (itemer[20] != null) {
            itemer[20].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[20], itemestr[20]);
            });
        }if (itemer[21] != null) {
            itemer[21].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[21], itemestr[21]);
            });
        } if (itemer[22] != null) {
            itemer[22].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[22], itemestr[22]);
            });
        } if (itemer[23] != null) {
            itemer[23].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[23], itemestr[23]);
            });
        } if (itemer[24] != null) {
            itemer[24].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[24], itemestr[24]);
            });
        } if (itemer[25] != null) {
            itemer[25].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[25], itemestr[25]);
            });
        }if (itemer[26] != null) {
            itemer[26].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[26], itemestr[26]);
            });
        } if (itemer[27] != null) {
            itemer[27].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[27], itemestr[27]);
            });
        } if (itemer[28] != null) {
            itemer[28].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[28], itemestr[28]);
            });
        } if (itemer[29] != null) {
            itemer[29].Activated.connect(function (player_inventory, item) {
                API.triggerServerEvent("Player_Menu_Inventory_Select", itemenr[29], itemestr[29]);
            });
        } 

        inapoi.Activated.connect(function (player_inventory, item) {
            player_inventory.Visible = false;
            player_menu.Visible = true;
        });

        player_inventory.AddItem(inapoi);


        player_inventory.Visible = true;
    }
    if (eventName == "Inventory_arme") {
        player_inventory.Visible = false;
        var numeitem = args[0];
        var cantitateitem = args[1];
        var aruncabila = args[2].toString();
        inventory_arme = API.createMenu(numeitem + " (" + cantitateitem.toString() + ") ", "", 0, 0, 6);
        let echipeaza = API.createMenuItem("Echipeaza", "Echipeaza arma.");
        let arunca = API.createMenuItem("Arunca", "Arunca arma pe jos.");
        let ofera = API.createMenuItem("Ofera cuiva", "Introdu cantitate de iteme pentru transfer.");
        let inchide = API.createMenuItem("Inapoi", "Inapoi la meniu.");


        echipeaza.Activated.connect(function (inventory_arme, item) {
            if (aruncabila == "Da") {
                var descos;
                descos = API.getUserInput("", 10);
                if(descos.length > 0)
                    API.triggerServerEvent("Inventory_arme_equip_aruncabila", numeitem, descos);
                else
                    API.sendChatMessage("~r~[Info]Introdu o suma corecta.")

            }
            else {
                API.triggerServerEvent("Inventory_arme_equip", numeitem);
            }
            inventory_arme.Visible = false;
        });

        arunca.Activated.connect(function (inventory_arme, item) {
            var cantitate = API.getUserInput("", 4);
            inventory_arme.Visible = false;
            API.triggerServerEvent("Inventory_arme_drop", numeitem, cantitate, cantitateitem);
        });

        ofera.Activated.connect(function (inventory_arme, item) {
            var cantitate = API.getUserInput("", 4);
            if (aruncabila == "Da") {
                API.triggerServerEvent("inventory_transferitem", numeitem, cantitateitem, cantitate, "aruncabila");
            }
            else {
                API.triggerServerEvent("inventory_transferitem", numeitem, cantitateitem, cantitate, "arma");
            }
            inventory_arme.Visible = false;
        });

        inchide.Activated.connect(function (inventory_arme, item) {
            inventory_arme.Visible = false;
            player_inventory.Visible = true;
        });

        inventory_arme.AddItem(echipeaza);
        inventory_arme.AddItem(arunca);
        inventory_arme.AddItem(ofera);
        inventory_arme.AddItem(inchide);
        inventory_arme.Visible = true;
    }
    if (eventName == "Player_Menu_Acte")
    {
        var jucator = args[0];
        var player_acte = API.createMenu(jucator, "", 0, 0, 6);
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
        
        if (inventory_arme != null && inventory_arme.Visible == true)
            inventory_arme.Visible = false;

        if (player_inventory != null && player_inventory.Visible == true)
            player_inventory.Visible = false;

        if (telefon_servicii != null && telefon_servicii.Visible == true)
            telefon_servicii.Visible = false;

        if (telefon_urgente != null && telefon_urgente.Visible == true)
            telefon_urgente.Visible = false;

        if (player_telefon != null && player_telefon.Visible == true)
            player_telefon.Visible = false;

        if (player_telefonnosim != null && player_telefonnosim.Visible == true)
            player_telefonnosim.Visible = false;

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
    if (eventName == "Player_Menu_Telefon_NoSim") {

        player_telefonnosim = API.createMenu("No Sim", "Introduceti cartela sim.", 0, 0, 6);

        let nosim = API.createMenuItem("No Sim", "Introdu cartela sim pentru a putea folosi serviciile.");
        let urgente = API.createMenuItem("Urgente", "Apeleaza numarul de urgente 112.");
        let inapoi = API.createMenuItem("Inapoi", "Inapoi in meniu.");

        
        urgente.Activated.connect(function (player_telefonnosim, item) {
            player_telefonnosim.Visible = false;
            telefonurgente();
        });


        inapoi.Activated.connect(function (player_telefonnosim, item) {
            player_telefonnosim.Visible = false;
            player_menu.Visible = true;
        });


        player_telefonnosim.AddItem(nosim);
        player_telefonnosim.AddItem(urgente);
        player_telefonnosim.AddItem(inapoi);
        player_telefonnosim.Visible = true;
    }
    if (eventName == "Player_Menu_Telefon") {
        var nrtel = args[0];
        player_telefon = API.createMenu(nrtel, "iFon 5.", 0, 0, 6);

        let servicii = API.createMenuItem("Servicii", "Ai nevoie de un serviciu? Atunci suna aici.");
        let urgente = API.createMenuItem("Urgente", "Apeleaza numarul de urgente 112.");
        let agenda = API.createMenuItem("Agenda - SOON", "Aici gasesti toate numerele de telefon salvate.");
        let direct = API.createMenuItem("Introdu numar - SOON", "Suna direct, introdu numarul si apasa enter.");
        let inapoi = API.createMenuItem("Inapoi", "Inapoi in meniu.");

        servicii.Activated.connect(function (player_telefon, item) {
            player_telefon.Visible = false;
            telefonservicii();
        });
        urgente.Activated.connect(function (player_telefon, item) {
            player_telefon.Visible = false;
            telefonurgente();
        });

        inapoi.Activated.connect(function (player_telefon, item) {
            player_telefon.Visible = false;
            player_menu.Visible = true;
        });


        player_telefon.AddItem(servicii);
        player_telefon.AddItem(urgente);
        player_telefon.AddItem(agenda);
        player_telefon.AddItem(direct);
        player_telefon.AddItem(inapoi);
        player_telefon.Visible = true;
    }
    if (eventName == "destroy_scoaladesoferi")
    {
        scoaladesoferi.Visible = false;
    }
});