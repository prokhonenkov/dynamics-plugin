﻿#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\BasePlugin.cs
namespace Kipon.Fake.Xrm
{
    using System;
    using Microsoft.Xrm.Sdk;
    public class BasePlugin : IPlugin
    {
        public string UnsecureConfig { get; private set; }
        public string SecureConfig { get; private set; }

        private Reflection.PluginMethod.Cache pluginMethodcache;

        #region constructors
        public BasePlugin() : base()
        {
            this.pluginMethodcache = new Reflection.PluginMethod.Cache(typeof(BasePlugin).Assembly);
            Reflection.Types.Instance.SetAssembly(typeof(BasePlugin).Assembly);
        }

        public BasePlugin(string unSecure, string secure) : this()
        {
            this.UnsecureConfig = unSecure;
            this.SecureConfig = secure;
        }
        #endregion

        #region iplugin impl
        public void Execute(IServiceProvider serviceProvider)
        {
            var context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            var serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            var tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            var userId = context.UserId;
            var message = context.MessageName;
            var stage = context.Stage;
            var isAsync = context.Mode == 1;

            using (var serviceCache = new Reflection.ServiceCache(context, serviceFactory, tracingService))
            {
                var methods = this.pluginMethodcache.ForPlugin(this.GetType(), stage, message, context.PrimaryEntityName, context.Mode == 1);

                foreach (var method in methods)
                {
                    #region find out if method is relevant, looking a target fields
                    if (message == Attributes.StepAttribute.MessageEnum.Update.ToString() && !method.FilterAllProperties)
                    {
                        var target = (Microsoft.Xrm.Sdk.Entity)context.InputParameters["Target"];
                        if (!method.IsRelevant(target))
                        {
                            continue;
                        }
                    }
                    #endregion

                    #region now resolve all parameters
                    var args = new object[method.Parameters.Length];
                    var ix = 0;
                    foreach (var p in method.Parameters)
                    {
                        args[ix] = serviceCache.Resolve(p);
                        ix++;
                    }
                    #endregion

                    #region run the method
                    method.Invoke(this, args);
                    #endregion

                    #region prepare for next method
                    serviceCache.OnStepFinalize();
                    #endregion
                }
            }
        }
        #endregion
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\IAdminUnitOfWork.cs
namespace Kipon.Fake.Xrm
{
    public interface IAdminUnitOfWork : IUnitOfWork
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\IRepository.cs
namespace Kipon.Fake.Xrm
{
    using System;
    using System.Linq;

    /// <summary>
    /// Interface for generic repository implementations.
    /// </summary>
    public interface IRepository<T> where T : Microsoft.Xrm.Sdk.Entity, new()
    {
        /// <summary>
        /// Gets an <see cref="IQueryable{T}"/> to perform further operations.
        /// </summary>
        /// <returns>An <see cref="IQueryable{T}"/>.</returns>
        IQueryable<T> GetQuery();

        T GetById(Guid id);

        /// <summary>
        /// Deletes the given entity.
        /// </summary>
        /// <param name="entity">The entity to delete.</param>
        void Delete(T entity);

        /// <summary>
        /// Adds the given entity.
        /// </summary>
        /// <param name="entity">The entity to add.</param>
        void Add(T entity);

        /// <summary>
        /// Updates the given entity.
        /// </summary>
        /// <param name="entity">The entity to update.</param>
        void Update(T entity);

        /// <summary>
        /// Attaches the the given entity to the current context.
        /// </summary>
        /// <param name="entity">The entity to attach.</param>
        void Attach(T entity);


        /// <summary>
        /// Detach the object from the context
        /// </summary>
        /// <param name="entity"></param>
        void Detach(T entity);

    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\IService.cs
namespace Kipon.Fake.Xrm
{
    public interface IService
    {
        void OnStepFinalized();
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\IUnitOfWork.cs
namespace Kipon.Fake.Xrm
{
    using Microsoft.Xrm.Sdk;
    using System;
    public interface IUnitOfWork
    {
        R ExecuteRequest<R>(OrganizationRequest request) where R : OrganizationResponse;
        OrganizationResponse Execute(OrganizationRequest request);
        System.Guid Create(Entity entity);
        void Update(Entity entity);
        void Delete(Entity entity);
        void SaveChanges();
        void ClearChanges();
        void Detach(string logicalname, Guid? id);
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Mergedimage.cs
namespace Kipon.Fake.Xrm
{
    // Declarative interface to request a merged image
    // A merged image takes the tarkget of a plugin, and combine it with the field available in the target.
    // A merged image should only expose the get part of properties, because any change will not be posted 
    // back to the server.
    public interface Mergedimage<T> where T : Microsoft.Xrm.Sdk.Entity
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Postimage.cs
namespace Kipon.Fake.Xrm
{
    // Represent a post image
    // an extension should only expose getters, because any change will not be send back to crm
    public interface Postimage<T> where T : Microsoft.Xrm.Sdk.Entity
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Preimage.cs
namespace Kipon.Fake.Xrm
{
    // Declarativ interface to represent a pre image.
    // any extension should only have get properties, because any change will not be pushed back to the server
    public interface Preimage<T> where T : Microsoft.Xrm.Sdk.Entity
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Target.cs
namespace Kipon.Fake.Xrm
{

    /// <summary>
    /// Declarativ inteface to represent the target of a plugin
    /// Any extension can have getters and setters, and if changed in a pre process, any change will be send back
    /// to the server together with the initial state of the target
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface Target<T> where T : Microsoft.Xrm.Sdk.Entity
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\TargetReference.cs
namespace Kipon.Fake.Xrm
{
    using System;
    public abstract class TargetReference<T> where T : Microsoft.Xrm.Sdk.Entity
    {
        private Microsoft.Xrm.Sdk.EntityReference target;

        public TargetReference(Microsoft.Xrm.Sdk.EntityReference target)
        {
            if (target.LogicalName != this._logicalName)
            {
                throw new ArgumentException($"Target reference does not match this type, expected {_logicalName} got {target.LogicalName }");
            }
            this.target = target;
        }

        public Microsoft.Xrm.Sdk.EntityReference Value
        {
            get
            {
                return this.target;
            }
        }

        protected abstract string _logicalName { get; }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\AdminAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;

    /// <summary>
    /// Property indicating that the underlying IOrganizationService must be run with system priviliges
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, Inherited = false, AllowMultiple = false)]
    public class AdminAttribute : Attribute
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\ExportAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// Use Export attribute as decoration for interfaces with multi implementation to state the one and only to be used.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = true)]
    public class ExportAttribute : Attribute
    {
        private Type type;
        public ExportAttribute(Type type)
        {
            this.type = type;
        }

        public Type Type
        {
            get
            {
                return this.type;
            }
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\ImportingConstructorAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// Use ImportingConstructor to decorate classes with multi public constructor, decorating the one and only constructor to be used to create the instance
    /// </summary>
    [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false)]
    public class ImportingConstructorAttribute : Attribute
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\LogicalNameAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// For steps supporting multi entity types, decorate the method with one ore mote logical names to be supported
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    class LogicalNameAttribute : Attribute
    {
        public LogicalNameAttribute(string name)
        {
            this.Value = name;
        }

        public string Value
        {
            get; private set;
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\MergedimageAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// Use this attribute to state that the parameter should be populated with the MergedImage
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class MergedimageAttribute : Attribute
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\PostimageAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// Use the attribute to state that the parameter should be populated with the post image
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class PostimageAttribute : Attribute
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\PreimageAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// Use the attribute to state the the parameter should be populated with the preimage
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class PreimageAttribute : Attribute
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\RequiredAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;

    [AttributeUsage(AttributeTargets.Property)]
    public class RequiredAttribute : Attribute
    {

        public RequiredAttribute()
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\SortAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// Use the parameter on plugin methods to indicate order of execution for methods triggered by the same event
    /// The lowest value for a plugin will be used as the deployment value as well.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class SortAttribute : Attribute
    {

        public SortAttribute(int value)
        {
            this.Value = value;
        }

        public int Value { get; private set; }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\StepAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// Use this method in plugins to state that the method should be called as a step.
    /// The recommended approach is using naming conventions, but if for some reason this cannot be used, add the attribute to the method.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class StepAttribute : Attribute
    {

        public StepAttribute(StageEnum stage, MessageEnum message, string primaryEntityName)
        {
            this.Stage = (int)stage;
            this.Message = message.ToString();
            this.PrimaryEntityName = primaryEntityName;
        }

        public int Stage { get; private set; }
        public string Message { get; private set; }
        public string PrimaryEntityName { get; set; }

        public bool IsAsync
        {
            get
            {
                return this.Stage == (int)StageEnum.PostAsync;
            }
        }

        public enum StageEnum
        {
            Validate = 10,
            Pre = 20,
            Post = 40,
            PostAsync = 41
        }

        public enum MessageEnum
        {
            Create,
            Update,
            Delete,
            Associate,
            Disassociate,
            RetrieveMultiple,
            Retrieve,
            AddMember,
            AddListMembers,
            RemoveMember,
            Merge
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Attributes\TargetAttribute.cs
namespace Kipon.Fake.Xrm.Attributes
{
    using System;
    /// <summary>
    /// Use this decoration to get target populated in the parameter
    /// </summary>
    [AttributeUsage(AttributeTargets.Parameter, AllowMultiple = false)]
    public class TargetAttribute : Attribute
    {
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\BaseException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    using System;
    public class BaseException : Exception
    {
        public BaseException(string message) : base(message) { }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\CircularDependencyException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    public class CircularDependencyException : BaseException
    {
        public CircularDependencyException(string path) : base($"Circular dependendy detected. Path: {path}.")
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\InvalidConstructorServiceArgumentException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    public class InvalidConstructorServiceArgumentException : BaseException
    {
        public InvalidConstructorServiceArgumentException(System.Reflection.ConstructorInfo con, System.Reflection.ParameterInfo par) : base($"constructor on {con.DeclaringType.FullName} is requesting parameter types that are only available on plugin methods. Parse these arguments to service methods instead of constructor injection: { par.Name}")
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\MultiImplementationOfSameInterfaceException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    using System;
    public class MultiImplementationOfSameInterfaceException : BaseException
    {
        public MultiImplementationOfSameInterfaceException(Type type) : base($"{type.FullName} has more than one implementation. Mark the one to be used with export flag.")
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\MultipleLogicalNamesException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    using System;
    public class MultipleLogicalNamesException : BaseException
    {
        public MultipleLogicalNamesException(Type type, System.Reflection.MethodInfo method) : base($"{ type.FullName }, method { method.Name } is requesting entities of different types. That is not allowed.")
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\TypeMismatchException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    using System;
    public class TypeMismatchException : BaseException
    {
        public TypeMismatchException(Type fromType, Type toType) : base($"{toType.FullName} does not implement expected interface {fromType.FullName}")
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\UnavailableImageException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    using System;
    public class UnavailableImageException : BaseException
    {
        public UnavailableImageException(Type type, System.Reflection.MethodInfo method, string image, int stage, string message) : base($"{type.FullName}.{method.Name} is requesting image {image}. That is not supported in state {stage}, message {message}.")
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\UnknownEntityTypeException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    public class UnknownEntityTypeException : BaseException
    {
        public UnknownEntityTypeException(string logicalname) : base($"{logicalname} cannot be converted to an early bound entity.")
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\UnresolvableConstructorException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    using System;
    public class UnresolvableConstructorException : BaseException
    {
        public UnresolvableConstructorException(Type type) : base($"{type.FullName} has more than one constructor, mark exactly one of them with the (ImportingConstructor] attribute to indicate witch to use.)")
        {

        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\UnresolvablePluginMethodException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    using System;
    public class UnresolvablePluginMethodException : BaseException
    {
        public UnresolvablePluginMethodException(Type type) : base($"{type.FullName} did not have any steps to be executed. Ether follow the naming convention for the plugin method, or add the [Step] custom attributes to methods to be executed")
        {

        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Exceptions\UnresolvableTypeException.cs
namespace Kipon.Fake.Xrm.Exceptions
{
    using System;
    public class UnresolvableTypeException : BaseException
    {
        public UnresolvableTypeException(Type fromType) : base($"{fromType.FullName} could not be resolved to a class with an available public constructor.")
        {
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Extensions\Sdk\KiponSdkGeneratedExtensionMethods.cs
namespace Kipon.Fake.Xrm.Extensions.Sdk
{
    using System;
    using System.Collections.Generic;
    public static partial class KiponSdkGeneratedExtensionMethods
    {
        private static readonly Dictionary<string, Type> entittypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, System.Reflection.MethodInfo> TO_ENT_GENS = new Dictionary<string, System.Reflection.MethodInfo>();
        private static readonly System.Reflection.MethodInfo TO_ENTITY = typeof(Microsoft.Xrm.Sdk.Entity).GetMethod("ToEntity", new Type[0]);

        public static T ToEarlyBoundEntity<T>(this T ent) where T : Microsoft.Xrm.Sdk.Entity
        {
            if (ent.GetType().BaseType == typeof(Microsoft.Xrm.Sdk.Entity))
            {
                return ent;
            }

            if (TO_ENT_GENS.ContainsKey(ent.LogicalName))
            {
                return TO_ENT_GENS[ent.LogicalName].Invoke(ent, new object[0]) as T;
            }

            if (!entittypes.ContainsKey(ent.LogicalName))
            {
                throw new Exceptions.UnknownEntityTypeException(ent.LogicalName);
            }

            var type = entittypes[ent.LogicalName];
            TO_ENT_GENS[ent.LogicalName] = TO_ENTITY.MakeGenericMethod(type);

            return TO_ENT_GENS[ent.LogicalName].Invoke(ent, new object[0]) as T;
        }

        public static object GetSafeValue(this Microsoft.Xrm.Sdk.Entity entity, string attribLogicalName)
        {
            if (!entity.Attributes.ContainsKey(attribLogicalName))
            {
                return null;
            }
            return entity[attribLogicalName];
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Extensions\Strings\StringMethods.cs
namespace Kipon.Fake.Xrm.Extensions.Strings
{
    using System.Linq;
    public static class StringMethods
    {
        public static string FirstToUpper(this string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }

            return value.First().ToString().ToUpper() + value.Substring(1).ToLower();
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Reflection\CommonProperty.cs
namespace Kipon.Fake.Xrm.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    public class CommonProperty
    {

        public class Cache
        {
            private readonly Dictionary<Type, CommonProperty[]> cache = new Dictionary<Type, CommonProperty[]>();
            private readonly Types Types;

            public Cache(System.Reflection.Assembly assm)
            {
                this.Types = Types.Instance;
            }

            public CommonProperty[] ForType(Type interfaceType, Type entityType)
            {
                if (cache.ContainsKey(interfaceType))
                {
                    return cache[interfaceType];
                }

                var interfaceProperties = interfaceType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var instanceProperties = entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                var result = new List<CommonProperty>();

                foreach (var interfaceProp in interfaceProperties)
                {
                    var instanceProp = (from i in instanceProperties where i.Name == interfaceProp.Name select i).SingleOrDefault();
                    if (instanceProp != null)
                    {
                        var customProp = (Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute)instanceProp.GetCustomAttributes(typeof(Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute), false).FirstOrDefault();
                        if (customProp != null)
                        {
                            var attr = new CommonProperty { LogicalName = customProp.LogicalName };
                            attr.Required = interfaceProp.GetCustomAttributes(Types.RequiredAttribute, false).Any();

                            result.Add(attr);
                        }
                    }
                }
                cache[interfaceType] = result.ToArray();
                return cache[interfaceType];
            }

        }

        private static readonly Dictionary<Type, CommonProperty[]> cache = new Dictionary<Type, CommonProperty[]>();

        private static Types Types;

        static CommonProperty()
        {
            CommonProperty.Types = Types.Instance;
        }

        public static CommonProperty[] ForType(Type interfaceType, Type entityType)
        {
            if (cache.ContainsKey(interfaceType))
            {
                return cache[interfaceType];
            }

            var interfaceProperties = interfaceType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            var instanceProperties = entityType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

            var result = new List<CommonProperty>();

            foreach (var interfaceProp in interfaceProperties)
            {
                var instanceProp = (from i in instanceProperties where i.Name == interfaceProp.Name select i).SingleOrDefault();
                if (instanceProp != null)
                {
                    var customProp = (Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute)instanceProp.GetCustomAttributes(typeof(Microsoft.Xrm.Sdk.AttributeLogicalNameAttribute), false).FirstOrDefault();
                    if (customProp != null)
                    {
                        var attr = new CommonProperty { LogicalName = customProp.LogicalName };
                        attr.Required = interfaceProp.GetCustomAttributes(Types.RequiredAttribute, false).Any();

                        result.Add(attr);
                    }
                }
            }
            cache[interfaceType] = result.ToArray();
            return cache[interfaceType];
        }

        public string LogicalName { get; private set; }
        public bool Required { get; private set; }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Reflection\PluginMethod.cs
namespace Kipon.Fake.Xrm.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    public class PluginMethod
    {
        public class Cache
        {
            private Types Types;
            private readonly Dictionary<string, PluginMethod[]> cache = new Dictionary<string, PluginMethod[]>();

            public Cache(System.Reflection.Assembly assm)
            {
                this.Types = Types.Instance;
            }

            public PluginMethod[] ForPlugin(Type type, int stage, string message, string primaryEntityName, bool isAsync, bool throwIfEmpty = true)
            {
                var key = type.FullName + "|" + stage + "|" + message + "|" + primaryEntityName + "|" + isAsync.ToString();

                if (cache.ContainsKey(key))
                {
                    return cache[key];
                }

                var lookFor = $"On{stage.ToStage()}{message}{(isAsync ? "Async" : "")}";

                var methods = type.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                var stepStage = stage == 40 && isAsync ? 41 : stage;

                List<PluginMethod> results = new List<PluginMethod>();

                foreach (var method in methods)
                {
                    #region explicit step decoration mathing
                    var cas = method.GetCustomAttributes(Types.StepAttribute, false);
                    var found = false;
                    foreach (var ca in cas)
                    {
                        var at = ca.GetType();
                        var _stage = (int)at.GetProperty("Stage").GetValue(ca);
                        var _message = at.GetProperty("Message").GetValue(ca).ToString();
                        var _primaryEntityName = (string)at.GetProperty("PrimaryEntityName").GetValue(ca);
                        var _isAsync = (bool)at.GetProperty("IsAsync").GetValue(ca);


                        if (_stage == stepStage && _message == message && _primaryEntityName == primaryEntityName && _isAsync == isAsync)
                        {
                            var next = CreateFrom(method);
                            AddIfConsistent(type, method, results, next, message, stage);
                            found = true;
                            break;
                        }
                    }

                    if (found)
                    {
                        continue;
                    }

                    if (cas.Length > 0)
                    {
                        continue;
                    }
                    #endregion

                    #region find by naming convention
                    if (method.Name == lookFor)
                    {
                        var next = CreateFrom(method);
                        var logicalNames = (from n in next.Parameters where n.LogicalName != null select n.LogicalName).Distinct().ToArray();

                        if (logicalNames.Length == 1)
                        {
                            if (logicalNames[0] == primaryEntityName)
                            {
                                AddIfConsistent(type, method, results, next, message, stage);
                                found = true;
                            }
                            continue;
                        }

                        if (logicalNames.Length > 1)
                        {
                            throw new Exceptions.MultipleLogicalNamesException(type, method);
                        }

                        if (next.HasTargetPreOrPost() || next.HasTargetReference())
                        {
                            var logicalNamesAttrs = method.GetCustomAttributes(Types.LogicalNameAttribute, false).ToArray();
                            foreach (var attr in logicalNamesAttrs)
                            {
                                var v = (string)attr.GetType().GetProperty("primaryEntityName").GetValue(attr);
                                if (v == primaryEntityName)
                                {
                                    found = true;
                                    break;
                                }
                            }

                            if (found)
                            {
                                AddIfConsistent(type, method, results, next, message, stage);
                                continue;
                            }
                        }
                        else
                        {
#warning TO-DO
                            // TO-DO: what to do, we have a method match, but no entities. Some methods ex. assosiate does not have target in deployment process.
                            throw new NotImplementedException("handling method not attached to a logicalname is not supported yet.");
                        }
                    }
                    #endregion
                }

                if (results.Count == 0 && throwIfEmpty)
                {
                    throw new Exceptions.UnresolvablePluginMethodException(type);
                }

                cache[key] = results.OrderBy(r => r.Sort).ToArray();
                return cache[key];
            }

            private void AddIfConsistent(Type type, System.Reflection.MethodInfo method, List<PluginMethod> results, PluginMethod result, string message, int stage)
            {
                #region validate pre and post image consistancy
                switch (stage)
                {
                    case 10:
                    case 20:
                        /* pre image pre event */
                        if (result.HasPreimage() && message == "Create")
                        {
                            throw new Exceptions.UnavailableImageException(type, method, "Preimage", stage, message);
                        }
                        /* post image pre event */
                        if (result.HasPostimage())
                        {
                            throw new Exceptions.UnavailableImageException(type, method, "Postimage", stage, message);
                        }
                        break;
                    case 40:
                    case 41:
                        if (result.HasPostimage() && message == "Delete")
                        {
                            throw new Exceptions.UnavailableImageException(type, method, "Postimage", stage, message);
                        }
                        break;
                }
                #endregion

                #region validate target consistancy
                if (result.HasTarget() && message == "Delete")
                {
                    throw new Exceptions.UnavailableImageException(type, method, "Target", stage, message);
                }

                if (result.HasTargetReference())
                {
                    var inError = true;
                    if (message == "Delete")
                    {
                        inError = false;
                    }

                    // ADD other conditions where target is relevant, ex associate

                    if (inError)
                    {
                        throw new Exceptions.UnavailableImageException(type, method, "Target", stage, message);
                    }
                }
                #endregion

                results.Add(result);
            }

            private PluginMethod CreateFrom(System.Reflection.MethodInfo method)
            {
                var result = new PluginMethod();
                result.method = method;
                var parameters = method.GetParameters().DefaultIfEmpty().ToArray();

                result.Parameters = new TypeCache[parameters.Length];
                var ix = 0;
                foreach (var parameter in parameters)
                {
                    result.Parameters[ix] = TypeCache.ForParameter(parameter);
                    ix++;
                }

                var sortAttr = method.GetCustomAttributes(Types.SortAttribute, false).SingleOrDefault();
                if (sortAttr != null)
                {
                    result.Sort = (int)sortAttr.GetType().GetProperty("Value").GetValue(sortAttr);
                }
                else
                {
                    result.Sort = 1;
                }
                return result;
            }

        }

        private PluginMethod()
        {
        }

        public static string ImageSuffixFor(int pre1post2, int stage, bool async)
        {
            var first = "Preimage";
            if (pre1post2 == 2)
            {
                first = "Postimage";
            }
            switch (stage)
            {
                case 10: return $"{first}Validate";
                case 20: return $"{first}Pre";
                case 40:
                    {
                        if (async) return $"{first}PostAsync";
                        return $"{first}Post";
                    }
            }
            throw new ArgumentException($"{nameof(stage)} can be 10, 20 or 40");
        }

        private System.Reflection.MethodInfo method;
        public int Sort { get; set; }
        public TypeCache[] Parameters { get; private set; }

        #region target filter attributes
        private bool? _filterAllProperties;
        public bool FilterAllProperties
        {
            get
            {
                if (_filterAllProperties == null)
                {
                    _filterAllProperties = this.Parameters != null && this.Parameters.Where(r => r.IsTarget && r.AllProperties).Any();
                }
                return _filterAllProperties.Value;
            }
        }

        private CommonProperty[] _filteredProperties;
        public CommonProperty[] FilteredProperties
        {
            get
            {
                if (_filteredProperties == null)
                {
                    if (Parameters != null)
                    {
                        var result = new List<CommonProperty>();
                        foreach (var p in Parameters)
                        {
                            if (p.IsTarget && p.FilteredProperties != null && p.FilteredProperties.Length > 0)
                            {
                                result.AddRange(p.FilteredProperties);
                            }
                        }
                        _filteredProperties = result.ToArray();
                    }
                    else
                    {
                        _filteredProperties = new CommonProperty[0];
                    }
                }
                return _filteredProperties;
            }
        }
        #endregion

        #region preimage attributes
        private bool? _needPreimage;
        public bool NeedPreimage
        {
            get
            {
                if (this._needPreimage == null)
                {
                    this._needPreimage = this.Parameters != null && this.Parameters.Where(r => r.IsPreimage || r.IsMergedimage).Any();
                }
                return this._needPreimage.Value;
            }
        }

        private bool? _allPreimageProperties;
        public bool AllPreimageProperties
        {
            get
            {
                if (_allPreimageProperties == null)
                {
                    _allPreimageProperties = this.Parameters != null && this.Parameters.Where(r => (r.IsPreimage || r.IsMergedimage) && r.AllProperties).Any();
                }
                return _allPreimageProperties.Value;
            }
        }

        private CommonProperty[] _preimageProperties;
        public CommonProperty[] PreimageProperties
        {
            get
            {
                if (_preimageProperties == null)
                {
                    if (this.Parameters != null)
                    {
                        var result = new List<CommonProperty>();
                        foreach (var p in Parameters)
                        {
                            if ((p.IsPreimage || p.IsMergedimage) && p.FilteredProperties != null && p.FilteredProperties.Length > 0)
                            {
                                result.AddRange(p.FilteredProperties);
                            }
                        }
                        _preimageProperties = result.ToArray();
                    }
                    else
                    {
                        this._preimageProperties = new CommonProperty[0];
                    }
                }
                return _preimageProperties;
            }
        }
        #endregion

        #region postimage attributes
        private bool? _needPostimage;
        public bool NeedPostimage
        {
            get
            {
                if (this._needPostimage == null)
                {
                    this._needPostimage = this.Parameters != null && this.Parameters.Where(r => r.IsPostimage).Any();
                }
                return this._needPostimage.Value;
            }
        }

        private bool? _allPostimageProperties;
        public bool AllPostimageProperties
        {
            get
            {
                if (this._allPostimageProperties == null)
                {
                    this._allPostimageProperties = this.Parameters != null && this.Parameters.Where(r => r.AllProperties).Any();
                }
                return this._allPostimageProperties.Value;
            }
        }

        private CommonProperty[] _postimageProperties;
        public CommonProperty[] PostimageProperties
        {
            get
            {
                if (this._postimageProperties == null)
                {
                    if (this.Parameters != null)
                    {
                        var result = new List<CommonProperty>();
                        foreach (var p in this.Parameters)
                        {
                            if (p.IsPostimage && p.FilteredProperties != null && p.FilteredProperties.Length > 0)
                            {
                                result.AddRange(p.FilteredProperties);
                            }
                        }
                        this._postimageProperties = result.ToArray();
                    }
                    else
                    {
                        this._postimageProperties = new CommonProperty[0];
                    }
                }
                return this._postimageProperties;
            }
        }
        #endregion

        private bool? _hasRequredProperties = null;
        public bool HasRequiredProperties
        {
            get
            {
                if (this._hasRequredProperties == null)
                {
                    this._hasRequredProperties = (from f in this.FilteredProperties where f.Required == true select f).Any();
                }
                return this._hasRequredProperties.Value;
            }
        }

        public bool HasPreimage()
        {
            return this.Parameters != null && (this.Parameters.Where(r => r.IsPreimage || r.IsMergedimage)).Any();
        }

        public void Invoke(object instance, object[] args)
        {
            this.method.Invoke(instance, args);
        }

        public bool HasPostimage()
        {
            return this.Parameters != null && (this.Parameters.Where(r => r.IsPostimage)).Any();
        }

        public bool HasTarget()
        {
            return this.Parameters != null && (this.Parameters.Where(r => r.IsTarget)).Any();
        }

        public bool HasTargetPreOrPost()
        {
            return this.Parameters != null && this.Parameters.Where(r => r.IsTarget || r.IsPreimage || r.IsMergedimage || r.IsPostimage).Any();
        }

        public bool HasTargetReference()
        {
            return this.Parameters != null && this.Parameters.Where(r => r.IsReference).Any();
        }

        public bool IsRelevant(Microsoft.Xrm.Sdk.Entity target)
        {
            if (this.FilterAllProperties)
            {
                return true;
            }

            if (this.FilteredProperties != null && this.FilteredProperties.Length > 0)
            {
                foreach (var f in this.FilteredProperties)
                {
                    if (target.Attributes.Keys.Contains(f.LogicalName))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
    }

    internal static class PluginMethodCacheLocalExtensions
    {
        public static string ToStage(this int value)
        {
            switch (value)
            {
                case 10: return "Validate";
                case 20: return "Pre";
                case 40: return "Post";
                default: throw new Microsoft.Xrm.Sdk.InvalidPluginExecutionException($"Unknown state {value}");
            }
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Reflection\ServiceCache.cs
namespace Kipon.Fake.Xrm.Reflection
{
    using Microsoft.Xrm.Sdk;
    using System;
    using System.Collections.Generic;
    public class ServiceCache : System.IDisposable
    {
        private readonly Dictionary<string, object> services = new Dictionary<string, object>();

        private Guid systemuserid;

        private IPluginExecutionContext pluginExecutionContext;
        private IOrganizationServiceFactory organizationServiceFactory;

        public ServiceCache(IPluginExecutionContext pluginExecutionContext, IOrganizationServiceFactory organizationServiceFactory, ITracingService traceService)
        {
            this.pluginExecutionContext = pluginExecutionContext;
            this.organizationServiceFactory = organizationServiceFactory;
            this.services.Add(typeof(IPluginExecutionContext).FullName, pluginExecutionContext);
            this.services.Add(typeof(IOrganizationServiceFactory).FullName, organizationServiceFactory);
            this.services.Add(typeof(ITracingService).FullName, traceService);
            this.systemuserid = pluginExecutionContext.UserId;
        }

        public object Resolve(TypeCache type)
        {
            if (services.ContainsKey(type.ObjectInstanceKey))
            {
                return services[type.ObjectInstanceKey];
            }

            if (type.IsTarget)
            {
                var entity = (Microsoft.Xrm.Sdk.Entity)pluginExecutionContext.InputParameters["Target"];
                services[type.ObjectInstanceKey] = Extensions.Sdk.KiponSdkGeneratedExtensionMethods.ToEarlyBoundEntity(entity);
                return services[type.ObjectInstanceKey];
            }

            if (type.IsPreimage)
            {
                var imgName = PluginMethod.ImageSuffixFor(1, pluginExecutionContext.Stage, pluginExecutionContext.Mode == 1);
                var entity = (Microsoft.Xrm.Sdk.Entity)pluginExecutionContext.PreEntityImages[imgName];
                services[type.ObjectInstanceKey] = Extensions.Sdk.KiponSdkGeneratedExtensionMethods.ToEarlyBoundEntity(entity);
                return services[type.ObjectInstanceKey];
            }

            if (type.IsPostimage)
            {
                var imgName = PluginMethod.ImageSuffixFor(2, pluginExecutionContext.Stage, pluginExecutionContext.Mode == 1);
                var entity = (Microsoft.Xrm.Sdk.Entity)pluginExecutionContext.PostEntityImages[$"postimage{imgName}"];
                services[type.ObjectInstanceKey] = Extensions.Sdk.KiponSdkGeneratedExtensionMethods.ToEarlyBoundEntity(entity);
                return services[type.ObjectInstanceKey];
            }

            if (type.IsMergedimage)
            {
                var target = (Microsoft.Xrm.Sdk.Entity)pluginExecutionContext.InputParameters["Target"];
                var merged = new Microsoft.Xrm.Sdk.Entity();
                merged.Id = target.Id;
                merged.LogicalName = target.LogicalName;

                var imgName = PluginMethod.ImageSuffixFor(1, pluginExecutionContext.Stage, pluginExecutionContext.Mode == 1);
                var pre = (Microsoft.Xrm.Sdk.Entity)pluginExecutionContext.PreEntityImages[imgName];

                foreach (var attr in pre.Attributes.Keys)
                {
                    merged[attr] = pre[attr];
                }

                foreach (var attr in target.Attributes.Keys)
                {
                    merged[attr] = target[attr];
                }

                services[type.ObjectInstanceKey] = Extensions.Sdk.KiponSdkGeneratedExtensionMethods.ToEarlyBoundEntity(merged);
                return services[type.ObjectInstanceKey];
            }

            if (type.IsReference)
            {
                var target = (Microsoft.Xrm.Sdk.EntityReference)pluginExecutionContext.InputParameters["Target"];
                if (type.Constructor != null)
                {
                    services[type.ObjectInstanceKey] = type.Constructor.Invoke(new object[] { target });
                }
                else
                {
                    services[type.ObjectInstanceKey] = target;
                }
                return services[type.ObjectInstanceKey];
            }

            if (type.FromType == typeof(Microsoft.Xrm.Sdk.IOrganizationService))
            {
                return this.GetOrganizationService(type.RequireAdminService);
            }

            if (type.IsQuery)
            {
                var uow = this.GetIUnitOfWork(type.RequireAdminService);
                var queryProperty = type.RepositoryProperty;
                var repository = queryProperty.GetValue(uow, new object[0]);
                var queryMethod = type.QueryMethod;
                return queryMethod.Invoke(repository, new object[0]);
            }
            return this.CreateServiceInstance(type);
        }

        private List<string> resolving = new List<string>();

        private IUnitOfWork GetIUnitOfWork(bool admin)
        {
            TypeCache tc = TypeCache.ForUow(admin);
            return (IUnitOfWork)this.CreateServiceInstance(tc);
        }

        private Microsoft.Xrm.Sdk.IOrganizationService GetOrganizationService(bool admin)
        {
            var objectInstanceKey = typeof(Microsoft.Xrm.Sdk.IOrganizationService).FullName;
            if (admin)
            {
                objectInstanceKey += ":admin";
            }
            if (services.ContainsKey(objectInstanceKey))
            {
                return (Microsoft.Xrm.Sdk.IOrganizationService)services[objectInstanceKey];
            }
            if (admin)
            {
                services[objectInstanceKey] = this.organizationServiceFactory.CreateOrganizationService(null);
            }
            else
            {
                services[objectInstanceKey] = this.organizationServiceFactory.CreateOrganizationService(this.systemuserid);
            }

            return (Microsoft.Xrm.Sdk.IOrganizationService)services[objectInstanceKey];
        }

        private object CreateServiceInstance(TypeCache type)
        {
            if (resolving.Contains(type.ObjectInstanceKey))
            {
                throw new Exceptions.CircularDependencyException(string.Join(">", resolving.ToArray()));
            }
            try
            {
                resolving.Add(type.ObjectInstanceKey);
                var argTypes = ServiceConstructorCache.ForConstructor(type.Constructor);
                var args = new object[argTypes != null ? argTypes.Length : 0];

                if (args.Length == 0)
                {
                    services[type.ObjectInstanceKey] = type.Constructor.Invoke(args);
                    return services[type.ObjectInstanceKey];
                }

                var ix = 0;
                foreach (var argType in argTypes)
                {
                    if (argType.FromType == typeof(Microsoft.Xrm.Sdk.IOrganizationService))
                    {
                        if (type.RequireAdminService)
                        {
                            var key = typeof(Microsoft.Xrm.Sdk.IOrganizationService).FullName + ":admin";
                            if (services.ContainsKey(key))
                            {
                                args[ix] = services[key];
                                ix++;
                                continue;
                            }
                            else
                            {
                                services[key] = this.organizationServiceFactory.CreateOrganizationService(null);
                                args[ix] = services[key];
                                ix++;
                                continue;
                            }
                        }
                        else
                        {
                            var key = typeof(Microsoft.Xrm.Sdk.IOrganizationService).FullName;
                            if (services.ContainsKey(key))
                            {
                                args[ix] = services[key];
                                ix++;
                                continue;
                            }
                            else
                            {
                                services[key] = this.organizationServiceFactory.CreateOrganizationService(this.systemuserid);
                                args[ix] = services[key];
                                ix++;
                                continue;
                            }
                        }
                    }

                    if (services.ContainsKey(argType.ObjectInstanceKey))
                    {
                        args[ix] = services[argType.ObjectInstanceKey];
                        ix++;
                        continue;
                    }

                    {
                        services[argType.ObjectInstanceKey] = this.CreateServiceInstance(argType);
                        args[ix] = services[argType.ObjectInstanceKey];
                        ix++;
                        continue;
                    }
                }
                return type.Constructor.Invoke(args);
            }
            finally
            {
                resolving.Remove(type.ObjectInstanceKey);
            }
        }

        public void OnStepFinalize()
        {
            if (this.services != null)
            {
                foreach (var s in this.services.Values)
                {
                    var asdispos = s as IService;
                    if (asdispos != null)
                    {
                        asdispos.OnStepFinalized();
                    }
                }
            }
        }

        public void Dispose()
        {
            if (this.services != null)
            {
                foreach (var s in this.services.Values)
                {
                    var asdispos = s as System.IDisposable;
                    if (asdispos != null)
                    {
                        asdispos.Dispose();
                    }
                }
                this.services.Clear();
            }
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Reflection\ServiceConstructorCache.cs
namespace Kipon.Fake.Xrm.Reflection
{
    using System.Collections.Generic;
    public class ServiceConstructorCache
    {
        private readonly static Dictionary<System.Reflection.ConstructorInfo, TypeCache[]> cache = new Dictionary<System.Reflection.ConstructorInfo, TypeCache[]>();

        public static TypeCache[] ForConstructor(System.Reflection.ConstructorInfo constructor)
        {
            if (cache.ContainsKey(constructor))
            {
                return cache[constructor];
            }

            var parameters = constructor.GetParameters();
            if (parameters == null || parameters.Length == 0)
            {
                cache[constructor] = new TypeCache[0];
                return cache[constructor];
            }

            var result = new TypeCache[parameters.Length];

            var ix = 0;
            foreach (var par in parameters)
            {
                result[ix] = TypeCache.ForParameter(par);
                if (result[ix].RequirePluginContext)
                {
                    throw new Exceptions.InvalidConstructorServiceArgumentException(constructor, par);
                }
                ix++;
            }
            cache[constructor] = result;
            return result;
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Reflection\TypeCache.cs
namespace Kipon.Fake.Xrm.Reflection
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    /// <summary>
    /// Type cache is used to resolved types for each parameter in a context. 
    /// If the parameter is related to images (target, pre, post etc.), 
    /// The type is cached on the pointer to the parameter, so each parameter is only resolved 
    /// once in the system life-time.
    /// </summary>
    public class TypeCache
    {
        public static Types Types { get; set; }

        private static System.Reflection.ParameterInfo UOW;
        private static System.Reflection.ParameterInfo UOW_ADMIN;

        static TypeCache()
        {
            TypeCache.Types = Types.Instance;
            var method = typeof(TypeCache).GetMethod(nameof(TypeCache.DummyUOW), System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            var pms = method.GetParameters();
            UOW = pms[0];
            UOW_ADMIN = pms[1];
        }

        private static Dictionary<System.Reflection.ParameterInfo, TypeCache> resolvedTypes = new Dictionary<System.Reflection.ParameterInfo, TypeCache>();

        public static TypeCache ForParameter(System.Reflection.ParameterInfo parameter)
        {
            var type = parameter.ParameterType;

            if (resolvedTypes.ContainsKey(parameter))
            {
                return resolvedTypes[parameter];
            }

            if (parameter.ParameterType == typeof(Microsoft.Xrm.Sdk.IOrganizationService))
            {
                resolvedTypes[parameter] = new TypeCache { FromType = type, ToType = type };

                resolvedTypes[parameter].RequireAdminService = parameter.GetCustomAttributes(Types.AdminAttribute, false).Any();
                return resolvedTypes[parameter];
            }

            if (parameter.ParameterType == typeof(Microsoft.Xrm.Sdk.IOrganizationServiceFactory))
            {
                resolvedTypes[parameter] = new TypeCache { FromType = type, ToType = type };
                return resolvedTypes[parameter];
            }

            if (parameter.ParameterType == typeof(Microsoft.Xrm.Sdk.IPluginExecutionContext))
            {
                resolvedTypes[parameter] = new TypeCache { FromType = type, ToType = type };
                return resolvedTypes[parameter];
            }

            if (parameter.ParameterType == typeof(Microsoft.Xrm.Sdk.ITracingService))
            {
                resolvedTypes[parameter] = new TypeCache { FromType = type, ToType = type };
                return resolvedTypes[parameter];
            }

            #region not an abstract, and not an interface, the type can be used directly, see if the name indicates that it is target, preimage, mergedimage or postimage
            if (!type.IsInterface && !type.IsAbstract)
            {
                var constructors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

                if (constructors != null && constructors.Length > 0)
                {
                    var result = new TypeCache { FromType = type, ToType = type };
                    var isEntity = false;

                    #region see if we can resolve parameter to the target as an entity
                    if (parameter.MatchPattern(Types.TargetAttribute, "target") && type.BaseType == typeof(Microsoft.Xrm.Sdk.Entity))
                    {
                        isEntity = true;
                        result.IsTarget = true;
                    }
                    else
                    if (parameter.MatchPattern(Types.PreimageAttribute, "preimage") && type.BaseType == typeof(Microsoft.Xrm.Sdk.Entity))
                    {
                        isEntity = true;
                        result.IsPreimage = true;
                    }
                    else
                    if (parameter.MatchPattern(Types.MergedimageAttribute, "mergedimage") && type.BaseType == typeof(Microsoft.Xrm.Sdk.Entity))
                    {
                        isEntity = true;
                        result.IsMergedimage = true;
                    }
                    else
                    if (parameter.MatchPattern(Types.PostimageAttribute, "postimage") && type.BaseType == typeof(Microsoft.Xrm.Sdk.Entity))
                    {
                        isEntity = true;
                        result.IsPostimage = true;
                    }

                    if (isEntity)
                    {
                        var entity = (Microsoft.Xrm.Sdk.Entity)Activator.CreateInstance(type);
                        result.LogicalName = entity.LogicalName;
                        result.ResolveProperties();
                    }
                    #endregion

                    var isReference = false;

                    #region see if we can resolve parameter to the target as en entity reference
                    if (!isEntity && type.ExtendsGenericClassOf(Types.TargetReference))
                    {
                        isReference = true;
                        result.IsTarget = true;
                        result.IsReference = true;
                        result.ToType = type.BaseType.GetGenericArguments()[0];

                        var entity = (Microsoft.Xrm.Sdk.Entity)Activator.CreateInstance(result.ToType);
                        result.LogicalName = entity.LogicalName;
                        result.Constructor = type.GetConstructor(new Type[] { typeof(Microsoft.Xrm.Sdk.EntityReference) });
                    }

                    if (!isEntity && !isReference && type == typeof(Microsoft.Xrm.Sdk.EntityReference))
                    {
                        if (parameter.MatchPattern(Types.TargetAttribute, "target"))
                        {
                            isReference = true;
                            result.IsTarget = true;
                            result.IsReference = true;
                            result.ToType = type;
                            result.Constructor = null;
                        }
                    }
                    #endregion

                    if (!isEntity && !isReference)
                    {
                        result.Constructor = GetConstructor(type);
                        resolvedTypes[parameter] = result;
                    }
                    else
                    {
                        resolvedTypes[parameter] = result;
                    }
                    return result;
                }
            }
            #endregion

            #region see if it is target, preimage post image or merged image interface
            if (type.IsInterface)
            {
                Type toType = type.ImplementsGenericInterface(Types.Target);
                if (toType != null)
                {
                    var result = new TypeCache { FromType = type, ToType = toType, IsTarget = true };
                    var entity = (Microsoft.Xrm.Sdk.Entity)Activator.CreateInstance(result.ToType);
                    result.LogicalName = entity.LogicalName;
                    result.ResolveProperties();

                    if (ReturnIfOk(type, result))
                    {
                        resolvedTypes[parameter] = result;
                        return result;
                    }
                }

                toType = type.ImplementsGenericInterface(Types.Preimage);
                if (toType != null)
                {
                    var result = new TypeCache { FromType = type, ToType = toType, IsPreimage = true };
                    var entity = (Microsoft.Xrm.Sdk.Entity)Activator.CreateInstance(result.ToType);
                    result.LogicalName = entity.LogicalName;
                    result.ResolveProperties();
                    if (ReturnIfOk(type, result))
                    {
                        resolvedTypes[parameter] = result;
                        return resolvedTypes[parameter];
                    }
                }

                toType = type.ImplementsGenericInterface(Types.Mergedimage);
                if (toType != null)
                {
                    var result = new TypeCache { FromType = type, ToType = toType, IsMergedimage = true };
                    var entity = (Microsoft.Xrm.Sdk.Entity)Activator.CreateInstance(result.ToType);
                    result.LogicalName = entity.LogicalName;
                    result.ResolveProperties();
                    if (ReturnIfOk(type, result))
                    {
                        resolvedTypes[parameter] = result;
                        return resolvedTypes[parameter];
                    }
                }

                toType = type.ImplementsGenericInterface(Types.Postimage);
                if (toType != null)
                {
                    var result = new TypeCache { FromType = type, ToType = toType, IsPostimage = true };
                    var entity = (Microsoft.Xrm.Sdk.Entity)Activator.CreateInstance(result.ToType);
                    result.LogicalName = entity.LogicalName;
                    result.ResolveProperties();
                    if (ReturnIfOk(type, result))
                    {
                        resolvedTypes[parameter] = result;
                        return result;
                    }
                }
            }
            #endregion

            #region IQueryable
            if (type.IsInterface && type.IsGenericType && type.GenericTypeArguments.Length == 1 && type.GenericTypeArguments[0].BaseType != null && type.GenericTypeArguments[0].BaseType == typeof(Microsoft.Xrm.Sdk.Entity))
            {
                var result = ForQuery(type);
                if (result != null)
                {
                    result.RequireAdminService = parameter.GetCustomAttributes(Types.AdminAttribute, false).Any();
                    return result;
                }
            }
            #endregion

            #region find implementing interface
            if (type.IsInterface)
            {
                var r1 = GetInterfaceImplementation(type);
                var result = new TypeCache { FromType = type, ToType = r1, Constructor = GetConstructor(r1) };

                resolvedTypes[parameter] = result;
                return result;

            }
            #endregion

            #region find relevant abstract extension
            if (type.IsAbstract)
            {
            }
            #endregion

            throw new Exceptions.UnresolvableTypeException(type);
        }

        public static TypeCache ForQuery(Type type)
        {
            var genericQueryable = typeof(System.Linq.IQueryable<>);
            var queryType = genericQueryable.MakeGenericType(type.GenericTypeArguments[0]);
            if (type == queryType)
            {
                var result = new TypeCache { FromType = type, ToType = queryType, IsQuery = true };
                return result;
            }
            return null;
        }

        public static TypeCache ForUow(bool admin)
        {
            var pi = admin ? UOW_ADMIN : UOW;
            if (resolvedTypes.ContainsKey(pi))
            {
                return resolvedTypes[pi];
            }

            var fromType = admin ? Types.IAdminUnitOfWork : Types.IUnitOfWork;
            var r1 = GetInterfaceImplementation(fromType);
            var result = new TypeCache { FromType = fromType, ToType = r1, Constructor = GetConstructor(r1), RequireAdminService = admin };
            resolvedTypes[pi] = result;
            return result;
        }

        private void DummyUOW(object uow, object adminUOW)
        {
        }

        #region private static helpers
        private static Type GetInterfaceImplementation(Type type)
        {
            var allTypes = Types.Assembly.GetTypes();
            var candidates = new List<Type>();

            foreach (var t in allTypes)
            {
                if (!t.IsInterface && !t.IsAbstract && type.IsAssignableFrom(t))
                {
                    var cons = t.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (cons.Length > 0)
                    {
                        candidates.Add(t);
                    }
                }
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            if (candidates.Count == 0)
            {
                throw new Exceptions.UnresolvableTypeException(type);
            }

            var all = candidates.ToArray();
            candidates.Clear();
            foreach (var t in all)
            {
                var exports = t.GetCustomAttributes(Types.ExportAttribute, false).ToArray();
                foreach (var exported in exports)
                {
                    if (exported != null && (Type)exported.GetType().GetProperty("Type").GetValue(exported) == type)
                    {
                        candidates.Add(t);
                    }
                }
            }

            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            throw new Exceptions.MultiImplementationOfSameInterfaceException(type);
        }

        private static System.Reflection.ConstructorInfo GetConstructor(Type type)
        {
            var constructors = type.GetConstructors(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            if (constructors.Length == 1)
            {
                return constructors[0];
            }

            if (constructors == null || constructors.Length == 0)
            {
                return null;
            }

            List<System.Reflection.ConstructorInfo> candidates = new List<System.Reflection.ConstructorInfo>();
            foreach (var c in constructors)
            {
                var ca = c.GetCustomAttributes(Types.ImportingConstructorAttribute, false).FirstOrDefault();
                if (ca != null)
                {
                    candidates.Add(c);
                }
            }
            if (candidates.Count == 1)
            {
                return candidates[0];
            }

            throw new Exceptions.UnresolvableConstructorException(type);
        }

        private static bool ReturnIfOk(Type from, TypeCache result)
        {
            if (from.IsInterface && !from.IsAssignableFrom(result.ToType))
            {
                throw new Exceptions.TypeMismatchException(from, result.ToType);
            }
            return true;
        }
        #endregion

        #region private helpers
        private void ResolveProperties()
        {
            if (this.FromType.Inheriting(typeof(Microsoft.Xrm.Sdk.Entity)))
            {
                this.AllProperties = true;
                return;
            }

            if (this.ToType.Inheriting(typeof(Microsoft.Xrm.Sdk.Entity)))
            {
                this.FilteredProperties = CommonProperty.ForType(this.FromType, this.ToType);
            }
        }
        #endregion

        #region properties
        public Type FromType { get; private set; }
        public Type ToType { get; private set; }

        public System.Reflection.ConstructorInfo Constructor { get; private set; }

        public bool IsTarget { get; private set; }
        public bool IsReference { get; private set; }
        public bool IsPreimage { get; private set; }
        public bool IsMergedimage { get; private set; }
        public bool IsPostimage { get; private set; }
        public string LogicalName { get; private set; }

        public bool IsQuery { get; private set; }

        public bool RequireAdminService { get; private set; }
        public bool AllProperties { get; private set; }
        public CommonProperty[] FilteredProperties { get; private set; }

        public bool RequirePluginContext
        {
            get
            {
                return IsTarget || IsReference || IsPreimage || IsPostimage || IsMergedimage;
            }
        }

        private System.Reflection.PropertyInfo _repositoryProperty;
        public System.Reflection.PropertyInfo RepositoryProperty
        {
            get
            {
                if (_repositoryProperty != null)
                {
                    return _repositoryProperty;
                }

                var repositoryType = Types.Instance.IRepository.GetGenericTypeDefinition();
                var entityType = this.ToType.GetGenericArguments()[0];
                var queryType = repositoryType.MakeGenericType(entityType);

                var uowTC = TypeCache.ForUow(this.RequireAdminService);

                var properties = uowTC.ToType.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                                        .Where(r => r.PropertyType == queryType).ToArray();

                if (properties.Length == 0)
                {
                    throw new Exceptions.UnresolvableTypeException(this.ToType);
                }

                if (properties.Length > 1)
                {
                    throw new Exceptions.MultiImplementationOfSameInterfaceException(this.ToType);
                }

                this._repositoryProperty = properties[0];
                return _repositoryProperty;
            }
        }

        private System.Reflection.MethodInfo _queryMethod;
        public System.Reflection.MethodInfo QueryMethod
        {
            get
            {
                if (this._queryMethod == null)
                {
                    var repository = this.RepositoryProperty;
                    this._queryMethod = repository.PropertyType.GetMethod("GetQuery", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                    if (this.QueryMethod == null)
                    {
                        throw new Exceptions.UnresolvableTypeException(this.ToType);
                    }
                }
                return this._queryMethod;
            }
        }

        private string _ik;
        public string ObjectInstanceKey
        {
            get
            {
                if (_ik == null)
                {
                    if (this.IsTarget) this._ik = "target:" + this.ToType.FullName;
                    else if (this.IsPreimage) this._ik = "preimage:" + this.ToType.FullName;
                    else if (this.IsMergedimage) this._ik = "mergedimage:" + this.ToType.FullName;
                    else if (this.IsPostimage) this._ik = "postimage:" + this.ToType.FullName;
                    else if (this.IsReference) this._ik = "ref:" + this.ToType.FullName;
                    else if (this.FromType.Implements(Types.IAdminUnitOfWork))
                    {
                        this._ik = Types.IAdminUnitOfWork.FullName;
                        this.RequireAdminService = true;
                    }
                    else if (this.FromType == typeof(Microsoft.Xrm.Sdk.IOrganizationService))
                    {
                        if (this.RequireAdminService)
                        {
                            this._ik = this.FromType.FullName + ":admin";
                        }
                        else
                        {
                            this._ik = this.FromType.FullName;
                        }
                    }
                    else if (this.IsQuery)
                    {
                        if (this.RequireAdminService)
                        {
                            return this.ToType.FullName + ":admin";
                        }
                        return this.ToType.FullName;
                    }
                    else if (this.FromType.Implements(Types.IAdminUnitOfWork)) this._ik = Types.IAdminUnitOfWork.FullName;
                    else if (this.FromType.Implements(Types.IUnitOfWork)) this._ik = Types.IUnitOfWork.FullName;
                    else if (this.ToType != null) this._ik = this.ToType.FullName;
                    else this._ik = this.FromType.FullName;
                }
                return _ik;
            }
        }
        #endregion
    }


    public static class TypeCacheLocalExtensions
    {
        public static bool Inheriting(this Type value, Type other)
        {
            if (value.BaseType == other)
            {
                return true;
            }

            if (value.IsSubclassOf(other))
            {
                return true;
            }

            if (value.BaseType != null)
            {
                return value.BaseType.Inheriting(other);
            }
            return false;
        }

        public static bool Implements(this Type value, Type other)
        {
            var intf = value.GetInterfaces();
            return intf != null && intf.Contains(other);
        }

        public static Type ImplementsGenericInterface(this Type value, Type other)
        {
            if (!other.IsGenericType)
            {
                return null;
            }

            var match = value.GetInterfaces()
                .Where(i => i.IsGenericType)
                .FirstOrDefault(i => i.GetGenericTypeDefinition() == other);

            if (match != null)
            {
                return match.GetGenericArguments()[0];
            }

            return null;
        }

        public static bool ExtendsGenericClassOf(this Type toCheck, Type generic)
        {
            while (toCheck != null && toCheck != typeof(object))
            {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur)
                {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }

        public static bool MatchPattern(this System.Reflection.ParameterInfo parameter, Type customAttribute, string attributeName)
        {
            if (parameter == null)
            {
                return false;
            }
            var hasCA = parameter.GetCustomAttributes(customAttribute, false).Any();
            if (hasCA)
            {
                return true;
            }

            return !string.IsNullOrEmpty(attributeName) && parameter.Name.ToLower().Equals(attributeName.ToLower());
        }
    }
}

#endregion
#region source: F:\Projects\OpenSource\Kipon.Dynamics.Plugin\Kipon.Solid.Plugin\Xrm\Reflection\Types.cs
namespace Kipon.Fake.Xrm.Reflection
{
    using System;
    using System.Linq;

    public sealed class Types
    {
        private const string NAMESPACE = "Kipon" + "." + "Xrm" + ".";

        private static Types _instance;

        static Types()
        {
            _instance = new Types();
        }

        private Types()
        {
        }


        public static Types Instance
        {
            get
            {
                return _instance;
            }
        }

        public void SetAssembly(System.Reflection.Assembly assembly)
        {
            this.Assembly = assembly;
            var allTypes = assembly.GetTypes().ToDictionary(r => r.FullName);

            this.TargetAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.TargetAttribute)}"];
            this.PreimageAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.PreimageAttribute)}"];
            this.MergedimageAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.MergedimageAttribute)}"];
            this.PostimageAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.PostimageAttribute)}"];
            this.AdminAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.AdminAttribute)}"];
            this.ExportAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.ExportAttribute)}"];
            this.ImportingConstructorAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.ImportingConstructorAttribute)}"];
            this.RequiredAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.RequiredAttribute)}"];
            this.StepAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.StepAttribute)}"];
            this.LogicalNameAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.LogicalNameAttribute)}"];
            this.SortAttribute = allTypes[$"{NAMESPACE}Attributes.{nameof(_instance.SortAttribute)}"];

            this.Target = allTypes[$"{NAMESPACE}{nameof(_instance.Target)}`1"];
            this.TargetReference = allTypes[$"{NAMESPACE}{nameof(_instance.TargetReference)}`1"];
            this.Preimage = allTypes[$"{NAMESPACE}{nameof(_instance.Preimage)}`1"];
            this.Mergedimage = allTypes[$"{NAMESPACE}{nameof(_instance.Mergedimage)}`1"];
            this.Postimage = allTypes[$"{NAMESPACE}{nameof(_instance.Postimage)}`1"];

            this.IUnitOfWork = allTypes[$"{NAMESPACE}{nameof(_instance.IUnitOfWork)}"];
            this.IAdminUnitOfWork = allTypes[$"{NAMESPACE}{nameof(_instance.IAdminUnitOfWork)}"];

            this.IRepository = allTypes[$"{NAMESPACE}{nameof(_instance.IRepository)}`1"];
        }

        public Type TargetAttribute { get; private set; }
        public Type PreimageAttribute { get; private set; }
        public Type MergedimageAttribute { get; private set; }
        public Type PostimageAttribute { get; private set; }
        public Type AdminAttribute { get; private set; }
        public Type ExportAttribute { get; private set; }
        public Type ImportingConstructorAttribute { get; private set; }
        public Type RequiredAttribute { get; private set; }
        public Type StepAttribute { get; private set; }
        public Type LogicalNameAttribute { get; private set; }
        public Type SortAttribute { get; private set; }

        public Type Target { get; private set; }
        public Type TargetReference { get; private set; }
        public Type Preimage { get; private set; }
        public Type Mergedimage { get; private set; }
        public Type Postimage { get; private set; }
        public Type IUnitOfWork { get; private set; }
        public Type IAdminUnitOfWork { get; private set; }

        public Type IRepository { get; private set; }

        public System.Reflection.Assembly Assembly { get; private set; }

    }
}

#endregion
