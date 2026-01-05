using Npgsql;
using System;
using System.IO;

namespace E5_ClubManager
{
    class EventManager
    {
        private static string connectionString = "Host=localhost;Username=postgres;Password=you2005123;Database=NightClub";
        private static string logFile = "log.txt";

        // Main menu for managing events
        public static void ManageEvents()
        {
            while (true)
            {
                Console.Clear();
                Console.WriteLine("\n--- Manage Event ---");
                Console.WriteLine("1. Add Event");
                Console.WriteLine("2. View Events");
                Console.WriteLine("3. Update Event");
                Console.WriteLine("4. Delete Event");
                Console.WriteLine("5. Assign Staff to Event");
                Console.WriteLine("6. Remove Staff from Event");
                Console.WriteLine("7. View Staff for Event");
                Console.WriteLine("8. View Events for Staff");
                Console.WriteLine("9. Back to Main Menu");
                Console.Write("Choose an option: ");
                string choice = Console.ReadLine() ?? "";

                // Handle user input to perform the corresponding action
                switch (choice)
                {
                    case "1": AddEvent(); break;
                    case "2": ViewEvents(); break;
                    case "3": UpdateEvent(); break;
                    case "4": DeleteEvent(); break;
                    case "5": AssignStaffToEvent(); break;
                    case "6": RemoveStaffFromEvent(); break;
                    case "7": ViewStaffForEvent(); break;
                    case "8": ViewEventsForStaff(); break;
                    case "9": return;
                    default: Console.WriteLine("Invalid option. Try again."); break;
                }
                Console.WriteLine("Press ENTER to continue...");
                Console.ReadLine();
            }
        }

        // Add a new event
        private static void AddEvent()
        {
            try
            {
                Console.Write("Enter Event Name: ");
                string eventName = Console.ReadLine() ?? "";
                Console.Write("Enter Event Date (yyyy-mm-dd): ");
                DateTime eventDate = DateTime.Parse(Console.ReadLine()!);

                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                string query = "INSERT INTO Event (Event_name, Event_date) VALUES (@eventName, @eventDate)";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@eventName", eventName);
                cmd.Parameters.AddWithValue("@eventDate", eventDate);

                cmd.ExecuteNonQuery();

                Console.WriteLine("Event added successfully.");
                LogAction($"Added event: {eventName} on {eventDate}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error adding event: " + ex.Message);
            }
        }

        // View all events
        private static void ViewEvents()
        {
            try
            {
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                string query = "SELECT Event_id, Event_name, Event_date FROM Event";
                using var cmd = new NpgsqlCommand(query, connection);
                using var reader = cmd.ExecuteReader();

                Console.WriteLine("\n-- All Events --");
                while (reader.Read())
                {
                    Console.WriteLine($"Event ID: {reader["Event_id"]}, Event Name: {reader["Event_name"]}, Event Date: {reader["Event_date"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error viewing events: " + ex.Message);
            }
        }

        // Update an existing event
        private static void UpdateEvent()
        {
            try
            {
                Console.Write("Enter Event ID to update: ");
                int eventId = int.Parse(Console.ReadLine()!);
                Console.Write("Enter new Event Name: ");
                string eventName = Console.ReadLine() ?? "";
                Console.Write("Enter new Event Date (yyyy-mm-dd): ");
                DateTime eventDate = DateTime.Parse(Console.ReadLine()!);

                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                string query = "UPDATE Event SET Event_name = @eventName, Event_date = @eventDate WHERE Event_id = @eventId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@eventId", eventId);
                cmd.Parameters.AddWithValue("@eventName", eventName);
                cmd.Parameters.AddWithValue("@eventDate", eventDate);

                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine(rows > 0 ? "Event updated successfully." : "Event not found.");
                LogAction($"Updated event ID: {eventId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error updating event: " + ex.Message);
            }
        }

        // Delete an event
        private static void DeleteEvent()
        {
            try
            {
                Console.Write("Enter Event ID to delete: ");
                int eventId = int.Parse(Console.ReadLine()!);

                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                string query = "DELETE FROM Event WHERE Event_id = @eventId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@eventId", eventId);

                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine(rows > 0 ? "Event deleted successfully." : "Event not found.");
                LogAction($"Deleted event ID: {eventId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error deleting event: " + ex.Message);
            }
        }

        // Assign staff to event
        private static void AssignStaffToEvent()
        {
            try
            {
                // Request Staff ID and Event ID from the user
                Console.Write("Enter Staff ID: ");
                int staffId = int.Parse(Console.ReadLine()!);
                Console.Write("Enter Event ID: ");
                int eventId = int.Parse(Console.ReadLine()!);

                // Open a connection to the database
                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                // Query to check if the staff exists in the database
                string checkStaffQuery = "SELECT COUNT(*) FROM Staff WHERE staff_id = @staffId";
                string checkEventQuery = "SELECT COUNT(*) FROM Event WHERE event_id = @eventId";

                // Check if the Staff exists
                using (var cmd = new NpgsqlCommand(checkStaffQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@staffId", staffId);
                    // ExecuteScalar returns the result of the query, which will be a long (Int64), so we cast it to long
                    long staffExists = (long)cmd.ExecuteScalar();  // Using long to handle the result of COUNT(*)
                    if (staffExists == 0)
                    {
                        Console.WriteLine("Staff with ID " + staffId + " does not exist.");
                        return;
                    }
                }

                // Check if the Event exists
                using (var cmd = new NpgsqlCommand(checkEventQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@eventId", eventId);
                    // Again, use long for the result of COUNT(*)
                    long eventExists = (long)cmd.ExecuteScalar();  // Using long here as well
                    if (eventExists == 0)
                    {
                        Console.WriteLine("Event with ID " + eventId + " does not exist.");
                        return;
                    }
                }

                // Query to check if the Staff is already assigned to this Event
                string checkAssignmentQuery = "SELECT COUNT(*) FROM Organisation WHERE Staff_id = @staffId AND Event_id = @eventId";
                using (var cmd = new NpgsqlCommand(checkAssignmentQuery, connection))
                {
                    cmd.Parameters.AddWithValue("@staffId", staffId);
                    cmd.Parameters.AddWithValue("@eventId", eventId);
                    // Check if the staff is already assigned to the event
                    long assignmentExists = (long)cmd.ExecuteScalar();  // Use long for the COUNT(*) result
                    if (assignmentExists > 0)
                    {
                        Console.WriteLine("Staff is already assigned to this event.");
                        return;
                    }
                }

                // If no issues, proceed to assign the staff to the event
                string query = "INSERT INTO Organisation (Staff_id, Event_id) VALUES (@staffId, @eventId)";
                using var insertCmd = new NpgsqlCommand(query, connection);
                insertCmd.Parameters.AddWithValue("@staffId", staffId);
                insertCmd.Parameters.AddWithValue("@eventId", eventId);

                // Execute the insertion query to add the staff-event relationship
                insertCmd.ExecuteNonQuery();
                Console.WriteLine("Staff assigned to event successfully.");

                // Log the action for auditing purposes
                LogAction($"Assigned staff ID: {staffId} to event ID: {eventId}");
            }
            catch (Exception ex)
            {
                // Handle any exceptions that occur during the operation
                Console.WriteLine("Error assigning staff to event: " + ex.Message);
            }
        }



        // Remove staff from an event
        private static void RemoveStaffFromEvent()
        {
            try
            {
                Console.Write("Enter Staff ID: ");
                int staffId = int.Parse(Console.ReadLine()!);
                Console.Write("Enter Event ID: ");
                int eventId = int.Parse(Console.ReadLine()!);

                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                string query = "DELETE FROM Organisation WHERE Staff_id = @staffId AND Event_id = @eventId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@staffId", staffId);
                cmd.Parameters.AddWithValue("@eventId", eventId);

                int rows = cmd.ExecuteNonQuery();
                Console.WriteLine(rows > 0 ? "Staff removed from event." : "Staff not found for this event.");
                LogAction($"Removed staff ID: {staffId} from event ID: {eventId}");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error removing staff from event: " + ex.Message);
            }
        }

        // View all staff for a specific event
        private static void ViewStaffForEvent()
        {
            try
            {
                Console.Write("Enter Event ID: ");
                int eventId = int.Parse(Console.ReadLine()!);

                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                string query = "SELECT s.staff_id, s.name, s.position FROM Staff s " +
                               "JOIN Organisation o ON s.staff_id = o.Staff_id " +
                               "WHERE o.Event_id = @eventId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@eventId", eventId);

                using var reader = cmd.ExecuteReader();
                Console.WriteLine("\n-- Staff for Event --");
                while (reader.Read())
                {
                    Console.WriteLine($"Staff ID: {reader["staff_id"]}, Name: {reader["name"]}, Position: {reader["position"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error viewing staff for event: " + ex.Message);
            }
        }

        // View all events for a specific staff
        private static void ViewEventsForStaff()
        {
            try
            {
                Console.Write("Enter Staff ID: ");
                int staffId = int.Parse(Console.ReadLine()!);

                using var connection = new NpgsqlConnection(connectionString);
                connection.Open();

                string query = "SELECT e.Event_id, e.Event_name, e.Event_date FROM Event e " +
                               "JOIN Organisation o ON e.Event_id = o.Event_id " +
                               "WHERE o.Staff_id = @staffId";
                using var cmd = new NpgsqlCommand(query, connection);
                cmd.Parameters.AddWithValue("@staffId", staffId);

                using var reader = cmd.ExecuteReader();
                Console.WriteLine("\n-- Events for Staff --");
                while (reader.Read())
                {
                    Console.WriteLine($"Event ID: {reader["Event_id"]}, Event Name: {reader["Event_name"]}, Event Date: {reader["Event_date"]}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error viewing events for staff: " + ex.Message);
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
