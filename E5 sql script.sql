CREATE TABLE Customer (
    Customer_id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    phone VARCHAR(20),
    age INT,
    email VARCHAR(100)
);

CREATE TABLE Event (
    Event_id SERIAL PRIMARY KEY,
    Event_name VARCHAR(100),
    Event_date DATE
);

CREATE TABLE Staff (
    Staff_id SERIAL PRIMARY KEY,
    name VARCHAR(100),
    position VARCHAR(50),
    email VARCHAR(100),
    password VARCHAR(100)
);

CREATE TABLE Tickets (
    Ticket_id SERIAL PRIMARY KEY,
    Customer_id INT,
    Event_id INT,
    purchaseDate DATE,
    MembershipLevel VARCHAR(50),
    price DECIMAL(10,2),
    FOREIGN KEY (Customer_id) REFERENCES Customer(Customer_id),
    FOREIGN KEY (Event_id) REFERENCES Event(Event_id)
);

CREATE TABLE Organisation (
    Staff_id INT,
    Event_id INT,
    PRIMARY KEY (Staff_id, Event_id),
    FOREIGN KEY (Staff_id) REFERENCES Staff(Staff_id),
    FOREIGN KEY (Event_id) REFERENCES Event(Event_id)
);