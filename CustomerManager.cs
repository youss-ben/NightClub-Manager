using Npgsql;
using System;
using System.IO;  // For writing logs
using System.Collections.Generic;

namespace E5_ClubManager
{
    class CustomerManager
    {
        // Connection string to PostgreSQL database
        private static string connectionString = "Host=localhost;Username=postgres;Password=you2005123;Database=NightClub";

        // Main menu for customer management
        public static void ManageCustomer()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n--- Manage Customer ---");
                Console.WriteLine("1. Add a Customer");
                Console.WriteLine("2. View all Customers");
                Console.WriteLine("3. Update a Customer");
                Console.WriteLine("4. Delete a Customer");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine() ?? "";

                // Switch case to handle different actions based on user input
                switch (choice)
                {
                    case "1": AddCustomer(); break;  // Add a new customer
                    case "2": ViewCustomer(); break;  // View all customers
                    case "3": UpdateCustomer(); break;  // Update an existing customer
                    case "4": DeleteCustomer(); break;  // Delete a customer by ID
                    case "5": return;  // Exit the menu and return to the main menu
                    default: Console.WriteLine("Invalid option. Try again."); break;  // Invalid input handling
                }
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();  // Wait for user input to proceed
            }
        }

        // Add a new Customer
        private static void AddCustomer()
        {
            try
            {
                // Get customer details from the user
                Console.Write("Enter a customer name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Enter age: ");
                int age = int.Parse(Console.ReadLine());
                Console.Write("Enter email: ");
                string email = Console.ReadLine() ?? "";
                Console.Write("Enter phone: ");
                decimal phone = decimal.Parse(Console.ReadLine()!);

                // Establishing a connection to the database
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL query to insert the new customer into the database
                    string query = "INSERT INTO Customer (name, age, email, phone) VALUES (@name, @age, @email, @phone)";
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        // Adding parameters to avoid SQL injection
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@age", age);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@phone", phone);

                        // Executing the SQL command to insert the data
                        cmd.ExecuteNonQuery();
                        Console.WriteLine("Customer added successfully!");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handling any exceptions that occur during the insertion process
                Console.WriteLine("Error adding customer: " + ex.Message);
            }
        }

        // View all Customers
        private static void ViewCustomer()
        {
            try
            {
                // Establishing a connection to the database
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL query to retrieve all customers from the database
                    string query = "SELECT customer_id, name, age, email, phone FROM Customer";
                    using (var cmd = new NpgsqlCommand(query, connection))
                    using (var reader = cmd.ExecuteReader())
                    {
                        Console.WriteLine("\n-- All Customers --");

                        // Reading and displaying each customer from the result set
                        while (reader.Read())
                        {
                            Console.WriteLine(
                                $"ID: {reader["customer_id"]}, " +  // Retrieve and display the customer ID
                                $"Name: {reader["name"]}, " +  // Retrieve and display the customer name
                                $"Age: {reader["age"]}, " +  // Retrieve and display the customer age
                                $"Email: {reader["email"]}, " +  // Retrieve and display the customer email
                                $"Phone: {reader["phone"]}"  // Retrieve and display the customer phone number
                            );
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // Handling any exceptions that occur during the reading process
                Console.WriteLine("Error viewing customers: " + ex.Message);
            }
        }

        // Update a customer by Id
        private static void UpdateCustomer()
        {
            try
            {
                // Prompting the user to enter the customer ID they want to update
                Console.Write("Enter customer Id to update: ");
                int id = int.Parse(Console.ReadLine()!);

                // Get the updated details from the user
                Console.Write("Enter a new customer name: ");
                string name = Console.ReadLine() ?? "";

                Console.Write("Enter new age: ");
                int age = int.Parse(Console.ReadLine() ?? "0");  // Ensure that age is an integer, default to 0 if input is invalid

                Console.Write("Enter new email: ");
                string email = Console.ReadLine() ?? "";

                Console.Write("Enter new phone: ");
                decimal phone = decimal.Parse(Console.ReadLine()!);

                // Establishing a connection to the database
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL query to update the customer with the provided ID
                    string query = @"
                    UPDATE Customer
                    SET name = @name,
                        age = @age,
                        email = @email,
                        phone = @phone
                    WHERE customer_id = @id";  // Correct column name: customer_id

                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        // Adding parameters to avoid SQL injection
                        cmd.Parameters.AddWithValue("@id", id);  // Customer ID to identify the record
                        cmd.Parameters.AddWithValue("@name", name);
                        cmd.Parameters.AddWithValue("@age", age);
                        cmd.Parameters.AddWithValue("@email", email);
                        cmd.Parameters.AddWithValue("@phone", phone);

                        // Executing the update command
                        int rowsAffected = cmd.ExecuteNonQuery();

                        // Providing feedback based on the result of the update
                        if (rowsAffected > 0)
                            Console.WriteLine("Customer updated successfully!");
                        else
                            Console.WriteLine("No customer found with the provided Id.");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handling any exceptions that occur during the update process
                Console.WriteLine("Error updating customer: " + ex.Message);
            }
        }

        // Delete a Customer by ID
        private static void DeleteCustomer()
        {
            try
            {
                // Prompting the user to enter the customer ID they want to delete
                Console.Write("Enter Customer ID to delete: ");
                int id = int.Parse(Console.ReadLine());

                // Establishing a connection to the database
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    connection.Open();

                    // SQL query to delete the customer with the provided ID
                    string query = "DELETE FROM Customer WHERE customer_id = @id";  // Correct column name: customer_id
                    using (var cmd = new NpgsqlCommand(query, connection))
                    {
                        // Adding parameter to avoid SQL injection
                        cmd.Parameters.AddWithValue("@id", id);

                        // Executing the delete command and checking how many rows were affected
                        int rows = cmd.ExecuteNonQuery();
                        Console.WriteLine(rows > 0 ? "Customer deleted." : "Customer not found.");

                        // Log the deletion action (optional for auditing purposes)
                        LogAction($"Deleted Customer ID: {id}");
                    }
                }
            }
            catch (Exception ex)
            {
                // Handling any exceptions that occur during the deletion process
                Console.WriteLine("Error deleting customer: " + ex.Message);
            }
        }

        // Log actions to a text file for audit or debugging purposes
        private static void LogAction(string message)
        {
            string logFile = "log.txt";
            try
            {
                // Append the log message with the current timestamp
                File.AppendAllText(logFile, $"[{DateTime.Now}] {message}\n");
            }
            catch (Exception ex)
            {
                // Handling any errors while writing to the log file
                Console.WriteLine("Error writing log: " + ex.Message);
            }
        }
    }
}
