using KursovaRabotaOpit3.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static KursovaRabotaOpit3.Attributes.TextOperations;
using static KursovaRabotaOpit3.DataBaseManager;

namespace KursovaRabotaOpit3
{
    public static class Utility
    {
        public static string FilePath = "C:\\Users\\Dido\\OneDrive\\Работен плот\\Databases";
        public static int DeletedRecordsNeededToRecreate = 100; 
        
        public static void ErrorDisplay(string message)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(message);
            Console.ResetColor();
        }
        public static void SuccessDisplay(string message)
        {
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine(message); Console.ResetColor();
        }
        public static void DataDisplay(string message)
        {
            Console.ForegroundColor = ConsoleColor.DarkCyan;
            Console.Write(message); Console.ResetColor();
        }
        public static bool CanConvertTypeToString(Type type)
        {
            if (type == typeof(string) || type == typeof(int) || type == typeof(DateOnly))
            {
                return true;
            }

            return false;
        }
        public static bool CanConvertStringToType(string str)
        {
            if (str == "string" || str == "int" || str == "date")
            {
                return true;
            }
            return false;
        }

        public static string TypeToString(Type type)
        {
            if (type == typeof(string))
            {
                return "string";
            }
            if (type == typeof(int))
            {
                return "int";
            }
            if (type == typeof(DateOnly))
            {
                return "date";
            }
            throw new Exception();
        }
        public static Type StringToType(string str)
        {
            if (str == "string")
            {
                return typeof(string);
            }
            if (str == "int")
            {
                return typeof(int);
            }
            if (str == "date")
            {
                return typeof(DateOnly);
            }
            return typeof(string);
        }
        public static void ConditionParser(string[] parts,DbList<Column> cols,out DbList<Condition> conditions, out bool distinct, out bool orderby,out string columnfororder)
        {
            columnfororder = "";
            distinct = false;
            orderby = false;
            conditions = new DbList<Condition>();
            DbList<string> columnnames = new DbList<string>();
            foreach(Column c in cols)
            {
                columnnames.Add(c.ColumnName);
            }
            if(parts.Length <3)
            {
                ErrorDisplay("Invalid usage of WHERE statement");               
                return;
            }
            Condition condition = new Condition();
            bool first = true;
            int wholewordlength = 0;
            for (int i = 0; i < parts.Length; i += 3, wholewordlength += 3)
            {
                bool andCondition = true;
                bool notOperator = false;
                int wordcount = 3;
                //проверяваме дали subcondition-а е с достатъчно елементи
                if (!first)
                {
                    if (parts[i].ToUpper() =="DISTINCT")
                    {
                        distinct = true;
                        wholewordlength++;
                        i++;
                    }
                    if (parts.Length>wholewordlength+2)
                    {
                        if (parts[i].ToUpper() == "ORDER")
                        {

                            if (parts[i].ToUpper() == "ORDER" && parts[i + 1].ToUpper() == "BY" && cols.Exists(c => c.ColumnName == parts[i + 2]))
                            {
                                orderby = true;
                                columnfororder = parts[i + 2];
                            }
                            else if (parts[i].ToUpper() == "ORDER")
                            {
                                ErrorDisplay($"invalid closing tag: {parts[i]} {parts[i + 1]} {parts[i + 2]} ");
                                return;
                            }
                            if (parts.Length > wholewordlength + 3)
                            {
                                if (parts[i + 3] == "DISTINCT")
                                {

                                    if (!distinct)
                                    {
                                        distinct = true;
                                        conditions.Add(condition);
                                        return;
                                    }
                                    else if (distinct)
                                    {
                                        ErrorDisplay("Cannot apply DISTINCT twice");
                                        return;
                                    }
                                    else
                                    {
                                        ErrorDisplay($"Input not valid: {parts[i + 3]}");

                                        return;
                                    }
                                }

                            }
                        }
                    }
                    if(distinct)
                    {
                        conditions.Add(condition) ;
                        return;
                    }
                    if(orderby)
                    {
                        conditions.Add(condition);
                        return;
                    }
                }
                if (parts.Length < i + 3)
                {
                    ErrorDisplay($"Invalid query usage: {parts[i]} ");
                    return;
                }
                
                if(!first)
                {
                    if(parts[i].ToUpper()=="AND")
                    {
                        
                        andCondition = true;
                        i++;
                        wordcount++;
                        wholewordlength++;
                    }
                    else if (parts[i] == "OR")
                    {
                      
                        andCondition = false;
                        wordcount++;
                        i++;
                        wholewordlength++;
                    }
                    else
                    {
                        ErrorDisplay($"Invalid query operator: {parts[i]}");
                        return;
                    }
                }
                if(first)
                {
                    first = false;
                }
                if (parts[i].ToUpper() == "NOT")
                {
                    i++;
                    notOperator = true;
                    wholewordlength++;
                }
                if (!columnnames.Exists(c => c == parts[i]))
                {
                    ErrorDisplay($"Invalid column: {parts[i]}");
                    return;
                }
                if(cols.Where(c => c.ColumnName == parts[i])[0].DataType == typeof(string) && parts[i+1]!="=")
                {
                    ErrorDisplay($"Invalid operator {parts[i+1]} for column {parts[i]}");
                    return;
                }
                if (parts[i + 1] != ">" && parts[i + 1] != "<" && parts[i + 1] != "=")
                {
                    ErrorDisplay($"Invalid operator: {parts[i + 1]}");
                    return;
                }
                if (cols.Where(c => c.ColumnName == parts[i])[0].DataType == typeof(int) && !int.TryParse(parts[i + 2],out int value))
                {
                    ErrorDisplay($"Not valid value for an integer: {parts[i + 2]}");
                    return;
                }
                if (cols.Where(c => c.ColumnName == parts[i])[0].DataType == typeof(DateOnly) && !DateOnly.TryParse(parts[i + 2], out DateOnly date))
                {
                    ErrorDisplay($"Not valid value for a date: {parts[i + 2]}");
                    return;
                }
                if (!andCondition)
                {
                    conditions.Add(condition);
                    condition = new Condition();
                }

                    Type datatype = cols.Where(c=>c.ColumnName
                    == parts[i])[0].DataType;
                    
                
                    condition.SubConditions.Add(new SubCondition()
                    {
                        NotOperator = notOperator,
                        Type = datatype,
                        strings = new DbList<string>(parts[i], parts[i + 1], parts[i + 2])
                        
                    });
                

            }
            conditions.Add(condition); 
        }
        public static void DeleteConditionParser(string[] parts, DbList<Column> cols, out DbList<Condition> conditions)
        {
            
            conditions = new DbList<Condition>();
            DbList<string> columnnames = new DbList<string>();
            foreach (Column c in cols)
            {
                columnnames.Add(c.ColumnName);
            }
            if (parts.Length < 3)
            {
                ErrorDisplay("Invalid usage of WHERE statement");
                return;
            }
            Condition condition = new Condition();
            bool first = true;
            int wholewordlength = 0;
            for (int i = 0; i < parts.Length; i += 3, wholewordlength += 3)
            {
                bool andCondition = true;
                bool notOperator = false;
                int wordcount = 3;
                //проверяваме дали subcondition-а е с достатъчно елементи
                
                if (parts.Length < i + 3)
                {
                    ErrorDisplay($"Invalid query usage: {parts[i]} ");
                    return;
                }

                if (!first)
                {
                    if (parts[i].ToUpper() == "AND")
                    {

                        andCondition = true;
                        i++;
                        wordcount++;
                        wholewordlength++;
                    }
                    else if (parts[i] == "OR")
                    {

                        andCondition = false;
                        wordcount++;
                        i++;
                        wholewordlength++;
                    }
                    else
                    {
                        ErrorDisplay($"Invalid query operator: {parts[i]}");
                        return;
                    }
                }
                if (first)
                {
                    first = false;
                }
                if (parts[i].ToUpper() == "NOT")
                {
                    i++;
                    notOperator = true;
                    wholewordlength++;
                }
                if (!columnnames.Exists(c => c == parts[i]))
                {
                    ErrorDisplay($"Invalid column: {parts[i]}");
                    return;
                }
                if (cols.Where(c => c.ColumnName == parts[i])[0].DataType == typeof(string) && parts[i + 1] != "=")
                {
                    ErrorDisplay($"Invalid operator {parts[i + 1]} for column {parts[i]}");
                    return;
                }
                if (parts[i + 1] != ">" && parts[i + 1] != "<" && parts[i + 1] != "=")
                {
                    ErrorDisplay($"Invalid operator: {parts[i + 1]}");
                    return;
                }
                if (cols.Where(c => c.ColumnName == parts[i])[0].DataType == typeof(int) && !int.TryParse(parts[i + 2], out int value))
                {
                    ErrorDisplay($"Not valid value for an integer: {parts[i + 2]}");
                    return;
                }
                if (cols.Where(c => c.ColumnName == parts[i])[0].DataType == typeof(DateOnly) && !DateOnly.TryParse(parts[i + 2], out DateOnly date))
                {
                    ErrorDisplay($"Not valid value for a date: {parts[i + 2]}");
                    return;
                }
                if (!andCondition)
                {
                    conditions.Add(condition);
                    condition = new Condition();
                }

                Type datatype = cols.Where(c => c.ColumnName
                == parts[i])[0].DataType;
                

                condition.SubConditions.Add(new SubCondition()
                {
                    NotOperator = notOperator,
                    Type = datatype,
                    strings = new DbList<string>(parts[i], parts[i + 1], parts[i + 2])

                });


            }
            conditions.Add(condition);
        }
        public static void RecreateTable(string tablename)
        {
            if (tablename == null) { return; }
            if (!File.Exists(FilePath+"\\"+tablename+".dat"))
            {
                return;
            }
            string tablePath = FilePath + "\\" + tablename + ".dat";
            string tablePath2 = FilePath + "\\" +tablename+ "replacement" + ".dat";
            DbList<Column> columns = new DbList<Column>();
            FileStream fs = new FileStream(tablePath, FileMode.Open, FileAccess.ReadWrite);
            BinaryReader reader = new BinaryReader(fs);
            BinaryWriter writer = new BinaryWriter(fs);
            FileStream fs2 = new FileStream(tablePath2, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            BinaryReader reader2 = new BinaryReader(fs2);
            BinaryWriter writer2 = new BinaryWriter(fs2);
            using (fs)
            using (reader)
            using(writer)
            using (fs2)
            using (reader2)
            using (writer2)
            {
                columns = DeserializeColumns(reader);
                SerializeColumns(columns, writer2);
                long firstemptyposition = fs2.Position;
                fs2.Seek(3, SeekOrigin.Begin);
                writer2.Write(firstemptyposition);
                columns = DeserializeColumns(reader2);


                reader.BaseStream.Seek(11, SeekOrigin.Begin);
                long address = reader.ReadInt64();
                fs.Seek(address, SeekOrigin.Begin);
                if (address != 0)
                {

                Record record = ReadRecord(reader, address, columns, fs);
                while (record.nextRecordAddress != 0)
                {
                    AddRecord(record, writer2, reader2, fs2, columns);
                    record = ReadRecord(reader, record.nextRecordAddress, columns, fs);
                }
                AddRecord(record, writer2, reader2, fs2, columns);

                }
                    



            }
            File.Delete(tablePath);
            File.Move(tablePath2, tablePath);
            
            
        }


    }
}
