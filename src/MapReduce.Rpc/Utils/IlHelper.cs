using System.Reflection;
using System.Reflection.Emit;

namespace QRWells.MapReduce.Rpc.Utils;

public class MethodHandler
{
    public MethodHandler(MethodInfo method)
    {
        Execute = ReflectionHandlerFactory.MethodHandler(method);
        Info = method;
    }

    public MethodInfo Info { get; }

    public FastMethodHandler Execute { get; }
}

public delegate object GetValueHandler(object source);

public delegate object ObjectInstanceHandler();

public delegate void SetValueHandler(object source, object value);

public delegate object FastMethodHandler(object target, object[] parameters);

public class ReflectionHandlerFactory
{
    private static void EmitCastToReference(ILGenerator il, Type type)
    {
        il.Emit(type.IsValueType ? OpCodes.Unbox_Any : OpCodes.Castclass, type);
    }

    private static void EmitBoxIfNeeded(ILGenerator il, Type type)
    {
        if (type.IsValueType) il.Emit(OpCodes.Box, type);
    }

    private static void EmitFastInt(ILGenerator il, int value)
    {
        switch (value)
        {
            case -1:
                il.Emit(OpCodes.Ldc_I4_M1);
                return;
            case 0:
                il.Emit(OpCodes.Ldc_I4_0);
                return;
            case 1:
                il.Emit(OpCodes.Ldc_I4_1);
                return;
            case 2:
                il.Emit(OpCodes.Ldc_I4_2);
                return;
            case 3:
                il.Emit(OpCodes.Ldc_I4_3);
                return;
            case 4:
                il.Emit(OpCodes.Ldc_I4_4);
                return;
            case 5:
                il.Emit(OpCodes.Ldc_I4_5);
                return;
            case 6:
                il.Emit(OpCodes.Ldc_I4_6);
                return;
            case 7:
                il.Emit(OpCodes.Ldc_I4_7);
                return;
            case 8:
                il.Emit(OpCodes.Ldc_I4_8);
                return;
        }

        if (value is > -129 and < 128)
            il.Emit(OpCodes.Ldc_I4_S, (sbyte)value);
        else
            il.Emit(OpCodes.Ldc_I4, value);
    }

    #region field handler

    private static readonly Dictionary<FieldInfo, GetValueHandler> FieldGetHandlers = new();

    private static readonly Dictionary<FieldInfo, SetValueHandler> FieldSetHandlers = new();

    public static GetValueHandler FieldGetHandler(FieldInfo field)
    {
        GetValueHandler handler;
        if (FieldGetHandlers.ContainsKey(field))
            handler = FieldGetHandlers[field];
        else
            lock (typeof(ReflectionHandlerFactory))
            {
                if (FieldGetHandlers.ContainsKey(field))
                {
                    handler = FieldGetHandlers[field];
                }
                else
                {
                    handler = CreateFieldGetHandler(field);
                    FieldGetHandlers.Add(field, handler);
                }
            }

        return handler;
    }

    private static GetValueHandler CreateFieldGetHandler(FieldInfo field)
    {
        var dm = new DynamicMethod("", typeof(object), new[] { typeof(object) }, field.DeclaringType);
        var ilGenerator = dm.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldfld, field);
        EmitBoxIfNeeded(ilGenerator, field.FieldType);
        ilGenerator.Emit(OpCodes.Ret);
        return (GetValueHandler)dm.CreateDelegate(typeof(GetValueHandler));
    }

    public static SetValueHandler FieldSetHandler(FieldInfo field)
    {
        SetValueHandler handler;
        if (FieldSetHandlers.ContainsKey(field))
            handler = FieldSetHandlers[field];
        else
            lock (typeof(ReflectionHandlerFactory))
            {
                if (FieldSetHandlers.ContainsKey(field))
                {
                    handler = FieldSetHandlers[field];
                }
                else
                {
                    handler = CreateFieldSetHandler(field);
                    FieldSetHandlers.Add(field, handler);
                }
            }

        return handler;
    }

    private static SetValueHandler CreateFieldSetHandler(FieldInfo field)
    {
        var dm =
            new DynamicMethod("", null, new[] { typeof(object), typeof(object) }, field.DeclaringType);
        var ilGenerator = dm.GetILGenerator();
        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        EmitCastToReference(ilGenerator, field.FieldType);
        ilGenerator.Emit(OpCodes.Stfld, field);
        ilGenerator.Emit(OpCodes.Ret);
        return (SetValueHandler)dm.CreateDelegate(typeof(SetValueHandler));
    }

    #endregion

    #region Property Handler

    private static readonly Dictionary<PropertyInfo, GetValueHandler> PropertyGetHandlers = new();

    private static readonly Dictionary<PropertyInfo, SetValueHandler> PropertySetHandlers = new();

    public static SetValueHandler PropertySetHandler(PropertyInfo property)
    {
        SetValueHandler handler;
        if (PropertySetHandlers.ContainsKey(property))
            handler = PropertySetHandlers[property];
        else
            lock (typeof(ReflectionHandlerFactory))
            {
                if (PropertySetHandlers.ContainsKey(property))
                {
                    handler = PropertySetHandlers[property];
                }
                else
                {
                    handler = CreatePropertySetHandler(property);
                    PropertySetHandlers.Add(property, handler);
                }
            }

        return handler;
    }

    private static SetValueHandler CreatePropertySetHandler(PropertyInfo property)
    {
        var dynamicMethod = new DynamicMethod(string.Empty, null,
            new[] { typeof(object), typeof(object) }, property.DeclaringType.Module);

        var ilGenerator = dynamicMethod.GetILGenerator();

        ilGenerator.Emit(OpCodes.Ldarg_0);
        ilGenerator.Emit(OpCodes.Ldarg_1);
        EmitCastToReference(ilGenerator, property.PropertyType);
        ilGenerator.EmitCall(OpCodes.Callvirt, property.GetSetMethod(), null);
        ilGenerator.Emit(OpCodes.Ret);
        var setter = (SetValueHandler)dynamicMethod.CreateDelegate(typeof(SetValueHandler));

        return setter;
    }

    public static GetValueHandler PropertyGetHandler(PropertyInfo property)
    {
        GetValueHandler handler;
        if (PropertyGetHandlers.ContainsKey(property))
            handler = PropertyGetHandlers[property];
        else
            lock (typeof(ReflectionHandlerFactory))
            {
                if (PropertyGetHandlers.ContainsKey(property))
                {
                    handler = PropertyGetHandlers[property];
                }
                else
                {
                    handler = CreatePropertyGetHandler(property);
                    PropertyGetHandlers.Add(property, handler);
                }
            }

        return handler;
    }

    private static GetValueHandler CreatePropertyGetHandler(PropertyInfo property)
    {
        var dynamicMethod = new DynamicMethod(string.Empty, typeof(object), new[] { typeof(object) },
            property.DeclaringType.Module);

        var ilGenerator = dynamicMethod.GetILGenerator();


        ilGenerator.Emit(OpCodes.Ldarg_0);


        ilGenerator.EmitCall(OpCodes.Callvirt, property.GetGetMethod(), null);


        EmitBoxIfNeeded(ilGenerator, property.PropertyType);


        ilGenerator.Emit(OpCodes.Ret);


        var getter = (GetValueHandler)dynamicMethod.CreateDelegate(typeof(GetValueHandler));

        return getter;
    }

    #endregion

    #region Method Handler

    private static readonly Dictionary<MethodInfo, FastMethodHandler> MethodHandlers = new();

    public static FastMethodHandler MethodHandler(MethodInfo method)
    {
        FastMethodHandler handler;
        if (MethodHandlers.ContainsKey(method))
            handler = MethodHandlers[method];
        else
            lock (typeof(ReflectionHandlerFactory))
            {
                if (MethodHandlers.ContainsKey(method))
                {
                    handler = MethodHandlers[method];
                }
                else
                {
                    handler = CreateMethodHandler(method);
                    MethodHandlers.Add(method, handler);
                }
            }

        return handler;
    }

    private static FastMethodHandler CreateMethodHandler(MethodInfo methodInfo)
    {
        var dynamicMethod = new DynamicMethod(string.Empty, typeof(object),
            new[] { typeof(object), typeof(object[]) }, methodInfo.DeclaringType.Module);
        var il = dynamicMethod.GetILGenerator();
        var ps = methodInfo.GetParameters();
        var paramTypes = new Type[ps.Length];
        for (var i = 0; i < paramTypes.Length; i++)
            if (ps[i].ParameterType.IsByRef)
                paramTypes[i] = ps[i].ParameterType.GetElementType();
            else
                paramTypes[i] = ps[i].ParameterType;

        var locals = new LocalBuilder[paramTypes.Length];

        for (var i = 0; i < paramTypes.Length; i++)
        {
            locals[i] = il.DeclareLocal(paramTypes[i], true);
            il.Emit(OpCodes.Ldarg_1);
            EmitFastInt(il, i);
            il.Emit(OpCodes.Ldelem_Ref);
            EmitCastToReference(il, paramTypes[i]);
            il.Emit(OpCodes.Stloc, locals[i]);
        }

        if (!methodInfo.IsStatic) il.Emit(OpCodes.Ldarg_0);

        for (var i = 0; i < paramTypes.Length; i++)
            il.Emit(ps[i].ParameterType.IsByRef ? OpCodes.Ldloca_S : OpCodes.Ldloc, locals[i]);

        il.EmitCall(methodInfo.IsStatic ? OpCodes.Call : OpCodes.Callvirt, methodInfo, null);

        if (methodInfo.ReturnType == typeof(void))
            il.Emit(OpCodes.Ldnull);
        else
            EmitBoxIfNeeded(il, methodInfo.ReturnType);

        for (var i = 0; i < paramTypes.Length; i++)
        {
            if (!ps[i].ParameterType.IsByRef) continue;
            il.Emit(OpCodes.Ldarg_1);
            EmitFastInt(il, i);
            il.Emit(OpCodes.Ldloc, locals[i]);
            if (locals[i].LocalType.IsValueType)
                il.Emit(OpCodes.Box, locals[i].LocalType);
            il.Emit(OpCodes.Stelem_Ref);
        }

        il.Emit(OpCodes.Ret);
        return (FastMethodHandler)dynamicMethod.CreateDelegate(typeof(FastMethodHandler));
    }

    #endregion

    #region Instance Handler

    private static readonly Dictionary<Type, ObjectInstanceHandler> InstanceHandlers = new();

    public static ObjectInstanceHandler InstanceHandler(Type type)
    {
        ObjectInstanceHandler handler;
        if (InstanceHandlers.ContainsKey(type))
            handler = InstanceHandlers[type];
        else
            lock (typeof(ReflectionHandlerFactory))
            {
                if (InstanceHandlers.ContainsKey(type))
                {
                    handler = InstanceHandlers[type];
                }
                else
                {
                    handler = CreateInstanceHandler(type);
                    InstanceHandlers.Add(type, handler);
                }
            }

        return handler;
    }

    private static ObjectInstanceHandler CreateInstanceHandler(Type type)
    {
        var method = new DynamicMethod(string.Empty, type, null, type.Module);
        var il = method.GetILGenerator();
        il.DeclareLocal(type, true);
        il.Emit(OpCodes.Newobj, type.GetConstructor(new Type[0]));
        il.Emit(OpCodes.Stloc_0);
        il.Emit(OpCodes.Ldloc_0);
        il.Emit(OpCodes.Ret);
        return (ObjectInstanceHandler)method.CreateDelegate(typeof(ObjectInstanceHandler));
    }

    #endregion
}