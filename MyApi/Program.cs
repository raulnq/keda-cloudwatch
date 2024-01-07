using Amazon.CloudWatch;
using System.Threading.Channels;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDefaultAWSOptions(builder.Configuration.GetAWSOptions());
builder.Services.AddAWSService<IAmazonCloudWatch>();
builder.Services.AddHostedService<CloudWatchChannelConsumer>();
var channel = Channel.CreateUnbounded<string>();
builder.Services.AddSingleton(channel);
var app = builder.Build();
app.MapGet("/", () => "Hello World!");
var client = app.Services.GetRequiredService<IAmazonCloudWatch>();
var listener = new RequestPerSecondChannelProducer(channel);
app.Run();