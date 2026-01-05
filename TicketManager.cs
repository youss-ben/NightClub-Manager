using Npgsql;
using System;
using System.IO;

namespace E5_ClubManager
{
    class TicketManager
    {
        private static string connectionString = "Host=localhost;Username=postgres;Password=you2005123;Database=NightClub";
        private static string logFile = "log.txt";

        // Main menu for managing tickets
        public static void ManageTicket()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n--- Manage Tickets ---");
                Console.WriteLine("1. Add Ticket");
                Console.WriteLine("2. View Tickets");
                Console.WriteLine("3. Update Ticket");
                Console.WriteLine("4. Delete Ticket");
                Console.WriteLine("5. Back to Main Menu");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine() ?? "";

                // Handle user input to perform the corresponding action
                switch (choice)
                {
                    case "1": AddTicket(); break;
                    case "2": ViewTickets(); break;
                    case "3": UpdateTicket(); break;
                    case "4": DeleteTicket(); break;
                    case "5": return;
                    default: Console.WriteLine("Invalid option. Try again."); break;
                }
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();
            }
        }

        // Add a new ticket
        private static void AddTicket()
        {
            try
            {
                // Get input from user
                Console.Write("Enter Customer ID: ");
                int customerId = int.Parse(Console.ReadLine()!);
                Console.Write("Enter Event ID: ");
                int eventId = int.Parse(Console.ReadLine()!);
                Console.Write("Enter Purchase Date (yyyy-mm-dd): ");
                DateTime purchaseDate = DateTime.Parse(Console.ReadLine()!);
                Console.Write("Enter Membership Level: ");
                string membershipLevel = Console.ReadLine() ?? "";
                Console.Write("Enter Price: ");
                decimal price = decimal.Parse(Console.ReadLine()!);

                // Check if the customer exists
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                string checkCustomerQuery = "SELECT COUNT(*) FROM Customer WHERE Customer_id = @customerId";
                using (var cmd = new NpgsqlCommand(checkCustomerQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    long customerExists = (long)cmd.ExecuteScalar();
                    if (customerExists == 0)
                    {
                        Console.WriteLine($"Customer with ID {customerId} does not exist.");
                        return;
                    }
                }

                // Check if the event exists
                string checkEventQuery = "SELECT COUNT(*) FROM Event WHERE Event_id = @eventId";
                using (var cmd = new NpgsqlCommand(checkEventQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@eventId", eventId);
                    long eventExists = (long)cmd.ExecuteScalar();
                    if (eventExists == 0)
                    {
                        Console.WriteLine($"Event with ID {eventId} does not exist.");
                        return;
                    }
                }

                // Insert the ticket if both customer and event exist
                string insertTicketQuery = "INSERT INTO Tickets (Customer_id, Event_id, purchaseDate, MembershipLevel, price) VALUES (@customerId, @eventId, @purchaseDate, @membershipLevel, @price)";
                using (var cmd = new NpgsqlCommand(insertTicketQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@customerId", customerId);
                    cmd.Parameters.AddWithValue("@eventId", eventId);
                    cmd.Parameters.AddWithValue("@purchaseDate", purchaseDate);
                    cmd.Parameters.AddWithValue("@membershipLevel", membershipLevel);
                    cmd.Parameters.AddWithValue("@price", price);

                    cmd.ExecuteNonQuery();
                    Console.WriteLine("Ticket added successfully.");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error adding ticket: " + ex.Message);
            }
        }


        // View all tickets
        private static void ViewTickets()
        {
            try
            {
                // Establish connection to database
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // SQL query to retrieve all tickets
                string query = "SELECT * FROM Tickets";
                using var cmd = new NpgsqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                Console.WriteLine("\n-- All Tickets --");
                while (reader.Read())
                {
                    Console.WriteLine($"Ticket ID: {reader["Ticket_id"]}, Customer ID: {reader["Customer_id"]}, Event ID: {reader["Event_id"]}, " +
                                      $"Purchase Date: {reader["purchaseDate"]}, Membership Level: {reader["MembershipLevel"]}, Price: {reader["price"]}");
                }
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error viewing tickets: " + ex.Message);
            }
        }

        // Update an existing ticket
        private static void UpdateTicket()
        {
            try
            {
                // Get the ticket ID to update
                Console.Write("Enter Ticket ID to update: ");
                int ticketId = int.Parse(Console.ReadLine()!);
                Console.Write("Enter new Customer ID: ");
                int customerId = int.Parse(Console.ReadLine()!);
                Console.Write("Enter new Event ID: ");
                int eventId = int.Parse(Console.ReadLine()!);
                Console.Write("Enter new Purchase Date (yyyy-mm-dd): ");
                DateTime purchaseDate = DateTime.Parse(Console.ReadLine()!);
                Console.Write("Enter new Membership Level: ");
                string level = Console.ReadLine() ?? "";
                Console.Write("Enter new Price: ");
                decimal price = decimal.Parse(Console.ReadLine()!);

                // Establish connection to database
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // SQL query to update the ticket in the database
                string query = "UPDATE Tickets SET Customer_id = @customerId, Event_id = @eventId, " +
                               "purchaseDate = @purchaseDate, MembershipLevel = @level, price = @price " +
                               "WHERE Ticket_id = @ticketId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ticketId", ticketId);
                cmd.Parameters.AddWithValue("@customerId", customerId);
                cmd.Parameters.AddWithValue("@eventId", eventId);
                cmd.Parameters.AddWithValue("@purchaseDate", purchaseDate);
                cmd.Parameters.AddWithValue("@level", level);
                cmd.Parameters.AddWithValue("@price", price);

                // Execute query to update the ticket
                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine(rows > 0 ? "Ticket updated successfully!" : "No ticket found.");
                // Log the action
                LogAction($"Updated ticket ID: {ticketId}");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error updating ticket: " + ex.Message);
            }
        }

        // Delete a ticket
        private static void DeleteTicket()
        {
            try
            {
                // Get the ticket ID to delete
                Console.Write("Enter Ticket ID to delete: ");
                int ticketId = int.Parse(Console.ReadLine()!);

                // Establish connection to database
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // SQL query to delete a ticket
                string query = "DELETE FROM Tickets WHERE Ticket_id = @ticketId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@ticketId", ticketId);

                // Execute query to delete the ticket
                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine(rows > 0 ? "Ticket deleted." : "Ticket not found.");
                // Log the action
                LogAction($"Deleted ticket ID: {ticketId}");
            }
            catch (Exception ex)
            {
                // Handle exceptions
                Console.WriteLine("Error deleting ticket: " + ex.Message);
            }
        }

        // Log actions to a text file for auditing purposes
        private static void LogAction(string message)
        {
            try
            {
                File.AppendAllText(logFile, $"[{DateTime.Now}] {message}\n");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error writing log: " + ex.Message);
            }
        }
    }
}
