using System.Diagnostics;
using BenchmarkDotNet.Running;
using BettingEdge.POC.ODataToMongo.Models;
using Microsoft.AspNetCore.OData;
using Microsoft.OData.ModelBuilder;

namespace BettingEdge.POC.ODataToMongo
{
    public class Program
	{
		public static void Main(string[] args)
		{

			//BenchmarkRunner.Run<ODataQueryBuilder<TodoItem>>();
			//return;

			var builder = WebApplication.CreateBuilder(args);

			// Add services to the container.

			builder.Services.AddControllers();
			// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
			builder.Services.AddEndpointsApiExplorer();

			var modelBuilder = new ODataConventionModelBuilder();
			modelBuilder.EntitySet<TodoItem>("Todos");

			builder.Services.AddControllers()
				.AddOData(
					options => options.Select().Filter().OrderBy().Expand().Count().SetMaxTop(null)
						.AddRouteComponents(
							routePrefix: "odata",
							model: modelBuilder.GetEdmModel()));

			builder.Services.AddSwaggerGen();
			var app = builder.Build();

			// Configure the HTTP request pipeline.
			if (app.Environment.IsDevelopment())
			{
				app.UseSwagger();
				app.UseSwaggerUI();
			}

			app.Use(async (context, next) =>
			{
				Stopwatch sw = Stopwatch.StartNew();
				await next(context);
				sw.Stop();
				Console.WriteLine($"\n\t\t\t ELAPSED TIME: {sw.ElapsedMilliseconds} ms\n");
			});


			app.UseODataRouteDebug();
			// Add OData /$query middleware
			app.UseODataQueryRequest();
			// Add the OData Batch middleware to support OData $Batch
			app.UseODataBatching();


			app.MapControllers();

			app.Run();
		}
	}
}