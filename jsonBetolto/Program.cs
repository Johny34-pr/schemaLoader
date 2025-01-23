using jsonBetolto;

Console.Title = "SchemaLoader";
Console.ForegroundColor = ConsoleColor.White;
Console.BackgroundColor = ConsoleColor.DarkBlue;
Console.Clear();

bool showMenu = true;

Menu(showMenu);

static void Menu(bool showMenu = true)
{
    while (showMenu)
    {
        ShowMenu();
        int userChoice = Convert.ToInt16(Console.ReadLine());

        switch (userChoice)
        {
            case 1:
                showMenu = false;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Gray;
                TournamentStart();
                break;
            case 2:
                showMenu = false;
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Green;
                break;
            case 3:
                Console.Clear();
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.WriteLine("Kilépés...");
                Environment.Exit(0);
                break;
            default:
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("\nÉrvénytelen választás! Kérlek próbáld újra.");
                break;
        }
    }
}

static void TournamentStart()
{
    string configFilePath = "config.json";

    var manager = new TournamentManager(configFilePath);

    Console.Write("Írd be a játékosszámot: ");
    int playerNumber = Convert.ToInt16(Console.ReadLine());

    Console.Write("Írd be a csoportot: ");
    int groupNumber = Convert.ToInt16(Console.ReadLine());

    Console.Clear();

    manager.ProcessTournament(playerNumber, groupNumber);
    Console.WriteLine("A menübe visszalépéshez nyomj meg egy gombot!");

    Console.ReadKey();

    Menu();
}
static void ShowMenu()
{
    Console.Clear();
    Console.ForegroundColor = ConsoleColor.Cyan;
    Console.WriteLine("=================================");
    Console.WriteLine("       Sémabetöltő Menü    ");
    Console.WriteLine("=================================");
    Console.ForegroundColor = ConsoleColor.White;
    Console.WriteLine("1. Séma kiválasztása");
    Console.WriteLine("2. Második lehetőség");
    Console.WriteLine("3. Kilépés");
    Console.WriteLine("\nKérlek válassz egy opciót (1-3):");
}