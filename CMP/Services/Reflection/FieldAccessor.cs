using System;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;

namespace CMP.Services.Reflection
{
    public static class CompiledFieldAccessor<T>
    {
        // 缓存单个字段的getter
        private static readonly ConcurrentDictionary<string, Func<T, object>> _fieldGetterCache = new();

        // 缓存所有字段的getter数组
        private static readonly Lazy<Func<T, object>[]> _allFieldGetters;

        static CompiledFieldAccessor()
        {
            _allFieldGetters = new Lazy<Func<T, object>[]>(() =>
            {
                var fields = typeof(T).GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                var getters = new Func<T, object>[fields.Length];

                for (int i = 0; i < fields.Length; i++)
                {
                    getters[i] = CreateFieldGetter(fields[i]);
                }

                return getters;
            }, System.Threading.LazyThreadSafetyMode.ExecutionAndPublication);
        }

        /// <summary>
        /// 获取指定字段的值 - 热路径优化
        /// </summary>
        //[MethodImpl(MethodImplOptions.AggressiveInlining)]
        //public static object GetFieldValue(T instance, string fieldName)
        //{
        //    if (instance == null) return null;

        //    var getter = _fieldGetterCache.GetOrAdd(fieldName, name =>
        //    {
        //        var field = typeof(T).GetField(name,
        //            BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //        return field != null ? CreateFieldGetter(field) : null;
        //    });

        //    return getter?.Invoke(instance);
        //}

        /// <summary>
        /// 遍历所有字段并获取值 - 最优性能
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void ProcessAllFields(T instance, Action<string, object> processor)
        {
            if (instance == null) return;

            var getters = _allFieldGetters.Value;
            var fields = typeof(T).GetFields(
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            for (int i = 0; i < getters.Length; i++)
            {
                var value = getters[i](instance);
                processor(fields[i].Name, value);
            }
        }

        /// <summary>
        /// 获取所有字段的值数组
        /// </summary>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static object[] GetAllFieldValues(T instance)
        {
            if (instance == null) return Array.Empty<object>();

            var getters = _allFieldGetters.Value;
            var values = new object[getters.Length];

            for (int i = 0; i < getters.Length; i++)
            {
                values[i] = getters[i](instance);
            }

            return values;
        }

        /// <summary>
        /// 创建字段getter委托
        /// </summary>
        private static Func<T, object> CreateFieldGetter(FieldInfo field)
        {
            var instanceParam = Expression.Parameter(typeof(T), "instance");
            var fieldAccess = Expression.Field(instanceParam, field);

            // 值类型需要装箱，引用类型直接返回
            Expression body = field.FieldType.IsValueType
                ? Expression.Convert(fieldAccess, typeof(object))  // 装箱
                : fieldAccess;

            return Expression.Lambda<Func<T, object>>(body, instanceParam).Compile();
        }
    }

    /// <summary>
    /// 通用版本（非泛型）- 适用于运行时类型不确定的情况
    /// </summary>
    public static class CompiledFieldAccessor
    {
        private static readonly ConcurrentDictionary<Type, FieldAccessorCache> _typeCache = new();

        private class FieldAccessorCache
        {
            public Func<object, object>[] FieldGetters { get; }
            public string[] FieldNames { get; }

            public FieldAccessorCache(Type type)
            {
                var fields = type.GetFields(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

                FieldGetters = new Func<object, object>[fields.Length];
                FieldNames = new string[fields.Length];

                for (int i = 0; i < fields.Length; i++)
                {
                    FieldGetters[i] = CreateFieldGetter(fields[i]);
                    FieldNames[i] = fields[i].Name;
                }
            }

            private static Func<object, object> CreateFieldGetter(FieldInfo field)
            {
                var instanceParam = Expression.Parameter(typeof(object), "instance");
                var castInstance = Expression.Convert(instanceParam, field.DeclaringType);
                var fieldAccess = Expression.Field(castInstance, field);

                Expression body = field.FieldType.IsValueType
                    ? Expression.Convert(fieldAccess, typeof(object))
                    : fieldAccess;

                return Expression.Lambda<Func<object, object>>(body, instanceParam).Compile();
            }
        }

        /// <summary>
        /// 通用字段值获取
        /// </summary>
        public static object GetFieldValue(object instance, string fieldName)
        {
            if (instance == null) return null;

            var cache = _typeCache.GetOrAdd(instance.GetType(), t => new FieldAccessorCache(t));
            var fieldIndex = Array.IndexOf(cache.FieldNames, fieldName);

            if (fieldIndex >= 0)
            {
                return cache.FieldGetters[fieldIndex](instance);
            }

            return null;
        }

        /// <summary>
        /// 遍历所有字段
        /// </summary>
        public static void ProcessAllFields(object instance, Action<string, object> processor)
        {
            if (instance == null) return;

            var cache = _typeCache.GetOrAdd(instance.GetType(), t => new FieldAccessorCache(t));

            for (int i = 0; i < cache.FieldGetters.Length; i++)
            {
                var value = cache.FieldGetters[i](instance);
                processor(cache.FieldNames[i], value);
            }
        }
    }

    public class FieldAccessorUsage
    {
        //public void OriginalSlowMethod(RollerAlarm rollerAlarm)
        //{
        //    // 原始慢速代码
        //    FieldInfo[] fields = typeof(RollerAlarm).GetFields(
        //        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //    foreach (FieldInfo field in fields)
        //    {
        //        object value = field.GetValue(rollerAlarm);  // 慢！
        //        ProcessValue(field.Name, value);
        //    }
        //}

        //public void OptimizedMethod1(RollerAlarm rollerAlarm)
        //{
        //    // 优化方案1：使用泛型预编译访问器
        //    CompiledFieldAccessor<RollerAlarm>.ProcessAllFields(rollerAlarm, (name, value) =>
        //    {
        //        ProcessValue(name, value);
        //    });
        //}

        //public void OptimizedMethod2(RollerAlarm rollerAlarm)
        //{
        //    // 优化方案2：获取所有值数组
        //    object[] allValues = CompiledFieldAccessor<RollerAlarm>.GetAllFieldValues(rollerAlarm);
        //    var fields = typeof(RollerAlarm).GetFields(
        //        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

        //    for (int i = 0; i < allValues.Length; i++)
        //    {
        //        ProcessValue(fields[i].Name, allValues[i]);
        //    }
        //}

        //public void OptimizedMethod3(RollerAlarm rollerAlarm)
        //{
        //    // 优化方案3：单个字段访问
        //    object nameValue = CompiledFieldAccessor<RollerAlarm>.GetFieldValue(rollerAlarm, "Name");
        //    object idValue = CompiledFieldAccessor<RollerAlarm>.GetFieldValue(rollerAlarm, "_id");

        //    ProcessValue("Name", nameValue);
        //    ProcessValue("_id", idValue);
        //}

        public void GenericVersion(object obj)
        {
            // 通用版本（非泛型），适用于运行时类型不确定
            CompiledFieldAccessor.ProcessAllFields(obj, (name, value) =>
            {
                ProcessValue(name, value);
            });
        }

        private void ProcessValue(string fieldName, object value)
        {
            // 处理字段值
            Console.WriteLine($"{fieldName}: {value}");
        }
    }
}
