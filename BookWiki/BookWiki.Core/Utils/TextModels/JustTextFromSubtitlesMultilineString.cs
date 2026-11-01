using BookWiki.Core.Agents.SDK;
using Newtonsoft.Json;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using System.Reactive.Subjects;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BookWiki.Core.Utils.TextModels
{
    public interface ISentence
    {
        Subject<Unit> Changed { get; }

        string Origin { get; set; }

        string Lemma { get; }

        string Context { get; }
    }

    public class EngRuWordsTranslatorQueue
    {
        private Queue<ISentence> sentences = new Queue<ISentence>();

        private bool processing = false;

        public Action<(string Message, bool WithNewLine)> Log { get; set; } = (x) => { };

        private List<ISentence> all = new List<ISentence>();

        private async void ProcessUntilDone()
        {
            if (processing)
            {
                return;
            }

            processing = true;

            while (true)
            {
                var toInclude = all.ToArray().ToList();

                var pack = DequeuPack();

                if (pack.Any())
                {
                    try
                    {
                        var result = await Translate(pack.ToArray());

                        WriteLog($"From agent '{result}'");

                        if (string.IsNullOrEmpty(result))
                        {
                            return;
                        }

                        var result2 = result.Split('|');
                        var agentResult = result2.Select(x => new AgentResult()
                        {
                            Lemma = x.Split('=')[0].Trim(),
                            Origin = x.Split('=')[1].Trim()
                        });

                        foreach (var fromAgent in agentResult)
                        {
                            var sentence = toInclude.FirstOrDefault(x => x.Lemma == fromAgent.Lemma);
                            if (sentence == null)
                            {
                                WriteLog($"Agent misused or failed to translate {fromAgent.Lemma}");
                            }
                            else
                            {
                                sentence.Origin = fromAgent.Origin;
                                sentence.Changed.OnNext(Unit.Default);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        WriteLog($"Could not translate {JsonConvert.SerializeObject(pack)}.");
                    }
                }
                else
                {
                    processing = false;

                    return;
                }
            }
        }

        private class AgentResult
        {
            public string Lemma;

            public string Origin;
        }

        private List<ISentence> DequeuPack()
        {
            var result = new List<ISentence>();

            lock (sentences)
            {
                for (var i = 0; i < 20; i++)
                {
                    if (sentences.Count > 0)
                    {
                        var c = sentences.Dequeue();

                        result.Add(c);
                    }
                    else
                    {
                        break;
                    }

                }
            }

            return result;

        }

        private void WriteLog(string message, bool inline = false)
        {
            Log((message, !inline));
        }

        public void QueueTranslation(ISentence sentence)
        {
            lock (sentences)
            {
                sentences.Enqueue(sentence);
            }

            all.Add(sentence);

            ProcessUntilDone();
        }

        public async Task<string> Translate(ISentence[] words)
        {
            return await Task.Run(async () =>
            {
                var agent = new Agent(
                    "Ты переводчик, умеешь переводить слова в контексте на русский язык. " +
                    "Переведи каждый Word, используя Context для определения правильного перевода на русский язык. " +
                    "Глаголы ставь в инфинитив совершенной формы если это возможно. " +
                    "Прилагательные делай мужского рода. " +
                    "Если запутался, бери последний свой вывод. " +
                    "Используй формат пара (слово из Word=перевод одним словом или одной фразой) разделённые |, строго в одну строку, пример: do=делать|work=работа. " +
                    "Кроме этого ничего не пиши"
                );

                agent.Log.Subscribe(x =>
                {
                    if (x.Category == AgentLogCategory.Reasoning)
                    {
                        WriteLog(x.Message, inline: true);
                    }
                });

                var response = await agent.Ask(JsonConvert.SerializeObject(words.Select(x => new WordToTranslate()
                {
                    Word = x.Lemma,
                    Context = x.Context
                }).ToArray()));

                return response;
            });


        }

        private class WordToTranslate
        {
            public string Word { get; set; }

            public string Context { get; set; }
        }
    }

    public class JustTextFromSubtitlesMultilineString : IEnumerable<string>
    {
        private readonly string[] lines;

        public JustTextFromSubtitlesMultilineString(string[] lines)
        {
            this.lines = lines;
        }

        public IEnumerator<string> GetEnumerator()
        {
            foreach(var l in lines)
            {
                var cleaned = RemoveBrackets(l);
                cleaned = RemoveTags(cleaned);

                if (string.IsNullOrEmpty(cleaned))
                {
                    continue;
                }

                if (cleaned.Any(x => char.IsLetter(x)) == false)
                {
                    continue;
                }

                yield return cleaned;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        private static readonly Regex BracketRegex = new Regex(@"\[[^\]]*\]", RegexOptions.Compiled);
        private static readonly Regex CleanSpacesRegex = new Regex(@"\s+", RegexOptions.Compiled);

        public static string RemoveBrackets(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return input;

            // 1. Вырезаем всё внутри квадратных скобок [shouts] вместе со скобками
            string result = BracketRegex.Replace(input, "");

            // 2. Схлопываем двойные пробелы и убираем пробелы по краям
            return CleanSpacesRegex.Replace(result, " ").Trim();
        }

        public static string RemoveTags(string input)
        {
            return input.Replace("<i>", "").Replace("</i>", "");
        }
    }

    public static class VocabularyDefaults
    {
        public static readonly string[] Top500English = new string[]
        {
            "the", "and", "to", "of", "a", "in", "that", "is", "it", "you", "that", "he", "was", "for", "on", "are",
            "as", "with", "his", "they", "i", "at", "be", "this", "have", "from", "or", "one", "had", "by", "word",
            "but", "not", "what", "all", "were", "we", "when", "your", "can", "said", "there", "use", "an", "each",
            "which", "she", "do", "how", "their", "if", "will", "up", "other", "about", "out", "many", "then", "them",
            "these", "so", "some", "her", "would", "make", "like", "him", "into", "time", "has", "look", "two", "more",
            "write", "go", "see", "number", "no", "way", "could", "people", "my", "than", "first", "water", "been",
            "call", "who", "oil", "its", "now", "find", "long", "down", "day", "did", "get", "come", "made", "may",
            "part", "over", "new", "sound", "take", "only", "little", "work", "know", "place", "year", "live", "me",
            "back", "give", "most", "very", "after", "thing", "our", "just", "name", "good", "sentence", "man", "think",
            "say", "great", "where", "help", "through", "much", "before", "line", "right", "too", "mean", "old", "any",
            "same", "tell", "boy", "follow", "came", "want", "show", "also", "around", "form", "three", "small", "set",
            "put", "end", "does", "another", "well", "large", "must", "big", "even", "such", "because", "turn", "here",
            "why", "ask", "went", "men", "read", "need", "land", "different", "home", "us", "move", "try", "kind",
            "hand", "picture", "again", "change", "off", "play", "spell", "air", "away", "animal", "house", "point",
            "page", "letter", "mother", "answer", "found", "study", "still", "learn", "should", "america", "world",
            "high", "every", "near", "add", "food", "between", "own", "below", "country", "plant", "last", "school",
            "father", "keep", "tree", "never", "start", "city", "earth", "eyes", "head", "story", "saw", "left",
            "don't", "few", "while", "along", "might", "close", "something", "seem", "next", "hard", "open", "example",
            "begin", "life", "always", "those", "both", "paper", "together", "got", "group", "often", "run", "important",
            "until", "children", "side", "feet", "car", "mile", "night", "walk", "white", "sea", "began", "grow",
            "took", "river", "four", "carry", "state", "once", "book", "hear", "stop", "without", "second", "later",
            "miss", "idea", "enough", "eat", "face", "watch", "far", "really", "almost", "let", "above", "girl",
            "sometimes", "mountain", "cut", "young", "talk", "soon", "list", "song", "being", "leave", "family", "it's",
            "body", "music", "color", "stand", "sun", "questions", "fish", "area", "mark", "dog", "horse", "birds",
            "problem", "complete", "room", "knew", "since", "ever", "piece", "told", "usually", "didn't", "friends",
            "easy", "heard", "order", "red", "door", "sure", "become", "top", "ship", "across", "today", "during",
            "short", "better", "best", "however", "low", "hours", "black", "products", "happened", "whole", "measure",
            "remember", "early", "waves", "reached", "listen", "wind", "rock", "space", "covered", "fast", "several",
            "hold", "himself", "toward", "five", "step", "morning", "passed", "vowel", "true", "hundred", "against",
            "pattern", "num", "table", "north", "slow", "money", "map", "farm", "leather", "pull", "draw", "voice",
            "seen", "cold", "cried", "plan", "notice", "south", "sing", "war", "ground", "fall", "king", "town",
            "i'll", "unit", "figure", "certain", "field", "travel", "wood", "fire", "upon", "i'm", "ready", "feel",
            "can't", "couldn't", "wont", "brother", "sister", "son", "daughter", "marry", "love", "hate", "nice",
            "bad", "cool", "warm", "hot", "outside", "inside", "window", "floor", "wall", "ceiling", "kitchen",
            "bathroom", "bed", "chair", "table", "desk", "phone", "computer", "screen", "key", "mouse", "code",
            "job", "money", "pay", "buy", "sell", "price", "cost", "free", "cheap", "expensive", "rich", "poor",
            "clean", "dirty", "wash", "dry", "wet", "full", "empty", "heavy", "light", "dark", "bright", "soft",
            "hard", "loud", "quiet", "silent", "noise", "sound", "voice", "speak", "tell", "say", "ask", "answer",
            "question", "true", "false", "wrong", "correct", "right", "left", "front", "back", "side", "middle",
            "center", "corner", "edge", "line", "point", "shape", "circle", "square", "box", "bag", "case", "pack"
        };
    }

}
