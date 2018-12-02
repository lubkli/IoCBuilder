using System;
using System.Globalization;
using System.Reflection;
using System.Runtime.Serialization;
using System.Security.Permissions;
using IoCBuilder.Lifetime;
using IoCBuilder.Strategies.Singleton;

namespace IoCBuilder.Strategies.Creation
{
    /// <summary>
    /// Implementation of <see cref="IBuilderStrategy"/> which creates objects.
    /// </summary>
    /// <remarks>
    /// <para>This strategy looks for policies in the context registered under the interface type
    /// <see cref="ICreationPolicy"/>. If it cannot find a policy on how to create the object,
    /// it will select the first constructor that returns from reflection, and re-runs the chain
    /// to create all the objects required to fulfill the constructor's parameters.</para>
    /// <para>If the Build method is passed an object via the existing parameter, then it
    /// will do nothing (since the object already exists). This allows this strategy to be
    /// in the chain when running dependency injection on existing objects, without fear that
    /// it will attempt to re-create the object.</para>
    /// </remarks>
    public class CreationStrategy : BuilderStrategy
    {
        /// <summary>
        /// Override of <see cref="IBuilderStrategy.BuildUp"/>. Creates the requested object.
        /// </summary>
        /// <param name="context">The build context.</param>
        /// <param name="buildKey">The build key of object to be created.</param>
        /// <param name="existing">The existing object. If not null, this strategy does nothing.</param>
        /// <returns>The created object</returns>
        public override object BuildUp(IBuilderContext context,
                                       object buildKey,
                                       object existing)
        {
            //return base.BuildUp(context, buildKey, existing ?? BuildUpNewObject(context, buildKey));
            if (existing != null)
                BuildUpExistingObject(context, buildKey, existing);
            else
                existing = BuildUpNewObject(context, buildKey, existing);

            return base.BuildUp(context, buildKey, existing);
        }

        private void BuildUpExistingObject(IBuilderContext context, object buildKey, object existing)
        {
            RegisterObject(context, buildKey, existing);
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.SerializationFormatter)]
        private object BuildUpNewObject(IBuilderContext context, object buildKey, object existing)
        {
            ICreationPolicy policy = context.Policies.Get<ICreationPolicy>(buildKey);

            if (policy == null)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture,
                                                          Resources.ResourceManager.GetString("MissingCreationPolicy"), buildKey));
            }

            try
            {
                existing = FormatterServices.GetSafeUninitializedObject(BuilderStrategy.GetTypeFromBuildKey(buildKey));
            }
            catch (MemberAccessException exception)
            {
                throw new ArgumentException(String.Format(CultureInfo.CurrentCulture, Resources.ResourceManager.GetString("CannotCreateInstanceOfType"), buildKey), exception);
            }

            RegisterObject(context, buildKey, existing);
            InitializeObject(context, existing, buildKey, policy);
            return existing;
        }

        private void RegisterObject(IBuilderContext context, object buildKey, object existing)
        {
            if (context.Locator != null)
            {
                ILifetimeContainer lifetime = context.Locator.Get<ILifetimeContainer>(typeof(ILifetimeContainer), SearchMode.Local);

                if (lifetime != null)
                {
                    ISingletonPolicy singletonPolicy = context.Policies.Get<ISingletonPolicy>(buildKey);

                    if (singletonPolicy != null && singletonPolicy.IsSingleton)
                    {
                        context.Locator.Add(buildKey, existing);
                        lifetime.Add(existing);

                        TraceBuildUp(context, buildKey, Resources.ResourceManager.GetString("SingletonRegistered"));
                    }
                }
            }
        }

        private void InitializeObject(IBuilderContext context, object existing, object buildKey, ICreationPolicy policy)
        {
            Type type = existing.GetType();
            ConstructorInfo constructor = policy.GetConstructor(context, buildKey);

            if (constructor == null)
            {
                if (type.IsValueType)
                    return;
                throw new ArgumentException(Resources.ResourceManager.GetString("NoAppropriateConstructor"));
            }

            object[] parms = policy.GetParameters(context, constructor);

            MethodBase method = (MethodBase)constructor;
            Guard.ValidateMethodParameters(method, parms, existing.GetType());

            if (TraceEnabled(context)) // Convert parameters only if trace is enabled
                TraceBuildUp(context, buildKey, Resources.ResourceManager.GetString("CallingConstructor"), ParametersToTypeList(parms));

            method.Invoke(existing, parms);
        }
    }
}