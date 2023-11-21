# Backend-Task4

Bank API Development in ASP.NET 7 with CSV File Integration

Overview:
Develop an ASP.NET 7-based Bank API that manages a group of clients. Each client record includes a name, ID, salary, balance, and creation date.
The API should offer a range of services, ensuring data integrity, thread safety, and periodic backups.

Requirements:
    Client Operations:
    
        Create: Allow the creation of new client accounts.
		Requires: Name and Salary, the other fields should be filled by the API

        Delete: Provide functionality to delete client accounts; Its preferred to keep the details of the client but to add a value that indicates that its deleted.
		Requires: ID  

        Deposit: Enable clients to deposit funds into their accounts.
		Requires: ID, Deposit amount

        Withdraw: Allow clients to withdraw funds from their accounts.
		Requires: ID, Withdraw amount

        Transfer: Facilitate fund transfers between two clients (Taking funds from an account and adding it to another).
		Requires: Sender ID, Receiver ID, Transfer amount


	All requests should return meaningful response messages, No requests are allowed on deleted accounts


    Backup Functionality:
        Implement an automatic backup system that saves all client account data every 10 seconds while the API is running.

    Thread Safety:
        Ensure thread safety in the API operations to prevent race conditions. The API should handle concurrent requests without compromising data integrity.

    Monthly Processing:
        Adds the monthly salary to the balance of all clients every period of time (consider it 60 seconds for simplicity)

    Retrieval Services:
        Allow retrieval of client information(Entire record) based on the following criteria:
            Retrieve by ID.
            Retrieve by salary (higher or lower than a given value).
            Retrieve by creation date (after or before a given time).
            Retrieve by balance (higher or lower than a given value).
            Retrieve the client with the highest/lowest salary.
            Retrieve the client with the highest/lowest balance.
            Retrieve the oldest/newest account based on creation date.

    Logging:
        Maintain logs (of the query parameters) for each request, whether they are successful or not, their Timestamp and log exceptions.



    Clients:
	Download a sample csv file from
	https://github.com/YaseenAlali/JoVisionRNTasksSolutions/blob/main/BankApiTask/clients.csv

    Hints:
	Language Integrated Query (LINQ)
	Mutual Exclusion Locks (Mutex)
