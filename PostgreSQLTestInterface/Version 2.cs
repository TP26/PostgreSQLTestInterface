using Npgsql;

namespace PostgreSQLTestInterface
{
    internal class Version_2
    {
        //2 - data is parsed into data insertion statements that are then stored inside of subroutines.
        //Experiment with insertion statement concatenation and the failures of these statements
        public static void Version2(NpgsqlConnection conn)
        {
            Object[] objects = GetTestObjects();
            Console.WriteLine("Beginning individual insertion test");
            Console.WriteLine("Creating object table");
            executeQuery(conn, "drop table if exists objecttable;");
            executeQuery(conn, "create table ObjectTable (Id Int, Name varchar, Size Int, Weight Int, Location Int);");

            Console.WriteLine("Inserting records into object table");
            insertObjectsIntoDatabase(conn, objects);

            Console.WriteLine("Retrieving records from object table");
            List<List<string>> objectRecords = retrieveRecords(conn, "select * from ObjectTable;");

            List<Object> objectsFromRecords = new List<Object>();

            foreach (List<string> objectRecord in objectRecords)
            {
                int Id = Int32.Parse(objectRecord[0]);
                string name = objectRecord[1];
                int size = Int32.Parse(objectRecord[2]);
                int weight = Int32.Parse(objectRecord[3]);
                int location = Int32.Parse(objectRecord[4]);

                Object recordObject =  new Object { Id = Id, Name = name, Size = size, Weight = weight, Location = location};
                objectsFromRecords.Add(recordObject);
            }

            Console.WriteLine("Comparing inserted objects with those retrieved from the database");
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

            Console.WriteLine("Beginning concatenated insertion test");
            Console.WriteLine("Creating object table");
            executeQuery(conn, "drop table if exists objecttable;");
            executeQuery(conn, "create table ObjectTable (Id Int, Name varchar, Size Int, Weight Int, Location Int);");

            Console.WriteLine("Inserting records into object table");
            insertObjectsIntoDatabaseConcatenated(conn, objects);

            Console.WriteLine("Retrieving records from object table");
            objectRecords = retrieveRecords(conn, "select * from ObjectTable;");

            objectsFromRecords = new List<Object>();

            foreach (List<string> objectRecord in objectRecords)
            {
                int Id = Int32.Parse(objectRecord[0]);
                string name = objectRecord[1];
                int size = Int32.Parse(objectRecord[2]);
                int weight = Int32.Parse(objectRecord[3]);
                int location = Int32.Parse(objectRecord[4]);

                Object recordObject = new Object { Id = Id, Name = name, Size = size, Weight = weight, Location = location };
                objectsFromRecords.Add(recordObject);
            }

            Console.WriteLine("Comparing inserted objects with those retrieved from the database");
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

            Console.WriteLine("Beginning faulty table insertion tests");
            try
            {
                executeQuery(conn, $"INSERT INTO ObjectTable(Id, Name, Size, Weight, Location) VALUES (A, 15, B, C, 5);");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Faulty insertion statement returned error as expected: {ex.Message}");
            }
            try
            {
                executeQuery(conn, $"INSERT INTO ObjectTable(Id, Name, Size, Weight, Location) VALUES (1, 'Test', 2, 3, 4);" +
                    $"INSERT INTO ObjectTable(Id, Name, Size, Weight, Location) VALUES ('2', 3, g, h, j);" +
                    $"INSERT INTO ObjectTable(Id, Name, Size, Weight, Location) VALUES (3, 'Test2', 4, 5, 6);");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Faulty concatenated insertion statement returned error as expected: {ex.Message}");
            }
        }

        static List<List<string>> retrieveRecords(NpgsqlConnection conn, string query)
        {
            List<List<string>> records = new List<List<string>>();

            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    List<string> record = new List<string>();
                    int valuePosition = 0;
                    bool readValue = true;

                    while (readValue)
                    {
                        try
                        {
                            string fieldValue = reader.GetValue(valuePosition).ToString() ?? "";
                            record.Add(fieldValue);
                            valuePosition++;
                        }
                        catch (Exception ex)
                        {
                            readValue = false;
                        }
                    }

                    records.Add(record);
                }
            }

            return records;
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