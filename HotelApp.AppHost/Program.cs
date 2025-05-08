var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.HotelApp>("hotelapp");

builder.Build().Run();
