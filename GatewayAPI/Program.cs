using GatewayAPI.Services;

var builder = WebApplication.CreateBuilder(args);

builder.AddServiceDefaults(); // ?? ADD THIS


builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// ?? Gateway dependency
// ?? THIS WAS MISSING / WRONG
builder.Services.AddHttpClient<CandidateClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7199"); // ?? CandidateService URL
});
builder.Services.AddHttpClient<InterviewClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7200"); // InterviewService
});
builder.Services.AddHttpClient<RequirementClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7175"); // RequirementService port
});
builder.Services.AddHttpClient<ReportClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7266"); // ReportService port
});



var app = builder.Build();



if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseDefaultFiles();   // ?? REQUIRED
app.UseStaticFiles();
app.UseAuthorization();
app.MapControllers();
app.MapDefaultEndpoints(); // ?? ADD THIS
app.MapGet("/", () => Results.Redirect("/swagger"));
app.Run();
