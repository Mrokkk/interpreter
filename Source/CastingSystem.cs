using System;
using System.Collections.Generic;

public class CastingSystem
{
    static Dictionary<System.Type, Types> nativeTypeToType;
    static Dictionary<Types, System.Type> typeToNativeType;
    static Dictionary<(Types, Types), Func<dynamic, dynamic>> castingTable;
    static Dictionary<Types, TypeInfo> typeInfo;
    static Dictionary<string, TypeInfo> keywordToTypeInfo;

    public static void Initialize()
    {
        // TODO: how to treat builtin functions?

        typeInfo = new Dictionary<Types, TypeInfo>{
            {Types.Int, new TypeInfo(Types.Int, typeof(int), "int")},
            {Types.Float, new TypeInfo(Types.Float, typeof(float), "float")},
            {Types.Double, new TypeInfo(Types.Double, typeof(double), "double")},
            {Types.String, new TypeInfo(Types.String, typeof(string), "string")},
            {Types.Bool, new TypeInfo(Types.Bool, typeof(bool), "bool")},
            {Types.function, new TypeInfo(Types.function, typeof(FunctionObject), "function")},
            {Types.exception, new TypeInfo(Types.exception, typeof(Exception), "exception")},
            {Types.List, new TypeInfo(Types.List, typeof(List<dynamic>), "[]")},
            {Types.IntList, new TypeInfo(Types.IntList, typeof(List<int>), "int[]")},
            {Types.FloatList, new TypeInfo(Types.FloatList, typeof(List<float>), "float[]")},
            {Types.StringList, new TypeInfo(Types.StringList, typeof(List<string>), "string[]")},
        };

        nativeTypeToType = new Dictionary<System.Type, Types>();
        typeToNativeType = new Dictionary<Types, System.Type>();
        keywordToTypeInfo = new Dictionary<string, TypeInfo>();

        foreach (var entry in typeInfo)
        {
            var typeInfo = entry.Value;
            nativeTypeToType[typeInfo.nativeType] = typeInfo.type;
            typeToNativeType[typeInfo.type] = typeInfo.nativeType;
            keywordToTypeInfo[typeInfo.name] = typeInfo;
        }

        nativeTypeToType[typeof(Block)] = Types.function;

        castingTable = new Dictionary<(Types, Types), Func<dynamic, dynamic>>();
        castingTable[(Types.Int, Types.Float)] = v => (float)v;
        castingTable[(Types.Float, Types.Int)] = v => (int)v;
        castingTable[(Types.Int, Types.Double)] = v => (double)v;
        castingTable[(Types.Float, Types.Double)] = v => (double)v;
        castingTable[(Types.Double, Types.Float)] = v => (float)v;
        castingTable[(Types.Double, Types.Int)] = v => (int)v;
    }

    public static Types FromNative(Type type)
    {
        return nativeTypeToType[type];
    }

    public static Type ToNative(Types type)
    {
        return typeToNativeType[type];
    }

    public static dynamic Cast(Types from, Types to, dynamic value)
    {
        var tuple = (from, to);
        if (from == to)
        {
            return value;
        }
        if (!castingTable.ContainsKey(tuple))
        {
            var fromTypeInfo = GetTypeInfo(from) ?? new TypeInfo(Types.Null, typeof(int), "nulltype");
            var toTypeInfo = GetTypeInfo(to) ?? new TypeInfo(Types.Null, typeof(int), "nulltype");
            throw new Exception($"Unsupported cast from type {fromTypeInfo} to type {toTypeInfo}");
        }
        return castingTable[tuple](value);
    }

    public static TypeInfo GetTypeInfo(Types type)
    {
        return typeInfo.GetValue(type);
    }

    public static TypeInfo GetTypeInfo(Type type)
    {
        return typeInfo.GetValue(nativeTypeToType.GetValue(type));
    }

    public static TypeInfo GetTypeInfo(string name)
    {
        return keywordToTypeInfo.GetValue(name);
    }
}
