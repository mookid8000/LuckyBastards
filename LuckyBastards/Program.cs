﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Tababular;

namespace LuckyBastards;

class Program
{
    const string FilePath = @"C:\temp\Azure_Functions_Hello_World__Real_life_usecases.csv";

    static readonly TableFormatter Formatter = new(new Hints { CollapseVerticallyWhenSingleLine = true });

    static async Task Main()
    {
        var lines = await File.ReadAllLinesAsync(FilePath);

        var columns = lines.First().Split('\t');

        var rows = lines.Skip(1)
            .Select(line => line.Split('\t'))
            .Where(cells => cells.Length == columns.Length)
            .ToList();

        PrintRows(rows, columns);

        var rowsInRandomOrder = await rows.InRandomOrderAsync();

        using (var enumerator = rowsInRandomOrder.GetEnumerator())
        {
            while (enumerator.MoveNext())
            {
                Console.WriteLine("Tryk (N) for NÆSTE VINDER, (Q) for at slutte.");

                if (Console.ReadKey(true).KeyChar == 'q') break;

                Console.WriteLine("VINDEREN ER:");

                PrintRows(new[] { enumerator.Current }, columns);

                Console.WriteLine();

                var meetupProfileUrl = enumerator.Current?.Last();

                OpenUrl(meetupProfileUrl);
            }
        }

        Console.WriteLine("Slut");
        Console.ReadLine();
    }

    static void PrintRows(IEnumerable<string[]> rows, string[] columns)
    {
        var dictionaries = rows
            .Select(cells => cells.Zip(columns, (cell, header) => (cell, header)))
            .Select(tuples => tuples.ToDictionary(a => a.header, a => a.cell))
            .ToList();

        Console.WriteLine(Formatter.FormatDictionaries(dictionaries));
    }

    static void OpenUrl(string url)
    {
        try
        {
            Process.Start(url);
        }
        catch (Exception exception)
        {
            // hack because of this: https://github.com/dotnet/corefx/issues/10361
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                url = url.Replace("&", "^&");
                Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Process.Start("xdg-open", url);
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Process.Start("open", url);
            }
            else
            {
                Console.WriteLine($"Sorry! Could not open '{url}' because of this: {exception}");
            }
        }
    }
}