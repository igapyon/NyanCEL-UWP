# NyanCEL-UWP

NyanCEL-UWP is a server tool that enables SQL queries on the contents of Excel workbooks (.xlsx).

- NyanCEL-UWP is implemented in UWP and C#.
- By providing an Excel workbook (.xlsx), it allows you to query each sheet as a table using SQL.
- Executes SQL queries and retrieves results via a REST interface.
- It is released under the MIT license.
- NyanCEL-UWP is part of the NyanCEL project.

## Relationship with the NyanQL Project

- NyanCEL is a friend project of the NyanQL project.
- It is an independent project inspired by the NyanQL project.
- NyanCEL respects and honors the NyanQL project.
- It is implemented in UWP and C# and is planned to be distributed through the Microsoft Store.

## Workflow

1. Provide an Excel workbook (.xlsx) file:
   - Use sheet names as table names.
   - Use the values of the first row as column names.
   - Derive column data types from the format of the second row.
2. Load the content of the specified Excel workbook into an SQLite database:
   - Read data from the second row onwards.
   - Load the read data into an in-memory SQLite.
3. Execute SELECT statements from the REST interface:
   - Search the database with the provided SELECT statement.
   - Use SQLite SQL syntax.
   - GET and POST methods are available (currently only GET is supported).
   - The default port number is 28096.
4. Return the search results of the SELECT statement:
   - Return search results as row data.
   - Support for json, xml, xlsx formats as return results.
   - By default, return search results as json data.
   - Add the parameter fmt=xml to change the return format to XML.
   - Add the parameter fmt=xlsx to change the return format to xlsx.
   - Specify the parameter fmt=json&target=data.1 to filter and specify return data.
   - Apply jsonpath to the search results with fmt=json&jsonpath=.
   - Apply xpath to the search results with fmt=xml&xpath=.

As a feature of UWP, only access to the desktop and removable disks is set.

## Internally Used OSS

NyanCEL-UWP uses the following OSS internally. We appreciate the providers of each OSS.

- ClosedXML
  - MIT
  - 0.102.2
- EmbedIO
  - MIT
  - 3.5.2
- Microsoft.Data.Sqlite
  - MIT
  - 8.0.6
- Microsoft.UI.Xaml
  - 2.8.6
- Newtonsoft.Json
  - MIT
  - 13.0.3
- Serilog
  - MIT
  - 4.0.0
- Serilog.Sinks.File
  - MIT
  - 5.0.0
- Igapyon.NyanCEL
  - MIT
  - 0.5.0

## Path of Operation Logs

Operation logs of NyanCEL are stored in the following folder hierarchy:

```sh
USERROOTPATH\AppData\Local\Packages\NyanCEL-XXXXXXXXXXXXX\LocalState
```

- Log files related to operation and executed SQL log files are created.

## SQL Useful for Operation Confirmation

```sh
http://IPADDRESS:28096/api?sql=SELECT%20*%20FROM%20sqlite_master
```

# Limitations

- Since it operates on an in-memory RDBMS, it may not work with large amounts of data.
- Only supports .xlsx files.
- Cannot connect via HTTP loopback. Please access from another machine.
- The app is basically intended to operate in the foreground.
- Double quotes cannot be included in the Excel sheet names or column names of the title row.

# Install

- Prepare NyanCEL_1.0.1.0_x86_x64_arm_arm64.cer.
- Use certlm.msc to open "Trusted Root Certification Authorities > Certificates".
- Right-click > All Tasks > Import. Install the certificate (NyanCEL_1.0.1.0_x86_x64_arm_arm64.cer).
- Double-click NyanCEL_1.0.1.0_x86_x64_arm_arm64.msixbundle to install.

# TODO

- (ASAP) Enable specifying BASIC authentication username and password at startup.
- (ASAP) Function to operate with https using user-specified certificates.
- Operation boundaries:
  - Error handling. Handle failures in loading Excel workbooks (inputting non-Excel data). Verify errors for file loading from locations other than documents or removable disks.
- Appearance:
  - Update the app store images appropriately.
  - Properly document the README.md.
- Testing:
  - Create test cases.
  - Obtain an official code signing key.
  - Verify operation in KIOSK mode.
  - Publish a beta version on the store.
  - Handle .xlsx files with columns of the same name.
  - Handle .xlsx files containing NyanRowId.
  - Handle .xlsx files with a null first row.
- Future version features:
  - Ensure some compatibility with NyanQL configuration files.
  - Add an API (/dml) for injecting DML.
  - Add an API (/exp) for exporting in-memory data.
  - Port number change feature.
  - URI scheme launch: Specify Excel workbook, port number, etc. as arguments.
  - Enable or disable SQL log output.
  - Support for CSV.
