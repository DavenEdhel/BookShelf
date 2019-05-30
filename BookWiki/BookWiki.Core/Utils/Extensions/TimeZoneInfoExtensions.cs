using System.Collections.Generic;

using Keurig.IQ.Core.CrossCutting.Extensions;

namespace System
{
    public static class TimeZoneInfoExtensions
    {
        private static readonly Dictionary<string, KeyValuePair<double, double>> TimeZonesDictionary =
            new Dictionary<string, KeyValuePair<double, double>>
                {
                    {
                        "EST",
                        new KeyValuePair<double, double>(44.35, -68.21)
                    },
                    {
                        "EDT",
                        new KeyValuePair<double, double>(44.35, -68.21)
                    },
                    {
                        "CST",
                        new KeyValuePair<double, double>(34.51, -93.05)
                    },
                    {
                        "CDT",
                        new KeyValuePair<double, double>(34.51, -93.05)
                    },
                    {
                        "MST",
                        new KeyValuePair<double, double>(38.68, -109.57)
                    },
                    {
                        "MDT",
                        new KeyValuePair<double, double>(38.68,
                                                         -109.57)
                    },
                    {
                        "PST",
                        new KeyValuePair<double, double>(46.85,
                                                         -121.75)
                    },
                    {
                        "PDT",
                        new KeyValuePair<double, double>(46.85,
                                                         -121.75)
                    },
                    {
                        "AKST",
                        new KeyValuePair<double, double>(63.33, -150.5)
                    },
                    {
                        "AKDT",
                        new KeyValuePair<double, double>(63.33, -150.5)
                    },
                    {
                        "HST",
                        new KeyValuePair<double, double>(20.72,
                                                         -156.17)
                    },
                    {
                        "HDT",
                        new KeyValuePair<double, double>(20.72,
                                                         -156.17)
                    }
                };

        public static bool ContainsTimeZoneCoordinates(this TimeZoneInfo timeZoneInfo)
        {
            return TimeZonesDictionary.ContainsKey(timeZoneInfo.StandardName);
        }
    }
}
