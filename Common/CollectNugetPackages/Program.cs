using System;
using System.IO;
using Microsoft.Extensions.Configuration;

var builder = new ConfigurationBuilder().SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false);
IConfiguration config = builder.Build();
var sourceNugetPath = config["SourceNugetPath"];
var targetNugetPath = config["TargetNugetPath"];
if(!Directory.Exists(sourceNugetPath))
{
    throw new Exception($"Не существует папки - {sourceNugetPath}");
}
//очищаем папку, куда будем копировать nuget-пакеты (если там есть какие-то файлы)
var di = new DirectoryInfo(targetNugetPath!);
foreach(FileInfo file in di.GetFiles()) file.Delete();
//в каждой папке ищем файл с расширением .nupkg
var nuget_files = Directory.GetFiles(sourceNugetPath, "*.nupkg", SearchOption.AllDirectories);
foreach(var nuget_file in nuget_files)
{
    File.Copy(nuget_file, $"{targetNugetPath}/{Path.GetFileName(nuget_file)}" );
}
Console.WriteLine($"Обновлено {nuget_files.Length} пакетов");




