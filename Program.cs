using System;
using System.IO;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using Npgsql;
using Spectre.Console;
namespace E5_ClubManager
{
    class Program
    {
        static string dbConnectionString = "Host=localhost;Username=postgres;Password=you2005123;Database=NightClub";
        static string logFile = "log.txt";
        static void Main()
        {
            // Ensure the database and tables are set up
            EnsureDatabaseSetup();

            // Display the main menu
            DisplayMenu();
        }
        static void DisplayMenu()
        {
            while (true)
            {
                Console.Clear();

                // Big centered title
                AnsiConsole.Write(
                    new FigletText("Night Club Manager")
                        .Centered()
                        .Color(Color.Aqua));

                // Interactive selection menu
                var choice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("[bold yellow]Select an option:[/]")
                        .PageSize(10)
                        .AddChoices(new[]
                        {
                    "1. Manage Customer",
                    "2. Manage Staff",
                    "3. Manage Event",
                    "4. Manage Ticket",
                    "5. View Statistics",
                    "6. Clear DataBase",
                    "7. Exit"
                        }));

                // Action based on selection
                switch (choice)
                {
                    case "1. Manage Customer":
                        CustomerManager.ManageCustomer();
                        break;
                    case "2. Manage Staff":
                        StaffManager.ManageStaff();
                        break;
                    case "3. Manage Event":
                        EventManager.ManageEvents();
                        break;
                    case "4. Manage Ticket":
                        TicketManager.ManageTicket();
                        break;
                    case "5. View Statistics":
                        ViewStatistics();
                        break;
                    case "6. Clear DataBase":
                        ClearDatabase();
                        break;
                    case "7. Exit":
                        AnsiConsole.MarkupLine("[bold red]Goodbye![/]");
                        return;
                }

            }

        }

        public static void ClearDatabase()
        {
            using (var connection = new NpgsqlConnection(dbConnectionString))
            {
                try
                {
                    connection.Open();
                    var cmd = new NpgsqlCommand(@"
                DELETE FROM Organisation;
                DELETE FROM Tickets;
                DELETE FROM Staff;
                DELETE FROM Event;
                DELETE FROM Customer;
            ", connection);
                    cmd.ExecuteNonQuery();
                    Console.WriteLine("DataBase cleared with success.");
                    // Wait for user to return to the menu
                    AnsiConsole.MarkupLine("Press ENTER to return to the main menu.");
                    Console.ReadLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error : {ex.Message}");
                }
            }

        }


        // Ensure that the database and tables exist
        static void EnsureDatabaseSetup()
        {
            using (var connection = new NpgsqlConnection(dbConnectionString))
            {
                connection.Open();

                // Create tables
                string createTables = @"
CREATE TABLE IF NOT EXISTS Customer (
    Customer_id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    phone VARCHAR(20),
    age INT,
    email VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS Event (
    Event_id SERIAL PRIMARY KEY,
    Event_name VARCHAR(100),
    Event_date DATE
);

CREATE TABLE IF NOT EXISTS Staff (
    Staff_id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    position VARCHAR(50),
    email VARCHAR(100),
    password VARCHAR(100)
);

CREATE TABLE IF NOT EXISTS Tickets (
    Ticket_id SERIAL PRIMARY KEY,
    Customer_id INT,
    Event_id INT,
    purchaseDate DATE,
    MembershipLevel VARCHAR(50),
    price DECIMAL(10,2),
    FOREIGN KEY (Customer_id) REFERENCES Customer(Customer_id) ON DELETE CASCADE,
    FOREIGN KEY (Event_id) REFERENCES Event(Event_id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS Organisation (
    Staff_id INT,
    Event_id INT,
    PRIMARY KEY (Staff_id, Event_id),
    FOREIGN KEY (Staff_id) REFERENCES Staff(Staff_id) ON DELETE CASCADE,
    FOREIGN KEY (Event_id) REFERENCES Event(Event_id) ON DELETE CASCADE
);";

                new NpgsqlCommand(createTables, connection).ExecuteNonQuery();

                // Insert Customers (in English)
                var customersToAdd = new[] {
            ("Lucas Walker", "0712345678", 25, "lucas.walker@mail.com"),
            ("Chloe Smith", "0723456789", 30, "chloe.smith@mail.com"),
            ("Peter Johnson", "0734567890", 35, "peter.johnson@mail.com"),
            ("Sophie Adams", "0745678901", 28, "sophie.adams@mail.com"),
            ("Anthony Brown", "0756789012", 22, "anthony.brown@mail.com")
        };

                Dictionary<string, int> customerIds = new();

                foreach (var (name, phone, age, email) in customersToAdd)
                {
                    var check = new NpgsqlCommand("SELECT Customer_id FROM Customer WHERE email = @em", connection);
                    check.Parameters.AddWithValue("@em", email);
                    var existingId = check.ExecuteScalar();

                    if (existingId == null)
                    {
                        var insert = new NpgsqlCommand("INSERT INTO Customer (name, phone, age, email) VALUES (@n, @ph, @ag, @em) RETURNING Customer_id", connection);
                        insert.Parameters.AddWithValue("@n", name);
                        insert.Parameters.AddWithValue("@ph", phone);
                        insert.Parameters.AddWithValue("@ag", age);
                        insert.Parameters.AddWithValue("@em", email);
                        int newId = (int)insert.ExecuteScalar();
                        customerIds[email] = newId;
                    }
                    else
                    {
                        customerIds[email] = (int)existingId;
                    }
                }

                // Insert Events (in English)
                var eventsToAdd = new[] {
            ("Jazz Concert", DateTime.Parse("2025-06-15")),
            ("Music Festival", DateTime.Parse("2025-07-20")),
            ("Classical Theater", DateTime.Parse("2025-08-10")),
            ("Art Exhibition", DateTime.Parse("2025-09-01")),
            ("Paris Marathon", DateTime.Parse("2025-10-05"))
        };

                Dictionary<string, int> eventIds = new();

                foreach (var (eventName, eventDate) in eventsToAdd)
                {
                    var check = new NpgsqlCommand("SELECT Event_id FROM Event WHERE Event_name = @en AND Event_date = @ed", connection);
                    check.Parameters.AddWithValue("@en", eventName);
                    check.Parameters.AddWithValue("@ed", eventDate);
                    var existingId = check.ExecuteScalar();

                    if (existingId == null)
                    {
                        var insert = new NpgsqlCommand("INSERT INTO Event (Event_name, Event_date) VALUES (@en, @ed) RETURNING Event_id", connection);
                        insert.Parameters.AddWithValue("@en", eventName);
                        insert.Parameters.AddWithValue("@ed", eventDate);
                        int newId = (int)insert.ExecuteScalar();
                        eventIds[eventName] = newId;
                    }
                    else
                    {
                        eventIds[eventName] = (int)existingId;
                    }
                }

                // Insert Staff (in English)
                var staffToAdd = new[] {
            ("Emily Walker", "Manager", "emily.walker@mail.com", "emily123"),
            ("Julian Green", "Technician", "julian.green@mail.com", "julian456"),
            ("Isabelle White", "Security", "isabelle.white@mail.com", "isabelle789"),
            ("Mark Black", "Sales", "mark.black@mail.com", "mark101"),
            ("Natalie Moore", "Coordinator", "natalie.moore@mail.com", "natalie112")
        };

                Dictionary<string, int> staffIds = new();

                foreach (var (name, position, email, password) in staffToAdd)
                {
                    var check = new NpgsqlCommand("SELECT Staff_id FROM Staff WHERE email = @em", connection);
                    check.Parameters.AddWithValue("@em", email);
                    var existingId = check.ExecuteScalar();

                    if (existingId == null)
                    {
                        var insert = new NpgsqlCommand("INSERT INTO Staff (name, position, email, password) VALUES (@n, @pos, @em, @pwd) RETURNING Staff_id", connection);
                        insert.Parameters.AddWithValue("@n", name);
                        insert.Parameters.AddWithValue("@pos", position);
                        insert.Parameters.AddWithValue("@em", email);
                        insert.Parameters.AddWithValue("@pwd", HashPassword(password));
                        int newId = (int)insert.ExecuteScalar();
                        staffIds[email] = newId;
                    }
                    else
                    {
                        staffIds[email] = (int)existingId;
                    }
                }

                // Insert Organisation (assign staff to events)
                var organisationToAdd = new[] {
            (staffIds["emily.walker@mail.com"], eventIds["Jazz Concert"]),
            (staffIds["julian.green@mail.com"], eventIds["Music Festival"]),
            (staffIds["isabelle.white@mail.com"], eventIds["Classical Theater"]),
            (staffIds["mark.black@mail.com"], eventIds["Art Exhibition"]),
            (staffIds["natalie.moore@mail.com"], eventIds["Paris Marathon"])
        };

                foreach (var (staffId, eventId) in organisationToAdd)
                {
                    var check = new NpgsqlCommand("SELECT 1 FROM Organisation WHERE Staff_id = @sid AND Event_id = @eid", connection);
                    check.Parameters.AddWithValue("@sid", staffId);
                    check.Parameters.AddWithValue("@eid", eventId);
                    var exists = check.ExecuteScalar();

                    if (exists == null)
                    {
                        var insert = new NpgsqlCommand("INSERT INTO Organisation (Staff_id, Event_id) VALUES (@sid, @eid)", connection);
                        insert.Parameters.AddWithValue("@sid", staffId);
                        insert.Parameters.AddWithValue("@eid", eventId);
                        insert.ExecuteNonQuery();
                    }
                }

                // Insert Tickets (make it varied!)
                var membershipLevels = new[] { "Standard", "VIP", "VVIP" };
                var random = new Random();

                // Sample ticket creation
                foreach (var (customerEmail, customerId) in customerIds)
                {
                    // Chaque client achète entre 1 et 3 tickets
                    int ticketCount = random.Next(1, 4);

                    for (int i = 0; i < ticketCount; i++)
                    {
                        // Choisir un événement aléatoire
                        var randomEvent = new List<int>(eventIds.Values)[random.Next(eventIds.Count)];

                        // Choisir un niveau de membership aléatoire
                        string membership = membershipLevels[random.Next(membershipLevels.Length)];

                        // Déterminer un prix selon membership
                        decimal price = membership switch
                        {
                            "Standard" => random.Next(20, 40),
                            "VIP" => random.Next(50, 70),
                            "VVIP" => random.Next(100, 150),
                            _ => 30
                        };

                        // Insérer le ticket
                        var insertTicket = new NpgsqlCommand(@"
            INSERT INTO Tickets (Customer_id, Event_id, purchaseDate, MembershipLevel, price)
            VALUES (@cid, @eid, @pdate, @mlevel, @price)", connection);

                        insertTicket.Parameters.AddWithValue("@cid", customerId);
                        insertTicket.Parameters.AddWithValue("@eid", randomEvent);
                        insertTicket.Parameters.AddWithValue("@pdate", DateTime.Now.AddDays(-random.Next(0, 30)));
                        insertTicket.Parameters.AddWithValue("@mlevel", membership);
                        insertTicket.Parameters.AddWithValue("@price", price);

                        insertTicket.ExecuteNonQuery();
                    }
                }

            }
        }


        // Local hash method (SHA256)
        static string HashPassword(string password)
            {
                using (SHA256 sha256 = SHA256.Create())
                {
                    byte[] input = Encoding.UTF8.GetBytes(password);
                    byte[] hash = sha256.ComputeHash(input);
                    return Convert.ToBase64String(hash);
                }
            }
        


        static void ViewStatistics()
        {
            try
            {
                using (var connection = new NpgsqlConnection(dbConnectionString))
                {
                    connection.Open();

                    // Clear console and show the header
                    AnsiConsole.Clear();
                    AnsiConsole.Write(
                        new FigletText("Event Stats")
                            .Centered()
                            .Color(Color.Cyan1));

                    // SECTION: Top 5 customers by number of tickets
                    AnsiConsole.Write(new Rule("[yellow]Top 5 Customers by Tickets[/]"));

                    var customerTable = new Table();
                    customerTable.AddColumn("Customer");
                    customerTable.AddColumn("Tickets");

                    var cmd1 = new NpgsqlCommand(@"
            SELECT c.name, COUNT(t.ticket_id) AS ticket_count
            FROM Customer c
            JOIN Tickets t ON c.customer_id = t.customer_id
            GROUP BY c.name
            ORDER BY ticket_count DESC
            LIMIT 5", connection);

                    var reader1 = cmd1.ExecuteReader();

                    var customerChartData = new List<(string name, int tickets)>();

                    while (reader1.Read())
                    {
                        string name = reader1["name"].ToString() ?? "Unknown";
                        int tickets = Convert.ToInt32(reader1["ticket_count"]);
                        customerTable.AddRow(name, tickets.ToString());
                        customerChartData.Add((name, tickets));
                    }
                    reader1.Close();
                    AnsiConsole.Write(customerTable);

                    // Check if customer data exists and create chart
                    if (customerChartData.Count > 0)
                    {
                        var chart = new BarChart()
                            .Width(60)
                            .Label("[green bold]Tickets per Customer[/]")
                            .CenterLabel();

                        foreach (var (name, tickets) in customerChartData)
                        {
                            chart.AddItem(name, tickets, Color.Blue);
                        }
                        AnsiConsole.Write(chart);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]No ticket data to display for customers.[/]");
                    }

                    // SECTION: Global Stats
                    AnsiConsole.Write(new Rule("[yellow]Global Stats[/]"));

                    // Safe count queries to avoid null returns
                    int customerCount = Convert.ToInt32(new NpgsqlCommand("SELECT COUNT(*) FROM Customer", connection).ExecuteScalar());
                    int staffCount = Convert.ToInt32(new NpgsqlCommand("SELECT COUNT(*) FROM Staff", connection).ExecuteScalar());
                    int eventCount = Convert.ToInt32(new NpgsqlCommand("SELECT COUNT(*) FROM Event", connection).ExecuteScalar());
                    int ticketCount = Convert.ToInt32(new NpgsqlCommand("SELECT COUNT(*) FROM Tickets", connection).ExecuteScalar());

                    var panelContent = new Grid();
                    panelContent.AddColumn();
                    panelContent.AddRow($"Total Customers: [bold green]{customerCount}[/]");
                    panelContent.AddRow($"Total Staff: [bold green]{staffCount}[/]");
                    panelContent.AddRow($"Total Events: [bold green]{eventCount}[/]");
                    panelContent.AddRow($"Total Tickets Sold: [bold green]{ticketCount}[/]");

                    AnsiConsole.Write(new Panel(panelContent)
                        .Header("Overview", Justify.Center)
                        .Border(BoxBorder.Rounded)
                        .BorderStyle(new Style(Color.Orange1)));

                    // SECTION: Tickets per Event
                    AnsiConsole.Write(new Rule("[yellow]Tickets per Event[/]"));

                    var ticketPerEventCmd = new NpgsqlCommand(@"
            SELECT e.Event_name, COUNT(t.ticket_id) AS ticket_count
            FROM Event e
            LEFT JOIN Tickets t ON e.event_id = t.event_id
            GROUP BY e.Event_name
            ORDER BY ticket_count DESC", connection);

                    var reader2 = ticketPerEventCmd.ExecuteReader();

                    var eventTable = new Table();
                    eventTable.AddColumn("Event");
                    eventTable.AddColumn("Tickets Sold");

                    var eventChartData = new List<(string name, int total)>();

                    while (reader2.Read())
                    {
                        string eventName = reader2.GetString(0);
                        int total = reader2.GetInt32(1);
                        eventTable.AddRow(eventName, total.ToString());
                        eventChartData.Add((eventName, total));
                    }
                    reader2.Close();

                    AnsiConsole.Write(eventTable);

                    // Check if event data exists and create chart
                    if (eventChartData.Count > 0)
                    {
                        var eventBarChart = new BarChart()
                            .Width(60)
                            .Label("[green]Event Ticket Sales Chart[/]")
                            .CenterLabel();

                        foreach (var (name, total) in eventChartData)
                        {
                            eventBarChart.AddItem(name, total, Color.Aqua);
                        }
                        AnsiConsole.Write(eventBarChart);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]No ticket data to display for events.[/]");
                    }

                    // SECTION: Events per Staff
                    AnsiConsole.Write(new Rule("[yellow]Events per Staff[/]"));

                    var staffEventCmd = new NpgsqlCommand(@"
            SELECT s.name, COUNT(es.event_id) AS event_count
            FROM Staff s
            LEFT JOIN Organisation es ON s.staff_id = es.staff_id
            GROUP BY s.name
            ORDER BY event_count DESC", connection);

                    var reader3 = staffEventCmd.ExecuteReader();

                    var staffTable = new Table();
                    staffTable.AddColumn("Staff");
                    staffTable.AddColumn("Events Assigned");

                    var staffEventData = new List<(string name, int count)>();

                    while (reader3.Read())
                    {
                        string staffName = reader3.GetString(0);
                        int count = reader3.GetInt32(1);
                        staffEventData.Add((staffName, count));
                        staffTable.AddRow(staffName, count.ToString());
                    }
                    reader3.Close();

                    AnsiConsole.Write(staffTable);

                    // Check if staff data exists and create chart
                    if (staffEventData.Count > 0)
                    {
                        var staffBarChart = new BarChart()
                            .Width(60)
                            .Label("[green]Staff Assignments[/]")
                            .CenterLabel();

                        foreach (var (name, count) in staffEventData)
                        {
                            staffBarChart.AddItem(name, count, Color.Orange1);
                        }
                        AnsiConsole.Write(staffBarChart);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]No staff assignments to display.[/]");
                    }

                    // SECTION: Revenue per Event
                    AnsiConsole.Write(new Rule("[yellow]Revenue per Event[/]"));

                    var revenueCmd = new NpgsqlCommand(@"
            SELECT e.Event_name, COALESCE(SUM(t.price), 0) AS total_revenue
            FROM Event e
            LEFT JOIN Tickets t ON e.event_id = t.event_id
            GROUP BY e.Event_name
            ORDER BY total_revenue DESC", connection);

                    var revenueReader = revenueCmd.ExecuteReader();

                    var revenueTable = new Table();
                    revenueTable.AddColumn("Event");
                    revenueTable.AddColumn("Total Revenue");

                    var revenueData = new List<(string name, decimal revenue)>();

                    while (revenueReader.Read())
                    {
                        string eventName = revenueReader.GetString(0);
                        decimal revenue = revenueReader.GetDecimal(1);
                        revenueData.Add((eventName, revenue));
                        revenueTable.AddRow(eventName, $"{revenue:N0} €");
                    }
                    revenueReader.Close();

                    AnsiConsole.Write(revenueTable);

                    // Check if revenue data exists and create chart
                    if (revenueData.Count > 0)
                    {
                        var revenueChart = new BarChart()
                            .Width(60)
                            .Label("[green]Event Revenues[/]")
                            .CenterLabel();

                        foreach (var (name, amount) in revenueData)
                        {
                            revenueChart.AddItem(name, (float)amount, Color.Green);
                        }
                        AnsiConsole.Write(revenueChart);
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]No revenue data to display.[/]");
                    }

                    // Allow user to return to the main menu
                    AnsiConsole.MarkupLine("Press ENTER to return to the main menu.");
                    Console.ReadLine();
                }
            }
            catch (Exception ex)
            {
                // Log any errors
                Console.WriteLine($"Error: {ex.Message}");
            }

        }

    }
}
