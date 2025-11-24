using Npgsql;

namespace PostgreSQLTestInterface
{
    public class Version_1
    {
        //This is a simple hand-written version of interaction with a postgresql database
        //Reduced use of subroutines, avoiding object usage
        public static async void Version1(NpgsqlConnection conn)
        {
            Object[] objects = new Object[]
            {
                new Object() { Id = 1, Name = "Slab", Size = 10, Weight = 10, Location = 3},
                new Object() { Id = 2, Name = "Stone", Size = 2, Weight = 1, Location = 3},
                new Object() { Id = 3, Name = "Shard", Size = 4, Weight = 7, Location = 4},
                new Object() { Id = 4, Name = "Block", Size = 5, Weight = 4 , Location = 5},
                new Object() { Id = 5, Name = "Sphere", Size = 8, Weight = 9, Location = 5}
            };

            Console.WriteLine("Creating ObjectTable");

            //Create table
            using (NpgsqlCommand cmd = new NpgsqlCommand("drop table if exists objecttable;", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("create table ObjectTable (Id Int, Name varchar, Size Int, Weight Int);", conn))
            {
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Insert data into ObjectTable");
            // Insert some data
            await using (NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO ObjectTable(Id, Name, Size, Weight) VALUES (1, 'Slab', 10, 10);", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO ObjectTable(Id, Name, Size, Weight) VALUES (2, 'Stone', 2, 1);", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO ObjectTable(Id, Name, Size, Weight) VALUES (3, 'Shard', 4, 7);", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO ObjectTable(Id, Name, Size, Weight) VALUES (4, 'Block', 5, 4);", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("INSERT INTO ObjectTable(Id, Name, Size, Weight) VALUES (5, 'Sphere', 8, 9);", conn))
            {
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Compare ObjectTable data to inserted data");
            // Retrieve all rows
            compareObjectsToObjectTable(conn, objects, false);

            //Edit data
            //Convert Stone to Rock
            Console.WriteLine("Modifying inserted data");
            using (NpgsqlCommand cmd = new NpgsqlCommand("update objecttable set Name = 'Rock', size = 3, weight = 2 where Id = 2", conn))
            {
                cmd.ExecuteNonQuery();
            }
            objects[1] = new Object() { Id = 2, Name = "Rock", Size = 3, Weight = 2, Location = 3 };
            compareObjectsToObjectTable(conn, objects, false);

            //Editing table
            Console.WriteLine("Altering table to add location");
            await using (NpgsqlCommand cmd = new NpgsqlCommand("alter table objecttable add Location int;", conn))
            {
                cmd.ExecuteNonQuery();
            }
            //Updating new locations
            await using (NpgsqlCommand cmd = new NpgsqlCommand("update objecttable set Location = 3 where Id = 1", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("update objecttable set Location = 3 where Id = 2", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("update objecttable set Location = 4 where Id = 3", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("update objecttable set Location = 5 where Id = 4", conn))
            {
                cmd.ExecuteNonQuery();
            }
            await using (NpgsqlCommand cmd = new NpgsqlCommand("update objecttable set Location = 5 where Id = 5", conn))
            {
                cmd.ExecuteNonQuery();
            }

            compareObjectsToObjectTable(conn, objects, true);

            Console.WriteLine("Removing ObjectTable");
            //Remove table
            await using (NpgsqlCommand cmd = new NpgsqlCommand("drop table if exists objecttable;", conn))
            {
                cmd.ExecuteNonQuery();
            }

            Console.WriteLine("Attempting to read from deleted table");
            //Read from table
            try
            {
                await using (NpgsqlCommand cmd = new NpgsqlCommand("select * from objecttable;", conn))
                {
                    cmd.ExecuteNonQuery();
                }
            }
            catch
            {
                Console.WriteLine("Could not read from deleted table");
            }
        }
        public static void compareObjectsToObjectTable(NpgsqlConnection conn, Object[] objects, bool includeLocation)
        {
            using (NpgsqlCommand cmd = new NpgsqlCommand("SELECT * FROM objecttable order by Id;", conn))
            using (NpgsqlDataReader reader = cmd.ExecuteReader())
            {
                int objectIndex = 0;
                Console.WriteLine("Comparing objects with database versions");
                while (reader.Read())
                {
                    Object comparisonObject = objects[objectIndex];
                    int Id = (int)reader.GetInt32(0);
                    string name = (string)reader.GetString(1);
                    int size = (int)reader.GetInt32(2);
                    int weight = (int)reader.GetInt32(3);

                    if (comparisonObject.Id != Id)
                    {
                        Console.WriteLine($"Ids do not match, object Id {comparisonObject.Id} =/= record Id {Id}");
                    }

                    if (comparisonObject.Name != name)
                    {
                        Console.WriteLine($"Names do not match, object name {comparisonObject.Name} =/= record name {name}");
                    }

                    if (comparisonObject.Size != size)
                    {
                        Console.WriteLine($"Sizes do not match, object size {comparisonObject.Size} =/= record Id {size}");
                    }

                    if (comparisonObject.Weight != weight)
                    {
                        Console.WriteLine($"Weights do not match, object weight {comparisonObject.Weight} =/= record Id {weight}");
                    }

                    if (includeLocation)
                    {
                        int location = (int)reader.GetInt32(4);
                        if (comparisonObject.Location != location)
                        {
                            Console.WriteLine($"Locations do not match, object location {comparisonObject.Location} =/= record location {location}");
                        }
                    }

                    objectIndex++;
                }
            }
        }
    }
}
