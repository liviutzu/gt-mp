var angajat;
var incursa;
var permis;
var TaxaPermisPlatita;
var bani;
var Camionagiu_EsteInCursa;

var Job_Trucker_Marker;
var Job_Trucker_Blip;

var Camionagiu_InCursa = false;
var Camionagiu_RemorcaDetasata = false;
var Camionagiu_IesitDinCamion = false;

API.onUpdate.connect(function () {
    if (Camionagiu_InCursa == true)
    {
        if (Camionagiu_IesitDinCamion == false)
        {
            if (Camionagiu_RemorcaDetasata == true)
            {
                API.displaySubtitle("Reataseaza remorca!", 5);
            }
            else
            {
                API.displaySubtitle("Livreaza remorca la destinatie pentru recompensa.", 5);
            }
        }
        else
        {
            API.displaySubtitle("Reintra in camion!", 5);
        }
    }
});

API.onServerEventTrigger.connect(function (eventName, args) {
    if (eventName == "Job_Trucker_InCursaFalse")
    {
        Camionagiu_InCursa = false;
        Camionagiu_IesitDinCamion = false;
        Camionagiu_RemorcaDetasata = false;
    }
    if (eventName == "Job_Trucker_IesitDinCamion")
    {
        Camionagiu_IesitDinCamion = true;
    }
    if (eventName == "Job_Trucker_RemorcaDeconectata")
    {
        Camionagiu_RemorcaDetasata = true;
    }
    if (eventName == "Job_Trucker_RemorcaReConectata") {
        Camionagiu_RemorcaDetasata = false;
    }
    if (eventName == "Job_Trucker_AReintratInCamion")
    {
        Camionagiu_IesitDinCamion = false;
    }
    if (eventName == "Job_Trucker_CreateStuffs")
    {
        var locatie = args[0];
        Job_Trucker_Blip = API.createBlip(locatie);
        API.setBlipRouteVisible(Job_Trucker_Blip, true);
        API.setBlipRouteColor(Job_Trucker_Blip, 81);
        API.setBlipColor(Job_Trucker_Blip, 81);
        API.setBlipShortRange(Job_Trucker_Blip, true);
        Job_Trucker_Marker = API.createMarker(1, locatie, new Vector3(), new Vector3(), new Vector3(1, 1, 1), 255, 255, 0, 255); //
    }
    if (eventName == "Job_Trucker_DestroyStuffs")
    {
        API.deleteEntity(Job_Trucker_Marker);
        API.deleteEntity(Job_Trucker_Blip);
    }
    if (eventName == "primarie_meniu") {
        angajat = args[0];
        permis = args[1];
        TaxaPermisPlatita = args[2];
        bani = args[3];
        taxapescuitbool = args[4];
        Camionagiu_EsteInCursa = args[5]; //daca e in cursa de camionagiu

        meniu_primarie = API.createMenu("Primarie", "Aceasta este primaria serverului", 0, 0, 6);

        let numeitem = API.createColoredItem("Locuri de munca", "Esti somer? Aunci intra aici.", "#2a770e", "#53d125");
        let taxapermis = API.createColoredItem("Taxa permis - 20$", "Daca vrei sa obtii permisul trebuie sa platesti aceasta taxa.", "#2a770e", "#53d125");
        let taxapescuit = API.createColoredItem("Licenta pescar - 50$", "Daca vrei sa pescuiesti in legalitate atunci iti trebuie aceasta licenta.", "#2a770e", "#53d125");
        let inscrieriauto = API.createColoredItem("Inscrieri auto - Soon", "Locul in care iti poti inscrie masina.", "#2a770e", "#53d125");
        let taxeimpozite = API.createColoredItem("Taxe si impozite - Soon", "Ai taxe? Atunci scapa de ele acum.", "#2a770e", "#53d125");

        let inchideprimarie = API.createColoredItem("Inchide", "Inchide meniul", "#2a770e", "#53d125");


        meniu_primarie.ResetKey(menuControl.Back);


        numeitem.Activated.connect(function (meniu_primarie, item) {
            if (angajat == true) {
                if (Camionagiu_EsteInCursa == true) {
                    API.sendChatMessage("~r~[Info]Nu poti schimba locul de munca in timp ce esti intr-o cursa.");
                    API.sendChatMessage("~r~[Info]Mergi la cel mai apropiat centru de distributie pentru a anula cursa.");
                    meniu_primarie.Visible = false;
                } else {
                    demisioneaza();
                }
            }
            else {
                joburi();
            }
        });
        taxapermis.Activated.connect(function (meniu_primarie, item) {
            if (permis == true)
            {
                API.sendChatMessage("~r~Nu poti plati taxa deoarece ai permisul de conducere");
            }
            else if (TaxaPermisPlatita == true)
            {
                API.sendChatMessage("~r~Ai platit deja taxa pentru permis.")
            }
            else
            {
                if (bani > 20)
                {
                    API.triggerServerEvent("ScoalaDeSoferi_TaxaTrigger");
                    API.setWaypoint(-545.1738, -203.9771, 37);
                    meniu_primarie.Visible = false;
                }
                else
                {
                    API.sendChatMessage("~r~Nu ai destui bani la tine.")
                }
            }
        });

        taxapescuit.Activated.connect(function (meniu_primarie, item) {
            if (taxapescuitbool == true) {
                API.sendChatMessage("~r~Ai deja licenta pentru pescuit.");
            }
            else {
                if (bani > 50) {
                    API.triggerServerEvent("Primarie_Taxa_Pescuit");
                    meniu_primarie.Visible = false;
                }
                else {
                    API.sendChatMessage("~r~Nu ai destui bani la tine.")
                }
            }
        });

        inscrieriauto.Activated.connect(function (meniu_primarie, item) {
            API.sendChatMessage("In curand!");

        });
        inchideprimarie.Activated.connect(function (meniu_primarie, item) {
            meniu_primarie.Visible = false;

        });


        meniu_primarie.AddItem(numeitem);
        meniu_primarie.AddItem(taxapermis);
        meniu_primarie.AddItem(taxapescuit);
        meniu_primarie.AddItem(inscrieriauto);
        meniu_primarie.AddItem(taxeimpozite);
        meniu_primarie.AddItem(inchideprimarie);
        meniu_primarie.Visible = true;

        //demisioneaza


        meniu_demisioneaza = API.createMenu("Resurse umane", "Ai deja un loc de munca.", 0, 0, 6);

        let dem = API.createColoredItem("Demisioneaza", "Ai deja un loc de munca, vrei sa demisionezi?", "#2a770e", "#53d125");

        let inapoidemisioneaza = API.createColoredItem("Inapoi", "Inapoi", "#2a770e", "#53d125");

        meniu_demisioneaza.ResetKey(menuControl.Back);

        dem.Activated.connect(function (meniu_demisioneaza, item) {
            API.sendPictureNotification("Ai demisionat de la locul de munca. Acum te poti angaja in alta parte.", "CHAR_ACTING_UP", 0, 1, "Klaus Iohannis", "Demisie");
            API.triggerServerEvent("joburi_demisioneaza");
            meniu_demisioneaza.Visible = false;
            meniu_primarie.Visible = true;
            angajat = false;

        });

        inapoidemisioneaza.Activated.connect(function (meniu_demisioneaza, item) {
            meniu_demisioneaza.Visible = false;
            meniu_primarie.Visible = true;

        });


        meniu_demisioneaza.AddItem(dem);
        meniu_demisioneaza.AddItem(inapoidemisioneaza);

        //joburi

        meniu_joburi = API.createMenu("Resurse umane", "Daca esti somer, aici te poti angaja", 0, 0, 6);

        let truckerjob = API.createColoredItem("Camionagiu - ~r~ Necesita permis", "Daca iti place sa fii numai pe drum, atunci acest job este perfect pentru tine.", "#2a770e", "#53d125");
        let job2 = API.createColoredItem("Fermier - Soon", "Descriere job 2", "#2a770e", "#53d125");
        let job3 = API.createColoredItem("Baiatul cu pizza - Soon", "Descriere job 3", "#2a770e", "#53d125");
        let job4 = API.createColoredItem("Curier - Soon", "Descriere job 4", "#2a770e", "#53d125");

        let inapoijoburi = API.createColoredItem("Inapoi", "Inapoi", "#2a770e", "#53d125");

        meniu_joburi.ResetKey(menuControl.Back);

        truckerjob.Activated.connect(function (meniu_joburi, item) {
            if (permis == true) {
                API.sendPictureNotification("Bine ai venit in echipa noastra de soferi de tir, mergi la waypoint pentru a incepe munca.", "CHAR_BEVERLY", 0, 1, "Dumitru Mihai", "Angajare");
                API.triggerServerEvent("joburi_camionagiu");
                API.setWaypoint(861.3986, -3185.125);
                angajat = true;
                meniu_joburi.Visible = false;
                meniu_primarie.Visible = true;
            } else {
                API.sendChatMessage("~r~[Info]Pentru acest job ai nevoie de permis de conducere. Mergi la scoala pentru a obtine unul.");
            }

        });

        inapoijoburi.Activated.connect(function (meniu_joburi, item) {
            meniu_joburi.Visible = false;
            meniu_primarie.Visible = true;

        });


        meniu_joburi.AddItem(truckerjob);
        meniu_joburi.AddItem(job2);
        meniu_joburi.AddItem(job3);
        meniu_joburi.AddItem(job4);
        meniu_joburi.AddItem(inapoijoburi);

    }
    if (eventName == "destroy_primarie") {
        meniu_primarie.Visible = false;


        meniu_joburi.Visible = false;

        meniu_demisioneaza.Visible = false;
    }


    if (eventName == "job_trucker") {
        incursa = args[0];
        job_trucker = API.createMenu("Distributie", "Alege o traiectorie si incepe munca.", 0, 0, 6);

        let primacursa = API.createColoredItem("Container - La Mesa - ~g~200$", "Livreaza containerul la La Mesa si vei primi 200$ la finalul cursei.", "#2a770e", "#53d125");
        let adouacursa = API.createColoredItem("Alimente - Davis - ~g~400$", "Livreaza alimente la Davis si vei primi 400$ la finalul cursei.", "#2a770e", "#53d125");
        let atreia = API.createColoredItem("Peste Congelat - Hookies - ~g~700$", "Livreaza pestele congelat la Hookies si vei primi 700$ la finalul cursei.", "#2a770e", "#53d125");
        let apatra = API.createColoredItem("Busteni - Paleto Forest - ~g~1000$", "Livreaza bustenii la Paleto Forest si vei primi 1000$ la finalul cursei.", "#2a770e", "#53d125");
        let acincea = API.createColoredItem("Instrumente Macelarie - Paleto Bay - ~g~1200$", "Livreaza instrumentele pentru macelarie la Paleto Bay si vei primi 1200$ la finalul cursei.", "#2a770e", "#53d125");
        let asasea = API.createColoredItem("Platforma - Grapeseed - ~g~1000$", "Livreaza platforma la Grapeseed si vei primi 1000$ la finalul cursei.", "#2a770e", "#53d125");
        let asaptea = API.createColoredItem("Cisterna - San Chainski - ~g~1100$", "Livreaza cisterna la San Chainski si vei primi 1100$ la finalul cursei.", "#2a770e", "#53d125");
        let aopta = API.createColoredItem("Trailer - Grand Senora - ~g~900$", "Livreaza trailerul la Grand Senora si vei primi 900$ la finalul cursei.", "#2a770e", "#53d125");

        let inchide = API.createColoredItem("Inchide", "Inchide meniul", "#2a770e", "#53d125");


        job_trucker.ResetKey(menuControl.Back);


        primacursa.Activated.connect(function (job_trucker, item) {

            API.triggerServerEvent("job_trucker_cursa", 200, 0, 3, "Docuri", "La Mesa", "Container");
            Camionagiu_InCursa = true;
            API.displaySubtitle("Livreaza remorca pentru a primi recompensa.", 5);
            job_trucker.Visible = false;
        });
        adouacursa.Activated.connect(function (job_trucker, item) {
            API.triggerServerEvent("job_trucker_cursa", 400, 1, 12, "Docuri", "Davis", "Alimente");
            API.displaySubtitle("Livreaza remorca pentru a primi recompensa.", 5);
            Camionagiu_InCursa = true;
            job_trucker.Visible = false;
        });
        atreia.Activated.connect(function (job_trucker, item) {
            API.triggerServerEvent("job_trucker_cursa", 700, 2, 12, "Docuri", "Hookies", "Peste congelat");
            API.displaySubtitle("Livreaza remorca pentru a primi recompensa.", 15);
            Camionagiu_InCursa = true;
            job_trucker.Visible = false;
        });
        apatra.Activated.connect(function (job_trucker, item) {
            API.triggerServerEvent("job_trucker_cursa", 1000, 3, 11, "Docuri", "Paleto Forest", "Busteni");
            API.displaySubtitle("Livreaza remorca pentru a primi recompensa.", 15);
            Camionagiu_InCursa = true;
            job_trucker.Visible = false;
        });
        acincea.Activated.connect(function (job_trucker, item) {
            API.triggerServerEvent("job_trucker_cursa", 1200, 4, 13, "Docuri", "Paleto Bay", "Instrumente Macelarie");
            API.displaySubtitle("Livreaza remorca pentru a primi recompensa.", 15);
            Camionagiu_InCursa = true;
            job_trucker.Visible = false;
        });
        asasea.Activated.connect(function (job_trucker, item) {
            API.triggerServerEvent("job_trucker_cursa", 1000, 5, 7, "Docuri", "Grapeseed", "Platforma");
            API.displaySubtitle("Livreaza remorca pentru a primi recompensa.", 15);
            Camionagiu_InCursa = true;
            job_trucker.Visible = false;
        });
        asaptea.Activated.connect(function (job_trucker, item) {
            API.triggerServerEvent("job_trucker_cursa", 1100, 6, 9, "Docuri", "San Chainski", "Cisterna");
            API.displaySubtitle("Livreaza remorca pentru a primi recompensa.", 15);
            Camionagiu_InCursa = true;
            job_trucker.Visible = false;
        });
        aopta.Activated.connect(function (job_trucker, item) {
            API.triggerServerEvent("job_trucker_cursa", 900, 7, 4, "Docuri", "Grand Senora", "Trailer");
            API.displaySubtitle("Livreaza remorca pentru a primi recompensa.", 15);
            Camionagiu_InCursa = true;
            job_trucker.Visible = false;
        });
        inchide.Activated.connect(function (job_trucker, item) {

            job_trucker.Visible = false;
        });


        job_trucker.AddItem(primacursa);
        job_trucker.AddItem(adouacursa);
        job_trucker.AddItem(atreia);
        job_trucker.AddItem(apatra);
        job_trucker.AddItem(acincea);
        job_trucker.AddItem(asasea);
        job_trucker.AddItem(asaptea);
        job_trucker.AddItem(aopta);
        job_trucker.AddItem(inchide);
        //canceljob
        trucker_canceljob = API.createMenu("Anulare cursa", "Esti deja intr-o cursa, vrei sa o anulezi?", 0, 0, 6);

        let anularecursa = API.createColoredItem("Anuleaza cursa", "Daca vrei sa anulezi cursa trebuie sa platesti 100$", "#2a770e", "#53d125");

        let inchidemeniucursa = API.createColoredItem("Inchide", "Inchide meniul", "#2a770e", "#53d125");


        trucker_canceljob.ResetKey(menuControl.Back);


        anularecursa.Activated.connect(function (trucker_canceljob, item) {
            API.sendPictureNotification("Ai anulat cursa si ai fost sanctionat cu 100$", "CHAR_ACTING_UP", 0, 1, "Dispecerat", "Informatii");
            Camionagiu_InCursa = false;
            API.triggerServerEvent("trucker_anularecursa");
            trucker_canceljob.Visible = false;

        });
        inchidemeniucursa.Activated.connect(function (trucker_canceljob, item) {

            trucker_canceljob.Visible = false;
        });


        trucker_canceljob.AddItem(anularecursa);
        trucker_canceljob.AddItem(inchidemeniucursa);

        if (incursa == false) {
            job_trucker.Visible = true;

        }
        else
        {
            trucker_canceljob.Visible = true;
        }
    }
    if (eventName == "dostroy_trcucker_menu") {
        job_trucker.Visible = false;
        trucker_canceljob.Visible = false;
    }

});
function demisioneaza()
{
    meniu_primarie.Visible = false;
    meniu_demisioneaza.Visible = true;
}
function joburi()
{
    meniu_primarie.Visible = false;
    meniu_joburi.Visible = true;
}