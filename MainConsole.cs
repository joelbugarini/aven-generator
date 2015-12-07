﻿using System;
using System.Collections.Generic;

using System.Text;

using System.Data.SqlClient;
using System.Reflection;
using System.IO;

namespace hix
{
    class MainConsole
    {
        static void Main(string[] args)
        {
            //Validate args
            if (args.Length > 0)
            {
                //Read Config File
                Config config = new Config().ReadConfig();

                //Get the DB schema
                Console.WriteLine("hix Class Generator is connecting to Database " + config.Server + "\\" + config.Database);

                //try
                //{
                    Database db = config.GenerateDb(config);

                    //Read File
                    
                    Console.WriteLine("Reading the " + args[0] + " File");
                    RootNode rootNode = new RootNode();
                    rootNode.Create(args[0], db);

                    //Map File to Generate Code

                    foreach (TableNode tableNode in rootNode.TableNodes)
                    {
                        foreach (Table table in db.Tables)
                        {
                            table.Content = tableNode.Scheme;
                            foreach (ColumnNode columnNode in tableNode.ColumnNodes)
                            {
                                string dataColumnNode = "";
                                foreach (Column column in table.Columns)
                                {
                                    dataColumnNode += columnNode.Scheme.Replace(
                                        "[[column.name]]", column.Name).Replace(
                                        "[[column.type]]", column.Type).Replace(
                                        "[[project.name]]", config.Project).Replace(
                                        "[[database.name]]", config.Database);
                                }



                                table.Content = ReplaceFirst(table.Content, "[[columns]]", dataColumnNode).Replace(
                                    "[[table.name]]", table.Name).Replace(
                                    "[[project.name]]", config.Project).Replace(
                                    "[[database.name]]", config.Database);

                            }
                        }

                    }
                    
                    if (IsTableNextToRoot(rootNode.Original))
                    {
                        foreach (Table table in db.Tables)
                        {
                            string file = Path.GetFileNameWithoutExtension(args[0]);
                            string extension = Path.GetExtension(args[0]);
                            (new FileInfo(file + "/")).Directory.Create();
                            CreateFile(file + "/" + file + table.Name + extension, table.Content);
                            Console.WriteLine(file + "/" + file + table.Name + extension + " Created");
                        }
                    }
                    else
                    {
                        string document = "";
                        foreach (Table table in db.Tables)
                        {
                            document += table.Content;
                        }
                        document = rootNode.Scheme.Replace("[[table.container]]", document);

                        string file = Path.GetFileNameWithoutExtension(args[0]);
                        string extension = Path.GetExtension(args[0]);
                        (new FileInfo(file + "/")).Directory.Create();
                        CreateFile(file + "/" + file + extension, document);
                        Console.WriteLine(file + "/" + file + extension + " Created");

                    }

                    //FileLoop();

                    //Save Code on Disk


                //}
                //catch { Console.WriteLine("Can't reach " + config.GetSqlCon()); }
            }
            else { Console.WriteLine("This is the hix's Class Generator\n Provide \"hix -all\" to interpretate all *.hix files\n Provide \"hix {file name}\" to interpretate specific file"); }


           // Console.ReadLine();
        }
        
        private static bool IsTableNextToRoot(string rootNode)
        {
            string text = rootNode.Replace(" ", "").Replace("\r", "").Replace("\n", "");
            int rootHead = text.IndexOf("[<]");
            int rootTail = text.IndexOf("[>]");

            int tableHead = text.IndexOf("[[table]]");
            int tableTail = text.IndexOf("[>]");

            bool result = false;
            if ((tableHead - rootHead) == "[<]".Length && (rootTail - tableTail -1) == "[>]".Length) result = true;
            return result;
        }

        public static void CreateFile(string path, string data)
        {
            using (TextWriter writer = File.CreateText(path))
            {
                writer.WriteLine(data);
            }
        }

        public static string ReplaceFirst(string text, string search, string replace)
        {
            int pos = text.IndexOf(search);
            if (pos < 0)
            {
                return text;
            }
            return text.Substring(0, pos) + replace + text.Substring(pos + search.Length);
        }
    }
}
