using CameraVideoRecorder.Arguments;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using CameraVideoRecorder.Camera;
using CameraVideoRecorder.Recording;
using CameraVideoRecorder.Ffmpeg;
using Azure.Identity;
using Azure.Storage.Blobs;
using CameraVideoRecorder.AzureIntegration;
using Azure.Security.KeyVault.Secrets;
using Serilog;
using Microsoft.Extensions.Logging;

HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

var argumentProvider = new CameraRecorderArgumentProvider();
argumentProvider.ParseArguments();

builder.Services.AddSingleton<ICameraRecorderArgumentProvider>(argumentProvider);

builder.Services.AddSingleton<ICameraIpService, CameraIpService>();
builder.Services.AddSingleton<IFfmpegService, FfMpegService>();
builder.Services.AddSingleton<IVideoStorer, VideoStorer>();
builder.Services.AddSingleton<ISecretProvider, SecretProvider>();

// DefaultAzureCredential will use environment variable
// See https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication/on-premises-apps?tabs=azure-portal%2Cwindows%2Ccommand-line#3---configure-environment-variables-for-application
builder.Services.AddSingleton(_ => new BlobServiceClient(new Uri("https://videorecorder.blob.core.windows.net"), new DefaultAzureCredential()));
builder.Services.AddSingleton(_ => new SecretClient(new Uri("https://videorecorderkv.vault.azure.net/"), new DefaultAzureCredential()));

builder.Services.AddHostedService<CameraRecorderHostedService>();

string fullPath = $"{argumentProvider.Arguments[ArgumentConstants.LogPath]}/log.txt";
Log.Logger = new LoggerConfiguration().WriteTo.File(fullPath, rollingInterval: RollingInterval.Day).CreateLogger();
builder.Services.AddLogging(x =>
{
    x.AddSerilog();
});

using IHost host = builder.Build();

await host.RunAsync();