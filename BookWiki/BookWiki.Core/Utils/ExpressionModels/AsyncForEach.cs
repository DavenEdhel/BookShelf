using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BookWiki.Core
{
    public class AsyncForEach<T>
    {
        private readonly IEnumerator<T> _source;

        public AsyncForEach(IEnumerable<T> source)
        {
            _source = source.GetEnumerator();
        }

        public async Task Execute(Action<T> body, Action onProgress)
        {
            while (true)
            {
                var moveNextTask = Task.Run(() =>
                {
                    try
                    {
                        return _source.MoveNext();
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                });
                while (moveNextTask.IsCompleted == false)
                {
                    await Task.WhenAny(moveNextTask, Task.Delay(TimeSpan.FromSeconds(1)));

                    onProgress();
                }

                var canMoveNext = await moveNextTask;

                if (canMoveNext)
                {
                    body(_source.Current);

                    onProgress();
                }
                else
                {
                    return;
                }
            }
        }
    }
}