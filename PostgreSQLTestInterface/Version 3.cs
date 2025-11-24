using Npgsql;

namespace PostgreSQLTestInterface
{
    public class Version_3
    {
        //3 - Subroutines only pass in objects for insertion and only receive objects when returned
        public static void Version3(NpgsqlConnection conn)
        {
            Object[] objects = GetTestObjects();
            Console.WriteLine("Initialising object table");
            initialiseObjectTable(conn, objects);

            Console.WriteLine("Retrieving objects from table");
            List<Object> objectsFromRecords = retrieveObjects(conn);
            Console.WriteLine("Comparing inserted objects with records retrieved from table");
            compareObjectLists(objects, objectsFromRecords);

            Console.WriteLine("Recreating object table");
            createObjectTable(conn);

            Console.WriteLine("Inserting objects into object table using concatenated statements");
            insertObjectsIntoDatabaseConcatenated(conn, objects);
            Console.WriteLine("Retrieving objects from table");
            objectsFromRecords = retrieveObjects(conn);
            Console.WriteLine("Comparing inserted objects with records retrieved from table");
            compareObjectLists(objects, objectsFromRecords);
        }

        private static void compareObjectLists(Object[] objects, List<Object> objectsFromRecords)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                Object recordObject = objectsFromRecords[i];
                Object originalObject = objects[i];

                if (recordObject.Id != originalObject.Id)
                {
                    Console.WriteLine($"Ids do not match");
                }

                if (recordObject.Name != originalObject.Name)
                {
                    Console.WriteLine($"Names do not match");
                }

                if (recordObject.Size != originalObject.Size)
                {
                    Console.WriteLine($"Sizes do not match");
                }

                if (recordObject.Weight != originalObject.Weight)
                {
                    Console.WriteLine($"Weights do not match");
                }

                if (recordObject.Location != originalObject.Location)
                {
                    Console.WriteLine($"Locations do not match");
                }
            }
        }

        private static void initialiseObjectTable(NpgsqlConnection conn, Object[] objects)
        {
            createObjectTable(conn);
            insertObjectsIntoDatabase(conn, objects);
        }

        private static void createObjectTable(NpgsqlConnection conn)
        {
            executeQuery(conn, "drop table if exists objecttable;");
            executeQuery(conn, "create table ObjectTable (Id Int, Name varchar, Size Int, Weight Int, Location Int);");
        }

        static List<List<string>> retrieveRecords(NpgsqlConnection conn, string query)
        {
            List<List<string>> records = new List<List<string>>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                int valuePosition = 0;
                List<string> record = new List<string>();
                while (reader.Read())
                {
                    string fieldValue = (string)reader.GetString(valuePosition);
                    record.Add(fieldValue);
                    valuePosition++;
                }

                records.Add(record);
            }

            return records;
        }

        static List<Object> retrieveObjects(NpgsqlConnection conn)
        {
            List<Object> objects = new List<Object>();

            using (NpgsqlCommand cmd = new NpgsqlCommand("select * from ObjectTable;", conn))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    int Id = (int)reader.GetInt32(0);
                    string name = (string)reader.GetString(1);
                    int size = (int)reader.GetInt32(2);
                    int weight = (int)reader.GetInt32(3);
                    int location = (int)reader.GetInt32(4);

                    objects.Add(new Object { Id = Id, Name = name, Size = size, Weight = weight , Location = location });
                }
            }

            return objects;
        }

        private static void insertObjectsIntoDatabase(NpgsqlConnection conn, Object[] objects)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                Object insertObject = objects[i];
                string sqlCommand = $"INSERT INTO ObjectTable(Id, Name, Size, Weight, Location) VALUES ({insertObject.Id}, '{insertObject.Name}', {insertObject.Size}, {insertObject.Weight}, {insertObject.Location});";
                executeQuery(conn, sqlCommand);
            }
        }

        private static void insertObjectsIntoDatabaseConcatenated(NpgsqlConnection conn, Object[] objects)
        {
            string insertionCommand = "";
            for (int i = 0; i < objects.Length; i++)
            {
                Object insertObject = objects[i];
                insertionCommand += $"INSERT INTO ObjectTable(Id, Name, Size, Weight, Location) VALUES ({insertObject.Id}, '{insertObject.Name}', {insertObject.Size}, {insertObject.Weight}, {insertObject.Location});";
            }

            executeQuery(conn, insertionCommand);
        }

        private static Object[] GetTestObjects()
        {
            return new Object[]
            {
                new Object() { Id = 1, Name = "Slab", Size = 10, Weight = 10, Location = 3},
                new Object() { Id = 2, Name = "Stone", Size = 2, Weight = 1, Location = 3},
                new Object() { Id = 3, Name = "Shard", Size = 4, Weight = 7, Location = 4},
                new Object() { Id = 4, Name = "Block", Size = 5, Weight = 4 , Location = 5},
                new Object() { Id = 5, Name = "Sphere", Size = 8, Weight = 9, Location = 5}
            };
        }

        private static void executeQuery(NpgsqlConnection conn, string query)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
