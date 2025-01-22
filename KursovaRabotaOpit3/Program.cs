using KursovaRabotaOpit3.Attributes;
using static KursovaRabotaOpit3.Utility;
using static KursovaRabotaOpit3.DataBaseManager;
using static KursovaRabotaOpit3.QueryMethods;
using static KursovaRabotaOpit3.Attributes.TextOperations;
namespace KursovaRabotaOpit3;
static class Program
{
    public static void Main()
    {
        while (true)
        {
            Console.Title = "SUBD";
            Console.Write("> ");
            string input = MyTrim(Console.ReadLine());
            
            string[] commandParts = MySplit2(input);




            if (commandParts.Length >= 1)
            {

                string command = commandParts[0];

                switch (command)
                {
                    case "":
                        break;
                    case "CreateTable":
                        if (commandParts.Length >= 2)
                        {
                            string cm = input[12..];
                            CreateTable(cm);
                        }
                        else
                        {
                            ErrorDisplay("Usage: CreateTable <TableName(<columnname>:<columntype>,<columnname>:<columntype>)>");
                        }
                        break;

                    case "DropTable":
                        if (commandParts.Length >= 2)
                        {
                            DropTable(commandParts[1]);
                        }
                        else
                        {
                            ErrorDisplay("Usage: DropTable <TableName>");
                        }
                        break;

                    case "ListTables":
                        ListTables();
                        break;

                    case "TableInfo":
                        if (commandParts.Length == 2)
                        {

                            TableInfo(commandParts[1]);
                        }
                        else
                        {
                            ErrorDisplay("Usage: TableInfo <TableName>");
                        }
                        break;
                    //case "Help":
                    //    if (commandParts.Length == 1)
                    //    {
                    //        Console.WriteLine("Supported commands: CreateTable, DropTable, ListTables, TableInfo, Exit, Help");
                    //        Console.WriteLine("You can type the Help command and after it the command you want info for from the command list");
                    //    }
                    //    else if (commandParts.Length == 2)
                    //    {
                    //        switch (commandParts[1])
                    //        {
                    //            case "TableInfo":
                    //                Console.WriteLine("Gives information about a table");
                    //                Console.WriteLine("Usage: TableInfo <TableName>");
                    //                break;
                    //            case "DropTable":
                    //                Console.WriteLine("Deletes a table by it's name");
                    //                Console.WriteLine("Usage: DropTable <TableName>");
                    //                break;
                    //            case "CreateTable":
                    //                Console.WriteLine("Creates a table and it's given components");
                    //                Console.WriteLine("Usage: CreateTable <TableName(<columnname>:<columntype>,<columnname>:<columntype>)>");
                    //                break;
                    //            case "ListTables":
                    //                Console.WriteLine("Provides a list of the names of the tables inside the database");
                    //                break;
                    //            case "Exit":
                    //                Console.WriteLine("Exits the command prompt");
                    //                break;
                    //            case "DELETE":
                    //                Console.WriteLine("Deletes records from the chosen database");
                    //                break;
                    //            default:
                    //                ErrorDisplay("Invalid command");
                    //                break;
                    //        }
                    //    }
                    //    else
                    //    {

                    //        ErrorDisplay("invalid usage of the help command");
                    //    }
                    //    break;

                    case "Insert":
                        if (commandParts.Length > 2)
                        {
                            string commandline = input[12..];
                            Insert(commandline);

                        }
                        break;
                    case "DELETE":
                        {
                            if (commandParts.Length > 2)
                            {
                                DeleteFROM(input[7..]);
                            }
                            else
                            {
                                ErrorDisplay("Usage: DELETE FROM <Table Name> WHERE <Column> <operator> <data> (AND/OR/NOT)");

                            }
                            break;
                        }
                    case "SELECT":
                        {
                            if (commandParts.Length > 6)
                            {
                                

                                SelectFrom(input[7..]);
                            }
                            else
                            {
                                ErrorDisplay("Usage: SELECT <column1,column2> FROM <Table Name> WHERE <Column> <operator> <data> (AND/OR/NOT)");
                            }
                            break;
                        }
                    case "RECREATE":
                        {
                            if (commandParts.Length == 2)
                            {


                                RecreateTable(input[9..]);
                            }
                            else
                            {
                                ErrorDisplay("Usage: RECREATE <tablename>");
                            }
                            break;
                        }
                    case "DATACHECK":
                        {
                            DataCheck();
                            break;
                        }
                        //opravi firstpassed ako purviq elementminava conditiona
                        //opravi subcondition da raboti s <T>


                    case "exit":
                        return;

                    default:

                        ErrorDisplay("Invalid command. Supported commands: CreateTable, DropTable, ListTables, TableInfo, Exit, Insert INTO");

                        break;
                }
            }
        }
    }
}

