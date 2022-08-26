using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Linq;

namespace TextDumper
{
    class Program
    {
        static void DumpText(string dictPath, string tableName, string[] targetField, SqliteConnection conn, string refinementField = "", int refinementNum = -1)
        {
            var cmd = conn.CreateCommand();

            var targetFieldStr = @"""" + String.Join(@""",""", targetField) + @"""";

            cmd.CommandText =
            $@" SELECT {targetFieldStr}
                FROM {tableName}
            ";
            if (refinementField != "")
                cmd.CommandText += $@" WHERE {refinementField} = {refinementNum}";

            var dumpedText = "";

            using (SqliteDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    for (int i = 0; i < targetField.Length; i++)
                    {
                        dumpedText += reader.GetString(i);
                        if (i == targetField.Length - 1)
                            dumpedText += "\r\n";
                        else
                            dumpedText += "\t";
                    }
                }
            }

            File.WriteAllText(dictPath, dumpedText);
        }

        static void Main(string[] args)
        {
            var localappdata = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            var dbPath = localappdata + "Low\\Cygames\\umamusume\\master\\master.mdb";
            var dumpPath = "dumped_data";
            if (!Directory.Exists(dumpPath))
                Directory.CreateDirectory(dumpPath);

            if (File.Exists(dbPath))
            {
                using (SqliteConnection conn = new($"Data Source={dbPath}"))
                {
                    conn.Open();

                    Console.WriteLine("キャラIDと名前をダンプしています...");
                    DumpText(dumpPath + "/キャラIDと名前.txt", "text_data", new string[] { "index", "text" }, conn, "category", 6);

                    Console.WriteLine("衣装IDと名前をダンプしています...");
                    DumpText(dumpPath + "/衣装IDと名前.txt", "text_data", new string[] { "index", "text" }, conn, "category", 14);

                    Console.WriteLine("ライブIDと名前をダンプしています...");
                    DumpText(dumpPath + "/ライブIDと名前.txt", "text_data", new string[] { "index", "text" }, conn, "category", 16);

                    conn.Close();
                }
            }

            Console.WriteLine("完了しました。 ダンプ先のフォルダを開きますか？(Y)");
            var pressedKey = Console.ReadKey();
            if (pressedKey.Key == ConsoleKey.Y)
                System.Diagnostics.Process.Start("explorer.exe", dumpPath);
        }
    }
}
