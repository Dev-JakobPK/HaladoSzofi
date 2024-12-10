using Microsoft.Data.Sqlite;
using Newtonsoft.Json;
using SQLitePCL;
using System;
using System.Collections.Generic;
using System.Data.SQLite;
using System.IO;
using System.Linq;

namespace Beadando_JakobPKristof_KatonaBence
{
	class Adat
	{
		public static int szamlalo = 0;
		private string datum;
		public string Datum { 
			get 
			{ 
				return datum; 
			} 
			set 
			{
				datum = value;
                Console.WriteLine("A(z) {0}, ID-val rendelkező mérés időbéllyeg meg lett változtatava!",id);
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
	internal class Program
	{
		public static List<Adat> jwst = new List<Adat>();
		static string DatumGen(Random random)
		{
			int ev = random.Next(2023, 2025);
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
		static double HomersekletGen(Random random)
		{
			// Kelvinben a 0 az -273.15 Celsius, 1000 Kelvi után nagyeséllyel elvesztettük a teleszkópot
			int anomalia = random.Next(0,1000);
			if (anomalia == 0) return random.Next(0, 1000);
			// A teleszkópok pontosabban működnek hűtött állapotban
			return random.Next(0,300);
		}
		static void MeresiAdatokGeneralasa()
		{
			Random random = new Random();
			string datum;
			double ho1;
			double ho2;
			double sugarzas;

			for (int i = 0; i < 50; i++)
			{
		        datum = DatumGen(random);
				ho1 = HomersekletGen(random);
				ho2 = HomersekletGen(random);
			    sugarzas = Math.Round(random.NextDouble() * 10, 2);
				jwst.Add(new Adat(datum, ho1, ho2, sugarzas));
			}
			Tores();
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
			Tores();

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
			Tores();
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
			Tores();
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
		static void LinQFeladatok()
		{
			// 1. ÁtlagHőmérsékletet számolja ki Kelvinbe
			var atlagHomerseklet = jwst.Average(adat => (adat.Homerseklet1 + adat.Homerseklet2) / 2);
			Console.WriteLine($"Átlagos hőmérséklet: {atlagHomerseklet:F2} K");

			// 2. Legmagasabb sugárzási szinthez tartozó mérés
			var maxSugarzasAdat = jwst.OrderByDescending(adat => adat.SugárzásiSzint).First();
			Console.WriteLine($"Legmagasabb sugárzási szint: {maxSugarzasAdat.SugárzásiSzint} mért adatai: ");
			Console.WriteLine($"Dátum: {maxSugarzasAdat.Datum}, Hőmérséklet1: {maxSugarzasAdat.Homerseklet1} K, Hőmérséklet2: {maxSugarzasAdat.Homerseklet2} K");

			// 3. Mérések száma adott hőmérsékleti tartományban
			int szurtMersekletekSzama = jwst.Count(adat => adat.Homerseklet1 > 250 && adat.Homerseklet2 > 250);
			Console.WriteLine($"Mérések száma, ahol mindkét hőmérséklet 250 K fölött van: {szurtMersekletekSzama}");

			Tores();
		}
		static void Tores()
		{
            Console.WriteLine("Nyomjon egy gombot a tovább haladáshoz!");
            Console.ReadKey(true);
			Console.Clear();
		}
		static void Main()
		{
			MeresiAdatokGeneralasa();

			JSONbaIras();

			LinQFeladatok();

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
