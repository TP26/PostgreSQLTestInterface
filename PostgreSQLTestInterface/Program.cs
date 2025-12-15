using Npgsql;
using static PostgreSQLTestInterface.Version_1;
using static PostgreSQLTestInterface.Version_2;
using static PostgreSQLTestInterface.Version_3;
using static PostgreSQLTestInterface.Version_4;

//Programme structure
//1 - All SQl statements are written by hand, conversion to objects by hand
//2 - data is parsed into data insertion statements that are then stored inside of subroutines. Experiment with insertion statement concatenation and the failures of these statements
//3 - Subroutines only pass in objects for insertion and only receive objects when returned
//4 - Subroutines become further genericised where the conversion function is passed in with the object type on retrieval.

Console.WriteLine("Postgresql interface");
string connectionString = getConnectionInformationFromUser();

try
{
    Console.WriteLine("Connecting to database");
    await using NpgsqlConnection conn = new NpgsqlConnection(connectionString);
    await conn.OpenAsync();
    Console.WriteLine("Connected to database");

    try
    {
        Console.WriteLine("-----\nVersion 1\n-----");
        Version1(conn);
        Console.WriteLine("-----\nVersion 2\n-----");
        Version2(conn);
        Console.WriteLine("-----\nVersion 3\n-----");
        Version3(conn);
        Console.WriteLine("-----\nVersion 4\n-----");
        Version4(conn);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error executing database operations: {ex.Message}");
    }
}
catch(Exception ex)
{
    Console.WriteLine($"Error connecting to database: {ex.Message}");
}

static string getConnectionInformationFromUser()
{
    Console.WriteLine("Please enter Postgresql database information: ");
    Console.Write("Host: ");
    string host = Console.ReadLine() ?? "";
    Console.Write("Username: ");
    string username = Console.ReadLine() ?? "";
    Console.Write("Database: ");
    string database = Console.ReadLine() ?? "";

    return $"Host={host};Username={username};Database={database}";
}

public class Object
{
    public int Id { get; set; }
    public string Name { get; set; }
    public int Size { get; set; }
    public int Weight { get; set; }
    public int Location { get; set; } = 0;
}