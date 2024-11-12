using System;
using System.Data.SqlClient;

namespace BuySellOrders
{
    internal class Program
    {
        static string sqlConnectionString = "Data Source=(localdb)\\MSSQLLocalDB;Initial Catalog=master;Integrated Security=True;Connect Timeout=30;Encrypt=False";

        static void Main(string[] args)
        {
            CreateDatabase();
            for (; ; )
            {
                Console.WriteLine("Do you want to Add or Remove?");
                Console.WriteLine("1. Add");
                Console.WriteLine("2. Remove");

                string choice = Console.ReadLine();

                switch (choice)
                {
                    case "1":
                        Add();
                        break;
                    case "2":
                        Remove();
                        break;
                    default:
                        Console.Clear();
                        Console.WriteLine("Wrong input, please choose 1 or 2");
                        break;
                }
            }
        }
        public static void CreateDatabase()
        {
            /*
             try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    string createDatabaseQuery = "CREATE DATABASE OrdersDB";

                    SqlCommand createDatabaseCommand = new SqlCommand(createDatabaseQuery, connection);

                    connection.Open();
                    createDatabaseCommand.ExecuteNonQuery();
                    Console.WriteLine("Database 'OrdersDB' created successfully.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
            */
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    string createTableQuery =
                        "CREATE TABLE OrdersTable(" +
                                "id int NOT NULL PRIMARY KEY IDENTITY(1,1), " +
                                "orderType varchar(4) NOT NULL, " +
                                "price float NOT NULL, " +
                                "quantity int NOT NULL, " +
                                "CHECK (orderType = 'buy' OR orderType = 'sell'));";
                    SqlCommand createTableCommand = new SqlCommand(createTableQuery, connection);

                    connection.Open();
                    createTableCommand.ExecuteNonQuery();
                    Console.WriteLine("Table 'Orders' created successfully in 'OrdersDB'.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
                Console.WriteLine("Table with that name already exists");
            }
        }

        static void Add()
        {
            Console.Clear();
            Console.WriteLine("What order would you like to add?");
            Console.WriteLine("What's the type?");
            Console.WriteLine("1. Buy");
            Console.WriteLine("2. Sell");
            int orderType = -1;
            do
            {
                orderType = Convert.ToInt32(Console.ReadLine());
            } while (orderType != 1 && orderType != 2);
            orderType -= 1;

            Console.WriteLine("What's the price?");
            float price = -1;
            do
            {
                price = (float)Convert.ToDouble(Console.ReadLine());
            } while (price < 0);
            Console.WriteLine("What's the quantity?");
            int quantity = -1;
            do
            {
                quantity = Convert.ToInt32(Console.ReadLine());
            } while (quantity < 0);

            AddToDatabase((OrderType)orderType, price, quantity);
        }

        static void AddToDatabase(OrderType orderType, float price, int quantity)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    string insertOrderQuery = @"
                INSERT INTO OrdersTable (orderType, price, quantity)
                VALUES (@Order, @Price, @Quantity)";

                    var insertCommand = new SqlCommand(insertOrderQuery, connection);
                    insertCommand.Parameters.AddWithValue("@Order", orderType.ToString());
                    insertCommand.Parameters.AddWithValue("@Price", price);
                    insertCommand.Parameters.AddWithValue("@Quantity", quantity);

                    insertCommand.ExecuteNonQuery();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while adding to the database");
                Console.WriteLine(ex);
            }
            finally
            {
                GetBestPrice(orderType);
                Console.Clear();
            }
        }

        static void Remove()
        {
            Console.Clear();
            Console.WriteLine("Which order would you like to remove?");
            int orderId = -1;
            do
            {
                orderId = Convert.ToInt32(Console.ReadLine());
            } while (orderId < 0);

            RemoveFromDatabase(orderId);
        }

        static void RemoveFromDatabase(int orderId)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(sqlConnectionString))
                {
                    connection.Open();

                    string deleteOrderQuery = "DELETE FROM OrdersTable WHERE Id = @OrderId";

                    using (SqlCommand deleteCommand = new SqlCommand(deleteOrderQuery, connection))
                    {
                        deleteCommand.Parameters.AddWithValue("@OrderId", OrderType.Sell.ToString());
                        deleteCommand.ExecuteNonQuery();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error while deleting from the database");
                Console.WriteLine(ex);
            }
            finally
            {
                Console.Clear();
            }

        }

        static void GetBestPrice(OrderType orderType)
        {
            using (SqlConnection connection = new SqlConnection(sqlConnectionString))
            {
                connection.Open();

                string findOrderQuery = "SELECT * " +
                    "FROM OrdersTable " +
                    "WHERE Price = (SELECT Min(Price) FROM OrdersTable WHERE orderType = @OrderType)";
                using (SqlCommand findCommand = new SqlCommand(findOrderQuery, connection))
                {
                    if (orderType == OrderType.Sell)
                    {
                        findCommand.Parameters.AddWithValue("@OrderType", "Buy");
                        Console.WriteLine("Best price Buy orders");
                    }
                    else
                    {
                        findCommand.Parameters.AddWithValue("@OrderType", "Sell");
                        Console.WriteLine("Best price Sell orders");
                    }
                    SqlDataReader reader = findCommand.ExecuteReader();
                    Console.WriteLine("ID - Price - Quantity");
                    while (reader.Read())
                    {
                        Console.WriteLine(reader.GetValue(0) + " - " + reader.GetValue(2) + " - " + reader.GetValue(3));
                    }
                    Console.WriteLine("Press any button to continue");
                    Console.ReadKey();
                }
            }
        }

        enum OrderType
        {
            Buy,
            Sell
        }
    }
}
