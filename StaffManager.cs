using Npgsql;
using System;
using System.IO;

namespace E5_ClubManager
{
    class StaffManager
    {
        private static string connectionString = "Host=localhost;Username=postgres;Password=you2005123;Database=NightClub";
        private static string logFile = "log.txt";

        // Main menu for managing staff
        public static void ManageStaff()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n--- Manage Staff ---");
                Console.WriteLine("1. Add a Staff Member");
                Console.WriteLine("2. View all Staff");
                Console.WriteLine("3. Update a Staff Member");
                Console.WriteLine("4. Delete a Staff Member");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine() ?? "";

                // Handle user input to perform the corresponding action
                switch (choice)
                {
                    case "1": AddStaff(); break;  // Add a new staff member
                    case "2": ViewStaff(); break;  // View all staff members
                    case "3": UpdateStaff(); break;  // Update a specific staff member
                    case "4": DeleteStaff(); break;  // Delete a staff member
                    case "5": return;  // Return to main menu
                    default: Console.WriteLine("Invalid option. Try again."); break;  // Handle invalid input
                }
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();  // Wait for user to press ENTER before continuing
            }
        }

        // Add a new staff member
        private static void AddStaff()
        {
            try
            {
                // Get input for the new staff member
                Console.Write("Enter name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Enter position: ");
                string position = Console.ReadLine() ?? "";
                Console.Write("Enter email: ");
                string email = Console.ReadLine() ?? "";
                Console.Write("Enter password: ");
                string password = Console.ReadLine() ?? "";

                // Hash the password before storing it
                string hashedPassword = HashPassword(password);

                // Establish a connection to the database
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // SQL query to insert a new staff member
                string query = "INSERT INTO Staff (name, position, email, password) " +
                               "VALUES (@name, @position, @email, @password)";
                using var cmd = new NpgsqlCommand(query, connection);
                // Add parameters to the query to prevent SQL injection
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@position", position);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", hashedPassword);
                // Execute the query to insert the data into the database
                cmd.ExecuteNonQuery();

                Console.WriteLine("Staff member added successfully.");
                // Log the action
                LogAction($"Added staff: {name}");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the add operation
                Console.WriteLine("Error adding staff: " + ex.Message);
            }
        }

        // View all staff members
        private static void ViewStaff()
        {
            try
            {
                // Establish a connection to the database
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // SQL query to retrieve all staff members from the database
                string query = "SELECT staff_id, name, position, email FROM Staff";
                using var cmd = new NpgsqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                Console.WriteLine("\n-- All Staff --");
                // Loop through each row in the result set and display the staff details
                while (reader.Read())
                {
                    Console.WriteLine($"ID: {reader["staff_id"]}, Name: {reader["name"]}, Position: {reader["position"]}, Email: {reader["email"]}");
                }
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the viewing operation
                Console.WriteLine("Error viewing staff: " + ex.Message);
            }
        }

        // Update an existing staff member's details
        private static void UpdateStaff()
        {
            try
            {
                // Prompt for the staff ID to update
                Console.Write("Enter Staff ID to update: ");
                int id = int.Parse(Console.ReadLine()!);
                Console.Write("Enter new name: ");
                string name = Console.ReadLine() ?? "";
                Console.Write("Enter new position: ");
                string position = Console.ReadLine() ?? "";
                Console.Write("Enter new email: ");
                string email = Console.ReadLine() ?? "";
                Console.Write("Enter new password: ");
                string password = Console.ReadLine() ?? "";

                // Hash the new password before storing it
                string hashedPassword = HashPassword(password);

                // Establish a connection to the database
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // SQL query to update the existing staff member based on staff_id
                string query = @"
                    UPDATE Staff
                    SET name = @name, position = @position, email = @email, password = @password
                    WHERE staff_id = @id";

                using var cmd = new NpgsqlCommand(query, connection);
                // Add parameters to prevent SQL injection
                cmd.Parameters.AddWithValue("@id", id);
                cmd.Parameters.AddWithValue("@name", name);
                cmd.Parameters.AddWithValue("@position", position);
                cmd.Parameters.AddWithValue("@email", email);
                cmd.Parameters.AddWithValue("@password", hashedPassword);

                // Execute the update query
                int rows = cmd.ExecuteNonQuery();
                // Feedback to the user based on whether the update was successful
                Console.WriteLine(rows > 0 ? "Staff updated." : "Staff not found.");
                // Log the action
                LogAction($"Updated staff ID: {id}");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the update operation
                Console.WriteLine("Error updating staff: " + ex.Message);
            }
        }

        // Delete a staff member by ID
        private static void DeleteStaff()
        {
            try
            {
                // Prompt for the staff ID to delete
                Console.Write("Enter Staff ID to delete: ");
                int id = int.Parse(Console.ReadLine()!);

                // Establish a connection to the database
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // SQL query to delete a staff member based on staff_id
                string query = "DELETE FROM Staff WHERE staff_id = @id";
                using var cmd = new NpgsqlCommand(query, connection);
                // Add parameter to prevent SQL injection
                cmd.Parameters.AddWithValue("@id", id);
                // Execute the delete query and check if any rows were affected
                int rows = cmd.ExecuteNonQuery();

                // Provide feedback based on the result of the delete operation
                Console.WriteLine(rows > 0 ? "Staff deleted." : "Staff not found.");
                // Log the action
                LogAction($"Deleted staff ID: {id}");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the delete operation
                Console.WriteLine("Error deleting staff: " + ex.Message);
            }
        }

        // Hash the password using SHA256
        private static string HashPassword(string password)
        {
            using var sha256 = System.Security.Cryptography.SHA256.Create();
            // Convert the password to bytes and compute the hash
            byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            // Convert the byte array to a hexadecimal string
            return Convert.ToHexString(bytes).ToLower();
        }

        // Log actions to a text file for auditing purposes
        private static void LogAction(string message)
        {
            try
            {
                // Append the log message with the current timestamp
                File.AppendAllText(logFile, $"[{DateTime.Now}] {message}\n");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur while writing to the log
                Console.WriteLine("Error writing log: " + ex.Message);
            }
        }
    }
}
