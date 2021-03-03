using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Text.RegularExpressions;

namespace Dapper
{
    public static partial class SqlMapper
    {
        public interface IDynamicParameters
        {
            void AddParameter(IDbCommand command);
        }

        class CacheInfo
        {
            public object Deserializer { get; set; }
            public object[] OtherDeserializers { get; set; }
            public Action<IDbCommand, object> ParamReader { get; set; }
        }
#if CSHARP30
        private static readonly Dictionary<Identity, CacheInfo> _queryCache = new Dictionary<Identity, CacheInfo>();
        // note: conflicts between readers and writers are so short-lived that it isn't worth the overhead of
        // ReaderWriterLockSlim etc; a simple lock is faster
        private static void SetQueryCache(Identity key, CacheInfo value)
        {
            lock (_queryCache) { _queryCache[key] = value; }
        }
        private static bool TryGetQueryCache(Identity key, out CacheInfo value)
        {
            lock (_queryCache) { return _queryCache.TryGetValue(key, out value); }
        }
#else
        static readonly System.Collections.Concurrent.ConcurrentDictionary<Identity, CacheInfo> _queryCache = new System.Collections.Concurrent.ConcurrentDictionary<Identity, CacheInfo>();
        private static void SetQueryCache(Identity key, CacheInfo value)
        {
            _queryCache[key] = value;
        }
        private static bool TryGetQueryCache(Identity key, out CacheInfo value)
        {
            return _queryCache.TryGetValue(key, out value);
        }
#endif
        static readonly Dictionary<RuntimeTypeHandle, DbType> typeMap;

        static SqlMapper()
        {
            typeMap = new Dictionary<RuntimeTypeHandle, DbType>();
            typeMap[typeof(byte).TypeHandle] = DbType.Byte;
            typeMap[typeof(sbyte).TypeHandle] = DbType.SByte;
            typeMap[typeof(short).TypeHandle] = DbType.Int16;
            typeMap[typeof(ushort).TypeHandle] = DbType.UInt16;
            typeMap[typeof(int).TypeHandle] = DbType.Int32;
            typeMap[typeof(uint).TypeHandle] = DbType.UInt32;
            typeMap[typeof(long).TypeHandle] = DbType.Int64;
            typeMap[typeof(ulong).TypeHandle] = DbType.UInt64;
            typeMap[typeof(float).TypeHandle] = DbType.Single;
            typeMap[typeof(double).TypeHandle] = DbType.Double;
            typeMap[typeof(decimal).TypeHandle] = DbType.Decimal;
            typeMap[typeof(bool).TypeHandle] = DbType.Boolean;
            typeMap[typeof(string).TypeHandle] = DbType.String;
            typeMap[typeof(char).TypeHandle] = DbType.StringFixedLength;
            typeMap[typeof(Guid).TypeHandle] = DbType.Guid;
            typeMap[typeof(DateTime).TypeHandle] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset).TypeHandle] = DbType.DateTimeOffset;
            typeMap[typeof(byte[]).TypeHandle] = DbType.Binary;
            typeMap[typeof(byte?).TypeHandle] = DbType.Byte;
            typeMap[typeof(sbyte?).TypeHandle] = DbType.SByte;
            typeMap[typeof(short?).TypeHandle] = DbType.Int16;
            typeMap[typeof(ushort?).TypeHandle] = DbType.UInt16;
            typeMap[typeof(int?).TypeHandle] = DbType.Int32;
            typeMap[typeof(uint?).TypeHandle] = DbType.UInt32;
            typeMap[typeof(long?).TypeHandle] = DbType.Int64;
            typeMap[typeof(ulong?).TypeHandle] = DbType.UInt64;
            typeMap[typeof(float?).TypeHandle] = DbType.Single;
            typeMap[typeof(double?).TypeHandle] = DbType.Double;
            typeMap[typeof(decimal?).TypeHandle] = DbType.Decimal;
            typeMap[typeof(bool?).TypeHandle] = DbType.Boolean;
            typeMap[typeof(char?).TypeHandle] = DbType.StringFixedLength;
            typeMap[typeof(Guid?).TypeHandle] = DbType.Guid;
            typeMap[typeof(DateTime?).TypeHandle] = DbType.DateTime;
            typeMap[typeof(DateTimeOffset?).TypeHandle] = DbType.DateTimeOffset;
        }

        private static DbType LookupDbType(Type type)
        {
            DbType dbType;
            if (typeMap.TryGetValue(type.TypeHandle, out dbType))
            {
                return dbType;
            }
            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                // use xml to denote its a list, hacky but will work on any DB
                return DbType.Xml;
            }

            throw new NotSupportedException(string.Format("The type : {0} is not supported by dapper", type));
        }

        private class Identity : IEquatable<Identity>
        {

            internal Identity(string sql, IDbConnection cnn, Type type, Type parametersType, Type[] otherTypes)
            {
                this.sql = sql;
                this.connectionString = cnn.ConnectionString;
                this.type = type;
                this.parametersType = parametersType;
                unchecked
                {
                    hashCode = 17; // we *know* we are using this in a dictionary, so pre-compute this
                    hashCode = hashCode * 23 + (sql == null ? 0 : sql.GetHashCode());
                    hashCode = hashCode * 23 + (type == null ? 0 : type.GetHashCode());
                    if (otherTypes != null)
                    {
                        foreach (var t in otherTypes)
                        {
                            hashCode = hashCode * 23 + (t == null ? 0 : t.GetHashCode());
                        }
                    }
                    hashCode = hashCode * 23 + (connectionString == null ? 0 : connectionString.GetHashCode());
                    hashCode = hashCode * 23 + (parametersType == null ? 0 : parametersType.GetHashCode());
                }
            }
            public override bool Equals(object obj)
            {
                return Equals(obj as Identity);
            }
            private readonly string sql;
            private readonly int hashCode;
            private readonly Type type;
            private readonly string connectionString;
            internal readonly Type parametersType;
            public override int GetHashCode()
            {
                return hashCode;
            }
            public bool Equals(Identity other)
            {
                return
                    other != null &&
                    type == other.type &&
                    sql == other.sql &&
                    connectionString == other.connectionString &&
                    parametersType == other.parametersType;
            }
        }

        /// <summary>
        /// Execute parameterized SQL  
        /// </summary>
        /// <returns>Number of rows affected</returns>
        public static int Execute(
#if CSHARP30
            this IDbConnection cnn, string sql, object param, IDbTransaction transaction, int? commandTimeout, CommandType? commandType
#else
this IDbConnection cnn, string sql, dynamic param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
#endif
)
        {
            IEnumerable multiExec = (object)param as IEnumerable;
            Identity identity;
            CacheInfo info;
            if (multiExec != null && !(multiExec is string))
            { // we actually want a T from IEnumerable<T>
                var interfaces = multiExec.GetType().GetInterfaces();
                var openType = typeof(IEnumerable<>);
                Type foundType = null;
                for (int i = 0; i < interfaces.Length; i++)
                {
                    if (interfaces[i].IsGenericType && interfaces[i].GetGenericTypeDefinition() == openType)
                    { // implementing more than one T is self-inflicted
                        foundType = interfaces[i].GetGenericArguments()[0];
                        identity = new Identity(sql, cnn, null, foundType, null);
                        info = GetCacheInfo(identity);
                        using (var cmd = SetupCommand(cnn, transaction, sql, null, null, commandTimeout, commandType))
                        {
                            bool isFirst = true;
                            var masterSql = cmd.CommandText;
                            var reader = info.ParamReader;
                            int total = 0;
                            foreach (var obj in multiExec)
                            {
                                if (isFirst) { isFirst = false; }
                                else
                                {
                                    cmd.CommandText = masterSql; // because we do magic replaces on "in" etc
                                    cmd.Parameters.Clear(); // current code is Add-tastic
                                }
                                reader(cmd, obj);
                                total += cmd.ExecuteNonQuery();
                            }
                            return total;
                        }
                    }
                }

            }

            // nice and simple
            identity = new Identity(sql, cnn, null, (object)param == null ? null : ((object)param).GetType(), null);
            info = GetCacheInfo(identity);
            return ExecuteCommand(cnn, transaction, sql, info.ParamReader, (object)param, commandTimeout, commandType);
        }
#if !CSHARP30
        /// <summary>
        /// Return a list of dynamic objects, reader is closed after the call
        /// </summary>
        public static IEnumerable<dynamic> Query(this IDbConnection cnn, string sql, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null)
        {
            return Query<FastExpando>(cnn, sql, param as object, transaction, buffered, commandTimeout, commandType);
        }
#endif

        // the dynamic param may seem a bit odd, but this works around a major usability issue in vs, if it is Object vs completion gets annoying. Eg type new <space> get new object
        public static IEnumerable<T> Query<T>(
#if CSHARP30
            this IDbConnection cnn, string sql, object param, IDbTransaction transaction, bool buffered, int? commandTimeout, CommandType? commandType
#else
this IDbConnection cnn, string sql, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, int? commandTimeout = null, CommandType? commandType = null
#endif
)
        {
            var data = QueryInternal<T>(cnn, sql, param as object, transaction, commandTimeout, commandType);
            return buffered ? data.ToList() : data;
        }

        /// <summary>
        /// Execute a command that returns multiple result sets, and access each in turn
        /// </summary>
        public static GridReader QueryMultiple(
#if CSHARP30  
            this IDbConnection cnn, string sql, object param, IDbTransaction transaction, int? commandTimeout, CommandType? commandType
#else
this IDbConnection cnn, string sql, dynamic param = null, IDbTransaction transaction = null, int? commandTimeout = null, CommandType? commandType = null
#endif

)
        {
            Identity identity = new Identity(sql, cnn, typeof(GridReader), (object)param == null ? null : ((object)param).GetType(), null);
            CacheInfo info = GetCacheInfo(identity);

            IDbCommand cmd = null;
            IDataReader reader = null;
            try
            {
                cmd = SetupCommand(cnn, transaction, sql, info.ParamReader, (object)param, commandTimeout, commandType);
                reader = cmd.ExecuteReader();
                return new GridReader(cmd, reader);
            }
            catch
            {
                if (reader != null) reader.Dispose();
                if (cmd != null) cmd.Dispose();
                throw;
            }
        }

        /// <summary>
        /// Return a typed list of objects, reader is closed after the call
        /// </summary>
        private static IEnumerable<T> QueryInternal<T>(this IDbConnection cnn, string sql, object param, IDbTransaction transaction, int? commandTimeout, CommandType? commandType)
        {
            var identity = new Identity(sql, cnn, typeof(T), param == null ? null : param.GetType(), null);
            var info = GetCacheInfo(identity);

            using (var cmd = SetupCommand(cnn, transaction, sql, info.ParamReader, param, commandTimeout, commandType))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (info.Deserializer == null)
                    {
                        info.Deserializer = GetDeserializer<T>(reader, 0, -1, false);
                        SetQueryCache(identity, info);
                    }

                    var deserializer = (Func<IDataReader, T>)info.Deserializer;

                    while (reader.Read())
                    {
                        yield return deserializer(reader);
                    }
                }
            }
        }

        /// <summary>
        /// Maps a query to objects
        /// </summary>
        /// <typeparam name="T">The return type</typeparam>
        /// <typeparam name="U"></typeparam>
        /// <param name="cnn"></param>
        /// <param name="sql"></param>
        /// <param name="map"></param>
        /// <param name="param"></param>
        /// <param name="transaction"></param>
        /// <param name="buffered"></param>
        /// <param name="splitOn">The Field we should split and read the second object from (default: id)</param>
        /// <param name="commandTimeout">Number of seconds before command execution timeout</param>
        /// <returns></returns>
        public static IEnumerable<TReturn> Query<TFirst, TSecond, TReturn>(
#if CSHARP30  
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TReturn> map, object param, IDbTransaction transaction, bool buffered, string splitOn, int? commandTimeout, CommandType? commandType
#else
this IDbConnection cnn, string sql, Func<TFirst, TSecond, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null
#endif
)
        {
            return MultiMap<TFirst, TSecond, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }


        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TReturn>(
#if CSHARP30
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TReturn> map, object param, IDbTransaction transaction, bool buffered, string splitOn, int? commandTimeout, CommandType? commandType
#else
this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null
#endif
)
        {
            return MultiMap<TFirst, TSecond, TThird, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TReturn>(
#if CSHARP30
            this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, object param, IDbTransaction transaction, bool buffered, string splitOn, int? commandTimeout, CommandType? commandType
#else
this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null
#endif
)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

        /// Added By Gokul Starts

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, DontMap, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSixth, TSeventh, TEighth, TNinth, TTenth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, DontMap, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, DontMap, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, DontMap, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, DontMap, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, DontMap, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

#if !CSHARP30

        public static IEnumerable<TReturn> Query<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TReturn>(this IDbConnection cnn, string sql, Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TReturn> map, dynamic param = null, IDbTransaction transaction = null, bool buffered = true, string splitOn = "Id", int? commandTimeout = null, CommandType? commandType = null)
        {
            return MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TReturn>(cnn, sql, map, param as object, transaction, buffered, splitOn, commandTimeout, commandType);
        }

#endif

        /// Added By Gokul Ends

        class DontMap { }
        // Function Modified By Gokul for support 15 Multiple Mapping Objects
        static IEnumerable<TReturn> MultiMap<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TReturn>(
            this IDbConnection cnn, string sql, object map, object param, IDbTransaction transaction, bool buffered, string splitOn, int? commandTimeout, CommandType? commandType)
        {
            var results = MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TReturn>(cnn, sql, map, param, transaction, splitOn, commandTimeout, commandType);
            return buffered ? results.ToList() : results;
        }
        // Function Modified By Gokul for support 15 Multiple Mapping Objects
        static IEnumerable<TReturn> MultiMapImpl<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TReturn>(this IDbConnection cnn, string sql, object map, object param, IDbTransaction transaction, string splitOn, int? commandTimeout, CommandType? commandType)
        {
            Identity identity = new Identity(sql, cnn, typeof(TFirst), (object)param == null ? null : ((object)param).GetType(), new[] { typeof(TFirst), typeof(TSecond), typeof(TThird), typeof(TFourth), typeof(TFifth) });
            CacheInfo info = GetCacheInfo(identity);
            info.Deserializer = null;
            using (var cmd = SetupCommand(cnn, transaction, sql, info.ParamReader, (object)param, commandTimeout, commandType))
            {
                using (var reader = cmd.ExecuteReader())
                {
                    if (info.Deserializer == null)
                    {
                        int current = 0;

                        var splits = splitOn.Split(',').ToArray();
                        var splitIndex = 0;

                        Func<int> nextSplit = () =>
                        {
                            var currentSplit = splits[splitIndex];
                            if (splits.Length > splitIndex + 1)
                            {
                                splitIndex++;
                            }
                            int pos;
                            for (pos = current + 1; pos < reader.FieldCount; pos++)
                            {
                                // some people like ID some id ... assuming case insensitive splits for now
                                if (splitOn == "*" || string.Equals(reader.GetName(pos), currentSplit, StringComparison.InvariantCultureIgnoreCase))
                                {
                                    break;
                                }
                            }
                            current = pos;
                            return pos;
                        };

                        var otherDeserializer = new List<object>();

                        int split = nextSplit();
                        info.Deserializer = GetDeserializer<TFirst>(reader, 0, split, false);

                        if (typeof(TSecond) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TSecond>(reader, split, next - split, true));
                            split = next;
                        }
                        if (typeof(TThird) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TThird>(reader, split, next - split, true));
                            split = next;
                        }
                        if (typeof(TFourth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TFourth>(reader, split, next - split, true));
                            split = next;
                        }
                        if (typeof(TFifth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TFifth>(reader, split, next - split, true));
                        }
                        /// Added By Gokul Starts
                        if (typeof(TSixth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TSixth>(reader, split, next - split, true));
                        }
                        if (typeof(TSeventh) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TSeventh>(reader, split, next - split, true));
                        }
                        if (typeof(TEighth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TEighth>(reader, split, next - split, true));
                        }
                        if (typeof(TNinth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TNinth>(reader, split, next - split, true));
                        }
                        if (typeof(TTenth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TTenth>(reader, split, next - split, true));
                        }
                        if (typeof(TEleventh) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TEleventh>(reader, split, next - split, true));
                        }
                        if (typeof(TTwelfth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TTwelfth>(reader, split, next - split, true));
                        }
                        if (typeof(TThirteenth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TThirteenth>(reader, split, next - split, true));
                        }
                        if (typeof(TFourteenth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TFourteenth>(reader, split, next - split, true));
                        }
                        if (typeof(TFifteenth) != typeof(DontMap))
                        {
                            var next = nextSplit();
                            otherDeserializer.Add(GetDeserializer<TFifteenth>(reader, split, next - split, true));
                        }
                        /// Added By Gokul Ends
                        info.OtherDeserializers = otherDeserializer.ToArray();
                        SetQueryCache(identity, info);
                    }

                    var deserializer = (Func<IDataReader, TFirst>)info.Deserializer;
                    var deserializer2 = (Func<IDataReader, TSecond>)info.OtherDeserializers[0];


                    Func<IDataReader, TReturn> mapIt = null;

                    if (info.OtherDeserializers.Length == 1)
                    {
                        mapIt = r => ((Func<TFirst, TSecond, TReturn>)map)(deserializer(r), deserializer2(r));
                    }

                    if (info.OtherDeserializers.Length > 1)
                    {
                        var deserializer3 = (Func<IDataReader, TThird>)info.OtherDeserializers[1];

                        if (info.OtherDeserializers.Length == 2)
                        {
                            mapIt = r => ((Func<TFirst, TSecond, TThird, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r));
                        }
                        if (info.OtherDeserializers.Length > 2)
                        {
                            var deserializer4 = (Func<IDataReader, TFourth>)info.OtherDeserializers[2];
                            if (info.OtherDeserializers.Length == 3)
                            {
                                mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r));
                            }

                            if (info.OtherDeserializers.Length > 3)
                            {
#if CSHARP30
                                throw new NotSupportedException();
#else
                                var deserializer5 = (Func<IDataReader, TFifth>)info.OtherDeserializers[3];
                                /// Added By Gokul Starts
                                if (info.OtherDeserializers.Length == 4)
                                {
                                    mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r));
                                }
                                if (info.OtherDeserializers.Length > 4)
                                {
                                    var deserializer6 = (Func<IDataReader, TSixth>)info.OtherDeserializers[4];
                                    if (info.OtherDeserializers.Length == 5)
                                    {
                                        mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r));
                                    }
                                    if (info.OtherDeserializers.Length > 5)
                                    {
                                        var deserializer7 = (Func<IDataReader, TSeventh>)info.OtherDeserializers[5];
                                        if (info.OtherDeserializers.Length == 6)
                                        {
                                            mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r));
                                        }
                                        if (info.OtherDeserializers.Length > 6)
                                        {
                                            var deserializer8 = (Func<IDataReader, TEighth>)info.OtherDeserializers[6];
                                            if (info.OtherDeserializers.Length == 7)
                                            {
                                                mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r), deserializer8(r));
                                            }
                                            if (info.OtherDeserializers.Length > 7)
                                            {
                                                var deserializer9 = (Func<IDataReader, TNinth>)info.OtherDeserializers[7];
                                                if (info.OtherDeserializers.Length == 8)
                                                {
                                                    mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r), deserializer8(r), deserializer9(r));
                                                }
                                                if (info.OtherDeserializers.Length > 8)
                                                {
                                                    var deserializer10 = (Func<IDataReader, TTenth>)info.OtherDeserializers[8];
                                                    if (info.OtherDeserializers.Length == 9)
                                                    {
                                                        mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r), deserializer8(r), deserializer9(r), deserializer10(r));
                                                    }
                                                    if (info.OtherDeserializers.Length > 9)
                                                    {
                                                        var deserializer11 = (Func<IDataReader, TEleventh>)info.OtherDeserializers[9];
                                                        if (info.OtherDeserializers.Length == 10)
                                                        {
                                                            mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r), deserializer8(r), deserializer9(r), deserializer10(r), deserializer11(r));
                                                        }
                                                        if (info.OtherDeserializers.Length > 10)
                                                        {
                                                            var deserializer12 = (Func<IDataReader, TTwelfth>)info.OtherDeserializers[10];
                                                            if (info.OtherDeserializers.Length == 11)
                                                            {
                                                                mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r), deserializer8(r), deserializer9(r), deserializer10(r), deserializer11(r), deserializer12(r));
                                                            }
                                                            if (info.OtherDeserializers.Length > 11)
                                                            {
                                                                var deserializer13 = (Func<IDataReader, TThirteenth>)info.OtherDeserializers[11];
                                                                if (info.OtherDeserializers.Length == 12)
                                                                {
                                                                    mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r), deserializer8(r), deserializer9(r), deserializer10(r), deserializer11(r), deserializer12(r), deserializer13(r));
                                                                }
                                                                if (info.OtherDeserializers.Length > 12)
                                                                {
                                                                    var deserializer14 = (Func<IDataReader, TFourteenth>)info.OtherDeserializers[12];
                                                                    if (info.OtherDeserializers.Length == 13)
                                                                    {
                                                                        mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r), deserializer8(r), deserializer9(r), deserializer10(r), deserializer11(r), deserializer12(r), deserializer13(r), deserializer14(r));
                                                                    }
                                                                    if (info.OtherDeserializers.Length > 13)
                                                                    {
                                                                        var deserializer15 = (Func<IDataReader, TFifteenth>)info.OtherDeserializers[13];
                                                                        if (info.OtherDeserializers.Length == 14)
                                                                        {
                                                                            mapIt = r => ((Func<TFirst, TSecond, TThird, TFourth, TFifth, TSixth, TSeventh, TEighth, TNinth, TTenth, TEleventh, TTwelfth, TThirteenth, TFourteenth, TFifteenth, TReturn>)map)(deserializer(r), deserializer2(r), deserializer3(r), deserializer4(r), deserializer5(r), deserializer6(r), deserializer7(r), deserializer8(r), deserializer9(r), deserializer10(r), deserializer11(r), deserializer12(r), deserializer13(r), deserializer14(r), deserializer15(r));
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                /// Added By Gokul Ends
#endif
                            }
                        }
                    }

                    if (mapIt != null)
                        while (reader.Read())
                        {
                            yield return mapIt(reader);
                        }
                }
            }
        }

        private static CacheInfo GetCacheInfo(Identity identity)
        {
            CacheInfo info;
            if (!TryGetQueryCache(identity, out info))
            {
                info = new CacheInfo();
                if (identity.parametersType != null)
                {
                    if (typeof(IDynamicParameters).IsAssignableFrom(identity.parametersType))
                    {
                        info.ParamReader = (cmd, obj) => { (obj as IDynamicParameters).AddParameter(cmd); };
                    }
                    else
                    {
                        info.ParamReader = CreateParamInfoGenerator(identity.parametersType);
                    }
                }
                SetQueryCache(identity, info);
            }
            return info;
        }

        private static Func<IDataReader, T> GetDeserializer<T>(IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing)
        {
#if !CSHARP30
            // dynamic is passed in as Object ... by c# design
            if (typeof(T) == typeof(object)
                || typeof(T) == typeof(FastExpando))
            {
                return GetDynamicDeserializer<T>(reader, startBound, length, returnNullIfFirstMissing);
            }
#endif
            if (typeof(T).IsClass && typeof(T) != typeof(string))
            {
                return GetClassDeserializer<T>(reader, startBound, length, returnNullIfFirstMissing);
            }
            return GetStructDeserializer<T>(startBound);

        }
#if !CSHARP30
        private class FastExpando : System.Dynamic.DynamicObject, IDictionary<string, object>
        {
            IDictionary<string, object> data;

            public static FastExpando Attach(IDictionary<string, object> data)
            {
                return new FastExpando { data = data };
            }

            public override bool TrySetMember(System.Dynamic.SetMemberBinder binder, object value)
            {
                data[binder.Name] = value;
                return true;
            }

            public override bool TryGetMember(System.Dynamic.GetMemberBinder binder, out object result)
            {
                return data.TryGetValue(binder.Name, out result);
            }

            #region IDictionary<string,object> Members

            void IDictionary<string, object>.Add(string key, object value)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<string, object>.ContainsKey(string key)
            {
                return data.ContainsKey(key);
            }

            ICollection<string> IDictionary<string, object>.Keys
            {
                get { return data.Keys; }
            }

            bool IDictionary<string, object>.Remove(string key)
            {
                throw new NotImplementedException();
            }

            bool IDictionary<string, object>.TryGetValue(string key, out object value)
            {
                return data.TryGetValue(key, out value);
            }

            ICollection<object> IDictionary<string, object>.Values
            {
                get { return data.Values; }
            }

            object IDictionary<string, object>.this[string key]
            {
                get
                {
                    return data[key];
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            #endregion

            #region ICollection<KeyValuePair<string,object>> Members

            void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            void ICollection<KeyValuePair<string, object>>.Clear()
            {
                throw new NotImplementedException();
            }

            bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
            {
                return data.Contains(item);
            }

            void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
            {
                data.CopyTo(array, arrayIndex);
            }

            int ICollection<KeyValuePair<string, object>>.Count
            {
                get { return data.Count; }
            }

            bool ICollection<KeyValuePair<string, object>>.IsReadOnly
            {
                get { return true; }
            }

            bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
            {
                throw new NotImplementedException();
            }

            #endregion

            #region IEnumerable<KeyValuePair<string,object>> Members

            IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
            {
                return data.GetEnumerator();
            }

            #endregion

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return data.GetEnumerator();
            }

            #endregion
        }


        private static Func<IDataReader, T> GetDynamicDeserializer<T>(IDataRecord reader, int startBound, int length, bool returnNullIfFirstMissing)
        {
            if (length == -1)
            {
                length = reader.FieldCount - startBound;
            }

            return
                 r =>
                 {
                     IDictionary<string, object> row = new Dictionary<string, object>(length);
                     for (var i = startBound; i < startBound + length; i++)
                     {
                         var tmp = r.GetValue(i);
                         tmp = tmp == DBNull.Value ? null : tmp;
                         row[r.GetName(i)] = tmp;
                         if (returnNullIfFirstMissing && i == startBound && tmp == null)
                         {
                             return default(T);
                         }
                     }
                     //we know this is an object so it will not box
                     return (T)(object)FastExpando.Attach(row);
                 };
        }
#endif
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        [Obsolete("This method is for internal usage only", true)]
        public static void PackListParameters(IDbCommand command, string namePrefix, object value)
        {
            // initially we tried TVP, however it performs quite poorly.
            // keep in mind SQL support up to 2000 params easily in sp_executesql, needing more is rare

            var list = value as IEnumerable;
            var count = 0;

            if (list != null)
            {
                bool isString = value is IEnumerable<string>;
                foreach (var item in list)
                {
                    count++;
                    var listParam = command.CreateParameter();
                    listParam.ParameterName = namePrefix + count;
                    listParam.Value = item ?? DBNull.Value;
                    if (isString)
                    {
                        listParam.Size = 4000;
                        if (item != null && ((string)item).Length > 4000)
                        {
                            listParam.Size = -1;
                        }
                    }
                    command.Parameters.Add(listParam);
                }

                if (count == 0)
                {
                    command.CommandText = command.CommandText.Replace(namePrefix, "(SELECT NULL WHERE 1 = 0)");
                }
                else
                {
                    var sb = new StringBuilder("(").Append(namePrefix).Append(1);
                    for (int i = 2; i <= count; i++)
                    {
                        sb.Append(',').Append(namePrefix).Append(i);
                    }
                    string inQuery = sb.Append(')').ToString();
                    command.CommandText = command.CommandText.Replace(namePrefix, inQuery);
                }
            }

        }


        private static Action<IDbCommand, object> CreateParamInfoGenerator(Type type)
        {
            var dm = new DynamicMethod(string.Format("ParamInfo{0}", Guid.NewGuid()), null, new[] { typeof(IDbCommand), typeof(object) }, type, true);

            var il = dm.GetILGenerator();

            il.DeclareLocal(type); // 0
            bool haveInt32Arg1 = false;
            il.Emit(OpCodes.Ldarg_1); // stack is now [untyped-param]
            il.Emit(OpCodes.Unbox_Any, type); // stack is now [typed-param]
            il.Emit(OpCodes.Stloc_0);// stack is now empty

            il.Emit(OpCodes.Ldarg_0); // stack is now [command]
            il.EmitCall(OpCodes.Callvirt, typeof(IDbCommand).GetProperty("Parameters").GetGetMethod(), null); // stack is now [parameters]


            foreach (var prop in type.GetProperties().OrderBy(p => p.Name))
            {
                if (prop.PropertyType == typeof(DbString))
                {
                    il.Emit(OpCodes.Ldloc_0); // stack is now [parameters] [typed-param]
                    il.Emit(OpCodes.Callvirt, prop.GetGetMethod()); // stack is [parameters] [dbstring]
                    il.Emit(OpCodes.Ldarg_0); // stack is now [parameters] [dbstring] [command]
                    il.Emit(OpCodes.Ldstr, "@" + prop.Name); // stack is now [parameters] [dbstring] [command] [name]
                    il.EmitCall(OpCodes.Callvirt, typeof(DbString).GetMethod("AddParameter"), null); // stack is now [parameters]
                    continue;
                }
                DbType dbType = LookupDbType(prop.PropertyType);
                if (dbType == DbType.Xml)
                {
                    // this actually represents special handling for list types;
                    il.Emit(OpCodes.Ldarg_0); // stack is now [parameters] [command]
                    il.Emit(OpCodes.Ldstr, "@" + prop.Name); // stack is now [parameters] [command] [name]
                    il.Emit(OpCodes.Ldloc_0); // stack is now [parameters] [command] [name] [typed-param]
                    il.Emit(OpCodes.Callvirt, prop.GetGetMethod()); // stack is [parameters] [command] [name] [typed-value]
                    if (prop.PropertyType.IsValueType)
                    {
                        il.Emit(OpCodes.Box, prop.PropertyType); // stack is [parameters] [command] [name] [boxed-value]
                    }
                    il.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("PackListParameters"), null); // stack is [parameters]
                    continue;
                }
                il.Emit(OpCodes.Dup); // stack is now [parameters] [parameters]

                il.Emit(OpCodes.Ldarg_0); // stack is now [parameters] [parameters] [command]
                il.EmitCall(OpCodes.Callvirt, typeof(IDbCommand).GetMethod("CreateParameter"), null);// stack is now [parameters] [parameters] [parameter]

                il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                il.Emit(OpCodes.Ldstr, "@" + prop.Name); // stack is now [parameters] [parameters] [parameter] [parameter] [name]
                il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("ParameterName").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                EmitInt32(il, (int)dbType);// stack is now [parameters] [parameters] [parameter] [parameter] [db-type]

                il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("DbType").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                EmitInt32(il, (int)ParameterDirection.Input);// stack is now [parameters] [parameters] [parameter] [parameter] [dir]
                il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("Direction").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                il.Emit(OpCodes.Ldloc_0); // stack is now [parameters] [parameters] [parameter] [parameter] [typed-param]
                il.Emit(OpCodes.Callvirt, prop.GetGetMethod()); // stack is [parameters] [parameters] [parameter] [parameter] [typed-value]
                bool checkForNull = true;
                if (prop.PropertyType.IsValueType)
                {
                    il.Emit(OpCodes.Box, prop.PropertyType); // stack is [parameters] [parameters] [parameter] [parameter] [boxed-value]
                    if (Nullable.GetUnderlyingType(prop.PropertyType) == null)
                    {   // struct but not Nullable<T>; boxed value cannot be null
                        checkForNull = false;
                    }
                }
                if (checkForNull)
                {
                    if (dbType == DbType.String && !haveInt32Arg1)
                    {
                        il.DeclareLocal(typeof(int));
                        haveInt32Arg1 = true;
                    }
                    // relative stack: [boxed value]
                    il.Emit(OpCodes.Dup);// relative stack: [boxed value] [boxed value]
                    Label notNull = il.DefineLabel();
                    Label? allDone = dbType == DbType.String ? il.DefineLabel() : (Label?)null;
                    il.Emit(OpCodes.Brtrue_S, notNull);
                    // relative stack [boxed value = null]
                    il.Emit(OpCodes.Pop); // relative stack empty
                    il.Emit(OpCodes.Ldsfld, typeof(DBNull).GetField("Value")); // relative stack [DBNull]
                    if (dbType == DbType.String)
                    {
                        EmitInt32(il, 0);
                        il.Emit(OpCodes.Stloc_1);
                    }
                    if (allDone != null) il.Emit(OpCodes.Br_S, allDone.Value);
                    il.MarkLabel(notNull);
                    if (prop.PropertyType == typeof(string))
                    {
                        il.Emit(OpCodes.Dup); // [string] [string]
                        il.EmitCall(OpCodes.Callvirt, typeof(string).GetProperty("Length").GetGetMethod(), null); // [string] [length]
                        EmitInt32(il, 4000); // [string] [length] [4000]
                        il.Emit(OpCodes.Cgt); // [string] [0 or 1]
                        Label isLong = il.DefineLabel(), lenDone = il.DefineLabel();
                        il.Emit(OpCodes.Brtrue_S, isLong);
                        EmitInt32(il, 4000); // [string] [4000]
                        il.Emit(OpCodes.Br_S, lenDone);
                        il.MarkLabel(isLong);
                        EmitInt32(il, -1); // [string] [-1]
                        il.MarkLabel(lenDone);
                        il.Emit(OpCodes.Stloc_1); // [string] 
                    }
                    if (allDone != null) il.MarkLabel(allDone.Value);
                    // relative stack [boxed value or DBNull]
                }
                il.EmitCall(OpCodes.Callvirt, typeof(IDataParameter).GetProperty("Value").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                if (prop.PropertyType == typeof(string))
                {
                    var endOfSize = il.DefineLabel();
                    // don't set if 0
                    il.Emit(OpCodes.Ldloc_1); // [parameters] [parameters] [parameter] [size]
                    il.Emit(OpCodes.Brfalse_S, endOfSize); // [parameters] [parameters] [parameter]

                    il.Emit(OpCodes.Dup);// stack is now [parameters] [parameters] [parameter] [parameter]
                    il.Emit(OpCodes.Ldloc_1); // stack is now [parameters] [parameters] [parameter] [parameter] [size]
                    il.EmitCall(OpCodes.Callvirt, typeof(IDbDataParameter).GetProperty("Size").GetSetMethod(), null);// stack is now [parameters] [parameters] [parameter]

                    il.MarkLabel(endOfSize);
                }

                il.EmitCall(OpCodes.Callvirt, typeof(IList).GetMethod("Add"), null); // stack is now [parameters]
                il.Emit(OpCodes.Pop); // IList.Add returns the new index (int); we don't care
            }
            // stack is currently [command]
            il.Emit(OpCodes.Pop); // stack is now empty
            il.Emit(OpCodes.Ret);
            return (Action<IDbCommand, object>)dm.CreateDelegate(typeof(Action<IDbCommand, object>));
        }

        private static IDbCommand SetupCommand(IDbConnection cnn, IDbTransaction transaction, string sql, Action<IDbCommand, object> paramReader, object obj, int? commandTimeout, CommandType? commandType)
        {
            var cmd = cnn.CreateCommand();

            cmd.Transaction = transaction;
            cmd.CommandText = sql;
            if (commandTimeout.HasValue)
                cmd.CommandTimeout = commandTimeout.Value;
            if (commandType.HasValue)
                cmd.CommandType = commandType.Value;
            if (paramReader != null)
            {
                paramReader(cmd, obj);
            }
            return cmd;
        }


        private static int ExecuteCommand(IDbConnection cnn, IDbTransaction tranaction, string sql, Action<IDbCommand, object> paramReader, object obj, int? commandTimeout, CommandType? commandType)
        {
            using (var cmd = SetupCommand(cnn, tranaction, sql, paramReader, obj, commandTimeout, commandType))
            {
                return cmd.ExecuteNonQuery();
            }
        }

        private static Func<IDataReader, T> GetStructDeserializer<T>(int index)
        {
            return r =>
            {
                var val = r.GetValue(index);
                if (val == DBNull.Value)
                {
                    val = null;
                }
                return (T)val;
            };
        }
        static readonly MethodInfo
                    enumParse = typeof(Enum).GetMethod("Parse", new Type[] { typeof(Type), typeof(string), typeof(bool) }),
                    getItem = typeof(IDataRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                        .Where(p => p.GetIndexParameters().Any() && p.GetIndexParameters()[0].ParameterType == typeof(int))
                        .Select(p => p.GetGetMethod()).First();

        public static Func<IDataReader, T> GetClassDeserializer<T>(
#if CSHARP30
            IDataReader reader, int startBound, int length, bool returnNullIfFirstMissing
#else
IDataReader reader, int startBound = 0, int length = -1, bool returnNullIfFirstMissing = false
#endif
)
        {

            var dm = new DynamicMethod(string.Format("Deserialize{0}", Guid.NewGuid()), typeof(T), new[] { typeof(IDataReader) }, true);

            var il = dm.GetILGenerator();
            il.DeclareLocal(typeof(int));
            il.DeclareLocal(typeof(T));
            bool haveEnumLocal = false;
            il.Emit(OpCodes.Ldc_I4_0);
            il.Emit(OpCodes.Stloc_0);
            var properties = typeof(T)
                .GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Select(p => new
                {
                    p.Name,
                    Setter = p.DeclaringType == typeof(T) ? p.GetSetMethod(true) : p.DeclaringType.GetProperty(p.Name).GetSetMethod(true),
                    Type = p.PropertyType
                })
                .Where(info => info.Setter != null)
                .ToList();
            var fields = typeof(T).GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if (length == -1)
            {
                length = reader.FieldCount - startBound;
            }

            var names = new List<string>();
            for (int i = startBound; i < startBound + length; i++)
            {
                names.Add(reader.GetName(i));
            }

            var setters = (
                            from n in names
                            let prop = properties.FirstOrDefault(p => string.Equals(p.Name, n, StringComparison.InvariantCulture)) // property case sensitive first
                                  ?? properties.FirstOrDefault(p => string.Equals(p.Name, n, StringComparison.InvariantCultureIgnoreCase)) // property case insensitive second
                            let field = prop != null ? null : (fields.FirstOrDefault(p => string.Equals(p.Name, n, StringComparison.InvariantCulture)) // field case sensitive third
                                ?? fields.FirstOrDefault(p => string.Equals(p.Name, n, StringComparison.InvariantCultureIgnoreCase))) // field case insensitive fourth
                            select new { Name = n, Property = prop, Field = field }
                          ).ToList();

            int index = startBound;

            il.BeginExceptionBlock();
            // stack is empty
            il.Emit(OpCodes.Newobj, typeof(T).GetConstructor(Type.EmptyTypes)); // stack is now [target]
            bool first = true;
            var @allDone = il.DefineLabel();
            foreach (var item in setters)
            {
                if (item.Property != null || item.Field != null)
                {
                    il.Emit(OpCodes.Dup); // stack is now [target][target]
                    Label isDbNullLabel = il.DefineLabel();
                    Label finishLabel = il.DefineLabel();

                    il.Emit(OpCodes.Ldarg_0); // stack is now [target][target][reader]
                    EmitInt32(il, index); // stack is now [target][target][reader][index]
                    il.Emit(OpCodes.Dup);// stack is now [target][target][reader][index][index]
                    il.Emit(OpCodes.Stloc_0);// stack is now [target][target][reader][index]
                    il.Emit(OpCodes.Callvirt, getItem); // stack is now [target][target][value-as-object]


                    il.Emit(OpCodes.Dup); // stack is now [target][target][value][value]
                    il.Emit(OpCodes.Isinst, typeof(DBNull)); // stack is now [target][target][value-as-object][DBNull or null]
                    il.Emit(OpCodes.Brtrue_S, isDbNullLabel); // stack is now [target][target][value-as-object]

                    // unbox nullable enums as the primitive, i.e. byte etc
                    Type memberType = item.Property != null ? item.Property.Type : item.Field.FieldType;
                    var nullUnderlyingType = Nullable.GetUnderlyingType(memberType);
                    var unboxType = nullUnderlyingType != null && nullUnderlyingType.IsEnum ? nullUnderlyingType : memberType;

                    if (unboxType.IsEnum)
                    {
                        if (!haveEnumLocal)
                        {
                            il.DeclareLocal(typeof(string));
                            haveEnumLocal = true;
                        }

                        Label isNotString = il.DefineLabel();
                        il.Emit(OpCodes.Dup); // stack is now [target][target][value][value]
                        il.Emit(OpCodes.Isinst, typeof(string)); // stack is now [target][target][value-as-object][string or null]
                        il.Emit(OpCodes.Dup);// stack is now [target][target][value-as-object][string or null][string or null]
                        il.Emit(OpCodes.Stloc_2); // stack is now [target][target][value-as-object][string or null]
                        il.Emit(OpCodes.Brfalse_S, isNotString); // stack is now [target][target][value-as-object]

                        il.Emit(OpCodes.Pop); // stack is now [target][target]


                        il.Emit(OpCodes.Ldtoken, unboxType); // stack is now [target][target][enum-type-token]
                        il.EmitCall(OpCodes.Call, typeof(Type).GetMethod("GetTypeFromHandle"), null);// stack is now [target][target][enum-type]
                        il.Emit(OpCodes.Ldloc_2); // stack is now [target][target][enum-type][string]
                        il.Emit(OpCodes.Ldc_I4_1); // stack is now [target][target][enum-type][string][true]
                        il.EmitCall(OpCodes.Call, enumParse, null); // stack is now [target][target][enum-as-object]

                        il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]

                        if (nullUnderlyingType != null)
                        {
                            il.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType }));
                        }
                        if (item.Property != null)
                        {
                            il.Emit(OpCodes.Callvirt, item.Property.Setter); // stack is now [target]
                        }
                        else
                        {
                            il.Emit(OpCodes.Stfld, item.Field); // stack is now [target]
                        }
                        il.Emit(OpCodes.Br_S, finishLabel);


                        il.MarkLabel(isNotString);
                    }
                    il.Emit(OpCodes.Unbox_Any, unboxType); // stack is now [target][target][typed-value]
                    if (nullUnderlyingType != null && nullUnderlyingType.IsEnum)
                    {
                        il.Emit(OpCodes.Newobj, memberType.GetConstructor(new[] { nullUnderlyingType }));
                    }
                    if (item.Property != null)
                    {
                        il.Emit(OpCodes.Callvirt, item.Property.Setter); // stack is now [target]
                    }
                    else
                    {
                        il.Emit(OpCodes.Stfld, item.Field); // stack is now [target]
                    }
                    il.Emit(OpCodes.Br_S, finishLabel); // stack is now [target]

                    il.MarkLabel(isDbNullLabel); // incoming stack: [target][target][value]

                    il.Emit(OpCodes.Pop); // stack is now [target][target]
                    il.Emit(OpCodes.Pop); // stack is now [target]

                    if (first && returnNullIfFirstMissing)
                    {
                        il.Emit(OpCodes.Pop);
                        il.Emit(OpCodes.Ldnull); // stack is now [null]
                        il.Emit(OpCodes.Stloc_1);
                        il.Emit(OpCodes.Br, @allDone);
                    }

                    il.MarkLabel(finishLabel);
                }
                first = false;
                index += 1;
            }
            il.Emit(OpCodes.Stloc_1); // stack is empty
            il.MarkLabel(@allDone);
            il.BeginCatchBlock(typeof(Exception)); // stack is Exception
            il.Emit(OpCodes.Ldloc_0); // stack is Exception, index
            il.Emit(OpCodes.Ldarg_0); // stack is Exception, index, reader
            il.EmitCall(OpCodes.Call, typeof(SqlMapper).GetMethod("ThrowDataException"), null);
            il.Emit(OpCodes.Ldnull);
            il.Emit(OpCodes.Stloc_1); // to make it verifiable
            il.EndExceptionBlock();

            il.Emit(OpCodes.Ldloc_1); // stack is empty
            il.Emit(OpCodes.Ret);

            return (Func<IDataReader, T>)dm.CreateDelegate(typeof(Func<IDataReader, T>));
        }
        public static void ThrowDataException(Exception ex, int index, IDataReader reader)
        {
            string name = "(n/a)", value = "(n/a)";
            if (reader != null && index >= 0 && index < reader.FieldCount)
            {
                name = reader.GetName(index);
                object val = reader.GetValue(index);
                if (val == null || val is DBNull)
                {
                    value = "<null>";
                }
                else
                {
                    value = Convert.ToString(val) + " - " + Type.GetTypeCode(val.GetType());
                }
            }
            throw new DataException(string.Format("Error parsing column {0} ({1}={2})", index, name, value), ex);
        }
        private static void EmitInt32(ILGenerator il, int value)
        {
            switch (value)
            {
                case -1: il.Emit(OpCodes.Ldc_I4_M1); break;
                case 0: il.Emit(OpCodes.Ldc_I4_0); break;
                case 1: il.Emit(OpCodes.Ldc_I4_1); break;
                case 2: il.Emit(OpCodes.Ldc_I4_2); break;
                case 3: il.Emit(OpCodes.Ldc_I4_3); break;
                case 4: il.Emit(OpCodes.Ldc_I4_4); break;
                case 5: il.Emit(OpCodes.Ldc_I4_5); break;
                case 6: il.Emit(OpCodes.Ldc_I4_6); break;
                case 7: il.Emit(OpCodes.Ldc_I4_7); break;
                case 8: il.Emit(OpCodes.Ldc_I4_8); break;
                default:
                    if (value >= -128 && value <= 127)
                    {
                        il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
                    }
                    else
                    {
                        il.Emit(OpCodes.Ldc_I4, value);
                    }
                    break;
            }
        }

        public class GridReader : IDisposable
        {
            private IDataReader reader;
            private IDbCommand command;
            internal GridReader(IDbCommand command, IDataReader reader)
            {
                this.command = command;
                this.reader = reader;
            }
            /// <summary>
            /// Read the next grid of results
            /// </summary>
            public IEnumerable<T> Read<T>()
            {
                if (reader == null) throw new ObjectDisposedException(GetType().Name);
                if (consumed) throw new InvalidOperationException("Each grid can only be iterated once");
                var deserializer = GetDeserializer<T>(reader, 0, -1, false);
                consumed = true;
                return ReadDeferred(gridIndex, deserializer);
            }

            // todo multimapping. 

            private IEnumerable<T> ReadDeferred<T>(int index, Func<IDataReader, T> deserializer)
            {
                try
                {
                    while (index == gridIndex && reader.Read())
                    {
                        yield return deserializer(reader);
                    }
                }
                finally // finally so that First etc progresses things even when multiple rows
                {
                    if (index == gridIndex)
                    {
                        NextResult();
                    }
                }
            }
            private int gridIndex;
            private bool consumed;
            private void NextResult()
            {
                if (reader.NextResult())
                {
                    gridIndex++;
                    consumed = false;
                }
                else
                {
                    Dispose();
                }

            }
            public void Dispose()
            {
                if (reader != null)
                {
                    reader.Dispose();
                    reader = null;
                }
                if (command != null)
                {
                    command.Dispose();
                    command = null;
                }
            }
        }
    }

    public class DynamicParameters : SqlMapper.IDynamicParameters
    {
        Dictionary<string, ParamInfo> parameters = new Dictionary<string, ParamInfo>();

        class ParamInfo
        {
            public string Name { get; set; }
            public object Value { get; set; }
            public ParameterDirection ParameterDirection { get; set; }
            public DbType? DbType { get; set; }
            public int? Size { get; set; }
            public IDbDataParameter AttachedParam { get; set; }
        }

        public DynamicParameters() { }
        public DynamicParameters(object template)
        {
            const BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance;
            if (template != null)
            {
                foreach (PropertyInfo prop in template.GetType().GetProperties(bindingFlags))
                {
                    if (!prop.CanRead) continue;
                    var idx = prop.GetIndexParameters();
                    if (idx != null && idx.Length != 0) continue;
                    Add("@" + prop.Name, prop.GetValue(template, null), null, ParameterDirection.Input, null);
                }
                foreach (FieldInfo field in template.GetType().GetFields(bindingFlags))
                {
                    Add("@" + field.Name, field.GetValue(template), null, ParameterDirection.Input, null);
                }

            }
        }


        public void Add(
#if CSHARP30
            string name, object value, DbType? dbType, ParameterDirection? direction, int? size
#else
string name, object value = null, DbType? dbType = null, ParameterDirection? direction = null, int? size = null
#endif
)
        {
            parameters[name] = new ParamInfo() { Name = name, Value = value, ParameterDirection = direction ?? ParameterDirection.Input, DbType = dbType, Size = size };
        }


        void SqlMapper.IDynamicParameters.AddParameter(IDbCommand command)
        {
            foreach (var param in parameters.Values)
            {
                var p = command.CreateParameter();
                var val = param.Value;
                p.ParameterName = param.Name;
                p.Value = val ?? DBNull.Value;
                p.Direction = param.ParameterDirection;
                var s = val as string;
                if (s != null)
                {
                    if (s.Length <= 4000)
                    {
                        p.Size = 4000;
                    }
                }
                if (param.Size != null)
                {
                    p.Size = param.Size.Value;
                }
                if (param.DbType != null)
                {
                    p.DbType = param.DbType.Value;
                }
                command.Parameters.Add(p);
                param.AttachedParam = p;
            }
        }

        public T Get<T>(string name)
        {
            if (parameters[name].AttachedParam != null)
                return (T)parameters[name].AttachedParam.Value;
            else
                return default(T);
        }
    }

    public sealed class DbString
    {
        public DbString() { Length = -1; }
        public bool IsAnsi { get; set; }
        public bool IsFixedLength { get; set; }
        public int Length { get; set; }
        public string Value { get; set; }
        public void AddParameter(IDbCommand command, string name)
        {
            if (IsFixedLength && Length == -1)
            {
                throw new InvalidOperationException("If specifying IsFixedLength,  a Length must also be specified");
            }
            var param = command.CreateParameter();
            param.ParameterName = name;
            param.Value = (object)Value ?? DBNull.Value;
            if (Length == -1 && Value != null && Value.Length <= 4000)
            {
                param.Size = 4000;
            }
            else
            {
                param.Size = Length;
            }
            param.DbType = IsAnsi ? (IsFixedLength ? DbType.AnsiStringFixedLength : DbType.AnsiString) : (IsFixedLength ? DbType.StringFixedLength : DbType.String);
            command.Parameters.Add(param);
        }
    }

    public class Sql
    {
        public Sql()
        {
        }

        public Sql(string sql, params object[] args)
        {
            _sql = sql;
            _args = args;
        }

        public static Sql Builder
        {
            get { return new Sql(); }
        }

        string _sql;
        object[] _args;
        Sql _rhs;
        string _sqlFinal;
        object[] _argsFinal;

        private void Build()
        {
            // already built?
            if (_sqlFinal != null)
                return;

            // Build it
            var sb = new StringBuilder();
            var args = new List<object>();
            Build(sb, args, null);
            _sqlFinal = sb.ToString();
            _argsFinal = args.ToArray();
        }

        public string SQL
        {
            get
            {
                Build();
                return _sqlFinal;
            }
        }

        public object[] Arguments
        {
            get
            {
                Build();
                return _argsFinal;
            }
        }

        public Sql Append(Sql sql)
        {
            if (_rhs != null)
                _rhs.Append(sql);
            else
                _rhs = sql;

            return this;
        }

        public Sql Append(string sql, params object[] args)
        {
            return Append(new Sql(sql, args));
        }

        static bool Is(Sql sql, string sqltype)
        {
            return sql != null && sql._sql != null && sql._sql.StartsWith(sqltype, StringComparison.InvariantCultureIgnoreCase);
        }

        private void Build(StringBuilder sb, List<object> args, Sql lhs)
        {
            if (!String.IsNullOrEmpty(_sql))
            {
                // Add SQL to the string
                if (sb.Length > 0)
                {
                    sb.Append("\n");
                }

                var sql = _sql;  // ProcessParams(_sql, _args, args);

                if (Is(lhs, "WHERE ") && Is(this, "WHERE "))
                    sql = "AND " + sql.Substring(6);
                if (Is(lhs, "ORDER BY ") && Is(this, "ORDER BY "))
                    sql = ", " + sql.Substring(9);

                sb.Append(sql);
            }

            // Now do rhs
            if (_rhs != null)
                _rhs.Build(sb, args, this);
        }

        public Sql Where(string sql, params object[] args)
        {
            return Append(new Sql("WHERE (" + sql + ")", args));
        }

        public Sql OrderBy(params object[] columns)
        {
            return Append(new Sql("ORDER BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public Sql Select(params object[] columns)
        {
            return Append(new Sql("SELECT " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        public Sql From(params object[] tables)
        {
            return Append(new Sql("FROM " + String.Join(", ", (from x in tables select x.ToString()).ToArray())));
        }

        public Sql GroupBy(params object[] columns)
        {
            return Append(new Sql("GROUP BY " + String.Join(", ", (from x in columns select x.ToString()).ToArray())));
        }

        private SqlJoinClause Join(string JoinType, string table)
        {
            return new SqlJoinClause(Append(new Sql(JoinType + table)));
        }

        public SqlJoinClause InnerJoin(string table) { return Join("INNER JOIN ", table); }
        public SqlJoinClause LeftJoin(string table) { return Join("LEFT JOIN ", table); }

        public class SqlJoinClause
        {
            private readonly Sql _sql;

            public SqlJoinClause(Sql sql)
            {
                _sql = sql;
            }

            public Sql On(string onClause, params object[] args)
            {
                return _sql.Append("ON " + onClause, args);
            }
        }

        // Helper to handle named parameters from object properties
        static Regex rxParams = new Regex(@"(?<!@)@\w+", RegexOptions.Compiled);
        public static string ProcessParams(string _sql, object[] args_src, List<object> args_dest)
        {
            return rxParams.Replace(_sql, m =>
            {
                string param = m.Value.Substring(1);

                object arg_val;

                int paramIndex;
                if (int.TryParse(param, out paramIndex))
                {
                    // Numbered parameter
                    if (paramIndex < 0 || paramIndex >= args_src.Length)
                        throw new ArgumentOutOfRangeException(string.Format("Parameter '@{0}' specified but only {1} parameters supplied (in `{2}`)", paramIndex, args_src.Length, _sql));
                    arg_val = args_src[paramIndex];
                }
                else
                {
                    // Look for a property on one of the arguments with this name
                    bool found = false;
                    arg_val = null;
                    foreach (var o in args_src)
                    {
                        var pi = o.GetType().GetProperty(param);
                        if (pi != null)
                        {
                            arg_val = pi.GetValue(o, null);
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        throw new ArgumentException(string.Format("Parameter '@{0}' specified but none of the passed arguments have a property with this name (in '{1}')", param, _sql));
                }

                // Expand collections to parameter lists
                if ((arg_val as System.Collections.IEnumerable) != null &&
                    (arg_val as string) == null &&
                    (arg_val as byte[]) == null)
                {
                    var sb = new StringBuilder();
                    foreach (var i in arg_val as System.Collections.IEnumerable)
                    {
                        sb.Append((sb.Length == 0 ? "@" : ",@") + args_dest.Count.ToString());
                        args_dest.Add(i);
                    }
                    return sb.ToString();
                }
                else
                {
                    args_dest.Add(arg_val);
                    return "@" + (args_dest.Count - 1).ToString();
                }
            }
            );
        }
    }
}
