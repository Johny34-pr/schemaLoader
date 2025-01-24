using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace jsonBetolto
{
    public class TournamentSchema
    {
        public Dictionary<int, Dictionary<int, string>> Configurations { get; set; } = new();
        public string TournamentName { get; set; }
        public List<Round> Rounds { get; set; }

        public static TournamentSchema Load(string configFilePath)
        {
            if (!File.Exists(configFilePath))
                throw new FileNotFoundException($"Konfigurációs fájl nem található {configFilePath}!");

            string json = File.ReadAllText(configFilePath);

            if (string.IsNullOrEmpty(json))
                throw new Exception("A JSON fájl üres vagy érvénytelen.");

            return JsonConvert.DeserializeObject<TournamentSchema>(json);
        }

        public List<string> GetAdvancingPlayers(string group, List<string> players, int? groupSize)
        {
            var advancingPlayers = new List<string>();

            // Keresés a megfelelő szabályok között, hogy megtaláljuk a csoportmérethez illeszkedő szabályt
            var round = Rounds?.FirstOrDefault(r => r.RoundNumber == 1); // Az 1. forduló keressük, mivel itt vannak a csoportok
            var rule = round?.QualificationRules?.FirstOrDefault(r => r.Criteria.GroupSize == groupSize);

            // Ha nem találunk szabályt, akkor dobunk egy hibát
            if (rule == null)
            {
                throw new Exception($"Nincs érvényes továbbjutási szabály a(z) {groupSize}-es csoportokra.");
            }

            // Az alap továbbjutók hozzáadása
            foreach (var position in rule.Criteria.AdvancingPositions)
            {
                if (position <= players.Count)
                {
                    advancingPlayers.Add(players[position - 1]);
                }
            }

            // Ha a szabály speciális továbbjutókat is tartalmaz, hozzáadjuk azokat
            if (rule.Criteria.Special)
            {
                // Az "X" jelöléssel adunk hozzá speciális továbbjutókat
                foreach (var position in rule.Criteria.AdvancingPositions)
                {
                    if (position <= players.Count)
                    {
                        advancingPlayers.Add($"X{position}");
                    }
                }
            }

            return advancingPlayers;
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

        public class Round
        {
            public int RoundNumber { get; set; }
            public string Description { get; set; }
            public List<Match> Matches { get; set; }
            public List<QualificationRule> QualificationRules { get; set; }
        }

        public class Match
        {
            public string Group { get; set; }
            public List<string> Players { get; set; }
            public int? Table { get; set; }
            public string MatchType { get; set; }
        }

        public class QualificationRule
        {
            public string Description { get; set; }
            public Criteria Criteria { get; set; }
        }

        public class Criteria
        {
            public int? GroupSize { get; set; } // Nullable int a GroupSize típus számára
            public List<int> AdvancingPositions { get; set; }
            public bool Special { get; set; }
            public int? Limit { get; set; }

            public List<string> AssignedAdvancingPositions { get; set; } // AssignedAdvancingPositions hozzáadása
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

                // Kiírjuk a továbbjutási szabályokat az 1. forduló alapján
                var firstRound = loadedSchema.Rounds.FirstOrDefault(r => r.RoundNumber == 1);
                if (firstRound != null)
                {
                    Console.WriteLine("\nTovábbjutási szabályok az 1. fordulóban:");
                    foreach (var rule in firstRound.QualificationRules)
                    {
                        Console.WriteLine($"  - Leírás: {rule.Description}");
                        Console.WriteLine($"    Csoportméret: {(rule.Criteria.GroupSize.HasValue ? rule.Criteria.GroupSize.Value.ToString() : groupSize)}");
                        Console.WriteLine($"    Továbbjutók helyezései: {string.Join(", ", rule.Criteria.AdvancingPositions)}");
                        Console.WriteLine($"    Speciális: {(rule.Criteria.Special ? "Igen" : "Nem")}");
                        Console.WriteLine($"    Limit: {(rule.Criteria.Limit.HasValue ? rule.Criteria.Limit.Value.ToString() : "Nincs limit")}");

                        // Kiírjuk az AssignedAdvancingPositions mezőt
                        if (rule.Criteria.AssignedAdvancingPositions != null && rule.Criteria.AssignedAdvancingPositions.Any())
                        {
                            Console.WriteLine($"    Assigned Advancing Positions: {string.Join(", ", rule.Criteria.AssignedAdvancingPositions)}");
                        }
                    }
                }

                foreach (var round in loadedSchema.Rounds)
                {
                    Console.WriteLine($"\n{round.RoundNumber}. forduló");
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
