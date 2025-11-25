using Npgsql;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;

namespace PostgreSQLTestInterface
{
    internal class Version_4
    {
        //4 - Subroutines become further genericised where the conversion function is passed in with the object type on retrieval.
        public static void Version4(NpgsqlConnection conn)
        {
            //Creating object table
            Console.WriteLine("Creating object table");
            createTable<Object>(conn, Table.ObjectTable);
            Object[] testObjects = GetTestObjects();
            Console.WriteLine("Inserting into object table");
            insertObjectsIntoDatabase<Object>(conn, testObjects, Table.ObjectTable);
            Console.WriteLine("Retrieving objects from object table");
            List<Object> databaseObjects = retrieveObjects<Object>(conn, Table.ObjectTable);
            Console.WriteLine("Comparing inserted objects with objects retrieved from database");
            compareObjectLists<Object>(testObjects, databaseObjects);

            //Modifying object record
            Console.WriteLine("Modifying object record in database");
            testObjects[0].Name = "New Shard";
            testObjects[0].Location = 10;
            testObjects[0].Size = 10;
            testObjects[0].Weight = 10;
            updateRecord<Object>(conn, testObjects[0], Table.ObjectTable);
            Console.WriteLine("Checking object modification");
            databaseObjects = retrieveObjects<Object>(conn, Table.ObjectTable);
            compareObjectLists<Object>(testObjects, databaseObjects);

            //Creating island table
            Console.WriteLine("Creating island table");
            createTable<Island>(conn, Table.IslandTable);
            List<Island> testIslands = getTestIslands();
            Console.WriteLine("Inserting into island table");
            insertObjectsIntoDatabase<Island>(conn, testIslands.ToArray(), Table.IslandTable);
            Console.WriteLine("Retrieving islands from island table");
            List<Island> databaseIslands = retrieveObjects<Island>(conn, Table.IslandTable);
            Console.WriteLine("Comparing inserted islands with islands retrieved from database");
            compareObjectLists<Island>(testIslands, databaseIslands.ToArray());

            //Creating ship table
            Console.WriteLine("Creating ship table");
            createTable<Ship>(conn, Table.ShipTable);
            List<Ship> testShips = getTestShips();
            Console.WriteLine("Inserting into ship table");
            insertObjectsIntoDatabase<Ship>(conn, testShips.ToArray(), Table.ShipTable);
            Console.WriteLine("Retrieving ships from ship table");
            List<Ship> databaseShips = retrieveObjects<Ship>(conn, Table.ShipTable);
            Console.WriteLine("Comparing inserted ships with ships retrieved from database");
            compareObjectLists<Ship>(testShips, databaseShips.ToArray());
        }

        private static void updateRecord<T>(NpgsqlConnection conn, T record, Table table)
        {
            string updateQuery = $"update {table} set ";
            bool firstProperty = true;
            string? primaryKeyName = null;
            string? primaryKeyValue = null;

            Type type = typeof(T);
            foreach (PropertyInfo property in type.GetProperties())
            {
                CategoryAttribute[] attributes = property.GetCustomAttributes(typeof(CategoryAttribute), false) as CategoryAttribute[];
                bool isPrimaryKey = false;

                foreach (CategoryAttribute attribute in attributes)
                {
                    if (attribute.Category == "PrimaryKey")
                    {
                        isPrimaryKey = true;
                    }
                }

                if (isPrimaryKey)
                {
                    primaryKeyName = property.Name;
                    primaryKeyValue = $"{property.GetValue(record) ?? ""}";
                }
                else
                {
                    if (!firstProperty)
                    {
                        updateQuery += ", ";
                    }
                    else
                    {
                        firstProperty = false;
                    }

                    object? propertyvalue = property.GetValue(record);
                    updateQuery += $"{property.Name} = ";

                    if (property.PropertyType.Name == "String")
                    {
                        string insertionString = (propertyvalue as string) ?? "";
                        updateQuery += $"'{insertionString}'";
                    }
                    else
                    {
                        int insertionNumber = (int)propertyvalue;
                        updateQuery += $"{insertionNumber}";
                    }
                }
            }

            if (primaryKeyName == null)
            {
                throw new Exception("No primary key value found");
            }
            else
            {
                updateQuery += $" where {primaryKeyName} = {primaryKeyValue}";
            }

            executeQuery(conn, updateQuery);
        }

        private static List<T> retrieveObjects<T>(NpgsqlConnection conn, Table table) where T : new()
        {
            List<T> retrievedObjects = new List<T>();
            Type type = typeof(T);

            string query = $"select * from {table};";
            using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    T retrievedObject = new T();
                    int readerPosition = 0;

                    foreach (PropertyInfo property in type.GetProperties())
                    {
                        var propertyValue = Convert.ChangeType(reader.GetValue(readerPosition), property.PropertyType);
                        property.SetValue(retrievedObject, propertyValue);
                        readerPosition++;
                    }

                    retrievedObjects.Add(retrievedObject);
                }
            }

            return retrievedObjects;
        }

        static void createTable<T>(NpgsqlConnection conn, Table table)
        {
            executeQuery(conn, $"drop table if exists {table};");

            string createTableQuery = $"create table {table} (";

            Type type = typeof(T);
            bool firstProperty = true;
            foreach (PropertyInfo property in type.GetProperties())
            {
                if (!firstProperty)
                {
                    createTableQuery += ", ";
                }
                else
                {
                    firstProperty = false;
                }

                string propertyType;

                switch (property.PropertyType.Name)
                {
                    case "Int32":
                        propertyType = "Int";
                        break;
                    case "String":
                        propertyType = "varchar";
                        break;
                    default:
                        propertyType = property.PropertyType.Name;
                        break;
                }

                createTableQuery += $"{property.Name} {propertyType}";
            }

            createTableQuery += ");";

            executeQuery(conn, createTableQuery);
        }

        private static void executeQuery(NpgsqlConnection conn, string query)
        {
            try
            {
                using (NpgsqlCommand cmd = new NpgsqlCommand(query, conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error executing query:\n{query}\nException message: {ex.Message}");
            }
        }
        

        private static void insertObjectsIntoDatabase<T>(NpgsqlConnection conn, T[] insertionObjects, Table table)
        {
            Type type = typeof(T);
            string insertionStatement = $"INSERT INTO {table}(";
            bool firstProperty = true;

            foreach (PropertyInfo property in type.GetProperties())
            {
                if (!firstProperty)
                {
                    insertionStatement += ", ";
                }
                else
                {
                    firstProperty = false;
                }

                insertionStatement += property.Name;
            }

            insertionStatement += ") VALUES (";
            string statementStart = insertionStatement;

            for (int i = 0; i < insertionObjects.Length; i++)
            {
                insertionStatement = statementStart;
                T insertionObject = insertionObjects[i];
                firstProperty = true;
                foreach (PropertyInfo property in type.GetProperties())
                {
                    object? propertyvalue = property.GetValue(insertionObject);

                    if (!firstProperty)
                    {
                        insertionStatement += ", ";
                    }
                    else
                    {
                        firstProperty = false;
                    }

                    if (property.PropertyType.Name == "String")
                    {
                        string insertionString = (propertyvalue as string) ?? "";
                        insertionStatement += $"'{insertionString}'";
                    }
                    else
                    {
                        int insertionNumber = (int)propertyvalue;
                        insertionStatement += $"{insertionNumber}";
                    }
                }

                insertionStatement += ");";

                executeQuery(conn, insertionStatement);
            }
        }

        private static void compareObjectLists<T>(List<T> objects, List<T> objectsFromRecords)
        {
            Type type = typeof(T);

            try
            {
                for (int i = 0; i < objects.Count; i++)
                {
                    object? recordObject = objectsFromRecords[i];
                    object? originalObject = objects[i];

                    foreach (PropertyInfo property in type.GetProperties())
                    {
                        try
                        {
                            if (compareObjectValues(originalObject, recordObject) == false)
                            {
                                Console.WriteLine("Objects being compared are not equal");
                                Console.WriteLine("Object1");
                                displayObject(recordObject);
                                Console.WriteLine("Object2");
                                displayObject(originalObject);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Exception whilst comparing objects: {ex.Message}");
                        }
                    }
                }
            }
            catch(Exception ex)
            {
                Console.WriteLine($"Error attempting to compare object lists - {ex.Message}");
            }
        }

        private static bool compareObjectValues<T>(T objectOne, T objectTwo)
        {
            bool objectsMatch = true;
            Type type = typeof(T);

            foreach (PropertyInfo property in type.GetProperties())
            {
                var objectOneValue = property.GetValue(objectOne);
                var objectTwoValue = property.GetValue(objectTwo);

                try
                {
                    if (objectOneValue != objectTwoValue)
                    {
                        Console.WriteLine($"Object property of {property.Name} not equal: {objectOneValue} - {objectTwoValue}");
                        objectsMatch = false;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception whilst comparing objects: {ex.Message}");
                }
            }

            return objectsMatch;
        }

        private static void displayObject<T>(T objectToDisplay)
        {
            Type type = typeof(T);
            foreach (PropertyInfo property in type.GetProperties())
            {
                string propertyType = property.PropertyType.Name;
                string propertyName = property.Name;

                Console.WriteLine($"{propertyType} {propertyName} : {property.GetValue(objectToDisplay)}");
            }
        }

        private static void compareObjectLists<T>(T[] objects, T[] objectsFromRecords)
        {
            List<T> objectsList = objects.ToList();
            List<T> objectsFromRecordsList = objectsFromRecords.ToList();
            compareObjectLists<T>(objectsList, objectsFromRecordsList);
        }

        private static void compareObjectLists<T>(List<T> objects, T[] objectsFromRecords)
        {
            List<T> objectsFromRecordsList = objectsFromRecords.ToList();
            compareObjectLists<T>(objects, objectsFromRecordsList);
        }

        private static void compareObjectLists<T>(T[] objects, List<T> objectsFromRecords)
        {
            List<T> objectsList = objects.ToList();
            compareObjectLists<T>(objectsList, objectsFromRecords);
        }

        enum Table
        {
            [Description("ObjectTable")]
            ObjectTable = 0,
            [Description("IslandTable")]
            IslandTable = 1,
            [Description("ShipTable")]
            ShipTable = 2,
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

        private static List<Island> getTestIslands()
        {
            return new List<Island>() 
            {
                new Island(){ Id = 1, Name = "Island 1", XCoOrdinate = 150, YCoOrdinate = 450, Area = 5100},
                new Island(){ Id = 2, Name = "Island 2", XCoOrdinate = 0, YCoOrdinate = 1000, Area = 3200},
                new Island(){ Id = 3, Name = "Island 3", XCoOrdinate = 1000, YCoOrdinate = 600, Area = 900},
                new Island(){ Id = 4, Name = "Island 4", XCoOrdinate = 60, YCoOrdinate = 1200, Area = 10},
                new Island(){ Id = 5, Name = "Island 5", XCoOrdinate = 600, YCoOrdinate = 70, Area = 750},
                new Island(){ Id = 6, Name = "Island 6", XCoOrdinate = 1000, YCoOrdinate = 1000, Area = 1000},
                new Island(){ Id = 7, Name = "Island 7", XCoOrdinate = 750, YCoOrdinate = 750, Area = 7500},
                new Island(){ Id = 8, Name = "Island 8", XCoOrdinate = 450, YCoOrdinate = 1500, Area = 120},
                new Island(){ Id = 9, Name = "Island 9", XCoOrdinate = 700, YCoOrdinate = 120, Area = 1750},
                new Island(){ Id = 10, Name = "Island 10", XCoOrdinate = 25, YCoOrdinate = 35, Area = 850}
            };
        }

        private static List<Ship> getTestShips()
        {
            return new List<Ship>()
            {
                new Ship(){ Id = 1, Name = "Ship 1", Speed = 15, Size = 50, Length = 50 },
                new Ship(){ Id = 2, Name = "Ship 2", Speed = 100, Size = 5, Length = 10 },
                new Ship(){ Id = 3, Name = "Ship 3", Speed = 50, Size = 30, Length = 35 },
                new Ship(){ Id = 4, Name = "Ship 4", Speed = 70, Size = 20, Length = 45 },
                new Ship(){ Id = 5, Name = "Ship 5", Speed = 30, Size = 20, Length = 20 },
                new Ship(){ Id = 6, Name = "Ship 6", Speed = 85, Size = 10, Length = 25 }
            };
        }

        private class Object
        {
            [Category("PrimaryKey")]
            public int Id { get; set; }
            public string Name { get; set; }
            public int Size { get; set; }
            public int Weight { get; set; }
            public int Location { get; set; } = 0;
        }

        private class Island
        {
            [Category("PrimaryKey")]
            public int Id { get; set; }
            public string Name { get; set; }
            public int XCoOrdinate { get; set; }
            public int YCoOrdinate { get; set; }
            public int Area { get; set; }
        }

        private class Ship
        {
            [Category("PrimaryKey")]
            public int Id { get; set; }
            public string Name { get; set; }
            public int Speed { get; set; }
            public int Size { get; set; }
            public int Length { get; set; }
        }
    }
}
