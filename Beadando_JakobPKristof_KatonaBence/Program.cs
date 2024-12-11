using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;
using DLL;

namespace Beadando_JakobPKristof_KatonaBence
{
	internal class Program
	{
		public static List<Adat> jwst = new List<Adat>();

		static void MeresiAdatokGeneralasa()
		{
			Random random = new Random();
			string datum;
			double ho1;
			double ho2;
			double sugarzas;

			for (int i = 0; i < 50; i++)
			{
				datum = Methodusok.DatumGen(random);
				ho1 = Methodusok.HomersekletGen(random);
				ho2 = Methodusok.HomersekletGen(random);
				sugarzas = Math.Round(random.NextDouble() * 10, 2);
				jwst.Add(new Adat(datum, ho1, ho2, sugarzas));
			}
			Methodusok.Tores();
		}

		// JSON fájlbaírás - Kristóf
		static void JSONbaIras()
		{
			string json = JsonConvert.SerializeObject(jwst, Formatting.Indented);
			using (StreamWriter sw = new StreamWriter("meresi_adatok.json")) 
			{
				sw.WriteLine(json);
				sw.Flush();
				sw.Close();
			}
            Console.WriteLine("A mérési adatok mentve lettek a 'meresi_adatok.json' fájlba.");
			Methodusok.Tores();

		}
		// AI + Kristóf
		static void AdatbazisLetrehozasa(string dbPath)
		{
			if (dbPath == null || string.IsNullOrEmpty(dbPath.ToString()))
			{
				Console.WriteLine("Hiba: Az adatbázis elérési útja nem lett megadva.");
				return;
			}
			// Adatbázis létrehozása és kapcsolat nyitása
			using (SqliteConnection connection = new SqliteConnection(dbPath))
			{
				connection.Open();
				Console.WriteLine("Kapcsolódva az adatbázishoz!");

				// Tábla létrehozása (ha nem létezik)
				var createTableQuery = @"
                CREATE TABLE IF NOT EXISTS Meresek (
					Id INTEGER PRIMARY KEY AUTOINCREMENT,
                    Datum DATE,
                    HomersekletEgy REAL,
                    HomersekletKetto REAL,
                    Sugarzasi REAL
                )";

				using (SqliteCommand command = new SqliteCommand(createTableQuery, connection))
				{
					command.ExecuteNonQuery();
					Console.WriteLine("Meresek tábla létrehozva vagy már létezik.");
				}
			}
			Methodusok.Tores();
		}
		static void AdatokMentese(string dbPath, string datum, double homereseklet1, double homereseklet2, double sugarzas)
		{
			if (dbPath == null || string.IsNullOrEmpty(dbPath.ToString()))
			{
				Console.WriteLine("Hiba: Az adatbázis elérési útja nem lett megadva.");
				return;
			}
			using (SqliteConnection connection = new SqliteConnection())
			{
				connection.Open();

				var insertQuery = @"
				INSERT INTO Meresek (Datum, HomersekletEgy, HomersekletKetto, Sugarzasi)
				VALUES (@Datum, @HomersekletEgy, @HomersekletKetto, @Sugarzasi)";

				using (SqliteCommand command = new SqliteCommand(insertQuery, connection))
				{
					command.Parameters.AddWithValue("@Datum", datum);
					command.Parameters.AddWithValue("@HomersekletEgy", homereseklet1);
					command.Parameters.AddWithValue("@HomersekletKetto", homereseklet2);
					command.Parameters.AddWithValue("@Sugarzasi", sugarzas);

					command.ExecuteNonQuery();
					Console.WriteLine($"Adatok mentve!");
				}
			}
			Methodusok.Tores();
		}
		static void AdatbazisLekerdezes(string dbPath)
		{
			using (SQLiteConnection connection = new SQLiteConnection(dbPath))
			{
				connection.Open();

				string selectQuery = "SELECT * FROM Meresek";

				using (SQLiteCommand command = new SQLiteCommand(selectQuery, connection))
				using (SQLiteDataReader reader = command.ExecuteReader())
				{
					while (reader.Read())
					{
						Console.WriteLine($"Id: {reader["Id"]}, Datum: {reader["Datum"]}, " +
										  $"HomersekletEgy: {reader["HomersekletEgy"]}, HomersekletKetto: {reader["HomersekletKetto"]}, " +
										  $"Sugarzasi: {reader["Sugarzasi"]}");
					}
				}
			}
		}
        //LinQ lekérdezések --> Katona Bence

        public delegate double HomersekletConverterCelsiusba(double kelvin); 

        static void LinQFeladatok(List<Adat> jwst)
        {
            // Hőmérséklet-átalakítási metódus definiálása lambda kifejezéssel
            HomersekletConverterCelsiusba kelvinCelsiusra = (kelvin) => kelvin - 273.15;

            // 1. Átlaghőmérséklet kiszámítása Celsiusban a delegált segítségével
            var atlagHomerseklet = jwst.Average(adat =>
                (kelvinCelsiusra(adat.Homerseklet1) + kelvinCelsiusra(adat.Homerseklet2)) / 2);
            Console.WriteLine($"Átlagos hőmérséklet: {atlagHomerseklet:F2} °C");

            // 2. A legmagasabb sugárzási szintű mérés megkeresése és hőmérsékleteinek átalakítása
            var maxSugarzasAdat = jwst.OrderByDescending(adat => adat.SugárzásiSzint).First();
            Console.WriteLine($"Legmagasabb sugárzási szint: {maxSugarzasAdat.SugárzásiSzint} mért adatai: ");
            Console.WriteLine($"Dátum: {maxSugarzasAdat.Datum}, " +
                                $"Hőmérséklet1: {kelvinCelsiusra(maxSugarzasAdat.Homerseklet1):F2} °C, " +
                                $"Hőmérséklet2: {kelvinCelsiusra(maxSugarzasAdat.Homerseklet2):F2} °C");

            // 3. Mérések megszámlálása egy adott hőmérséklet felett (most Celsiusban)
            int szurtMersekletekSzama = jwst.Count(adat =>
                kelvinCelsiusra(adat.Homerseklet1) > -23.15 &&
                kelvinCelsiusra(adat.Homerseklet2) > -23.15);
            Console.WriteLine($"Mérések száma, ahol mindkét hőmérséklet 0 °C fölött van: {szurtMersekletekSzama}");
        }

        static void Main()
		{
			MeresiAdatokGeneralasa();

			JSONbaIras();

			LinQFeladatok(jwst);

			// SQLite inicializálás
			Batteries.Init();
			string dbPath = @"..\..\HaladoSzofi\SzenzorAdatok.db";
			AdatbazisLetrehozasa(dbPath);
			foreach (var item in jwst)
			{
				string datum = item.Datum;
				double ho1 = item.Homerseklet1;
				double ho2 = item.Homerseklet2;
				double sugarzas= item.SugárzásiSzint;
				AdatokMentese(dbPath, datum, ho1,ho2,sugarzas);
			}
			AdatbazisLekerdezes(dbPath);
			Console.ReadKey();
		}
	}
}
