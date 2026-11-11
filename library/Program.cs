using library.Data;
using Npgsql;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllersWithViews();

// Database: read the connection string from User Secrets and create one shared data source.
var connectionString = builder.Configuration.GetConnectionString("Library");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException("Connection string 'Library' is missing. Add it in Manage User Secrets.");
}
var dataSource = NpgsqlDataSource.Create(connectionString);
builder.Services.AddSingleton(dataSource);

// Repositories: one new object per web request.
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<MemberRepository>();
builder.Services.AddScoped<LoanRepository>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// THIS IS THE LINE THAT FIXES YOUR CSS!
app.UseStaticFiles();

app.UseRouting();

app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();