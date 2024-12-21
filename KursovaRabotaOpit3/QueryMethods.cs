using KursovaRabotaOpit3.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.PortableExecutable;
using System.Text;
using System.Threading.Tasks;
using static KursovaRabotaOpit3.DataBaseManager;
using static KursovaRabotaOpit3.Attributes.TextOperations;
using static KursovaRabotaOpit3.Utility;
using System.Data.Common;
using System.Security.Cryptography;
using System.IO;

namespace KursovaRabotaOpit3
{
    public static class QueryMethods
    {
        
        public static void CreateTable(string cmd)
        {
            string TableName;
            DbList<Column> Columns = new DbList<Column>();
            string[] parts = MySplit(cmd, '(');
            string columnPart;
            if (parts.Length != 2)
            {
                ErrorDisplay("Invalid query format");
                return;
            }
            TableName = MyTrim(parts[0]);
            columnPart = MyTrimEnd(parts[1], ')');
            string[] columnDefinitions = MySplit(columnPart, ',');
            int count = 0;
            foreach (var column in columnDefinitions)
            {
                string[] allColumnInfo = MySplit(MyTrim(column), ' ');
                string[] columnInfo;
                if (allColumnInfo.Length == 1)
                {

                    columnInfo = MySplit(MyTrim(column), ':');
                }
                else
                {
                    columnInfo = MySplit(MyTrim(allColumnInfo[0]), ':');
                }
                Type datatype = typeof(string);
                if (columnInfo.Length != 2)
                {
                    ErrorDisplay("Invalid column definition: " + column);
                    return;
                }



                string columnName = MyTrim(columnInfo[0]);

                string type = MyTrim(columnInfo[1]);
                if (!CanConvertStringToType(type)
                    )
                {
                    ErrorDisplay("not valid data type: " + type);
                    return;
                }
                datatype = StringToType(type);
                Column col = new Column(columnName, datatype, false, null);

                if (allColumnInfo.Length == 3)
                {
                    if (allColumnInfo[1].ToUpper() != "DEFAULT" )
                    {
                        ErrorDisplay("Invalid operator: " + allColumnInfo[1] + ", default expected.");
                        return;
                    }
                    if (datatype == typeof(int) && int.TryParse(allColumnInfo[2], out int value))
                    {
                        col.HasDefaultValue = true;
                        col.DefaultValue = value;
                    }
                    else if (datatype == typeof(DateOnly) && DateOnly.TryParse(allColumnInfo[2], out DateOnly date))
                    {
                        col.HasDefaultValue = true;
                        col.DefaultValue = date;
                    }
                    else if (datatype == typeof(string))
                    {
                        col.HasDefaultValue = true;
                        col.DefaultValue = allColumnInfo[2];
                    }
                    else
                    {
                        ErrorDisplay("Invalid data for a default value for column: " + col.ColumnName);
                        return;
                    }
                    
                }
                if(allColumnInfo.Length ==2)
                {
                    if (allColumnInfo[1].ToUpper() != "AUTO")
                    {
                        ErrorDisplay("Invalid operator: " + allColumnInfo[1] + ", auto expected.");
                        return;
                    }
                    if(datatype != typeof(int))
                    {
                        ErrorDisplay("Cannot apply auto for anything else than an integer");
                        return;
                    }
                    
                    col.HasAutoValue = true;
                }












                DbList<string> columnnames = new DbList<string>();
                for (int i = 0; i < Columns.Count; i++)
                {
                    columnnames.Add(Columns[i].ColumnName);
                }
                if (columnnames.Exists(c => c == col.ColumnName))
                {
                    ErrorDisplay("columnnames cannot match");
                    return;
                }

                Columns.Add(col);
                count++;


            }


            Console.WriteLine("Creating table: " + TableName);

            string filePath = Path.Combine(FilePath, TableName + ".dat");
            if (Columns.Count == 0)
            {
                ErrorDisplay("please create atleast one column");
                return;
            }
            if (File.Exists(filePath))
            {
                ErrorDisplay("table already exists");
                return;
            }

            using (File.Create(filePath))
            { }
            Console.WriteLine(filePath);
            SuccessDisplay("successfully created table " + TableName);
            FileStream fs = new FileStream(filePath, FileMode.Append);
            BinaryWriter writer = new BinaryWriter(fs);
            using (fs)
            using (writer)
            {
                SerializeColumns(Columns, writer);
                long firstemptyaddress = fs.Position;
                fs.Seek(3, SeekOrigin.Begin);
                writer.Write(firstemptyaddress);

            }



        }
        public static void ListTables()
        {
            string searchPattern = "*.dat";

            string[] tblFiles = Directory.GetFiles(FilePath, searchPattern);
            DbList<string> filenames = new DbList<string>();
            foreach (string filePath in tblFiles)
            {
                string fileName = MyTrimEnd(Path.GetFileName(filePath), ".dat");
                filenames.Add(fileName);


            }

            filenames.OrderBy(x => x);
            for (int i = 0; i < filenames.Count; i++)
            {
                Console.ForegroundColor = ConsoleColor.Cyan;
                Console.WriteLine(filenames[i]);
                Console.ResetColor();
            }
        }
        public static void TableInfo(string TableName)
        {
            string fileName = TableName + ".dat"; 

            string filePathToExamine = Path.Combine(FilePath, fileName);
            if (!File.Exists(filePathToExamine))
            {
                ErrorDisplay($"Table {TableName} does not exist in the database");
                return;
            }
                if (!DataCheck(filePathToExamine))
            {
                return;
            }

            
                DbList<Column> columns;
                using(FileStream fs = new FileStream(filePathToExamine,FileMode.Open,FileAccess.ReadWrite))
                using(BinaryReader reader = new BinaryReader(fs))
                {
                    columns = DeserializeColumns(reader);
                    byte count = 1;
                    foreach(Column col in columns)
                    {
                        
                        Console.ForegroundColor= ConsoleColor.Cyan;
                        Console.Write($"{col.ColumnName}"); Console.ResetColor();
                           Console.Write( $" {TypeToString(col.DataType)}");
                            
                        if(col.HasDefaultValue)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write(" default");
                            Console.ResetColor();
                            Console.Write(" ");
                            Console.Write(col.DefaultValue);
                        }
                        if(col.HasAutoValue)
                        {
                            Console.ForegroundColor = ConsoleColor.DarkCyan;
                            Console.Write("auto");
                            Console.ResetColor();
                        }
                        Console.ForegroundColor = ConsoleColor.DarkBlue;
                        if(count!=columns.Count)
                        {
                        Console.Write(" | "); 
                        }
                        Console.ResetColor();
                        count++;
                    }
                Console.WriteLine();
                    ReadRecords(reader, columns, fs);
                }




            

           
        }
        public static void Insert(string cmd)
        {

            
            string[] commandparts = MySplit2(cmd);
            string tablename = commandparts[0];
            if (!File.Exists(FilePath + "\\" + tablename + ".dat"))
            {
                ErrorDisplay("File Doesn't Exists");
                return;
            }
            if(!DataCheck(FilePath + "\\" + tablename + ".dat"))
            {
                return;
            }
            FileStream fs = new FileStream(FilePath + "\\" + tablename + ".dat", FileMode.Open, FileAccess.ReadWrite);
            DbList<Column> cols = DeserializeColumns(new BinaryReader(fs));
            DbDictionary<string, object> data = new DbDictionary<string, object>(cols.Count);


            using (fs)
            {

                //получаваме колоните
                string[] columnstoadd = MySplit(MyTrimEnd(MyTrim(commandparts[1], '('), ')'), ',');
            for (int i = 0; i < columnstoadd.Length; i++)
            {
                columnstoadd[i] = MyTrim(columnstoadd[i]);
            }
            string[] columnnames = new string[cols.Count];
            for (int i = 0; i < cols.Count; i++)
            {


                columnnames[i] = cols[i].ColumnName;

            }

            for (int i = 0; i < columnstoadd.Length; i++)
            {
                bool found = false;
                for (int o = 0; o < columnnames.Length; o++)
                {

                    if (columnstoadd[i] == columnnames[o])
                    {
                    
                        found = true;

                    }
                }
                if (!found)
                {
                    ErrorDisplay("category not found: " + columnstoadd[i]);
                    return;
                }
            }
            if (commandparts[2] != "VALUES")
            {
                ErrorDisplay("unrecognized command:" + commandparts[2]);
                return;
            }
            string[] valuestoadd = MySplit(MyTrimEnd(MyTrim(commandparts[3], '('), ')'), ',');
            if (valuestoadd.Length != columnstoadd.Length)
            {
                ErrorDisplay("invalid format");

                return;
            }
            foreach(string s in valuestoadd)
            {
                if (s == string.Empty)
                {
                    ErrorDisplay("Value cannot be empty");
                    return;
                }
            }

            for (int i = 0; i < columnstoadd.Length; i++)
            {
                DbList<Column> column = cols.Where(col => col.ColumnName == columnstoadd[i]);
                Type dtype = column[0].DataType;
                if (column[0].HasAutoValue)
                {
                    ErrorDisplay($"Cannot insert value to column {column[0].ColumnName}, because the column is set to auto");
                    return;
                }
                if (dtype == typeof(string))
                {

                    data[columnstoadd[i]] = valuestoadd[i];
                }
                else if (dtype == typeof(int))
                {
                    if (int.TryParse(valuestoadd[i], out int a))
                    {

                        data[columnstoadd[i]] = int.Parse(valuestoadd[i]);
                    }
                    else
                    {
                        ErrorDisplay("invalid format for an integer: " + valuestoadd[i]);
                        return;
                    }
                }
                else
                {
                    if (DateOnly.TryParse(valuestoadd[i], out DateOnly dt))
                    {

                        data[columnstoadd[i]] = DateOnly.Parse(valuestoadd[i]);
                    }
                    else
                    {
                        ErrorDisplay("invalid format for a date: " + valuestoadd[i]);
                        return;
                    }
                }

            }
            //тук трябва да се добави check за default стойности
            DbList<string> remainingcolumns = new DbList<string>();
            if (columnstoadd.Length < columnnames.Length)
            {
                for (int i = 0; i < columnnames.Length; i++)
                {
                    bool found = false;
                    for (int o = 0; o < columnstoadd.Length; o++)
                    {
                        if (columnnames[i] == columnstoadd[o])
                        {
                            found = true; break;
                        }
                    }
                    if (!found)
                    {
                        remainingcolumns.Add(columnnames[i]);
                    }
                }
            }
            for (int i = 0; i < remainingcolumns.Count; i++)
            {
                Column matchingcolumn = new Column();
                for (int o = 0; o < cols.Count; o++)
                {
                    if (remainingcolumns[i] == cols[o].ColumnName)
                    {
                        matchingcolumn = cols[o];
                    }
                }
                if (matchingcolumn.HasDefaultValue)
                {
                    if(matchingcolumn.DataType ==typeof(int))
                    {

                    data.Add(remainingcolumns[i],
                        int.Parse((string)matchingcolumn.DefaultValue));
                    }
                    else if(matchingcolumn.DataType == typeof(DateOnly))
                    {
                        data.Add(remainingcolumns[i],
                        DateOnly.Parse((string)matchingcolumn.DefaultValue));
                    }
                    else 
                    {
                        data.Add(remainingcolumns[i],
                       matchingcolumn.DefaultValue);
                    }

                }
                else if(matchingcolumn.HasAutoValue)
                {
                    
                }
                else
                {
                    ErrorDisplay("you need to add a value to the current column: " + matchingcolumn.ColumnName);
                    return;
                }
            }
           



            }
            Record record = new Record(cols);
            record.data = data;
            FileStream str = new FileStream(FilePath + "\\" + tablename + ".dat", FileMode.Open, FileAccess.ReadWrite);
            BinaryWriter writer = new BinaryWriter(str);
            BinaryReader reader = new BinaryReader(str);
            AddRecord(record, writer, reader, str, cols);
            str.Dispose();
            SuccessDisplay("Successfully inserted a new row inside table: " + tablename);


        }
        public static void SelectFrom(string cmd)
        {
            string tablename = "";
            string[] columnstoselect = new string[0];
            string[] parts = new string[0];
            try
            {

            ProcessSelect(cmd, out parts, out tablename, out columnstoselect);
            }
            catch (Exception e)
            {
                ErrorDisplay(e.Message);
                return;
            }
            if (!DataCheck(FilePath + "\\" + tablename + ".dat"))
            {
                return;
            }

            FileStream str = new FileStream(FilePath + "\\" + tablename + ".dat", FileMode.Open, FileAccess.ReadWrite);
            _ = new BinaryWriter(str);
            BinaryReader reader = new BinaryReader(str);
            _ = new DbList<Column>();
            using (str)
            using (reader)
            {
                DbList<Column> cols = DeserializeColumns(reader);
                DbList<Condition> conditions = new DbList<Condition>();
                ConditionParser(parts, cols, out conditions, out bool distinct, out bool orderby, out string columnfororder); 
                SelectRecords(reader, cols, str, conditions,columnstoselect, out DbList<Record> records); 
                if(distinct)
                {
                    records = records.Distinct();
                }
                if(orderby)
                {
                    records = records.OrderBy(r => r.data[columnfororder]);
                }
                for(int i = 0;i< records.Count;i++)
                {
                    foreach(string col in columnstoselect)
                    {
                        DataDisplay(records[i].data[col].ToString()+ " ");
                    }
                    Console.WriteLine();

                }
                if(records.Count==0)
                {
                    SuccessDisplay("No records met the requirements");
                }
                
            }

        }
        public static void DeleteFROM(string cmd)
        {
            string tablename = "";
            string[] parts = new string[0];
            try
            {
                ProcessDelete(cmd, out parts, out tablename);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }
            if (!DataCheck(FilePath + "\\" + tablename + ".dat"))
            {
                return;
            }
            FileStream str = new FileStream(FilePath + "\\" + tablename + ".dat", FileMode.Open, FileAccess.ReadWrite);
            var writer = new BinaryWriter(str);
            BinaryReader reader = new BinaryReader(str);
            bool torecreate = false;
            using (str)
            using (reader)
            using (writer)
            {
                DbList<Column> cols = DeserializeColumns(reader);
                DbList<Condition> conditions = new DbList<Condition>();
                DeleteConditionParser(parts, cols, out conditions);
                DeleteRecords(reader, writer, str, cols, conditions);
                str.Seek(1,SeekOrigin.Begin);
                int num = reader.ReadInt16();
                if(num>=DeletedRecordsNeededToRecreate)
                {
                    torecreate = true;
                }
            }
            if(torecreate)
            RecreateTable(tablename);

            SuccessDisplay("Successfully deleted records from table " + tablename);
        }
        public static void ProcessSelect(string str, out string[] parts, out string tablename, out string[] columnstoselect)
        {
            
            columnstoselect = new string[0];
            tablename = "";
            parts = new string[0];
            string[] lines = MySplit2(str);
            if (lines.Length < 4)
            {

                throw new Exception("Usage: SELECT <column name, column name> FROM <table name> WHERE <column name> <operator> <value>");
            }
            parts = new string[lines.Length - 4];
            for(int i = 4;i<lines.Length;i++)
            {
                parts[i-4] = lines[i].Trim();
            }
            string[] columnnames = MySplit(MyTrim(lines[0]), ',');
            tablename = lines[2];
            if (lines[1].ToUpper()!="FROM")
            {

                throw new Exception("FROM expected, not " + lines[1]);
            }
            if (!File.Exists(FilePath + "\\" + lines[2]+".dat"))
            {
                throw new Exception("Table doesn't exist " + lines[2]);
            }
            
            

            DbList<Column> columns = new DbList<Column>();
            FileStream fs = new FileStream(FilePath + "\\" + lines[2] + ".dat",FileMode.Open,FileAccess.Read);
            BinaryReader reader = new BinaryReader(fs);
            using (fs)
            using (reader)
            {
                columns = DeserializeColumns(reader);
            }
            fs.Dispose();
            foreach(string col in columnnames)
            {
                if(!columns.Exists(c=>c.ColumnName==col))
                {
                    
                    throw new Exception($"Column doesn't exist: {col}");

                }
            }
            columnstoselect = columnnames;
            if (lines[3].ToUpper()!="WHERE")
            {

                throw new Exception("WHERE expected, not " + lines[3]);
            
            }
            
            
        }
        public static void ProcessDelete(string str, out string[] parts, out string tablename)
        {
            string[] lines = MySplit2(str);
            if(lines.Length<3)
            {
                throw new Exception("Usage: SELECT FROM <table name> WHERE <NOT operator?> <column name> <operator> <value> <AND, OR operator>");
            }
            if (lines[0].ToUpper() != "FROM")
            {
                throw new Exception("FROM expected, not " + lines[0]);
            }
            if (!File.Exists(FilePath + "\\" + lines[1] + ".dat"))
            {
                throw new Exception("Table doesn't exist " + lines[1]);
            }
            if (lines[2].ToUpper() != "WHERE")
            {
                throw new Exception("WHERE expected, not " + lines[2]);
            }
            tablename = lines[1];
            parts = new string[lines.Length - 3];
            for (int i = 3; i < lines.Length; i++)
            {
                parts[i - 3] = lines[i].Trim();
            }


        }
        public static void DropTable(string command)
        {
            string fileNameToDelete = command + ".dat"; 

            string filePathToDelete = Path.Combine(FilePath, fileNameToDelete);

            if (File.Exists(filePathToDelete))
            {
                try
                {
                    File.Delete(filePathToDelete);
                    SuccessDisplay($"{MyTrimEnd(fileNameToDelete, ".dat")} has been deleted.");
                }
                catch (Exception ex)
                {
                    ErrorDisplay($"Error deleting {fileNameToDelete}: {ex.Message}");
                    return;
                }
            }
            else
            {
                ErrorDisplay($"{MyTrimEnd(fileNameToDelete, ".dat")} does not exist in the database" + $".");
                return;
            }
        }

    }
}
