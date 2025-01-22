using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace jsonBetolto
{
    public class TournamentSchema
    {
        public Dictionary<int, Dictionary<int, string>> Configurations { get; set; }
        public string TournamentName { get; set; }
        public List<Round> Rounds { get; set; }

        public static TournamentSchema Load(string configFilePath)
        {
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException($"Konfigurációs fájl nem található {configFilePath}!");

            string json = File.ReadAllText(configFilePath);
            return JsonConvert.DeserializeObject<TournamentSchema>(json);
        }

        public Dictionary<int, string> GetPlayersForRound(int roundNumber)
        {
            var round = Rounds.Find(r => r.RoundNumber == roundNumber);

            if (round == null)
            {
                throw new ArgumentException($"Nem található ilyen forduló: {roundNumber}");
            }

            var players = new Dictionary<int, string>();
            foreach (var match in round.Matches)
            {
                foreach (var player in match.Players)
                {
                    if (!players.ContainsValue(player))
                        players.Add(players.Count + 1, player);
                }
            }
            return players;
        }

        public class Round
        {
            public int RoundNumber { get; set; }
            public string Descripton { get; set; }
            public List<Match> Matches { get; set; }
        }

        public class Match
        {
            public List<string> Players { get; set; }

            public int? Table { get; set; }

            public string MatchType { get; set; }
        }

        public string GetSchemaFilePath(int totalPlayers, int groupSize)
        {
            if (Configurations == null)
            {
                throw new InvalidOperationException("A konfiguráció nincs inicializálva.");
            }

            if (Configurations.TryGetValue(totalPlayers, out var groupConfigs) && groupConfigs.TryGetValue(groupSize, out var schemaPath))
            {
                return "schema/" + schemaPath;
            }
            throw new ArgumentException($"Nincs séma ehhez: {totalPlayers} játékos és {groupSize}-es csoport");
        }
    }

    public class TournamentManager
    {
        private readonly TournamentSchema _schema;

        public TournamentManager(string configFilePath)
        {
            _schema = TournamentSchema.Load(configFilePath);
        }

        public void ProcessTournament(int totalPlayers, int groupSize)
        {
            try
            {
                string schemaFilePath = _schema.GetSchemaFilePath(totalPlayers, groupSize);
                Console.WriteLine($"{schemaFilePath} séma használata.");

                string schemaJson = File.ReadAllText(schemaFilePath);
                var loadedSchema = JsonConvert.DeserializeObject<TournamentSchema>(schemaJson);

                Console.WriteLine("\nVerseny neve: " + loadedSchema.TournamentName);
                foreach (var round in loadedSchema.Rounds)
                {
                    Console.WriteLine($"\n{round.RoundNumber}. forduló: {round.Descripton}");
                    foreach (var match in round.Matches)
                    {
                        Console.WriteLine("  Meccs:");
                        Console.WriteLine("    Játékosok: " + string.Join(", ", match.Players));
                        Console.WriteLine("    Asztal: " + (match.Table.HasValue ? match.Table.ToString() : "Nincs megadva"));
                        Console.WriteLine("    Típus: " + match.MatchType);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hiba a séma feldolgozása közben: {ex.Message}");
            }
        }
    }
}
