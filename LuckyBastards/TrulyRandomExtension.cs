using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
// ReSharper disable ArgumentsStyleLiteral
// ReSharper disable ArgumentsStyleOther

namespace LuckyBastards;

static class TrulyRandomExtension
{
    public static async Task<IReadOnlyCollection<T>> InRandomOrderAsync<T>(this IEnumerable<T> items)
    {
        var list = items.ToList();
        var iterations = list.Count * 5;
            
        var numberGenerator = TrulyRandomNumbers(min: 0, max: list.Count - 1).GetAsyncEnumerator();

        async Task<int> NextRandomIndex() => await numberGenerator.MoveNextAsync()
            ? numberGenerator.Current
            : throw new ApplicationException("Did not expect to reach the end of this sequence");

        for (var counter = 0; counter < iterations; counter++)
        {
            var index1 = await NextRandomIndex();
            var index2 = await NextRandomIndex();

            (list[index1], list[index2]) = (list[index2], list[index1]);
        }

        return list;
    }

    static async IAsyncEnumerable<int> TrulyRandomNumbers(int min, int max)
    {
        using var client = new HttpClient { BaseAddress = new Uri("https://www.random.org") };

        while (true)
        {
            var route = $"integers/?num=100&min={min}&max={max}&col=1&base=10&format=plain&rnd=new";
            var lines = await client.GetStringAsync(route);

            var numbers = lines
                .Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse);

            foreach (var number in numbers)
            {
                yield return number;
            }
        }
    }
}