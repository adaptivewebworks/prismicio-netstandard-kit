using System;
using System.Linq;
using System.Collections.Generic;
using System.Globalization;
using System.Collections;

namespace prismic
{
    public interface IPredicate
    {
        string q();
    }

    public class Predicate : IPredicate
    {

        private static readonly CultureInfo _defaultCultureInfo = new CultureInfo("en-US");
        private readonly string Name;
        private readonly string Fragment;
        private readonly object Value1;
        private readonly object Value2;
        private readonly object Value3;

        public Predicate(string name, string path) : this(name, path, null, null, null) { }

        public Predicate(string name, string fragment, object value1) : this(name, fragment, value1, null, null) { }

        public Predicate(string name, string fragment, object value1, object value2) : this(name, fragment, value1, value2, null) { }

        public Predicate(string name, string fragment, object value1, object value2, object value3)
        {
            Name = name;
            Fragment = fragment;
            Value1 = value1;
            Value2 = value2;
            Value3 = value3;
        }

        public string q()
        {
            string result = "[:d = " + Name + "(";
            if ("similar" == Name)
            {
                result += ("\"" + Fragment + "\"");
            }
            else
            {
                result += Fragment;
            }

            var values = string.Join(
                ", ",
                new[] { Value1, Value2, Value3 }
                    .Where(x => x != null)
                    .Select(SerializeField)
                    .ToList()
            );

            if (!string.IsNullOrWhiteSpace(values))
                result += $", {values}";

            result += ")]";
            return result;
        }

        private static string SerializeField(object value)
        {
            switch (value)
            {
                case string s:
                    return $"\"{value}\"";

                case IEnumerable items:
                    var serializedItems = items.Cast<object>().Select(item => SerializeField(item));
                    return $"[{string.Join(",", serializedItems)}]";

                case Predicates.Months months:
                    return $"\"{Capitalize(months.ToString())}\"";

                case DayOfWeek day:
                    return $"\"{Capitalize(day.ToString())}\"";

                case DateTime dt:
                    return (dt - new DateTime(1970, 1, 1)).TotalMilliseconds.ToString(_defaultCultureInfo);

                case double d:
                    return d.ToString(_defaultCultureInfo);

                case decimal d:
                    return d.ToString(_defaultCultureInfo);

                default:
                    return value.ToString();
            }
        }

        private static string Capitalize(string line)
        {
            if (string.IsNullOrWhiteSpace(line))
                return "";

            return line.Substring(0, 1).ToUpper() + line.Substring(1).ToLower();
        }
    }

    public static class Predicates
    {

        public enum Months
        {
            January, February, March, April, May, June,
            July, August, September, October, November, December
        }

        /**
         * TODO this is the biggest candidate for idomatic refactoring, capitalizing method names.
         * The cost being compatibility, but this could be handled using a compat library.
         */
        public static IPredicate at(string fragment, string value) => new Predicate("at", fragment, value);

        public static IPredicate at(string fragment, string[] values) => new Predicate("at", fragment, values);

        public static IPredicate any(string fragment, IEnumerable<string> values) => new Predicate("any", fragment, values);

        public static IPredicate @in(string fragment, IEnumerable<string> values) => new Predicate("in", fragment, values);

        public static IPredicate fulltext(string fragment, string value) => new Predicate("fulltext", fragment, value);

        public static IPredicate similar(string documentId, int maxResults) => new Predicate("similar", documentId, maxResults);

        public static IPredicate lt(string fragment, double lowerBound) => new Predicate("number.lt", fragment, lowerBound);

        public static IPredicate lt(string fragment, int lowerBound) => new Predicate("number.lt", fragment, lowerBound);

        public static IPredicate gt(string fragment, double upperBound) => new Predicate("number.gt", fragment, upperBound);

        public static IPredicate gt(string fragment, int upperBound) => new Predicate("number.gt", fragment, (double)upperBound);

        public static IPredicate inRange(string fragment, int lowerBound, int upperBound) => new Predicate("number.inRange", fragment, (double)lowerBound, (double)upperBound);

        public static IPredicate inRange(string fragment, double lowerBound, double upperBound) => new Predicate("number.inRange", fragment, lowerBound, upperBound);

        public static IPredicate dateBefore(string fragment, DateTime before) => new Predicate("date.before", fragment, before);

        public static IPredicate dateAfter(string fragment, DateTime after) => new Predicate("date.after", fragment, after);

        public static IPredicate dateBetween(string fragment, DateTime lower, DateTime upper) => new Predicate("date.between", fragment, lower, upper);

        public static IPredicate dayOfMonth(string fragment, int day) => new Predicate("date.day-of-month", fragment, day);

        public static IPredicate dayOfMonthBefore(string fragment, int day) => new Predicate("date.day-of-month-before", fragment, day);

        public static IPredicate dayOfMonthAfter(string fragment, int day) => new Predicate("date.day-of-month-after", fragment, day);

        public static IPredicate dayOfWeek(string fragment, DayOfWeek day) => new Predicate("date.day-of-week", fragment, day);

        public static IPredicate dayOfWeekAfter(string fragment, DayOfWeek day) => new Predicate("date.day-of-week-after", fragment, day);

        public static IPredicate dayOfWeekBefore(string fragment, DayOfWeek day) => new Predicate("date.day-of-week-before", fragment, day);

        public static IPredicate month(string fragment, Months month) => new Predicate("date.month", fragment, month);

        public static IPredicate monthBefore(string fragment, Months month) => new Predicate("date.month-before", fragment, month);

        public static IPredicate monthAfter(string fragment, Months month) => new Predicate("date.month-after", fragment, month);

        public static IPredicate year(string fragment, int year) => new Predicate("date.year", fragment, year);

        public static IPredicate hour(string fragment, int hour) => new Predicate("date.hour", fragment, hour);

        public static IPredicate hourBefore(string fragment, int hour) => new Predicate("date.hour-before", fragment, hour);

        public static IPredicate hourAfter(string fragment, int hour) => new Predicate("date.hour-after", fragment, hour);

        public static IPredicate near(string fragment, double latitude, double longitude, int radius) => new Predicate("geopoint.near", fragment, latitude, longitude, radius);

        public static IPredicate not(string fragment, string value) => new Predicate("not", fragment, value);

        public static IPredicate not(string fragment, string[] values) => new Predicate("not", fragment, values);

        public static IPredicate has(string path) => new Predicate("has", path);
    }
}

