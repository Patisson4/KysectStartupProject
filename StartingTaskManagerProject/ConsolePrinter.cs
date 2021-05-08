using System;
using System.Data;
using System.Linq;

namespace StartingTaskManagerProject
{
    public static class ConsolePrinter
    {
        private static void Print(DataTable table)
        {
            foreach (DataColumn c in table.Columns)
                Console.Write("{0,-20}", c.ColumnName);
            Console.WriteLine();

            foreach (DataRow r in table.Rows)
            {
                foreach (DataColumn c in table.Columns)
                    Console.Write("{0,-20}", r[c]);
                Console.WriteLine();
            }
        }
        
        [Obsolete("Used for Newсomer")]
        public static void Show(Manager<Task> manager, string mode = "standard")
        {
            var table = new DataTable("Content of Manager");
            
            var column = new DataColumn {DataType = typeof(string), ColumnName = "Type"};
            table.Columns.Add(column);
            
            column = new DataColumn {DataType = typeof(string), ColumnName = "ID"};
            table.Columns.Add(column);

            column = new DataColumn {DataType = typeof(string), ColumnName = "TaskInfo"};
            table.Columns.Add(column);

            column = new DataColumn {DataType = typeof(string), ColumnName = "Сompleteness"};
            table.Columns.Add(column);

            var selected = mode switch
            {
                "standard" => manager.Data,
                "completed" => manager.Data.Where(item => item.Value.IsCompleted),
                _ => throw new ArgumentException("Wrong mode of function Show(Manager<Task> manager, string mode)")
            };
            
            foreach (var (key, value) in selected)
            {
                var row = table.NewRow();
                row["Type"] = "Task";
                row["ID"] = key;
                row["TaskInfo"] = value.TaskInfo;
                row["Сompleteness"] = value.IsCompleted;
                table.Rows.Add(row);
            }

            Print(table);
        }

        public static void Show(Manager<SuperTask> manager, string mode = "standard")
        {
            var table = new DataTable("Content of Manager");
            
            var column = new DataColumn {DataType = typeof(string), ColumnName = "Type"};
            table.Columns.Add(column);
            
            column = new DataColumn {DataType = typeof(string), ColumnName = "ID"};
            table.Columns.Add(column);

            column = new DataColumn {DataType = typeof(string), ColumnName = "TaskInfo"};
            table.Columns.Add(column);

            column = new DataColumn {DataType = typeof(string), ColumnName = "Сompleteness"};
            table.Columns.Add(column);

            column = new DataColumn {DataType = typeof(string), ColumnName = "Deadline"};
            table.Columns.Add(column);

            var selected = mode switch
            {
                "standard" => manager.Data,
                "completed" => manager.Data.Where(item => item.Value.IsCompleted),
                "today" => manager.Data.Where(item => item.Value.Deadline == DateTime.Today),
                _ => throw new ArgumentException("Wrong mode of function Show(Manager<SuperTask> manager, string mode)")
            };

            foreach (var (key, value) in selected)
            {
                var row = table.NewRow();
                row["Type"] = "SuperTask";
                row["ID"] = key;
                row["TaskInfo"] = value.TaskInfo;
                row["Сompleteness"] = value.Subs.Data.Count > 0 ? "" + value.Subs.CountCompleted + '/' + value.Subs.Data.Count : value.IsCompleted;
                row["Deadline"] = value.Deadline?.ToString("d") ?? "not set";
                table.Rows.Add(row);
                foreach (var (subKey, subValue) in value.Subs.Data)
                {
                    row = table.NewRow();
                    row["Type"] = "├─── Task";
                    row["ID"] = subKey;
                    row["TaskInfo"] = subValue.TaskInfo;
                    row["Сompleteness"] = subValue.IsCompleted;
                    row["Deadline"] = "---";
                    table.Rows.Add(row);
                }
            }

            Print(table);
        }
        
    }
}