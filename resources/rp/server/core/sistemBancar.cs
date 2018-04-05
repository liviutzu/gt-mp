using System;
using System.IO;
using System.Collections.Generic;
using System.Globalization;
using GrandTheftMultiplayer.Server.API;
using GrandTheftMultiplayer.Server;
using GrandTheftMultiplayer.Server.Constant;
using GrandTheftMultiplayer.Server.Elements;
using GrandTheftMultiplayer.Server.Managers;
using GrandTheftMultiplayer.Shared;
using GrandTheftMultiplayer.Shared.Math;

namespace ServerGTMP
{
    class sistemBancar:Script
    {
        public static int MAX_ATM = 91;

        public static Vector3[] positionATM =
        {
            new Vector3(-301.6573, -829.5886, 31.41977),
            new Vector3(-303.2257, -829.3121, 31.41977),
            new Vector3(-204.0193, -861.0091, 29.27133),
            new Vector3(118.6416, -883.5695, 30.13945),
            new Vector3(24.5933, -945.543, 28.33305),
            new Vector3(5.686035, -919.9551, 28.48088),
            new Vector3(296.1756, -896.2318, 28.29015),
            new Vector3(296.8775, -894.3196, 28.26148),
            new Vector3(47.4731, -1036.218, 28.36778),
            new Vector3(45.8392, -1035.625, 28.36778),
            new Vector3(112.4761, -819.808, 30.33955),
            new Vector3(111.3886, -774.8401, 30.43766),
            new Vector3(114.5474, -775.9721, 30.41736),
            new Vector3(-256.6386, -715.8898, 32.7883),
            new Vector3(-259.2767, -723.2652, 32.70155),
            new Vector3(-254.5219, -692.8869, 32.57825),
            new Vector3(-27.89034, -724.1089, 43.22287),
            new Vector3(-30.09957, -723.2863, 43.22287),
            new Vector3(228.0324, 337.8501, 104.5013),
            new Vector3(158.7965, 234.7452, 105.6433),
            new Vector3(527.7776, -160.6609, 56.13671),
            new Vector3(-57.17029, -92.37918, 56.75069),
            new Vector3(89.81339, 2.880325, 67.35214),
            new Vector3(285.3485, 142.9751, 103.1623),
            new Vector3(357.1284, 174.0836, 102.0597),
            new Vector3(1137.811, -468.8625, 65.69865),
            new Vector3(1167.06, -455.6541, 65.81857),
            new Vector3(1077.779, -776.9664, 57.25652),
            new Vector3(289.53, -1256.788, 28.44057),
            new Vector3(289.2679, -1282.32, 28.65519),
            new Vector3(-165.5844, 234.7659, 93.92897),
            new Vector3(-165.5844, 232.6955, 93.92897),
            new Vector3(-1044.466, -2739.641, 8.12406),
            new Vector3(-1205.378, -326.5286, 36.85104),
            new Vector3(-1206.142, -325.0316, 36.85104),
            new Vector3(-846.6537, -341.509, 37.6685),
            new Vector3(-847.204, -340.4291, 37.6793),
            new Vector3(-720.6288, -415.5243, 33.97996),
            new Vector3(-867.013, -187.9928, 36.88218),
            new Vector3(-867.9745, -186.3419, 36.88218),
            new Vector3(-1415.48, -212.3324, 45.49542),
            new Vector3(-1430.663, -211.3587, 45.47162),
            new Vector3(-1410.736, -98.92789, 51.39701),
            new Vector3(-1410.183, -100.6454, 51.39652),
            new Vector3(-1282.098, -210.5599, 41.43031),
            new Vector3(-1286.704, -213.7827, 41.43031),
            new Vector3(-1289.742, -227.165, 41.43031),
            new Vector3(-1285.136, -223.9422, 41.43031),
            new Vector3(-712.9357, -818.4827, 22.74066),
            new Vector3(-710.0828, -818.4756, 22.73634),
            new Vector3(-617.8035, -708.8591, 29.04321),
            new Vector3(-617.8035, -706.8521, 29.04321),
            new Vector3(-614.5187, -705.5981, 30.224),
            new Vector3(-611.8581, -705.5981, 30.224),
            new Vector3(-660.6763, -854.4882, 23.45663),
            new Vector3(-537.8052, -854.9357, 28.27543),
            new Vector3(-594.6144, -1160.852, 21.33351),
            new Vector3(-596.1251, -1160.85, 21.3336),
            new Vector3(-526.7791, -1223.374, 17.45272),
            new Vector3(-1569.84, -547.0309, 33.93216),
            new Vector3(-1570.765, -547.7035, 33.93216),
            new Vector3(-1305.708, -706.6881, 24.31447),
            new Vector3(-1315.416, -834.431, 15.95233),
            new Vector3(-1314.466, -835.6913, 15.95233),
            new Vector3(-2071.928, -317.2862, 12.31808),
            new Vector3(-821.8936, -1081.555, 10.13664),
            new Vector3(-1110.228, -1691.154, 3.378483),
            new Vector3(-2956.848, 487.2158, 14.478),
            new Vector3(-2958.977, 487.3071, 14.478),
            new Vector3(-2974.586, 380.1269, 14),
            new Vector3(-1091.887, 2709.053, 17.91941),
            new Vector3(-2295.853, 357.9348, 173.6014),
            new Vector3(-2295.069, 356.2556, 173.6014),
            new Vector3(-2294.3, 354.6056, 173.6014),
            new Vector3(-3144.887, 1127.811, 19.83804),
            new Vector3(-3043.835, 594.1639, 6.732796),
            new Vector3(-3241.455, 997.9085, 11.54837),
            new Vector3(2564, 2584.553, 37.06807),
            new Vector3(2558.324, 350.988, 107.5975),
            new Vector3(156.1886, 6643.2, 30.59372),
            new Vector3(173.8246, 6638.217, 30.59372),
            new Vector3(-282.7141, 6226.43, 30.49648),
            new Vector3(-95.87029, 6457.462, 30.47394),
            new Vector3(-97.63721, 6455.732, 30.46793),
            new Vector3(-132.6663, 6366.876, 30.47258),
            new Vector3(-386.4596, 6046.411, 30.47399),
            new Vector3(1687.395, 4815.9, 41.00647),
            new Vector3(1700.694, 6426.762, 31.63297),
            new Vector3(1822.971, 3682.577, 33.26745),
            new Vector3(1171.523, 2703.139, 37.1477),
            new Vector3(1172.457, 2703.139, 37.1477)
        };
        public static Blip[] BlipBancomate = new Blip[MAX_ATM];
        public static SphereColShape[] arrayspherecolshapeATM = new SphereColShape[MAX_ATM];



        public sistemBancar()
        {
            API.onEntityEnterColShape += onEntityEnterColShapeHandler;
            API.onEntityExitColShape += onEntityExitColShapeHandler;
        }

        public void onEntityEnterColShapeHandler(ColShape colshape, NetHandle entity)
        {
        }

        public void onEntityExitColShapeHandler(ColShape colshape, NetHandle entity)
        { 
        }
        public void banksystemLoad()
        {
            for (int index = 0; index < MAX_ATM; index++)
            {
                arrayspherecolshapeATM[index] = API.createSphereColShape(positionATM[index], 1.5f);
            }

            for (int i = 0; i < MAX_ATM; i++)
            {
                BlipBancomate[i] = API.createBlip(positionATM[i]);
                API.setBlipColor(BlipBancomate[i], 2);
                API.setBlipScale(BlipBancomate[i], 0.5f);
                API.setBlipSprite(BlipBancomate[i], 207);
                API.setBlipName(BlipBancomate[i], "ATM");
                API.setBlipShortRange(BlipBancomate[i], true);
            }
            API.consoleOutput("Bank system loaded.");
        }
    }
}
