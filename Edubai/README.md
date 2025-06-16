# Setup (VSCODE):
## Android:
1. Start Emulator (emulator -avd Pixel_6_Pro)
2. Change into BlazorMaui directory (cd BlazorMaui)
3. Start BlazorMaui (dotnet build -t:Run -f net7.0-android)

## Web:
1. Change into BlazorWasm directory (cd BlazorWasm)
2. Start BlazorWasm (dotnet watch)

# Setup (Visual Studio):
Use Project selection to start BlazorMaui or BlazorWasm



# Shared Components
Any assets in (any) wwwroot can be accessed in both BlazorMaui and BlazorWasm with prefix /_content/SharedComponents/...

Changed need to be made in: BlazorMaui and BlazorWasm

[ ]: # Path: BlazorMaui\wwwroot\index.html
[ ]: # Path: BlazorWasm\wwwroot\index.html



# Setup PostgreSQL
1. Setup PostgreSQL Database (eg. with pgAdmin)
2. Update EntityFramework in SharedComponents
    dotnet ef dbcontext scaffold "Name=ConnectionStrings:EdubaiPostgreSQL" Npgsql.EntityFrameworkCore.PostgreSQL -o PostgreSQL -f
    or
    dotnet ef dbcontext scaffold "Host=homebai.duckdns.org:5432;Username=edubai;Password=<Password>;Database=edubai" Npgsql.EntityFrameworkCore.PostgreSQL -o PostgreSQL -f
3. When encoutering errors while building the ef framework regarding MAUI, remove all references to Maui for the time of the build, and return to normal afterwards (e.g. "UseMauiEssentials")
4. Update Generated Files to not include the connectionstring


# Public Project
For depolyment on a selfhosted server, simply build the BlazorWasmHost project with the preset configuration and copy the files to the server.
