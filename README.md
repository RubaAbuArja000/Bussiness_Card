# Business Card Application

A web application for creating, updating, and managing digital business cards. Built with Angular for the front end and .NET Core 8 with SQL for the backend. Users can import contacts from CSV and XML files, generate QR codes, and save business cards for easy access.

## Features

- **Create Business Card**: Users can create a new business card by filling out a form with their details.
  
- **Update Business Card**: Existing business cards can be edited to reflect any changes.

- **Show List**: View a list of all created business cards for quick access and management.

- **Import from CSV**: Import contacts from CSV files to quickly add multiple business cards.

- **Import from XML**: Import contacts from XML files for those who prefer this format.

- **Generate QR Code**: Each business card can generate a unique QR code that links to the user's contact information.

- **Attach QR Code on Save**: A QR code will be automatically generated and attached to each business card when saved.

## Technologies Used

- **Frontend**: Angular
- **Backend**: .NET Core 8
- **Database**: SQL Server
- **QR Code Generation**: [QR Code Library](https://github.com/your-qr-code-library)

## Installation

### Prerequisites

- [.NET Core SDK 8](https://dotnet.microsoft.com/download)
- [Node.js](https://nodejs.org/)
- SQL Server

### Backend Setup

To set up the backend, first navigate to the backend directory using `cd business-card-app/backend`. Then, restore the NuGet packages with `dotnet restore`. Next, update the connection string in `appsettings.json` to point to your SQL Server instance. Run the migrations to set up the database by executing `dotnet ef database update`, and finally, start the backend server with `dotnet run`. 

### Frontend Setup

For the frontend, navigate to the frontend directory using `cd ../frontend`, and install the necessary dependencies with `npm install`. Start the Angular application by running `ng serve`, and open your browser to `http://localhost:4200`.

## Importing Contacts

Ensure your CSV file is structured as follows: `Name,Email,Phone,Company,Title` with an example entry like `John Doe,john@example.com,123-456-7890,Example Inc,Developer`. For XML, the structure should be: `<contacts><contact><name>John Doe</name><email>john@example.com</email><phone>123-456-7890</phone><company>Example Inc</company><title>Developer</title></contact></contacts>`.

## Generating QR Codes

Each business card will automatically generate a QR code upon saving, allowing it to be scanned for easy access to the card’s details.

## Contributing

Contributions are welcome! Please fork the repository and submit a pull request for any enhancements or bug fixes.
