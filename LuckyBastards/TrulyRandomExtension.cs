using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace LuckyBastards
{
    static class TrulyRandomExtension
    {
        public static async Task<IReadOnlyCollection<T>> InRandomOrder<T>(this IEnumerable<T> items)
        {
            using (var client = new HttpClient { BaseAddress = new Uri("https://www.random.org") })
            {
                var list = items.ToList();
                var max = list.Count - 1;

                var randomNumbers = new ConcurrentQueue<int>();

                async Task<int> NextRandomIndex()
                {
                    while (true)
                    {
                        if (randomNumbers.TryDequeue(out var result)) return result;

                        var route = $"integers/?num=100&min=0&max={max}&col=1&base=10&format=plain&rnd=new";
                        var lines = await client.GetStringAsync(route);

                        var numbers = lines
                            .Split("\r\n".ToCharArray(), StringSplitOptions.RemoveEmptyEntries)
                            .Select(int.Parse);

                        foreach (var number in numbers)
                        {
                            randomNumbers.Enqueue(number);
                        }
                    }
                }

                var iterations = list.Count * 5;

                for (var counter = 0; counter < iterations; counter++)
                {
                    var (index1, index2) = (await NextRandomIndex(), await NextRandomIndex());

                    (list[index1], list[index2]) = (list[index2], list[index1]);
                }

                return list;
            }
        }
    }
}