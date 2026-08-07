namespace CMP.Utils;

public static class TypeUtil
    {
        private static readonly Dictionary<string, Type> TypeAliases = new(StringComparer.OrdinalIgnoreCase)
        {
            ["bool"]    = typeof(bool),
            ["byte"]    = typeof(byte),
            ["sbyte"]   = typeof(sbyte),
            ["short"]   = typeof(short),
            ["ushort"]  = typeof(ushort),
            ["int"]     = typeof(int),
            ["uint"]    = typeof(uint),
            ["long"]    = typeof(long),
            ["ulong"]   = typeof(ulong),
            ["float"]   = typeof(float),
            ["double"]  = typeof(double),
            ["decimal"] = typeof(decimal),
            ["char"]    = typeof(char),
            ["string"]  = typeof(string),
            ["object"]  = typeof(object)
        };

        /// <summary>
        /// Resolves a type by its alias or assembly-qualified name.
        /// </summary>
        /// <param name="typeName">
        /// The C# alias (e.g., "int", "string"), simple type name, or assembly-qualified name.
        /// </param>
        /// <returns>The resolved <see cref="Type"/>.</returns>
        /// <exception cref="TypeLoadException">Thrown if the type cannot be found.</exception>
        public static Type GetTypeByName(string typeName)
        {
            if (string.IsNullOrWhiteSpace(typeName))
                throw new ArgumentException(@"Type name must be non-empty", nameof(typeName));

            // try lookup in predefined aliases first
            if (TypeAliases.TryGetValue(typeName.Trim(), out var aliasType))
                return aliasType;

            // try to resolve as an assembly-qualified name
            var t = Type.GetType(typeName, throwOnError: false, ignoreCase: true);
            if (t != null)
                return t;

            // search loaded assemblies for the type
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = asm.GetType(typeName, throwOnError: false, ignoreCase: true)
                    ?? asm.GetType(asm.GetName().Name + "." + typeName, false, true);
                if (t != null)
                    return t;
            }

            throw new TypeLoadException($"Unable to resolve type name '{typeName}'.");
        }
    }