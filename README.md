# MSTeams-notifier
MSTeams-notifier is a C# library/application for programmatically sending messages to Microsoft Teams channels. It can be used for alerts, notifications, or any messaging automation across various domains.

A proof-of-concept application demonstrating Microsoft Teams integration for operations using Adaptive Cards and Microsoft Graph API.

## Overview

This application enables  companies to send interactive notifications and shipment updates directly to Microsoft Teams channels. It showcases how logistics operations can be streamlined through real-time collaboration and actionable notifications.

## Features

- **Interactive Adaptive Cards** - Rich, actionable notifications with shipment details
- **Microsoft Teams Integration** - Send messages directly to Teams channels/chats
- **Authentication** - Secure MSAL-based Azure AD authentication
- **Real-time Updates** - Live shipment status with visual indicators
- **Business Actions** - Mark delivered, report delays, contact support, track live
- **Feedback Collection** - Multi-line input for delivery feedback and issue reporting

## Business Use Cases

- **Shipment Notifications** - Automated alerts for status changes, delays, arrivals
- **Actionable Workflows** - Enable teams to take immediate action from Teams
- **Customer Communication** - Streamlined feedback collection and issue reporting
- **Document Access** - Quick links to shipment documents and tracking
- **Team Collaboration** - Discuss shipments and escalate issues within Teams
- **KPI Dashboards** - Summary cards with delivery metrics and performance data

## Prerequisites

- .NET 9 SDK
- Microsoft Azure AD app registration
- Microsoft Teams with appropriate permissions
- Visual Studio 2022 or VS Code

## Setup

### 1. Azure AD App Registration

1. Navigate to [Azure Portal](https://portal.azure.com) → Azure Active Directory → App registrations
2. Create a new registration:
   - **Name**:   Teams Integration
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: Public client/native → `http://localhost:5000`
3. Note the **Application (client) ID** and **Directory (tenant) ID**
4. Under **API permissions**, add:
   - Microsoft Graph → Delegated permissions:
     - `Chat.ReadWrite`
     - `User.Read`
5. Grant admin consent for the permissions

### 2. Teams Setup

1. Get your Teams chat/channel ID:
   - Open Teams in web browser
   - Navigate to the desired chat/channel
   - Copy the ID from the URL (format: `19:xxx@thread.v2`)

### 3. Application Configuration

Update the following values in `Program.cs`:

```csharp
var tenantId = "YOUR_TENANT_ID";
var clientId = "YOUR_CLIENT_ID";
var chatId = "YOUR_TEAMS_CHAT_ID";
```

## Usage

1. **Build and Run**:
   ```bash
   dotnet build
   dotnet run
   ```

2. **Authentication**:
   - Application will prompt for sign-in on first run
   - Complete the interactive authentication flow
   - Token is reused for the session

3. **Send Messages**:
   - Type your message when prompted
   - Both text message and adaptive card will be sent to Teams
   - Type `exit` to quit

## Project Structure

```
├── Program.cs                 # Main application entry point
├── IAuthProvider              # Authentication abstraction
├── MsalAuthProvider          # MSAL implementation
├── ITeamsMessageService      # Teams messaging abstraction
├── TeamsMessageService       # Graph API implementation
├── AdaptiveCardFactory       # Adaptive card creation
└── README.md                 # This file
```

## Adaptive Card Features

The  demo card includes:

- **Visual Elements**:
  -  ship icon
  - Shipment ID with bold formatting
  - Color-coded status with emojis
  - Structured fact set (ETA, Origin, Destination)

- **Interactive Actions**:
  - Mark as Delivered
  - Report Delay
  - Track Live (opens maps)
  - Contact Support

- **Input Collection**:
  - Multi-line feedback text area

## Dependencies

- `Microsoft.Identity.Client` - Azure AD authentication
- `Newtonsoft.Json` - JSON serialization
- `System.Net.Http` - HTTP client for Graph API calls

## Architecture Benefits

- **Modular Design** - Separated concerns with interfaces and implementations
- **Testable** - Dependency injection ready for unit testing
- **Scalable** - Easy to extend with additional card types and business logic
- **Maintainable** - Clean abstractions and single responsibility principle

## Demo Scenarios

Perfect for demonstrating:

1. **Real-time Logistics** - Show how Teams becomes a logistics command center
2. **Process Automation** - Demonstrate workflow integration capabilities
3. **Customer Experience** - Showcase improved communication and feedback
4. **Team Productivity** - Highlight collaborative decision-making

## Future Enhancements

- **Webhook Integration** - Automatic status updates from logistics systems
- **Power Automate Flows** - Trigger workflows from card actions
- **Custom Connectors** - Integration with ERP/WMS systems
- **Multi-tenant Support** - Scale across multiple  companies
- **Analytics Dashboard** - Track card interactions and response times

## Security Considerations

- Uses Microsoft Graph API with proper scopes
- Secure token handling with MSAL
- No sensitive data stored in application
- Audit trail through Teams message history

## Support

For questions or issues related to this POC, please contact the development team or create an issue in the repository.
 

--- 
