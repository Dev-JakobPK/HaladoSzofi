namespace DLL
{
    public class Adat
    {
        public static int szamlalo = 0;
        private string datum = "2021.12.25";
        public string Datum
        {
            get
            {
                return datum;
            }
            set
            {
                datum = value;
                Console.WriteLine("A(z) {0}, ID-val rendelkező mérés időbéllyeg meg lett változtatava!", id);
            }
        } // Mérés timestapja
        private int id;
        public double Homerseklet1 { get; private set; } // A teleszkópon több hőmérséklet szenzor is található
        public double Homerseklet2 { get; private set; } // Ezek Kelvinben vannak megadva
        public double SugárzásiSzint { get; private set; } // Sugárzás szintje

        public Adat(string datum, double homerseklet1, double homerseklet2, double sugárzásiSzint)
        {
            id = szamlalo;
            szamlalo++;
            Datum = datum;
            Homerseklet1 = homerseklet1;
            Homerseklet2 = homerseklet2;
            SugárzásiSzint = sugárzásiSzint;
        }
    }
    public class Methodusok
    {
        public static string DatumGen(Random random)
        {
            int ev = random.Next(2022, 2025);
            int honap = random.Next(1, 13);
            int nap;

            if ((honap < 8 && honap % 2 == 1) || (honap > 7 && honap % 2 == 0))
            {
                nap = random.Next(1, 32);
            }
            else if (honap == 2 && ev % 4 == 3)
            {
                nap = 28;
            }
            else
            {
                nap = random.Next(0, 31);
            }
            string datum;
            if (honap < 10 && nap > 9)
            {
                datum = Convert.ToString(ev + ".0" + honap + "." + nap);
            }
            else if (honap < 10 && nap < 10)
            {
                datum = Convert.ToString(ev + ".0" + honap + ".0" + nap);
            }
            else
            {
                datum = Convert.ToString(ev + "." + honap + "." + nap);
            }
            return datum;
        }
        public static double HomersekletGen(Random random)
        {
            // Kelvinben a 0 az -273.15 Celsius, 1000 Kelvi után nagyeséllyel elvesztettük a teleszkópot
            int anomalia = random.Next(0, 1000);
            if (anomalia == 0) return random.Next(0, 1000);
            // A teleszkópok pontosabban működnek hűtött állapotban
            return random.Next(0, 300);
        }
        public static void Tores()
        {
            Console.WriteLine("Nyomjon egy gombot a tovább haladáshoz!");
            Console.ReadKey(true);
            Console.Clear();
        }
    }
}