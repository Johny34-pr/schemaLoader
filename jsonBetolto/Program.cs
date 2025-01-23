using jsonBetolto;

string configFilePath = "config.json";

var manager = new TournamentManager(configFilePath);

manager.ProcessTournament(41, 4);

Console.ReadKey();
