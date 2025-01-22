using jsonBetolto;

string configFilePath = "config.json";

var manager = new TournamentManager(configFilePath);

manager.ProcessTournament(48, 3);

Console.ReadKey();
